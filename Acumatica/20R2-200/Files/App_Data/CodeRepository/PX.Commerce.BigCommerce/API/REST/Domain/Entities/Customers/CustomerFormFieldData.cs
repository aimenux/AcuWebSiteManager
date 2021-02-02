using Newtonsoft.Json;
using PX.Commerce.Core;
using System.Collections.Generic;

namespace PX.Commerce.BigCommerce.API.REST
{
	[JsonObject(Description = "Customer -> CustomerFormField")]
    public class CustomerFormFieldData: BCAPIEntity
    {
        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("value")]
        public dynamic Value { get; set; }

        [JsonProperty("customer_id")]
        public int? CustomerId { get; set; }

        [JsonProperty("address_id")]
        public int? AddressId { get; set; }

    }
}
