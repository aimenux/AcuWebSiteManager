using System;
using System.Collections.Generic;
using System.ComponentModel;
using Newtonsoft.Json;
using PX.Commerce.Core;

namespace PX.Commerce.Shopify.API.REST
{
	public class ProductVariantResponse : IEntityResponse<ProductVariantData>
	{
		[JsonProperty("variant")]
		public ProductVariantData Data { get; set; }
	}

	public class ProductVariantsResponse : IEntitiesResponse<ProductVariantData>
	{
		[JsonProperty("variants")]
		public IEnumerable<ProductVariantData> Data { get; set; }
	}

	[JsonObject(Description = "Product -> Product Variant")]
	[Description(ShopifyCaptions.ProductVariants)]
	public class ProductVariantData : BCAPIEntity
	{
		public ProductVariantData()
		{
			
		}
		/// <summary>
		/// The barcode, UPC, or ISBN number for the product.
		/// </summary>
		[JsonProperty("barcode", NullValueHandling = NullValueHandling.Ignore)]
		[Description(ShopifyCaptions.Barcode)]
		public string Barcode { get; set; }

		/// <summary>
		/// The original price of the item before an adjustment or a sale.
		/// </summary>
		[JsonProperty("compare_at_price", NullValueHandling = NullValueHandling.Ignore)]
		[Description(ShopifyCaptions.RetailPrice)]
		public decimal? OriginalPrice { get; set; }

		/// <summary>
		/// The date and time (ISO 8601 format) when the product variant was created.
		/// </summary>
		[JsonProperty("created_at")]
		[ShouldNotSerialize]
		public DateTime? DateCreatedAt { get; set; }

		/// <summary>
		/// The fulfillment service associated with the product variant. Valid values: manual or the handle of a fulfillment service.
		/// </summary>
		[JsonProperty("fulfillment_service", NullValueHandling = NullValueHandling.Ignore)]
		public string FulfillmentService { get; set; }

		/// <summary>
		/// The weight of the product variant in grams.
		/// </summary>
		[JsonProperty("grams", NullValueHandling = NullValueHandling.Ignore)]
		public decimal? Grams { get; set; }

		/// <summary>
		/// The unique numeric identifier for the product variant.
		/// </summary>
		[JsonProperty("id", NullValueHandling = NullValueHandling.Ignore)]
		public long? Id { get; set; }

		/// <summary>
		/// The unique numeric identifier for a product's image. The image must be associated to the same product as the variant.
		/// </summary>
		[JsonProperty("image_id", NullValueHandling = NullValueHandling.Ignore)]
		public long? ImageId { get; set; }

		/// <summary>
		/// The unique identifier for the inventory item, which is used in the Inventory API to query for inventory information.
		/// </summary>
		[JsonProperty("inventory_item_id")]
		[ShouldNotSerialize]
		public long? InventoryItemId { get; set; }

		/// <summary>
		/// The fulfillment service that tracks the number of items in stock for the product variant. If you track the inventory yourself using the admin, then set the value to "shopify". 
		/// Valid values: shopify or the handle of a fulfillment service that has inventory management enabled. Must be the same fulfillment service referenced by the fulfillment_service property.
		/// </summary>
		[JsonProperty("inventory_management", NullValueHandling = NullValueHandling.Include)]
		[Description(ShopifyCaptions.InventoryManagement)]
		public string InventoryManagement { get; set; }

		/// <summary>
		/// Whether customers are allowed to place an order for the product variant when it's out of stock. Valid values:
		/// deny: Customers are not allowed to place orders for the product variant if it's out of stock.
		/// continue: Customers are allowed to place orders for the product variant if it's out of stock.
		/// Default value: deny.
		/// </summary>
		[JsonProperty("inventory_policy", NullValueHandling = NullValueHandling.Ignore)]
		[Description(ShopifyCaptions.InventoryPolicy)]
		public InventoryPolicy? InventoryPolicy { get; set; } 

		/// <summary>
		/// [READ-ONLY] An aggregate of inventory across all locations. To adjust inventory at a specific location, use the InventoryLevel resource.
		/// </summary>
		[JsonProperty("inventory_quantity")]
		[ShouldNotSerialize]
		public int? InventoryQuantity { get; set; }

		/// <summary>
		/// The custom properties that a shop owner uses to define product variants.
		/// Default value: Default Title.
		/// </summary>
		[JsonProperty("option1", NullValueHandling = NullValueHandling.Ignore)]
		[Description(ShopifyCaptions.Option1)]
		public string Option1 { get; set; }

        [JsonIgnore]
        public int OptionSortOrder1 { get; set; } = 0;

		/// <summary>
		/// The custom properties that a shop owner uses to define product variants.
		/// Default value: null.
		/// </summary>
		[JsonProperty("option2", NullValueHandling = NullValueHandling.Ignore)]
		[Description(ShopifyCaptions.Option2)]
		public string Option2 { get; set; }

        [JsonIgnore]
        public int OptionSortOrder2 { get; set; } = 0;

        /// <summary>
        /// The custom properties that a shop owner uses to define product variants.
        /// Default value: null.
        /// </summary>
        [JsonProperty("option3", NullValueHandling = NullValueHandling.Ignore)]
		[Description(ShopifyCaptions.Option3)]
		public string Option3 { get; set; }

        [JsonIgnore]
        public int OptionSortOrder3 { get; set; } = 0;

        /// <summary>
        /// A list of the variant's presentment prices and compare-at prices in each of the shop's enabled presentment currencies. Each price object has the following properties:
        /// currency_code: The three-letter code (ISO 4217 format) for one of the shop's enabled presentment currencies.
        /// amount: The variant's price or compare-at price in the presentment currency.
        /// Requires the header 'X-Shopify-Api-Features': 'include-presentment-prices'.
        /// </summary>
        [JsonProperty("presentment_prices", NullValueHandling = NullValueHandling.Ignore)]
		[ApiHeaderRequest("X-Shopify-Api-Features", "include-presentment-prices")]
		public List<ProductVariantPresentmentPriceData> PresentmentPrices { get; set; }

		/// <summary>
		/// The order of the product variant in the list of product variants. The first position in the list is 1. The position of variants is indicated by the order in which they are listed.
		/// </summary>
		[JsonProperty("position", NullValueHandling = NullValueHandling.Ignore)]
		[ShouldNotSerialize]
		public int? Position { get; set; }

		/// <summary>
		/// The price of the product variant.
		/// </summary>
		[JsonProperty("price", NullValueHandling = NullValueHandling.Ignore)]
		[Description(ShopifyCaptions.SalePrice)]
		public decimal? Price { get; set; }

		/// <summary>
		///The unique numeric identifier for the product.
		/// </summary>
		[JsonProperty("product_id", NullValueHandling = NullValueHandling.Ignore)]
		public long? ProductId { get; set; }

		/// <summary>
		/// A unique identifier for the product variant in the shop. Required in order to connect to a FulfillmentService.
		/// </summary>
		[JsonProperty("sku", NullValueHandling = NullValueHandling.Ignore)]
		[Description(ShopifyCaptions.SKU)]
		public String Sku { get; set; }

		/// <summary>
		/// Whether a tax is charged when the product variant is sold.
		/// </summary>
		[JsonProperty("taxable", NullValueHandling = NullValueHandling.Ignore)]
		[Description(ShopifyCaptions.Taxable)]
		public bool? Taxable { get; set; }

		/// <summary>
		/// This parameter applies only to the stores that have the Avalara AvaTax app installed. Specifies the Avalara tax code for the product variant.
		/// </summary>
		[JsonProperty("tax_code", NullValueHandling = NullValueHandling.Ignore)]
		[Description(ShopifyCaptions.AvalaraTaxCode)]
		public String TaxCode { get; set; }

		/// <summary>
		/// The title of the product variant.
		/// </summary>
		[JsonProperty("title", NullValueHandling = NullValueHandling.Ignore)]
		[Description(ShopifyCaptions.Title)]
		public String Title { get; set; }

		/// <summary>
		/// The date and time when the product variant was last modified. Gets returned in ISO 8601 format.
		/// </summary>
		[JsonProperty("updated_at")]
		[ShouldNotSerialize]
		public DateTime? DateModifiedAt { get; set; }

		/// <summary>
		/// The weight of the product variant in the unit system specified with weight_unit.
		/// </summary>
		[JsonProperty("weight", NullValueHandling = NullValueHandling.Ignore)]
		[Description(ShopifyCaptions.Weight)]
		public decimal? Weight { get; set; }

		/// <summary>
		///The unit of measurement that applies to the product variant's weight. If you don't specify a value for weight_unit, then the shop's default unit of measurement is applied. Valid values: g, kg, oz, and lb.
		/// </summary>
		[JsonProperty("weight_unit", NullValueHandling = NullValueHandling.Ignore)]
		[Description(ShopifyCaptions.WeightUnit)]
		public String WeightUnit { get; set; }

		/// <summary>
		/// If value is false : Customers won’t enter their shipping address or choose a shipping method when buying this product.
		/// If value is true : Shipping address is required when buying this product
		/// </summary>
		[JsonProperty("requires_shipping", NullValueHandling = NullValueHandling.Ignore)]
		public bool? RequiresShipping { get; set; }

		/// <summary>
		/// Attaches additional metadata to a shop's resources:
		///key(required) : An identifier for the metafield(maximum of 30 characters).
		///namespace(required): A container for a set of metadata(maximum of 20 characters). Namespaces help distinguish between metadata that you created and metadata created by another individual with a similar namespace.
		///value (required): Information to be stored as metadata.
		///value_type(required): The value type.Valid values: string and integer.
		///description(optional): Additional information about the metafield.
		/// </summary>
		[JsonProperty("metafields", NullValueHandling = NullValueHandling.Ignore)]
        [Description(ShopifyCaptions.Metafields)]
        [BCMetaFields]
        public List<MetafieldData> Metafields { get; set; }

		[JsonIgnore]
		public Guid? LocalID { get; set; }
	}


}
