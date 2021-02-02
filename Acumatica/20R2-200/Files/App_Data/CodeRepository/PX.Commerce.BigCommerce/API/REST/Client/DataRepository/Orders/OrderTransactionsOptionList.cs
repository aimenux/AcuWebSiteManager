using System.Collections.Generic;
using Newtonsoft.Json;

namespace PX.Commerce.BigCommerce.API.REST
{
    [JsonObject(Description = "Order Transaction Option (BigCommerce API v3 response)")]
    public class OrderTransactionsOptionList : IEntitiesResponse<OrdersTransactionData>
    {
        [JsonProperty("data")]
        public List<OrdersTransactionData> Data { get; set; }

        [JsonProperty("meta")]
        public Meta Meta { get; set; }
    }

	public class OrderTransactionsOption : IEntityResponse<OrdersTransactionData>
	{
		[JsonProperty("data")]
		public OrdersTransactionData Data { get; set; }

		[JsonProperty("meta")]
		public Meta Meta { get; set; }
	}
}
