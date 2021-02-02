using System.Collections.Generic;
using Newtonsoft.Json;

namespace PX.Commerce.BigCommerce.API.REST
{
    [JsonObject(Description = "Product Variant list)")]
    public class ProductsVariant : IEntityResponse<ProductsVariantData>
    {
        [JsonProperty("data")]
        public ProductsVariantData Data { get; set; }

        [JsonProperty("meta")]
        public Meta Meta { get; set; }
    }

    [JsonObject(Description = "Product Variant list (total  BigCommerce API v3 response)")]
    public class ProductVariantList: IEntitiesResponse<ProductsVariantData>
    {
        private List<ProductsVariantData> _data;

        [JsonProperty("data")]
        public List<ProductsVariantData> Data   
        {
            get => _data ?? (_data = new List<ProductsVariantData>());
            set => _data = value;
        }

        [JsonProperty("meta")]
        public Meta Meta { get; set; }
    }
}
