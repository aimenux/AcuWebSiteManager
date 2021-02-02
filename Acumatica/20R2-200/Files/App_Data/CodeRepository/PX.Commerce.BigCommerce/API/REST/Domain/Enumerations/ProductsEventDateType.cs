using System.Runtime.Serialization;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace PX.Commerce.BigCommerce.API.REST
{
    [JsonConverter(typeof(StringEnumConverter))]
    public enum ProductsEventDateType
    {
        [EnumMember(Value = "none")]
        None = 0,

        [EnumMember(Value = "after")]
        After = 1,

        [EnumMember(Value = "before")]
        Before = 2,

        [EnumMember(Value = "range")]
        Range = 3,
                
        [EnumMember(Value = "required")]
        Required = 4
    }
}
