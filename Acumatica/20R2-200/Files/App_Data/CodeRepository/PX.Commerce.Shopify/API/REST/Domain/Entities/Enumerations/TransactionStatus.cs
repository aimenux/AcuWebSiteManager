using System.Runtime.Serialization;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace PX.Commerce.Shopify.API.REST
{
	/// <summary>
	/// The status of the transaction. Valid values: pending, failure, success, and error.
	/// </summary>
	[JsonConverter(typeof(StringEnumConverter))]
	public enum TransactionStatus
	{
		/// <summary>
		/// pending: The transaction is pending.
		/// </summary>
		[EnumMember(Value = "pending")]
		Pending,

		/// <summary>
		/// failure: The transaction request failed.
		/// </summary>
		[EnumMember(Value = "failure")]
		Failure,

		/// <summary>
		/// success: The transaction was successful.
		/// </summary>
		[EnumMember(Value = "success")]
		Success,

		/// <summary>
		/// error: There was an error with the transaction request.
		/// </summary>
		[EnumMember(Value = "error")]
		Error = 4

	}
}
