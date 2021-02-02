using System;
using System.Collections.Generic;
using System.ComponentModel;
using Newtonsoft.Json;
using PX.Commerce.Core;

namespace PX.Commerce.Shopify.API.REST
{
	[JsonObject(Description = "Order Tax Line")]
	[Description(ShopifyCaptions.TaxLine)]
	public class OrderTaxLine : BCAPIEntity
	{
		/// <summary>
		/// The name of the tax.
		/// </summary>
		[JsonProperty("title")]
		[Description(ShopifyCaptions.TaxName)]
		public String TaxName { get; set; }

		/// <summary>
		/// The amount added to the order for this tax in the shop currency.
		/// </summary>
		[JsonProperty("price")]
		[Description(ShopifyCaptions.TaxLineAmount)]
		public decimal? TaxAmount { get; set; }

		/// <summary>
		/// The tax rate applied to the order to calculate the tax price.
		/// </summary>
		[JsonProperty("rate")]
		[Description(ShopifyCaptions.TaxRate)]
		public decimal? TaxRate { get; set; }

		/// <summary>
		/// The amount added to the order for this tax in shop and presentment currencies.
		/// </summary>
		[JsonProperty("price_set")]
		[Description(ShopifyCaptions.PriceSet)]
		public PriceSet TaxPriceSet { get; set; }
	}

}
