using System.ComponentModel;

namespace PX.Commerce.BigCommerce.API.REST
{


    public class FilterProductVariants : Filter
    {

        [Description("product_id")]
        public int? ProductId { get; set; }

        [Description("variant_id")]
        public int? VariantId { get; set; }

    }
}
