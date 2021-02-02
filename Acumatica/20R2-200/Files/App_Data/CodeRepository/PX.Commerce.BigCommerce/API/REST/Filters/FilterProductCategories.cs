using System.ComponentModel;

namespace PX.Commerce.BigCommerce.API.REST
{
    #region bigcommerce api. product categories filter params  
    /*
    https://developer.bigcommerce.com/api/v3/catalog.html#getcategories

    Parameter	    In	    Type	Required	Description
    -----------------------------------------------------------------------------     
    name	        query	string	false	    Filter items by name.
    parent_id	    query	integer	false	    Filter items by parent_id.
    page_title	    query	string	false	    Filter items by page_title.
    keyword	        query	string	false	    Filter items by keywords.
    is_visible	    query	integer	false	    Filter items by is_visible.
    page	        query	integer	false	    Specifies the page number in a limited (paginated) list of products.
    limit	        query	integer	false	    Controls the number of items per page in a limited (paginated) list of products.
    include_fields	query	string	false	    Fields to include, in a comma-separated list. The ID and the specified fields will be returned.
    exclude_fields	query	string	false	    Fields to exclude, in a comma-separated list. The specified fields will be excluded from a response. The ID cannot be excluded.
     */
    #endregion

    public class FilterProductCategories : Filter
    {
        [Description("parent_id")]
        public int? ParentId { get; set; }

        [Description("name")]
        public string Name { get; set; }

        [Description("is_visible")]
        public int? IsVisible { get; set; }
    }

}
