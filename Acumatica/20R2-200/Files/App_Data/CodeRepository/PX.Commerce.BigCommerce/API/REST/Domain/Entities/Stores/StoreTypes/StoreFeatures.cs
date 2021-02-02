using Newtonsoft.Json;

namespace PX.Commerce.BigCommerce.API.REST
{
    public class StoreFeatures
    {
        [JsonProperty("stencil_enabled")]
        public bool StencilEnabled { get; set; }
    }

}
