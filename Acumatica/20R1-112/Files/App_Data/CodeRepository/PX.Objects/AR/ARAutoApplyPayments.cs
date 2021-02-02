using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using PX.Data;
using System.Collections;
using PX.Api;
using PX.Objects.CS;
using PX.Objects.GL;
using PX.Objects.AR.MigrationMode;

using LoadChildDocumentsOptions = PX.Objects.AR.ARPaymentEntry.LoadOptions.loadChildDocuments;
using PX.Objects.AR.BQL;
using PX.Objects.GL.FinPeriods;
using PX.Objects.GL.FinPeriods.TableDefinition;

namespace PX.Objects.AR
{
	[Serializable]
	public partial class ARAutoApplyParameters: IBqlTable
	{
		#region ApplyCreditMemos
		public abstract class applyCreditMemos : PX.Data.BQL.BqlBool.Field<applyCreditMemos> { }
		protected bool? _ApplyCreditMemos;
		[PXBool()]
		[PXDefault(false)]
		[PXUIField(DisplayName = "Apply Credit Memos", Visibility = PXUIVisibility.Visible)]
		public virtual bool? ApplyCreditMemos
		{
			get
			{
				return this._ApplyCreditMemos;
			}
			set
			{
				this._ApplyCreditMemos = value;
			}
		}
		#endregion
		#region ReleaseBatchWhenFinished
		public abstract class releaseBatchWhenFinished : PX.Data.BQL.BqlBool.Field<releaseBatchWhenFinished> { }
		protected bool? _ReleaseBatchWhenFinished;
		[PXBool()]
		[PXDefault(false)]
		[PXUIField(DisplayName = "Release Batch When Finished", Visibility = PXUIVisibility.Visible)]
		public virtual bool? ReleaseBatchWhenFinished
		{
			get
			{
				return this._ReleaseBatchWhenFinished;
			}
			set
			{
				this._ReleaseBatchWhenFinished = value;
			}
		}
		#endregion
		#region LoadChildDocuments
		public abstract class loadChildDocuments : PX.Data.BQL.BqlString.Field<loadChildDocuments> { }

		[PXDBString(5, IsFixed = true)]
		[PXUIField(DisplayName = "Include Child Documents")]
		[LoadChildDocumentsOptions.List]
		[PXDefault(LoadChildDocumentsOptions.None)]
		public virtual string LoadChildDocuments { get; set; }
		#endregion
		#region ApplicationDate
		public abstract class applicationDate : PX.Data.BQL.BqlDateTime.Field<applicationDate> { }
		protected DateTime? _ApplicationDate;
		[PXDate()]
		[PXDefault(typeof(AccessInfo.businessDate))]
		[PXUIField(DisplayName = "Application Date", Visibility = PXUIVisibility.Visible)]
		public virtual DateTime? ApplicationDate
		{
			get
			{
				return this._ApplicationDate;
			}
			set
			{
				this._ApplicationDate = value;
			}
		}
		#endregion
		#region FinPeriod
		public abstract class finPeriodID : PX.Data.BQL.BqlString.Field<finPeriodID> { }
		protected String _FinPeriodID;
		[AROpenPeriod(typeof(ARAutoApplyParameters.applicationDate))]
		[PXUIField(DisplayName = "Application Period", Visibility = PXUIVisibility.Visible)]
		public virtual String FinPeriodID
		{
			get
			{
				return this._FinPeriodID;
			}
			set
			{
				this._FinPeriodID = value;
			}
		}
		#endregion
	}

	[TableAndChartDashboardType]
	public class ARAutoApplyPayments : PXGraph<ARAutoApplyPayments>
	{
		public PXCancel<ARAutoApplyParameters> Cancel;
		public PXFilter<ARAutoApplyParameters> Filter;
		[PXFilterable]
		public PXFilteredProcessing<ARStatementCycle, ARAutoApplyParameters> ARStatementCycleList;
		

		public ARAutoApplyPayments()
		{
			ARSetup setup = arsetup.Current;
		}

		public ARSetupNoMigrationMode arsetup;

		protected virtual void ARAutoApplyParameters_RowSelected(PXCache sender, PXRowSelectedEventArgs e)
		{
            if (e.Row == null) return;

            ARAutoApplyParameters filter = Filter.Current;
			ARStatementCycleList.SetProcessDelegate<ARPaymentEntry>(
				delegate(ARPaymentEntry graph, ARStatementCycle cycle)
				{
					ProcessDoc(graph, cycle, filter);
				}
			);

			PXStringListAttribute.SetList<ARAutoApplyParameters.loadChildDocuments>(Filter.Cache, filter,
				filter.ApplyCreditMemos == true ?
				(PXStringListAttribute)new LoadChildDocumentsOptions.ListAttribute() :
				(PXStringListAttribute)new LoadChildDocumentsOptions.NoCRMListAttribute());
		}

		protected virtual void ARAutoApplyParameters_ApplyCreditMemos_FieldUpdated(PXCache cache, PXFieldUpdatedEventArgs e)
		{
			var row = e.Row as ARAutoApplyParameters;
			if(row.ApplyCreditMemos == false && row.LoadChildDocuments == LoadChildDocumentsOptions.IncludeCRM)
			{
				row.LoadChildDocuments = LoadChildDocumentsOptions.ExcludeCRM;
			}
		}

		private static IEnumerable<Customer> GetCustomersForAutoApplication(PXGraph graph, ARStatementCycle cycle, ARAutoApplyParameters filter)
		{
			var result = new List<Customer> { };

			if (filter.LoadChildDocuments != LoadChildDocumentsOptions.None)
			{
				var childrenOfParentsWithCycle = PXSelectJoin<Customer,
						InnerJoin<CustomerMaster,
							On<Customer.parentBAccountID, Equal<CustomerMaster.bAccountID>,
							And<Customer.consolidateToParent, Equal<True>>>,
						InnerJoin<CustomerWithOpenInvoices,
							On<CustomerWithOpenInvoices.customerID, Equal<Customer.bAccountID>>>>,
						Where<CustomerMaster.statementCycleId, Equal<Required<Customer.statementCycleId>>,
							And<Match<Current<AccessInfo.userName>>>>>
					.Select(graph, cycle.StatementCycleId);

				result.AddRange(childrenOfParentsWithCycle.RowCast<Customer>());
			}

			var nonChildrenWithCycle = PXSelectJoin<Customer,
					LeftJoin<CustomerMaster,
						On<Customer.parentBAccountID, Equal<CustomerMaster.bAccountID>,
						And<Customer.consolidateToParent, Equal<True>>>, 
					InnerJoin<CustomerWithOpenPayments,
						On<CustomerWithOpenPayments.customerID, Equal<Customer.bAccountID>,
						And<CustomerWithOpenPayments.statementCycleId, Equal<Required<CustomerWithOpenPayments.statementCycleId>>>>>>,
					Where<Customer.statementCycleId, Equal<Required<Customer.statementCycleId>>,
						And2<Match<Current<AccessInfo.userName>>,
						And<Where<CustomerMaster.bAccountID, IsNull,
							Or<CustomerMaster.statementCycleId, NotEqual<Required<CustomerMaster.statementCycleId>>>>>>>>
				.Select(graph, cycle.StatementCycleId, cycle.StatementCycleId, cycle.StatementCycleId);

			var toAdd = nonChildrenWithCycle.RowCast<Customer>()
				.Where(c => result.FirstOrDefault(alreadyPresent => alreadyPresent.BAccountID == c.BAccountID) == null)
				.ToList();
			result.AddRange(toAdd);
			return result;
		}

		public static void ProcessDoc(ARPaymentEntry graph, ARStatementCycle cycle, ARAutoApplyParameters filter)
		{
			List<ARRegister> toRelease = new List<ARRegister>();

			HashSet<string> trace = new HashSet<string>();
			string warningRefNbr = String.Empty;

			int?[] branches = PXAccess.GetBranchIDs().Select(i => (int?)i).ToArray<int?>();

			foreach (Customer customer in GetCustomersForAutoApplication(graph, cycle, filter))
			{
				List<ARInvoice> arInvoiceList = new List<ARInvoice>();

				var invoiceQuery = new PXSelectJoin<ARInvoice,
					InnerJoin<Customer, On<ARInvoice.customerID, Equal<Customer.bAccountID>>>,
					Where<ARInvoice.released, Equal<True>,
						And<ARInvoice.openDoc, Equal<True>,
						And<ARInvoice.pendingPPD, NotEqual<True>, 
						And<Where<ARInvoice.docType, Equal<ARDocType.invoice>,
							Or<ARInvoice.docType, Equal<ARDocType.finCharge>,
							Or<ARInvoice.docType, Equal<ARDocType.debitMemo>>>>>>>>,
					OrderBy<Asc<ARInvoice.dueDate>>>(graph);

				List<object> invoiceQueryParameters = new List<object>();
				if(filter.LoadChildDocuments == LoadChildDocumentsOptions.None || customer.ParentBAccountID != null)
				{
					invoiceQuery.WhereAnd<Where<Customer.bAccountID, Equal<Required<ARInvoice.customerID>>>>();
				}
				else
				{
					invoiceQuery.WhereAnd<Where<Customer.consolidatingBAccountID, Equal<Required<ARInvoice.customerID>>>>();
				}
				invoiceQueryParameters.Add(customer.BAccountID);

				if (filter.ApplicationDate != null) 
				{
					invoiceQuery.WhereAnd<Where<ARInvoice.docDate,LessEqual<Required<ARInvoice.docDate>>>>();
					invoiceQueryParameters.Add(filter.ApplicationDate);
				}

				PXResultset<ARInvoice> invoices;
				using (new PXReadBranchRestrictedScope(null, branches, true, false))
				{
					invoices = invoiceQuery.Select(invoiceQueryParameters.ToArray());
				}

				foreach (ARInvoice invoice in invoices)
				{
					arInvoiceList.Add(invoice);
				}

				arInvoiceList.Sort(new Comparison<ARInvoice>(delegate(ARInvoice a, ARInvoice b)
					{
						if ((bool)graph.arsetup.Current.FinChargeFirst)
						{
							int aSortOrder = (a.DocType == ARDocType.FinCharge ? 0 : 1);
							int bSortOrder = (b.DocType == ARDocType.FinCharge ? 0 : 1);
							int ret = ((IComparable)aSortOrder).CompareTo(bSortOrder);
							if (ret != 0)
							{
								return ret;
							}
						}

						{
							object aDueDate = a.DueDate;
							object bDueDate = b.DueDate;
							int ret = ((IComparable)aDueDate).CompareTo(bDueDate);

							return ret;
						}
					}
				));

				if (arInvoiceList.Count > 0)
				{
					PXSelectBase<ARPayment> getPaymentsQuery =
						new PXSelectJoin<ARPayment,
								InnerJoin<Branch,
									On<ARPayment.branchID, Equal<Branch.branchID>>,
								InnerJoin<OrganizationFinPeriod,
									On<Branch.organizationID, Equal<OrganizationFinPeriod.organizationID>,
										And<OrganizationFinPeriod.finPeriodID, Equal<Required<OrganizationFinPeriod.finPeriodID>>>>>>,
								Where<
									OrganizationFinPeriod.status, NotEqual<FinPeriod.status.locked>,
									And<OrganizationFinPeriod.status, NotEqual<FinPeriod.status.inactive>,
									And<ARPayment.released, Equal<True>,
									And<ARPayment.openDoc, Equal<True>,
									And<ARPayment.customerID, Equal<Required<ARPayment.customerID>>,
									And<ARPayment.finPeriodID, LessEqual<Required<ARPayment.finPeriodID>>,
									And2<Not<HasUnreleasedVoidPayment<ARPayment.docType, ARPayment.refNbr>>,
									And<Where<
											ARPayment.docType, Equal<ARDocType.payment>,
											Or<ARPayment.docType, Equal<ARDocType.prepayment>,
											Or<ARPayment.docType, Equal<Required<ARPayment.docType>>>>>>>>>>>>>,
								OrderBy<
									Asc<ARPayment.docDate>>>(graph);

					if (!graph.FinPeriodUtils.CanPostToClosedPeriod())
					{
						getPaymentsQuery.WhereAnd<Where<OrganizationFinPeriod.status, NotEqual<FinPeriod.status.closed>>>();
					}

					foreach (ARPayment payment in 
						getPaymentsQuery.Select(filter.FinPeriodID,
												customer.BAccountID,
												filter.FinPeriodID,
												filter.ApplyCreditMemos == true ? ARDocType.CreditMemo : ARDocType.Payment))
					{
						ApplyPayment(graph, filter, payment, arInvoiceList, toRelease, out warningRefNbr);
						trace.Add(warningRefNbr);
					}
				}

				var remainingParentInvoices = arInvoiceList.Where(inv => inv.CustomerID == customer.BAccountID).ToList();

				if (remainingParentInvoices.Count > 0
					&& filter.ApplyCreditMemos == true 
					&& filter.LoadChildDocuments == LoadChildDocumentsOptions.IncludeCRM)
				{
					PXSelectBase<ARPayment> getCreditMemosQuery =
						new PXSelectJoin<ARPayment,
							InnerJoin<Customer, 
								On<ARPayment.customerID, Equal<Customer.bAccountID>>,
							InnerJoin<Branch,
								On<ARPayment.branchID, Equal<Branch.branchID>>,
							InnerJoin<OrganizationFinPeriod,
								On<Branch.organizationID, Equal<OrganizationFinPeriod.organizationID>,
									And<OrganizationFinPeriod.finPeriodID, Equal<Required<OrganizationFinPeriod.finPeriodID>>>>>>>,
							Where<
								OrganizationFinPeriod.status, NotEqual<FinPeriod.status.locked>,
								And<OrganizationFinPeriod.status, NotEqual<FinPeriod.status.inactive>,
								And<ARPayment.released, Equal<True>,
								And<ARPayment.openDoc, Equal<True>,
								And<Customer.consolidatingBAccountID, Equal<Required<Customer.consolidatingBAccountID>>,
								And<ARPayment.docType, Equal<ARDocType.creditMemo>,
								And<Not<HasUnreleasedVoidPayment<ARPayment.docType, ARPayment.refNbr>>>>>>>>>,
							OrderBy<
								Asc<ARPayment.docDate>>>(graph);

					if (!graph.FinPeriodUtils.CanPostToClosedPeriod())
					{
						getCreditMemosQuery.WhereAnd<Where<OrganizationFinPeriod.status, NotEqual<FinPeriod.status.closed>>>();
					}

					foreach (ARPayment payment in 
						getCreditMemosQuery.Select(filter.FinPeriodID, customer.BAccountID))
					{
						ApplyPayment(graph, filter, payment, remainingParentInvoices, toRelease, out warningRefNbr);
						trace.Add(warningRefNbr);
					}
				}
			}

			if (trace != null && trace.Count != 0)
			{
				PXTrace.WriteWarning(String.Format(Messages.FuturePayments, trace.Where(x => !x.IsNullOrEmpty()).JoinToString("; ")));
				trace.Clear();
			}

			if (toRelease.Count > 0)
			{
				ARDocumentRelease.ReleaseDoc(toRelease, false);
			}
		}

		private static void ApplyPayment(ARPaymentEntry graph, ARAutoApplyParameters filter, ARPayment payment, List<ARInvoice> arInvoiceList, List<ARRegister> toRelease, out string warningRefNbr)
		{
			warningRefNbr = String.Empty;

			if (arInvoiceList.Any() == false) return;

			if (payment.DocDate > filter.ApplicationDate)
			{
				warningRefNbr = payment.RefNbr;
				PXProcessing<ARStatementCycle>.SetWarning(Messages.FuturePaymentWarning);
				return;
			}

			int invoiceIndex = 0;
			var paymentsViewIntoInvoiceList = new List<ARInvoice>(arInvoiceList);

			graph.Clear();
			graph.Document.Current = payment;

			bool adjustmentAdded = false;
			while (graph.Document.Current.CuryUnappliedBal > 0 && invoiceIndex < paymentsViewIntoInvoiceList.Count)
			{
				if (graph.Document.Current.CuryApplAmt == null)
				{
					object curyapplamt = graph.Document.Cache.GetValueExt<ARPayment.curyApplAmt>(graph.Document.Current);
					if (curyapplamt is PXFieldState)
					{
						curyapplamt = ((PXFieldState)curyapplamt).Value;
					}
					graph.Document.Current.CuryApplAmt = (decimal?)curyapplamt;
				}
				graph.Document.Current.AdjDate = filter.ApplicationDate;
				
				FinPeriodIDAttribute.SetPeriodsByMaster<ARPayment.adjFinPeriodID>(graph.Document.Cache, graph.Document.Current, filter.FinPeriodID);

				graph.Document.Cache.Adjust<PX.Objects.AR.AROpenPeriodAttribute>().For<ARPayment.adjFinPeriodID>(atr => atr.RedefaultOnDateChanged = false);
				graph.Document.Cache.Update(graph.Document.Current);

				ARInvoice invoice = paymentsViewIntoInvoiceList[invoiceIndex];

				var trans = invoice.PaymentsByLinesAllowed == true
					? PXSelect<ARTran,
						Where<ARTran.tranType, Equal<Required<ARTran.tranType>>,
							And<ARTran.refNbr, Equal<Required<ARTran.refNbr>>>>>.Select(graph, invoice.DocType, invoice.RefNbr)
					: new PXResultset<ARTran>() { null };

				foreach (ARTran tran in trans)
				{
					ARAdjust adj = new ARAdjust();
					adj.AdjdDocType = invoice.DocType;
					adj.AdjdRefNbr = invoice.RefNbr;
					adj.AdjdLineNbr = tran?.LineNbr ?? 0;

					graph.AutoPaymentApp = true;
					adj = graph.Adjustments.Insert(adj);

					if (adj != null)
					{
						adjustmentAdded = true;
						if (adj.CuryDocBal <= 0m)
						{
							arInvoiceList.Remove(invoice);
						}
					}
				}

				invoiceIndex++;
			}
			if (adjustmentAdded)
			{
				graph.Save.Press();
				if (filter.ReleaseBatchWhenFinished == true)
				{
					toRelease.Add(graph.Document.Current);
				}
			}
		}

		protected virtual void ARStatementCycle_RowSelecting(PXCache sender, PXRowSelectingEventArgs e)
		{
			if (e.Row != null)
			{
				ARStatementCycle row = e.Row as ARStatementCycle;

				row.NextStmtDate = ARStatementProcess.CalcStatementDateBefore(
					sender.Graph,
					this.Accessinfo.BusinessDate.Value, 
					row.PrepareOn, 
					row.Day00, 
					row.Day01,
					row.DayOfWeek);

				if (row.LastStmtDate.HasValue && row.NextStmtDate <= row.LastStmtDate)
				{
					row.NextStmtDate = ARStatementProcess.CalcNextStatementDate(
						sender.Graph,
						row.LastStmtDate.Value, 
						row.PrepareOn, 
						row.Day00, 
						row.Day01,
						row.DayOfWeek);
				}
			}
		}
	}
}
