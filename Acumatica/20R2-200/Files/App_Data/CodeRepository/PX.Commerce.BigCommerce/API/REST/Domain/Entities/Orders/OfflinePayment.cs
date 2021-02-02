using Newtonsoft.Json;
using System.ComponentModel;

namespace PX.Commerce.BigCommerce.API.REST
{
    public class OfflinePayment
    {
        [JsonProperty("display_name")]
		[Description(BigCommerceCaptions.DisplayName)]
        public string DisplayName { get; set; }
    }
}
