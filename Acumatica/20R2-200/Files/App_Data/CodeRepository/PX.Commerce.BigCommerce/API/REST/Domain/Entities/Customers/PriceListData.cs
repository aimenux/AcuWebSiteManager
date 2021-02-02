using Newtonsoft.Json;
using PX.Commerce.Core;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;

namespace PX.Commerce.BigCommerce.API.REST
{
	[JsonObject(Description = "PriceList")]
	public class PriceList : BCAPIEntity
	{
		[JsonProperty("id")]
		public int? ID { get; set; }
		public bool ShouldSerializeId()
		{
			return false;
		}

		[JsonProperty("name")]
		public string Name { get; set; }

		public List<PriceListRecord> priceListRecords { get; set; }
		public string ExtrenalPriceClassID { get; set; }

	}

	[JsonObject(Description = "PriceListRecords")]
	public class PriceListRecord : BCAPIEntity
	{
		[JsonProperty("variant_id")]
		public string VariantID { get; set; }

		[JsonProperty("sku")]
		public string SKU { get; set; }

		[JsonProperty("currency")]
		public string Currency { get; set; }

		[JsonProperty("price")]
		public decimal? Price { get; set; }

		[JsonProperty("sale_price")]
		public decimal? SalesPrice { get; set; }

		[JsonProperty("product_id")]
		public int? ProductID { get; set; }
		public bool ShouldSerializeProductID()
		{
			return false;
		}

		[JsonProperty("bulk_pricing_tiers")]
		public List<BulKPricingTier> BulKPricingTier { get; set; }

		public PriceListRecord ShallowCopy()
		{
			return (PriceListRecord)this.MemberwiseClone();
		}
	}

	public class BulKPricingTier : BCAPIEntity
	{
		[JsonProperty("type")]
		public string Type { get; set; }

		[JsonProperty("quantity_min")]
		public int QuantityMinimum { get; set; }

		[JsonProperty("amount")]
		public decimal? Amount { get; set; }

		public string PriceCode { get; set; }
		
	}

	[JsonObject(Description = "Price list (BigCommerce API v3 response)")]
	public class PriceListResponse : IEntityResponse<PriceList>
	{
		[JsonProperty("data")]
		public PriceList Data { get; set; }

		[JsonProperty("meta")]
		public Meta Meta { get; set; }
	}

	[JsonObject(Description = "List of Pricelist (BigCommerce API v3 response)")]
	public class PriceListsResponse : IEntitiesResponse<PriceList>
	{
		public PriceListsResponse()
		{
			Data = new List<PriceList>();
		}

		[JsonProperty("data")]
		public List<PriceList> Data { get; set; }

		[JsonProperty("meta")]
		public Meta Meta { get; set; }
	}

	[JsonObject(Description = "List of Price list  records(BigCommerce API v3 response)")]
	public class PriceListRecordResponse : IEntitiesResponse<PriceListRecord>
	{
		public PriceListRecordResponse()
		{
			Data = new List<PriceListRecord>();
		}
		[JsonProperty("data")]
		public List<PriceListRecord> Data { get; set; }

		[JsonProperty("meta")]
		public Meta Meta { get; set; }
	}

}
