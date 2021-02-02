using System.Runtime.Serialization;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace PX.Commerce.Shopify.API.REST
{
	/// <summary>
	/// A standardized error code, independent of the payment provider. Valid values:
	/// incorrect_number
	/// invalid_number
	/// invalid_expiry_date
	/// invalid_cvc
	/// expired_card
	/// incorrect_cvc
	/// incorrect_zip
	/// incorrect_address
	/// card_declined
	/// processing_error
	/// call_issuer
	/// pick_up_card
	/// </summary>
	[JsonConverter(typeof(StringEnumConverter))]
	public enum TransactionErrorCode
	{
		[EnumMember(Value = "incorrect_number")]
		IncorrectNumber,

		[EnumMember(Value = "invalid_number")]
		InvalidNumber,

		[EnumMember(Value = "invalid_expiry_date")]
		InvalidExpiryDate,

		[EnumMember(Value = "invalid_cvc")]
		InvalidCVC,

		[EnumMember(Value = "expired_card")]
		ExpiredCard,

		[EnumMember(Value = "incorrect_cvc")]
		IncorrectCVC,

		[EnumMember(Value = "incorrect_zip")]
		IncorrectZip,

		[EnumMember(Value = "incorrect_address")]
		IncorrectAddress,

		[EnumMember(Value = "card_declined")]
		CardDeclined,

		[EnumMember(Value = "processing_error")]
		ProcessingError,

		[EnumMember(Value = "call_issuer")]
		CallIssuer,

		[EnumMember(Value = "pick_up_card")]
		PickUpCard,
	}
}
