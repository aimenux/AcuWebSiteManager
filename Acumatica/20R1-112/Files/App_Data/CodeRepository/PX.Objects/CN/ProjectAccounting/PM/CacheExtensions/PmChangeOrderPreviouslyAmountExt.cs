using PX.Data;
using PX.Objects.CS;
using PX.Objects.PM;

namespace PX.Objects.CN.ProjectAccounting.PM.CacheExtensions
{
    [PXNonInstantiatedExtension]
    public sealed class PmChangeOrderPreviouslyAmountExt : PXCacheExtension<PMChangeOrderPrevioslyAmount>
    {
	    public static bool IsActive() => PXAccess.FeatureInstalled<FeaturesSet.construction>();

        [PXDBInt(IsKey = true, BqlField = typeof(PMChangeOrderBudget.costCodeID))]
        public int? CostCodeID
        {
            get;
            set;
        }

        [PXDBInt(IsKey = true, BqlField = typeof(PMChangeOrderBudget.inventoryID))]
        public int? InventoryID
        {
            get;
            set;
        }
    }
}