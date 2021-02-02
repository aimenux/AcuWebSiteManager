using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using PX.Commerce.Core;

namespace PX.Commerce.BigCommerce.API.REST
{
    [JsonObject(Description = "Product -> Product Variant")]
    public class ProductsVariantData: BCAPIEntity
	{
        public ProductsVariantData()
        {
            OptionValues = new List<ProductVariantOptionValueData>();
            ModifierValues = new List<ProductVariantModifierValueData>();
        }
        /// <summary>
        ///  (optional) number (double float)	The cost price of the variant.
        /// </summary>
        [JsonProperty("cost_price")]
        public decimal? CostPrice { get; set; }

        /// <summary>
        ///  (optional) number (double float)	This variant’s base price on the storefront. If this value is null, the product’s default price (set in the Product resource’s price field) will be used as the base price.
        /// </summary>
        [JsonProperty("price")]
        public decimal? Price { get; set; }

        /// <summary>
        ///  (optional) number (double float)	This variant’s base weight on the storefront. If this value is null, the product’s default weight (set in the Product resource’s weight field) will be used as the base weight.
        /// </summary>
        [JsonProperty("sale_price")]
        public decimal? SalePrice { get; set; }

        [JsonProperty("retail_price")]
        public decimal? RetailPrice { get; set; }

        [JsonProperty("weight")]
        public decimal? Weight { get; set; }

        [JsonProperty("width")]
        public decimal? Width { get; set; }

        [JsonProperty("height")]
        public decimal? Height { get; set; }

        [JsonProperty("depth")]
        public decimal? Depth { get; set; }

        [JsonProperty("is_free_shipping")]
        public bool? IsFreeShipping { get; set; }

        [JsonProperty("fixed_cost_shipping_price")]
        public decimal? FixedCostShippingPrice { get; set; }

        [JsonProperty("purchasing_disabled")]
        public bool? PurchasingDisabled { get; set; }

        [JsonProperty("purchasing_disabled_message")]
        public string PurchasingDisabledMessage { get; set; }

        [JsonProperty("image_url")]
        public string ImageUrl { get; set; }

        [JsonProperty("upc")]
        public string Upc { get; set; }

        /// <summary>
        /// Manufacturer Part Number (MPN) — MPN is a series of letters or numbers assigned to product by its manufacturer. 
        /// There is no MPN standard so its format can vary between different manufacturers/companies.
        /// </summary>
        [JsonProperty("mpn")]
        public string Mpn { get; set; }

        /// <summary>
        /// Global Trade Item Number (GTIN) — An identifier for trade items that is 
        /// incorporated into several product identifying standards like ISBN, UPC, and EAN.
        /// </summary>
        [JsonProperty("gtin")]
        public string Gtin { get; set; }

        [JsonProperty("inventory_level")]
        public int? InventoryLevel { get; set; }

        [JsonProperty("inventory_warning_level")]
        public int? InventoryWarningLevel { get; set; }

        [JsonProperty("bin_picking_number")]
        public string BinPickingNumber { get; set; }

        [JsonProperty("id")]
        public int? Id { get; set; }

        [JsonProperty("product_id")]
        public int? ProductId { get; set; }

        [JsonProperty("sku")]
        public string Sku { get; set; }

        [JsonProperty("sku_id")]
        public int? SkuId { get; set; }

        /// <summary>
        /// Array of option and option values IDs that make up this variant. Will be empty if the variant is the product’s base variant
        /// </summary>
        [JsonProperty("option_values")]
        public IList<ProductVariantOptionValueData> OptionValues { get; set; }

        [JsonIgnore]
        public IList<ProductVariantModifierValueData> ModifierValues { get; set; }

        [JsonProperty("calculated_price")]
        public decimal? CalculatedPrice { get; set; }

		public Guid? LocalID { get; set; }

	}


}
