using Customization;
using PX.Data;
using PX.Objects.CS;

namespace PX.Objects.AM.Upgrade
{
    internal sealed class UpgradeConfigResultsOption : UpgradeProcessVersionBase
    {
        public UpgradeConfigResultsOption(UpgradeProcess upgradeGraph, CustomizationPlugin plugin) : base(upgradeGraph, plugin)
        {
        }

        public UpgradeConfigResultsOption(UpgradeProcess upgradeGraph) : base(upgradeGraph)
        {
        }

        public override int Version => UpgradeVersions.Version2019R2Ver08;

        protected override bool ProcessCompletedInPreviousVersion(int upgradeFrom)
        {
            return upgradeFrom.BetweenInclusive(UpgradeVersions.Version2018R2Ver82, UpgradeVersions.MaxVersionNumbers.Version2018R2) 
                   || upgradeFrom.BetweenInclusive(UpgradeVersions.Version2019R1Ver52, UpgradeVersions.MaxVersionNumbers.Version2019R1);
        }

        public override void ProcessTables()
        {
            //SetMaterialType();
        }

        /// <summary>
        /// Starting with this version <see cref="AMConfigResultsOption"/> MaterialType is a bound field (previously unbound).
        /// This change was made to avoid unnecessary sub query to <see cref="AMConfigurationOption"/> for MaterialType
        /// </summary>
        private void SetMaterialType()
        {
            PXUpdateJoin<
            Set<AMConfigResultsOption.materialType, AMConfigurationOption.materialType>,
            AMConfigResultsOption,
            InnerJoin<AMConfigurationOption,
                On<AMConfigResultsOption.configurationID, Equal<AMConfigurationOption.configurationID>,
                    And<AMConfigResultsOption.revision, Equal<AMConfigurationOption.revision>,
                    And<AMConfigResultsOption.featureLineNbr, Equal<AMConfigurationOption.configFeatureLineNbr>,
                    And<AMConfigResultsOption.optionLineNbr, Equal<AMConfigurationOption.lineNbr>>>>>>,
                Where<AMConfigResultsOption.materialType, Equal<int0>>>
                .Update(_upgradeGraph);

            /**************************************************************************************************
                UPDATE [AMConfigResultsOption]
                SET [AMConfigResultsOption].[MaterialType] = [AMConfigurationOption].[MaterialType]
                FROM [AMConfigResultsOption] [AMConfigResultsOption]
                    INNER JOIN [AMConfigurationOption] [AMConfigurationOption]
                        ON [AMConfigurationOption].CompanyID = 2
                           AND [AMConfigResultsOption].[ConfigurationID] = [AMConfigurationOption].[ConfigurationID]
                           AND [AMConfigResultsOption].[Revision] = [AMConfigurationOption].[Revision]
                           AND [AMConfigResultsOption].[FeatureLineNbr] = [AMConfigurationOption].[ConfigFeatureLineNbr]
                           AND [AMConfigResultsOption].[OptionLineNbr] = [AMConfigurationOption].[LineNbr]
                WHERE [AMConfigResultsOption].CompanyID = 2
                      AND [AMConfigResultsOption].[MaterialType] = 0;
             **************************************************************************************************/
        }
    }
}