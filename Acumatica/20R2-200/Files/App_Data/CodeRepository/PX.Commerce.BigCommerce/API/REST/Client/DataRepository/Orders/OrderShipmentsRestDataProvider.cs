using PX.Commerce.Core.Model;
using System.Collections.Generic;

namespace PX.Commerce.BigCommerce.API.REST
{
    public class OrderShipmentsRestDataProvider : RestDataProviderV2, IChildRestDataProvider<OrdersShipmentData>
    {
        protected override string GetListUrl { get; }   = "v2/orders/{parent_id}/shipments";
        protected override string GetSingleUrl { get; } = "v2/orders/{parent_id}/shipments/{id}";
        protected override string GetCountUrl { get; }  = "v2/orders/{parent_id}/shipments/count";

        public OrderShipmentsRestDataProvider(IBigCommerceRestClient restClient) : base()
		{
            _restClient = restClient;
		}
        public int Count(string parentId)
        {
            var segments = MakeParentUrlSegments(parentId);
            return GetCount(segments).Count;
        }

        public List<OrdersShipmentData> Get(string parentId)
        {
            var segments = MakeParentUrlSegments(parentId);
            return Get<OrdersShipmentData>(null, segments);
        }

        public IEnumerable<OrdersShipmentData> GetAll(string parentId)
        {
            var segments = MakeUrlSegments(parentId);
            return GetAll<OrdersShipmentData>(null, segments);
        }

        public OrdersShipmentData GetByID(string id, string parentId)
        {
            var segments = MakeUrlSegments(id, parentId);
            return GetByID<OrdersShipmentData>(segments);
        }

        public OrdersShipmentData Create(OrdersShipmentData entity, string parentId)
        {
            var segments = MakeParentUrlSegments(parentId);
            return base.Create(entity, segments);
        }

        public OrdersShipmentData Update(OrdersShipmentData entity, string id, string parentId)
        {
            var segments = MakeUrlSegments(id, parentId);
            return Update(entity, segments);
        }

        public bool Delete(string id, string parentId)
        {
            var segments = MakeUrlSegments(id, parentId);
            return Delete(segments);
        }
    }
}
