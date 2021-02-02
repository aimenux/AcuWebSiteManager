using System;
using System.ComponentModel;

namespace PX.Commerce.BigCommerce.API.REST
{
    // bigcommerce api orders filter parameters https://developer.bigcommerce.com/api/v2/#list-orders
    public class FilterOrders: Filter
    {
        [Description("min_id")]
        public int? MinimumId { get; set; }

        [Description("max_id")]
        public int? MaximumId { get; set; }

        [Description("min_total")]
        public int? MinTotal { get; set; }

        [Description("max_total")]
        public int? MaxTotal { get; set; }

        [Description("customer_id")]
        public string CustomerId { get; set; }

        [Description("email")]
        public string Email { get; set; }

        [Description("status_id")]
        public int? StatusId { get; set; }

        [Description("cart_id")]
        public string CartId { get; set; }

        /// <summary>
        /// ‘true’ or 'false’	  
        /// </summary>
        [Description("is_deleted")]
        public string IsDeleted { get; set; }

        [Description("payment_method")]
        public string PaymentMethod { get; set; }

        [Description("min_date_created")]
        public DateTime? MinDateCreated { get; set; }

        [Description("max_date_created")]
        public DateTime? MaxDateCreated  { get; set; }

        [Description("min_date_modified")]
        public DateTime?  MinDateModified { get; set; }

        [Description("max_date_modified")]
        public DateTime? MaxDateModified  { get; set; }

        [Description("sort")]
        public int? Sort { get; set; }
    }


}
