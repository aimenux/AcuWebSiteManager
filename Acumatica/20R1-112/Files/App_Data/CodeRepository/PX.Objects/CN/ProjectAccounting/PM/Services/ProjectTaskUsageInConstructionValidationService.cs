using PX.Data;
using PX.Objects.AP;
using PX.Objects.EP;
using PX.Objects.GL;
using PX.Objects.IN;
using PX.Objects.PM;
using PX.Objects.PO;

namespace PX.Objects.CN.ProjectAccounting.PM.Services
{
    public class ProjectTaskUsageInConstructionValidationService : ProjectTaskUsageValidationServiceBase
    {
        protected override bool IsTaskUsedInCostDocument(int? taskId)
        {
            return IsTaskUsed<POLine, POLine.taskID>(taskId)
                || IsTaskUsed<POReceiptLine, POReceiptLine.taskID>(taskId)
                || IsTaskUsed<INTran, INTran.taskID>(taskId)
                || IsTaskUsed<APTran, APTran.taskID>(taskId)
                || IsTaskUsed<EPEquipmentSummary, EPEquipmentSummary.projectTaskID>(taskId)
                || IsTaskUsed<EPEquipmentDetail, EPEquipmentDetail.projectTaskID>(taskId)
                || IsTaskUsed<EPActivityApprove, EPActivityApprove.projectTaskID>(taskId)
                || IsTaskUsed<EPTimeCardSummary, EPTimeCardSummary.projectTaskID>(taskId)
                || IsTaskUsed<TimeCardMaint.EPTimecardDetail,
                    TimeCardMaint.EPTimecardDetail.projectTaskID>(taskId)
                || IsTaskUsed<EPTimeCardItem, EPTimeCardItem.taskID>(taskId)
                || IsTaskUsed<EPExpenseClaimDetails, EPExpenseClaimDetails.taskID>(taskId)
                || IsTaskUsed<PMChangeOrderBudget,
                    PMChangeOrderBudget.projectTaskID, PMChangeOrderBudget.type>(taskId, AccountType.Expense)
                || IsTaskUsed<PMChangeOrderLine, PMChangeOrderLine.taskID>(taskId);
        }
    }
}