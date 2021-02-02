using System.Runtime.Serialization;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace PX.Commerce.BigCommerce.API.REST
{
    [JsonConverter(typeof(StringEnumConverter))]
    public enum ProductsCondition
    {
        [EnumMember(Value = "New")]
        New = 0,

        [EnumMember(Value = "Used")]
        Used = 1,

        [EnumMember(Value = "Refurbished")]
        Refurbished = 2
    }
}
