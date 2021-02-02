using PX.Data;
using PX.Objects.AR;
using System.Linq;
using PX.Objects.AR.CCPaymentProcessing.Common;
namespace PX.Objects.AR
{
	public static class ExtTransactionProcStatusCode
	{
		public const string AuthorizeFail = "AUF";
		public const string CaptureFail = "CAF";
		public const string VoidFail = "VDF";
		public const string CreditFail = "CDF";

		public const string AuthorizeSuccess = "AUS";
		public const string CaptureSuccess = "CAS";
		public const string VoidSuccess = "VDS";
		public const string CreditSuccess = "CDS";

		public const string AuthorizeExpired = "AUE";

		public const string AuthorizeHeldForReview = "AUH";
		public const string CaptureHeldForReview = "CAH";
		public const string VoidHeldForReview = "VDH";
		public const string CreditHeldForReview = "CDH";

		public const string AuthorizeDecline = "AUD";
		public const string CaptureDecline = "CAD";
		public const string VoidDecline = "VDD";
		public const string CreditDecline = "CDD";

		public const string Unknown = "UKN";

		public class ListAttribute : PXStringListAttribute
		{
			public ListAttribute()
				: base(
				new string[] {
					AuthorizeFail, CaptureFail, VoidFail, CreditFail,
					AuthorizeSuccess, CaptureSuccess, VoidSuccess, CreditSuccess,
					AuthorizeHeldForReview, CaptureHeldForReview, VoidHeldForReview, CreditHeldForReview,
					AuthorizeDecline, CaptureDecline, VoidDecline, CreditDecline,
					AuthorizeExpired, Unknown
				},
				new string[] {
					Messages.CCPreAuthorizationFailed, Messages.CCCaptureFailed, Messages.CCVoidFailed, Messages.CCRefundFailed,
					Messages.CCPreAuthorized, Messages.CCCaptured, Messages.CCVoided, Messages.CCRefunded,
					Messages.CCAuthorizedHoldingReview, Messages.CCCapturedHoldingReview, Messages.CCVoidedHoldingReview, Messages.CCRefundedHoldingReview,
					Messages.CCPreAuthorizationDeclined, Messages.CCCaptureDeclined, Messages.CCVoidDeclined, Messages.CCRefundDeclined,
					Messages.CCPreAuthorizationExpired, Messages.CCUnknown
				})
			{ }
		}

		public static string GetStatusByTranStatusTranType(string tranStatus, string tranType)
		{
			string ret = Unknown;
			if (tranStatus == CCTranStatusCode.Error)
			{
				switch (tranType)
				{
					case CCTranTypeCode.Authorize: ret = AuthorizeFail; break;
					case CCTranTypeCode.AuthorizeAndCapture:
					case CCTranTypeCode.PriorAuthorizedCapture:
					case CCTranTypeCode.CaptureOnly: ret = CaptureFail; break;
					case CCTranTypeCode.VoidTran: ret = VoidFail; break;
					case CCTranTypeCode.Credit: ret = CreditFail; break; 
				}
			}
			if (tranStatus == CCTranStatusCode.HeldForReview)
			{
				switch (tranType)
				{
					case CCTranTypeCode.Authorize: ret = AuthorizeHeldForReview; break;
					case CCTranTypeCode.AuthorizeAndCapture:
					case CCTranTypeCode.PriorAuthorizedCapture:
					case CCTranTypeCode.CaptureOnly: ret = CaptureHeldForReview; break;
					case CCTranTypeCode.VoidTran: ret = VoidHeldForReview; break;
					case CCTranTypeCode.Credit: ret = CreditHeldForReview; break; 
				}
			}
			if (tranStatus == CCTranStatusCode.Approved)
			{
				switch (tranType)
				{
					case CCTranTypeCode.Authorize: ret = AuthorizeSuccess; break;
					case CCTranTypeCode.AuthorizeAndCapture:
					case CCTranTypeCode.PriorAuthorizedCapture:
					case CCTranTypeCode.CaptureOnly: ret = CaptureSuccess; break;
					case CCTranTypeCode.VoidTran: ret = VoidSuccess; break;
					case CCTranTypeCode.Credit: ret = CreditSuccess; break; 
				}
			}
			if (tranStatus == CCTranStatusCode.Declined)
			{
				switch (tranType)
				{
					case CCTranTypeCode.Authorize: ret = AuthorizeDecline; break;
					case CCTranTypeCode.AuthorizeAndCapture:
					case CCTranTypeCode.PriorAuthorizedCapture:
					case CCTranTypeCode.CaptureOnly: ret = CaptureDecline; break;
					case CCTranTypeCode.VoidTran: ret = VoidDecline; break;
					case CCTranTypeCode.Credit: ret = CreditDecline; break;
				}
			}
			if (tranStatus == CCTranStatusCode.Expired && tranType == CCTranTypeCode.Authorize)
			{
				ret = AuthorizeExpired;
			}
			return ret;
		}

		public static ProcessingStatus GetProcessingStatusByProcStatusStr(string procStatusCode)
		{
			if (!mapping.Where(i => i.Item2 == procStatusCode).Any())
			{
				throw new PXInvalidOperationException();
			}
			return mapping.Where(i => i.Item2 == procStatusCode).Select(i => i.Item1).First();
		}

		public static string GetProcStatusStrByProcessingStatus(ProcessingStatus procStatus)
		{
			return mapping.Where(i => i.Item1 == procStatus).Select(i => i.Item2).First();
		}

		private static (ProcessingStatus, string)[] mapping = new[]
		{
			(ProcessingStatus.Unknown, Unknown), (ProcessingStatus.AuthorizeFail, AuthorizeFail),
			(ProcessingStatus.CaptureFail, CaptureFail), (ProcessingStatus.VoidFail, VoidFail),
			(ProcessingStatus.CreditFail, CreditFail), (ProcessingStatus.AuthorizeExpired, AuthorizeExpired),
			(ProcessingStatus.AuthorizeSuccess, AuthorizeSuccess), (ProcessingStatus.CaptureSuccess, CaptureSuccess),
			(ProcessingStatus.VoidSuccess, VoidSuccess), (ProcessingStatus.CreditSuccess, CreditSuccess),
			(ProcessingStatus.AuthorizeHeldForReview, AuthorizeHeldForReview), (ProcessingStatus.CaptureHeldForReview, CaptureHeldForReview),
			(ProcessingStatus.VoidHeldForReview, VoidHeldForReview), (ProcessingStatus.CreditHeldForReview, CreditHeldForReview),
			(ProcessingStatus.AuthorizeDecline, AuthorizeDecline), (ProcessingStatus.CaptureDecline, CaptureDecline),
			(ProcessingStatus.VoidDecline, VoidDecline), (ProcessingStatus.CreditDecline, CreditDecline)
		};

		public class captureSuccess : PX.Data.BQL.BqlString.Constant<captureSuccess>
		{
			public captureSuccess() : base(CaptureSuccess) { }
		}
	}
}
