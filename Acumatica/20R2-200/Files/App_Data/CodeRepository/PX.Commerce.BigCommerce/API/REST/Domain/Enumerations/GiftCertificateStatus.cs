using System.Runtime.Serialization;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace PX.Commerce.BigCommerce.API.REST
{
    [JsonConverter(typeof(StringEnumConverter))]
    public enum GiftCertificateStatus
    {
        [EnumMember(Value = "active")]
        Active,

        [EnumMember(Value = "pending")]
        Pending,

        [EnumMember(Value = "disabled")]
        Disabled,

        [EnumMember(Value = "expired")]
        Expired
    }
}
