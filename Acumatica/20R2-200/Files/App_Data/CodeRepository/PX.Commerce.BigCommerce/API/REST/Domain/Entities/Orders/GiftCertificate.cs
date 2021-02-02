using Newtonsoft.Json;
using System.ComponentModel;

namespace PX.Commerce.BigCommerce.API.REST
{
    public class GiftCertificate
    {
        [JsonProperty("code")]
		[Description(BigCommerceCaptions.Code)]
		public string Code { get; set; }

        [JsonProperty("original_balance")]
		[Description(BigCommerceCaptions.OriginalBalance)]
		public double OriginalBalance { get; set; }

        [JsonProperty("starting_balance")]
		[Description(BigCommerceCaptions.StartingBalance)]
		public double StartingBalance { get; set; }

        [JsonProperty("remaining_balance")]
		[Description(BigCommerceCaptions.RemainingBalance)]
		public double RemainingBalance { get; set; }

        [JsonProperty("status")]
		[Description(BigCommerceCaptions.GiftCertificateStatus)]
		public GiftCertificateStatus Status { get; set; }
    }
}
