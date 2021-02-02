using System;

namespace PX.Objects.AR.CCPaymentProcessing.Common
{
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
}
