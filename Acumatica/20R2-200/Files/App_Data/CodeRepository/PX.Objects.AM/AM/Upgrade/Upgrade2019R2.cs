using Customization;
using PX.Objects.AM.Attributes;
using PX.Data;
using PX.Objects.IN;

namespace PX.Objects.AM.Upgrade
{
    internal sealed class Upgrade2019R2 : UpgradeProcessVersionBase
    {
        public Upgrade2019R2(UpgradeProcess upgradeGraph, CustomizationPlugin plugin) : base(upgradeGraph, plugin)
        {
        }

        public Upgrade2019R2(UpgradeProcess upgradeGraph) : base(upgradeGraph)
        {
        }

        public override int Version => UpgradeVersions.Version2019R2Ver07;

        public override void ProcessTables()
        {
            //DefaultSortOrder();
            //SetAMOrderTypeLineCntr();
            //SetBomMatlIsStockItem();
            //SetProdMatlIsStockItem();
            //SetOutsideProcess();
        }

        private void SetOutsideProcess()
        {
            foreach (AMWC workcenter in PXSelect<AMWC, Where<AMWC.outsideFlg, Equal<True>>>.Select(_upgradeGraph))
            {
                if (workcenter?.WcID == null)
                {
                    continue;
                }

                SetOutsideProcess<AMBomOper, AMBomOper.outsideProcess, AMBomOper.wcID>(workcenter.WcID);
                SetOutsideProcess<AMProdOper, AMProdOper.outsideProcess, AMProdOper.wcID>(workcenter.WcID);
                SetOutsideProcess<AMEstimateOper, AMEstimateOper.outsideProcess, AMEstimateOper.workCenterID>(workcenter.WcID);
            }
        }

        private void SetOutsideProcess<Dac, outsideProcess, workcenter>(string wcId)
            where Dac : IBqlTable
            where outsideProcess : IBqlField
            where workcenter : IBqlField
        {
            TryUpdate<Dac>(
                new PXDataFieldAssign<outsideProcess>(PXDbType.Bit, true),
                new PXDataFieldRestrict<workcenter>(PXDbType.NVarChar, 20, wcId, PXComp.EQ));
        }

        private void SetBomMatlIsStockItem()
        {
            PXUpdateJoin<
                Set<AMBomMatl.isStockItem, InventoryItem.stkItem>,
                AMBomMatl,
                InnerJoin<InventoryItem,
                    On<AMBomMatl.inventoryID, Equal<InventoryItem.inventoryID>>>>
                .Update(_upgradeGraph);
        }

        private void SetProdMatlIsStockItem()
        {
            PXUpdateJoin<
                    Set<AMProdMatl.isStockItem, InventoryItem.stkItem>,
                    AMProdMatl,
                    InnerJoin<InventoryItem,
                        On<AMProdMatl.inventoryID, Equal<InventoryItem.inventoryID>>>>
                .Update(_upgradeGraph);
        }

        private void DefaultSortOrder()
        {
            ProcessDefaultStepSortOrder<AMBomStep, AMBomStep.lineID, AMBomStep.sortOrder>();
            ProcessDefaultStepSortOrder<AMProdStep, AMProdStep.lineID, AMProdStep.sortOrder>();
            ProcessDefaultStepSortOrder<AMConfigurationAttribute, AMConfigurationAttribute.lineNbr, AMConfigurationAttribute.sortOrder>();
            ProcessDefaultStepSortOrder<AMConfigurationOption, AMConfigurationOption.lineNbr, AMConfigurationOption.sortOrder>();
        }

        private void ProcessDefaultStepSortOrder<Dac, lineNbr, sortOrder>()
            where Dac : IBqlTable
            where lineNbr : IBqlField
            where sortOrder : IBqlField
        {
            TryUpdate<Dac>(
                new PXDataFieldAssign<sortOrder>(PXDbType.DirectExpression, SetDefaultSortOrder(typeof(lineNbr).Name.ToCapitalized())),
                // In case process is re-run
                new PXDataFieldRestrict<sortOrder>(PXDbType.Int, 4, null, PXComp.ISNULL));
        }

        private string SetDefaultSortOrder(string runTimeFieldName)
        {
            return $"{QuoteDbIdentifier(runTimeFieldName)} * {SortOrderDefaultAttribute.DEFAULTSTEPMULT}";
        }

        private void SetAMOrderTypeLineCntr()
        {
            foreach (AMOrderTypeAttribute row in PXSelectGroupBy<
                AMOrderTypeAttribute,
                Aggregate<
                    GroupBy<AMOrderTypeAttribute.orderType>>>
                .Select(_upgradeGraph))
            {
                if (row?.OrderType == null)
                {
                    continue;
                }

                TryUpdate<AMOrderType>(
                    new PXDataFieldAssign<AMOrderType.lineCntrAttribute>(PXDbType.Int, row.LineNbr ?? 0),
                    new PXDataFieldRestrict<AMOrderType.orderType>(PXDbType.Char, 2, row.OrderType));
            }

        }
    }
}