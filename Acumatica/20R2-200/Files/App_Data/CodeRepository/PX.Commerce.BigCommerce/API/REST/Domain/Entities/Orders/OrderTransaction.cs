using System;
using PX.Commerce.BigCommerce.API.REST;
using Newtonsoft.Json;
using PX.Commerce.Core;
using System.ComponentModel;

namespace PX.Commerce.BigCommerce.API.REST
{
	[Description(BigCommerceCaptions.OrdersTransactionData)]
    public class OrdersTransactionData : BCAPIEntity
	{
        [JsonProperty("id")]
        public int Id { get; set; }

        [JsonProperty("order_id")]
		[Description(BigCommerceCaptions.OrderId)]
        public string OrderId { get; set; }

        [JsonProperty("event")]
        public OrderPaymentEvent Event { get; set; }

		// Allowed values: "credit_card", "electronic_wallet", "apple_pay_card", "apple_pay_token", "store_credit", "gift_certificate", "custom", "token", "nonce", "offsite", "offline"
		[JsonProperty("method")]
		[Description(BigCommerceCaptions.PaymentMethod)]		
		public String PaymentMethod { get; set; }

        [JsonProperty("amount")]
		[Description(BigCommerceCaptions.Amount)]
		public double Amount { get; set; }

        [JsonProperty("currency")]
		[Description(BigCommerceCaptions.Currency)]
		public string Currency { get; set; }

        [JsonProperty("gateway")]
		[Description(BigCommerceCaptions.Gateway)]
		public string Gateway { get; set; }

        [JsonProperty("gateway_transaction_id")]
		[Description(BigCommerceCaptions.GatewayTranscationId)]
        public string GatewayTransactionId { get; set; }

        [JsonProperty("date_created")]
		[Description(BigCommerceCaptions.DateCreatedUT)]
		public virtual string DateCreatedUT { get; set; }

		[JsonProperty("test")]
        public bool Test { get; set; }

        [JsonProperty("status")]
		[Description(BigCommerceCaptions.OrderPaymentStatus)]
		public OrderPaymentStatus Status { get; set; }

        [JsonProperty("fraud_review")]
        public bool FraudReview { get; set; }

        [JsonProperty("reference_transaction_id")]
        public int? ReferenceTransactionId { get; set; }

        [JsonProperty("offline")]
		[Description(BigCommerceCaptions.OfflinePayment)]
		public OfflinePayment OfflinePayment { get; set; }

        [JsonProperty("custom")]
		[Description(BigCommerceCaptions.CustomPayment)]
		public CustomPayment CustomPayment { get; set; }

        [JsonProperty("payment_instrument_token")]
        public string PaymentInstrumentToken { get; set; }

        [JsonProperty("avs_result")]
		[Description(BigCommerceCaptions.AvsResult)]
		public AvsResult AvsResult { get; set; }

        [JsonProperty("cvv_result")]
		[Description(BigCommerceCaptions.CvvResult)]
		public CvvResult CvvResult { get; set; }

        [JsonProperty("credit_card")]
		[Description(BigCommerceCaptions.CreditCard)]
		public CreditCard CreditCard { get; set; }

        [JsonProperty("gift_certificate")]
		[Description(BigCommerceCaptions.GiftCertificate)]
		public GiftCertificate GiftCertificate { get; set; }

        [JsonProperty("store_credit")]
		[Description(BigCommerceCaptions.StoreCredit)]
		public StoreCredit StoreCredit { get; set; }

		[JsonIgnore]
		public string OrderPaymentMethod { get; set; }
	}
}
