using System.Runtime.Serialization;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace PX.Commerce.BigCommerce.API.REST
{
    [JsonConverter(typeof(StringEnumConverter))]
    public enum ProductsConfigurableFieldType
    {
        [EnumMember(Value = "T")]
        Text = 0,

        [EnumMember(Value = "ML")]
        MultiLineText = 1,

        [EnumMember(Value = "C")]
        Checkbox = 2,

        [EnumMember(Value = "F")]
        File = 3,

        [EnumMember(Value = "S")]
        SelectBox = 4

    }
}
