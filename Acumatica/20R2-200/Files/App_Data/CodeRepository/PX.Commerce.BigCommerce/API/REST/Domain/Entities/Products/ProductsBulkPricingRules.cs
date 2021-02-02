using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using PX.Commerce.Core;

namespace PX.Commerce.BigCommerce.API.REST
{
    [JsonObject(Description = "Product -> ProductsBulkPricingRules")]
    public class ProductsBulkPricingRules : BCAPIEntity
	{
        /// <summary>
        /// The minimum inclusive quantity of a product to satisfy this rule.Must be greater than or equal to zero.
        /// (optional) integer 
        /// </summary>
        [JsonProperty("quantity_min")]
        public int? QuantityMin { get; set; }

		/// <summary>
		/// The maximum inclusive quantity of a product to satisfy this rule.
		/// Must be greater than the quantity_min value – unless this field has a value of 0 (zero), 
		/// in which case there will be no maximum bound for this rule.
		/// (optional) integer 
		/// </summary>
		[JsonProperty("quantity_max")]
		public int? QuantityMax { get; set; }
		
		/// <summary>
		/// The type of adjustment that is made. Values: 
		/// price – the adjustment amount per product; 
		/// percent – the adjustment as a percentage of the original price; 
		/// fixed – the adjusted absolute price of the product.
		/// (optional) string 
		/// </summary>
		[JsonProperty("type")]
        public string Type { get; set; }

        /// <summary>
        /// The value of the adjustment by the bulk pricing rule.   double float (number)  (optional)  
        /// </summary>
        [JsonProperty("amount")]
		public decimal? Amount { get; set; }

		public ProductsVariantData Variant { get; set; }
		/// <summary>
		/// The ID of the bulk pricing rule. (optional) integer 
		/// </summary>
		[JsonProperty("id")]
        public int? Id { get; set; }
		
		public string ParentId { get; set; }

		public int? SyncID { get; set; }
	}

	public class BulkPricing : IEntityResponse<ProductsBulkPricingRules>
	{
		[JsonProperty("data")]
		public ProductsBulkPricingRules Data { get; set; }

		[JsonProperty("meta")]
		public Meta Meta { get; set; }
	}

	public class BulkPricingWithSalesPrice : BCAPIEntity
	{
		[JsonProperty("bulk_pricing_rules")]
		public List<ProductsBulkPricingRules> Data { get; set; }

		[JsonProperty("sale_price")]
		public decimal? SalePrice { get; set; }

		public ProductsVariantData Variant { get; set; }

		[JsonProperty("id")]
		public int? Id { get; set; }

		[JsonProperty("date_modified")]
		public string DateModifiedUT { get; set; }
	}

	public class BulkPricingListWithSalesPrice : IEntitiesResponse<BulkPricingWithSalesPrice>
	{
		public BulkPricingListWithSalesPrice()
		{
			Data = new List<BulkPricingWithSalesPrice>();
		}
		[JsonProperty("data")]
		public List<BulkPricingWithSalesPrice> Data { get; set; }

		[JsonProperty("meta")]
		public Meta Meta { get; set; }
	}

	public class BulkPricingList : IEntitiesResponse<ProductsBulkPricingRules>
	{
		public BulkPricingList()
		{
			Data = new List<ProductsBulkPricingRules>();
		}
		[JsonProperty("data")]
		public List<ProductsBulkPricingRules> Data { get; set; }

		[JsonProperty("meta")]
		public Meta Meta { get; set; }
	}
}


