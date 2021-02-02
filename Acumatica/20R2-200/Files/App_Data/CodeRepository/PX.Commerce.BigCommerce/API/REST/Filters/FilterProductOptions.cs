using System.ComponentModel;

namespace PX.Commerce.BigCommerce.API.REST
{
    class FilterProductOptions : Filter
    {
        [Description("product_id")]
        public int? ProductId { get; set; }
    }
}
