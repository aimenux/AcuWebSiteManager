using System;
using PX.Common;
using PX.Data;
using PX.Objects.AR;
using PX.Objects.CN.ProjectAccounting.Descriptor;
using PX.Objects.GL;
using PX.Objects.PM;

namespace PX.Objects.CN.ProjectAccounting.PM.Services
{
    public abstract class ProjectTaskUsageValidationServiceBase : ProjectTaskValidationServiceBase
    {
        public void ValidateProjectTask(PMTask projectTask)
        {
            if (projectTask.Status.IsIn(ProjectTaskStatus.Planned, ProjectTaskStatus.Canceled) &&
                (IsTaskUsedInCostDocument(projectTask.TaskID) || IsTaskUsedInRevenueDocument(projectTask.TaskID)))
            {
                throw new Exception(ProjectAccountingMessages.TaskCannotBeDeleted);
            }
        }

        protected override bool IsTaskUsedInRevenueDocument(int? taskId)
        {
            return IsTaskUsed<PMChangeOrderBudget, PMChangeOrderBudget.projectTaskID,
                PMChangeOrderBudget.type>(taskId, AccountType.Income)
                || IsTaskUsed<ARTran, ARTran.taskID>(taskId)
                || IsTaskUsed<PMProformaProgressLine, PMProformaLine.taskID>(taskId)
                || IsTaskUsed<PMProformaTransactLine, PMProformaLine.taskID>(taskId);
        }
    }
}