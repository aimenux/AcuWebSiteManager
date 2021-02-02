using PX.Objects.PJ.DrawingLogs.PJ.DAC;
using PX.Objects.PJ.ProjectsIssue.PJ.DAC;
using PX.Objects.PJ.RequestsForInformation.PJ.DAC;
using PX.Objects.CN.ProjectAccounting.PM.Services;

namespace PX.Objects.PJ.ProjectAccounting.PM.Services
{
    public class ProjectTaskUsageInProjectManagementValidationService : ProjectTaskUsageValidationServiceBase
    {
        protected override bool IsTaskUsedInCostDocument(int? taskId)
        {
            return IsTaskUsed<RequestForInformation, RequestForInformation.projectTaskId>(taskId)
                || IsTaskUsed<ProjectIssue, ProjectIssue.projectTaskId>(taskId)
                || IsTaskUsed<DrawingLog, DrawingLog.projectTaskId>(taskId);
        }
    }
}