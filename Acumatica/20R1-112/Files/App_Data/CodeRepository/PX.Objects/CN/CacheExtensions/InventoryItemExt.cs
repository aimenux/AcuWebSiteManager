using PX.Data;
using PX.Objects.CN.Subcontracts.SC.DAC;
using PX.Objects.CS;

namespace PX.Objects.CN.CacheExtensions
{
    public sealed class InventoryItemExt : PXCacheExtension<SubcontractInventoryItem>
    {
        [PXMergeAttributes(Method = MergeMethod.Merge)]
        [PXUIField(Visibility = PXUIVisibility.SelectorVisible, Visible = false)]
        public bool? StkItem
        {
            get;
            set;
        }

        [PXBool]
        [PXUIField(DisplayName = "Used in Project")]
        public bool? IsUsedInProject
        {
            get;
            set;
        }

        public static bool IsActive()
        {
            return PXAccess.FeatureInstalled<FeaturesSet.construction>();
        }

        public abstract class isUsedInProject : IBqlField
        {
        }
    }
}