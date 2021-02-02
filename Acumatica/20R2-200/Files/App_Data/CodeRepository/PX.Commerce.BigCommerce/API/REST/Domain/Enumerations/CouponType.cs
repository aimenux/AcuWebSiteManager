using System.Runtime.Serialization;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace PX.Commerce.BigCommerce.API.REST
{
    [JsonConverter(typeof(StringEnumConverter))]
    public enum CouponType
    {
        [EnumMember(Value = "per_item_discount")]
        PerItemDiscount = 0,

        [EnumMember(Value = "per_total_discount")]
        PerTotalDiscount = 1,

        [EnumMember(Value = "shipping_discount")]
        ShippingDiscount = 2,

        [EnumMember(Value = "free_shipping")]
        FreeShipping = 3,

        [EnumMember(Value = "percentage_discount")]
        PercentageDiscount = 4,

    }
}
