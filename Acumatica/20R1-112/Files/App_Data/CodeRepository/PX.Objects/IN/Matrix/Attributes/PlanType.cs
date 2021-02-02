using PX.Common;
using PX.Data;
using PX.Objects.CS;
using System.Collections.Generic;

namespace PX.Objects.IN.Matrix.Attributes
{
	public class PlanType
	{
		[PXLocalizable]
		public class UI
		{
			public const string Available = "Available";
			public const string AvailableforShipment = "Available for Shipment";
			public const string NotAvailable = "Not Available";
			public const string SOBooked = "SO Booked";
			public const string SOShipping = "SO Shipping";
			public const string SOShipped = "SO Shipped";
			public const string SOBackOrdered = "SO Back Ordered";
			public const string INIssues = "IN Issues";
			public const string INReceipts = "IN Receipts";
			public const string InTransit = "In Transit";
			public const string InAssemblyDemand = "Kit Assembly Demand";
			public const string InAssemblySupply = "Kit Assembly Supply";
			public const string PurchasePrepared = "Purchase Prepared";
			public const string PurchaseOrders = "Purchase Orders";
			public const string POReceipts = "PO Receipts";
			public const string Expired = "Expired";
			public const string OnHand = "On Hand";
			public const string SOtoPurchase = "SO to Purchase";
			public const string PurchaseforSO = "Purchase for SO";
			public const string PurchaseforSOPrepared = "Purchase for SO Prepared";
			public const string PurchaseReceiptsForSO = "Receipts for SO";
			public const string SOtoDropShip = "SO to Drop-Ship";
			public const string DropShipforSO = "Drop-Ship for SO";
			public const string DropShipforSOPrepared = "Drop-Ship for SO Prepared";
			public const string DropShipforSOReceipts = "Drop-Ship for SO Receipts";

			public const string FSSrvOrdPrepared = "FS Prepared";
			public const string FSSrvOrdBooked = "FS Booked";
			public const string FSSrvOrdAllocated = "FS Allocated";
			public const string FixedFSSrvOrd = "FS to Purchase";
			public const string POFixedFSSrvOrd = "Purchase for FS";
			public const string POFixedFSSrvOrdPrepared = "Purchase for FS Prepared";
			public const string POFixedFSSrvOrdReceipts = "Receipts for FS";

			public const string InTransitToProduction = "In Transit to Production";
			public const string ProductionSupplyPrepared = "Production Supply Prepared";
			public const string ProductionSupply = "Production Supply";
			public const string POFixedProductionPrepared = "Purchase for Prod. Prepared";
			public const string POFixedProductionOrders = "Purchase for Production";
			public const string ProductionDemandPrepared = "Production Demand Prepared";
			public const string ProductionDemand = "Production Demand";
			public const string ProductionAllocated = "Production Allocated";
			public const string SOFixedProduction = "SO to Production";
			public const string ProdFixedPurchase = "Production to Purchase";
			public const string ProdFixedProduction = "Production to Production";
			public const string ProdFixedProdOrdersPrepared = "Production for Prod. Prepared";
			public const string ProdFixedProdOrders = "Production for Production";
			public const string ProdFixedSalesOrdersPrepared = "Production for SO Prepared";
			public const string ProdFixedSalesOrders = "Production for SO";
		}

		public const string Available = "AA";
		public const string AvailableforShipment = "AS";
		public const string NotAvailable = "NA";
		public const string SOBooked = "SB";
		public const string SOShipping = "SS";
		public const string SOShipped = "SD";
		public const string SOBackOrdered = "SO";
		public const string INIssues = "II";
		public const string INReceipts = "IR";
		public const string InTransit = "IT";
		public const string InAssemblyDemand = "IA";
		public const string InAssemblySupply = "IS";
		public const string PurchasePrepared = "PP";
		public const string PurchaseOrders = "PO";
		public const string POReceipts = "PR";
		public const string Expired = "EX";
		public const string OnHand = "OH";
		public const string SOtoPurchase = "SP";
		public const string PurchaseforSO = "PS";
		public const string PurchaseforSOPrepared = "PU";
		public const string PurchaseReceiptsForSO = "PC";
		public const string SOtoDropShip = "DS";
		public const string DropShipforSO = "DH";
		public const string DropShipforSOPrepared = "DI";
		public const string DropShipforSOReceipts = "DP";

		public const string FSSrvOrdPrepared = "FP";
		public const string FSSrvOrdBooked = "FB";
		public const string FSSrvOrdAllocated = "FA";
		public const string FixedFSSrvOrd = "FF";
		public const string POFixedFSSrvOrd = "FO";
		public const string POFixedFSSrvOrdPrepared = "FW";
		public const string POFixedFSSrvOrdReceipts = "FR";

		public const string InTransitToProduction = "PT";
		public const string ProductionSupplyPrepared = "PZ";
		public const string ProductionSupply = "PX";
		public const string POFixedProductionPrepared = "PF";
		public const string POFixedProductionOrders = "PQ";
		public const string ProductionDemandPrepared = "PW";
		public const string ProductionDemand = "PD";
		public const string ProductionAllocated = "PA";
		public const string SOFixedProduction = "PE";
		public const string ProdFixedPurchase = "PJ";
		public const string ProdFixedProduction = "PY";
		public const string ProdFixedProdOrdersPrepared = "PN";
		public const string ProdFixedProdOrders = "PL";
		public const string ProdFixedSalesOrdersPrepared = "PG";
		public const string ProdFixedSalesOrders = "PH";

		public class ListAttribute : PXStringListAttribute
		{
			public ListAttribute()
			{
				RefillLabels();
			}

			public virtual void RefillLabels()
			{
				_AllowedValues = new string[]
				{
					Available,
					AvailableforShipment,
					NotAvailable,
					SOBooked,
					SOShipping,
					SOShipped,
					SOBackOrdered,
					INIssues,
					INReceipts,
					InTransit,
					InAssemblyDemand,
					InAssemblySupply,
					PurchasePrepared,
					PurchaseOrders,
					POReceipts,
					Expired,
					OnHand,
					SOtoPurchase,
					PurchaseforSO,
					PurchaseforSOPrepared,
					PurchaseReceiptsForSO,
					SOtoDropShip,
					DropShipforSO,
					DropShipforSOPrepared,
					DropShipforSOReceipts
				};
				_AllowedLabels = new string[]
				{
					UI.Available,
					UI.AvailableforShipment,
					UI.NotAvailable,
					UI.SOBooked,
					UI.SOShipping,
					UI.SOShipped,
					UI.SOBackOrdered,
					UI.INIssues,
					UI.INReceipts,
					UI.InTransit,
					UI.InAssemblyDemand,
					UI.InAssemblySupply,
					UI.PurchasePrepared,
					UI.PurchaseOrders,
					UI.POReceipts,
					UI.Expired,
					UI.OnHand,
					UI.SOtoPurchase,
					UI.PurchaseforSO,
					UI.PurchaseforSOPrepared,
					UI.PurchaseReceiptsForSO,
					UI.SOtoDropShip,
					UI.DropShipforSO,
					UI.DropShipforSOPrepared,
					UI.DropShipforSOReceipts
				};

				if (PXAccess.FeatureInstalled<FeaturesSet.serviceManagementModule>())
				{
					_AllowedValues = _AllowedValues.Append(
						FSSrvOrdPrepared,
						FSSrvOrdBooked,
						FSSrvOrdAllocated,
						FixedFSSrvOrd,
						POFixedFSSrvOrd,
						POFixedFSSrvOrdPrepared,
						POFixedFSSrvOrdReceipts);
					_AllowedLabels = _AllowedLabels.Append(
						UI.FSSrvOrdPrepared,
						UI.FSSrvOrdBooked,
						UI.FSSrvOrdAllocated,
						UI.FixedFSSrvOrd,
						UI.POFixedFSSrvOrd,
						UI.POFixedFSSrvOrdPrepared,
						UI.POFixedFSSrvOrdReceipts);
				}
				if (PXAccess.FeatureInstalled<FeaturesSet.manufacturing>())
				{
					_AllowedValues = _AllowedValues.Append(
						InTransitToProduction,
						ProductionSupplyPrepared,
						ProductionSupply,
						POFixedProductionPrepared,
						POFixedProductionOrders,
						ProductionDemandPrepared,
						ProductionDemand,
						ProductionAllocated,
						SOFixedProduction,
						ProdFixedPurchase,
						ProdFixedProduction,
						ProdFixedProdOrdersPrepared,
						ProdFixedProdOrders,
						ProdFixedSalesOrdersPrepared,
						ProdFixedSalesOrders);
					_AllowedLabels = _AllowedLabels.Append(
						UI.InTransitToProduction,
						UI.ProductionSupplyPrepared,
						UI.ProductionSupply,
						UI.POFixedProductionPrepared,
						UI.POFixedProductionOrders,
						UI.ProductionDemandPrepared,
						UI.ProductionDemand,
						UI.ProductionAllocated,
						UI.SOFixedProduction,
						UI.ProdFixedPurchase,
						UI.ProdFixedProduction,
						UI.ProdFixedProdOrdersPrepared,
						UI.ProdFixedProdOrders,
						UI.ProdFixedSalesOrdersPrepared,
						UI.ProdFixedSalesOrders);
				}
			}
		}
	}
}
