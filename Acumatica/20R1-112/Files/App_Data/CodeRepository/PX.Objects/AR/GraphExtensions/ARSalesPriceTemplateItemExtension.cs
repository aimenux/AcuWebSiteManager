using System;
using System.Linq;
using PX.Common;
using PX.Data;
using PX.Objects.Common;
using PX.Objects.IN;
using static PX.Objects.AR.ARSalesPriceMaint;

namespace PX.Objects.AR
{
	public class ARSalesPriceMaintTemplateItemExtension : PXGraphExtension<ARSalesPriceMaint>
	{
		public static bool IsActive() => PXAccess.FeatureInstalled<PX.Objects.CS.FeaturesSet.matrixItem>();

		public static int? GetTemplateInventoryID(PXCache sender, int? inventoryID)
		{
			return InventoryItem.PK.Find(sender.Graph, inventoryID)?.TemplateItemID;
		}

		public delegate SalesPriceItem FindSalesPriceOrig(PXCache sender, string custPriceClass, int? customerID, int? inventoryID, int? siteID, string baseCuryID, string curyID, decimal? quantity, string UOM, DateTime date, bool isFairValue);
		[PXOverride]
		public virtual SalesPriceItem FindSalesPrice(PXCache sender, string custPriceClass, int? customerID, int? inventoryID, int? siteID, string baseCuryID, string curyID, decimal? quantity, string UOM, DateTime date, bool isFairValue, FindSalesPriceOrig baseMethod)
		{
			var priceListExists = RecordExistsSlot<ARSalesPrice, ARSalesPrice.recordID>.IsRowsExists();
			if (!priceListExists)
				return isFairValue ? null : Base.SelectDefaultItemPrice(sender, inventoryID, baseCuryID);

			var salesPriceSelect =
				new SalesPriceSelectWithTemplateItem(sender, inventoryID, UOM, (decimal)quantity, isFairValue)
				{
					CustomerID = customerID,
					CustPriceClass = custPriceClass,
					CuryID = curyID,
					SiteID = siteID,
					Date = date,
				};

			SalesPriceForCurrentUOMWithTemplateItem priceForCurrentUOM = salesPriceSelect.ForCurrentUOM();
			SalesPriceForBaseUOMWithTemplateItem priceForBaseUOM = salesPriceSelect.ForBaseUOM();
			SalesPriceForSalesUOMWithTemplateItem priceForSalesUOM = salesPriceSelect.ForSalesUOM();

			return priceForCurrentUOM.SelectCustomerPrice()
				?? priceForBaseUOM.SelectCustomerPrice()
				?? priceForCurrentUOM.SelectBasePrice()
				?? priceForBaseUOM.SelectBasePrice()
				?? (isFairValue ? null : Base.SelectDefaultItemPrice(sender, inventoryID, baseCuryID))
				?? (isFairValue ? null : Base.SelectDefaultItemPrice(sender, GetTemplateInventoryID(sender, inventoryID), baseCuryID))
				?? priceForSalesUOM.SelectCustomerPrice()
				?? priceForSalesUOM.SelectBasePrice();
		}

		internal class SalesPriceSelectWithTemplateItem : SalesPriceSelect
		{
			public SalesPriceSelectWithTemplateItem(PXCache cache, int? inventoryID, string uom, decimal qty, bool isFairValue) : base(cache, inventoryID, uom, qty, isFairValue)
			{
			}

			#region Factories
			public new SalesPriceForCurrentUOMWithTemplateItem ForCurrentUOM() => new SalesPriceForCurrentUOMWithTemplateItem(Cache, InventoryID, UOM, Qty) { CustomerID = CustomerID, CustPriceClass = CustPriceClass, CuryID = CuryID, SiteID = SiteID, Date = Date, IsFairValue = IsFairValue };
			public new SalesPriceForBaseUOMWithTemplateItem ForBaseUOM() => new SalesPriceForBaseUOMWithTemplateItem(Cache, InventoryID, UOM, Qty) { CustomerID = CustomerID, CustPriceClass = CustPriceClass, CuryID = CuryID, SiteID = SiteID, Date = Date, IsFairValue = IsFairValue };
			public new SalesPriceForSalesUOMWithTemplateItem ForSalesUOM() => new SalesPriceForSalesUOMWithTemplateItem(Cache, InventoryID, UOM, Qty) { CustomerID = CustomerID, CustPriceClass = CustPriceClass, CuryID = CuryID, SiteID = SiteID, Date = Date, IsFairValue = IsFairValue };
			#endregion
		}

		internal class SalesPriceForCurrentUOMWithTemplateItem : SalesPriceForCurrentUOM
		{
			public SalesPriceForCurrentUOMWithTemplateItem(PXCache cache, int? inventoryID, string uom, decimal qty) : base(cache, inventoryID, uom, qty)
			{
				SelectCommand.Join<InnerJoin<InventoryItem,
						On<InventoryItem.inventoryID, Equal<ARSalesPrice.inventoryID>>>>();

				SelectCommand.OrderByNew<OrderBy<Desc<ARSalesPrice.isPromotionalPrice,
							Desc<ARSalesPrice.siteID,
							Asc<InventoryItem.isTemplate,
							Desc <ARSalesPrice.breakQty>>>>>>();
			}

			public override int?[] GetInventoryIDs(PXCache sender, int? inventoryID)
			{
				return new int?[] { inventoryID, GetTemplateInventoryID(sender, inventoryID) };
			}
		}

		internal class SalesPriceForBaseUOMWithTemplateItem : SalesPriceForBaseUOM
		{
			public SalesPriceForBaseUOMWithTemplateItem(PXCache cache, int? inventoryID, string uom, decimal qty) : base(cache, inventoryID, uom, qty)
			{
				SelectCommand.OrderByNew<OrderBy<Desc<ARSalesPrice.isPromotionalPrice,
							Desc<ARSalesPrice.siteID,
							Asc<InventoryItem.isTemplate,
							Desc <ARSalesPrice.breakQty>>>>>>();
			}

			public override int?[] GetInventoryIDs(PXCache sender, int? inventoryID)
			{
				return new int?[] { inventoryID, GetTemplateInventoryID(sender, inventoryID) };
			}
		}

		internal class SalesPriceForSalesUOMWithTemplateItem : SalesPriceForSalesUOM
		{
			public SalesPriceForSalesUOMWithTemplateItem(PXCache cache, int? inventoryID, string uom, decimal qty) : base(cache, inventoryID, uom, qty)
			{
				SelectCommand.OrderByNew<OrderBy<Desc<ARSalesPrice.isPromotionalPrice,
							Desc<ARSalesPrice.siteID,
							Asc<InventoryItem.isTemplate,
							Desc <ARSalesPrice.breakQty>>>>>>();
			}

			public override int?[] GetInventoryIDs(PXCache sender, int? inventoryID)
			{
				return new int?[] { inventoryID, GetTemplateInventoryID(sender, inventoryID) };
			}
		}
	}
}