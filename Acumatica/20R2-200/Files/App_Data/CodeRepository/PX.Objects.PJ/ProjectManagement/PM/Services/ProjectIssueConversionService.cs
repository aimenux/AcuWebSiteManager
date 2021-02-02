using System;
using System.Linq;
using PX.Objects.PJ.ProjectsIssue.PJ.DAC;
using PX.Objects.PJ.ProjectsIssue.PJ.Descriptor.Attributes;
using PX.Data;
using PX.Objects.PM;
using PX.Objects.PM.ChangeRequest;
using PX.Objects.PJ.ProjectManagement.PM.CacheExtensions;

namespace PX.Objects.PJ.ProjectManagement.PM.Services
{
    public class ProjectIssueConversionService : ConversionServiceBase
    {
        public ProjectIssueConversionService(ChangeRequestEntry graph)
            : base(graph)
        {
        }

        public override void UpdateConvertedEntity(PMChangeRequest changeRequest)
        {
            var projectIssue = GetProjectIssue(changeRequest);
            if (projectIssue != null)
            {
                UpdateProjectIssue(projectIssue, ProjectIssueStatusAttribute.Open, null);
            }
        }

        public override void SetFieldReadonly(PMChangeRequest changeRequest)
        {
            SetFieldReadOnly<PmChangeRequestExtension.projectIssueID>(changeRequest);
        }

        protected override void ProcessConvertedChangeRequest(PMChangeRequest changeRequest)
        {
            var projectIssue = GetProjectIssue(changeRequest);
            UpdateProjectIssue(projectIssue, ProjectIssueStatusAttribute.ConvertedToChangeRequest, changeRequest.NoteID);
            CopyFilesToChangeRequest<ProjectIssue>(projectIssue, changeRequest);
        }

        private void UpdateProjectIssue(ProjectIssue projectIssue, string status, Guid? noteId)
        {
            projectIssue.Status = status;
            projectIssue.MajorStatus = status;
            projectIssue.ConvertedTo = noteId;
            Graph.Caches<ProjectIssue>().PersistUpdated(projectIssue);
        }

        private ProjectIssue GetProjectIssue(PMChangeRequest changeRequest)
        {
            var changeRequestExt = PXCache<PMChangeRequest>.GetExtension<PmChangeRequestExtension>(changeRequest);
            return Graph.Select<ProjectIssue>()
                .SingleOrDefault(projectIssue => projectIssue.ProjectIssueId == changeRequestExt.ProjectIssueID);
        }
    }
}
