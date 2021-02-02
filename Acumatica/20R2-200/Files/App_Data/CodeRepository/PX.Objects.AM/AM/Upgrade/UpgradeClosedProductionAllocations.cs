using Customization;
using PX.Objects.AM.Attributes;
using PX.Data;
using PX.Objects.IN;

namespace PX.Objects.AM.Upgrade
{
    /// <summary>
    /// In early release versions with Allocations there was an issue when closing, deleting, cancelling a production order would either leave the allocation record or orphan the allocation record.
    /// This creates an issue when looking at allocation details because the record remain and the user doesn't have a way to clear them out.
    /// </summary>
    internal sealed class UpgradeClosedProductionAllocations : UpgradeProcessVersionBase
    {
        public UpgradeClosedProductionAllocations(UpgradeProcess upgradeGraph, CustomizationPlugin plugin) : base(upgradeGraph, plugin)
        {
        }

        public UpgradeClosedProductionAllocations(UpgradeProcess upgradeGraph) : base(upgradeGraph)
        {
        }

        public override int Version => UpgradeVersions.Version2019R1Ver31;

        public override void ProcessTables()
        {
            ProcessClosedProdItemSplitPlanRecords();
            ProcessClosedProdMatlSplitPlanRecords();
        }

        private void ProcessClosedProdItemSplitPlanRecords()
        {
            var allocGraph = PXGraph.CreateInstance<QtyAllocationUpgradeGraph>();

            foreach (PXResult<INItemPlan, AMProdItemSplit, AMProdItemStatus> result in PXSelectJoin<
                INItemPlan,
                InnerJoin<AMProdItemSplit,
                    On<INItemPlan.planID, Equal<AMProdItemSplit.planID>>,
                InnerJoin<AMProdItemStatus,
                    On<AMProdItemSplit.orderType, Equal<AMProdItemStatus.orderType>,
                    And<AMProdItemSplit.prodOrdID, Equal<AMProdItemStatus.prodOrdID>>>>>,
                Where<AMProdItemStatus.statusID, Equal<ProductionOrderStatus.closed>,
                    Or<AMProdItemStatus.statusID, Equal<ProductionOrderStatus.cancel>>>>
                .Select(allocGraph))
            {
                var split = (AMProdItemSplit)result;

                if (string.IsNullOrWhiteSpace(split?.ProdOrdID))
                {
                    continue;
                }
#if DEBUG
                AMDebug.TraceWriteMethodName($"{split.OrderType}:{split.ProdOrdID}:{split.SplitLineNbr}; PlanID = {split.PlanID}");
#endif
                allocGraph.ProdItemSplits.Delete(split);
            }

            allocGraph.Actions.PressSave();
        }

        private void ProcessClosedProdMatlSplitPlanRecords()
        {
            var allocGraph = PXGraph.CreateInstance<QtyAllocationUpgradeGraph>();

            foreach (PXResult<INItemPlan, AMProdMatlSplit, AMProdItemStatus> result in PXSelectJoin<
                INItemPlan,
                InnerJoin<AMProdMatlSplit,
                    On<INItemPlan.planID, Equal<AMProdMatlSplit.planID>>,
                InnerJoin<AMProdItemStatus,
                    On<AMProdMatlSplit.orderType, Equal<AMProdItemStatus.orderType>,
                    And<AMProdMatlSplit.prodOrdID, Equal<AMProdItemStatus.prodOrdID>>>>>,
                Where<AMProdItemStatus.statusID, Equal<ProductionOrderStatus.closed>,
                    Or<AMProdItemStatus.statusID, Equal<ProductionOrderStatus.cancel>>>>
                .Select(allocGraph))
            {
                //var itemPlan = (INItemPlan) result;
                var split = (AMProdMatlSplit)result;

                if (string.IsNullOrWhiteSpace(split?.ProdOrdID))
                {
                    continue;
                }
#if DEBUG
                AMDebug.TraceWriteMethodName($"{split.OrderType}:{split.ProdOrdID}:{split.OperationID}:{split.LineID}:{split.SplitLineNbr}; PlanID = {split.PlanID}");
#endif
                allocGraph.ProdMatlSplits.Delete(split);
            }

            allocGraph.Actions.PressSave();
        }
    }
}