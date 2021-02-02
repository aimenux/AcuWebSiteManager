using Newtonsoft.Json;

namespace PX.Commerce.BigCommerce.API.REST
{
    public class Pagination
    {
        [JsonProperty("total")]
        public int? Total { get; set; }

        [JsonProperty("count")]
        public int? Count { get; set; }

        [JsonProperty("per_page")]
        public int? PerPage { get; set; }

        [JsonProperty("current_page")]
        public int? CurrentPage { get; set; }

        [JsonProperty("total_pages")]
        public int? TotalPages { get; set; }

        [JsonProperty("links")]
        public Links Links { get; set; }

        [JsonProperty("too_many")]
        public bool? TooMany { get; set; }
    }
}
