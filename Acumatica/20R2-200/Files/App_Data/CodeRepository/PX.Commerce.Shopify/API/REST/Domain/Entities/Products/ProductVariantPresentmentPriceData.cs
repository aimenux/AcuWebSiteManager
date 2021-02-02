using System;
using System.Collections.Generic;
using System.ComponentModel;
using Newtonsoft.Json;

namespace PX.Commerce.Shopify.API.REST
{
    [JsonObject(Description = "Product Variant -> Presentment Prices")]
	[Description("Presentment Prices")]
	public class ProductVariantPresentmentPriceData
	{
		/// <summary>
		/// The variant's presentment prices
		/// </summary>
		[JsonProperty("price")]
        public PresentmentPrice Price { get; set; }

		/// <summary>
		/// The variant's presentment compare at price
		/// </summary>
		[JsonProperty("compare_at_price")]
        public PresentmentPrice OriginalPrice { get; set; }

        public PresentmentPrice AddPrice(PriceType priceType, String currencyCode, decimal amount )
		{
			var newPrice = new PresentmentPrice() { CurrencyCode = currencyCode, Amount = amount };
			switch(priceType)
			{
				case PriceType.Price: Price = newPrice; break;
				case PriceType.CompareAtPrice: OriginalPrice = newPrice; break;
				default: break;
			}
			return newPrice;
		}

		public enum PriceType
		{
			Price,
			CompareAtPrice
		}
	}
}
