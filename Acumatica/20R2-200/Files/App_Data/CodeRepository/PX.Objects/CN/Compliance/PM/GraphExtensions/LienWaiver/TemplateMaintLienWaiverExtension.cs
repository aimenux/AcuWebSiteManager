using PX.Data;
using PX.Objects.CS;
using PX.Objects.PM;

namespace PX.Objects.CN.Compliance.PM.GraphExtensions.LienWaiver
{
    public class TemplateMaintLienWaiverExtension : LienWaiverBaseExtension<TemplateMaint>
    {
        public static bool IsActive()
        {
            return PXAccess.FeatureInstalled<FeaturesSet.construction>();
        }
    }
}
