using System;
using PX.Commerce.BigCommerce.API.REST;
using Newtonsoft.Json;
using PX.Commerce.Core;
using System.ComponentModel;

namespace PX.Commerce.BigCommerce.API.REST
{
	[Description(BigCommerceCaptions.OrdersTax)]
	public class OrdersTaxData : BCAPIEntity
	{
        [JsonProperty("id")]
        public int Id { get; set; }

        [JsonProperty("order_id")]
        public int OrderId { get; set; }

		[JsonProperty("order_address_id")]
        public int OrderAddressId { get; set; }

		[JsonProperty("order_product_id", NullValueHandling = NullValueHandling.Ignore)]
		public decimal OrderProductId { get; set; }

		[JsonProperty("tax_rate_id")]
        public int TaxRateId { get; set; }

		[JsonProperty("tax_class_id")]
		public int TaxClassId { get; set; }

		[JsonProperty("class")]
		public string Class { get; set; }

		[JsonProperty("name")]
		[Description(BigCommerceCaptions.TaxName)]
		public string Name { get; set; }

        [JsonProperty("rate")]
		[Description(BigCommerceCaptions.TaxRate)]
		public decimal Rate { get; set; }

        [JsonProperty("priority")]
		[Description(BigCommerceCaptions.TaxPriority)]
		public int Priority { get; set; }

		[JsonProperty("priority_amount")]
		[Description(BigCommerceCaptions.TaxPriorityAmount)]
		public decimal PriorityAmount { get; set; }

		[JsonProperty("line_amount")]
		[Description(BigCommerceCaptions.TaxLineAmount)]
		public decimal LineAmount { get; set; }

		[JsonProperty("line_item_type", NullValueHandling = NullValueHandling.Ignore)]
		[Description(BigCommerceCaptions.TaxLineItemType)]
		public string LineItemType { get; set; }
	}
}
