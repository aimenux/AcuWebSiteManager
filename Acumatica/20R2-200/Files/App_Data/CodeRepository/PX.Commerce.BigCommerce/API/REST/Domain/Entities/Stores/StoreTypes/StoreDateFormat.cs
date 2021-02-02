using Newtonsoft.Json;

namespace PX.Commerce.BigCommerce.API.REST
{

    public class StoreDateFormat
    {

        [JsonProperty("display")]
        public string Display { get; set; }

        [JsonProperty("export")]
        public string Export { get; set; }

        [JsonProperty("extended_display")]
        public string ExtendedDisplay { get; set; }
    }

}
