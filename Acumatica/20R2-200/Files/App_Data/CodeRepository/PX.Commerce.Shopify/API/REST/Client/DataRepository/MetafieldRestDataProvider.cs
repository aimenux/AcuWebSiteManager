using PX.Commerce.Core.Model;
using System;
using System.Collections.Generic;

namespace PX.Commerce.Shopify.API.REST
{
    public class MetafieldRestDataProvider : RestDataProviderBase, IParentRestDataProvider<MetafieldData>
    {
        protected override string GetListUrl { get; } = "metafields.json";
        protected override string GetSingleUrl { get; } = "metafields/{id}.json";
		protected override string GetCountUrl { get; } = "metafields/count.json";
		protected override string GetSearchUrl => throw new NotImplementedException();

		public MetafieldRestDataProvider(IShopifyRestClient restClient) : base()
		{
            ShopifyRestClient = restClient;
		}

        #region IParentDataRestClient

        public ItemCount Count()
        {
			return base.GetCount();
        }

        public ItemCount Count(IFilter filter)
        {
			return base.GetCount(filter);
		}

        public MetafieldData Create(MetafieldData entity)
        {
            var result = base.Create<MetafieldData, MetafieldResponse>(entity);
            return result;
        }

		public MetafieldData Update(MetafieldData entity, string id)
        {
			var segments = MakeUrlSegments(id);
			return base.Update<MetafieldData, MetafieldResponse>(entity, segments);
		}

		public bool Delete(string id)
        {
            var segments = MakeUrlSegments(id);
            return base.Delete(segments);
        }

        public IEnumerable<MetafieldData> GetAll(IFilter filter = null)
        {
            var result = base.GetAll<MetafieldData, MetafieldsResponse>(filter);
            return result;
        }

        public MetafieldData GetByID(string id)
        {
            var segments = MakeUrlSegments(id);
            var result = GetByID<MetafieldData, MetafieldResponse>(segments);
            return result;
        }

		public IEnumerable<MetafieldData> GetCurrentList(out string previousList, out string nextList, IFilter filter = null)
		{
			var result = GetCurrentList<MetafieldData, MetafieldsResponse>(out previousList, out nextList, filter);
			return result;
		}

		public MetafieldData GetMetafieldBySpecifiedUrl(string url, string id)
		{
			var request = BuildRequest(url, nameof(GetMetafieldBySpecifiedUrl), MakeUrlSegments(id), null);
			return ShopifyRestClient.Get<MetafieldData, MetafieldResponse>(request);
		}

		public IEnumerable<MetafieldData> GetMetafieldsBySpecifiedUrl(string url, string id)
		{
			var request = BuildRequest(url, nameof(GetMetafieldBySpecifiedUrl), MakeUrlSegments(id), null);
			return ShopifyRestClient.GetAll<MetafieldData, MetafieldsResponse>(request);
		}
		#endregion
	}
}
