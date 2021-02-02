using Newtonsoft.Json;
using System.ComponentModel;

namespace PX.Commerce.BigCommerce.API.REST
{
    [JsonObject(Description = "Product->ProductsCustomUrl")]
    public class ProductCustomUrl
    {
        [JsonProperty("url")]
		[Description(BigCommerceCaptions.CustomUrl)]
        public string Url { get; set; }

        [JsonProperty("is_customized")]
		[Description(BigCommerceCaptions.IsCustomized)]
		public bool IsCustomized { get; set; }
    }
}
