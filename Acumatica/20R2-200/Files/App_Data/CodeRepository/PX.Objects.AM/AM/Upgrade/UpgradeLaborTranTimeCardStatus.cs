using Customization;
using PX.Data;
using PX.Objects.AM.Attributes;

namespace PX.Objects.AM.Upgrade
{
    internal sealed class UpgradeLaborTranTimeCardStatus : UpgradeProcessVersionBase
    {
        public UpgradeLaborTranTimeCardStatus(UpgradeProcess upgradeGraph, CustomizationPlugin plugin) : base(upgradeGraph, plugin)
        {
        }

        public UpgradeLaborTranTimeCardStatus(UpgradeProcess upgradeGraph) : base(upgradeGraph)
        {
        }

        public static bool Process(UpgradeProcess upgradeGraph, CustomizationPlugin plugin, int upgradeFrom)
        {
            var upgrade = new UpgradeLaborTranTimeCardStatus(upgradeGraph, plugin);
            if (upgradeFrom < upgrade.Version)
            {
                upgrade.Process();
                return true;
            }
            return false;
        }

        public override int Version => UpgradeVersions.Version2018R2Ver26;

        public override void ProcessTables()
        {
            // Default all labor documents to Unprocessed
            TryUpdate<AMMTran>(
                new PXDataFieldAssign<AMMTran.timeCardStatus>(PXDbType.Int, TimeCardStatus.Unprocessed),
                    new PXDataFieldRestrict<AMMTran.docType>(PXDbType.Char, AMDocType.Labor),
                    new PXDataFieldRestrict<AMMTran.timeCardStatus>(PXDbType.Int, 4, null, PXComp.ISNULL));
        }
    }
}
