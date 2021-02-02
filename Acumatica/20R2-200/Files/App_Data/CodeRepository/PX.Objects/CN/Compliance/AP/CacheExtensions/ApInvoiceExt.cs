using PX.Data;
using PX.Objects.AP;
using PX.Objects.CN.Compliance.CL.Descriptor.Attributes;
using PX.Objects.CS;

namespace PX.Objects.CN.Compliance.AP.CacheExtensions
{
    public sealed class ApInvoiceExt : PXCacheExtension<APInvoice>
    {
        [PXString]
        [ComplianceDocumentDisplayName(typeof(APInvoice))]
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