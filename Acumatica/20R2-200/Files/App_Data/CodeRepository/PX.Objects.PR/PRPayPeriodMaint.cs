using PX.Common;
using PX.Data;
using PX.Data.BQL;
using PX.Data.BQL.Fluent;
using PX.Objects.GL;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PX.Objects.PR
{
	public class PRPayPeriodMaint : PXGraph<PRPayPeriodMaint, PRPayGroupYear>
	{
		#region CTOR
		public PRPayPeriodMaint()
		{
			Periods.AllowInsert =
			Periods.AllowDelete = false;
			PayrollYear.AllowDelete = false;
			AskAutoFill.SetEnabled(false);
		}
		#endregion

		#region Views
		public PXSelect<PRPayGroupYear, Where<PRPayGroupYear.payGroupID, Equal<Optional<PRPayGroupYear.payGroupID>>>, OrderBy<Desc<PRPayGroupYear.year>>> PayrollYear;

		public SelectFrom<PRPayGroupYear>
			.Where<PRPayGroupYear.payGroupID.IsEqual<P.AsString>
				.And<PRPayGroupYear.year.IsEqual<P.AsString>>>.View PayrollYearQuery;

		public SelectFrom<PRPayGroupYear>
			.Where<PRPayGroupYear.payGroupID.IsEqual<P.AsString>
				.And<PRPayGroupYear.periodsFullyCreated.IsEqual<False>>>
			.OrderBy<PRPayGroupYear.year.Asc>.View FirstYearNotSetUp;

		public SelectFrom<PRPayGroupPeriod>
			.Where<PRPayGroupPeriod.payGroupID.IsEqual<PRPayGroupYear.payGroupID.FromCurrent>
				.And<PRPayGroupPeriod.finYear.IsEqual<PRPayGroupYear.year.FromCurrent>>>
			.OrderBy<PRPayGroupPeriod.transactionDate.Asc, PRPayGroupPeriod.startDate.Asc>.View Periods;

		public PXSelect<PRPayGroupYearSetup, Where<PRPayGroupYearSetup.payGroupID, Equal<Optional<PRPayGroupYear.payGroupID>>>> PayrollYearSetup;

		public PXSelect<PRPayGroupPeriodSetup, Where<PRPayGroupPeriodSetup.payGroupID, Equal<Optional<PRPayGroupYear.payGroupID>>>> PeriodsSetup;

		public PXFilter<PRPayPeriodCreationDialog> PeriodCreation;

		public PXSetup<PRSetup> Setup;

		public SelectFrom<PRTransactionDateException>
			.Where<PRTransactionDateException.date.IsEqual<P.AsDateTime>>.View ExceptionDates;
		#endregion

		#region Event Handlers
		[PXMergeAttributes(Method = MergeMethod.Append)]
		[PXCustomizeBaseAttribute(typeof(PXDefaultAttribute), nameof(PXDefaultAttribute.Constant), null)]
		protected virtual void _(Events.CacheAttached<PRPayGroupPeriod.transactionDate> _) { }

		protected virtual void PRPayGroupYear_RowInserting(PXCache cache, PXRowInsertingEventArgs e)
		{
			var row = (PRPayGroupYear)e.Row;

			if (string.IsNullOrEmpty(row.PayGroupID))
			{
				e.Cancel = true;
				return;
			}

			PRPayGroupYear firstYearNotSetUp = FirstYearNotSetUp.SelectSingle(row.PayGroupID);
			if (firstYearNotSetUp == null)
			{
				bool result = this.CreateNextYear(row.PayGroupID, ref row);
				if (!result)
					e.Cancel = true;
			}
			else
			{
				// Force re-insertion of existing row
				cache.RestoreCopy(row, firstYearNotSetUp);
				cache.Remove(firstYearNotSetUp);
			}
		}

		protected virtual void _(Events.RowSelected<PRPayGroupYear> e)
		{
			PRPayGroupYear row = e.Row as PRPayGroupYear;
			if (row == null)
			{
				return;
			}

			if (row.PeriodsFullyCreated != true)
			{
				e.Cache.SetStatus(row, PXEntryStatus.Inserted);
			}

			AskAutoFill.SetEnabled(row.PeriodsFullyCreated != true);
		}

		protected virtual void PRPayGroupPeriod_RowInserting(PXCache sender, PXRowInsertingEventArgs e)
		{
			var row = (PRPayGroupPeriod)e.Row;
			if (row != null)
			{
				PRPayGroupPeriod originalPeriod = SelectFrom<PRPayGroupPeriod>
					.Where<PRPayGroupPeriod.payGroupID.IsEqual<PRPayGroupPeriod.payGroupID.FromCurrent>
						.And<PRPayGroupPeriod.originalFinPeriodID.IsEqual<PRPayGroupPeriod.originalFinPeriodID.FromCurrent>>>.View.SelectSingleBound(this, new object[] { row });

				if (originalPeriod != null)
				{
					e.Cancel = true;
					return;
				}

				AssignTransactionDate(sender, row);
			}
		}

		protected virtual void _(Events.FieldUpdating<PRPayGroupPeriod.transactionDate> e)
		{
			PRPayGroupPeriod row = e.Row as PRPayGroupPeriod;
			if (row == null)
			{
				return;
			}

			PRPayment payPeriodPayment = SelectFrom<PRPayment>
				.Where<PRPayment.payGroupID.IsEqual<PRPayGroupPeriod.payGroupID.FromCurrent>
					.And<PRPayment.finPeriodID.IsEqual<PRPayGroupPeriod.finPeriodID.FromCurrent>>>.View.SelectSingleBound(this, new object[] { row });
			if (payPeriodPayment != null)
			{
				e.Cache.RaiseExceptionHandling<PRPayGroupPeriod.transactionDate>(
					row,
					row.TransactionDate,
					new PXSetPropertyException(Messages.CantChangePeriodTransDate, string.Format("{0},{1}", payPeriodPayment.DocType, payPeriodPayment.RefNbr)));
			}

			DateTime? transactionDate = e.NewValue as DateTime?;
			if (!transactionDate.HasValue)
			{
				return;
			}

			while (PayrollYear.Current.UseTransactionDateExceptions == true && IsTransactionExceptionDate(transactionDate.Value))
			{
				transactionDate = transactionDate.Value.AddDays(PayrollYear.Current.TransactionDateExceptionBehavior == TransactionDateExceptionBehavior.After ? 1 : -1);
			}

			e.NewValue = transactionDate;
			string transactionYear = transactionDate.Value.Year.ToString();
			if (!string.IsNullOrEmpty(row.FinYear) && transactionYear != row.FinYear)
			{
				// Shift the period from the current year to the year of the calculated transaction date.
				// Other fields nulled here will be calculated in PRPayGroupPeriod.RowPersisting.
				row.FinYear = transactionYear;
				row.Descr = transactionYear == row.OriginalYear ?
					row.OriginalDescr :
					PXLocalizer.LocalizeFormat(Messages.ShiftedPeriodDescrFormat, row.OriginalDescr, row.OriginalYear);
				row.FinPeriodID = null;
				row.PeriodNbr = null;

				e.Cache.RaiseExceptionHandling<PRPayGroupPeriod.transactionDate>(
					row,
					row.TransactionDate,
					new PXSetPropertyException(Messages.PeriodWillBeShifted, PXErrorLevel.Warning, transactionYear));
			}
		}

		protected virtual void _(Events.RowPersisting<PRPayGroupPeriod> e)
		{
			IEnumerable<PRPayGroupPeriod> pendingPeriods = e.Cache.Updated.Concat_(e.Cache.Inserted).Cast<PRPayGroupPeriod>().Where(x => x.FinPeriodID == null);
			if (pendingPeriods.Any())
			{
				foreach (IGrouping<string, PRPayGroupPeriod> pendingPeriodsPerGroupID in pendingPeriods.GroupBy(x => x.PayGroupID))
				{
					string payGroupID = pendingPeriodsPerGroupID.Key;
					foreach (IGrouping<string, PRPayGroupPeriod> pendingPeriodsPerYear in pendingPeriodsPerGroupID.Cast<PRPayGroupPeriod>().GroupBy(x => x.FinYear))
					{
						string year = pendingPeriodsPerYear.Key;
						int yearAsInt = int.Parse(year);

						PRPayGroupYear payrollYear = PayrollYearQuery.SelectSingle(payGroupID, year);
						if (payrollYear == null)
						{
							PRPayGroupYearSetup yearSetup = PayrollYearSetup.SelectSingle(payGroupID);
							if (yearSetup == null || int.Parse(yearSetup.FirstFinYear) > yearAsInt)
							{
								// Periods belongs to a year before the first year setup, drop period.
								pendingPeriodsPerYear.ForEach(x => e.Cache.Delete(x));
							}
							else
							{
								// Create the year that the period belongs to.
								PRPayGroupYear newYear;
								do
								{
									newYear = null;
									if (!CreateNextYear(payGroupID, ref newYear))
									{
										throw new PXException(Messages.CantCreatePayGroupYear, year, payGroupID);
									}

									PayrollYear.Insert(newYear);
								}
								while (newYear.Year != year);
							}
						}

						AssignID(pendingPeriodsPerYear.Cast<PRPayGroupPeriod>(), payGroupID, year);
					}
				}

				pendingPeriods.ForEach(x => e.Cache.Update(x));
			}

			PRPayGroupPeriod row = e.Row as PRPayGroupPeriod;
			if (row != null && int.TryParse(row.FinYear, out int periodYear))
			{
				PRPayGroupYearSetup yearSetup = PayrollYearSetup.SelectSingle(row.PayGroupID);
				if (yearSetup == null || int.Parse(yearSetup.FirstFinYear) > periodYear)
				{
					e.Cancel = true;
				}
			}
		}
		#endregion

		public override void Persist()
		{
			try
			{
				base.Persist();
			}
			catch (PXLockViolationException ex)
			{
				if (ex.Table == typeof(PRPayGroupYear) && (ex.Operation & PXDBOperation.Command) == PXDBOperation.Insert)
				{
					PXCache cache = Caches[typeof(PRPayGroupYear)];
					PRPayGroupYear violatedRecord = cache.Inserted.Cast<PRPayGroupYear>().FirstOrDefault(rec => ex.Keys.Contains(rec.PayGroupID) && ex.Keys.Contains(rec.Year));
					if (violatedRecord != null)
					{
						// In PRPayGroupYear.RowSelected, the latest record with PeriodsFullyCreated was aritfically set to Inserted status.
						// Once periods are inserted and we want to Persist the change, the insertion will fail if the record already existed.
						// So set its status to Updated and try again.
						cache.SetStatus(violatedRecord, PXEntryStatus.Updated);
						base.Persist();
					}
				}
			}
		}

		#region Actions
		[PXUIField(DisplayName = ActionsMessages.Insert, MapEnableRights = PXCacheRights.Insert, MapViewRights = PXCacheRights.Insert)]
		[PXInsertButton]
		protected IEnumerable insert(PXAdapter adapter)
		{
			PRPayGroupYear current = PayrollYear.Current;

			if (string.IsNullOrEmpty(current?.PayGroupID))
			{
				yield break;
			}

			PRPayGroupYear firstYearNotSetUp = FirstYearNotSetUp.SelectSingle(current.PayGroupID);
			if (firstYearNotSetUp == null)
			{
				if (!CreateNextYear(current.PayGroupID, ref firstYearNotSetUp))
				{
					yield break;
				}
				PayrollYear.Insert(firstYearNotSetUp);
			}
			yield return firstYearNotSetUp;
		}

		public PXAction<PRPayGroupYear> AskAutoFill;
		[PXButton(Tooltip = GL.Messages.GeneratePeriodsToolTip)]
		[PXUIField(DisplayName = GL.Messages.GeneratePeriods, MapEnableRights = PXCacheRights.Select)]
		public virtual void askAutoFill()
		{
			if (PeriodCreation.AskExt() == WebDialogResult.Yes)
			{
				AutoFill.Press();
			}
		}

		public PXAction<PRPayGroupYear> AutoFill;
		[PXButton]
		[PXUIField(DisplayName = "Generate")]
		public virtual IEnumerable autoFill(PXAdapter adapter)
		{
			PRPayGroupYear year = PayrollYear.Current;
			year.UseTransactionDateExceptions = PeriodCreation.Current.UseExceptions;
			year.TransactionDateExceptionBehavior = PeriodCreation.Current.ExceptionDateBehavior;
			year.FinPeriods = (short?)Periods.Select().RowCount; // Newly added periods will increment.
			PayrollYear.Update(year);

			var yearSetup = PayrollYearSetup.SelectSingle(year.PayGroupID);
			var periodsSetup = GetPeriodsSetupList(year.PayGroupID);
			FiscalYearCreator<PRPayGroupYear, PRPayGroupPeriod>.CreatePeriods(Periods.Cache, periodsSetup, year.Year, year.StartDate, yearSetup);

			year.PeriodsFullyCreated = true;
			PayrollYear.Update(year);

			if (year.UseTransactionDateExceptions == true && year.TransactionDateExceptionBehavior == TransactionDateExceptionBehavior.Before)
			{
				// There are potentially periods in the next year that will be shifted to the current year due to transaction date exceptions.
				// Create next year's first few periods, until we find one for which the transaction date falls in the next year once transaction
				// date exceptions have been factored in.
				int currentYearAsInt = int.Parse(year.Year);
				string nextYearNumber = (currentYearAsInt + 1).ToString();
				PRPayGroupYear nextYear = PayrollYearQuery.SelectSingle(year.PayGroupID, nextYearNumber);
				if (nextYear == null)
				{
					if (!CreateNextYear(year.PayGroupID, ref nextYear))
					{
						throw new PXException(Messages.CantCreatePayGroupYear, nextYearNumber, year.PayGroupID);
					}
					PayrollYear.Insert(nextYear);
				}

				var periodsToInsert = new List<PRPayGroupPeriod>();
				PRPayGroupPeriod nextYearPeriod = null;
				var preriodCreator = new FiscalPeriodCreator<PRPayGroupYear, PRPayGroupPeriod>(yearSetup, nextYearNumber, nextYear.StartDate, periodsSetup);
				preriodCreator.Graph = this;
				do
				{
					if (nextYearPeriod != null)
					{
						periodsToInsert.Add(nextYearPeriod);

						// Required to properly process createNextPeriod.
						nextYearPeriod.PeriodNbr = nextYearPeriod.OriginalPeriodNbr;
					}

					nextYearPeriod = preriodCreator.createNextPeriod(nextYearPeriod);
					if (nextYearPeriod == null)
					{
						break;
					}

					nextYearPeriod.PayGroupID = year.PayGroupID;

					nextYearPeriod.OriginalDescr = nextYearPeriod.Descr;
					nextYearPeriod.OriginalPeriodNbr = nextYearPeriod.PeriodNbr;
					nextYearPeriod.OriginalYear = nextYearPeriod.FinYear;
					nextYearPeriod.OriginalFinPeriodID = nextYearPeriod.FinPeriodID;
					AssignTransactionDate(Periods.Cache, nextYearPeriod);
				}
				while (nextYearPeriod.TransactionDate.HasValue && nextYearPeriod.TransactionDate.Value.Year == currentYearAsInt);

				if (periodsToInsert.Any())
				{
					AssignID(periodsToInsert, year.PayGroupID, year.Year);
					periodsToInsert.ForEach(x => Periods.Cache.Insert(x));
				}
			}

			return adapter.Get();
		}
		#endregion

		#region Helpers
		protected IEnumerable<PRPayGroupPeriodSetup> GetPeriodsSetupList(string payGroupID)
		{
			foreach (PRPayGroupPeriodSetup periodSetup in PeriodsSetup.Select(payGroupID))
			{
				yield return periodSetup;
			}
		}

		private bool CreateNextYear(string payGroupID, ref PRPayGroupYear newYear)
		{
			if (newYear == null)
			{
				newYear = new PRPayGroupYear();
				newYear.PayGroupID = payGroupID;
			}

			var yearSetup = PayrollYearSetup.SelectSingle(payGroupID);
			var lastYear = PayrollYear.SelectSingle(payGroupID);
			if(yearSetup == null)
			{
				throw new PXException(Messages.PayrollCalendarNotSetUp, payGroupID);
			}

			if (FiscalYearCreator<PRPayGroupYear, PRPayGroupPeriod>.CreateNextYear(yearSetup, lastYear, newYear))
			{
				//The creator doesn't work with custom periods for subsquent period. See with A. Turok if we fix it in the Fin module.
				if (!FiscalPeriodSetupCreator.IsFixedLengthPeriod(yearSetup.FPType))
				{
					newYear.FinPeriods = yearSetup.FinPeriods;
				}

				// Until the year is fully created with the AutoFill action, use the transaction date exception settings from the previous year.
				newYear.UseTransactionDateExceptions = lastYear?.UseTransactionDateExceptions;
				newYear.TransactionDateExceptionBehavior = lastYear?.TransactionDateExceptionBehavior;

				return true;
			}
			return false;
		}

		private bool IsTransactionExceptionDate(DateTime date)
		{
			if (Setup.Current.NoWeekendTransactionDate == true &&
				(date.DayOfWeek == DayOfWeek.Saturday || date.DayOfWeek == DayOfWeek.Sunday))
			{
				return true;
			}

			if (ExceptionDates.SelectSingle(date) != null)
			{
				return true;
			}

			return false;
		}

		private void AssignTransactionDate(PXCache cache, PRPayGroupPeriod row)
		{
			var currentYearSetup = (PRPayGroupYearSetup)PXResult<PRPayGroupYearSetup>.Current;
			if (currentYearSetup == null)
			{
				currentYearSetup = PayrollYearSetup.SelectSingle();
			}

			DateTime? transactionDate;
			if (FiscalPeriodSetupCreator.IsFixedLengthPeriod(currentYearSetup.FPType))
			{
				//Calendar doesn't have period template (Weekly / Bi-weekly)
				transactionDate = PRPayGroupYearSetupMaint.CalculateFixLengthTransactionDate(row.StartDate, currentYearSetup.PeriodsStartDate, currentYearSetup.TransactionsStartDate);
			}
			else
			{
				var currentPeriodSetup = (PRPayGroupPeriodSetup)PXSelect<PRPayGroupPeriodSetup,
					Where<PRPayGroupPeriodSetup.payGroupID, Equal<Required<PRPayGroupPeriod.payGroupID>>,
					And<PRPayGroupPeriodSetup.periodNbr, Equal<Required<PRPayGroupPeriod.originalPeriodNbr>>>>>.Select(this, row.PayGroupID, row.OriginalPeriodNbr);

				//Calendar has period templates
				transactionDate = PRPayGroupYearSetupMaint.CalculateTransactionDateFromTemplate(row.OriginalYear, currentYearSetup.FirstFinYear, currentPeriodSetup.TransactionDate);
			}

			// FieldUpdating will be called and will adjust transactionDate according to exception rules if needed.
			cache.SetValueExt<PRPayGroupPeriod.transactionDate>(row, transactionDate);
		}

		private void AssignID(IEnumerable<PRPayGroupPeriod> pendingPeriods, string payGroupID, string year)
		{
			var periodQuery = new SelectFrom<PRPayGroupPeriod>
				.Where<PRPayGroupPeriod.payGroupID.IsEqual<P.AsString>
				.And<PRPayGroupPeriod.finYear.IsEqual<P.AsString>>>
				.OrderBy<PRPayGroupPeriod.finPeriodID.Desc>.View(this);

			try
			{
				string lastShiftPeriodNumberUsed = periodQuery.SelectSingle(payGroupID, year)?.PeriodNbr;
				string periodNumber;
				if (lastShiftPeriodNumberUsed == null || lastShiftPeriodNumberUsed[0] <= '9')
				{
					periodNumber = "AA";
				}
				else
				{
					periodNumber = IncrementAlphaCounter(lastShiftPeriodNumberUsed);
				}

				foreach (PRPayGroupPeriod period in pendingPeriods.OrderBy(x => x.StartDate))
				{
					if (period.FinYear == period.OriginalYear)
					{
						period.PeriodNbr = period.OriginalPeriodNbr;
						period.FinPeriodID = period.OriginalFinPeriodID;
					}
					else
					{
						period.PeriodNbr = periodNumber;
						period.FinPeriodID = year + periodNumber;

						periodNumber = IncrementAlphaCounter(periodNumber);
					}
				}
			}
			catch (ArgumentOutOfRangeException)
			{
				throw new PXException(Messages.CantCreatePayGroupPeriodNumber, payGroupID);
			}
		}

		private string IncrementAlphaCounter(string counter)
		{
			StringBuilder builder = new StringBuilder(counter.ToUpper());
			for (int idx = builder.Length - 1; idx >= 0; idx--)
			{
				if (builder[idx] < 'Z')
				{
					builder[idx]++;
					return builder.ToString();
				}
				else
				{
					builder[idx] = 'A';
				}
			}

			throw new ArgumentOutOfRangeException();
		}
		#endregion

		public static PRPayGroupPeriod FindNextPayPeriod(PXGraph graph, string payGroupID, string payPeriodID)
		{
			if (graph == null || payPeriodID == null)
			{
				return null;
			}

			var period = (PRPayGroupPeriod)SelectFrom<PRPayGroupPeriod>
				.Where<PRPayGroupPeriod.payGroupID.IsEqual<@P.AsString>
					.And<PRPayGroupPeriod.finPeriodID.IsGreater<@P.AsString>>>
				.OrderBy<PRPayGroupPeriod.finPeriodID.Asc>.View.Select(graph, payGroupID, payPeriodID);

			return period;
		}
	}

	[PXCacheName(Messages.PREmployeeTax)]
	[Serializable]
	public class PRPayPeriodCreationDialog : IBqlTable
	{
		#region UseExceptions
		public abstract class useExceptions : PX.Data.BQL.BqlBool.Field<useExceptions> { }
		[PXBool]
		[PXUnboundDefault(true)]
		[PXUIField(DisplayName = "Automatically change transactions date based on exception calendar")]
		public virtual bool? UseExceptions { get; set; }
		#endregion
		#region ExceptionDateBehavior
		public abstract class exceptionDateBehavior : PX.Data.BQL.BqlString.Field<exceptionDateBehavior> { }
		[PXString(3, IsFixed = true)]
		[PXUIField(DisplayName = "Transaction Date Behavior")]
		[TransactionDateExceptionBehavior.List]
		[PXUIEnabled(typeof(Where<PRPayPeriodCreationDialog.useExceptions.IsEqual<True>>))]
		public virtual string ExceptionDateBehavior { get; set; }
		#endregion
	}
}