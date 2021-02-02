using System.Runtime.Serialization;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace PX.Commerce.Shopify.API.REST
{
	/// <summary>
	/// allocation_method: The method by which the discount application value has been allocated to entitled lines. Valid values:
	/// across: The value is spread across all entitled lines.
	/// each: The value is applied onto every entitled line.
	/// one: The value is applied onto a single line.
	/// </summary>
	[JsonConverter(typeof(StringEnumConverter))]
	public enum DiscountAllocationMethods
	{
		/// <summary>
		/// across: The value is spread across all entitled lines.
		/// </summary>
		[EnumMember(Value = "across")]
		Across = 0,

		/// <summary>
		/// each: The value is applied onto every entitled line.
		/// </summary>
		[EnumMember(Value = "each")]
		Each = 1,

		/// <summary>
		/// one: The value is applied onto a single line.
		/// </summary>
		[EnumMember(Value = "one")]
		One = 2
	}
}
