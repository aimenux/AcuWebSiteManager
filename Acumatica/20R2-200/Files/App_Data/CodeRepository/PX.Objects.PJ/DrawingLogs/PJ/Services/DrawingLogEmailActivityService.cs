using PX.Objects.PJ.Common.CacheExtensions;
using PX.Objects.PJ.Common.Services;
using PX.Objects.PJ.DrawingLogs.PJ.DAC;
using PX.Objects.PJ.DrawingLogs.PJ.Graphs;
using PX.Data;
using PX.Objects.CR;

namespace PX.Objects.PJ.DrawingLogs.PJ.Services
{
    public class DrawingLogEmailActivityService : EmailActivityService<DrawingLog>
    {
        public DrawingLogEmailActivityService(DrawingLogEntry graph)
            : base(graph, graph.CurrentDrawingLog.Current.OwnerId)
        {
            Entity = graph.CurrentDrawingLog.Current;
        }

        public PXGraph GetEmailActivityGraph()
        {
            return GetEmailActivityGraph<DrawingLog.noteID>();
        }

        protected override string GetSubject()
        {
            var projectNumber = GetProjectNumber();
            return $"Drawing Log # [{Entity.DrawingLogCd} {projectNumber}] {Entity.Title}";
        }
    }
}
