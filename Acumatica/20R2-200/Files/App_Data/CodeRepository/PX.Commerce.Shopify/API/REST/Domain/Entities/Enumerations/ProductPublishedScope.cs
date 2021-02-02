using System.Runtime.Serialization;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace PX.Commerce.Shopify.API.REST
{
	/// <summary>
	/// Whether the product is published to the Point of Sale channel. Valid values:
	/// web: The product is published to the Online Store channel but not published to the Point of Sale channel.
	/// global: The product is published to both the Online Store channel and the Point of Sale channel.
	/// </summary>
	[JsonConverter(typeof(StringEnumConverter))]
    public enum PublishedScope
    {
		/// <summary>
		/// web: The product is published to the Online Store channel but not published to the Point of Sale channel.
		/// </summary>
		[EnumMember(Value = "web")]
		Web = 0,

		/// <summary>
		/// global: The product is published to both the Online Store channel and the Point of Sale channel.
		/// </summary>
		[EnumMember(Value = "global")]
		Global = 1
    }
}
