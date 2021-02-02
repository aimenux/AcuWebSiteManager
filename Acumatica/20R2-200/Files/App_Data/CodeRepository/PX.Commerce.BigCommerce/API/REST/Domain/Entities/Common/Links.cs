using Newtonsoft.Json;

namespace PX.Commerce.BigCommerce.API.REST
{
    public class Links
    {
        [JsonProperty("current")]
        public string Current { get; set; }
    }
}
