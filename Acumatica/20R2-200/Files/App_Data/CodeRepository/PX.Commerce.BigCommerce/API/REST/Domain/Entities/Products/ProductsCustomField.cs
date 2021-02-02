using Newtonsoft.Json;
using System.Collections.Generic;

namespace PX.Commerce.BigCommerce.API.REST
{
    [JsonObject(Description = "Product Custom Field (BigCommerce API v3 response)")]
    public class ProductsCustomField : IEntityResponse<ProductsCustomFieldData>
    {
        [JsonProperty("data")]
        public ProductsCustomFieldData Data { get; set; }

        [JsonProperty("meta")]
        public Meta Meta { get; set; }
    }

    [JsonObject(Description = "Product Custom Field list (BigCommerce API v3 response)")]
    public class ProductsCustomFieldList : IEntitiesResponse<ProductsCustomFieldData>
    {
        private List<ProductsCustomFieldData> _data;

        [JsonProperty("data")]
        public List<ProductsCustomFieldData> Data
        {
            get => _data ?? (_data = new List<ProductsCustomFieldData>());
            set => _data = value;
        }

        [JsonProperty("meta")]
        public Meta Meta { get; set; }
    }
}