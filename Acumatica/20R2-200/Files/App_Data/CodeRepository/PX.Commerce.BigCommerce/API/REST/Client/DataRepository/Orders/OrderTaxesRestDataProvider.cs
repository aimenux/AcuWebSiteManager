using PX.Commerce.Core.Model;
using System.Collections.Generic;

namespace PX.Commerce.BigCommerce.API.REST
{
    public class OrderTaxesRestDataProvider : RestDataProviderV2, IChildRestDataProvider<OrdersTaxData>
    {
        protected override string GetListUrl { get; } = "v2/orders/{parent_id}/taxes?details=true";
        protected override string GetSingleUrl { get; } = "v2/orders/{parent_id}/taxes/{id}?details=true";
        protected override string GetCountUrl { get; } = string.Empty;

        public OrderTaxesRestDataProvider(IBigCommerceRestClient restClient) : base()
		{
            _restClient = restClient;
		}

        public int Count(string parentId)
        {
            var segments = MakeUrlSegments(parentId);
            return GetCount(segments).Count;
        }

        public List<OrdersTaxData> Get(string parentId)
        {
            var segments = MakeParentUrlSegments(parentId);
            return Get<OrdersTaxData>(null, segments);
        }

        public IEnumerable<OrdersTaxData> GetAll(string parentId)
        {
            var segments = MakeParentUrlSegments(parentId);
            return GetAll<OrdersTaxData>(null, segments);
        }
        
        public OrdersTaxData GetByID(string id, string parentId)
        {
            var segments = MakeUrlSegments(id, parentId);
            return GetByID<OrdersTaxData>(segments);
        }

        public OrdersTaxData Create(OrdersTaxData entity, string parentId)
        {
            var segments = MakeParentUrlSegments(parentId);
            return Create(entity, segments);
        }

        public OrdersTaxData Update(OrdersTaxData entity, string id, string parentId)
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
