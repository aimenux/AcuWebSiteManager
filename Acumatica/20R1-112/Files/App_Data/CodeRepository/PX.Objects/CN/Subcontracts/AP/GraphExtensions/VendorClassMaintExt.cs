using PX.Data;
using PX.Objects.AP;
using PX.Objects.CN.Common.Descriptor.Attributes;
using PX.Objects.CS;

namespace PX.Objects.CN.Subcontracts.AP.GraphExtensions
{
    public class VendorClassMaintExt : PXGraphExtension<VendorClassMaint>
    {
        public static bool IsActive()
        {
            return PXAccess.FeatureInstalled<FeaturesSet.construction>();
        }

        [PXMergeAttributes(Method = MergeMethod.Merge)]
        [ConstructionReportSelector]
        protected virtual void NotificationSource_ReportID_CacheAttached(PXCache sender)
        {
        }
    }
}