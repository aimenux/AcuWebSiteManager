using System.Collections.Generic;
using Newtonsoft.Json;

namespace PX.Commerce.BigCommerce.API.REST
{
    [JsonObject(Description = "Product Brand (total  BigCommerce API v3 response)")]
    public class ProductBrand : IEntityResponse<ProductBrandData>
    {
        [JsonProperty("data")]
        public ProductBrandData Data { get; set; }

        [JsonProperty("meta")]
        public Meta Meta { get; set; }
    }

    [JsonObject(Description = "Product Brand list (total  BigCommerce API v3 response)")]
    public class ProductBrandDataList : IEntitiesResponse<ProductBrandData>
    {
        [JsonProperty("data")]
        public List<ProductBrandData> Data { get; set; }

        [JsonProperty("meta")]
        public Meta Meta { get; set; }
    }

    
}
