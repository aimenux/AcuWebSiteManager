using PX.Objects.AM.Attributes;
using PX.Data;
using PX.Objects.CS;
using PX.Objects.IN;
using System;
using PX.Objects.AM.CacheExtensions;
using PX.Objects.AM.GraphExtensions;

namespace PX.Objects.AM
{
    /// <summary>
    /// Manage Update,Insert,Delete of Default and Planning BOM ids
    /// </summary>
    public class PrimaryBomIDManager : IDManagerBase
    {
        /// <summary>
        /// BOM Type to Manange. Set the correct type to get/set the correct BOMID while using BOM ID Manager
        /// </summary>
        public BomIDType BOMIDType;

        /// <summary>
        /// BOM Type to Manange
        /// </summary>
        public enum BomIDType
        {
            /// <summary>
            /// Default BOM ID Type (Default value)
            /// </summary>
            Default,

            /// <summary>
            /// Planning BOM ID Type
            /// </summary>
            Planning
        }

        public PrimaryBomIDManager(PXGraph graph) : base(graph)
        {
            PersistChanges = true;
            BOMIDType = BomIDType.Default;
        }

        /// <summary>
        /// Is the given BOM Item setup as a default or planning BOM?
        /// </summary>
        public bool IsPrimaryBomID(AMBomItem bomItem)
        {
            return bomItem != null && IsPrimaryBomID(bomItem.BOMID);
        }

        /// <summary>
        /// Is the given BOMID setup as a default or planning BOM?
        /// </summary>
        public bool IsPrimaryBomID(string bomID)
        {
            if (string.IsNullOrWhiteSpace(bomID))
            {
                return false;
            }

            InventoryItem inventoryItem = PXSelect<InventoryItem,
                Where<InventoryItemExt.aMBOMID, Equal<Required<InventoryItemExt.aMBOMID>>,
                    Or
                    <InventoryItemExt.aMPlanningBOMID, Equal<Required<InventoryItemExt.aMPlanningBOMID>>
                    >>
            >.SelectWindowed(_Graph, 0, 1, bomID, bomID);

            if (inventoryItem != null)
            {
                return true;
            }

            INItemSite inItemSite = PXSelect<INItemSite,
                Where<INItemSiteExt.aMBOMID, Equal<Required<INItemSiteExt.aMBOMID>>,
                    Or<INItemSiteExt.aMPlanningBOMID, Equal<Required<INItemSiteExt.aMPlanningBOMID>>>>
            >.SelectWindowed(_Graph, 0, 1, bomID, bomID);

            if (inItemSite != null)
            {
                return true;
            }

            if (InventoryHelper.SubItemFeatureEnabled && IsPrimaryBomIDBySubItem(bomID) != null)
            {
                return true;
            }

            return false;
        }

        public bool? IsPrimaryBomIDBySubItem(string bomID)
        {
            AMSubItemDefault subItemDefault = PXSelect<AMSubItemDefault,
                Where<AMSubItemDefault.bOMID, Equal<Required<AMSubItemDefault.bOMID>>,
                    Or<AMSubItemDefault.planningBOMID, Equal<Required<AMSubItemDefault.planningBOMID>>>>
            >.SelectWindowed(_Graph, 0, 1, bomID, bomID);

            if (subItemDefault != null)
            {
                return true;
            }

            return null;
        }

        /// <summary>
        /// Get the correct BOM ID based on class requested bom type
        /// </summary>
        protected string GetBomID(InventoryItemExt row, bool useDefault)
        {
            if (row == null)
            {
                return null;
            }

            if (BOMIDType == BomIDType.Planning
                && (!useDefault || !string.IsNullOrWhiteSpace(row.AMPlanningBOMID)))
            {
                return row.AMPlanningBOMID;
            }

            return row.AMBOMID;
        }

        /// <summary>
        /// Get the correct BOM ID based on class requested bom type
        /// </summary>
        protected string GetBomID(INItemSiteExt row, bool useDefault)
        {
            if (row == null)
            {
                return null;
            }

            if (BOMIDType == BomIDType.Planning
                && (!useDefault || !string.IsNullOrWhiteSpace(row.AMPlanningBOMID)))
            {
                return row.AMPlanningBOMID;
            }

            return row.AMBOMID;
        }

        /// <summary>
        /// Get the correct BOM ID based on class requested bom type
        /// </summary>
        protected string GetBomID(AMSubItemDefault row, bool useDefault)
        {
            if (row == null)
            {
                return null;
            }

            if (BOMIDType == BomIDType.Planning
                && (!useDefault || !string.IsNullOrWhiteSpace(row.PlanningBOMID)))
            {
                return row.PlanningBOMID;
            }

            return row.BOMID;
        }

        /// <summary>
        /// Get an items primary bom looking at all primary BOM levels starting with the lowest/highest (SubItem, Warehouse, Item)
        /// </summary>
        /// <param name="inventoryID"></param>
        /// <param name="siteID"></param>
        /// <param name="subItemID"></param>
        /// <returns>BOM ID</returns>
        public string GetPrimaryAllLevels(int? inventoryID, int? siteID, int? subItemID)
        {
            var warehouseBomID = GetItemSitePrimary(inventoryID, siteID, subItemID);

            return string.IsNullOrWhiteSpace(warehouseBomID) ? GetItemPrimary(inventoryID, subItemID) : warehouseBomID;
        }

        public string GetItemPrimary(int? inventoryID, int? subItemID)
        {
            return GetItemPrimary(GetInventoryItem(inventoryID), subItemID);
        }

        public string GetItemPrimary(InventoryItem inventoryItem, int? subItemID, bool useDefault = false)
        {
            if (inventoryItem == null)
            {
                return string.Empty;
            }

            if (_IsIDBySubItem)
            {
                string subItemBomID = GetItemSubItemPrimary(inventoryItem.InventoryID, subItemID);

                if (!string.IsNullOrWhiteSpace(subItemBomID))
                {
                    return subItemBomID;
                }
            }

            return GetBomID(GetInventoryItemExt(inventoryItem), useDefault);
        }

        public string GetItemSitePrimary(int? inventoryID, int? siteID, int? subItemID)
        {
            return GetItemSitePrimary(GetINItemSite(inventoryID, siteID), subItemID);
        }

        public string GetItemSitePrimary(INItemSite inItemSite, int? subItemID, bool useDefault = false)
        {
            if (inItemSite == null)
            {
                return string.Empty;
            }

            if (_IsIDBySubItem)
            {
                string subItemBomID = GetItemSiteSubItemPrimary(inItemSite.InventoryID, inItemSite.SiteID, subItemID);

                if (!string.IsNullOrWhiteSpace(subItemBomID))
                {
                    return subItemBomID;
                }
            }

            return GetBomID(GetINItemSiteExt(inItemSite), useDefault);
        }

        /// <summary>
        /// Get the primary BOM for the given item/subitem combo
        /// </summary>
        private string GetItemSubItemPrimary(int? inventoryID, int? subItemID, bool useDefault = false)
        {
            if (inventoryID == null || subItemID == null)
            {
                return string.Empty;
            }

            AMSubItemDefault amSubItemDefault = PXSelect<AMSubItemDefault,
                Where<AMSubItemDefault.inventoryID, Equal<Required<AMSubItemDefault.inventoryID>>,
                    And<AMSubItemDefault.subItemID, Equal<Required<AMSubItemDefault.subItemID>>,
                        And<AMSubItemDefault.isItemDefault, Equal<boolTrue>>>>>.Select(_Graph, inventoryID, subItemID);

            return GetBomID(amSubItemDefault, useDefault);
        }

        /// <summary>
        /// Get the primary BOM for the given item/site/subitem combo
        /// </summary>
        private string GetItemSiteSubItemPrimary(int? inventoryID, int? siteID, int? subItemID, bool useDefault = false)
        {
            if (inventoryID == null || siteID == null || subItemID == null)
            {
                return string.Empty;
            }

            AMSubItemDefault amSubItemDefault = PXSelect<AMSubItemDefault,
                Where<AMSubItemDefault.inventoryID, Equal<Required<AMSubItemDefault.inventoryID>>,
                    And<AMSubItemDefault.siteID, Equal<Required<AMSubItemDefault.siteID>>,
                        And<AMSubItemDefault.subItemID, Equal<Required<AMSubItemDefault.subItemID>>>>>>.Select(_Graph,
                inventoryID, siteID, subItemID);

            return GetBomID(amSubItemDefault, useDefault);
        }

        public bool HasItemPrimary(AMBomItem bomItem)
        {
            if (bomItem == null)
            {
                return false;
            }

            return HasItemPrimary(GetInventoryItem(bomItem.InventoryID));
        }

        public bool HasItemPrimary(int? inventoryID)
        {
            return HasItemPrimary(GetInventoryItem(inventoryID));
        }

        public bool HasItemPrimary(InventoryItem inventoryItem)
        {
            var ext = GetInventoryItemExt(inventoryItem);

            if (ext == null)
            {
                return false;
            }

            return !string.IsNullOrWhiteSpace(GetBomID(ext, false));
        }

        public bool HasItemSubItemPrimary(AMBomItem bomItem)
        {
            if (bomItem == null)
            {
                return false;
            }

            return HasItemSubItemPrimary(bomItem.InventoryID, bomItem.SubItemID);
        }

        public bool HasItemSubItemPrimary(int? inventoryID, int? subItemID)
        {
            return !string.IsNullOrWhiteSpace(GetItemSubItemPrimary(inventoryID, subItemID));
        }

        public bool HasItemSitePrimary(AMBomItem bomItem)
        {
            if (bomItem == null)
            {
                return false;
            }

            return HasItemSitePrimary(GetINItemSite(bomItem.InventoryID, bomItem.SiteID));
        }

        public bool HasItemSitePrimary(int? inventoryID, int? siteID)
        {
            return HasItemSitePrimary(GetINItemSite(inventoryID, siteID));
        }

        public bool HasItemSitePrimary(INItemSite inItemSite)
        {
            var ext = GetINItemSiteExt(inItemSite);

            if (ext == null)
            {
                return false;
            }

            return !string.IsNullOrWhiteSpace(GetBomID(ext, false));
        }

        public bool HasItemSiteSubItemPrimary(AMBomItem bomItem)
        {
            if (bomItem == null)
            {
                return false;
            }

            return HasItemSiteSubItemPrimary(bomItem.InventoryID, bomItem.SiteID, bomItem.SubItemID);
        }

        public bool HasItemSiteSubItemPrimary(int? inventoryID, int? siteID, int? subItemID)
        {
            return !string.IsNullOrWhiteSpace(GetItemSiteSubItemPrimary(inventoryID, siteID, subItemID));
        }

        #region SET

        /// <summary>
        /// Set the BOM ID for item/warehouse/subitem if a current BOM ID does not exist (first for any one of the combinations)
        /// </summary>
        public void SetAllFirstOnlyPrimary(AMBomItem bomItem)
        {
            if (bomItem == null)
            {
                return;
            }

            SetAllFirstOnlyPrimary(bomItem.BOMID, bomItem.InventoryID, bomItem.SiteID, bomItem.SubItemID);
        }

        /// <summary>
        /// Set the BOM ID for item/warehouse/subitem if a current BOM ID does not exist (first for any one of the combinations)
        /// </summary>
        public void SetAllFirstOnlyPrimary(string bomID, int? inventoryID, int? siteID, int? subItemID)
        {
            SetPrimary(bomID, inventoryID, siteID, subItemID, false, false, false);
        }

        /// <summary>
        /// Override the BOM ID of an Inventory ID for any given BOM ID level (item/warehouse/subitem)
        /// </summary>
        /// <param name="bomItem">BOM record of the BOM ID to be set</param>
        /// <param name="overrideItem">Should we override the current bom ID at the Item level. If false the BOM ID is set at this level if one does not exist</param>
        /// <param name="overrideWarehouse">Should we override the current bom ID at the Warehouse level. If false the BOM ID is set at this level if one does not exist</param>
        /// <param name="overrideSubItem">Should we override the current bom ID at the SubItem level. If false the BOM ID is set at this level if one does not exist. Only valid with the Subitem feature enabled</param>
        public void SetPrimaryOverride(AMBomItem bomItem, bool overrideItem, bool overrideWarehouse,
            bool overrideSubItem)
        {
            if (bomItem == null)
            {
                return;
            }

            SetPrimary(bomItem.BOMID, bomItem.InventoryID, bomItem.SiteID, bomItem.SubItemID, overrideItem,
                overrideWarehouse, overrideSubItem);
        }

        /// <summary>
        /// Root group call to set BOM IDs for the different BOM ID levels (Item, Warehouse, SubItem)
        /// </summary>
        /// <param name="bomItem">BOM record of the BOM ID to be set</param>
        /// <param name="inventoryID">BOM Inventory ID</param>
        /// <param name="siteID">BOM Site ID</param>
        /// <param name="subItemID">BOM Sub Item ID</param>
        /// <param name="overrideItem">Should we override the current bom ID at the Item level. If false the BOM ID is set at this level if one does not exist</param>
        /// <param name="overrideWarehouse">Should we override the current bom ID at the Warehouse level. If false the BOM ID is set at this level if one does not exist</param>
        /// <param name="overrideSubItem">Should we override the current bom ID at the SubItem level. If false the BOM ID is set at this level if one does not exist. Only valid with the Subitem feature enabled</param>
        private void SetPrimary(string bomID, int? inventoryID, int? siteID, int? subItemID, bool overrideItem,
            bool overrideWarehouse, bool overrideSubItem)
        {
            // SubItem
            if (overrideSubItem || !HasItemSiteSubItemPrimary(inventoryID, siteID, subItemID))
            {
                SetSubItemPrimary(bomID, inventoryID, siteID, subItemID, overrideSubItem);
            }

            // Item Warehouse
            if (overrideWarehouse || !HasItemSitePrimary(inventoryID, siteID))
            {
                SetItemSitePrimary(bomID, inventoryID, siteID);
            }

            //Item
            if (overrideItem || !HasItemPrimary(inventoryID))
            {
                SetItemPrimary(bomID, inventoryID);
            }
        }

        public void SetItemPrimary(AMBomItem bomItem)
        {
            if (bomItem == null)
            {
                return;
            }

            SetItemPrimary(bomItem.BOMID, GetInventoryItem(bomItem.InventoryID));
        }

        public void SetItemPrimary(string bomID, int? inventoryID)
        {
            SetItemPrimary(bomID, GetInventoryItem(inventoryID));
        }

        public void SetItemPrimary(string bomID, InventoryItem inventoryItem)
        {
            if (inventoryItem != null && !string.IsNullOrWhiteSpace(bomID))
            {
                InventoryItemExt itemExt = GetInventoryItemExt(inventoryItem);

                if (itemExt != null)
                {
                    if (BOMIDType == BomIDType.Default)
                    {
                        itemExt.AMBOMID = bomID;
                    }

                    if (BOMIDType == BomIDType.Planning)
                    {
                        itemExt.AMPlanningBOMID = bomID;
                    }

                    UpdateRow(inventoryItem, PersistInventoryItem);
                }
            }
        }

        public void SetItemSitePrimary(AMBomItem bomItem)
        {
            if (bomItem == null)
            {
                return;
            }

            SetItemSitePrimary(bomItem.BOMID, GetINItemSite(bomItem.InventoryID, bomItem.SiteID));
        }

        public void SetItemSitePrimary(string bomID, int? inventoryID, int? siteID)
        {
            SetItemSitePrimary(bomID, GetINItemSite(inventoryID, siteID));
        }

        public void SetItemSitePrimary(string bomID, INItemSite inItemSite)
        {
            if (inItemSite != null && !string.IsNullOrWhiteSpace(bomID))
            {
                INItemSiteExt itemExt = GetINItemSiteExt(inItemSite);

                if (itemExt != null)
                {
                    if (BOMIDType == BomIDType.Default)
                    {
                        itemExt.AMBOMID = bomID;
                    }

                    if (BOMIDType == BomIDType.Planning)
                    {
                        itemExt.AMPlanningBOMID = bomID;
                    }

                    UpdateRow(inItemSite, PersistINItemSite);
                }
            }
        }

        /// <summary>
        /// Maintains the AMSubItemDefault record
        /// </summary>
        /// <param name="bomID"></param>
        /// <param name="inventoryID"></param>
        /// <param name="siteID"></param>
        /// <param name="subItemID"></param>
        /// <param name="overrideItemDefault">Should the subitem be the new item default overriding any existing item/subitem default</param>
        public void SetSubItemPrimary(string bomID, int? inventoryID, int? siteID, int? subItemID,
            bool overrideItemDefault)
        {
            if (!_IsIDBySubItem
                || string.IsNullOrEmpty(bomID)
                || inventoryID == null
                || siteID == null
                || subItemID == null)
            {
                return;
            }

            if (overrideItemDefault)
            {
                //Remove current defaults if any first
                RemoveSubItemsAsDefault(inventoryID, subItemID);
            }

            AMSubItemDefault amSubItemDefault = PXSelectReadonly<AMSubItemDefault,
                Where<AMSubItemDefault.inventoryID, Equal<Required<AMSubItemDefault.inventoryID>>,
                    And<AMSubItemDefault.siteID, Equal<Required<AMSubItemDefault.siteID>>,
                        And<AMSubItemDefault.subItemID, Equal<Required<AMSubItemDefault.subItemID>>>>>
            >.Select(_Graph, inventoryID, siteID, subItemID);

            bool isInsert = false;
            if (amSubItemDefault == null)
            {
                amSubItemDefault = new AMSubItemDefault()
                {
                    InventoryID = inventoryID,
                    SiteID = siteID,
                    SubItemID = subItemID
                };
                isInsert = true;
            }

            // Then insert/update current as default
            amSubItemDefault.IsItemDefault = overrideItemDefault || !HasItemSubItemPrimary(inventoryID, subItemID);

            if (BOMIDType == BomIDType.Default)
            {
                amSubItemDefault.BOMID = bomID;
            }

            if (BOMIDType == BomIDType.Planning)
            {
                amSubItemDefault.PlanningBOMID = bomID;
            }

            if (isInsert)
            {
                InsertRow(amSubItemDefault, PersistAMSubItemDefault);
                return;
            }

            UpdateRow(amSubItemDefault, PersistAMSubItemDefault);
        }

        /// <summary>
        /// Remove item default from other records as only 1 row should have the default set per item/subitem combo
        /// </summary>
        /// <param name="amSubItemDefault">Current default row</param>
        private void RemoveSubItemsAsDefault(int? inventoryID, int? subItemID)
        {
            if (inventoryID == null || subItemID == null)
            {
                return;
            }

            foreach (AMSubItemDefault row in PXSelectReadonly<AMSubItemDefault,
                Where<AMSubItemDefault.inventoryID, Equal<Required<AMSubItemDefault.inventoryID>>,
                    And<AMSubItemDefault.subItemID, Equal<Required<AMSubItemDefault.subItemID>>,
                        And<AMSubItemDefault.isItemDefault, Equal<boolTrue>>>>
            >.Select(_Graph, inventoryID, subItemID))
            {
                row.IsItemDefault = false;
                UpdateRow(row, PersistAMSubItemDefault);
            }
        }

        #endregion

        #region REMOVE

        /// <summary>
        /// Remove the BOM ID for all levels
        /// </summary>
        public void RemovePrimary(AMBomItem bomItem)
        {
            RemoveItemPrimary(bomItem);
            RemoveItemSitePrimary(bomItem);
            RemoveSubItemPrimary(bomItem);
        }

        /// <summary>
        /// Remove the BOM ID at the item level
        /// </summary>
        public void RemoveItemPrimary(AMBomItem bomItem)
        {
            if (bomItem == null)
            {
                return;
            }

            RemoveItemPrimary(bomItem.BOMID, GetInventoryItem(bomItem.InventoryID), bomItem.SubItemID);
        }

        /// <summary>
        /// Remove the BOM ID the item level
        /// </summary>
        public void RemoveItemPrimary(string bomID, int? inventoryID, int? subItemID)
        {
            RemoveItemPrimary(bomID, GetInventoryItem(inventoryID), subItemID);
        }

        /// <summary>
        /// Remove the BOM ID at the item level
        /// </summary>
        public void RemoveItemPrimary(string bomID, InventoryItem inventoryItem, int? subItemID)
        {
            if (inventoryItem != null && !string.IsNullOrWhiteSpace(bomID))
            {
                InventoryItemExt itemExt = GetInventoryItemExt(inventoryItem);

                if (itemExt != null)
                {
                    bool update = false;
                    if (itemExt.AMBOMID.EqualsWithTrim(bomID))
                    {
                        itemExt.AMBOMID = null;
                        update = true;
                    }

                    if (itemExt.AMPlanningBOMID.EqualsWithTrim(bomID))
                    {
                        itemExt.AMPlanningBOMID = null;
                        update = true;
                    }

                    if (update)
                    {
                        UpdateRow(inventoryItem, PersistInventoryItem);
                    }

                }
            }
        }

        /// <summary>
        /// Remove the BOM ID at the item warehouse level
        /// </summary>
        public void RemoveItemSitePrimary(AMBomItem bomItem)
        {
            if (bomItem == null)
            {
                return;
            }

            RemoveItemSitePrimary(bomItem.BOMID, GetINItemSite(bomItem.InventoryID, bomItem.SiteID), bomItem.SubItemID);
        }

        /// <summary>
        /// Remove the BOM ID at the item warehouse level
        /// </summary>
        public void RemoveItemSitePrimary(string bomID, int? inventoryID, int? siteID, int? subItemID)
        {
            RemoveItemSitePrimary(bomID, GetINItemSite(inventoryID, siteID), subItemID);
        }

        /// <summary>
        /// Remove the BOM IDat the item warehouse level
        /// </summary>
        public void RemoveItemSitePrimary(string bomID, INItemSite inItemSite, int? subItemID)
        {
            if (inItemSite != null && !string.IsNullOrWhiteSpace(bomID))
            {
                INItemSiteExt itemExt = GetINItemSiteExt(inItemSite);

                if (itemExt != null)
                {
                    bool update = false;
                    if (itemExt.AMBOMID.EqualsWithTrim(bomID))
                    {
                        itemExt.AMBOMID = null;
                        update = true;
                    }

                    if (itemExt.AMPlanningBOMID.EqualsWithTrim(bomID))
                    {
                        itemExt.AMPlanningBOMID = null;
                        update = true;
                    }

                    if (update)
                    {
                        UpdateRow(inItemSite, PersistINItemSite);
                    }
                }
            }
        }


        public void RemoveSubItemPrimary(AMBomItem bomItem)
        {
            if (bomItem == null)
            {
                return;
            }

            RemoveSubItemPrimary(bomItem.BOMID, bomItem.InventoryID, bomItem.SiteID, bomItem.SubItemID);
        }

        public void RemoveSubItemPrimary(string bomID, int? inventoryID, int? siteID, int? subItemID)
        {
            if (!_IsIDBySubItem
                || string.IsNullOrEmpty(bomID)
                || inventoryID == null
                || siteID == null
                || subItemID == null)
            {
                return;
            }

            AMSubItemDefault amSubItemDefault = PXSelectReadonly<AMSubItemDefault,
                Where<AMSubItemDefault.inventoryID, Equal<Required<AMSubItemDefault.inventoryID>>,
                    And<AMSubItemDefault.siteID, Equal<Required<AMSubItemDefault.siteID>>,
                        And<AMSubItemDefault.subItemID, Equal<Required<AMSubItemDefault.subItemID>>>>>>.Select(_Graph,
                inventoryID, siteID, subItemID);

            if (amSubItemDefault == null)
            {
                return;
            }

            bool update = false;
            if (amSubItemDefault.BOMID.EqualsWithTrim(bomID))
            {
                amSubItemDefault.BOMID = null;
                amSubItemDefault.IsItemDefault = false;
                update = true;
            }

            if (amSubItemDefault.PlanningBOMID.EqualsWithTrim(bomID))
            {
                amSubItemDefault.PlanningBOMID = null;
                update = true;
            }

            if (update)
            {
                UpdateRow(amSubItemDefault, PersistAMSubItemDefault);
            }
        }

        #endregion

        public static AMBomItem GetActiveRevisionBomItem(PXGraph graph, string bomId)
        {
            if (string.IsNullOrWhiteSpace(bomId))
            {
                return null;
            }

            return PXSelectReadonly<
                    AMBomItem,
                    Where<AMBomItem.bOMID, Equal<Required<AMBomItem.bOMID>>,
                        And<AMBomItem.status, Equal<AMBomStatus.active>>>,
                    OrderBy<
                        Desc<AMBomItem.effStartDate,
                            Desc<AMBomItem.revisionID>>>>
                .SelectWindowed(graph, 0, 1, bomId);
        }

        public static AMBomItem GetActiveRevisionBomItemByDate(PXGraph graph, string bomId, DateTime? date)
        {
            if (string.IsNullOrWhiteSpace(bomId))
            {
                return null;
            }

            var bomDate = date ?? graph.Accessinfo.BusinessDate;

            return PXSelectReadonly<
                    AMBomItem,
                    Where<AMBomItem.bOMID, Equal<Required<AMBomItem.bOMID>>,
                        And<AMBomItem.status, Equal<AMBomStatus.active>,
                            And<Where<Required<AMBomItem.effStartDate>,
                                Between<AMBomItem.effStartDate, AMBomItem.effEndDate>,
                                Or<Where<AMBomItem.effStartDate, LessEqual<Required<AMBomItem.effStartDate>>,
                                    And<AMBomItem.effEndDate, IsNull>>>>>>>,
                    OrderBy<
                        Desc<AMBomItem.effStartDate,
                            Desc<AMBomItem.revisionID>>>>
                .SelectWindowed(graph, 0, 1, bomId, bomDate, bomDate);
        }

        public static AMBomItem GetNotArchivedRevisionBomItem(PXGraph graph, string bomId)
        {
            if (string.IsNullOrWhiteSpace(bomId))
            {
                return null;
            }

            return PXSelectReadonly<
                    AMBomItem,
                    Where<AMBomItem.bOMID, Equal<Required<AMBomItem.bOMID>>,
                        And<AMBomItem.status, In<Required<AMBomItem.status>>>>,
                    OrderBy<
                        Desc<AMBomItem.status,
                            Desc<AMBomItem.effStartDate,
                                Desc<AMBomItem.revisionID>>>>>
                .SelectWindowed(graph, 0, 1, bomId, new int?[]{ AMBomStatus.Hold, AMBomStatus.Active });
        }

        public static AMBomItem GetNotArchivedRevisionBomItemByDate(PXGraph graph, string bomId, DateTime? date)
        {
            if (string.IsNullOrWhiteSpace(bomId))
            {
                return null;
            }

            var bomDate = date ?? graph.Accessinfo.BusinessDate;

            return PXSelectReadonly<
                    AMBomItem,
                    Where<AMBomItem.bOMID, Equal<Required<AMBomItem.bOMID>>,
                        And2<Where<AMBomItem.status, Equal<AMBomStatus.hold>,
                                Or<AMBomItem.status, Equal<AMBomStatus.active>>>,
                            And<Where<Required<AMBomItem.effStartDate>,
                                Between<AMBomItem.effStartDate, AMBomItem.effEndDate>,
                                Or<Where<AMBomItem.effStartDate, LessEqual<Required<AMBomItem.effStartDate>>,
                                    And<AMBomItem.effEndDate, IsNull>>>>>>>,
                    OrderBy<
                        Desc<AMBomItem.status,
                            Desc<AMBomItem.effStartDate,
                                Desc<AMBomItem.revisionID>>>>>
                .SelectWindowed(graph, 0, 1, bomId, bomDate, bomDate);
        }
    }
}