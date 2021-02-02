using PX.Data;
using PX.Objects.AP;
using PX.Objects.CN.Common.Descriptor;
using PX.Objects.CN.ProjectAccounting.Descriptor;
using PX.Objects.CS;
using PX.Objects.GL;
using PX.Objects.IN;
using PX.Objects.PM;

namespace PX.Objects.CN.ProjectAccounting.AP.CacheExtensions
{
    public sealed class VendorExt : PXCacheExtension<Vendor>
    {
        [PXDBInt]
        [CostCodeDimensionSelector(null, null, null, null, false)]
        [PXUIField(DisplayName = "Cost Code")]
        public int? VendorDefaultCostCodeId
        {
            get;
            set;
        }

        [PXDBInt]
        [PXSelector(typeof(Search2<InventoryItem.inventoryID,
            InnerJoin<Account, On<Account.accountID, Equal<InventoryItem.cOGSAcctID>>,
            InnerJoin<PMAccountGroup, On<PMAccountGroup.groupID, Equal<Account.accountGroupID>>>>,
            Where<PMAccountGroup.type, Equal<AccountType.expense>,
                And<InventoryItem.stkItem, Equal<False>>>>), SubstituteKey = typeof(InventoryItem.inventoryCD))]
        [PXUIField(DisplayName = BusinessMessages.InventoryID)]
        public int? VendorDefaultInventoryId
        {
            get;
            set;
        }

        public static bool IsActive()
        {
            return PXAccess.FeatureInstalled<FeaturesSet.construction>();
        }

        public abstract class vendorDefaultInventoryId : IBqlField
        {
        }

        public abstract class vendorDefaultCostCodeId : IBqlField
        {
        }
    }
}