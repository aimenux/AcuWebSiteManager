using PX.Commerce.Core.Model;
using System.Collections.Generic;

namespace PX.Commerce.BigCommerce.API.REST
{
    public class OrderCouponsRestDataProvider : RestDataProviderV2, IChildRestDataProvider<OrdersCouponData>
    {
        protected override string GetListUrl { get; } = "v2/orders/{parent_id}/coupons";
        protected override string GetSingleUrl { get; } = "v2/orders/{parent_id}/coupons/{id}";
        protected override string GetCountUrl { get; } = string.Empty;

        public OrderCouponsRestDataProvider(IBigCommerceRestClient restClient) : base()
		{
            _restClient = restClient;
		}

        public int Count(string parentId)
        {
            var segments = MakeUrlSegments(parentId);
            return GetCount(segments).Count;
        }

        public List<OrdersCouponData> Get(string parentId)
        {
            var segments = MakeParentUrlSegments(parentId);
            return Get<OrdersCouponData>(null, segments);
        }

        public IEnumerable<OrdersCouponData> GetAll(string parentId)
        {
            var segments = MakeUrlSegments(parentId);
            return GetAll<OrdersCouponData>(null, segments);
        }
        
        public OrdersCouponData GetByID(string id, string parentId)
        {
            var segments = MakeUrlSegments(id, parentId);
            return GetByID<OrdersCouponData>(segments);
        }

        public OrdersCouponData Create(OrdersCouponData entity, string parentId)
        {
            var segments = MakeParentUrlSegments(parentId);
            return Create(entity, segments);
        }

        public OrdersCouponData Update(OrdersCouponData entity, string id, string parentId)
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
