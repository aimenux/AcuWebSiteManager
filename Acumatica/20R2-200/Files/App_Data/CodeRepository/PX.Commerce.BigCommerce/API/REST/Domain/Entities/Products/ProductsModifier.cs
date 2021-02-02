using Newtonsoft.Json;
using System.Collections.Generic;

namespace PX.Commerce.BigCommerce.API.REST
{
    [JsonObject(Description = "Products Modifier (total  BigCommerce API v3 response)")]
    public class ProductsModifier : IEntityResponse<ProductsModifierData>
    {
        [JsonProperty("data")]
        public ProductsModifierData Data { get; set; }

        [JsonProperty("meta")]
        public Meta Meta { get; set; }
    }

    [JsonObject(Description = "Products Modifier list (total  BigCommerce API v3 response)")]
    public class ProductsModifierList : IEntitiesResponse<ProductsModifierData>
    {
        private List<ProductsModifierData> _data;

        [JsonProperty("data")]
        public List<ProductsModifierData> Data
        {
            get => _data ?? (_data = new List<ProductsModifierData>());
            set => _data = value;
        }

        [JsonProperty("meta")]
        public Meta Meta { get; set; }
    }
}
