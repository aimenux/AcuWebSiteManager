using PX.Objects.PJ.DrawingLogs.Descriptor;
using PX.Objects.PJ.DrawingLogs.PJ.DAC;
using PX.Objects.PJ.ProjectManagement.PJ.DAC;
using PX.Data;
using PX.Objects.CN.Common.Descriptor;
using PX.Objects.CN.Common.Extensions;
using PX.Objects.CN.Common.Services.DataProviders;
using PX.Objects.CN.ProjectAccounting.Descriptor;
using PX.Objects.CN.ProjectAccounting.PM.Services;

namespace PX.Objects.PJ.DrawingLogs.PJ.Descriptor.Attributes
{
    public class ValidateDrawingLogPersistingAttribute : ValidateDrawingLogBaseAttribute, IPXRowPersistingSubscriber
    {
        public void RowPersisting(PXCache cache, PXRowPersistingEventArgs args)
        {
            if (args.Row is DrawingLog drawingLog)
            {
                ValidateFields(drawingLog);
                var drawingLogStatus = DrawingLogDataProvider.GetStatus(drawingLog.StatusId);
                if (drawingLogStatus == null)
                {
                    cache.RaiseException<DrawingLog.statusId>(args.Row,
                        string.Format(SharedMessages.CannotBeFound, DrawingLogLabels.Status));
                }
            }
        }

        protected override void ValidateDiscipline(DrawingLog drawingLog)
        {
            var discipline = DrawingLogDataProvider
                .GetDiscipline<DrawingLogDiscipline.drawingLogDisciplineId>(drawingLog.DisciplineId);
            if (discipline == null)
            {
                var message = string.Format(DrawingLogMessages.NoLongerExists, DrawingLogLabels.Discipline);
                Cache.RaiseException<DrawingLog.disciplineId>(drawingLog, message);
            }
        }

        protected override void ValidateProjectTask(IProjectManagementDocumentBase drawingLog)
        {
            var projectTaskDataProvider = Cache.Graph.GetService<IProjectTaskDataProvider>();
            var projectTask = projectTaskDataProvider.GetProjectTask(Cache.Graph, drawingLog.ProjectTaskId);
            if (projectTask == null)
            {
                var message = string.Format(DrawingLogMessages.NoLongerExists, "Project Task");
                Cache.RaiseException<DrawingLog.projectTaskId>(drawingLog, message);
            }
        }

        protected override void ValidateProject(IProjectManagementDocumentBase drawingLog)
        {
            var projectDataProvider = Cache.Graph.GetService<IProjectDataProvider>();
            var project = projectDataProvider.GetProject(Cache.Graph, drawingLog.ProjectId);
            if (project == null)
            {
                var message = string.Format(DrawingLogMessages.NoLongerExists, DrawingLogLabels.Project);
                Cache.RaiseException<DrawingLog.projectId>(drawingLog, message);
            }
        }
    }
}