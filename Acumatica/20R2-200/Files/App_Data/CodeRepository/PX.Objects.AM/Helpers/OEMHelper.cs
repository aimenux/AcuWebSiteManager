using PX.Data;

namespace PX.Objects.AM
{
    /// <summary>
    /// Acumatica OEM helper class for Manufacturing
    /// </summary>
    public static class OEMHelper
    {
        private const string CoreFeatureSet = "PX.Objects.CS.FeaturesSet+";
        private const string MYOBFeatureSet = "MYOB.AdvancedLive.Core.Extensions.CS.DAC.FeaturesSetExtension+";

        public static class MYOBFeatures
        {
            /// <summary>
            /// MYOB - "Basic Inventory Replenishments" feature.
            /// This is a limited display of the full Acumatica set of features found in "Replenishments" (PX.Objects.CS.FeaturesSet+replenishment)
            /// </summary>
            public const string BasicInvReplenish = "basicInvReplenish";
        }

        public static bool FeatureInstalled(string feature)
        {
            if (feature.StartsWith(MYOBFeatureSet))
            {
                return PXAccess.FeatureInstalled(feature.Replace(MYOBFeatureSet, CoreFeatureSet));
            }
            
            if (!feature.StartsWith(CoreFeatureSet))
            {
                return PXAccess.FeatureInstalled($"{CoreFeatureSet}{feature}");
            }

            return PXAccess.FeatureInstalled(feature);
        }
    }
}