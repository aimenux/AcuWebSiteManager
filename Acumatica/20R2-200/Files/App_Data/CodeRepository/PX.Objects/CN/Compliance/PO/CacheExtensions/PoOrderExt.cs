using PX.Data;
using PX.Objects.CN.Compliance.CL.Descriptor.Attributes;
using PX.Objects.CS;
using PX.Objects.PO;

namespace PX.Objects.CN.Compliance.PO.CacheExtensions
{
    public sealed class PoOrderExt : PXCacheExtension<POOrder>
    {
        [PXString]
        [ComplianceDocumentDisplayName(typeof(POOrder))]
        public string ClDisplayName
        {
            get;
            set;
        }

        public static bool IsActive()
        {
            return PXAccess.FeatureInstalled<FeaturesSet.construction>();
        }

        public abstract class clDisplayName : IBqlField
        {
        }
    }
}