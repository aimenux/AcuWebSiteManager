using System.Runtime.Serialization;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace PX.Commerce.Shopify.API.REST
{
	/// <summary>
	/// The tax type. Valid values: normal, null, harmonized or compounded. 
	/// If the value is harmonized, then the tax is compounded of the provincial and federal sales taxes.
	/// </summary>
	[JsonConverter(typeof(StringEnumConverter))]
	public enum TaxType
	{
        [EnumMember(Value = null)]
        Null = 0,

        [EnumMember(Value = "normal")]
		Normal = 1,

		[EnumMember(Value = "harmonized")]
		Harmonized = 2,

		[EnumMember(Value = "compounded")]
		Compounded = 3
	}
}
