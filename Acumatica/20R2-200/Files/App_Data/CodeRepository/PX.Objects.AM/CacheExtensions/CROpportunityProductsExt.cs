using System;
using PX.Data;
using PX.Objects.CR;

namespace PX.Objects.AM.CacheExtensions
{
    [Serializable]
    public sealed class CROpportunityProductsExt : PXCacheExtension<CROpportunityProducts>, IMfgConfigOrderLineExtension
    {
        public static bool IsActive()
        {
            // features in this extension only related to product configurator module
            return PXAccess.FeatureInstalled<CS.FeaturesSet.manufacturingProductConfigurator>();
        }

        #region AMSelected
        public abstract class aMSelected : PX.Data.BQL.BqlBool.Field<aMSelected> { }

        [PXBool]
        [PXUnboundDefault(false)]
        [PXUIField(DisplayName = "Selected")]
        public bool? AMSelected { get; set; }
        #endregion
        #region AMConfigurationID
        public abstract class aMConfigurationID : PX.Data.BQL.BqlString.Field<aMConfigurationID> { }

        [PXDBString]
        [PXUIField(DisplayName = "Configuration ID", Enabled = false, Visible = false)]
        public string AMConfigurationID { get; set; }
        #endregion
        #region IsConfigurable
        public abstract class isConfigurable : PX.Data.BQL.BqlBool.Field<isConfigurable> { }

        [PXBool]
        [PXUIField(DisplayName = "Configurable", Enabled = false)]
        public bool? IsConfigurable => !string.IsNullOrEmpty(AMConfigurationID);

        #endregion
        #region AMParentLineNbr
        public abstract class aMParentLineNbr : PX.Data.BQL.BqlInt.Field<aMParentLineNbr> { }

        [PXDBInt]
        [PXUIField(DisplayName = "Parent Line Nbr.", Visible = false, Enabled = false)]
        public Int32? AMParentLineNbr { get; set; }
        #endregion
        #region AMIsSupplemental
        public abstract class aMIsSupplemental : PX.Data.BQL.BqlBool.Field<aMIsSupplemental> { }

        [PXDBBool]
        [PXDefault(false, PersistingCheck = PXPersistingCheck.Nothing)]
        [PXUIField(DisplayName = "Is Supplemental", Visible = false, Enabled = false)]
        public Boolean? AMIsSupplemental { get; set; }
        #endregion
        #region AMConfigKeyID
        /// <summary>
        /// Configuration key ID which represents the key used/generated from the results of a finished configuration
        /// </summary>
        public abstract class aMConfigKeyID : PX.Data.BQL.BqlString.Field<aMConfigKeyID> { }

        /// <summary>
        /// Configuration key ID which represents the key used/generated from the results of a finished configuration
        /// </summary>
        [PXDBString(120, IsUnicode = true)]
        [PXUIField(DisplayName = "Config. Key", Enabled = false)]
        [PXSelector(typeof(Search<AMConfigurationKeys.keyID,
                Where<AMConfigurationKeys.configurationID, Equal<Current<CROpportunityProductsExt.aMConfigurationID>>>>),
            typeof(AMConfigurationKeys.keyID),
            typeof(AMConfigurationKeys.keyDescription),
            typeof(AMConfigurationKeys.createdDateTime),
            DescriptionField = typeof(AMConfigurationResults.keyDescription),
            ValidateValue = false)]
        public string AMConfigKeyID { get; set; }
        #endregion
    }
}