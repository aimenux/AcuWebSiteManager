using PX.Data;
using PX.Objects.AR;
using PX.Objects.CN.Common.Descriptor;
using PX.Objects.CN.ProjectAccounting.Descriptor;
using PX.Objects.CN.ProjectAccounting.PM.CacheExtensions;
using PX.Objects.CN.ProjectAccounting.PM.Descriptor;
using PX.Objects.CS;
using PX.Objects.PM;

namespace PX.Objects.CN.ProjectAccounting.AR.GraphExtensions
{
    public class ArCashSaleEntryExt : PXGraphExtension<ARCashSaleEntry>
    {
        public static bool IsActive()
        {
            return PXAccess.FeatureInstalled<FeaturesSet.construction>();
        }

		[PXMergeAttributes(Method = MergeMethod.Append)]
        [PXRestrictor(typeof(Where<PMTask.type, NotEqual<ProjectTaskType.cost>>),
            ProjectAccountingMessages.TaskTypeIsNotAvailable, typeof(PMTask.type))]
        [PXFormula(typeof(Validate<ARTran.inventoryID, ARTran.qty, ARTran.costCodeID, ARTran.curyExtPrice>))]
        protected virtual void ARTran_TaskID_CacheAttached(PXCache cache)
        {
        }
    }
}