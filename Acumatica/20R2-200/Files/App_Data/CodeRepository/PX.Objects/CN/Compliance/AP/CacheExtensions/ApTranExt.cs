using PX.Data;
using PX.Objects.AP;
using PX.Objects.CN.Compliance.Descriptor;
using PX.Objects.CS;

namespace PX.Objects.CN.Compliance.AP.CacheExtensions
{
    public sealed class ApTranExt : PXCacheExtension<APTran>
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