using Newtonsoft.Json;
using PX.Commerce.Core;
using System.ComponentModel;

namespace PX.Commerce.BigCommerce.API.REST
{
	[Description(BigCommerceCaptions.OrdersShippingAddress)]
	public class OrdersShippingAddressData : BCAPIEntity
	{
        /// <summary>
        /// The ID of the shipping address. 
        /// </summary>
        [JsonProperty("id")]
        public virtual int? Id { get; set; }

        /// <summary>
        /// The ID of the order this shipping address is associated with.
        /// </summary>
        [JsonProperty("order_id")]
        public virtual int? OrderId { get; set; }

        /// <summary>
        /// The first name of the addressee
        /// [string(255)]
        /// </summary>
        [JsonProperty("first_name")]
		[Description(BigCommerceCaptions.FirstName)]
		public virtual string FirstName { get; set; }

        /// <summary>
        /// The last name of the addressee.
        /// [string(255)]
        /// </summary>
        [JsonProperty("last_name")]
		[Description(BigCommerceCaptions.LastName)]
		public virtual string LastName { get; set; }

        /// <summary>
        /// The company of the addressee.
        /// [string(100)]
        /// </summary>
        [JsonProperty("company")]
		[Description(BigCommerceCaptions.CompanyName)]
		public virtual string Company { get; set; }

        /// <summary>
        /// The first street line of the address.
        /// [string(255)]
        /// </summary>
        [JsonProperty("street_1")]
		[Description(BigCommerceCaptions.Street1)]
		public virtual string Street1 { get; set; }

        /// <summary>
        /// The second street line of the address.
        /// [string(255)]
        /// </summary>
        [JsonProperty("street_2")]
		[Description(BigCommerceCaptions.Street2)]
		public virtual string Street2 { get; set; }

        /// <summary>
        /// The city or suburb of the address.
        /// [string(50)]
        /// </summary>
        [JsonProperty("city")]
		[Description(BigCommerceCaptions.City)]
		public virtual string City { get; set; }

        /// <summary>
        /// The zip or postcode of the address.
        /// [string(20)]
        /// </summary>
        [JsonProperty("zip")]
		[Description(BigCommerceCaptions.Zip)]
		public virtual string ZipCode { get; set; }

        /// <summary>
        /// The country of the address.
        /// string(50)
        /// </summary>
        [JsonProperty("country")]
		[Description(BigCommerceCaptions.Country)]
		public virtual string Country { get; set; }

        /// <summary>
        /// The country code of the country field.
        /// string(50)
        /// </summary>
        [JsonProperty("country_iso2")]
		[Description(BigCommerceCaptions.CountryISOCode)]
		public virtual string CountryIso2 { get; set; }

        /// <summary>
        /// The state of the address.
        /// string(50)
        /// </summary>
        [JsonProperty("state")]
		[Description(BigCommerceCaptions.State)]
		public virtual string State { get; set; }

        /// <summary>
        /// The email address of the addressee.
        /// string(255)
        /// </summary>
        [JsonProperty("email")]
		[Description(BigCommerceCaptions.Email)]
		public virtual string Email { get; set; }

        /// <summary>
        /// The phone number of the addressee.
        /// string(50)
        /// </summary>
        [JsonProperty("phone")]
		[Description(BigCommerceCaptions.Phone)]
		public virtual string Phone { get; set; }

        /// <summary>
        /// The total amount of items (sum of the products * quantity) assigned 
        /// to the shipping address. 
        /// </summary>
        [JsonProperty("items_total")]
		[Description(BigCommerceCaptions.ItemsTotal)]
		public virtual int ItemsTotal { get; set; }

        /// <summary>
        /// The base cost of the shipping for the items associated with the shipping address.
        /// decimal(20,4)
        /// </summary>
        [JsonProperty("base_cost")]
		[Description(BigCommerceCaptions.BaseCost)]
		public virtual decimal BaseCost { get; set; }

        /// <summary>
        /// The cost of the shipping excluding tax. 
        /// decimal(20,4)
        /// </summary>
        [JsonProperty("cost_ex_tax")]
		[Description(BigCommerceCaptions.CostExcludingTax)]
		public virtual decimal CostExcludingTax { get; set; }

        /// <summary>
        /// The cost of the shipping including tax. 
        /// 
        /// decimal(20,4)
        /// </summary>
        [JsonProperty("cost_inc_tax")]
		[Description(BigCommerceCaptions.CostIncludingTax)]
		public virtual decimal CostIncludingTax { get; set; }

        /// <summary>
        /// The amount of tax on the shipping cost. 
        /// decimal(20,4)
        /// </summary>
        [JsonProperty("cost_tax")]
		[Description(BigCommerceCaptions.CostTax)]
		public virtual decimal CostTax { get; set; }

        /// <summary>
        /// The name of the shipping method. 
        /// string(250) 
        /// </summary>
        [JsonProperty("shipping_method")]
		[Description(BigCommerceCaptions.ShippingMethod)]
		public virtual string ShippingMethod { get; set; }

        /// <summary>
        /// The ID of the tax class used to tax the shipping cost. 
        /// </summary>
        [JsonProperty("cost_tax_class_id")]
		[Description(BigCommerceCaptions.CostTaxClassId)]
		public virtual int CostTaxClassId { get; set; }

        /// <summary>
        /// The base handling cost.
        /// decimal(20,4)
        /// </summary>
        [JsonProperty("base_handling_cost")]
		[Description(BigCommerceCaptions.BaseHandlingCost)]
		public virtual decimal BaseHandlingCost { get; set; }

        /// <summary>
        /// The handling cost excluding tax. 
        /// 
        /// decimal(20,4)
        /// </summary>
        [JsonProperty("handling_cost_ex_tax")]
		[Description(BigCommerceCaptions.HandlingCostExcludingTax)]
		public virtual decimal HandlingCostExcludingTax { get; set; }

        /// <summary>
        /// The handling cost including tax. 
        /// decimal(20,4)
        /// </summary>
        [JsonProperty("handling_cost_inc_tax")]
		[Description(BigCommerceCaptions.HandlingCostIncludingTax)]
		public virtual decimal HandlingCostIncludingTax { get; set; }

        /// <summary>
        /// The amount of tax on the handling cost. 
        /// decimal(20,4)
        /// </summary>
        [JsonProperty("handling_cost_tax")]
		[Description(BigCommerceCaptions.HandlingCostTax)]
		public virtual decimal HandlingCostTax { get; set; }

        /// <summary>
        /// The ID of the tax class used to tax the handling cost. 
        /// </summary>
        [JsonProperty("handling_cost_tax_class_id")]
		[Description(BigCommerceCaptions.HandlingCostTaxClassId)]
		public virtual int HandlingCostTaxClassId { get; set; }

        /// <summary>
        /// The ID of the shipping zone the shipping address is associated with. 
        /// </summary>
        [JsonProperty("shipping_zone_id")]
		[Description(BigCommerceCaptions.ShippingZoneId)]
		public virtual int ShippingZoneId { get; set; }

        /// <summary>
        /// The name of the shipping zone the shipping address is associated with. 
        /// string(250)
        /// </summary>
        [JsonProperty("shipping_zone_name")]
		[Description(BigCommerceCaptions.ShippingZoneName)]
		public virtual string ShippingZoneName { get; set; }

        /// <summary>
        /// The quantity of items that have been shipped. 
        /// </summary>
        [JsonIgnore]
        public virtual int ItemsShipped { get; set; }

		[JsonProperty("items_shipped")]
		public virtual string ItemsShipped_InString
        {
            get
            {
                return ItemsShipped.ToString();
            }
            set
            {
                int tmpInt;
                bool wasParsed = int.TryParse(value, out tmpInt);
                if (wasParsed)
                    ItemsShipped = tmpInt;
                else
                    ItemsShipped = 0;
            }
        }

		public bool ShouldSerializeItemsShipped_InString()
		{
			return false;
		}
	}
}
