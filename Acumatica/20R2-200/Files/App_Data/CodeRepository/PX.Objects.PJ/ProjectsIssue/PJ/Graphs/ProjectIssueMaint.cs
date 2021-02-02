using System.Collections;
using System.Collections.Generic;
using System.Linq;
using PX.Objects.PJ.Common.Descriptor;
using PX.Objects.PJ.ProjectManagement.Descriptor;
using PX.Objects.PJ.ProjectManagement.PJ.DAC;
using PX.Objects.PJ.ProjectManagement.PJ.Graphs;
using PX.Objects.PJ.ProjectManagement.PJ.Services;
using PX.Objects.PJ.ProjectsIssue.Descriptor;
using PX.Objects.PJ.ProjectsIssue.PJ.DAC;
using PX.Objects.PJ.ProjectsIssue.PJ.Descriptor.Attributes;
using PX.Objects.PJ.RequestsForInformation.PJ.DAC;
using PX.Objects.PJ.RequestsForInformation.PJ.Graphs;
using PX.Data;
using PX.Objects.CN.Common.Extensions;
using PX.Objects.Common.Extensions;
using PX.Objects.CR;
using PX.Objects.CS;
using PX.Objects.PM;
using PX.Objects.PM.ChangeRequest;
using Constants = PX.Objects.PJ.ProjectsIssue.PJ.Descriptor.Constants;
using PmConstants = PX.Objects.PJ.ProjectManagement.PJ.Descriptor.Constants;
using PX.Objects.PJ.ProjectManagement.PM.CacheExtensions;

namespace PX.Objects.PJ.ProjectsIssue.PJ.Graphs
{
    public class ProjectIssueMaint : ProjectManagementBaseMaint<ProjectIssueMaint, ProjectIssue>
    {
        [PXViewName(CacheNames.ProjectIssue)]
        [RelatedEntitySelector(typeof(ProjectIssue), typeof(ProjectIssue.refNoteId))]
        [PXCopyPasteHiddenFields(
            typeof(ProjectIssue.status),
            typeof(ProjectIssue.resolvedOn),
            typeof(ProjectIssue.creationDate))]
        public PXSelect<ProjectIssue> ProjectIssue;

        [PXHidden]
        public PXSelect<ProjectIssue,
            Where<ProjectIssue.projectIssueId,
                Equal<Current<ProjectIssue.projectIssueId>>>> CurrentProjectIssue;

        [PXHidden]
        [PXCheckCurrent]
        public PXSetup<ProjectManagementSetup> ProjectManagementSetup;

        public PXAction<ProjectIssue> ConvertToRfi;
        public PXAction<ProjectIssue> ConvertToChangeRequest;
        public PXAction<ProjectIssue> Print;

        public ProjectIssueMaint()
        {
            UpdateLayoutVisibility();
        }

        [InjectDependency]
        public IProjectManagementClassDataProvider ProjectManagementClassDataProvider
        {
            get;
            set;
        }

        [InjectDependency]
        public IProjectManagementImpactService ProjectManagementImpactService
        {
            get;
            set;
        }

        public override IEnumerable ExecuteSelect(string viewName, object[] parameters, object[] searches,
            string[] sortColumns, bool[] descendings, PXFilterRow[] filters, ref int startRow, int maximumRows,
            ref int totalRows)
        {
            OverridePrioritiesSelectIfRequired(viewName, sortColumns, ref searches);
            return base.ExecuteSelect(viewName, parameters, searches, sortColumns, descendings, filters, ref startRow,
                maximumRows, ref totalRows);
        }

        [PXButton]
        [PXUIField(DisplayName = "Convert to RFI", MapEnableRights = PXCacheRights.Update,
            MapViewRights = PXCacheRights.Select)]
        public virtual void convertToRfi()
        {
            var graph = CreateInstance<RequestForInformationMaint>();
            var requestForInformation = CreateRequestForInformation(graph);
            PXRedirectHelper.TryRedirect(graph, requestForInformation,
                PXRedirectHelper.WindowMode.InlineWindow);
        }

        [PXButton(CommitChanges = true)]
        [PXUIField(DisplayName = "Print/Email", MapEnableRights = PXCacheRights.Select,
            MapViewRights = PXCacheRights.Select)]
        public virtual void print()
        {
            Persist();
            var parameters = new Dictionary<string, string>
            {
                [ProjectIssueConstants.Print.ProjectIssueId] = ProjectIssue.Current.ProjectIssueCd
            };
            throw new PXReportRequiredException(parameters, ScreenIds.ProjectIssueForm, null);
        }

        public override void _(Events.RowSelected<ProjectIssue> args)
        {
            base._(args);
            ConvertToChangeRequest.SetVisible(PXAccess.FeatureInstalled<FeaturesSet.changeRequest>());
        }

        public virtual void _(Events.FieldUpdated<ProjectIssue, ProjectIssue.status> args)
        {
            var projectIssue = args.Row;
            if (projectIssue != null)
            {
                UpdateResolvedOnDate(projectIssue);
            }
        }

        public virtual void _(Events.RowPersisting<ProjectIssue> args)
        {
            var projectIssue = args.Row;
            if (projectIssue != null)
            {
                projectIssue.MajorStatus = projectIssue.Status;
                ValidateClassId(args.Cache, projectIssue);
                ProjectManagementImpactService.ClearScheduleAndCostImpactIfRequired(projectIssue);
            }
        }

        protected override PMChangeRequest CreateChangeRequest(ChangeRequestEntry graph)
        {
            var changeRequest = base.CreateChangeRequest(graph);
            changeRequest.Description = ProjectIssue.Current.Summary;
            changeRequest.Text = ProjectIssue.Current.Description;

            var changeRequestExt = PXCache<PMChangeRequest>.GetExtension<PmChangeRequestExtension>(changeRequest);
            changeRequestExt.ProjectIssueID = ProjectIssue.Current.ProjectIssueId;
            
return changeRequest;
        }

        private void UpdateResolvedOnDate(ProjectIssue projectIssue)
        {
            projectIssue.ResolvedOn = projectIssue.Status == ProjectIssueStatusAttribute.Closed
                ? Accessinfo.BusinessDate
                : null;
        }

        private void UpdateLayoutVisibility()
        {
            PXUIFieldAttribute.SetVisible<CRPMTimeActivity.categoryID>(Activities.Cache, null, false);
        }

        private void ValidateClassId(PXCache cache, ProjectIssue projectIssue)
        {
            if (projectIssue.ClassId == null)
            {
                cache.RaiseException<ProjectIssue.classId>(projectIssue, PX.Objects.CN.Common.Descriptor.SharedMessages.FieldIsEmpty);
                return;
            }
            var projectManagementClass =
                ProjectManagementClassDataProvider.GetProjectManagementClass(projectIssue.ClassId);
            if (projectManagementClass == null || projectManagementClass.UseForProjectIssue == false)
            {
                cache.RaiseException<ProjectIssue.classId>(projectIssue, ProjectManagementMessages.ProjectManagementClassIsNotActive);
            }
        }

        private RequestForInformation CreateRequestForInformation(RequestForInformationMaint graph)
        {
            var projectIssue = CurrentProjectIssue.Current;
            var requestForInformation = graph.RequestForInformation.Insert();
            requestForInformation.ConvertedFrom = projectIssue.NoteID;
            requestForInformation.Summary = projectIssue.Summary;
            requestForInformation.ProjectId = projectIssue.ProjectId;
            requestForInformation.ProjectTaskId = projectIssue.ProjectTaskId;
            requestForInformation.PriorityId = projectIssue.PriorityId;
            requestForInformation.RequestDetails = projectIssue.Description;
            var classId = GetRequestForInformationClassId(projectIssue);
            graph.CurrentRequestForInformation.SetValueExt<RequestForInformation.classId>(
                requestForInformation, classId);
            requestForInformation.IsScheduleImpact = projectIssue.IsScheduleImpact;
            requestForInformation.ScheduleImpact = projectIssue.ScheduleImpact;
            requestForInformation.IsCostImpact = projectIssue.IsCostImpact;
            requestForInformation.CostImpact = projectIssue.CostImpact;
            return requestForInformation;
        }

        private string GetRequestForInformationClassId(ProjectIssue projectIssue)
        {
            var projectManagementClass =
                ProjectManagementClassDataProvider.GetProjectManagementClass(projectIssue.ClassId);
            return projectManagementClass?.UseForRequestForInformation == true
                ? projectManagementClass.ProjectManagementClassId
                : null;
        }

        private static void OverridePrioritiesSelectIfRequired(string viewName, string[] sortColumns,
            ref object[] searches)
        {
            if (viewName == Constants.ProjectManagementClassPriorityViewName
                && sortColumns.Contains(PmConstants.PriorityNameField))
            {
                var index = sortColumns.FindIndex(x => x == PmConstants.PriorityNameField);
                sortColumns[index] = PmConstants.SortOrderField;
                searches = new[]
                {
                    (object) null
                };
            }
        }
    }
}