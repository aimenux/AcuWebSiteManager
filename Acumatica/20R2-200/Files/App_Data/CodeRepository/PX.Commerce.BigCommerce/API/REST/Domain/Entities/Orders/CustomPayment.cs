using Newtonsoft.Json;
using System.ComponentModel;

namespace PX.Commerce.BigCommerce.API.REST
{
    public class CustomPayment
    {
        [JsonProperty("payment_method")]
		[Description(BigCommerceCaptions.PaymentMethod)]
		public string PaymentMethod { get; set; }
    }
}
