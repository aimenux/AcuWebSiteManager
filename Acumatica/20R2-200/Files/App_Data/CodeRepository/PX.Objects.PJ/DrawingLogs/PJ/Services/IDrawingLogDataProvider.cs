using System;
using System.Collections.Generic;
using PX.Objects.PJ.DrawingLogs.PJ.DAC;
using PX.Data;

namespace PX.Objects.PJ.DrawingLogs.PJ.Services
{
    public interface IDrawingLogDataProvider
    {
        bool IsAttachedToEntity<TTable, TField>(Guid? fileId)
            where TTable : class, IBqlTable, new()
            where TField : IBqlField;

        IEnumerable<Guid> GetDrawingLogFileIds(IEnumerable<Guid> fileIds);

        IEnumerable<DrawingLog> GetRequestForInformationDrawingLogs(int? requestForInformationId);

        DrawingLogDiscipline GetDiscipline<TDisciplineField>(object disciplineFieldValue)
            where TDisciplineField : IBqlOperand, IBqlField;

        DrawingLogStatus GetStatus(int? statusId);

        IEnumerable<DrawingLog> GetDrawingLogs<TDrawingLogField>(object drawingLogFieldValue)
            where TDrawingLogField : IBqlField;

        DrawingLog GetDrawingLog<TDrawingLogField>(object drawingLogFieldValue)
            where TDrawingLogField : IBqlField;

        DrawingLog GetDrawingLog(Guid? fileId);

        IEnumerable<DrawingLog> GetOriginalDrawingLogWithRevisions(Guid? originalDrawingLogNoteId);

        IEnumerable<int?> GetDisciplineSortOrders();
    }
}