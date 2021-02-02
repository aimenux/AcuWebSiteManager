using PX.Common;
using PX.Data;
using PX.Data.BQL;
using PX.Objects.AR;
using PX.Objects.AR.CCPaymentProcessing;
using PX.Objects.AR.CCPaymentProcessing.Common;
using PX.Objects.CA;
using PX.Objects.CM;
using PX.Objects.CS;
using PX.Objects.Extensions.PaymentTransaction;
using PX.Objects.GL;
using PX.Objects.SO.DAC.Unbound;
using PX.Objects.SO.GraphExtensions;
using PX.Objects.SO.Interfaces;
using PX.Objects.TX;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PX.Objects.SO
{
	[TableAndChartDashboardType]
	public class SOPaymentProcess : PXGraph<SOPaymentProcess>
	{
		#region Members

		public PXCancel<SOPaymentProcessFilter> Cancel;
		public PXAction<SOPaymentProcessFilter> viewDocument;
		public PXAction<SOPaymentProcessFilter> viewRelatedDocument;

		public PXFilter<SOPaymentProcessFilter> Filter;

		[PXFilterable]
		public PXFilteredProcessing<SOPaymentProcessResult, SOPaymentProcessFilter> Payments;

		#endregion Members

		#region Initialize

		public SOPaymentProcess()
		{
			Views.Caches.Remove(typeof(SOPaymentProcessFilter));
			Views.Caches.Remove(typeof(SOPaymentProcessResult));
		}

		#endregion // Initialize

		#region Request Data

		public IEnumerable payments()
		{
			if (PXView.MaximumRows == 1 && PXView.Searches?.Length == 2 &&
				PXView.Searches.All(f => f is string s && !string.IsNullOrEmpty(s)))
			{
				var cached = Payments.Cache.Locate(new SOPaymentProcessResult()
				{
					DocType = (string)PXView.Searches[0],
					RefNbr = (string)PXView.Searches[1]
				});

				if (cached != null)
					return new object[] { cached };
			}

			PXSelectBase<ARPayment> query = GetView();
			AddFilters(query, Filter.Current);

			VerifyNonDBFields(out bool filtersContainNonDBField, out bool sortsContainNonDBField);

			var resultList = new List<SOPaymentProcessResult>();

			int startRow = filtersContainNonDBField || sortsContainNonDBField ? 0 : PXView.StartRow;
			int maximumRows = filtersContainNonDBField || sortsContainNonDBField ? -1 : PXView.MaximumRows;
			int totalRows = 0;

			using (new PXFieldScope(query.View, GetViewFieldList()))
			{
				foreach (PXResult<ARPayment> row in query.View.Select(PXView.Currents, GetParameters(Filter.Current), PXView.Searches, PXView.SortColumns, PXView.Descendings,
					PXView.Filters, ref startRow, maximumRows, ref totalRows))
				{
					SOPaymentProcessResult newRow = CreateSOPaymentProcessResult(row);
					resultList.Add(newRow);
				}
			}

			PXView.StartRow = 0;

			return CreateDelegateResult(resultList, totalRows, filtersContainNonDBField, sortsContainNonDBField);
		}

		protected virtual PXSelectBase<ARPayment> GetView()
		{
			return new PXSelectJoin<ARPayment,
						InnerJoin<PaymentMethod, On<ARPayment.paymentMethodID, Equal<PaymentMethod.paymentMethodID>>,
						InnerJoin<ARPaymentTotals, On<ARPayment.docType, Equal<ARPaymentTotals.docType>, And<ARPayment.refNbr, Equal<ARPaymentTotals.refNbr>>>,
						LeftJoin<SOOrder, On<ARPaymentTotals.adjdOrderType, Equal<SOOrder.orderType>, And<ARPaymentTotals.adjdOrderNbr, Equal<SOOrder.orderNbr>>>,
						LeftJoin<SOInvoice, On<ARPaymentTotals.adjdDocType, Equal<SOInvoice.docType>, And<ARPaymentTotals.adjdRefNbr, Equal<SOInvoice.refNbr>>>,
						LeftJoin<ARInvoice, On<ARPaymentTotals.adjdDocType, Equal<ARInvoice.docType>, And<ARPaymentTotals.adjdRefNbr, Equal<ARInvoice.refNbr>>>,
						LeftJoin<ExternalTransaction, On<ExternalTransaction.transactionID, Equal<ARPayment.cCActualExternalTransactionID>>,
						LeftJoin<SOAdjust, On<ARPayment.docType, Equal<SOAdjust.adjgDocType>, And<ARPayment.refNbr, Equal<SOAdjust.adjgRefNbr>,
							And<SOAdjust.adjdOrderType, Equal<ARPaymentTotals.adjdOrderType>, And<SOAdjust.adjdOrderNbr, Equal<ARPaymentTotals.adjdOrderNbr>>>>>,
						LeftJoin<ARAdjust, On<ARPayment.docType, Equal<ARAdjust.adjgDocType>, And<ARPayment.refNbr, Equal<ARAdjust.adjgRefNbr>,
							And<ARAdjust.adjdDocType, Equal<ARPaymentTotals.adjdDocType>, And<ARAdjust.adjdRefNbr, Equal<ARPaymentTotals.adjdRefNbr>>>>>>>>>>>>>,
						Where<ARPayment.isCCPayment, Equal<True>,
							And<ARPayment.pendingProcessing, Equal<True>,
							And<ARPayment.released, NotEqual<True>,
							And<ARPayment.voided, NotEqual<True>,
							And2<Where2< // Single Application
								Where<ARPaymentTotals.invoiceCntr, Equal<Zero>, And<ARPaymentTotals.adjdOrderNbr, IsNotNull>>, // Only links to unique order
								Or2<Where<ARPaymentTotals.orderCntr, Equal<Zero>, And<ARPaymentTotals.adjdRefNbr, IsNotNull>>, // Only links to unique invoice
								Or<Where<ARPaymentTotals.adjdOrderType, Equal<SOInvoice.sOOrderType>, // Invoice has only links to unique order, and it is the same order which payment refers to
									And<ARPaymentTotals.adjdOrderNbr, Equal<SOInvoice.sOOrderNbr>>>>>>,
							And<ARPayment.docType, In3<ARDocType.prepayment, ARDocType.payment>,
							And<ARPayment.hold, Equal<False>,
							And<PaymentMethod.isActive, Equal<True>,
							And<PaymentMethod.useForAR, Equal<True>,
							And2<Where2< // If Invoice: Invoice released <> true / If Sales Order: Order status <> completed
								Where<ARPaymentTotals.adjdRefNbr, IsNotNull, And<SOInvoice.released, NotEqual<True>>>,
								Or<Where<ARPaymentTotals.adjdRefNbr, IsNull, And<SOOrder.completed, NotEqual<True>>>>>,
							// Invoice BachModule = SO (or it is Order)
							And<Where<ARPaymentTotals.adjdRefNbr, IsNull, Or<SOInvoice.refNbr, IsNotNull>>>>>>>>>>>>>>(this);
		}

		protected virtual void AddFilters(PXSelectBase<ARPayment> view, SOPaymentProcessFilter filter)
		{
			if (filter.CustomerID != null)
				view.WhereAnd<Where<Current<SOPaymentProcessFilter.customerID>, Equal<ARPayment.customerID>>>();

			if (filter.StartDate != null)
				view.WhereAnd<Where<Current<SOPaymentProcessFilter.startDate>, LessEqual<ARPayment.docDate>>>();

			if (filter.EndDate != null)
				view.WhereAnd<Where<Current<SOPaymentProcessFilter.endDate>, GreaterEqual<ARPayment.docDate>>>();

			switch (filter.Action)
			{
				case SOPaymentProcessFilter.action.CaptureCCPayment:
					view.WhereAnd<Where<ARPayment.isCCAuthorized, Equal<True>,
						And<ARPayment.isCCCaptureFailed, NotEqual<True>>>>();
					break;

				case SOPaymentProcessFilter.action.ValidateCCPayment:
					break;

				case SOPaymentProcessFilter.action.VoidExpiredCCPayment:
					view.WhereAnd<Where<ARPayment.isCCAuthorized, Equal<True>,
						And<ExternalTransaction.fundHoldExpDate,
							Less<Required<ExternalTransaction.fundHoldExpDate>>,
						And<ARPayment.pMInstanceID, IsNotNull>>>>();

					view.Join<InnerJoin<CCProcessingCenter,
						On<ARPayment.processingCenterID, Equal<CCProcessingCenter.processingCenterID>,
						And<CCProcessingCenter.reauthRetryNbr, Greater<Zero>>>>>();
					break;

				case SOPaymentProcessFilter.action.ReAuthorizeCCPayment:
					view.WhereAnd<Where<ARPayment.isCCAuthorized, NotEqual<True>,
						And<ARPayment.isCCCaptured, NotEqual<True>,
						And<ARPayment.pMInstanceID, IsNotNull,
						And<ARPayment.cCReauthDate, Less<Required<ARPayment.cCReauthDate>>,
						And<ARPayment.cCReauthTriesLeft, Greater<Zero>>>>>>>();
					break;

				default:
					view.WhereAnd<Where<True, Equal<False>>>();
					break;
			}
		}

		protected virtual object[] GetParameters(SOPaymentProcessFilter filter)
		{
			var parameters = new List<object>();

			if (filter.Action.IsIn(
				SOPaymentProcessFilter.action.VoidExpiredCCPayment,
				SOPaymentProcessFilter.action.ReAuthorizeCCPayment))
			{
				parameters.Add(PXTimeZoneInfo.Now);
			}

			return parameters.ToArray();
		}


		protected virtual SOPaymentProcessResult CreateSOPaymentProcessResult(PXResult<ARPayment> row)
		{
			var payment = PropertyTransfer.Transfer(row.GetItem<ARPayment>(), new SOPaymentProcessResult());
			var transaction = row.GetItem<ExternalTransaction>();
			var order = row.GetItem<SOOrder>();
			var invoice = row.GetItem<ARInvoice>();
			var aradjust = row.GetItem<ARAdjust>();
			var soadjust = row.GetItem<SOAdjust>();

			bool isInvoice = invoice?.RefNbr != null;
			payment.RelatedDocument = isInvoice ? SOPaymentProcessResult.relatedDocument.ListAttribute.Invoice :
				SOPaymentProcessResult.relatedDocument.ListAttribute.SalesOrder;
			payment.RelatedDocumentNumber = isInvoice ? invoice.RefNbr : order.OrderNbr;
			payment.RelatedDocumentType = isInvoice ? invoice.DocType : order.OrderType;
			payment.RelatedDocumentStatus = isInvoice ? PXStringListAttribute.GetLocalizedLabel<ARInvoice.status>(this.Caches<ARInvoice>(), invoice)
				: PXStringListAttribute.GetLocalizedLabel<SOOrder.status>(this.Caches<SOOrder>(), order);
			payment.RelatedDocumentCreditTerms = isInvoice ? invoice.TermsID : order.TermsID;
			payment.RelatedDocumentCuryInfoID = isInvoice ? aradjust.AdjgCuryInfoID : soadjust.AdjgCuryInfoID;
			payment.CuryRelatedDocumentAppliedAmount = isInvoice ? aradjust.CuryAdjgAmt : soadjust.CuryAdjgAmt;
			payment.RelatedDocumentAppliedAmount = isInvoice ? aradjust.CuryAdjgAmt : soadjust.CuryAdjgAmt;

			payment.RelatedTranProcessingStatus = transaction?.ProcStatus;
			payment.FundHoldExpDate = transaction?.FundHoldExpDate;

			var cached = (SOPaymentProcessResult)Payments.Cache.Locate(payment);
			if (cached == null)
			{
				Payments.Cache.Hold(payment);
			}
			else
			{
				payment.Selected = cached.Selected;
				payment.ErrorDescription = cached.ErrorDescription;
			}

			return payment;
		}

		protected virtual void VerifyNonDBFields(out bool filtersContainNonDbField, out bool sortsContainNonDbField)
		{
			const string RelatedDocument = "RelatedDocument";

			filtersContainNonDbField = PXView.Filters != null && ((PXFilterRow[])PXView.Filters)
				.Any(f => f?.DataField?.Contains(RelatedDocument) == true ||
				f?.DataField?.Contains(nameof(SOPaymentProcessResult.Selected)) == true ||
				f?.DataField?.Contains(nameof(SOPaymentProcessResult.ErrorDescription)) == true ||
				f?.DataField?.Contains(nameof(SOPaymentProcessResult.RelatedTranProcessingStatus)) == true) == true;

			sortsContainNonDbField = PXView.SortColumns?.Any(c => c?.Contains(RelatedDocument) == true ||
				c?.Contains(nameof(SOPaymentProcessResult.Selected)) == true ||
				c?.Contains(nameof(SOPaymentProcessResult.ErrorDescription)) == true ||
				c?.Contains(nameof(SOPaymentProcessResult.RelatedTranProcessingStatus)) == true) == true;
		}

		protected virtual IEnumerable<Type> GetViewFieldList()
		{
			return new Type[]
			{
				typeof(ARPayment),
				typeof(ExternalTransaction.procStatus),
				typeof(ExternalTransaction.fundHoldExpDate),
				typeof(SOOrder.orderType),
				typeof(SOOrder.orderNbr),
				typeof(SOOrder.status),
				typeof(SOOrder.termsID),
				typeof(ARInvoice.docType),
				typeof(ARInvoice.refNbr),
				typeof(ARInvoice.status),
				typeof(ARInvoice.termsID),
				typeof(ARAdjust.adjgCuryInfoID),
				typeof(ARAdjust.curyAdjgAmt),
				typeof(ARAdjust.curyAdjgAmt),
				typeof(SOAdjust.adjgCuryInfoID),
				typeof(SOAdjust.curyAdjgAmt),
				typeof(SOAdjust.curyAdjgAmt)
			};
		}

		protected virtual PXDelegateResult CreateDelegateResult(List<SOPaymentProcessResult> resultList, 
			int totalRows, bool filtersContainNonDBField, bool sortsContainNonDBField)
		{
			PXDelegateResult delegateResult = new PXDelegateResult();
			delegateResult.IsResultFiltered = !filtersContainNonDBField;
			delegateResult.IsResultTruncated = totalRows > resultList.Count;

			if (!sortsContainNonDBField)
			{
				delegateResult.IsResultSorted = true;

				if (!PXView.ReverseOrder)
				{
					delegateResult.AddRange(resultList);
				}
				else
				{
					var sortedList = PXView.Sort(resultList);
					delegateResult.AddRange(sortedList.Cast<SOPaymentProcessResult>());
				}
			}
			else
			{
				delegateResult.AddRange(resultList);
			}

			return delegateResult;
		}

		#endregion // Request Data
		
		#region Buttons

		[PXUIField(DisplayName = "", MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select)]
		[PXEditDetailButton]
		public virtual IEnumerable ViewDocument(PXAdapter adapter)
		{
			SOPaymentProcessResult line = Payments.Current;
			if (line == null) return adapter.Get();

			var graph = PXGraph.CreateInstance<ARPaymentEntry>();
			graph.Document.Current = graph.Document.Search<ARPayment.refNbr>(line.RefNbr, line.DocType);
			throw new PXRedirectRequiredException(graph, true, "View Document") { Mode = PXBaseRedirectException.WindowMode.NewWindow };
		}

		[PXUIField(DisplayName = "View Document", Visible = false, MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select)]
		[PXButton]
		public virtual IEnumerable ViewRelatedDocument(PXAdapter adapter)
		{
			SOPaymentProcessResult line = Payments.Current;
			if (line == null) return adapter.Get();

			PXGraph graph;

			if (line.RelatedDocument == SOPaymentProcessResult.relatedDocument.ListAttribute.Invoice)
			{
				var invoiceEntry = PXGraph.CreateInstance<SOInvoiceEntry>();
				invoiceEntry.Document.Current = invoiceEntry.Document.Search<SOInvoice.refNbr>(line.RelatedDocumentNumber, line.RelatedDocumentType);
				graph = invoiceEntry;
			}
			else
			{
				var orderEntry = PXGraph.CreateInstance<SOOrderEntry>();
				orderEntry.Document.Current = orderEntry.Document.Search<SOOrder.orderNbr>(line.RelatedDocumentNumber, line.RelatedDocumentType);
				graph = orderEntry;
			}

			throw new PXRedirectRequiredException(graph, true, "View Document") { Mode = PXBaseRedirectException.WindowMode.NewWindow };
		}

		#endregion // Buttons

		#region Events

		protected virtual void _(Events.FieldUpdated<SOPaymentProcessFilter.action> eventArgs)
		{
			Payments.Cache.Clear();
		}

		protected virtual void _(Events.RowSelected<SOPaymentProcessFilter> eventArgs)
		{
			string action = Filter.Current.Action;
			Payments.SetProcessDelegate(payments =>
				CreateInstance<SOPaymentProcess>().ProcessPayments(payments, action));
		}

		protected virtual void _(Events.RowSelected<SOPaymentProcessResult> eventArgs)
		{
			if (Filter.Current == null)
				return;

			PXUIFieldAttribute.SetVisible<SOPaymentProcessResult.fundHoldExpDate>(eventArgs.Cache, null,
				Filter.Current.Action == SOPaymentProcessFilter.action.VoidExpiredCCPayment);
		}

		#endregion // Events

		#region Process

		protected virtual void ProcessPayments(List<SOPaymentProcessResult> payments, string action)
		{
			SOOrderEntry orderEntry = null;
			SOInvoiceEntry invoiceEntry = null;
			ARPaymentEntry paymentEntry = PXGraph.CreateInstance<ARPaymentEntry>();
			CreatePaymentExtBase<SOOrderEntry, SOOrder, SOAdjust> orderPaymentExt = null;
			CreatePaymentExtBase<SOInvoiceEntry, ARInvoiceEntry, ARInvoice, ARAdjust2, ARAdjust> invoicePaymentExt = null;
			ARPaymentEntry.PaymentTransaction paymentTransactionExt = paymentEntry.GetExtension<ARPaymentEntry.PaymentTransaction>();
			int paymentIndex = 0;

			foreach (var payment in payments)
			{
				try
				{
					if (payment.RelatedDocument == SOPaymentProcessResult.relatedDocument.ListAttribute.Invoice)
					{
						if (invoiceEntry == null)
						{
							invoiceEntry = PXGraph.CreateInstance<SOInvoiceEntry>();
							invoicePaymentExt = invoiceEntry.FindImplementation<
								CreatePaymentExtBase<SOInvoiceEntry, ARInvoiceEntry, ARInvoice, ARAdjust2, ARAdjust>>();
						}

						ProcessInvoice(action, invoiceEntry, invoicePaymentExt, paymentEntry, paymentTransactionExt, payment);
					}
					else
					{
						if (orderEntry == null)
						{
							orderEntry = PXGraph.CreateInstance<SOOrderEntry>();
							orderPaymentExt = orderEntry.FindImplementation<CreatePaymentExtBase<SOOrderEntry, SOOrder, SOAdjust>>();
						}

						ProcessOrder(action, orderEntry, orderPaymentExt, paymentEntry, paymentTransactionExt, payment);
					}
				}
				catch (Exception exception)
				{
					PXProcessing<SOPaymentProcessResult>.SetError(paymentIndex, exception);
				}
				finally
				{
					payment.ErrorDescription = GetErrorDescription(paymentEntry, paymentTransactionExt, payment);

					paymentIndex++;
				}
			}
		}

		protected virtual void ProcessInvoice(string action, SOInvoiceEntry entry,
			CreatePaymentExtBase<SOInvoiceEntry, ARInvoiceEntry, ARInvoice, ARAdjust2, ARAdjust> paymentExt,
			ARPaymentEntry paymentEntry, ARPaymentEntry.PaymentTransaction paymentTransactionExt, SOPaymentProcessResult payment)
		{
			entry.Clear();
			entry.Document.Current = entry.Document.Search<SOInvoice.refNbr>(payment.RelatedDocumentNumber, payment.RelatedDocumentType);

			paymentEntry.Clear();
			paymentEntry.Document.Current = paymentEntry.Document.Search<ARPayment.refNbr>(payment.RefNbr, payment.DocType);

			var adjustments = paymentEntry.Adjustments.SelectMain().Where(a => a.AdjAmt > 0m);
			var adjustment = adjustments.FirstOrDefault();
			if (adjustments.Count() != 1 ||
				adjustment.AdjdRefNbr != payment.RelatedDocumentNumber ||
				adjustment.AdjdDocType != payment.RelatedDocumentType)
			{
				throw new Common.Exceptions.RowNotFoundException(entry.Caches<ARPayment>(), payment.DocType, payment.RefNbr);
			}

			var adjustment2 = PropertyTransfer.Transfer(adjustment, new ARAdjust2());

			RunAction(action, paymentEntry, paymentTransactionExt, adjustment2, paymentExt);

		}

		protected virtual void ProcessOrder(string action, SOOrderEntry entry, CreatePaymentExtBase<SOOrderEntry, SOOrder, SOAdjust> paymentExt,
			ARPaymentEntry paymentEntry, ARPaymentEntry.PaymentTransaction paymentTransactionExt, SOPaymentProcessResult payment)
		{
			entry.Clear();
			entry.Document.Current = entry.Document.Search<SOOrder.orderNbr>(payment.RelatedDocumentNumber, payment.RelatedDocumentType);

			paymentEntry.Clear();
			paymentEntry.Document.Current = paymentEntry.Document.Search<ARPayment.refNbr>(payment.RefNbr, payment.DocType);

			var adjustments = paymentEntry.SOAdjustments.SelectMain().Where(a => a.AdjAmt > 0m);
			var adjustment = adjustments.FirstOrDefault();
			if (adjustments.Count() != 1 ||
				adjustment.AdjdOrderNbr != payment.RelatedDocumentNumber ||
				adjustment.AdjdOrderType != payment.RelatedDocumentType)
			{
				throw new Common.Exceptions.RowNotFoundException(entry.Caches<ARPayment>(), payment.DocType, payment.RefNbr);
			}

			RunAction(action, paymentEntry, paymentTransactionExt, adjustment, paymentExt);
		}

		protected virtual void RunAction<TGraph, TFirstGraph, TDocument, TDocumentAdjust, TPaymentAdjust>(string action,
			ARPaymentEntry paymentEntry, ARPaymentEntry.PaymentTransaction paymentTransactionExt, TDocumentAdjust adjustment,
			CreatePaymentExtBase<TGraph, TFirstGraph, TDocument, TDocumentAdjust, TPaymentAdjust> paymentExt)
			where TGraph : PXGraph<TFirstGraph, TDocument>, new()
			where TFirstGraph : PXGraph
			where TDocument : class, IBqlTable, ICreatePaymentDocument, new()
			where TDocumentAdjust : class, IBqlTable, ICreatePaymentAdjust, new()
			where TPaymentAdjust : class, IBqlTable, new()
		{
			switch (action)
			{
				case SOPaymentProcessFilter.action.CaptureCCPayment:
					paymentExt.CapturePayment(adjustment, paymentEntry, paymentTransactionExt);
					break;
				case SOPaymentProcessFilter.action.VoidExpiredCCPayment:
					paymentExt.VoidCCTransactionForReAuthorization(adjustment, paymentEntry, paymentTransactionExt);
					break;
				case SOPaymentProcessFilter.action.ValidateCCPayment:
					paymentExt.ValidatePayment(adjustment, paymentEntry, paymentTransactionExt);
					break;
				case SOPaymentProcessFilter.action.ReAuthorizeCCPayment:
					paymentExt.AuthorizePayment(adjustment, paymentEntry, paymentTransactionExt);
					break;
				default:
					throw new NotImplementedException();
			}
		}

		protected virtual string GetErrorDescription(ARPaymentEntry paymentEntry, ARPaymentEntry.PaymentTransaction paymentTransactionExt, SOPaymentProcessResult payment)
		{
			try
			{
				ExternalTransactionState paymentState = paymentTransactionExt.GetActiveTransactionState();
				int? tranId = paymentState?.ExternalTransaction?.TransactionID;
				if (paymentState?.HasErrors == true && tranId != null)
				{
					string errorText = new PXSelect<CCProcTran,
						Where<CCProcTran.transactionID, Equal<Required<CCProcTran.transactionID>>>,
						OrderBy<Desc<CCProcTran.tranNbr>>>(paymentEntry)
						.SelectSingle(tranId)?.ErrorText;

					return errorText;
				}

				return null;
			}
			catch (Exception exception)
			{
				PXTrace.WriteError(exception);
				return exception.Message;
			}
		}

		#endregion // Process
	}
}
