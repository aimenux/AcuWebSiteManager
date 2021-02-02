using System.Runtime.Serialization;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace PX.Commerce.BigCommerce.API.REST
{
    [JsonConverter(typeof(StringEnumConverter))]
    public enum ProductsDiscountType
    {
        [EnumMember(Value = "price")]
        Price = 0,

        [EnumMember(Value = "percent")]
        Percent = 1,

        [EnumMember(Value = "fixed")]
        Fixed = 2
    }
}
