using System.ComponentModel;

namespace PX.Commerce.BigCommerce.API.REST
{
    public class FilterProductBrands : Filter 
    {
        [Description("name")]
        public string Name { get; set; }
    }
}
