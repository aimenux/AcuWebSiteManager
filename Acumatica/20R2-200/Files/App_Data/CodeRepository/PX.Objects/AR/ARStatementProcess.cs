using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

using PX.Data;

using PX.Common;

using PX.Objects.Common;
using PX.Objects.Common.Abstractions;
using PX.Objects.Common.Aging;
using PX.Objects.Common.Extensions;
using PX.Objects.Common.Tools;

using PX.Objects.AR.BQL;
using PX.Objects.AR.CustomerStatements;
using PX.Objects.CS;
using PX.Objects.CM.Standalone;
using PX.Objects.DR.Descriptor;
using PX.Objects.GL;
using PX.Objects.GL.FinPeriods;
using PX.Objects.GL.FinPeriods.TableDefinition;
using PX.Data.BQL.Fluent;
using PX.Data.BQL;

namespace PX.Objects.AR
{


	[TableAndChartDashboardType]
	public class ARStatementProcess : PXGraph<ARStatementProcess>
	{
		#region Internal Types Definition
		public class Parameters : IBqlTable
		{
			public abstract class statementDate : PX.Data.BQL.BqlDateTime.Field<statementDate> { }
			/// <summary>
			/// Indicates the date on which the statements are generated.
			/// Defaults to the current business date.
			/// </summary>
			[PXDate]
			[PXDefault(typeof(AccessInfo.businessDate))]
			[PXUIField(DisplayName = Messages.PrepareFor)]
			public virtual DateTime? StatementDate
			{
				get;
				set;
			}
		}
		#endregion

		public ARStatementProcess()
		{
			ARSetup setup = ARSetup.Current;
			CyclesList.SetProcessDelegate<StatementCycleProcessBO>(StatementCycleProcessBO.ProcessCycles);
		}

		public PXSetup<ARSetup> ARSetup;

		public PXCancel<Parameters> Cancel;
		public PXFilter<Parameters> Filter;

		[PXFilterable]
		public PXFilteredProcessing<ARStatementCycle, Parameters> CyclesList;

		protected virtual IEnumerable cycleslist()
		{
			ARSetup setup = this.ARSetup.Select();

			DateTime statementDate = Filter.Current?.StatementDate ?? Accessinfo.BusinessDate.Value;

			foreach (ARStatementCycle row in PXSelect<ARStatementCycle>.Select(this))
			{
				try
				{
					row.NextStmtDate = CalcStatementDateBefore(
						this,
						statementDate,
						row.PrepareOn,
						row.Day00,
						row.Day01,
						row.DayOfWeek);

					if (row.LastStmtDate != null && row.NextStmtDate <= row.LastStmtDate)
					{
						row.NextStmtDate = CalcNextStatementDate(
							this,
							row.LastStmtDate.Value,
							row.PrepareOn,
							row.Day00,
							row.Day01,
							row.DayOfWeek);
					}
				}
				catch (PXFinPeriodException)
				{
					row.NextStmtDate = null;
				}

				if (row.NextStmtDate > statementDate)
				{
					continue;
				}

				CyclesList.Cache.SetStatus(row, PXEntryStatus.Updated);

				yield return row;
			}
		}

		public static bool CheckForUnprocessedPPD(PXGraph graph, string statementCycleID, DateTime? nextStmtDate, int? customerID)
		{
			PXSelectBase<ARInvoice> select = new PXSelectJoin<ARInvoice, 
				InnerJoin<Customer, On<Customer.bAccountID, Equal<ARInvoice.customerID>>,
				InnerJoin<ARAdjust, On<ARAdjust.adjdDocType, Equal<ARInvoice.docType>,
					And<ARAdjust.adjdRefNbr, Equal<ARInvoice.refNbr>,
					And<ARAdjust.released, Equal<True>, 
					And<ARAdjust.voided, NotEqual<True>,
					And<ARAdjust.pendingPPD, Equal<True>,
					And<ARAdjust.adjgDocDate, LessEqual<Required<ARAdjust.adjgDocDate>>>>>>>>>>,
				Where<ARInvoice.pendingPPD, Equal<True>,
					And<ARInvoice.released, Equal<True>,
					And<ARInvoice.openDoc, Equal<True>,
					And<Customer.statementCycleId, Equal<Required<Customer.statementCycleId>>>>>>>(graph);

			if (customerID != null)
			{
				select.WhereAnd<Where<Customer.bAccountID, Equal<Required<Customer.bAccountID>>>>();
			}

			return select.SelectSingle(nextStmtDate, statementCycleID, customerID) != null;
		}

		/// <summary>
		/// Returns a boolean flag indicating whether the statement contains no details.
		/// For Balance Brought Forward statements, additionally checks that its
		/// forward balance is not zero.
		/// </summary>
		public static bool IsEmptyStatement(PXGraph graph, ARStatement statement)
		{
			if (statement.StatementType == ARStatementType.BalanceBroughtForward)
			{
				bool isForwardBalanceZero =
					!statement.BegBalance.IsNonZero()
					&& !statement.CuryBegBalance.IsNonZero()
					&& !statement.EndBalance.IsNonZero()
					&& !statement.CuryBegBalance.IsNonZero();

				if (!isForwardBalanceZero) return false;
			}

			IEnumerable<ARStatementDetail> details = PXSelect<
				ARStatementDetail,
				Where<
					ARStatementDetail.branchID, Equal<Required<ARStatementDetail.branchID>>,
					And<ARStatementDetail.curyID, Equal<Required<ARStatementDetail.curyID>>,
					And<ARStatementDetail.customerID, Equal<Required<ARStatementDetail.customerID>>,
					And<ARStatementDetail.statementDate, Equal<Required<ARStatementDetail.statementDate>>>>>>>
				.SelectWindowed(graph, 0, 1, statement.BranchID, statement.CuryID, statement.CustomerID, statement.StatementDate)
				.RowCast<ARStatementDetail>();

			IEnumerable<ARStatementAdjust> applications = PXSelect<
				ARStatementAdjust,
				Where<
					ARStatementAdjust.branchID, Equal<Required<ARStatementAdjust.branchID>>,
					And<ARStatementAdjust.curyID, Equal<Required<ARStatementAdjust.curyID>>,
					And<ARStatementAdjust.customerID, Equal<Required<ARStatementAdjust.customerID>>,
					And<ARStatementAdjust.statementDate, Equal<Required<ARStatementDetail.statementDate>>>>>>>
				.SelectWindowed(graph, 0, 1, statement.BranchID, statement.CuryID, statement.CustomerID, statement.StatementDate)
				.RowCast<ARStatementAdjust>();

			return IsEmptyStatement(statement, details, applications);
		}

		public static bool IsEmptyStatement(
			ARStatement statement, 
			IEnumerable<ARStatementDetail> statementDetails,
			IEnumerable<ARStatementAdjust> statementApplications)
		{
			if (statement.StatementType == ARStatementType.BalanceBroughtForward)
			{
				bool isForwardBalanceZero =
					!statement.BegBalance.IsNonZero()
					&& !statement.CuryBegBalance.IsNonZero()
					&& !statement.EndBalance.IsNonZero()
					&& !statement.CuryEndBalance.IsNonZero();

				return isForwardBalanceZero && !statementDetails.Any() && !statementApplications.Any();
			}
			else
			{
				return !statementDetails.Any();
			}
		}

		private static bool CheckForOpenPayments(PXGraph aGraph, string aStatementCycleID)
		{
			ARRegister doc = PXSelectJoin<ARPayment,
								InnerJoin<Customer, On<ARPayment.customerID, Equal<Customer.bAccountID>>>,
								Where<Customer.statementCycleId, Equal<Required<Customer.statementCycleId>>,
				And<ARPayment.openDoc, Equal<True>>>>.SelectWindowed(aGraph, 0, 1, aStatementCycleID);
			return (doc != null);
		}

		private static bool CheckForOverdueInvoices(PXGraph aGraph, string aStatementCycleID, DateTime aOpDate)
		{
			ARBalances doc = PXSelectJoin<ARBalances,
								InnerJoin<Customer, On<ARBalances.customerID, Equal<Customer.bAccountID>>>,
								Where<Customer.statementCycleId, Equal<Required<Customer.statementCycleId>>,
								And<ARBalances.oldInvoiceDate, LessEqual<Required<ARBalances.oldInvoiceDate>>>>>.SelectWindowed(aGraph, 0, 1, aStatementCycleID, aOpDate);
			return (doc != null);
		}
			   
		public virtual void ARStatementCycle_RowSelected(PXCache cache, PXRowSelectedEventArgs e)
		{
			if (e.Row == null) return;

			ARStatementCycle row = (ARStatementCycle)e.Row;
			ARSetup setup = this.ARSetup.Select();
			PXCache.TryDispose(cache.GetAttributes(e.Row, null));

			if (row.NextStmtDate == null)
			{
				cache.DisplayFieldError<ARStatementCycle.nextStmtDate>(
					row,
					PXErrorLevel.RowWarning,
					Messages.UnableToCalculateNextStatementDateForEndOfPeriodCycle);

				return;
			}

			if (CheckForUnprocessedPPD(this, row.StatementCycleId, row.NextStmtDate, null))
			{
				PXUIFieldAttribute.SetEnabled(cache, row, false);
				cache.RaiseExceptionHandling<ARStatementCycle.selected>(row, false, 
					new PXSetPropertyException(Messages.UnprocessedPPDExists, PXErrorLevel.RowError));

				return;
			}

			bool? hasOverdueInvoices = null;
			bool hasUnAppliedPayments = false;
			bool hasChargeableInvoices = false;
					
			if (row.RequirePaymentApplication == true)
			{
				hasOverdueInvoices = CheckForOverdueInvoices(this, row.StatementCycleId, row.NextStmtDate.Value);

				if (hasOverdueInvoices == true && CheckForOpenPayments(this, row.StatementCycleId))
				{
					hasUnAppliedPayments = true;
				}
			}

			// The third condition below conveys the 'hidden' meaning of 
			// DefFinChargeFromCycle, i.e. 'attaching' overdue charges calculation
			// to statement cycles.
			//
			// If DefFinChargeFromCycle is false, it is assumed that the users take 
			// care of overdue charges themselves and need not be warned.
			// -
			if (row.FinChargeApply == true 
				&& row.RequireFinChargeProcessing == true 
				&& setup.DefFinChargeFromCycle == true)
			{
				if (!hasOverdueInvoices.HasValue)
				{
					hasOverdueInvoices = CheckForOverdueInvoices(this, row.StatementCycleId, row.NextStmtDate.Value);
				}
				if (hasOverdueInvoices.Value && 
					(!row.LastFinChrgDate.HasValue || row.LastFinChrgDate.Value < row.NextStmtDate.Value))
				{
					hasChargeableInvoices = true;						
				}
			}

			if (hasChargeableInvoices && hasUnAppliedPayments)
			{
				CyclesList.Cache.RaiseExceptionHandling<ARStatementCycle.statementCycleId>(
					row, 
					row.StatementCycleId,
					new PXSetPropertyException(Messages.WRN_ProcessStatementDetectsOverdueInvoicesAndUnappliedPayments, PXErrorLevel.RowWarning));
			}
			else if (hasChargeableInvoices) 
			{
				CyclesList.Cache.RaiseExceptionHandling<ARStatementCycle.statementCycleId>(
					row, 
					row.StatementCycleId,
					new PXSetPropertyException(Messages.WRN_ProcessStatementDetectsOverdueInvoices, PXErrorLevel.RowWarning));						
			}
			else if (hasUnAppliedPayments) 
			{
				CyclesList.Cache.RaiseExceptionHandling<ARStatementCycle.statementCycleId>(
					row, 
					row.StatementCycleId,
					new PXSetPropertyException(Messages.WRN_ProcessStatementDetectsUnappliedPayments, PXErrorLevel.RowWarning));
			}
		}

		#region InstanceUtility functions
		public static DateTime? CalcNextStatementDate(
			PXGraph graph,
			DateTime aLastStmtDate, 
			string aPrepareOn, 
			int? aDay00, 
			int? aDay01, 
			int? dayofWeek)
		{
			DateTime? nextDate = null;
			switch (aPrepareOn)
			{
				case ARStatementScheduleType.FixedDayOfMonth:
					DateTime guessDate = new PXDateTime(aLastStmtDate.Year, aLastStmtDate.Month, aDay00 ?? 1);
					nextDate = getNextDate(guessDate, aLastStmtDate, aDay00 ?? 1);
					break;
				case ARStatementScheduleType.EndOfMonth:
					DateTime dateTime = new DateTime(aLastStmtDate.Year, aLastStmtDate.Month , 1);
					dateTime = dateTime.AddMonths(1);
					TimeSpan diff = (dateTime.Subtract(aLastStmtDate));
					int days = diff.Days;
					if (days < 2)
						nextDate = dateTime.AddMonths(1).AddDays(-1);
					else
						nextDate = dateTime.AddDays(-1);
					break;
				case ARStatementScheduleType.TwiceAMonth:

					DateTime dateTime1 = DateTime.MinValue;
					DateTime dateTime2 = DateTime.MinValue;
					bool useBoth = (aDay00 != null) && (aDay01 != null);
					if (aDay00 != null)
						dateTime1 = new PXDateTime(aLastStmtDate.Year, aLastStmtDate.Month, aDay00.Value);
					if (aDay01 != null)
						dateTime2 = new PXDateTime(aLastStmtDate.Year, aLastStmtDate.Month, aDay01.Value);
					if (useBoth)
					{
						Int32 Day00 = (Int32)aDay00;
						Int32 Day01 = (Int32)aDay01;
						Utilities.SwapIfGreater(ref dateTime1, ref dateTime2);
						Utilities.SwapIfGreater(ref Day00, ref Day01);
						if (aLastStmtDate < dateTime1)
							nextDate = dateTime1;
						else
						{
							if (aLastStmtDate < dateTime2)
								nextDate = dateTime2;
							else
								nextDate = PXDateTime.DatePlusMonthSetDay(dateTime1, 1, Day00);
						}
					}
					else
					{
						DateTime dt = (dateTime1 != DateTime.MinValue) ? dateTime1 : dateTime2;
						if (dt != DateTime.MinValue)
						{
							nextDate = getNextDate(dt, aLastStmtDate, aDay00 ?? aDay01 ?? 1);
						}
					}
					break;
				case ARStatementScheduleType.EndOfPeriod:
					try
					{
						IFinPeriodRepository finPeriodRepository = graph.GetService<IFinPeriodRepository>();

						string dateFinancialPeriod = finPeriodRepository.GetPeriodIDFromDate(aLastStmtDate, FinPeriod.organizationID.MasterValue);
						DateTime periodEndDate = finPeriodRepository.PeriodEndDate(dateFinancialPeriod, FinPeriod.organizationID.MasterValue);

						if (periodEndDate.Date > aLastStmtDate.Date)
						{
							nextDate = periodEndDate;
						}
						else
						{
							nextDate = finPeriodRepository.PeriodEndDate(
								finPeriodRepository.GetOffsetPeriodId(dateFinancialPeriod, 1, FinPeriod.organizationID.MasterValue),
								FinPeriod.organizationID.MasterValue);
						}
					}
					catch (PXFinPeriodException exception)
					{
						throw new PXFinPeriodException(
							$"{PXLocalizer.Localize(Messages.UnableToCalculateNextStatementDateForEndOfPeriodCycle)} {exception.MessageNoPrefix}");
					}
					break;
				case ARStatementScheduleType.Weekly:
					DateTime result = aLastStmtDate;

					do
					{
						result = result.AddDays(1);
					}
					while ((int?)result.DayOfWeek != dayofWeek);

					nextDate = result;
					break;
				default:
					throw new PXException(
						Messages.UnsupportedStatementScheduleType, 
						GetLabel.For<ARStatementScheduleType>(aPrepareOn));
			}
			return nextDate;
		}

		public static DateTime CalcStatementDateBefore(
			PXGraph graph,
			DateTime aBeforeDate, 
			string aPrepareOn, 
			int? aDay00, 
			int? aDay01,
			int? dayOfWeek)
		{
			DateTime statementDate = DateTime.MinValue;
			switch (aPrepareOn)
			{
				case ARStatementScheduleType.FixedDayOfMonth:
					statementDate = new PXDateTime(aBeforeDate.Year, aBeforeDate.Month, aDay00 ?? 1);
					if (statementDate.Date == aBeforeDate.Date)
						return statementDate;
					statementDate = getPrevDate(statementDate, aBeforeDate, aDay00 ?? 1);
					break;
				case ARStatementScheduleType.EndOfMonth:
					if (aBeforeDate.AddDays(1).Month != aBeforeDate.Month)
						return aBeforeDate;
					DateTime dateTime = new DateTime(aBeforeDate.Year, aBeforeDate.Month, 1);
					statementDate = dateTime.AddDays(-1);
					break;
				case ARStatementScheduleType.TwiceAMonth:
					DateTime dateTime1 = DateTime.MinValue;
					DateTime dateTime2 = DateTime.MinValue;
					bool useBoth = (aDay00 != null) && (aDay01 != null);
					if (aDay00 != null)
						dateTime1 = new PXDateTime(aBeforeDate.Year, aBeforeDate.Month, aDay00.Value);
					if (aDay01 != null)
						dateTime2 = new PXDateTime(aBeforeDate.Year, aBeforeDate.Month, aDay01.Value);
					if (useBoth)
					{
						Int32 Day00 = (Int32)aDay00;
						Int32 Day01 = (Int32)aDay01;
						Utilities.SwapIfGreater(ref dateTime1, ref dateTime2);
						Utilities.SwapIfGreater(ref Day00, ref Day01);
						if (aBeforeDate >= dateTime2)
							statementDate = dateTime2;
						else
						{
							if(aBeforeDate >= dateTime1)
								statementDate = dateTime1;
							else
								statementDate = PXDateTime.DatePlusMonthSetDay(dateTime2, -1, Day01);
						}
					}
					else
					{
						DateTime dt = (dateTime1 != DateTime.MinValue) ? dateTime1 : dateTime2;
						if (dt != DateTime.MinValue)
						{
							statementDate = getPrevDate(dt, statementDate, aDay00 ?? aDay01 ?? 1);
						}
					}
					break;
				case ARStatementScheduleType.EndOfPeriod:
					try
					{
						IFinPeriodRepository finPeriodRepository = graph.GetService<IFinPeriodRepository>();

						string dateFinancialPeriod = finPeriodRepository.GetPeriodIDFromDate(aBeforeDate, FinPeriod.organizationID.MasterValue);
						DateTime periodEndDate = finPeriodRepository.PeriodEndDate(dateFinancialPeriod, FinPeriod.organizationID.MasterValue);

						if (periodEndDate.Date == aBeforeDate.Date)
						{
							return periodEndDate;
						}
						else
						{
							string previousFinancialPeriod = finPeriodRepository.GetOffsetPeriodId(
								dateFinancialPeriod, -1, FinPeriod.organizationID.MasterValue);

							return finPeriodRepository.PeriodEndDate(previousFinancialPeriod, FinPeriod.organizationID.MasterValue);
						}
					}
					catch (PXFinPeriodException exception)
					{
						throw new PXFinPeriodException(
							$"{PXLocalizer.Localize(Messages.UnableToCalculateNextStatementDateForEndOfPeriodCycle)} {exception.MessageNoPrefix}");
					}
				case ARStatementScheduleType.Weekly:
					if ((int)aBeforeDate.DayOfWeek == dayOfWeek)
					{
						return aBeforeDate;
					}
					else
					{
						DateTime resultDate = aBeforeDate;

						while ((int)resultDate.DayOfWeek != dayOfWeek)
						{
							resultDate = resultDate.AddDays(-1);
						}

						return resultDate;
					}
				default:
					throw new PXException(
						Messages.UnsupportedStatementScheduleType,
						GetLabel.For<ARStatementScheduleType>(aPrepareOn));
			}
			return statementDate;
		}

		public static DateTime FindNextStatementDate(PXGraph graph, DateTime aBusinessDate, ARStatementCycle aCycle) 
		{
			DateTime? result = CalcStatementDateBefore(
				graph,
				aBusinessDate, 
				aCycle.PrepareOn, 
				aCycle.Day00, 
				aCycle.Day01,
				aCycle.DayOfWeek);

			if (aCycle.LastStmtDate.HasValue && result <= aCycle.LastStmtDate)
			{
				result = CalcNextStatementDate(
					graph,
					aCycle.LastStmtDate.Value, 
					aCycle.PrepareOn, 
					aCycle.Day00, 
					aCycle.Day01,
					aCycle.DayOfWeek);
			}

			return result.HasValue ? result.Value : aBusinessDate;
		}

		public static DateTime FindNextStatementDateAfter(PXGraph graph, DateTime aBusinessDate, ARStatementCycle aCycle)
		{
			DateTime? result = null;
			if (aCycle.LastStmtDate.HasValue)
			{
				result = CalcNextStatementDate(
					graph,
					aCycle.LastStmtDate.Value, 
					aCycle.PrepareOn, 
					aCycle.Day00, 
					aCycle.Day01,
					aCycle.DayOfWeek);

				if (result >= aBusinessDate)
				{
					return result.Value;
				}
			}

			result = CalcStatementDateBefore(
				graph,
				aBusinessDate, 
				aCycle.PrepareOn, 
				aCycle.Day00, 
				aCycle.Day01,
				aCycle.DayOfWeek);

			do
			{
				result = CalcNextStatementDate(
					graph,
					result.Value, 
					aCycle.PrepareOn, 
					aCycle.Day00, 
					aCycle.Day01,
					aCycle.DayOfWeek);
			}
			while (result != null && result < aBusinessDate);

			return result.Value;
		}

		protected static DateTime getNextDate(DateTime aGuessDate, DateTime aLastStatementDate, int Day)
		{
			return (aLastStatementDate < aGuessDate) ? aGuessDate : PXDateTime.DatePlusMonthSetDay(aGuessDate, 1, Day);
		}

		protected static DateTime getPrevDate(DateTime aGuessDate, DateTime aBeforeDate, int Day)
		{
			return (aGuessDate < aBeforeDate) ? aGuessDate : PXDateTime.DatePlusMonthSetDay(aGuessDate, -1, Day);
		}

		#endregion
	}

	public static class Utilities
	{
		public static void SwapIfGreater<T>(ref T lhs, ref T rhs) where T : System.IComparable<T>
		{
			T temp;
			if (lhs.CompareTo(rhs) > 0)
			{
				temp = lhs;
				lhs = rhs;
				rhs = temp;
			}
		}
	}

	[PXHidden]
	public class StatementCycleProcessBO : PXGraph<StatementCycleProcessBO>
	{
		public StatementCycleProcessBO()
		{
			ARSetup setup = ARSetup.Current;
		}

		#region Internal Types Definition
		[PXProjection(typeof(Select5<
			ARBalances,
			InnerJoin<Customer,
				On<Customer.bAccountID, Equal<ARBalances.customerID>>>,
			Where<ARBalances.lastDocDate, IsNull,
				Or<
					Where<ARBalances.statementRequired, Equal<True>,
					Or<ARBalances.lastDocDate, Greater<Customer.statementLastDate>>>>>,
			Aggregate<
				GroupBy<ARBalances.customerID>>>)
		, Persistent = false)]
		public partial class CustomerWithActiveBalance : PX.Data.IBqlTable
		{
			#region CustomerID
			public abstract class customerID : PX.Data.BQL.BqlInt.Field<customerID> { }
			[PXDBInt(IsKey = true, BqlField = typeof(ARBalances.customerID))]
			public virtual int? CustomerID { get; set; }
			#endregion
		}
		#endregion

		#region Public Views
		public PXSetup<ARSetup> ARSetup;

		public PXSelect<
			ARStatementCycle, 
			Where<
				ARStatementCycle.statementCycleId, Equal<Required<ARStatementCycle.statementCycleId>>>> 
			CyclesList;

		public PXSelect<ARRegister> Register; 
		#endregion

		[InjectDependency]
		public IFinPeriodRepository FinPeriodRepository { get; set; }

		#region External Processing Functions
		public static void ProcessCycles(StatementCycleProcessBO graph, ARStatementCycle aCycle)
		{
			if (aCycle.NextStmtDate == null)
			{
				return;
			}

			graph.Clear();

			ARStatementCycle cycle = graph.CyclesList.Select(aCycle.StatementCycleId);
			DateTime statementDate = aCycle.NextStmtDate ?? graph.Accessinfo.BusinessDate.Value;

			PXProcessing<ARStatementCycle>.SetCurrentItem(aCycle);

			graph.GenerateStatement(cycle, statementDate);
		}

		public static void RegenerateLastStatement(StatementCycleProcessBO graph, ARStatementCycle aCycle)
		{
			graph.Clear();
			ARStatementCycle cycle = graph.CyclesList.Select(aCycle.StatementCycleId);
			if (cycle.LastStmtDate != null)
			{
				DateTime stmtDate = (DateTime)cycle.LastStmtDate;
				graph.RegenerateLastStatement(cycle, stmtDate);
			}
		}

		private static IEnumerable<Customer> GetStatementCustomers(PXGraph graph, IEnumerable<Customer> customers)
		{
			List<Customer> result = new List<Customer>();

			foreach (int? customerID in customers
				.Select(customer => customer.StatementCustomerID)
				.Distinct())
			{
				IEnumerable<Customer> customersThatConsolidateToThisOne = PXSelect<
					Customer, 
					Where<
						Customer.statementCustomerID, Equal<Required<Customer.statementCustomerID>>>>
					.Select(graph, customerID)
					.RowCast<Customer>();

				result.AddRange(customersThatConsolidateToThisOne);
			}

			return result;
		}

		private class CustomerIDComparer : IEqualityComparer<Customer>
		{
			public bool Equals(Customer x, Customer y)
			{
				return x.BAccountID == y.BAccountID;
			}

			public int GetHashCode(Customer obj)
			{
				return obj.BAccountID.GetHashCode();
			}
		}

		/// <summary>
		/// After parent-child relationship is broken and a user wants to regenerate last statement
		/// for a customer that was in the family earlier, this method ensures that customers that 
		/// "were family" at the moment of previous statement generation will get into the family 
		/// for regeneration.
		/// </summary>
		/// <param name="customerFamily">
		/// The customer family collected on the basis of parent-child links between customers.
		/// </param>
		private static ICollection<Customer> PrependFamilyCustomersFromPreviousStatement(
			PXGraph graph, 
			IEnumerable<Customer> customerFamily, 
			DateTime previousStatementDate)
		{
			int[] customerIDs = customerFamily
				.Select(customer => customer.BAccountID.Value)
				.ToArray();

			IBqlSelect previousCustomersBase = new SelectFrom<Customer>
				// Collect customers from previous statements' ARStatement.statementCustomerID.
				.LeftJoin<ARStatement>
					.On<Customer.bAccountID.IsEqual<ARStatement.customerID>
						.And<ARStatement.statementDate.IsEqual<@P.AsDateTime>>
						.And<ARStatement.statementCustomerID.IsIn<@P.AsInt>>>
				.Where<ARStatement.statementDate.IsNotNull
					.Or<ARStatementAdjust.statementDate.IsNotNull>>();

			CustomerIDComparer customerIDComparer = new CustomerIDComparer();

					// Also collect customers from previous statement applications, because
					// e.g. on second re-generation the statement records have already lost
					// the information about the family.
			BqlCommand previousCustomersBaseAdjg = previousCustomersBase.AddNewJoin(typeof(LeftJoin<ARStatementAdjust,
				On<ARStatementAdjust.statementDate.IsEqual<@P.AsDateTime>
					.And<Customer.bAccountID.IsEqual<ARStatementAdjust.adjgCustomerID>>
					.And<ARStatementAdjust.adjdCustomerID.IsIn<@P.AsInt>>>>));

			// Single statement was splitted into two parts under AC-154357, MySql performance fix
			BqlCommand previousCustomersBaseAdjd = previousCustomersBase.AddNewJoin(typeof(LeftJoin<ARStatementAdjust,
				On<ARStatementAdjust.statementDate.IsEqual<@P.AsDateTime>
					.And<Customer.bAccountID.IsEqual<ARStatementAdjust.adjdCustomerID>>
					.And<ARStatementAdjust.adjgCustomerID.IsIn<@P.AsInt>>>>));

			return previousCustomersBaseAdjg
				.CreateView(graph)
				.SelectMulti(
					previousStatementDate, 
					customerIDs,
					previousStatementDate,
					customerIDs)
				.RowCast<Customer>()
			.Concat(previousCustomersBaseAdjd
				.CreateView(graph)
				.SelectMulti(
					previousStatementDate,
					customerIDs,
					previousStatementDate,
					customerIDs)
				.RowCast<Customer>())
			.Concat(customerFamily)
				.Distinct(customerIDComparer)
			.ToArray();
		}

		public static void RegenerateStatements(StatementCycleProcessBO graph, ARStatementCycle aCycle, IEnumerable<Customer> customers)
		{
			graph.Clear();

			// TODO: why not just use the statement cycle parameter 
			// passed, why should we force re-selection?
			// -
			ARStatementCycle cycle = graph.CyclesList.Select(aCycle.StatementCycleId);

			if (cycle.LastStmtDate != null)
			{
				DateTime stmtDate = (DateTime)cycle.LastStmtDate;

				StatementCreateBO statementGraph = CreateInstance<StatementCreateBO>();

				IEnumerable<IEnumerable<Customer>> customerFamilies = GetStatementCustomers(graph, customers)
					.GroupBy(customer => customer.StatementCustomerID)
					.Select(family => PrependFamilyCustomersFromPreviousStatement(graph, family, stmtDate))
					.OrderByDescending(family => family.Count);

				HashSet<int> processedCustomers = new HashSet<int>();

				foreach (IEnumerable<Customer> extendedFamily in customerFamilies)
				{
					// The trick with excluding processed customers is required when 
					// re-generating statements for customers that were a family during 
					// previous cycle, but are not now. The process of prepending previous
					// family customers may result multiple size families containing 
					// the same customers. We will only process the largest one, thus
					// avoiding lock violations.
					// -
					if (extendedFamily.Any(customer => processedCustomers.Contains(customer.BAccountID.Value)))
					{
						continue;
					}

					graph.GenerateStatementForCustomerFamily(
						statementGraph, 
						aCycle, 
						extendedFamily, 
						stmtDate, 
						true, 
						false);

					processedCustomers.UnionWith(extendedFamily.Select(customer => customer.BAccountID.Value));
				}
			}
		}

		public static void GenerateOnDemandStatement(
			StatementCycleProcessBO graph, 
			ARStatementCycle statementCycle, 
			Customer customer,
			DateTime statementDate)
		{
			graph.Clear();

			StatementCreateBO persistGraph = PXGraph.CreateInstance<StatementCreateBO>();

			IEnumerable<Customer> customerFamily = GetStatementCustomers(graph, customer.AsSingleEnumerable());

			graph.GenerateStatementForCustomerFamily(persistGraph, statementCycle, customerFamily, statementDate, true, true);
		}
		#endregion

		#region Internal Processing functions
		[Obsolete("This method is obsolete and will be removed in Acumatica 8. Use " + nameof(GenerateStatement) + " or " + nameof(RegenerateLastStatement) + " instead.")]
		protected virtual void GenerateStatement(ARStatementCycle cycle, DateTime statementDate, bool clearExisting)
		{
			if (clearExisting)
			{
				RegenerateLastStatement(cycle, statementDate);
			}
			else
			{
				GenerateStatement(cycle, statementDate);
			} 
		}

		protected virtual void GenerateStatement(ARStatementCycle cycle, DateTime statementDate)
		{
			bool hasInactiveBranches = PXSelect<Branch, Where<Branch.active, Equal<False>>>.Select(this).Any();
			Dictionary<int, HashSet<string>> inactiveBranchesOfCustomers = new Dictionary<int, HashSet<string>>();

			#region check inactive branches
			if (hasInactiveBranches)
			{
				PXSelectBase<Branch> viewInactiveBranchesOfCustomers =
					new PXSelectJoinGroupBy<Branch,
						InnerJoin<ARStatement, On<ARStatement.branchID, Equal<Branch.branchID>>>,
						Where<Branch.active, Equal<False>,
							And<ARStatement.statementDate, Equal<Required<ARStatement.statementDate>>,
							And<ARStatement.statementCycleId, Equal<Required<ARStatement.statementCycleId>>>>>,
						Aggregate<
							GroupBy<ARStatement.customerID,
							GroupBy<Branch.branchID,
							GroupBy<Branch.branchCD>>>>
						>(this);

				using (new PXFieldScope(viewInactiveBranchesOfCustomers.View, typeof(ARStatement.customerID), typeof(Branch.branchCD)))
				{
					int customerID;
					string branchCD;
					HashSet<string> setInactiveBranchesOfCustomers;
					foreach (PXResult<Branch, ARStatement> ba in viewInactiveBranchesOfCustomers.Select(cycle.LastStmtDate, cycle.StatementCycleId))
					{
						customerID = ((ARStatement)ba).CustomerID.Value;
						branchCD = ((Branch)ba).BranchCD;
						if (!inactiveBranchesOfCustomers.TryGetValue(customerID, out setInactiveBranchesOfCustomers))
						{
							setInactiveBranchesOfCustomers = new HashSet<string>();
							inactiveBranchesOfCustomers.Add(customerID, setInactiveBranchesOfCustomers);
						}
						if (!setInactiveBranchesOfCustomers.Contains(branchCD))
						{
							setInactiveBranchesOfCustomers.Add(branchCD);
						}
					}
				}
			}
			#endregion

			StatementCreateBO statementGraph = CreateInstance<StatementCreateBO>();

			IEnumerable<IEnumerable<Customer>> customerFamilies = CollectCustomerFamiliesForCycleProcessing(cycle, statementDate);

			ICollection<string> customersWithOnDemandStatementsOnDate = new List<string>();
			ICollection<string> customersWithExistingStatementsOnDate = new List<string>();
			HashSet<string> inactiveBranches = new HashSet<string>();

			foreach (IEnumerable<Customer> customerFamily in customerFamilies)
			{
				// If at least one of the customers in the family has on-demand statements
				// on the specified date, the statements will be re-generated for the whole
				// family.
				// -
				bool familyHasOnDemandStatementsOnDate = new PXSelect<
					ARStatement,
					Where<
						ARStatement.customerID, In<Required<ARStatement.customerID>>,
						And<ARStatement.onDemand, Equal<True>,
						And<ARStatement.statementDate, Equal<Required<ARStatement.statementDate>>>>>>(this)
					.Any(
						customerFamily.Select(customer => customer.BAccountID).ToArray(),
						statementDate);

				if (familyHasOnDemandStatementsOnDate)
				{
					customersWithOnDemandStatementsOnDate.Add(
						customerFamily.First().AcctCD.Trim());
				}

				IEnumerable<Customer> excludedCustomers = 
					customerFamily.Where(customer => customer.StatementLastDate >= statementDate);

				customersWithExistingStatementsOnDate.AddRange(
					excludedCustomers.Select(customer => customer.AcctCD));

				IEnumerable<Customer> neededCustomerFamily = customerFamily.Except(excludedCustomers);

				if (neededCustomerFamily.Count() > 0)
				{
					#region check inactive branches
					if (hasInactiveBranches)
					{
						HashSet<string> setInactiveBranches;
						foreach (Customer c in neededCustomerFamily)
						{
							if (inactiveBranchesOfCustomers.TryGetValue(c.BAccountID.Value, out setInactiveBranches))
							{
								if (setInactiveBranches.Count > 0)
								{
									PXTrace.WriteWarning(AR.Messages.DocumentsOfFollowingCustomersForInactiveBranchesHaveBeenExcludedFromPreparedStatements,
										c.AcctCD, 
										string.Join(", ", setInactiveBranches));
									inactiveBranches.AddRange(setInactiveBranches);
								}
							}
						}
					}
					#endregion

				GenerateStatementForCustomerFamily(
					statementGraph, 
					cycle, 
						neededCustomerFamily,
					statementDate, 
					clearExisting: familyHasOnDemandStatementsOnDate, 
					isOnDemand: false);
			}
			}

			if (customersWithOnDemandStatementsOnDate.Any())
			{
				PXTrace.WriteWarning(
					Messages.ExistingOnDemandStatementsForCustomersOverwritten,
					string.Join(", ", customersWithOnDemandStatementsOnDate));
			}

			if (customersWithExistingStatementsOnDate.Any())
			{
				PXTrace.WriteWarning(
					Messages.CustomersExcludedBecauseStatementsAlreadyExistForDate,
					string.Join(", ", customersWithExistingStatementsOnDate));

				PXProcessing<ARStatementCycle>.SetWarning(Messages.CustomersExcludedFromStatementGeneration);
			}

			if (inactiveBranches.Count > 0)
			{
				PXProcessing<ARStatementCycle>.SetWarning(
					string.Format(AR.Messages.DocumentsOfInactiveBranchHaveBeenExcludedFromPreparedStatements,
					string.Join(",", inactiveBranches))
				);
			}

			UpdateStatementCycleLastStatementDate(
				cycle, 
				statementDate);
		}

		protected virtual void RegenerateLastStatement(ARStatementCycle cycle, DateTime statementDate)
		{
			StatementCreateBO statementGraph = CreateInstance<StatementCreateBO>();

			IEnumerable<IEnumerable<Customer>> customerFamilies = CollectCustomerFamiliesForCycleProcessing(cycle, statementDate)
				// Customers who have been transferred from another statement cycle may have
				// last statement date later than the current statement date. They will be
				// excluded from processing.
				// -
				.Where(family => !family.Any(customer => customer.StatementLastDate > statementDate))
				.Select(family => PrependFamilyCustomersFromPreviousStatement(statementGraph, family, statementDate))
				.OrderByDescending(family => family.Count);

			HashSet<int> processedCustomers = new HashSet<int>();

			foreach (IEnumerable<Customer> extendedFamily in customerFamilies)
			{
				// The trick with excluding processed customers is required when 
				// re-generating statements for customers that were a family during 
				// previous cycle, but are not now. The process of prepending previous
				// family customers may result multiple size families containing 
				// the same customers. We will only process the largest one, thus
				// avoiding lock violations.
				// -
				if (extendedFamily.Any(customer => processedCustomers.Contains(customer.BAccountID.Value)))
				{
					continue;
				}

				GenerateStatementForCustomerFamily(
					statementGraph, 
					cycle, 
					extendedFamily, 
					statementDate, 
					clearExisting: true, 
					isOnDemand: false);

				processedCustomers.UnionWith(extendedFamily.Select(customer => customer.BAccountID.Value));
			}

			UpdateStatementCycleLastStatementDate(
				cycle, 
				statementDate);
		}

		protected virtual IEnumerable<ICollection<Customer>> CollectCustomerFamiliesForCycleProcessing(
			ARStatementCycle cycle, 
			DateTime statementDate)
		{
			ICollection<ICollection<Customer>> customerFamilies = new List<ICollection<Customer>>();

			PXSelectBase<Customer> selectCustomer = new PXSelectJoin<
				Customer,
					InnerJoin<CustomerMaster,
						On<CustomerMaster.bAccountID, Equal<Customer.statementCustomerID>>,
					LeftJoin<CustomerWithActiveBalance,
						On<CustomerWithActiveBalance.customerID, Equal<Customer.bAccountID>>>>,
				Where<
						CustomerMaster.statementCycleId, Equal<Required<Customer.statementCycleId>>,
							And < Where<Required<ARStatementCycle.printEmptyStatements>, Equal<True>,
								Or<CustomerWithActiveBalance.customerID, IsNotNull>>>>,
				OrderBy<
					Asc<Customer.statementCustomerID>>>
				(this);

			List<Customer> currentFamily = new List<Customer>();

			foreach (Customer customer in selectCustomer.Select(cycle.StatementCycleId, cycle.PrintEmptyStatements))
			{
				if (!currentFamily.Any() || currentFamily.First().StatementCustomerID == customer.StatementCustomerID)
				{
					currentFamily.Add(customer);
				}
				else
				{
					customerFamilies.Add(currentFamily);
					currentFamily = new List<Customer> { customer };
				}
			}

			if (currentFamily.Any())
			{
				customerFamilies.Add(currentFamily);
			}

			return customerFamilies;
		}

		/// <summary>
		/// Checks if all customers belonging to the specified statement cyclehave their 
		/// statement date updated (i.e. they have been properly processed by the
		/// <see cref="GenerateCustomerStatement(StatementCreateBO, ARStatementCycle, Customer, DateTime, 
		/// IDictionary{ARStatementKey, ARStatement}, IDictionary{ARStatementKey, ARStatement}, bool)"/> 
		/// function). If so, updates the <see cref="ARStatementCycle.LastStmtDate">last statement date</see> 
		/// of the statement cycle.
		/// </summary>
		protected virtual void UpdateStatementCycleLastStatementDate(ARStatementCycle cycle, DateTime statementDate, bool isOnDemand = false)
		{
			if (isOnDemand) return;

			// The INNER JOIN by the parent customer is required
			// because the parent customer's statement cycle ID value
			// determines the statement cycle for the whole family,
			// regardless of the children's values of that field.
			// -
			bool allCustomersProcessed = PXSelectReadonly2<
				Customer,
					InnerJoin<CustomerMaster,
					On<CustomerMaster.bAccountID, Equal<Customer.statementCustomerID>>,
				LeftJoin<CustomerWithActiveBalance,
					On<CustomerWithActiveBalance.customerID, Equal<Customer.bAccountID>>>>,
				Where<
					CustomerMaster.statementCycleId, Equal<Required<Customer.statementCycleId>>,
					And2<
						Where<Customer.statementLastDate, IsNull,
							Or<Customer.statementLastDate, Less<Required<Customer.statementLastDate>>>>,
						And<
						Where<Required<ARStatementCycle.printEmptyStatements>, Equal<True>,
							Or<CustomerWithActiveBalance.customerID, IsNotNull>>>>>>
				.SelectWindowed(this, 0, 1, cycle.StatementCycleId, statementDate, cycle.PrintEmptyStatements)
				.IsEmpty();

			// The statement cycle's last statement date is updated
			// only when all customers from the cycle have been
			// properly processed.
			// -
			if (allCustomersProcessed)
			{
				cycle.LastStmtDate = statementDate;

				CyclesList.Update(cycle);
				Actions.PressSave();
			}
		}

		/// <summary>
		/// Deletes the existing statements for the family on the specified date,
		/// depending on the settings for the current statement generation.
		/// </summary>
		/// <returns>
		/// Trace information about the deleted statements, which is required 
		/// to pass print / email counts to regenerated statements.
		/// </returns>
		protected virtual IDictionary<ARStatementKey, ARStatement> DeleteExistingStatements(
			IEnumerable<Customer> customerFamily,
			DateTime statementDate,
			bool clearExisting, 
			bool isOnDemand)
		{
			IEnumerable<ARStatement> deletedCustomerStatements = Enumerable.Empty<ARStatement>();

			foreach (Customer customer in customerFamily)
			{
				if (clearExisting || customer.StatementLastDate == statementDate)
				{
					if (isOnDemand)
					{
						EnsureNoRegularStatementExists(customer.BAccountID, statementDate);
					}

					deletedCustomerStatements = deletedCustomerStatements
						.Concat(DeleteCustomerStatement(customer, statementDate, isOnDemand));
				}
			}

			return deletedCustomerStatements.ToDictionary(statement => new ARStatementKey(statement));
		}

		protected virtual void ForceBeginningBalanceToPreviousStatementEndBalance(IEnumerable<ARStatement> familyStatements)
		{
			foreach (ARStatement statement in familyStatements)
			{
				ARStatement previousStatement = GetPreviousStatement(statement.BranchID, statement.CustomerID, statement.CuryID);

				if (previousStatement != null)
				{
					statement.BegBalance = previousStatement.EndBalance;
					statement.CuryBegBalance = previousStatement.CuryEndBalance;
				}

				ApplyFIFORule(statement, ARSetup.Current.AgeCredits == true);
			}
		}

		/// <summary>
		/// If <see cref="ARStatementCycle.PrintEmptyStatements"/> is set to
		/// <c>false</c>, marks all empty open item statements and empty 
		/// zero-balance BBF statements as "do not print" and "do not email".
		/// </summary>
		protected virtual void MarkEmptyStatementsForPrintingAndEmailing(
			ARStatementCycle statementCycle,
			IEnumerable<ARStatement> statements,
			IDictionary<ARStatementKey, ICollection<ARStatementDetail>> statementDetails,
			IDictionary<ARStatementKey, ICollection<ARStatementAdjust>> statementApplications)
		{
			if (statementCycle.PrintEmptyStatements == true) return;

			foreach (ARStatement statement in statements)
			{
				ARStatementKey statementKey = new ARStatementKey(statement);

				if (ARStatementProcess.IsEmptyStatement(
					statement, 
					statementDetails.GetValueOrEmpty(statementKey),
					statementApplications.GetValueOrEmpty(statementKey)))
				{
					statement.DontEmail = true;
					statement.DontPrint = true;
				}
			}
		}

		/// <param name="isOnDemand">
		/// If set to <c>true</c>, indicates that the statements to be persisted
		/// are on-demand statements, so that <see cref="Customer.statementLastDate"/>, 
		/// <see cref="ARRegister.statementDate"/>, and <see cref="ARAdjust.statementDate"/>
		/// will not be updated.
		/// </param>
		protected static bool PersistStatement(
			StatementCreateBO statementPersistGraph, 
			IEnumerable<ARStatement> statements, 
			IEnumerable<ARStatementDetail> statementDetails,
			IEnumerable<ARStatementAdjust> statementApplications)
		{
			statementPersistGraph.Clear();

			foreach (ARStatement statement in statements)
			{
				statementPersistGraph.Statement.Insert(statement);
			}

			foreach (ARStatementDetail statementDetail in statementDetails)
			{
				statementPersistGraph.StatementDetail.Insert(statementDetail);
			}

			foreach (ARStatementAdjust statementApplication in statementApplications)
			{
				statementPersistGraph.StatementApplications.Insert(statementApplication);
			}

			statementPersistGraph.Actions.PressSave();

			return true;
		}

		protected static void UpdateCustomersLastStatementDate(
			PXGraph statementPersistGraph,
			DateTime statementDate,
			int customerID) => 
				PXUpdate<
					Set<Override.Customer.statementLastDate, Required<Override.Customer.statementLastDate>>,
					Override.Customer,
					Where<
						Override.Customer.bAccountID, Equal<Required<Override.Customer.bAccountID>>,
						And<Where<
							Override.Customer.statementLastDate, IsNull,
							Or<Override.Customer.statementLastDate, Less<Required<Override.Customer.statementLastDate>>>>>>>
				.Update(
					statementPersistGraph, 
					statementDate, 
					customerID,
					statementDate);

		[Obsolete(Common.InternalMessages.MethodIsObsoleteAndWillBeRemoved2020R2)]
		protected static void UpdateARBalanceStatementNotRequired(
			PXGraph statementPersistGraph,
			int customerID,
			DateTime statementDate) =>
			PXUpdate<
				Set<ARBalances.statementRequired, False>,
				ARBalances,
				Where<ARBalances.customerID, Equal<Required<ARBalances.customerID>>,
					And<ARBalances.lastDocDate, LessEqual<Required<ARBalances.lastDocDate>>>>>
			.Update(
				statementPersistGraph,
				customerID,
				statementDate);

		protected static void UpdateARBalanceStatementNotRequired(
			PXGraph statementPersistGraph,
			ARStatement statement,
			DateTime statementDate) =>
			PXUpdate<
				Set<ARBalances.statementRequired, False>,
				ARBalances,
				Where<ARBalances.customerID, Equal<Required<ARBalances.customerID>>,
					And<ARBalances.branchID, Equal<Required<ARBalances.branchID>>,
					And<ARBalances.lastDocDate, LessEqual<Required<ARBalances.lastDocDate>>>>>>
			.Update(
				statementPersistGraph,
				statement.CustomerID.Value,
				statement.BranchID.Value,
				statementDate);


		/// <summary>
		/// Used for open item statement processing. Updates all documents of the customers that do not
		/// yet have an <see cref="ARRegister.StatementDate"/>, regardless of whether the document
		/// has got into statement. This is done so that if the customer switches to BBF statement
		/// type, the documents that didn't get into BBF statements don't suddenly appear in the
		/// new statements.
		/// </summary>
		protected static void UpdateDocumentsLastStatementDate(
			PXGraph statementPersistGraph,
			DateTime? statementDate,
			int? customerID) =>
			PXUpdate<
				Set<ARRegister.statementDate, Required<ARRegister.statementDate>>,
				ARRegister,
				Where<
					ARRegister.statementDate, IsNull,
					And<ARRegister.docDate, LessEqual<Required<ARRegister.docDate>>,
					And<ARRegister.customerID, Equal<Required<ARRegister.customerID>>>>>>
			.Update(
				statementPersistGraph,
				statementDate,
				statementDate,
				customerID);

		/// <summary>
		/// Used for balance brought forward statements. Updates <see cref="ARRegister.StatementDate"/>
		/// for documents corresponding to <see cref="ARStatementDetail"/> records of a given statement,
		/// so that these documents are not included into future BBF statements.
		/// </summary>
		protected static void UpdateDocumentsLastStatementDate(
			PXGraph statementPersistGraph,
			ARStatement statement) => 
				PXUpdateJoin<
					Set<ARRegister.statementDate, Required<ARRegister.statementDate>>,
					ARRegister,
						InnerJoin<ARStatementDetail,
							On<ARRegister.docType, Equal<ARStatementDetail.docType>,
							And<ARRegister.refNbr, Equal<ARStatementDetail.refNbr>>>>,
					Where<
						ARRegister.statementDate, IsNull,
						And<ARStatementDetail.branchID, Equal<Required<ARStatementDetail.branchID>>,
						And<ARStatementDetail.curyID, Equal<Required<ARStatementDetail.curyID>>,
						And<ARStatementDetail.customerID, Equal<Required<ARStatementDetail.customerID>>,
						And<ARStatementDetail.statementDate, Equal<Required<ARStatementDetail.statementDate>>>>>>>>
				.Update(
					statementPersistGraph, 
					statement.StatementDate,
					statement.BranchID, 
					statement.CuryID, 
					statement.CustomerID, 
					statement.StatementDate);

		/// <summary>
		/// Used for statement re-generation, when the old statement objects are deleted.
		/// In order for the documents to re-appear in the new statement, their statement
		/// date needs to be reset to <c>null</c>.
		/// </summary>
		/// <remarks>
		/// It is insufficient to just update documents for which matching statement details 
		/// exist, because in case of switching from Open Item to BBF, some documents could have
		/// been closed at the moment of Open Item processing, and no <see cref="ARStatementDetail"/> 
		/// was created for them.
		/// </remarks>
		protected static void ResetDocumentsLastStatementDate(
			PXGraph statementPersistGraph,
			DateTime? statementDate,
			int? customerID) =>
			PXUpdate<
				Set<ARRegister.statementDate, Null>,
				ARRegister,
				Where<
					ARRegister.statementDate, Equal<Required<ARRegister.statementDate>>,
					And<ARRegister.docDate, LessEqual<Required<ARRegister.docDate>>,
					And<ARRegister.customerID, Equal<Required<ARRegister.customerID>>>>>>
			.Update(
				statementPersistGraph,
				statementDate,
				statementDate,
				customerID);

		/// <summary>
		/// Used for open item statements. Updates <see cref="ARAdjust.StatementDate"/> for all relevant
		/// applications of the customer so that they don't suddenly show up in customer statement when
		/// the user switches the customer to BBF.
		/// </summary>
		protected static void UpdateApplicationsLastStatementDate(
			PXGraph statementPersistGraph,
			DateTime? statementDate,
			int? customerID) =>
			PXUpdate<
				Set<ARAdjust.statementDate, Required<ARAdjust.statementDate>>,
				ARAdjust,
				Where<
					ARAdjust.statementDate, IsNull,
					And<ARAdjust.adjgDocDate, LessEqual<Required<ARRegister.docDate>>,
					And<ARAdjust.customerID, Equal<Required<ARAdjust.customerID>>>>>>
			.Update(
				statementPersistGraph,
				statementDate,
				statementDate,
				customerID);

		/// <summary>
		/// Used for balance brought forward statements. Updates <see cref="ARAdjust.StatementDate"/>
		/// for applications corresponding to <see cref="ARStatementAdjust"/> records of a given statement,
		/// so that these applications are not included into future statements.
		/// </summary>
		protected static void UpdateApplicationsLastStatementDate(
			PXGraph statementPersistGraph,
			ARStatement statement) =>
				PXUpdateJoin<
					Set<ARAdjust.statementDate, Required<ARAdjust.statementDate>>,
					ARAdjust,
						InnerJoin<ARStatementAdjust,
							On<ARAdjust.adjgDocType, Equal<ARStatementAdjust.adjgDocType>,
							And<ARAdjust.adjgRefNbr, Equal<ARStatementAdjust.adjgRefNbr>,
							And<ARAdjust.adjdDocType, Equal<ARStatementAdjust.adjdDocType>,
							And<ARAdjust.adjdRefNbr, Equal<ARStatementAdjust.adjdRefNbr>,
							And<ARAdjust.adjNbr, Equal<ARStatementAdjust.adjNbr>>>>>>>,
					Where<
						ARAdjust.statementDate, IsNull,
						And<ARStatementAdjust.branchID, Equal<Required<ARStatementAdjust.branchID>>,
						And<ARStatementAdjust.curyID, Equal<Required<ARStatementAdjust.curyID>>,
						And<ARStatementAdjust.customerID, Equal<Required<ARStatementAdjust.customerID>>,
						And<ARStatementAdjust.statementDate, Equal<Required<ARStatementAdjust.statementDate>>>>>>>>
				.Update(
					statementPersistGraph,
					statement.StatementDate,
					statement.BranchID,
					statement.CuryID,
					statement.CustomerID,
					statement.StatementDate);

		/// <summary>
		/// Gets a value indicating whether an invoice is a first
		/// (master) invoice in a multiple installment sequence.
		/// </summary>
		protected static bool IsMultipleInstallmentMaster(ARInvoice invoice) 
			=> invoice.InstallmentCntr > 0;

		#endregion
		#region Utility Functions
		[Obsolete("This method has been deprecated and will be removed in Acumatica ERP 8.0. Use " + nameof(CombineStatementCustomizable) + " instead.")]
		protected static void Copy(ARStatement aDest, ARStatementKey key)
		{
			aDest.BranchID = key.BranchID;
			aDest.CuryID = key.CurrencyID;
			aDest.CustomerID = key.CustomerID;
		}

		[Obsolete("This method has been deprecated and will be removed in Acumatica ERP 8.0. Use " + nameof(CombineStatementCustomizable) + " instead.")]
		protected static void Copy(ARStatement aDest, ARStatementCycle aSrc)
		{
			aDest.StatementCycleId = aSrc.StatementCycleId;
			aDest.AgeDays00 = 0;
			aDest.AgeDays01 = aSrc.AgeDays00;
			aDest.AgeDays02 = aSrc.AgeDays01;
			aDest.AgeDays03 = aSrc.AgeDays02;
		}

		[Obsolete("This method has been deprecated and will be removed in Acumatica ERP 8.0. Use " + nameof(CombineStatementCustomizable) + " instead.")]
		protected static void Copy(ARStatement aDest, Customer aSrc)
		{
			aDest.CustomerID = aSrc.BAccountID;
			aDest.StatementCustomerID = aSrc.StatementCustomerID;
			aDest.StatementType = aSrc.StatementType;
			aDest.StatementCycleId = aSrc.StatementCycleId;
			aDest.DontPrint = aSrc.PrintStatements != true;
			aDest.DontEmail = aSrc.SendStatementByEmail != true;
		}

		[Obsolete("This method has been deprecated and will be removed in Acumatica ERP 8.0. Use " + nameof(SetStatementAgeDaysToZero) + " and " + nameof(SetStatementAgeBalancesToZero) + " instead.")]
		protected static void Clear(ARStatement aDest)
		{
			aDest.AgeBalance00 = aDest.AgeBalance01 = aDest.AgeBalance02 = aDest.AgeBalance03 = aDest.AgeBalance04 = Decimal.Zero;
			aDest.CuryAgeBalance00 = aDest.CuryAgeBalance01 = aDest.CuryAgeBalance02 = aDest.CuryAgeBalance03 = aDest.CuryAgeBalance04 = Decimal.Zero;
			aDest.BegBalance = aDest.EndBalance = Decimal.Zero;
			aDest.CuryBegBalance = aDest.CuryEndBalance = Decimal.Zero;
		}

		protected static void Recalculate(ARStatement aDest)
		{
			if (aDest.StatementType == ARStatementType.BalanceBroughtForward)
			{
				aDest.CuryEndBalance += aDest.CuryBegBalance;
				aDest.EndBalance += aDest.BegBalance;
			}
		}

		[Obsolete("This method has been deprecated and will be removed in Acumatica ERP 8.0. Use " + nameof(CombineStatementDetailCustomizable) + " instead.")]
		protected static void Copy(ARStatementDetail aDest, ARRegister aSrc)
		{
			aDest.DocType = aSrc.DocType;
			aDest.RefNbr = aSrc.RefNbr;
			aDest.BranchID = aSrc.BranchID;
			aDest.DocBalance = aSrc.DocBal;
			aDest.CuryDocBalance = aSrc.CuryDocBal;
			aDest.IsOpen = aSrc.OpenDoc;
		}

		[Obsolete("This method has been deprecated and will be removed in Acumatica ERP 8.0. Use " + nameof(CombineStatementDetailCustomizable) + " instead.")]
		protected static void Copy(ARStatementDetail aDest, ARStatement aSrc)
		{
			aDest.CustomerID = aSrc.CustomerID;
			aDest.CuryID = aSrc.CuryID;
			aDest.StatementDate = aSrc.StatementDate;
			//BranchID is copied earlier - from the document to both StatementHeader and details
		}

		protected static void ApplyFIFORule(ARStatement aDest, bool aAgeCredits)
		{
			//Apply Extra payment in the correct sequence - first to oldest, then - to closer debts
			//We assume, that allpayments are already applyed to oldest -this function propagates them up.
			if (!aAgeCredits)
			{
				if (aDest.AgeBalance04 < 0)//|| (aDest.AgeDays03 == null)) //Extra payments
				{
					aDest.AgeBalance03 += aDest.AgeBalance04;
					aDest.AgeBalance04 = Decimal.Zero;
					aDest.CuryAgeBalance03 += aDest.CuryAgeBalance04;
					aDest.CuryAgeBalance04 = Decimal.Zero;

				}
				if (aDest.AgeBalance03 < 0)//|| (aDest.AgeDays02 == null))
				{
					aDest.AgeBalance02 += aDest.AgeBalance03;
					aDest.AgeBalance03 = Decimal.Zero;
					aDest.CuryAgeBalance02 += aDest.CuryAgeBalance03;
					aDest.CuryAgeBalance03 = Decimal.Zero;
				}
				if (aDest.AgeBalance02 < 0)//|| (aDest.AgeDays01 == null))
				{
					aDest.AgeBalance01 += aDest.AgeBalance02;
					aDest.AgeBalance02 = Decimal.Zero;
					aDest.CuryAgeBalance01 += aDest.CuryAgeBalance02;
					aDest.CuryAgeBalance02 = Decimal.Zero;
				}
				if (aDest.AgeBalance01 < 0)
				{
					aDest.AgeBalance00 += aDest.AgeBalance01;
					aDest.AgeBalance01 = Decimal.Zero;
					aDest.CuryAgeBalance00 += aDest.CuryAgeBalance01;
					aDest.CuryAgeBalance01 = Decimal.Zero;
				}
			}
		}
		#endregion

		#region Statements Generation

		protected virtual void GenerateStatementForCustomerFamily(
			StatementCreateBO persistGraph, 
			ARStatementCycle statementCycle, 
			IEnumerable<Customer> customerFamily, 
			DateTime statementDate, 
			bool clearExisting,
			bool isOnDemand)
		{
			IDictionary<ARStatementKey, ARStatement> familyStatements = 
				new Dictionary<ARStatementKey, ARStatement>();

			IDictionary<ARStatementKey, ICollection<ARStatementDetail>> familyStatementDetails = 
				new Dictionary<ARStatementKey, ICollection<ARStatementDetail>>();

			IDictionary<ARStatementKey, ICollection<ARStatementAdjust>> familyStatementApplications = 
				new Dictionary<ARStatementKey, ICollection<ARStatementAdjust>>();

			using (PXTransactionScope ts = new PXTransactionScope())
			{
				IDictionary<ARStatementKey, ARStatement> deletedFamilyStatementsTrace = DeleteExistingStatements(
					customerFamily, 
					statementDate,
					clearExisting,
					isOnDemand);

				foreach (Customer customer in customerFamily)
				{
					GenerateCustomerStatement(
						statementCycle, 
						customer, 
						statementDate, 
						familyStatements,
						familyStatementDetails,
						familyStatementApplications,
						deletedFamilyStatementsTrace, 
						isOnDemand);
				}

				ForceBeginningBalanceToPreviousStatementEndBalance(familyStatements.Values);

				MarkEmptyStatementsForPrintingAndEmailing(
					statementCycle,
					familyStatements.Values,
					familyStatementDetails,
					familyStatementApplications);

				PersistStatement(
					persistGraph, 
					familyStatements.Values, 
					familyStatementDetails.Values.SelectMany(sequence => sequence), 
					familyStatementApplications.Values.SelectMany(sequence => sequence));

				if (!isOnDemand)
				{
					foreach (Customer customer in customerFamily)
					{
						UpdateCustomersLastStatementDate(
							persistGraph, 
							statementDate, 
							customer.BAccountID.Value);

						if (GetStatementType(customer) == ARStatementType.OpenItem)
						{
							UpdateDocumentsLastStatementDate(persistGraph, statementDate, customer.BAccountID);
							UpdateApplicationsLastStatementDate(persistGraph, statementDate, customer.BAccountID);
						}
					}

					foreach (ARStatement statement in familyStatements.Values)
					{
						UpdateDocumentsLastStatementDate(persistGraph, statement);
						UpdateApplicationsLastStatementDate(persistGraph, statement);

						ARStatementKey statementKey = new ARStatementKey(statement);
						if (ARStatementProcess.IsEmptyStatement(
							statement,
							familyStatementDetails.GetValueOrEmpty(statementKey),
							familyStatementApplications.GetValueOrEmpty(statementKey)))
						{
							UpdateARBalanceStatementNotRequired(persistGraph, statement, statementDate);
						}
					}
				}

				ts.Complete();
			}
		}

		private string GetStatementType(Customer customer)
			=> customer.StatementChild == true
			? PXSelect<
					Customer,
					Where<Customer.bAccountID, Equal<Required<Customer.bAccountID>>>>
					.Select(this, customer.StatementCustomerID)
					.RowCast<Customer>()
					.FirstOrDefault()
					?.StatementType
			: customer.StatementType;

		/// <summary>
		/// Returns the last statement generated for the specified customer,
		/// branch, and currency. Excludes on-demand statements.
		/// In case no last statement is present, returns <c>null</c>.
		/// </summary>
		private ARStatement GetPreviousStatement(int? branchID, int? customerID, string currencyID) =>
			PXSelectJoin<ARStatement,
				InnerJoin<Branch,
					On<Branch.branchID, Equal<ARStatement.branchID>, And<Branch.active, Equal<True>>>>,
				Where<
					ARStatement.branchID, Equal<Required<ARStatement.branchID>>,
					And<ARStatement.customerID, Equal<Required<ARStatement.customerID>>,
					And<ARStatement.curyID, Equal<Required<ARStatement.curyID>>,
					And<ARStatement.onDemand, Equal<False>>>>>,
				OrderBy<
					Desc<ARStatement.statementDate>>>
			.SelectWindowed(this, 0, 1, branchID, customerID, currencyID);

		[Obsolete("This method is not used anymore and will be removed in Acumatica 8.0. Use other method overloads.")]
		protected virtual void GenerateCustomerStatement(
			StatementCreateBO persistGraph,
			ARStatementCycle statementCycle,
			Customer customer,
			DateTime statementDate,
			IDictionary<Tuple<int, string, int>, ARStatement> statementsTrace)
		{
			throw new NotImplementedException();
		}

		protected virtual void GenerateCustomerStatement(
			ARStatementCycle statementCycle, 
			Customer customer, 
			DateTime statementDate,
			IDictionary<ARStatementKey, ARStatement> familyStatements,
			IDictionary<ARStatementKey, ICollection<ARStatementDetail>> familyStatementDetails,
			IDictionary<ARStatementKey, ICollection<ARStatementAdjust>> familyStatementApplications,
			IDictionary<ARStatementKey, ARStatement> deletedFamilyStatementsTrace,
			bool isOnDemand)
		{
			ICollection<StatementDocument> customerDocumentsToAge = new List<StatementDocument>();

			bool ageCredits = ARSetup.Current.AgeCredits == true;
			bool isBalanceBroughtForwardStatement = customer.StatementType == ARStatementType.BalanceBroughtForward;
			bool isOpenItemStatement = customer.StatementType == ARStatementType.OpenItem;

			foreach (DocumentWithApplications result in GetDocumentsWithApplicationsToDate(customer, statementDate))
			{
				ARRegister document = result.Document;
				ARInvoice documentAsInvoice = result.DocumentAsInvoice;

				if (IsMultipleInstallmentMaster(documentAsInvoice)) continue;

				// We need to know the actual balance on the given date
				// for aging purposes - just using document balance fields
				// may be misleading in case of applications later than the
				// statement date. 
				// -
				CalculateBalanceOnStatementDate(
					document, 
					documentAsInvoice, 
					result.Applications);

				if (document.HasBalance())
				{
					customerDocumentsToAge.Add(result);
				}

				ARStatement statement = GetOrAddStatementForDocument(
					familyStatements,
					deletedFamilyStatementsTrace,
					document,
					customer,
					statementCycle,
					statementDate,
					isOnDemand);

				if (isBalanceBroughtForwardStatement && document.StatementDate == null
					|| isOpenItemStatement && document.HasBalance())
				{
					AdjustStatementEndBalance(statement, document);

					ICollection<ARStatementDetail> statementDetails = familyStatementDetails.GetOrAdd(
						new ARStatementKey(statement),
						() => new List<ARStatementDetail>());

					statementDetails.Add(CombineStatementDetailCustomizable(statement, document));
				}
			}

			// Workaround for AC-64681 - because there is no application
			// from void payment to the payment, we always want both documents
			// to be present in the aging collection for correct Amount Due
			// calculation, so that the two documents' amounts cancel each other. 
			// This whole block must be removed once _AC-84653_ is implemented. 
			// The ugliness of this crutch is both intentional and unavoidable.
			// -
			if (isBalanceBroughtForwardStatement)
			{
				IDictionary<DocumentKey, StatementDocument> openDocumentsDictionary =
					customerDocumentsToAge.ToDictionary(document => new DocumentKey(document.Document));

				openDocumentsDictionary
					.Values
					.Where(document =>
						document.Document.DocType == ARDocType.VoidPayment
						&& !openDocumentsDictionary.ContainsKey(
							new DocumentKey(ARDocType.Payment, document.Document.RefNbr))
						&& !openDocumentsDictionary.ContainsKey(
							new DocumentKey(ARDocType.Prepayment, document.Document.RefNbr)))
					.ForEach(document =>
					{
						ARPayment voidedPayment = PXSelect<
							ARPayment,
							Where<
								ARPayment.docType, In<Required<ARPayment.docType>>,
								And<ARPayment.refNbr, Equal<Required<ARPayment.refNbr>>,
								And<ARPayment.released, Equal<True>,
								And<ARPayment.voided, Equal<True>>>>>>
							.Select(
								this,
								new string[] { ARDocType.Payment, ARDocType.Prepayment },
								document.Document.RefNbr);

						voidedPayment.DocBal = voidedPayment.OrigDocAmt;
						voidedPayment.CuryDocBal = voidedPayment.CuryOrigDocAmt;

						customerDocumentsToAge.Add(new StatementDocument
						{
							Document = voidedPayment,
							DocumentAsInvoice = new ARInvoice(),
						});
					});

				openDocumentsDictionary
					.Values
					.Where(document =>
						document.Document.DocType == ARDocType.VoidRefund
						&& !openDocumentsDictionary.ContainsKey(
							new DocumentKey(ARDocType.Refund, document.Document.RefNbr)))
					.ForEach(document =>
					{
						ARPayment voidedRefund = PXSelect<
							ARPayment,
							Where<
								ARPayment.docType, Equal<ARDocType.refund>,
								And<ARPayment.refNbr, Equal<Required<ARPayment.refNbr>>,
								And<ARPayment.released, Equal<True>,
								And<ARPayment.voided, Equal<True>>>>>>
							.Select(
								this,
								document.Document.RefNbr);

						voidedRefund.DocBal = voidedRefund.OrigDocAmt;
						voidedRefund.CuryDocBal = voidedRefund.CuryOrigDocAmt;

						customerDocumentsToAge.Add(new StatementDocument
						{
							Document = voidedRefund,
							DocumentAsInvoice = new ARInvoice(),
						});
					});
			}

			AccumulateAgeBalancesIntoStatements(
				statementCycle,
				statementDate,
				customerDocumentsToAge,
				familyStatements,
				ageCredits);

			if (isBalanceBroughtForwardStatement)
			{
				foreach (ApplicationWithCurrencyAndAdjdCustomer applicationWithCurrency in GetUnreportedApplications(customer, statementDate))
				{
					ARAdjust application = applicationWithCurrency.Application;

					string adjustingCurrencyID = applicationWithCurrency.AdjustingCurrencyID;
					string adjustedCurrencyID = applicationWithCurrency.AdjustedCurrencyID;

					ARStatementKey[] applicableStatementKeys;

					if (applicationWithCurrency.AdjdCustomer?.ConsolidateStatements == false
						&& application.CustomerID.Value != application.AdjdCustomerID.Value)
					{
						applicableStatementKeys = new[]
						{
							new ARStatementKey(application.AdjgBranchID.Value, adjustingCurrencyID, application.CustomerID.Value, statementDate)
						};
					}
					else
					{
						applicableStatementKeys = new[]
						{
							new ARStatementKey(application.AdjgBranchID.Value, adjustingCurrencyID, application.CustomerID.Value, statementDate),
							new ARStatementKey(application.AdjdBranchID.Value, adjustedCurrencyID, application.AdjdCustomerID.Value, statementDate)
						};
						}

					IEnumerable<ARStatement> applicableStatements =
						applicableStatementKeys
						.Distinct()
						.Select(statementKey => GetOrAddStatement(
							familyStatements,
							statementKey,
							statementCycle,
							customer,
							isOnDemand));

					foreach (ARStatement statement in applicableStatements)
					{
						ICollection<ARStatementAdjust> statementApplications = familyStatementApplications.GetOrAdd(
							new ARStatementKey(statement),
							() => new List<ARStatementAdjust>());

						ARStatementAdjust statementApplication = CombineStatementApplicationCustomizable(
							statement, 
							application,
							adjustingCurrencyID,
							adjustedCurrencyID);

						statementApplications.Add(statementApplication);

						SetPreviousStatementInfo(statement, deletedFamilyStatementsTrace);
						AdjustStatementEndBalance(statement, statementApplication);
					}
				}
			}

			PXSelectBase<ARStatement> selectAllPreviousByCustomer = 
				new PXSelectJoin<ARStatement,
				InnerJoin<Branch, 
					On<Branch.branchID,Equal<ARStatement.branchID>, And<Branch.active, Equal<True>>>>,
				Where<ARStatement.customerID, Equal<Required<ARStatement.customerID>>,
					And<ARStatement.onDemand, Equal<False>>>,
				OrderBy<
					Asc<ARStatement.curyID,
					Desc<ARStatement.statementDate>>>>
				(this);

			// Merge with previous statements - is needed for Balance Brought Forward.
			// -
			IDictionary<ARStatementKey, DateTime> lastStatementDates = new Dictionary<ARStatementKey, DateTime>();

			foreach (ARStatement statement in selectAllPreviousByCustomer.Select(customer.BAccountID))
			{
				ARStatementKey statementKey = new ARStatementKey(
					statement.BranchID.Value, 
					statement.CuryID, 
					customer.BAccountID.Value,
					statementDate);

				if (lastStatementDates.ContainsKey(statementKey)
					&& lastStatementDates[statementKey] > statement.StatementDate)
				{
					continue;
				}

				ARStatement header = GetOrAddStatement(
					familyStatements, 
					statementKey, 
					statementCycle, 
					customer, 
					isOnDemand);
				
				header.BegBalance = statement.EndBalance;
				header.CuryBegBalance = statement.CuryEndBalance;

				Recalculate(header);

				lastStatementDates[statementKey] = statement.StatementDate.Value;
			}

			if (isOnDemand && familyStatements.Values.Count == 0)
			{
				ARStatementKey statementKey = new ARStatementKey(
					Accessinfo.BranchID.Value,
					customer.CuryID ?? 
						PXSelect<Branch, Where<Branch.branchID, Equal<Required<Branch.branchID>>>>
							.Select(this, Accessinfo.BranchID.Value)
							.RowCast<Branch>().FirstOrDefault<Branch>().BaseCuryID,
					customer.BAccountID.Value,
					statementDate);

				ARStatement header = GetOrAddStatement(
					familyStatements,
					statementKey,
					statementCycle,
					customer,
					isOnDemand);
			}
		}

		private ARStatement GetOrAddStatementForDocument(
			IDictionary<ARStatementKey, ARStatement> familyStatements, 
			IDictionary<ARStatementKey, ARStatement> deletedStatementsTrace,
			ARRegister document, 
			Customer customer, 
			ARStatementCycle statementCycle, 
			DateTime statementDate, 
			bool isOnDemand)
		{
			ARStatementKey statementKey = new ARStatementKey(
				document.BranchID.Value,
				document.CuryID,
				customer.BAccountID.Value,
				statementDate);

			ARStatement statement = GetOrAddStatement(
				familyStatements,
				statementKey,
				statementCycle,
				customer,
				isOnDemand);

			SetPreviousStatementInfo(statement, deletedStatementsTrace);

			// Ensure the existence of statement object for the parent customer
			// in case the parent customer does not have his own relevant 
			// documents to be processed.
			// -
			if (customer.BAccountID != customer.StatementCustomerID)
			{
				ARStatementKey parentStatementKey = new ARStatementKey(
					document.BranchID.Value,
					document.CuryID,
					customer.StatementCustomerID.Value,
					statementDate);

				ARStatement statementForParent = GetOrAddStatement(
					familyStatements,
					parentStatementKey,
					statementCycle,
					customer,
					isOnDemand);

				SetPreviousStatementInfo(statementForParent, deletedStatementsTrace);
			}

			return statement;
		}

		/// <summary>
		/// Represents a document to be potentially reported in
		/// a customer statement. Consists of the <see cref="ARRegister"/>
		/// part and an optional <see cref="ARInvoice"/> part, which 
		/// remains empty for payments.
		/// </summary>
		protected class StatementDocument
		{
			public ARRegister Document { get; set; }
			public ARInvoice DocumentAsInvoice { get; set; }
		}

		/// <summary>
		/// Represents a customer document along with its
		/// incoming and outgoing applications.
		/// </summary>
		protected class DocumentWithApplications : StatementDocument
		{
			public IEnumerable<ARAdjust> Applications { get; set; }
		}

		/// <summary>
		/// Represents a document application along with
		/// the currency ID of the adjusting / adjusted document.
		/// </summary>
		protected class ApplicationWithCurrency
		{
			public ARAdjust Application { get; set; }
			public string AdjustingCurrencyID { get; set; }
			public string AdjustedCurrencyID { get; set; }
		}

		protected class ApplicationWithCurrencyAndAdjdCustomer : ApplicationWithCurrency
		{
			public Customer AdjdCustomer { get; set; }
		}

		private void FillDictionaryDocumentsWithApplicationsToDate(
			Customer customer, 
			DateTime statementDate,
			IDictionary<DocumentKey, DocumentWithApplications> DocumentsWithApplications,
			bool getPaymentApplications)
		{
			PXSelectBase<ARRegister> documentsWithApplicationsView;

			if (getPaymentApplications)
		{
				documentsWithApplicationsView = new PXSelectJoin<
				ARRegister,
					InnerJoin<Branch, 
							On<Branch.branchID, Equal<ARRegister.branchID>, And<Branch.active,Equal<True>>>,
					LeftJoin<ARInvoice,
						On<ARInvoice.docType, Equal<ARRegister.docType>,
						And<ARInvoice.refNbr, Equal<ARRegister.refNbr>>>,
					// Incoming and outgoing released applications.
					// -
					LeftJoin<ARAdjust,
						On<ARAdjust.released, Equal<True>,
						And<ARAdjust.adjgDocDate, LessEqual<Required<ARAdjust.adjgDocDate>>,
						And<ARAdjust.adjgDocType, Equal<ARRegister.docType>,
						And<ARAdjust.adjgRefNbr, Equal<ARRegister.refNbr>,
						// InitialApplication should be excluded from one side,
						// otherwise it will be processed twice.
						// 
						And<ARAdjust.isInitialApplication, NotEqual<True>>>>>>>>>,
				Where2<
					IsNotSelfApplying<ARRegister.docType>,
					And<ARRegister.customerID, Equal<Required<ARRegister.customerID>>,
					And<ARRegister.released, Equal<True>,
					And<ARRegister.docDate, LessEqual<Required<ARRegister.docDate>>>>>>>(this);
			}
			else
			{
				documentsWithApplicationsView = new PXSelectJoin<
					ARRegister,
						InnerJoin<Branch,
							On<Branch.branchID, Equal<ARRegister.branchID>, And<Branch.active, Equal<True>>>,
						LeftJoin<ARInvoice,
							On<ARInvoice.docType, Equal<ARRegister.docType>,
							And<ARInvoice.refNbr, Equal<ARRegister.refNbr>>>,
						// Incoming and outgoing released applications.
						// -
						LeftJoin<ARAdjust,
							On<ARAdjust.released, Equal<True>,
							And<ARAdjust.adjgDocDate, LessEqual<Required<ARAdjust.adjgDocDate>>,
							And<ARAdjust.adjdDocType, Equal<ARRegister.docType>,
							And<ARAdjust.adjdRefNbr, Equal<ARRegister.refNbr>>>>>>>>,
					Where2<
						IsNotSelfApplying<ARRegister.docType>,
						And<ARRegister.customerID, Equal<Required<ARRegister.customerID>>,
						And<ARRegister.released, Equal<True>,
						And<ARRegister.docDate, LessEqual<Required<ARRegister.docDate>>>>>>>(this);
			}

			if (customer.StatementType == ARStatementType.OpenItem)
			{
				documentsWithApplicationsView.WhereAnd<Where<
					// For Open Item statements, we exclude documents that are
					// definitely closed by the statement calculation date.
					// -
					ARRegister.closedTranPeriodID, IsNull,
					Or<ARRegister.closedTranPeriodID, GreaterEqual<Required<ARRegister.closedTranPeriodID>>>>>();
			}
			else
			{
				documentsWithApplicationsView.WhereAnd<Where<
					// For BBF statements, we should fetch even closed documents
					// if they are not yet reported.
					// -
					ARRegister.closedTranPeriodID, IsNull,
					Or<ARRegister.closedTranPeriodID, GreaterEqual<Required<ARRegister.closedTranPeriodID>>,
					Or<ARRegister.statementDate, IsNull>>>>();
			}

			// Due to a bug in the platform AC-104654 default sorting by keys is not applied implicitly -> add it explicitly
			documentsWithApplicationsView.OrderByNew<OrderBy<Asc<ARRegister.docType, Asc<ARRegister.refNbr>>>>();

			object[] queryParameters = new object[]
			{
				statementDate,
				customer.BAccountID,
				statementDate,
				FinPeriodRepository.GetPeriodIDFromDate(statementDate, FinPeriod.organizationID.MasterValue),
			};

			ARRegister previousDocument = null;
			ARInvoice previousDocumentAsInvoice = null;
			List<ARAdjust> documentApplications = null;

			foreach (PXResult<ARRegister, Branch, ARInvoice, ARAdjust> result in
				documentsWithApplicationsView.Select(queryParameters.ToArray()))
			{
				ARRegister currentDocument = result;
				ARInvoice currentDocumentAsInvoice = result;
				ARAdjust currentApplication = result;

				if (currentDocument.DocType != previousDocument?.DocType
					|| currentDocument.RefNbr != previousDocument?.RefNbr)
				{
					// Since the records in the result set are sorted by keys, 
					// the truth of the above condition means that the document
					// has changed, hence we need to return the previous one.
					// -
					if (previousDocument != null)
					{
						DocumentKey key = new DocumentKey(previousDocument.DocType, previousDocument.RefNbr);
						DocumentWithApplications documentWithApplications;
						if (DocumentsWithApplications.TryGetValue(key, out documentWithApplications))
						{
							documentWithApplications.Applications = documentWithApplications.Applications.Concat(documentApplications);
						}
						else
						{
							DocumentsWithApplications.Add(
								key,
								new DocumentWithApplications
						{
							Document = previousDocument,
							DocumentAsInvoice = previousDocumentAsInvoice,
							Applications = documentApplications,
								});
						}
					}

					previousDocument = currentDocument;
					previousDocumentAsInvoice = currentDocumentAsInvoice;
					documentApplications = new List<ARAdjust>();
				}

				if (currentApplication.AreAllKeysFilled(this))
				{
					documentApplications.Add(currentApplication);
				}
			}

			if (previousDocument != null)
			{
				DocumentKey key = new DocumentKey(previousDocument.DocType, previousDocument.RefNbr);
				DocumentWithApplications documentWithApplications;
				if (DocumentsWithApplications.TryGetValue(key, out documentWithApplications))
				{
					documentWithApplications.Applications = documentWithApplications.Applications.Concat(documentApplications);
				}
				else
				{
					DocumentsWithApplications.Add(
						key,
						new DocumentWithApplications
				{
					Document = previousDocument,
					DocumentAsInvoice = previousDocumentAsInvoice,
					Applications = documentApplications,
						});
			}
		}
		}

		/// <summary>
		/// Returns documents with their applications to date that can be used
		/// to calculate the documents' actual balances on the statement date.
		/// </summary>
		/// <remarks>
		/// For Open Item statement type, for the sake of performance optimization, 
		/// this method does not return documents that have been marked as closed in 
		/// periods earlier than the one corresponding to statement date. This is not
		/// done for BBF statements because we should report all unreported documents,
		/// even the closed ones.
		/// </remarks>
		private IEnumerable<DocumentWithApplications> GetDocumentsWithApplicationsToDate(
			Customer customer, 
			DateTime statementDate)
		{
			IDictionary<DocumentKey, DocumentWithApplications> documentsWithApplications = new Dictionary<DocumentKey, DocumentWithApplications>();

			FillDictionaryDocumentsWithApplicationsToDate(
				customer,
				statementDate,
				documentsWithApplications,
				true);

			FillDictionaryDocumentsWithApplicationsToDate(
				customer,
				statementDate,
				documentsWithApplications,
				false);

			return documentsWithApplications.Values;
		}
		private IEnumerable<ApplicationWithCurrencyAndAdjdCustomer> GetUnreportedApplications(
			Customer customer,
			DateTime statementDate)
			=> PXSelectJoin<
				ARAdjust,
					LeftJoin<CurrencyInfoAlias,
						On<ARAdjust.adjgCuryInfoID, Equal<CurrencyInfoAlias.curyInfoID>>,
					LeftJoin<CurrencyInfoAlias2,
						On<ARAdjust.adjdCuryInfoID, Equal<CurrencyInfoAlias2.curyInfoID>>,
					LeftJoin<Customer,
						On<ARAdjust.adjdCustomerID, Equal<Customer.bAccountID>>>>>,
				Where<
					ARAdjust.customerID, Equal<Required<ARAdjust.customerID>>,
					And<ARAdjust.released, Equal<True>,
					And<ARAdjust.adjgDocDate, LessEqual<Required<ARRegister.docDate>>,
					And<ARAdjust.statementDate, IsNull,
					And2<Where<IsNotSelfApplying<ARAdjust.adjgDocType>>,
					// We report only inter-statement applications
					// or those applications that affect overall
					// customer balances.
					// -
					And<Where2<
						HasNonZeroGLAmount<ARAdjust>,
						Or<ARAdjust.adjgBranchID, NotEqual<ARAdjust.adjdBranchID>,
						Or<ARAdjust.customerID, NotEqual<ARAdjust.adjdCustomerID>,
						Or<CurrencyInfoAlias.curyID, NotEqual<CurrencyInfoAlias2.curyID>,
						Or<IsSelfVoidingVoidApplication<ARAdjust>>>>>>>>>>>>>
			.Select(this, customer.BAccountID, statementDate)
			.Cast<PXResult<ARAdjust, CurrencyInfoAlias, CurrencyInfoAlias2, Customer>>()
			.ToList()
			.Select(result => new ApplicationWithCurrencyAndAdjdCustomer
			{
				Application = result,
				AdjustingCurrencyID = (CurrencyInfoAlias)result == null ? null : ((CurrencyInfoAlias)result).CuryID,
				AdjustedCurrencyID = (CurrencyInfoAlias2)result == null ? null : ((CurrencyInfoAlias2)result).CuryID,
				AdjdCustomer = result
			});

		private void AdjustStatementEndBalance(ARStatement statement, ARRegister document)
		{
			decimal baseDelta;
			decimal currencyDelta;

			if (statement.StatementType == ARStatementType.OpenItem)
			{
				baseDelta = (document.DocBal ?? 0m);
				currencyDelta = (document.CuryDocBal ?? 0m);
			}
			else
			{
				if (statement.StatementType == ARStatementType.BalanceBroughtForward && document.IsMigratedRecord == true)
				{
					baseDelta = (document.InitDocBal ?? 0m);
					currencyDelta = (document.CuryInitDocBal ?? 0m);
				}
				else 
				{
				baseDelta = (document.OrigDocAmt ?? 0m);
				currencyDelta = (document.CuryOrigDocAmt ?? 0m);
			}
			}

			statement.EndBalance = 
				(statement.EndBalance ?? 0m) 
				+ document.SignBalance * baseDelta;

			statement.CuryEndBalance =
				(statement.CuryEndBalance ?? 0m)
				+ document.SignBalance * currencyDelta;
		}

		private void AdjustStatementEndBalance(
			ARStatement statement, 
			ARStatementAdjust statementApplication)
		{
			FullBalanceDelta balanceDeltaInformation = statementApplication.GetFullBalanceDelta();

			decimal fullBalanceDeltaBase = 0;
			decimal fullBalanceDeltaCurrency = 0;

			if (statementApplication.IsIncomingApplication == true)
			{
				fullBalanceDeltaBase += balanceDeltaInformation.BaseAdjustedExtraAmount;
				fullBalanceDeltaCurrency += balanceDeltaInformation.CurrencyAdjustedExtraAmount;
			}

			if (statementApplication.IsInterStatementApplication == true
				|| statementApplication.IsSelfVoidingVoidApplication == true)
			{
				fullBalanceDeltaBase += (statementApplication.AdjAmt ?? 0m);
				fullBalanceDeltaCurrency += statementApplication.IsIncomingApplication == true
					? (statementApplication.CuryAdjdAmt ?? 0m)
					: (statementApplication.CuryAdjgAmt ?? 0m);
			}

			statement.EndBalance = 
				(statement.EndBalance ?? 0m) + statementApplication.SignBalanceDelta.Value * fullBalanceDeltaBase;

			statement.CuryEndBalance = 
				(statement.CuryEndBalance ?? 0m) + statementApplication.SignBalanceDelta.Value * fullBalanceDeltaCurrency;
		}

		private void SetPreviousStatementInfo(ARStatement statement, IDictionary<ARStatementKey, ARStatement> statementsTrace)
		{
			if (statement.Processed == true) return;

			ARStatement previousStatement = GetPreviousStatement(
				statement.BranchID,
				statement.CustomerID,
				statement.CuryID);

			if (previousStatement != null)
			{
				statement.PrevStatementDate = previousStatement.StatementDate;
			}

			ARStatement deletedStatementTrace;

			if (statementsTrace.TryGetValue(new ARStatementKey(statement), out deletedStatementTrace))
			{
				statement.PrevPrintedCnt = deletedStatementTrace.PrevPrintedCnt;
				statement.PrevEmailedCnt = deletedStatementTrace.PrevEmailedCnt;
			}

			statement.Processed = true;
		}

		/// <summary>
		/// Given a document (along with its optional invoice part) and its relevant incoming / 
		/// outgoing application history, re-calculates the document balances by resetting the
		/// balances to original amounts and replaying the applications against the document.
		/// </summary>
		/// <param name="applications">
		/// The relevant incoming and outgoing applications of the document.
		/// </param>
		private void CalculateBalanceOnStatementDate(ARRegister document, ARInvoice documentAsInvoice, IEnumerable<ARAdjust> applications)
		{
			document.DocBal = document.OrigDocAmt;
			document.CuryDocBal = document.CuryOrigDocAmt;

			applications
				// Applications that void self-voiding documents should be excluded from the
				// balance calculation process. Otherwise, these applications will cause the
				// document to "re-open" and have the original balance. See AC-83929.
				// -
				.Where(application => 
					!ARDocType.IsSelfVoiding(document.DocType)
					|| !IsSelfVoidingVoidApplication<ARAdjust>.Verify(application))
				.ForEach(application => document.AdjustBalance(application));

			document.OpenDoc = document.DocBal.IsNonZero() || document.CuryDocBal.IsNonZero();

			if (documentAsInvoice.AreAllKeysFilled(this))
			{
				documentAsInvoice.DocBal = document.DocBal;
				documentAsInvoice.CuryDocBal = document.CuryDocBal;
				documentAsInvoice.OpenDoc = document.OpenDoc;
			}
		}

		[Obsolete("This method is not used anymore and will be removed in Acumatica ERP 8.0. Use other method overloads.")]
		protected virtual ARStatement AddARStatement(
			Dictionary<ARStatementKey, ARStatement> statements,
			ARStatementKey key,
			ARStatementCycle cycle,
			Customer customer,
			DateTime statementDate)
		{
			throw new NotImplementedException();
		}

		protected virtual ARStatement GetOrAddStatement(
			IDictionary<ARStatementKey, ARStatement> statementsDictionary,
			ARStatementKey statementKey,
			ARStatementCycle statementCycle,
			Customer customer,
			bool isOnDemand)
			=> statementsDictionary.GetOrAdd(statementKey, () =>
			{
				ARStatement statement = CombineStatementCustomizable(
					statementKey,
					statementCycle,
					customer,
					isOnDemand);

				using (new PXLocaleScope(statement.LocaleName))
				{
					FillBucketDescriptions(statement, statementCycle);
				}

				return statement;
			});

		#region Statement Entities Creation Utility Functions

		/// <param name="familyCustomer">
		/// Any customer from the family. The method operates under
		/// assumption that all customers in the family share their
		/// statement parameters, such as statement type, printing
		/// flags etc.
		/// </param>
		public static ARStatement CombineStatement(
			ARStatementKey statementKey,
			ARStatementCycle statementCycle,
			Customer familyCustomer,
			bool isOnDemand)
		{
			ARStatement result = new ARStatement();

			SetStatementAgeDaysToZero(result);
			SetStatementAgeBalancesToZero(result);

			result.BranchID = statementKey.BranchID;
			result.CuryID = statementKey.CurrencyID;
			result.CustomerID = statementKey.CustomerID;
			result.StatementDate = statementKey.StatementDate;
			result.StatementCycleId = statementCycle.StatementCycleId;
			result.StatementCustomerID = familyCustomer.StatementCustomerID;
			result.StatementType = familyCustomer.StatementType;
			result.DontPrint = familyCustomer.PrintStatements != true;
			result.DontEmail = familyCustomer.SendStatementByEmail != true;
			result.OnDemand = isOnDemand;
			result.AgeDays00 = 0;
			result.AgeDays01 = statementCycle.AgeDays00;
			result.AgeDays02 = statementCycle.AgeDays01;
			result.AgeDays03 = statementCycle.AgeDays02;
			result.LocaleName = familyCustomer.LocaleName ?? (System.Globalization.CultureInfo.CurrentCulture.Name);

			return result;
		}

		protected static void SetStatementAgeDaysToZero(ARStatement statement)
		{
			statement.AgeDays00 =
			statement.AgeDays01 =
			statement.AgeDays02 =
			statement.AgeDays03 = 0;
		}

		protected static void SetStatementAgeBalancesToZero(ARStatement statement)
		{
			statement.AgeBalance00 =
			statement.AgeBalance01 =
			statement.AgeBalance02 =
			statement.AgeBalance03 =
			statement.AgeBalance04 =
			statement.CuryAgeBalance00 =
			statement.CuryAgeBalance01 =
			statement.CuryAgeBalance02 =
			statement.CuryAgeBalance03 =
			statement.CuryAgeBalance04 =
			statement.BegBalance =
			statement.EndBalance =
			statement.CuryBegBalance =
			statement.CuryEndBalance = decimal.Zero;
		}

		/// <summary>
		/// Creates a new <see cref="ARStatementDetail"/> record using the information
		/// from the given customer statement and document records.
		/// </summary>
		/// <param name="statement">The statement to which the created detail belongs.</param>
		/// <param name="document">The document to which the created detail corresponds.</param>
		public static ARStatementDetail CombineStatementDetail(ARStatement statement, ARRegister document) 
			=> new ARStatementDetail
			{
				DocType = document.DocType,
				RefNbr = document.RefNbr,
				BranchID = document.BranchID,
				DocBalance = document.DocBal,
				CuryDocBalance = document.CuryDocBal,
				IsOpen = document.OpenDoc,
				CustomerID = statement.CustomerID,
				CuryID = statement.CuryID,
				StatementDate = statement.StatementDate,
			};

		/// <summary>
		/// Creates a new <see cref="ARStatementAdjust"/> record from the given
		/// customer statement record, document application record, and information
		/// about adjusting / adjusted document currencies.
		/// </summary>
		/// <param name="customerStatement">The statement to which the created application belongs.</param>
		/// <param name="application">The AR application record to which the created record corresponds.</param>
		public static ARStatementAdjust CombineStatementApplication(
			ARStatement customerStatement,
			ARAdjust application,
			string adjustingCurrencyID,
			string adjustedCurrencyID)
			=> new ARStatementAdjust
			{
				BranchID = customerStatement.BranchID,
				CuryID = customerStatement.CuryID,
				CustomerID = customerStatement.CustomerID,
				StatementDate = customerStatement.StatementDate,

				AdjdDocType = application.AdjdDocType,
				AdjdRefNbr = application.AdjdRefNbr,
				AdjdLineNbr = application.AdjdLineNbr,
				AdjgDocType = application.AdjgDocType,
				AdjgRefNbr = application.AdjgRefNbr,

				AdjNbr = application.AdjNbr,

				AdjAmt = application.AdjAmt,
				AdjDiscAmt = application.AdjDiscAmt,
				AdjWOAmt = application.AdjWOAmt,
				RGOLAmt = application.RGOLAmt,

				AdjgBranchID = application.AdjgBranchID,
				AdjdBranchID = application.AdjdBranchID,

				AdjgCuryID = adjustingCurrencyID,
				AdjdCuryID = adjustedCurrencyID,

				AdjgCustomerID = application.CustomerID,
				AdjdCustomerID = application.AdjdCustomerID,

				CuryAdjdAmt = application.CuryAdjdAmt,
				CuryAdjdDiscAmt = application.CuryAdjdDiscAmt,
				CuryAdjdWOAmt = application.CuryAdjdWOAmt,

				CuryAdjgAmt = application.CuryAdjgAmt,
				CuryAdjgDiscAmt = application.CuryAdjgDiscAmt,
				CuryAdjgWOAmt = application.CuryAdjgWOAmt,

				IsSelfVoidingVoidApplication = IsSelfVoidingVoidApplication<ARAdjust>.Verify(application)
			};

		protected virtual ARStatement CombineStatementCustomizable(
			ARStatementKey statementKey,
			ARStatementCycle statementCycle,
			Customer familyCustomer,
			bool isOnDemand)
			=> CombineStatement(statementKey, statementCycle, familyCustomer, isOnDemand);

		protected virtual ARStatementDetail CombineStatementDetailCustomizable(
			ARStatement statement,
			ARRegister document)
			=> CombineStatementDetail(statement, document);

		protected virtual ARStatementAdjust CombineStatementApplicationCustomizable(
			ARStatement customerStatement,
			ARAdjust application,
			string adjustingCurrencyID,
			string adjustedCurrencyID)
			=> CombineStatementApplication(
				customerStatement,
				application,
				adjustingCurrencyID,
				adjustedCurrencyID);
		#endregion

		protected virtual void FillBucketDescriptions(
			ARStatement statement, 
			ARStatementCycle statementCycle)
		{
			DateTime statementDate = statement.StatementDate.Value;

			if (statementCycle.UseFinPeriodForAging == true)
			{
				IList<string> bucketDescriptions = AgingEngine
					.GetPeriodAgingBucketDescriptions(
						FinPeriodRepository,
						statementDate,
						AgingDirection.Backwards,
						5)
					.ToArray();

				statement.AgeBucketCurrentDescription = bucketDescriptions[0];
				statement.AgeBucket01Description = bucketDescriptions[1];
				statement.AgeBucket02Description = bucketDescriptions[2];
				statement.AgeBucket03Description = bucketDescriptions[3];
				statement.AgeBucket04Description = bucketDescriptions[4];
			}
			else
			{
				IList<string> bucketDescriptions = AgingEngine
					.GetDayAgingBucketDescriptions(
						AgingDirection.Backwards,
						new int[]
						{
							statement.AgeDays00 ?? 0,
							statement.AgeDays01 ?? 0,
							statement.AgeDays02 ?? 0,
							statement.AgeDays03 ?? 0,
						},
						false)
					.ToArray();

				PXCache statementCycleCache = this.Caches[typeof(ARStatementCycle)];

				// Take custom bucket descriptions if they are specified in the
				// statement cycle. Otherwise, use the calculated descriptions.
				// -
				statement.AgeBucketCurrentDescription = 
					GetBucketDescription<ARStatementCycle.ageMsgCurrent>(statementCycleCache, statementCycle) ?? bucketDescriptions[0];

				statement.AgeBucket01Description = 
					GetBucketDescription<ARStatementCycle.ageMsg00>(statementCycleCache, statementCycle) ?? bucketDescriptions[1];

				statement.AgeBucket02Description =
					GetBucketDescription<ARStatementCycle.ageMsg01>(statementCycleCache, statementCycle) ?? bucketDescriptions[2];

				statement.AgeBucket03Description =
					GetBucketDescription<ARStatementCycle.ageMsg02>(statementCycleCache, statementCycle) ?? bucketDescriptions[3];

				statement.AgeBucket04Description =
					GetBucketDescription<ARStatementCycle.ageMsg03>(statementCycleCache, statementCycle) ?? bucketDescriptions[4];
			}
		}

		private string GetBucketDescription<Field>(PXCache cache, object data)
			where Field : IBqlField
		{
			object value = cache.GetValueExt<Field>(data);
			return (value is PXFieldState ? ((PXFieldState)value).Value : value) as string;
		}

		[Obsolete("This method is not used anymore and will be removed in Acumatica ERP 8.0. Use other method overloads.")]
		protected virtual void AccumulateAgeBalancesIntoStatements(
			IEnumerable<Tuple<ARRegister, ARInvoice>> customerDocuments,
			IDictionary<ARStatementKey, ARStatement> statements,
			bool ageCredits)
		{
			throw new NotImplementedException();
		}

		/// <summary>
		/// Fills the age balances of relevant statements from the provided statement
		/// dictionary based on the information of invoices and payments open on the
		/// statement date.
		/// </summary>
		protected virtual void AccumulateAgeBalancesIntoStatements(
			ARStatementCycle statementCycle,
			DateTime statementDate,
			IEnumerable<StatementDocument> openDocuments,
			IDictionary<ARStatementKey, ARStatement> statements,
			bool ageCredits)
		{
			foreach (StatementDocument statementDocument in openDocuments)
			{
				ARRegister document = statementDocument.Document;
				ARInvoice documentAsInvoice = statementDocument.DocumentAsInvoice;

				ARStatementKey statementKey = new ARStatementKey(
					document.BranchID.Value, 
					document.CuryID, 
					document.CustomerID.Value,
					statementDate);

				if (statements.ContainsKey(statementKey)
					&& IsMultipleInstallmentMaster(documentAsInvoice) == false
					&& document.DocType != ARDocType.CashSale
					&& document.DocType != ARDocType.CashReturn)
				{
					AccumulateAgeBalances(
						this,
						statementCycle,
						statements[statementKey],
						document.Payable == true ? documentAsInvoice : document,
						ageCredits);
				}
			}
		}

		[Obsolete("This method is not used anymore and will be removed in Acumatica ERP 8.0. Use other method overloads.")]
		protected static void AccumulateAgeBalances(
			ARStatement statement,
			ARRegister document,
			bool ageCredits)
		{
			throw new NotImplementedException();
		}

		protected static void AccumulateAgeBalances(
			PXGraph graph,
			ARStatementCycle statementCycle,
			ARStatement statement, 
			ARRegister document, 
			bool ageCredits)
		{
			ARInvoice invoice = document as ARInvoice;

			// Small Credit WO is a type of invoice but it must be processed as a payment.
			// -
			if (invoice != null && invoice?.DocType != ARDocType.SmallCreditWO || ageCredits)
			{
				DateTime statementDate = statement.StatementDate.Value;

				DateTime dateForAging = 
					statementCycle.AgeBasedOn == AgeBasedOnType.DueDate && invoice != null
						? invoice.DueDate.Value
						: document.DocDate.Value;

				int bucketNumber = statementCycle.UseFinPeriodForAging == true
					? AgingEngine.AgeByPeriods(statementDate, dateForAging, graph.GetService<IFinPeriodRepository>(), AgingDirection.Backwards, 5)
					: AgingEngine.AgeByDays(statementDate, dateForAging, AgingDirection.Backwards, new int[] 
					{
						statement.AgeDays00 ?? 0,
						statement.AgeDays01 ?? 0,
						statement.AgeDays02 ?? 0,
						statement.AgeDays03 ?? 0,
					});

				decimal docBal = document.Paying == true 
					? -document.DocBal.Value 
					: document.DocBal.Value;

				decimal curyDocBal = document.Paying == true 
					? -document.CuryDocBal.Value 
					: document.CuryDocBal.Value;

				switch (bucketNumber)
				{
					case 0:
						statement.AgeBalance00 = (statement.AgeBalance00 ?? decimal.Zero) + docBal;
						statement.CuryAgeBalance00 = (statement.CuryAgeBalance00 ?? decimal.Zero) + curyDocBal;
						break;
					case 1:
						statement.AgeBalance01 = (statement.AgeBalance01 ?? decimal.Zero) + docBal;
						statement.CuryAgeBalance01 = (statement.CuryAgeBalance01 ?? decimal.Zero) + curyDocBal;
						break;
					case 2:
						statement.AgeBalance02 = (statement.AgeBalance02 ?? decimal.Zero) + docBal;
						statement.CuryAgeBalance02 = (statement.CuryAgeBalance02 ?? decimal.Zero) + curyDocBal;
						break;
					case 3:
						statement.AgeBalance03 = (statement.AgeBalance03 ?? decimal.Zero) + docBal;
						statement.CuryAgeBalance03 = (statement.CuryAgeBalance03 ?? decimal.Zero) + curyDocBal;
						break;
					case 4:
						statement.AgeBalance04 = (statement.AgeBalance04 ?? decimal.Zero) + docBal;
						statement.CuryAgeBalance04 = (statement.CuryAgeBalance04 ?? decimal.Zero) + curyDocBal;
						break;
					default:
						throw new PXException(Messages.ImpossibleToAgeDocumentUnexpectedBucketNumber);
				}
			}
			else
			{
				// Payments or small credit write-offs, in case when credits are not aged.
				// After completion we must apply residual payments to previous buckets.
				// -
				statement.AgeBalance04 = statement.AgeBalance04 - document.DocBal; 
				statement.CuryAgeBalance04 = statement.CuryAgeBalance04 - document.CuryDocBal;
			}
		}

		[Obsolete("This method is not used anymore and will be removed in Acumatica ERP 8.0. Use other method overloads.")]
		protected virtual IEnumerable<ARStatement> DeleteCustomerStatement(
			ARStatementCycle statementCycle,
			Customer customer,
			DateTime statementDate)
		{
			throw new NotImplementedException();
		}

		/// <param name="isOnDemand">
		/// If set to <c>true</c>, indicates that the existing statement should be deleted
		/// so that a new on-demand statement will be generated on that date.
		/// </param>
		protected virtual IEnumerable<ARStatement> DeleteCustomerStatement(
			Customer customer, 
			DateTime statementDate,
			bool isOnDemand) 
		{
			StatementCreateBO persistGraph = PXGraph.CreateInstance<StatementCreateBO>();

			List<ARStatement> deletedStatementsTrace = new List<ARStatement>();

			int deletedCount = 0;

			foreach (ARStatement statement in persistGraph.CustomerStatement.Select(customer.BAccountID, statementDate))
			{
				deletedStatementsTrace.Add(StatementTrace(statement));
				persistGraph.CustomerStatement.Delete(statement);

				++deletedCount;
			}

			if (deletedCount == 0 && !isOnDemand)
			{
				customer.StatementLastDate = persistGraph.FindLastCstmStatementDate(customer.BAccountID, statementDate);
				PXUpdate<
					Set<Override.Customer.statementLastDate, Required<Override.Customer.statementLastDate>>,
					Override.Customer,
					Where<Override.Customer.bAccountID, Equal<Required<Override.Customer.bAccountID>>>>
				.Update(this, customer.StatementLastDate, customer.BAccountID);
			}

			ResetDocumentsLastStatementDate(persistGraph, statementDate, customer.BAccountID);

			persistGraph.Actions.PressSave();

			return deletedStatementsTrace;
		}

		private void EnsureNoRegularStatementExists(int? customerID, DateTime statementDate)
		{
			PXSelectBase<ARStatement> nonOnDemandStatements = new PXSelect<
				ARStatement,
				Where<
					ARStatement.customerID, Equal<Required<ARStatement.customerID>>,
					And<ARStatement.statementDate, Equal<Required<ARStatement.statementDate>>,
					And<ARStatement.onDemand, NotEqual<True>>>>>(this);

			if (nonOnDemandStatements.Any(customerID, statementDate))
			{
				throw new PXException(Messages.StatementCoveringDateAlreadyExistsForCustomer);
			}
		}

		protected static ARStatement StatementTrace(ARStatement statement)
		{
			var trace = new ARStatement
			{ 
				BranchID = statement.BranchID,
				CuryID = statement.CuryID,
				CustomerID = statement.CustomerID,
				StatementDate = statement.StatementDate,
				PrevPrintedCnt = statement.PrevPrintedCnt,
				PrevEmailedCnt = statement.PrevEmailedCnt
			};

			if (statement.Printed == true)
				trace.PrevPrintedCnt++;

			if (statement.Emailed == true)
				trace.PrevEmailedCnt++;

			return trace;
		}
		#endregion
	}

	[PXHidden]
	public class StatementCreateBO : PXGraph<StatementCreateBO>
	{
		public PXSelect<Customer, Where<Customer.bAccountID, Equal<Required<Customer.bAccountID>>>> Customer;

		public PXSelect<
			ARStatement, 
			Where<
				ARStatement.statementCycleId, Equal<Required<ARStatement.statementCycleId>>,
				And<ARStatement.statementDate, Equal<Required<ARStatement.statementDate>>>>> 
			Statement;

		public PXSelect<
			ARStatement, 
			Where<
				ARStatement.customerID, Equal<Required<Customer.bAccountID>>,
				And<ARStatement.statementDate, Equal<Required<ARStatement.statementDate>>>>> 
			CustomerStatement;

		public PXSelect<
			ARStatementDetail, 
			Where<
				ARStatementDetail.customerID, Equal<Current<ARStatement.customerID>>,
				And<ARStatementDetail.statementDate, Equal<Current<ARStatement.statementDate>>,
				And<ARStatementDetail.curyID, Equal<Current<ARStatement.curyID>>>>>> 
			StatementDetail;

		public PXSelect<
			ARStatementAdjust,
			Where<
				ARStatementAdjust.curyID, Equal<Current<ARStatement.curyID>>,
				And<ARStatementAdjust.customerID, Equal<Current<ARStatement.customerID>>,
				And<ARStatementAdjust.statementDate, Equal<Current<ARStatement.statementDate>>>>>>
			StatementApplications;

		public PXSelect<
			ARRegister, 
			Where<
				ARRegister.docType, Equal<Optional<ARStatementDetail.docType>>, 
				And<ARRegister.refNbr, Equal<Optional<ARStatementDetail.refNbr>>>>> 
			Docs;

		public PXSelect<
			ARAdjust,
			Where<
				ARAdjust.adjgDocType, Equal<Optional<ARStatementAdjust.adjgDocType>>,
				And<ARAdjust.adjgRefNbr, Equal<Optional<ARStatementAdjust.adjgRefNbr>>,
				And<ARAdjust.adjdDocType, Equal<Optional<ARStatementAdjust.adjdDocType>>,
				And<ARAdjust.adjdRefNbr, Equal<Optional<ARStatementAdjust.adjdRefNbr>>,
				And<ARAdjust.adjNbr, Equal<Optional<ARStatementAdjust.adjNbr>>>>>>>> 
			Applications;

		public StatementCreateBO()
		{
			// Since documents and applications do not follow the master / detail
			// relationship within the graph, we need to prevent defaulting
			// of application fields marked with the PXDBDefaultAttribute.
			// -
			foreach (string field in Applications.Cache.Fields)
			{
				foreach (PXDBDefaultAttribute dbDefaultAttribute in 
					Applications.Cache.GetAttributesReadonly(field).OfType<PXDBDefaultAttribute>())
				{
					dbDefaultAttribute.DefaultForUpdate = false;
				}
			}
		}

		public virtual void ARStatement_RowDeleted(PXCache cache, PXRowDeletedEventArgs e)
		{
			ARStatement statement = e.Row as ARStatement;

			if (statement == null) return;

			if (statement.OnDemand != true) 
			{
				DateTime? newStatementDate = FindLastCstmStatementDate(statement.CustomerID, statement.StatementDate);
				PXUpdate<
					Set<Override.Customer.statementLastDate, Required<Override.Customer.statementLastDate>>,
					Override.Customer,
					Where<
						Override.Customer.bAccountID, Equal<Required<Override.Customer.bAccountID>>,
						And<Override.Customer.statementLastDate, Equal<Required<Override.Customer.statementLastDate>>>>>
				.Update(this, newStatementDate, statement.CustomerID, statement.StatementDate);
			}
		}

		public virtual void ARStatementAdjust_RowDeleted(PXCache cache, PXRowDeletedEventArgs e)
		{
			ARStatementAdjust statementApplication = e.Row as ARStatementAdjust;

			if (statementApplication == null) return;

			ARStatement parentStatement = FindParentStatement(
				statementApplication.BranchID,
				statementApplication.CuryID,
				statementApplication.CustomerID,
				statementApplication.StatementDate);

			foreach (ARAdjust application in Applications.Select(
				statementApplication.AdjgDocType,
				statementApplication.AdjgRefNbr,
				statementApplication.AdjdDocType,
				statementApplication.AdjdRefNbr,
				statementApplication.AdjNbr))
			{
				if (parentStatement.OnDemand != true
					&& application.StatementDate == statementApplication.StatementDate)
				{
					application.StatementDate = null;
					Applications.Update(application);
				}
			}
		}

		public DateTime? FindLastCstmStatementDate(int? aCustomer, DateTime? aBeforeDate) => PXSelect<
			ARStatement, 
			Where<
				ARStatement.customerID, Equal<Required<ARStatement.customerID>>,
				And<ARStatement.statementDate, Less<Required<ARStatement.statementDate>>,
				And<ARStatement.onDemand, Equal<False>>>>, 
			OrderBy<
				Desc<ARStatement.statementDate>>>
			.SelectWindowed(this, 0, 1, aCustomer, aBeforeDate)
			.RowCast<ARStatement>()
			.FirstOrDefault()
			?.StatementDate;

		[Obsolete("This method has been deprecated and will be removed in Acumatica ERP 8.0")]
		public DateTime? FindLastCycleStatementDate(string aCycleID, DateTime aBeforeDate) => PXSelect<
			ARStatement,
			Where<
				ARStatement.statementCycleId, Equal<Required<ARStatement.statementCycleId>>,
				And<ARStatement.statementDate, Less<Required<ARStatement.statementDate>>>>,
			OrderBy<
				Desc<ARStatement.statementDate>>>
			.SelectWindowed(this, 0, 1, aCycleID, aBeforeDate)
			.RowCast<ARStatement>()
			.FirstOrDefault()
			?.StatementDate;

		private ARStatement FindParentStatement(int? branchID, string currencyID, int? customerID, DateTime? statementDate)
		{
			if (Statement.Current?.BranchID == branchID
				&& Statement.Current?.CuryID == currencyID
				&& Statement.Current?.CustomerID == customerID
				&& Statement.Current?.StatementDate == statementDate)
			{
				return Statement.Current;
			}

			ARStatement statementKey = new ARStatement
			{
				BranchID = branchID,
				CuryID = currencyID,
				CustomerID = customerID,
				StatementDate = statementDate,
			};

			ARStatement statementFromCache = Statement.Locate(statementKey);

			if (statementFromCache != null)
			{
				return statementFromCache;
			}

			return PXSelect<
				ARStatement,
				Where<
					ARStatement.branchID, Equal<Required<ARStatement.branchID>>,
					And<ARStatement.curyID, Equal<Required<ARStatement.curyID>>,
					And<ARStatement.customerID, Equal<Required<ARStatement.customerID>>,
					And<ARStatement.statementDate, Equal<Required<ARStatement.statementDate>>>>>>>
				.SelectWindowed(this, 0, 1, branchID, currencyID, customerID, statementDate);
		}
	}
}