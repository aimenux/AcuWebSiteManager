using System.Runtime.Serialization;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace PX.Commerce.Shopify.API.REST
{
	/// <summary>
	/// The recommended action given to the merchant. Valid values:
	/// cancel: There is a high level of risk that this order is fraudulent. The merchant should cancel the order.
	/// investigate: There is a medium level of risk that this order is fraudulent. The merchant should investigate the order.
	/// accept: There is a low level of risk that this order is fraudulent. The order risk found no indication of fraud.
	/// </summary>
	[JsonConverter(typeof(StringEnumConverter))]
	public enum OrderRiskActionType
	{
		/// <summary>
		/// cancel: There is a high level of risk that this order is fraudulent. The merchant should cancel the order.
		/// </summary>
		[EnumMember(Value = "cancel")]
		Cancel = 0,

		/// <summary>
		/// investigate: There is a medium level of risk that this order is fraudulent. The merchant should investigate the order.
		/// </summary>
		[EnumMember(Value = "investigate")]
		Investigate = 1,

		/// <summary>
		/// accept: There is a low level of risk that this order is fraudulent. The order risk found no indication of fraud.
		/// </summary>
		[EnumMember(Value = "accept")]
		Accept = 2
	}
}
