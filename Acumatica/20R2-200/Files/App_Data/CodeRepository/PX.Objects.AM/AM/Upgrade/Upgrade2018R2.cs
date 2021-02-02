using Customization;
using PX.Data;

namespace PX.Objects.AM.Upgrade
{
    internal sealed class Upgrade2018R2 : UpgradeProcessVersionBase
    {
        public Upgrade2018R2(UpgradeProcess upgradeGraph, CustomizationPlugin plugin) : base(upgradeGraph, plugin)
        {
        }

        public Upgrade2018R2(UpgradeProcess upgradeGraph) : base(upgradeGraph)
        {
        }

        public static bool Process(UpgradeProcess upgradeGraph, CustomizationPlugin plugin, int upgradeFrom)
        {
            var upgrade = new Upgrade2018R2(upgradeGraph, plugin);

            if (upgradeFrom < upgrade.Version)
            {
                upgrade.Process();

                return true;
            }
            return false;
        }

        public override int Version => UpgradeVersions.Version2018R2Ver03;
        public override void ProcessTables()
        {
            ProcessRTrimUpdates();
            DeleteAllBomCostRecords();
            SetBomEffEndDate();
            UpdateConfigurationWithBomChanges();
            SetSchdOperSortOrder();
            SetProductionBomRevision();
        }

        /// <summary>
        /// As BOM Revision was not previously required, existing production orders with a BOM ID need to have the BOM Revision value set
        /// </summary>
        private void SetProductionBomRevision()
        {
            PXUpdateJoin<
                    Set<AMProdItem.bOMRevisionID, AMBomItemActive.revisionID>,
                    AMProdItem,
                    InnerJoin<AMBomItemActive, On<AMProdItem.bOMID, Equal<AMBomItemActive.bOMID>>>,
                    Where<AMProdItem.bOMID, IsNotNull,
                        And<Where<AMProdItem.bOMRevisionID, IsNull,
                            Or<AMProdItem.bOMRevisionID, Equal<StringEmpty>>>>>>
                .Update(_upgradeGraph);
        }

        /// <summary>
        /// On upgrade the operationID is already in the correct sequence/sort order. After upgrading users can change the operation values around which no longer keeps the correct order.
        /// </summary>
        private void SetSchdOperSortOrder()
        {
            TryUpdate<AMSchdOper>(new PXDataFieldAssign<AMSchdOper.sortOrder>(PXDbType.DirectExpression, QuoteDbIdentifier<AMSchdOper.operationID>()));
        }

        /// <summary>
        /// AMBomItem.EffEndDate = Null Where year 2079 since EffEndDate is now user enterable. We want the user to define and end date when the bom rev has truely ended
        /// </summary>
        private void SetBomEffEndDate()
        {
            TryUpdate<AMBomItem>(
                new PXDataFieldAssign<AMBomItem.effEndDate>(null),
                new PXDataFieldRestrict<AMBomItem.effEndDate>(PXDbType.DateTime, 8, Common.Dates.EndOfTimeDate, PXComp.EQ));
        }

        private void ProcessRTrimUpdates()
        {
            ProcessRTrimUpdate<AMBomOper, AMBomOper.wcID>();
            ProcessRTrimUpdate<AMBomOvhd, AMBomOvhd.ovhdID>();
            ProcessRTrimUpdate<AMBomRef, AMBomRef.descr>();
            ProcessRTrimUpdate<AMBomRef, AMBomRef.refDes>();
            ProcessRTrimUpdate<AMBomStep, AMBomStep.descr>();
            ProcessRTrimUpdate<AMBomTool, AMBomTool.descr>();
            ProcessRTrimUpdate<AMBomTool, AMBomTool.toolID>();
            ProcessRTrimUpdate<AMBSetup, AMBSetup.wcID>();
            ProcessRTrimUpdate<AMEstimateOper, AMEstimateOper.operationCD>();
            ProcessRTrimUpdate<AMEstimateOper, AMEstimateOper.workCenterID>();
            ProcessRTrimUpdate<AMEstimateOvhd, AMEstimateOvhd.ovhdID>();
            ProcessRTrimUpdate<AMEstimateSetup, AMEstimateSetup.defaultRevisionID>();
            ProcessRTrimUpdate<AMEstimateSetup, AMEstimateSetup.defaultWorkCenterID>();
            ProcessRTrimUpdate<AMMach, AMMach.machID>();
            ProcessRTrimUpdate<AMMTran, AMMTran.shiftID>();
            ProcessRTrimUpdate<AMOrderCrossRef, AMOrderCrossRef.referenceType>();
            ProcessRTrimUpdate<AMOverhead, AMOverhead.descr>();
            ProcessRTrimUpdate<AMOverhead, AMOverhead.ovhdID>();
            ProcessRTrimUpdate<AMProdOper, AMProdOper.wcID>();
            ProcessRTrimUpdate<AMProdOvhd, AMProdOvhd.ovhdID>();
            ProcessRTrimUpdate<AMRPSetup, AMRPSetup.defaultMPSTypeID>();
            ProcessRTrimUpdate<AMSchdOper, AMSchdOper.wcID>();
            ProcessRTrimUpdate<AMSchdOper, AMSchdOper.machID>();
            ProcessRTrimUpdate<AMShift, AMShift.shiftID>();
            ProcessRTrimUpdate<AMShift, AMShift.wcID>();
            ProcessRTrimUpdate<AMShiftMst, AMShiftMst.shftDesc>();
            ProcessRTrimUpdate<AMShiftMst, AMShiftMst.shiftID>();
            ProcessRTrimUpdate<AMToolMst, AMToolMst.descr>();
            ProcessRTrimUpdate<AMToolMst, AMToolMst.toolID>();
            ProcessRTrimUpdate<AMWC, AMWC.wcID>();
            ProcessRTrimUpdate<AMWC, AMWC.descr>();
            ProcessRTrimUpdate<AMWCMach, AMWCMach.machID>();
            ProcessRTrimUpdate<AMWCMach, AMWCMach.wcID>();
            ProcessRTrimUpdate<AMWCOvhd, AMWCOvhd.ovhdID>();
            ProcessRTrimUpdate<AMWCOvhd, AMWCOvhd.wcID>();
            ProcessRTrimUpdate<AMWCSchd, AMWCSchd.wcID>();
            ProcessRTrimUpdate<AMWCSchd, AMWCSchd.shiftID>();
            ProcessRTrimUpdate<AMWCSchdDetail, AMWCSchdDetail.wcID>();
            ProcessRTrimUpdate<AMWCSchdDetail, AMWCSchdDetail.shiftID>();
        }

        private void UpdateConfigurationWithBomChanges()
        {
            PXUpdateJoin<
                Set<AMConfiguration.bOMRevisionID, AMBomItemActive.revisionID>,
                AMConfiguration,
                InnerJoin<AMBomItemActive, On<AMConfiguration.bOMID, Equal<AMBomItemActive.bOMID>>>,
                Where<AMConfiguration.bOMRevisionID, IsNull>>.Update(_upgradeGraph);
        }

        /// <summary>
        /// The table is receiving a new revision string value as a key field.
        /// Because the data is built while running the cost roll process and cleared
        /// and rebuild for each cost roll we can delete any exiting values and let the users rebuild as needed after upgrading
        /// </summary>
        private void DeleteAllBomCostRecords()
        {
            TryDeleteAll<AMBomCost>();
        }
    }
}