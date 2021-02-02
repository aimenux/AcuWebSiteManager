using System.Runtime.Serialization;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace PX.Commerce.Shopify.API.REST
{
	/// <summary>
	/// target_selection: The lines on the order, of the type defined by target_type, that the discount is allocated over. Valid values:
	/// all: The discount is allocated onto all lines,
	/// entitled: The discount is allocated only onto lines it is entitled for.
	/// explicit: The discount is allocated onto explicitly selected lines.
	/// </summary>
	[JsonConverter(typeof(StringEnumConverter))]
	public enum DiscountTargetSelection
	{
		/// <summary>
		/// all: The discount is allocated onto all lines
		/// </summary>
		[EnumMember(Value = "all")]
		All = 0,

		/// <summary>
		/// entitled: The discount is allocated only onto lines it is entitled for.
		/// </summary>
		[EnumMember(Value = "entitled")]
		Entitled = 1,

		/// <summary>
		/// explicit: The discount is allocated onto explicitly selected lines.
		/// </summary>
		[EnumMember(Value = "explicit")]
		Explicit = 2
	}
}
