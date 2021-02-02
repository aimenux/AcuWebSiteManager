using Newtonsoft.Json;

namespace PX.Commerce.BigCommerce.API.REST
{
    [JsonObject(Description = "Product -> Product Variant -> Product Variant Option Value")]
    public class ProductVariantOptionValueData
    {
        [JsonProperty("option_display_name")]
        public string OptionDisplayName { get; set; }

        [JsonProperty("label")]
        public string Label { get; set; }

        [JsonProperty("id")]
        public int? Id { get; set; }

        [JsonProperty("option_id")]
        public int? OptionId { get; set; }
    }
}
