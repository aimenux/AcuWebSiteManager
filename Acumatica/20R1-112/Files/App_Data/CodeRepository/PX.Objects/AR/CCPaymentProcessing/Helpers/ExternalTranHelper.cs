using PX.Common;
using PX.Data;
using PX.Objects.AR.CCPaymentProcessing.Common;
using PX.Objects.AR.CCPaymentProcessing.Interfaces;
using PX.Objects.AR.Repositories;
using PX.Objects.Common;
using PX.Objects.SO;
using System;
using System.Collections.Generic;
using System.Linq;
namespace PX.Objects.AR.CCPaymentProcessing.Helpers
{
	public static class ExternalTranHelper
	{
		public static bool HasTransactions(PXSelectBase<ExternalTransaction> extTrans)
		{
			return extTrans.Any();
		}

		public static IExternalTransaction GetActiveTransaction(PXSelectBase<ExternalTransaction> extTrans)
		{
			return GetActiveTransaction(extTrans.Select().RowCast<ExternalTransaction>());
		}

		public static IExternalTransaction GetActiveTransaction(IEnumerable<IExternalTransaction> extTrans)
		{
			extTrans = extTrans.OrderByDescending(i => i.TransactionID);
			IExternalTransaction extTran = extTrans.Where(i => i.Active == true).FirstOrDefault();
			return extTran;
		}

		public static bool HasSuccessfulTrans(PXSelectBase<ExternalTransaction> extTrans)
		{
			IExternalTransaction extTran = GetActiveTransaction(extTrans);
			if (extTran != null && !IsExpired(extTran))
			{
				return true;
			}
			return false;
		}

		public static bool HasVoidPreAuthorizedInHistory(PXGraph graph, IExternalTransaction extTran)
		{
			if (graph == null)
			{
				throw new ArgumentNullException(nameof(graph));
			}
			if (extTran == null)
			{
				throw new ArgumentNullException(nameof(extTran));
			}
			CCProcTranRepository repo = new CCProcTranRepository(graph);
			var history = repo.GetCCProcTranByTranID(extTran.TransactionID);
			return CCProcTranHelper.HasVoidPreAuthorized(history);
		}

		public static bool HasTransactions(PXGraph graph, int? pmInstanceId)
		{
			ExternalTransaction tran = PXSelect<ExternalTransaction, 
				Where<ExternalTransaction.pMInstanceID, Equal<Required<ExternalTransaction.pMInstanceID>>>>
				.SelectWindowed(graph, 0, 1, pmInstanceId);
			return tran != null;
		}

		public static bool IsExpired(IExternalTransaction extTran)
		{
			return (extTran.ExpirationDate.HasValue && extTran.ExpirationDate.Value < PXTimeZoneInfo.Now) 
				|| extTran.ProcessingStatus == ExtTransactionProcStatusCode.AuthorizeExpired ;
		}

		public static ExternalTransactionState GetActiveTransactionState(PXGraph graph, PXSelectBase<ExternalTransaction> extTran)
		{
			var trans = extTran.Select().RowCast<ExternalTransaction>();
			return GetActiveTransactionState(graph, trans);
		}

		public static ExternalTransactionState GetTransactionState(PXGraph graph, IExternalTransaction extTran)
		{
			if (graph == null)
			{
				throw new ArgumentNullException(nameof(graph));
			}
			if (extTran == null)
			{
				throw new ArgumentNullException(nameof(extTran));
			}
			CCProcTranRepository repo = new CCProcTranRepository(graph);
			ExternalTransactionState state = new ExternalTransactionState(extTran);
			CheckAuthExpired(state);
			if (state.HasErrors)
			{
				ApplyLastSuccessfulTran(repo, state);
			}
			FormatDescription(repo, state);
			return state;
		}

		public static bool HasOpenCCProcTran(PXGraph graph, IExternalTransaction extTran)
		{
			if (graph == null)
			{
				throw new ArgumentNullException(nameof(graph));
			}
			if (extTran == null)
				return false;
			CCProcTranRepository repo = new CCProcTranRepository(graph);
			var records = repo.GetCCProcTranByTranID(extTran.TransactionID);
			return CCProcTranHelper.HasOpenCCTran(records);
		}

		public static ExternalTransactionState GetActiveTransactionState(PXGraph graph, IEnumerable<IExternalTransaction> extTrans)
		{
			if (graph == null)
			{
				throw new ArgumentNullException(nameof(graph));
			}
			if (extTrans == null)
			{
				throw new ArgumentNullException(nameof(extTrans));
			}
			ExternalTransactionState state = new ExternalTransactionState();
			CCProcTranRepository repo = new CCProcTranRepository(graph);
			var extTran = GetActiveTransaction(extTrans);
			if (extTran != null)
			{
				state = GetTransactionState(graph, extTran);
			}
			return state;
		}

		private static void ApplyLastSuccessfulTran(CCProcTranRepository repo, ExternalTransactionState state)
		{
			ICCPaymentTransaction paymentTran = LastSuccessfulCCProcTranTran(state.ExternalTransaction.TransactionID, repo);
			if (paymentTran != null)
			{
				switch (paymentTran.TranType)
				{
					case CCTranTypeCode.Authorize: state.IsPreAuthorized = true; break;
					case CCTranTypeCode.AuthorizeAndCapture:
					case CCTranTypeCode.PriorAuthorizedCapture:
					case CCTranTypeCode.CaptureOnly: state.IsCaptured = true; break;
					case CCTranTypeCode.Credit: state.IsRefunded = true; break;
				}
				state.IsOpenForReview = paymentTran.TranStatus == CCTranStatusCode.HeldForReview;
				CheckAuthExpired(state);
			}
		}

		private static ICCPaymentTransaction LastSuccessfulCCProcTranTran(int? extTranId, CCProcTranRepository repo)
		{
			var procTrans = repo.GetCCProcTranByTranID(extTranId).Cast<ICCPaymentTransaction>();
			ICCPaymentTransaction paymentTran = CCProcTranHelper.FindCCLastSuccessfulTran(procTrans);
			return paymentTran;
		}

		private static void CheckAuthExpired(ExternalTransactionState state)
		{
			if (state.IsPreAuthorized && IsExpired(state.ExternalTransaction))
			{
				state.IsPreAuthorized = false;
				state.ProcessingStatus = ProcessingStatus.AuthorizeExpired;
				state.HasErrors = false;
			}
		}

		public static void FormatDescription(CCProcTranRepository repo, ExternalTransactionState extTranState)
		{
			string descr = null;
			string currStatus = null;
			string prevStatus = null;
			IExternalTransaction extTran = extTranState.ExternalTransaction;
			if (extTran == null)
			{
				return;
			}
			ExtTransactionProcStatusCode.ListAttribute attr = new ExtTransactionProcStatusCode.ListAttribute();
			string procStatusStr = ExtTransactionProcStatusCode.GetProcStatusStrByProcessingStatus(extTranState.ProcessingStatus);
			if (!string.IsNullOrEmpty(procStatusStr))
			{
				currStatus = PXMessages.LocalizeNoPrefix(attr.ValueLabelDic[procStatusStr]);
			}
			bool needPrevStatus = extTranState.HasErrors;
			if (!string.IsNullOrEmpty(currStatus) && needPrevStatus)
			{
				ICCPaymentTransaction procTran = LastSuccessfulCCProcTranTran(extTranState.ExternalTransaction.TransactionID, repo);
				if (procTran != null)
				{
					prevStatus = GetStatusByTranType(procTran.TranType);
				}
			}
			if (!string.IsNullOrEmpty(currStatus) && !string.IsNullOrEmpty(prevStatus))
			{
				descr = prevStatus + ", " + currStatus;
			}
			else if (!string.IsNullOrEmpty(currStatus))
			{
				descr = currStatus;
			}
			else
			{
				descr = string.Empty;
			}
			extTranState.Description = descr;
		}

		public static bool UpdateCapturedState<T>(T doc, ExternalTransactionState tranState)
		where T : class, IBqlTable, ICCCapturePayment
		{
			bool needUpdate = false;
			IExternalTransaction extTran = tranState.ExternalTransaction;
			if (doc.IsCCCaptured != tranState.IsCaptured)
			{
				doc.IsCCCaptured = tranState.IsCaptured;
				needUpdate = true;
			}

			if (tranState.IsCaptured)
			{
				doc.CuryCCCapturedAmt = extTran.Amount;
				doc.IsCCCaptureFailed = false;
				needUpdate = true;
			}

			if (tranState.ProcessingStatus == ProcessingStatus.CaptureFail)
			{
				doc.IsCCCaptureFailed = true;
				needUpdate = true;
			}

			if (doc.IsCCCaptured == false && (doc.CuryCCCapturedAmt != decimal.Zero))
			{
				doc.CuryCCCapturedAmt = decimal.Zero;
				needUpdate = true;
			}

			return needUpdate;
		}

		public static bool UpdateCCPaymentState<T>(T doc, ExternalTransactionState tranState)
			where T : class, ICCAuthorizePayment, ICCCapturePayment
		{
			IExternalTransaction externalTran = tranState.ExternalTransaction;
			bool needUpdate = false;

			if (doc.IsCCAuthorized != tranState.IsPreAuthorized || doc.IsCCCaptured != tranState.IsCaptured)
			{
				if (!(tranState.ProcessingStatus == ProcessingStatus.VoidFail || tranState.ProcessingStatus == ProcessingStatus.CreditFail))
				{
					doc.IsCCAuthorized = tranState.IsPreAuthorized;
					doc.IsCCCaptured = tranState.IsCaptured;
					needUpdate = true;
				}
				else
				{
					doc.IsCCAuthorized = false;
					doc.IsCCCaptured = false;
					needUpdate = false;
				}
			}

			if (externalTran != null && tranState.IsPreAuthorized)
			{
				doc.CCAuthExpirationDate = externalTran.ExpirationDate;
				doc.CuryCCPreAuthAmount = externalTran.Amount;
				needUpdate = true;
			}

			if (doc.IsCCAuthorized == false && (doc.CCAuthExpirationDate != null || doc.CuryCCPreAuthAmount > Decimal.Zero))
			{
				doc.CCAuthExpirationDate = null;
				doc.CuryCCPreAuthAmount = Decimal.Zero;

				needUpdate = true;
			}

			if (tranState.IsCaptured)
			{
				doc.CuryCCCapturedAmt = externalTran.Amount;
				doc.IsCCCaptureFailed = false;
				needUpdate = true;
			}
		
			if(tranState.ProcessingStatus == ProcessingStatus.CaptureFail)
			{
				doc.IsCCCaptureFailed = true;
				needUpdate = true;
			}

			if (doc.IsCCCaptured == false && (doc.CuryCCCapturedAmt != decimal.Zero))
			{
				doc.CuryCCCapturedAmt = decimal.Zero;
				needUpdate = true;
			}
			return needUpdate;
		}

		public static IEnumerable<ExternalTransaction> GetSOInvoiceExternalTrans(PXGraph graph, ARInvoice currentInvoice)
		{
			foreach (ExternalTransaction tran in PXSelectReadonly<ExternalTransaction,
				Where<ExternalTransaction.refNbr, Equal<Current<ARInvoice.refNbr>>,
					And<ExternalTransaction.docType, Equal<Current<ARInvoice.docType>>>>,
				OrderBy<Desc<ExternalTransaction.transactionID>>>.SelectMultiBound(graph, new object[] { currentInvoice }))
			{
				yield return tran;
			}

			foreach (ExternalTransaction tran in PXSelectReadonly2<ExternalTransaction,
					InnerJoin<SOOrderShipment, On<SOOrderShipment.orderNbr, Equal<ExternalTransaction.origRefNbr>,
						And<SOOrderShipment.orderType, Equal<ExternalTransaction.origDocType>>>>,
					Where<SOOrderShipment.invoiceNbr, Equal<Current<ARInvoice.refNbr>>,
						And<SOOrderShipment.invoiceType, Equal<Current<ARInvoice.docType>>,
						And<ExternalTransaction.refNbr, IsNull>>>,
					OrderBy<Desc<CCProcTran.tranNbr>>>.SelectMultiBound(graph, new object[] { currentInvoice }))
			{
				yield return tran;
			}
		}

		private static string GetStatusByTranType(string tranType)
		{
			string ret = null;
			switch (tranType)
			{
				case CCTranTypeCode.Authorize: ret = PXMessages.LocalizeNoPrefix(Messages.CCPreAuthorized); break;
				case CCTranTypeCode.PriorAuthorizedCapture:
				case CCTranTypeCode.AuthorizeAndCapture:
				case CCTranTypeCode.CaptureOnly: ret = PXMessages.LocalizeNoPrefix(Messages.CCCaptured); break;
				case CCTranTypeCode.VoidTran: ret = PXMessages.LocalizeNoPrefix(Messages.CCVoided); break;
				case CCTranTypeCode.Credit: ret = PXMessages.LocalizeNoPrefix(Messages.CCRefunded); break;
			}
			return ret;
		}

		public static bool IsOrderSelfCaptured(PXGraph graph, SOOrder doc)
		{
			return PXSelectReadonly<ExternalTransaction,
				Where<ExternalTransaction.origDocType, Equal<Required<SOOrder.orderType>>, And<ExternalTransaction.origRefNbr, Equal<Required<SOOrder.orderNbr>>,
				And<ExternalTransaction.origDocType, NotEqual<ExternalTransaction.docType>, And<ExternalTransaction.origRefNbr, NotEqual<ExternalTransaction.refNbr>>>>>>
					.SelectWindowed(graph, 0, 1, doc.OrderType, doc.OrderNbr).Count == 0;
		}
	}
}
