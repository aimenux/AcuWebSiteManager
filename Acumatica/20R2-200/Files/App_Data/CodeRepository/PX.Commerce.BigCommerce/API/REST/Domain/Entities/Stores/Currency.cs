using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PX.Commerce.BigCommerce.API.REST
{
	[JsonObject(Description = "Currency")]
	public class Currency
	{
		[JsonProperty("id")]
		public int Id { get; set; }

		[JsonProperty("is_default")]
		public bool Default { get; set; }

		[JsonProperty("enabled")]
		public bool Enabled { get; set; }

		[JsonProperty("name")]
		public string Name { get; set; }

		[JsonProperty("currency_code")]
		public string CurrencyCode { get; set; }
	}
}
