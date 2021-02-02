using PX.Objects.PJ.DrawingLogs.PJ.DAC;
using PX.Objects.PJ.DrawingLogs.PJ.Services;
using PX.Objects.PJ.ProjectManagement.PJ.DAC;
using PX.Data;

namespace PX.Objects.PJ.DrawingLogs.PJ.Descriptor.Attributes
{
    public abstract class ValidateDrawingLogBaseAttribute : PXEventSubscriberAttribute
    {
        protected DrawingLogDataProvider DrawingLogDataProvider;
        protected PXCache Cache;

        public override void CacheAttached(PXCache cache)
        {
            DrawingLogDataProvider = new DrawingLogDataProvider(cache.Graph);
            Cache = cache;
        }

        protected void ValidateFields(DrawingLog drawingLog)
        {
            if (drawingLog.ProjectId.HasValue)
            {
                ValidateProject(drawingLog);
            }
            if (drawingLog.ProjectTaskId.HasValue)
            {
                ValidateProjectTask(drawingLog);
            }
            if (drawingLog.DisciplineId.HasValue)
            {
                ValidateDiscipline(drawingLog);
            }
        }

        protected abstract void ValidateDiscipline(DrawingLog drawingLog);

        protected abstract void ValidateProjectTask(IProjectManagementDocumentBase drawingLog);

        protected abstract void ValidateProject(IProjectManagementDocumentBase drawingLog);
    }
}