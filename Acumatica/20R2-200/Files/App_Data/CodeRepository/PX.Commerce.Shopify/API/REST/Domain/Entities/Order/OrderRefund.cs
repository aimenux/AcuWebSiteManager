using System;
using System.Collections.Generic;
using System.ComponentModel;
using Newtonsoft.Json;
using PX.Commerce.Core;

namespace PX.Commerce.Shopify.API.REST
{
	public class OrderRefundResponse : IEntityResponse<OrderRefund>
	{
		[JsonProperty("refund")]
		public OrderRefund Data { get; set; }
	}

	public class OrderRefundsResponse : IEntitiesResponse<OrderRefund>
	{
		[JsonProperty("refunds")]
		public IEnumerable<OrderRefund> Data { get; set; }
	}

	[JsonObject(Description = "Order Refund")]
	[Description(ShopifyCaptions.Refund)]
	public class OrderRefund : BCAPIEntity
	{
		/// <summary>
		/// [READ-ONLY] The date and time (ISO 8601 format) when the refund was created.
		/// </summary>
		[JsonProperty("created_at")]
		[ShouldNotSerialize]
		public DateTime? DateCreatedAt { get; set; }

		/// <summary>
		///  [READ-ONLY] The unique identifier for the refund.
		/// </summary>
		[JsonProperty("id", NullValueHandling = NullValueHandling.Ignore)]
		[Description(ShopifyCaptions.Id)]
		public long? Id { get; set; }

		/// <summary>
		///  [READ-ONLY] The unique identifier for the refund.
		/// </summary>
		[JsonProperty("order_id", NullValueHandling = NullValueHandling.Ignore)]
		[Description(ShopifyCaptions.OrderId)]
		public long? OrderId { get; set; }

		/// <summary>
		/// An optional note attached to a refund.
		/// </summary>
		[JsonProperty("note", NullValueHandling = NullValueHandling.Ignore)]
		[Description(ShopifyCaptions.Note)]
		public String Note { get; set; }

		/// <summary>
		/// [READ-ONLY] An optional note attached to a refund.
		/// </summary>
		[JsonProperty("order_adjustments", NullValueHandling = NullValueHandling.Ignore)]
		[Description(ShopifyCaptions.OrderAdjustment)]
		[ShouldNotSerialize]
		public List<OrderAdjustment> OrderAdjustments { get; set; }

		/// <summary>
		/// The date and time (ISO 8601 format) when the refund was imported. 
		/// This value can be set to a date in the past when importing from other systems. 
		/// If no value is provided, then it will be auto-generated as the current time in Shopify.
		/// </summary>
		[JsonProperty("processed_at", NullValueHandling = NullValueHandling.Ignore)]
		[Description(ShopifyCaptions.ProcessedAt)]
		public DateTime? ProcessedAt { get; set; }

		/// <summary>
		/// A list of refunded line items. 
		/// </summary>
		[JsonProperty("refund_line_items", NullValueHandling = NullValueHandling.Ignore)]
		[Description(ShopifyCaptions.RefundItem)]
		[ShouldNotSerialize]
		public List<RefundLineItem> RefundLineItems { get; set; }

		/// <summary>
		/// A list of transactions involved in the refund. 
		/// </summary>
		[JsonProperty("transactions", NullValueHandling = NullValueHandling.Ignore)]
		[Description(ShopifyCaptions.Transactions)]
		public List<OrderTransaction> Transactions { get; set; }

		/// <summary>
		///  The unique identifier of the user who performed the refund.
		/// </summary>
		[JsonProperty("user_id", NullValueHandling = NullValueHandling.Ignore)]
		[Description(ShopifyCaptions.UserId)]
		public long? UserId { get; set; }
	}

}
