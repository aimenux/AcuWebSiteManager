using System;
using System.Collections.Generic;
using System.ComponentModel;
using Newtonsoft.Json;
using PX.Commerce.Core.REST;
using PX.Commerce.Objects;

namespace PX.Commerce.BigCommerce.API.REST
{
	//This is the simplified model, if we need to use more fields in future, need to implement more details.
	[JsonObject(Description = "Shipping Method")]
	[Description(BigCommerceCaptions.ShippingMethod)]
	public class ShippingMethod : IShippingMethod
    {
		/// <summary>
		/// The ID of shipping method.
		/// </summary>
		[JsonProperty("id")]
		[Description(BigCommerceCaptions.ShippingMethodId)]
		public virtual long? Id { get; set; }

		/// <summary>
		/// The shipping method name.
		/// </summary>
		[JsonProperty("name")]
		[Description(BigCommerceCaptions.ShippingMethodName)]
		public string Name { get; set; }

		/// <summary>
		/// The shipping method type.
		/// </summary>
		[JsonProperty("type")]
		[Description(BigCommerceCaptions.ShippingMethodType)]
		public string Type { get; set; }

		[JsonProperty("handling_fees")]
		public object HandlingFees { get; set; }

		[JsonProperty("is_fallback")]
		public bool? IsFallback { get; set; }

		[JsonProperty("enabled")]
		public bool? Enabled { get; set; }

		[JsonProperty("settings")]
		[JsonConverter(typeof(SingleOrArrayConverter<ShippingMethodSettings>))]
		public List<ShippingMethodSettings> Settings { get; set; }

		[JsonIgnore]
		public List<String> ShippingServices { get; set; }

		public override string ToString()
		{
			return $"{Name}, ID:{this.Id}, Type:{this.Type}";
		}
	}

	[JsonObject(Description = "Shipping Method Settings")]
	public class ShippingMethodSettings
	{
		[JsonProperty("carrier_options")]
		public ShippingMethodCarrierOptions CarrierOptions { get; set; }
	}

	[JsonObject(Description = "Shipping Method Carrier Options")]
	public class ShippingMethodCarrierOptions
	{
		[JsonProperty("delivery_services")]
		public List<String> DeliveryServices { get; set; }
	}
}




