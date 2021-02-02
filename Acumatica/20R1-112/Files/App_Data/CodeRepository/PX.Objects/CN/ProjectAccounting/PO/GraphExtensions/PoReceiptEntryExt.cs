using PX.Data;
using PX.Objects.CN.Common.Descriptor;
using PX.Objects.CN.ProjectAccounting.Descriptor;
using PX.Objects.CN.ProjectAccounting.PM.CacheExtensions;
using PX.Objects.CN.ProjectAccounting.PM.Descriptor;
using PX.Objects.CS;
using PX.Objects.PM;
using PX.Objects.PO;

namespace PX.Objects.CN.ProjectAccounting.PO.GraphExtensions
{
    public class PoReceiptEntryExt : PXGraphExtension<POReceiptEntry>
    {
        public static bool IsActive()
        {
            return PXAccess.FeatureInstalled<FeaturesSet.construction>();
        }

        [PXMergeAttributes(Method = MergeMethod.Append)]
		[PXRestrictor(typeof(Where<PMTask.type, NotEqual<ProjectTaskType.revenue>>),
            ProjectAccountingMessages.TaskTypeIsNotAvailable)]
        [PXFormula(typeof(Validate<POReceiptLine.projectID, POReceiptLine.costCodeID,
            POReceiptLine.inventoryID, POReceiptLine.siteID>))]
        protected virtual void POReceiptLine_TaskID_CacheAttached(PXCache cache)
        {
        }
    }
}