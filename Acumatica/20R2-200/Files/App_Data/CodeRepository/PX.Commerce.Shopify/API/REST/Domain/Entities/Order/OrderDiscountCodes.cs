using System;
using System.Collections.Generic;
using System.ComponentModel;
using Newtonsoft.Json;
using PX.Commerce.Core;

namespace PX.Commerce.Shopify.API.REST
{
	/// <summary>
	/// A list of discounts applied to the order. Each discount object includes the following properties:
	/// amount: The amount that's deducted from the order total. When you create an order, this value is the percentage or monetary amount to deduct. After the order is created, this property returns the calculated amount.
	/// code: When the associated discount application is of type code, this property returns the discount code that was entered at checkout. Otherwise this property returns the title of the discount that was applied.
	/// type: The type of discount. Default value: fixed_amount.
	/// </summary>
	[JsonObject(Description = "discount_codes")]
	[Description(ShopifyCaptions.Discount)]
	public class OrderDiscountCodes : BCAPIEntity
	{
		/// <summary>
		/// The amount that's deducted from the order total. When you create an order, this value is the percentage or monetary amount to deduct. After the order is created, this property returns the calculated amount.
		/// </summary>
		[JsonProperty("amount")]
		[Description(ShopifyCaptions.Amount)]
		public decimal? Amount { get; set; }

		/// <summary>
		/// When the associated discount application is of type code, this property returns the discount code that was entered at checkout. 
		/// Otherwise this property returns the title of the discount that was applied.
		/// </summary>
		[JsonProperty("code")]
		[Description(ShopifyCaptions.Code)]
		public string Code { get; set; }

		/// <summary>
		/// The type of discount. Default value: fixed_amount.
		/// </summary>
		[JsonProperty("type")]
		[Description(ShopifyCaptions.Type)]
		public DiscountType Type { get; set; }
	}

}
