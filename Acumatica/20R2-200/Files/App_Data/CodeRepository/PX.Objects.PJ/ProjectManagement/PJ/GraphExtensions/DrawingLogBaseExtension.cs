using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using PX.Objects.PJ.DrawingLogs.PJ.DAC;
using PX.Objects.PJ.ProjectManagement.PJ.DAC;
using PX.Common;
using PX.Data;
using PX.Data.BQL;
using PX.Objects.CN.Common.Descriptor;
using PX.Objects.Common;
using PX.Objects.CS;
using PX.SM;

namespace PX.Objects.PJ.ProjectManagement.PJ.GraphExtensions
{
    public abstract class DrawingLogBaseExtension<TGraph, TCache, TDrawingLogReference> : PXGraphExtension<TGraph>
        where TGraph : PXGraph, new()
        where TCache : class, IBqlTable, IProjectManagementDocumentBase, new()
        where TDrawingLogReference : DrawingLogReferenceBase, IBqlTable, new()
    {
        public PXAction<TCache> LinkDrawing;
        public PXAction<TCache> LinkDrawingLogToEntity;
        public PXAction<TCache> UnlinkDrawing;
        public PXAction<TCache> ViewAttachments;
        public PXAction<TCache> ViewAttachment;

        [PXCopyPasteHiddenView]
        public PXSelect<DrawingLog> LinkedDrawingLogs;

        [PXCopyPasteHiddenView]
        public PXSelect<DrawingLog> DrawingLogs;

        [PXCopyPasteHiddenView]
        public PXSelectJoin<UploadFile,
            InnerJoin<UploadFileRevision,
                On<UploadFileRevision.fileID, Equal<UploadFile.fileID>>>> DrawingLogsAttachments;

        [PXCopyPasteHiddenView]
        public PXSelect<TDrawingLogReference> DrawingLogReferences;

        protected abstract string ProjectChangeWarningMessage
        {
            get;
        }

        public static bool IsActive()
        {
            return PXAccess.FeatureInstalled<FeaturesSet.constructionProjectManagement>();
        }

        public IEnumerable linkedDrawingLogs()
        {
            var drawingLogIds = DrawingLogReferences.Select().FirstTableItems.Select(x => x.DrawingLogId);
            return new PXSelect<DrawingLog>(Base).Select().FirstTableItems
                .Where(x => x.DrawingLogId.IsIn(drawingLogIds));
        }

        public IEnumerable drawingLogs()
        {
            var existingDrawingLogs = LinkedDrawingLogs.Select().FirstTableItems;
            return GetDrawingLogs().Except(existingDrawingLogs);
        }

        public IEnumerable drawingLogsAttachments()
        {
            var selectedDrawingLogIds = GetSelectedLinkedDrawingLogNoteIds().ToArray();
            if (selectedDrawingLogIds.Any())
            {
                return new PXSelectJoin<UploadFile,
                    InnerJoin<UploadFileRevision, On<UploadFileRevision.fileID, Equal<UploadFile.fileID>>,
                    InnerJoin<NoteDoc, On<NoteDoc.fileID, Equal<UploadFile.fileID>>,
                    InnerJoin<DrawingLog, On<DrawingLog.noteID, Equal<NoteDoc.noteID>>>>>,
                    Where<NoteDoc.noteID, In<Required<NoteDoc.noteID>>>>(Base).Select(selectedDrawingLogIds);
            }
            return new PXResultset<UploadFile>();
        }

        [PXOverride]
        public void Persist(Action baseHandler)
        {
            if (Base.GetPrimaryCache().Current is TCache entity && IsUnlinkDrawingLogsNeeded(entity))
            {
                if (!IsDeletingLinksConfirmed(ProjectChangeWarningMessage))
                {
                    return;
                }
                UnlinkDrawingLogsByProject(entity.ProjectId, entity.ProjectTaskId);
            }
            baseHandler();
        }

        [PXUIField(DisplayName = "Link Drawing",
            MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select)]
        [PXButton(CommitChanges = true)]
        public virtual void linkDrawing()
        {
            if (DrawingLogs.AskExt((graph, viewName) => ClearDrawingLogsCache(), true).IsPositive())
            {
                LinkToEntity();
            }
        }

        [PXUIField(DisplayName = "View Attachments",
            MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select)]
        [PXButton(CommitChanges = true)]
        public virtual void viewAttachments()
        {
            DrawingLogsAttachments.AskExt(true);
        }

        [PXButton(CommitChanges = true)]
        [PXUIField]
        public virtual void viewAttachment()
        {
            if (DrawingLogsAttachments.Current == null)
            {
                return;
            }
            var graph = PXGraph.CreateInstance<UploadFileMaintenance>();
            var fileInfo = graph.GetFile(DrawingLogsAttachments.Current.FileID.GetValueOrDefault());
            throw new PXRedirectToFileException(fileInfo.UID, true);
        }

        [PXUIField]
        [PXButton(CommitChanges = true)]
        public void linkDrawingLogToEntity()
        {
            LinkToEntity();
        }

        [PXButton]
        [PXUIField(DisplayName = "Unlink Drawing",
            MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select)]
        public virtual void unlinkDrawing()
        {
            var selectedDrawingLogs = GetSelectedLinkedDrawingLogs().ToList();
            var selectedDrawingLogIds = selectedDrawingLogs.Select(y => y.DrawingLogId);
            DeleteDrawingLogReferences(selectedDrawingLogIds);
        }

        public virtual void _(Events.RowSelected<DrawingLog> args)
        {
            PXUIFieldAttribute.SetEnabled(args.Cache, null, false);
            PXUIFieldAttribute.SetEnabled<DrawingLog.selected>(args.Cache, null, true);
        }

        public virtual void _(Events.RowSelected<UploadFile> args)
        {
            PXUIFieldAttribute.SetEnabled(args.Cache, null, false);
        }

        public virtual void _(Events.RowSelected<TCache> args)
        {
            var entity = args.Row;
            if (entity == null)
            {
                return;
            }
            LinkDrawing.SetEnabled(IsLinkDrawingActionEnabled(entity));
            ViewAttachments.SetEnabled(IsViewAttachmentActionEnabled(entity));
            UnlinkDrawing.SetEnabled(IsUnlinkDrawingActionEnabled(entity));
        }

        public void InsertDrawingLogLink(int? drawingLogId)
        {
            var drawingLogLink = CreateDrawingLogReference(drawingLogId);
            DrawingLogReferences.Cache.Insert(drawingLogLink);
        }

        public virtual void _(Events.RowPersisting<TDrawingLogReference> args)
        {
            if (args.Row != null && Base.GetPrimaryCache().Current != null)
            {
                SetReferenceEntityId(args.Row);
            }
        }

        public abstract IEnumerable drawingLogReferences();

        protected abstract void SetReferenceEntityId(TDrawingLogReference reference);

        protected abstract DrawingLogReferenceBase CreateDrawingLogReference(int? drawingLogId);

        protected abstract bool IsLinkDrawingActionEnabled(TCache entity);

        private bool IsUnlinkDrawingLogsNeeded(TCache entity)
        {
            return entity != null && Base.GetPrimaryCache().GetOriginal(entity) is TCache originalEntity
                && ShouldUnlinkDrawingLogsByProject(entity, originalEntity);
        }

        private bool IsUnlinkDrawingActionEnabled(TCache entity)
        {
            return IsLinkDrawingActionEnabled(entity) && GetSelectedLinkedDrawingLogs().Any();
        }

        private bool IsViewAttachmentActionEnabled(TCache entity)
        {
            return Base.GetPrimaryCache().GetStatus(entity) != PXEntryStatus.Inserted &&
                GetSelectedLinkedDrawingLogs().Any();
        }

        private void ClearDrawingLogsCache()
        {
            DrawingLogs.Cache.ClearQueryCache();
            DrawingLogs.Cache.Clear();
        }

        private void LinkToEntity()
        {
            var drawingLogs = DrawingLogs.Select().FirstTableItems.ToList();
            var selectedDrawingLogIds = drawingLogs.Where(x => x.Selected == true).Select(x => x.DrawingLogId);
            ClearDrawingLogsCache();
            var selectedDrawingLogs = drawingLogs.Where(x => x.DrawingLogId.IsIn(selectedDrawingLogIds));
            selectedDrawingLogs.ForEach(InsertDrawingLogLink);
        }

        private void InsertDrawingLogLink(DrawingLog drawingLog)
        {
            drawingLog.Selected = false;
            InsertDrawingLogLink(drawingLog.DrawingLogId);
        }

        private IEnumerable<Guid?> GetSelectedLinkedDrawingLogNoteIds()
        {
            return GetSelectedLinkedDrawingLogs().Select(x => x.NoteID);
        }

        private IEnumerable<DrawingLog> GetSelectedLinkedDrawingLogs()
        {
            return LinkedDrawingLogs.Select().FirstTableItems.Where(x => x.Selected == true);
        }

        private IEnumerable<DrawingLog> GetDrawingLogs()
        {
            var entity = Base.GetPrimaryCache().Current as TCache;
            var query = new PXSelect<DrawingLog,
                Where<DrawingLog.projectId.IsEqual<P.AsInt>>>(Base);
            if (entity?.ProjectTaskId != null)
            {
                query.WhereAnd<Where<DrawingLog.projectTaskId.IsEqual<P.AsInt>
                    .Or<DrawingLog.projectTaskId.IsNull>>>();
            }
            return query.SelectMain(entity?.ProjectId, entity?.ProjectTaskId);
        }

        private void UnlinkDrawingLogsByProject(int? projectId, int? projectTaskId)
        {
            var drawingLogToUnlinkIds = LinkedDrawingLogs.Select().FirstTableItems
                .Where(dl => dl.ProjectId != projectId || projectTaskId.HasValue && dl.ProjectTaskId != projectTaskId)
                .Select(dl => dl.DrawingLogId);
            DeleteDrawingLogReferences(drawingLogToUnlinkIds);
        }

        private void DeleteDrawingLogReferences(IEnumerable<int?> drawingLogToUnlinkIds)
        {
            var references = DrawingLogReferences.Select().FirstTableItems
                .Where(dlr => drawingLogToUnlinkIds.Contains(dlr.DrawingLogId));
            references.ForEach(r => DrawingLogReferences.Cache.Delete(r));
        }

        private bool ShouldUnlinkDrawingLogsByProject(TCache entity, TCache originalEntity)
        {
            return entity.ProjectId != null && entity.ProjectId != originalEntity.ProjectId
                && LinkedDrawingLogs.Any();
        }

        private bool IsDeletingLinksConfirmed(string message)
        {
            return LinkedDrawingLogs.Ask(SharedMessages.Warning, message, MessageButtons.YesNo).IsPositive();
        }
    }
}