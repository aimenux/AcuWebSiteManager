using System;
using System.Linq;
using PX.Objects.PJ.DailyFieldReports.PJ.DAC;
using PX.Data;
using PX.Data.BQL;
using PX.Data.BQL.Fluent;

namespace PX.Objects.PJ.DailyFieldReports.SM.Services
{
    public class FilesDataProvider : IFilesDataProvider
    {
        private readonly PXGraph graph;

        public FilesDataProvider(PXGraph graph)
        {
            this.graph = graph;
        }

        public bool DoesFileHaveRelatedHistoryRevision(Guid? fileId)
        {
            return SelectFrom<DailyFieldReportHistory>
                .InnerJoin<NoteDoc>.On<NoteDoc.noteID.IsEqual<DailyFieldReportHistory.noteID>>
                .Where<NoteDoc.fileID.IsEqual<P.AsGuid>>.View.Select(graph, fileId).Any();
        }
    }
}
