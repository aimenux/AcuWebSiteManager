using Newtonsoft.Json;
using PX.Commerce.Core;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PX.Commerce.BigCommerce.API.REST
{
	[JsonObject(Description = "Customer Group")]
	[Description(BigCommerceCaptions.CustomerGroup)]
	public class CustomerGroupData : BCAPIEntity
	{
		[JsonProperty("id")]
		public virtual int? Id { get; set; }

		[JsonProperty("name")]
		[Description(BigCommerceCaptions.Name)]
		public virtual string Name { get; set; }

		[JsonProperty("is_default")]
		[Description(BigCommerceCaptions.IsDefault)]
		public virtual bool IsDefault { get; set; }

		[JsonProperty("category_access")]
		[Description(BigCommerceCaptions.CategoryAccess)]
		public virtual CategoryAccess CategoryAccess { get; set; }

		[JsonProperty("discount_rules")]
		[Description(BigCommerceCaptions.DiscountRule)]
		public virtual List<DiscountRule> DiscountRule { get; set; }
	}

	public class DiscountRule
	{
		[JsonProperty("type")]
		[Description(BigCommerceCaptions.Type)]
		public virtual string Type { get; set; }

		[JsonProperty("method")]
		[Description(BigCommerceCaptions.Method)]
		public virtual string Method { get; set; }

		[JsonProperty("amount")]
		[Description(BigCommerceCaptions.Amount)]
		public virtual string Amount { get; set; }

		[JsonProperty("price_list_id")]
		[Description(BigCommerceCaptions.PriceListId)]
		public virtual int? PriceListId { get; set; }
	}

	public class CategoryAccess
	{
		[JsonProperty("type")]
		[Description(BigCommerceCaptions.Type)]
		public virtual string Type { get; set; }

		[JsonProperty("categories")]
		[Description(BigCommerceCaptions.Categories)]
		public virtual string[] Categories { get; set; }
	}
}
