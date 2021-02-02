using System;
using System.Collections.Generic;
using System.Text;
using PX.Objects.AM.GraphExtensions;
using PX.Objects.AM.Attributes;
using PX.Common;
using PX.Data;
using PX.Objects.AM.CacheExtensions;
using PX.Objects.CS;
using PX.Objects.IN;
using PX.Objects.PO;
using VendorLocation = PX.Objects.CR.Standalone.Location;

namespace PX.Objects.AM
{
    /// <summary>
    /// Cache inventory and related values used throughout the MRP process
    /// </summary>
    public class MRPProcessCache
    {
        public MrpItemDictionary MrpItems;
        public DetailSupply DetailSupply;
        public CalendarHelper PurchaseCalendar;
        public readonly int MaxLowLevel;

        public MRPProcessCache(int maxLowLevel)
        {
            MrpItems = new MrpItemDictionary();
            DetailSupply = new DetailSupply();
            MaxLowLevel = maxLowLevel;
        }

        /// <summary>
        /// Only necessary when sub item is enabled. load sub item level replenishment settings from POVendorInventory and INItemSiteReplenishment
        /// </summary>
        /// <param name="mrpEngine">Calling MRP Graph</param>
        /// <param name="inItemSite">item warehouse record currently being processed</param>
        /// <param name="defaultSubItemID">Sub item ID already used to get default POVendorInventory record to exclude in next POVendorInventory records lookup</param>
        /// <param name="useSafetyStock">Indicator to use safety stock or reorder point as the Manufacturing safety stock value</param>
        /// <returns></returns>
        private static Dictionary<int, ReplenishmentSourceCache> BuildItemSiteReplenishmentSourceDictionary(MRPEngine mrpEngine, INItemSite inItemSite, int? defaultSubItemID, bool useSafetyStock)
        {
            var subItemRepSourceDictionary = new Dictionary<int, ReplenishmentSourceCache>();

            if (mrpEngine == null || inItemSite == null)
            {
                return subItemRepSourceDictionary;
            }

            // Vendor inventory per sub item
            if (inItemSite.PreferredVendorID != null)
            {
                foreach (POVendorInventory poVendorInventory in PXSelect<POVendorInventory,
                    Where<POVendorInventory.vendorID, Equal<Required<POVendorInventory.vendorID>>,
                        And<POVendorInventory.inventoryID, Equal<Required<POVendorInventory.inventoryID>>,
                            And<POVendorInventory.subItemID, NotEqual<Required<POVendorInventory.subItemID>>>>>>.Select(
                            mrpEngine, inItemSite.PreferredVendorID, inItemSite.InventoryID, defaultSubItemID.GetValueOrDefault()))
                {

                    //Not ideal as a sub query but for now only for those subitem customers
                    var bomIDManager = new PrimaryBomIDManager(mrpEngine)
                    {
                        BOMIDType = PrimaryBomIDManager.BomIDType.Planning
                    };
                    var bomID = bomIDManager.GetItemSitePrimary(inItemSite, poVendorInventory.SubItemID);
                    var bomItem = PrimaryBomIDManager.GetActiveRevisionBomItem(mrpEngine, bomID);

                    var replenishmentCache = new ReplenishmentSourceCache
                    {
                        InventoryID = poVendorInventory.InventoryID.GetValueOrDefault(),
                        SiteID = inItemSite.SiteID.GetValueOrDefault(),
                        SubItemID = poVendorInventory.SubItemID.GetValueOrDefault(),
                        ReplenishmentSource = inItemSite.ReplenishmentSource,
                        UseSafetyStock = useSafetyStock,
                        SafetyStock = inItemSite.SafetyStock.GetValueOrDefault(),
                        ReorderPoint = inItemSite.MinQty.GetValueOrDefault(),
                        LeadTime = poVendorInventory.VLeadTime.GetValueOrDefault() + poVendorInventory.AddLeadTimeDays.GetValueOrDefault(),
                        LotSize = PX.Objects.AM.InventoryHelper.ConvertToBaseUnits(mrpEngine, poVendorInventory, 
                            poVendorInventory.LotSize.GetValueOrDefault()),
                        MinOrderQty = PX.Objects.AM.InventoryHelper.ConvertToBaseUnits(mrpEngine, poVendorInventory,
                            poVendorInventory.MinOrdQty.GetValueOrDefault()),
                        MaxOrderQty = PX.Objects.AM.InventoryHelper.ConvertToBaseUnits(mrpEngine, poVendorInventory,
                            poVendorInventory.MaxOrdQty.GetValueOrDefault()),
                        BomID = bomItem?.BOMID,
                        BomRevisionID = bomItem?.RevisionID,
                        BomStartDate = bomItem?.EffStartDate,
                        BomEndDate = bomItem?.EffEndDate
                    };

                    if (!subItemRepSourceDictionary.ContainsKey(replenishmentCache.SubItemID))
                    {
                        subItemRepSourceDictionary.Add(replenishmentCache.SubItemID, replenishmentCache);
                    }
                }
            }

            // Item warehouse replenishment values per sub item
            if (inItemSite.SubItemOverride ?? false)
            {
                foreach (INItemSiteReplenishment inItemSiteReplenishment in PXSelect<INItemSiteReplenishment,
                    Where<INItemSiteReplenishment.siteID, Equal<Required<INItemSiteReplenishment.siteID>>,
                        And<INItemSiteReplenishment.inventoryID, Equal<Required<INItemSiteReplenishment.inventoryID>>>>>
                    .Select(mrpEngine, inItemSite.SiteID, inItemSite.InventoryID))
                {
                    ReplenishmentSourceCache replenishmentCache = null;
                    if (
                        subItemRepSourceDictionary.TryGetValue(
                            inItemSiteReplenishment.SubItemID.GetValueOrDefault(), out replenishmentCache))
                    {
                        replenishmentCache.UseSafetyStock = useSafetyStock;
                        replenishmentCache.SafetyStock = inItemSiteReplenishment.SafetyStock.GetValueOrDefault();
                        replenishmentCache.ReorderPoint = inItemSiteReplenishment.MinQty.GetValueOrDefault();

                        subItemRepSourceDictionary.Remove(replenishmentCache.SubItemID);
                    }

                    if (replenishmentCache == null)
                    {
                        replenishmentCache = new ReplenishmentSourceCache
                        {
                            InventoryID = inItemSiteReplenishment.InventoryID.GetValueOrDefault(),
                            SiteID = inItemSiteReplenishment.SiteID.GetValueOrDefault(),
                            SubItemID = inItemSiteReplenishment.SubItemID.GetValueOrDefault(),
                            ReplenishmentSource = inItemSite.ReplenishmentSource,
                            UseSafetyStock = useSafetyStock,
                            SafetyStock = inItemSiteReplenishment.SafetyStock.GetValueOrDefault(),
                            ReorderPoint = inItemSiteReplenishment.MinQty.GetValueOrDefault()
                        };
                    }

                    if (string.IsNullOrWhiteSpace(replenishmentCache.BomID))
                    {
                        //Not ideal as a sub query but for now only for those subitem customers
                        var bomIDManager = new PrimaryBomIDManager(mrpEngine)
                        {
                            BOMIDType = PrimaryBomIDManager.BomIDType.Planning
                        };
                        var bomId = bomIDManager.GetItemSitePrimary(inItemSite, replenishmentCache.SubItemID);
                        var bomItem = PrimaryBomIDManager.GetActiveRevisionBomItem(mrpEngine, bomId);
                        replenishmentCache.BomID = bomItem?.BOMID;
                        replenishmentCache.BomRevisionID = bomItem?.RevisionID;
                        replenishmentCache.BomStartDate = bomItem?.EffStartDate;
                        replenishmentCache.BomEndDate = bomItem?.EffEndDate;
                    }

                    if (!subItemRepSourceDictionary.ContainsKey(replenishmentCache.SubItemID))
                    {
                        subItemRepSourceDictionary.Add(replenishmentCache.SubItemID, replenishmentCache);
                    }
                }
            }

            foreach (AMSubItemDefault amSubItemDefault in PXSelect<AMSubItemDefault,
                    Where<AMSubItemDefault.siteID, Equal<Required<AMSubItemDefault.siteID>>,
                        And<AMSubItemDefault.inventoryID, Equal<Required<AMSubItemDefault.inventoryID>>>>>
                    .Select(mrpEngine, inItemSite.SiteID, inItemSite.InventoryID))
            {
                ReplenishmentSourceCache replenishmentCache = null;
                if (
                    subItemRepSourceDictionary.TryGetValue(
                        amSubItemDefault.SubItemID.GetValueOrDefault(), out replenishmentCache))
                {
                    if (string.IsNullOrWhiteSpace(replenishmentCache.BomID))
                    {
                        var bomIDManager = new PrimaryBomIDManager(mrpEngine)
                        {
                            BOMIDType = PrimaryBomIDManager.BomIDType.Planning
                        };
                        var bomId = bomIDManager.GetItemSitePrimary(inItemSite, replenishmentCache.SubItemID);
                        var bomItem = PrimaryBomIDManager.GetActiveRevisionBomItem(mrpEngine, bomId);
                        replenishmentCache.BomID = bomItem?.BOMID;
                        replenishmentCache.BomRevisionID = bomItem?.RevisionID;
                    }

                    subItemRepSourceDictionary.Remove(replenishmentCache.SubItemID);
                }

                if (replenishmentCache == null)
                {
                    var bomIDManager = new PrimaryBomIDManager(mrpEngine)
                    {
                        BOMIDType = PrimaryBomIDManager.BomIDType.Planning
                    };
                    var bomId = bomIDManager.GetItemSitePrimary(inItemSite, amSubItemDefault.SubItemID, true);
                    var bomItem = PrimaryBomIDManager.GetActiveRevisionBomItem(mrpEngine, bomId);

                    replenishmentCache = new ReplenishmentSourceCache
                    {
                        InventoryID = amSubItemDefault.InventoryID.GetValueOrDefault(),
                        SiteID = amSubItemDefault.SiteID.GetValueOrDefault(),
                        SubItemID = amSubItemDefault.SubItemID.GetValueOrDefault(),
                        ReplenishmentSource = inItemSite.ReplenishmentSource,
                        UseSafetyStock = useSafetyStock,
                        SafetyStock = inItemSite.SafetyStock.GetValueOrDefault(),
                        ReorderPoint = inItemSite.MinQty.GetValueOrDefault(),
                        LeadTime = 0,
                        LotSize = 0,
                        MinOrderQty = 0,
                        MaxOrderQty = 0,
                        BomID = bomItem?.BOMID,
                        BomRevisionID = bomItem?.RevisionID,
                        BomStartDate = bomItem?.EffStartDate,
                        BomEndDate = bomItem?.EffStartDate
                    };
                }

                if (string.IsNullOrWhiteSpace(replenishmentCache.BomID))
                {
                    //Not ideal as a sub query but for now only for those subitem customers
                    var bomIDManager = new PrimaryBomIDManager(mrpEngine)
                    {
                        BOMIDType = PrimaryBomIDManager.BomIDType.Planning
                    };
                    var bomId = bomIDManager.GetItemSitePrimary(inItemSite, replenishmentCache.SubItemID, true);
                    var bomItem = PrimaryBomIDManager.GetActiveRevisionBomItem(mrpEngine, bomId);
                    replenishmentCache.BomID = bomItem?.BOMID;
                    replenishmentCache.BomRevisionID = bomItem?.RevisionID;
                    replenishmentCache.BomStartDate = bomItem?.EffStartDate;
                    replenishmentCache.BomEndDate = bomItem?.EffEndDate;
                }

                if (!subItemRepSourceDictionary.ContainsKey(replenishmentCache.SubItemID))
                {
                    subItemRepSourceDictionary.Add(replenishmentCache.SubItemID, replenishmentCache);
                }
            }

            return subItemRepSourceDictionary;
        }

        /// <summary>
        /// Cache item replenishment source values
        /// This is necessary because the INItemRep is multiple per item. This defines it 1 to 1 for first item replenishment and per sub item
        /// </summary>
        private static Dictionary<int, Dictionary<int, ReplenishmentSourceCache>> BuildItemReplenishmentSourceDictionary(MRPEngine mrpEngine, bool useSafetyStock)
        {
            var itemRepSourceDictionary = new Dictionary<int, Dictionary<int, ReplenishmentSourceCache>>();
            var subItemRepSourceDictionary = new Dictionary<int, ReplenishmentSourceCache>();

            int previousItem = 0;
            string previousReplenishmentClass = string.Empty;

#if DEBUG
            var sw = System.Diagnostics.Stopwatch.StartNew();
#endif
            PXResultset<INItemRep> resultset = PXSelectJoin<INItemRep,
                InnerJoin<InventoryItem, On<INItemRep.inventoryID, Equal<InventoryItem.inventoryID>,
                        And<INItemRep.replenishmentSource, Equal<InventoryItemExt.aMReplenishmentSource>>>,
                    LeftJoin<INSubItemRep, On<INItemRep.inventoryID, Equal<INSubItemRep.inventoryID>,
                        And<INItemRep.replenishmentClassID, Equal<INSubItemRep.replenishmentClassID>>>>>,
                Where<InventoryItem.stkItem, Equal<boolTrue>,
                    And2<Where<InventoryItemExt.aMMRPItem, Equal<True>,
                        Or<InventoryItemExt.aMMRPItem, IsNull>>,
                        And<Where<INItemRep.terminationDate, Greater<Current<AccessInfo.businessDate>>,
                            Or<INItemRep.terminationDate, IsNull>>>>>,
                OrderBy<Asc<INItemRep.inventoryID>>>.Select(mrpEngine);

            int lastRow = resultset.Count;
            int counter = 0;
            foreach (PXResult<INItemRep, InventoryItem, INSubItemRep> result in resultset)
            {
                counter++;

                var itemRep = (INItemRep) result;
                var subItemRep = (INSubItemRep) result;
                var inventoryItem = (InventoryItem) result;

                if (itemRep == null || itemRep.InventoryID == null)
                {
                    continue;
                }

                var inventoryExt = inventoryItem.GetExtension<InventoryItemExt>();

                // Find the first item rep record per item/sub item and ignore all others
                if (counter > 1
                    && (( previousItem != itemRep.InventoryID ) 
                    || ( previousItem == itemRep.InventoryID && previousReplenishmentClass != itemRep.ReplenishmentClassID )))
                {
                    if (!itemRepSourceDictionary.ContainsKey(previousItem) && subItemRepSourceDictionary.Count == 0)
                    {
                        itemRepSourceDictionary.Add(previousItem, subItemRepSourceDictionary);
                    }

                    subItemRepSourceDictionary = new Dictionary<int, ReplenishmentSourceCache>();

                    if (previousItem == itemRep.InventoryID &&
                        (subItemRep.InventoryID == null || subItemRep.SubItemID == null))
                    {
                        previousItem = itemRep.InventoryID ?? 0;
                        previousReplenishmentClass = itemRep.ReplenishmentClassID;
                        continue;
                    }
                }

                var replenishmentCache = new ReplenishmentSourceCache()
                {
                    InventoryID = itemRep.InventoryID.GetValueOrDefault(),
                    SiteID = 0,
                    SubItemID = 0,
                    ReplenishmentSource = itemRep.ReplenishmentSource,
                    UseSafetyStock = useSafetyStock,
                    SafetyStock = inventoryExt != null && inventoryExt.AMSafetyStockOverride.GetValueOrDefault()
                        ? inventoryExt.AMSafetyStock.GetValueOrDefault()
                        : itemRep.SafetyStock.GetValueOrDefault(),
                    ReorderPoint = inventoryExt != null && inventoryExt.AMMinQtyOverride.GetValueOrDefault()
                            ? inventoryExt.AMMinQty.GetValueOrDefault()
                            : itemRep.MinQty.GetValueOrDefault()
                };

                if (subItemRep != null && subItemRep.InventoryID != null && subItemRep.SubItemID != null)
                {
                    //sub item replenishment
                    replenishmentCache.SubItemID = subItemRep.SubItemID.GetValueOrDefault();
                    replenishmentCache.UseSafetyStock = useSafetyStock;
                    replenishmentCache.SafetyStock = subItemRep.SafetyStock.GetValueOrDefault();
                    replenishmentCache.ReorderPoint = subItemRep.MinQty.GetValueOrDefault();
                }

                if (!subItemRepSourceDictionary.ContainsKey(replenishmentCache.SubItemID))
                {
                    subItemRepSourceDictionary.Add(replenishmentCache.SubItemID, replenishmentCache);
                }

                previousItem = itemRep.InventoryID.GetValueOrDefault();
                previousReplenishmentClass = itemRep.ReplenishmentClassID;

                if (counter == lastRow && !itemRepSourceDictionary.ContainsKey(previousItem))
                {
                    itemRepSourceDictionary.Add(previousItem, subItemRepSourceDictionary);
                }
            }
#if DEBUG
            sw.Stop();
            PXTraceHelper.WriteTimespan(sw.Elapsed, "MRPProcessCache.BuildItemReplenishmentSourceDictionary");
#endif

            return itemRepSourceDictionary;
        }

        /// <summary>
        /// Determines which POVendorInventory record to use
        /// </summary>
        /// <param name="defaultVendorInventory">The item default vendor record</param>
        /// <param name="itemSite">Item warehouse record</param>
        /// <param name="siteVendorInventory">item warehouse referenced vendor inventory record</param>
        /// <returns>POVendorInventory record to use for MRP Processing for the given item warehouse</returns>
        protected static POVendorInventory UseVendorInventory(POVendorInventory defaultVendorInventory, INItemSite itemSite, POVendorInventory siteVendorInventory)
        {
            POVendorInventory defaultVI = defaultVendorInventory != null &&
                                                 (defaultVendorInventory.VendorID.GetValueOrDefault() == 0 || !defaultVendorInventory.Active.GetValueOrDefault())
                ? null
                : defaultVendorInventory;

            if (itemSite == null 
                || itemSite.SiteID.GetValueOrDefault() == 0 
                || itemSite.PreferredVendorID.GetValueOrDefault() == 0
                || siteVendorInventory == null
                || siteVendorInventory.VendorID.GetValueOrDefault() == 0
                || !siteVendorInventory.Active.GetValueOrDefault())
            {
                return defaultVI;
            }

            if (itemSite.PreferredVendorOverride.GetValueOrDefault())
            {
                return siteVendorInventory;
            }

            return defaultVI;
        }

        /// <summary>
        /// Create a dictionary of all item and itemsite values cached for use during the MRP process and reduce nested/repeat queries
        /// </summary>
        public static MrpItemDictionary BuildMrpItemDictionary(MRPEngine mrpEngine, bool subItemEnabled)
        {
            var itemDictionary = new MrpItemDictionary {SubItemEnabled = subItemEnabled, UseSafetyStock = mrpEngine.Setup.Current.StockingMethod == AMRPSetup.MRPStockingMethod.SafetyStock};

            bool useSafetyStock = mrpEngine.Setup.Current.StockingMethod == AMRPSetup.MRPStockingMethod.SafetyStock;

            //  cache replenishment sources per item
            Dictionary<int, Dictionary<int, ReplenishmentSourceCache>> itemReplenishmentSourceDictionary = BuildItemReplenishmentSourceDictionary(mrpEngine, useSafetyStock);

            //Query sourced from replaced stored procedure "AMMInvGet"
            InventoryCache inventoryCache = null;

            LoadItemSiteDetails(mrpEngine, ref itemDictionary, ref inventoryCache, itemReplenishmentSourceDictionary, subItemEnabled);
            LoadItemWithoutItemSiteDetails(mrpEngine, ref itemDictionary, ref inventoryCache, itemReplenishmentSourceDictionary, subItemEnabled);

            //this covers the last record
            itemDictionary.Add(inventoryCache);

#if DEBUG
            AMDebug.TraceWriteMethodName(string.Format("{0} Inventory Items added to cache", itemDictionary.Count()));
#endif
            return itemDictionary;
        }

        private static IMRPCacheBomItem SelectCachebomItem(IMRPCacheBomItem b1, IMRPCacheBomItem b2)
        {
            return b1?.BOMID == null ? b2 : b1;
        }

        /// <summary>
        /// Load items with item site details
        /// </summary>
        private static void LoadItemSiteDetails(MRPEngine mrpEngine, ref MrpItemDictionary itemDictionary, ref InventoryCache inventoryCache,
            Dictionary<int, Dictionary<int, ReplenishmentSourceCache>> itemReplenishmentSourceDictionary, bool subItemEnabled)
        {
#if DEBUG
            var sb = new StringBuilder();
            var sw = System.Diagnostics.Stopwatch.StartNew();
            var isFirst = true;
#endif
            foreach (PXResult<INItemSite, InventoryItem, POVendorInventory, VendorLocation, BomItemActiveRevision, BomItemActiveRevision2, BomItemActiveRevision3, BomItemActiveRevision4> result in PXSelectJoin<INItemSite,
            InnerJoin<InventoryItem, On<INItemSite.inventoryID, Equal<InventoryItem.inventoryID>>,
            LeftJoin<POVendorInventory, On<InventoryItem.inventoryID, Equal<POVendorInventory.inventoryID>,
                And<InventoryItem.preferredVendorID, Equal<POVendorInventory.vendorID>,
                And<InventoryItem.preferredVendorLocationID, Equal<POVendorInventory.vendorLocationID>,
                And<IsNull<InventoryItem.defaultSubItemID, int0>, Equal<IsNull<POVendorInventory.subItemID, int0>>>>>>,
            LeftJoin<VendorLocation, On<InventoryItem.preferredVendorID, Equal<VendorLocation.bAccountID>,
                And<InventoryItem.preferredVendorLocationID, Equal<VendorLocation.locationID>>>,
            LeftJoin<BomItemActiveRevision, On<INItemSiteExt.aMBOMID, Equal<BomItemActiveRevision.bOMID>>,
                LeftJoin<BomItemActiveRevision2, On<INItemSiteExt.aMPlanningBOMID, Equal<BomItemActiveRevision2.bOMID>>,
                    LeftJoin<BomItemActiveRevision3, On<InventoryItemExt.aMBOMID, Equal<BomItemActiveRevision3.bOMID>>,
                        LeftJoin<BomItemActiveRevision4, On<InventoryItemExt.aMPlanningBOMID, Equal<BomItemActiveRevision4.bOMID>>>>>>>>>,
                Where<InventoryItem.stkItem, Equal<True>,
                    And<Where<InventoryItemExt.aMMRPItem, Equal<True>,
                            Or<InventoryItemExt.aMMRPItem, IsNull>>>>,
                OrderBy<Asc<INItemSite.inventoryID, Asc<INItemSite.siteID>>>>.Select(mrpEngine))
            {
#if DEBUG
                if (isFirst)
                {
                    sb.Append(PXTraceHelper.CreateTimespanMessage(sw.Elapsed, "MRPProcessCache.BuildMrpItemDictionary (Executing Query: Load Item Site Details) "));
                    isFirst = false;
                }
#endif
                var inventoryItem = (InventoryItem)result;
                var itemSite = (INItemSite)result;
                var poVendorInventoryDefault = (POVendorInventory)result;

                if (inventoryItem?.InventoryID == null || itemSite?.InventoryID == null)
                {
                    continue;
                }

                POVendorInventory poVendorInventorySite = itemSite.PreferredVendorID != null
                    && itemSite.PreferredVendorID.GetValueOrDefault() == inventoryItem.PreferredVendorID.GetValueOrDefault()
                    && itemSite.PreferredVendorLocationID.GetValueOrDefault() == inventoryItem.PreferredVendorLocationID.GetValueOrDefault() ? poVendorInventoryDefault : null;
                if (poVendorInventorySite == null)
                {
                    //handle itemsite with a different default vendor
                    poVendorInventorySite = GetPOVendorInventory(mrpEngine, itemSite, inventoryItem);
                }

                try
                {
                    // switching from planning to regular default bom
                    var itemSiteBom = SelectCachebomItem((BomItemActiveRevision)result, (BomItemActiveRevision2)result);
                    var inventoryBom = SelectCachebomItem((BomItemActiveRevision3)result, (BomItemActiveRevision4)result);

                    BuildDictionary(mrpEngine, ref itemDictionary, ref inventoryCache, itemReplenishmentSourceDictionary,
                        subItemEnabled, inventoryItem, inventoryBom, itemSite, itemSiteBom, UseVendorInventory(poVendorInventoryDefault, itemSite, poVendorInventorySite), (VendorLocation)result);
                }
                catch (Exception e)
                {
                    INSite site = PXSelect<INSite, Where<INSite.siteID, Equal<Required<INSite.siteID>>>>.Select(mrpEngine, itemSite.SiteID);

                    var errMsg = AM.Messages.GetLocal(AM.Messages.MRPErrorCachingItemWithWarehouse,
                        inventoryItem.InventoryCD.TrimIfNotNullEmpty(),
                        site?.SiteCD.TrimIfNotNullEmpty());
                    throw new MRPRegenException(errMsg, e);
                }
            }

#if DEBUG
            sb.Append(PXTraceHelper.CreateTimespanMessage(sw.Elapsed, "Process Complete"));
            AMDebug.TraceWriteMethodName(sb.ToString());
#endif
        }

        /// <summary>
        /// Load items without item site details
        /// </summary>
        private static void LoadItemWithoutItemSiteDetails(MRPEngine mrpEngine, ref MrpItemDictionary itemDictionary, ref InventoryCache inventoryCache,
            Dictionary<int, Dictionary<int, ReplenishmentSourceCache>> itemReplenishmentSourceDictionary, bool subItemEnabled)
        {
#if DEBUG
            var sb = new StringBuilder();
            var sw = System.Diagnostics.Stopwatch.StartNew();
            bool isFirst = true;
#endif
            foreach (PXResult<InventoryItem, INItemSite, VendorLocation, BomItemActiveRevision, BomItemActiveRevision2> result in PXSelectJoin<
                InventoryItem,
                LeftJoin<INItemSite, 
                    On<InventoryItem.inventoryID, Equal<INItemSite.inventoryID>>,
                LeftJoin<VendorLocation, 
                    On<InventoryItem.preferredVendorID, Equal<VendorLocation.bAccountID>,
                    And<InventoryItem.preferredVendorLocationID, Equal<VendorLocation.locationID>>>,
                LeftJoin<BomItemActiveRevision, 
                    On<InventoryItemExt.aMBOMID, Equal<BomItemActiveRevision.bOMID>>,
                LeftJoin<BomItemActiveRevision2, 
                    On<InventoryItemExt.aMPlanningBOMID, Equal<BomItemActiveRevision2.bOMID>>>>>>,
                Where<INItemSite.siteID, IsNull,
                    And<InventoryItem.stkItem, Equal<True>,
                    And<Where<InventoryItemExt.aMMRPItem, Equal<True>,
                        Or<InventoryItemExt.aMMRPItem, IsNull>>>>>,
                OrderBy<
                    Asc<InventoryItem.inventoryID>>>
                .Select(mrpEngine))
            {
#if DEBUG
                if (isFirst)
                {
                    sb.Append(PXTraceHelper.CreateTimespanMessage(sw.Elapsed, "MRPProcessCache.BuildMrpItemDictionary (Executing Query: Items without item warehouse details) "));
                    isFirst = false;
                }
#endif
                var inventoryItem = (InventoryItem)result;
                if (inventoryItem?.InventoryID == null)
                {
                    continue;
                }
                try
                {
                    // switching from planning to regular default bom
                    var inventoryBomRev = SelectCachebomItem((BomItemActiveRevision)result, (BomItemActiveRevision2)result);

                    BuildDictionary(mrpEngine, ref itemDictionary, ref inventoryCache, itemReplenishmentSourceDictionary,
                        subItemEnabled, inventoryItem, inventoryBomRev, null, null, null, (VendorLocation)result);
                }
                catch (Exception e)
                {
                    var errMsg = AM.Messages.GetLocal(AM.Messages.MRPErrorCachingItemWithoutWarehouse, inventoryItem.InventoryCD.TrimIfNotNullEmpty());
                    throw new MRPRegenException(errMsg, e);
                }
            }

#if DEBUG
            sb.Append(PXTraceHelper.CreateTimespanMessage(sw.Elapsed, "Process Complete"));
            AMDebug.TraceWriteMethodName(sb.ToString());
#endif
        }

        private static POVendorInventory GetPOVendorInventory(MRPEngine mrpEngine, INItemSite inItemSite, InventoryItem inventoryItem)
        {
            if (mrpEngine == null || inItemSite == null || inventoryItem == null
                || inItemSite.PreferredVendorID == null)
            {
                return null;
            }

            return PXSelect<POVendorInventory, 
                Where<POVendorInventory.inventoryID, Equal<Required<POVendorInventory.inventoryID>>,
                    And<POVendorInventory.vendorID, Equal<Required<POVendorInventory.vendorID>>,
                    And<POVendorInventory.vendorLocationID, Equal<Required<POVendorInventory.vendorLocationID>>,
                    And<IsNull<POVendorInventory.subItemID, int0>, Equal<Required<POVendorInventory.subItemID>>>>>>
                        >.SelectWindowed(mrpEngine, 0, 1, 
                        inItemSite.InventoryID, inItemSite.PreferredVendorID, inItemSite.PreferredVendorLocationID, inventoryItem.DefaultSubItemID.GetValueOrDefault());
        }
        
        private static void BuildDictionary(MRPEngine mrpEngine, ref MrpItemDictionary itemDictionary,
            ref InventoryCache inventoryCache,
            Dictionary<int, Dictionary<int, ReplenishmentSourceCache>> itemReplishmentSourceDictionary,
            bool subItemEnabled, InventoryItem inventoryItem, IMRPCacheBomItem inventoryItemBom, 
            INItemSite inItemSite, IMRPCacheBomItem inItemSiteBom, POVendorInventory poVendorInventory, VendorLocation vendorLocation)
        {
            var useSafetyStock = mrpEngine.Setup.Current.StockingMethod == AMRPSetup.MRPStockingMethod.SafetyStock;
            
            if (inventoryItem?.InventoryID == null)
            {
                return;
            }

            if (inventoryCache == null || inventoryCache.InventoryID != inventoryItem.InventoryID.GetValueOrDefault())
            {
                // -----------------------
                //  New item 
                //      add the last item to the dictionary and create a new item cache record.
                itemDictionary.Add(inventoryCache);
                // -----------------------

                inventoryCache = SetInventoryCache(inventoryItem);
                inventoryCache.BomRevisionID = inventoryItemBom?.RevisionID;
                inventoryCache.BomStartDate = inventoryItemBom?.EffStartDate;
                inventoryCache.BomEndDate = inventoryItemBom?.EffEndDate;
                if (inventoryCache.BomRevisionID == null)
                {
                    // no bom revision most likely indicates no active rev for bomid, so lets remove to prevent issues later
                    inventoryCache.BomID = null;
                }

                Dictionary<int, ReplenishmentSourceCache> repCache = null;
                if (itemReplishmentSourceDictionary.TryGetValue(inventoryCache.InventoryID, out repCache))
                {
                    inventoryCache = SetInventoryCache(repCache, inventoryCache);
                }
                if (string.IsNullOrWhiteSpace(inventoryCache.ReplenishmentSource))
                {
                    // Default as None. If a source is not manufactured it is treated as purchased
                    inventoryCache.ReplenishmentSource = INReplenishmentSource.None;
                }
            }

            if (inItemSite?.InventoryID == null || inItemSite.SiteID == null)
            {
#if DEBUG
                var dm = $"inventoryItem [{inventoryCache.InventoryID}] {inventoryItem.InventoryCD} <no warehouse> add to cache";
                if (inventoryCache.ItemStatus == INItemStatus.Inactive ||
                    inventoryCache.ItemStatus == INItemStatus.ToDelete)
                {
                    dm = $"{dm} [Inactive/ToDelete status]";
                }
                AMDebug.TraceWriteMethodName(dm);
#endif
                // we are processing an item without any warehouse detail
                return;
            }

            if (inventoryCache.ItemSiteCacheDictionary.ContainsKey(inItemSite.SiteID.GetValueOrDefault()))
            {
                //it is possible to have multiple POVendorInventory records for the same item (different Purchase Units)
                // If we make it here we already processed the first record
                return;
            }

            var itemSiteCache = SetItemSiteCache(inItemSite);
            itemSiteCache.DefaultSubItemID = inventoryCache.DefaultSubItemID;
            itemSiteCache.BomRevisionID = inItemSiteBom?.RevisionID;
            itemSiteCache.BomStartDate = inItemSiteBom?.EffStartDate;
            itemSiteCache.BomEndDate = inItemSiteBom?.EffEndDate;
            if (itemSiteCache.BomRevisionID == null)
            {
                // no bom revision most likely indicates no active rev for bomid, so lets remove to prevent issues later
                itemSiteCache.BomID = null;
            }

            //Replenishment Source
            if (string.IsNullOrWhiteSpace(itemSiteCache.ReplenishmentSource) &&
                !string.IsNullOrWhiteSpace(inventoryCache.ReplenishmentSource))
            {
                itemSiteCache.ReplenishmentSource = inventoryCache.ReplenishmentSource;
            }

            //Sub Item Override set when each subitem
            if (subItemEnabled)
            {
                //Not ideal as a sub query but for now only for those subitem customers
                var bomId = new PrimaryBomIDManager(mrpEngine).GetItemSitePrimary(inItemSite, itemSiteCache.DefaultSubItemID);

                var subItemDefault = poVendorInventory == null
                    ? inventoryCache.DefaultSubItemID
                    : poVendorInventory.SubItemID;
                // this loads subitem level replenishments
                var itemSiteReplishmentSourceDictionary = BuildItemSiteReplenishmentSourceDictionary(mrpEngine,
                    inItemSite, subItemDefault, useSafetyStock);
                itemSiteCache = SetItemSiteCache(itemSiteReplishmentSourceDictionary, itemSiteCache, inventoryCache.DefaultSubItemID);

                if (!string.IsNullOrWhiteSpace(bomId))
                {
                    var bomItem = PrimaryBomIDManager.GetActiveRevisionBomItem(mrpEngine, bomId);
                    itemSiteCache.BomID = bomItem?.BOMID;
                    itemSiteCache.BomRevisionID = bomItem?.RevisionID;
                    itemSiteCache.BomStartDate = bomItem?.EffStartDate;
                    itemSiteCache.BomEndDate = bomItem?.EffEndDate;
                }
            }

            itemSiteCache.UseSafetyStock = useSafetyStock;
            itemSiteCache.SafetyStock = inItemSite.SafetyStockOverride.GetValueOrDefault()
                ? inItemSite.SafetyStock.GetValueOrDefault()
                : (inItemSite.SafetyStock.GetValueOrDefault() != 0
                    ? inItemSite.SafetyStock.GetValueOrDefault()
                    : inventoryCache.SafetyStock);
            itemSiteCache.ReorderPoint = inItemSite.MinQtyOverride.GetValueOrDefault()
                ? inItemSite.MinQty.GetValueOrDefault()
                : (inItemSite.MinQty.GetValueOrDefault() != 0
                    ? inItemSite.MinQty.GetValueOrDefault()
                    : inventoryCache.SafetyStock);

            //non manufactured values (assumed source other than manufactured is a type of purchased)
            if (itemSiteCache.ReplenishmentSource != INReplenishmentSource.Manufactured)
            {
                if (vendorLocation?.BAccountID != null)
                {
                    // Make sure we at least get the vendor lead time if no specific vendor inventory record exists
                    itemSiteCache.LeadTime = vendorLocation.VLeadTime.GetValueOrDefault();
                }

                //if a primary vendor is found use its values for the following.
                if (poVendorInventory?.InventoryID != null)
                {
                    itemSiteCache.LeadTime = poVendorInventory.VLeadTime.GetValueOrDefault() + poVendorInventory.AddLeadTimeDays.GetValueOrDefault();
                    itemSiteCache.LotSize = PX.Objects.AM.InventoryHelper.ConvertToBaseUnits(mrpEngine, poVendorInventory,
                        poVendorInventory.LotSize.GetValueOrDefault());
                    itemSiteCache.MinOrderQty = PX.Objects.AM.InventoryHelper.ConvertToBaseUnits(mrpEngine, poVendorInventory,
                        poVendorInventory.MinOrdQty.GetValueOrDefault());
                    itemSiteCache.MaxOrderQty = PX.Objects.AM.InventoryHelper.ConvertToBaseUnits(mrpEngine, poVendorInventory,
                        poVendorInventory.MaxOrdQty.GetValueOrDefault());
                }

                if (inventoryCache.ReplenishmentSource == itemSiteCache.ReplenishmentSource)
                {
                    //  set inventory Cache to have the max/min values when not manufactured.
                    //  If the inventory values are used its a better starting point before getting to the itemsite values
                    if (itemSiteCache.LeadTime > inventoryCache.LeadTime)
                    {
                        inventoryCache.LeadTime = itemSiteCache.LeadTime;
                    }

                    if (itemSiteCache.MaxOrderQty > inventoryCache.MaxOrderQty)
                    {
                        inventoryCache.MaxOrderQty = itemSiteCache.MaxOrderQty;
                    }

                    if (itemSiteCache.MinOrderQty < inventoryCache.MinOrderQty)
                    {
                        inventoryCache.MinOrderQty = itemSiteCache.MinOrderQty;
                    }
                }
            }

            //Product Manager
            if (itemSiteCache.ProductManagerID == null && inventoryCache.ProductManagerID != null)
            {
                itemSiteCache.ProductManagerID = inventoryCache.ProductManagerID;
            }

#if DEBUG
            string debugMsg = string.Format("inventoryItem [{0}] {1} warehouse [{2}] add to cache", inventoryCache.InventoryID, inventoryItem.InventoryCD, itemSiteCache.SiteID);
            if (inventoryCache.ItemStatus == INItemStatus.Inactive ||
                inventoryCache.ItemStatus == INItemStatus.ToDelete)
            {
                debugMsg = string.Format("{0} [Inactive/ToDelete status]", debugMsg);
                AMDebug.TraceWriteMethodName(debugMsg);
            }
            //AMDebug.TraceWriteMethodName(debugMsg);
#endif
            inventoryCache.ItemSiteCacheDictionary.Add(itemSiteCache.SiteID, itemSiteCache);
        }

        /// <summary>
        /// Map InventoryItem to InventoryCache
        /// </summary>
        private static InventoryCache SetInventoryCache(InventoryItem inventoryItem, InventoryCache inventoryCache = null)
        {
            if (inventoryCache == null)
            {
                inventoryCache = new InventoryCache();
            }

            if (inventoryItem?.InventoryID == null)
            {
                return null;
            }

            inventoryCache.InventoryID = inventoryItem.InventoryID.GetValueOrDefault();
            inventoryCache.BaseUnit = inventoryItem.BaseUnit;
            inventoryCache.ItemStatus = inventoryItem.ItemStatus;
            inventoryCache.ProductManagerID = inventoryItem.ProductManagerID;
            inventoryCache.IsKitItem = inventoryItem.KitItem.GetValueOrDefault();
            inventoryCache.DefaultSubItemID = inventoryItem.DefaultSubItemID;
            inventoryCache.ItemClassID = inventoryItem.ItemClassID;

            var amExtension = PXCache<InventoryItem>.GetExtension<InventoryItemExt>(inventoryItem);

            if (amExtension == null)
            {
                return inventoryCache;
            }

            inventoryCache.BomID = amExtension.AMPlanningBOMID ?? amExtension.AMBOMID;
            inventoryCache.LowLevel = amExtension.AMLowLevel.GetValueOrDefault();
            inventoryCache.LeadTime = amExtension.AMMFGLeadTime.GetValueOrDefault();
            inventoryCache.MinOrderQty = amExtension.AMMinOrdQty.GetValueOrDefault();
            inventoryCache.MaxOrderQty = amExtension.AMMaxOrdQty.GetValueOrDefault();
            inventoryCache.LotSize = amExtension.AMLotSize.GetValueOrDefault();
            inventoryCache.QtyRoundUp = amExtension.AMQtyRoundUp.GetValueOrDefault();

            return inventoryCache;
        }

        /// <summary>
        /// Map ReplenishmentSourceCache to InventoryCache
        /// </summary>
        private static InventoryCache SetInventoryCache(Dictionary<int, ReplenishmentSourceCache> replenishmentSourceCache, InventoryCache inventoryCache)
        {
            if (replenishmentSourceCache == null || inventoryCache == null)
            {
                return inventoryCache;
            }

            ReplenishmentSourceCache repCache = null;
            if (replenishmentSourceCache.TryGetValue(0, out repCache))
            {
                inventoryCache.ReplenishmentSource = repCache.ReplenishmentSource;
                inventoryCache.SafetyStock = repCache.SafetyStock;
                inventoryCache.ReorderPoint = repCache.ReorderPoint;
            }
            else if (replenishmentSourceCache.Count > 0)
            {
                //get any first for source
                var e = replenishmentSourceCache.GetEnumerator();
                e.MoveNext();
                inventoryCache.ReplenishmentSource = e.Current.Value.ReplenishmentSource;
                inventoryCache.SafetyStock = 0;
                inventoryCache.ReorderPoint = 0;
            }

            inventoryCache.ReplenishmentSourceDictionary = replenishmentSourceCache;

            return inventoryCache;
        }

        private static ItemSiteCache SetItemSiteCache(Dictionary<int, ReplenishmentSourceCache> replenishmentSourceCache, ItemSiteCache itemSiteCache, int? defaultSubItemId = null)
        {
            if (replenishmentSourceCache == null || itemSiteCache == null)
            {
                return itemSiteCache;
            }

            itemSiteCache.DefaultSubItemID = defaultSubItemId;

            ReplenishmentSourceCache repCache = null;
            if (replenishmentSourceCache.TryGetValue(defaultSubItemId.GetValueOrDefault(), out repCache))
            {
                itemSiteCache.DefaultSubItemID = repCache.SubItemID;
                itemSiteCache.LeadTime = repCache.LeadTime;
                itemSiteCache.MinOrderQty = repCache.MinOrderQty;
                itemSiteCache.MaxOrderQty = repCache.MaxOrderQty;
                itemSiteCache.LotSize = repCache.LotSize;
            }

            itemSiteCache.ReplenishmentSourceDictionary = replenishmentSourceCache;

            return itemSiteCache;
        }

        /// <summary>
        /// Map INItemSite to ItemSiteCache
        /// </summary>
        private static ItemSiteCache SetItemSiteCache(INItemSite inItemSite, ItemSiteCache itemSiteCache = null)
        {
            if (itemSiteCache == null)
            {
                itemSiteCache = new ItemSiteCache();
            }

            if (inItemSite?.InventoryID == null || inItemSite.SiteID == null)
            {
                return null;
            }

            itemSiteCache.InventoryID = inItemSite.InventoryID.GetValueOrDefault();
            itemSiteCache.SiteID = inItemSite.SiteID.GetValueOrDefault();
            itemSiteCache.ReplenishmentSource = inItemSite.ReplenishmentSource;
            itemSiteCache.ReplenishmentClassID = inItemSite.ReplenishmentClassID;
            itemSiteCache.PreferredVendorID = inItemSite.PreferredVendorID;
            itemSiteCache.ProductManagerID = inItemSite.ProductManagerID;

            var amExtension = PXCache<INItemSite>.GetExtension<INItemSiteExt>(inItemSite);

            if (amExtension == null)
            {
                return itemSiteCache;
            }

            itemSiteCache.BomID = amExtension.AMPlanningBOMID ?? amExtension.AMBOMID;
            itemSiteCache.LeadTime = amExtension.AMMFGLeadTime.GetValueOrDefault();
            itemSiteCache.MinOrderQty = amExtension.AMMinOrdQty.GetValueOrDefault();
            itemSiteCache.MaxOrderQty = amExtension.AMMaxOrdQty.GetValueOrDefault();
            itemSiteCache.LotSize = amExtension.AMLotSize.GetValueOrDefault();

            return itemSiteCache;
        }

        public abstract class InventoryCacheBase
        {
            /// <summary>
            /// KEY
            /// </summary>
            public int InventoryID { get; set; }

            public int? DefaultSubItemID { get; set; }
            public string ItemStatus { get; set; }
            public int LowLevel { get; set; }
            public string BaseUnit { get; set; }
            public string BomID { get; set; }
            public string BomRevisionID { get; set; }
            public DateTime? BomStartDate { get; set; }
            public DateTime? BomEndDate { get; set; }
            public int LeadTime { get; set; }
            public decimal MinOrderQty { get; set; }
            public decimal MaxOrderQty { get; set; }
            public decimal LotSize { get; set; }
            public decimal SafetyStock { get; set; }
            public decimal ReorderPoint { get; set; }
            public int? ProductManagerID { get; set; }
            public string ReplenishmentSource { get; set; }
            public bool IsKitItem { get; set; }
            public bool QtyRoundUp { get; set; }
            public bool IsManufacturedItem { get { return ReplenishmentSource == INReplenishmentSource.Manufactured && !string.IsNullOrWhiteSpace(BomID); } }
            public bool HasReplenishmenSource { get { return ReplenishmentSource != INReplenishmentSource.None; } }
            public int? ItemClassID { get; set; }
        }

        public class InventoryCache : InventoryCacheBase
        {
            public Dictionary<int, ItemSiteCache> ItemSiteCacheDictionary;
            public Dictionary<int, ReplenishmentSourceCache> ReplenishmentSourceDictionary; 

            public InventoryCache()
            {
                ItemSiteCacheDictionary = new Dictionary<int, ItemSiteCache>();
                ReplenishmentSourceDictionary = new Dictionary<int, ReplenishmentSourceCache>();
            }
        }

        public class ItemSiteCache
        {
            /// <summary>
            /// KEY
            /// </summary>
            public int SiteID { get; set; }
            public int InventoryID { get; set; }
            public int? PreferredVendorID { get; set; }
            public string ReplenishmentClassID { get; set; }
            public decimal LotSize { get; set; }
            public int? DefaultSubItemID { get; set; }
            public string BomID { get; set; }
            public string BomRevisionID { get; set; }
            public DateTime? BomStartDate { get; set; }
            public DateTime? BomEndDate { get; set; }
            public int LeadTime { get; set; }
            public decimal MinOrderQty { get; set; }
            public decimal MaxOrderQty { get; set; }
            /// <summary>
            /// Indicates if use stock level is for safetystock (true) or reorderpoint (false)
            /// </summary>
            public bool UseSafetyStock { get; set; }
            public decimal SafetyStock { get; set; }
            public decimal ReorderPoint { get; set; }
            public int? ProductManagerID { get; set; }
            public string ReplenishmentSource { get; set; }
            public bool IsManufacturedItem => ReplenishmentSource.TrimIfNotNull() == INReplenishmentSource.Manufactured;
            public bool HasReplenishmentSource => ReplenishmentSource != INReplenishmentSource.None;

            public Dictionary<int, ReplenishmentSourceCache> ReplenishmentSourceDictionary; 

            public ItemSiteCache()
            {
                ReplenishmentSourceDictionary = new Dictionary<int, ReplenishmentSourceCache>();
            }

            public bool IsDateBetweenBomDates(DateTime? date)
            {
                return BomStartDate == null ||
                       date != null && BomStartDate <= date && Common.Dates.IsDateNull(BomEndDate) ||
                       date.BetweenInclusive(BomStartDate, BomEndDate);
            }
        }

        public class ReplenishmentSourceCache
        {
            public int InventoryID;
            public int SiteID;
            public int SubItemID;
            public string ReplenishmentSource;
            public string BomID;
            public string BomRevisionID;
            public DateTime? BomStartDate;
            public DateTime? BomEndDate;
            public bool UseSafetyStock;
            public decimal SafetyStock;
            public decimal ReorderPoint;
            public int LeadTime;
            public decimal MinOrderQty;
            public decimal MaxOrderQty;
            public decimal LotSize;
        }

        [Serializable]
        public sealed class MrpItemDictionary : Dictionary<int, InventoryCache>
        {
            private InventoryCache _currentInventoryCache;
            private ItemSiteCache _currentItemSiteCache;
            private ReplenishmentSourceCache _currentReplenishmentSourceCache;
            public int? LastLoadInventoryID { get; private set; }
            public int? LastLoadSiteID { get; private set; }
            public bool SubItemEnabled;
            public bool UseSafetyStock;

            public void Add(InventoryCache inventoryCache)
            {
                if (inventoryCache != null && inventoryCache.InventoryID > 0)
                {
                    if (this.ContainsKey(inventoryCache.InventoryID))
                    {
                        AMDebug.TraceWriteMethodName(string.Format("Attempt to add existing key {0}",inventoryCache.InventoryID));
                        return;
                    }

                    this.Add(inventoryCache.InventoryID, inventoryCache);
                }
            }

            public static InventoryCache GetInventoryCache(MrpItemDictionary dictionary, int? inventoryID)
            {
                if (dictionary == null
                    || !dictionary.Any_()
                    || (inventoryID ?? 0) <= 0)
                {
                    return null;
                }

                InventoryCache inventoryCache = null;
                if (dictionary.TryGetValue(inventoryID ?? 0, out inventoryCache))
                {
                    return inventoryCache;
                }

                return null;
            }

            public static ItemSiteCache GetItemSiteCache(InventoryCache inventoryCache, int? siteID)
            {
                if (inventoryCache == null
                    || !inventoryCache.ItemSiteCacheDictionary.Any_()
                    || (siteID ?? 0) <= 0)
                {
                    return null;
                }

                ItemSiteCache itemSiteCache = null;
                if (inventoryCache.ItemSiteCacheDictionary.TryGetValue(siteID ?? 0, out itemSiteCache))
                {
                    return itemSiteCache;
                }

                return null;
            }

            public ItemSiteCache GetCurrentItemSiteCache(int? inventoryID, int? siteID = null)
            {
                if (!LoadCurrent(inventoryID, siteID))
                {
                    return null;
                }

                return _currentItemSiteCache;
            }

            public ItemCache GetCurrentItemCache(int? inventoryID, int? siteID = null, int? subItemID = null)
            {
                if (!LoadCurrent(inventoryID, siteID))
                {
                    return null;
                }

                var itemCache = new ItemCache
                {
                    InventoryID = this._currentItemSiteCache.InventoryID,
                    SiteID = this._currentItemSiteCache.SiteID,
                    DefaultSubItemID = this._currentInventoryCache.DefaultSubItemID,
                    BaseUnit = this._currentInventoryCache.BaseUnit,
                    BomID = this._currentItemSiteCache.BomID,
                    BomRevisionID = this._currentItemSiteCache.BomRevisionID,
                    BomStartDate = this._currentItemSiteCache.BomStartDate,
                    BomEndDate = this._currentItemSiteCache.BomEndDate,
                    IsKitItem = this._currentInventoryCache.IsKitItem,
                    ItemStatus = this._currentInventoryCache.ItemStatus,
                    InvalidItemStatus = PX.Objects.AM.InventoryHelper.IsInvalidItemStatus(_currentInventoryCache.ItemStatus),
                    LeadTime = this._currentItemSiteCache.LeadTime,
                    LotSize = this._currentItemSiteCache.LotSize,
                    LowLevel = this._currentInventoryCache.LowLevel,
                    MinOrderQty = this._currentItemSiteCache.MinOrderQty,
                    MaxOrderQty = this._currentItemSiteCache.MaxOrderQty,
                    UseSafetyStock = this._currentItemSiteCache.UseSafetyStock,
                    SafetyStock = this._currentItemSiteCache.SafetyStock,
                    ReorderPoint = this._currentItemSiteCache.ReorderPoint,
                    PreferredVendorID = this._currentItemSiteCache.PreferredVendorID,
                    ReplenishmentClassID = this._currentItemSiteCache.ReplenishmentClassID,
                    ProductManagerID = this._currentItemSiteCache.ProductManagerID,
                    ReplenishmentSource = this._currentItemSiteCache.ReplenishmentSource,
                    QtyRoundUp = this._currentInventoryCache.QtyRoundUp,
                    IemClassID = this._currentInventoryCache.ItemClassID,
                };

                if (SubItemEnabled && LoadCurrentSubItem(subItemID ?? itemCache.DefaultSubItemID))
                {
                    // Sub Item specific replenishment settings
                    itemCache.ReplenishmentSource = _currentReplenishmentSourceCache.ReplenishmentSource;
                    itemCache.SafetyStock = _currentReplenishmentSourceCache.SafetyStock;
                    itemCache.ReorderPoint = _currentReplenishmentSourceCache.ReorderPoint;
                    itemCache.LeadTime = _currentReplenishmentSourceCache.LeadTime == 0 ? itemCache.LeadTime : _currentReplenishmentSourceCache.LeadTime;
                    itemCache.MinOrderQty = _currentReplenishmentSourceCache.MinOrderQty == 0 ? itemCache.MinOrderQty : _currentReplenishmentSourceCache.MinOrderQty;
                    itemCache.MaxOrderQty = _currentReplenishmentSourceCache.MaxOrderQty == 0 ? itemCache.MaxOrderQty : _currentReplenishmentSourceCache.MaxOrderQty;
                    itemCache.LotSize = _currentReplenishmentSourceCache.LotSize == 0 ? itemCache.LotSize : _currentReplenishmentSourceCache.LotSize;
                    itemCache.BomID = _currentReplenishmentSourceCache.BomID;
                    itemCache.BomRevisionID = _currentReplenishmentSourceCache.BomRevisionID;
                    itemCache.BomStartDate = _currentReplenishmentSourceCache.BomStartDate;
                    itemCache.BomEndDate = _currentReplenishmentSourceCache.BomEndDate;
                }

                return itemCache;
            }

            private bool LoadCurrentSubItem(int? subItemID)
            {
                //pull site level first
                if ((_currentReplenishmentSourceCache == null 
                    || _currentReplenishmentSourceCache.InventoryID != _currentItemSiteCache.InventoryID 
                    || _currentReplenishmentSourceCache.SiteID != _currentItemSiteCache.SiteID
                    || _currentReplenishmentSourceCache.SubItemID != subItemID.GetValueOrDefault())
                    && _currentItemSiteCache.ReplenishmentSourceDictionary.Count > 0)
                {
                    _currentReplenishmentSourceCache = null;

                    if (_currentItemSiteCache.ReplenishmentSourceDictionary.TryGetValue(
                            subItemID.GetValueOrDefault(), out _currentReplenishmentSourceCache))
                    {
                        return true;
                    }
                }

                // pull generic item level
                if ((_currentReplenishmentSourceCache == null
                    || _currentReplenishmentSourceCache.InventoryID != _currentInventoryCache.InventoryID
                    || _currentReplenishmentSourceCache.SubItemID != subItemID.GetValueOrDefault())
                    && _currentInventoryCache.ReplenishmentSourceDictionary.Count > 0)
                {
                    _currentReplenishmentSourceCache = null;

                    if (_currentInventoryCache.ReplenishmentSourceDictionary.TryGetValue(
                            subItemID.GetValueOrDefault(), out _currentReplenishmentSourceCache))
                    {
                        return true;
                    }
                }

                return _currentReplenishmentSourceCache != null && _currentReplenishmentSourceCache.InventoryID == _currentInventoryCache.InventoryID;
            }

            private bool LoadCurrent(int? inventoryID, int? siteID = null)
            {
                LastLoadInventoryID = inventoryID;
                LastLoadSiteID = siteID;

                if (inventoryID.GetValueOrDefault() <= 0)
                {
                    _currentInventoryCache = null;
                    _currentItemSiteCache = null;
                    _currentReplenishmentSourceCache = null;
                    return false;
                }

                bool foundItem = true;

                if (_currentInventoryCache == null
                    || _currentInventoryCache.InventoryID != inventoryID)
                {
                    _currentInventoryCache = null;
                    _currentItemSiteCache = null;
                    _currentReplenishmentSourceCache = null;

                    InventoryCache inventoryCache = null;
                    foundItem = this.TryGetValue(inventoryID.GetValueOrDefault(), out inventoryCache);

                    if (foundItem)
                    {
                        _currentInventoryCache = inventoryCache;
                    }
                }

                //either no item cache found or the siteid is invalid/null - done
                if (_currentInventoryCache == null || (siteID ?? 0) <= 0)
                {
                    _currentItemSiteCache = null;
                    return foundItem;
                }

                if (_currentItemSiteCache == null
                    || _currentItemSiteCache.InventoryID != inventoryID
                    || _currentItemSiteCache.SiteID != siteID)
                {
                    ItemSiteCache itemSiteCache = null;
                    _currentItemSiteCache = null;
                    if (_currentInventoryCache.ItemSiteCacheDictionary.TryGetValue(siteID ?? 0, out itemSiteCache))
                    {
                        _currentItemSiteCache = itemSiteCache;
                    }
                    else
                    {
                        //default filler using the item specific settings
                        //  This might happen if there is no INItemSite records yet for the item
                        _currentItemSiteCache = MakeItemSiteCacheFromInventoryCache(_currentInventoryCache);
                        _currentItemSiteCache.SiteID = siteID.GetValueOrDefault(-1);
                        _currentItemSiteCache.UseSafetyStock = this.UseSafetyStock;
                    }
                }

                return foundItem;
            }

            private static ItemSiteCache MakeItemSiteCacheFromInventoryCache(InventoryCache inventoryCache)
            {
                var itemSiteCache = new ItemSiteCache();

                if (inventoryCache == null)
                {
                    return itemSiteCache;
                }

                itemSiteCache.InventoryID = inventoryCache.InventoryID;
                itemSiteCache.DefaultSubItemID = inventoryCache.DefaultSubItemID;
                itemSiteCache.LeadTime = inventoryCache.LeadTime;
                itemSiteCache.ProductManagerID = inventoryCache.ProductManagerID;
                itemSiteCache.BomID = inventoryCache.BomID;
                itemSiteCache.BomRevisionID = inventoryCache.BomRevisionID;
                itemSiteCache.BomStartDate = inventoryCache.BomStartDate;
                itemSiteCache.BomEndDate = inventoryCache.BomEndDate;
                itemSiteCache.SafetyStock = inventoryCache.SafetyStock;
                itemSiteCache.MinOrderQty = inventoryCache.MinOrderQty;
                itemSiteCache.MaxOrderQty = inventoryCache.MaxOrderQty;
                itemSiteCache.LotSize = inventoryCache.LotSize;
                itemSiteCache.ReplenishmentSource = inventoryCache.ReplenishmentSource;

                return itemSiteCache;
            }
        }

        public class ItemCache : ItemSiteCache
        {
            public string ItemStatus { get; set; }
            public bool InvalidItemStatus { get; set; }
            public int LowLevel { get; set; }
            public string BaseUnit { get; set; }
            public bool IsKitItem { get; set; }
            public bool QtyRoundUp { get; set; }
            public int? IemClassID { get; set; }
        }

        /// <summary>
        /// Determine/Load the purchase calendar (Purchase Calendar is Optional)
        /// </summary>
        /// <param name="mrpEngineGraph">Calling MRP Engine Graph</param>
        /// <returns>True if the purchase calendar was loaded</returns>
        public virtual bool LoadPurchaseCalendar(MRPEngine mrpEngineGraph)
        {
            if (mrpEngineGraph.Setup.Current != null
                && !string.IsNullOrWhiteSpace(mrpEngineGraph.Setup.Current.PurchaseCalendarID))
            {
                if (PurchaseCalendar == null
                    || PurchaseCalendar.CurrentCalendarId != mrpEngineGraph.Setup.Current.PurchaseCalendarID)
                {
                    try
                    {
                        PurchaseCalendar = new CalendarHelper(mrpEngineGraph, mrpEngineGraph.Setup.Current.PurchaseCalendarID);
                        //purchase calendar calculates date back from a plan date (to calculate action date)
                        PurchaseCalendar.CalendarReadDirection = ReadDirection.Backward;
                    }
                    catch (Exception exception)
                    {
                        if (exception is InvalidWorkCalendarException)
                        {
                            //if the purchase calendar is no longer valid -> avoid repeat exceptions for each action date calculation
                            mrpEngineGraph.Setup.Current.PurchaseCalendarID = null;
                        }

                        return false;
                    }
                }

                return PurchaseCalendar != null && !string.IsNullOrWhiteSpace(PurchaseCalendar.CurrentCalendarId);
            }

            return false;
        }

        internal interface IMRPCacheBomItem
        {
            string BOMID { get; set; }
            string RevisionID { get; set; }
            DateTime? EffStartDate { get; set; }
            DateTime? EffEndDate { get; set; }
        }

        [PXHidden]
        [Serializable]
        [PXProjection(typeof(Select2<AMBomItem,
            InnerJoin<AMBomItemActiveAggregate,
                On<AMBomItem.bOMID, Equal<AMBomItemActiveAggregate.bOMID>,
                    And<AMBomItem.revisionID, Equal<AMBomItemActiveAggregate.revisionID>>>>>), Persistent = false)]
        public class BomItemActiveRevision : IBqlTable, IMRPCacheBomItem
        {
            #region BOMID
            public abstract class bOMID : PX.Data.BQL.BqlString.Field<bOMID> { }
            protected string _BOMID;
            [BomID(IsKey = true, BqlField = typeof(AMBomItem.bOMID))]
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
            [RevisionIDField(BqlField = typeof(AMBomItem.revisionID))]
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
            #region EffStartDate
            public abstract class effStartDate : PX.Data.BQL.BqlDateTime.Field<effStartDate> { }

            protected DateTime? _EffStartDate;
            [PXDBDate(BqlField = typeof(AMBomItem.effStartDate))]
            [PXUIField(DisplayName = "Start Date")]
            public virtual DateTime? EffStartDate
            {
                get
                {
                    return this._EffStartDate;
                }
                set
                {
                    this._EffStartDate = value;
                }
            }
            #endregion
            #region EffEndDate
            public abstract class effEndDate : PX.Data.BQL.BqlDateTime.Field<effEndDate> { }

            protected DateTime? _EffEndDate;
            [PXDBDate(BqlField = typeof(AMBomItem.effEndDate))]
            [PXUIField(DisplayName = "End Date")]
            public virtual DateTime? EffEndDate
            {
                get
                {
                    return this._EffEndDate;
                }
                set
                {
                    this._EffEndDate = value;
                }
            }
            #endregion
        }

        [PXHidden]
        [Serializable]
        [PXProjection(typeof(Select2<AMBomItem,
            InnerJoin<AMBomItemActiveAggregate,
                On<AMBomItem.bOMID, Equal<AMBomItemActiveAggregate.bOMID>,
                    And<AMBomItem.revisionID, Equal<AMBomItemActiveAggregate.revisionID>>>>>), Persistent = false)]
        public class BomItemActiveRevision2 : IBqlTable, IMRPCacheBomItem
        {
            #region BOMID
            public abstract class bOMID : PX.Data.BQL.BqlString.Field<bOMID> { }
            protected string _BOMID;
            [BomID(IsKey = true, BqlField = typeof(AMBomItem.bOMID))]
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
            [RevisionIDField(BqlField = typeof(AMBomItem.revisionID))]
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
            #region EffStartDate
            public abstract class effStartDate : PX.Data.BQL.BqlDateTime.Field<effStartDate> { }

            protected DateTime? _EffStartDate;
            [PXDBDate(BqlField = typeof(AMBomItem.effStartDate))]
            [PXUIField(DisplayName = "Start Date")]
            public virtual DateTime? EffStartDate
            {
                get
                {
                    return this._EffStartDate;
                }
                set
                {
                    this._EffStartDate = value;
                }
            }
            #endregion
            #region EffEndDate
            public abstract class effEndDate : PX.Data.BQL.BqlDateTime.Field<effEndDate> { }

            protected DateTime? _EffEndDate;
            [PXDBDate(BqlField = typeof(AMBomItem.effEndDate))]
            [PXUIField(DisplayName = "End Date")]
            public virtual DateTime? EffEndDate
            {
                get
                {
                    return this._EffEndDate;
                }
                set
                {
                    this._EffEndDate = value;
                }
            }
            #endregion
        }

        [PXHidden]
        [Serializable]
        [PXProjection(typeof(Select2<AMBomItem,
            InnerJoin<AMBomItemActiveAggregate,
                On<AMBomItem.bOMID, Equal<AMBomItemActiveAggregate.bOMID>,
                    And<AMBomItem.revisionID, Equal<AMBomItemActiveAggregate.revisionID>>>>>), Persistent = false)]
        public class BomItemActiveRevision3 : IBqlTable, IMRPCacheBomItem
        {
            #region BOMID
            public abstract class bOMID : PX.Data.BQL.BqlString.Field<bOMID> { }
            protected string _BOMID;
            [BomID(IsKey = true, BqlField = typeof(AMBomItem.bOMID))]
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
            [RevisionIDField(BqlField = typeof(AMBomItem.revisionID))]
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
            #region EffStartDate
            public abstract class effStartDate : PX.Data.BQL.BqlDateTime.Field<effStartDate> { }

            protected DateTime? _EffStartDate;
            [PXDBDate(BqlField = typeof(AMBomItem.effStartDate))]
            [PXUIField(DisplayName = "Start Date")]
            public virtual DateTime? EffStartDate
            {
                get
                {
                    return this._EffStartDate;
                }
                set
                {
                    this._EffStartDate = value;
                }
            }
            #endregion
            #region EffEndDate
            public abstract class effEndDate : PX.Data.BQL.BqlDateTime.Field<effEndDate> { }

            protected DateTime? _EffEndDate;
            [PXDBDate(BqlField = typeof(AMBomItem.effEndDate))]
            [PXUIField(DisplayName = "End Date")]
            public virtual DateTime? EffEndDate
            {
                get
                {
                    return this._EffEndDate;
                }
                set
                {
                    this._EffEndDate = value;
                }
            }
            #endregion
        }

        [PXHidden]
        [Serializable]
        [PXProjection(typeof(Select2<AMBomItem,
            InnerJoin<AMBomItemActiveAggregate,
                On<AMBomItem.bOMID, Equal<AMBomItemActiveAggregate.bOMID>,
                    And<AMBomItem.revisionID, Equal<AMBomItemActiveAggregate.revisionID>>>>>), Persistent = false)]
        public class BomItemActiveRevision4 : IBqlTable, IMRPCacheBomItem
        {
            #region BOMID
            public abstract class bOMID : PX.Data.BQL.BqlString.Field<bOMID> { }
            protected string _BOMID;
            [BomID(IsKey = true, BqlField = typeof(AMBomItem.bOMID))]
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
            [RevisionIDField(BqlField = typeof(AMBomItem.revisionID))]
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
            #region EffStartDate
            public abstract class effStartDate : PX.Data.BQL.BqlDateTime.Field<effStartDate> { }

            protected DateTime? _EffStartDate;
            [PXDBDate(BqlField = typeof(AMBomItem.effStartDate))]
            [PXUIField(DisplayName = "Start Date")]
            public virtual DateTime? EffStartDate
            {
                get
                {
                    return this._EffStartDate;
                }
                set
                {
                    this._EffStartDate = value;
                }
            }
            #endregion
            #region EffEndDate
            public abstract class effEndDate : PX.Data.BQL.BqlDateTime.Field<effEndDate> { }

            protected DateTime? _EffEndDate;
            [PXDBDate(BqlField = typeof(AMBomItem.effEndDate))]
            [PXUIField(DisplayName = "End Date")]
            public virtual DateTime? EffEndDate
            {
                get
                {
                    return this._EffEndDate;
                }
                set
                {
                    this._EffEndDate = value;
                }
            }
            #endregion
        }
    }
}