using System;
using System.Collections.Generic;
using System.Linq;
using PX.Data;
using PX.Objects.AM.CacheExtensions;
using PX.Objects.CS;
using PX.Objects.IN;

namespace PX.Objects.AM.GraphExtensions
{
    public class InventoryItemMaintAMExtension : PXGraphExtension<InventoryItemMaint>
    {
        public static bool IsActive()
        {
            return PXAccess.FeatureInstalled<CS.FeaturesSet.manufacturing>();
        }

        [PXCopyPasteHiddenView]
        public PXSelect<AMSubItemDefault, Where<AMSubItemDefault.inventoryID, Equal<Current<InventoryItem.inventoryID>>>, OrderBy<Asc<AMSubItemDefault.subItemID>>> AMSubItemDefaults;

        public override void Initialize()
        {
            base.Initialize();

            Base.inquiry.AddMenuAction(this.WhereUsedInq);

            AMSubItemDefaults.AllowDelete = false;
            AMSubItemDefaults.AllowInsert = false;

            var subItemEnabled = PXAccess.FeatureInstalled<FeaturesSet.subItem>();
            // Controls the display of the grid on stock items
            AMSubItemDefaults.AllowSelect = subItemEnabled;

            PXUIFieldAttribute.SetVisible<InventoryItemExt.aMReplenishmentSourceOverride>(Base.ItemSettings.Cache, null, FullReplenishmentsEnabled);
            PXUIFieldAttribute.SetVisible<InventoryItemExt.aMSafetyStockOverride>(Base.ItemSettings.Cache, null, FullReplenishmentsEnabled);
            PXUIFieldAttribute.SetVisible<InventoryItemExt.aMMinQtyOverride>(Base.ItemSettings.Cache, null, FullReplenishmentsEnabled || BasicReplenishmentsEnabled);

            PXUIFieldAttribute.SetVisible<INItemSiteExt.aMReplenishmentSource>(Base.itemsiterecords.Cache, null, !FullReplenishmentsEnabled);
        }

        [PXOverride]
        public void Persist(Action del)
        {
            var estimateInventoryCdUpdateRequired = !Base.Item.Cache.IsCurrentRowInserted() &&
                                                    EstimateGraphHelper
                                                        .InventoryCDUpdateRequired<InventoryItem.inventoryCD>(
                                                            Base.Item.Cache);

            del();

            if (!FullReplenishmentsEnabled)
            {
                //Issue with the grid rep source not refreshing correctly... force refresh of 'Warehouse Details' tab
                Base.itemsiterecords.Cache.Clear();
                Base.itemsiterecords.Cache.ClearQueryCache();
            }

            if (estimateInventoryCdUpdateRequired)
            {
                EstimateGraphHelper.UpdateEstimateInventoryCD(Base.Item.Current, Base);
            }
        }

        public PXAction<InventoryItem> WhereUsedInq;
        [PXButton]
        [PXUIField(DisplayName = "BOM Where Used", MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select, Visible = false)]
        public virtual System.Collections.IEnumerable whereUsedInq(PXAdapter adapter)
        {
            if (Base.Item.Current == null
                || Base.Item.Current.InventoryID.GetValueOrDefault() <= 0)
            {
                return adapter.Get();
            }

            var inqGraph = PXGraph.CreateInstance<BOMWhereUsedInq>();
            inqGraph.Filter.Current.InventoryID = Base.Item.Current.InventoryID;
            PXRedirectHelper.TryRedirect(inqGraph, PXRedirectHelper.WindowMode.NewWindow);

            return adapter.Get();
        }

        [PXString(1, IsFixed = true)]
        [PXUIField(DisplayName = "Replenishment Source")]
        [INReplenishmentSource.List]
        protected virtual void INItemSite_AMReplenishmentSource_CacheAttached(PXCache sender)
        {
        }

        [PXDBString(1, IsFixed = true)]
        [PXUIField(DisplayName = "Source")]
        [INReplenishmentSource.List]
        [PXFormula(typeof(Default<InventoryItem.itemClassID, InventoryItemExt.aMReplenishmentSourceOverride>))]
        protected virtual void InventoryItem_AMReplenishmentSource_CacheAttached(PXCache sender)
        {
        }

        /// <summary>
        /// MYOB - "Basic Inventory Replenishments" feature.
        /// Indicates is this feature is enabled/turned on
        /// </summary>
        protected virtual bool BasicReplenishmentsEnabled => OEMHelper.FeatureInstalled(OEMHelper.MYOBFeatures.BasicInvReplenish);

        /// <summary>
        /// Indicates if the full replenishment feature is enabled/turned on
        /// </summary>
        protected virtual bool FullReplenishmentsEnabled => PXAccess.FeatureInstalled<FeaturesSet.replenishment>();

        protected virtual void InventoryItem_AMReplenishmentSource_FieldDefaulting(PXCache sender, PXFieldDefaultingEventArgs e, PXFieldDefaulting del)
        {
            del?.Invoke(sender, e);

            var row = (InventoryItem)e.Row;
            if (row == null)
            {
                return;
            }

            var ext = row.GetExtension<InventoryItemExt>();
            if (ext == null)
            {
                return;
            }

            var itemClass = PXSelectorAttribute.Select<InventoryItem.itemClassID>(sender, row) as INItemClass;
            if (itemClass == null)
            {
                return;
            }

            if (FullReplenishmentsEnabled && ext.AMReplenishmentSourceOverride.GetValueOrDefault() == true)
            {
                e.NewValue = ext.AMReplenishmentSource;
                return;
            }
            
            if (FullReplenishmentsEnabled)
            {
                INItemRep itemRep = PXSelect<INItemRep,
                    Where<INItemRep.inventoryID, Equal<Required<INItemRep.inventoryID>>
                >>.SelectWindowed(Base, 0, 1, row.InventoryID.GetValueOrDefault());

                e.NewValue = itemRep == null ? INReplenishmentSource.Purchased : itemRep.ReplenishmentSource;

                return;
            }

            var itemClassExt = PXCache<INItemClass>.GetExtension<INItemClassExt>(itemClass);

            e.NewValue = itemClassExt == null ? INReplenishmentSource.Purchased : itemClassExt.AMReplenishmentSource;
        }

        protected virtual void InventoryItem_AMScrapSiteID_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
        {
            var row = (InventoryItem)e.Row;
            var rowExt = row.GetExtension<InventoryItemExt>();

            if (row == null)
            {
                return;
            }

            Base.Item.Cache.SetValueExt<InventoryItemExt.aMScrapLocationID>(Base.Item.Current, null);
        }

        public virtual void AMSubItemDefault_IsItemDefault_FieldUpdating(PXCache cache, PXFieldUpdatingEventArgs e)
        {
            if (!PXAccess.FeatureInstalled<FeaturesSet.subItem>())
            {
                return;
            }

            var newValue = (bool?)e.NewValue;

            var row = (AMSubItemDefault)e.Row;

            if (row?.InventoryID == null || row.SubItemID == null || !newValue.GetValueOrDefault())
            {
                return;
            }

            //if new value is true we need to check other subitem defaults to make sure there isn't a default already
            System.Collections.Generic.List<AMSubItemDefault> subItemDefaults = AMSubItemDefaults.Cache.Cached.Cast<AMSubItemDefault>().Where(
                cacheRow => cacheRow.InventoryID == row.InventoryID && cacheRow.SubItemID == row.SubItemID && cacheRow.IsItemDefault.GetValueOrDefault()
                ).ToList();

            if (subItemDefaults.Count > 0)
            {
                e.NewValue = false;
                e.Cancel = true;
                cache.RaiseExceptionHandling<AMSubItemDefault.isItemDefault>(
                    row,
                    row.IsItemDefault,
                    new PXSetPropertyException(AM.Messages.GetLocal(AM.Messages.OneDefaultSubItem), PXErrorLevel.Error)
                    );
            }

        }

        protected virtual void INItemSite_RowSelecting(PXCache cache, PXRowSelectingEventArgs e, PXRowSelecting del)
        {
            if (del != null)
            {
                del(cache, e);
            }

            var row = (INItemSite)e.Row;
            if (row == null)
            {
                return;
            }

            var extension = row.GetExtension<INItemSiteExt>();
            if (extension == null)
            {
                return;
            }

            // Make sure these unbound fields have a value (possibly used on Warehouse Details tab)
            extension.AMReplenishmentSource = row.ReplenishmentSource ?? INReplenishmentSource.Purchased;
            extension.AMReplenishmentSourceSiteID = row.ReplenishmentSourceSiteID;
            extension.AMReplenishmentPolicyOverride = row.ReplenishmentPolicyOverride.GetValueOrDefault();
            extension.AMSafetyStock = row.SafetyStock.GetValueOrDefault();
            extension.AMSafetyStockOverride = row.SafetyStockOverride.GetValueOrDefault();
            extension.AMMinQty = row.MinQty.GetValueOrDefault();
            extension.AMMinQtyOverride = row.MinQtyOverride.GetValueOrDefault();
        }

        protected virtual void InventoryItem_RowSelected(PXCache cache, PXRowSelectedEventArgs e, PXRowSelected del)
        {
            if (del != null)
            {
                del(cache, e);
            }

            var row = (InventoryItem)e.Row;
            if (row == null)
            {
                return;
            }

            var extension = PXCache<InventoryItem>.GetExtension<InventoryItemExt>(row);
            if (extension == null)
            {
                return;
            }

            bool isManufactured = extension.AMReplenishmentSource == INReplenishmentSource.Manufactured;

            if (FullReplenishmentsEnabled)
            {
                PXUIFieldAttribute.SetEnabled<InventoryItemExt.aMReplenishmentSource>(Base.ItemSettings.Cache, row, extension.AMReplenishmentSourceOverride.GetValueOrDefault());
                PXUIFieldAttribute.SetEnabled<InventoryItemExt.aMSafetyStock>(Base.ItemSettings.Cache, row, extension.AMSafetyStockOverride.GetValueOrDefault());
                PXUIFieldAttribute.SetEnabled<InventoryItemExt.aMMinQty>(Base.ItemSettings.Cache, row, extension.AMMinQtyOverride.GetValueOrDefault());
            }
            else if (BasicReplenishmentsEnabled)
            {
                PXUIFieldAttribute.SetEnabled<InventoryItemExt.aMMinQty>(Base.ItemSettings.Cache, row, extension.AMMinQtyOverride.GetValueOrDefault());
            }
            else
            {
                PXUIFieldAttribute.SetEnabled<InventoryItemExt.aMReplenishmentSource>(Base.ItemSettings.Cache, row, true);
                PXUIFieldAttribute.SetEnabled<InventoryItemExt.aMSafetyStock>(Base.ItemSettings.Cache, row, true);
                PXUIFieldAttribute.SetEnabled<InventoryItemExt.aMMinQty>(Base.ItemSettings.Cache, row, true);
            }

            PXUIFieldAttribute.SetEnabled<InventoryItemExt.aMMakeToOrderItem>(Base.ItemSettings.Cache, row, isManufactured);
        }

        private List<INItemSite> GetCurrentINItemSites()
        {
            var inItemSites = new List<INItemSite>();
            foreach (INItemSite result in Base.itemsiterecords.Select())
            {
                inItemSites.Add((INItemSite)Base.itemsiterecords.Cache.CreateCopy(result));
            }
            return inItemSites;
        }

        protected virtual void SyncReplenishmentSettings(List<INItemSite> inItemSitesToSync)
        {
            if (inItemSitesToSync == null
                || inItemSitesToSync.Count == 0)
            {
                return;
            }

            var itemExtension = Base.Item.Current.GetExtension<InventoryItemExt>();

            foreach (INItemSite inItemSite in inItemSitesToSync)
            {
                bool isUpdated = false;
                INItemSite inItemSiteUpdate = (INItemSite)Base.itemsiterecords.Cache.Locate(inItemSite);
                if (inItemSiteUpdate == null)
                {
                    inItemSiteUpdate = PXSelect<INItemSite,
                    Where<INItemSite.inventoryID, Equal<Required<INItemSite.inventoryID>>,
                        And<INItemSite.siteID, Equal<Required<INItemSite.siteID>>>>
                        >.SelectWindowed(Base, 0, 1, inItemSite.InventoryID, inItemSite.SiteID);
                }
                if (inItemSiteUpdate == null)
                {
                    continue;
                }
                var inItemSiteUpdateExtension = inItemSiteUpdate.GetExtension<INItemSiteExt>();

                // Replenishment Source (and override)
                if (!FullReplenishmentsEnabled
                    && inItemSite.ReplenishmentPolicyOverride.GetValueOrDefault() != inItemSiteUpdate.ReplenishmentPolicyOverride.GetValueOrDefault())
                {
                    inItemSiteUpdate.ReplenishmentPolicyOverride = inItemSite.ReplenishmentPolicyOverride.GetValueOrDefault();
                    isUpdated = true;
                }
                if (!FullReplenishmentsEnabled
                    && inItemSiteUpdate.ReplenishmentPolicyOverride.GetValueOrDefault())
                {
                    inItemSiteUpdate.ReplenishmentSource = inItemSite.ReplenishmentSource;
                    isUpdated = true;
                }
                if (itemExtension != null
                    && !inItemSiteUpdate.ReplenishmentPolicyOverride.GetValueOrDefault()
                    && (!FullReplenishmentsEnabled || string.IsNullOrWhiteSpace(inItemSiteUpdate.ReplenishmentClassID))
                    && !inItemSiteUpdate.ReplenishmentSource.EqualsWithTrim(itemExtension.AMReplenishmentSource))
                {
                    inItemSiteUpdate.ReplenishmentSource = itemExtension.AMReplenishmentSource ?? INReplenishmentSource.Purchased;
                    isUpdated = true;
                }

                // Safety Stock (and override)
                if (!FullReplenishmentsEnabled
                    && inItemSite.SafetyStockOverride.GetValueOrDefault() != inItemSiteUpdate.SafetyStockOverride.GetValueOrDefault())
                {
                    inItemSiteUpdate.SafetyStockOverride = inItemSite.SafetyStockOverride.GetValueOrDefault();
                    isUpdated = true;
                }
                if (!FullReplenishmentsEnabled
                    && inItemSiteUpdate.SafetyStockOverride.GetValueOrDefault())
                {
                    inItemSiteUpdate.SafetyStock = inItemSite.SafetyStock.GetValueOrDefault();
                    isUpdated = true;
                }
                if (itemExtension != null
                    && !inItemSiteUpdate.SafetyStockOverride.GetValueOrDefault()
                    && (!FullReplenishmentsEnabled || string.IsNullOrWhiteSpace(inItemSiteUpdate.ReplenishmentClassID))
                    && inItemSiteUpdate.SafetyStock.GetValueOrDefault() != itemExtension.AMSafetyStock.GetValueOrDefault())
                {
                    inItemSiteUpdate.SafetyStock = itemExtension.AMSafetyStock.GetValueOrDefault();
                    isUpdated = true;
                }

                // Reorder Point (and override)
                if (!FullReplenishmentsEnabled && !BasicReplenishmentsEnabled
                    && inItemSite.MinQtyOverride.GetValueOrDefault() != inItemSiteUpdate.MinQtyOverride.GetValueOrDefault())
                {
                    inItemSiteUpdate.MinQtyOverride = inItemSite.MinQtyOverride.GetValueOrDefault();
                    isUpdated = true;
                }
                if (!FullReplenishmentsEnabled && !BasicReplenishmentsEnabled
                    && inItemSiteUpdate.MinQtyOverride.GetValueOrDefault())
                {
                    inItemSiteUpdate.MinQty = inItemSite.MinQty.GetValueOrDefault();
                    isUpdated = true;
                }
                if (itemExtension != null
                    && !inItemSiteUpdate.MinQtyOverride.GetValueOrDefault()
                    && (!FullReplenishmentsEnabled || string.IsNullOrWhiteSpace(inItemSiteUpdate.ReplenishmentClassID))
                    && inItemSiteUpdate.MinQty.GetValueOrDefault() != itemExtension.AMMinQty.GetValueOrDefault())
                {
                    inItemSiteUpdate.MinQty = itemExtension.AMMinQty.GetValueOrDefault();
                    isUpdated = true;
                }

                // Min Order Qty
                if (inItemSiteUpdateExtension != null
                    && !inItemSiteUpdateExtension.AMMinOrdQtyOverride.GetValueOrDefault()
                    && inItemSiteUpdateExtension.AMMinOrdQty.GetValueOrDefault() != itemExtension.AMMinOrdQty.GetValueOrDefault())
                {
                    inItemSiteUpdateExtension.AMMinOrdQty = itemExtension.AMMinOrdQty.GetValueOrDefault();
                    isUpdated = true;
                }

                // Max Order Qty
                if (inItemSiteUpdateExtension != null
                    && !inItemSiteUpdateExtension.AMMaxOrdQtyOverride.GetValueOrDefault()
                    && inItemSiteUpdateExtension.AMMaxOrdQty.GetValueOrDefault() != itemExtension.AMMaxOrdQty.GetValueOrDefault())
                {
                    inItemSiteUpdateExtension.AMMaxOrdQty = itemExtension.AMMaxOrdQty.GetValueOrDefault();
                    isUpdated = true;
                }

                // Lot Size
                if (inItemSiteUpdateExtension != null
                    && !inItemSiteUpdateExtension.AMLotSizeOverride.GetValueOrDefault()
                    && inItemSiteUpdateExtension.AMLotSize.GetValueOrDefault() != itemExtension.AMLotSize.GetValueOrDefault())
                {
                    inItemSiteUpdateExtension.AMLotSize = itemExtension.AMLotSize.GetValueOrDefault();
                    isUpdated = true;
                }

                // MFG Lead Time
                if (inItemSiteUpdateExtension != null
                    && !inItemSiteUpdateExtension.AMMFGLeadTimeOverride.GetValueOrDefault()
                    && inItemSiteUpdateExtension.AMMFGLeadTime.GetValueOrDefault() != itemExtension.AMMFGLeadTime.GetValueOrDefault())
                {
                    inItemSiteUpdateExtension.AMMFGLeadTime = itemExtension.AMMFGLeadTime.GetValueOrDefault();
                    isUpdated = true;
                }

                // Group Planning
                if (inItemSiteUpdateExtension != null
                    && !inItemSiteUpdateExtension.AMGroupWindowOverride.GetValueOrDefault()
                    && inItemSiteUpdateExtension.AMGroupWindow.GetValueOrDefault() != itemExtension.AMGroupWindow.GetValueOrDefault())
                {
                    inItemSiteUpdateExtension.AMGroupWindow = itemExtension.AMGroupWindow.GetValueOrDefault();
                    isUpdated = true;
                }

                // Scrap Warehouse and Location
                if (inItemSiteUpdateExtension != null
                    && !inItemSiteUpdateExtension.AMScrapOverride.GetValueOrDefault()
                    && itemExtension.AMScrapSiteID != null
                    && itemExtension.AMScrapLocationID != null)
                {
                    inItemSiteUpdateExtension.AMScrapSiteID = itemExtension.AMScrapSiteID;
                    inItemSiteUpdateExtension.AMScrapLocationID = itemExtension.AMScrapLocationID;
                    isUpdated = true;
                }

                if (isUpdated && Base.itemsiterecords.Cache.GetStatus(inItemSiteUpdate) == PXEntryStatus.Notchanged)
                {
                    Base.itemsiterecords.Cache.SetStatus(inItemSiteUpdate, PXEntryStatus.Updated);
                }
            }
        }

        public virtual void InventoryItem_RowUpdated(PXCache cache, PXRowUpdatedEventArgs e, PXRowUpdated del)
        {
            var inItemSitesBefore = GetCurrentINItemSites();
            del?.Invoke(cache, e);
            SyncReplenishmentSettings(inItemSitesBefore);

            var row = (InventoryItem)e.Row;
            var oldRow = (InventoryItem)e.OldRow;
            if (row == null || oldRow == null)
            {
                return;
            }

            var rowExtension = row.GetExtension<InventoryItemExt>();
            var oldRowExtension = oldRow.GetExtension<InventoryItemExt>();
            if (rowExtension == null || oldRowExtension == null)
            {
                return;
            }

            INItemRep itemRep = null;
            // OVERRIDES REMOVED... RESET VALUE BACK FROM THE STOCK ITEM VALUE
            if (oldRowExtension.AMReplenishmentSourceOverride.GetValueOrDefault() && !rowExtension.AMReplenishmentSourceOverride.GetValueOrDefault())
            {
                if (itemRep == null)
                {
                    itemRep = GetINItemRep(rowExtension.AMReplenishmentSource);
                }
                if (itemRep == null)
                {
                    Base.Item.Cache.SetDefaultExt<InventoryItemExt.aMSafetyStock>(Base.Item.Current);
                }
                else
                {
                    Base.Item.Cache.SetValueExt<InventoryItemExt.aMSafetyStock>(Base.Item.Current, itemRep.SafetyStock.GetValueOrDefault());
                }
            }
            if (oldRowExtension.AMSafetyStockOverride.GetValueOrDefault() && !rowExtension.AMSafetyStockOverride.GetValueOrDefault())
            {
                if (itemRep == null)
                {
                    itemRep = GetINItemRep(rowExtension.AMReplenishmentSource);
                }
                if (itemRep == null)
                {
                    Base.Item.Cache.SetDefaultExt<InventoryItemExt.aMSafetyStock>(Base.Item.Current);
                }
                else
                {
                    Base.Item.Cache.SetValueExt<InventoryItemExt.aMSafetyStock>(Base.Item.Current, itemRep.SafetyStock.GetValueOrDefault());
                }
            }
            if (oldRowExtension.AMMinQtyOverride.GetValueOrDefault() && !rowExtension.AMMinQtyOverride.GetValueOrDefault())
            {
                if (itemRep == null)
                {
                    itemRep = GetINItemRep(rowExtension.AMReplenishmentSource);
                }
                if (itemRep == null)
                {
                    Base.Item.Cache.SetDefaultExt<InventoryItemExt.aMMinQty>(Base.Item.Current);
                }
                else
                {
                    Base.Item.Cache.SetValueExt<InventoryItemExt.aMMinQty>(Base.Item.Current, itemRep.MinQty.GetValueOrDefault());
                }
            }

            // auto set override fields...
            if (!FullReplenishmentsEnabled)
            {
                if (!rowExtension.AMReplenishmentSourceOverride.GetValueOrDefault()
                    && !rowExtension.AMReplenishmentSource.EqualsWithTrim(oldRowExtension.AMReplenishmentSource))
                {
                    cache.SetValue<InventoryItemExt.aMReplenishmentSourceOverride>(row, true);
                }
                if (!rowExtension.AMSafetyStockOverride.GetValueOrDefault()
                    && rowExtension.AMSafetyStock.GetValueOrDefault() != oldRowExtension.AMSafetyStock.GetValueOrDefault())
                {
                    cache.SetValue<InventoryItemExt.aMSafetyStockOverride>(row, true);
                }
                if (!BasicReplenishmentsEnabled
                    && !rowExtension.AMMinQtyOverride.GetValueOrDefault()
                    && rowExtension.AMMinQty.GetValueOrDefault() != oldRowExtension.AMMinQty.GetValueOrDefault())
                {
                    cache.SetValue<InventoryItemExt.aMMinQtyOverride>(row, true);
                }
            }
        }

        protected virtual void INItemRep_RowInserted(PXCache cache, PXRowInsertedEventArgs e, PXRowInserted del)
        {
            var inItemSitesBefore = GetCurrentINItemSites();
            if (del != null)
            {
                del(cache, e);
            }
            SyncStockItemReplenishmentSettings((INItemRep)e.Row, e);
            SyncReplenishmentSettings(inItemSitesBefore);
        }
        protected virtual void INItemRep_RowUpdated(PXCache cache, PXRowUpdatedEventArgs e, PXRowUpdated del)
        {
            var inItemSitesBefore = GetCurrentINItemSites();
            if (del != null)
            {
                del(cache, e);
            }
            SyncStockItemReplenishmentSettings((INItemRep)e.Row, e);
            SyncReplenishmentSettings(inItemSitesBefore);
        }
        protected virtual void INItemRep_RowDeleted(PXCache cache, PXRowDeletedEventArgs e, PXRowDeleted del)
        {
            var inItemSitesBefore = GetCurrentINItemSites();
            if (del != null)
            {
                del(cache, e);
            }
            SyncStockItemReplenishmentSettings((INItemRep)e.Row, e);
            SyncReplenishmentSettings(inItemSitesBefore);
        }

        protected virtual void SyncStockItemReplenishmentSettings(INItemRep inItemRep, EventArgs eventArgs)
        {
            if (inItemRep == null
                || string.IsNullOrWhiteSpace(inItemRep.ReplenishmentClassID))
            {
                return;
            }

            var itemExtension = Base.Item?.Current?.GetExtension<InventoryItemExt>();
            if (itemExtension == null)
            {
                return;
            }

            var inItemRepReferenced = inItemRep.ReplenishmentSource.EqualsWithTrim(itemExtension.AMReplenishmentSource) ? inItemRep : GetINItemRep(itemExtension.AMReplenishmentSource);

            if (inItemRepReferenced == null && eventArgs is PXRowInsertedEventArgs && !itemExtension.AMReplenishmentSourceOverride.GetValueOrDefault())
            {
                //If not override source and could not find a related rep we will reset if the itemclass was changed

                var origItemRow = Base.Item.Cache.GetOriginal(Base.Item.Current);
                if (Base.Item.Current != null && (origItemRow == null || !Base.Item.Cache.ObjectsEqual<InventoryItem.itemClassID>(Base.Item.Current, origItemRow)))
                {
                    inItemRepReferenced = GetFirstINItemRepNotMatchingSource(itemExtension.AMReplenishmentSource);
                }
            }

            // INSERTED and no other matching reps
            if (!inItemRep.ReplenishmentSource.EqualsWithTrim(itemExtension.AMReplenishmentSource)
                && eventArgs is PXRowInsertedEventArgs
                && (itemExtension.AMReplenishmentSource == null
                || inItemRepReferenced != null))
            {
                SetAMReplenishmentValues(inItemRepReferenced);
                return;
            }

            // UPDATED and source is a match
            if ((inItemRep.ReplenishmentSource.EqualsWithTrim(itemExtension.AMReplenishmentSource) || inItemRepReferenced == null)
                && eventArgs is PXRowUpdatedEventArgs)
            {
                SetAMReplenishmentValues(inItemRepReferenced ?? inItemRep);
                return;
            }

            // DELETED and the source needs to be reset (or pick another existing initemrep record)
            if (inItemRep.ReplenishmentSource.EqualsWithTrim(itemExtension.AMReplenishmentSource)
                && eventArgs is PXRowDeletedEventArgs)
            {
                var newINItemRepRef = itemExtension.AMReplenishmentSourceOverride.GetValueOrDefault()
                    ? null
                    : GetFirstINItemRepNotMatchingSource(inItemRep.ReplenishmentSource);

                SetAMReplenishmentValues(newINItemRepRef);
                return;
            }
        }

        protected virtual void SetAMReplenishmentValues(INItemRep inItemRep)
        {
            if (inItemRep == null
                || string.IsNullOrWhiteSpace(inItemRep.ReplenishmentClassID))
            {
                SetAMReplenishmentDefaultValues();
                return;
            }

            var itemExtension = Base.Item.Current.GetExtension<InventoryItemExt>();
            if (itemExtension == null)
            {
                return;
            }

            if (!itemExtension.AMReplenishmentSourceOverride.GetValueOrDefault())
            {
                if (string.IsNullOrWhiteSpace(inItemRep.ReplenishmentSource))
                {
                    Base.Item.Cache.SetDefaultExt<InventoryItemExt.aMReplenishmentSource>(Base.Item.Current);
                }
                else
                {
                    Base.Item.Cache.SetValueExt<InventoryItemExt.aMReplenishmentSource>(Base.Item.Current, inItemRep.ReplenishmentSource);
                }
            }

            if (!itemExtension.AMSafetyStockOverride.GetValueOrDefault())
            {
                Base.Item.Cache.SetValueExt<InventoryItemExt.aMSafetyStock>(Base.Item.Current, inItemRep.SafetyStock.GetValueOrDefault());
            }

            if (!itemExtension.AMMinQtyOverride.GetValueOrDefault())
            {
                Base.Item.Cache.SetValueExt<InventoryItemExt.aMMinQty>(Base.Item.Current, inItemRep.MinQty.GetValueOrDefault());
            }
        }

        /// <summary>
        /// Reset Manufacturing replenishment settings on stock items to default values
        /// </summary>
        protected virtual void SetAMReplenishmentDefaultValues()
        {
            var itemExtension = Base.Item.Current.GetExtension<InventoryItemExt>();
            if (itemExtension == null)
            {
                return;
            }

            if (!itemExtension.AMReplenishmentSourceOverride.GetValueOrDefault())
            {
                Base.Item.Cache.SetDefaultExt<InventoryItemExt.aMReplenishmentSource>(Base.Item.Current);
            }

            if (!itemExtension.AMSafetyStockOverride.GetValueOrDefault())
            {
                Base.Item.Cache.SetDefaultExt<InventoryItemExt.aMSafetyStock>(Base.Item.Current);
            }

            if (!itemExtension.AMMinQtyOverride.GetValueOrDefault())
            {
                Base.Item.Cache.SetDefaultExt<InventoryItemExt.aMMinQty>(Base.Item.Current);
            }
        }

        protected virtual INItemRep GetINItemRep(string replenishmentSource)
        {
            if (string.IsNullOrWhiteSpace(replenishmentSource))
            {
                return null;
            }

            //Do not convert to linq - will receive invalid cast in some instances
            foreach (INItemRep inItemRep in Base.replenishment.Select())
            {
                if (inItemRep.ReplenishmentSource.EqualsWithTrim(replenishmentSource))
                {
                    return inItemRep;
                }
            }
            return null;
        }

        protected virtual INItemRep GetFirstINItemRepNotMatchingSource(string replenishmentSource)
        {
            if (string.IsNullOrWhiteSpace(replenishmentSource))
            {
                return null;
            }

            //Do not convert to linq - will receive invalid cast in some instances
            foreach (INItemRep inItemRep in Base.replenishment.Select())
            {
                if (!inItemRep.ReplenishmentSource.EqualsWithTrim(replenishmentSource))
                {
                    return inItemRep;
                }
            }
            return null;
        }

        protected virtual INItemRep GetBestINItemRep(string replenishmentSource)
        {
            if (string.IsNullOrWhiteSpace(replenishmentSource))
            {
                return null;
            }

            INItemRep firstRep = null;
            INItemRep matchingSource = null;

            //Do not convert to linq - will receive invalid cast in some instances
            foreach (INItemRep inItemRep in Base.replenishment.Select())
            {
                if (inItemRep.ReplenishmentSource.EqualsWithTrim(replenishmentSource)
                    && matchingSource == null)
                {
                    matchingSource = inItemRep;
                }

                if (!inItemRep.ReplenishmentSource.EqualsWithTrim(replenishmentSource)
                    && firstRep == null)
                {
                    firstRep = inItemRep;
                }

                if (firstRep != null && matchingSource != null)
                {
                    break;
                }
            }
            return matchingSource ?? firstRep;
        }

        protected virtual void InventoryItem_AMReplenishmentSource_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
        {
            var row = (InventoryItem)e.Row;
            var rowExt = row.GetExtension<InventoryItemExt>();

            if (row == null)
            {
                return;
            }

            bool isManufactured = rowExt.AMReplenishmentSource == INReplenishmentSource.Manufactured;

            if (!isManufactured)
            {
                Base.Item.Cache.SetValueExt<InventoryItemExt.aMMakeToOrderItem>(Base.Item.Current, false);
            }
        }

        protected virtual void InventoryItem_PostClassID_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e, PXFieldUpdated del)
        {
            del?.Invoke(sender, e);
            sender.SetDefaultExt<InventoryItemExt.aMWIPVarianceAcctID>(e.Row);
            sender.SetDefaultExt<InventoryItemExt.aMWIPVarianceSubID>(e.Row);
            sender.SetDefaultExt<InventoryItemExt.aMWIPAcctID>(e.Row);
            sender.SetDefaultExt<InventoryItemExt.aMWIPSubID>(e.Row);
        }
    }
}
