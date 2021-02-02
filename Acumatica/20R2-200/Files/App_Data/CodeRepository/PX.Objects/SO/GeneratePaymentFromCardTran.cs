using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PX.Data;
using PX.Common;
using PX.Objects.AR;
using PX.Objects.AR.CCPaymentProcessing.Helpers;
using PX.Objects.AR.CCPaymentProcessing.Common;
using PX.Objects.SO;
using PX.Objects.GL;
using PX.Objects.Extensions.PaymentTransaction;

namespace PX.Objects.SO
{
	public class GeneratePaymentFromCardTran : PXGraph<GeneratePaymentFromCardTran>
	{
		public PXFilter<TransactionFilter> Filter;
		public PXCancel<TransactionFilter> Cancel;
		public PXFilteredProcessingOrderBy<ExternalTransactionExt, TransactionFilter, OrderBy<Asc<ExternalTransactionExt.lastActivityDate>>> ExternalTransactionList;

		public PXSetup<SOOrderType, Where<SOOrderType.orderType, Equal<Optional<SOOrder.orderType>>>> SOOrderType;

		public GeneratePaymentFromCardTran()
		{
			ExternalTransactionList.SetProcessDelegate(Process);
		}

		public virtual IEnumerable externalTransactionList()
		{
			var query = new PXSelect<ExternalTransactionExt,
				Where<ExternalTransactionExt.active, Equal<True>,
					And<ExternalTransactionExt.direction, Equal<ExternalTransaction.TransactionDirection.debetTransactionDirection>,
					And2<Where<ExternalTransactionExt.aRDocTypeForSO, IsNull, Or<ExternalTransactionExt.aRDocTypeForSO, Equal<ARDocType.invoice>>>,
					And2<Where<ExternalTransactionExt.origDocType, IsNotNull, And<ExternalTransactionExt.docType, IsNull, Or<ExternalTransactionExt.docType, Equal<ARDocType.invoice>>>>,
						And<Not<ExternalTransactionExt.procStatus, NotEqual<ExtTransactionProcStatusCode.captureSuccess>,
							And<ExternalTransactionExt.procStatus, NotEqual<ExtTransactionProcStatusCode.voidFailed>,
							And<ExternalTransactionExt.procStatus, NotEqual<ExtTransactionProcStatusCode.voidDeclined>,
							And<ExternalTransactionExt.expirationDate, IsNotNull,
							And<ExternalTransactionExt.expirationDate, Less<Required<ExternalTransaction.expirationDate>>>>>>>>>>>>,
				OrderBy<Asc<ExternalTransactionExt.lastActivityDate>>>(this);

			if (Filter.Current.StartDate != null)
			{
				query.WhereAnd<Where<ExternalTransactionExt.lastActivityDate, GreaterEqual<Current<TransactionFilter.startDate>>>>();
			}

			if (Filter.Current.EndDate != null)
			{
				query.WhereAnd<Where<DateDiff<ExternalTransactionExt.lastActivityDate, Current<TransactionFilter.endDate>, DateDiff.day>, GreaterEqual<Zero>>>();
			}

			object[] prms = new object[] { PXTimeZoneInfo.Now };
			object[] currents = new object[] { Filter.Current };
			int startRow = PXView.StartRow;
			int totalRows = 0;
			foreach (ExternalTransactionExt tran in query.View.Select(currents, prms, PXView.Searches, PXView.SortColumns, PXView.Descendings, PXView.Filters, ref startRow, PXView.MaximumRows, ref totalRows))
			{
				ExternalTransactionState state = ExternalTranHelper.GetTransactionState(this, tran);
				tran.PreAuthorized = state.IsPreAuthorized;
				tran.Captured = state.IsCaptured;

				if (tran.OrigDocType != null && tran.OrigRefNbr != null && tran.DocType == null 
					&& tran.ARDocTypeForSO == ARDocType.Invoice)
				{
					SOOrderType type = SOOrderType.Select(tran.OrigDocType);
					tran.SODocType = type.Descr;
					tran.SORefNbr = tran.OrigRefNbr;
					tran.InitFromSO = true;
				}
				if (tran.DocType != null && tran.RefNbr != null && IsInvoice(tran))
				{
					tran.SODocType = PXMessages.LocalizeNoPrefix(AR.Messages.Invoice);
					tran.SORefNbr = tran.RefNbr;
				}
	
				ExternalTransactionList.Cache.Hold(tran);
				yield return tran;
			}
			PXView.StartRow = 0;
		}

		[PXHidden]
		public class TransactionFilter : IBqlTable
		{
			#region StartDate
			public abstract class startDate : PX.Data.BQL.BqlDateTime.Field<startDate> { }

			[PXDBDate(PreserveTime = true, DisplayMask = "d")]
			[PXUIField(DisplayName = "Start Date")]
			public virtual DateTime? StartDate {
				get; 
				set; 
			}
			#endregion
			#region EndDate
			public abstract class endDate : PX.Data.BQL.BqlDateTime.Field<endDate> { }

			[PXDBDate(PreserveTime = true, DisplayMask = "d")]
			[PXUIField(DisplayName = "End Date")]
			public virtual DateTime? EndDate { 
				get; 
				set; 
			}
			#endregion
		}

		[PXHidden]
		[PXProjection(typeof(Select2<ExternalTransaction,
			LeftJoin<CustomerPaymentMethod,On<CustomerPaymentMethod.pMInstanceID, Equal<ExternalTransaction.pMInstanceID>>,
			LeftJoin<SOOrderType,On<SOOrderType.orderType,Equal<ExternalTransaction.origDocType>>>>>))]
		public class ExternalTransactionExt : ExternalTransaction
		{
			public new abstract class transactionID : PX.Data.BQL.BqlInt.Field<transactionID> { }
			
			[PXDBDate(PreserveTime = true, DisplayMask = "d")]
			[PXUIField(DisplayName = "Last CC Tran. Date", Visibility = PXUIVisibility.SelectorVisible)]
			public override DateTime? LastActivityDate { get; set; }

			#region SODocType
			public abstract class sOrderType : PX.Data.BQL.BqlString.Field<sOrderType> { }
			[PXString]
			[PXUIField(DisplayName = "Doc. Type", Visible = true)]
			public virtual string SODocType { get; set; }
			#endregion

			#region SORefNbr
			public abstract class sORefNbr : PX.Data.BQL.BqlString.Field<sORefNbr> { }
			[PXString]
			[PXUIField(DisplayName = "Doc Reference Nbr.", Visible = true)]
			public virtual string SORefNbr { get; set; }
			#endregion

			#region PaymentMethodID
			public abstract class descr : PX.Data.BQL.BqlString.Field<descr> { }
			[PXDBString(255, BqlField = typeof(CustomerPaymentMethod.descr))]
			[PXUIField(DisplayName = "Card/Account Nbr.")]
			public string Descr { get; set; }
			#endregion

			#region BAccountID
			public abstract class bAccountID : PX.Data.BQL.BqlInt.Field<bAccountID> { }
			[Customer(DescriptionField = typeof(Customer.acctCD), BqlField = typeof(CustomerPaymentMethod.bAccountID))]
			public virtual Int32? BAccountID
			{
				get; set;
			}
			#endregion

			#region PreAuthorized
			public abstract class preAuthorized : PX.Data.BQL.BqlBool.Field<preAuthorized> { }
			[PXBool]
			[PXUIField(DisplayName = "Pre-Authorized", Visible = true)]
			public bool? PreAuthorized { get; set; }
			#endregion

			#region Captured
			public abstract class captured : PX.Data.BQL.BqlBool.Field<captured> { }
			[PXBool]
			[PXUIField(DisplayName = "Captured", Visible = true)]
			public bool? Captured { get; set; }
			#endregion

			#region ARDocTypeForSO
			public abstract class aRDocTypeForSO : PX.Data.BQL.BqlInt.Field<aRDocTypeForSO> { }
			[PXDBString(3, BqlField = typeof(SOOrderType.aRDocType))]
			public virtual string ARDocTypeForSO
			{
				get; set;
			}
			#endregion

			public bool InitFromSO { get; set; }
		}

		protected virtual void TransactionFilterStartDateFieldDefaulting(Events.FieldDefaulting<TransactionFilter.startDate> e)
		{
			var filter = e.Row as TransactionFilter;
			if (filter == null) return;
			var startDate = PXTimeZoneInfo.Now.Subtract(new TimeSpan(62, 0, 0, 0)).Date;
			e.NewValue = startDate;
		}

		private static void Process(List<ExternalTransactionExt> items)
		{
			var factory = PXGraph.CreateInstance<PaymentCreatorFactory>();
			bool failed = false;
			for (int i = 0; i < items.Count; i++)
			{
				ExternalTransactionExt item = items[i];
				try
				{
					factory.ClearGraphs();
					var creator = factory.GetCreatorByTran(item);
					if (item.PreAuthorized == true)
					{
						creator.CreateAuthorizedPayment();
					}
					else if (item.Captured == true)
					{
						creator.CreateCapturedPayment();
					}
					PXProcessing<ExternalTransactionExt>.SetInfo(i, Messages.Completed);
				}
				catch (PXOuterException ex)
				{
					failed = true;
					string errMsg = ex.Message + Environment.NewLine + string.Join(Environment.NewLine, ex.InnerMessages);
					PXProcessing<ExternalTransactionExt>.SetError(i, errMsg);
				}
				catch (Exception ex)
				{
					failed = true;
					if (ex.InnerException != null)
					{
						PXProcessing<ExternalTransactionExt>.SetError(i, ex.InnerException);
					}
					else
					{
						PXProcessing<ExternalTransactionExt>.SetError(i, ex);
					}
				}
			}

			if (failed)
			{
				throw new PXOperationCompletedWithErrorException(ErrorMessages.SeveralItemsFailed);
			}
		}

		private static bool IsInvoice(ExternalTransactionExt tran)
		{
			return tran.DocType == ARDocType.Invoice;
		}

		public static CustomerPaymentMethod GetCustPaymentMethodById(PXGraph graph, int? pmInstanceId)
		{
			CustomerPaymentMethod cpm = PXSelect<CustomerPaymentMethod,
				Where<CustomerPaymentMethod.pMInstanceID, Equal<Required<CustomerPaymentMethod.pMInstanceID>>>>.Select(graph, pmInstanceId);
			return cpm;
		}

		[PXMergeAttributes(Method = MergeMethod.Append)]
		[PXCustomizeBaseAttribute(typeof(PXUIFieldAttribute), nameof(PXUIFieldAttribute.DisplayName), "")]
		protected virtual void ExternalTransactionExt_Selected_CacheAttached(PXCache sender)
		{

		}

		public abstract class PaymentCreator
		{
			public abstract void CreateAuthorizedPayment();
			public abstract void CreateCapturedPayment();
		}

		public class PaymentCreatorFactory : PXGraph<PaymentCreatorFactory>
		{
			ARPaymentEntry paymentGraph;
			SOOrderEntry orderGraph;
			SOInvoiceEntry invoiceGraph;

			public PaymentCreatorFactory() : base()
			{
				paymentGraph = CreateInstance<ARPaymentEntry>();
				orderGraph = CreateInstance<SOOrderEntry>();
				invoiceGraph = CreateInstance<SOInvoiceEntry>();
			}

			public virtual PaymentCreator GetCreatorByTran(ExternalTransactionExt tran)
			{
				PaymentCreator ret = null;
				bool isInvoice = IsInvoice(tran);
				List<ARInvoice> refInvoices = new List<ARInvoice>();				
				if (!isInvoice)
				{
					refInvoices = GetInvoicesByOrigRefNbr(tran);
					var invoice = refInvoices.FirstOrDefault();
					if (invoice != null)
					{
						tran.RefNbr = invoice.RefNbr;
						tran.DocType = invoice.DocType;
						isInvoice = true;
					}
				}
				CheckDocumentHasMultipleActiveTransactions(tran);
				if (!isInvoice)
				{
					ret = new SOOrderPaymentCreator(tran, orderGraph, paymentGraph);
				}
				else
				{
					ret = new SOInvoicePaymentCreator(tran, refInvoices, invoiceGraph, paymentGraph);
				}
				return ret;
			}

			public void ClearGraphs()
			{
				paymentGraph.Clear();
				orderGraph.Clear();
				invoiceGraph.Clear();
			}

			private void CheckDocumentHasMultipleActiveTransactions(ExternalTransactionExt tran)
			{
				IEnumerable<ExternalTransactionExt> trans = null;
				string doc = string.Empty;
				if (IsInvoice(tran))
				{
					doc = tran.DocType + tran.RefNbr;
					var query = new PXSelect<ExternalTransactionExt, Where<ExternalTransactionExt.docType, Equal<Required<ExternalTransactionExt.refNbr>>,
						And<ExternalTransactionExt.refNbr, Equal<Required<ExternalTransactionExt.refNbr>>,
						And<ExternalTransactionExt.active, Equal<True>>>>>(this);
					trans = query.Select(tran.DocType, tran.RefNbr).RowCast<ExternalTransactionExt>();
				}
				else
				{
					doc = tran.OrigDocType + tran.OrigRefNbr;
					var query = new PXSelect<ExternalTransactionExt, Where<ExternalTransactionExt.origDocType, Equal<Required<ExternalTransactionExt.origDocType>>,
						And<ExternalTransactionExt.origRefNbr, Equal<Required<ExternalTransactionExt.origRefNbr>>,
						And<ExternalTransactionExt.refNbr, Equal<Null>, And<ExternalTransactionExt.active, Equal<True>>>>>>(this);
					trans = query.Select(tran.OrigDocType, tran.OrigRefNbr).RowCast<ExternalTransactionExt>();
				}

				int activeTranCnt = trans.Select(i => {
					return SuitableForProcessing(ExternalTranHelper.GetTransactionState(this, i));
				}).Where(i => i == true).Count();

				if (activeTranCnt > 1)
				{
					throw new PXException(AR.Messages.CCProcessingMultipleActiveTran, doc);
				}
			}

			private List<ARInvoice> GetInvoicesByOrigRefNbr(ExternalTransactionExt tran)
			{
				List<ARInvoice> ret = new List<ARInvoice>();
				var query = new PXSelectJoin<ARInvoice,
					InnerJoin<SOOrderShipment, On<SOOrderShipment.invoiceType, Equal<ARInvoice.docType>,
						And<SOOrderShipment.invoiceNbr, Equal<ARInvoice.refNbr>>>,
					InnerJoin<ARRegister, On<ARRegister.docType, Equal<ARInvoice.docType>, And<ARRegister.refNbr, Equal<ARInvoice.refNbr>>>>>,
					Where<SOOrderShipment.orderType, Equal<Required<SOOrderShipment.orderType>>,
						And<SOOrderShipment.orderNbr, Equal<Required<SOOrderShipment.orderNbr>>,
						And<ARInvoice.docType, Equal<ARDocType.invoice>, And<ARRegister.openDoc, Equal<True>>>>>,
					OrderBy<Asc<ARRegister.createdDateTime>>>(this);
				foreach (ARInvoice invoice in query.Select(tran.OrigDocType, tran.OrigRefNbr))
				{
					ret.Add(invoice);
				}
				return ret;
			}
		}

		public static bool SuitableForProcessing(ExternalTransactionState ext)
		{
			return !(ext.IsActive == false || ext.IsRefunded || ext.ProcessingStatus == ProcessingStatus.AuthorizeExpired);
		}

		public class SOInvoicePaymentCreator : PaymentCreator
		{
			ExternalTransactionExt tran;
			SOInvoiceEntry invoiceGraph;
			ARPaymentEntry paymentGraph;
			List<ARInvoice> refInvoices;

			ARPayment payment => paymentGraph.Document.Current;
			ARInvoice invoice => invoiceGraph.Document.Current;
			SOInvoice soInvoice => invoiceGraph.SODocument.Select();

			public SOInvoicePaymentCreator(ExternalTransactionExt tran, List<ARInvoice> refInvoices, SOInvoiceEntry invoiceGraph, ARPaymentEntry paymentGraph)
			{
				this.tran = tran;
				this.invoiceGraph = invoiceGraph;
				this.paymentGraph = paymentGraph;
				this.refInvoices = refInvoices;
				invoiceGraph.Document.Current = invoiceGraph.Document.Search<ARInvoice.refNbr>(tran.RefNbr, tran.DocType);
			}

			public override void CreateAuthorizedPayment()
			{
				using (PXTransactionScope scope = new PXTransactionScope())
				{
					UpdateRelatedSalesOrder();

					SOInvoice soInvoice = this.soInvoice;
					PopulateFields();
					SetFinPeriod();
					paymentGraph.Save.Press();

					MoveCurrentActiveTransaction();

					ARPaymentAfterProcessingManager manager = new ARPaymentAfterProcessingManager() { Graph = paymentGraph };
					manager.RunAuthorizeActions(payment, true);
					manager.PersistData();

					MoveInactiveTransactions();

					soInvoice.IsCCCaptured = false;
					soInvoice.IsCCCaptureFailed = false;
					soInvoice.CuryCCCapturedAmt = 0m;
					soInvoice.HasLegacyCCTran = false;
					invoiceGraph.SODocument.Update(soInvoice);
					decimal? transferred = 0m;
					if (tran.OrigDocType != null && tran.OrigRefNbr != null && refInvoices.Count > 0)
					{
						transferred = UpdateInvoices();
					}
					else
					{
						LinkPaymentWithDoc(payment.CuryDocBal);
						soInvoice.HasLegacyCCTran = false;
						invoiceGraph.SODocument.Update(soInvoice);
						invoiceGraph.Save.Press();
						ARAdjust2 adjust = invoiceGraph.Adjustments.Select();
						transferred = adjust.CuryAdjdAmt;
					}

					if (tran.OrigDocType != null && tran.OrigRefNbr != null)
					{
						AddSOAdjustLinkARAdjust(transferred);
					}
					scope.Complete();
				}
			}

			public override void CreateCapturedPayment()
			{
				using (PXTransactionScope scope = new PXTransactionScope())
				{
					UpdateRelatedSalesOrder();

					SOInvoice soInvoice = invoiceGraph.SODocument.Select();
					PopulateFields();
					SetFinPeriod();
					paymentGraph.Save.Press();

					MoveCurrentActiveTransaction();

					ARPaymentAfterProcessingManager manager = new ARPaymentAfterProcessingManager() { ReleaseDoc = true, Graph = paymentGraph };
					manager.RunCaptureActions(payment, true);
					manager.PersistData();

					MoveInactiveTransactions();

					soInvoice.IsCCCaptured = false;
					soInvoice.IsCCCaptureFailed = false;
					soInvoice.CuryCCCapturedAmt = 0m;
					soInvoice.HasLegacyCCTran = false;
					invoiceGraph.SODocument.Update(soInvoice);

					LinkPaymentWithDoc(payment.CuryDocBal);

					invoiceGraph.Save.Press();
					ARAdjust2 adjust = invoiceGraph.Adjustments.Select();
					decimal? transferred = adjust.CuryAdjdAmt;

					if (tran.OrigDocType != null && tran.OrigRefNbr != null)
					{
						AddSOAdjustLinkARAdjust(transferred);
					}
					scope.Complete();
				}
			}

			private void AddSOAdjustLinkARAdjust(decimal? transferred)
			{
				PXFieldVerifying cancelHandler = (PXCache sender, PXFieldVerifyingEventArgs e) => { e.Cancel = true; };
				paymentGraph.IgnoreNegativeOrderBal = true;
				paymentGraph.FieldVerifying.AddHandler<SOAdjust.adjdOrderNbr>(cancelHandler);
				paymentGraph.FieldVerifying.AddHandler<SOAdjust.curyAdjgAmt>(cancelHandler);
								
				SOAdjust adj = new SOAdjust();
				try
				{
					adj.AdjdOrderType = tran.OrigDocType;
					adj.AdjdOrderNbr = tran.OrigRefNbr;
					adj = paymentGraph.SOAdjustments.Insert(adj);
					if (transferred != 0)
					{
						adj.CuryAdjgAmt
							= adj.CuryOrigAdjgAmt
							= adj.CuryOrigAdjdAmt
							= adj.OrigAdjAmt
							= payment.CuryDocBal;
						adj = paymentGraph.SOAdjustments.Update(adj);

						adj.CuryAdjdBilledAmt
							= adj.CuryAdjgBilledAmt
							= adj.AdjBilledAmt
							= transferred;
						adj = paymentGraph.SOAdjustments.Update(adj);
					}
					paymentGraph.Save.Press();
				}
				finally
				{
					paymentGraph.FieldVerifying.RemoveHandler<SOAdjust.adjdOrderNbr>(cancelHandler);
					paymentGraph.FieldVerifying.RemoveHandler<SOAdjust.curyAdjgAmt>(cancelHandler);
					paymentGraph.IgnoreNegativeOrderBal = false;
				}

				foreach (ARAdjust item in paymentGraph.Adjustments_Invoices.Select())
				{
					item.AdjdOrderType = adj.AdjdOrderType;
					item.AdjdOrderNbr = adj.AdjdOrderNbr;
					paymentGraph.Adjustments_Invoices.Cache.MarkUpdated(item);
				}
				paymentGraph.Save.Press();
			}

			private bool InvoiceHasAnotherTran(ARInvoice invoice)
			{
				var query = new PXSelectReadonly<ExternalTransactionExt, Where<ExternalTransactionExt.docType, Equal<ARDocType.invoice>,
					And<ExternalTransactionExt.origDocType, IsNull, And<ExternalTransactionExt.active, Equal<True>, 
					And<ExternalTransactionExt.docType, Equal<Required<ExternalTransactionExt.docType>>,
					And<ExternalTransactionExt.refNbr, Equal<Required<ExternalTransactionExt.refNbr>>,
					And<ExternalTransactionExt.transactionID, NotEqual<Required<ExternalTransactionExt.transactionID>>>>>>>>>(invoiceGraph);
				ExternalTransactionExt res = query.Select(invoice.DocType, invoice.RefNbr, tran.TransactionID);
				return res != null;
			}

			private decimal? UpdateInvoices()
			{
				decimal? transferred = 0m;
				decimal? pCuryDocBal = payment.CuryDocBal;
				foreach (ARInvoice invoice in refInvoices)
				{
					if (InvoiceHasAnotherTran(invoice)) continue;
					invoiceGraph.Document.Current = invoiceGraph.Document.Search<ARInvoice.refNbr>(invoice.RefNbr, invoice.DocType);
					ARAdjust2 newAdj = null;
					if (pCuryDocBal > 0)
					{
						newAdj = LinkPaymentWithDoc(pCuryDocBal);
					}
					soInvoice.HasLegacyCCTran = false;
					invoiceGraph.SODocument.Update(soInvoice);
					invoiceGraph.Save.Press();
					if (newAdj?.CuryAdjdAmt > 0)
					{
						pCuryDocBal -= newAdj.CuryAdjdAmt;
						transferred += newAdj.CuryAdjdAmt;
					}
				}
				return transferred;
			}

			private void UpdateRelatedSalesOrder()
			{
				if (tran.OrigRefNbr == null) return;
				PXDatabase.Update<SOOrder>(
					new PXDataFieldAssign("IsCCAuthorized", false),
					new PXDataFieldAssign("IsCCCaptured", false),
					new PXDataFieldAssign("IsCCCaptureFailed", false),
					new PXDataFieldAssign("CuryCCCapturedAmt", 0m),
					new PXDataFieldAssign("CCCapturedAmt", 0m),
					new PXDataFieldAssign("CuryCCPreAuthAmount", 0m),
					new PXDataFieldAssign("CCAuthExpirationDate", null),
					new PXDataFieldAssign("HasLegacyCCTran", false),
					new PXDataFieldRestrict("OrderType", PXDbType.Char, tran.OrigDocType),
					new PXDataFieldRestrict("OrderNbr", PXDbType.NVarChar, tran.OrigRefNbr)
				);
			}

			private void SetFinPeriod()
			{

				AROpenPeriodAttribute openPeriodAttribute = paymentGraph.Document.Cache.GetAttributesReadonly<ARPayment.adjFinPeriodID>()
					.OfType<AROpenPeriodAttribute>().FirstOrDefault();
				PeriodValidation prevPeriodValidationVal = openPeriodAttribute.ValidatePeriod;
				AROpenPeriodAttribute.SetValidatePeriod<ARPayment.adjFinPeriodID>(paymentGraph.Document.Cache, payment, PeriodValidation.Nothing);

				paymentGraph.Document.Update(payment);

				OpenPeriodAttribute.SetValidatePeriod<ARPayment.adjFinPeriodID>(paymentGraph.Document.Cache, payment, prevPeriodValidationVal);
				if (soInvoice.AdjTranPeriodID != null)
					FinPeriodIDAttribute.SetPeriodsByMaster<ARPayment.adjFinPeriodID>(paymentGraph.Document.Cache, payment, soInvoice.AdjTranPeriodID);
				openPeriodAttribute.IsValidPeriod(paymentGraph.Document.Cache, payment, payment.AdjFinPeriodID);
			}

			private void PopulateFields()
			{
				ARPayment payment = new ARPayment();
				CustomerPaymentMethod cpm = GeneratePaymentFromCardTran.GetCustPaymentMethodById(paymentGraph, tran.PMInstanceID);
				ARInvoice invoice = invoiceGraph.Document.Current;
				payment.AdjDate = soInvoice.AdjDate;
				payment.DocType = ARDocType.Payment;
				payment.CustomerID = cpm.BAccountID;
				payment.CustomerLocationID = invoice.CustomerLocationID;
				payment.ARAccountID = invoice.ARAccountID;
				payment.ARSubID = invoice.ARSubID;
				payment.PaymentMethodID = cpm.PaymentMethodID;
				payment.PMInstanceID = cpm.PMInstanceID;
				payment.CashAccountID = invoice.CashAccountID;
				payment.CuryOrigDocAmt = tran.Amount;
				payment.DocDesc = invoice.DocDesc;
				paymentGraph.Document.Update(payment);
			}

			private ARAdjust2 LinkPaymentWithDoc(decimal? pCuryDocBal)
			{
				ARPayment payment = paymentGraph.Document.Current;
				ARInvoice invoice = invoiceGraph.Document.Current;
				decimal? amt = pCuryDocBal > invoice.CuryUnpaidBalance ? invoice.CuryUnpaidBalance : pCuryDocBal;
				decimal? discAmt = pCuryDocBal > invoice.CuryUnpaidBalance ? 0m : invoice.CuryDiscBal;

				if (discAmt + amt > invoice.CuryUnpaidBalance)
				{
					discAmt = invoice.CuryDocBal - amt;
				}

				ARAdjust2 adj = new ARAdjust2()
				{
					AdjNbr = payment.AdjCntr,
					AdjdBranchID = invoice.BranchID,
					AdjgBranchID = payment.BranchID,
					AdjdDocType = invoice.DocType,
					AdjdRefNbr = invoice.RefNbr,
					AdjgDocType = payment.DocType,
					AdjgRefNbr = payment.RefNbr,
					CuryAdjdAmt = amt,
					CuryAdjgAmt = amt,
					CuryAdjgDiscAmt = discAmt,
					AdjdCustomerID = invoice.CustomerID,
					CustomerID = invoice.CustomerID,
					AdjgCuryInfoID = payment.CuryInfoID,
					AdjdCuryInfoID = invoice.CuryInfoID
				};
				adj = invoiceGraph.Adjustments.Insert(adj);
				return adj;
			}


			private void MoveCurrentActiveTransaction()
			{
				IEnumerable<ExternalTransactionExt> ordersWithTrans = PXSelectJoin<ExternalTransactionExt,
						InnerJoin<SOOrderShipment, On<SOOrderShipment.orderType, Equal<ExternalTransactionExt.origDocType>,
							And<SOOrderShipment.orderNbr, Equal<ExternalTransactionExt.origRefNbr>>>>,
						Where<SOOrderShipment.invoiceType, Equal<Required<SOOrderShipment.invoiceType>>,
							And<SOOrderShipment.invoiceNbr, Equal<Required<SOOrderShipment.invoiceNbr>>, And<ExternalTransactionExt.active, Equal<True>>>>>
						.Select(paymentGraph, invoice.DocType, invoice.RefNbr).RowCast<ExternalTransactionExt>();
				if (ordersWithTrans.Select(i => i.OrigRefNbr).Distinct().Count() > 1)
				{
					throw new PXException(AR.Messages.ERR_CCMultiplyPreauthCombined);
				}
				MoveTransactions(new[] { tran });
			}

			private void MoveInactiveTransactions()
			{
				IEnumerable<ExternalTransaction> res = null;
				if (tran.InitFromSO)
				{
					var query = new PXSelectReadonly<ExternalTransaction, Where<ExternalTransaction.origDocType, Equal<Required<ExternalTransaction.origDocType>>,
						And<ExternalTransaction.origRefNbr, Equal<Required<ExternalTransaction.origRefNbr>>,
						And<ExternalTransaction.refNbr, IsNull>>>>(invoiceGraph);
					res = query.Select(tran.OrigDocType, tran.OrigRefNbr).RowCast<ExternalTransaction>();
				}
				else
				{
					var query = new PXSelectReadonly<ExternalTransaction, Where<ExternalTransaction.docType, Equal<Required<ExternalTransaction.docType>>,
						And<ExternalTransaction.refNbr, Equal<Required<ExternalTransaction.refNbr>>>>>(invoiceGraph);
					res = query.Select(tran.DocType, tran.RefNbr).RowCast<ExternalTransaction>();
				}

				res = res.Where(i => i.Active == false 
					|| ExternalTranHelper.GetTransactionState(invoiceGraph, i).ProcessingStatus == ProcessingStatus.AuthorizeExpired);

				MoveTransactions(res);
			}

			private void MoveTransactions(IEnumerable<ExternalTransaction> trans)
			{
				foreach (ExternalTransaction item in trans)
				{
					PXDatabase.Update<ExternalTransaction>(
						new PXDataFieldAssign("DocType", payment.DocType),
						new PXDataFieldAssign("RefNbr", payment.RefNbr),
						new PXDataFieldRestrict("TransactionID", PXDbType.Int, 4, item.TransactionID, PXComp.EQ)
					);
					PXDatabase.Update<CCProcTran>(
						new PXDataFieldAssign("DocType", payment.DocType),
						new PXDataFieldAssign("RefNbr", payment.RefNbr),
						new PXDataFieldRestrict("TransactionID", PXDbType.Int, 4, item.TransactionID, PXComp.EQ)
					);
				}
			}
		}

		public class SOOrderPaymentCreator : PaymentCreator
		{
			ExternalTransactionExt tran;
			SOOrderEntry orderGraph;
			ARPaymentEntry paymentGraph;

			ARPayment payment => paymentGraph.Document.Current;
			SOOrder order => orderGraph.Document.Current;

			public SOOrderPaymentCreator(ExternalTransactionExt tran, SOOrderEntry orderGraph, ARPaymentEntry paymentGraph)
			{
				this.tran = tran;
				this.orderGraph = orderGraph;
				this.paymentGraph = paymentGraph;
				orderGraph.Document.Current = orderGraph.Document.Search<SOOrder.orderNbr>(tran.OrigRefNbr, tran.OrigDocType);
			}

			public override void CreateAuthorizedPayment()
			{
				using (PXTransactionScope scope = new PXTransactionScope())
				{
					PXDatabase.Update<SOOrder>(
						new PXDataFieldAssign("HasLegacyCCTran", false),
						new PXDataFieldAssign("IsCCAuthorized", false),
						new PXDataFieldAssign("CuryCCPreAuthAmount", 0m),
						new PXDataFieldAssign("CCAuthExpirationDate", null),
						new PXDataFieldRestrict("OrderType", PXDbType.Char, tran.OrigDocType),
						new PXDataFieldRestrict("OrderNbr", PXDbType.NVarChar, tran.OrigRefNbr)
					);

					PopulateFields();
					paymentGraph.Save.Press();

					MoveCurrentActiveTransaction();

					ARPaymentAfterProcessingManager manager = new ARPaymentAfterProcessingManager() { Graph = paymentGraph };
					manager.RunAuthorizeActions(payment, true);

					MoveInactiveTransactions();

					LinkPaymentWithDoc();
					paymentGraph.Save.Press();

					scope.Complete();
				}
			}

			public override void CreateCapturedPayment()
			{
				using (PXTransactionScope scope = new PXTransactionScope())
				{
					PXDatabase.Update<SOOrder>(
						new PXDataFieldAssign("HasLegacyCCTran", false),
						new PXDataFieldAssign("IsCCAuthorized", false),
						new PXDataFieldAssign("IsCCCaptured", false),
						new PXDataFieldAssign("IsCCCaptureFailed", false),
						new PXDataFieldAssign("CCAuthExpirationDate", null),
						new PXDataFieldAssign("CuryCCPreAuthAmount", 0m),
						new PXDataFieldAssign("CuryCCCapturedAmt", 0m),
						new PXDataFieldAssign("CCCapturedAmt", 0m),
						new PXDataFieldRestrict("OrderType", PXDbType.Char, tran.OrigDocType),
						new PXDataFieldRestrict("OrderNbr", PXDbType.NVarChar, tran.OrigRefNbr)
					);

					PopulateFields();
					paymentGraph.Save.Press();

					MoveCurrentActiveTransaction();

					ARPaymentAfterProcessingManager manager = new ARPaymentAfterProcessingManager() { ReleaseDoc = true };
					manager.RunCaptureActions(payment, true);

					MoveInactiveTransactions();

					LinkPaymentWithDoc();
					paymentGraph.Actions.PressSave();

					scope.Complete();
				}
			}

			private void PopulateFields()
			{
				int? pmInstance = tran.PMInstanceID;
				CustomerPaymentMethod cpm = GetCustPaymentMethodById(paymentGraph, pmInstance);
				ARPayment payment = new ARPayment();
				payment.DocType = ARPaymentType.Payment;
				AROpenPeriodAttribute.SetThrowErrorExternal<ARPayment.adjFinPeriodID>(paymentGraph.Document.Cache, true);
				payment = paymentGraph.Document.Insert(payment);
				AROpenPeriodAttribute.SetThrowErrorExternal<ARPayment.adjFinPeriodID>(paymentGraph.Document.Cache, false);
				payment.CustomerID = cpm.BAccountID;
				payment.CustomerLocationID = order.CustomerLocationID;
				payment.PaymentMethodID = cpm.PaymentMethodID;
				payment.CashAccountID = order.CashAccountID;
				payment.PMInstanceID = cpm.PMInstanceID;
				payment.CuryOrigDocAmt = tran.Amount;
				payment.DocDesc = order.DocDesc;
				paymentGraph.Document.Update(payment);
			}

			private void LinkPaymentWithDoc()
			{
				decimal? amt = payment.CuryOrigDocAmt > order.CuryDocBal ? order.CuryDocBal : payment.CuryOrigDocAmt;
				SOAdjust adj = new SOAdjust();
				adj.AdjdOrderType = tran.OrigDocType;
				adj.AdjdOrderNbr = tran.OrigRefNbr;
				adj.CuryAdjdAmt = amt;
				paymentGraph.SOAdjustments.Insert(adj);
			}

			private void MoveCurrentActiveTransaction()
			{
				MoveTransactions(new[] { tran });
			}

			private void MoveInactiveTransactions()
			{
				IEnumerable<ExternalTransaction> res = null;
				var query = new PXSelectReadonly<ExternalTransaction, Where<ExternalTransaction.origDocType, Equal<Required<ExternalTransaction.origDocType>>,
					And<ExternalTransaction.origRefNbr, Equal<Required<ExternalTransaction.origRefNbr>>,
					And<ExternalTransaction.refNbr, IsNull>>>>(orderGraph);
				res = query.Select(tran.OrigDocType, tran.OrigRefNbr).RowCast<ExternalTransaction>().Where(i => i.Active == false
					|| ExternalTranHelper.GetTransactionState(orderGraph, i).ProcessingStatus == ProcessingStatus.AuthorizeExpired);

				MoveTransactions(res);
			}

			private void MoveTransactions(IEnumerable<ExternalTransaction> trans)
			{
				foreach (ExternalTransaction item in trans)
				{
					PXDatabase.Update<ExternalTransaction>(
						new PXDataFieldAssign("DocType", payment.DocType),
						new PXDataFieldAssign("RefNbr", payment.RefNbr),
						new PXDataFieldRestrict("TransactionID", PXDbType.Int, 4, item.TransactionID, PXComp.EQ)
					);
					PXDatabase.Update<CCProcTran>(
						new PXDataFieldAssign("DocType", payment.DocType),
						new PXDataFieldAssign("RefNbr", payment.RefNbr),
						new PXDataFieldRestrict("TransactionID", PXDbType.Int, 4, item.TransactionID, PXComp.EQ)
					);
				}
			}

		}
	}
}