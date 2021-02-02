using System;
using System.Collections.Generic;
using System.Linq;
using PX.Objects.PJ.DrawingLogs.Descriptor;
using PX.Objects.PJ.DrawingLogs.PJ.DAC;
using PX.Objects.PJ.DrawingLogs.PJ.Services;
using PX.Data;
using PX.Objects.CN.Common.Extensions;
using PX.Objects.CS;

namespace PX.Objects.PJ.DrawingLogs.PJ.GraphExtensions.Revisions
{
    public abstract class DrawingLogRevisionsBaseExtension<TGraph> : PXGraphExtension<TGraph>
        where TGraph : PXGraph, new()
    {
        public PXSelectJoin<DrawingLogView,
            InnerJoin<DrawingLogRevision,
                On<DrawingLog.drawingLogId, Equal<DrawingLogRevision.drawingLogRevisionId>>>,
            Where<DrawingLogRevision.drawingLogId, Equal<Current<DrawingLog.drawingLogId>>>> Revisions;

        protected DrawingLogDataProvider DrawingLogDataProvider;

        public static bool IsActive()
        {
            return PXAccess.FeatureInstalled<FeaturesSet.constructionProjectManagement>();
        }

        public override void Initialize()
        {
            DrawingLogDataProvider = new DrawingLogDataProvider(Base);
        }

        public virtual void _(Events.RowPersisting<DrawingLog> args)
        {
            if (args.Row is DrawingLog drawingLog)
            {
                UpdateRevisionDrawingLogsIfRequired(args.Cache, drawingLog);
            }
        }

        public virtual void _(Events.RowSelected<DrawingLog> args)
        {
            if (args.Row is DrawingLog drawingLog)
            {
                var isRevisionDrawingLog = drawingLog.OriginalDrawingId != null;
                PXDefaultAttribute.SetPersistingCheck<DrawingLog.revision>(args.Cache, drawingLog,
                    GetPersistingCheckState(isRevisionDrawingLog));
                if (isRevisionDrawingLog)
                {
                    DisableDrawingLogFields(args.Cache, drawingLog);
                }
            }
        }

        protected static void UpdateFields(DrawingLog originalDrawingLog, DrawingLog drawingLog)
        {
            drawingLog.ProjectId = originalDrawingLog.ProjectId;
            drawingLog.ProjectTaskId = originalDrawingLog.ProjectTaskId;
            drawingLog.DisciplineId = originalDrawingLog.DisciplineId;
            drawingLog.Number = originalDrawingLog.Number;
        }

        protected void CheckRevisionsAvailability(Guid? noteId)
        {
            if (DoesDrawingLogHaveAnyRevision(noteId))
            {
                throw new Exception(DrawingLogMessages.DrawingLogWithRevisionCannotBeDeleted);
            }
        }

        protected abstract WebDialogResult ShowConfirmationDialog(string message);

        private bool DoesDrawingLogHaveAnyRevision(Guid? drawingLogNoteId)
        {
            return Base.Select<DrawingLog>().Any(d => d.OriginalDrawingId == drawingLogNoteId);
        }

        private static PXPersistingCheck GetPersistingCheckState(bool isFieldRequired)
        {
            return isFieldRequired
                ? PXPersistingCheck.NullOrBlank
                : PXPersistingCheck.Nothing;
        }

        private static void DisableDrawingLogFields(PXCache cache, DrawingLog drawingLog)
        {
            PXUIFieldAttribute.SetEnabled<DrawingLog.projectTaskId>(cache, drawingLog, false);
            PXUIFieldAttribute.SetEnabled<DrawingLog.disciplineId>(cache, drawingLog, false);
            PXUIFieldAttribute.SetEnabled<DrawingLog.number>(cache, drawingLog, false);
        }

        private void UpdateRevisionDrawingLogsIfRequired(PXCache cache, DrawingLog drawingLog)
        {
            var revisionDrawingLogs =
                DrawingLogDataProvider.GetDrawingLogs<DrawingLog.originalDrawingId>(drawingLog.NoteID).ToList();
            if (ShouldUpdateRevisionDrawingLogs(cache, drawingLog, revisionDrawingLogs))
            {
                UpdateRevisionDrawingLogs(drawingLog, revisionDrawingLogs);
            }
        }

        private void UpdateRevisionDrawingLogs(DrawingLog drawingLog, List<DrawingLog> revisionDrawingLogs)
        {
            foreach (var revisionDrawingLog in revisionDrawingLogs)
            {
                UpdateFields(drawingLog, revisionDrawingLog);
                Base.Caches<DrawingLog>().PersistUpdated(revisionDrawingLog);
            }
        }

        private bool ShouldUpdateRevisionDrawingLogs(PXCache cache, DrawingLog drawingLog,
            List<DrawingLog> revisionDrawingLogs)
        {
            var originalDrawingLog = (DrawingLog) cache.GetOriginal(drawingLog);
            var checkRevisionsRestrictions = !Base.HasErrors() && originalDrawingLog != null &&
                revisionDrawingLogs.Any() && drawingLog.OriginalDrawingId == null;
            if (!checkRevisionsRestrictions)
            {
                return false;
            }
            return (originalDrawingLog.ProjectId != drawingLog.ProjectId ||
                    originalDrawingLog.ProjectTaskId != drawingLog.ProjectTaskId ||
                    originalDrawingLog.DisciplineId != drawingLog.DisciplineId ||
                    originalDrawingLog.Number != drawingLog.Number) &&
                ShowConfirmationDialog(DrawingLogMessages.RevisionWillBeUpdated).IsPositive();
        }
    }
}