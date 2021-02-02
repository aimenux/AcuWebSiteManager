namespace PX.Objects.AR.CCPaymentProcessing.Common
{
	public interface ICCPaymentProcessor
	{
		TranOperationResult Authorize(ICCPayment payment, bool aCapture);
		TranOperationResult Capture(ICCPayment payment, int? transactionId);
		TranOperationResult CaptureOnly(ICCPayment payment, string aAuthorizationNbr);
		TranOperationResult Credit(ICCPayment payment, string aExtRefTranNbr, string procCetnerId);
		TranOperationResult Credit(ICCPayment payment, int? transactionId);
		TranOperationResult Void(ICCPayment payment, int? transactionId);
		TranOperationResult VoidOrCredit(ICCPayment payment, int? transactionId);
	}
}