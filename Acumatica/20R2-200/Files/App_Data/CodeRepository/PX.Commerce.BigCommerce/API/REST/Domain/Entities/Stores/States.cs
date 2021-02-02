using Newtonsoft.Json;

namespace PX.Commerce.BigCommerce.API.REST
{
	public class States
	{
		[JsonProperty("state_abbreviation")]
		public string StateID { get; set; }

		[JsonProperty("state")]
		public string State { get; set; }
		
	}
}
