using PX.Data;
using PX.Objects.AR;
using PX.Objects.CN.Compliance.CL.Descriptor.Attributes;
using PX.Objects.CS;

namespace PX.Objects.CN.Compliance.AR.CacheExtensions
{
    public sealed class ArPaymentExt : PXCacheExtension<ARPayment>
    {
        [PXString]
        [ComplianceDocumentDisplayName(typeof(ARPayment))]
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