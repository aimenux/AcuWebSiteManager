using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace PX.Commerce.BigCommerce.API.REST
{
    [JsonObject(Description = "Product -> Product Option")]
    public class ProductsOptionData
    {
        public ProductsOptionData()
        {
            OptionValues = new List<ProductOptionValueData>();
            NewOptionValues = new List<ProductOptionValueData>();
        }
        
        [JsonProperty("id")]
        public int? Id { get; set; }

        [JsonProperty("product_id")]
        public int ProductId { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("display_name")]
        public string DisplayName { get; set; }

        [JsonProperty("type")]
        public string Type { get; set; }

        [JsonProperty("sort_order")]
        public int SortOrder { get; set; }
        
        [JsonProperty("config")]
        public List<ProductsOptionConfig> Config { get; set; }

        [JsonProperty("option_values")]
        public List<ProductOptionValueData> OptionValues { get; set; }

        [JsonIgnore]
        public List<ProductOptionValueData> NewOptionValues { get; set; }

		[JsonIgnore]
		public Guid? LocalID { get; set; }
	}
}
