using Newtonsoft.Json;

namespace PX.Commerce.BigCommerce.API.REST
{

    public class OrdersProductsDiscount
    {
        /// <summary>
        /// The identifier for this discount. Possible IDs are listed below:
        ///
        /// coupon (A regular per-item coupon)
        /// total-coupon (A coupon discounting the order total)
        /// Discount Rule ID (Internal ID of a Discount Rule) 
        /// </summary>
        [JsonProperty("id")]
        public virtual string Id { get; set; }

        /// <summary>
        /// The amount of the discount 
        /// </summary>
        [JsonProperty("amount")]
        public virtual decimal DiscountAmount { get; set; }
    }
}
