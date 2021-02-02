using PX.Data;
using PX.Objects.CN.Common.Descriptor;
using PX.Objects.CN.ProjectAccounting.Descriptor;
using PX.Objects.CN.ProjectAccounting.PM.CacheExtensions;
using PX.Objects.CN.ProjectAccounting.PM.Descriptor;
using PX.Objects.CS;
using PX.Objects.IN;
using PX.Objects.PM;

namespace PX.Objects.CN.ProjectAccounting.PM.GraphExtensions
{
    public class ChangeOrderEntryExt : PXGraphExtension<ChangeOrderEntry>
    {
        public static bool IsActive()
        {
            return PXAccess.FeatureInstalled<FeaturesSet.construction>();
        }

        [PXMergeAttributes(Method = MergeMethod.Merge)]
        [PXRestrictor(typeof(Where<PMTask.type, NotEqual<ProjectTaskType.cost>>),
            ProjectAccountingMessages.TaskTypeIsNotAvailable)]
        [PXRemoveBaseAttribute(typeof(PXDefaultAttribute))]
        [PXDefault(typeof(Search<PMTask.taskID,
            Where<PMTask.projectID, Equal<Current<PMChangeOrderRevenueBudget.projectID>>,
                And<PMTask.isDefault, Equal<True>,
                And<PMTask.type, NotEqual<ProjectTaskType.cost>>>>>))]
        protected virtual void PMChangeOrderRevenueBudget_ProjectTaskID_CacheAttached(PXCache cache)
        {
        }

        [PXMergeAttributes(Method = MergeMethod.Merge)]
        [PXRestrictor(typeof(Where<PMTask.type, NotEqual<ProjectTaskType.revenue>>),
            ProjectAccountingMessages.TaskTypeIsNotAvailable)]
        [PXRemoveBaseAttribute(typeof(PXDefaultAttribute))]
        [PXDefault(typeof(Search<PMTask.taskID,
            Where<PMTask.projectID, Equal<Current<PMChangeOrderCostBudget.projectID>>,
                And<PMTask.isDefault, Equal<True>,
                And<PMTask.type, NotEqual<ProjectTaskType.revenue>>>>>))]
        protected virtual void PMChangeOrderCostBudget_ProjectTaskID_CacheAttached(PXCache cache)
        {
        }

        [PXMergeAttributes(Method = MergeMethod.Merge)]
        [PXRestrictor(typeof(Where<PMTask.type, NotEqual<ProjectTaskType.revenue>>),
            ProjectAccountingMessages.TaskTypeIsNotAvailable)]
        [PXFormula(typeof(Validate<PMChangeOrderLine.costCodeID, PMChangeOrderLine.inventoryID,
            PMChangeOrderLine.description, PMChangeOrderLine.vendorID>))]
        [PXRemoveBaseAttribute(typeof(PXDefaultAttribute))]
        [PXDefault(typeof(Search<PMTask.taskID,
            Where<PMTask.projectID, Equal<Current<PMChangeOrderLine.projectID>>,
                And<PMTask.isDefault, Equal<True>,
                And<PMTask.type, NotEqual<ProjectTaskType.revenue>>>>>))]
        protected virtual void PMChangeOrderLine_TaskID_CacheAttached(PXCache cache)
        {
        }
    }
}