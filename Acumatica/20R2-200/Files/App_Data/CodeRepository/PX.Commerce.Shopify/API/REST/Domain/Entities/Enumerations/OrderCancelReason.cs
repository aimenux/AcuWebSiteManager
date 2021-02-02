using System.Runtime.Serialization;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace PX.Commerce.Shopify.API.REST
{
    [JsonConverter(typeof(StringEnumConverter))]
    public enum OrderCancelReason
    {
		/// <summary>
		/// Customer - The customer canceled the order.
		/// </summary>
		[EnumMember(Value = "customer")]
        Customer = 0,

		/// <summary>
		/// Fraud - The order was fraudulent.
		/// </summary>
		[EnumMember(Value = "fraud")]
		Fraud = 1,

		/// <summary>
		/// Inventory - Items in the order were not in inventory.
		/// </summary>
		[EnumMember(Value = "inventory")]
		Inventory = 2,

		/// <summary>
		/// Declined - The payment was declined.
		/// </summary>
		[EnumMember(Value = "declined")]
		Declined = 3,

		/// <summary>
		/// other - A reason not in this list.
		/// </summary>
		[EnumMember(Value = "other")]
        Other = 4
    }
}
