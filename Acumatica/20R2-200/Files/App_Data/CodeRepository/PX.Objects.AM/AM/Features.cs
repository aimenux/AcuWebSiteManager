using PX.Data;
using PX.Objects.CS;

namespace PX.Objects.AM
{
    /// <summary>
    /// Manufacturing features
    /// </summary>
    public static class Features
    {
        public const string MANUFACTURINGFIELDCLASS = "MANUFACTURING";
        public const string MRPFIELDCLASS = "MFGMRP";
        public const string PRODUCTCONFIGURATORFIELDCLASS = "MFGPRODUCTCONFIGURATOR";
        public const string ESTIMATINGFIELDCLASS = "MFGESTIMATING";
        public const string ADVANCEDPLANNINGFIELDCLASS = "MFGADVANCEDPLANNING";
        public const string ECCFIELDCLASS = "MFGECC";
        public const string DATACOLLECTIONFIELDCLASS = "MFGDATACOLLECTION";

        public static bool ManufacturingEnabled()
        {
            return PXAccess.FeatureInstalled<FeaturesSet.manufacturing>();
        }

        public static bool AdvancedPlanningEnabled()
        {
            return PXAccess.FeatureInstalled<FeaturesSet.manufacturingAdvancedPlanning>();
        }

        public static bool MRPEnabled()
        {
            return PXAccess.FeatureInstalled<FeaturesSet.manufacturingMRP>();
        }

        public static bool ProductConfiguratorEnabled()
        {
            return PXAccess.FeatureInstalled<FeaturesSet.manufacturingProductConfigurator>();
        }

        public static bool EstimatingEnabled()
        {
            return PXAccess.FeatureInstalled<FeaturesSet.manufacturingEstimating>();
        }

        public static bool ECCEnabled()
        {
            return PXAccess.FeatureInstalled<FeaturesSet.manufacturingECC>();
        }

        public static bool DataCollectionEnabled()
        {
            return PXAccess.FeatureInstalled<FeaturesSet.manufacturingDataCollection>();
        }
    }
}