using System.Runtime.Serialization;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace PX.Commerce.BigCommerce.API.REST
{
    [JsonConverter(typeof(StringEnumConverter))]
    public enum ProductsType
    {
        [EnumMember(Value = "physical")]
        Physical = 0,

        [EnumMember(Value = "digital")]
        Digital = 1
    }
}
