using Newtonsoft.Json;

namespace PX.Commerce.BigCommerce.API.REST
{

    public class CatalogSummary
    {
        [JsonProperty("data")]
        public CatalogSummaryData Data { get; set; }

        [JsonProperty("meta")]
        public CatalogSummaryMeta Meta { get; set; }
    }

}
