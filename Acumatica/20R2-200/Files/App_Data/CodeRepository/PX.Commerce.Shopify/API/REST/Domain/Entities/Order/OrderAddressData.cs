using System;
using System.Collections.Generic;
using System.ComponentModel;
using Newtonsoft.Json;
using PX.Commerce.Core;

namespace PX.Commerce.Shopify.API.REST
{

	[JsonObject(Description = "Order Address")]
	[Description(ShopifyCaptions.OrderAddress)]
	public class OrderAddressData : BCAPIEntity
	{
		/// <summary>
		/// The first name of the person associated with the payment method.
		/// </summary>
		[JsonProperty("first_name", NullValueHandling = NullValueHandling.Ignore)]
		[Description(ShopifyCaptions.FirstName)]
		public virtual string FirstName { get; set; }

		/// <summary>
		/// The last name of the person associated with the payment method.
		/// </summary>
		[JsonProperty("last_name", NullValueHandling = NullValueHandling.Ignore)]
		[Description(ShopifyCaptions.LastName)]
		public virtual string LastName { get; set; }

		/// <summary>
		/// The full name of the person associated with the payment method.
		/// </summary>
		[JsonProperty("name", NullValueHandling = NullValueHandling.Ignore)]
		[Description(ShopifyCaptions.Name)]
		public virtual string Name { get; set; }

		/// <summary>
		/// The company of the person associated with the address.
		/// </summary>
		[JsonProperty("company", NullValueHandling = NullValueHandling.Ignore)]
		[Description(ShopifyCaptions.CompanyName)]
		public virtual string Company { get; set; }

		/// <summary>
		/// The street address of the address.
		/// </summary>
		[JsonProperty("address1", NullValueHandling = NullValueHandling.Ignore)]
		[Description(ShopifyCaptions.AddressLine1)]
		public string Address1 { get; set; }

		/// <summary>
		/// An optional additional field for the street address of the address.
		/// </summary>
		[JsonProperty("address2", NullValueHandling = NullValueHandling.Ignore)]
		[Description(ShopifyCaptions.AddressLine2)]
		[ValidateRequired(AutoDefault = true)]
		public string Address2 { get; set; }

		/// <summary>
		/// The city, town, or village of the address.
		/// </summary>
		[JsonProperty("city", NullValueHandling = NullValueHandling.Ignore)]
		[Description(ShopifyCaptions.City)]
		[ValidateRequired(AutoDefault = true)]
		public virtual string City { get; set; }

		/// <summary>
		/// The name of the region (province, state, prefecture, …) of the address.
		/// </summary>
		[JsonProperty("province", NullValueHandling = NullValueHandling.Ignore)]
		[Description(ShopifyCaptions.Province)]
		[ValidateRequired(AutoDefault = true)]
		public virtual string Province { get; set; }

		/// <summary>
		/// The postal code (zip, postcode, Eircode, …) of the address.
		/// </summary>
		[JsonProperty("zip", NullValueHandling = NullValueHandling.Ignore)]
		[Description(ShopifyCaptions.PostalCode)]
		[ValidateRequired(AutoDefault = true)]
		public virtual string PostalCode { get; set; }

		/// <summary>
		/// The name of the country of the address.
		/// </summary>
		[JsonProperty("country", NullValueHandling = NullValueHandling.Ignore)]
		[Description(ShopifyCaptions.Country)]
		[ValidateRequired()]
		public virtual string Country { get; set; }

		/// <summary>
		/// The two-letter code (ISO 3166-1 format) for the country of the address.
		/// </summary>
		[JsonProperty("country_code", NullValueHandling = NullValueHandling.Ignore)]
		[Description(ShopifyCaptions.CountryISOCode)]
		public string CountryCode { get; set; }

		/// <summary>
		/// The two-letter abbreviation of the region of the address.
		/// </summary>
		[JsonProperty("province_code", NullValueHandling = NullValueHandling.Ignore)]
		[Description(ShopifyCaptions.ProvinceCode)]
		public string ProvinceCode { get; set; }

		/// <summary>
		/// The phone number at the address.
		/// </summary>
		[JsonProperty("phone", NullValueHandling = NullValueHandling.Ignore)]
		[Description(ShopifyCaptions.PhoneNumber)]
		[ValidateRequired(AutoDefault = true)]
		public virtual string Phone { get; set; }

		/// <summary>
		/// The latitude of the address.
		/// </summary>
		[JsonProperty("latitude", NullValueHandling = NullValueHandling.Ignore)]
		[Description(ShopifyCaptions.Latitude)]
		public virtual string Latitude { get; set; }

		/// <summary>
		/// The longitude of the address.
		/// </summary>
		[JsonProperty("longitude", NullValueHandling = NullValueHandling.Ignore)]
		[Description(ShopifyCaptions.Longitude)]
		public virtual string Longitude { get; set; }

		/// <summary>
		/// The ID for the shipping zone that the address belongs to.
		/// </summary>
		[JsonProperty("shipping_zone_id")]
		[Description(ShopifyCaptions.ShippingZoneId)]
		[JsonIgnore]
		public long? ShippingZoneId { get; set; }
	}

}
