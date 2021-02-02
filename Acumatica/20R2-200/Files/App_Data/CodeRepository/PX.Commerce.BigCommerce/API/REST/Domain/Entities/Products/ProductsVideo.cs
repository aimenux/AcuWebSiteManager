using Newtonsoft.Json;
using PX.Commerce.Core;

namespace PX.Commerce.BigCommerce.API.REST
{
    [JsonObject(Description = "Product-> ProductVideo")]
    public class ProductsVideo: BCAPIEntity
	{
        [JsonProperty("title")]
        public string Title { get; set; }

        [JsonProperty("description")]
        public string Description { get; set; }

        [JsonProperty("sort_order")]
        public int SortOrder { get; set; }

        [JsonProperty("type")]
        public string Type { get; set; }

        [JsonProperty("id")]
        public int Id { get; set; }

        [JsonProperty("video_id")]
        public string VideoId { get; set; }

        [JsonProperty("product_id")]
        public int ProductId { get; set; }

        [JsonProperty("length")]
        public string Length { get; set; }
    }
}
