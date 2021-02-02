using Newtonsoft.Json;
using System.ComponentModel;

namespace PX.Commerce.BigCommerce.API.REST
{
    public class CvvResult
    {
        [JsonProperty("code")]
		[Description(BigCommerceCaptions.Code)]
		public string Code { get; set; }

        [JsonProperty("message")]
		[Description(BigCommerceCaptions.Message)]
		public string Message { get; set; }
    }
}
