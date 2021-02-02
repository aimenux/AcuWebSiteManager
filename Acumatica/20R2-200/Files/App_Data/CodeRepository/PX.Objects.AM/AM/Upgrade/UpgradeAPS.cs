using System;
using Customization;
using PX.Objects.AM.Attributes;
using PX.Common;
using PX.Data;
using PX.Objects.Common;
using PX.Objects.CS;
using PX.Objects.IN;
using PX.SiteMap.Graph;
using SiteMap = PX.SiteMap.DAC.SiteMap;

namespace PX.Objects.AM.Upgrade
{
    /// <summary>
    /// Upgrade process for APS implementation
    /// </summary>
    internal sealed class UpgradeAPS : UpgradeProcessVersionBase
    {
        private UpgradeAPS(UpgradeProcess upgradeGraph) : base(upgradeGraph)
        {
        }

        private UpgradeAPS(UpgradeProcess upgradeGraph, CustomizationPlugin plugin) : base(upgradeGraph, plugin)
        {
        }

        public static bool Process(UpgradeProcess upgradeGraph, CustomizationPlugin plugin, int upgradeFrom)
        {
            var upgrade = new UpgradeAPS(upgradeGraph, plugin) {_upgradeFromVersion = upgradeFrom};
            if (upgradeFrom < upgrade.Version)
            {
                upgrade.Process();
                return true;
            }
            return false;
        }

        private int _upgradeFromVersion;
        public override int Version => UpgradeVersions.Version2018R2Ver00;
        private bool UpgradeRanInPreviousVersion => _upgradeFromVersion.BetweenInclusive(UpgradeVersions.Version2017R2Ver1001, UpgradeVersions.MaxVersionNumbers.Version2017R2) || _upgradeFromVersion.BetweenInclusive(UpgradeVersions.Version2018R1Ver1000, UpgradeVersions.MaxVersionNumbers.Version2018R1);

        public override void ProcessTables()
        {
            if (UpgradeRanInPreviousVersion)
            {
#if DEBUG
                AMDebug.TraceWriteMethodName($"Process ran in previous version; Upgrading from {_upgradeFromVersion}");
#endif
                return;
            }

            SetAvailabilitySchemeDefaults();
            SetProdItemTranType();
            SetProdMatlTranDate();
            SyncSchdItem();
            SyncSchdOper();
            CleanupOrphandedSchdRecords();
            CleanupWcSchdRecords();
            SetProductionMaterialWarehouse();
            if (RemoveUnreleasedPoReceipts())
            {
                WriteInfo("Removed screen 'Unreleased PO Receipts'");
            }

            InitAPSMaintSetupRecord();

            WriteInfo("Saving changes");
            _upgradeGraph.Persist();
            _upgradeGraph.ClearAll();
            SetQtyAllocations();
        }

        /// <summary>
        /// Set the <see cref="INAvailabilityScheme"/> production field default values
        /// </summary>
        private void SetAvailabilitySchemeDefaults()
        {
            /* SET "Include Qty. of Production Supply Prepared" = "Include Qty. on Purchase Prepared" */
            SetAvailabilitySchemeDefaults<INAvailabilityScheme.inclQtyProductionSupplyPrepared, INAvailabilityScheme.inclQtyPOPrepared>();

            /* SET "Include Qty. of Production Supply" = "Include Qty. on Purchase Orders" */
            SetAvailabilitySchemeDefaults<INAvailabilityScheme.inclQtyProductionSupply, INAvailabilityScheme.inclQtyPOOrders>();

            /* SET "Deduct Qty. on Production Demand Prepared" = "Deduct Qty. on Sales Prepared" */
            SetAvailabilitySchemeDefaults<INAvailabilityScheme.inclQtyProductionDemandPrepared, INAvailabilityScheme.inclQtySOPrepared>();

            /* SET "Deduct Qty. on Production Demand Prepared" = "Deduct Qty. on Sales Orders" */
            SetAvailabilitySchemeDefaults<INAvailabilityScheme.inclQtyProductionDemand, INAvailabilityScheme.inclQtySOBooked>();

            /* SET "Deduct Qty. on Production Allocated" = "Deduct Qty. Allocated" */
            SetAvailabilitySchemeDefaults<INAvailabilityScheme.inclQtyProductionAllocated, INAvailabilityScheme.inclQtySOShipping>();
        }

        private void SetAvailabilitySchemeDefaults<TSetField, TSourceField>()
            where TSetField : IBqlField
            where TSourceField : IBqlField
        {
            TryUpdate<INAvailabilityScheme>(
                DirectExpressionFieldAssign<TSetField, TSourceField>(),
                new PXDataFieldRestrict<TSetField>(PXDbType.Bit, false)
            );
        }

        private void SetProdMatlTranDate()
        {
            PXUpdateJoin<
                    Set<AMProdMatl.tranDate, IsNull<AMProdOper.startDate, AMProdMatl.tranDate>>,
                    AMProdMatl,
                    InnerJoin<AMProdOper,
                        On<AMProdMatl.orderType, Equal<AMProdOper.orderType>,
                            And<AMProdMatl.prodOrdID, Equal<AMProdOper.prodOrdID>,
                                And<AMProdMatl.operationID, Equal<AMProdOper.operationID>>>>>>
                .Update(_upgradeGraph);

            PXUpdateJoin<
                    Set<AMProdMatlSplit.tranDate, AMProdMatl.tranDate>,
                    AMProdMatlSplit,
                    InnerJoin<AMProdMatl,
                        On<AMProdMatlSplit.orderType, Equal<AMProdMatl.orderType>,
                            And<AMProdMatlSplit.prodOrdID, Equal<AMProdMatl.prodOrdID>,
                                And<AMProdMatlSplit.operationID, Equal<AMProdMatl.operationID>,
                                    And<AMProdMatlSplit.lineID, Equal<AMProdMatl.lineID>>>>>>>
                .Update(_upgradeGraph);

            PXUpdateJoin<
                    Set<INItemPlan.planDate, AMProdMatlSplit.tranDate>,
                    INItemPlan,
                    InnerJoin<AMProdMatlSplit,
                        On<INItemPlan.planID, Equal<AMProdMatlSplit.planID>>>>
                .Update(_upgradeGraph);
        }

        private void SetProdItemTranType()
        {
            // Disassembly orders
            TryUpdate<AMProdItem>(
                new PXDataFieldAssign<AMProdItem.tranType>(PXDbType.Char, 3, INTranType.Issue),
                new PXDataFieldAssign<AMProdItem.invtMult>(PXDbType.SmallInt, 2, (short) -1),
                new PXDataFieldRestrict<AMProdItem.function>(PXDbType.Int, 4, OrderTypeFunction.Disassemble, PXComp.EQ)
            );

            // All other order types
            TryUpdate<AMProdItem>(
                new PXDataFieldAssign<AMProdItem.tranType>(PXDbType.Char, 3, INTranType.Receipt),
                new PXDataFieldAssign<AMProdItem.invtMult>(PXDbType.SmallInt, 2, (short) 1),
                new PXDataFieldRestrict<AMProdItem.function>(PXDbType.Int, 4, OrderTypeFunction.Disassemble, PXComp.NE)
            );

            //Sync AMProdItemSplits (in case they exist)
            PXUpdateJoin<
                    Set<AMProdItemSplit.tranType, AMProdItem.tranType,
                        Set<AMProdItemSplit.invtMult, AMProdItem.invtMult>>,
                    AMProdItemSplit,
                    InnerJoin<AMProdItem,
                        On<AMProdItemSplit.orderType, Equal<AMProdItem.orderType>,
                            And<AMProdItemSplit.prodOrdID, Equal<AMProdItem.prodOrdID>>>>>
                .Update(_upgradeGraph);
        }

        private void SetQtyAllocations()
        {
            WriteInfo("Initializing Production Allocations");
            var qtyAllocUpgradeGraph = PXGraph.CreateInstance<QtyAllocationUpgradeGraph>();

            using (new DisableSelectorValidationScope(qtyAllocUpgradeGraph.ProdItemSplits.Cache))
            using (new DisableSelectorValidationScope(qtyAllocUpgradeGraph.ProdOpers.Cache))
            using (new DisableSelectorValidationScope(qtyAllocUpgradeGraph.ProdMatls.Cache))
            using (new DisableSelectorValidationScope(qtyAllocUpgradeGraph.ProdMatlSplits.Cache))
            using (new DisableSelectorValidationScope(qtyAllocUpgradeGraph.Caches<INItemPlan>()))
            {
                qtyAllocUpgradeGraph.SyncProdItemSplits();
                qtyAllocUpgradeGraph.SyncProdMatlSplits(out var matlUpdatesSkipped);

                var prodItemSplitCount = qtyAllocUpgradeGraph.ProdItemSplits.Cache.Inserted.Count();
                if (prodItemSplitCount > 0)
                {
                    WriteInfo($"Inserting {prodItemSplitCount} Production Item Allocations");
                }

                prodItemSplitCount = qtyAllocUpgradeGraph.ProdItemSplits.Cache.Updated.Count();
                if (prodItemSplitCount > 0)
                {
                    WriteInfo($"Updating {prodItemSplitCount} Production Item Allocations");
                }

                var prodMatlSplitCount = qtyAllocUpgradeGraph.ProdMatlSplits.Cache.Inserted.Count();
                if (prodMatlSplitCount > 0)
                {
                    WriteInfo($"Inserting {prodMatlSplitCount} Production Material Allocations");
                }

                prodMatlSplitCount = qtyAllocUpgradeGraph.ProdMatlSplits.Cache.Updated.Count();
                if (prodMatlSplitCount > 0)
                {
                    WriteInfo($"Updating {prodMatlSplitCount} Production Material Allocations");
                }

                if (matlUpdatesSkipped > 0)
                {
                    WriteInfo($"Skipped {matlUpdatesSkipped} Production Material Allocations");
                }

                var itemPlanCount = qtyAllocUpgradeGraph.Caches<INItemPlan>().Inserted.Count();
                if (itemPlanCount > 0)
                {
                    WriteInfo($"Inserting {itemPlanCount} Item Plan Records");
                }

                itemPlanCount = qtyAllocUpgradeGraph.Caches<INItemPlan>().Updated.Count();
                if (itemPlanCount > 0)
                {
                    WriteInfo($"Updating {itemPlanCount} Item Plan Records");
                }

                WriteInfo("Saving changes");
                qtyAllocUpgradeGraph.Persist();
            }
        }

        private void InitAPSMaintSetupRecord()
        {
            var setup = (AMAPSMaintenanceSetup) PXSelect<AMAPSMaintenanceSetup>.Select(_upgradeGraph);
            if (setup != null)
            {
                return;
            }

            Common.Cache.AddCacheView<AMAPSMaintenanceSetup>(_upgradeGraph);
            _upgradeGraph.Caches<AMAPSMaintenanceSetup>().Insert();
        }

        /// <summary>
        /// As production material warehouse is a required field now, lets fill in all existing material warehouse values if empty
        /// </summary>
        private void SetProductionMaterialWarehouse()
        {
            /*
    -- Require warehouse on the material record...

    UPDATE m SET m.SiteID = i.SiteID FROM dbo.AMProdMatl m
    INNER JOIN dbo.AMProdItem i ON i.CompanyID = m.CompanyID AND i.OrderType = m.OrderType AND i.ProdOrdID = m.ProdOrdID
    WHERE m.SiteID IS NULL;

 */

            PXUpdateJoin<
                    Set<AMProdMatl.siteID, AMProdItem.siteID>,
                    AMProdMatl,
                    InnerJoin<AMProdItem,
                        On<AMProdMatl.orderType, Equal<AMProdItem.orderType>,
                            And<AMProdMatl.prodOrdID, Equal<AMProdItem.prodOrdID>>>>,
                Where<AMProdMatl.siteID, IsNull>>
                .Update(_upgradeGraph);
        }

        private void CleanupOrphandedSchdRecords()
        {
            var deletedSchdItem = 0;
            var deletedSchdOper = 0;
            foreach (AMSchdItem row in PXSelectReadonly2<
                    AMSchdItem,
                    LeftJoin<AMProdItem,
                        On<AMProdItem.orderType, Equal<AMSchdItem.orderType>,
                            And<AMProdItem.prodOrdID, Equal<AMSchdItem.prodOrdID>>>>,
                    Where<AMProdItem.prodOrdID, IsNull>>
                .Select(_upgradeGraph))
            {
                if (row?.ProdOrdID == null)
                {
                    continue;
                }

                if (TryDelete<AMSchdItem>(
                    new PXDataFieldRestrict(nameof(AMSchdItem.orderType), PXDbType.Char, 2, row.OrderType, PXComp.EQ),
                    new PXDataFieldRestrict(nameof(AMSchdItem.prodOrdID), PXDbType.NVarChar, 15, row.ProdOrdID, PXComp.EQ)
                ))
                {
                    deletedSchdItem++;
                }
            }

            foreach (AMSchdOper row in PXSelectReadonly2<
                    AMSchdOper,
                    LeftJoin<AMProdItem,
                        On<AMProdItem.orderType, Equal<AMSchdOper.orderType>,
                            And<AMProdItem.prodOrdID, Equal<AMSchdOper.prodOrdID>>>>,
                    Where<AMProdItem.prodOrdID, IsNull>>
                .Select(_upgradeGraph))
            {
                if (row?.ProdOrdID == null)
                {
                    continue;
                }

                if (TryDelete<AMSchdOper>(
                    new PXDataFieldRestrict(nameof(AMSchdOper.orderType), PXDbType.Char, 2, row.OrderType, PXComp.EQ),
                    new PXDataFieldRestrict(nameof(AMSchdOper.prodOrdID), PXDbType.NVarChar, 15, row.ProdOrdID, PXComp.EQ)
                ))
                {
                    deletedSchdOper++;
                }
            }

            foreach (AMSchdOper row in PXSelectReadonly2<
                    AMSchdOper,
                    LeftJoin<AMProdOper,
                        On<AMProdOper.orderType, Equal<AMSchdOper.orderType>,
                            And<AMProdOper.prodOrdID, Equal<AMSchdOper.prodOrdID>,
                                And<AMProdOper.operationID, Equal<AMSchdOper.operationID>>>>>,
                    Where<AMProdOper.operationID, IsNull>>
                .Select(_upgradeGraph))
            {
                if (row?.ProdOrdID == null)
                {
                    continue;
                }

                if (TryDelete<AMSchdOper>(
                    new PXDataFieldRestrict(nameof(AMSchdOper.orderType), PXDbType.Char, 2, row.OrderType, PXComp.EQ),
                    new PXDataFieldRestrict(nameof(AMSchdOper.prodOrdID), PXDbType.NVarChar, 15, row.ProdOrdID, PXComp.EQ),
                    new PXDataFieldRestrict(nameof(AMSchdOper.operationID), PXDbType.NChar, 10, row.OperationID, PXComp.EQ)
                ))
                {
                    deletedSchdOper++;
                }
            }

            if (deletedSchdItem != 0 || deletedSchdOper != 0)
            {
                WriteInfo($"Deleted {deletedSchdItem} AMSchdItem records; Deleted {deletedSchdOper} AMSchdOper records");
            }

            /*
            DELETE s
            FROM dbo.AMSchdItem s
            WHERE NOT EXISTS
            (
                SELECT *
                FROM dbo.AMProdItem p
                WHERE p.CompanyID = s.CompanyID
                      AND p.OrderType = s.OrderType
                      AND p.ProdOrdID = s.ProdOrdID
            );

            DELETE s
            FROM dbo.AMSchdOper s
            WHERE NOT EXISTS
            (
                SELECT *
                FROM dbo.AMProdItem p
                WHERE p.CompanyID = s.CompanyID
                      AND p.OrderType = s.OrderType
                      AND p.ProdOrdID = s.ProdOrdID
            );

            DELETE s
            FROM dbo.AMSchdOper s
            WHERE NOT EXISTS
            (
                SELECT *
                FROM dbo.AMProdOper p
                WHERE p.CompanyID = s.CompanyID
                      AND p.OrderType = s.OrderType
                      AND p.ProdOrdID = s.ProdOrdID
                      AND p.OperationID = s.OperationID
            );
            */
        }

        /// <summary>
        /// Make sure AMWCSchd record totals match sum of AMWCSchdDetail records
        /// </summary>
        private void CleanupWcSchdRecords()
        {
            // Step 1 - make sure totals on schd are in sync with values in schddetail

            //Common.Cache.AddCacheView<AMWCSchdNotMatchingDetail>(_upgradeGraph);
            var counter = 0;
            foreach (AMWCSchdNotMatchingDetail result in PXSelect<AMWCSchdNotMatchingDetail>.Select(_upgradeGraph))
            {
                if (result?.WcID == null)
                {
                    return;
                }

                counter++;
                result.SchdTime = result.DetailSchdTime.GetValueOrDefault();
                result.SchdEfficiencyTime = result.DetailSchdEfficiencyTime.GetValueOrDefault();
                result.PlanBlocks = result.DetailPlanBlocks.GetValueOrDefault();
                result.SchdBlocks = result.DetailSchdBlocks.GetValueOrDefault();
                _upgradeGraph.Caches<AMWCSchdNotMatchingDetail>().Update(result);
            }

            if (counter > 0)
            {
                WriteInfo($"Updated {counter} AMWCSchd records");
                _upgradeGraph.Caches<AMWCSchdNotMatchingDetail>().Persist(PXDBOperation.Update);
            }

            // Step 2 - delete old records no longer required to be stored in the DB

            var deleteDate = Common.Dates.UtcToday.AddDays(-2);

            TryDelete<AMWCSchd>(
                new PXDataFieldRestrict(nameof(AMWCSchd.schdTime), PXDbType.Int, 4, 0, PXComp.EQ),
                new PXDataFieldRestrict(nameof(AMWCSchd.planBlocks), PXDbType.Int, 4, 0, PXComp.EQ),
                new PXDataFieldRestrict(nameof(AMWCSchd.schdBlocks), PXDbType.Int, 4, 0, PXComp.EQ),
                new PXDataFieldRestrict(nameof(AMWCSchd.schdDate), PXDbType.DateTime, 8, deleteDate, PXComp.LE)
            );


        }

        /// <summary>
        /// Make sure the AMSchdItem records are in sync with the AMProdItem records
        /// </summary>
        private void SyncSchdItem()
        {
            PXUpdateJoin<
                    Set<AMSchdItem.inventoryID, AMProdItem.inventoryID,
                        Set<AMSchdItem.siteID, AMProdItem.siteID,
                            Set<AMSchdItem.qtyComplete, AMProdItem.qtyComplete,
                                Set<AMSchdItem.qtyScrapped, AMProdItem.qtyScrapped,
                                    Set<AMSchdItem.qtytoProd, AMProdItem.qtytoProd,
                                        Set<AMSchdItem.schPriority, AMProdItem.schPriority>>>>>>,
                    AMSchdItem,
                    InnerJoin<AMProdItem,
                        On<AMSchdItem.orderType, Equal<AMProdItem.orderType>,
                            And<AMSchdItem.prodOrdID, Equal<AMProdItem.prodOrdID>>>>>
                .Update(_upgradeGraph);
        }

        private void SyncSchdOper()
        {
            PXUpdateJoin<
                    Set<AMSchdOper.siteID, AMProdItem.siteID>,
                    AMSchdOper,
                    InnerJoin<AMProdItem,
                        On<AMSchdOper.orderType, Equal<AMProdItem.orderType>,
                            And<AMSchdOper.prodOrdID, Equal<AMProdItem.prodOrdID>>>>>
                .Update(_upgradeGraph);

            PXUpdateJoin<
                    Set<AMSchdOper.qtyComplete, AMProdOper.qtyComplete,
                        Set<AMSchdOper.qtyScrapped, AMProdOper.qtyScrapped,
                            Set<AMSchdOper.qtytoProd, AMProdOper.qtytoProd>>>,
                    AMSchdOper,
                    InnerJoin<AMProdOper,
                        On<AMSchdOper.orderType, Equal<AMProdOper.orderType>,
                            And<AMSchdOper.prodOrdID, Equal<AMProdOper.prodOrdID>,
                                And<AMSchdOper.operationID, Equal<AMProdOper.operationID>>>>>>
                .Update(_upgradeGraph);
        }

        private static bool RemoveUnreleasedPoReceipts()
        {
            var unreleasedPoReceiptsSiteMap = "AM305000";

            var siteMapGraph = PXGraph.CreateInstance<SiteMapMaint>();

            PX.SiteMap.DAC.SiteMap siteMap = PXSelect<
                    PX.SiteMap.DAC.SiteMap,
                    Where<PX.SiteMap.DAC.SiteMap.screenID, Equal<Required<PX.SiteMap.DAC.SiteMap.screenID>>>>
                .Select(siteMapGraph, unreleasedPoReceiptsSiteMap);

            if (siteMap?.ScreenID == null)
            {
                return false;
            }

            siteMapGraph.SiteMap.Delete(siteMap);
            siteMapGraph.Persist();

            return true;
        }
    }

    [Serializable]
    [PXHidden]
    [PXProjection(typeof(Select2<
        AMWCSchd,
        LeftJoin<AMWCSchdDetailGroupByWcSchiftDate, 
            On<AMWCSchd.wcID, Equal<AMWCSchdDetailGroupByWcSchiftDate.wcID>,
            And<AMWCSchd.shiftID, Equal<AMWCSchdDetailGroupByWcSchiftDate.shiftID>,
            And<AMWCSchd.schdDate, Equal<AMWCSchdDetailGroupByWcSchiftDate.schdDate>>>>>,
        Where<AMWCSchd.schdTime, NotEqual<IsNull<AMWCSchdDetailGroupByWcSchiftDate.schdTime, int0>>,
            Or<AMWCSchd.planBlocks, NotEqual<IsNull<AMWCSchdDetailGroupByWcSchiftDate.planBlocks, int0>>,
            Or<AMWCSchd.schdBlocks, NotEqual<IsNull<AMWCSchdDetailGroupByWcSchiftDate.schdBlocks, int0>>>>>>),
        new[] { typeof(AMWCSchd) })]
    public class AMWCSchdNotMatchingDetail : IBqlTable
    {
        #region WcID

        public abstract class wcID : PX.Data.BQL.BqlString.Field<wcID> { }
        protected String _WcID;
        [WorkCenterIDField(IsKey = true, Visibility = PXUIVisibility.SelectorVisible, Enabled = false, BqlField = typeof(AMWCSchd.wcID))]
        [PXDefault]
        [PXSelector(typeof(Search<AMWC.wcID>))]
        public virtual String WcID
        {
            get { return this._WcID; }
            set { this._WcID = value; }
        }

        #endregion
        #region ShiftID
        public abstract class shiftID : PX.Data.BQL.BqlString.Field<shiftID> { }
        protected String _ShiftID;
        [PXDBString(4, IsKey = true, BqlField = typeof(AMWCSchd.shiftID))]
        [PXDefault]
        [PXUIField(DisplayName = "Shift", Visibility = PXUIVisibility.SelectorVisible, Enabled = false)]
        [PXSelector(typeof(Search<AMShiftMst.shiftID>))]
        public virtual String ShiftID
        {
            get { return this._ShiftID; }
            set { this._ShiftID = value; }
        }
        #endregion
        #region SchdDate

        public abstract class schdDate : PX.Data.BQL.BqlDateTime.Field<schdDate> { }
        protected DateTime? _SchdDate;
        [PXDBDate(IsKey = true, BqlField = typeof(AMWCSchd.schdDate))]
        [PXDefault]
        [PXUIField(DisplayName = "Schedule Date", Enabled = false)]
        public virtual DateTime? SchdDate
        {
            get { return this._SchdDate; }
            set { this._SchdDate = value; }
        }

        #endregion

        #region SchdTime
        public abstract class schdTime : PX.Data.BQL.BqlInt.Field<schdTime> { }
        [PXDBTimeSpanLong(Format = TimeSpanFormatType.LongHoursMinutes, BqlField = typeof(AMWCSchd.schdTime))]
        [PXUIField(DisplayName = "Schedule Time w / o Efficiency", Enabled = false)]
        [PXDefault(0)]
        public virtual int? SchdTime { get; set; }
        #endregion
        #region SchdEfficiencyTime
        public abstract class schdEfficiencyTime : PX.Data.IBqlField
        {
        }
        [PXDBTimeSpanLong(Format = TimeSpanFormatType.LongHoursMinutes, BqlField = typeof(AMWCSchd.schdEfficiencyTime))]
        [PXUIField(DisplayName = "Schedule Time", Enabled = false)]
        [PXDefault(0)]
        public virtual int? SchdEfficiencyTime { get; set; }
        #endregion
        #region PlanBlocks
        public abstract class planBlocks : PX.Data.BQL.BqlInt.Field<planBlocks> { }

        [PXDBInt(MinValue = 0, BqlField = typeof(AMWCSchd.planBlocks))]
        [PXUIField(DisplayName = "Plan Blocks", Enabled = false)]
        [PXDefault(0)]
        public virtual int? PlanBlocks { get; set; }
        #endregion
        #region SchdBlocks
        public abstract class schdBlocks : PX.Data.BQL.BqlInt.Field<schdBlocks> { }

        [PXDBInt(MinValue = 0, BqlField = typeof(AMWCSchd.schdBlocks))]
        [PXUIField(DisplayName = "Scheduled Blocks", Enabled = false)]
        [PXDefault(0)]
        public virtual int? SchdBlocks { get; set; }
        #endregion

        #region DetailSchdTime (SUM)

        public abstract class detailSchdTime : PX.Data.BQL.BqlInt.Field<detailSchdTime> { }


        [PXDBTimeSpanLong(Format = TimeSpanFormatType.LongHoursMinutes, BqlField = typeof(AMWCSchdDetail.schdTime))]
        [PXUIField(DisplayName = "Detail Schedule Time w / o Efficiency", Enabled = false)]
        public virtual int? DetailSchdTime { get; set; }

        #endregion
        #region DetailSchdEfficiencyTime (SUM)

        public abstract class detailSchdEfficiencyTime : PX.Data.IBqlField
        {
        }

        [PXDBTimeSpanLong(Format = TimeSpanFormatType.LongHoursMinutes, BqlField = typeof(AMWCSchdDetail.schdEfficiencyTime))]
        [PXUIField(DisplayName = "Detail Schedule Time", Enabled = false)]
        public virtual int? DetailSchdEfficiencyTime { get; set; }

        #endregion
        #region DetailPlanBlocks (SUM)
        public abstract class detailPlanBlocks : PX.Data.BQL.BqlInt.Field<detailPlanBlocks> { }

        [PXDBInt(MinValue = 0, BqlField = typeof(AMWCSchdDetail.planBlocks))]
        [PXUIField(DisplayName = "Detail Plan Blocks", Enabled = false)]
        public virtual int? DetailPlanBlocks { get; set; }

        #endregion
        #region DetailSchdBlocks (SUM)
        public abstract class detailSchdBlocks : PX.Data.BQL.BqlInt.Field<detailSchdBlocks> { }


        [PXDBInt(MinValue = 0, BqlField = typeof(AMWCSchdDetail.schdBlocks))]
        [PXUIField(DisplayName = "Detail Scheduled Blocks", Enabled = false)]
        public virtual int? DetailSchdBlocks { get; set; }
        #endregion
    }
}