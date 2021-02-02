using Newtonsoft.Json;
using PX.Commerce.Core;
using System.ComponentModel;
using System.Collections.Generic;

namespace PX.Commerce.BigCommerce.API.REST
{
	[JsonObject(Description = "Customer address list (BigCommerce API v3 response)")]
	public class CustomerAddressSingle : IEntityResponse<CustomerAddressData>
	{
		[JsonProperty("data")]
		public CustomerAddressData Data { get; set; }

		[JsonProperty("meta")]
		public Meta Meta { get; set; }

	}
	[JsonObject(Description = "Customer address list (BigCommerce API v3 response)")]
	public class CustomerAddressList : IEntitiesResponse<CustomerAddressData>
	{
		public CustomerAddressList()
		{
			Data = new List<CustomerAddressData>();
		}

		[JsonProperty("data")]
		public List<CustomerAddressData> Data { get; set; }

		[JsonProperty("meta")]
		public Meta Meta { get; set; }

	}

	[JsonObject(Description = "Customer -> Customer Address")]
	[Description(BigCommerceCaptions.CustomerAddressData)]
	public class CustomerAddressData : BCAPIEntity
	{
        [JsonProperty("id")]
        public virtual int? Id { get; set; }

        [JsonProperty("customer_id")]
		public virtual int? CustomerId { get; set; }

        [JsonProperty("first_name")]
		[Description(BigCommerceCaptions.FirstName)]
		[ValidateRequired()]
		public virtual string FirstName { get; set; }

        [JsonProperty("last_name")]
		[Description(BigCommerceCaptions.LastName)]
		public virtual string LastName { get; set; }

        [JsonProperty("company")]
		[Description(BigCommerceCaptions.CompanyName)]
		public virtual string Company { get; set; }

		[JsonProperty("address1")]
		[Description(BigCommerceCaptions.AddressLine1)]
		[ValidateRequired(AutoDefault = true)]
		public string Address1 { get; set; }

		[JsonProperty("address2")]
		[Description(BigCommerceCaptions.AddressLine2)]
		public string Address2 { get; set; }
		
		[JsonProperty("city")]
		[Description(BigCommerceCaptions.City)]
		[ValidateRequired(AutoDefault = true)]
		public virtual string City { get; set; }

        [JsonProperty("state_or_province")]
		[Description(BigCommerceCaptions.State)]
		[ValidateRequired(AutoDefault = true)]
		public virtual string State { get; set; }

		[JsonProperty("postal_code")]
		[Description(BigCommerceCaptions.PostalCode)]
		[ValidateRequired(AutoDefault = true)]
		public virtual string PostalCode { get; set; }

		[JsonProperty("country")]
		[Description(BigCommerceCaptions.Country)]
		[ValidateRequired()]
		public virtual string Country { get; set; }

        [JsonProperty("country_code")]
		[Description(BigCommerceCaptions.CountryIso2)]
        public virtual string CountryCode { get; set; }

		[JsonProperty("phone")]
		[Description(BigCommerceCaptions.PhoneNumber)]
		[ValidateRequired(AutoDefault = true)]
		public virtual string Phone { get; set; }

        [JsonProperty("form_fields")]
        [Description(BigCommerceCaptions.FormFields)]
		[BCCustomerFormFields]
        public IList<CustomerFormFieldData> FormFields { get; set; }
    }
}
