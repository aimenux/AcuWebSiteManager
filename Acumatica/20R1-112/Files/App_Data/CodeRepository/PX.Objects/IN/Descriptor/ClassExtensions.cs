using System;
using System.Collections.Generic;

namespace PX.Objects.IN
{
	public static class ClassExtensions
	{
		#region InventoryUnitType extension

		internal static InventoryUnitType ToUnitTypes(this InventoryItem inventoryItem, string unit)
		{
			var purpose = InventoryUnitType.None;
			if (inventoryItem.BaseUnit == unit)
				purpose |= InventoryUnitType.BaseUnit;
			if (inventoryItem.SalesUnit == unit)
				purpose |= InventoryUnitType.SalesUnit;
			if (inventoryItem.PurchaseUnit == unit)
				purpose |= InventoryUnitType.PurchaseUnit;
			return purpose;
		}

		internal static InventoryUnitType ToIntegerUnits(this InventoryItem inventoryItem)
		{
			var purpose = InventoryUnitType.None;
			if (inventoryItem.DecimalBaseUnit == false)
				purpose |= InventoryUnitType.BaseUnit;
			if (inventoryItem.DecimalSalesUnit == false)
				purpose |= InventoryUnitType.SalesUnit;
			if (inventoryItem.DecimalPurchaseUnit == false)
				purpose |= InventoryUnitType.PurchaseUnit;
			return purpose;
		}

		internal static string GetUnitID(this InventoryItem inventoryItem, InventoryUnitType inventoryUnitType)
		{
			switch(inventoryUnitType)
			{
				case InventoryUnitType.None:
					return null;
				case InventoryUnitType.BaseUnit:
					return inventoryItem.BaseUnit;
				case InventoryUnitType.SalesUnit:
					return inventoryItem.SalesUnit;
				case InventoryUnitType.PurchaseUnit:
					return inventoryItem.PurchaseUnit;
				default:
					throw new ArgumentOutOfRangeException(nameof(inventoryUnitType));
			}
		}

		/// <summary>
		/// Is equivalent to Enum.GetValues(typeof(InventoryUnitType)) without default value
		/// </summary>
		/// <param name="unitType"></param>
		/// <returns></returns>
		internal static IEnumerable<InventoryUnitType> Split(this InventoryUnitType unitType)
		{
			if ((unitType & InventoryUnitType.PurchaseUnit) != 0)
				yield return InventoryUnitType.PurchaseUnit;
			if ((unitType & InventoryUnitType.SalesUnit) != 0)
				yield return InventoryUnitType.SalesUnit;
			if ((unitType & InventoryUnitType.BaseUnit) != 0)
				yield return InventoryUnitType.BaseUnit;
		}

		#endregion

		public static bool IsZero(this IStatus a)
		{
			return (a.QtyAvail == 0m)
			  && (a.QtyHardAvail == 0m)
			  && (a.QtyActual == 0m)
			  && (a.QtyNotAvail == 0m)
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
			  && (a.QtyINReplaned == 0m)
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
