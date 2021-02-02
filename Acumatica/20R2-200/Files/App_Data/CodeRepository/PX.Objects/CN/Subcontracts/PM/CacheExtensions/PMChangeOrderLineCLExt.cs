using PX.Data;
using PX.Objects.CN.Subcontracts.PM.Descriptor;
using PX.Objects.CS;
using PX.Objects.PM;
using PmMessages = PX.Objects.CN.Subcontracts.PM.Descriptor.Messages;

namespace PX.Objects.CN.Subcontracts.PM.CacheExtensions
{
    public sealed class PMChangeOrderLineCLExt : PXCacheExtension<PMChangeOrderLine>
    {
        [PXDBString(15)]
        [PXDefault(CommitmentBaseType.PurchaseOrder)]
        [PXUIField(DisplayName = PmMessages.PmChangeOrderLine.CommitmentType)]
        [CommitmentBaseType.List]
        public string CommitmentType
        {
            get;
            set;
        }

        public static bool IsActive()
        {
            return PXAccess.FeatureInstalled<FeaturesSet.construction>();
        }

        public abstract class commitmentType : IBqlField
        {
        }
    }
}