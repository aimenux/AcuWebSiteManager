using Newtonsoft.Json;
using System.Collections.Generic;

namespace PX.Commerce.BigCommerce.API.REST
{
    public class ProductsModifierValue : IEntityResponse<ProductModifierValueData>
    {
        [JsonProperty("data")]
        public ProductModifierValueData Data { get; set; }

        [JsonProperty("meta")]
        public Meta Meta { get; set; }
    }

    public class ProductsModifierValueList : IEntitiesResponse<ProductModifierValueData>
    {
        private List<ProductModifierValueData> _data;

        [JsonProperty("data")]
        public List<ProductModifierValueData> Data
        {
            get => _data ?? (_data = new List<ProductModifierValueData>());
            set => _data = value;
        }

        [JsonProperty("meta")]
        public Meta Meta { get; set; }
    }
}
