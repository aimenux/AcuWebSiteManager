using Newtonsoft.Json;
using System.ComponentModel;

namespace PX.Commerce.BigCommerce.API.REST
{
    public class AvsResult
    {
        [JsonProperty("code")]
		[Description(BigCommerceCaptions.Code)]
		public string Code { get; set; }

        [JsonProperty("message")]
		[Description(BigCommerceCaptions.Message)]
		public string Message { get; set; }

        [JsonProperty("street_match")]
		[Description(BigCommerceCaptions.StreetMatch)]
		public string StreetMatch { get; set; }

		[JsonProperty("postal_match")]
		[Description(BigCommerceCaptions.PostalMatch)]
		public string PostalMatch { get; set; }
    }
}
