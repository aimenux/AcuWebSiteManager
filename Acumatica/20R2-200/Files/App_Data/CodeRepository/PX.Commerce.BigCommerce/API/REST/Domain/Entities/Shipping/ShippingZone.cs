using System.Collections.Generic;
using System.ComponentModel;
using Newtonsoft.Json;
using PX.Commerce.Objects;

namespace PX.Commerce.BigCommerce.API.REST
{
	//This is the simplified model, if we need to use more fields in future, need to implement more details.
	[JsonObject(Description = "Shipping Zone")]
	[Description(BigCommerceCaptions.ShippingZone)]
	public class ShippingZone : IShippingZone
    {
		/// <summary>
		/// The ID of shipping zone.
		/// </summary>
		[JsonProperty("id")]
		[Description(BigCommerceCaptions.ShippingZoneId)]
		public virtual long? Id { get; set; }

		/// <summary>
		/// The shipping zone name.
		/// </summary>
		[JsonProperty("name")]
		[Description(BigCommerceCaptions.ShippingZoneName)]
		public string Name { get; set; }

		/// <summary>
		/// The shipping zone type.
		/// </summary>
		[JsonProperty("type")]
		[Description(BigCommerceCaptions.ShippingZoneType)]
		public string Type { get; set; }


		/// <summary>
		/// Flag used to indicate the shipping zone is enabled or disabled
		/// </summary>
		[JsonProperty("enabled")]
		public bool? Enabled { get; set; }

		[JsonIgnore]
		public List<IShippingMethod> ShippingMethods { get; set; }

		public override string ToString()
		{
			return $"{Name}, ID:{this.Id}, Type:{this.Type}";
		}
	}
}




