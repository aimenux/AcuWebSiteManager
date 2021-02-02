using System;
using System.Collections.Generic;
using System.ComponentModel;
using Newtonsoft.Json;

namespace PX.Commerce.Shopify.API.REST
{
    [JsonObject(Description = "Product -> Product Option")]
	[Description(ShopifyCaptions.ProductOptions)]
	public class ProductOptionData
    {
		/// <summary>
		/// An unsigned 64-bit integer that's used as a unique identifier for the product option
		/// </summary>
		[JsonProperty("id")]
        public long? Id { get; set; }

		/// <summary>
		/// The unique numeric identifier for the product.
		/// </summary>
		[JsonProperty("product_id")]
        public long ProductId { get; set; }

		/// <summary>
		/// The display name of Option
		/// </summary>
        [JsonProperty("name")]
		[Description(ShopifyCaptions.Name)]
        public string Name { get; set; }

		/// <summary>
		/// The position of Option
		/// </summary>
        [JsonProperty("position")]
		public int Position { get; set; }

		/// <summary>
		/// All available values in current product option
		/// </summary>
        [JsonProperty("values")]
        public string[] Values { get; set; }
        
	}
}
