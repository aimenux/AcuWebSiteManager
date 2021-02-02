using System;
using System.Collections.Generic;
using System.ComponentModel;
using Newtonsoft.Json;
using PX.Commerce.Core;

namespace PX.Commerce.Shopify.API.REST
{
	public class OrderRiskResponse : IEntityResponse<OrderRisk>
	{
		[JsonProperty("risk")]
		public OrderRisk Data { get; set; }
	}

	public class OrderRisksResponse : IEntitiesResponse<OrderRisk>
	{
		[JsonProperty("risks")]
		public IEnumerable<OrderRisk> Data { get; set; }
	}

	[JsonObject(Description = "Order Risk")]
	[Description(ShopifyCaptions.OrderRisk)]
	public class OrderRisk : BCAPIEntity
	{
		/// <summary>
		/// Whether this order risk is severe enough to force the cancellation of the order. 
		/// If true, then this order risk is included in the Order canceled message that's shown on the details page of the canceled order.
		/// Note: Setting this property to true does not cancel the order. 
		/// Use this property only if your app automatically cancels the order using the Order resource. 
		/// If your app doesn't automatically cancel orders based on order risks, then leave this property set to false.
		/// </summary>
		[JsonProperty("cause_cancel", NullValueHandling = NullValueHandling.Ignore)]
		public bool? CauseCancel { get; set; }

		/// <summary>
		/// The ID of the checkout that the order risk belongs to. 
		/// </summary>
		[JsonProperty("checkout_id", NullValueHandling = NullValueHandling.Ignore)]
		public long? CheckoutId { get; set; }

		/// <summary>
		/// Whether the order risk is displayed on the order details page in the Shopify admin. 
		/// If false, then this order risk is ignored when Shopify determines your app's overall risk level for the order.
		/// </summary>
		[JsonProperty("display", NullValueHandling = NullValueHandling.Ignore)]
		public bool? Display { get; set; }

		/// <summary>
		/// A unique numeric identifier for the order risk.
		/// </summary>
		[JsonProperty("id", NullValueHandling = NullValueHandling.Ignore)]
		[Description(ShopifyCaptions.Id)]
		public long? Id { get; set; }

		/// <summary>
		/// The ID of the order that the order risk belongs to.
		/// </summary>
		[JsonProperty("order_id", NullValueHandling = NullValueHandling.Ignore)]
		[Description(ShopifyCaptions.OrderId)]
		public long? OrderId { get; set; }

		/// <summary>
		/// The message that's displayed to the merchant to indicate the results of the fraud check. 
		/// The message is displayed only if display is set totrue.
		/// </summary>
		[JsonProperty("message", NullValueHandling = NullValueHandling.Ignore)]
		[Description(ShopifyCaptions.Message)]
		public string Message { get; set; }

		/// <summary>
		/// The recommended action given to the merchant. Valid values:
		/// cancel: There is a high level of risk that this order is fraudulent. The merchant should cancel the order.
		/// investigate: There is a medium level of risk that this order is fraudulent. The merchant should investigate the order.
		/// accept: There is a low level of risk that this order is fraudulent. The order risk found no indication of fraud.
		/// </summary>
		[JsonProperty("recommendation")]
		[Description(ShopifyCaptions.Recommendation)]
		public OrderRiskActionType Recommendation { get; set; }

		/// <summary>
		/// For internal use only. A number between 0 and 1 that's assigned to the order. 
		/// The closer the score is to 1, the more likely it is that the order is fraudulent.
		/// </summary>
		[JsonProperty("score", NullValueHandling = NullValueHandling.Ignore)]
		[Description(ShopifyCaptions.Score)]
		public decimal Score { get; set; }

		/// <summary>
		/// The source of the order risk.
		/// </summary>
		[JsonProperty("source", NullValueHandling = NullValueHandling.Ignore)]
		public string Source { get; set; }
	}
}
