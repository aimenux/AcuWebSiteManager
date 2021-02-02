using System.Runtime.Serialization;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace PX.Commerce.Shopify.API.REST
{
	/// <summary>
	/// How the payment was processed. It has the following valid values:
	/// checkout: The order was processed using the Shopify checkout.
	/// direct: The order was processed using a direct payment provider.
	/// manual: The order was processed using a manual payment method.
	/// offsite: The order was processed by an external payment provider to the Shopify checkout.
	/// express: The order was processed using PayPal Express Checkout.
	/// free: The order was processed as a free order using a discount code.
	/// </summary>
	[JsonConverter(typeof(StringEnumConverter))]
	public enum OrderProcessingMethod
	{
		/// <summary>
		/// checkout: The order was processed using the Shopify checkout.
		/// </summary>
		[EnumMember(Value = "checkout")]
		Checkout = 0,

		/// <summary>
		/// direct: The order was processed using a direct payment provider.
		/// </summary>
		[EnumMember(Value = "direct")]
		Direct = 1,

		/// <summary>
		/// manual: The order was processed using a manual payment method.
		/// </summary>
		[EnumMember(Value = "manual")]
		Manual = 2,

		/// <summary>
		/// offsite: The order was processed by an external payment provider to the Shopify checkout.
		/// </summary>
		[EnumMember(Value = "offsite")]
		Offsite = 3,

		/// <summary>
		/// express: The order was processed using PayPal Express Checkout.
		/// </summary>
		[EnumMember(Value = "express")]
		Express = 4,

		/// <summary>
		/// free: The order was processed as a free order using a discount code.
		/// </summary>
		[EnumMember(Value = "free")]
		Free = 5,

        /// <summary>
		/// cash: The order was processed using a cash payment method.
		/// </summary>
		[EnumMember(Value = "cash")]
        Cash = 6
    }
}
