using System;
using System.Collections.Generic;
using System.ComponentModel;
using Newtonsoft.Json;
using PX.Commerce.Core;

namespace PX.Commerce.Shopify.API.REST
{
	/// <summary>
	/// Order refunded line item.
	/// </summary>
	[JsonObject(Description = "Order Refund Line Item")]
	[Description(ShopifyCaptions.RefundItem)]
	public class RefundLineItem : BCAPIEntity
	{
		/// <summary>
		/// The unique identifier of the line item in the refund.
		[JsonProperty("id", NullValueHandling = NullValueHandling.Ignore)]
		[Description(ShopifyCaptions.Id)]
		public long? Id { get; set; }

		/// <summary>
		/// A line item being returned.
		/// </summary>
		[JsonProperty("line_item", NullValueHandling = NullValueHandling.Ignore)]
		[Description(ShopifyCaptions.LineItem)]
		public OrderLineItem OrderLineItem { get; set; }

		/// <summary>
		/// The ID of the related line item in the order.
		/// </summary>
		[JsonProperty("line_item_id", NullValueHandling = NullValueHandling.Ignore)]
		[Description(ShopifyCaptions.LineItemId)]
		public long? LineItemId { get; set; }

		/// <summary>
		/// The quantity of the associated line item that was returned.
		/// </summary>
		[JsonProperty("quantity")]
		[Description(ShopifyCaptions.Quantity)]
		public int? Quantity { get; set; }

		/// <summary>
		/// The unique identifier of the location where the items will be restocked. Required when restock_type has the value return or cancel.
		/// </summary>
		[JsonProperty("location_id", NullValueHandling = NullValueHandling.Ignore)]
		public long? LocationId { get; set; }

		/// <summary>
		/// How this refund line item affects inventory levels. 
		/// </summary>
		[JsonProperty("restock_type", NullValueHandling = NullValueHandling.Ignore)]
		[Description(ShopifyCaptions.RestockType)]
		public RestockType RestockType { get; set; }

		/// <summary>
		/// The subtotal of the refund line item.
		/// </summary>
		[JsonProperty("subtotal", NullValueHandling = NullValueHandling.Ignore)]
		[Description(ShopifyCaptions.Subtotal)]
		public decimal? SubTotal { get; set; }

		/// <summary>
		/// The total tax on the refund line item.
		/// </summary>
		[JsonProperty("total_tax", NullValueHandling = NullValueHandling.Ignore)]
		[Description(ShopifyCaptions.TotalTax)]
		public decimal? TotalTax { get; set; }

		/// <summary>
		/// The subtotal of the refund line item in shop and presentment currencies.
		/// </summary>
		[JsonProperty("subtotal_set", NullValueHandling = NullValueHandling.Ignore)]
		[Description(ShopifyCaptions.SubtotalSet)]
		public PriceSet SubTotalSet { get; set; }

		/// <summary>
		/// The total tax of the line item in shop and presentment currencies.
		/// </summary>
		[JsonProperty("total_tax_set", NullValueHandling = NullValueHandling.Ignore)]
		[Description(ShopifyCaptions.TotalTaxSet)]
		public PriceSet TotalTaxSet { get; set; }
	}
}
