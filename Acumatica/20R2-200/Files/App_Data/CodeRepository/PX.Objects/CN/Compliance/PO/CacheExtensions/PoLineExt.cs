using PX.Data;
using PX.Objects.CN.Compliance.Descriptor;
using PX.Objects.CS;
using PX.Objects.PO;

namespace PX.Objects.CN.Compliance.PO.CacheExtensions
{
    public sealed class PoLineExt : PXCacheExtension<POLine>
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