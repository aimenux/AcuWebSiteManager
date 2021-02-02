using Newtonsoft.Json;

namespace PX.Commerce.BigCommerce.API.REST
{
    [JsonObject(Description = "Product -> Product Modifier -> Product Modifier Config")]
    public class ProductsModifierConfig
    {
        [JsonProperty("checkbox_label")]
        public string CheckboxLabel { get; set; }

        [JsonProperty("checked_by_default")]
        public bool CheckedByDefault { get; set; }

        [JsonProperty("default_value")]
        public string DefaultValue { get; set; }

        [JsonProperty("text_min_length")]
        public int TextMinLength { get; set; }

        [JsonProperty("text_max_length")]
        public int TextMaxLength { get; set; }

        [JsonProperty("text_characters_limited")]
        public bool TextCharactersLimited { get; set; }
    }
}
