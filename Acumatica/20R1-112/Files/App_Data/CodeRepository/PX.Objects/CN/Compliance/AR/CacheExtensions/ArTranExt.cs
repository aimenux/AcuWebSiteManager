using PX.Data;
using PX.Objects.AR;
using PX.Objects.CN.Compliance.Descriptor;
using PX.Objects.CS;

namespace PX.Objects.CN.Compliance.AR.CacheExtensions
{
    public sealed class ArTranExt : PXCacheExtension<ARTran>
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