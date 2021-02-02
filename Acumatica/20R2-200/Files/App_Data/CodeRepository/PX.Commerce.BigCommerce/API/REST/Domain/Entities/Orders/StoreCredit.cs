using Newtonsoft.Json;
using System.ComponentModel;

namespace PX.Commerce.BigCommerce.API.REST
{
    public class StoreCredit
    {
        [JsonProperty("remaining_balance")]
		[Description(BigCommerceCaptions.RemainingBalance)]
		public string RemainingBalance { get; set; }
    }
}
