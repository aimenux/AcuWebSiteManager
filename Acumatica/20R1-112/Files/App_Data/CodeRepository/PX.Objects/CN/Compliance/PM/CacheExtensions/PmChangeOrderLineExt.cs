using PX.Data;
using PX.Objects.CN.Compliance.Descriptor;
using PX.Objects.CS;
using PX.Objects.PM;

namespace PX.Objects.CN.Compliance.PM.CacheExtensions
{
    public sealed class PmChangeOrderLineExt : PXCacheExtension<PMChangeOrderLine>
    {
        [PXBool]
        [PXUIField(DisplayName = ComplianceMessages.HasExpiredComplianceDocuments, Visible = false, IsReadOnly = true)]
        public bool? HasExpiredComplianceDocuments
        {
            get;
            set;
        }

        public static bool IsActive()
        {
            return PXAccess.FeatureInstalled<FeaturesSet.construction>();
        }

        public abstract class hasExpiredComplianceDocuments : IBqlField
        {
        }
    }
}