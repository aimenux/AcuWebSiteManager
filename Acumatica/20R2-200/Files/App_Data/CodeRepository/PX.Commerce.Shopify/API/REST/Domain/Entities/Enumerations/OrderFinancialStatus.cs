using System.Runtime.Serialization;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace PX.Commerce.Shopify.API.REST
{
	/// <summary>
	/// The status of payments associated with the order. Can only be set when the order is created. Valid values:
	/// pending: The payments are pending.Payment might fail in this state.Check again to confirm whether the payments have been paid successfully.
	/// authorized: The payments have been authorized.
	/// partially_paid: The order have been partially paid.
	/// paid: The payments have been paid.
	/// partially_refunded: The payments have been partially refunded.
	/// refunded: The payments have been refunded.
	/// voided: The payments have been voided.
	/// </summary>
	[JsonConverter(typeof(StringEnumConverter))]
	public enum OrderFinancialStatus
	{
		/// <summary>
		/// pending: The payments are pending.Payment might fail in this state.Check again to confirm whether the payments have been paid successfully.
		/// </summary>
		[EnumMember(Value = "pending")]
		Pending = 0,

		/// <summary>
		/// authorized: The payments have been authorized.
		/// </summary>
		[EnumMember(Value = "authorized")]
		Authorized = 1,

		/// <summary>
		/// partially_paid: The order have been partially paid.
		/// </summary>
		[EnumMember(Value = "partially_paid")]
		Partially_paid = 2,

		/// <summary>
		/// Paid: The payments have been paid.
		/// </summary>
		[EnumMember(Value = "paid")]
		Paid = 3,

		/// <summary>
		/// partially_refunded: The payments have been partially refunded.
		/// </summary>
		[EnumMember(Value = "partially_refunded")]
		PartiallyRefunded = 4,

		/// <summary>
		/// refunded: The payments have been refunded.
		/// </summary>
		[EnumMember(Value = "refunded")]
		Refunded = 5,

		/// <summary>
		/// voided: The payments have been voided.
		/// </summary>
		[EnumMember(Value = "voided")]
		Voided = 6
	}
}
