using System.Runtime.Serialization;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace PX.Commerce.Shopify.API.REST
{
	/// <summary>
	/// The status of the fulfillment. Valid values:
	/// pending: The fulfillment is pending.
	/// open: The fulfillment has been acknowledged by the service and is in processing.
	/// success: The fulfillment was successful.
	/// cancelled: The fulfillment was cancelled.
	/// error: There was an error with the fulfillment request.
	/// failure: The fulfillment request failed.
	/// </summary>
	[JsonConverter(typeof(StringEnumConverter))]
	public enum FulfillmentStatus
	{
		/// <summary>
		/// pending: The fulfillment is pending.
		/// </summary>
		[EnumMember(Value = "pending")]
		Pending = 0,

		/// <summary>
		/// open: The fulfillment has been acknowledged by the service and is in processing.
		/// </summary>
		[EnumMember(Value = "open")]
		Open = 1,

		/// <summary>
		/// success: The fulfillment was successful.
		/// </summary>
		[EnumMember(Value = "success")]
		Success = 2,

		/// <summary>
		/// cancelled: The fulfillment was cancelled.
		/// </summary>
		[EnumMember(Value = "cancelled")]
		Cancelled = 3,

		/// <summary>
		/// error: There was an error with the fulfillment request.
		/// </summary>
		[EnumMember(Value = "error")]
		Error = 4,

		/// <summary>
		/// failure: The fulfillment request failed.
		/// </summary>
		[EnumMember(Value = "failure")]
		Failure = 5

	}
}
