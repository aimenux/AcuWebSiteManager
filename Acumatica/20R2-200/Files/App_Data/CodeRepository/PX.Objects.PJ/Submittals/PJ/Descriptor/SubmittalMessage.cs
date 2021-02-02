using PX.Common;

namespace PX.Objects.PJ.Submittals.PJ.Descriptor
{
	[PXLocalizable]
	public static class SubmittalMessage
	{
		public const string WorkflowPromptFormTitle = "Details";
		public const string DateClosed = "Date Closed";
		public const string CloseAction = "Close Submittal";
		public const string OpenAction = "Open Submittal";
		public const string SubmittleReportNamePattern = "Submittal {0}-{1} ({2}).pdf";
		public const string OldRevisionWarning = "Viewing an old revision of this submittal";
		public const string SubmittleSearchTitle = "Submittal: {0} Revision: {1}";
		public const string BallInCourt = "Ball in Court";


	}

	[PXLocalizable]
	public static class SubmittalReason
	{
		public const string New = "New";
		public const string Revision = "Revision";
		public const string Issued = "Issued";
		public const string Submitted = "Submitted";
		public const string PendingApproval = "Pending Approval";
		public const string Approved = "Approved";
		public const string ApprovedAsNoted = "Approved as Noted";
		public const string Rejected = "Rejected";
		public const string Canceled = "Canceled";
		public const string ReviseAndResubmit = "Revise and Resubmit";
	}
}
