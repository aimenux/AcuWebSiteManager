using System;
using System.Collections.Generic;
using System.ComponentModel;
using Newtonsoft.Json;
using PX.Commerce.Core;

namespace PX.Commerce.Shopify.API.REST
{
	public class InventoryLocationResponse : IEntityResponse<InventoryLocationData>
	{
		[JsonProperty("location")]
		public InventoryLocationData Data { get; set; }
	}

	public class InventoryLocationsResponse : IEntitiesResponse<InventoryLocationData>
	{
		[JsonProperty("locations")]
		public IEnumerable<InventoryLocationData> Data { get; set; }
	}

	[JsonObject(Description = "Inventory Location")]
	public class InventoryLocationData : BCAPIEntity
	{
		/// <summary>
		/// Whether the location is active. If true, then the location can be used to sell products, stock inventory, and fulfill orders. Merchants can deactivate locations from the Shopify admin. 
		/// Deactivated locations don't contribute to the shop's location limit.
		/// </summary>
		[JsonProperty("active")]
		public bool? Active { get; set; }

		/// <summary>
		/// The first line of the address.
		/// </summary>
		[JsonProperty("address1")]
		public string Address1 { get; set; }

		/// <summary>
		/// The second line of the address.
		/// </summary>
		[JsonProperty("address2")]
		public string Address2 { get; set; }

		/// <summary>
		/// The city the location is in.
		/// </summary>
		[JsonProperty("city")]
		public virtual string City { get; set; }

		/// <summary>
		/// The province the location is in.
		/// </summary>
		[JsonProperty("province")]
		public virtual string Province { get; set; }

		/// <summary>
		/// The zip or postal code.
		/// </summary>
		[JsonProperty("zip")]
		public virtual string PostalCode { get; set; }

		/// <summary>
		/// The country the location is in.
		/// </summary>
		[JsonProperty("country")]
		public virtual string Country { get; set; }

		/// <summary>
		/// The two-letter code (ISO 3166-1 alpha-2 format) corresponding to country the location is in.
		/// </summary>
		[JsonProperty("country_code")]
		public string CountryCode { get; set; }

		/// <summary>
		/// The two-letter code corresponding to province or state the location is in.
		/// </summary>
		[JsonProperty("province_code")]
		public string ProvinceCode { get; set; }

		/// <summary>
		/// The name of the location.
		/// </summary>
		[JsonProperty("name")]
		public string Name { get; set; }

		/// <summary>
		/// The phone number of the location. This value can contain special characters like - and +.
		/// </summary>
		[JsonProperty("phone")]
		public virtual string Phone { get; set; }

		/// <summary>
		/// The date and time (ISO 8601 format) when the location  was created.
		/// </summary>
		[JsonProperty("created_at")]
		[ShouldNotSerialize]
		public DateTime? DateCreatedAt { get; set; }

		/// <summary>
		/// The ID for the location.
		/// </summary>
		[JsonProperty("id")]
		public long? Id { get; set; }

		/// <summary>
		/// The date and time (ISO 8601 format) when the lcoation was last modified.
		/// </summary>
		[JsonProperty("updated_at")]
		public String DateModifiedAt { get; set; }

	}

}
