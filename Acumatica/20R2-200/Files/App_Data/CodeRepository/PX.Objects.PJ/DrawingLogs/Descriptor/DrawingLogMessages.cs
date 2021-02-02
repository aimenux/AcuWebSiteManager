using PX.Common;

namespace PX.Objects.PJ.DrawingLogs.Descriptor
{
    [PXLocalizable]
    public class DrawingLogMessages
    {
        public const string DisciplineNameUniqueConstraint = "Discipline already exists in the system.";
        public const string ActiveDisciplineCannotBeDeleted = "Active Discipline cannot be deleted.";
        public const string NoRecordsWereSelected = "No records were selected.";
        public const string NoLongerExists = "{0} no longer exists in the system.";
        public const string RecordHasStatus = "{0} has '{1}' status.";
        public const string DisciplineIsInactive = "Discipline is Inactive.";
        public const string AddNewRowIsNotAvailable = "Add New Row is not available if you have required Attributes.";
        public const string AddNewRow = "Add New Row";
        public const string DrawingLogWillBeDeleted = "The current Drawing Log record will be deleted.";
        public const string DrawingLogEmailDefaultSubject = "Drawing Log #[{0} {1}] {2}";
        public const string DrawingLogDocuments = "Drawing Log Documents";

        public const string NewRequestForInformationWithDifferentProjects =
            "New RFI cannot be created. Selected Drawing Log Documents belong to different Projects.";

        public const string NewProjectIssueWithDifferentProjects =
            "New Project Issue cannot be created. Selected Drawing Log Documents belong to different Projects.";

        public const string DisableDrawingLogWithAttributes =
            "Drawing Log Document with required Attributes must be entered through the form view (PJ303000)";

        public const string DrawingLogWithRevisionCannotBeDeleted =
            "Drawing Log Document cannot be deleted since it has at least one Revision associated with it.";

        public const string NoAttachedFilesAreAvailableForSelectedDrawingLogs =
            "No attached files are available for the selected Drawing Log Documents.";

        public const string NoAttachedFilesAreAvailableForDrawingLog =
            "No attached files are available for the Drawing Log Document.";

        public const string OnlyProjectsInStatusesAreAllowed =
            "Only Projects in \"Active\" and \"In Planning\" statuses are allowed to be indicated for the " +
            "Drawing Log Documents.";

        public const string DisciplineAlreadyInUse =
            "The value cannot be deleted. It is used in at least one Drawing Log Document.";

        public const string CancelDrawingLogChanges = "Changes you made to Drawing Log Documents will not be saved. " +
            "Continue?";

        public const string RevisionWillBeUpdated =
            "Updating Project, Project Task, Discipline or Number values for the Original Drawing will update " +
            "respective values for all Revisions. Would you like to proceed?";

        public const string UnlinkRelationIfProjectChanged =
            "Changing the Project unlinks the RFI/Project Issue associated with Drawing Log " +
            "Document where Project no longer matches. Continue?";
    }
}