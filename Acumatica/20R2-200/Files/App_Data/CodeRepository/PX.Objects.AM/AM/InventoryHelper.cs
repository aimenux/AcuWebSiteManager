using System;
using System.Collections.Generic;
using PX.Data;
using PX.Objects.AM.CacheExtensions;
using PX.Objects.CS;
using PX.Objects.IN;
using PX.Objects.PO;
using PX.Objects.AM.GraphExtensions;

namespace PX.Objects.AM
{
    /// <summary>
    /// Manufacturing Inventory helper class
    /// Shared method such as get lot size, default bin location, etc.
    /// </summary>
    public static class InventoryHelper
    {
        public static bool IsInvalidItemStatus(InventoryItem inventoryItem)
        {
            if (inventoryItem == null)
            {
                return false;
            }

            return IsInvalidItemStatus(inventoryItem.ItemStatus);
        }

        public static bool IsInvalidItemStatus(string itemStatus)
        {
            return itemStatus == INItemStatus.Inactive ||
                   itemStatus == INItemStatus.ToDelete;
        }
        
        public static decimal GetMfgReorderQty(PXGraph graph, int? inventoryID, int? siteId = null, decimal? qty = null)
        {
            if (inventoryID == null)
            {
                return 0;
            }

            decimal orderQty = qty ?? 1m;

            if (siteId != null)
            {
                INItemSite inItemSite = PXSelect
                    <INItemSite, Where<INItemSite.inventoryID, Equal<Required<INItemSite.inventoryID>>,
                        And<INItemSite.siteID, Equal<Required<INItemSite.siteID>>>>>.Select(graph, inventoryID,
                            siteId);

                if (inItemSite != null)
                {
                    INItemSiteExt inItemSiteExt =
                        PXCache<INItemSite>.GetExtension<INItemSiteExt>(inItemSite);

                    if (inItemSiteExt != null)
                    {
                        return ReorderQuantity(orderQty, inItemSiteExt.AMMinOrdQty, inItemSiteExt.AMMaxOrdQty, inItemSiteExt.AMLotSize);
                    }
                }
            }

            InventoryItem inventoryItem = PXSelect
                <InventoryItem, Where<InventoryItem.inventoryID, Equal<Required<InventoryItem.inventoryID>>>>
                .Select(graph, inventoryID);

            if (inventoryItem == null)
            {
                return 0;
            }

            InventoryItemExt inventoryItemExt =
                PXCache<InventoryItem>.GetExtension<InventoryItemExt>(inventoryItem);

            if (inventoryItemExt != null)
            {
                return ReorderQuantity(orderQty, inventoryItemExt.AMMinOrdQty, inventoryItemExt.AMMaxOrdQty, inventoryItemExt.AMLotSize);
            }

            return orderQty;
        }

        public static decimal GetPurchaseOrderQtyByVendor(PXGraph graph, int? inventoryID, int? vendorID, decimal? qty)
        {
            if (inventoryID == null || qty == 0 || vendorID == null)
            {
                return 0;
            }

            POVendorInventory vendorInventory = PXSelect<POVendorInventory,
                Where<POVendorInventory.vendorID, Equal<Required<POVendorInventory.vendorID>>,
                    And<POVendorInventory.inventoryID, Equal<Required<POVendorInventory.inventoryID>>>
                >>.SelectWindowed(graph, 0, 1, vendorID, inventoryID);

            if(vendorInventory == null)
            {
                return qty.GetValueOrDefault();
            }

            // Assumes the returning value is the UOM related to the Vendor Inventory...
            return ReorderQuantity(qty.GetValueOrDefault(), vendorInventory.MinOrdQty.GetValueOrDefault(), vendorInventory.MaxOrdQty.GetValueOrDefault(),
                vendorInventory.LotSize.GetValueOrDefault());
        }

        /// <summary>
        /// Retrieves the fixed manufacturing lead time value for an item [and site]
        /// </summary>
        /// <param name="graph">calling graph</param>
        /// <param name="inventoryID">Inventory ID</param>
        /// <param name="siteID">Warehouse ID</param>
        /// <returns></returns>
        public static int GetFixMfgLeadTime(PXGraph graph, int? inventoryID, int? siteID = null)
        {
            if (graph == null)
            {
                throw new ArgumentNullException(nameof(graph));
            }

            if (inventoryID == null)
            {
                return 0;
            }

            if (siteID != null)
            {
                INItemSite inItemSite = PXSelectReadonly
                    <INItemSite, Where<INItemSite.inventoryID, Equal<Required<INItemSite.inventoryID>>,
                        And<INItemSite.siteID, Equal<Required<INItemSite.siteID>>>>>.Select(graph, inventoryID,
                            siteID);

                if (inItemSite != null)
                {
                    INItemSiteExt inItemSiteExt =
                        PXCache<INItemSite>.GetExtension<INItemSiteExt>(inItemSite);

                    if (inItemSiteExt != null && inItemSiteExt.AMMFGLeadTime.GetValueOrDefault() != 0)
                    {
                        return inItemSiteExt.AMMFGLeadTime.GetValueOrDefault();
                    }
                }
            }

            InventoryItem inventoryItem = PXSelectReadonly
                <InventoryItem, Where<InventoryItem.inventoryID, Equal<Required<InventoryItem.inventoryID>>>>
                .Select(graph, inventoryID);

            if (inventoryItem == null)
            {
                return 0;
            }

            InventoryItemExt inventoryItemExt =
                PXCache<InventoryItem>.GetExtension<InventoryItemExt>(inventoryItem);

            return inventoryItemExt.AMMFGLeadTime.GetValueOrDefault();
        }

        public static PXResult<InventoryItem, INLotSerClass> GetItemLotSerClass(PXGraph graph, int? inventoryId)
        {
            if (graph == null || inventoryId.GetValueOrDefault() == 0)
            {
                return null;
            }

            return (PXResult<InventoryItem, INLotSerClass>)PXSelectJoin<InventoryItem,
                LeftJoin<INLotSerClass, On<INLotSerClass.lotSerClassID, Equal<InventoryItem.lotSerClassID>>>,
                Where<InventoryItem.inventoryID, Equal<Required<InventoryItem.inventoryID>>>>.SelectWindowed(graph, 0, 1, inventoryId);
        }

        /// <summary>
        /// Get the qty avail for an item/subitem/bin combo. 
        /// </summary>
        /// <param name="graph">Calling PXGraph</param>
        /// <param name="inventoryId">Inventory Item ID</param>
        /// <param name="subItemId">Sub Item</param>
        /// <param name="locationID">Bin Location ID</param>
        /// <returns>Current quantity available</returns>
        public static decimal GetQtyAvail(PXGraph graph, int? inventoryId, int? subItemId, int? siteID, int? locationID)
        {
            return GetQtyAvail(graph, inventoryId, subItemId, siteID, locationID, null);
        }

        /// <summary>
        /// Get the qty avail for an item/subitem/bin combo. 
        /// </summary>
        /// <param name="graph">Calling PXGraph</param>
        /// <param name="inventoryId">Inventory Item ID</param>
        /// <param name="subItemId">Sub Item</param>
        /// <param name="locationID">Bin Location ID</param>
        /// <param name="lotSerialNbr">Lot/Serial Number (leave null/empty to skip check on lot/serial)</param>
        /// <returns>Current quantity available</returns>
        public static decimal GetQtyAvail(PXGraph graph, int? inventoryId, int? subItemId, int? siteID, int? locationID, string lotSerialNbr)
        {
            return GetQtyAvail(graph, inventoryId, subItemId, siteID, locationID, lotSerialNbr, null, null, true);
        }

        /// <summary>
        /// Get the qty avail for an item/subitem/bin combo. 
        /// </summary>
        /// <param name="graph">Calling PXGraph</param>
        /// <param name="inventoryId">Inventory Item ID</param>
        /// <param name="subItemId">Sub Item</param>
        /// <param name="locationID">Bin Location ID</param>
        /// <param name="lotSerialNbr">Lot/Serial Number (leave null/empty to skip check on lot/serial)</param>
        /// <param name="receiptsAllowed">find bin locations with receipts valid matching value (null = any value)</param>
        /// <param name="salesAllowed">find bin locations with sales valid matching value (null = any value)</param>
        /// <param name="assemblyAllowed">find bin locations with assembly valid matching value (null = any value)</param>
        /// <returns>Current quantity available</returns>
        public static decimal GetQtyAvail(PXGraph graph, int? inventoryId, int? subItemId, int? siteID, int? locationID, string lotSerialNbr,
            bool? receiptsAllowed, bool? salesAllowed, bool? assemblyAllowed)
        {
            if (graph == null || inventoryId.GetValueOrDefault() == 0)
            {
                return 0m;
            }

            if (string.IsNullOrWhiteSpace(lotSerialNbr))
            {
                var locactionStatus = GetLocationStatusSum(graph, inventoryId, subItemId, siteID, locationID, receiptsAllowed, salesAllowed, assemblyAllowed);
                return locactionStatus?.QtyOnHand ?? 0m;
            }

            var lotSerialStatus = GetLotSerialStatusSum(graph, inventoryId, subItemId, siteID, locationID, lotSerialNbr, receiptsAllowed, salesAllowed, assemblyAllowed);
            return lotSerialStatus?.QtyOnHand ?? 0m;
        }

        /// <summary>
        /// Get the qty hard avail for an item/subitem/bin combo. 
        /// </summary>
        /// <param name="graph">Calling PXGraph</param>
        /// <param name="inventoryId">Inventory Item ID</param>
        /// <param name="subItemId">Sub Item</param>
        /// <param name="locationID">Bin Location ID</param>
        /// <param name="lotSerialNbr">Lot/Serial Number (leave null/empty to skip check on lot/serial)</param>
        /// <param name="receiptsAllowed">find bin locations with receipts valid matching value (null = any value)</param>
        /// <param name="salesAllowed">find bin locations with sales valid matching value (null = any value)</param>
        /// <param name="assemblyAllowed">find bin locations with assembly valid matching value (null = any value)</param>
        /// <returns>Current quantity available</returns>
        public static decimal GetQtyHardAvail(PXGraph graph, int? inventoryId, int? subItemId, int? siteID, int? locationID, string lotSerialNbr,
            bool? receiptsAllowed, bool? salesAllowed, bool? assemblyAllowed)
        {
            if (graph == null || inventoryId.GetValueOrDefault() == 0)
            {
                return 0m;
            }

            //Getting INSiteStatus hard available as INLocationStatus does not store hard avail value
            var siteHardAvail = GetSiteStatusSum(graph, inventoryId, subItemId, siteID)?.QtyHardAvail;
            var hardAvail = 0m;

            if (string.IsNullOrWhiteSpace(lotSerialNbr))
            {
                var locactionStatus = GetLocationStatusSum(graph, inventoryId, subItemId, siteID, locationID, receiptsAllowed, salesAllowed, assemblyAllowed);
                hardAvail = locactionStatus?.QtyHardAvail ?? 0m;
            }
            else
            {
                var lotSerialStatus = GetLotSerialStatusSum(graph, inventoryId, subItemId, siteID, locationID, lotSerialNbr, receiptsAllowed, salesAllowed, assemblyAllowed);
                hardAvail = lotSerialStatus?.QtyHardAvail ?? 0m;
            }

            if (siteHardAvail != null)
            {
                if (hardAvail == 0 && siteHardAvail.GetValueOrDefault() != 0)
                {
                    return siteHardAvail.GetValueOrDefault();
                }

                return Math.Min(hardAvail, siteHardAvail.GetValueOrDefault());
            }

            return hardAvail;
        }

        /// <summary>
        /// Query INLocationStatus and return the results
        /// </summary>
        public static PXResultset<INLocationStatus> GetLocationStatus(PXGraph graph, int? inventoryID, int? subitemID, int? siteID, int? locationID)
        {
            return GetLocationStatus(graph, inventoryID, subitemID, siteID, locationID, false, false, true);
        }

        /// <summary>
        /// Query INLocationStatus and return the results
        /// </summary>
        public static PXResultset<INLocationStatus> GetLocationStatus(PXGraph graph, int? inventoryID, int? subitemID, int? siteID, int? locationID,
            bool? receiptsAllowed, bool? salesAllowed, bool? assemblyAllowed)
        {
            if (inventoryID.GetValueOrDefault() == 0)
            {
                throw new PXArgumentException("inventoryID");
            }

            bool multiWarehouseEnabled = PXAccess.FeatureInstalled<FeaturesSet.warehouse>();
            if (siteID.GetValueOrDefault() == 0 && multiWarehouseEnabled)
            {
                throw new PXArgumentException("siteID");
            }

            PXSelectBase<INLocationStatus> cmd = new PXSelectJoin<INLocationStatus,
                InnerJoin<INLocation, On<INLocationStatus.locationID, Equal<INLocation.locationID>>>,
                    Where<INLocationStatus.inventoryID, Equal<Required<INLocationStatus.inventoryID>>,
                        And<INLocationStatus.qtyHardAvail, Greater<decimal0>,
                        And<INLocation.active, Equal<True>>>>,
                    OrderBy<Asc<INLocation.pickPriority>>>(graph);

            var cmdParms = new List<object> { inventoryID };

            BuildLocationStatusCommand(ref cmd, ref cmdParms, subitemID, siteID, locationID,
                receiptsAllowed, salesAllowed, assemblyAllowed);

            return cmd.Select(cmdParms.ToArray());
        }

        /// <summary>
        /// Query INLocationStatus and return the sum of the results
        /// </summary>
        public static INLocationStatus GetLocationStatusSum(PXGraph graph, int? inventoryID, int? subitemID, int? siteID, int? locationID,
            bool? receiptsAllowed, bool? salesAllowed, bool? assemblyAllowed)
        {
            if (inventoryID.GetValueOrDefault() == 0)
            {
                throw new PXArgumentException(nameof(inventoryID));
            }

            bool multiWarehouseEnabled = PXAccess.FeatureInstalled<FeaturesSet.warehouse>();
            if (siteID.GetValueOrDefault() == 0 && multiWarehouseEnabled)
            {
                throw new PXArgumentException(nameof(siteID));
            }

            PXSelectBase<INLocationStatus> cmd = new PXSelectJoinGroupBy<INLocationStatus,
                InnerJoin<INLocation, On<INLocationStatus.locationID, Equal<INLocation.locationID>>>,
                            Where<INLocationStatus.inventoryID, Equal<Required<INLocationStatus.inventoryID>>>,
                    Aggregate<
                        Sum<INLocationStatus.qtyOnHand,
                        Sum<INLocationStatus.qtyAvail,
                        Sum<INLocationStatus.qtyNotAvail,
                        Sum<INLocationStatus.qtyExpired,
                        Sum<INLocationStatus.qtyHardAvail,
                        Sum<INLocationStatus.qtyActual,
                        Sum<INLocationStatus.qtyFSSrvOrdBooked,
                        Sum<INLocationStatus.qtyFSSrvOrdAllocated,
                        Sum<INLocationStatus.qtyFSSrvOrdPrepared,
                        Sum<INLocationStatus.qtySOBackOrdered,
                        Sum<INLocationStatus.qtySOPrepared,
                        Sum<INLocationStatus.qtySOBooked,
                        Sum<INLocationStatus.qtySOShipped,
                        Sum<INLocationStatus.qtySOShipping,
                        Sum<INLocationStatus.qtyINIssues,
                        Sum<INLocationStatus.qtyINReceipts,
                        Sum<INLocationStatus.qtyInTransit,
                        Sum<INLocationStatus.qtyInTransitToSO,
                        Sum<INLocationStatus.qtyPOReceipts,
                        Sum<INLocationStatus.qtyPOPrepared,
                        Sum<INLocationStatus.qtyPOOrders,
                        Sum<INLocationStatus.qtyFixedFSSrvOrd,
                        Sum<INLocationStatus.qtyPOFixedFSSrvOrd,
                        Sum<INLocationStatus.qtyPOFixedFSSrvOrdPrepared,
                        Sum<INLocationStatus.qtyPOFixedFSSrvOrdReceipts,
                        Sum<INLocationStatus.qtySOFixed,
                        Sum<INLocationStatus.qtyPOFixedOrders,
                        Sum<INLocationStatus.qtyPOFixedPrepared,
                        Sum<INLocationStatus.qtyPOFixedReceipts,
                        Sum<INLocationStatus.qtySODropShip,
                        Sum<INLocationStatus.qtyPODropShipOrders,
                        Sum<INLocationStatus.qtyPODropShipPrepared,
                        Sum<INLocationStatus.qtyPODropShipReceipts,
                        Sum<INLocationStatus.qtyINAssemblySupply,
                        Sum<INLocationStatus.qtyINAssemblyDemand,
                        Sum<INLocationStatus.qtyInTransitToProduction,
                        Sum<INLocationStatus.qtyProductionSupplyPrepared,
                        Sum<INLocationStatus.qtyProductionSupply,
                        Sum<INLocationStatus.qtyPOFixedProductionPrepared,
                        Sum<INLocationStatus.qtyPOFixedProductionOrders,
                        Sum<INLocationStatus.qtyProductionDemandPrepared,
                        Sum<INLocationStatus.qtyProductionDemand,
                        Sum<INLocationStatus.qtyProductionAllocated,
                        Sum<INLocationStatus.qtySOFixedProduction,
                        Sum<INLocationStatus.qtyProdFixedPurchase,
                        Sum<INLocationStatus.qtyProdFixedProduction,
                        Sum<INLocationStatus.qtyProdFixedProdOrdersPrepared,
                        Sum<INLocationStatus.qtyProdFixedProdOrders,
                        Sum<INLocationStatus.qtyProdFixedSalesOrdersPrepared,
                        Sum<INLocationStatus.qtyProdFixedSalesOrders
                        >>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>(graph);

            var cmdParms = new List<object> { inventoryID };

            BuildLocationStatusCommand(ref cmd, ref cmdParms, subitemID, siteID, locationID,
                receiptsAllowed, salesAllowed, assemblyAllowed);

            var locationStatus = new INLocationStatus
            {
                InventoryID = inventoryID,
                SubItemID = subitemID,
                SiteID = siteID,
                LocationID = locationID,
                QtyOnHand = 0m,
                QtyAvail = 0m,
                QtyNotAvail = 0m,
                QtyExpired = 0m,
                QtyHardAvail = 0m,
                QtyActual = 0m,
                QtyFSSrvOrdBooked = 0m,
                QtyFSSrvOrdAllocated = 0m,
                QtyFSSrvOrdPrepared = 0m,
                QtySOBackOrdered = 0m,
                QtySOPrepared = 0m,
                QtySOBooked = 0m,
                QtySOShipped = 0m,
                QtySOShipping = 0m,
                QtyINIssues = 0m,
                QtyINReceipts = 0m,
                QtyInTransit = 0m,
                QtyInTransitToSO = 0m,
                QtyPOReceipts = 0m,
                QtyPOPrepared = 0m,
                QtyPOOrders = 0m,
                QtyFixedFSSrvOrd = 0m,
                QtyPOFixedFSSrvOrd = 0m,
                QtyPOFixedFSSrvOrdPrepared = 0m,
                QtyPOFixedFSSrvOrdReceipts = 0m,
                QtySOFixed = 0m,
                QtyPOFixedOrders = 0m,
                QtyPOFixedPrepared = 0m,
                QtyPOFixedReceipts = 0m,
                QtySODropShip = 0m,
                QtyPODropShipOrders = 0m,
                QtyPODropShipPrepared = 0m,
                QtyPODropShipReceipts = 0m,
                QtyINAssemblySupply = 0m,
                QtyINAssemblyDemand = 0m,
                QtyInTransitToProduction = 0m,
                QtyProductionSupplyPrepared = 0m,
                QtyProductionSupply = 0m,
                QtyPOFixedProductionPrepared = 0m,
                QtyPOFixedProductionOrders = 0m,
                QtyProductionDemandPrepared = 0m,
                QtyProductionDemand = 0m,
                QtyProductionAllocated = 0m,
                QtySOFixedProduction = 0m,
                QtyProdFixedPurchase = 0m,
                QtyProdFixedProduction = 0m,
                QtyProdFixedProdOrdersPrepared = 0m,
                QtyProdFixedProdOrders = 0m,
                QtyProdFixedSalesOrdersPrepared = 0m,
                QtyProdFixedSalesOrders = 0m
            };

            foreach (INLocationStatus result in cmd.Select(cmdParms.ToArray()))
            {
                locationStatus.QtyOnHand += result.QtyOnHand.GetValueOrDefault();
                locationStatus.QtyAvail += result.QtyAvail.GetValueOrDefault();
                locationStatus.QtyNotAvail += result.QtyNotAvail.GetValueOrDefault();
                locationStatus.QtyExpired += result.QtyExpired.GetValueOrDefault();
                locationStatus.QtyHardAvail += result.QtyHardAvail.GetValueOrDefault();
                locationStatus.QtyActual += result.QtyActual.GetValueOrDefault();
                locationStatus.QtyFSSrvOrdBooked += result.QtyFSSrvOrdBooked.GetValueOrDefault();
                locationStatus.QtyFSSrvOrdAllocated += result.QtyFSSrvOrdAllocated.GetValueOrDefault();
                locationStatus.QtyFSSrvOrdPrepared += result.QtyFSSrvOrdPrepared.GetValueOrDefault();
                locationStatus.QtySOBackOrdered += result.QtySOBackOrdered.GetValueOrDefault();
                locationStatus.QtySOPrepared += result.QtySOPrepared.GetValueOrDefault();
                locationStatus.QtySOBooked += result.QtySOBooked.GetValueOrDefault();
                locationStatus.QtySOShipped += result.QtySOShipped.GetValueOrDefault();
                locationStatus.QtySOShipping += result.QtySOShipping.GetValueOrDefault();
                locationStatus.QtyINIssues += result.QtyINIssues.GetValueOrDefault();
                locationStatus.QtyINReceipts += result.QtyINReceipts.GetValueOrDefault();
                locationStatus.QtyInTransit += result.QtyInTransit.GetValueOrDefault();
                locationStatus.QtyInTransitToSO += result.QtyInTransitToSO.GetValueOrDefault();
                locationStatus.QtyPOReceipts += result.QtyPOReceipts.GetValueOrDefault();
                locationStatus.QtyPOPrepared += result.QtyPOPrepared.GetValueOrDefault();
                locationStatus.QtyPOOrders += result.QtyPOOrders.GetValueOrDefault();
                locationStatus.QtyFixedFSSrvOrd += result.QtyFixedFSSrvOrd.GetValueOrDefault();
                locationStatus.QtyPOFixedFSSrvOrd += result.QtyPOFixedFSSrvOrd.GetValueOrDefault();
                locationStatus.QtyPOFixedFSSrvOrdPrepared += result.QtyPOFixedFSSrvOrdPrepared.GetValueOrDefault();
                locationStatus.QtyPOFixedFSSrvOrdReceipts += result.QtyPOFixedFSSrvOrdReceipts.GetValueOrDefault();
                locationStatus.QtySOFixed += result.QtySOFixed.GetValueOrDefault();
                locationStatus.QtyPOFixedOrders += result.QtyPOFixedOrders.GetValueOrDefault();
                locationStatus.QtyPOFixedPrepared += result.QtyPOFixedPrepared.GetValueOrDefault();
                locationStatus.QtyPOFixedReceipts += result.QtyPOFixedReceipts.GetValueOrDefault();
                locationStatus.QtySODropShip += result.QtySODropShip.GetValueOrDefault();
                locationStatus.QtyPODropShipOrders += result.QtyPODropShipOrders.GetValueOrDefault();
                locationStatus.QtyPODropShipPrepared += result.QtyPODropShipPrepared.GetValueOrDefault();
                locationStatus.QtyPODropShipReceipts += result.QtyPODropShipReceipts.GetValueOrDefault();
                locationStatus.QtyINAssemblySupply += result.QtyINAssemblySupply.GetValueOrDefault();
                locationStatus.QtyINAssemblyDemand += result.QtyINAssemblyDemand.GetValueOrDefault();
                locationStatus.QtyInTransitToProduction += result.QtyInTransitToProduction.GetValueOrDefault();
                locationStatus.QtyProductionSupplyPrepared += result.QtyProductionSupplyPrepared.GetValueOrDefault();
                locationStatus.QtyProductionSupply += result.QtyProductionSupply.GetValueOrDefault();
                locationStatus.QtyPOFixedProductionPrepared += result.QtyPOFixedProductionPrepared.GetValueOrDefault();
                locationStatus.QtyPOFixedProductionOrders += result.QtyPOFixedProductionOrders.GetValueOrDefault();
                locationStatus.QtyProductionDemandPrepared += result.QtyProductionDemandPrepared.GetValueOrDefault();
                locationStatus.QtyProductionDemand += result.QtyProductionDemand.GetValueOrDefault();
                locationStatus.QtyProductionAllocated += result.QtyProductionAllocated.GetValueOrDefault();
                locationStatus.QtySOFixedProduction += result.QtySOFixedProduction.GetValueOrDefault();
                locationStatus.QtyProdFixedPurchase += result.QtyProdFixedPurchase.GetValueOrDefault();
                locationStatus.QtyProdFixedProduction += result.QtyProdFixedProduction.GetValueOrDefault();
                locationStatus.QtyProdFixedProdOrdersPrepared += result.QtyProdFixedProdOrdersPrepared.GetValueOrDefault();
                locationStatus.QtyProdFixedProdOrders += result.QtyProdFixedProdOrders.GetValueOrDefault();
                locationStatus.QtyProdFixedSalesOrdersPrepared += result.QtyProdFixedSalesOrdersPrepared.GetValueOrDefault();
                locationStatus.QtyProdFixedSalesOrders += result.QtyProdFixedSalesOrders.GetValueOrDefault();
            }

            return locationStatus;
        }

        /// <summary>
        /// Builds the where statement for the base select for location status records
        /// </summary>
        private static void BuildLocationStatusCommand(ref PXSelectBase<INLocationStatus> cmd, ref List<object> cmdParms,
            int? subitemID, int? siteID, int? locationID,
            bool? receiptsAllowed, bool? salesAllowed, bool? assemblyAllowed)
        {
            if (siteID != null)
            {
                cmd.WhereAnd<Where<INLocationStatus.siteID, Equal<Required<INLocationStatus.siteID>>>>();
                cmdParms.Add(siteID);
            }

            if (subitemID.GetValueOrDefault() != 0 && PXAccess.FeatureInstalled<FeaturesSet.subItem>())
            {
                cmd.WhereAnd<Where<INLocationStatus.subItemID, Equal<Required<INLocationStatus.subItemID>>,
                    Or<INLocationStatus.subItemID, IsNull>>>();
                cmdParms.Add(subitemID);
            }

            if (locationID.GetValueOrDefault() != 0 && PXAccess.FeatureInstalled<FeaturesSet.warehouseLocation>())
            {
                cmd.WhereAnd<Where<INLocationStatus.locationID, Equal<Required<INLocationStatus.locationID>>>>();
                cmdParms.Add(locationID);
            }

            if (receiptsAllowed != null)
            {
                cmd.WhereAnd<Where<INLocation.receiptsValid, Equal<Required<INLocation.receiptsValid>>>>();
                cmdParms.Add(receiptsAllowed.GetValueOrDefault());
            }

            if (salesAllowed != null)
            {
                cmd.WhereAnd<Where<INLocation.salesValid, Equal<Required<INLocation.salesValid>>>>();
                cmdParms.Add(salesAllowed.GetValueOrDefault());
            }

            if (assemblyAllowed != null)
            {
                cmd.WhereAnd<Where<INLocation.assemblyValid, Equal<Required<INLocation.assemblyValid>>>>();
                cmdParms.Add(assemblyAllowed.GetValueOrDefault());
            }
        }

        public static INSiteStatus GetSiteStatusSum(PXGraph graph, int? inventoryID, int? subitemID, int? siteID)
        {
            if (inventoryID.GetValueOrDefault() == 0)
            {
                throw new PXArgumentException(nameof(inventoryID));
            }

            bool multiWarehouseEnabled = PXAccess.FeatureInstalled<FeaturesSet.warehouse>();
            if (siteID.GetValueOrDefault() == 0 && multiWarehouseEnabled)
            {
                throw new PXArgumentException(nameof(siteID));
            }

            PXSelectBase<INSiteStatus> cmd = new PXSelectGroupBy<INSiteStatus,
                            Where<INSiteStatus.inventoryID, Equal<Required<INSiteStatus.inventoryID>>>,
                    Aggregate<
                        Sum<INSiteStatus.qtyOnHand,
                        Sum<INSiteStatus.qtyAvail,
                        Sum<INSiteStatus.qtyNotAvail,
                        Sum<INSiteStatus.qtyExpired,
                        Sum<INSiteStatus.qtyHardAvail,
                        Sum<INSiteStatus.qtyActual,
                        Sum<INSiteStatus.qtyFSSrvOrdBooked,
                        Sum<INSiteStatus.qtyFSSrvOrdAllocated,
                        Sum<INSiteStatus.qtyFSSrvOrdPrepared,
                        Sum<INSiteStatus.qtySOBackOrdered,
                        Sum<INSiteStatus.qtySOPrepared,
                        Sum<INSiteStatus.qtySOBooked,
                        Sum<INSiteStatus.qtySOShipped,
                        Sum<INSiteStatus.qtySOShipping,
                        Sum<INSiteStatus.qtyINIssues,
                        Sum<INSiteStatus.qtyINReceipts,
                        Sum<INSiteStatus.qtyInTransit,
                        Sum<INSiteStatus.qtyInTransitToSO,
                        Sum<INSiteStatus.qtyPOReceipts,
                        Sum<INSiteStatus.qtyPOPrepared,
                        Sum<INSiteStatus.qtyPOOrders,
                        Sum<INSiteStatus.qtyFixedFSSrvOrd,
                        Sum<INSiteStatus.qtyPOFixedFSSrvOrd,
                        Sum<INSiteStatus.qtyPOFixedFSSrvOrdPrepared,
                        Sum<INSiteStatus.qtyPOFixedFSSrvOrdReceipts,
                        Sum<INSiteStatus.qtySOFixed,
                        Sum<INSiteStatus.qtyPOFixedOrders,
                        Sum<INSiteStatus.qtyPOFixedPrepared,
                        Sum<INSiteStatus.qtyPOFixedReceipts,
                        Sum<INSiteStatus.qtySODropShip,
                        Sum<INSiteStatus.qtyPODropShipOrders,
                        Sum<INSiteStatus.qtyPODropShipPrepared,
                        Sum<INSiteStatus.qtyPODropShipReceipts,
                        Sum<INSiteStatus.qtyINAssemblySupply,
                        Sum<INSiteStatus.qtyINAssemblyDemand,
                        Sum<INSiteStatus.qtyInTransitToProduction,
                        Sum<INSiteStatus.qtyProductionSupplyPrepared,
                        Sum<INSiteStatus.qtyProductionSupply,
                        Sum<INSiteStatus.qtyPOFixedProductionPrepared,
                        Sum<INSiteStatus.qtyPOFixedProductionOrders,
                        Sum<INSiteStatus.qtyProductionDemandPrepared,
                        Sum<INSiteStatus.qtyProductionDemand,
                        Sum<INSiteStatus.qtyProductionAllocated,
                        Sum<INSiteStatus.qtySOFixedProduction,
                        Sum<INSiteStatus.qtyProdFixedPurchase,
                        Sum<INSiteStatus.qtyProdFixedProduction,
                        Sum<INSiteStatus.qtyProdFixedProdOrdersPrepared,
                        Sum<INSiteStatus.qtyProdFixedProdOrders,
                        Sum<INSiteStatus.qtyProdFixedSalesOrdersPrepared,
                        Sum<INSiteStatus.qtyProdFixedSalesOrders
                        >>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>(graph);

            var cmdParms = new List<object> { inventoryID };

            if (siteID != null)
            {
                cmd.WhereAnd<Where<INSiteStatus.siteID, Equal<Required<INSiteStatus.siteID>>>>();
                cmdParms.Add(siteID);
            }

            if (subitemID.GetValueOrDefault() != 0 && PXAccess.FeatureInstalled<FeaturesSet.subItem>())
            {
                cmd.WhereAnd<Where<INSiteStatus.subItemID, Equal<Required<INSiteStatus.subItemID>>,
                    Or<INSiteStatus.subItemID, IsNull>>>();
                cmdParms.Add(subitemID);
            }

            var siteStatus = new INSiteStatus
            {
                InventoryID = inventoryID,
                SubItemID = subitemID,
                SiteID = siteID,
                QtyOnHand = 0m,
                QtyAvail = 0m,
                QtyNotAvail = 0m,
                QtyExpired = 0m,
                QtyHardAvail = 0m,
                QtyActual = 0m,
                QtyFSSrvOrdBooked = 0m,
                QtyFSSrvOrdAllocated = 0m,
                QtyFSSrvOrdPrepared = 0m,
                QtySOBackOrdered = 0m,
                QtySOPrepared = 0m,
                QtySOBooked = 0m,
                QtySOShipped = 0m,
                QtySOShipping = 0m,
                QtyINIssues = 0m,
                QtyINReceipts = 0m,
                QtyInTransit = 0m,
                QtyInTransitToSO = 0m,
                QtyPOReceipts = 0m,
                QtyPOPrepared = 0m,
                QtyPOOrders = 0m,
                QtyFixedFSSrvOrd = 0m,
                QtyPOFixedFSSrvOrd = 0m,
                QtyPOFixedFSSrvOrdPrepared = 0m,
                QtyPOFixedFSSrvOrdReceipts = 0m,
                QtySOFixed = 0m,
                QtyPOFixedOrders = 0m,
                QtyPOFixedPrepared = 0m,
                QtyPOFixedReceipts = 0m,
                QtySODropShip = 0m,
                QtyPODropShipOrders = 0m,
                QtyPODropShipPrepared = 0m,
                QtyPODropShipReceipts = 0m,
                QtyINAssemblySupply = 0m,
                QtyINAssemblyDemand = 0m,
                QtyInTransitToProduction = 0m,
                QtyProductionSupplyPrepared = 0m,
                QtyProductionSupply = 0m,
                QtyPOFixedProductionPrepared = 0m,
                QtyPOFixedProductionOrders = 0m,
                QtyProductionDemandPrepared = 0m,
                QtyProductionDemand = 0m,
                QtyProductionAllocated = 0m,
                QtySOFixedProduction = 0m,
                QtyProdFixedPurchase = 0m,
                QtyProdFixedProduction = 0m,
                QtyProdFixedProdOrdersPrepared = 0m,
                QtyProdFixedProdOrders = 0m,
                QtyProdFixedSalesOrdersPrepared = 0m,
                QtyProdFixedSalesOrders = 0m
            };

            foreach (INSiteStatus result in cmd.Select(cmdParms.ToArray()))
            {
                siteStatus.QtyOnHand += result.QtyOnHand.GetValueOrDefault();
                siteStatus.QtyAvail += result.QtyAvail.GetValueOrDefault();
                siteStatus.QtyNotAvail += result.QtyNotAvail.GetValueOrDefault();
                siteStatus.QtyExpired += result.QtyExpired.GetValueOrDefault();
                siteStatus.QtyHardAvail += result.QtyHardAvail.GetValueOrDefault();
                siteStatus.QtyActual += result.QtyActual.GetValueOrDefault();
                siteStatus.QtyFSSrvOrdBooked += result.QtyFSSrvOrdBooked.GetValueOrDefault();
                siteStatus.QtyFSSrvOrdAllocated += result.QtyFSSrvOrdAllocated.GetValueOrDefault();
                siteStatus.QtyFSSrvOrdPrepared += result.QtyFSSrvOrdPrepared.GetValueOrDefault();
                siteStatus.QtySOBackOrdered += result.QtySOBackOrdered.GetValueOrDefault();
                siteStatus.QtySOPrepared += result.QtySOPrepared.GetValueOrDefault();
                siteStatus.QtySOBooked += result.QtySOBooked.GetValueOrDefault();
                siteStatus.QtySOShipped += result.QtySOShipped.GetValueOrDefault();
                siteStatus.QtySOShipping += result.QtySOShipping.GetValueOrDefault();
                siteStatus.QtyINIssues += result.QtyINIssues.GetValueOrDefault();
                siteStatus.QtyINReceipts += result.QtyINReceipts.GetValueOrDefault();
                siteStatus.QtyInTransit += result.QtyInTransit.GetValueOrDefault();
                siteStatus.QtyInTransitToSO += result.QtyInTransitToSO.GetValueOrDefault();
                siteStatus.QtyPOReceipts += result.QtyPOReceipts.GetValueOrDefault();
                siteStatus.QtyPOPrepared += result.QtyPOPrepared.GetValueOrDefault();
                siteStatus.QtyPOOrders += result.QtyPOOrders.GetValueOrDefault();
                siteStatus.QtyFixedFSSrvOrd += result.QtyFixedFSSrvOrd.GetValueOrDefault();
                siteStatus.QtyPOFixedFSSrvOrd += result.QtyPOFixedFSSrvOrd.GetValueOrDefault();
                siteStatus.QtyPOFixedFSSrvOrdPrepared += result.QtyPOFixedFSSrvOrdPrepared.GetValueOrDefault();
                siteStatus.QtyPOFixedFSSrvOrdReceipts += result.QtyPOFixedFSSrvOrdReceipts.GetValueOrDefault();
                siteStatus.QtySOFixed += result.QtySOFixed.GetValueOrDefault();
                siteStatus.QtyPOFixedOrders += result.QtyPOFixedOrders.GetValueOrDefault();
                siteStatus.QtyPOFixedPrepared += result.QtyPOFixedPrepared.GetValueOrDefault();
                siteStatus.QtyPOFixedReceipts += result.QtyPOFixedReceipts.GetValueOrDefault();
                siteStatus.QtySODropShip += result.QtySODropShip.GetValueOrDefault();
                siteStatus.QtyPODropShipOrders += result.QtyPODropShipOrders.GetValueOrDefault();
                siteStatus.QtyPODropShipPrepared += result.QtyPODropShipPrepared.GetValueOrDefault();
                siteStatus.QtyPODropShipReceipts += result.QtyPODropShipReceipts.GetValueOrDefault();
                siteStatus.QtyINAssemblySupply += result.QtyINAssemblySupply.GetValueOrDefault();
                siteStatus.QtyINAssemblyDemand += result.QtyINAssemblyDemand.GetValueOrDefault();
                siteStatus.QtyInTransitToProduction += result.QtyInTransitToProduction.GetValueOrDefault();
                siteStatus.QtyProductionSupplyPrepared += result.QtyProductionSupplyPrepared.GetValueOrDefault();
                siteStatus.QtyProductionSupply += result.QtyProductionSupply.GetValueOrDefault();
                siteStatus.QtyPOFixedProductionPrepared += result.QtyPOFixedProductionPrepared.GetValueOrDefault();
                siteStatus.QtyPOFixedProductionOrders += result.QtyPOFixedProductionOrders.GetValueOrDefault();
                siteStatus.QtyProductionDemandPrepared += result.QtyProductionDemandPrepared.GetValueOrDefault();
                siteStatus.QtyProductionDemand += result.QtyProductionDemand.GetValueOrDefault();
                siteStatus.QtyProductionAllocated += result.QtyProductionAllocated.GetValueOrDefault();
                siteStatus.QtySOFixedProduction += result.QtySOFixedProduction.GetValueOrDefault();
                siteStatus.QtyProdFixedPurchase += result.QtyProdFixedPurchase.GetValueOrDefault();
                siteStatus.QtyProdFixedProduction += result.QtyProdFixedProduction.GetValueOrDefault();
                siteStatus.QtyProdFixedProdOrdersPrepared += result.QtyProdFixedProdOrdersPrepared.GetValueOrDefault();
                siteStatus.QtyProdFixedProdOrders += result.QtyProdFixedProdOrders.GetValueOrDefault();
                siteStatus.QtyProdFixedSalesOrdersPrepared += result.QtyProdFixedSalesOrdersPrepared.GetValueOrDefault();
                siteStatus.QtyProdFixedSalesOrders += result.QtyProdFixedSalesOrders.GetValueOrDefault();
            }

            return siteStatus;
        }

        /// <summary>
        /// Query INLotSerialStatus and return the results
        /// </summary>
        public static PXResultset<INLotSerialStatus> GetLotSerialStatus(PXGraph graph, int? inventoryID, int? subitemID, int? siteID, int? locationID, string lotSerialNbr)
        {
            return GetLotSerialStatus(graph, inventoryID, subitemID, siteID, locationID, lotSerialNbr, null, null, true);
        }

        /// <summary>
        /// Query INLotSerialStatus and return the results
        /// </summary>
        public static PXResultset<INLotSerialStatus> GetLotSerialStatus(PXGraph graph, int? inventoryID, int? subitemID, int? siteID, int? locationID, string lotSerialNbr,
            bool? receiptsAllowed, bool? salesAllowed, bool? assemblyAllowed)
        {
            var result = GetItemLotSerClass(graph, inventoryID);

            INLotSerClass lsClass = null;
            if (result != null)
            {
                lsClass = result;
            }

            return GetLotSerialStatus(graph, lsClass, inventoryID, subitemID, siteID, locationID, lotSerialNbr, receiptsAllowed, salesAllowed, assemblyAllowed);
        }

        /// <summary>
        /// Query INLotSerialStatus and return the results
        /// </summary>
        public static PXResultset<INLotSerialStatus> GetLotSerialStatus(PXGraph graph, INLotSerClass lotSerClass, int? inventoryID, int? subitemID, int? siteID, int? locationID)
        {
            return GetLotSerialStatus(graph, lotSerClass, inventoryID, subitemID, siteID, locationID, null);
        }

        /// <summary>
        /// Query INLotSerialStatus and return the results
        /// </summary>
        public static PXResultset<INLotSerialStatus> GetLotSerialStatus(PXGraph graph, INLotSerClass lotSerClass, int? inventoryID, int? subitemID, int? siteID, int? locationID, string lotSerialNbr)
        {
            return GetLotSerialStatus(graph, lotSerClass, inventoryID, subitemID, siteID, locationID, lotSerialNbr, null, null, true);
        }

        /// <summary>
        /// Query INLotSerialStatus and return the results
        /// </summary>
        public static PXResultset<INLotSerialStatus> GetLotSerialStatus(PXGraph graph, INLotSerClass lotSerClass, int? inventoryID, int? subitemID, int? siteID, int? locationID, string lotSerialNbr,
            bool? receiptsAllowed, bool? salesAllowed, bool? assemblyAllowed)
        {
            if (inventoryID.GetValueOrDefault() == 0)
            {
                throw new PXArgumentException(nameof(inventoryID));
            }

            if (lotSerClass == null
                || string.IsNullOrWhiteSpace(lotSerClass.LotSerClassID))
            {
                throw new PXArgumentException(nameof(lotSerClass));
            }

            if (lotSerClass.LotSerTrack == INLotSerTrack.NotNumbered)
            {
                return null;
            }

            var multiWarehouseEnabled = PXAccess.FeatureInstalled<FeaturesSet.warehouse>();
            if (siteID.GetValueOrDefault() == 0 && multiWarehouseEnabled)
            {
                throw new PXArgumentException(nameof(siteID));
            }

            PXSelectBase<INLotSerialStatus> cmd = new PXSelectJoin<INLotSerialStatus,
                InnerJoin<INLocation, On<INLotSerialStatus.locationID, Equal<INLocation.locationID>>,
                InnerJoin<INItemLotSerial, On<INLotSerialStatus.inventoryID, Equal<INItemLotSerial.inventoryID>,
                    And<INLotSerialStatus.lotSerialNbr, Equal<INItemLotSerial.lotSerialNbr>>>>>,
                    Where<INLotSerialStatus.inventoryID, Equal<Required<INLotSerialStatus.inventoryID>>,
                        And<INLotSerialStatus.qtyHardAvail, Greater<decimal0>,
                        And<INLocation.active, Equal<True>>>>,
                    OrderBy<Asc<INLocation.pickPriority>>>(graph);

            var cmdParms = new List<object> { inventoryID };

            BuildLotSerialStatusCommand(ref cmd, ref cmdParms, lotSerClass, subitemID, siteID, locationID, lotSerialNbr,
                receiptsAllowed, salesAllowed, assemblyAllowed);

            return cmd.Select(cmdParms.ToArray());
        }

        /// <summary>
        /// Query INLotSerialStatus and return the results as a list
        /// </summary>
        public static List<PXResult<INLotSerialStatus, INLocation, INItemLotSerial>> GetLotSerialStatusList(PXGraph graph, INLotSerClass lotSerClass, int? inventoryID, int? subitemID, int? siteID, int? locationID, string lotSerialNbr,
            bool? receiptsAllowed, bool? salesAllowed, bool? assemblyAllowed)
        {
            return GetLotSerialStatus(graph, lotSerClass, inventoryID, subitemID, siteID, locationID, lotSerialNbr,
                    receiptsAllowed, salesAllowed, assemblyAllowed)
                ?.ToList<INLotSerialStatus, INLocation, INItemLotSerial>();
        }

        /// <summary>
        /// Query INLotSerialStatus and return the sum of the results
        /// </summary>
        public static INLotSerialStatus GetLotSerialStatusSum(PXGraph graph, int? inventoryID, int? subitemID, int? siteID, int? locationID, string lotSerialNbr,
            bool? receiptsAllowed, bool? salesAllowed, bool? assemblyAllowed)
        {
            var result = GetItemLotSerClass(graph, inventoryID);
            INLotSerClass lsClass = null;
            if (result != null)
            {
                lsClass = result;
            }

            return GetLotSerialStatusSum(graph, lsClass, inventoryID, subitemID, siteID, locationID, lotSerialNbr, receiptsAllowed, salesAllowed, assemblyAllowed);
        }

        /// <summary>
        /// Query INLotSerialStatus and return the sum of the results
        /// </summary>
        public static INLotSerialStatus GetLotSerialStatusSum(PXGraph graph, INLotSerClass lotSerClass, int? inventoryID, int? subitemID, int? siteID, int? locationID, string lotSerialNbr,
            bool? receiptsAllowed, bool? salesAllowed, bool? assemblyAllowed)
        {
            if (inventoryID.GetValueOrDefault() == 0)
            {
                throw new PXArgumentException(nameof(inventoryID));
            }

            if (lotSerClass == null
                || string.IsNullOrWhiteSpace(lotSerClass.LotSerClassID))
            {
                throw new PXArgumentException(nameof(lotSerClass));
            }

            if (lotSerClass.LotSerTrack == INLotSerTrack.NotNumbered)
            {
                return null;
            }

            bool multiWarehouseEnabled = PXAccess.FeatureInstalled<FeaturesSet.warehouse>();
            if (siteID.GetValueOrDefault() == 0 && multiWarehouseEnabled)
            {
                throw new PXArgumentException(nameof(siteID));
            }

            PXSelectBase<INLotSerialStatus> cmd = new PXSelectJoinGroupBy<INLotSerialStatus,
                    InnerJoin<INLocation, On<INLotSerialStatus.locationID, Equal<INLocation.locationID>>,
                    InnerJoin<INItemLotSerial, On<INLotSerialStatus.inventoryID, Equal<INItemLotSerial.inventoryID>, 
                        And<INLotSerialStatus.lotSerialNbr, Equal<INItemLotSerial.lotSerialNbr>>>>>,
                    Where<INLotSerialStatus.inventoryID, Equal<Required<INLotSerialStatus.inventoryID>>>,
                    Aggregate<
                        Sum<INLotSerialStatus.qtyOnHand,
                        Sum<INLotSerialStatus.qtyAvail,
                        Sum<INLotSerialStatus.qtyNotAvail,
                        Sum<INLotSerialStatus.qtyExpired,
                        Sum<INLotSerialStatus.qtyHardAvail,
                        Sum<INLotSerialStatus.qtyActual,
                        Sum<INLotSerialStatus.qtyFSSrvOrdBooked,
                        Sum<INLotSerialStatus.qtyFSSrvOrdAllocated,
                        Sum<INLotSerialStatus.qtyFSSrvOrdPrepared,
                        Sum<INLotSerialStatus.qtySOBackOrdered,
                        Sum<INLotSerialStatus.qtySOPrepared,
                        Sum<INLotSerialStatus.qtySOBooked,
                        Sum<INLotSerialStatus.qtySOShipped,
                        Sum<INLotSerialStatus.qtySOShipping,
                        Sum<INLotSerialStatus.qtyINIssues,
                        Sum<INLotSerialStatus.qtyINReceipts,
                        Sum<INLotSerialStatus.qtyInTransit,
                        Sum<INLotSerialStatus.qtyInTransitToSO,
                        Sum<INLotSerialStatus.qtyPOReceipts,
                        Sum<INLotSerialStatus.qtyPOPrepared,
                        Sum<INLotSerialStatus.qtyPOOrders,
                        Sum<INLotSerialStatus.qtyFixedFSSrvOrd,
                        Sum<INLotSerialStatus.qtyPOFixedFSSrvOrd,
                        Sum<INLotSerialStatus.qtyPOFixedFSSrvOrdPrepared,
                        Sum<INLotSerialStatus.qtyPOFixedFSSrvOrdReceipts,
                        Sum<INLotSerialStatus.qtySOFixed,
                        Sum<INLotSerialStatus.qtyPOFixedOrders,
                        Sum<INLotSerialStatus.qtyPOFixedPrepared,
                        Sum<INLotSerialStatus.qtyPOFixedReceipts,
                        Sum<INLotSerialStatus.qtySODropShip,
                        Sum<INLotSerialStatus.qtyPODropShipOrders,
                        Sum<INLotSerialStatus.qtyPODropShipPrepared,
                        Sum<INLotSerialStatus.qtyPODropShipReceipts,
                        Sum<INLotSerialStatus.qtyINAssemblySupply,
                        Sum<INLotSerialStatus.qtyINAssemblyDemand,
                        Sum<INLotSerialStatus.qtyInTransitToProduction,
                        Sum<INLotSerialStatus.qtyProductionSupplyPrepared,
                        Sum<INLotSerialStatus.qtyProductionSupply,
                        Sum<INLotSerialStatus.qtyPOFixedProductionPrepared,
                        Sum<INLotSerialStatus.qtyPOFixedProductionOrders,
                        Sum<INLotSerialStatus.qtyProductionDemandPrepared,
                        Sum<INLotSerialStatus.qtyProductionDemand,
                        Sum<INLotSerialStatus.qtyProductionAllocated,
                        Sum<INLotSerialStatus.qtySOFixedProduction,
                        Sum<INLotSerialStatus.qtyProdFixedPurchase,
                        Sum<INLotSerialStatus.qtyProdFixedProduction,
                        Sum<INLotSerialStatus.qtyProdFixedProdOrdersPrepared,
                        Sum<INLotSerialStatus.qtyProdFixedProdOrders,
                        Sum<INLotSerialStatus.qtyProdFixedSalesOrdersPrepared,
                        Sum<INLotSerialStatus.qtyProdFixedSalesOrders
                        >>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>(graph);

            var cmdParms = new List<object> { inventoryID };

            BuildLotSerialStatusCommand(ref cmd, ref cmdParms, lotSerClass, subitemID, siteID, locationID, lotSerialNbr,
                receiptsAllowed, salesAllowed, assemblyAllowed);

            var lotSerialStatus = new INLotSerialStatus
            {
                InventoryID = inventoryID,
                SubItemID = subitemID,
                SiteID = siteID,
                LocationID = locationID,
                LotSerialNbr = lotSerialNbr,
                QtyOnHand = 0m,
                QtyAvail = 0m,
                QtyNotAvail = 0m,
                QtyExpired = 0m,
                QtyHardAvail = 0m,
                QtyActual = 0m,
                QtyFSSrvOrdBooked = 0m,
                QtyFSSrvOrdAllocated = 0m,
                QtyFSSrvOrdPrepared = 0m,
                QtySOBackOrdered = 0m,
                QtySOPrepared = 0m,
                QtySOBooked = 0m,
                QtySOShipped = 0m,
                QtySOShipping = 0m,
                QtyINIssues = 0m,
                QtyINReceipts = 0m,
                QtyInTransit = 0m,
                QtyInTransitToSO = 0m,
                QtyPOReceipts = 0m,
                QtyPOPrepared = 0m,
                QtyPOOrders = 0m,
                QtyFixedFSSrvOrd = 0m,
                QtyPOFixedFSSrvOrd = 0m,
                QtyPOFixedFSSrvOrdPrepared = 0m,
                QtyPOFixedFSSrvOrdReceipts = 0m,
                QtySOFixed = 0m,
                QtyPOFixedOrders = 0m,
                QtyPOFixedPrepared = 0m,
                QtyPOFixedReceipts = 0m,
                QtySODropShip = 0m,
                QtyPODropShipOrders = 0m,
                QtyPODropShipPrepared = 0m,
                QtyPODropShipReceipts = 0m,
                QtyINAssemblySupply = 0m,
                QtyINAssemblyDemand = 0m,
                QtyInTransitToProduction = 0m,
                QtyProductionSupplyPrepared = 0m,
                QtyProductionSupply = 0m,
                QtyPOFixedProductionPrepared = 0m,
                QtyPOFixedProductionOrders = 0m,
                QtyProductionDemandPrepared = 0m,
                QtyProductionDemand = 0m,
                QtyProductionAllocated = 0m,
                QtySOFixedProduction = 0m,
                QtyProdFixedPurchase = 0m,
                QtyProdFixedProduction = 0m,
                QtyProdFixedProdOrdersPrepared = 0m,
                QtyProdFixedProdOrders = 0m,
                QtyProdFixedSalesOrdersPrepared = 0m,
                QtyProdFixedSalesOrders = 0m
            };

            foreach (PXResult<INLotSerialStatus, INLocation, INItemLotSerial> result in cmd.Select(cmdParms.ToArray()))
            {
                // we need item lot serial because hard allocations from sales order does not update lotserialstatus (only itemlotserial if allocated to a lot/serial)
                var lotSerialStatusResult = (INLotSerialStatus) result;
                var itemLotSerialResult = (INItemLotSerial) result;

                lotSerialStatus.QtyOnHand += lotSerialStatusResult.QtyOnHand.GetValueOrDefault();
                lotSerialStatus.QtyAvail += lotSerialStatusResult.QtyAvail.GetValueOrDefault();
                lotSerialStatus.QtyNotAvail += lotSerialStatusResult.QtyNotAvail.GetValueOrDefault();
                lotSerialStatus.QtyExpired += lotSerialStatusResult.QtyExpired.GetValueOrDefault();
                lotSerialStatus.QtyHardAvail += Math.Min(lotSerialStatusResult.QtyHardAvail.GetValueOrDefault(), itemLotSerialResult.QtyHardAvail.GetValueOrDefault());
                lotSerialStatus.QtyActual += lotSerialStatusResult.QtyActual.GetValueOrDefault();
                lotSerialStatus.QtyFSSrvOrdBooked += lotSerialStatusResult.QtyFSSrvOrdBooked.GetValueOrDefault();
                lotSerialStatus.QtyFSSrvOrdAllocated += lotSerialStatusResult.QtyFSSrvOrdAllocated.GetValueOrDefault();
                lotSerialStatus.QtyFSSrvOrdPrepared += lotSerialStatusResult.QtyFSSrvOrdPrepared.GetValueOrDefault();
                lotSerialStatus.QtySOBackOrdered += lotSerialStatusResult.QtySOBackOrdered.GetValueOrDefault();
                lotSerialStatus.QtySOPrepared += lotSerialStatusResult.QtySOPrepared.GetValueOrDefault();
                lotSerialStatus.QtySOBooked += lotSerialStatusResult.QtySOBooked.GetValueOrDefault();
                lotSerialStatus.QtySOShipped += lotSerialStatusResult.QtySOShipped.GetValueOrDefault();
                lotSerialStatus.QtySOShipping += lotSerialStatusResult.QtySOShipping.GetValueOrDefault();
                lotSerialStatus.QtyINIssues += lotSerialStatusResult.QtyINIssues.GetValueOrDefault();
                lotSerialStatus.QtyINReceipts += lotSerialStatusResult.QtyINReceipts.GetValueOrDefault();
                lotSerialStatus.QtyInTransit += lotSerialStatusResult.QtyInTransit.GetValueOrDefault();
                lotSerialStatus.QtyInTransitToSO += lotSerialStatusResult.QtyInTransitToSO.GetValueOrDefault();
                lotSerialStatus.QtyPOReceipts += lotSerialStatusResult.QtyPOReceipts.GetValueOrDefault();
                lotSerialStatus.QtyPOPrepared += lotSerialStatusResult.QtyPOPrepared.GetValueOrDefault();
                lotSerialStatus.QtyPOOrders += lotSerialStatusResult.QtyPOOrders.GetValueOrDefault();
                lotSerialStatus.QtyFixedFSSrvOrd += lotSerialStatusResult.QtyFixedFSSrvOrd.GetValueOrDefault();
                lotSerialStatus.QtyPOFixedFSSrvOrd += lotSerialStatusResult.QtyPOFixedFSSrvOrd.GetValueOrDefault();
                lotSerialStatus.QtyPOFixedFSSrvOrdPrepared += lotSerialStatusResult.QtyPOFixedFSSrvOrdPrepared.GetValueOrDefault();
                lotSerialStatus.QtyPOFixedFSSrvOrdReceipts += lotSerialStatusResult.QtyPOFixedFSSrvOrdReceipts.GetValueOrDefault();
                lotSerialStatus.QtySOFixed += lotSerialStatusResult.QtySOFixed.GetValueOrDefault();
                lotSerialStatus.QtyPOFixedOrders += lotSerialStatusResult.QtyPOFixedOrders.GetValueOrDefault();
                lotSerialStatus.QtyPOFixedPrepared += lotSerialStatusResult.QtyPOFixedPrepared.GetValueOrDefault();
                lotSerialStatus.QtyPOFixedReceipts += lotSerialStatusResult.QtyPOFixedReceipts.GetValueOrDefault();
                lotSerialStatus.QtySODropShip += lotSerialStatusResult.QtySODropShip.GetValueOrDefault();
                lotSerialStatus.QtyPODropShipOrders += lotSerialStatusResult.QtyPODropShipOrders.GetValueOrDefault();
                lotSerialStatus.QtyPODropShipPrepared += lotSerialStatusResult.QtyPODropShipPrepared.GetValueOrDefault();
                lotSerialStatus.QtyPODropShipReceipts += lotSerialStatusResult.QtyPODropShipReceipts.GetValueOrDefault();
                lotSerialStatus.QtyINAssemblySupply += lotSerialStatusResult.QtyINAssemblySupply.GetValueOrDefault();
                lotSerialStatus.QtyINAssemblyDemand += lotSerialStatusResult.QtyINAssemblyDemand.GetValueOrDefault();
                lotSerialStatus.QtyInTransitToProduction += lotSerialStatusResult.QtyInTransitToProduction.GetValueOrDefault();
                lotSerialStatus.QtyProductionSupplyPrepared += lotSerialStatusResult.QtyProductionSupplyPrepared.GetValueOrDefault();
                lotSerialStatus.QtyProductionSupply += lotSerialStatusResult.QtyProductionSupply.GetValueOrDefault();
                lotSerialStatus.QtyPOFixedProductionPrepared += lotSerialStatusResult.QtyPOFixedProductionPrepared.GetValueOrDefault();
                lotSerialStatus.QtyPOFixedProductionOrders += lotSerialStatusResult.QtyPOFixedProductionOrders.GetValueOrDefault();
                lotSerialStatus.QtyProductionDemandPrepared += lotSerialStatusResult.QtyProductionDemandPrepared.GetValueOrDefault();
                lotSerialStatus.QtyProductionDemand += lotSerialStatusResult.QtyProductionDemand.GetValueOrDefault();
                lotSerialStatus.QtyProductionAllocated += lotSerialStatusResult.QtyProductionAllocated.GetValueOrDefault();
                lotSerialStatus.QtySOFixedProduction += lotSerialStatusResult.QtySOFixedProduction.GetValueOrDefault();
                lotSerialStatus.QtyProdFixedPurchase += lotSerialStatusResult.QtyProdFixedPurchase.GetValueOrDefault();
                lotSerialStatus.QtyProdFixedProduction += lotSerialStatusResult.QtyProdFixedProduction.GetValueOrDefault();
                lotSerialStatus.QtyProdFixedProdOrdersPrepared += lotSerialStatusResult.QtyProdFixedProdOrdersPrepared.GetValueOrDefault();
                lotSerialStatus.QtyProdFixedProdOrders += lotSerialStatusResult.QtyProdFixedProdOrders.GetValueOrDefault();
                lotSerialStatus.QtyProdFixedSalesOrdersPrepared += lotSerialStatusResult.QtyProdFixedSalesOrdersPrepared.GetValueOrDefault();
                lotSerialStatus.QtyProdFixedSalesOrders += lotSerialStatusResult.QtyProdFixedSalesOrders.GetValueOrDefault();
            }

            return lotSerialStatus;
        }

        /// <summary>
        /// Builds the where statement for the base select for lotserial status records
        /// </summary>
        private static void BuildLotSerialStatusCommand(ref PXSelectBase<INLotSerialStatus> cmd, ref List<object> cmdParms,
            INLotSerClass lotSerClass, int? subitemID, int? siteID, int? locationID, string lotSerialNbr,
            bool? receiptsAllowed, bool? salesAllowed, bool? assemblyAllowed)
        {
            if (siteID != null)
            {
                cmd.WhereAnd<Where<INLotSerialStatus.siteID, Equal<Required<INLotSerialStatus.siteID>>>>();
                cmdParms.Add(siteID);
            }

            if (subitemID.GetValueOrDefault() != 0 && PXAccess.FeatureInstalled<FeaturesSet.subItem>())
            {
                cmd.WhereAnd<Where<INLotSerialStatus.subItemID, Equal<Required<INLotSerialStatus.subItemID>>,
                    Or<INLotSerialStatus.subItemID, IsNull>>>();
                cmdParms.Add(subitemID);
            }

            if (locationID.GetValueOrDefault() != 0 && PXAccess.FeatureInstalled<FeaturesSet.warehouseLocation>())
            {
                cmd.WhereAnd<Where<INLotSerialStatus.locationID, Equal<Required<INLotSerialStatus.locationID>>>>();
                cmdParms.Add(locationID);
            }

            if (!string.IsNullOrWhiteSpace(lotSerialNbr))
            {
                cmd.WhereAnd<Where<INLotSerialStatus.lotSerialNbr, Equal<Required<INLotSerialStatus.lotSerialNbr>>>>();
                cmdParms.Add(lotSerialNbr);
            }

            if (receiptsAllowed != null)
            {
                cmd.WhereAnd<Where<INLocation.receiptsValid, Equal<Required<INLocation.receiptsValid>>>>();
                cmdParms.Add(receiptsAllowed.GetValueOrDefault());
            }

            if (salesAllowed != null)
            {
                cmd.WhereAnd<Where<INLocation.salesValid, Equal<Required<INLocation.salesValid>>>>();
                cmdParms.Add(salesAllowed.GetValueOrDefault());
            }

            if (assemblyAllowed != null)
            {
                cmd.WhereAnd<Where<INLocation.assemblyValid, Equal<Required<INLocation.assemblyValid>>>>();
                cmdParms.Add(assemblyAllowed.GetValueOrDefault());
            }

            switch (lotSerClass.LotSerIssueMethod)
            {
                case INLotSerIssueMethod.FIFO:
                    cmd.OrderByNew<OrderBy<Asc<INLocation.pickPriority, Asc<INLotSerialStatus.receiptDate, Asc<INLotSerialStatus.lotSerialNbr>>>>>();
                    break;
                case INLotSerIssueMethod.LIFO:
                    cmd.OrderByNew<OrderBy<Asc<INLocation.pickPriority, Desc<INLotSerialStatus.receiptDate, Asc<INLotSerialStatus.lotSerialNbr>>>>>();
                    break;
                case INLotSerIssueMethod.Expiration:
                    cmd.OrderByNew<OrderBy<Asc<INLotSerialStatus.expireDate, Asc<INLocation.pickPriority, Asc<INLotSerialStatus.lotSerialNbr>>>>>();
                    break;
                case INLotSerIssueMethod.Sequential:
                    cmd.OrderByNew<OrderBy<Asc<INLocation.pickPriority, Asc<INLotSerialStatus.lotSerialNbr>>>>();
                    break;
                case INLotSerIssueMethod.UserEnterable:
                default:
                    if (string.IsNullOrWhiteSpace(lotSerialNbr))
                    {
                        cmd.WhereAnd<Where<True, Equal<False>>>();
                    }
                    break;
            }
        }

        #region Default Bin Locations
        /// <summary>
        /// Class containing Manufacturing calls for getting an items default bin location information
        /// </summary>
        public static class DfltLocation
        {
            /// <summary>
            /// Bin Default Type
            /// </summary>
            public enum BinType
            {
                /// <summary>
                /// Put Away or Receipt Default Bin Location
                /// </summary>
                Receipt,   // 0
                /// <summary>
                /// Pick or Ship Default Bin Location
                /// </summary>
                Ship    // 1
            };

            /// <summary>
            /// Get the inventory item default location (no match to items default warehouse).
            /// </summary>
            /// <param name="graph">calling graph</param>
            /// <param name="binLocationType">bin location Type to retrieve</param>
            /// <param name="inventoryID">inventory item ID</param>
            /// <param name="checkAssemblyValid">Restrict the bin location with assembly allowed option checked</param>
            /// <returns>default bin location ID</returns>
            public static int? GetInventoryDefault(PXGraph graph, BinType binLocationType, int? inventoryID, bool checkAssemblyValid)
            {
                return GetInventoryDefault(graph, binLocationType, inventoryID, null, checkAssemblyValid);
            }

            /// <summary>
            /// Get the inventory item default location.
            /// </summary>
            /// <param name="graph">calling graph</param>
            /// <param name="binLocationType">bin location Type to retrieve</param>
            /// <param name="inventoryID">inventory item ID</param>
            /// <param name="siteID">(optional) warehouse ID. When entered - the default is returned only when item has a default warehouse entered</param>
            /// <param name="checkAssemblyValid">Restrict the bin location with assembly allowed option checked</param>
            /// <returns>default bin location ID</returns>
            public static int? GetInventoryDefault(PXGraph graph, BinType binLocationType, int? inventoryID, int? siteID, bool checkAssemblyValid)
            {
                if (graph == null || inventoryID == null)
                {
                    return null;
                }

                PXSelectBase<InventoryItem> cmd = new PXSelectJoin<InventoryItem,
                            InnerJoin<INLocation, On<InventoryItem.dfltReceiptLocationID, Equal<INLocation.locationID>>>,
                            Where<InventoryItem.inventoryID, Equal<Required<InventoryItem.inventoryID>>,
                                And<INLocation.active, Equal<True>>>>(graph);

                if (binLocationType == BinType.Ship)
                {
                    cmd.WhereAnd<Where<INLocation.salesValid, Equal<True>>>();
                }
                else
                {
                    cmd.WhereAnd<Where<INLocation.receiptsValid, Equal<True>>>();
                }

                if (checkAssemblyValid)
                {
                    cmd.WhereAnd<Where<INLocation.assemblyValid, Equal<True>>>();
                }

                var item = (InventoryItem)cmd.Select(inventoryID);

                if (item != null && (siteID == null || item.DfltSiteID == siteID))
                {
                    return binLocationType == BinType.Ship
                        ? item.DfltShipLocationID
                        : item.DfltReceiptLocationID;
                }
                return null;
            }

            /// <summary>
            /// Get the item warehouse default location
            /// </summary>
            /// <param name="graph">calling graph</param>
            /// <param name="binLocationType">bin location Type to retrieve</param>
            /// <param name="inventoryID">inventory item ID</param>
            /// <param name="siteID">warehouse ID</param>
            /// <param name="checkAssemblyValid">Restrict the bin location with assembly allowed option checked</param>
            /// <returns>default bin location ID</returns>
            public static int? GetItemWarehouseDefault(PXGraph graph, BinType binLocationType, int? inventoryID, int? siteID, bool checkAssemblyValid)
            {
                if (graph == null || inventoryID == null || siteID == null)
                {
                    return null;
                }

                PXSelectBase<INItemSite> cmd = new PXSelectJoin<INItemSite,
                    InnerJoin<INLocation, On<INItemSite.dfltReceiptLocationID, Equal<INLocation.locationID>>>,
                    Where<INItemSite.inventoryID, Equal<Required<INItemSite.inventoryID>>,
                        And<INItemSite.siteID, Equal<Required<INItemSite.siteID>>,
                        And<INLocation.active, Equal<True>>>>>(graph);

                if (binLocationType == BinType.Ship)
                {
                    cmd.WhereAnd<Where<INLocation.salesValid, Equal<True>>>();
                }
                else
                {
                    cmd.WhereAnd<Where<INLocation.receiptsValid, Equal<True>>>();
                }

                if (checkAssemblyValid)
                {
                    cmd.WhereAnd<Where<INLocation.assemblyValid, Equal<True>>>();
                }

                var itemsite = (INItemSite)cmd.Select(inventoryID, siteID);

                if (itemsite != null)
                {
                    return binLocationType == BinType.Ship
                        ? itemsite.DfltShipLocationID
                        : itemsite.DfltReceiptLocationID;
                }
                return null;
            }

            /// <summary>
            /// Get a warehouse's default location.
            /// </summary>
            /// <param name="graph">calling graph</param>
            /// <param name="binLocationType">bin location Type to retrieve</param>
            /// <param name="siteID">warehouse ID</param>
            /// <param name="checkAssemblyValid">Restrict the bin location with assembly allowed option checked</param>
            /// <returns>default bin location ID</returns>
            public static int? GetWarehouseDefault(PXGraph graph, BinType binLocationType, int? siteID, bool checkAssemblyValid)
            {
                if (graph == null || siteID == null)
                {
                    return null;
                }

                PXSelectBase<INSite> cmd = new PXSelectJoin<INSite,
                    InnerJoin<INLocation, On<INSite.receiptLocationID, Equal<INLocation.locationID>>>,
                    Where<INSite.siteID, Equal<Required<INSite.siteID>>,
                        And<INLocation.active, Equal<True>>>>(graph);

                if (binLocationType == BinType.Ship)
                {
                    cmd.WhereAnd<Where<INLocation.salesValid, Equal<True>>>();
                }
                else
                {
                    cmd.WhereAnd<Where<INLocation.receiptsValid, Equal<True>>>();
                }

                if (checkAssemblyValid)
                {
                    cmd.WhereAnd<Where<INLocation.assemblyValid, Equal<True>>>();
                }

                var warehouse = (INSite)cmd.SelectWindowed(0, 1, siteID);

                if (warehouse != null)
                {
                    return binLocationType == BinType.Receipt
                        ? warehouse.ReceiptLocationID
                        : warehouse.ShipLocationID;
                }
                return null;
            }

            /// <summary>
            /// Get the first location allowed for a given warehouse.
            /// </summary>
            /// <param name="graph">calling graph</param>
            /// <param name="binLocationType">bin location Type to retrieve</param>
            /// <param name="siteID">warehouse ID</param>
            /// <param name="checkAssemblyValid">Restrict the bin location with assembly allowed option checked</param>
            /// <returns>default bin location ID</returns>
            public static int? GetFirstWarehouseLocation(PXGraph graph, BinType binLocationType, int? siteID, bool checkAssemblyValid)
            {
                if (graph == null || siteID == null)
                {
                    return null;
                }

                PXSelectBase<INLocation> cmd = new PXSelect<INLocation,
                        Where<INLocation.siteID, Equal<Required<INLocation.siteID>>,
                            And<INLocation.active, Equal<True>>>,
                        OrderBy<Asc<INLocation.pickPriority>>>(graph);

                if (binLocationType == BinType.Ship)
                {
                    cmd.WhereAnd<Where<INLocation.salesValid, Equal<True>>>();
                }
                else
                {
                    cmd.WhereAnd<Where<INLocation.receiptsValid, Equal<True>>>();
                }

                if (checkAssemblyValid)
                {
                    cmd.WhereAnd<Where<INLocation.assemblyValid, Equal<True>>>();
                }

                var inLocation = (INLocation)cmd.SelectWindowed(0, 1, siteID);

                return inLocation?.LocationID;
            }

            /// <summary>
            /// Get a set of default locations for an item & warehouse.
            /// Defaults searched: item warehouse default, item default (matching default warehouse), warehouse default
            /// </summary>
            /// <param name="graph">calling graph</param>
            /// <param name="binLocationType">bin location Type to retrieve</param>
            /// <param name="inventoryID">inventory item ID</param>
            /// <param name="siteID">warehouse ID</param>
            /// <param name="checkAssemblyValid">Restrict the bin locations with assembly allowed option checked</param>
            /// <returns>a set of unique bin location ids</returns>
            public static List<int> GetDefaults(PXGraph graph, BinType binLocationType, int? inventoryID, int? siteID, bool checkAssemblyValid)
            {
                return GetDefaults(graph, binLocationType, inventoryID, siteID, false, false, checkAssemblyValid);
            }

            /// <summary>
            /// Get a set of default locations for an item & warehouse.
            /// Defaults searched: item warehouse default, item default (matching default warehouse), warehouse default
            /// </summary>
            /// <param name="graph">calling graph</param>
            /// <param name="binLocationType">bin location Type to retrieve</param>
            /// <param name="inventoryID">inventory item ID</param>
            /// <param name="siteID">warehouse ID</param>
            /// <param name="returnAnyLocation">when true, find at least 1 location regardless of found default locations</param>
            /// <param name="returnFirst">when true, return the first found location (only 1 location needed)</param>
            /// <param name="checkAssemblyValid">Restrict the bin locations with assembly allowed option checked</param>
            /// <returns>a set of unique bin location ids</returns>
            public static List<int> GetDefaults(PXGraph graph, BinType binLocationType, int? inventoryID, int? siteID, bool returnAnyLocation, bool returnFirst, bool checkAssemblyValid)
            {
                var defaults = new List<int>();
                var hashDefaults = new HashSet<int>();

                try
                {
                    if (siteID == null || inventoryID == null)
                    {
                        return defaults;
                    }

                    var itemWarehouseDefault = GetItemWarehouseDefault(graph, binLocationType, inventoryID, siteID, checkAssemblyValid);
                    if (itemWarehouseDefault.GetValueOrDefault() != 0
                            && hashDefaults.Add(itemWarehouseDefault.GetValueOrDefault()))
                    {
                        defaults.Add(itemWarehouseDefault.GetValueOrDefault());
                        if (returnFirst)
                        {
                            return defaults;
                        }
                    }

                    var itemDefault = GetInventoryDefault(graph, binLocationType, inventoryID, siteID, checkAssemblyValid);
                    if (itemDefault.GetValueOrDefault() != 0
                            && hashDefaults.Add(itemDefault.GetValueOrDefault()))
                    {
                        defaults.Add(itemDefault.GetValueOrDefault());
                        if (returnFirst)
                        {
                            return defaults;
                        }
                    }

                    var warehouseDefault = GetWarehouseDefault(graph, binLocationType, siteID, checkAssemblyValid);
                    if (warehouseDefault.GetValueOrDefault() != 0
                            && hashDefaults.Add(warehouseDefault.GetValueOrDefault()))
                    {
                        defaults.Add(warehouseDefault.GetValueOrDefault());
                        if (returnFirst)
                        {
                            return defaults;
                        }
                    }

                    if (returnAnyLocation)
                    {
                        var firstLoc = GetFirstWarehouseLocation(graph, binLocationType, siteID, checkAssemblyValid);
                        if (firstLoc.GetValueOrDefault() != 0
                            && hashDefaults.Add(firstLoc.GetValueOrDefault()))
                        {
                            defaults.Add(firstLoc.GetValueOrDefault());
                            if (returnFirst)
                            {
                                return defaults;
                            }
                        }
                    }
                }
                catch (Exception e)
                {
                    PXTraceHelper.PxTraceException(e);
                }

                return defaults;
            }

            /// <summary>
            /// Find an items first default location.
            /// Checks [1] item warehouse [2] inventory item (matching default site) [3] warehouse default
            /// </summary>
            /// <param name="graph">calling graph</param>
            /// <param name="binLocationType">Bin type - Ship or Receipt</param>
            /// <param name="inventoryID">Inventory ID</param>
            /// <param name="siteID">Warehouse ID</param>
            /// <param name="returnAnyLocation">True: A bin location must be returned. First valid bin found is used after all defaults are checked. This helps to prevent a null return providing some type of overall default.
            /// False: Only defaults or null are returned.</param>
            /// <returns>A location ID</returns>
            public static int? GetDefault(PXGraph graph, BinType binLocationType, int? inventoryID, int? siteID, bool returnAnyLocation)
            {
                return GetDefault(graph, binLocationType, inventoryID, siteID, returnAnyLocation, false);
            }

            /// <summary>
            /// Find an items first default location.
            /// Checks [1] item warehouse [2] inventory item (matching default site) [3] warehouse default
            /// </summary>
            /// <param name="graph">calling graph</param>
            /// <param name="binLocationType">Bin type - Ship or Receipt</param>
            /// <param name="inventoryID">Inventory ID</param>
            /// <param name="siteID">Warehouse ID</param>
            /// <param name="returnAnyLocation">True: A bin location must be returned. First valid bin found is used after all defaults are checked. This helps to prevent a null return providing some type of overall default.
            /// False: Only defaults or null are returned.</param>
            /// <param name="checkAssemblyValid">Check locations only marked as assembly allowed</param>
            /// <returns>A location ID</returns>
            public static int? GetDefault(PXGraph graph, BinType binLocationType, int? inventoryID, int? siteID, bool returnAnyLocation, bool checkAssemblyValid)
            {
                var defaults = GetDefaults(graph, binLocationType, inventoryID, siteID, returnAnyLocation, true, checkAssemblyValid);

                if (defaults == null || defaults.Count == 0)
                {
                    return null;
                }

                return defaults[0];
            }
        }
        #endregion

        /// <summary>
        /// Determines the replenishment source for a specific inventory item using the InventoryID and SiteID
        /// </summary>
        public static string GetReplenishmentSource(PXGraph graph, int? inventoryID, int? siteID)
        {
            //Find the replenishment source int the INItemSite Table
            INItemSite inItemSite = PXSelect<INItemSite, Where<INItemSite.inventoryID, Equal<Required<INItemSite.inventoryID>>,
                    And<INItemSite.siteID, Equal<Required<INItemSite.siteID>>>>>.SelectWindowed(graph, 0, 1, inventoryID, siteID);

            if (inItemSite != null && !string.IsNullOrWhiteSpace(inItemSite.ReplenishmentSource))
            {
                return inItemSite.ReplenishmentSource;
            }

            InventoryItem item = PXSelect<InventoryItem,
                Where<InventoryItem.inventoryID, Equal<Required<InventoryItem.inventoryID>>>>.Select(graph, inventoryID);

            var itemExtension = item.GetExtension<InventoryItemExt>();
            if (itemExtension != null && !string.IsNullOrWhiteSpace(itemExtension.AMReplenishmentSource))
            {
                return itemExtension.AMReplenishmentSource;
            }

            return INReplenishmentSource.None;
        }

        /// <summary>
        /// Determines the replenishment source for a specific inventory item using the InventoryItem and INItemSite
        /// Also checks for default BOMID, Must have one for a Manufactured item
        /// NO Default BOMID, then consider the Item to be purchased
        /// </summary>
        public static string GetReplenishmentSource(InventoryItem invItem, INItemSite inItemSite)
        {
            var inSiteExtension = inItemSite.GetExtension<INItemSiteExt>();
            if (inItemSite != null && !string.IsNullOrWhiteSpace(inItemSite.ReplenishmentSource))
            {
                return inSiteExtension.AMBOMID != null ? INReplenishmentSource.Manufactured : INReplenishmentSource.Purchased;
            }

            var itemExtension = invItem.GetExtension<InventoryItemExt>();
            if (itemExtension != null && !string.IsNullOrWhiteSpace(itemExtension.AMReplenishmentSource))
            {
                return itemExtension.AMBOMID != null ? INReplenishmentSource.Manufactured : INReplenishmentSource.Purchased;
            }

            return INReplenishmentSource.None;
        }


        public static decimal ReorderQuantity(decimal orderQty, decimal? minQty, decimal? maxQty, decimal? lotSize)
        {
            decimal reorderQty = orderQty;

            if (reorderQty < 0)
            {
                return reorderQty;
            }

            decimal minQtyDecimal = (minQty ?? 0) <= 0 ? 0 : minQty ?? 0;
            decimal maxQtyDecimal = (maxQty ?? 0) <= 0 ? 0 : maxQty ?? 0;
            decimal lotSizeDecimal = (lotSize ?? 0) <= 0 ? 0 : lotSize ?? 0;

            if (reorderQty < minQtyDecimal && minQtyDecimal > 0)
            {
                // APPLY MIN First
                reorderQty = minQtyDecimal;
            }

            // APPLY LOT SIZE Second
            reorderQty = lotSizeDecimal <= 0 ? reorderQty : Math.Ceiling(reorderQty / lotSizeDecimal) * lotSizeDecimal;

            decimal maxByLotQtyDecimal = lotSizeDecimal <= 0 ? maxQtyDecimal : Math.Floor(maxQtyDecimal / lotSizeDecimal) * lotSizeDecimal;

            if (reorderQty > maxByLotQtyDecimal && maxByLotQtyDecimal > 0)
            {
                // APPLY MAX Last
                reorderQty = maxByLotQtyDecimal;
            }

            return UomHelper.QuantityRound(reorderQty);
        }

        public static bool MakeItemSiteByItem(PXGraph graph, int? inventoryID, int? siteID, out INItemSite inItemSite)
        {
            if (graph == null || inventoryID == null)
            {
                inItemSite = null;
                return false;
            }

            InventoryItem inventoryItem =
                PXSelect<InventoryItem, Where<InventoryItem.inventoryID, Equal<Required<InventoryItem.inventoryID>>>>
                    .Select(graph, inventoryID);

            return MakeItemSiteByItem(graph, inventoryItem, siteID, out inItemSite);
        }

        public static bool MakeItemSiteByItem(PXGraph graph, InventoryItem inventoryItem, int? siteID, out INItemSite inItemSite)
        {
            if (graph == null
                || inventoryItem == null
                || !inventoryItem.StkItem.GetValueOrDefault()
                || inventoryItem.InventoryID == null
                || siteID == null)
            {
                inItemSite = null;
                return false;
            }

            INSite site = PXSelectReadonly<INSite, Where<INSite.siteID, Equal<Required<INSite.siteID>>>>.Select(graph, siteID);

            if (site == null)
            {
                inItemSite = null;
                return false;
            }

            inItemSite = PXSelectReadonly<INItemSite,
                Where<INItemSite.inventoryID, Equal<Required<INItemSite.inventoryID>>,
                    And<INItemSite.siteID, Equal<Required<INItemSite.siteID>>>>>.Select(graph, inventoryItem.InventoryID, site.SiteID);

            if (inItemSite == null)
            {
                INPostClass postclass =
                    PXSelect<INPostClass, Where<INPostClass.postClassID, Equal<Required<INPostClass.postClassID>>>>
                        .Select(graph, inventoryItem.PostClassID);

                if (postclass == null)
                {
                    return false;
                }

                inItemSite = new INItemSite
                {
                    InventoryID = inventoryItem.InventoryID,
                    SiteID = site.SiteID
                };

                INItemSiteMaint.DefaultItemSiteByItem(graph, inItemSite, inventoryItem, site, postclass);

                DefaultItemSiteManufacturing(graph, inventoryItem, inItemSite);

                return true;
            }

            return false;
        }

        /// <summary>
        /// Default Manufacturing values from InventoryItem to INItemSite
        /// </summary>
        public static bool DefaultItemSiteManufacturing(PXGraph graph, InventoryItem inventoryItem, INItemSite inItemSite)
        {
            if (graph == null || inventoryItem == null || inItemSite == null)
            {
                return false;
            }

            InventoryItemExt inventoryItemExt = PXCache<InventoryItem>.GetExtension<InventoryItemExt>(inventoryItem);
            if (inventoryItemExt == null)
            {
                return false;
            }

            INItemSiteExt inItemSiteExt = PXCache<INItemSite>.GetExtension<INItemSiteExt>(inItemSite);
            if (inItemSiteExt == null)
            {
                return false;
            }

            //ready
            inItemSiteExt.MapAMExtensionFields(inventoryItemExt);
            
            return true;
        }

        public static string GetbaseUOM(PXGraph graph, int? inventoryid)
        {
            InventoryItem inventoryItem = PXSelect<InventoryItem, Where<InventoryItem.inventoryID, Equal<Required<InventoryItem.inventoryID>>>>
                .Select(graph, inventoryid);
            if (inventoryItem != null)
            {
                return inventoryItem.BaseUnit;
            }

            return String.Empty;

        }

        /// <summary>
        /// Get the INSubItem row for a given sub item segment CD value
        /// </summary>
        /// <param name="graph">calling graph</param>
        /// <param name="subitemCD">Sub item CD Segment value</param>
        /// <param name="createIfNotFound">Should the call create an INSubItem record on the fly if not found</param>
        /// <returns>Found (or Created) INSubItem row</returns>
        public static INSubItem GetSubItem(PXGraph graph, string subitemCD, bool createIfNotFound = false)
        {
            if (graph == null || string.IsNullOrWhiteSpace(subitemCD))
            {
                return null;
            }

            INSubItem inSubItem = PXSelect<INSubItem,
                Where<INSubItem.subItemCD, Equal<Required<INSubItem.subItemCD>>>>.Select(graph, subitemCD);

            if (inSubItem != null)
            {
                return inSubItem;
            }

            if (createIfNotFound && TryCreateSubItem(graph, subitemCD, out inSubItem))
            {
                return inSubItem;
            }

            return null;
        }

        /// <summary>
        /// Indicates if the Lot Serial Tracking feature is enabled
        /// </summary>
        public static bool LotSerialTrackingFeatureEnabled => PXAccess.FeatureInstalled<FeaturesSet.lotSerialTracking>();

        /// <summary>
        /// Indicates if the Multiple Warehouse feature is enabled
        /// </summary>
        public static bool MultiWarehousesFeatureEnabled => PXAccess.FeatureInstalled<FeaturesSet.warehouse>();

        /// <summary>
        /// Indicates if the Multiple Warehouse Locations feature is enabled
        /// </summary>
        public static bool MultiWarehouseLocationFeatureEnabled => PXAccess.FeatureInstalled<FeaturesSet.warehouseLocation>();

        /// <summary>
        /// Indicates if the sub item feature is turned on (return = True)
        /// </summary>
        public static bool SubItemFeatureEnabled => PXAccess.FeatureInstalled<FeaturesSet.subItem>();

        /// <summary>
        /// MYOB - "Basic Inventory Replenishments" feature.
        /// Indicates is this feature is enabled/turned on
        /// </summary>
        public static bool BasicReplenishmentsEnabled => OEMHelper.FeatureInstalled(OEMHelper.MYOBFeatures.BasicInvReplenish);

        /// <summary>
        /// Indicates if the full replenishment feature is enabled/turned on
        /// </summary>
        public static bool FullReplenishmentsEnabled => PXAccess.FeatureInstalled<FeaturesSet.replenishment>();

        /// <summary>
        /// Try to create a sub item record if one does not exist for the entered sub item CD value
        /// </summary>
        /// <param name="subitemCD">Sub item CD Segment value</param>
        /// <param name="inSubItem">if created the new INSubItem row</param>
        /// <returns>True if a row was created, otherwise false</returns>
        private static bool TryCreateSubItem(PXGraph graph, string subitemCD, out INSubItem inSubItem)
        {
            inSubItem = null;

            if (!SubItemFeatureEnabled || string.IsNullOrWhiteSpace(subitemCD))
            {
                return false;
            }

            try
            {
                graph.Views.Caches.Add(typeof(INSubItem));

                inSubItem = PXSelect<INSubItem,
                    Where<INSubItem.subItemCD, Equal<Required<INSubItem.subItemCD>>>>.Select(graph, subitemCD);

                if (inSubItem != null)
                {
                    return false;
                }

                inSubItem = new INSubItem() { SubItemCD = subitemCD };

                graph.Caches[typeof(INSubItem)].Insert(inSubItem);

                graph.Persist();

                inSubItem = PXSelect<INSubItem,
                    Where<INSubItem.subItemCD, Equal<Required<INSubItem.subItemCD>>>>.Select(graph, subitemCD);

                if (inSubItem != null)
                {
                    PXTrace.WriteInformation(Messages.GetLocal(Messages.CreatedNewINSubItemRecord), subitemCD);
                    return true;
                }
            }
            catch (PXSetPropertyException)
            {
                //typically a bad subitem entered segment so lets toss that one up the chain
                throw;
            }
            catch (Exception e)
            {
                PXTraceHelper.PxTraceException(e);
            }

            return false;
        }

        public static string GetINDocTypeDescription(string inDocType)
        {
            switch (inDocType.TrimIfNotNullEmpty())
            {
                case INDocType.Adjustment:
                    return PX.Objects.IN.Messages.Adjustment;
                case INDocType.Issue:
                    return PX.Objects.IN.Messages.Issue;
                case INDocType.Receipt:
                    return PX.Objects.IN.Messages.Receipt;
                case INDocType.Transfer:
                    return PX.Objects.IN.Messages.Transfer;
                default:
                    return PX.Objects.IN.Messages.Undefined;

            }
        }
        
        public static string GetPurchaseOrderUnit(PXGraph graph, int? inventoryID, int? vendorID, string uom)
        {
            if (inventoryID == null)
            {
                return string.Empty;
            }

            //PO Unit by vendor
            if ((vendorID ?? 0) != 0)
            {
                POVendorInventory vendorInventory = PXSelect<POVendorInventory,
                    Where<POVendorInventory.vendorID, Equal<Required<POVendorInventory.vendorID>>,
                        And<POVendorInventory.inventoryID, Equal<Required<POVendorInventory.inventoryID>>>
                    >>.Select(graph, vendorID, inventoryID);

                if (vendorInventory != null)
                {
                    if (!string.IsNullOrWhiteSpace(vendorInventory.PurchaseUnit))
                    {
                        return vendorInventory.PurchaseUnit;
                    }
                }
            }

            //PO Unit by item (not vendor specific)
            InventoryItem inventoryItem = PXSelect<InventoryItem, Where<InventoryItem.inventoryID, Equal<Required<InventoryItem.inventoryID>>>>
                .Select(graph, inventoryID);

            if (inventoryItem != null)
            {
                if (!string.IsNullOrWhiteSpace(inventoryItem.PurchaseUnit))
                {
                    return inventoryItem.PurchaseUnit;
                }
            }

            return uom;
        }

        public static decimal ConvertToBaseUnits(PXGraph graph, POVendorInventory poVendorInventory, decimal qty)
        {
            if (UomHelper.TryConvertToBaseQty<POVendorInventory.inventoryID>(graph.Caches[typeof(POVendorInventory)],
                poVendorInventory, poVendorInventory.PurchaseUnit, qty, out var convertedUnits))
            {
                return convertedUnits.GetValueOrDefault();
            }

            return qty;
        }

        public static InventoryAllocDetEnq GetInventoryAllocDetEnq<InventoryIDField, SiteIDField, SubItemIDField>(PXCache cache, object currentRow)
            where InventoryIDField : IBqlField
            where SiteIDField : IBqlField
            where SubItemIDField : IBqlField
        {
            var allocGraph = PXGraph.CreateInstance<InventoryAllocDetEnq>();
            if (currentRow == null)
            {
                return null;
            }

            allocGraph.Filter.Current.InventoryID = (int?)cache.GetValue<InventoryIDField>(currentRow);
            if (allocGraph.Filter.Current.InventoryID == null)
            {
                return null;
            }

            if (InventoryHelper.SubItemFeatureEnabled)
            {
                var subItem = (INSubItem)PXSelectorAttribute.Select<SubItemIDField>(cache, currentRow);
                if (!string.IsNullOrWhiteSpace(subItem?.SubItemCD))
                {
                    allocGraph.Filter.Current.SubItemCD = subItem.SubItemCD;
                }
            }

            allocGraph.Filter.Current.SiteID = (int?)cache.GetValue<SiteIDField>(currentRow);
            allocGraph.RefreshTotal.Press();
            return allocGraph;
        }

        public static int GetTransferLeadTime(PXGraph graph, int? inventoryID, int? fromSiteID)
        {
            INItemClassRep firstItemClassRep = null;

            foreach (PXResult<INItemRep, InventoryItem, INItemClassRep> result in PXSelectJoin<INItemRep,
                InnerJoin<InventoryItem,
                    On<INItemRep.inventoryID, Equal<InventoryItem.inventoryID>>,
                InnerJoin<INItemClassRep,
                    On<INItemClassRep.itemClassID, Equal<InventoryItem.itemClassID>,
                        And<INItemClassRep.replenishmentClassID, Equal<INItemRep.replenishmentClassID>>>>>,
                Where<INItemRep.inventoryID, Equal<Required<INItemRep.inventoryID>>>
                >.Select(graph, inventoryID))
            {
                var itemRep = (INItemRep)result;
                var itemClassRep = (INItemClassRep)result;

                if (itemRep.ReplenishmentSource != INReplenishmentSource.Transfer)
                {
                    continue;
                }

                if (itemRep.ReplenishmentSourceSiteID == fromSiteID)
                {
                    firstItemClassRep = itemClassRep;
                    break;
                }

                firstItemClassRep = itemClassRep;
            }

            return firstItemClassRep?.TransferLeadTime ?? 0;
        }
    }
}
