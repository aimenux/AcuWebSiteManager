using Customization;
using PX.Data;
using PX.Objects.AM.Attributes;

namespace PX.Objects.AM.Upgrade
{
    /// <summary>
    /// Default a field value for 2019R2 upgrade also performed in 2020R1 in case skipping over 19R2
    /// </summary>
    internal sealed class UpgradeSubcontractSource : UpgradeProcessVersionBase
    {
        public UpgradeSubcontractSource(UpgradeProcess upgradeGraph, CustomizationPlugin plugin) : base(upgradeGraph, plugin)  { }

        public UpgradeSubcontractSource(UpgradeProcess upgradeGraph) : base(upgradeGraph) { }

        public override int Version => UpgradeVersions.Version2020R1Ver26;

        protected override bool ProcessCompletedInPreviousVersion(int upgradeFrom)
        {
            return upgradeFrom.BetweenInclusive(UpgradeVersions.Version2019R2Ver61, UpgradeVersions.MaxVersionNumbers.Version2019R2);
        }

        public override void ProcessTables()
        {
            //ProcessMaterialTransactionSubcontractSource();
        }

        /// <summary>
        /// <see cref="AMMTran.SubcontractSource"/> is new in 2019R2 but nullable. Need to default for existing Material transactions.
        /// Some users have unreleased transactions before upgrade and this creates issues as the field is required on material transactions.
        /// </summary>
        private void ProcessMaterialTransactionSubcontractSource()
        {
            /**********************************************
                UPDATE AMMTran
                SET [SubcontractSource] = 0
                WHERE (
                          [DocType] = 'M'
                          AND [SubcontractSource] IS NULL
                      )
                      AND CompanyID = 2;
             ***********************************************/

            TryUpdate<AMMTran>(
                new PXDataFieldAssign<AMMTran.subcontractSource>(PXDbType.Int, AMSubcontractSource.None),
                new PXDataFieldRestrict<AMMTran.docType>(PXDbType.Char, 1, AMDocType.Material, PXComp.EQ),
                new PXDataFieldRestrict<AMMTran.subcontractSource>(PXDbType.Int, 4, null, PXComp.ISNULL));
        }
    }
}