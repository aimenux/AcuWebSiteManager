using System.Runtime.Serialization;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace PX.Commerce.Shopify.API.REST
{
	/// <summary>
	/// How this refund line item affects inventory levels. Valid values:
	/// no_restock: Refunding these items won't affect inventory. The number of fulfillable units for this line item will remain unchanged. For example, a refund payment can be issued but no items will be returned or made available for sale again.
	/// cancel: The items have not yet been fulfilled. The canceled quantity will be added back to the available count. The number of fulfillable units for this line item will decrease.
	/// return: The items were already delivered, and will be returned to the merchant. The returned quantity will be added back to the available count. The number of fulfillable units for this line item will remain unchanged.
	/// legacy_restock: The deprecated restock property was used for this refund. These items were made available for sale again. This value is not accepted when creating new refunds.
	/// </summary>
	[JsonConverter(typeof(StringEnumConverter))]
	public enum RestockType
	{
		/// <summary>
		/// no_restock: Refunding these items won't affect inventory. The number of fulfillable units for this line item will remain unchanged. 
		/// For example, a refund payment can be issued but no items will be returned or made available for sale again.
		/// </summary>
		[EnumMember(Value = "no_restock")]
		NoRestock = 0,

		/// <summary>
		/// cancel: The items have not yet been fulfilled. The canceled quantity will be added back to the available count. The number of fulfillable units for this line item will decrease.
		/// </summary>
		[EnumMember(Value = "cancel")]
		Cancel = 1,

		/// <summary>
		/// return: The items were already delivered, and will be returned to the merchant. The returned quantity will be added back to the available count. The number of fulfillable units for this line item will remain unchanged.
		/// </summary>
		[EnumMember(Value = "return")]
		Return = 2,

		/// <summary>
		/// legacy_restock: The deprecated restock property was used for this refund. These items were made available for sale again. This value is not accepted when creating new refunds.
		/// </summary>
		[EnumMember(Value = "legacy_restock")]
		LegacyRestock = 3
	}
}
