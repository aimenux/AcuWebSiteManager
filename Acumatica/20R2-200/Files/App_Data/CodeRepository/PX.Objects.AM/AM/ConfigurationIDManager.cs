using System;
using PX.Data;
using PX.Objects.AM.CacheExtensions;

namespace PX.Objects.AM
{
    /// <summary>
    /// Manage the use of configuration IDs for given inventory ID and/or warehouse ID
    /// </summary>
    public class ConfigurationIDManager : IDManagerBase
    {
        protected enum SetMode
        {
            FirstSet,
            Replace,
            Remove
        }

        public ConfigurationIDManager(PXGraph graph) : base(graph)
        {
            graph.FieldVerifying.AddHandler<InventoryItemExt.aMConfigurationID>(InventoryItem_AMConfigurationID_FieldVerifying);
            graph.FieldVerifying.AddHandler<INItemSiteExt.aMConfigurationID>(INItemSite_AMConfigurationID_FieldVerifying);
        }

        protected virtual void InventoryItem_AMConfigurationID_FieldVerifying(PXCache sender, PXFieldVerifyingEventArgs e)
        {
            e.Cancel = true;
        }

        protected virtual void INItemSite_AMConfigurationID_FieldVerifying(PXCache sender, PXFieldVerifyingEventArgs e)
        {
            e.Cancel = true;
        }

        public virtual string GetID(int? inventoryID)
        {
            return GetID(inventoryID, null);
        }

        public virtual string GetID(AMConfiguration configuration)
        {
            if (configuration == null)
            {
                throw new PXArgumentException(nameof(configuration));
            }

            return GetID(configuration.InventoryID, GetAMBomItem(configuration)?.SiteID );
        }

        public virtual string GetID(int? inventoryID, int? siteID)
        {
            if (siteID.GetValueOrDefault() == 0)
            {
                return GetIDByInventoryItem(inventoryID);
            }

            return GetIDByWarehouse(inventoryID, siteID) ?? GetIDByInventoryItem(inventoryID);
        }

        public static string GetID(PXGraph graph, int? inventoryID, int? siteID)
        {
            return new ConfigurationIDManager(graph).GetID(inventoryID, siteID);
        }

        protected virtual string GetIDByInventoryItem(int? inventoryID)
        {
            if (inventoryID.GetValueOrDefault() == 0)
            {
                throw new PXArgumentException(nameof(inventoryID));
            }

            return GetInventoryItemExt(inventoryID)?.AMConfigurationID;
        }

        public virtual string GetIDByWarehouse(int? inventoryID, int? siteID)
        {
            if (inventoryID.GetValueOrDefault() == 0)
            {
                throw new PXArgumentException(nameof(inventoryID));
            }

            if (siteID.GetValueOrDefault() == 0)
            {
                throw new PXArgumentException(nameof(siteID));
            }

            return GetINItemSiteExt(inventoryID, siteID)?.AMConfigurationID;
        }

        public virtual bool RemoveID(AMConfiguration configuration)
        {
            if (configuration == null)
            {
                throw new PXArgumentException(nameof(configuration));
            }

            return RemoveID(configuration.InventoryID, GetAMBomItem(configuration)?.SiteID, configuration.ConfigurationID);
        }

        public virtual bool RemoveID(int? inventoryID, int? siteID, string configuraitonID)
        {
            if (inventoryID.GetValueOrDefault() == 0)
            {
                throw new PXArgumentException(nameof(inventoryID));
            }

            return SetID(inventoryID, siteID, configuraitonID, SetMode.Remove);
        }

        public virtual bool UpdateID(AMConfiguration configuration, bool updateInventoryItem, bool updateItemSite)
        {
            if (configuration == null)
            {
                throw new PXArgumentException(nameof(configuration));
            }

            return UpdateID(configuration.InventoryID, GetAMBomItem(configuration)?.SiteID, configuration.ConfigurationID, updateInventoryItem, updateItemSite);
        }

        public virtual bool UpdateID(int? inventoryID, int? siteID, string configuraitonID, bool updateInventoryItem, bool updateItemSite)
        {
            if (inventoryID.GetValueOrDefault() == 0)
            {
                throw new PXArgumentException(nameof(inventoryID));
            }

            var isSet = false;

            if (updateInventoryItem)
            {
                isSet |= SetIDByInventoryItem(inventoryID, configuraitonID, SetMode.Replace);
            }

            if (updateItemSite && siteID.GetValueOrDefault() != 0)
            {
                isSet |= SetIDByWarehouse(inventoryID, siteID, configuraitonID, SetMode.Replace);
            }

            return isSet;
        }

        public virtual bool SetID(int? inventoryID, string configuraitonID)
        {
            return SetID(inventoryID, null, configuraitonID);
        }

        public virtual bool SetID(AMConfiguration configuration)
        {
            if (configuration == null)
            {
                throw new PXArgumentException(nameof(configuration));
            }

            return SetID(configuration.InventoryID, GetAMBomItem(configuration)?.SiteID, configuration.ConfigurationID);
        }

        public virtual bool SetID(int? inventoryID, int? siteID, string configuraitonID)
        {
            return SetID(inventoryID, siteID, configuraitonID, SetMode.FirstSet);
        }

        protected virtual bool SetID(int? inventoryID, int? siteID, string configuraitonID, SetMode setMode)
        {
            var isSet = SetIDByInventoryItem(inventoryID, configuraitonID, setMode);

            if (siteID.GetValueOrDefault() != 0)
            {
                isSet |= SetIDByWarehouse(inventoryID, siteID, configuraitonID, setMode);
            }

            return isSet;
        }

        public virtual AMBomItem GetAMBomItem(AMConfiguration configuration)
        {
            if (configuration?.BOMID == null || configuration.BOMRevisionID == null)
            {
                return null;
            }

            return PXSelect<
                AMBomItem,
                Where<AMBomItem.bOMID, Equal<Required<AMBomItem.bOMID>>,
                    And<AMBomItem.revisionID, Equal<Required<AMBomItem.revisionID>>
                    >>>
                .Select(_Graph, configuration.BOMID, configuration.BOMRevisionID);
        }

        protected virtual bool SetIDByInventoryItem(int? inventoryID, string configuraitonID, SetMode setMode)
        {
            if (inventoryID.GetValueOrDefault() == 0)
            {
                throw new PXArgumentException(nameof(inventoryID));
            }

            if (string.IsNullOrWhiteSpace(configuraitonID))
            {
                return false;
            }

            var inventoryItem = GetInventoryItem(inventoryID);
            var extension = GetInventoryItemExt(inventoryItem);
            if (extension == null || inventoryItem == null)
            {
                return false;
            }

            switch (setMode)
            {
                case SetMode.FirstSet:
                    if (!string.IsNullOrWhiteSpace(extension.AMConfigurationID))
                    {
                        return false;
                    }
                    extension.AMConfigurationID = configuraitonID;
                    break;
                case SetMode.Replace:
                    extension.AMConfigurationID = configuraitonID;
                    break;
                case SetMode.Remove:
                    if (!configuraitonID.EqualsWithTrim(extension.AMConfigurationID))
                    {
                        return false;
                    }
                    extension.AMConfigurationID = null;
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(setMode), setMode, "Unknown Configuration ID Update Mode");
            }

            UpdateRow(inventoryItem, PersistInventoryItem);

            return true;
        }

        public virtual bool SetIDByWarehouse(int? inventoryID, int? siteID, string configuraitonID)
        {
            return SetIDByWarehouse(inventoryID, siteID, configuraitonID, SetMode.FirstSet);
        }

        protected virtual bool SetIDByWarehouse(int? inventoryID, int? siteID, string configuraitonID, SetMode setMode)
        {
            if (inventoryID.GetValueOrDefault() == 0)
            {
                throw new PXArgumentException(nameof(inventoryID));
            }

            if (siteID.GetValueOrDefault() == 0)
            {
                throw new PXArgumentException(nameof(siteID));
            }

            if (string.IsNullOrWhiteSpace(configuraitonID))
            {
                return false;
            }

            var itemSite = GetINItemSite(inventoryID, siteID);
            var extension = GetINItemSiteExt(itemSite);
            if (extension == null || itemSite == null)
            {
                return false;
            }

            switch (setMode)
            {
                case SetMode.FirstSet:
                    if (!string.IsNullOrWhiteSpace(extension.AMConfigurationID))
                    {
                        return false;
                    }
                    extension.AMConfigurationID = configuraitonID;
                    break;
                case SetMode.Replace:
                    extension.AMConfigurationID = configuraitonID;
                    break;
                case SetMode.Remove:
                    if (!configuraitonID.EqualsWithTrim(extension.AMConfigurationID))
                    {
                        return false;
                    }
                    extension.AMConfigurationID = null;
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(setMode), setMode, "Unknown Configuration ID Update Mode");
            }

            UpdateRow(itemSite, PersistINItemSite);

            return true;
        }
    }
}