using System;
using PX.Objects.AM.GraphExtensions;
using PX.Data;
using PX.Objects.AM.CacheExtensions;
using PX.Objects.CS;
using PX.Objects.IN;

namespace PX.Objects.AM
{
    public abstract class IDManagerBase
    {
        protected PXGraph _Graph;
        protected readonly bool _IsIDBySubItem;

        public bool PersistInventoryItem;
        public bool PersistINItemSite;
        public bool PersistAMSubItemDefault;

        public IDManagerBase(PXGraph graph)
        {
            _Graph = graph ?? throw new ArgumentNullException(nameof(graph));
            _IsIDBySubItem = PXAccess.FeatureInstalled<FeaturesSet.subItem>();
        }

        public bool PersistChanges
        {
            set
            {
                PersistInventoryItem = value;
                PersistINItemSite = value;
                PersistAMSubItemDefault = value;
            }
        }

        protected InventoryItem GetInventoryItem(int? inventoryID)
        {
            if (inventoryID != null)
            {
                InventoryItem item = PXSelect<InventoryItem,
                    Where<InventoryItem.inventoryID, Equal<Required<InventoryItem.inventoryID>>>>.Select(_Graph,
                    inventoryID);

                return item;
            }

            return null;
        }

        protected InventoryItemExt GetInventoryItemExt(int? inventoryID)
        {
            return GetInventoryItemExt(GetInventoryItem(inventoryID));
        }

        protected InventoryItemExt GetInventoryItemExt(InventoryItem inventoryItem)
        {
            if (inventoryItem != null)
            {
                InventoryItemExt itemExt =
                    PXCache<InventoryItem>.GetExtension<InventoryItemExt>(inventoryItem);
                return itemExt;
            }

            return null;
        }

        protected INItemSite GetINItemSite(int? inventoryID, int? siteID)
        {
            if (inventoryID != null && siteID != null)
            {
                INItemSite inItemSite = PXSelect<INItemSite,
                    Where<INItemSite.inventoryID, Equal<Required<INItemSite.inventoryID>>,
                        And<INItemSite.siteID, Equal<Required<INItemSite.siteID>>>>>.Select(_Graph, inventoryID, siteID);

                return inItemSite;
            }

            return null;
        }

        protected INItemSiteExt GetINItemSiteExt(int? inventoryID, int? siteID)
        {
            return GetINItemSiteExt(GetINItemSite(inventoryID, siteID));
        }

        protected INItemSiteExt GetINItemSiteExt(INItemSite inItemSite)
        {
            if (inItemSite != null)
            {
                INItemSiteExt itemSiteExt = PXCache<INItemSite>.GetExtension<INItemSiteExt>(inItemSite);
                return itemSiteExt;
            }

            return null;
        }

        protected void AddSupportingINSetupCache()
        {
            var missingSetup = false;
            try
            {
                Common.Cache.AddCacheView<INSetup>(_Graph);

                // The graph updating the InventoryItemExt needs INSetup -this will make sure its there
                // Any extension updates to inventory tables with UOM must have INSetup per Acumatica case 031594
                // (Hidden requirement)

                //Make sure the current for setup is always set...
                if (_Graph.Caches[typeof(INSetup)].Current == null)
                {
                    _Graph.Caches[typeof(INSetup)].Current = (INSetup)PXSelect<INSetup>.Select(_Graph);
                }

                if (_Graph.Caches[typeof(INSetup)].Current == null)
                {
                    missingSetup = true;
                    throw new PXException(Messages.INSetupNotFound);
                }

                // Starting sometime in 2017R2/6.1 update we need CommonSetup. It may or may no replace INSetup but it works
                Common.Cache.AddCacheView<CommonSetup>(_Graph);
                if (_Graph.Caches[typeof(CommonSetup)].Current == null)
                {
                    _Graph.Caches[typeof(CommonSetup)].Current = (CommonSetup)PXSelect<CommonSetup>.Select(_Graph);
                }
            }
            catch (Exception ex)
            {
                PXTrace.WriteWarning("update ID process: Unable to add supporting cache {0} with error: {1}", typeof(INSetup).Name, ex.Message);

                if (missingSetup)
                {
                    throw;
                }
            }
        }

        protected void AddSupportingCache<T>() where T : class, IBqlTable, new()
        {
            try
            {
                Common.Cache.AddCacheView<T>(_Graph);
            }
            catch (Exception ex)
            {
                PXTrace.WriteWarning("Default ID process: Unable to add supporting cache {0} with error: {1}", typeof(T).Name, ex.Message);

                if (ex is PXSetupNotEnteredException)
                {
                    throw;
                }
            }
        }

        protected dac InsertRow<dac>(dac row) where dac : class, IBqlTable, new()
        {
            return InsertRow<dac>(row, true);
        }

        protected dac InsertRow<dac>(dac row, bool persist) where dac : class, IBqlTable, new()
        {
            var row2 = row;
            if (row == null)
            {
                return row2;
            }

            AddSupportingINSetupCache();

            try
            {
                if (!persist)
                {
                    AddSupportingCache<dac>();
                }

                row2 = (dac)_Graph.Caches[typeof(dac)].Insert(row);

                if (persist)
                {
                    _Graph.Caches[typeof(dac)].Persist(PXDBOperation.Insert);
                }
                else
                {
                    _Graph.Caches[typeof(dac)].SetStatus(row2, PXEntryStatus.Inserted);
                }
            }
            catch
            {
                PXTrace.WriteInformation(Messages.GetLocal(Messages.UnableToInsertDAC, typeof(dac).Name));
                throw;
            }

            return row2;
        }

        protected dac UpdateRow<dac>(dac row) where dac : class, IBqlTable, new()
        {
            return UpdateRow<dac>(row, true);
        }

        protected dac UpdateRow<dac>(dac row, bool persist) where dac : class, IBqlTable, new()
        {
            var row2 = row;
            if (row == null)
            {
                return row2;
            }

            AddSupportingINSetupCache();

            try
            {
                if (persist)
                {
                    _Graph.Caches[typeof(dac)].Persist(row, PXDBOperation.Update);
                    row2 = FindRow(row) ?? row;
                }
                else
                {
                    AddSupportingCache<dac>();
                    row2 = (dac)_Graph.Caches[typeof(dac)].Update(row);
                }
            }
            catch
            {
                PXTrace.WriteInformation(Messages.GetLocal(Messages.UnableToUpdateDAC, typeof(dac).Name));
                throw;
            }

            return row2;
        }

        /// <summary>
        /// Found the given row in the cache
        /// </summary>
        protected dac FindRow<dac>(dac row) where dac : class, IBqlTable, new()
        {
            return (dac)_Graph.Caches[typeof(dac)].Locate(row);
        }
    }
}