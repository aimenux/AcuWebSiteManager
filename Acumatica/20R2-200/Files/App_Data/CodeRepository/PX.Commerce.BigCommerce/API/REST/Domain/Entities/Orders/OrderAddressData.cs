using Newtonsoft.Json;
using System.ComponentModel;

namespace PX.Commerce.BigCommerce.API.REST
{
    [JsonObject(Description = "Order -> Billing Address")]
    public class OrderAddressData
    {
        /// <summary>
        /// The first name of the addressee
        /// 
        /// [string(255)]
        /// </summary>
        [JsonProperty("first_name")]
		[Description(BigCommerceCaptions.FirstName)]
		public virtual string FirstName { get; set; }

        /// <summary>
        /// The last name of the addressee.
        /// 
        /// [string(255)]
        /// </summary>
        [JsonProperty("last_name")]
		[Description(BigCommerceCaptions.LastName)]
		public virtual string LastName { get; set; }

        /// <summary>
        /// The company of the addressee.
        /// 
        /// [string(100)]
        /// </summary>
        [JsonProperty("company")]
		[Description(BigCommerceCaptions.CompanyName)]
		public virtual string Company { get; set; }

        /// <summary>
        /// The first street line of the address.
        /// 
        /// [string(255)]
        /// </summary>
        [JsonProperty("street_1")]
		[Description(BigCommerceCaptions.Street1)]
		public virtual string Street1 { get; set; }

        /// <summary>
        /// The second street line of the address.
        /// 
        /// [string(255)]
        /// </summary>
        [JsonProperty("street_2")]
		[Description(BigCommerceCaptions.Street2)]
		public virtual string Street2 { get; set; }

        /// <summary>
        /// The city or suburb of the address.
        /// 
        /// [string(50)]
        /// </summary>
        [JsonProperty("city")]
		[Description(BigCommerceCaptions.City)]
		public virtual string City { get; set; }

        /// <summary>
        /// The state of the address.
        /// 
        /// [string(50)]
        /// </summary>
        [JsonProperty("state")]
		[Description(BigCommerceCaptions.State)]
		public virtual string State { get; set; }

        /// <summary>
        /// The zip or postcode of the address.
        /// 
        /// [string(50)]
        /// </summary>
        [JsonProperty("zip")]
		[Description(BigCommerceCaptions.Zipcode)]
		public virtual string ZipCode { get; set; }

        /// <summary>
        /// The country of the address.
        /// 
        /// [string(50)]
        /// </summary>
        [JsonProperty("country")]
		[Description(BigCommerceCaptions.Country)]
		public virtual string Country { get; set; }

        /// <summary>
        /// The country code of the country field.
        /// 
        /// [string(50)]
        /// </summary>
        [JsonProperty("country_iso2")]
        public virtual string CountryIso2 { get; set; }

        /// <summary>
        /// The phone number of the addressee.
        /// 
        /// [string(50)]
        /// </summary>

        [JsonProperty("phone")]
		[Description(BigCommerceCaptions.PhoneNumber)]
		public virtual string Phone { get; set; }

        /// <summary>
        /// The email address of the addressee.
        /// 
        /// [string(255)]
        /// </summary>
        [JsonProperty("email")]
		[Description(BigCommerceCaptions.EmailAddress)]
		public virtual string Email { get; set; }        
    }
}