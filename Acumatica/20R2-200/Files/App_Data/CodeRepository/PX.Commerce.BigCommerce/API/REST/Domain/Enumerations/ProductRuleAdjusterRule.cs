using System.Runtime.Serialization;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace PX.Commerce.BigCommerce.API.REST
{
    [JsonConverter(typeof(StringEnumConverter))]
    public enum ProductRuleAdjusterRule
    {
        [EnumMember(Value = "percent")]
        Percent = 0,

        [EnumMember(Value = "relative")]
        Relative = 1,

        [EnumMember(Value = "absolute")]
        Absolute = 2,
    }
}
