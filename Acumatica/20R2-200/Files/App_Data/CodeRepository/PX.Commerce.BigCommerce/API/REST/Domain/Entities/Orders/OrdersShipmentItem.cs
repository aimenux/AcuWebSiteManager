using Newtonsoft.Json;
using System;
using System.ComponentModel;

namespace PX.Commerce.BigCommerce.API.REST
{
    [JsonObject(Description = "Order -> Shipment -> ShipmentItem")]
    public class OrdersShipmentItem
    {
        /// <summary>
        /// The ID of the Order Product the item is associated with.
        /// </summary>
        [JsonProperty("order_product_id")]
        public virtual int? OrderProductId { get; set; }

        /// <summary>
        /// The ID of the Product the item is associated with.
        /// </summary>
        [JsonProperty("product_id")]
        public virtual int ProductId { get; set; }

        /// <summary>
        /// The quantity of the item in the shipment.
        /// </summary>
        [JsonProperty("quantity")]
		[Description(BigCommerceCaptions.Quantity)]
		public virtual int Quantity { get; set; }

		[JsonIgnore]
		public virtual string OrderID { get; set; }
        
        [JsonIgnore]
        public virtual Guid? PackageId { get; set; }
    }
}
