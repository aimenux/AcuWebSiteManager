using System;
using PX.Data;
using PX.Objects.CR;

namespace PX.Objects.AM.CacheExtensions
{
    [Serializable]
    public sealed class CRSetupExt : PXCacheExtension<CRSetup>
    {
        public static bool IsActive()
        {
            return PXAccess.FeatureInstalled<CS.FeaturesSet.manufacturingEstimating>() ||
                   PXAccess.FeatureInstalled<CS.FeaturesSet.manufacturingProductConfigurator>();
        }

        #region AMConfigurationEntry
        public abstract class aMConfigurationEntry : PX.Data.BQL.BqlBool.Field<aMConfigurationEntry> { }

        /// <summary>
        /// Indicates if Opportunities allow the user to launch the configuration entry page.
        /// </summary>
        [PXDBBool]
        [PXDefault(false, PersistingCheck = PXPersistingCheck.Nothing)]
        [PXUIField(DisplayName = "Allow Configuration Entry", FieldClass = Features.PRODUCTCONFIGURATORFIELDCLASS)]
        public bool? AMConfigurationEntry { get; set; }
        #endregion
        #region AMEstimateEntry
        public abstract class aMEstimateEntry : PX.Data.BQL.BqlBool.Field<aMEstimateEntry> { }

        /// <summary>
        /// Indicates if opportunities will allow estimates
        /// </summary>
        [PXDBBool]
        [PXDefault(false, PersistingCheck = PXPersistingCheck.Nothing)]
        [PXUIField(DisplayName = "Allow Estimating", FieldClass = Features.ESTIMATINGFIELDCLASS)]
        public bool? AMEstimateEntry { get; set; }
        #endregion
    }
}