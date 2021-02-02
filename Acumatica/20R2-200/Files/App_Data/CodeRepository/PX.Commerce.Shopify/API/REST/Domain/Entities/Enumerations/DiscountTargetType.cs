using System.Runtime.Serialization;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace PX.Commerce.Shopify.API.REST
{
	/// <summary>
	/// The type of line on the order that the discount is applicable on. Valid values:
	/// line_item: The discount applies to line items.
	/// shipping_line: The discount applies to shipping lines.
	/// </summary>
	[JsonConverter(typeof(StringEnumConverter))]
	public enum DiscountTargetType
	{
		/// <summary>
		/// line_item: The discount applies to line items.
		/// </summary>
		[EnumMember(Value = "line_item")]
		LineItem = 0,

		/// <summary>
		/// shipping_line: The discount applies to shipping lines.
		/// </summary>
		[EnumMember(Value = "shipping_line")]
		ShippingLine = 1
	}
}
