using System.Collections.Generic;
using Newtonsoft.Json;

namespace PX.Commerce.BigCommerce.API.REST
{
    [JsonObject(Description = "Product -> Product Option -> Product Option Config")]
    public class ProductsOptionConfig
    {
        [JsonProperty("default_value")]
        public string DefaultValue { get; set; }

        [JsonProperty("checked_by_default")]
        public bool CheckedByDefault { get; set; }

        [JsonProperty("checkbox_label")]
        public string CheckboxLabel { get; set; }

        [JsonProperty("date_limited")]
        public bool DateLimited { get; set; }

        [JsonProperty("date_limit_mode")]
        public string DateLimitMode { get; set; }

        [JsonProperty("date_earliest_value")]
        public string DateEarliestValue { get; set; }

        [JsonProperty("date_latest_value")]
        public string DateLatestValue { get; set; }

        [JsonProperty("file_types_mode")]
        public string FileTypesMode { get; set; }

        [JsonProperty("file_types_supported")]
        public List<string>FileTypesSupported { get; set; }

        [JsonProperty("file_types_other")]
        public List<string> FileTypesOther { get; set; }

        [JsonProperty("file_max_size")]
        public int FileMaxSize { get; set; }

        [JsonProperty("text_characters_limited")]
        public bool TextCharactersLimited { get; set; }

        [JsonProperty("text_min_length")]
        public int TextMinLength { get; set; }

        [JsonProperty("text_max_length")]
        public int TextMaxLength { get; set; }

        [JsonProperty("text_lines_limited")]
        public bool TextLinesLimited { get; set; }

        [JsonProperty("text_max_lines")]
        public int TextMaxLines { get; set; }

        [JsonProperty("number_limited")]
        public bool NumberLimited { get; set; }

        [JsonProperty("number_limit_mode")]
        public string NumberLimitMode { get; set; }

        [JsonProperty("number_lowest_value")]
        public int NumberLowestValue { get; set; }

        [JsonProperty("number_highest_value")]
        public int NumberHighestValue { get; set; }

        [JsonProperty("number_integers_only")]
        public bool NumberIntegersOnly { get; set; }

        [JsonProperty("product_list_adjusts_inventory")]
        public bool ProductListAdjustsInventory { get; set; }

        [JsonProperty("product_list_adjusts_pricing")]
        public bool ProductListAdjustsPricing { get; set; }

        [JsonProperty("product_list_shipping_calc")]
        public string ProductListShippingCalc { get; set; }
    }
}
