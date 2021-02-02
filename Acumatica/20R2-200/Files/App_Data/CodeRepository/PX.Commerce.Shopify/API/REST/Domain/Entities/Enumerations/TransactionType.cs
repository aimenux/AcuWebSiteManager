using System.Runtime.Serialization;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace PX.Commerce.Shopify.API.REST
{
	/// <summary>
	/// The transaction's type. Valid values:
	/// authorization: Money that the customer has agreed to pay. The authorization period can be between 7 and 30 days (depending on your payment service) while a store waits for a payment to be captured.
	/// capture: A transfer of money that was reserved during the authorization of a shop.
	/// sale: The authorization and capture of a payment performed in one single step.
	/// void: The cancellation of a pending authorization or capture.
	/// refund: The partial or full return of captured money to the customer.
	/// </summary>
	[JsonConverter(typeof(StringEnumConverter))]
    public enum TransactionType
    {
		/// <summary>
		/// authorization: Money that the customer has agreed to pay. The authorization period can be between 7 and 30 days (depending on your payment service) while a store waits for a payment to be captured.
		/// </summary>
		[EnumMember(Value = "authorization")]
		Authorization = 0,

		/// <summary>
		/// capture: A transfer of money that was reserved during the authorization of a shop.
		/// </summary>
		[EnumMember(Value = "capture")]
		Capture = 1,

		/// <summary>
		/// sale: The authorization and capture of a payment performed in one single step.
		/// </summary>
		[EnumMember(Value = "sale")]
		Sale = 2,

		/// <summary>
		/// void: The cancellation of a pending authorization or capture.
		/// </summary>
		[EnumMember(Value = "void")]
		Void = 3,

		/// <summary>
		/// refund: The partial or full return of captured money to the customer.
		/// </summary>
		[EnumMember(Value = "refund")]
		Refund = 4
    }
}
