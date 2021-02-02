using System.Runtime.Serialization;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace PX.Commerce.BigCommerce.API.REST
{
    [JsonConverter(typeof(StringEnumConverter))]
    public enum OrderPaymentEvent
    {
        [EnumMember(Value = "purchase")]
        Purchase,

        [EnumMember(Value = "authorization")]
        Authorization,

        [EnumMember(Value = "capture")]
        Capture,

        [EnumMember(Value = "refund")]
        Refund,

        [EnumMember(Value = "void")]
        Void,

        [EnumMember(Value = "pending")]
        Pending,

        [EnumMember(Value = "settled")]
        Settled
    }
}
