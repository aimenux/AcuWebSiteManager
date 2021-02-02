using System.Collections.Generic;
using System.Linq;
using AutoMapper;
using PX.Objects.PJ.OutlookIntegration.Initialize.Mappers;
using PX.Objects.PJ.OutlookIntegration.OU.DAC;
using PX.Objects.PJ.ProjectManagement.PJ.DAC;
using PX.Objects.PJ.ProjectsIssue.PJ.DAC;
using PX.Objects.PJ.ProjectsIssue.PJ.Graphs;
using PX.Objects.PJ.RequestsForInformation.PJ.DAC;
using PX.Objects.PJ.RequestsForInformation.PJ.Descriptor.Attributes;
using PX.Objects.PJ.RequestsForInformation.PJ.Graphs;
using PX.Data;
using PX.Objects.CR;
using PX.Objects.CS;

namespace PX.Objects.PJ.OutlookIntegration.OU.GraphExtensions
{
    public class OuSearchMaintExtension : PXGraphExtension<OuSearchMaintExtensionBase, OUSearchMaint>
    {
        [PXHidden]
        [PXCheckCurrent]
        public PXSetup<ProjectManagementSetup> ProjectManagementSetup;

        public PXAction<OUSearchEntity> RedirectToCreateRequestForInformation;
        public PXAction<OUSearchEntity> CreateRequestForInformation;
        public PXAction<OUSearchEntity> RedirectToCreateProjectIssue;
        public PXAction<OUSearchEntity> CreateProjectIssue;

        private IMapper mapper;

        protected IMapper Mapper => mapper = mapper ?? CreateMapper();

        public static bool IsActive()
        {
            return PXAccess.FeatureInstalled<FeaturesSet.constructionProjectManagement>();
        }

        [PXUIField(DisplayName = "Create Request for Information", MapEnableRights = PXCacheRights.Update,
            MapViewRights = PXCacheRights.Select, Visible = false)]
        [PXButton]
        protected virtual void redirectToCreateRequestForInformation()
        {
            Base.Filter.Current.Operation = nameof(RequestForInformationOutlook);
            Base1.RequestForInformationOutlook.Current = Base1.RequestForInformationOutlook.Insert();
        }

        [PXUIField(DisplayName = "Create Project Issue", MapEnableRights = PXCacheRights.Update,
            MapViewRights = PXCacheRights.Select, Visible = false)]
        [PXButton]
        protected virtual void redirectToCreateProjectIssue()
        {
            Base.Filter.Current.Operation = nameof(ProjectIssueOutlook);
            Base1.ProjectIssueOutlook.Current = Base1.ProjectIssueOutlook.Insert();
        }

        [PXUIField(DisplayName = "Create Request for Information", MapEnableRights = PXCacheRights.Update,
            MapViewRights = PXCacheRights.Select, Visible = false)]
        [PXButton(CommitChanges = true)]
        protected virtual void createRequestForInformation()
        {
            Base1.CreateEntity(CreateRequestForInformationEntity);
        }

        [PXUIField(DisplayName = "Create Project Issue", MapEnableRights = PXCacheRights.Update,
            MapViewRights = PXCacheRights.Select, Visible = false)]
        [PXButton(CommitChanges = true)]
        protected virtual void createProjectIssue()
        {
            Base1.CreateEntity(CreateProjectIssueEntity);
        }

        protected virtual void _(Events.RowSelected<OUSearchEntity> args, PXRowSelected baseHandler)
        {
            baseHandler(args.Cache, args.Args);
            if (args.Row is OUSearchEntity searchEntity)
            {
                SetCreateActionsVisibility(searchEntity);
                SetOutlookEntitiesVisibility(searchEntity);
                SetRedirectionActionVisibility(searchEntity);
                ConfigureRequestForInformationFields(searchEntity);
            }
        }

        private static IMapper CreateMapper()
        {
            var configuration = new MapperConfiguration(cfg => cfg.AddProfile<OutlookMapperProfile>());
            return configuration.CreateMapper();
        }

        private void CreateRequestForInformationEntity()
        {
            var graph = PXGraph.CreateInstance<RequestForInformationMaint>();
            var requestForInformation = InsertRequestForInformation(graph);
            if (IsEmailCreatedSuccessfully<RequestForInformation.noteID>(graph.RequestForInformation.Cache,
                requestForInformation))
            {
                requestForInformation.RequestDetails = GetEmailBody(graph, graph.Activities);
                graph.RequestForInformation.Update(requestForInformation);
                graph.Save.PressImpl(false, true);
            }
        }

        private RequestForInformation InsertRequestForInformation(RequestForInformationMaint graph)
        {
            var requestForInformation = Mapper.Map<RequestForInformation>(Base1.RequestForInformationOutlook.Current);
            return graph.RequestForInformation.Insert(requestForInformation);
        }

        private void CreateProjectIssueEntity()
        {
            var graph = PXGraph.CreateInstance<ProjectIssueMaint>();
            var projectIssue = InsertProjectIssue(graph);
            if (IsEmailCreatedSuccessfully<ProjectIssue.noteID>(graph.ProjectIssue.Cache, projectIssue))
            {
                projectIssue.Description = GetEmailBody(graph, graph.Activities);
                graph.ProjectIssue.Update(projectIssue);
                graph.Save.PressImpl(false, true);
            }
        }

        private ProjectIssue InsertProjectIssue(ProjectIssueMaint graph)
        {
            var projectIssue = Mapper.Map<ProjectIssue>(Base1.ProjectIssueOutlook.Current);
            return graph.ProjectIssue.Insert(projectIssue);
        }

        private string GetEmailBody(PXGraph graph, PXSelectBase<CRPMTimeActivity> activityList)
        {
            var activity = activityList.Select().FirstOrDefault()?.GetItem<CRPMTimeActivity>();
            var query = new PXSelect<SMEmailBody,
                Where<SMEmailBody.refNoteID, Equal<Required<SMEmailBody.refNoteID>>>>(graph);
            return query.SelectSingle(activity?.NoteID)?.Body;
        }

        private bool IsEmailCreatedSuccessfully<TNoteIdField>(PXCache cache, object entity)
            where TNoteIdField : IBqlField
        {
            var noteId = PXNoteAttribute.GetNoteID<TNoteIdField>(cache, entity);
            return Base.PersisMessage(noteId, null, null);
        }

        private void SetCostAndScheduleImpactVisibility(RequestForInformationOutlook requestForInformation)
        {
            var cache = Base1.RequestForInformationOutlook.Cache;
            PXUIFieldAttribute.SetVisible<ProjectManagementImpact.scheduleImpact>(
                cache, null, requestForInformation.IsScheduleImpact.GetValueOrDefault());
            PXUIFieldAttribute.SetVisible<ProjectManagementImpact.costImpact>(
                cache, null, requestForInformation.IsCostImpact.GetValueOrDefault());
        }

        private void SetCreateActionsVisibility(OUSearchEntity searchEntity)
        {
            SetCreateActionVisibility<RequestForInformation, RequestForInformationOutlook>(searchEntity);
            SetCreateActionVisibility<ProjectIssue, ProjectIssueOutlook>(searchEntity);
        }

        private void SetOutlookEntitiesVisibility(OUSearchEntity searchEntity)
        {
            SetOutlookEntityVisibility<RequestForInformationOutlook>(searchEntity);
            SetOutlookEntityVisibility<ProjectIssueOutlook>(searchEntity);
        }

        private void SetRedirectionActionVisibility(OUSearchEntity searchEntity)
        {
            SetRedirectionActionVisibility<RequestForInformationMaint, RequestForInformation>(searchEntity);
            SetRedirectionActionVisibility<ProjectIssueMaint, ProjectIssue>(searchEntity);
        }

        private void ConfigureRequestForInformationFields(OUSearchEntity searchEntity)
        {
            var requestForInformation = Base1.RequestForInformationOutlook.Current;
            if (searchEntity.Operation == nameof(RequestForInformationOutlook) && requestForInformation != null)
            {
                SetCostAndScheduleImpactVisibility(requestForInformation);
                SetRequestForInformationStatusSelector(requestForInformation);
                PXUIFieldAttribute.SetEnabled<RequestForInformationOutlook.incoming>(
                    Base1.RequestForInformationOutlook.Cache, null,
                    requestForInformation.Status == RequestForInformationStatusAttribute.NewStatus);
            }
        }

        private void SetOutlookEntityVisibility<TTable>(OUSearchEntity searchEntity)
        {
            var viewName = typeof(TTable).Name;
            var cache = Base.Views[viewName].Cache;
            PXUIFieldAttribute.SetVisible(cache, cache.Current, null, searchEntity.Operation == viewName);
        }

        private void SetCreateActionVisibility<TTable, TOutlookTable>(OUSearchEntity searchEntity)
        {
            Base.Actions[$"Create{typeof(TTable).Name}"]
                .SetVisible(searchEntity.Operation == typeof(TOutlookTable).Name);
        }

        private void SetRedirectionActionVisibility<TGraph, TTable>(OUSearchEntity searchEntity)
        {
            Base.Actions[$"RedirectToCreate{typeof(TTable).Name}"]
                .SetVisible(IsRedirectActionVisible<TGraph, TTable>(searchEntity));
        }

        private void SetRequestForInformationStatusSelector(RequestForInformationOutlook requestForInformation)
        {
            var (allowedValues, allowedLabels) =
                GetStatusListProperties(requestForInformation.Incoming.GetValueOrDefault());
            PXStringListAttribute.SetList<RequestForInformationOutlook.status>(
                Base1.RequestForInformationOutlook.Cache, null, allowedValues.ToArray(), allowedLabels.ToArray());
        }

        private static (List<string>, List<string>) GetStatusListProperties(bool isIncoming)
        {
            var allowedValues = new List<string>
            {
                RequestForInformationStatusAttribute.NewStatus
            };
            var allowedLabels = new List<string>
            {
                RequestForInformationStatusAttribute.NewStatusLabel
            };
            if (isIncoming)
            {
                allowedValues.Add(RequestForInformationStatusAttribute.OpenStatus);
                allowedLabels.Add(RequestForInformationStatusAttribute.OpenStatusLabel);
            }
            return (allowedValues, allowedLabels);
        }

        private bool IsRedirectActionVisible<TGraph, TTable>(OUSearchEntity searchEntity)
        {
            var message = Base.SourceMessage.Current;
            return message.IsIncome.GetValueOrDefault() && message.MessageId != null && searchEntity.Operation == null
                && Base.IsRights(typeof(TGraph), typeof(TTable), PXCacheRights.Insert);
        }
    }
}