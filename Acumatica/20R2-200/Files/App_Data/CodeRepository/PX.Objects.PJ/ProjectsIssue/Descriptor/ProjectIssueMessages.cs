using PX.Common;

namespace PX.Objects.PJ.ProjectsIssue.Descriptor
{
    [PXLocalizable]
    public static class ProjectIssueMessages
    {
        public const string WarningRemovingProjectIssueAttributes =
            "Changing the Project Issue class will remove all attribute values associated with the current " +
            "class and replace them with the attribute values of the new class. Continue?";

        public const string UnlinkDrawingLogsOnProjectChange =
            "Changing the Project unlinks the Drawing Log Documents associated with Project Issue where " +
            "Project no longer matches. Continue?";

        public const string LinkToProjectIssue = "Link to Issue";
    }
}