using Newtonsoft.Json;
using System.ComponentModel;

namespace PX.Commerce.BigCommerce.API.REST
{
    public class CreditCard
    {
        [JsonProperty("card_type")]
		[Description(BigCommerceCaptions.CardType)]
		public string CardType { get; set; }

        [JsonProperty("card_iin")]
		[Description(BigCommerceCaptions.CardIin)]
		public string CardIin { get; set; }

        [JsonProperty("card_last4")]
		[Description(BigCommerceCaptions.CardLast4)]
		public string CardLast4 { get; set; }
    }
}
