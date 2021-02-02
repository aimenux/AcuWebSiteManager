using System;
using System.Collections.Generic;
using System.ComponentModel;
using Newtonsoft.Json;
using PX.Commerce.Core;

namespace PX.Commerce.Shopify.API.REST
{
	[JsonObject(Description = "Order Shipping Line")]
	[Description(ShopifyCaptions.ShippingLine)]
	public class OrderShippingLine : BCAPIEntity
	{
		/// <summary>
		/// A reference to the shipping method.
		/// </summary>
		[JsonProperty("code")]
		[Description(ShopifyCaptions.Code)]
		public String Code { get; set; }

		/// <summary>
		/// The price of the shipping method after line-level discounts have been applied. Doesn't reflect cart-level or order-level discounts.
		/// </summary>
		[JsonProperty("discounted_price")]
		[Description(ShopifyCaptions.DiscountedPrice)]
		public decimal? DiscountedPrice { get; set; }

		/// <summary>
		/// The price of the shipping method in both shop and presentment currencies after line-level discounts have been applied.
		/// </summary>
		[JsonProperty("discounted_price_set")]
		[Description(ShopifyCaptions.DiscountedPriceSet)]
		public PriceSet DiscountedPriceSet { get; set; }

		/// <summary>
		/// A list of amounts allocated by discount applications. Each discount allocation is associated to a particular discount application.
		/// </summary>
		[JsonProperty("discount_allocations", NullValueHandling = NullValueHandling.Ignore)]
		[Description(ShopifyCaptions.DiscountAllocation)]
		public List<OrderDiscountAllocation> DiscountAllocations { get; set; }

		/// <summary>
		/// The price of this shipping method in the shop currency. Can't be negative.
		/// </summary>
		[JsonProperty("price")]
		[Description(ShopifyCaptions.ShippingCostExcludingTax)]
		public decimal? ShippingCostExcludingTax { get; set; }

		/// <summary>
		/// The price of the shipping method in shop and presentment currencies.
		/// </summary>
		[JsonProperty("price_set")]
		[Description(ShopifyCaptions.PriceSet)]
		public PriceSet PriceSet { get; set; }

		/// <summary>
		/// The source of the shipping method.
		/// </summary>
		[JsonProperty("source")]
		[Description(ShopifyCaptions.SourceName)]
		public String SourceName { get; set; }

		/// <summary>
		/// The title of the shipping method.
		/// </summary>
		[JsonProperty("title")]
		[Description(ShopifyCaptions.Title)]
		public String Title { get; set; }

		/// <summary>
		/// A list of tax line objects, each of which details a tax applicable to this shipping line.
		/// </summary>
		[JsonProperty("tax_lines", NullValueHandling = NullValueHandling.Ignore)]
		[Description(ShopifyCaptions.TaxLine)]
		public List<OrderTaxLine> TaxLines { get; set; }

		/// <summary>
		/// A reference to the carrier service that provided the rate. Present when the rate was computed by a third-party carrier service.
		/// </summary>
		[JsonProperty("carrier_identifier")]
		[Description(ShopifyCaptions.CarrierIdentifier)]
		public String CarrierIdentifier { get; set; }
	}

}
