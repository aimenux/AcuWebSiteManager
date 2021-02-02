using System;

namespace PX.Objects.AR.CCPaymentProcessing.Common
{
	/// <summary>Defines the credit card transaction types.</summary>
	public enum CCTranType
	{
		/// <summary>Checks if the requested amount might be taken from the credit card, locks it on the credit card account, and takes the authorized amount from the card simultaneously.</summary>
		AuthorizeAndCapture,
		/// <summary>Checks if the requested amount might be taken from the credit card and locks it on the credit card account. </summary>
		AuthorizeOnly,
		/// <summary>Captures the previously authorized transaction.</summary>
		PriorAuthorizedCapture,
		/// <summary>Captures the manually authorized transaction.</summary>
		CaptureOnly,
		/// <summary>Returns the money back to the card.</summary>
		Credit,
		/// <summary>Reverses the authorized or captured transaction.</summary>
		Void,
		/// <summary>Performs a void first and then performs a credit if the void failed.</summary>
		VoidOrCredit
	}

	/// <summary>Contains the transaction statuses returned by the processing center.</summary>
	public enum CCTranStatus
	{
		/// <summary>The transaction was approved.</summary>
		Approved,
		/// <summary>The transaction was declined.</summary>
		Declined,
		/// <summary>
		/// An error occurred when the transaction is processed.
		/// </summary>
		Error,
		/// <summary>
		/// The transaction is under review.
		/// </summary>
		HeldForReview,
		/// <summary>The transaction was expired.</summary>
		Expired,
		/// <summary>
		/// There was no answer or the answer can't be interpreted.
		/// </summary>
		Unknown
	}

	/// <summary>Defines the CVV verification statuses returned by the credit card authority.</summary>
	public enum CcvVerificationStatus
	{
		/// <summary>The CVV code is correct.</summary>
		Match,
		/// <summary>The CVV code is incorrect.</summary>
		NotMatch,
		/// <summary>The CVV code is not processed.</summary>
		NotProcessed,
		/// <summary>The CVV code was not provided but is required for the authorization.</summary>
		ShouldHaveBeenPresent,
		/// <summary>The card issue authority was unable to verify the code.</summary>
		IssuerUnableToProcessRequest,
		/// <summary>The CVV code has already been verified by the Acumatica ERP core.</summary>
		RelyOnPreviousVerification,
		/// <summary>Any other status is returned.</summary>
		Unknown
	}

	[Flags]
	public enum CCResultFlag
	{
		None,
		OrigTransactionExpired,
		OrigTransactionNotFound,
	}

	[Flags]
	public enum CCPaymentState
	{
		None = 0,
		PreAuthorized = 1,
		PreAuthorizationFailed = 2,
		Captured = 4,
		CaptureFailed = 8,
		Voided = 16,
		VoidFailed = 32,
		Refunded = 64,
		RefundFailed = 128,
		PreAuthorizationExpired = 256,
		AuthorizedHoldingReview = 512,
		CapturedHoldingReview = 1024
	}

	public enum CCProcessingFeature
	{
		Base,
		ProfileManagement,
		ExtendedProfileManagement,
		HostedForm,
		PaymentHostedForm,
		WebhookManagement,
		TransactionGetter
	}

	public enum ProcessingStatus
	{
		Unknown,
		AuthorizeFail,
		CaptureFail,
		VoidFail,
		CreditFail,
		AuthorizeSuccess,
		AuthorizeExpired,
		CaptureSuccess,
		VoidSuccess,
		CreditSuccess,
		AuthorizeHeldForReview,
		CaptureHeldForReview,
		VoidHeldForReview,
		CreditHeldForReview,
		AuthorizeDecline,
		CaptureDecline,
		VoidDecline,
		CreditDecline
	}
}
