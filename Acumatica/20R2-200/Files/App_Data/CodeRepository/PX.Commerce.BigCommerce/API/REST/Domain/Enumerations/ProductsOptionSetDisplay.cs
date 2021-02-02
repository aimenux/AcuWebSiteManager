using System.Runtime.Serialization;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace PX.Commerce.BigCommerce.API.REST
{
    [JsonConverter(typeof(StringEnumConverter))]
    public enum ProductsOptionSetDisplay
    {
        [EnumMember(Value = "right")]
        Right = 0,

        [EnumMember(Value = "below")]
        Below = 1
    }
    
    
}
