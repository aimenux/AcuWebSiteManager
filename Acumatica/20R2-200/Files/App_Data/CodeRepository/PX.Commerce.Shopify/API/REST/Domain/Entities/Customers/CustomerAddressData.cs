using Newtonsoft.Json;
using PX.Commerce.Core;
using System.ComponentModel;
using System.Collections.Generic;

namespace PX.Commerce.Shopify.API.REST
{
	public class CustomerAddressResponse : IEntityResponse<CustomerAddressData>
	{
		[JsonProperty("customer_address")]
		public CustomerAddressData Data { get; set; }
	}
	public class CustomerAddressesResponse : IEntitiesResponse<CustomerAddressData>
	{
		[JsonProperty("addresses")]
		public IEnumerable<CustomerAddressData> Data { get; set; }
	}

	[JsonObject(Description = "Customer -> Customer Address")]
	[Description(ShopifyCaptions.CustomerAddressData)]

	public class CustomerAddressData : BCAPIEntity
	{
		/// <summary>
		/// A unique identifier for the address.
		/// </summary>
		[JsonProperty("id", NullValueHandling = NullValueHandling.Ignore)]
		[Description(ShopifyCaptions.LocationId)]
		public virtual long? Id { get; set; }

		/// <summary>
		/// A unique identifier for the customer where the address attaches to
		/// </summary>
		[JsonProperty("customer_id", NullValueHandling = NullValueHandling.Ignore)]
		[Description(ShopifyCaptions.CustomerId)]
		public virtual long? CustomerId { get; set; }

		/// <summary>
		/// The customer’s first name.
		/// </summary>
		[JsonProperty("first_name", NullValueHandling = NullValueHandling.Ignore)]
		[Description(ShopifyCaptions.FirstName)]
		public virtual string FirstName { get; set; }

		/// <summary>
		/// The customer’s last name.
		/// </summary>
		[JsonProperty("last_name", NullValueHandling = NullValueHandling.Ignore)]
		[Description(ShopifyCaptions.LastName)]
		public virtual string LastName { get; set; }

		/// <summary>
		/// The customer’s first and last names.
		/// </summary>
		[JsonProperty("name", NullValueHandling = NullValueHandling.Ignore)]
		[Description(ShopifyCaptions.Name)]
		public virtual string Name { get; set; }

		/// <summary>
		/// The customer’s company.
		/// </summary>
		[JsonProperty("company", NullValueHandling = NullValueHandling.Ignore)]
		[Description(ShopifyCaptions.CompanyName)]
		public virtual string Company { get; set; }

		/// <summary>
		/// The customer's mailing address
		/// </summary>
		[JsonProperty("address1", NullValueHandling = NullValueHandling.Ignore)]
		[Description(ShopifyCaptions.AddressLine1)]
		public string Address1 { get; set; }

		/// <summary>
		/// An additional field for the customer's mailing address.
		/// </summary>
		[JsonProperty("address2", NullValueHandling = NullValueHandling.Ignore)]
		[Description(ShopifyCaptions.AddressLine2)]
		[ValidateRequired(AutoDefault = true)]
		public string Address2 { get; set; }

		/// <summary>
		/// The customer's city, town, or village.
		/// </summary>
		[JsonProperty("city", NullValueHandling = NullValueHandling.Ignore)]
		[Description(ShopifyCaptions.City)]
		[ValidateRequired(AutoDefault = true)]
		public virtual string City { get; set; }

		/// <summary>
		/// The customer’s region name. Typically a province, a state, or a prefecture.
		/// </summary>
		[JsonProperty("province", NullValueHandling = NullValueHandling.Ignore)]
		[Description(ShopifyCaptions.Province)]
		[ValidateRequired(AutoDefault = true)]
		public virtual string Province { get; set; }

		/// <summary>
		/// The customer’s postal code, also known as zip, postcode, Eircode, etc.
		/// </summary>
		[JsonProperty("zip", NullValueHandling = NullValueHandling.Ignore)]
		[Description(ShopifyCaptions.PostalCode)]
		[ValidateRequired(AutoDefault = true)]
		public virtual string PostalCode { get; set; }

		/// <summary>
		/// The customer's country.
		/// </summary>
		[JsonProperty("country", NullValueHandling = NullValueHandling.Ignore)]
		[Description(ShopifyCaptions.Country)]
		[ValidateRequired()]
		public virtual string Country { get; set; }

		/// <summary>
		/// The customer’s normalized country name.
		/// </summary>
		[JsonProperty("country_name", NullValueHandling = NullValueHandling.Ignore)]
		[Description(ShopifyCaptions.CountryName)]
		[ValidateRequired()]
		public virtual string CountryName { get; set; }

		/// <summary>
		/// The two-letter country code corresponding to the customer's country.
		/// </summary>
		[JsonProperty("country_code", NullValueHandling = NullValueHandling.Ignore)]
		[Description(ShopifyCaptions.CountryISOCode)]
		public string CountryCode { get; set; }

		/// <summary>
		/// The two-letter code for the customer’s region.
		/// </summary>
		[JsonProperty("province_code", NullValueHandling = NullValueHandling.Ignore)]
		[Description(ShopifyCaptions.ProvinceCode)]
		public string ProvinceCode { get; set; }

		/// <summary>
		/// The customer’s phone number at this address.
		/// </summary>
		[JsonProperty("phone", NullValueHandling = NullValueHandling.Ignore)]
		[Description(ShopifyCaptions.PhoneNumber)]
		[ValidateRequired(AutoDefault = true)]
		public virtual string Phone { get; set; }

		/// <summary>
		/// Whether this address is the default address for the customer.
		/// </summary>
		[JsonProperty("default", NullValueHandling = NullValueHandling.Ignore)]
		[Description(ShopifyCaptions.IsDefault)]
		public virtual bool? Default { get; set; }
	}
}
