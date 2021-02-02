using System.Collections.Generic;
using Newtonsoft.Json;

namespace PX.Commerce.BigCommerce.API.REST
{

    [JsonObject(Description = "Products Option (total  BigCommerce API v3 response)")]
    public class ProductsOption : IEntityResponse<ProductsOptionData>
    {
        [JsonProperty("data")]
        public ProductsOptionData Data { get; set; }

        [JsonProperty("meta")]
        public Meta Meta { get; set; }
    }

    [JsonObject(Description = "Products Option list (total  BigCommerce API v3 response)")]
    public class ProductsOptionList : IEntitiesResponse<ProductsOptionData>
    {
        private List<ProductsOptionData> _data;

        [JsonProperty("data")]
        public List<ProductsOptionData> Data    
        {
            get => _data ?? (_data = new List<ProductsOptionData>());
            set => _data = value;
        }

        [JsonProperty("meta")]
        public Meta Meta { get; set; }
    }

}
