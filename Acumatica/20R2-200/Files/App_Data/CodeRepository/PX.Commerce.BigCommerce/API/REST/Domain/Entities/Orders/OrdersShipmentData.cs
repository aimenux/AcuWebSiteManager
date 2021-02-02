using System;
using System.Collections.Generic;
using PX.Commerce.BigCommerce.API.REST;
using Newtonsoft.Json;
using PX.Commerce.Core;
using System.ComponentModel;

namespace PX.Commerce.BigCommerce.API.REST
{
	[Description(BigCommerceCaptions.ShipmentData)]
    public class OrdersShipmentData : BCAPIEntity
	{
        /// <summary>
        /// The ID of the shipment.
        /// </summary>
        [JsonProperty("id")]
		[Description(BigCommerceCaptions.ShipmentID)]
		public virtual int? Id { get; set; }
		public bool ShouldSerializeId()
		{
			return  false;
		}
		/// <summary>
		/// The ID of the customer that placed the order.
		/// </summary>
		[JsonProperty("customer_id")]
        public virtual int CustomerId { get; set; }
		public bool ShouldSerializeCustomerId()
		{
			return false;
		}

		[JsonProperty("date_created")]
		[Description(BigCommerceCaptions.DateShipped)]
		public virtual string DateCreatedUT { get; set; }
		public bool ShouldSerializeDateCreatedUT()
		{
			return false;
		}

		/// <summary>
		/// The tracking number for the shipment.
		///  string(50)
		/// </summary>
		[JsonProperty("tracking_number")]
		[Description(BigCommerceCaptions.TrackingID)]
		public virtual string TrackingNumber { get; set; }

		/// <summary>
		/// Extra detail to describe the shipment, with values like: Standard, My Custom Shipping Method Name, etc. 
		/// Can also be used for live quotes from some shipping providers.
		/// string(100)
		/// </summary>
		[JsonProperty("shipping_method")]
		[Description(BigCommerceCaptions.ShippingMethod)]
		public virtual string ShippingMethod { get; set; }

		/// <summary>
		/// Enum of the BigCommerce shipping-carrier integration/module. 
		/// (Note: This property should be included in a POST request to create a shipment object. 
		/// If it is omitted from the request, the property’s value will default to custom, and no tracking link will be generated in the email. 
		/// To avoid this behavior, you can pass the property as an empty string.)
		///  string(50)
		/// </summary>
		[JsonProperty("shipping_provider")]
		[Description(BigCommerceCaptions.ShippingProvider)]
		public virtual string ShippingProvider { get; set; }

		/// <summary>
		/// Optional, but if you include it, its value must refer/map to the same carrier service as the shipping_provider
		/// string(100)
		/// </summary>
		[JsonProperty("tracking_carrier")]
		[Description(BigCommerceCaptions.TrackingCarrier)]
		public virtual string TrackingCarrier { get; set; }

		/// <summary>
		/// The ID of the order this shipment is associated with.
		/// </summary>
		[JsonProperty("order_id")]
        public virtual int OrderId { get; set; }
		public bool ShouldSerializeOrderId()
		{
			return false;
		}

		[JsonProperty("order_date")]
		[Description(BigCommerceCaptions.OrderDate)]
		public virtual string OrderDateUT { get; set; }
		public bool ShouldSerializeOrderDateUT()
		{
			return false;
		}

		/// <summary>
		/// Any comments the store owner has added regarding the shipment.
		/// 
		/// text
		/// </summary>
		[JsonProperty("comments")]
		[Description(BigCommerceCaptions.PackingSlipNotes)]
		public virtual string Comments { get; set; }

        /// <summary>
        /// The ID of the order address this shipment is associated with.
        /// </summary>
        [JsonProperty("order_address_id")]
        public virtual int OrderAddressId { get; set; }

        /// <summary>
        /// The billing address of the order. 
        /// </summary>
        [JsonProperty("billing_address")]
		[Description(BigCommerceCaptions.BillingAddress)]
		public virtual OrderAddressData BillingAddress { get; set; }
		public bool ShouldSerializeBillingAddress()
		{
			return false;
		}

		/// <summary>
		/// The shipping address of the shipment. 
		/// </summary>
		[JsonProperty("shipping_address")]
		[Description(BigCommerceCaptions.ShippingTo)]
		public virtual OrderAddressData ShippingAddress { get; set; }
		public bool ShouldSerializeShippingAddress()
		{
			return false;
		}

		/// <summary>
		/// The items in the shipment. 
		/// </summary>
		[JsonProperty("items")]
		[Description(BigCommerceCaptions.ShipmentItems)]
		public virtual IList<OrdersShipmentItem> ShipmentItems { get; set; }
    }
}
