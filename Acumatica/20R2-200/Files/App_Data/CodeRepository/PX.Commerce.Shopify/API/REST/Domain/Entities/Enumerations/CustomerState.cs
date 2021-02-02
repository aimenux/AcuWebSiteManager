using System.Runtime.Serialization;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace PX.Commerce.Shopify.API.REST
{
    [JsonConverter(typeof(StringEnumConverter))]
    public enum CustomerState
    {
		/// <summary>
		/// disabled: The customer doesn't have an active account. Customer accounts can be disabled from the Shopify admin at any time.
		/// </summary>
		[EnumMember(Value = "disabled")]
		Disabled = 0,

		/// <summary>
		/// invited: The customer has received an email invite to create an account.
		/// </summary>
		[EnumMember(Value = "invited")]
		Invited = 1,

		/// <summary>
		/// enabled: The customer has created an account.
		/// </summary>
		[EnumMember(Value = "enabled")]
		Enabled = 2,

		/// <summary>
		/// declined: The customer declined the email invite to create an account.
		/// </summary>
		[EnumMember(Value = "declined")]
		Declined = 3
    }
}
