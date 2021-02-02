using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using PX.Commerce.Core;
using System.ComponentModel;

namespace PX.Commerce.BigCommerce.API.REST
{
	[JsonObject(Description = "Customer list (BigCommerce API v3 response)")]
	public class CustomerSingle: IEntityResponse<CustomerData>
	{
		[JsonProperty("data")]
		public CustomerData Data { get; set; }

		[JsonProperty("meta")]
		public Meta Meta { get; set; }
	}
	[JsonObject(Description = "Customer list (BigCommerce API v3 response)")]
	public class CustomerList : IEntitiesResponse<CustomerData>
	{
		public CustomerList()
		{
			Data = new List<CustomerData>();
		}

		[JsonProperty("data")]
		public List<CustomerData> Data { get; set; }

		[JsonProperty("meta")]
		public Meta Meta { get; set; }		
	}

	[JsonObject(Description = "Customer")]
	[Description(BigCommerceCaptions.Customer)]
	public class CustomerData: BCAPIEntity
	{
        public CustomerData()
        {
            Addresses = new List<CustomerAddressData>();
        }

        [JsonProperty("id")]
        public virtual int? Id { get; set; }

        [JsonProperty("company")]
		[Description(BigCommerceCaptions.CompanyName)]
        public virtual string Company { get; set; }

        [JsonProperty("first_name")]
		[Description(BigCommerceCaptions.FirstName)]
		[ValidateRequired]
		public virtual string FirstName { get; set; }

        [JsonProperty("last_name")]
		[Description(BigCommerceCaptions.LastName)]
		[ValidateRequired()]
		public virtual string LastName { get; set; }

        [JsonProperty("email")]
		[Description(BigCommerceCaptions.EmailAddress)]
		[ValidateRequired()]
		public virtual string Email { get; set; }

        [JsonProperty("phone")]
		[Description(BigCommerceCaptions.PhoneNumber)]
		[ValidateRequired(AutoDefault = true)]
        public virtual string Phone { get; set; }

        [JsonProperty("date_created")]
        public virtual string DateCreatedUT { get; set; }

        [JsonProperty("date_modified")]
        public virtual string DateModifiedUT { get; set; }


        [JsonProperty("registration_ip_address")]
        public virtual string RegistrationIpAddress { get; set; }

        [JsonProperty("customer_group_id")]
		[Description(BigCommerceCaptions.CustomerGroupId)]
		public virtual int? CustomerGroupId { get; set; }

        [JsonProperty("notes")]
        public virtual string Notes { get; set; }

        [JsonProperty("tax_exempt_category")]
        [Description(BigCommerceCaptions.TaxExemptCode)]
        public virtual string TaxExemptCode { get; set; }

        [JsonProperty("accepts_product_review_abandoned_cart_emails")]
        [Description(BigCommerceCaptions.ReceiveACSOrReviewEmails)]
        public virtual bool ReceiveACSOrReviewEmails { get; set; }

        [JsonProperty("authentication")]
        [Description(BigCommerceCaptions.Authentication)]
        public virtual Authentication Authentication { get; set; }
       
        [JsonProperty("addresses")]
		[Description(BigCommerceCaptions.CustomerAddress)]
        public IList<CustomerAddressData> Addresses { get; set; }

        [JsonProperty("form_fields")]
        [Description(BigCommerceCaptions.FormFields)]
		[BCCustomerFormFields]
		public IList<CustomerFormFieldData> FormFields { get; set; }

        public void AddAddresses(CustomerAddressData address)
        {
            Addresses.Add(address);
        }

        public void AddAddresses(List<CustomerAddressData> addresses)
        {
            if (addresses != null)
            {
                ((List<CustomerAddressData>)Addresses).AddRange(addresses);
            }
        }

        //Conditional Serialization
        public bool ShouldSerializeId()
        {
            return Id != null ? true: false;
        }

        public bool ShouldSerializeDateCreatedUT()
        {
            return false;
        }

        public bool ShouldSerializeDateModifiedUT()
        {
            return false;
        }

        public bool ShouldSerializeAddresses()
        {
            return false;
        }
	}

    [JsonObject(Description = "Customer -> Authentication")]
    [Description(BigCommerceCaptions.Authentication)]
    public class Authentication
    {
        [JsonProperty("force_password_reset")]
        [Description(BigCommerceCaptions.ForcePasswordReset)]
        public virtual bool ForcePasswordReset { get; set; }

    }

}
