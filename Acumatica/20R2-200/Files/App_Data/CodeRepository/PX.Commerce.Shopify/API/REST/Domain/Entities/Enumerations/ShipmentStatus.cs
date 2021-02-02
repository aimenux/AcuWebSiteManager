using System.Runtime.Serialization;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace PX.Commerce.Shopify.API.REST
{
	/// <summary>
	/// The current shipment status of the fulfillment. Valid values:
	/// label_printed: A label for the shipment was purchased and printed.
	/// label_purchased: A label for the shipment was purchased, but not printed.
	/// attempted_delivery: Delivery of the shipment was attempted, but unable to be completed.
	/// ready_for_pickup: The shipment is ready for pickup at a shipping depot.
	/// confirmed: The carrier is aware of the shipment, but hasn't received it yet.
	/// in_transit: The shipment is being transported between shipping facilities on the way to its destination.
	/// out_for_delivery: The shipment is being delivered to its final destination.
	/// delivered: The shipment was succesfully delivered.
	/// failure: Something went wrong when pulling tracking information for the shipment, such as the tracking number was invalid or the shipment was canceled.
	/// </summary>
	[JsonConverter(typeof(StringEnumConverter))]
	public enum ShipmentStatus
	{
		/// <summary>
		/// label_printed: A label for the shipment was purchased and printed.
		/// </summary>
		[EnumMember(Value = "label_printed")]
		LabelPrinted = 0,

		/// <summary>
		/// label_purchased: A label for the shipment was purchased, but not printed.
		/// </summary>
		[EnumMember(Value = "label_purchased")]
		LabelPurchased = 1,

		/// <summary>
		/// attempted_delivery: Delivery of the shipment was attempted, but unable to be completed.
		/// </summary>
		[EnumMember(Value = "attempted_delivery")]
		AttemptedDelivery = 2,

		/// <summary>
		/// ready_for_pickup: The shipment is ready for pickup at a shipping depot.
		/// </summary>
		[EnumMember(Value = "ready_for_pickup")]
		ReadyForPickup = 3,

		/// <summary>
		/// confirmed: The carrier is aware of the shipment, but hasn't received it yet.
		/// </summary>
		[EnumMember(Value = "confirmed")]
		Confirmed = 4,

		/// <summary>
		/// in_transit: The shipment is being transported between shipping facilities on the way to its destination.
		/// </summary>
		[EnumMember(Value = "in_transit")]
		InTransit = 5,

		/// <summary>
		/// out_for_delivery: The shipment is being delivered to its final destination.
		/// </summary>
		[EnumMember(Value = "out_for_delivery")]
		OutForDelivery = 6,

		/// <summary>
		/// delivered: The shipment was succesfully delivered.
		/// </summary>
		[EnumMember(Value = "delivered")]
		Delivered = 7,

		/// <summary>
		/// failure: Something went wrong when pulling tracking information for the shipment, such as the tracking number was invalid or the shipment was canceled.
		/// </summary>
		[EnumMember(Value = "failure")]
		Failure = 8
	}
}
