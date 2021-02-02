using System;
using PX.Data;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using PX.Objects.AM.GraphExtensions;
using PX.Common;
using PX.Objects.AM.CacheExtensions;
using PX.Objects.CS;
using PX.Objects.IN;

namespace PX.Objects.AM
{
    using PX.Objects.AM.Attributes;

    public class BOMCostRoll : PXGraph<BOMCostRoll>
    {
        public PXFilter<RollupSettings> Settings;
        [PXViewName(Messages.BOMCost)]
        public PXSelectJoin<AMBomCost,
            InnerJoin<AMBomItem, On<AMBomCost.bOMID, Equal<AMBomItem.bOMID>,
                And<AMBomCost.revisionID, Equal<AMBomItem.revisionID>>>>,
            Where<AMBomCost.userID, Equal<Current<AccessInfo.userID>>>> BomCostRecs;
        public PXSelect<AMBomMatl> BomMatlRecs;
        public PXSelect<InventoryItem> InvItemRecs;
        public PXSelect<INItemSite> ItemSiteRecs;
        //Required to update InventoryItem
        public PXSetup<CommonSetup> CommonSetupView;

        //Required when updating InventoryItem and/or INItemSite
        public PXSetup<INSetup> INSetup;

        //Required to control Visibility of the Archive button
        public PXSetup<AMBSetup> AMBomSetup;
        public PXSelect<AMBomCostHistory> BomCostHistoryRecs;

        public BOMCostRoll()
        {
            BomCostRecs.AllowDelete = false;
            BomCostRecs.AllowInsert = false;
            PXUIFieldAttribute.SetEnabled(BomCostRecs.Cache, null, false);
            PXUIFieldAttribute.SetEnabled<AMBomCost.selected>(BomCostRecs.Cache, null, true);
            ViewBOM.SetEnabled(true);

            Archive.SetVisible(AMBomSetup?.Current?.AllowArchiveWithoutUpdatePending == true);
        }

        public PXAction<RollupSettings> ViewBOM;

        [PXUIField(DisplayName = "View BOM")]
        [PXButton]
        protected virtual void viewBOM()
        {
            BOMMaint.Redirect(BomCostRecs?.Current?.BOMID, BomCostRecs?.Current?.RevisionID);
        }

        public PXAction<RollupSettings> start;
        [PXUIField(DisplayName = Messages.RollCosts)]
        [PXProcessButton]
        protected virtual IEnumerable Start(PXAdapter adapter)
        {
            PXLongOperation.StartOperation(this, () => RollCostsRetry(this.Settings.Current));
            return adapter.Get();
        }

        public PXAction<RollupSettings> Archive;

        [PXUIField(DisplayName = "Archive")]
        [PXButton]
        protected IEnumerable archive(PXAdapter adapter)
        {
            PXLongOperation.StartOperation(this, ArchiveBomCostRecords);
            return adapter.Get();
        }

        /// <summary>
        /// Indicates if the process is running for multi level
        /// </summary>
        public virtual bool IsMultiLevel => Settings?.Current != null && Settings.Current.SnglMlti == RollupSettings.SelectOptSM.Multi;

        /// <summary>
        /// Perform cost roll with retry on PXExceptions
        /// </summary>
        /// <param name="filter"></param>
        public static void RollCostsRetry(RollupSettings filter)
        {
            var retryCnt = 1;
            while (true)
            {
                try
                {
                    RollCosts(filter);
                    break;
                }
                catch (PXException pe)
                {
                    if (retryCnt-- < 0)
                    {
                        throw;
                    }

                    PXTrace.WriteError(pe);
                }
            }
        }

        public static void RollCosts(RollupSettings filter)
        {
            var costRollGraph = CreateInstance<BOMCostRoll>();
            costRollGraph.Settings.Current = filter;
            costRollGraph.RollCosts();
        }

        /// <summary>
        /// New call to roll costs
        /// </summary>
        protected virtual void RollCosts()
        {
            var isPersistMode = Settings?.Current?.IsPersistMode == true;
            var processedAll = true;
            if (isPersistMode)
            {
                DeleteUserCostRollData();
            }

            this.BomCostRecs.Cache.Clear();
#if DEBUG
            var sw = System.Diagnostics.Stopwatch.StartNew();
#endif
            var resultset = BuildBomItemResultSet();

            if (!IsMultiLevel)
            {
                foreach (PXResult<AMBomItem, AMBomItemActiveAggregate, AMBomItemBomDefaults> result in resultset)
                {
                    if (!ProcessCost(result, 0, IsDefaultBOM((AMBomItem)result, (AMBomItemBomDefaults)result, ((AMBomItemActiveAggregate)result)?.RevisionID)))
                    {
                        processedAll = false;
                    }
                }

                if (isPersistMode)
                {
                    Actions.PressSave();
                }

                if (!processedAll)
                {
                    throw new PXSetPropertyException<RollupSettings.snglMlti>(Messages.InvalidValuesOnOneOrMoreBOMS,
                        PXErrorLevel.Warning);
                }

                return;
            }

            var bomList = new RollBomList();
            var levelBomList = new List<AMBomItem>();
            foreach (PXResult<AMBomItem, AMBomItemActiveAggregate, AMBomItemBomDefaults> result in resultset)
            {
                var bomItem = (AMBomItem) result;
                if (bomList.Add(bomItem, 0, IsDefaultBOM((AMBomItem)result, (AMBomItemBomDefaults)result, ((AMBomItemActiveAggregate)result)?.RevisionID)))
                {
                    levelBomList.Add(bomItem);
                }
            }
#if DEBUG
            var cntr = 0;
#endif
            foreach (var bomItem in levelBomList)
            {
#if DEBUG
                AMDebug.TraceWriteLine($"[{1}-{++cntr}] DrillDown({bomItem.BOMID}, {bomItem.RevisionID}, {1}, bomListCount: {bomList.Count})".Indent(1));
#endif
                DrillDown(bomItem.BOMID, bomItem.RevisionID, ref bomList);
            }

            processedAll = RollCosts(bomList);
            if (isPersistMode)
            {
                Actions.PressSave();
            }

#if DEBUG
            sw.Stop();
            AMDebug.TraceWriteLine("CostRoll - Total Process Time: {0}", PXTraceHelper.CreateTimespanMessage(sw.Elapsed));
#endif
            var warningMsg = string.Empty;

            if (!processedAll || Settings?.Current?.FoundRecursiveBom == true)
            {
                warningMsg = Messages.GetLocal(Messages.InvalidValuesOnOneOrMoreBOMS);
            }

            if(string.IsNullOrWhiteSpace(warningMsg))
            {
                return;
            }

            if (IsImport || IsContractBasedAPI)
            {
                PXTrace.WriteWarning(warningMsg);
                return;
            }

            throw new PXException(warningMsg);
        }

        protected virtual PXResultset<AMBomItem> BuildBomItemResultSet()
        {
            var cmdBOM = string.IsNullOrWhiteSpace(Settings.Current.BOMID)
                ? (PXSelectBase<AMBomItem>) new PXSelectJoin<AMBomItem,
                    InnerJoin<AMBomItemActiveAggregate,
                        On<AMBomItem.bOMID, Equal<AMBomItemActiveAggregate.bOMID>>,
                    LeftJoin<AMBomItemBomDefaults,
                        On<AMBomItem.bOMID, Equal<AMBomItemBomDefaults.bOMID>,
                            And<AMBomItem.revisionID, Equal<AMBomItemBomDefaults.revisionID>>>,
                    InnerJoin<InventoryItem,
                        On<AMBomItem.inventoryID, Equal<InventoryItem.inventoryID>>>>>,
                    Where<AMBomItem.status, NotEqual<AMBomStatus.archived>>>(this)
                : (PXSelectBase<AMBomItem>) new PXSelectJoin<AMBomItem,
                    LeftJoin<AMBomItemActiveAggregate,
                        On<AMBomItem.bOMID, Equal<AMBomItemActiveAggregate.bOMID>>,
                    LeftJoin<AMBomItemBomDefaults,
                        On<AMBomItem.bOMID, Equal<AMBomItemBomDefaults.bOMID>,
                            And<AMBomItem.revisionID, Equal<AMBomItemBomDefaults.revisionID>>>,
                    InnerJoin<InventoryItem,
                        On<AMBomItem.inventoryID, Equal<InventoryItem.inventoryID>>>>>>(this);

            if (Settings.Current.ItemClassID != null)
            {
                cmdBOM.WhereAnd<Where<InventoryItem.itemClassID, Equal<Current<RollupSettings.itemClassID>>>>();
            }

            if (Settings.Current.SiteID != null)
            {
                cmdBOM.WhereAnd<Where<AMBomItem.siteID, Equal<Current<RollupSettings.siteID>>>>();
            }

            if (Settings.Current.InventoryID != null)
            {
                cmdBOM.WhereAnd<Where<AMBomItem.inventoryID, Equal<Current<RollupSettings.inventoryID>>>>();
            }

            if (PXAccess.FeatureInstalled<FeaturesSet.subItem>() && Settings.Current.SubItemID != null)
            {
                cmdBOM.WhereAnd<Where<AMBomItem.subItemID, Equal<Current<RollupSettings.subItemID>>>>();
            }

            if (!string.IsNullOrWhiteSpace(Settings.Current.BOMID))
            {
                cmdBOM.WhereAnd<Where<AMBomItem.bOMID, Equal<Current<RollupSettings.bOMID>>>>();
            }

            if (!string.IsNullOrWhiteSpace(Settings.Current.RevisionID))
            {
                cmdBOM.WhereAnd<Where<AMBomItem.revisionID, Equal<Current<RollupSettings.revisionID>>>>();
            }

            if (Settings.Current.EffectiveDate != null)
            {
                cmdBOM.WhereAnd<Where<AMBomItem.effStartDate, IsNull,
                            Or<AMBomItem.effStartDate, LessEqual<Current<RollupSettings.effectiveDate>>,
                        And<Where<AMBomItem.effEndDate, IsNull,
                            Or<AMBomItem.effEndDate, GreaterEqual<Current<RollupSettings.effectiveDate>>>>>>>>();
            }

            return cmdBOM.Select();
        }

        public PXAction<RollupSettings> updpnd;
        [PXUIField(DisplayName = Messages.UpdatePending)]
        [PXProcessButton]
        protected virtual IEnumerable UpdPnd(PXAdapter adapter)
        {
            PXLongOperation.StartOperation(this, UpdatePending);
            return adapter.Get();
        }

        /// <summary>
        /// Update the pending standard cost for each BOM cost item
        /// </summary>
        protected virtual void UpdatePending()
        {
            foreach (PXResult<INItemSite, AMBomCost, InventoryItem> result in PXSelectJoin<
                    INItemSite,
                    InnerJoin<AMBomCost,
                        On<INItemSite.inventoryID, Equal<AMBomCost.inventoryID>>,
                    InnerJoin<InventoryItem,
                        On<AMBomCost.inventoryID, Equal<InventoryItem.inventoryID>>>>,
                    Where<AMBomCost.userID, Equal<Current<AccessInfo.userID>>,
                        And<InventoryItem.valMethod, Equal<INValMethod.standard>,
                        And<Where<InventoryItemExt.aMBOMID, Equal<AMBomCost.bOMID>,
                            Or<InventoryItemExt.aMBOMID, IsNull>>>>>,
                    OrderBy<
                        Asc<INItemSite.inventoryID,
                            Asc<AMBomCost.isDefaultBom,
                                Asc<AMBomCost.bOMID,
                                    Asc<AMBomCost.revisionID,
                                        Asc<INItemSite.siteID>>>>>>>
                .Select(this))
            {
                var bomCost = (AMBomCost) result;
                var inventoryItem = (InventoryItem) result;
                var itemSite = (INItemSite) result;

                if (bomCost.Selected != true || bomCost?.InventoryID == null || bomCost.UnitCost.GetValueOrDefault() < 0 || inventoryItem?.InventoryID == null || itemSite?.InventoryID == null)
                {
                    continue;
                }

                inventoryItem.PendingStdCost = bomCost.UnitCost.GetValueOrDefault();
                inventoryItem.PendingStdCostDate = Accessinfo.BusinessDate;
                InvItemRecs.Update(inventoryItem);
#if DEBUG
                INSite inSite = PXSelect<INSite, Where<INSite.siteID, Equal<Required<INSite.siteID>>>>.Select(this, itemSite.SiteID);
                if (inSite != null)
                {
                    AMDebug.TraceWriteMethodName($"Updating item {inventoryItem.InventoryCD.TrimIfNotNullEmpty()} at warehouse {inSite.SiteCD.TrimIfNotNullEmpty()} pending cost to {bomCost.UnitCost.GetValueOrDefault()} from BOM {bomCost.BOMID.TrimIfNotNullEmpty()}");
                }
                else
                {
                    AMDebug.TraceWriteMethodName("No INSite record found");
                }
#endif

                itemSite.PendingStdCost = bomCost.UnitCost.GetValueOrDefault();
                itemSite.PendingStdCostDate = Accessinfo.BusinessDate;
                ItemSiteRecs.Update(itemSite);
            }

            UpdateItemSiteStdCostOverrides();

            if (AMBomSetup.Current.AutoArchiveWhenUpdatePending == true)
            {
                ArchiveBomCostRecords();
            }

            Actions.PressSave();
        }

        /// <summary>
        /// Handle standard cost overrides at the site level using primary bom for the site
        /// </summary>
        protected virtual void UpdateItemSiteStdCostOverrides()
        {
            foreach (PXResult<AMBomCost, INItemSite> result in PXSelectJoin<
                AMBomCost,
                InnerJoin<INItemSite,
                    On<AMBomCost.inventoryID, Equal<INItemSite.inventoryID>,
                    And<AMBomCost.siteID, Equal<INItemSite.siteID>>>>,
                Where<AMBomCost.userID, Equal<Current<AccessInfo.userID>>,
                    And<INItemSite.valMethod, Equal<INValMethod.standard>,
                    And<INItemSite.stdCostOverride, Equal<boolTrue>,
                    And<Where<INItemSiteExt.aMBOMID, Equal<AMBomCost.bOMID>,
                        Or<INItemSiteExt.aMBOMID, IsNull>>>>>>>
                .Select(this))
            {
                var amBomCost = (AMBomCost)result;
                var inItemSite = (INItemSite)result;

                if (amBomCost?.InventoryID == null || amBomCost.UnitCost.GetValueOrDefault() < 0 ||
                    amBomCost.SiteID == null || inItemSite?.InventoryID == null || inItemSite.SiteID == null)
                {
                    continue;
                }
#if DEBUG
                INSite inSite = PXSelect<INSite, Where<INSite.siteID, Equal<Required<INSite.siteID>>>>.Select(this, inItemSite.SiteID);
                if (inSite != null)
                {
                    AMDebug.TraceWriteMethodName($"Updating item ID [{amBomCost.InventoryID}] at warehouse {inSite.SiteCD.TrimIfNotNullEmpty()} pending cost to {amBomCost.UnitCost.GetValueOrDefault()} from BOM {amBomCost.BOMID.TrimIfNotNullEmpty()}");
                }
                else
                {
                    AMDebug.TraceWriteMethodName("No INSite record found");
                }
#endif

                INItemSite itemSite = this.ItemSiteRecs.Locate(inItemSite) ?? inItemSite;
                itemSite.PendingStdCost = amBomCost.UnitCost.GetValueOrDefault();
                itemSite.PendingStdCostDate = Accessinfo.BusinessDate;
                ItemSiteRecs.Update(itemSite);
            }
        }

        protected virtual void DrillDown(string bomID, string revisionID, ref RollBomList bomList)
        {
            DrillDown(bomID, revisionID, 1, ref bomList, null);
        }

        /// <summary>
        /// Drill down to lower levels and save the BOMs captured in the process
        /// </summary>
        /// <param name="bomID">Current BOM to check for material with BOMs</param>
        /// <param name="revisionID">Current BOM revision</param>
        /// <param name="level">current processing level</param>
        /// <param name="bomList">object containing the rolling BOM list</param>
        /// <param name="drillDownBoms">in the current line of processing we want to record what we already processed so we can avoid recursives</param>
        protected virtual void DrillDown(string bomID, string revisionID, int level, ref RollBomList bomList, AMBomItem[] drillDownBoms)
        {
            if (level >= LowLevel.MaxLowLevel)
            {
#if DEBUG
                AMDebug.TraceWriteLine($"*****{Messages.MaxLevelsReached}*****");
#endif
                PXTrace.WriteWarning(Messages.GetLocal(Messages.MaxLevelsReached));
                return;
            }

            if (bomList == null)
            {
                throw new ArgumentNullException(nameof(bomList));
            }

            var bomRevs = new AMBomItem[LowLevel.MaxLowLevel];
            if (drillDownBoms != null)
            {
                Array.Copy(drillDownBoms, bomRevs, LowLevel.MaxLowLevel);
            }

            var levelBomList = new List<AMBomItem>();

            foreach (PXResult<AMBomMatlBomCostDrillDown, AMBomItem, AMBomItemActiveAggregate, AMBomItemBomDefaults> result in PXSelectJoin<
                AMBomMatlBomCostDrillDown,
                InnerJoin<AMBomItem,
                    On<AMBomMatlBomCostDrillDown.inventoryID, Equal<AMBomItem.inventoryID>>,
                InnerJoin<AMBomItemActiveAggregate,
                    On<AMBomItem.bOMID, Equal<AMBomItemActiveAggregate.bOMID>>,
                LeftJoin<AMBomItemBomDefaults,
                    On<AMBomItem.bOMID, Equal<AMBomItemBomDefaults.bOMID>,
                    And<AMBomItem.revisionID, Equal<AMBomItemBomDefaults.revisionID>>>>>>,
                Where<AMBomMatlBomCostDrillDown.bOMID, Equal<Required<AMBomMatlBomCostDrillDown.bOMID>>,
                    And<AMBomMatlBomCostDrillDown.revisionID, Equal<Required<AMBomMatlBomCostDrillDown.revisionID>>,
                    And<AMBomItem.status, In<Required<AMBomItem.status>>>>>,
                OrderBy<
                    Asc<AMBomMatlBomCostDrillDown.operationCD,
                    Asc<AMBomMatlBomCostDrillDown.sortOrder,
                    Asc<AMBomMatlBomCostDrillDown.lineID>>>>
                        >
                .Select(this, bomID, revisionID, new int?[] { AMBomStatus.Hold, AMBomStatus.Active }))
            {
                var bomItem = (AMBomItem) result;
                var bomMatl = (AMBomMatlBomCostDrillDown) result;
                if (string.IsNullOrWhiteSpace(bomItem?.BOMID) || string.IsNullOrWhiteSpace(bomMatl.BOMID))
                {
                    continue;
                }

                //To exclude the date fields from the query for performance we will check here... (usage of these dates is not the norm)
                if (Settings?.Current?.EffectiveDate != null &&
                    (bomMatl.EffDate != null && bomMatl.EffDate.GreaterThan(Settings.Current.EffectiveDate)
                     || bomMatl.ExpDate != null && bomMatl.ExpDate.LessThanOrEqualTo(Settings.Current.EffectiveDate)))
                {
                    // Material that is no longer effective or not yet effective is excluded from this process
                    continue;
                }

                var isRecursive = ContainsBom(bomRevs, bomItem);
                bomList.Add(bomItem, isRecursive ? LowLevel.MaxLowLevel : level, IsDefaultBOM(bomItem, (AMBomItemBomDefaults)result, ((AMBomItemActiveAggregate)result)?.RevisionID));

                if (isRecursive)
                {
                    var recursiveMessage = GetRecursiveMessage(bomRevs, bomItem);
                    if(recursiveMessage == null)
                    {
                        continue;
                    }
#if DEBUG
                    AMDebug.TraceWriteMethodName(recursiveMessage);
#endif
                    if (Settings?.Current != null)
                    {
                        Settings.Current.FoundRecursiveBom = true;
                    }

                    PXTrace.WriteWarning(recursiveMessage);
                    continue;
                }

                if (!bomList.LastAddIsNewLevel)
                {
                    // These items have already been processed at the same or higher level - skip
                    continue;
                }

                levelBomList.Add(bomItem);
            }

#if DEBUG
            var cntr = 0;
#endif
            foreach (var bomItem in levelBomList)
            {
#if DEBUG
                var nxtLevel = level + 1;
                AMDebug.TraceWriteLine($"[{nxtLevel}-{++cntr}] DrillDown({bomItem.BOMID}, {bomItem.RevisionID}, {nxtLevel}, bomListCount: {bomList.Count})   [From {bomID}:{revisionID}]".Indent(nxtLevel));
#endif
                if (level.BetweenInclusive(1, LowLevel.MaxLowLevel))
                {
                    bomRevs[level - 1] = bomItem;
                }
                DrillDown(bomItem.BOMID, bomItem.RevisionID, level + 1, ref bomList, bomRevs);
            }
        }

        protected static bool ContainsBom(AMBomItem[] boms, AMBomItem bom)
        {
            return boms != null && bom?.BOMID != null &&
                   Array.Find(boms, b => b?.BOMID == bom.BOMID && b?.RevisionID == bom.RevisionID) != null;
        }

        protected virtual string GetRecursiveMessage(AMBomItem[] bomLevels, AMBomItem recursiveBom)
        {
            if (bomLevels == null || bomLevels.Length == 0 || recursiveBom?.BOMID == null)
            {
                return null;
            }

            //TODO - not ideal but sub query the InventoryItem records so we can print the CD value for Inventory ID for improved message

            var sb = new System.Text.StringBuilder();
            sb.AppendLine(Messages.GetLocal(Messages.RecursiveBomFound, recursiveBom.BOMID, recursiveBom.RevisionID));
            var lastLevel = -1;
            for (var i = 0; i < bomLevels.Length; i++)
            {
                var bom = bomLevels[i];
                if (bom?.BOMID == null)
                {
                    continue;
                }

                lastLevel = i + 1;

                var inventoryCD = InventoryItem.PK.Find(this, bom.InventoryID)?.InventoryCD ?? string.Empty;

                sb.Append($"[{i + 1}] {bom.BOMID}:{bom.RevisionID} {inventoryCD} -> ");
            }

            if (lastLevel != -1)
            {
                var inventoryCD = InventoryItem.PK.Find(this, recursiveBom.InventoryID)?.InventoryCD ?? string.Empty;
                sb.Append($"[{lastLevel+1}] {recursiveBom.BOMID}:{recursiveBom.RevisionID} {inventoryCD}");
            }

            return sb.ToString();
        }

        [Obsolete(InternalMessages.MethodIsObsoleteAndWillBeRemoved2020R2)]
        protected virtual bool IsDefaultBOM(AMBomItem bomItem, INItemSite inItemSite, string defaultActiveRev)
        {
            return IsMatchingRevision(bomItem, defaultActiveRev) && IsDefaultBOM(bomItem, inItemSite);
        }

        protected virtual bool IsDefaultBOM(AMBomItem bomItem, AMBomItemBomDefaults bomDefault, string defaultActiveRev)
        {
            return IsMatchingRevision(bomItem, defaultActiveRev) && bomDefault?.IsDefaultBOM == true;
        }

        protected virtual bool IsMatchingRevision(AMBomItem bomItem, string defaultActiveRev)
        {
            return !string.IsNullOrWhiteSpace(defaultActiveRev) && bomItem?.RevisionID != null &&
                   bomItem.RevisionID.Equals(defaultActiveRev);
        }

        /// <summary>
        /// Determines if the given BOM Item is a default for the given information
        /// </summary>
        /// <param name="bomItem"></param>
        /// <param name="inItemSite"></param>
        /// <returns></returns>
        protected virtual bool IsDefaultBOM(AMBomItem bomItem, INItemSite inItemSite)
        {
            if (string.IsNullOrWhiteSpace(bomItem?.BOMID)
                || inItemSite?.SiteID == null)
            {
                return false;
            }

            if (AM.InventoryHelper.SubItemFeatureEnabled)
            {
                // Get Default by subitem...
                var pbm = new PrimaryBomIDManager(this);
                if (pbm.IsPrimaryBomIDBySubItem(bomItem.BOMID) != null)
                {
                    return true;
                }
            }

            var extension = inItemSite.GetExtension<INItemSiteExt>();
            return extension?.AMBOMID != null && extension.AMBOMID.EqualsWithTrim(bomItem.BOMID);
        }

        protected virtual bool ProcessCost(AMBomItem bomItem)
        {
            return ProcessCost(bomItem, 0, false);
        }

        /// <summary>
        /// Process Costs for a BOM
        /// </summary>
        protected virtual bool ProcessCost(AMBomItem bomItem, int level, bool isDefault)
        {
            var successful = true;

            if (bomItem?.BOMID == null)
            {
                return false;
            }

            var bomcostrec = new AMBomCost
            {
                InventoryID = bomItem.InventoryID,
                SubItemID = bomItem.SubItemID,
                BOMID = bomItem.BOMID,
                RevisionID = bomItem.RevisionID,
                SiteID = bomItem.SiteID,
                MultiLevelProcess = Settings.Current.SnglMlti == RollupSettings.SelectOptSM.Multi,
                UserID = this.Accessinfo.UserID,
                Level = level,
                // Might have to update later for subitem indication - currently only looks at INItemSite default BOM ID
                IsDefaultBom = isDefault
            };

            // Set the ItemClass from Inventory Item
            InventoryItem invItem = PXSelect<InventoryItem,
                Where<InventoryItem.inventoryID, Equal<Required<InventoryItem.inventoryID>>
                >>.Select(this, bomcostrec.InventoryID.GetValueOrDefault());

            bomcostrec.ItemClassID = invItem?.ItemClassID;

            //Set the Current and Pending cost from INItemSite
            INItemSite inItemSite = PXSelect<INItemSite, Where<INItemSite.inventoryID, Equal<Required<INItemSite.inventoryID>>,
                    And<INItemSite.siteID, Equal<Required<INItemSite.siteID>>>
                    >>.Select(this, bomcostrec.InventoryID.GetValueOrDefault(), bomcostrec.SiteID.GetValueOrDefault());

            bomcostrec.StdCost = inItemSite?.StdCost;
            bomcostrec.PendingStdCost = inItemSite?.PendingStdCost;

            // Set Lot Size based on Filter Settings
            if(Settings.Current.IgnoreMinMaxLotSizeValues == true)
            {
                bomcostrec.LotSize = 1;
            }
            else if(bomcostrec.BOMID == Settings.Current.BOMID && Settings.Current.LotSize.GetValueOrDefault() != 0
                && Settings.Current.IgnoreMinMaxLotSizeValues == false)
            {
                bomcostrec.LotSize = Settings.Current.LotSize.GetValueOrDefault();
            }
            else
            {
                bomcostrec.LotSize = InventoryHelper.GetMfgReorderQty(this, bomcostrec.InventoryID, bomcostrec.SiteID);
            }

            if (bomcostrec.LotSize.GetValueOrDefault() <= 0)
            {
                bomcostrec.LotSize = 1;
            }

            bomcostrec.FLaborCost = 0;
            bomcostrec.VLaborCost = 0;
            var laborCostAndHours = SetLaborCost(ref bomcostrec, Settings.Current?.IncFixed == true);

            bomcostrec.MachCost = GetMachineCost(bomcostrec);

            bomcostrec.ToolCost = GetToolCost(bomcostrec);
            
            var allMaterial = PXSelectReadonly2<AMBomMatl,
                InnerJoin<InventoryItem, On<AMBomMatl.inventoryID, Equal<InventoryItem.inventoryID>>,
                LeftJoin<INItemSite, On<AMBomMatl.inventoryID, Equal<INItemSite.inventoryID>,
                      And<INItemSite.siteID, Equal<Required<INItemSite.siteID>>>>>>,
                Where<AMBomMatl.bOMID, Equal<Required<AMBomMatl.bOMID>>,
                    And<AMBomMatl.revisionID, Equal<Required<AMBomMatl.revisionID>>
                    >>>.Select(this, bomcostrec.SiteID, bomcostrec.BOMID, bomcostrec.RevisionID);

            //Merge of Regular Material and Subcontract Material (excluding Reference/vendor supplied material)
            OperationCosts matlTotal = new OperationCosts();

            if (allMaterial.Count > 0)
            {
                var purchase = new List<PXResult<AMBomMatl, InventoryItem>>();
                var manufactured = new List<PXResult<AMBomMatl, InventoryItem>>();
                var subcontract = new List<PXResult<AMBomMatl, InventoryItem>>();
                var refMaterial = new List<PXResult<AMBomMatl, InventoryItem>>();

                foreach (PXResult<AMBomMatl, InventoryItem, INItemSite> result in allMaterial)
                {
                    var bomMatl = (AMBomMatl) result;
                    if(bomMatl == null || 
                        (bomMatl.EffDate != null && bomMatl.EffDate > Accessinfo.BusinessDate) ||
                        (bomMatl.ExpDate != null && bomMatl.ExpDate <= Accessinfo.BusinessDate))
                    {
                        continue;
                    }    

                    // Check for COMP BOMID, if exists, item is Manufactured
                    if (bomMatl.CompBOMID != null)
                    {
                        manufactured.Add(result);
                        continue;
                    }

                    if (bomMatl.MaterialType == AMMaterialType.Subcontract && bomMatl.SubcontractSource != AMSubcontractSource.VendorSupplied)
                    {
                        subcontract.Add(result);
                        continue;
                    }

                    if (bomMatl.MaterialType == AMMaterialType.Subcontract && bomMatl.SubcontractSource == AMSubcontractSource.VendorSupplied)
                    {
                        refMaterial.Add(result);
                        continue;
                    }

                    var replenishmentSource = InventoryHelper.GetReplenishmentSource((InventoryItem)result, (INItemSite)result);
                    if (replenishmentSource == INReplenishmentSource.Manufactured)
                    {
                        manufactured.Add(result);
                        continue;
                    }

                    if (replenishmentSource == INReplenishmentSource.Purchased)
                    {
                        purchase.Add(result);
                    }

                }

                var purchaseCost = GetMaterialCost(bomcostrec, purchase, IsMultiLevel, out var purchaseMatlMessages);
                var manufacturedCost = GetMaterialCost(bomcostrec, manufactured, IsMultiLevel, out var manufacturedMatlMessages);
                var subcontractCost = GetMaterialCost(bomcostrec, subcontract, IsMultiLevel, out var subContractMatlMessages);
                var refmaterialCost = GetMaterialCost(bomcostrec, refMaterial, IsMultiLevel, out var refMaterialMatlMessages);

                if (purchaseMatlMessages != null)
                {
                    foreach (var matlMessage in purchaseMatlMessages)
                    {
                        successful = false;
                        PXTrace.WriteWarning(matlMessage);
                    }
                }

                if (manufacturedMatlMessages != null)
                {
                    foreach (var matlMessage in manufacturedMatlMessages)
                    {
                        successful = false;
                        PXTrace.WriteWarning(matlMessage);
                    }
                }

                if (subContractMatlMessages != null)
                {
                    foreach (var matlMessage in subContractMatlMessages)
                    {
                        successful = false;
                        PXTrace.WriteWarning(matlMessage);
                    }
                }

                if (refMaterialMatlMessages != null)
                {
                    foreach (var matlMessage in refMaterialMatlMessages)
                    {
                        successful = false;
                        PXTrace.WriteWarning(matlMessage);
                    }
                }

                bomcostrec.MatlManufacturedCost = manufacturedCost?.TotalCost ?? 0m;
                bomcostrec.MatlNonManufacturedCost = purchaseCost?.TotalCost ?? 0m;
                bomcostrec.SubcontractMaterialCost = subcontractCost?.TotalCost ?? 0m;
                bomcostrec.ReferenceMaterialCost = refmaterialCost?.TotalCost ?? 0m;

                matlTotal = new OperationCosts(manufacturedCost);
                matlTotal.Add(purchaseCost, true);
                matlTotal.Add(subcontractCost, true);
            }

            bomcostrec.FOvdCost = 0;
            bomcostrec.VOvdCost = 0;
            SetOverheadCosts(ref bomcostrec, Settings.Current.IncFixed.GetValueOrDefault(), matlTotal, laborCostAndHours.Item1, laborCostAndHours.Item2);

            bomcostrec.TotalCost = bomcostrec.FLaborCost.GetValueOrDefault()
                + bomcostrec.VLaborCost.GetValueOrDefault()
                + bomcostrec.MachCost.GetValueOrDefault()
                + bomcostrec.MatlManufacturedCost.GetValueOrDefault()
                + bomcostrec.MatlNonManufacturedCost.GetValueOrDefault()
                + bomcostrec.FOvdCost.GetValueOrDefault()
                + bomcostrec.VOvdCost.GetValueOrDefault()
                + bomcostrec.ToolCost.GetValueOrDefault()
                + bomcostrec.OutsideCost.GetValueOrDefault()
                + bomcostrec.DirectCost.GetValueOrDefault()
                + bomcostrec.SubcontractMaterialCost.GetValueOrDefault()
                + bomcostrec.ReferenceMaterialCost.GetValueOrDefault();

            bomcostrec.UnitCost = UomHelper.PriceCostRound(bomcostrec.TotalCost.GetValueOrDefault() / bomcostrec.LotSize.GetValueOrDefault());

            try
            {
#if DEBUG
                var sb = new System.Text.StringBuilder("BomCostRecs.Insert: ");
                InventoryItem item = PXSelect<InventoryItem, Where<InventoryItem.inventoryID, Equal<Required<InventoryItem.inventoryID>>>>
                        .Select(this, bomcostrec.InventoryID);
                if (item != null)
                {
                    sb.Append($"{item.InventoryCD.TrimIfNotNullEmpty()} ");
                }

                sb.Append($"BOM {bomcostrec.BOMID.TrimIfNotNullEmpty()} ");
                sb.Append($"Inserting Cntr {BomCostRecs.Cache.Inserted.Count()+1} ");
                AMDebug.TraceWriteLine(sb.ToString());
#endif
                BomCostRecs.Insert(bomcostrec);
            }
            catch (Exception e)
            {
                if (e is PXOuterException)
                {
                    PXTraceHelper.PxTraceOuterException((PXOuterException)e, PXTraceHelper.ErrorLevel.Error);
                }

                InventoryItem item = PXSelect<InventoryItem, Where<InventoryItem.inventoryID, Equal<Required<InventoryItem.inventoryID>>>>
                        .Select(this, bomcostrec.InventoryID);

                if (item == null)
                {
                    PXTrace.WriteInformation(Messages.InvalidInventoryIDOnBOM, bomItem.BOMID);
                    successful = false;
                }
                else
                {
                    throw new PXException(Messages.GetLocal(Messages.UnableToSaveRecordForInventoryID), Messages.GetLocal(Messages.BOMCost), item.InventoryCD.Trim(), e.Message);
                }

            }

            return successful;
        }

        /// <summary>
        /// Calculate the total tool cost for a BOM
        /// </summary>
        protected virtual decimal? GetToolCost(AMBomCost amBomCost)
        {
            var vartool = 0m;
            foreach (AMBomTool bomToolRec in PXSelect<
                AMBomTool,
                Where<AMBomTool.bOMID, Equal<Required<AMBomTool.bOMID>>,
                    And<AMBomTool.revisionID, Equal<Required<AMBomTool.revisionID>>>>>
                .Select(this, amBomCost.BOMID, amBomCost.RevisionID))
            {
                vartool += bomToolRec.UnitCost.GetValueOrDefault() * bomToolRec.QtyReq.GetValueOrDefault() * amBomCost.LotSize.GetValueOrDefault();
            }
            return UomHelper.PriceCostRound(vartool);
        }

        /// <summary>
        /// Update the current BOM Cost row with the correct calculated overhead costs
        /// </summary>
        /// <param name="currentAMBomCost">BOM Cost row being updated</param>
        /// <param name="includeFixValues">Should the fixed overheads be included</param>
        /// <param name="operationMatlCosts">previously calculated material costs</param>
        /// <param name="operationLaborCosts">previously calculated labor costs</param>
        /// <param name="operationLaborHours">previously calculated labor hours</param>
        protected virtual void SetOverheadCosts(ref AMBomCost currentAMBomCost, bool includeFixValues,
            OperationCosts operationMatlCosts, OperationCosts operationLaborCosts, OperationCosts operationLaborHours)
        {
            SetWorkCenterOverheadCosts(ref currentAMBomCost, includeFixValues,
                operationMatlCosts, operationLaborCosts, operationLaborHours);

            SetBomOverheadCosts(ref currentAMBomCost, includeFixValues,
                operationMatlCosts, operationLaborCosts, operationLaborHours);
        }

        /// <summary>
        /// Update the current BOM Cost row with the correct calculated BOM Only overhead costs
        /// </summary>
        /// <param name="currentAMBomCost">BOM Cost row being updated</param>
        /// <param name="includeFixValues">Should the fixed overheads be included</param>
        /// <param name="operationMatlCosts">previously calculated material costs</param>
        /// <param name="operationLaborCosts">previously calculated labor costs</param>
        /// <param name="operationLaborHours">previously calculated labor hours</param>
        protected virtual void SetBomOverheadCosts(ref AMBomCost currentAMBomCost, bool includeFixValues,
            OperationCosts operationMatlCosts, OperationCosts operationLaborCosts, OperationCosts operationLaborHours)
        {
            foreach (PXResult<AMBomOper, AMBomOvhd, AMOverhead> result in PXSelectJoin<
                AMBomOper,
                InnerJoin<AMBomOvhd,
                    On<AMBomOper.bOMID, Equal<AMBomOvhd.bOMID>,
                    And<AMBomOper.revisionID, Equal<AMBomOvhd.revisionID>,
                    And<AMBomOper.operationID, Equal<AMBomOvhd.operationID>>>>,
                InnerJoin<AMOverhead,
                    On<AMBomOvhd.ovhdID, Equal<AMOverhead.ovhdID>>>>,
                Where<AMBomOper.bOMID, Equal<Required<AMBomOper.bOMID>>,
                    And<AMBomOper.revisionID, Equal<Required<AMBomOper.revisionID>>>>>
                .Select(this, currentAMBomCost.BOMID, currentAMBomCost.RevisionID))
            {
                var amBomOper = (AMBomOper)result;
                var amBomOvhd = (AMBomOvhd)result;
                var amOverhead = (AMOverhead)result;

                if (string.IsNullOrEmpty(amBomOper?.BOMID)
                   || string.IsNullOrWhiteSpace(amBomOvhd?.OvhdID)
                   || string.IsNullOrWhiteSpace(amOverhead?.OvhdID))
                {
                    continue;
                }

                var overheadCost = CalculateOverheadCost(amOverhead, amBomOvhd, amBomOper, currentAMBomCost.LotSize.GetValueOrDefault(),
                    operationMatlCosts, operationLaborCosts, operationLaborHours);

                SetBomCostOverheadValues(ref currentAMBomCost, amOverhead, overheadCost, includeFixValues);
            }
        }

        /// <summary>
        /// Sets the correct overhead field of the BOM Cost row
        /// </summary>
        /// <param name="currentAMBomCost">BOM Cost row being updated</param>
        /// <param name="amOverhead">Overhead master row</param>
        /// <param name="overheadCost">calculated overhead cost</param>
        /// <param name="includeFixValues">Should the fixed overheads be included</param>
        protected virtual void SetBomCostOverheadValues(ref AMBomCost currentAMBomCost, AMOverhead amOverhead, decimal overheadCost, bool includeFixValues)
        {
            if (amOverhead.OvhdType == OverheadType.FixedType)
            {
                currentAMBomCost.FOvdCost += includeFixValues ? overheadCost : 0;
                return;
            }

            currentAMBomCost.VOvdCost += overheadCost;
        }

        /// <summary>
        /// Update the current BOM Cost row with the correct calculated Operation Work Center Only overhead costs
        /// </summary>
        /// <param name="currentAMBomCost">BOM Cost row being updated</param>
        /// <param name="includeFixValues">Should the fixed overheads be included</param>
        /// <param name="operationMatlCosts">previously calculated material costs</param>
        /// <param name="operationLaborCosts">previously calculated labor costs</param>
        /// <param name="operationLaborHours">previously calculated labor hours</param>
        protected virtual void SetWorkCenterOverheadCosts(ref AMBomCost currentAMBomCost, bool includeFixValues,
            OperationCosts operationMatlCosts, OperationCosts operationLaborCosts, OperationCosts operationLaborHours)
        {
            foreach (PXResult<AMBomOper, AMWC, AMWCOvhd, AMOverhead> result in PXSelectJoin<
                AMBomOper,
                InnerJoin<AMWC,
                    On<AMBomOper.wcID, Equal<AMWC.wcID>>,
                InnerJoin<AMWCOvhd,
                    On<AMWC.wcID, Equal<AMWCOvhd.wcID>>,
                InnerJoin<AMOverhead,
                    On<AMWCOvhd.ovhdID, Equal<AMOverhead.ovhdID>>>>>,
                Where<AMBomOper.bOMID, Equal<Required<AMBomOper.bOMID>>,
                    And<AMBomOper.revisionID, Equal<Required<AMBomOper.revisionID>>>>>
                .Select(this, currentAMBomCost.BOMID, currentAMBomCost.RevisionID))
            {
                var amBomOper = (AMBomOper)result;
                var amWCOvhd = (AMWCOvhd)result;
                var amOverhead = (AMOverhead)result;

                if (string.IsNullOrEmpty(amBomOper?.BOMID)
                   || string.IsNullOrWhiteSpace(amWCOvhd?.OvhdID)
                   || string.IsNullOrWhiteSpace(amOverhead?.OvhdID))
                {
                    continue;
                }

                var overheadCost = CalculateOverheadCost(amOverhead, amWCOvhd, amBomOper, currentAMBomCost.LotSize.GetValueOrDefault(),
                    operationMatlCosts, operationLaborCosts, operationLaborHours);

                SetBomCostOverheadValues(ref currentAMBomCost, amOverhead, overheadCost, includeFixValues);
            }
        }

        /// <summary>
        /// Calculate the overhead costs based on a work center overhead row
        /// </summary>
        /// <returns>Calculated overhead cost</returns>
        protected virtual decimal CalculateOverheadCost(AMOverhead amOverhead, AMWCOvhd amWCOvhd, AMBomOper amBomOper, decimal quantity,
            OperationCosts operationMatlCosts, OperationCosts operationLaborCosts, OperationCosts operationLaborHours)
        {
            var baseOverheadCost = amWCOvhd.OFactor.GetValueOrDefault() * amOverhead.CostRate.GetValueOrDefault();
            return CalculateOverheadCost(baseOverheadCost, amOverhead.OvhdType, quantity,
                operationLaborHours?.OperationCost(amBomOper.OperationID) ?? 0,
                operationLaborCosts?.OperationCost(amBomOper.OperationID) ?? 0,
                operationMatlCosts?.OperationCost(amBomOper.OperationID) ?? 0,
                amBomOper.GetMachineUnitsPerHour() == 0m ? 0m : quantity / amBomOper.GetMachineUnitsPerHour());
        }

        /// <summary>
        /// Calculate the overhead costs based on a BOM overhead row
        /// </summary>
        /// <returns>Calculated overhead cost</returns>
        protected virtual decimal CalculateOverheadCost(AMOverhead amOverhead, AMBomOvhd amBomOvhd, AMBomOper amBomOper, decimal quantity,
            OperationCosts operationMatlCosts, OperationCosts operationLaborCosts, OperationCosts operationLaborHours)
        {
            var baseOverheadCost = amBomOvhd.OFactor.GetValueOrDefault() * amOverhead.CostRate.GetValueOrDefault();
            return CalculateOverheadCost(baseOverheadCost, amOverhead.OvhdType, quantity,
                operationLaborHours?.OperationCost(amBomOper.OperationID) ?? 0,
                operationLaborCosts?.OperationCost(amBomOper.OperationID) ?? 0,
                operationMatlCosts?.OperationCost(amBomOper.OperationID) ?? 0,
                amBomOper.GetMachineUnitsPerHour() == 0m ? 0m : quantity / amBomOper.GetMachineUnitsPerHour());
        }

        /// <summary>
        /// Calculate the overhead costs based on type and correct variables
        /// </summary>
        /// <returns>Calculated overhead cost</returns>
        protected virtual decimal CalculateOverheadCost(decimal baseOverheadCost, string overheadType,
            decimal quantity, decimal laborHours, decimal laborCosts, decimal materialCosts, decimal machineHours)
        {
            switch (overheadType)
            {
                case OverheadType.VarQtyTot:
                case OverheadType.VarQtyComp:
                    return baseOverheadCost * quantity;

                case OverheadType.VarLaborHrs:
                    return baseOverheadCost * laborHours;

                case OverheadType.VarLaborCost:
                    return baseOverheadCost * laborCosts;

                case OverheadType.VarMatlCost:
                    return baseOverheadCost * materialCosts;

                case OverheadType.VarMachHrs:
                    return baseOverheadCost * machineHours;

                case OverheadType.FixedType:
                    return baseOverheadCost;
            }

            return 0;
        }

        /// <summary>
        /// Update the BOM Material unit cost into the cache
        /// </summary>
        /// <param name="amBomMatl">BOM Material Row</param>
        /// <param name="unitCost">Unit cost to use in the update process</param>
        protected virtual void UpdateMaterialUnitCost(AMBomMatl amBomMatl, decimal? unitCost)
        {
            if (amBomMatl == null || unitCost == null)
            {
                return;
            }

            AMBomMatl cacheAmBomMatl = BomMatlRecs.Locate(amBomMatl);

            if (cacheAmBomMatl != null)
            {
                cacheAmBomMatl.UnitCost = unitCost.GetValueOrDefault();
                BomMatlRecs.Update(cacheAmBomMatl);
                return;
            }

            amBomMatl.UnitCost = unitCost.GetValueOrDefault();
            BomMatlRecs.Update(amBomMatl);
        }

        [Obsolete(InternalMessages.MethodIsObsoleteAndWillBeRemoved2020R2)]
        protected virtual OperationCosts GetMaterialCost(AMBomCost currentAMBomCost, out bool successful) { throw new NotImplementedException(); }

        /// <summary>
        /// Get the total BOM/Operations material cost
        /// </summary>
        protected virtual OperationCosts GetMaterialCost(AMBomCost currentAMBomCost, IEnumerable<PXResult<AMBomMatl, InventoryItem>> material, bool isMultLevel, out List<string> materialMessage)
        {
            materialMessage = new List<string>();

            if (currentAMBomCost == null || string.IsNullOrWhiteSpace(currentAMBomCost.BOMID) || material == null)
            {
                return null;
            }

            var operationMaterialCosts = new OperationCosts();
            foreach(var result in material)
            {
                var matlRec = (AMBomMatl) result;
                var inventoryItem = (InventoryItem) result;

                if (matlRec?.BOMID == null || inventoryItem?.InventoryCD == null)
                {
                    continue;
                }

                decimal? unitcost = null;
                var matlSiteID = matlRec.SiteID ?? currentAMBomCost.SiteID;
                if (isMultLevel)
                {
                    var bomManager = new PrimaryBomIDManager(this);
                    var bomItem = PrimaryBomIDManager.GetNotArchivedRevisionBomItem(this, bomManager.GetPrimaryAllLevels(matlRec.InventoryID, matlSiteID, matlRec.SubItemID));
                    unitcost = GetCurrentBomCost(bomItem?.BOMID, bomItem?.RevisionID);
#if DEBUG
                    if (unitcost != null)
                    {
                        AMDebug.TraceWriteMethodName($"Item {inventoryItem.InventoryCD} ({matlRec.InventoryID}) on BOM {currentAMBomCost.BOMID}-{currentAMBomCost.RevisionID} using bom cost value of {unitcost}");
                    }
#endif
                }

                if (unitcost == null)
                {
                    unitcost = GetUnitCost(inventoryItem, matlSiteID);
#if DEBUG
                    AMDebug.TraceWriteMethodName($"Item {inventoryItem.InventoryCD} ({matlRec.InventoryID}) on BOM {currentAMBomCost.BOMID}-{currentAMBomCost.RevisionID} using inventory cost value of {unitcost}");
#endif
                }

                var inUnit = (INUnit) PXSelectorAttribute.Select<AMBomMatl.uOM>(this.Caches<AMBomMatl>(), matlRec) ??
                             (INUnit) PXSelect<INUnit,
                                Where<INUnit.inventoryID, Equal<Required<INUnit.inventoryID>>,
                                   And<INUnit.fromUnit, Equal<Required<INUnit.fromUnit>>>
                                >>.Select(this, matlRec.InventoryID, matlRec.UOM);

                if (inUnit == null)
                {
                    materialMessage.Add(Messages.GetLocal(Messages.InvalidUOMForMaterialonBOM, matlRec.UOM.TrimIfNotNullEmpty(), inventoryItem.InventoryCD, matlRec.BOMID, matlRec.RevisionID));
                    continue;
                }

                if (UomHelper.TryConvertToBaseCost<AMBomMatl.inventoryID>(BomMatlRecs.Cache, matlRec, matlRec.UOM, unitcost.GetValueOrDefault(), out var matlUnitCost))
                {
                    unitcost = matlUnitCost.GetValueOrDefault();
                }

                var itemExt = inventoryItem.GetExtension<InventoryItemExt>();

                var totalQtyRequired = matlRec.QtyReq.GetValueOrDefault() *
                        (1 + (Settings.Current.IncMatScrp.GetValueOrDefault() ? matlRec.ScrapFactor.GetValueOrDefault() : 0m)) *
                        (matlRec.BatchSize.GetValueOrDefault() == 0m ? 1m :
                        currentAMBomCost.LotSize.GetValueOrDefault() / matlRec.BatchSize.GetValueOrDefault());

                totalQtyRequired = itemExt.AMQtyRoundUp == false ? totalQtyRequired : Math.Ceiling(totalQtyRequired);

                var matlCost = totalQtyRequired * unitcost.GetValueOrDefault();

                operationMaterialCosts.Add(matlRec.OperationID, matlCost, true);

                if (Settings.Current.UpdateMaterial.GetValueOrDefault())
                {
                    UpdateMaterialUnitCost(matlRec, unitcost);
                }
            }

            return operationMaterialCosts;
        }

        /// <summary>
        /// Get the current BOM unitcost calculated in the AMBomCost table/cache
        /// </summary>
        /// <param name="bomId">BOM ID of unit cost to search for</param>
        /// <param name="revisionId">BOM Revision of unit cost to search for</param>
        /// <returns></returns>
        protected virtual decimal? GetCurrentBomCost(string bomId, string revisionId)
        {
            if (string.IsNullOrWhiteSpace(bomId) || string.IsNullOrWhiteSpace(revisionId))
            {
                return null;
            }

            var foundAMBomCost = BomCostRecs.Locate(new AMBomCost
            {
                BOMID = bomId,
                RevisionID = revisionId,
                UserID = this.Accessinfo.UserID
            }) ?? PXSelect<
                    AMBomCost,
                    Where<AMBomCost.bOMID, Equal<Required<AMBomCost.bOMID>>,
                        And<AMBomCost.revisionID, Equal<Required<AMBomCost.revisionID>>,
                            And<AMBomCost.userID, Equal<Current<AccessInfo.userID>>>>>>
                .Select(this, bomId, revisionId);

            return foundAMBomCost?.UnitCost;
        }

        /// <summary>
        /// Get total BOM Machine cost
        /// </summary>
        /// <param name="amBomCost">BOM Cost row being processed</param>
        /// <returns></returns>
        protected virtual decimal? GetMachineCost(AMBomCost amBomCost)
        {
            var varmach = 0m;

            decimal machineMinutes = 0;

            foreach (AMBomOper operrec in PXSelect<AMBomOper,
                Where<AMBomOper.bOMID, Equal<Required<AMBomOper.bOMID>>,
                 And<AMBomOper.revisionID, Equal<Required<AMBomOper.revisionID>>
                >>>.Select(this, amBomCost.BOMID, amBomCost.RevisionID))
            {
                if (operrec.MachineUnitTime.GetValueOrDefault() <= 0 || operrec.MachineUnits.GetValueOrDefault() <= 0)
                {
                    continue;
                }

                machineMinutes += operrec.MachineUnitTime.GetValueOrDefault() / operrec.MachineUnits.GetValueOrDefault();
                //TODO: remove the subquery and make this a single query

                foreach (PXResult<AMWCMach, AMMach> result in PXSelectJoin<AMWCMach,
                    InnerJoin<AMMach, On<AMWCMach.machID, Equal<AMMach.machID>>>,
                    Where<AMWCMach.wcID, Equal<Required<AMWCMach.wcID>>>
                >.Select(this, operrec.WcID))
                {
                    var wcMachine = (AMWCMach)result;
                    var machine = (AMMach)result;

                    if (string.IsNullOrWhiteSpace(wcMachine?.MachID)
                        || string.IsNullOrWhiteSpace(machine?.MachID)
                        || !machine.ActiveFlg.GetValueOrDefault())
                    {
                        continue;
                    }

                    decimal? standardCost = machine.StdCost.GetValueOrDefault();
                    if (wcMachine.MachineOverride.GetValueOrDefault())
                    {
                        standardCost = wcMachine.StdCost.GetValueOrDefault();
                    }

                    varmach += operrec.GetMachineUnitsPerHour() == 0m ? 0m
                        : standardCost.GetValueOrDefault() * amBomCost.LotSize.GetValueOrDefault() / operrec.GetMachineUnitsPerHour();
                }
            }

            // Set the machine Time Rounded to nearest Minute
            amBomCost.MachineTime = machineMinutes.ToCeilingInt();

            return UomHelper.PriceCostRound(varmach);
        }

        /// <summary>
        /// Converts the bom operation record into the total labor hours
        /// </summary>
        /// <param name="amBomOper"></param>
        /// <param name="includeSetupTime">Include the setup hours in the total</param>
        /// <returns>Total operation labor hours</returns>
        protected static decimal? GetLaborHours(AMBomOper amBomOper, bool includeSetupTime = true)
        {
            if (amBomOper == null
                || string.IsNullOrWhiteSpace(amBomOper.BOMID)
                || amBomOper.OperationID == null)
            {
                return null;
            }

            decimal? operSetupHrsPerPc = 0;
            decimal? operRunHrsPerPc = 0;

            operSetupHrsPerPc = amBomOper.SetupTime.ToHours();

            operRunHrsPerPc = amBomOper.GetRunUnitsPerHour() == 0m ? 0m : 1 / amBomOper.GetRunUnitsPerHour();

            if (includeSetupTime)
            {
                return operSetupHrsPerPc.GetValueOrDefault() + operRunHrsPerPc.GetValueOrDefault();
            }

            return operRunHrsPerPc.GetValueOrDefault();
        }

        /// <summary>
        /// Get the total labor cost and hours while updating the current ambomcost record with the correct labor values.
        /// </summary>
        /// <param name="currentAmBomCost">Current AMBomCost record that will be updated</param>
        /// <param name="includeFixValues">Indicates if fixed labor should be included in the calculations</param>
        /// <returns>Operation Labor Costs (Item1) and Operation Labor Hours (Item2)</returns>
        protected virtual Tuple<OperationCosts, OperationCosts> SetLaborCost(ref AMBomCost currentAmBomCost, bool includeFixValues)
        {
            var operationLaborCosts = new OperationCosts();
            var operationLaborHours = new OperationCosts();

            decimal fixedMinutes = 0;
            decimal variableMinutes = 0;

            foreach (PXResult<AMBomOper, AMWC> result in PXSelectJoin<
                AMBomOper,
                InnerJoin<AMWC,
                    On<AMBomOper.wcID, Equal<AMWC.wcID>>>,
                Where<AMBomOper.bOMID, Equal<Required<AMBomOper.bOMID>>,
                    And<AMBomOper.revisionID, Equal<Required<AMBomOper.revisionID>>
                >>>.Select(this, currentAmBomCost.BOMID, currentAmBomCost.RevisionID))
            {
                var amBomOper = (AMBomOper)result;
                var amwc = (AMWC)result;

                if (amBomOper?.OperationID == null
                    || string.IsNullOrWhiteSpace(amwc?.WcID))
                {
                    continue;
                }

                var laborHours = includeFixValues && amBomOper.SetupTime.GetValueOrDefault() > 0 ? amBomOper.SetupTime.ToHours() : 0;
                var wcStdCost = ShiftDiffType.GetShiftDifferentialCost(this, amwc);
                var laborCosts = includeFixValues && amBomOper.SetupTime.GetValueOrDefault() > 0 ? amBomOper.SetupTime.ToHours() * wcStdCost : 0;

                currentAmBomCost.FLaborCost += laborCosts;
                fixedMinutes += laborHours.GetValueOrDefault() * 60m;

                var varLaborHours = GetLaborHours(amBomOper, false) * currentAmBomCost.LotSize.GetValueOrDefault();
                variableMinutes += varLaborHours.GetValueOrDefault() * 60m;
                currentAmBomCost.VLaborCost += varLaborHours.GetValueOrDefault() * wcStdCost;

                laborCosts += varLaborHours.GetValueOrDefault() * wcStdCost;

                laborHours += varLaborHours.GetValueOrDefault();

                operationLaborHours.Add(amBomOper.OperationID, laborHours.GetValueOrDefault());
                operationLaborCosts.Add(amBomOper.OperationID, laborCosts.GetValueOrDefault());
            }

            // Set the Fixed and Variable Labor Time Rounded to nearest Minute
            currentAmBomCost.FixedLaborTime = fixedMinutes.ToCeilingInt();
            currentAmBomCost.VariableLaborTime = variableMinutes.ToCeilingInt();

            return new Tuple<OperationCosts, OperationCosts>(operationLaborCosts, operationLaborHours);
        }

        public static void UpdatePlannedMaterialCosts(PXGraph graph, AMProdItem amproditem)
        {
            if (amproditem == null || graph == null)
            {
                return;
            }

            var costRollGraph = PXGraph.CreateInstance<BOMCostRoll>();
            foreach (PXResult<AMProdMatl, InventoryItem> result in PXSelectJoin<AMProdMatl,
                InnerJoin<InventoryItem, On<AMProdMatl.inventoryID, Equal<InventoryItem.inventoryID>>>,
                    Where<AMProdMatl.orderType, Equal<Required<AMProdMatl.orderType>>,
                    And<AMProdMatl.prodOrdID, Equal<Required<AMProdMatl.prodOrdID>>>
                        >>.Select(graph, amproditem.OrderType, amproditem.ProdOrdID))
            {
                UpdatePlannedMaterialCost(graph, costRollGraph, amproditem, graph.Caches[typeof(AMProdMatl)].LocateElse((AMProdMatl)result), result);
            }
        }

        public static void UpdatePlannedMaterialCost(PXGraph callingGraph, BOMCostRoll costRollGraph, AMProdItem amproditem, AMProdMatl amProdMatl, InventoryItem inventoryItem)
        {
            if (callingGraph == null)
            {
                throw new PXArgumentException(nameof(callingGraph));
            }

            if (costRollGraph == null)
            {
                throw new PXArgumentException(nameof(costRollGraph));
            }

            if (amproditem == null || string.IsNullOrWhiteSpace(amproditem.ProdOrdID)
                                   || amProdMatl == null || string.IsNullOrWhiteSpace(amProdMatl.ProdOrdID) ||
                                   inventoryItem?.InventoryID == null)
            {
                return;
            }

            decimal? materialUnitCost = null;
            costRollGraph.Clear();
            costRollGraph.IsImport = true;
            if (costRollGraph?.Settings?.Current == null)
            {
                costRollGraph.Settings.Current = new RollupSettings();
            }
            costRollGraph.Settings.Current.IsPersistMode = false;
            costRollGraph.Settings.Current.ApplyPend = false;
            costRollGraph.Settings.Current.IncFixed = true;
            costRollGraph.Settings.Current.IncMatScrp = true;
            costRollGraph.Settings.Current.UpdateMaterial = false;
            costRollGraph.Settings.Current.SnglMlti = RollupSettings.SelectOptSM.Multi;

            int? siteID = amProdMatl.SiteID ?? amproditem.SiteID;

            //for production we do not want to roll standard cost items. Standard cost is standard cost.
            if (inventoryItem.ValMethod != INValMethod.Standard
                && InventoryHelper.GetReplenishmentSource(callingGraph, amProdMatl.InventoryID, siteID) == INReplenishmentSource.Manufactured)
            {
                var totalQtyRequired = amProdMatl.TotalQtyRequired.GetValueOrDefault();
                if (totalQtyRequired <= 0)
                {
                    totalQtyRequired = 1;
                }

                costRollGraph.Settings.Current.LotSize = totalQtyRequired;

                if (!string.IsNullOrWhiteSpace(amProdMatl.CompBOMID))
                {
                    costRollGraph.Settings.Current.BOMID = amProdMatl.CompBOMID;
                    costRollGraph.Settings.Current.RevisionID = amProdMatl.CompBOMRevisionID;
                }
                else
                {
                    var bomId = new PrimaryBomIDManager(callingGraph).GetItemSitePrimary(amProdMatl.InventoryID, siteID, amProdMatl.SubItemID);
                    var bomItem = PrimaryBomIDManager.GetNotArchivedRevisionBomItem(costRollGraph, bomId);
                    if (bomItem?.BOMID != null)
                    {
                        costRollGraph.Settings.Current.BOMID = bomItem.BOMID;
                        costRollGraph.Settings.Current.RevisionID = bomItem.RevisionID;
                    }
                }

                if (costRollGraph.Settings.Current != null
                    && !string.IsNullOrWhiteSpace(costRollGraph.Settings.Current.BOMID)
                    && !string.IsNullOrWhiteSpace(costRollGraph.Settings.Current.RevisionID))
                {
                    costRollGraph.RollCosts();
                    var amBomCost = costRollGraph.LocateBomCost(costRollGraph.Settings.Current.BOMID, costRollGraph.Settings.Current.RevisionID);
                    if (amBomCost != null && amBomCost.InventoryID == amProdMatl.InventoryID)
                    {
                        materialUnitCost = amBomCost.UnitCost.GetValueOrDefault();
                    }
                }
            }

            if (materialUnitCost.GetValueOrDefault() == 0)
            {
                materialUnitCost = costRollGraph.GetUnitCost(inventoryItem, siteID);
            }

            // Convert the Unit Cost to Production Material UOM
            if (UomHelper.TryConvertToBaseCost<AMProdMatl.inventoryID>(callingGraph.Caches[typeof(AMProdMatl)], amProdMatl, amProdMatl.UOM, materialUnitCost.GetValueOrDefault(), out var matlUnitCost))
            {
                materialUnitCost = matlUnitCost;
            }

            if (materialUnitCost.GetValueOrDefault() == 0 ||
                amProdMatl.UnitCost.GetValueOrDefault() == materialUnitCost.GetValueOrDefault())
            {
                //leave the current cost if the new calculated cost is zero
                return;
            }

            amProdMatl.UnitCost = materialUnitCost.GetValueOrDefault();
            callingGraph.Caches[typeof(AMProdMatl)].Update(amProdMatl);
        }

        private AMBomCost LocateBomCost(string bomId, string revisionId)
        {
            return (AMBomCost)BomCostRecs.Cache.Locate(new AMBomCost
            {
                BOMID = bomId,
                RevisionID = revisionId,
                UserID = Accessinfo.UserID
            });
        }

        protected virtual decimal? GetUnitCostFromINItemSite(INItemSite inItemSite)
        {
            if (inItemSite != null && (inItemSite.TranUnitCost != null || inItemSite.LastCost != null))
            {
                if ((inItemSite.TranUnitCost ?? 0) == 0 && (inItemSite.LastCost ?? 0) != 0)
                {
                    return inItemSite.LastCost;
                }

                return inItemSite.TranUnitCost;
            }

            return null;
        }

        protected virtual decimal? GetUnitCostFromINItemCost(INItemCost inItemCost)
        {
            return GetUnitCostFromINItemCostTable(inItemCost);
        }

        public static decimal? GetUnitCostFromINItemCostTable(INItemCost inItemCost)
        {
            if (inItemCost != null && (inItemCost.TranUnitCost != null || inItemCost.LastCost != null))
            {
                if ((inItemCost.TranUnitCost ?? 0) == 0 && (inItemCost.LastCost ?? 0) != 0)
                {
                    return inItemCost.LastCost;
                }

                return inItemCost.TranUnitCost;
            }

            return null;
        }

        protected static decimal? GetStandardCost(InventoryItem inventoryItem, INItemSite itemSite, bool usePending)
        {
            if (itemSite != null && itemSite.StdCostOverride == true)
            {
                return usePending ? itemSite.PendingStdCost : itemSite.StdCost;
            }

            if (inventoryItem == null)
            {
                throw new PXArgumentException("inventoryItem");
            }

            return usePending ? inventoryItem.PendingStdCost : inventoryItem.StdCost;
        }

        protected virtual decimal? GetUnitCost(InventoryItem inventoryItem, int? siteid)
        {
            decimal? unitCost = null;

            if (inventoryItem == null)
            {
                return unitCost;
            }

            // [1.1] Get the InItemSite Record
            INItemSite itemsite = PXSelect<INItemSite,
                Where<INItemSite.inventoryID, Equal<Required<INItemSite.inventoryID>>,
                    And<INItemSite.siteID, Equal<Required<INItemSite.siteID>>>>>.Select(this, inventoryItem.InventoryID, siteid);

            if (inventoryItem.ValMethod == INValMethod.Standard)
            {
                unitCost = GetStandardCost(inventoryItem, itemsite, Settings.Current.UsePending.GetValueOrDefault());
                if (unitCost != null)
                {
                    return unitCost;
                }
            }

            unitCost = GetUnitCostFromINItemSite(itemsite);
            if (unitCost != null)
            {
                return unitCost;
            }

            // [1.2] Get the item cost Record (same lookup order as bom/prod matl unit cost default)
            INItemCost itemCost = PXSelect<INItemCost,
                Where<INItemCost.inventoryID, Equal<Required<INItemCost.inventoryID>>>>.Select(this, inventoryItem.InventoryID);

            unitCost = GetUnitCostFromINItemCost(itemCost);
            if (unitCost != null)
            {
                return unitCost;
            }

            // [2] Get the Default site for the Inventory Item
            itemsite = PXSelect<INItemSite,
                Where<INItemSite.inventoryID, Equal<Required<INItemSite.inventoryID>>,
                    And<INItemSite.isDefault, Equal<boolTrue>>>>.Select(this, inventoryItem.InventoryID);

            unitCost = GetUnitCostFromINItemSite(itemsite);
            if (unitCost != null)
            {
                return unitCost;
            }

            // [3] Get the first found site for the Inventory Item
            itemsite = PXSelect<INItemSite,
                Where<INItemSite.inventoryID, Equal<Required<INItemSite.inventoryID>>
                >>.Select(this, inventoryItem.InventoryID);

            unitCost = GetUnitCostFromINItemSite(itemsite);
            if (unitCost != null)
            {
                return unitCost;
            }

            return 0m;
        }

        /// <summary>
        /// Delete any calculated BOM cost records for the current user
        /// </summary>
        protected virtual void DeleteUserCostRollData()
        {
            DeleteUserCostRollData(Accessinfo.UserID);
        }

        /// <summary>
        /// Delete any calculated BOM cost records for the given user
        /// </summary>
        public static void DeleteUserCostRollData(Guid userID)
        {
            PXDatabase.Delete<AMBomCost>(new PXDataFieldRestrict<AMBomCost.userID>(userID));
        }

        /// <summary>
        /// Record costs or units for a set of operations
        /// </summary>
        public class OperationCosts
        {
            private Dictionary<int?, decimal> _operationCostDictionary;
            private decimal _totalCost;

            /// <summary>
            /// Total costs across all operation costs
            /// </summary>
            public decimal TotalCost => _totalCost;

            /// <summary>
            /// Cost for the given operation
            /// </summary>
            /// <param name="operationID">Operation ID</param>
            /// <returns></returns>
            public decimal OperationCost(int? operationID)
            {
                if (_operationCostDictionary.ContainsKey(operationID))
                {
                    return _operationCostDictionary[operationID];
                }

                return 0;
            }

            public OperationCosts()
            {
                _operationCostDictionary = new Dictionary<int?, decimal>();
            }

            public OperationCosts(OperationCosts existingOperCost)
            {
                _operationCostDictionary = existingOperCost?._operationCostDictionary ?? new Dictionary<int?, decimal>();
                _totalCost = existingOperCost?._totalCost ?? 0m;
            }

            public void Add(OperationCosts existingOperCost, bool addToExistingCost)
            {
                if (existingOperCost?._operationCostDictionary == null)
                {
                    return;
                }

                foreach (var kvp in existingOperCost._operationCostDictionary)
                {
                    Add(kvp.Key, kvp.Value, addToExistingCost);
                }
            }

            /// <summary>
            /// Add costs to an operation
            /// </summary>
            /// <param name="operationID">OperationID</param>
            /// <param name="cost">cost/unit value</param>
            /// <param name="addToExistingCost">Should the entry add to an existing cost if found (when true) or be replaced (when false)</param>
            public void Add(int? operationID, decimal cost, bool addToExistingCost = false)
            {
                decimal currentCost = 0;
                if (operationID == null)
                {
                    return;
                }

                if (_operationCostDictionary.ContainsKey(operationID))
                {
                    currentCost = _operationCostDictionary[operationID];
                    _operationCostDictionary.Remove(operationID);
                    _totalCost -= currentCost;
                    if (!addToExistingCost)
                    {
                        currentCost = 0;
                        if (_totalCost < 0)
                        {
                            _totalCost = 0;
                        }
                    }
                }

                _operationCostDictionary.Add(operationID, cost + currentCost);
                _totalCost += cost + currentCost;
            }
        }

        /// <summary>
        /// Roll Costs Call with BOM List
        /// </summary>
        public virtual bool RollCosts(RollBomList rollBomList)
        {
            var processedAll = true;

            BomCostRecs.Cache.Clear();

            var orderedBomList = rollBomList.GetBomItemsByLevelDesc();
            foreach (var bomItem in orderedBomList)
            {
                var levelDefault = rollBomList.GetLevelDefault(bomItem);
                if (levelDefault == null)
                {
                    if (!ProcessCost(bomItem))
                    {
                        processedAll = false;
                    }
                    continue;
                }

                if (!ProcessCost(bomItem, levelDefault.Item1, levelDefault.Item2))
                {
                    processedAll = false;
                }
            }

            return processedAll;
        }

        /// <summary>
        /// Write AMBomCostHistory record from AMBomCost record
        /// </summary>
        protected virtual AMBomCostHistory WriteAMBomCostHistoryRecord(AMBomCost bomCost)
        {
            var aMBomCostHistory = new AMBomCostHistory
            {
                BOMID = bomCost.BOMID,
                RevisionID = bomCost.RevisionID,
                StartDate = Common.Dates.Today,
                EndDate = null,
                MatlManufacturedCost = bomCost.MatlManufacturedCost,
                MatlNonManufacturedCost = bomCost.MatlNonManufacturedCost,
                FLaborCost = bomCost.FLaborCost,
                VLaborCost = bomCost.VLaborCost,
                MachCost = bomCost.MachCost,
                OutsideCost = bomCost.OutsideCost,
                DirectCost = bomCost.DirectCost,
                FOvdCost = bomCost.FOvdCost,
                VOvdCost = bomCost.VOvdCost,
                ToolCost = bomCost.ToolCost,
                SubcontractMaterialCost = bomCost.SubcontractMaterialCost,
                ReferenceMaterialCost = bomCost.ReferenceMaterialCost,
                InventoryID = bomCost.InventoryID,
                SubItemID = bomCost.SubItemID,
                SiteID = bomCost.SiteID,
                UnitCost = bomCost.UnitCost,
                TotalCost = bomCost.TotalCost,
                LotSize = bomCost.LotSize,
                MultiLevelProcess = bomCost.MultiLevelProcess,
                Level = bomCost.Level,
                IsDefaultBom = bomCost.IsDefaultBom,
                FixedLaborTime = bomCost.FixedLaborTime,
                VariableLaborTime = bomCost.VariableLaborTime,
                MachineTime = bomCost.MachineTime,
                ItemClassID = bomCost.ItemClassID,
                StdCost = bomCost.StdCost,
                PendingStdCost = bomCost.PendingStdCost
            };

            return BomCostHistoryRecs.Insert(aMBomCostHistory);
        }

        /// <summary>
        /// Update  existing AMBomCostHistory record from AMBomCost record
        /// </summary>
        protected virtual AMBomCostHistory UpdateAMBomCostHistoryRecord(AMBomCostHistory existingBomCostHistory, AMBomCost bomCost)
        {
            existingBomCostHistory.MatlManufacturedCost = bomCost.MatlManufacturedCost;
            existingBomCostHistory.MatlNonManufacturedCost = bomCost.MatlNonManufacturedCost;
            existingBomCostHistory.FLaborCost = bomCost.FLaborCost;
            existingBomCostHistory.VLaborCost = bomCost.VLaborCost;
            existingBomCostHistory.MachCost = bomCost.MachCost;
            existingBomCostHistory.OutsideCost = bomCost.OutsideCost;
            existingBomCostHistory.DirectCost = bomCost.DirectCost;
            existingBomCostHistory.FOvdCost = bomCost.FOvdCost;
            existingBomCostHistory.VOvdCost = bomCost.VOvdCost;
            existingBomCostHistory.ToolCost = bomCost.ToolCost;
            existingBomCostHistory.SubcontractMaterialCost = bomCost.SubcontractMaterialCost;
            existingBomCostHistory.ReferenceMaterialCost = bomCost.ReferenceMaterialCost;
            existingBomCostHistory.InventoryID = bomCost.InventoryID;
            existingBomCostHistory.SubItemID = bomCost.SubItemID;
            existingBomCostHistory.SiteID = bomCost.SiteID;
            existingBomCostHistory.UnitCost = bomCost.UnitCost;
            existingBomCostHistory.TotalCost = bomCost.TotalCost;
            existingBomCostHistory.LotSize = bomCost.LotSize;
            existingBomCostHistory.MultiLevelProcess = bomCost.MultiLevelProcess;
            existingBomCostHistory.Level = bomCost.Level;
            existingBomCostHistory.IsDefaultBom = bomCost.IsDefaultBom;
            existingBomCostHistory.FixedLaborTime = bomCost.FixedLaborTime;
            existingBomCostHistory.VariableLaborTime = bomCost.VariableLaborTime;
            existingBomCostHistory.MachineTime = bomCost.MachineTime;
            existingBomCostHistory.ItemClassID = bomCost.ItemClassID;
            existingBomCostHistory.StdCost = bomCost.StdCost;
            existingBomCostHistory.PendingStdCost = bomCost.PendingStdCost;

            return BomCostHistoryRecs.Update(existingBomCostHistory);
        }

        protected virtual void ArchiveBomCostRecords()
        {
            foreach (AMBomCost bomCostRec in PXSelect<AMBomCost,
                Where<AMBomCost.userID, Equal<Current<AccessInfo.userID>>
                   >>.Select(this))
            {
                if (bomCostRec.Selected != true)
                {
                    continue;
                }

                AMBomCostHistory bomCostHistory = PXSelect<AMBomCostHistory,
                    Where<AMBomCostHistory.bOMID, Equal<Required<AMBomCostHistory.bOMID>>,
                        And<AMBomCostHistory.revisionID, Equal<Required<AMBomCostHistory.revisionID>>>>,
                    OrderBy<Desc<AMBomCostHistory.startDate>>
                    >.SelectWindowed(this, 0, 1, bomCostRec.BOMID, bomCostRec.RevisionID);

                if (bomCostHistory == null)
                {
                    WriteAMBomCostHistoryRecord(bomCostRec);
                }
                else if (bomCostHistory.CreatedDateTime >= Common.Dates.UtcToday && 
                    bomCostHistory.CreatedDateTime <= Common.Dates.UtcToday.AddHours(23.98))
                {
                    UpdateAMBomCostHistoryRecord(bomCostHistory, bomCostRec);
                }
                else
                {
                    bomCostHistory.EndDate = Common.Dates.Today.AddDays(-1);
                    BomCostHistoryRecs.Update(bomCostHistory);
                    WriteAMBomCostHistoryRecord(bomCostRec);
                }
            }

            Actions.PressSave();
        }

        /// <summary>
        /// Projection of <see cref="AMBomMatl"/> including required fields for drill down process
        /// </summary>
        [PXProjection(typeof(Select2<AMBomMatl,
            InnerJoin<AMBomOper, On<AMBomOper.bOMID, Equal<AMBomMatl.bOMID>,
            And<AMBomOper.revisionID, Equal<AMBomMatl.revisionID>,
            And<AMBomOper.operationID, Equal<AMBomMatl.operationID>>>>>>), Persistent = false)]
        [Serializable]
        [PXHidden]
        public class AMBomMatlBomCostDrillDown : IBqlTable
        {
            #region BOMID
            public abstract class bOMID : PX.Data.BQL.BqlString.Field<bOMID> { }

            protected string _BOMID;
            [BomID(IsKey = true, BqlField = typeof(AMBomMatl.bOMID))]
            public virtual string BOMID
            {
                get
                {
                    return this._BOMID;
                }
                set
                {
                    this._BOMID = value;
                }
            }
            #endregion
            #region RevisionID
            public abstract class revisionID : PX.Data.BQL.BqlString.Field<revisionID> { }

            protected string _RevisionID;
            [RevisionIDField(IsKey = true, BqlField = typeof(AMBomMatl.revisionID))]
            public virtual string RevisionID
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
            #region OperationID
            public abstract class operationID : PX.Data.BQL.BqlInt.Field<operationID> { }

            protected int? _OperationID;
            [OperationIDField(IsKey = true, BqlField = typeof(AMBomMatl.operationID))]
            public virtual int? OperationID
            {
                get
                {
                    return this._OperationID;
                }
                set
                {
                    this._OperationID = value;
                }
            }
            #endregion
            #region LineID
            public abstract class lineID : PX.Data.BQL.BqlInt.Field<lineID> { }

            protected Int32? _LineID;
            [PXDBInt(IsKey = true, BqlField = typeof(AMBomMatl.lineID))]
            [PXUIField(DisplayName = "Line Nbr.")]
            public virtual Int32? LineID
            {
                get
                {
                    return this._LineID;
                }
                set
                {
                    this._LineID = value;
                }
            }
            #endregion
            #region InventoryID
            public abstract class inventoryID : PX.Data.BQL.BqlInt.Field<inventoryID> { }

            protected Int32? _InventoryID;
            [PXDBInt(BqlField = typeof(AMBomMatl.inventoryID))]
            [PXUIField(DisplayName = "Inventory ID")]
            public virtual Int32? InventoryID
            {
                get
                {
                    return this._InventoryID;
                }
                set
                {
                    this._InventoryID = value;
                }
            }
            #endregion
            #region SubItemID
            public abstract class subItemID : PX.Data.BQL.BqlInt.Field<subItemID> { }

            protected Int32? _SubItemID;
            [PXDBInt(BqlField = typeof(AMBomMatl.subItemID))]
            [PXUIField(DisplayName = "Subitem", FieldClass = "INSUBITEM", Visibility = PXUIVisibility.Visible)]
            public virtual Int32? SubItemID
            {
                get
                {
                    return this._SubItemID;
                }
                set
                {
                    this._SubItemID = value;
                }
            }
            #endregion
            #region SortOrder
            public abstract class sortOrder : PX.Data.BQL.BqlInt.Field<sortOrder> { }

            protected Int32? _SortOrder;
            [PXUIField(DisplayName = PX.Objects.AP.APTran.sortOrder.DispalyName)]
            [PXDBInt(BqlField = typeof(AMBomMatl.sortOrder))]
            public virtual Int32? SortOrder
            {
                get
                {
                    return this._SortOrder;
                }
                set
                {
                    this._SortOrder = value;
                }
            }
            #endregion
            #region OperationCD
            public abstract class operationCD : PX.Data.BQL.BqlString.Field<operationCD> { }

            protected string _OperationCD;
            [OperationCDField(BqlField = typeof(AMBomOper.operationCD))]
            public virtual string OperationCD
            {
                get { return this._OperationCD; }
                set { this._OperationCD = value; }
            }

            #endregion
            #region EffDate
            public abstract class effDate : PX.Data.BQL.BqlDateTime.Field<effDate> { }

            protected DateTime? _EffDate;
            [PXDBDate(BqlField = typeof(AMBomMatl.effDate))]
            [PXUIField(DisplayName = "Effective Date")]
            public virtual DateTime? EffDate
            {
                get
                {
                    return this._EffDate;
                }
                set
                {
                    this._EffDate = value;
                }
            }
            #endregion
            #region ExpDate
            public abstract class expDate : PX.Data.BQL.BqlDateTime.Field<expDate> { }

            protected DateTime? _ExpDate;
            [PXDBDate(BqlField = typeof(AMBomMatl.expDate))]
            [PXUIField(DisplayName = "Expiration Date")]
            public virtual DateTime? ExpDate
            {
                get
                {
                    return this._ExpDate;
                }
                set
                {
                    this._ExpDate = value;
                }
            }
            #endregion
        }
    }

    /// <summary>
    /// BOM Cost roll filter DAC
    /// </summary>
    [Serializable]
    [PXCacheName("Cost Roll Settings")]
    public class RollupSettings : IBqlTable
    {
        #region SnglMlti
        public abstract class snglMlti : PX.Data.BQL.BqlString.Field<snglMlti> { }

        protected String _SnglMlti;
        [PXDBString(1, IsFixed = true)]
        [PXDefault(SelectOptSM.Sngle)]
        [PXUIField(DisplayName = "Level")]
        [SelectOptSM.List]
        public virtual String SnglMlti
        {
            get
            {
                return this._SnglMlti;
            }
            set
            {
                this._SnglMlti = value;
            }
        }
        #endregion
        #region SiteID
        public abstract class siteID : PX.Data.BQL.BqlInt.Field<siteID> { }

        protected Int32? _SiteID;
        [Site]
        public virtual Int32? SiteID
        {
            get
            {
                return this._SiteID;
            }
            set
            {
                this._SiteID = value;
            }
        }
        #endregion
        #region InventoryID
        public abstract class inventoryID : PX.Data.BQL.BqlInt.Field<inventoryID> { }

        protected Int32? _InventoryID;
        [StockItemNoRestrict]
        public virtual Int32? InventoryID
        {
            get
            {
                return this._InventoryID;
            }
            set
            {
                this._InventoryID = value;
            }
        }
        #endregion
        #region SubItemID
        public abstract class subItemID : PX.Data.BQL.BqlInt.Field<subItemID> { }

        protected Int32? _SubItemID;
        [SubItem(typeof(RollupSettings.inventoryID))]
        public virtual Int32? SubItemID
        {
            get
            {
                return this._SubItemID;
            }
            set
            {
                this._SubItemID = value;
            }
        }
        #endregion
        #region LotSize
        public abstract class lotSize : PX.Data.BQL.BqlDecimal.Field<lotSize> { }

        protected decimal? _LotSize;
        [PXQuantity]
        [PXUIField(DisplayName = "Lot Size")]
        [PXUnboundDefault(TypeCode.Decimal,"0.0")]
        public virtual decimal? LotSize
        {
            get
            {
                return this._LotSize;
            }
            set
            {
                this._LotSize = value;
            }
        }
        #endregion
        #region BOMID
        public abstract class bOMID : PX.Data.BQL.BqlString.Field<bOMID> { }

        protected String _BOMID;
        [BomID]
        [BOMIDSelector]
        public virtual String BOMID
        {
            get
            {
                return this._BOMID;
            }
            set
            {
                this._BOMID = value;
            }
        }
        #endregion
        #region RevisionID
        public abstract class revisionID : PX.Data.BQL.BqlString.Field<revisionID> { }

        protected String _RevisionID;
        [RevisionIDField]
        [PXRestrictor(typeof(Where<AMBomItem.status, Equal<AMBomStatus.active>, Or<AMBomItem.status, Equal<AMBomStatus.hold>>>), Messages.BomRevisionIsArchived, typeof(AMBomItem.bOMID), typeof(AMBomItem.revisionID), CacheGlobal = true)]
        [PXSelector(typeof(Search<AMBomItem.revisionID,
                Where<AMBomItem.bOMID, Equal<Current<RollupSettings.bOMID>>>>)
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
        #region incMatScrp
        public abstract class incMatScrp : PX.Data.BQL.BqlBool.Field<incMatScrp> { }

        protected Boolean? _IncMatScrp;
        [PXDBBool]
        [PXDefault(false)]
        [PXUIField(DisplayName = "Include Material Scrap Factors")]
        public virtual Boolean? IncMatScrp
        {
            get
            {
                return this._IncMatScrp;
            }
            set
            {
                this._IncMatScrp = value;
            }
        }
        #endregion
        #region IncFixed
        public abstract class incFixed : PX.Data.BQL.BqlBool.Field<incFixed> { }

        protected Boolean? _IncFixed;
        [PXDBBool]
        [PXDefault(false)]
        [PXUIField(DisplayName = "Include Fixed Costs")]
        public virtual Boolean? IncFixed
        {
            get
            {
                return this._IncFixed;
            }
            set
            {
                this._IncFixed = value;
            }
        }
        #endregion
        #region UsePending
        public abstract class usePending : PX.Data.BQL.BqlBool.Field<usePending> { }

        protected Boolean? _UsePending;
        [PXDBBool]
        [PXDefault(false)]
        [PXUIField(DisplayName = "Use Pending Standard Cost for Purchase Items")]
        public virtual Boolean? UsePending
        {
            get
            {
                return this._UsePending;
            }
            set
            {
                this._UsePending = value;
            }
        }
        #endregion
        #region Update Material
        public abstract class updateMaterial : PX.Data.BQL.BqlBool.Field<updateMaterial> { }

        protected Boolean? _UpdateMaterial;
        [PXDBBool]
        [PXDefault(true)]
        [PXUIField(DisplayName = "Update Material")]
        public virtual Boolean? UpdateMaterial
        {
            get
            {
                return this._UpdateMaterial;
            }
            set
            {
                this._UpdateMaterial = value;
            }
        }
        #endregion
        #region ApplyPend
        public abstract class applyPend : PX.Data.BQL.BqlBool.Field<applyPend> { }

        protected Boolean? _ApplyPend;
        [PXDBBool]
        [PXDefault]
        [PXUIField(DisplayName = "Apply to Pending Costs")]
        public virtual Boolean? ApplyPend
        {
            get
            {
                return this._ApplyPend;
            }
            set
            {
                this._ApplyPend = value;
            }
        }
        #endregion
        #region Item Class ID
        public abstract class itemClassID : PX.Data.BQL.BqlInt.Field<itemClassID> { }

        protected int? _ItemClassID;
        [PXDBInt]
        [PXUIField(DisplayName = "Item Class")]
        [PXDimensionSelector(INItemClass.Dimension, typeof(Search<INItemClass.itemClassID>), typeof(INItemClass.itemClassCD), DescriptionField = typeof(INItemClass.descr))]
        public virtual int? ItemClassID
        {
            get
            {
                return this._ItemClassID;
            }
            set
            {
                this._ItemClassID = value;
            }
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

        #region SelectSM Option
        public static class SelectOptSM
        {
            //Constants declaration 
            public const string Sngle = "S";
            public const string Multi = "M";

            //List attribute 
            public class ListAttribute : PXStringListAttribute
            {
                public ListAttribute()
                    : base(
                    new string[] { Sngle, Multi },
                    new string[] { "Single", "Multi" })
                {}
            }
            //BQL constants declaration
            public class sngle : PX.Data.BQL.BqlString.Constant<sngle>
            {
                public sngle() : base(Sngle) {}
            }
            public class multi : PX.Data.BQL.BqlString.Constant<multi>
            {
                public multi() : base(Multi) {}
            }

        }
        #endregion

        #region EffectiveDate
        public abstract class effectiveDate : PX.Data.IBqlField
        {
        }
        protected DateTime? _EffectiveDate;
        [PXDBDate]
        [PXDefault(typeof(AccessInfo.businessDate))]
        [PXUIField(DisplayName = "Effective Date")]
        public virtual DateTime? EffectiveDate
        {
            get
            {
                return this._EffectiveDate;
            }
            set
            {
                this._EffectiveDate = value;
            }
        }
        #endregion

        #region IsPersistMode
        /// <summary>
        /// Is the cost roll processing going to persist the overall results after processing?
        /// Useful when call from other processes where we want to skip persisting and just get the results from the cache
        /// </summary>
        public abstract class isPersistMode : PX.Data.BQL.BqlBool.Field<isPersistMode> { }

        protected Boolean? _IsPersistMode;
        /// <summary>
        /// Is the cost roll processing going to persist the overall results after processing?
        /// Useful when call from other processes where we want to skip persisting and just get the results from the cache
        /// </summary>
        [PXBool]
        [PXUnboundDefault(true)]
        [PXUIField(DisplayName = "Persist Results", Enabled = false, Visibility = PXUIVisibility.Invisible)]
        public virtual Boolean? IsPersistMode
        {
            get
            {
                return this._IsPersistMode;
            }
            set
            {
                this._IsPersistMode = value;
            }
        }
        #endregion

        #region FoundRecursiveBom
        /// <summary>
        /// Updated fields from cost roll process which sets the field true when a recursive is found
        /// </summary>
        public abstract class foundRecursiveBom : PX.Data.BQL.BqlBool.Field<foundRecursiveBom> { }

        protected Boolean? _FoundRecursiveBom;
        /// <summary>
        /// Updated fields from cost roll process which sets the field true when a recursive is found
        /// </summary>
        [PXBool]
        [PXUnboundDefault(false)]
        [PXUIField(DisplayName = "Recursive BOM Exists", Enabled = false, Visibility = PXUIVisibility.Invisible)]
        public virtual Boolean? FoundRecursiveBom
        {
            get
            {
                return this._FoundRecursiveBom;
            }
            set
            {
                this._FoundRecursiveBom = value;
            }
        }
        #endregion
    }

    /// <summary>
    /// List of RollBomItem entires - list built during cost roll process
    /// </summary>
    public class RollBomList
    {
        private Dictionary<string, BomItemLevel> _dictionary;
        /// <summary>
        /// The last bomitem add was a new level entry
        /// </summary>
        public bool LastAddIsNewLevel { get; protected set; }

        public RollBomList()
        {
            _dictionary = new Dictionary<string, BomItemLevel>();
        }

        public int Count => _dictionary?.Count ?? 0;

        private string GetDicKey(AMBomItem bomItem)
        {
            return bomItem?.BOMID == null ? null : $"{bomItem.BOMID};{bomItem.RevisionID}";
        }

        /// <summary>
        /// Add given BOM item to list. The list will maintain a unique list and updated levels as repeat BOM items are found
        /// </summary>
        /// <param name="bomItem"></param>
        /// <param name="level">Current processing level</param>
        /// <param name="isDefaultBom">Is the given item a default BOM ID</param>
        /// <returns>True if first add to list.</returns>
        public bool Add(AMBomItem bomItem, int level, bool isDefaultBom)
        {
            LastAddIsNewLevel = false;
            if (string.IsNullOrWhiteSpace(bomItem?.BOMID))
            {
                return false;
            }

            var bomRevKey = GetDicKey(bomItem);
            if (_dictionary.TryGetValue(bomRevKey, out var bomItemLevel))
            {
                if (level > bomItemLevel.Level)
                {
                    bomItemLevel.Level = level;
                    _dictionary[bomRevKey] = bomItemLevel;
                    LastAddIsNewLevel = true;
                }

                return false;
            }

            LastAddIsNewLevel = true;
            _dictionary.Add(bomRevKey, new BomItemLevel(bomItem) {Level = level, IsDefaultBom = isDefaultBom});
            return true;
        }

        /// <summary>
        /// Get a bom items process level and if marked as default BOM
        /// </summary>
        /// <param name="bomItem"></param>
        /// <returns>Tuple item 1 = level, item 2 = default bom</returns>
        public Tuple<int, bool> GetLevelDefault(AMBomItem bomItem)
        {
            return _dictionary.TryGetValue(GetDicKey(bomItem), out var bomItemLevel) ? new Tuple<int, bool>(bomItemLevel.Level.GetValueOrDefault(), bomItemLevel.IsDefaultBom) : null;
        }

        /// <summary>
        /// Returns the correct ordered list for gathered Bom Items
        /// </summary>
        /// <returns></returns>
        public List<AMBomItem> GetBomItemsByLevelDesc()
        {
            return _dictionary.OrderByDescending(key => key.Value.Level).Select(kv => kv.Value.BOMItem).ToList();
        }

        [Serializable]
        protected class BomItemLevel
        {
            public AMBomItem BOMItem;
            public int? Level;
            public bool IsDefaultBom;

            public BomItemLevel(AMBomItem bomItem)
            {
                if (string.IsNullOrWhiteSpace(bomItem?.BOMID))
                {
                    throw new PXArgumentException(nameof(bomItem));
                }
                BOMItem = bomItem;
                Level = 0;
                IsDefaultBom = false;
            }
        }
    }
}