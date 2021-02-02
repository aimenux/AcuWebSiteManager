using PX.CCProcessingBase;
using PX.Objects.AR.CCPaymentProcessing.Interfaces;
namespace PX.Objects.AR.CCPaymentProcessing.Common
{
	public interface ICCTransactionsProcessor
	{
		void ProcessAuthorize(ICCPayment doc, IExternalTransaction tran);
		void ProcessAuthorizeCapture(ICCPayment doc, IExternalTransaction tran);
		void ProcessPriorAuthorizedCapture(ICCPayment doc, IExternalTransaction tran);
		void ProcessVoid(ICCPayment doc, IExternalTransaction tran);
		void ProcessVoidOrCredit(ICCPayment doc, IExternalTransaction tran);
		void ProcessCredit(ICCPayment doc, IExternalTransaction tran);
		void ProcessCaptureOnly(ICCPayment doc, IExternalTransaction tran);
	}
}