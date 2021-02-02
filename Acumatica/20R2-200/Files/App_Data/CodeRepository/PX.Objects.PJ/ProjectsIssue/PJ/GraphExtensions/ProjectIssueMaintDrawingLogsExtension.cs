using System.Collections;
using PX.Objects.PJ.ProjectManagement.PJ.DAC;
using PX.Objects.PJ.ProjectManagement.PJ.GraphExtensions;
using PX.Objects.PJ.ProjectsIssue.Descriptor;
using PX.Objects.PJ.ProjectsIssue.PJ.DAC;
using PX.Objects.PJ.ProjectsIssue.PJ.Descriptor.Attributes;
using PX.Objects.PJ.ProjectsIssue.PJ.Graphs;
using PX.Data;
using Constants = PX.Objects.CN.Common.Descriptor.Constants;

namespace PX.Objects.PJ.ProjectsIssue.PJ.GraphExtensions
{
    public class ProjectIssueMaintDrawingLogsExtension :
        DrawingLogBaseExtension<ProjectIssueMaint, ProjectIssue, ProjectIssueDrawingLog>
    {
        protected override string ProjectChangeWarningMessage => ProjectIssueMessages.UnlinkDrawingLogsOnProjectChange;

        public override void Initialize()
        {
            LinkDrawingLogToEntity.SetCaption(ProjectIssueMessages.LinkToProjectIssue);
        }

        public override IEnumerable drawingLogReferences()
        {
            return new PXSelect<ProjectIssueDrawingLog,
                Where<ProjectIssueDrawingLog.projectIssueId,
                    Equal<Current<ProjectIssue.projectIssueId>>>>(Base).Select();
        }

        [PXMergeAttributes(Method = MergeMethod.Merge)]
        [PXCustomizeBaseAttribute(typeof(PXUIFieldAttribute), Constants.AttributeProperties.Visible, false)]
        protected virtual void DrawingLog_OwnerId_CacheAttached(PXCache cache)
        {
        }

        protected override bool IsLinkDrawingActionEnabled(ProjectIssue projectIssue)
        {
            return Base.ProjectIssue.Cache.GetStatus(projectIssue) != PXEntryStatus.Inserted &&
                projectIssue.Status == ProjectIssueStatusAttribute.Open;
        }

        protected override void SetReferenceEntityId(ProjectIssueDrawingLog reference)
        {
            reference.ProjectIssueId = Base.ProjectIssue.Current.ProjectIssueId;
        }

        protected override DrawingLogReferenceBase CreateDrawingLogReference(int? drawingLogId)
        {
            return new ProjectIssueDrawingLog
            {
                ProjectIssueId = Base.ProjectIssue.Current.ProjectIssueId,
                DrawingLogId = drawingLogId
            };
        }
    }
}