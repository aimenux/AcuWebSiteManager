using Customization;
using PX.Objects.IN;
using PX.Data;
using PX.Objects.AM.CacheExtensions;

namespace PX.Objects.AM.Upgrade
{
    internal sealed class Upgrade2018R1Ver27 : UpgradeProcessVersionBase
    {
        public Upgrade2018R1Ver27(UpgradeProcess upgradeGraph, CustomizationPlugin plugin) : base(upgradeGraph, plugin)
        {
        }

        public Upgrade2018R1Ver27(UpgradeProcess upgradeGraph) : base(upgradeGraph)
        {
        }

        public static bool Process(UpgradeProcess upgradeGraph, CustomizationPlugin plugin, int upgradeFrom)
        {
            var upgrade = new Upgrade2018R1Ver27(upgradeGraph, plugin);
            upgrade.UpgradeFromVersion = upgradeFrom;

            if (upgradeFrom < upgrade.Version)
            {
                upgrade.Process();
                return true;
            }
            return false;
        }

        private int UpgradeFromVersion;
        public override int Version => UpgradeVersions.Version2018R1Ver27;

        public override void ProcessTables()
        {
            SetMtoItems();
            Bug1945DataCorrection();
        }

        private void Bug1945DataCorrection()
        {
            var tfs1945 = new TFS1945(UpgradeFromVersion, _upgradeGraph);

            tfs1945.ProcessUpgrade();

            if (tfs1945.HasProcessErrors)
            {
                WriteCstInfoOnly($"{tfs1945.ProcessExceptions.Count} errors found updating production orders and transactions:");
                var tracedErrors = 0;
                var maxErrors = 5;
                foreach (var exception in tfs1945.ProcessExceptions)
                {
                    //Keep posted errors to a min...
                    if (tracedErrors >= maxErrors)
                    {
                        WriteCstInfoOnly(2, $"... {tfs1945.ProcessExceptions.Count - maxErrors} more errors ...");
                        break;
                    }

                    tracedErrors++;
                    WriteInfo(2, exception.Message);
                }
            }

            if (tfs1945.ProductionOrderUpdateCounter > 0)
            {
                WriteInfo($"Updated {tfs1945.ProductionOrderUpdateCounter} production orders and {tfs1945.TransactionUpdateCounter} transactions");
            }
        }

        /// <summary>
        /// Set Make To Order Items based on history of sales orders with created production order
        /// </summary>
        private void SetMtoItems()
        {
            foreach (PX.Objects.SO.SOLine result in PXSelectGroupBy<
                    PX.Objects.SO.SOLine,
                    Where<SOLineExt.aMProdOrdID, IsNotNull>,
                    Aggregate<
                        GroupBy<PX.Objects.SO.SOLine.inventoryID>>>
                .Select(_upgradeGraph))
            {
                if (result?.InventoryID == null)
                {
                    continue;
                }

                TryUpdate<InventoryItem>(
                    new PXDataFieldAssign<InventoryItemExt.aMMakeToOrderItem>(PXDbType.Bit, true),
                    new PXDataFieldRestrict<InventoryItem.inventoryID>(PXDbType.Int, result.InventoryID));
            }

        }
    }
}