using Customization;
using PX.Data;

namespace PX.Objects.AM.Upgrade
{
    internal sealed class UpgradeProject : UpgradeProcessVersionBase
    {
        public UpgradeProject(UpgradeProcess upgradeGraph, CustomizationPlugin plugin) : base(upgradeGraph, plugin)
        {
        }

        public UpgradeProject(UpgradeProcess upgradeGraph) : base(upgradeGraph)
        {
        }

        public static bool Process(UpgradeProcess upgradeGraph, CustomizationPlugin plugin, int upgradeFrom)
        {
            var upgrade = new UpgradeProject(upgradeGraph, plugin);
            if (upgradeFrom < upgrade.Version)
            {
                upgrade.Process();
                return true;
            }
            return false;
        }

        public override int Version => UpgradeVersions.Version2018R2Ver00;

        public override void ProcessTables()
        {
            /* Non-Project ID appears to always be value zero */
            var nonProjectId = PX.Objects.PM.ProjectDefaultAttribute.NonProject() ?? 0;

            SetDefaultProjectProdItem(nonProjectId);
            SetDefaultProjectAMMTran(nonProjectId);
            SetDefaultProjectAMEstimateReference(nonProjectId);
        }

        private void SetDefaultProjectProdItem(int defaultProjectId)
        {
            TryUpdate<AMProdItem>(
                new PXDataFieldAssign<AMProdItem.projectID>(PXDbType.Int, defaultProjectId),
                new PXDataFieldAssign<AMProdItem.updateProject>(PXDbType.Bit, 0),
                new PXDataFieldRestrict<AMProdItem.taskID>(PXDbType.Int, 4, null, PXComp.ISNULL));
        }

        private void SetDefaultProjectAMMTran(int defaultProjectId)
        {
            TryUpdate<AMMTran>(
                new PXDataFieldAssign<AMMTran.projectID>(PXDbType.Int, defaultProjectId),
                new PXDataFieldRestrict<AMMTran.taskID>(PXDbType.Int, 4, null, PXComp.ISNULL));
        }

        private void SetDefaultProjectAMEstimateReference(int defaultProjectId)
        {
            TryUpdate<AMEstimateReference>(
                new PXDataFieldAssign<AMEstimateReference.projectID>(PXDbType.Int, defaultProjectId),
                new PXDataFieldRestrict<AMEstimateReference.taskID>(PXDbType.Int, 4, null, PXComp.ISNULL));
        }
    }
}