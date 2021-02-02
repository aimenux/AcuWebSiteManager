using System;
using System.Collections.Generic;
using System.ComponentModel;
using Newtonsoft.Json;
using PX.Commerce.Core;

namespace PX.Commerce.Shopify.API.REST
{
	/// <summary>
	/// A list of line item objects, each containing information about an item in the order.
	/// </summary>
	[JsonObject(Description = "Order Line Item")]
	[Description(ShopifyCaptions.LineItem)]
	public class OrderLineItem : BCAPIEntity
	{

		[JsonIgnore]
		public virtual Guid? PackageId { get; set; }

		/// <summary>
		/// The amount available to fulfill, calculated as follows:
		/// quantity - max(refunded_quantity, fulfilled_quantity) - pending_fulfilled_quantity - open_fulfilled_quantity
		/// </summary>
		[JsonProperty("fulfillable_quantity", NullValueHandling = NullValueHandling.Ignore)]
		public int? FulfillableQuantity { get; set; }

		/// <summary>
		/// The service provider that's fulfilling the item. Valid values: manual, or the name of the provider, such as amazon or shipwire. 
		/// </summary>
		[JsonProperty("fulfillment_service", NullValueHandling = NullValueHandling.Ignore)]
		[Description(ShopifyCaptions.FulfillmentService)]
		public string FulfillmentService { get; set; }

		/// <summary>
		/// How far along an order is in terms line items fulfilled. Valid values: null, fulfilled, partial, and not_eligible.
		/// </summary>
		[JsonProperty("fulfillment_status", NullValueHandling = NullValueHandling.Ignore)]
		[Description(ShopifyCaptions.FulfillmentStatus)]
		public OrderFulfillmentStatus? FulfillmentStatus { get; set; }

		/// <summary>
		/// The weight of the item in grams.
		/// </summary>
		[JsonProperty("grams", NullValueHandling = NullValueHandling.Ignore)]
		[Description(ShopifyCaptions.Weight)]
		public decimal? WeightInGrams { get; set; }

		/// <summary>
		/// The ID of the line item.
		[JsonProperty("id", NullValueHandling = NullValueHandling.Ignore)]
		[Description(ShopifyCaptions.Id)]
		public long? Id { get; set; }

		/// <summary>
		/// The ID of the Order.
		[JsonIgnore]
		public long? OrderId { get; set; }

		/// <summary>
		/// The name of the Order.
		[JsonIgnore]
		public string OrderName { get; set; }

		/// <summary>
		/// The price of the item before discounts have been applied in the shop currency.
		/// </summary>
		[JsonProperty("price", NullValueHandling = NullValueHandling.Ignore)]
		[Description(ShopifyCaptions.Price)]
		public decimal? Price { get; set; }

		/// <summary>
		/// The price of the line item in shop and presentment currencies.
		/// </summary>
		[JsonProperty("price_set", NullValueHandling = NullValueHandling.Ignore)]
		[Description(ShopifyCaptions.PriceSet)]
		public PriceSet PriceSet { get; set; }

		/// <summary>
		/// Whether the product exists.
		/// </summary>
		[JsonProperty("product_exists", NullValueHandling = NullValueHandling.Ignore)]
		[Description(ShopifyCaptions.ProductExists)]
		public bool? ProductExists { get; set; }

		/// <summary>
		///The ID of the product that the line item belongs to. Can be null if the original product associated with the order is deleted at a later date.
		/// </summary>
		[JsonProperty("product_id", NullValueHandling = NullValueHandling.Ignore)]
		[Description(ShopifyCaptions.ProductId)]
		public long? ProductId { get; set; }

		/// <summary>
		/// The number of items that were purchased.
		/// </summary>
		[JsonProperty("quantity")]
		[Description(ShopifyCaptions.Quantity)]
		public int? Quantity { get; set; }

		/// <summary>
		/// Whether the item requires shipping.
		/// </summary>
		[JsonProperty("requires_shipping", NullValueHandling = NullValueHandling.Ignore)]
		[Description(ShopifyCaptions.RequiresShipping)]
		public bool? RequiresShipping { get; set; }

		/// <summary>
		/// A unique identifier for the product variant in the shop. Required in order to connect to a FulfillmentService.
		/// </summary>
		[JsonProperty("sku", NullValueHandling = NullValueHandling.Ignore)]
		[Description(ShopifyCaptions.SKU)]
		public String Sku { get; set; }

		/// <summary>
		/// The title of the product.
		/// </summary>
		[JsonProperty("title", NullValueHandling = NullValueHandling.Ignore)]
		[Description(ShopifyCaptions.Title)]
		public String Title { get; set; }

		/// <summary>
		/// The ID of the product variant.
		/// </summary>
		[JsonProperty("variant_id", NullValueHandling = NullValueHandling.Ignore)]
		[Description(ShopifyCaptions.VariantId)]
		public long? VariantId { get; set; }

		/// <summary>
		/// The title of the product variant.
		/// </summary>
		[JsonProperty("variant_title", NullValueHandling = NullValueHandling.Ignore)]
		[Description(ShopifyCaptions.VariantTitle)]
		public String VariantTitle { get; set; }

		/// <summary>
		/// The name of the inventory management system.
		/// </summary>
		[JsonProperty("variant_inventory_management", NullValueHandling = NullValueHandling.Ignore)]
		[Description(ShopifyCaptions.InventoryManagement)]
		public String VariantInventoryManagement { get; set; }

		/// <summary>
		/// The name of the item's supplier.
		/// </summary>
		[JsonProperty("vendor", NullValueHandling = NullValueHandling.Ignore)]
		[Description(ShopifyCaptions.Vendor)]
		public String Vendor { get; set; }

		/// <summary>
		/// The name of the product variant.
		/// </summary>
		[JsonProperty("name", NullValueHandling = NullValueHandling.Ignore)]
		[Description(ShopifyCaptions.Name)]
		public String Name { get; set; }

		/// <summary>
		/// Whether the item is a gift card. If true, then the item is not taxed or considered for shipping charges.
		/// </summary>
		[JsonProperty("gift_card", NullValueHandling = NullValueHandling.Ignore)]
		[Description(ShopifyCaptions.GiftCard)]
		public bool? IsGiftCard { get; set; }

		/// <summary>
		/// An array of custom information for the item that has been added to the cart. Often used to provide product customization options.
		/// </summary>
		[JsonProperty("properties", NullValueHandling = NullValueHandling.Ignore)]
		[Description(ShopifyCaptions.Properties)]
		public List<String> Properties { get; set; }

		/// <summary>
		/// Whether the item was taxable.
		/// </summary>
		[JsonProperty("taxable", NullValueHandling = NullValueHandling.Ignore)]
		[Description(ShopifyCaptions.Taxable)]
		public bool? Taxable { get; set; }

		/// <summary>
		/// A list of tax line objects, each of which details a tax applied to the item.
		/// </summary>
		[JsonProperty("tax_lines", NullValueHandling = NullValueHandling.Ignore)]
		[Description(ShopifyCaptions.TaxLine)]
		public List<OrderTaxLine> TaxLines { get; set; }

		/// <summary>
		/// The payment gateway used to tender the tip, such as shopify_payments. Present only on tips.
		/// </summary>
		[JsonProperty("tip_payment_gateway", NullValueHandling = NullValueHandling.Ignore)]
		[Description(ShopifyCaptions.TipPaymentGateway)]
		public String TipPaymentGateway { get; set; }

		/// <summary>
		/// The payment method used to tender the tip, such as Visa. Present only on tips.
		/// </summary>
		[JsonProperty("tip_payment_method", NullValueHandling = NullValueHandling.Ignore)]
		[Description(ShopifyCaptions.TipPaymentMethod)]
		public String TipPaymentMethod { get; set; }

		/// <summary>
		/// [Note] This field value is always 0, cannot use this to get the line item discount.
		/// The total discount amount applied to this line item in the shop currency. This value is not subtracted in the line item price.
		/// </summary>
		[JsonProperty("total_discount", NullValueHandling = NullValueHandling.Ignore)]
		[Description(ShopifyCaptions.TotalDiscount)]
		public decimal? TotalDiscount { get; set; }

		/// <summary>
		/// The total discount applied to the line item in shop and presentment currencies.
		/// </summary>
		[JsonProperty("total_discount_set", NullValueHandling = NullValueHandling.Ignore)]
		[Description(ShopifyCaptions.TotalDiscountSet)]
		public PriceSet TotalDiscountSet { get; set; }

		/// <summary>
		/// An ordered list of amounts allocated by discount applications. Each discount allocation is associated to a particular discount application.
		/// </summary>
		[JsonProperty("discount_allocations", NullValueHandling = NullValueHandling.Ignore)]
		[Description(ShopifyCaptions.DiscountAllocation)]
		public List<OrderDiscountAllocation> DiscountAllocations { get; set; }
	}
}
