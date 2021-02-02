using PX.Data;
using PX.Objects.IN;
using System;

namespace PX.Objects.AR
{
	/// <summary>
	/// This class contains <see cref="ARSalesPrice"/> projection definition.
	/// For <see cref="ARSalesPriceMaint"/> graph the projection contains joined <see cref="InventoryItem"/>, <see cref="ARPriceClass"/> and <see cref="Customer"/> entities.
	/// </summary>
	public class ARSalesPriceProjectionAttribute : PXProjectionAttribute
	{
		public ARSalesPriceProjectionAttribute() 
			: base(typeof(Select<ARSalesPrice>), new[] { typeof(ARSalesPrice) })
		{ }

		protected override Type GetSelect(PXCache sender)
		{
			if (sender.Graph is ARSalesPriceMaint)
				return typeof(Select2<ARSalesPrice,
					InnerJoin<InventoryItem,
						On<ARSalesPrice.inventoryID, Equal<InventoryItem.inventoryID>>,
					LeftJoin<ARPriceClass,
						On<ARSalesPrice.priceType, Equal<PriceTypes.customerPriceClass>,
						And<ARSalesPrice.custPriceClassID, Equal<ARPriceClass.priceClassID>>>,
					LeftJoin<Customer,
						On<ARSalesPrice.priceType, Equal<PriceTypes.customer>,
						And<ARSalesPrice.customerID, Equal<Customer.bAccountID>>>>>>>);

			return base.GetSelect(sender);
		}
	}
}
