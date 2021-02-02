using System.Runtime.Serialization;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace PX.Commerce.BigCommerce.API.REST
{
    [JsonConverter(typeof(StringEnumConverter))]
    public enum OrdersProductsType
    {
        [EnumMember(Value = "physical")]
        Physical = 0,
        
        [EnumMember(Value = "digital")]
        Digital = 1,
        
        [EnumMember(Value = "giftcertificate")]
        GiftCertificate = 2
    }
}
