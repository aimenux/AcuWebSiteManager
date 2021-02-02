using PX.Data;
using PX.Objects.AR;
using PX.Objects.CN.Common.Extensions;
using PX.Objects.CN.ProjectAccounting.Descriptor;
using PX.Objects.CN.ProjectAccounting.PM.CacheExtensions;
using PX.Objects.CN.ProjectAccounting.PM.Descriptor;
using PX.Objects.GL;
using PX.Objects.PM;
using PmMessages = PX.Objects.PM.Messages;

namespace PX.Objects.CN.ProjectAccounting.PM.Services
{
    public abstract class ProjectTaskTypeUsageValidationServiceBase : ProjectTaskValidationServiceBase
    {
        public void ValidateProjectTaskType(PXCache cache, PMTask projectTask)
        {
            var status = cache.GetStatus(projectTask);
            if (status == PXEntryStatus.Updated)
            {
                if (projectTask.Type == ProjectTaskType.Cost
                    && IsTaskUsedInRevenueDocument(projectTask.TaskID))
                {
                    cache.RaiseException<PMTask.type>(projectTask,
                        string.Format(ProjectAccountingMessages.TaskTypeCannotBeChanged, PmMessages.TaskType_Revenue),
                        projectTask.Type);
                }
                if (projectTask.Type == ProjectTaskType.Revenue
                    && IsTaskUsedInCostDocument(projectTask.TaskID))
                {
                    cache.RaiseException<PMTask.type>(projectTask,
                        string.Format(ProjectAccountingMessages.TaskTypeCannotBeChanged, PmMessages.TaskType_Expense),
                        projectTask.Type);
                }
            }
        }

        protected override bool IsTaskUsedInRevenueDocument(int? taskId)
        {
            return IsTaskUsed<PMBudget, PMBudget.projectTaskID, PMBudget.type>(taskId, AccountType.Income)
                || IsTaskUsed<PMChangeOrderBudget, PMChangeOrderBudget.projectTaskID,
                    PMChangeOrderBudget.type>(taskId, AccountType.Income)
                || IsTaskUsed<ARTran, ARTran.taskID>(taskId)
                || IsTaskUsed<PMProformaProgressLine, PMProformaLine.taskID>(taskId)
                || IsTaskUsed<PMProformaTransactLine, PMProformaLine.taskID>(taskId);
        }
    }
}