using System;
using System.Linq;
using PX.Data;
using System.Collections;
using System.Collections.Generic;
using PX.Objects.AM.GraphExtensions;
using PX.Objects.IN;
using PX.Objects.AM.Attributes;
using PX.Objects.AM.CacheExtensions;

namespace PX.Objects.AM
{
    /// <summary>
    /// Multi-level BOM report pre-process page
    /// </summary>
    public class MultiLevelBomInq : PXGraph<MultiLevelBomInq>
    {
        /// <summary>
        /// Multi-level BOM Engineering Report Number
        /// </summary>
        public const string MultiLevelBomReportID = "AM613000";
        /// <summary>
        /// Multi-level BOM Costed Report Number
        /// </summary>
        public const string MultiLevelBomCostedReportID = "AM614000";

        public PXCancel<AMMultiLevelBomFilter> Cancel;
        public PXFilter<AMMultiLevelBomFilter> Filter;

        [PXFilterable]
        public PXSelect<AMMultiLevelBomData> Results;

        public override void InitCacheMapping(Dictionary<Type, Type> map)
        {
            base.InitCacheMapping(map);

            this.Caches.AddCacheMapping(typeof(AMBomItem), typeof(AMBomItem));
            this.Caches.AddCacheMapping(typeof(AMBomItemActive), typeof(AMBomItemActive));
        }

        protected virtual IEnumerable results()
        {
            return LoadAllData();
        }

        public MultiLevelBomInq()
        {
            this.Results.Cache.AllowInsert = false;
            this.Results.Cache.AllowDelete = false;
            PXUIFieldAttribute.SetEnabled(Results.Cache, null, false);
        }

        public PXAction<AMMultiLevelBomFilter> report;
        [PXUIField(DisplayName = Messages.Engineering, MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select)]
        [PXButton]
        protected virtual IEnumerable Report(PXAdapter adapter)
        {
            if (string.IsNullOrWhiteSpace(Filter.Current.BOMID) && Filter.Current.InventoryID == null)
            {
                //Have the user confirm running report for all BOMS
                if (Results.Ask(Messages.ConfirmProcess, Messages.MultiLevelRunAll, MessageButtons.YesNo) != WebDialogResult.Yes)
                {
                    return adapter.Get();
                }
            }

            PXLongOperation.StartOperation(this, () => { throw RunReport(); });

            return adapter.Get();
        }

        public PXAction<AMMultiLevelBomFilter> costedReport;
        [PXUIField(DisplayName = Messages.Costed, MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select)]
        [PXButton]
        protected virtual IEnumerable CostedReport(PXAdapter adapter)
        {
            if (string.IsNullOrWhiteSpace(Filter.Current.BOMID) && Filter.Current.InventoryID == null)
            {
                //Have the user confirm running report for all BOMS
                if (Results.Ask(Messages.ConfirmProcess, Messages.MultiLevelRunAll, MessageButtons.YesNo) != WebDialogResult.Yes)
                {
                    return adapter.Get();
                }
            }

            PXLongOperation.StartOperation(this, () => { throw RunCostedReport(); });

            return adapter.Get();
        }

        public virtual PXReportRequiredException RunReport()
        {
            return RunReport(PXBaseRedirectException.WindowMode.Same);
        }

        public virtual PXReportRequiredException RunCostedReport()
        {
            return RunCostedReport(PXBaseRedirectException.WindowMode.Same);
        }

        /// <summary>
        /// Open the multi-level bom engineering report into a new window
        /// </summary>
        public static void RunReportNewWindow(string bomId, string revisionId)
        {
            if (string.IsNullOrWhiteSpace(bomId) || string.IsNullOrWhiteSpace(revisionId))
            {
                throw new PXException($"{Messages.ReportParametersInvalid}: bomId = {bomId}; revisionId = {revisionId}");
            }

            MultiLevelBomInq reportGraph = PXGraph.CreateInstance<MultiLevelBomInq>();
            reportGraph.Filter.Current = new AMMultiLevelBomFilter {BOMID = bomId, RevisionID = revisionId, BOMDate = reportGraph.Accessinfo.BusinessDate, RollCosts = true, IncludeBomsOnHold = true };
            PXRedirectHelper.TryRedirect(reportGraph, PXRedirectHelper.WindowMode.New);
        }

        public virtual PXReportRequiredException RunReport(PXBaseRedirectException.WindowMode windowMode)
        {
            var reportData = new PXReportResultset(typeof(AMMultiLevelBomData));

            foreach (AMMultiLevelBomData row in Results.Select())
            {
                reportData.Add(row);
            }

            return new PXReportRequiredException(reportData, MultiLevelBomReportID, windowMode, Messages.MultiLevelBOM);
        }

        public virtual PXReportRequiredException RunCostedReport(PXBaseRedirectException.WindowMode windowMode)
        {
            var reportData = new PXReportResultset(typeof(AMMultiLevelBomData));

            foreach (AMMultiLevelBomData row in Results.Select())
            {
                reportData.Add(row);
            }

            return new PXReportRequiredException(reportData, MultiLevelBomCostedReportID, windowMode, Messages.MultiLevelBOMCosted);
        }

        public virtual List<AMMultiLevelBomData> LoadAllData()
        {
            var multiLevelBomRecs = new List<AMMultiLevelBomData>();

            PXSelectBase<AMBomItem> cmdBOM = new PXSelect<AMBomItem>(this);

            if (Filter.Current.BOMID != null)
            {
                cmdBOM.WhereAnd<Where<AMBomItem.bOMID, Equal<Current<AMMultiLevelBomFilter.bOMID>>>>();
            }

            if (Filter.Current.RevisionID != null)
            {
                cmdBOM.WhereAnd<Where<AMBomItem.revisionID, Equal<Current<AMMultiLevelBomFilter.revisionID>>>>();
            }

            if (Filter.Current.InventoryID != null)
            {
                cmdBOM.WhereAnd<Where<AMBomItem.inventoryID, Equal<Current<AMMultiLevelBomFilter.inventoryID>>>>();
            }

            // If the user is going to ask for a specific BOM/Revision then we are going to assume include bom on hold - give us that bom/rev regardless
            var isBomRev = Filter.Current.BOMID != null && Filter.Current.RevisionID != null;
            if (Filter.Current.IncludeBomsOnHold == false && !isBomRev)
            {
                cmdBOM.WhereAnd<Where<AMBomItem.hold, Equal<False>>>();
            }

            if (Filter.Current.BOMDate != null)
            {
                cmdBOM.WhereAnd<Where<Current<AMMultiLevelBomFilter.bOMDate>,
                    Between<AMBomItem.effStartDate, AMBomItem.effEndDate>,
                    Or<Where<AMBomItem.effStartDate, LessEqual<Current<AMMultiLevelBomFilter.bOMDate>>,
                    And<AMBomItem.effEndDate, IsNull>>>>>();
            }

            foreach (AMBomItem bomitem in cmdBOM.Select())
            {
                LoadDataRecords(bomitem.BOMID, bomitem.RevisionID, 0, 1, bomitem, multiLevelBomRecs);
            }

            if (Filter.Current.RollCosts.GetValueOrDefault())
            {
                multiLevelBomRecs = RollCostUpdate(multiLevelBomRecs);
            }

            return multiLevelBomRecs;
        }

        protected virtual bool ExcludeMaterial(AMBomMatl bomMatl, InventoryItem inventoryItem, AMBomItem bomItem, AMBomOper bomOper, DateTime reportDate)
        {
            if (bomMatl?.InventoryID == null || inventoryItem?.InventoryID == null || bomItem?.InventoryID == null)
            {
                return true;
            }

            if (bomMatl.ExpDate != null && bomMatl.ExpDate.GetValueOrDefault() <= reportDate)
            {
#if DEBUG
                AMDebug.TraceWriteMethodName($"ExpDate {bomMatl.ExpDate.GetValueOrDefault().ToShortDateString()} is less or equal to {reportDate.ToShortDateString()} for {inventoryItem.InventoryCD.TrimIfNotNullEmpty()} {bomMatl.DebuggerDisplay}");
#endif
                return true;
            }

            if (bomMatl.EffDate != null && bomMatl.EffDate.GetValueOrDefault() > reportDate)
            {
#if DEBUG
                AMDebug.TraceWriteMethodName($"EffDate {bomMatl.EffDate.GetValueOrDefault().ToShortDateString()} is greater than {reportDate.ToShortDateString()} for {inventoryItem.InventoryCD.TrimIfNotNullEmpty()} {bomMatl.DebuggerDisplay}");
#endif
                return true;
            }

            return false;
        }

        protected virtual AMMultiLevelBomData CreateHeaderRow(AMBomItem parentBomItem, int lineID, int level, decimal? totalQtyReq)
        {
#if DEBUG
            AMDebug.TraceWriteMethodName($"[{level}] {parentBomItem.DebuggerDisplay}");
#endif
            return new AMMultiLevelBomData
            {
                ParentBOMID = parentBomItem.BOMID,
                RevisionID = parentBomItem.RevisionID,
                LineID = lineID,
                Level = level,
                ParentInventoryID = parentBomItem.InventoryID,
                ParentSubItemID = parentBomItem.SubItemID,
                ParentDescr = parentBomItem.Descr,
                EffStartDate = parentBomItem.EffStartDate,
                EffEndDate = parentBomItem.EffEndDate,
                SiteID = parentBomItem.SiteID,
                Status = parentBomItem.Status,
                ManufacturingBOMID = parentBomItem.BOMID,
                ManufacturingRevisionID = parentBomItem.RevisionID,
                TotalQtyReq = totalQtyReq,
                IsHeaderRecord = true
            };
        }

        protected virtual AMMultiLevelBomData CreateDetailRow(AMBomMatl amBomMatl, AMBomOper amBomOper,
            AMBomItem amBomItem, InventoryItem inventoryItem, AMBomItem parentBomItem, int lineID, int level, decimal totalQtyReq,
            AMMultiLevelBomFilter filter, string levelBomid, string levelRevisionID)
        {
            var itemExt = inventoryItem.GetExtension<InventoryItemExt>();

            var qtyRequired = amBomMatl.QtyReq.GetValueOrDefault() * (1 + amBomMatl.ScrapFactor.GetValueOrDefault()) *
                                  (amBomMatl.BatchSize.GetValueOrDefault() == 0m ? 1m
                                      : 1 / amBomMatl.BatchSize.GetValueOrDefault());

            qtyRequired = itemExt.AMQtyRoundUp == false ? qtyRequired :
                Math.Ceiling(qtyRequired);

            var totalQtyRequired = amBomMatl.QtyReq.GetValueOrDefault() * (1 + amBomMatl.ScrapFactor.GetValueOrDefault()) *
                                       (amBomMatl.BatchSize.GetValueOrDefault() == 0m ? 1m
                                           : totalQtyReq / amBomMatl.BatchSize.GetValueOrDefault());

            totalQtyRequired = itemExt.AMQtyRoundUp == false ? totalQtyRequired :
                Math.Ceiling(totalQtyRequired);

            var baseTotalQtyRequired = amBomMatl.BaseQty.GetValueOrDefault() * (1 + amBomMatl.ScrapFactor.GetValueOrDefault()) *
                                           (amBomMatl.BatchSize.GetValueOrDefault() == 0m ? 1m
                                               : totalQtyReq / amBomMatl.BatchSize.GetValueOrDefault());

            baseTotalQtyRequired = itemExt.AMQtyRoundUp == false ? baseTotalQtyRequired :
                Math.Ceiling(baseTotalQtyRequired);

            var row = new AMMultiLevelBomData
            {
                ParentBOMID = parentBomItem.BOMID,
                RevisionID = parentBomItem.RevisionID,
                LineID = lineID,
                Level = level,
                InventoryID = amBomMatl.InventoryID,
                Descr = amBomMatl.Descr,
                ParentInventoryID = parentBomItem.InventoryID,
                ParentSubItemID = parentBomItem.SubItemID,
                ParentDescr = parentBomItem.Descr,
                SubItemID = amBomMatl.SubItemID,
                UOM = amBomMatl.UOM,
                ScrapFactor = amBomMatl.ScrapFactor.GetValueOrDefault(),
                BatchSize = amBomMatl.BatchSize.GetValueOrDefault(),
                BOMQtyReq = amBomMatl.QtyReq.GetValueOrDefault(),
                BaseQtyReq = amBomMatl.BaseQty.GetValueOrDefault(),
                QtyReq = qtyRequired,
                TotalQtyReq = totalQtyRequired,
                BaseTotalQtyReq = baseTotalQtyRequired,
                UnitCost = amBomMatl.UnitCost.GetValueOrDefault(),
                ExtCost = qtyRequired * amBomMatl.UnitCost.GetValueOrDefault(),
                TotalExtCost = totalQtyRequired * amBomMatl.UnitCost.GetValueOrDefault(),
                LineBOMID = amBomMatl.BOMID,
                LineRevisionID = amBomMatl.RevisionID,
                OperationID = amBomOper.OperationID,
                OperationCD = amBomOper.OperationCD,
                EffStartDate = parentBomItem.EffStartDate,
                EffEndDate = parentBomItem.EffEndDate,
                SiteID = parentBomItem.SiteID,
                Status = parentBomItem.Status,
                LineStatus = amBomItem.Status,
                MaterialStatus = amBomItem.Status,
                OperationDescription = amBomOper.Descr,
                WcID = amBomOper.WcID,
                IsHeaderRecord = false,
                SortOrder = amBomMatl.SortOrder
            };

            var materialSiteID = amBomMatl.SiteID ?? amBomItem.SiteID;

#if DEBUG
            AMDebug.TraceWriteMethodName($"[{level}] {inventoryItem.InventoryCD.TrimIfNotNullEmpty()}; {amBomMatl.DebuggerDisplay}; OperationCD = {row.OperationCD}");
#endif

            if (filter.IgnoreReplenishmentSettings.GetValueOrDefault()
                || InventoryHelper.GetReplenishmentSource(this, row.InventoryID, materialSiteID) == INReplenishmentSource.Manufactured)
            {
                levelBomid = amBomMatl.CompBOMID;
                levelRevisionID = amBomMatl.CompBOMRevisionID;

                if (!string.IsNullOrWhiteSpace(levelBomid) && !string.IsNullOrWhiteSpace(levelRevisionID))
                {
                    AMBomItem bomItem = null;
                    if (Filter.Current.IncludeBomsOnHold.GetValueOrDefault())
                    {
                        bomItem = PXSelect<AMBomItem,
                            Where<AMBomItem.bOMID, Equal<Required<AMBomItem.bOMID>>,
                                And<AMBomItem.revisionID, Equal<Required<AMBomItem.revisionID>>,
                                And2<Where<AMBomItem.status, Equal<AMBomStatus.hold>,
                                    Or<AMBomItem.status, Equal<AMBomStatus.active>>>,
                                And<Where<Required<AMBomItem.effStartDate>,
                                    Between<AMBomItem.effStartDate, AMBomItem.effEndDate>,
                                    Or<Where<AMBomItem.effStartDate, LessEqual<Required<AMBomItem.effStartDate>>,
                                    And<AMBomItem.effEndDate, IsNull>>>>>>>
                            >>.Select(this, levelBomid, levelRevisionID, Filter.Current.BOMDate.GetValueOrDefault(), Filter.Current.BOMDate.GetValueOrDefault());
                    }
                    else
                    {
                        bomItem = PXSelect<AMBomItem,
                            Where<AMBomItem.bOMID, Equal<Required<AMBomItem.bOMID>>,
                                And<AMBomItem.revisionID, Equal<Required<AMBomItem.revisionID>>,
                                And<AMBomItem.status, Equal<AMBomStatus.active>,
                                And<Where<Required<AMBomItem.effStartDate>,
                                    Between<AMBomItem.effStartDate, AMBomItem.effEndDate>,
                                    Or<Where<AMBomItem.effStartDate, LessEqual<Required<AMBomItem.effStartDate>>,
                                    And<AMBomItem.effEndDate, IsNull>>>>>>>
                            >>.Select(this, levelBomid, levelRevisionID, Filter.Current.BOMDate.GetValueOrDefault(), Filter.Current.BOMDate.GetValueOrDefault());
                    }

                    if (bomItem == null)
                    {
                        PXTrace.WriteWarning(Messages.GetLocal(Messages.ComponentBOMRevisionNotActive, levelBomid, levelRevisionID, inventoryItem.InventoryCD));
                        return row;
                    }
                }

                if (!string.IsNullOrWhiteSpace(levelBomid) && string.IsNullOrWhiteSpace(levelRevisionID))
                {
                    var compBomItem = filter.IncludeBomsOnHold == false
                        ? PrimaryBomIDManager.GetActiveRevisionBomItemByDate(this, levelBomid, filter.BOMDate.GetValueOrDefault())
                        : PrimaryBomIDManager.GetNotArchivedRevisionBomItemByDate(this, levelBomid, filter.BOMDate.GetValueOrDefault());
                    if (compBomItem == null)
                    {
                        PXTrace.WriteWarning(Messages.GetLocal(Messages.NoActiveRevisionForBom, parentBomItem.BOMID));
                        return row;
                    }

                    levelRevisionID = compBomItem.RevisionID;
                }

                if (string.IsNullOrWhiteSpace(levelBomid))
                {
                    var bomItem = filter.IncludeBomsOnHold == false ?
                        PrimaryBomIDManager.GetActiveRevisionBomItemByDate(this, new PrimaryBomIDManager(this).GetPrimaryAllLevels(row.InventoryID,
                            materialSiteID, row.SubItemID), filter.BOMDate.GetValueOrDefault()) :
                        PrimaryBomIDManager.GetNotArchivedRevisionBomItemByDate(this, new PrimaryBomIDManager(this).GetPrimaryAllLevels(row.InventoryID,
                            materialSiteID, row.SubItemID), filter.BOMDate.GetValueOrDefault());

                    if (bomItem == null)
                    {
                        PXTrace.WriteWarning(Messages.GetLocal(Messages.NoActiveRevisionForBom, parentBomItem.BOMID));
                        return row;
                    }

                    levelBomid = bomItem.BOMID;
                    levelRevisionID = bomItem.RevisionID;
                }

                row.ManufacturingBOMID = levelBomid;
                row.ManufacturingRevisionID = levelRevisionID;
            }

            return row;
        }

        public virtual void LoadDataRecords(string levelBomid, string levelRevisionID, int? level, decimal? totalQtyReq, AMBomItem parentBomItem, List<AMMultiLevelBomData> multiLevelBomRecs)
        {
            if (level == null || level > LowLevel.MaxLowLevel || string.IsNullOrWhiteSpace(levelBomid) || Filter.Current == null)
            {
                return;
            }

            // We need a Header for each new level to add the costs from the Cost Roll and to Insert the record as material
            var headerRow = CreateHeaderRow(parentBomItem, multiLevelBomRecs.Count + 1, level.GetValueOrDefault(), totalQtyReq);
            if (headerRow == null)
            {
                return;
            }

            multiLevelBomRecs.Add(headerRow);

            var bomOpersWithoutMatl = new List<AMBomOper>();
            var includeOpersWithoutMatl = false;
            var includeOperations = Filter?.Current?.IncludeOperations == true;
            var lastOperationCD = string.Empty;
            if(includeOperations)
            {
                bomOpersWithoutMatl = GetOperationsWithoutMaterial(levelBomid, levelRevisionID).ToList();
            }
            includeOpersWithoutMatl = bomOpersWithoutMatl.Count > 0;
            
            foreach (PXResult<AMBomMatl, AMBomItem, AMBomOper, InventoryItem, INItemCost> result in PXSelectJoin<
                AMBomMatl,
                InnerJoin<AMBomItem,
                    On<AMBomMatl.bOMID, Equal<AMBomItem.bOMID>,
                    And<AMBomMatl.revisionID, Equal<AMBomItem.revisionID>>>,
                InnerJoin<AMBomOper,
                    On<AMBomMatl.bOMID, Equal<AMBomOper.bOMID>,
                    And<AMBomMatl.revisionID, Equal<AMBomOper.revisionID>,
                    And<AMBomMatl.operationID, Equal<AMBomOper.operationID>>>>,
                InnerJoin<InventoryItem,
                    On<AMBomMatl.inventoryID, Equal<InventoryItem.inventoryID>>,
                LeftJoin<INItemCost,
                    On<AMBomMatl.inventoryID, Equal<INItemCost.inventoryID>>>>>>,
                Where<AMBomMatl.bOMID, Equal<Required<AMBomMatl.bOMID>>,
                    And<AMBomMatl.revisionID, Equal<Required<AMBomMatl.revisionID>>,
                    And2<
                        Where<AMBomMatl.effDate, IsNull,
                            Or<AMBomMatl.effDate, LessEqual<Current<AMMultiLevelBomFilter.bOMDate>>>>,
                        And<Where<AMBomMatl.expDate, IsNull,
                            Or<AMBomMatl.expDate, GreaterEqual<Current<AMMultiLevelBomFilter.bOMDate>>>>>>>>,
                OrderBy<
                    Asc<AMBomOper.operationCD,
                    Asc<AMBomMatl.sortOrder,
                    Asc<AMBomMatl.lineID>>>>>
                .Select(this, levelBomid, levelRevisionID))
            {
                var amBomMatl = (AMBomMatl) result;
                var amBomItem = (AMBomItem) result;
                var amBomOper = (AMBomOper)result;
                var invItem = (InventoryItem) result;
                var itemCost = (INItemCost)result;

                if (ExcludeMaterial(amBomMatl, invItem, amBomItem, amBomOper, Filter.Current?.BOMDate ?? Accessinfo.BusinessDate.GetValueOrDefault()))
                {
                    continue;
                }

                var row = CreateDetailRow(amBomMatl, amBomOper, amBomItem, invItem, parentBomItem,
                    multiLevelBomRecs.Count + 1, level.GetValueOrDefault(), totalQtyReq.GetValueOrDefault(), Filter.Current, levelBomid,
                    levelRevisionID);
                if (row == null)
                {
                    continue;
                }

                if (itemCost != null && Filter.Current.UseCurrentInventoryCost.GetValueOrDefault())
                {
                    row.UnitCost = BOMCostRoll.GetUnitCostFromINItemCostTable(itemCost) ?? amBomMatl.UnitCost.GetValueOrDefault();
                }

                row.ExtCost = row.QtyReq * row.UnitCost;
                row.TotalExtCost = row.TotalQtyReq * row.UnitCost;

                if(includeOperations && !lastOperationCD.Equals(row.OperationCD))
                {
                    if (includeOpersWithoutMatl)
                    {
                        var indexes2Remove = new List<int>();
                        for (int i = 0; i < bomOpersWithoutMatl.Count; i++)
                        {
                            var op = bomOpersWithoutMatl[i];
                            if (OperationHelper.LessThan(op.OperationCD, row.OperationCD))
                            {
                                indexes2Remove.Add(i);
                                var operBomData = CreateOperationRow(op, parentBomItem, multiLevelBomRecs.Count + 1, level, 0);
                                multiLevelBomRecs.Add(operBomData);
                            }
                        }

                        foreach (var idx in indexes2Remove.OrderByDescending(x => x))
                        {
                            bomOpersWithoutMatl.RemoveAt(idx);
                        }
                    }

                    // include current operation as an entry
                    multiLevelBomRecs.Add(CreateOperationRow(amBomOper, parentBomItem, multiLevelBomRecs.Count + 1, level, 0));
                }

                row.LineID = multiLevelBomRecs.Count + 1;
                multiLevelBomRecs.Add(row);

                if (!string.IsNullOrWhiteSpace(row.ManufacturingBOMID) &&
                    !string.IsNullOrWhiteSpace(row.ManufacturingRevisionID))
                {
                    LoadDataRecords(row.ManufacturingBOMID, row.ManufacturingRevisionID, level + 1, row.BaseTotalQtyReq.GetValueOrDefault(), parentBomItem, multiLevelBomRecs);
                }

                lastOperationCD = row.OperationCD;
            }

            if (includeOpersWithoutMatl)
            {
                foreach (var op in bomOpersWithoutMatl)
                {
                    var operBomData = CreateOperationRow(op, parentBomItem, multiLevelBomRecs.Count + 1, level, 0);
                    multiLevelBomRecs.Add(operBomData);
                } 
            }
        }

        protected virtual AMMultiLevelBomData CreateOperationRow(AMBomOper amBomOper, AMBomItem parentBomItem, int lineID, int? level, decimal totalQtyReq)
        {
            return new AMMultiLevelBomData
            {
                ParentBOMID = parentBomItem.BOMID,
                RevisionID = parentBomItem.RevisionID,
                LineBOMID = amBomOper.BOMID,
                LineRevisionID = amBomOper.RevisionID,
                LineID = lineID,
                Level = level.GetValueOrDefault(),
                ParentInventoryID = parentBomItem.InventoryID,
                ParentSubItemID = parentBomItem.SubItemID,
                ParentDescr = parentBomItem.Descr,
                EffStartDate = parentBomItem.EffStartDate,
                EffEndDate = parentBomItem.EffEndDate,
                SiteID = parentBomItem.SiteID,
                Status = parentBomItem.Status,
                ManufacturingBOMID = parentBomItem.BOMID,
                ManufacturingRevisionID = parentBomItem.RevisionID,
                TotalQtyReq = totalQtyReq,
                IsHeaderRecord = false,
                OperationID = amBomOper.OperationID,
                OperationCD = amBomOper.OperationCD,
                OperationDescription = amBomOper.Descr,
                WcID = amBomOper.WcID
            };
        }

        protected IEnumerable<AMBomOper> GetOperationsWithoutMaterial(string bomId, string revisionId)
        {
            foreach (AMBomOper bomOper in PXSelectReadonly2<
                AMBomOper,
                LeftJoin<AMBomMatl,
                    On<
                    AMBomOper.bOMID, Equal<AMBomMatl.bOMID>,
                    And<AMBomOper.revisionID, Equal<AMBomMatl.revisionID>,
                    And<AMBomOper.operationID, Equal<AMBomMatl.operationID>>>>>,
                Where<AMBomOper.bOMID, Equal<Required<AMBomOper.bOMID>>,
                    And<AMBomOper.revisionID, Equal<Required<AMBomOper.revisionID>>,
                    And<AMBomMatl.inventoryID, IsNull>
                    >>>
                .Select(this, bomId, revisionId))
            {
                if (bomOper?.OperationID == null)
                {
                    continue;
                }

                yield return bomOper;
            }
        }

        protected virtual void AMMultiLevelBomFilter_BOMID_FieldUpdated(PXCache cache, PXFieldUpdatedEventArgs e)
        {
            var amMultiFilter = (AMMultiLevelBomFilter)e.Row;
            if (amMultiFilter == null)
            {
                return;
            }

            amMultiFilter.RevisionID = null;
        }

        protected virtual AMMultiLevelBomData RollCostUpdate(AMMultiLevelBomData multiLevelRecord, AMBomCost bomCostRec)
        {
            if (multiLevelRecord == null)
            {
                return null;
            }

            var row = multiLevelRecord.Copy<AMMultiLevelBomData>();

            row.HasCostRoll = false;

            if (bomCostRec?.BOMID == null)
            {
                return row;
            }

            row.HasCostRoll = true;
            row.LotSize = bomCostRec.LotSize;
            row.FixedLaborTime = bomCostRec.FixedLaborTime;
            row.VariableLaborTime = (int?)(bomCostRec.VariableLaborTime * multiLevelRecord.TotalQtyReq);
            row.MachineTime = (int?)(bomCostRec.MachineTime * multiLevelRecord.TotalQtyReq);
            row.UnitCost = bomCostRec.UnitCost;
            row.ToolCost = bomCostRec.ToolCost * multiLevelRecord.TotalQtyReq;
            row.MatlManufacturedCost = bomCostRec.MatlManufacturedCost * multiLevelRecord.TotalQtyReq;
            row.MatlNonManufacturedCost = bomCostRec.MatlNonManufacturedCost * multiLevelRecord.TotalQtyReq;
            row.FixedLaborCost = bomCostRec.FLaborCost;
            row.VariableLaborCost = bomCostRec.VLaborCost * multiLevelRecord.TotalQtyReq;
            row.FixedOvdCost = bomCostRec.FOvdCost;
            row.VariableOvdCost = bomCostRec.VOvdCost * multiLevelRecord.TotalQtyReq;
            row.MachineCost = bomCostRec.MachCost * multiLevelRecord.TotalQtyReq;
            row.ExtCost = bomCostRec.UnitCost * multiLevelRecord.QtyReq;
            row.TotalExtCost = bomCostRec.UnitCost * multiLevelRecord.TotalQtyReq;
            row.TotalCost = bomCostRec.UnitCost * row.LotSize; // For MLB Report Header only

            return row;
        }

        public virtual List<AMMultiLevelBomData> RollCostUpdate(List<AMMultiLevelBomData> multiLevelBomRecs)
        {
            var newMultiLevelBomRecs = new List<AMMultiLevelBomData>();
            var rollBomList = new RollBomList();
            var uniqueBoms = new HashSet<string>();
            AMBomItem bomItem;

            foreach (var multiLevelRecord in multiLevelBomRecs)
            {
                var bomKey = string.Join(":", multiLevelRecord.LineBOMID, multiLevelRecord.LineRevisionID);

                if (!uniqueBoms.Add(bomKey) || multiLevelRecord.IsHeaderRecord == true || multiLevelRecord.InventoryID == null)
                {
                    //Repeat bom/rev
                    continue;
                }

                bomItem = (AMBomItem)this.Caches<AMBomItem>().Locate(new AMBomItem { BOMID = multiLevelRecord.LineBOMID, RevisionID = multiLevelRecord.LineRevisionID });
                if (bomItem == null)
                {
                    bomItem = PXSelect<AMBomItem,
                        Where<AMBomItem.bOMID, Equal<Required<AMBomItem.bOMID>>,
                            And<AMBomItem.revisionID, Equal<Required<AMBomItem.revisionID>>
                            >>>.Select(this, multiLevelRecord.LineBOMID, multiLevelRecord.LineRevisionID);
                }
                rollBomList.Add(bomItem, multiLevelRecord.Level.GetValueOrDefault(), false);
            }

            var costRollGraph = CreateInstance<BOMCostRoll>();
            var costRollFilter = new RollupSettings
            {
                SnglMlti = "M",
                SiteID = null,
                InventoryID = Filter.Current.InventoryID,
                SubItemID = null,
                BOMID = Filter.Current.BOMID,
                RevisionID = Filter.Current.RevisionID,
                EffectiveDate = Filter.Current.BOMDate,
                IncMatScrp = true,
                IncFixed = true,
                UpdateMaterial = false,
                UsePending = false,
                IgnoreMinMaxLotSizeValues = Filter.Current.IgnoreMinMaxLotSizeValues
            };
            costRollGraph.Settings.Current = costRollFilter;
            costRollGraph.RollCosts(rollBomList);

            foreach (var mutliLevelRecord in multiLevelBomRecs)
            {
                AMBomCost bomCostRec = null;
                if (!string.IsNullOrWhiteSpace(mutliLevelRecord.ManufacturingBOMID) && !string.IsNullOrWhiteSpace(mutliLevelRecord.ManufacturingRevisionID))
                {
                    bomCostRec = (AMBomCost)costRollGraph.Caches<AMBomCost>().Locate(new AMBomCost
                    {
                        BOMID = mutliLevelRecord.ManufacturingBOMID,
                        RevisionID = mutliLevelRecord.ManufacturingRevisionID,
                        UserID = costRollGraph.Accessinfo.UserID
                    });
                }

                var row = RollCostUpdate(mutliLevelRecord, bomCostRec);

                if (row == null)
                {
                    continue;
                }

                newMultiLevelBomRecs.Add(row);
            }
            return newMultiLevelBomRecs;
        }

        protected virtual void AMMultiLevelBomFilter_RowSelected(PXCache sender, PXRowSelectedEventArgs e)
        {
            var filter = (AMMultiLevelBomFilter)e.Row;
            PXUIFieldAttribute.SetEnabled<AMMultiLevelBomFilter.ignoreMinMaxLotSizeValues>(sender, filter, filter.RollCosts == true);
        }

        protected virtual void AMMultiLevelBomFilter_RollCosts_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
        {
            var filter = (AMMultiLevelBomFilter)e.Row;

            if(filter.RollCosts == false)
            {
                filter.IgnoreMinMaxLotSizeValues = false;
            }
        }
    }

    [PXCacheName("Multi Level BOM Report Filter")]
    [Serializable]
    public class AMMultiLevelBomFilter : IBqlTable
    {
        #region BOMID
        public abstract class bOMID : PX.Data.BQL.BqlString.Field<bOMID> { }

        protected String _BOMID;
        [BomID]
        [BOMIDSelector]
        public virtual String BOMID
        {
            get { return this._BOMID; }
            set { this._BOMID = value; }
        }

        #endregion

        #region RevisionID
        public abstract class revisionID : PX.Data.BQL.BqlString.Field<revisionID> { }

        protected String _RevisionID;
        [RevisionIDField]
        [PXSelector(typeof(Search<AMBomItem.revisionID,
                Where<AMBomItem.bOMID, Equal<Current<AMMultiLevelBomFilter.bOMID>>>>)
            , typeof(AMBomItem.revisionID)
            , typeof(AMBomItem.status)
            , typeof(AMBomItem.descr)
            , typeof(AMBomItem.effStartDate)
            , typeof(AMBomItem.effEndDate)
            , DescriptionField = typeof(AMBomItem.descr))]
        public virtual String RevisionID
        {
            get
            {
                return this._RevisionID;
            }
            set
            {
                this._RevisionID = value;
            }
        }
        #endregion

        #region InventoryID
        public abstract class inventoryID : PX.Data.BQL.BqlInt.Field<inventoryID> { }

        protected Int32? _InventoryID;
        [StockItem]
        public virtual Int32? InventoryID
        {
            get { return this._InventoryID; }
            set { this._InventoryID = value; }
        }

        #endregion

        #region IgnoreReplenishmentSettings
        public abstract class ignoreReplenishmentSettings : PX.Data.BQL.BqlBool.Field<ignoreReplenishmentSettings> { }

        protected Boolean? _IgnoreReplenishmentSettings;
        /// <summary>
        /// When checked the exploded levels will ignore the items replenishment settings for manufactured items only. 
        /// If a default bom is found it will be used during processing
        /// </summary>
        [PXBool]
        [PXUnboundDefault(false)]
        [PXUIField(DisplayName = "Ignore Replenishment Source")]
        public virtual Boolean? IgnoreReplenishmentSettings
        {
            get { return this._IgnoreReplenishmentSettings; }
            set { this._IgnoreReplenishmentSettings = value; }
        }

        #endregion

        #region IncludeBomsOnHold
        public abstract class includeBomsOnHold : PX.Data.BQL.BqlBool.Field<includeBomsOnHold> { }

        protected bool? _IncludeBomsOnHold;
        [PXBool]
        [PXUIField(DisplayName = "Include BOMs on hold")]
        [PXUnboundDefault(false)]
        public virtual bool? IncludeBomsOnHold
        {
            get { return this._IncludeBomsOnHold; }
            set { this._IncludeBomsOnHold = value; }
        }

        #endregion

        #region BOMDate
        public abstract class bOMDate : PX.Data.BQL.BqlDateTime.Field<bOMDate> { }
        protected DateTime? _BOMDate;
        [PXDate]
        [PXDefault(typeof(AccessInfo.businessDate), PersistingCheck = PXPersistingCheck.Nothing)]
        [PXUIField(DisplayName = "Date", Visibility = PXUIVisibility.SelectorVisible)]
        public virtual DateTime? BOMDate
        {
            get
            {
                return this._BOMDate;
            }
            set
            {
                this._BOMDate = value;
            }
        }
        #endregion

        #region RollCosts
        public abstract class rollCosts : PX.Data.BQL.BqlBool.Field<rollCosts> { }

        protected Boolean? _RollCosts;
        /// <summary>
        /// When checked, will run Bom Cost Roll prior to loading the reports
        /// </summary>
        [PXBool]
        [PXUnboundDefault(true)]
        [PXUIField(DisplayName = "Roll Costs")]
        public virtual Boolean? RollCosts
        {
            get { return this._RollCosts; }
            set { this._RollCosts = value; }
        }

        #endregion

        #region UseCurrentInventoryCost
        public abstract class useCurrentInventoryCost : PX.Data.BQL.BqlBool.Field<useCurrentInventoryCost> { }

        protected bool? _UseCurrentInventoryCost;
        /// <summary>
        /// When checked, will use current Inventory Unit Cost
        /// </summary>
        [PXBool]
        [PXUnboundDefault(true)]
        [PXUIField(DisplayName = "Use Current Inventory Cost")]
        public virtual bool? UseCurrentInventoryCost
        {
            get { return this._UseCurrentInventoryCost; }
            set { this._UseCurrentInventoryCost = value; }
        }

        #endregion

        #region IgnoreMinMaxLotSizeValues
        public abstract class ignoreMinMaxLotSizeValues : PX.Data.BQL.BqlBool.Field<ignoreMinMaxLotSizeValues> { }

        protected Boolean? _IgnoreMinMaxLotSizeValues;
        [PXDBBool]
        [PXDefault(false)]
        [PXUIField(DisplayName = "Ignore Min/Max/Lot Size Values")]
        public virtual Boolean? IgnoreMinMaxLotSizeValues
        {
            get
            {
                return this._IgnoreMinMaxLotSizeValues;
            }
            set
            {
                this._IgnoreMinMaxLotSizeValues = value;
            }
        }
        #endregion

        #region IncludeOperations
        public abstract class includeOperations : PX.Data.BQL.BqlBool.Field<includeOperations> { }

        protected Boolean? _IncludeOperations;
        [PXDBBool]
        [PXDefault(false)]
        [PXUIField(DisplayName = "Include Operations")]
        public virtual Boolean? IncludeOperations
        {
            get
            {
                return this._IncludeOperations;
            }
            set
            {
                this._IncludeOperations = value;
            }
        }
        #endregion
    }
}