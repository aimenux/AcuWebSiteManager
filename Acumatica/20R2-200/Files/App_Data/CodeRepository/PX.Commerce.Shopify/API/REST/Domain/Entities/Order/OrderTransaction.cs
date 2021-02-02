﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using Newtonsoft.Json;
using PX.Commerce.Core;

namespace PX.Commerce.Shopify.API.REST
{
	public class OrderTransactionResponse : IEntityResponse<OrderTransaction>
	{
		[JsonProperty("transaction")]
		public OrderTransaction Data { get; set; }
	}

	public class OrderTransactionsResponse : IEntitiesResponse<OrderTransaction>
	{
		[JsonProperty("transactions")]
		public IEnumerable<OrderTransaction> Data { get; set; }
	}

	[JsonObject(Description = "Order Transaction")]
	[Description(ShopifyCaptions.OrdersTransaction)]
	public class OrderTransaction : BCAPIEntity
	{
        private DateTime? _dateModifiedAt;
        /// <summary>
        /// The amount of money included in the transaction. If you don't provide a value for `amount`, then it defaults to the total cost of the order (even if a previous transaction has been made towards it).
        /// </summary>
        [JsonProperty("amount", NullValueHandling = NullValueHandling.Ignore)]
		[Description(ShopifyCaptions.Amount)]
		public decimal? Amount { get; set; }

		/// <summary>
		/// The authorization code associated with the transaction.
		/// </summary>
		[JsonProperty("authorization", NullValueHandling = NullValueHandling.Ignore)]
		[Description(ShopifyCaptions.Authorization)]
		public String Authorization { get; set; }

		/// <summary>
		/// [READ-ONLY] The date and time (ISO 8601 format) when the transaction was created.
		/// </summary>
		[JsonProperty("created_at")]
		[Description(ShopifyCaptions.DateCreated)]
		[ShouldNotSerialize]
		public DateTime? DateCreatedAt { get; set; }

        [ShouldNotSerialize]
        public DateTime? DateModifiedAt { get { return _dateModifiedAt == null ? DateCreatedAt : _dateModifiedAt; } set { _dateModifiedAt = value; } }

        /// <summary>
        /// The three-letter code (ISO 4217 format) for the currency used for the payment.
        /// </summary>
        [JsonProperty("currency", NullValueHandling = NullValueHandling.Ignore)]
		[Description(ShopifyCaptions.Currency)]
		public string Currency { get; set; }

		/// <summary>
		///  [READ-ONLY] The ID for the device.
		/// </summary>
		[JsonProperty("device_id", NullValueHandling = NullValueHandling.Ignore)]
		[Description(ShopifyCaptions.DeviceId)]
		[ShouldNotSerialize]
		public long? DeviceId { get; set; }

		/// <summary>
		/// [READ-ONLY] A standardized error code, independent of the payment provider. 
		/// </summary>
		[JsonProperty("error_code")]
		[Description(ShopifyCaptions.ErrorCode)]
		[ShouldNotSerialize]
		public TransactionErrorCode? ErrorCode { get; set; }

		/// <summary>
		/// The name of the gateway the transaction was issued through. A list of gateways can be found on Shopify's payment gateways page.
		/// </summary>
		[JsonProperty("gateway", NullValueHandling = NullValueHandling.Ignore)]
		[Description(ShopifyCaptions.Gateway)]
		public string Gateway { get; set; }

		/// <summary>
		///  [READ-ONLY] The ID for the transaction.
		/// </summary>
		[JsonProperty("id", NullValueHandling = NullValueHandling.Ignore)]
		[Description(ShopifyCaptions.Id)]
		public long? Id { get; set; }

		/// <summary>
		/// The transaction's type.
		/// </summary>
		[JsonProperty("kind")]
		[Description(ShopifyCaptions.Kind)]
		[ValidateRequired(AutoDefault = false)]
		public TransactionType Kind { get; set; }

		/// <summary>
		///  [READ-ONLY] The ID of the physical location where the transaction was processed.
		/// </summary>
		[JsonProperty("location_id")]
		[Description(ShopifyCaptions.LocationId)]
		public long? LocationId { get; set; }

		/// <summary>
		/// [READ-ONLY] A string generated by the payment provider with additional information about why the transaction succeeded or failed. 
		/// </summary>
		[JsonProperty("message")]
		[Description(ShopifyCaptions.Message)]
		[ShouldNotSerialize]
		public string Message { get; set; }

		/// <summary>
		///  The ID for the order that the transaction is associated with.
		/// </summary>
		[JsonProperty("order_id", NullValueHandling = NullValueHandling.Ignore)]
		[Description(ShopifyCaptions.OrderId)]
		public long? OrderId { get; set; }

		/// <summary>
		/// Information about the credit card used for this transaction.
		/// </summary>
		[JsonProperty("payment_details")]
		[Description(ShopifyCaptions.PaymentDetail)]
		[ShouldNotSerialize]
		public PaymentDetail PaymentDetail { get; set; }

		/// <summary>
		///  The ID of an associated transaction.
		///  For capture transactions, the parent needs to be an authorization transaction.
		///  For void transactions, the parent needs to be an authorization transaction.
		///  For refund transactions, the parent needs to be a capture or sale transaction.
		/// </summary>
		[JsonProperty("parent_id", NullValueHandling = NullValueHandling.Ignore)]
		[Description(ShopifyCaptions.ParentId)]
		public long? ParentId { get; set; }

		/// <summary>
		/// The date and time (ISO 8601 format) when a transaction was processed. 
		/// This value is the date that's used in the analytic reports. By default, it matches the created_at value. 
		/// If you're importing transactions from an app or another platform, then you can set processed_at to a date and time in the past to match when the original transaction was processed.
		/// </summary>
		[JsonProperty("processed_at", NullValueHandling = NullValueHandling.Ignore)]
		[Description(ShopifyCaptions.ProcessedAt)]
		public DateTime? ProcessedAt { get; set; }

		/// <summary>
		/// A transaction receipt attached to the transaction by the gateway. The value of this field depends on which gateway the shop is using.
		/// </summary>
		[JsonProperty("receipt")]
		[Description(ShopifyCaptions.Receipt)]
		[ShouldNotSerialize]
		public object Receipt { get; set; }

		/// <summary>
		/// [READ-ONLY] The origin of the transaction. This is set by Shopify and can't be overridden. Example values: web, pos, iphone, and android.
		/// </summary>
		[JsonProperty("source_name")]
		[Description(ShopifyCaptions.SourceName)]
		[ShouldNotSerialize]
		public string SourceName { get; set; }

		/// <summary>
		/// The status of the transaction. Valid values: pending, failure, success, and error.
		/// </summary>
		[JsonProperty("status", NullValueHandling = NullValueHandling.Ignore)]
		[Description(ShopifyCaptions.Status)]
		public TransactionStatus? Status { get; set; }

		/// <summary>
		/// Whether the transaction is a test transaction.
		/// </summary>
		[JsonProperty("test", NullValueHandling = NullValueHandling.Ignore)]
		[Description(ShopifyCaptions.TestTransaction)]
		public bool? IsTestTransaction { get; set; }

		/// <summary>
		///  The ID for the user who was logged into the Shopify POS device when the order was processed, if applicable.
		/// </summary>
		[JsonProperty("user_id", NullValueHandling = NullValueHandling.Ignore)]
		[Description(ShopifyCaptions.UserId)]
		public long? UserId { get; set; }
	}

}
