using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using AutoMapper;
using PX.Objects.PJ.Common.Descriptor;
using PX.Objects.PJ.DrawingLogs.Descriptor;
using PX.Objects.PJ.DrawingLogs.PJ.DAC;
using PX.Objects.PJ.ProjectManagement.PJ.DAC;
using PX.Objects.PJ.ProjectsIssue.PJ.DAC;
using PX.Objects.PJ.RequestsForInformation.PJ.DAC;
using PX.Objects.PJ.RequestsForInformation.PJ.Descriptor.Attributes;
using PX.Api;
using PX.Common;
using PX.Data;
using PX.Objects.CN.Common.Extensions;
using PX.Objects.Common;
using PX.Objects.CS;
using PX.Objects.PJ.Common.Mappers;
using PX.Objects.PM;

namespace PX.Objects.PJ.DrawingLogs.PJ.GraphExtensions.Relations
{
    public abstract class DrawingLogRelationsBaseExtension<TGraph> : PXGraphExtension<TGraph>
        where TGraph : PXGraph, new()
    {
        [PXCopyPasteHiddenView]
        public PXSelect<LinkedDrawingLogRelation> LinkedDrawingLogRelations;

        public PXSelect<RequestForInformationDrawingLog> RequestForInformationDrawingLog;
        public PXSelect<ProjectIssueDrawingLog> ProjectIssueDrawingLog;

        private IMapper mapper;

        protected IMapper Mapper => mapper ?? (mapper = CreateMapper());

        public static bool IsActive()
        {
            return PXAccess.FeatureInstalled<FeaturesSet.constructionProjectManagement>();
        }

        public IEnumerable linkedDrawingLogRelations()
        {
            var relations = GetLinkedDrawingLogRelations();
            InitializeRelationsInCache(relations, LinkedDrawingLogRelations.Cache);
            return LinkedDrawingLogRelations.Cache.Cached;
        }

        public virtual void _(Events.RowPersisted<DrawingLog> args)
        {
            var drawingLog = args.Row;
            if (args.Cache.GetOriginal(drawingLog) is DrawingLog originalDrawingLog)
            {
                UnLinkRelationsIfRequired(drawingLog, originalDrawingLog);
            }
        }

        protected void UnLinkRelations(List<DrawingLogRelation> selectedRelations)
        {
            DeleteDrawingLogReference(selectedRelations, GetRequestForInformationReferences);
            DeleteDrawingLogReference(selectedRelations, GetProjectIssueReferences);
            selectedRelations.ForEach(DeleteRelationsFromCache);
            Base.Caches<DrawingLog>().IsDirty = true;
        }

        protected void ClearCache<TEntity>()
        {
            Base.Caches[typeof(TEntity)].Clear();
            Base.Caches[typeof(TEntity)].ClearQueryCache();
        }

        protected static void InitializeRelationsInCache<TEntity>(IEnumerable<TEntity> relations, PXCache cache)
        {
            relations.ForEach(entity => cache.SetStatus(entity, PXEntryStatus.Held));
        }

        protected static IEnumerable<Guid?> GetSelectedDocumentIds(IEnumerable<DrawingLogRelation> selectedRelations,
            string documentType)
        {
            return selectedRelations
                .Where(relation => relation.DocumentType == documentType)
                .Select(relation => relation.DocumentId);
        }

        protected List<TEntity> GetDrawingLogRelation<TEntity>(
            IEnumerable<RequestForInformation> requestForInformation, IEnumerable<ProjectIssue> projectIssues)
        {
            var linkedRelations = requestForInformation.Select(Mapper.Map<TEntity>).ToList();
            linkedRelations.AddRange(projectIssues.Select(Mapper.Map<TEntity>));
            return linkedRelations;
        }

        protected abstract DrawingLog GetCurrentDrawingLog();

        protected abstract WebDialogResult ShowConfirmationDialog(string message);

        private static IMapper CreateMapper()
        {
            var configuration = new MapperConfiguration(cfg => cfg.AddProfile<DrawingLogRelationMapperProfile>());
            return configuration.CreateMapper();
        }

        private IEnumerable<RequestForInformationDrawingLog> GetRequestForInformationReferences(
            IEnumerable<DrawingLogRelation> selectedRelations)
        {
            var selectedDocumentIds = GetSelectedDocumentIds(selectedRelations,
                CacheNames.RequestForInformation);
            return GetRequestForInformationReferences(selectedDocumentIds);
        }

        private IEnumerable<ProjectIssueDrawingLog> GetProjectIssueReferences(
            IEnumerable<DrawingLogRelation> selectedLinkedRelations)
        {
            var selectedDocumentIds = GetSelectedDocumentIds(selectedLinkedRelations,
                CacheNames.ProjectIssue);
            return GetProjectIssueReferences(selectedDocumentIds);
        }

        private IEnumerable<RequestForInformationDrawingLog> GetRequestForInformationReferences(
            IEnumerable<Guid?> documentIds)
        {
            return RequestForInformationDrawingLog.Select().FirstTableItems
                .Join(Base.Select<RequestForInformation>(), reference => reference.RequestForInformationId,
                    request => request.RequestForInformationId, (reference, request) => new
                    {
                        reference,
                        NoteId = request.NoteID
                    })
                .Where(jointReference => jointReference.reference.DrawingLogId == GetCurrentDrawingLog().DrawingLogId)
                .ToList().Where(jointReference => jointReference.NoteId.IsIn(documentIds))
                .Select(jointReference => jointReference.reference);
        }

        private IEnumerable<RequestForInformation> GetLinkedRequestsForInformation()
        {
            return RequestForInformationDrawingLog.Select().FirstTableItems
                .Where(reference => reference.DrawingLogId == GetCurrentDrawingLog().DrawingLogId)
                .Join(Base.Select<RequestForInformation>(), reference => reference.RequestForInformationId,
                    request => request.RequestForInformationId, (reference, request) => request)
                .Join(Base.Select<PMProject>(), request => request.ProjectId,
                    project => project.ContractID, (request, project) => request)
                .Where(request => request.Status != RequestForInformationStatusAttribute.ClosedStatus);
        }

        private IEnumerable<LinkedDrawingLogRelation> GetLinkedDrawingLogRelations()
        {
            var existingRelations = GetExistingLinkedDrawingLogRelations();
            LinkedDrawingLogRelations.Cache.Cached.Select<LinkedDrawingLogRelation>()
                .Where(relation => LinkedDrawingLogRelations.Cache.GetStatus(relation) == PXEntryStatus.Held)
                .ForEach(relation => RefreshCachedRelation(existingRelations, relation));
            return existingRelations;
        }

        private void RefreshCachedRelation(IEnumerable<LinkedDrawingLogRelation> existingRelations,
            LinkedDrawingLogRelation linkedRelation)
        {
            var existingRelation = existingRelations
                .SingleOrDefault(relation => relation.DocumentId == linkedRelation.DocumentId);
            if (existingRelation != null)
            {
                Mapper.Map(existingRelation, linkedRelation);
            }
            else
            {
                LinkedDrawingLogRelations.Cache.Remove(linkedRelation);
            }
        }

        private List<LinkedDrawingLogRelation> GetExistingLinkedDrawingLogRelations()
        {
            if (GetCurrentDrawingLog() == null)
            {
                return new List<LinkedDrawingLogRelation>();
            }
            var requestsForInformation = GetLinkedRequestsForInformation();
            var projectIssues = GetLinkedProjectIssueDrawingLog();
            return GetDrawingLogRelation<LinkedDrawingLogRelation>(requestsForInformation, projectIssues);
        }

        private void DeleteDrawingLogReference<TEntity>(IEnumerable<DrawingLogRelation> selectedRelations,
            Func<IEnumerable<DrawingLogRelation>, IEnumerable<TEntity>> getDrawingLogReference)
        {
            var drawingLogReference = getDrawingLogReference(selectedRelations);
            Base.Caches[typeof(TEntity)].DeleteAll((IEnumerable<object>) drawingLogReference);
        }

        private void DeleteRelationsFromCache(DrawingLogRelation relation)
        {
            LinkedDrawingLogRelations.Cache.Remove(relation);
        }

        private IEnumerable<ProjectIssue> GetLinkedProjectIssueDrawingLog()
        {
            return ProjectIssueDrawingLog.Select().FirstTableItems
                .Where(reference => reference.DrawingLogId == GetCurrentDrawingLog().DrawingLogId)
                .Join(Base.Select<ProjectIssue>(), reference => reference.ProjectIssueId,
                    issue => issue.ProjectIssueId, (reference, issue) => issue)
                .Join(Base.Select<PMProject>(), issue => issue.ProjectId,
                    project => project.ContractID, (issue, project) => issue)
                .Where(request => request.Status != RequestForInformationStatusAttribute.ClosedStatus);
        }

        private IEnumerable<ProjectIssueDrawingLog> GetProjectIssueReferences(IEnumerable<Guid?> documentIds)
        {
            return ProjectIssueDrawingLog.Select().FirstTableItems
                .Join(Base.Select<ProjectIssue>(), reference => reference.ProjectIssueId,
                    issue => issue.ProjectIssueId, (reference, issue) => new
                    {
                        reference,
                        NoteId = issue.NoteID
                    })
                .Where(jointReference => jointReference.reference.DrawingLogId == GetCurrentDrawingLog().DrawingLogId)
                .ToList().Where(jointReference => jointReference.NoteId.IsIn(documentIds))
                .Select(jointReference => jointReference.reference);
        }

        private void UnLinkRelationsIfRequired(IProjectManagementDocumentBase drawingLog,
            IProjectManagementDocumentBase originalDrawingLog)
        {
            if (ShouldUnlinkRelations(originalDrawingLog, drawingLog))
            {
                var relations = LinkedDrawingLogRelations.Select().FirstTableItems
                    .Select(Mapper.Map<DrawingLogRelation>).ToList();
                UnLinkRelations(relations);
            }
        }

        private bool ShouldUnlinkRelations(IProjectManagementDocumentBase originalDrawingLog,
            IProjectManagementDocumentBase drawingLog)
        {
            if (Base.HasErrors() || originalDrawingLog == null || !LinkedDrawingLogRelations.Any()
                || originalDrawingLog.ProjectId == drawingLog.ProjectId)
            {
                return false;
            }
            return ShowConfirmationDialog(DrawingLogMessages.UnlinkRelationIfProjectChanged).IsPositive();
        }
    }
}
