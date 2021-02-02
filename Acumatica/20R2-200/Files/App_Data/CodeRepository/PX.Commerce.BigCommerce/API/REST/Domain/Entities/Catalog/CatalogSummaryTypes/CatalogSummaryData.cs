using Newtonsoft.Json;

namespace PX.Commerce.BigCommerce.API.REST
{

    public class CatalogSummaryData
    {

        [JsonProperty("inventory_count")]
        public int InventoryCount { get; set; }

        [JsonProperty("inventory_value")]
        public int InventoryValue { get; set; }

        [JsonProperty("primary_category_id")]
        public int PrimaryCategoryId { get; set; }

        [JsonProperty("primary_category_name")]
        public string PrimaryCategoryName { get; set; }
    }

}
