using Newtonsoft.Json;

namespace PX.Commerce.BigCommerce.API.REST
{
    [JsonObject(Description = "Product -> Product Modifier -> Product Modifier Value")]
    public class ProductModifierValueData
    {
        public ProductModifierValueData()
        {
            //ValueData = new List<object>();
        }

        [JsonProperty("id")]
        public int? Id { get; set; }

        [JsonProperty("option_id")]
        public int? OptionId { get; set; }

        [JsonProperty("label")]
        public string Label { get; set; }

        [JsonProperty("sort_order")]
        public int SortOrder { get; set; }
        [JsonProperty("value_data")]
        public ValueData ValueData { get; set; }

        [JsonProperty("is_default")]
        public bool IsDefault { get; set; }

        [JsonProperty("adjusters")]
        public object Adjusters { get; set; }
    }

    public class ValueData
    {
        [JsonProperty("checked_value")]
        public bool? CheckedValue { get; set; }
    }
}
