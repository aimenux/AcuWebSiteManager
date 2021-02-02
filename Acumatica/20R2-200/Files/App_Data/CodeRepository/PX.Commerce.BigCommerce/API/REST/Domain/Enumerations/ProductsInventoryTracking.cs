using System.Runtime.Serialization;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace PX.Commerce.BigCommerce.API.REST
{
    [JsonConverter(typeof(StringEnumConverter))]
    public enum ProductsInventoryTracking
    {
        [EnumMember(Value = "none")]
        None = 0,

        [EnumMember(Value = "simple")]
        Simple = 1,

        [EnumMember(Value = "sku")]
        Sku = 2
    }
}
