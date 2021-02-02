using System;
using System.ComponentModel;

namespace PX.Commerce.BigCommerce.API.REST
{
    #region bigcommerce api. products filter params  
    /*
    https://developer.bigcommerce.com/api/v3/catalog.html#catalog-api         
    Parameter	                In	    Type	Required	Description
    --------------------------------------------------------------------------
    id	                        query	integer	false   	Filter items by id.
    name                        query   string	false   	Filter items by name.
    sku                         query   string  false   	Filter items by sku.
    upc                         query   string  false   	Filter items by upc.
    price                       query   number  false   	Filter items by price.
    weight                      query   number  false   	Filter items by weight.
    condition                   query   integer false   	Filter items by condition.
    brand_id                    query   integer false   	Filter items by brand_id.
    date_modified               query   string  false   	Filter items by date_modified.
    date_last_imported          query   string  false   	Filter items by date_last_imported.
    is_visible                  query   integer false   	Filter items by is_visible.
    is_featured                 query   integer false   	Filter items by is_featured.
    is_free_shipping            query   integer false   	Filter items by is_free_shipping.
    inventory_level             query   integer false   	Filter items by inventory_level.
    inventory_low               query   integer false   	Filter items by inventory_low; values: 1, 0.
    out_of_stock                query   integer	false   	Filter items by out_of_stock.To enable the filter, pass out_of_stock = 1.
    total_sold                  query   integer false   	Filter items by total_sold.
    type                        query   string  false   	Filter items by type: physical or digital.
    categories                  query   integer false   	Filter items by categories. (NOTE: To ensure that your request will retrieve products that are also cross-listed in additional categories beyond the categories you’ve specified, use the syntax: categories:in=.)
    keyword                     query   string	false   	Filter items by keywords found in the name, description, or sku fields, or in the brand name.
    keyword_context             query   string	false   	Set context for a product search.
    status                      query   integer	false   	Filter items by status.
    include                     query   string  false   	Sub-resources to include on a product, in a comma-separated list. Valid expansions currently include variants, images, custom_fields, and bulk_pricing_rules.
    include_fields              query   string  false   	Fields to include, in a comma-separated list. The ID and the specified fields will be returned.
    exclude_fields              query   string  false   	Fields to exclude, in a comma-separated list. The specified fields will be excluded from a response.The ID cannot be excluded.
    availability                query   string  false   	Filter items by availability. Acceptable values are: available, disabled, preorder.
    price_list_id               query   integer false   	The ID fo the Price List.
    page                        query   integer false   	Specifies the page number in a limited (paginated) list of products.
    limit                       query   integer false   	Controls the number of items per page in a limited (paginated) list of products.
    direction                   query   string  false   	Sort direction. Acceptable values are: asc, desc.
    sort                        query   string  false   	Field name to sort by

    */
    #endregion

    public class FilterProducts : Filter
    {
        [Description("min:id")]
        public int? MinimumId { get; set; }

        [Description("max:id")]
        public int? MaximumId { get; set; }

        [Description("name")]
        public string Name { get; set; }

        /// <summary>
        /// Names to include, in a comma-separated list
        /// </summary>
        [Description("name:in")]
        public string NameIn { get; set; }

        [Description("sku")]
        public string SKU { get; set; }

        /// <summary>
        /// Filter items by categories. 
        /// NOTE: To ensure that your request will retrieve products that are also cross-listed in 
        /// additional categories beyond the categories  you’ve specified. 
        /// use the syntax: CategoriesIn = "295,296")
        /// </summary>
        [Description("categories:in")]
        public string CategoriesIn { get; set; }

        [Description("date_modified:min")]
        public DateTime? MinDateModified { get; set; }

        [Description("date_modified:max")]
        public DateTime? MaxDateModified { get; set; }


        [Description("date_last_imported:min")]
        public DateTime? MinDateLastImported { get; set; }

        [Description("date_last_imported:max")]
        public DateTime? MaxDateLastImported { get; set; }

        [Description("is_visible")]
        public DateTime? IsVisible { get; set; }

		[Description("type")]
		public String Type { get; set; }

        [Description("include")]
        public string Include { get; set; }
    }
}
