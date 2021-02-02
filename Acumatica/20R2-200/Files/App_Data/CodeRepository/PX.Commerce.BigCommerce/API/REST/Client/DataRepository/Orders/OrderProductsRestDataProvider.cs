using PX.Commerce.Core.Model;
using System.Collections.Generic;

namespace PX.Commerce.BigCommerce.API.REST
{
    public class OrderProductsRestDataProvider : RestDataProviderV2, IChildRestDataProvider<OrdersProductData>
    {
        protected override string GetListUrl { get; }   = "v2/orders/{parent_id}/products";
        protected override string GetSingleUrl { get; } = "v2/orders/{parent_id}/products/{id}";
        protected override string GetCountUrl { get; }  = "v2/orders/{parent_id}/products/count";

        public OrderProductsRestDataProvider(IBigCommerceRestClient restClient) : base()
		{
            _restClient = restClient;
		}
        public int Count(string parentId)
        {
            var segments = MakeUrlSegments(parentId);
            return GetCount(segments).Count;
        }

        public List<OrdersProductData> Get(string parentId)
        {
            var segments = MakeParentUrlSegments(parentId);
            return Get<OrdersProductData>(null, segments);
        }

        public IEnumerable<OrdersProductData> GetAll(string parentId)
        {
            var segments = MakeUrlSegments(parentId);
            return GetAll<OrdersProductData>(null, segments);
        }
        
        public OrdersProductData GetByID(string id, string parentId)
        {
            var segments = MakeUrlSegments(id, parentId);
            return GetByID<OrdersProductData>(segments);
        }

        public OrdersProductData Create(OrdersProductData entity, string parentId)
        {
            var segments = MakeParentUrlSegments(parentId);
            return Create(entity, segments);
        }

        public OrdersProductData Update(OrdersProductData entity, string id, string parentId)
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
