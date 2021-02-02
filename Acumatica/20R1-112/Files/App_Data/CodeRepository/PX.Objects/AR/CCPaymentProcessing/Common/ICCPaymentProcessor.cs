namespace PX.Objects.AR.CCPaymentProcessing.Common
{
	public interface ICCPaymentProcessor
	{
		TranOperationResult Authorize(ICCPayment payment, bool aCapture);
		TranOperationResult Capture(ICCPayment payment, int? transactionId);
		TranOperationResult CaptureOnly(ICCPayment payment, string aAuthorizationNbr);
		TranOperationResult Credit(ICCPayment payment, string aExtRefTranNbr);
		TranOperationResult Credit(ICCPayment payment, int? transactionId);
		TranOperationResult Void(int? pMInstanceID, int? transactionId);
		TranOperationResult VoidOrCredit(int? pMInstanceID, int? transactionId);
	}
}