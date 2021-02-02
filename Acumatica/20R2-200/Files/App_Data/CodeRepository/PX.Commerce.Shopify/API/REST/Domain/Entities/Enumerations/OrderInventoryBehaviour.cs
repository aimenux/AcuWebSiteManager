using System.Runtime.Serialization;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace PX.Commerce.Shopify.API.REST
{
	/// <summary>
	/// Creates an order. By default, product inventory is not claimed. The behaviour to use when updating inventory. (default: bypass)
	/// </summary>
	[JsonConverter(typeof(StringEnumConverter))]
    public enum OrderInventoryBehaviour
    {
		/// <summary>
		/// bypass: Do not claim inventory.
		/// </summary>
		[EnumMember(Value = "bypass")]
		Bypass = 0,

		/// <summary>
		/// decrement_ignoring_policy: Ignore the product's inventory policy and claim inventory.
		/// </summary>
		[EnumMember(Value = "decrement_ignoring_policy")]
		DecrementIgnoringPolicy = 1,

		/// <summary>
		/// decrement_obeying_policy: Follow the product's inventory policy and claim inventory, if possible.
		/// </summary>
		[EnumMember(Value = "decrement_obeying_policy")]
		DecrementObeyingPolicy = 2
    }
}
