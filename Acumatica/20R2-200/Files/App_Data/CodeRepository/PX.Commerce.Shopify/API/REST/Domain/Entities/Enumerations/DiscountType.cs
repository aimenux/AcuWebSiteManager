using System.Runtime.Serialization;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace PX.Commerce.Shopify.API.REST
{
	/// <summary>
	/// The type of discount. Default value: fixed_amount. Valid values:
	/// fixed_amount: Applies amount as a unit of the store's currency. For example, if amount is 30 and the store's currency is USD, then 30 USD is deducted from the order total when the discount is applied.
	/// percentage: Applies a discount of amount as a percentage of the order total.
	/// shipping: Applies a free shipping discount on orders that have a shipping rate less than or equal to amount. For example, if amount is 30, then the discount will give the customer free shipping for any shipping rate that is less than or equal to $30.
	/// </summary>
	[JsonConverter(typeof(StringEnumConverter))]
	public enum DiscountType
	{
		/// <summary>
		/// fixed_amount: Applies amount as a unit of the store's currency. 
		/// For example, if amount is 30 and the store's currency is USD, then 30 USD is deducted from the order total when the discount is applied.
		/// </summary>
		[EnumMember(Value = "fixed_amount")]
		FixedAmount = 0,

		/// <summary>
		/// percentage: Applies a discount of amount as a percentage of the order total.
		/// </summary>
		[EnumMember(Value = "percentage")]
		Percentage = 1,

		/// <summary>
		/// shipping: Applies a free shipping discount on orders that have a shipping rate less than or equal to amount. 
		/// For example, if amount is 30, then the discount will give the customer free shipping for any shipping rate that is less than or equal to $30.
		/// </summary>
		[EnumMember(Value = "shipping")]
		Shipping = 2
	}
}
