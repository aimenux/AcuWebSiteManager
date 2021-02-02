using System.Collections.Generic;
using Newtonsoft.Json;

namespace PX.Commerce.BigCommerce.API.REST
{
    [JsonObject(Description = "Product Tax")]
    public class ProductsTax : IEntityResponse<ProductsTaxData>
    {
        [JsonProperty("data")]
        public ProductsTaxData Data { get; set; }

        [JsonProperty("meta")]
        public Meta Meta { get; set; }
    }

    public class ProductsTaxList : IEntitiesResponse<ProductsTaxData>
    {
        private List<ProductsTaxData> _data;

        [JsonProperty("data")]
        public List<ProductsTaxData> Data
        {
            get => _data ?? (_data = new List<ProductsTaxData>());
            set => _data = value;
        }

        [JsonProperty("meta")]
        public Meta Meta { get; set; }
    }

}
