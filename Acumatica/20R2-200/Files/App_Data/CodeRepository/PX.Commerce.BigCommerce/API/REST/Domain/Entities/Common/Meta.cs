using Newtonsoft.Json;

namespace PX.Commerce.BigCommerce.API.REST
{
    public class Meta
    {
        [JsonProperty("pagination")]
        public Pagination Pagination { get; set; }
    }

}
