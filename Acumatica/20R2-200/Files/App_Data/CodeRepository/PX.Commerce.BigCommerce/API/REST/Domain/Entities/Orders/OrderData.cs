using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using PX.Commerce.Core;
using System.ComponentModel;
using Newtonsoft.Json.Serialization;

namespace PX.Commerce.BigCommerce.API.REST
{
	[JsonObject(Description = "Order")]
	[Description(BigCommerceCaptions.OrderData)]
	public class OrderData : BCAPIEntity
	{
		public OrderData()
		{
			Shipments = new List<OrdersShipmentData>();
			Transactions = new List<OrdersTransactionData>();
			Taxes = new List<OrdersTaxData>();
		}

		/// <summary>
		/// The ID of the order. This is auto-generated for new orders.
		/// </summary>
		[JsonProperty("id")]
		[Description(BigCommerceCaptions.OrderId)]
		public virtual int? Id { get; set; }

		public bool ShouldSerializeId()
		{
			return false;
		}

		[JsonProperty("date_modified")]
		public virtual string DateModifiedUT { get; set; }

		/// <summary>
		/// The amount of tax on the subtotal.
		/// </summary>
		[JsonProperty("subtotal_tax")]
		[Description(BigCommerceCaptions.SubtotalTax)]
		public virtual decimal SubtotalTax { get; set; }

		/// <summary>
		/// The total tax for the order.
		/// </summary>
		[JsonProperty("total_tax")]
		[Description(BigCommerceCaptions.TotalTax)]
		public virtual decimal TotalTax { get; set; }

		/// <summary>
		/// The total tax for the shipping cost.
		/// </summary>
		[JsonProperty("shipping_cost_tax")]
		[Description(BigCommerceCaptions.ShippingCostTax)]
		public virtual decimal ShippingCostTax { get; set; }

		/// <summary>
		/// The ID of the tax class used for taxing shipping costs
		/// </summary>
		[JsonProperty("shipping_cost_tax_class_id")]
		public virtual int ShippingCostTaxClassId { get; set; }


		/// <summary>
		/// The total tax for the handling cost.
		/// </summary>
		[JsonProperty("handling_cost_tax")]
		public virtual decimal HandlingCostTax { get; set; }

		/// <summary>
		/// The ID of the tax class used for taxing handling costs.
		/// </summary>
		[JsonProperty("handling_cost_tax_class_id")]
		public virtual int HandlingCostTaxClassId { get; set; }

		/// <summary>
		/// The total tax for the wrapping cost.
		/// </summary>
		[JsonProperty("wrapping_cost_tax")]
		public virtual decimal WrappingCostTax { get; set; }

		/// <summary>
		/// The ID of the tax class used for taxing wrapping costs.
		/// </summary>
		[JsonProperty("wrapping_cost_tax_class_id")]
		public virtual int WrappingCostTaxClassId { get; set; }

		/// <summary>
		/// The textual description of the status. 
		/// 
		/// [string(100)]
		/// </summary>
		[JsonProperty("status")]
		[Description(BigCommerceCaptions.Status)]
		public virtual string Status { get; set; }

		/// <summary>
		/// The status of the payment. A payment status may be one of the following, 
		/// depending on the payment method used: 
		/// 
		/// authorized
		/// captured
		/// refunded
		/// partially
		/// refunded
		/// voided
		/// </summary>
		/// 
		[JsonProperty("payment_status")]
		public virtual string PaymentStatus { get; set; }

		/// <summary>
		/// The date the order was shipped.
		/// </summary>
		public virtual DateTime? DateShipped { get; set; }

		[JsonProperty("date_shipped")]
		public virtual string DateShippedUT
		{
			get
			{
				return DateShipped.TDToString();
			}
			set
			{
				DateShipped = value.ToDate();
			}
		}

		/// <summary>
		/// The amount of store credit the customer used to pay for the order.
		/// </summary>
		[JsonProperty("store_credit_amount")]
		[Description(BigCommerceCaptions.StoreCreditAmount)]
		public virtual decimal StoreCreditAmount { get; set; }

		/// <summary>
		/// The ID of the currency used by the customer to place the order.
		/// </summary>
		[JsonProperty("currency_id")]
		public virtual int CurrencyId { get; set; }

		/// <summary>
		/// The 3 letter currency code of the currency used to place the order. (Store version 7.3.6+)
		/// 
		/// [string(3)]
		/// </summary>
		[JsonProperty("currency_code")]
		[Description(BigCommerceCaptions.CurrencyCode)]
		public virtual string CurrencyCode { get; set; }

		/// <summary>
		/// The ID of the store's default currency at the time of the order.
		/// </summary>
		[JsonProperty("default_currency_id")]
		public virtual int DefaultCurrencyId { get; set; }

		/// <summary>
		/// The 3 letter currency code of the store's default currency at the time of the order. (Store version 7.3.6+) 
		/// 
		/// [string(3)]
		/// </summary>
		[JsonProperty("default_currency_code")]
		public virtual string DefaultCurrencyCode { get; set; }

		/// <summary>
		/// The exchange rate of the currency used to place the order.
		/// </summary>
		[JsonProperty("currency_exchange_rate")]
		[Description(BigCommerceCaptions.CurrencyExchangeRate)]
		public virtual decimal CurrencyExchangeRate { get; set; }

		/// <summary>
		/// The amount of addresses that items were shipped to for the order.
		/// </summary>
		[JsonProperty("shipping_address_count")]
		public virtual int ShippingAddressCount { get; set; }

		/// <summary>
		/// The total discount given on the order due to applied coupons.
		/// </summary>
		[JsonProperty("coupon_discount")]
		[Description(BigCommerceCaptions.CouponDiscount)]
		public virtual decimal CouponDiscount { get; set; }

		[Description(BigCommerceCaptions.OrdersShipment)]
		public virtual IList<OrdersShipmentData> Shipments { get; set; }
		public bool ShouldSerializeShipments()
		{
			return false;
		}

		[Description(BigCommerceCaptions.OrdersTransaction)]
		public virtual IList<OrdersTransactionData> Transactions { get; set; }
		public bool ShouldSerializeTransactions()
		{
			return false;
		}

		[Description(BigCommerceCaptions.OrdersTax)]
		public virtual IList<OrdersTaxData> Taxes { get; set; }

		public bool ShouldSerializeTaxes()
		{
			return false;
		}

		[JsonProperty("products")]
		[ShouldNotDeserialize]
		public virtual List<OrdersProductData> OrderProducts { get; set; }

		[JsonProperty("shipping_addresses")]
		[ShouldNotDeserialize]
		public virtual List<OrdersShippingAddressData> OrderShippingAddresses { get; set; }

		[JsonProperty("coupons")]
		[ShouldNotDeserialize]
		public virtual List<OrdersCouponData> OrdersCoupons { get; set; }

		public virtual CustomerData Customer { get; set; }

		/// <summary>
		/// The address that the order is billed to.  
		/// Billing Address section below for a definition of this object. 
		/// 
		/// See https://developer.bigcommerce.com/display/API/Orders
		/// </summary>
		[JsonProperty("billing_address")]
		[Description(BigCommerceCaptions.BillingAddress)]
		public virtual OrderAddressData BillingAddress { get; set; }

		/// <summary>
		/// The ID of the customer that placed the order or 0 if it was a guest order.
		/// </summary>
		[JsonProperty("customer_id")]
		[Description(BigCommerceCaptions.CustomerId)]
		public virtual int? CustomerId { get; set; }

		/// <summary>
		/// The status of the order. A list of available statuses can be retrieved 
		/// from Order Statuses
		/// </summary>
		[JsonProperty("status_id")]
		public virtual int StatusId { get; set; }

		[JsonProperty("date_created")]
		[Description(BigCommerceCaptions.DateCreatedUT)]
		public virtual string DateCreatedUT { get; set; }

		/// <summary>
		/// The subtotal of the order excluding tax.
		/// </summary>
		[JsonProperty("subtotal_ex_tax")]
		[Description(BigCommerceCaptions.SubtotalExcludingTax)]
		public virtual decimal SubtotalExcludingTax { get; set; }

		/// <summary>
		/// The subtotal of the order including tax.
		/// </summary>
		[JsonProperty("subtotal_inc_tax")]
		[Description(BigCommerceCaptions.SubtotalIncludingTax)]
		public virtual decimal SubtotalIncludingTax { get; set; }


		/// <summary>
		/// The base shipping cost of the order. The base shipping cost is the 
		/// sum of all of the shipping costs minus any discount amounts to the 
		/// shipping costs. It may be inc or ex tax depending on the "Prices Entered With Tax?" 
		/// tax setting.The base shipping * cost is the sum of all of the base shipping costs on 
		/// each address. A * base shipping cost is a price entered by the store owner, either 
		/// inc or * ex tax.
		/// </summary>
		[JsonProperty("base_shipping_cost")]
		[Description(BigCommerceCaptions.BaseShippingCost)]
		public virtual decimal BaseShippingCost { get; set; }

		/// <summary>
		/// The total shipping cost of the order excluding tax.
		/// </summary>
		[JsonProperty("shipping_cost_ex_tax")]
		[Description(BigCommerceCaptions.ShippingCostExcludingTax)]
		//[JsonProperty("shipping_cost_ex_tax", DefaultValueHandling = DefaultValueHandling.Include)]
		//[DefaultValue(0)]
		public virtual decimal ShippingCostExcludingTax { get; set; }

		/// <summary>
		/// The total shipping cost of the order including tax.
		/// </summary>
		[JsonProperty("shipping_cost_inc_tax")]
		[Description(BigCommerceCaptions.ShippingCostIncludingTax)]
		//[JsonProperty("shipping_cost_inc_tax", DefaultValueHandling = DefaultValueHandling.Include)]
		//[DefaultValue(0)]
		public virtual decimal ShippingCostIncludingTax { get; set; }

		/// <summary>
		/// The base handling cost of the order. The base handling cost is the 
		/// sum of the all the handling costs. It may be inc or ex tax depending 
		/// on the "Prices Entered With Tax?" tax setting.
		/// </summary>
		[JsonProperty("base_handling_cost")]
		public virtual decimal BaseHandlingCost { get; set; }

		/// <summary>
		/// The handling cost of the order excluding tax.
		/// </summary>
		[JsonProperty("handling_cost_ex_tax")]
		public virtual decimal HandlingCostExcludingTax { get; set; }

		/// <summary>
		/// The handing cost of the order including tax.
		/// </summary>
		[JsonProperty("handling_cost_inc_tax")]
		public virtual decimal HandlingCostIncludingTax { get; set; }

		/// <summary>
		/// The base wrapping cost of the order. The base wrapping cost is the sum 
		/// of all the wrapping costs on the items. It may be inc or ex tax depending 
		/// on the "Prices Entered With Tax?" tax setting.
		/// </summary>
		[JsonProperty("base_wrapping_cost")]
		public virtual decimal BaseWrappingCost { get; set; }

		/// <summary>
		/// The wrapping cost of the order including tax.
		/// </summary>
		[JsonProperty("wrapping_cost_ex_tax")]
		public virtual decimal WrappingCostExcludingTax { get; set; }

		/// <summary>
		/// The wrapping cost of the order excluding tax.
		/// </summary>
		[JsonProperty("wrapping_cost_inc_tax")]
		public virtual decimal WrappingCostIncludingTax { get; set; }


		/// <summary>
		/// The total of the order excluding tax.
		/// </summary>
		[JsonProperty("total_ex_tax")]
		public virtual decimal TotalExcludingTax { get; set; }

		/// <summary>
		/// The total of the order including tax.
		/// </summary>
		[JsonProperty("total_inc_tax")]
		public virtual decimal TotalIncludingTax { get; set; }

		/// <summary>
		/// The total quantity of the items (sum of products * quantity) in the order.
		/// </summary>
		[JsonProperty("items_total")]
		[Description(BigCommerceCaptions.ItemsTotal)]
		public virtual int ItemsTotal { get; set; }

		/// <summary>
		/// The quantity of items that have been shipped.
		/// </summary>
		[JsonProperty("items_shipped")]
		[Description(BigCommerceCaptions.ItemShipped)]
		public virtual int ItemsShipped { get; set; }

		/// <summary>
		/// The payment method used for the order.
		/// 
		/// [string(100)]
		/// </summary>
		[JsonProperty("payment_method")]
		[Description(BigCommerceCaptions.PaymentMethod)]
		public virtual string PaymentMethod { get; set; }

		/// <summary>
		/// The ID or reference number of a payment from the payment provider/gateway.
		/// 
		/// [string(255)]
		/// </summary>
		[JsonProperty("payment_provider_id")]
		public virtual string PaymentProviderId { get; set; }


		/// <summary>
		/// The amount that that has been refunded for the order.
		/// </summary>
		[JsonProperty("refunded_amount")]
		public virtual decimal RefundedAmount { get; set; }

		/// <summary>
		/// Indicates if the order contains purely digital delivery products.
		/// </summary>
		[JsonProperty("order_is_digital")]
		public virtual bool OrderIsDigital { get; set; }


		/// <summary>
		/// The amount of a gift certificate the customer used to pay for the order.
		/// </summary>
		[JsonProperty("gift_certificate_amount")]
		[Description(BigCommerceCaptions.GiftCertificateAmount)]
		public virtual decimal GiftCertificateAmount { get; set; }

		/// <summary>
		/// The IP address of the user that placed the order.
		/// 
		/// [string(30)]
		/// </summary>
		[JsonProperty("ip_address")]
		public virtual string IpAddress { get; set; }

		/// <summary>
		/// The country the order was placed from as determined by GeoIP location.
		/// 
		/// [string(50)]
		/// </summary>
		[JsonProperty("geoip_country")]
		public virtual string GeoIpCountry { get; set; }

		/// <summary>
		/// The country code of the geoip_country field.
		/// 
		/// [string(50)]
		/// </summary>
		[JsonProperty("geoip_country_iso2")]
		public virtual string GeoIpCountryIso2 { get; set; }

		/// <summary>
		/// Staff notes on the order.
		/// </summary>
		[JsonProperty("staff_notes")]
		[Description(BigCommerceCaptions.StaffNotes)]
		public virtual string StaffNotes { get; set; }

		/// <summary>
		/// An order message left by the customer when placing the order.
		/// </summary>
		[JsonProperty("customer_message")]
		[Description(BigCommerceCaptions.CustomerMessage)]
		public virtual string CustomerMessage { get; set; }

		/// <summary>
		/// The total discounts applied to the order, excluding coupons.
		/// </summary>
		[JsonProperty("discount_amount")]
		[Description(BigCommerceCaptions.DiscountAmount)]
		public virtual decimal DiscountAmount { get; set; }


		/// <summary>
		/// Indicates if the order was deleted (archived).
		/// </summary>
		[JsonProperty("is_deleted")]
		public virtual bool IsDeleted { get; set; }
	}
}

