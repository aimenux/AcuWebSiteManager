using PX.Data;
using PX.Objects.AM.CacheExtensions;
using PX.Objects.CS;
using PX.Objects.IN;

namespace PX.Objects.AM.GraphExtensions
{
    public class INItemSiteMaintAMExtension : PXGraphExtension<INItemSiteMaint>
    {
        public static bool IsActive()
        {
            return PXAccess.FeatureInstalled<CS.FeaturesSet.manufacturing>();
        }

        public PXSelect<AMSubItemDefault,
            Where<AMSubItemDefault.inventoryID, Equal<Current<INItemSite.inventoryID>>,
                And<AMSubItemDefault.siteID, Equal<Current<INItemSite.siteID>>>>,
            OrderBy<Asc<AMSubItemDefault.subItemID>>> AMSubItemDefaults;

        public override void Initialize()
        {
            base.Initialize();

            AMSubItemDefaults.AllowDelete = false;
            AMSubItemDefaults.AllowInsert = false;

            var subItemEnabled = PXAccess.FeatureInstalled<FeaturesSet.subItem>();
            // Controls the display of the grid on stock items
            AMSubItemDefaults.AllowSelect = subItemEnabled;

            PXUIFieldAttribute.SetVisible<INItemSiteExt.aMReplenishmentPolicyOverride>(Base.itemsitesettings.Cache, null, !FullReplenishmentsEnabled);
            PXUIFieldAttribute.SetVisible<INItemSiteExt.aMSafetyStockOverride>(Base.itemsitesettings.Cache, null, !FullReplenishmentsEnabled);
            PXUIFieldAttribute.SetVisible<INItemSiteExt.aMMinQtyOverride>(Base.itemsitesettings.Cache, null, !FullReplenishmentsEnabled && !BasicReplenishmentsEnabled);
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

        protected virtual void INItemSite_RowSelecting(PXCache cache, PXRowSelectingEventArgs e, PXRowSelecting del)
        {
            del?.Invoke(cache, e);

            var row = (INItemSite)e.Row;
            if (row == null)
            {
                return;
            }

            var extension = PXCache<INItemSite>.GetExtension<INItemSiteExt>(row);
            if (extension == null)
            {
                return;
            }

            string source = row.ReplenishmentSource ?? INReplenishmentSource.Purchased;
            
            // Make sure these unbound fields have a value
            extension.AMReplenishmentSource = source;
            extension.AMReplenishmentSourceSiteID = row.ReplenishmentSourceSiteID;
            extension.AMReplenishmentPolicyOverride = row.ReplenishmentPolicyOverride.GetValueOrDefault();
            extension.AMSafetyStock = row.SafetyStock.GetValueOrDefault();
            extension.AMSafetyStockOverride = row.SafetyStockOverride.GetValueOrDefault();
            extension.AMMinQty = row.MinQty.GetValueOrDefault();
            extension.AMMinQtyOverride = row.MinQtyOverride.GetValueOrDefault();
        }
        
        protected virtual void INItemSite_RowSelected(PXCache cache, PXRowSelectedEventArgs e, PXRowSelected del)
        {
            del?.Invoke(cache, e);

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

            PXUIFieldAttribute.SetEnabled<INItemSiteExt.aMReplenishmentSource>(Base.itemsitesettings.Cache, row, !FullReplenishmentsEnabled && extension.AMReplenishmentPolicyOverride.GetValueOrDefault());
            PXUIFieldAttribute.SetEnabled<INItemSiteExt.aMReplenishmentSourceSiteID>(Base.itemsitesettings.Cache, row, !FullReplenishmentsEnabled && extension.AMReplenishmentSource == INReplenishmentSource.Transfer);
            PXUIFieldAttribute.SetEnabled<INItemSiteExt.aMSafetyStock>(Base.itemsitesettings.Cache, row, !FullReplenishmentsEnabled && extension.AMSafetyStockOverride.GetValueOrDefault());
            PXUIFieldAttribute.SetEnabled<INItemSiteExt.aMMinQty>(Base.itemsitesettings.Cache, row, !FullReplenishmentsEnabled && !BasicReplenishmentsEnabled && extension.AMMinQtyOverride.GetValueOrDefault());
            PXUIFieldAttribute.SetEnabled<INItemSiteExt.aMMinOrdQty>(Base.itemsitesettings.Cache, row, extension.AMMinOrdQtyOverride.GetValueOrDefault());
            PXUIFieldAttribute.SetEnabled<INItemSiteExt.aMMaxOrdQty>(Base.itemsitesettings.Cache, row, extension.AMMaxOrdQtyOverride.GetValueOrDefault());
            PXUIFieldAttribute.SetEnabled<INItemSiteExt.aMLotSize>(Base.itemsitesettings.Cache, row, extension.AMLotSizeOverride.GetValueOrDefault());
            PXUIFieldAttribute.SetEnabled<INItemSiteExt.aMMFGLeadTime>(Base.itemsitesettings.Cache, row, extension.AMMFGLeadTimeOverride.GetValueOrDefault());
            PXUIFieldAttribute.SetEnabled<INItemSiteExt.aMGroupWindow>(Base.itemsitesettings.Cache, row, extension.AMGroupWindowOverride.GetValueOrDefault());
            PXUIFieldAttribute.SetEnabled<INItemSiteExt.aMScrapSiteID>(Base.itemsitesettings.Cache, row, extension.AMScrapOverride.GetValueOrDefault());
            PXUIFieldAttribute.SetEnabled<INItemSiteExt.aMScrapLocationID>(Base.itemsitesettings.Cache, row, extension.AMScrapOverride.GetValueOrDefault());
        }

        public virtual void INItemSite_RowUpdated(PXCache cache, PXRowUpdatedEventArgs e, PXRowUpdated del)
        {
            if (del != null)
            {
                del(cache, e);
            }

            var row = (INItemSite) e.Row;
            var oldRow = (INItemSite) e.OldRow;
            if (row == null)
            {
                return;
            }

            var extension = row.GetExtension<INItemSiteExt>();
            var oldRowExtension = oldRow.GetExtension<INItemSiteExt>();
            var itemExt = Base.itemrecord.Current.GetExtension<InventoryItemExt>();
            if (extension == null)
            {
                return;
            }

            // OVERRIDES REMOVED... RESET VALUE BACK FROM THE STOCK ITEM VALUE
            if (itemExt != null && oldRowExtension != null)
            {
                if (oldRowExtension.AMReplenishmentPolicyOverride.GetValueOrDefault() && !extension.AMReplenishmentPolicyOverride.GetValueOrDefault())
                {
                    cache.SetValue<INItemSiteExt.aMReplenishmentSource>(row, itemExt.AMReplenishmentSource ?? INReplenishmentSource.Purchased);
                }
                if (oldRowExtension.AMSafetyStockOverride.GetValueOrDefault() && !extension.AMSafetyStockOverride.GetValueOrDefault())
                {
                    cache.SetValue<INItemSiteExt.aMSafetyStock>(row, itemExt.AMSafetyStock.GetValueOrDefault());
                }
                if (oldRowExtension.AMMinQtyOverride.GetValueOrDefault() && !extension.AMMinQtyOverride.GetValueOrDefault())
                {
                    cache.SetValue<INItemSiteExt.aMMinQty>(row, itemExt.AMMinQty.GetValueOrDefault());
                }
                if (oldRowExtension.AMMinOrdQtyOverride.GetValueOrDefault() && !extension.AMMinOrdQtyOverride.GetValueOrDefault())
                {
                    cache.SetValue<INItemSiteExt.aMMinOrdQty>(row, itemExt.AMMinOrdQty.GetValueOrDefault());
                }
                if (oldRowExtension.AMMaxOrdQtyOverride.GetValueOrDefault() && !extension.AMMaxOrdQtyOverride.GetValueOrDefault())
                {
                    cache.SetValue<INItemSiteExt.aMMaxOrdQty>(row, itemExt.AMMaxOrdQty.GetValueOrDefault());
                }
                if (oldRowExtension.AMLotSizeOverride.GetValueOrDefault() && !extension.AMLotSizeOverride.GetValueOrDefault())
                {
                    cache.SetValue<INItemSiteExt.aMLotSize>(row, itemExt.AMLotSize.GetValueOrDefault());
                }
                if (oldRowExtension.AMMFGLeadTimeOverride.GetValueOrDefault() && !extension.AMMFGLeadTimeOverride.GetValueOrDefault())
                {
                    cache.SetValue<INItemSiteExt.aMMFGLeadTime>(row, itemExt.AMMFGLeadTime.GetValueOrDefault());
                }
                if (oldRowExtension.AMGroupWindowOverride.GetValueOrDefault() && !extension.AMGroupWindowOverride.GetValueOrDefault())
                {
                    cache.SetValue<INItemSiteExt.aMGroupWindow>(row, itemExt.AMGroupWindow.GetValueOrDefault());
                }
            }

            if (FullReplenishmentsEnabled)
            {
                cache.SetValue<INItemSiteExt.aMReplenishmentSource>(row, row.ReplenishmentSource ?? INReplenishmentSource.Purchased);
                cache.SetValue<INItemSiteExt.aMSafetyStock>(row, row.SafetyStock);
            }
            else
            {
                // Replenishment Source (and override)
                if ((oldRowExtension != null
                    && !extension.AMReplenishmentSource.EqualsWithTrim(oldRowExtension.AMReplenishmentSource))
                    || row.ReplenishmentPolicyOverride.GetValueOrDefault() != extension.AMReplenishmentPolicyOverride.GetValueOrDefault())
                {
                    cache.SetValue<INItemSite.replenishmentPolicyOverride>(row, extension.AMReplenishmentPolicyOverride.GetValueOrDefault());
                }
                if(row.ReplenishmentSource == null
                    || !row.ReplenishmentSource.EqualsWithTrim(extension.AMReplenishmentSource))
                {
                    cache.SetValue<INItemSite.replenishmentSource>(row, extension.AMReplenishmentSource ?? INReplenishmentSource.Purchased);
                }
                if (row.ReplenishmentSourceSiteID == null || !row.ReplenishmentSourceSiteID.Equals(extension.AMReplenishmentSourceSiteID))
                {
                    cache.SetValue<INItemSite.replenishmentSourceSiteID>(row, extension.AMReplenishmentSourceSiteID.GetValueOrDefault());
                }


                // Safety Stock (and override)
                if ((oldRowExtension != null
                    && extension.AMSafetyStock.GetValueOrDefault() != oldRowExtension.AMSafetyStock.GetValueOrDefault())
                    || row.SafetyStockOverride.GetValueOrDefault() != extension.AMSafetyStockOverride.GetValueOrDefault())
                {
                    cache.SetValue<INItemSite.safetyStockOverride>(row, extension.AMSafetyStockOverride.GetValueOrDefault());
                }
                if (row.SafetyStock.GetValueOrDefault() != extension.AMSafetyStock.GetValueOrDefault())
                {
                    cache.SetValue<INItemSite.safetyStock>(row, extension.AMSafetyStock);
                }
            }

            if (FullReplenishmentsEnabled || BasicReplenishmentsEnabled)
            {
                cache.SetValue<INItemSiteExt.aMMinQty>(row, row.MinQty);
                return;
            }

            // Reorder Point (and override)
            if ((oldRowExtension != null
                && extension.AMMinQty.GetValueOrDefault() != oldRowExtension.AMMinQty.GetValueOrDefault())
                || row.SafetyStockOverride.GetValueOrDefault() != extension.AMSafetyStockOverride.GetValueOrDefault())
            {
                cache.SetValue<INItemSite.minQtyOverride>(row, extension.AMMinQtyOverride.GetValueOrDefault());
            }
            if (row.MinQty.GetValueOrDefault() != extension.AMMinQty.GetValueOrDefault())
            {
                cache.SetValue<INItemSite.minQty>(row, extension.AMMinQty);
            }
        }

        protected virtual void INItemSite_AMScrapSiteID_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
        {
            var row = (INItemSite)e.Row;
            var rowExt = row.GetExtension<INItemSiteExt>();

            if (row == null)
            {
                return;
            }

            rowExt.AMScrapLocationID = null;
        }

        //Purpose of override is to copy InventoryItemExt values over to the new INItemSiteExt record as defaults similar to Acumatica's DefaultItemSiteByitem call
        public virtual void INItemSite_RowInserted(PXCache sender, PXRowInsertedEventArgs e, PXRowInserted del)
        {
            if (del != null)
            {
                del(sender, e);
            }

            INItemSite inItemSite = (INItemSite)e.Row;
            if (inItemSite == null || !inItemSite.InventoryID.HasValue || !inItemSite.SiteID.HasValue)
            {
                return;
            }

            InventoryItem currentInventoryItem = Base.itemrecord.Current;
            if (currentInventoryItem != null && Base.itemrecord.Cache.IsDirty)
            {
                AM.InventoryHelper.DefaultItemSiteManufacturing(Base, currentInventoryItem, inItemSite);
            }
        }

        // Added this cache attached because without it, the page times out when changing the Warehouse
        [PXDBInt]
        [PXUIField(DisplayName = "Scrap Location", FieldClass = "INLOCATION")]
        protected virtual void INSite_AMScrapLocationID_CacheAttached(PXCache sender)
        {
        }
    }
}