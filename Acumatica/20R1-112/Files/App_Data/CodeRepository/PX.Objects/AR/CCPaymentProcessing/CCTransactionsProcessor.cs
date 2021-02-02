using System;
using PX.Data;
using PX.Objects.AR.CCPaymentProcessing.Common;
using PX.Objects.AR.CCPaymentProcessing.Interfaces;

namespace PX.Objects.AR.CCPaymentProcessing
{
	public class CCTransactionsProcessor : ICCTransactionsProcessor
	{
		private ICCPaymentProcessor _processingClass;

		protected CCTransactionsProcessor(ICCPaymentProcessor processingClass)
		{
			_processingClass = processingClass;
		}

		public static ICCTransactionsProcessor GetCCTransactionsProcessor()
		{
			return new CCTransactionsProcessor(CCPaymentProcessing.GetCCPaymentProcessing());
		}

		public void ProcessAuthorize(ICCPayment doc, IExternalTransaction tran)
		{
			CheckInput(doc, tran);
			Process(() => { return _processingClass.Authorize(doc, false); });
		}

		public void ProcessAuthorizeCapture(ICCPayment doc, IExternalTransaction tran)
		{
			CheckInput(doc, tran);
			Process(() => { return _processingClass.Authorize(doc, true); });
		}

		public void ProcessPriorAuthorizedCapture(ICCPayment doc, IExternalTransaction tran)
		{
			CheckInput(doc, tran);
			Process(() => { return _processingClass.Capture(doc, tran.TransactionID); });
		}

		public void ProcessVoid(ICCPayment doc, IExternalTransaction tran)
		{
			CheckInput(doc, tran);
			Process(() => { return _processingClass.Void(doc.PMInstanceID, tran.TransactionID); });
		}

		public void ProcessVoidOrCredit(ICCPayment doc, IExternalTransaction tran)
		{
			CheckInput(doc, tran);
			Process(() => { return _processingClass.VoidOrCredit(doc.PMInstanceID, tran.TransactionID); });
		}

		public void ProcessCredit(ICCPayment doc, IExternalTransaction tran)
		{
			CheckInput(doc, tran);
			Process(() => {
				TranOperationResult opRes = null;
				if (tran.TransactionID.HasValue)
				{
					opRes = _processingClass.Credit(doc, tran.TransactionID.Value);
				}
				else
				{
					opRes = _processingClass.Credit(doc, tran.TranNumber);
				}
				return opRes;
			});
		}

		public void ProcessCaptureOnly(ICCPayment doc, IExternalTransaction tran)
		{
			CheckInput(doc, tran);
			Process(() => { return _processingClass.CaptureOnly(doc, tran.AuthNumber); });
		}

		private void CheckInput(ICCPayment doc, IExternalTransaction tran)
		{
			if (doc == null)
			{
				throw new ArgumentNullException(nameof(doc));
			}
		}

		private void Process(Func<TranOperationResult> func)
		{
			TranOperationResult res = func();
			if (!res.Success)
			{
				throw new PXException(Messages.ERR_CCTransactionProcessingFailed, res.TransactionId);
			}
		}
	}
}
