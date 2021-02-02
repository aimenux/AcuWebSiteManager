using System.Runtime.Serialization;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace PX.Commerce.BigCommerce.API.REST
{
    [JsonConverter(typeof(StringEnumConverter))]
    public enum ProductsAvailability
    {
        [EnumMember(Value = "available")]
        Available = 0,

        [EnumMember(Value = "disabled")]
        Disabled = 1,

        [EnumMember(Value = "preorder")]
        Preorder = 2,
    }
}
