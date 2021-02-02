namespace PX.Objects.AR.CCPaymentProcessing.Common
{
	/// <summary>A supplementary class to return the result of the authorization center transaction to Acumatica ERP.</summary>
	public class TranProcessingResult
	{
		/// <summary>The internal transaction identifier, which must be the same as the <see cref="ProcessingInput.TranID" /> passed to the payment gateway.</summary>
		public int TranID;
		/// <summary>The transaction status.</summary>
		public CCTranStatus TranStatus = CCTranStatus.Error;
		/// <summary>A field that indicates (if set to <tt>true</tt>) that the transaction was authorized.</summary>
		public bool Success;
		/// <summary>The transaction number that was assigned by the authorization center.</summary>
		public string PCTranNumber;
		/// <summary>The raw authorization center response code.</summary>
		public string PCResponseCode;
		/// <summary>The raw response reason code.</summary>
		public string PCResponseReasonCode;
		/// <summary>The complete raw response from the authorization center.</summary>
		public string PCResponse;
		/// <summary>The additional CVV code from the authorization center (part of the complete response).</summary>
		public string PCCVVResponse;
		/// <summary>The authorization number.</summary>
		public string AuthorizationNbr;
		/// <summary>The response reason message. This text will be displayed in the credit card payment processing interface.</summary>
		public string PCResponseReasonText;
		/// <summary>The error message.</summary>
		public string ErrorText;
		/// <summary>The period (in days) after which the transaction automatically expires (for authorization transactions).</summary>
		public int? ExpireAfterDays;
		/// <summary>The result flag.</summary>
		public CCResultFlag ResultFlag;
		/// <summary>The CVV verification status.</summary>
		public CcvVerificationStatus CcvVerificatonStatus;
		/// <summary>The error source.</summary>
		public CCError.CCErrorSource ErrorSource;
	}
}
