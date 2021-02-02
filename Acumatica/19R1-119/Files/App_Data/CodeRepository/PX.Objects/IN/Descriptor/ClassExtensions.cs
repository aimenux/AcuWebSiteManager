namespace PX.Objects.IN
{
	public static class ClassExtensions
	{
		public static bool IsZero(this IStatus a)
		{
			return (a.QtyAvail == 0m)
			  && (a.QtyHardAvail == 0m)
			  && (a.QtyActual == 0m)
			  && (a.QtyOnHand == 0m)
			  && (a.QtyFSSrvOrdPrepared == 0m)
			  && (a.QtyFSSrvOrdBooked == 0m)
			  && (a.QtyFSSrvOrdAllocated == 0m)
			  && (a.QtySOPrepared == 0m)
			  && (a.QtySOBooked == 0m)
			  && (a.QtySOShipping == 0m)
			  && (a.QtySOShipped == 0m)
			  && (a.QtySOBackOrdered == 0m)
			  && (a.QtyINIssues == 0m)
			  && (a.QtyINReceipts == 0m)
			  && (a.QtyInTransit == 0m)
			  && (a.QtyInTransitToSO == 0m)
			  && (a.QtyPOPrepared == 0m)
			  && (a.QtyPOOrders == 0m)
			  && (a.QtyPOReceipts == 0m)
			  && (a.QtyINAssemblyDemand == 0m)
			  && (a.QtyINAssemblySupply == 0m)
			  && (a.QtyInTransitToProduction == 0m)
			  && (a.QtyProductionSupplyPrepared == 0m)
			  && (a.QtyProductionSupply == 0m)
			  && (a.QtyPOFixedProductionPrepared == 0m)
			  && (a.QtyPOFixedProductionOrders == 0m)
			  && (a.QtyProductionDemandPrepared == 0m)
			  && (a.QtyProductionDemand == 0m)
			  && (a.QtyProductionAllocated == 0m)
			  && (a.QtySOFixedProduction == 0m)
			  && (a.QtyProdFixedPurchase == 0m)
			  && (a.QtyProdFixedProduction == 0m)
			  && (a.QtyProdFixedProdOrdersPrepared == 0m)
			  && (a.QtyProdFixedProdOrders == 0m)
			  && (a.QtyProdFixedSalesOrdersPrepared == 0m)
			  && (a.QtyProdFixedSalesOrders == 0m)
			  && (a.QtyFixedFSSrvOrd == 0m)
			  && (a.QtyPOFixedFSSrvOrd == 0m)
			  && (a.QtyPOFixedFSSrvOrdPrepared == 0m)
			  && (a.QtyPOFixedFSSrvOrdReceipts == 0m)
			  && (a.QtySOFixed == 0m)
			  && (a.QtyPOFixedOrders == 0m)
			  && (a.QtyPOFixedPrepared == 0m)
			  && (a.QtyPOFixedReceipts == 0m)
			  && (a.QtySODropShip == 0m)
			  && (a.QtyPODropShipOrders == 0m)
			  && (a.QtyPODropShipPrepared == 0m)
			  && (a.QtyPODropShipReceipts == 0m);
		}

		public static bool IsZero(this Overrides.INDocumentRelease.ItemLotSerial a)
		{
			return (a.QtyOnHand == 0m)
			  && (a.QtyAvail == 0m)
			  && (a.QtyHardAvail == 0m)
			  && (a.QtyActual == 0m)
			  && (a.QtyNotAvail == 0m)
			  && (a.QtyInTransit == 0m)
			  && (a.QtyOnReceipt == 0m)
			  && (a.UpdateExpireDate != true);
		}

		public static bool IsZero(this Overrides.INDocumentRelease.SiteLotSerial a)
		{
			return (a.QtyOnHand == 0m)
			  && (a.QtyAvail == 0m)
			  && (a.QtyHardAvail == 0m)
			  && (a.QtyActual == 0m)
			  && (a.QtyNotAvail == 0m)
			  && (a.QtyInTransit == 0m)
			  && (a.UpdateExpireDate != true);
		}
	}
}
