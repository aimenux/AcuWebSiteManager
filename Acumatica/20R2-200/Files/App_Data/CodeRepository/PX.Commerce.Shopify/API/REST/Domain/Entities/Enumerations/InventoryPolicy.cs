using System.Runtime.Serialization;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace PX.Commerce.Shopify.API.REST
{
	/// <summary>
	/// Whether customers are allowed to place an order for the product variant when it's out of stock. Valid values:
	/// deny: Customers are not allowed to place orders for the product variant if it's out of stock.
	/// continue: Customers are allowed to place orders for the product variant if it's out of stock.
	/// Default value: deny.
	/// </summary>
	[JsonConverter(typeof(StringEnumConverter))]
    public enum InventoryPolicy
	{
		/// <summary>
		/// deny: Customers are not allowed to place orders for the product variant if it's out of stock.
		/// </summary>
		[EnumMember(Value = "deny")]
		Deny = 0,

		/// <summary>
		/// continue: Customers are allowed to place orders for the product variant if it's out of stock.
		/// </summary>
		[EnumMember(Value = "continue")]
		Continue = 1
    }
}
