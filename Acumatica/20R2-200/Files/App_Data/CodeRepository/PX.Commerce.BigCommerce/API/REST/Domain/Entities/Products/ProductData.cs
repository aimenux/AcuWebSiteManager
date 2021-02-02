using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using PX.Commerce.Core;
using System.ComponentModel;

namespace PX.Commerce.BigCommerce.API.REST
{
    [JsonObject(Description = "Product")]
	[Description(BigCommerceCaptions.Product)]
    public class ProductData : BCAPIEntity
	{
        public ProductData()
        {
            Categories = new List<int>();
            RelatedProducts = new List<int>();
            CustomFields = new List<ProductsCustomField>();
            BulkPricingRules = new List<ProductsBulkPricingRules>();
            Images = new List<ProductsImageData>();
            Videos = new List<ProductsVideo>();
            Variants = new List<ProductsVariantData>();
            ProductsOptionData = new List<ProductsOptionData>();
        }

        /// <summary>
        /// The product name.
        /// </summary>
        [JsonProperty("name")]
		[Description(BigCommerceCaptions.ProductName)]
		public string Name { get; set; }

        /// <summary>
        /// The product type: physical – a physical stock unit; digital – a digital download.
        /// </summary>
        [JsonProperty("type")]
		[Description(BigCommerceCaptions.ProductType)]
		public string Type { get; set; }

        /// <summary>
        /// User-defined product code/stock keeping unit (SKU).
        /// </summary>
        [JsonProperty("sku")]
		[Description(BigCommerceCaptions.StockKeepingUnit)]
		public string Sku { get; set; }

        /// <summary>
        /// The product description, which can include HTML formatting.
        /// </summary>        
        [JsonProperty("description")]
		[Description(BigCommerceCaptions.ProductDescription)]
		public string Description { get; set; }

        /// <summary>
        /// Weight of the product, which can be used when calculating shipping costs. (optional)	number (double float)
        /// </summary>
        [JsonProperty("weight")]
		[Description(BigCommerceCaptions.Weight)]
		public Decimal? Weight { get; set; }

        /// <summary>
        /// Width of the product, which can be used when calculating shipping costs. (optional)	number (double float)
        /// </summary>
        [JsonProperty("width")]
		[Description(BigCommerceCaptions.Width)]
		public Decimal? Width { get; set; }

        /// <summary>
        /// Depth of the product, which can be used when calculating shipping costs. (optional)	number (double float)
        /// </summary>
        [JsonProperty("depth")]
		[Description(BigCommerceCaptions.Depth)]
		public Decimal? Depth { get; set; }

        /// <summary>
        /// Height of the product, which can be used when calculating shipping costs. (optional)	number (double float)
        /// </summary>
        [JsonProperty("height")]
		[Description(BigCommerceCaptions.Height)]
		public Decimal? Height { get; set; }

        /// <summary>
        /// The price of the product. The price should include or exclude tax, based on the store settings.  Number (double float)	 (optional)
        /// </summary>
        [JsonProperty("price")]
		[Description(BigCommerceCaptions.Price)]
		public Decimal? Price { get; set; }

        /// <summary>
        /// The cost price of the product. Stored for reference only; it is not used or displayed anywhere on the store.  (optional)	number (double float)
        /// </summary>
        [JsonProperty("cost_price")]
		[Description(BigCommerceCaptions.CostPrice)]
		public Decimal? CostPrice { get; set; }

        /// <summary>
        /// The retail cost of the product. If entered, the retail cost price will be shown on the product page. (optional)	number (double float)
        /// </summary>
        [JsonProperty("retail_price")]
		[Description(BigCommerceCaptions.RetailPrice)]
		public Decimal? RetailPrice { get; set; }

        /// <summary>
        /// If entered, the sale price will be used instead of value in the price field when calculating the product’s cost. (optional)	number (double float)
        /// </summary>
        [JsonProperty("sale_price")]
		[Description(BigCommerceCaptions.SalePrice)]
		public Decimal? SalePrice { get; set; }

        /// <summary>
        /// The ID of the tax class applied to the product. (NOTE: Value ignored if automatic tax is enabled.)   (optional)	integer
        /// </summary>
        [JsonProperty("tax_class_id")]
		[Description(BigCommerceCaptions.TaxClassId)]
		public int? TaxClassId { get; set; }

        /// <summary>
        /// Accepts AvaTax System Tax Codes, which identify products and services that fall into special sales-tax categories. By using these codes, 
        /// merchants who subscribe to BigCommerce’s Avalara Premium integration can calculate sales taxes more accurately. Stores without Avalara 
        /// Premium will ignore the code when calculating sales tax. Do not pass more than one code; the codes are case-sensitive. For details, 
        /// please see Avalara’s documentation.  (optional)	string
        /// </summary>
        [JsonProperty("product_tax_code")]
		[Description(BigCommerceCaptions.ProductTaxCode)]
		public string ProductTaxCode { get; set; }

        /// <summary>
        /// An array of IDs for the categories to which this product belongs. When updating a product, 
        /// if an array of categories is supplied, all product categories will be overwritten. 
        /// Does not accept more than 1,000 ID values.
        /// </summary>
        [JsonProperty("categories")]
		public IList<int> Categories { get; set; }

        /// <summary>
        /// The ID associated with the product’s brand.  (optional)	integer
        /// </summary>
        [JsonProperty("brand_id")]
		[Description(BigCommerceCaptions.BrandId)]
		public int? BrandId { get; set; }

        /// <summary>
        /// The type of inventory tracking for the product. 
        /// Values are: 
        /// none – inventory levels will not be tracked; 
        /// product – inventory levels will be tracked using the inventory_level and inventory_warning_level fields; 
        /// variant – inventory levels will be tracked based on variants, which maintain their own warning levels and inventory levels.
        /// (optional) string
        /// </summary>
        [JsonProperty("inventory_tracking")]
		[Description(BigCommerceCaptions.InventoryTracking)]
		public string InventoryTracking { get; set; }

        /// <summary>
        /// A fixed shipping cost for the product. 
        /// If defined, this value will be used during checkout instead of normal shipping-cost calculation.
        ///  (optional)  number (double float)
        /// </summary>
        [JsonProperty("fixed_cost_shipping_price")]
		[Description(BigCommerceCaptions.FixedCostShippingPrice)]
		public Decimal? FixedCostShippingPrice { get; set; }

        /// <summary>
        /// Flag used to indicate whether the product has free shipping. 
        /// If true, the shipping cost for the product will be zero.
        ///  (optional)	boolean
        /// </summary>
        [JsonProperty("is_free_shipping")]
		[Description(BigCommerceCaptions.IsFreeShipping)]
		public bool? IsFreeShipping { get; set; }

        /// <summary>
        /// Flag used to determine whether the product should be displayed to customers browsing the store. 
        /// If true, the product will be displayed. 
        /// If false, the product will be hidden from view.
        ///  (optional)	boolean
        /// </summary>
        [JsonProperty("is_visible")]
		[Description(BigCommerceCaptions.IsVisible)]
		public bool? IsVisible { get; set; }

        /// <summary>
        /// Flag used to determine whether the product should be 
        /// included in the featured products panel when visitors view the store.
        ///  (optional)	boolean
        /// </summary>
        [JsonProperty("is_featured")]
		[Description(BigCommerceCaptions.IsFeatured)]
		public bool? IsFeatured { get; set; }

        /// <summary>
        /// An array of IDs for related products.
        /// (optional)	integer array
        /// </summary>
        [JsonProperty("related_products")]
		public IList<int> RelatedProducts { get; set; }

        /// <summary>
        /// Warranty information displayed on the product page. Can include HTML formatting. 
        /// (optional)	string	
        /// </summary>
        [JsonProperty("warranty")]
		[Description(BigCommerceCaptions.Warranty)]
		public string Warranty { get; set; }

        /// <summary>
        /// The BIN picking number for the product.  (optional)	string	
        /// </summary>
        [JsonProperty("bin_picking_number")]
		[Description(BigCommerceCaptions.BinPickingNumber)]
		public string BinPickingNumber { get; set; }

        /// <summary>
        /// The layout template file used to render this product. (optional)	string	
        /// </summary>
        [JsonProperty("layout_file")]
		[Description(BigCommerceCaptions.LayoutFile)]
		public string LayoutFile { get; set; }

        /// <summary>
        /// The product UPC code, which is used in feeds for shopping comparison 
        /// sites and external channel integrations. (optional)	string	
        /// </summary>
        [JsonProperty("upc")]
		[Description(BigCommerceCaptions.UPCCode)]
		public string Upc { get; set; }

        /// <summary>
        ///  A comma-separated list of keywords that can be used to locate the product when searching the store. (optional)	string	
        /// </summary>
        [JsonProperty("search_keywords")]
		[Description(BigCommerceCaptions.SearchKeywords)]
		public string SearchKeywords { get; set; }

        /// <summary>
        /// Type of gift-wrapping options. Values: 
        /// any – allow any gift-wrapping options in the store; 
        /// none – disallow gift-wrapping on the product; 
        /// list – provide a list of IDs in the gift_wrapping_options_list field. 
        /// (optional)	string	
        /// </summary>
        [JsonProperty("gift_wrapping_options_type")]
		[Description(BigCommerceCaptions.GiftWrappingOptionsType)]
		public string GiftWrappingOptionsType { get; set; }

        /// <summary>
        /// A list of gift-wrapping option IDs.  
        /// (optional)	integer array	
        /// </summary>
        [JsonProperty("gift_wrapping_options_list")]
        public int[] GiftWrappingOptionsList { get; set; }


        /// <summary>
        /// Priority to give this product when included in product lists 
        /// on category pages and in search results. 
        /// Lower integers will place the product closer to the top of the results.
        /// (optional)	integer
        /// </summary>
        [JsonProperty("sort_order")]
		[Description(BigCommerceCaptions.SortOrder)]
		public int? SortOrder { get; set; }

        /// <summary>
        /// 
        /// </summary>
        [JsonProperty("condition")]
		[Description(BigCommerceCaptions.Condition)]
		public string Condition { get; set; }

        /// <summary>
        /// The product condition. 
        /// Will be shown on the product page if the is_condition_shown field’s value is true. 
        /// Possible values: New, Used, Refurbished.        
        /// (optional)	string	
        /// </summary>
        [JsonProperty("is_condition_shown")]
		[Description(BigCommerceCaptions.IsConditionShown)]
		public bool? IsConditionShown { get; set; }

        /// <summary>
        /// The minimum quantity an order must contain, to be eligible to purchase this product. (optional)	integer	
        /// </summary>
        [JsonProperty("order_quantity_minimum")]
		[Description(BigCommerceCaptions.MinimumOrderQuantity)]
		public int? OrderQuantityMinimum { get; set; }

        /// <summary>
        /// The maximum quantity an order can contain when purchasing the product.  (optional)	integer	
        /// </summary>
        [JsonProperty("order_quantity_maximum")]
		[Description(BigCommerceCaptions.MaximumOrderQuantityn)]
		public int? OrderQuantityMaximum { get; set; }

        /// <summary>
        /// Custom title for the product page. If not defined, the product name will be used as the meta title.  (optional)	string	
        /// </summary>
        [JsonProperty("page_title")]
		[Description(BigCommerceCaptions.PageTitle)]
		public string PageTitle { get; set; }

        /// <summary>
        ///  Custom meta keywords for the product page. If not defined, the store’s default keywords will be used. (optional)	string array	
        /// </summary>
        [JsonProperty("meta_keywords")]
		public string[] MetaKeywords { get; set; }

        /// <summary>
        /// Custom meta description for the product page. If not defined, the store’s default meta description will be used. (optional)	string	
        /// </summary>
        [JsonProperty("meta_description")]
		[Description(BigCommerceCaptions.MetaDescription)]
		public string MetaDescription { get; set; }

        /// <summary>
        /// The number of times the product has been viewed.   (optional)	integer	
        /// </summary>
        [JsonProperty("view_count")]
		[Description(BigCommerceCaptions.ViewCount)]
		public int? ViewCount { get; set; }

        /// <summary>
        /// Pre-order release date. See the availability field for details on setting a product’s availability to accept pre-orders.   
        /// (optional)	DateTime string	
        /// </summary>
        [JsonProperty("preorder_release_date")]
		[Description(BigCommerceCaptions.PreorderReleaseDate)]
		public string PreorderReleaseDate { get; set; }

        /// <summary>
        /// Custom expected-date message to display on the product page. 
        /// If undefined, the message defaults to the storewide setting. 
        /// Can contain the %%DATE%% placeholder, which will be substituted for the release date.
        /// (optional)	string	
        /// </summary>
        [JsonProperty("preorder_message")]
		[Description(BigCommerceCaptions.PreorderMessage)]
		public string PreorderMessage { get; set; }

        /// <summary>
        /// If set to false, the product will not change its availability from preorder to available on the release date. 
        /// Otherwise, the product’s availability/status will change to available on the release date.
        ///  (optional)	boolean
        /// </summary>
        [JsonProperty("is_preorder_only")]
		[Description(BigCommerceCaptions.IsPreorderOnly)]
		public bool? IsPreorderOnly { get; set; }

        /// <summary>
        /// Set to false by default, indicating that this product’s price should be shown on the product page. 
        /// If set to true, the price is hidden. 
        /// NOTE: To successfully set is_price_hidden to true, the availability value must be disabled.
        /// (optional)	boolean
        /// </summary>
        //[JsonProperty("is_price_hidden")]
        //public bool? IsPriceHidden { get; set; }

        /// <summary>
        /// By default, an empty string. If is_price_hidden is true, the value of price_hidden_label is displayed instead of the price. 
        /// (NOTE: To successfully set a non-empty string value, with is_price_hidden set to true, the availability value must be disabled.)
        /// (optional)	string
        /// </summary>
        [JsonProperty("price_hidden_label")]
		[Description(BigCommerceCaptions.PriceHiddenLabel)]
		public string PriceHiddenLabel { get; set; }

        /// <summary>
        /// The custom URL for the product on the storefront. 
        /// inline_response_200_custom_url	
        /// </summary>
        [JsonProperty("custom_url")]
		public ProductCustomUrl CustomUrl { get; set; }

        [JsonProperty("open_graph_type")]
		public string OpenGraphType { get; set; }

        [JsonProperty("open_graph_title")]
		public string OpenGraphTitle { get; set; }

        [JsonProperty("open_graph_description")]
		public string OpenGraphDescription { get; set; }

        [JsonProperty("open_graph_use_meta_description")]
		public bool? OpenGraphUseMetaDescription { get; set; }

        [JsonProperty("open_graph_use_product_name")]
		public bool? OpenGraphUseProductName { get; set; }

        [JsonProperty("open_graph_use_image")]
		public bool? OpenGraphUseImage { get; set; }

        [JsonProperty("id")]
        public int? Id { get; set; }

        [JsonProperty("calculated_price")]
        public decimal? CalculatedPrice { get; set; }

        [JsonProperty("reviews_rating_sum")]
        public int? ReviewsRatingSum { get; set; }

        [JsonProperty("reviews_count")]
        public int? ReviewsCount { get; set; }

        [JsonProperty("total_sold")]
        public int? TotalSold { get; set; }

        [Automap]
        [Description(BigCommerceCaptions.CustomFields)]
        public IList<ProductsCustomField> CustomFields { get; set; }

        /// <summary>
        /// Common BulkPricingRule properties
        /// </summary>
        [JsonProperty("bulk_pricing_rules")]
        public IList<ProductsBulkPricingRules> BulkPricingRules { get; set; }

        [JsonProperty("date_created")]
        public string DateCreatedUT { get; set; }

        [JsonProperty("date_modified")]
        public string DateModifiedUT { get; set; }

        [JsonProperty("images")]
        public IList<ProductsImageData> Images { get; set; }

        [JsonProperty("videos")]
        public IList<ProductsVideo> Videos { get; set; }

        [JsonProperty("variants")]
        public IList<ProductsVariantData> Variants { get; set; }

        [JsonProperty("base_variant_id")]
        public int? BaseVariantId { get; set; }

		[JsonProperty("gtin")]
		[Description(BigCommerceCaptions.GlobalTradeNumber)]
		public string GTIN { get; set; }

		[JsonProperty("mpn")]
		[Description(BigCommerceCaptions.ManufacturerPartNumber)]
		public string MPN { get; set; }

		/// <summary>
		///  The product’s availability. Availability options are: 
		///  available – the product can be purchased in the storefront; 
		///  disabled – the product is listed in the storefront, but cannot be purchased; 
		///  preorder – the product is listed for pre-orders.
		///  (optional)	string	
		/// </summary>
		[JsonProperty("availability")]
		[Description(BigCommerceCaptions.Availability)]
		public string Availability { get; set; }

        [JsonProperty("options")]
		public IList<ProductsOptionData> ProductsOptionData { get; set; }


		public override string ToString()
        {
            return $"{Name}, ID:{this.Id}, SKU:{this.Sku}"; 
        }

    }
	[Description(BigCommerceCaptions.ProductQuantityData)]
	public class ProductQtyData : BCAPIEntity
	{
		[JsonProperty("id")]
		[Description(BigCommerceCaptions.Productid)]
		public int? Id { get; set; }

		/// <summary>
		/// Current inventory level of the product. Simple inventory tracking 
		/// must be enabled (see the inventory_tracking field) for this to 
		/// take effect.  (optional)	integer
		/// </summary>
		[JsonProperty("inventory_level")]
		[Description(BigCommerceCaptions.InventoryLevel)]
		public int? InventoryLevel { get; set; }

		/// <summary>
		/// Inventory warning level for the product. When the product’s inventory level drops below the warning level, 
		/// the store owner will be informed. Simple inventory tracking must be enabled 
		/// (see the inventory_tracking field) for this to take effect. (optional)	integer
		/// </summary>
		[JsonProperty("inventory_warning_level")]
		public int? InventoryWarningLevel { get; set; }

		/// <summary>
		/// The type of inventory tracking for the product. 
		/// Values are: 
		/// none – inventory levels will not be tracked; 
		/// product – inventory levels will be tracked using the inventory_level and inventory_warning_level fields; 
		/// variant – inventory levels will be tracked based on variants, which maintain their own warning levels and inventory levels.
		/// (optional) string
		/// </summary>
		[JsonProperty("inventory_tracking")]
		[Description(BigCommerceCaptions.InventoryTracking)]
		public string InventoryTracking { get; set; }

		public bool? IsPreorderOnly { get; set; }

		/// <summary>
		/// Set to false by default, indicating that this product’s price should be shown on the product page. 
		/// If set to true, the price is hidden. 
		/// NOTE: To successfully set is_price_hidden to true, the availability value must be disabled.
		/// (optional)	boolean
		/// </summary>
		[JsonProperty("is_price_hidden")]
		public bool? IsPriceHidden { get; set; }

		/// <summary>
		///  The product’s availability. Availability options are: 
		///  available – the product can be purchased in the storefront; 
		///  disabled – the product is listed in the storefront, but cannot be purchased; 
		///  preorder – the product is listed for pre-orders.
		///  (optional)	string	
		/// </summary>
		[JsonProperty("availability")]
		[Description(BigCommerceCaptions.ProductsAvailability)]
		public string Availability { get; set; }

		/// <summary>
		/// Availability text displayed on the checkout page, under the product title. 
		/// Tells the customer how long it will normally take to ship this product, 
		/// for example: “Usually ships in 24 hours.” 
		/// (optional)	string	
		/// </summary>
		[JsonProperty("availability_description")]
		[Description(BigCommerceCaptions.AvailabilityDescription)]
		public string AvailabilityDescription { get; set; }

		[JsonIgnore]
		public virtual DateTime? DateCreated { get; set; }

		[JsonProperty("date_created")]
		public string DateCreatedUT
		{
			get => DateCreated.TDToString();
			set => DateCreated = value.ToDate();
		}

		[JsonIgnore]
		public virtual DateTime? DateModified { get; set; }

		[JsonProperty("date_modified")]
		public string DateModifiedUT
		{
			get => DateModified.TDToString();
			set => DateModified = value.ToDate();
		}
		[JsonProperty("variants")]
		public IList<ProductsVariantData> Variants { get; set; }
	}

	[Description(BigCommerceCaptions.RelatedProductsData)]
	public class RelatedProductsData : BCAPIEntity
	{
		[JsonProperty("id")]
		[Description(BigCommerceCaptions.Productid)]
		public int? Id { get; set; }

		/// <summary>
		/// An array of IDs for related products.
		/// (optional)	integer array
		/// </summary>
		[JsonProperty("related_products")]
		public IList<int> RelatedProducts { get; set; }
		[JsonIgnore]
		public virtual DateTime? DateCreated { get; set; }

		[JsonProperty("date_created")]
		public string DateCreatedUT
		{
			get => DateCreated.TDToString();
			set => DateCreated = value.ToDate();
		}

		[JsonIgnore]
		public virtual DateTime? DateModified { get; set; }

		[JsonProperty("date_modified")]
		public string DateModifiedUT
		{
			get => DateModified.TDToString();
			set => DateModified = value.ToDate();
		}
	}


	public enum ProductTypes
	{
		physical,
		digital
	}
}
