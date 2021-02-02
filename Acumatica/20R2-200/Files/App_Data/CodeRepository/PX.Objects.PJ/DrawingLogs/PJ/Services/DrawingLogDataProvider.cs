using System;
using System.Collections.Generic;
using System.Linq;
using PX.Objects.PJ.DrawingLogs.PJ.DAC;
using PX.Objects.PJ.ProjectManagement.PJ.DAC;
using PX.Objects.PJ.RequestsForInformation.PJ.DAC;
using PX.Data;
using PX.Data.BQL;
using PX.Data.BQL.Fluent;

namespace PX.Objects.PJ.DrawingLogs.PJ.Services
{
    public class DrawingLogDataProvider : IDrawingLogDataProvider
    {
        private readonly PXGraph graph;

        public DrawingLogDataProvider(PXGraph graph)
        {
            this.graph = graph;
        }

        public bool IsAttachedToEntity<TTable, TField>(Guid? fileId)
            where TTable : class, IBqlTable, new()
            where TField : IBqlField
        {
            return new PXSelectJoin<TTable,
                    InnerJoin<NoteDoc,
                        On<NoteDoc.noteID, Equal<TField>>>,
                    Where<NoteDoc.fileID, Equal<Required<NoteDoc.fileID>>>>(graph)
                .SelectSingle(fileId) != null;
        }

        public IEnumerable<Guid> GetDrawingLogFileIds(IEnumerable<Guid> fileIds)
        {
            return new PXSelectJoin<NoteDoc,
                    InnerJoin<DrawingLog,
                        On<NoteDoc.noteID, Equal<DrawingLog.noteID>>>,
                    Where<NoteDoc.fileID, In<Required<NoteDoc.fileID>>>>(graph)
                .Select(fileIds.ToArray()).FirstTableItems.Select(x => x.FileID.GetValueOrDefault());
        }

        public IEnumerable<DrawingLog> GetRequestForInformationDrawingLogs(int? requestForInformationId)
        {
            return new PXSelectJoin<DrawingLog,
                    InnerJoin<RequestForInformationDrawingLog,
                        On<DrawingLog.drawingLogId, Equal<RequestForInformationDrawingLog.drawingLogId>>>,
                    Where<RequestForInformationDrawingLog.requestForInformationId,
                        Equal<Required<RequestForInformationDrawingLog.requestForInformationId>>>>(graph)
                .Select(requestForInformationId).FirstTableItems;
        }

        public DrawingLogDiscipline GetDiscipline<TDisciplineField>(object disciplineFieldValue)
            where TDisciplineField : IBqlOperand, IBqlField
        {
            return new PXSelect<DrawingLogDiscipline,
                    Where<TDisciplineField,
                        Equal<Required<TDisciplineField>>>>(graph)
                .SelectSingle(disciplineFieldValue);
        }

        public DrawingLogStatus GetStatus(int? statusId)
        {
            return new SelectFrom<DrawingLogStatus>
                .Where<DrawingLogStatus.statusId.IsEqual<P.AsInt>>.View(graph).Select(statusId);
        }

        public IEnumerable<DrawingLog> GetDrawingLogs<TDrawingLogField>(object drawingLogFieldValue)
            where TDrawingLogField : IBqlField
        {
            return new PXSelect<DrawingLog,
                    Where<TDrawingLogField, Equal<Required<TDrawingLogField>>>>(graph)
                .Select(drawingLogFieldValue).FirstTableItems;
        }

        public DrawingLog GetDrawingLog<TDrawingLogField>(object drawingLogFieldValue)
            where TDrawingLogField : IBqlField
        {
            return new PXSelect<DrawingLog,
                    Where<TDrawingLogField, Equal<Required<TDrawingLogField>>>>(graph)
                .SelectSingle(drawingLogFieldValue);
        }

        public DrawingLog GetDrawingLog(Guid? fileId)
        {
            return new PXSelectJoin<DrawingLog,
                InnerJoin<NoteDoc, On<DrawingLog.noteID, Equal<NoteDoc.noteID>>>,
                Where<NoteDoc.fileID, Equal<Required<NoteDoc.fileID>>>>(graph).SelectSingle(fileId);
        }

        public IEnumerable<DrawingLog> GetOriginalDrawingLogWithRevisions(Guid? originalDrawingLogNoteId)
        {
            return new PXSelect<DrawingLog,
                    Where<DrawingLog.originalDrawingId, Equal<Required<DrawingLog.noteID>>,
                        Or<DrawingLog.noteID, Equal<Required<DrawingLog.noteID>>>>>(graph)
                .Select(originalDrawingLogNoteId, originalDrawingLogNoteId).FirstTableItems;
        }

        public virtual IEnumerable<int?> GetDisciplineSortOrders()
        {
            return new PXSelect<DrawingLogDiscipline>(graph).Select().FirstTableItems.Select(d => d.SortOrder);
        }
    }
}