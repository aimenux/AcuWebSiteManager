using System.Runtime.Serialization;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace PX.Commerce.BigCommerce.API.REST
{
    [JsonConverter(typeof(StringEnumConverter))]
    public enum OptionType
    {
        [EnumMember(Value = "C")]
        Checkbox = 0,

        [EnumMember(Value = "D")]
        Date = 1,

        [EnumMember(Value = "F")]
        File = 2,

        [EnumMember(Value = "N")]
        NumbersOnlyText = 3,

        [EnumMember(Value = "T")]
        Text = 4,

        [EnumMember(Value = "MT")]
        MultiLineText = 5,

        [EnumMember(Value = "P")]
        ProductList = 6,

        [EnumMember(Value = "PI")]
        ProductListWithImages = 7,

        [EnumMember(Value = "RB")]
        RadioList = 8,

        [EnumMember(Value = "RT")]
        RectangleList = 9,

        [EnumMember(Value = "S")]
        SelectBox = 10,

        [EnumMember(Value = "CS")]
        Swatch = 11,
    }
}
