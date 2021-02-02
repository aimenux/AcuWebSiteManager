using PX.Common;

namespace PX.Objects.PJ.ProjectManagement.Descriptor
{
	[PXLocalizable("PM Error")]
	public class ProjectManagementMessages
	{
		public const string ProjectManagementClassIsNotActive = "Project Management Class is not active or deleted.";
		public const string AssignmentMapIdIsNotSpecified = "Assignment Map ID is not specified.";

		public const string UnableToFindRouteForAssignmentProcess =
			"The document hasn't met any criteria in the specified assignment rules. You may need to review the assignment map.";

		public const string ProjectIssueTypeUniqueConstraint = "Project Issue Type already exists in the system.";
		public const string SubmittalTypeUniqueConstraint = "Submittal Type already exists in the system.";
		public const string SystemRecordCannotBeDeleted = "This is a system record and cannot be deleted.";
		public const string InactivePriorityCannotBeDefault = "Inactive Priority cannot be marked as default.";
		public const string NoDefaultPrioritySpecified = "No default Priority is specified.";
		public const string OnlyActivePrioritiesAreAllowed = "Only active Priorities are allowed";
		public const string ActivePriorityCannotBeDeleted = "Active Priority cannot be deleted.";
		public const string StatusNameUniqueConstraint = "Status already exists in the system.";

		public const string UsedValueCannotBeDeleted =
			"The value cannot be deleted. It is used in at least one Record.";

		public const string CannotDeleteProjectManagementClassInUse =
			"Cannot delete because Project Management Class is already in use.";

		public const string ProjectManagementClassAlreadyInUse =
			"Project Management Class is already in use. Please refresh the page.";

		public const string ValueUsedInProjectIssue =
			"The value cannot be deleted. It is used in at least one Project Issue";

		public const string EnableChangeOrderWorkflowForTheProject =
			"To convert to the change request enable Change Order Workflow for the Project.";

		public const string WorkCalendarCannotBeDeleted =
			"Work Calendar cannot be deleted. It is used on Project Management Preferences.";

		public const string AssignDefaultStatus =
			"All {0} with '{1}' status will be automatically assigned the default status '{2}'. " +
			"Continue?";

		public const string ChangeRequestsFeatureHasBeenDisabled =
			"The document cannot be opened because the Change Requests feature has been disabled on the " +
			"Enable/Disable Features (CS100000) form.";

		public const string ChangeOrderClassIsTwoTier =
			"On 2-tier change management classes change requests need to be managed via web application.";

		public const string DaysForReviewIsNegative =	"Days For Review cannot be negative.";

		#region Combo Values

		public const string New = "New";
		public const string Open = "Open";
		public const string Closed = "Closed";
		public const string Rejected = "Rejected";
		public const string Planned = "Planned";
		public const string Pending = "Pending";
		public const string Completed = "Completed";
		public const string Canceled = "Canceled";
		public const string Approved = "Approved";

		public const string SubmitterLabel = "Submitter";
		public const string ApproverLabel = "Approver";
		public const string ReviewerLabel = "Reviewer";

		#endregion

		#region Names

		public const string ContactPhone = "Phone";
		public const string EmailTo = "Email To";

		#endregion
	}
}