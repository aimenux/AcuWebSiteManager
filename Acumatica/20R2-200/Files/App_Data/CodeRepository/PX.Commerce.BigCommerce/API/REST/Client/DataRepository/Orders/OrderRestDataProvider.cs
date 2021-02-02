using PX.Commerce.Core.Model;
using System;
using System.Collections.Generic;

namespace PX.Commerce.BigCommerce.API.REST
{
    public class OrderRestDataProvider : RestDataProviderV2
    {
        protected override string GetListUrl { get; } = "v2/orders";
        protected override string GetSingleUrl { get; } = "v2/orders/{id}";
        protected override string GetCountUrl { get; } = "v2/orders/count";

		public OrderRestDataProvider(IBigCommerceRestClient restClient) : base()
		{
            _restClient = restClient;

		}

        public ItemCount Count()
        {
            return GetCount();
        }

        public ItemCount Count(IFilter filter)
        {
            return GetCount(filter);
        }

		public OrderData Create(OrderData order)
		{
			var newOrder = Create<OrderData>(order);
			return newOrder;
		}

        public OrderData Update(OrderData order, int id)
		{
			var segments = MakeUrlSegments(id.ToString());
			var updated = Update(order, segments);
			return updated;

		}

		public OrderStatus Update(OrderStatus order, string id)
		{
			var segments = MakeUrlSegments(id);
			var updated = Update(order, segments);
			return updated;
		}

		public bool Delete(OrderData order, int id)
        {
            return Delete(id);
        }

        public bool Delete(int id)
        {
            var segments = MakeUrlSegments(id.ToString());
            return base.Delete(segments);
        }

        public List<OrderData> Get(IFilter filter = null)
		{
			return base.Get<OrderData>(filter);
        }

        public IEnumerable<OrderData> GetAll(IFilter filter = null)
		{
			return base.GetAll<OrderData>(filter);
        }

        public OrderData GetByID(string id)
		{
			var segments = MakeUrlSegments(id);
            var orderData = GetByID<OrderData>(segments);

			return orderData;
        }
    }
}
