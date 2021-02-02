using System;
using System.Collections.Generic;
using System.Linq;

namespace PX.Objects.AM
{
    /// <summary>
    /// Tracks excess supply during MRP Regen process
    /// </summary>
    public sealed class DetailSupply
    {
        /// <summary>
        /// Dictionary taking first key InventoryID, second dictionary key siteid
        /// </summary>
        private Dictionary<int, List<Supply>> _supplyDictionary;

        public DetailSupply()
        {
            //_supplyDictionary = new System.Collections.Generic.Dictionary<int, SiteSupplyDictionary>();
            _supplyDictionary = new Dictionary<int, List<Supply>>();
        }

        /// <summary>
        /// Add excess supply
        /// </summary>
        /// <param name="inventoryId"></param>
        /// <param name="siteId"></param>
        /// <param name="supplyDate"></param>
        /// <param name="supplyQuantity"></param>
        public void AddSupply(int? inventoryId, int? siteId, int? subItemId, DateTime? supplyDate, decimal? supplyQuantity)
        {
            //all values required
            if ( inventoryId.GetValueOrDefault() == 0
                || siteId.GetValueOrDefault() == 0
                || supplyDate == null
                || supplyQuantity.GetValueOrDefault() == 0)
            {
                return;
            }

            var siteSupplyList = new List<Supply>();

            bool key1Exists = _supplyDictionary.TryGetValue(inventoryId.GetValueOrDefault(), out siteSupplyList);

            decimal currentSupplyQuantity = 0;

            if (siteSupplyList == null)
            {
                siteSupplyList = new List<Supply>();
            }

            if (key1Exists)
            {
                //make sure we only have a single item/site/date combo
                var currentSupply = siteSupplyList.SingleOrDefault(x => x.SiteID == siteId.GetValueOrDefault() && x.SubItemID == subItemId.GetValueOrDefault() && DateTime.Compare(x.SupplyDate, (DateTime)supplyDate) == 0);
                if (currentSupply != null)
                {
                    currentSupplyQuantity = currentSupply.Quantity;
                    siteSupplyList.Remove(currentSupply);
                }
            }

            var newSupply = new Supply()
            {
                InventoryID = inventoryId.GetValueOrDefault(),
                SiteID = siteId.GetValueOrDefault(),
                SubItemID = subItemId.GetValueOrDefault(),
                SupplyDate = supplyDate.GetValueOrDefault(),
                Quantity = (supplyQuantity.GetValueOrDefault() + currentSupplyQuantity)
            };

            siteSupplyList.Add(newSupply);

            //sort the list by date (performance impact?)
            siteSupplyList.Sort(SupplySortBy.Sort());

            if (key1Exists)
            {
                _supplyDictionary[inventoryId.GetValueOrDefault()] = siteSupplyList;
                return;
            }

            _supplyDictionary.Add(inventoryId.GetValueOrDefault(), siteSupplyList);

        }

        /// <summary>
        /// Update supply per item
        /// </summary>
        /// <param name="inventoryID">InventoryID receiving the update</param>
        /// <param name="supplyUpdates">Supply list needing updated</param>
        private void UpdateSupply(int inventoryID, List<Supply> supplyUpdates)
        {
            if (supplyUpdates == null || !supplyUpdates.Any())
            {
                return;
            }

            List<Supply> siteSupplyList = null;
            if (!_supplyDictionary.TryGetValue(inventoryID, out siteSupplyList))
            {
                return;
            }

            if (siteSupplyList == null || !siteSupplyList.Any())
            {
                return;
            }

            foreach (var supplyUpdate in supplyUpdates)
            {
                var currentSupply = siteSupplyList.SingleOrDefault(x => x.SiteID == supplyUpdate.SiteID && x.SubItemID == supplyUpdate.SubItemID && DateTime.Compare(x.SupplyDate, supplyUpdate.SupplyDate) == 0);
                if (currentSupply != null)
                {
                    siteSupplyList.Remove(currentSupply);
                    
                    if(supplyUpdate.Quantity >= 0)
                    {
                        siteSupplyList.Add(supplyUpdate);
                    }
                }
            }

            siteSupplyList.Sort(SupplySortBy.Sort());
            _supplyDictionary[inventoryID] = siteSupplyList;
        }

        /// <summary>
        /// Get cached supply and consume the supplied quantity if found
        /// </summary>
        /// <param name="inventoryId">KEY: InventoryID</param>
        /// <param name="siteId">KEY: SiteID</param>
        /// <param name="subItemId">KEY: Sub Item ID</param>
        /// <param name="gracePeriodDateTime">Grace period date to check supply against the item/site</param>
        /// <param name="orderQuantity">Original order quantity</param>
        /// <returns>Adjusted order quantity accounting for excess supply</returns>
        public decimal GetSupply(int? inventoryId, int? siteId, int? subItemId, System.DateTime? gracePeriodDateTime, decimal? orderQuantity)
        {
            //all values required
            if (inventoryId.GetValueOrDefault() == 0
                || siteId.GetValueOrDefault() == 0
                || gracePeriodDateTime == null
                || orderQuantity.GetValueOrDefault() == 0)
            {
                return orderQuantity ?? 0;
            }

            var siteSupplyList = new List<Supply>();

            bool keyExists = _supplyDictionary.TryGetValue(inventoryId.GetValueOrDefault(), out siteSupplyList);

            decimal adjustedOrderQuantity = orderQuantity.GetValueOrDefault();

            if (keyExists)
            {
                var updateSupplyList = new List<Supply>();

                foreach (var supply in siteSupplyList.Where(supply => supply.SiteID == siteId.GetValueOrDefault() && supply.SubItemID == subItemId.GetValueOrDefault() && supply.SupplyDate <= gracePeriodDateTime.GetValueOrDefault(DateTime.Today.Date)))
                {
                    decimal adjustQty = (adjustedOrderQuantity < supply.Quantity) ? adjustedOrderQuantity : supply.Quantity;

                    adjustedOrderQuantity -= adjustQty;
                    supply.Quantity -= adjustQty;

                    updateSupplyList.Add(supply);

                    if (adjustedOrderQuantity <= 0)
                    {
                        break;
                    }
                }

                UpdateSupply(inventoryId.GetValueOrDefault(), updateSupplyList);
            }

            return adjustedOrderQuantity;
        }

        private class SupplySortBy : System.Collections.Generic.IComparer<Supply>
        {
            public int Compare(Supply x, Supply y)
            {
                int compareItem = x.InventoryID.CompareTo(y.InventoryID);

                if (compareItem == 0)
                {
                    int compareSite = x.SiteID.CompareTo(y.SiteID);

                    if (compareSite == 0)
                    {
                        int compareSubItem = x.SubItemID.CompareTo(y.SubItemID);

                        if (compareSubItem == 0)
                        {
                            return x.SupplyDate.CompareTo(y.SupplyDate);    
                        }

                        return compareSubItem;
                    }

                    return compareSite;
                }

                return compareItem;
            }

            public static SupplySortBy Sort()
            {
                return new SupplySortBy();
            }
        } 

        private class Supply
        {
            public int InventoryID;
            public int SiteID;
            public int SubItemID;
            public System.DateTime SupplyDate;
            public decimal Quantity;
        }

    }
}