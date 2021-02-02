using System.Runtime.Serialization;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace PX.Commerce.Shopify.API.REST
{
	/// <summary>
	/// The discount application type. Valid values:
	/// automatic: The discount was applied automatically, such as by a Buy X Get Y automatic discount.
	/// discount_code: The discount was applied by a discount code.
	/// manual: The discount was manually applied by the merchant (for example, by using an app or creating a draft order).
	/// script: The discount was applied by a Shopify Script.
	/// </summary>
	[JsonConverter(typeof(StringEnumConverter))]
	public enum DiscountApplicationType
	{
		/// <summary>
		/// automatic: The discount was applied automatically, such as by a Buy X Get Y automatic discount.
		/// </summary>
		[EnumMember(Value = "automatic")]
		Automatic = 0,

		/// <summary>
		/// discount_code: The discount was applied by a discount code.
		/// </summary>
		[EnumMember(Value = "discount_code")]
		DiscountCode = 1,

		/// <summary>
		/// manual: The discount was manually applied by the merchant (for example, by using an app or creating a draft order).
		/// </summary>
		[EnumMember(Value = "manual")]
		Manual = 2,

		/// <summary>
		/// script: The discount was applied by a Shopify Script.
		/// </summary>
		[EnumMember(Value = "script")]
		Script = 3
	}
}
