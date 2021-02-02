using System.Linq;
using PX.Objects.PJ.ProjectManagement.PJ.DAC;
using PX.Objects.PJ.ProjectsIssue.PJ.DAC;
using PX.Common;
using PX.Data;
using PX.Objects.CN.Common.Extensions;
using PX.Objects.CR;

namespace PX.Objects.PJ.ProjectsIssue.PJ.Graphs
{
    public class ProjectIssuesMaint : PXGraph<ProjectIssuesMaint>
    {
        public PXFilter<ProjectFilter> Filter;

        [PXFilterable]
        public PXProcessing<ProjectIssue,
            Where<ProjectIssue.projectId, Equal<Current<ProjectFilter.projectId>>,
                Or<Current<ProjectFilter.projectId>, IsNull>>,
            OrderBy<Desc<ProjectIssue.projectIssueId>>> ProjectIssues;

        public PXCancel<ProjectFilter> Cancel;
        public PXAction<ProjectFilter> InsertProjectIssue;
        public PXAction<ProjectFilter> EditProjectIssue;

        [PXHidden]
        [PXCheckCurrent]
        public PXSetup<ProjectManagementSetup> Setup;

        public ProjectIssuesMaint()
        {
            UpdateLayout();
            UpdateProjectIssueViewCacheIfRequired();
        }

        [PXInsertButton]
        [PXUIField(DisplayName = "")]
        protected virtual void insertProjectIssue()
        {
            var graph = CreateInstance<ProjectIssueMaint>();
            graph.ProjectIssue.Insert();
            graph.ProjectIssue.Cache.IsDirty = false;
            PXRedirectHelper.TryRedirect(graph, PXRedirectHelper.WindowMode.InlineWindow);
        }

        [PXEditDetailButton]
        [PXUIField(DisplayName = "")]
        protected virtual void editProjectIssue()
        {
            this.RedirectToEntity(ProjectIssues.Current, PXRedirectHelper.WindowMode.InlineWindow);
        }

        private void UpdateLayout()
        {
            HideField<ProjectIssue.workgroupID>();
            HideField<ProjectIssue.dueDate>();
            HideField<ProjectIssue.creationDate>();
            HideField<ProjectIssue.ownerID>();
        }

        private void HideField<TField>()
            where TField : IBqlField
        {
            PXUIFieldAttribute.SetVisible<TField>(ProjectIssues.Cache, null, false);
        }

        /// <summary>
        /// This method is used for updating ProjectIssue view cache, because
        /// mobile framework without these data can't find the row on which users are clicking.
        /// By default Cached collection contains only the last row in the list.
        /// </summary>
        private void UpdateProjectIssueViewCacheIfRequired()
        {
            if (ProjectIssues.Cache.Cached.Count() == 1)
            {
                var dummy = ProjectIssues.Select().FirstTableItems.ToList();
            }
        }
    }
}
