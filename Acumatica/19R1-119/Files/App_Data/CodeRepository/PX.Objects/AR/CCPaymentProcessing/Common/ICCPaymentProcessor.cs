namespace PX.Objects.AR.CCPaymentProcessing.Common
{
	public interface ICCPaymentProcessor
	{
		bool Authorize(ICCPayment aPmtInfo, bool aCapture, ref int aTranNbr);
		bool Capture(int? aPMInstanceID, int? aAuthTranNbr, string aCuryID, decimal? aAmount, ref int aTranNbr);
		bool CaptureOnly(ICCPayment aPmtInfo, string aAuthorizationNbr, ref int aTranNbr);
		bool Credit(ICCPayment aPmtInfo, string aExtRefTranNbr, ref int aTranNbr);
		bool Credit(ICCPayment aPmtInfo, int aRefTranNbr, ref int aTranNbr);
		bool Void(int? aPMInstanceID, int? aRefTranNbr, ref int aTranNbr);
		bool VoidOrCredit(int? aPMInstanceID, int? aRefTranNbr, ref int aTranNbr);
	}
}