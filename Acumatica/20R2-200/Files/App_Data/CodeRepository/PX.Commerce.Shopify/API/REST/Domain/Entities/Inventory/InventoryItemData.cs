using System;
using System.Collections.Generic;
using System.ComponentModel;
using Newtonsoft.Json;
using PX.Commerce.Core;

namespace PX.Commerce.Shopify.API.REST
{
	public class InventoryItemResponse : IEntityResponse<InventoryItemData>
	{
		[JsonProperty("inventory_item")]
		public InventoryItemData Data { get; set; }
	}

	public class InventoryItemsResponse : IEntitiesResponse<InventoryItemData>
	{
		[JsonProperty("inventory_items")]
		public IEnumerable<InventoryItemData> Data { get; set; }
	}

	[JsonObject(Description = ShopifyCaptions.InventoryLevel)]
	public class InventoryItemData : BCAPIEntity
	{
		/// <summary>
		/// The date and time (ISO 8601 format) when the inventory item  was created.
		/// </summary>
		[JsonProperty("created_at")]
		[ShouldNotSerialize]
		public DateTime? DateCreatedAt { get; set; }

		/// <summary>
		/// The ID of the inventory item.
		/// </summary>
		[JsonProperty("id")]
		public long? Id { get; set; }

		/// <summary>
		/// The unit cost of the inventory item.
		/// </summary>
		[JsonProperty("cost")]
		public decimal? Cost { get; set; }

		/// <summary>
		/// The two-digit code for the country where the inventory item was made.
		/// </summary>
		[JsonProperty("country_code_of_origin")]
		public string CountryCodeOfOrigin { get; set; }

		/// <summary>
		/// The general Harmonized System (HS) code for the inventory item. Used if a country-specific HS code is not available.
		/// </summary>
		[JsonProperty("harmonized_system_code")]
		public string HarmonizedSystemCode { get; set; }

		/// <summary>
		/// The two-digit code for the province where the inventory item was made. Used only if the shipping provider for the inventory item is Canada Post.
		/// </summary>
		[JsonProperty("province_code_of_origin")]
		public string ProvinceCodeOfOrigin { get; set; }

		/// <summary>
		/// The unique SKU (stock keeping unit) of the inventory item.
		/// </summary>
		[JsonProperty("sku")]
		public string Sku { get; set; }

		/// <summary>
		/// Whether the inventory item is tracked. If true, then inventory quantity changes are tracked by Shopify.
		/// </summary>
		[JsonProperty("tracked")]
		public bool? Tracked { get; set; }

		/// <summary>
		/// Whether a customer needs to provide a shipping address when placing an order containing the inventory item.
		/// </summary>
		[JsonProperty("requires_shipping")]
		public bool? RequiresShipping { get; set; }

		/// <summary>
		/// The date and time (ISO 8601 format) when the inventory item was last modified.
		/// </summary>
		[JsonProperty("updated_at")]
		public String DateModifiedAt { get; set; }

	}

}
