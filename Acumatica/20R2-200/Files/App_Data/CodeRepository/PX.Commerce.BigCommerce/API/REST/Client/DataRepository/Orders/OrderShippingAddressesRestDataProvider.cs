using PX.Commerce.Core.Model;
using System.Collections.Generic;

namespace PX.Commerce.BigCommerce.API.REST
{
    public class OrderShippingAddressesRestDataProvider : RestDataProviderV2, IChildRestDataProvider<OrdersShippingAddressData>
    {
        protected override string GetListUrl { get; }   = "v2/orders/{parent_id}/shipping_addresses";
        protected override string GetSingleUrl { get; } = "v2/orders/{parent_id}/shipping_addresses/{id}";
        protected override string GetCountUrl { get; }  = "v2/orders/{parent_id}/shipping_addresses/count";

        public OrderShippingAddressesRestDataProvider(IBigCommerceRestClient restClient) : base()
		{
            _restClient = restClient;
		}
        public int Count(string parentId)
        {
            var segments = MakeUrlSegments(parentId);
            return GetCount(segments).Count;
        }

        public List<OrdersShippingAddressData> Get(string parentId)
        {
            var segments = MakeParentUrlSegments(parentId);
            return Get<OrdersShippingAddressData>(null, segments);
        }

        public IEnumerable<OrdersShippingAddressData> GetAll(string parentId)
        {
            var segments = MakeUrlSegments(parentId);
            return GetAll<OrdersShippingAddressData>(null, segments);
        }
        
        public OrdersShippingAddressData GetByID(string parentId, string id)
        {
            var segments = MakeUrlSegments(id, parentId);
            return GetByID<OrdersShippingAddressData>(segments);
        }

        public OrdersShippingAddressData Create(OrdersShippingAddressData entity, string parentId)
        {
            var segments = MakeParentUrlSegments(parentId);
            return Create(entity, segments);
        }

        public OrdersShippingAddressData Update(OrdersShippingAddressData entity, string id, string parentId)
        {
            var segments = MakeUrlSegments(id, parentId);
            return Update(entity, segments);
        }

        public bool Delete(string id, string parentId)
        {
            var segments = MakeUrlSegments(id, parentId);
            return base.Delete(segments);
        }
    }
}
