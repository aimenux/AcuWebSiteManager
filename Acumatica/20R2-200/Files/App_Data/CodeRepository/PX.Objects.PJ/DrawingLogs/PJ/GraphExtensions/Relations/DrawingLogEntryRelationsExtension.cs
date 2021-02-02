using System.Collections;
using System.Collections.Generic;
using System.Linq;
using PX.Objects.PJ.Common.Descriptor;
using PX.Objects.PJ.DrawingLogs.PJ.DAC;
using PX.Objects.PJ.DrawingLogs.PJ.Graphs;
using PX.Objects.PJ.ProjectsIssue.PJ.DAC;
using PX.Objects.PJ.ProjectsIssue.PJ.Descriptor.Attributes;
using PX.Objects.PJ.RequestsForInformation.PJ.DAC;
using PX.Objects.PJ.RequestsForInformation.PJ.Descriptor.Attributes;
using PX.Common;
using PX.Data;
using PX.Objects.CN.Common.Descriptor;
using PX.Objects.CS;
using PX.Objects.PM;

namespace PX.Objects.PJ.DrawingLogs.PJ.GraphExtensions.Relations
{
    public class DrawingLogEntryRelationsExtension : DrawingLogRelationsBaseExtension<DrawingLogEntry>
    {
        [PXCopyPasteHiddenView]
        public PXSelect<UnlinkedDrawingLogRelation> UnlinkedDrawingLogRelations;

        public PXAction<DrawingLog> Link;
        public PXAction<DrawingLog> Unlink;
        public PXAction<DrawingLog> LinkEntity;

        public new static bool IsActive()
        {
            return PXAccess.FeatureInstalled<FeaturesSet.constructionProjectManagement>();
        }

        public IEnumerable unlinkedDrawingLogRelations()
        {
            if (UnlinkedDrawingLogRelations.Cache.Cached.Empty_())
            {
                var relations = GetUnlinkedDrawingLogRelations();
                InitializeRelationsInCache(relations, UnlinkedDrawingLogRelations.Cache);
            }
            return UnlinkedDrawingLogRelations.Cache.Cached;
        }

        [PXUIField(DisplayName = "Link",
            MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select)]
        [PXButton(CommitChanges = true)]
        public virtual void link()
        {
            if (UnlinkedDrawingLogRelations.AskExt((graph, viewName) =>
                ClearCache<UnlinkedDrawingLogRelation>(), true).IsPositive())
            {
                LinkToEntity();
            }
        }

        [PXButton(CommitChanges = true)]
        [PXUIField(DisplayName = "Unlink",
            MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select)]
        public virtual void unLink()
        {
            var selectedRelations = GetSelectedRelations(LinkedDrawingLogRelations)
                .Select(Mapper.Map<DrawingLogRelation>).ToList();
            UnLinkRelations(selectedRelations);
        }

        [PXUIField(DisplayName = "Link")]
        [PXButton(CommitChanges = true)]
        public void linkEntity()
        {
            LinkToEntity();
        }

        public virtual void _(Events.RowSelected<DrawingLog> args)
        {
            var isDrawingLogSaved = Base.IsDrawingLogSaved(args.Row);
            var isAnyRelationSelected = GetSelectedRelations(LinkedDrawingLogRelations).Any();
            Link.SetEnabled(isDrawingLogSaved);
            Unlink.SetEnabled(isDrawingLogSaved && isAnyRelationSelected);
        }

        public virtual void _(Events.FieldUpdating<LinkedDrawingLogRelation.selected> args)
        {
            if (args.NewValue != null)
            {
                args.Cache.SetValue<LinkedDrawingLogRelation.selected>(LinkedDrawingLogRelations.Cache.Current,
                    args.NewValue);
            }
        }

        public virtual void _(Events.FieldUpdating<UnlinkedDrawingLogRelation.selected> args)
        {
            if (args.NewValue != null)
            {
                args.Cache.SetValue<UnlinkedDrawingLogRelation.selected>(UnlinkedDrawingLogRelations.Cache.Current,
                    args.NewValue);
            }
        }

        public virtual void _(Events.RowInserting<UnlinkedDrawingLogRelation> args)
        {
            args.Cancel = true;
        }

        public virtual void _(Events.RowInserting<LinkedDrawingLogRelation> args)
        {
            args.Cancel = true;
        }

        protected override DrawingLog GetCurrentDrawingLog()
        {
            return Base.CurrentDrawingLog.Current;
        }

        protected override WebDialogResult ShowConfirmationDialog(string message)
        {
            return Base.DrawingLog.Ask(SharedMessages.Warning, message, MessageButtons.OKCancel);
        }

        private void LinkToEntity()
        {
            var selectedUnLinkedRelations = GetSelectedRelations(UnlinkedDrawingLogRelations);
            LinkRequestsForInformation(selectedUnLinkedRelations);
            LinkProjectIssues(selectedUnLinkedRelations);
            ClearCache<UnlinkedDrawingLogRelation>();
        }

        private void LinkProjectIssues(IEnumerable<UnlinkedDrawingLogRelation> selectedUnLinkedRelations)
        {
            var selectedProjectIssueIds = GetSelectedDocumentIds(selectedUnLinkedRelations,
                CacheNames.ProjectIssue);
            Base.Select<ProjectIssue>().ToList()
                .Where(projectIssue => projectIssue.NoteID.IsIn(selectedProjectIssueIds))
                .ForEach(LinkProjectIssue);
        }

        private void LinkRequestsForInformation(IEnumerable<UnlinkedDrawingLogRelation> selectedUnLinkedRelations)
        {
            var selectedRequestForInformationIds = GetSelectedDocumentIds(selectedUnLinkedRelations,
                CacheNames.RequestForInformation);
            Base.Select<RequestForInformation>().ToList()
                .Where(requestForInformation => requestForInformation.NoteID.IsIn(selectedRequestForInformationIds))
                .ForEach(LinkRequestForInformation);
        }

        private void LinkRequestForInformation(RequestForInformation requestForInformation)
        {
            var requestForInformationDrawingLog = CreateRequestForInformationDrawingLog(requestForInformation);
            RequestForInformationDrawingLog.Insert(requestForInformationDrawingLog);
            var relation = Mapper.Map<LinkedDrawingLogRelation>(requestForInformation);
            LinkedDrawingLogRelations.Cache.SetStatus(relation, PXEntryStatus.Inserted);
        }

        private void LinkProjectIssue(ProjectIssue projectIssue)
        {
            var projectIssueDrawingLog = CreateProjectIssueDrawingLog(projectIssue);
            ProjectIssueDrawingLog.Insert(projectIssueDrawingLog);
            var relation = Mapper.Map<LinkedDrawingLogRelation>(projectIssue);
            LinkedDrawingLogRelations.Cache.SetStatus(relation, PXEntryStatus.Inserted);
        }

        private IEnumerable<UnlinkedDrawingLogRelation> GetUnlinkedDrawingLogRelations()
        {
            var linkedRelations = LinkedDrawingLogRelations.Select()
                .FirstTableItems.Select(Mapper.Map<UnlinkedDrawingLogRelation>);
            var requestsForInformation = GetRequestsForInformation();
            var projectIssues = GetProjectIssues();
            var relations = GetDrawingLogRelation<UnlinkedDrawingLogRelation>(requestsForInformation, projectIssues);
            return relations.Where(relation => linkedRelations
                .All(linkedRelation => linkedRelation.DocumentId != relation.DocumentId));
        }

        private IEnumerable<ProjectIssue> GetProjectIssues()
        {
            return Base.Select<ProjectIssue>()
                .Join(Base.Select<PMProject>(), issue => issue.ProjectId, project => project.ContractID,
                    (issue, project) => issue).Where(issue => issue.Status == ProjectIssueStatusAttribute.Open
                    && issue.ProjectId == Base.DrawingLog.Current.ProjectId);
        }

        private IEnumerable<RequestForInformation> GetRequestsForInformation()
        {
            return Base.Select<RequestForInformation>()
                .Join(Base.Select<PMProject>(), request => request.ProjectId, project => project.ContractID,
                    (request, project) => request).Where(request =>
                    request.Status != RequestForInformationStatusAttribute.ClosedStatus
                    && request.ProjectId == Base.DrawingLog.Current.ProjectId);
        }

        private RequestForInformationDrawingLog CreateRequestForInformationDrawingLog(
            RequestForInformation requestForInformation)
        {
            return new RequestForInformationDrawingLog
            {
                DrawingLogId = Base.CurrentDrawingLog.Current.DrawingLogId,
                RequestForInformationId = requestForInformation.RequestForInformationId
            };
        }

        private ProjectIssueDrawingLog CreateProjectIssueDrawingLog(ProjectIssue projectIssue)
        {
            return new ProjectIssueDrawingLog
            {
                DrawingLogId = Base.CurrentDrawingLog.Current.DrawingLogId,
                ProjectIssueId = projectIssue.ProjectIssueId
            };
        }

        private static List<TEntity> GetSelectedRelations<TEntity>(PXSelectBase<TEntity> dataView)
            where TEntity : class, IBqlTable, IPXSelectable, new()
        {
            return dataView.Select().FirstTableItems.Where(entity => entity.Selected == true).ToList();
        }
    }
}
