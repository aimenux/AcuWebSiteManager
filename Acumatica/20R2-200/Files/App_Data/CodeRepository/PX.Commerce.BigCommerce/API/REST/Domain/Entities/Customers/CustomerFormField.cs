using Newtonsoft.Json;
using System.Collections.Generic;

namespace PX.Commerce.BigCommerce.API.REST
{
    [JsonObject(Description = "Customer Form Field (BigCommerce API v3 response)")]
    public class CustomerFormField : IEntityResponse<CustomerFormFieldData>
    {
        [JsonProperty("data")]
        public CustomerFormFieldData Data { get; set; }

        [JsonProperty("meta")]
        public Meta Meta { get; set; }
    }

    [JsonObject(Description = "Customer Form Field list (BigCommerce API v3 response)")]
    public class CustomerFormFieldList : IEntitiesResponse<CustomerFormFieldData>
    {
        private List<CustomerFormFieldData> _data;

        [JsonProperty("data")]
        public List<CustomerFormFieldData> Data
        {
            get => _data ?? (_data = new List<CustomerFormFieldData>());
            set => _data = value;
        }

        [JsonProperty("meta")]
        public Meta Meta { get; set; }
    }
}