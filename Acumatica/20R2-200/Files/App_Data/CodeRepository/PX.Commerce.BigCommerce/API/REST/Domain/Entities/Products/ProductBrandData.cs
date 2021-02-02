using Newtonsoft.Json;

namespace PX.Commerce.BigCommerce.API.REST
{
    [JsonObject(Description = "Product Brand")]
    public class ProductBrandData
    { 
        [JsonProperty("id")]
        public int? Id { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("page_title")]
        public string PageTitle { get; set; }

        [JsonProperty("meta_keywords")]
        public string[] MetaKeywords { get; set; }

        [JsonProperty("meta_description")]
        public string MetaDescription { get; set; }

        [JsonProperty("image_url")]
        public string ImageUrl { get; set; }

        [JsonProperty("search_keywords")]
        public string SearchKeywords { get; set; }

        [JsonProperty("custom_url")]
        public ProductCustomUrl CustomUrl { get; set; }

        public override string ToString()
        {
            return $"{Name}  , Id:{Id}] ";
        }
    }
}
