using PX.Data;
using PX.Objects.CN.Subcontracts.PM.Descriptor;
using PX.Objects.CS;
using PX.Objects.PM;
using Messages = PX.Objects.CN.Subcontracts.PM.Descriptor.Messages;

namespace PX.Objects.CN.Subcontracts.PM.CacheExtensions
{
    public sealed class PoLinePmExt : PXCacheExtension<POLinePM>
    {
        [PXString(15)]
        [PXUIField(DisplayName = Messages.PmChangeOrderLine.CommitmentType)]
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