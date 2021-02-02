using Newtonsoft.Json;
using PX.Commerce.Objects;

namespace PX.Commerce.Shopify.API.REST
{ 
	public class PaymentMethod : IPaymentMethod
	{

		[JsonProperty("name")]
		public string Name { get; set; }

		public bool CreatePaymentfromOrder { get; set; }
	}
}
