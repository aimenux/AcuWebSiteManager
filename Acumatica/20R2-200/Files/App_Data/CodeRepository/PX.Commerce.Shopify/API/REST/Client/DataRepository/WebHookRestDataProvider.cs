using PX.Commerce.Core.Model;
using System;
using System.Collections.Generic;

namespace PX.Commerce.Shopify.API.REST
{
    public class WebHookRestDataProvider : RestDataProviderBase, IParentRestDataProvider<WebHookData>
    {
        protected override string GetListUrl { get; } = "/webhooks.json";
        protected override string GetSingleUrl { get; } = "/webhooks/{id}.json";
		protected override string GetCountUrl { get; } = "/webhooks/count.json";
		protected override string GetSearchUrl => throw new NotImplementedException();

		public WebHookRestDataProvider(IShopifyRestClient restClient) : base()
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

        public WebHookData Create(WebHookData entity)
        {
            var newwebhook = base.Create<WebHookData, WebHookResponse>(entity);
            return newwebhook;
        }

		public WebHookData Update(WebHookData entity, string id)
        {
			var segments = MakeUrlSegments(id);
			return base.Update<WebHookData, WebHookResponse>(entity, segments);
		}

		public bool Delete(string id)
        {
            var segments = MakeUrlSegments(id);
            return base.Delete(segments);
        }

        public IEnumerable<WebHookData> GetAll(IFilter filter = null)
        {
            var allWebHooks = base.GetAll<WebHookData, WebHooksResponse>(filter);
            return allWebHooks;
        }

        public WebHookData GetByID(string id)
        {
            var segments = MakeUrlSegments(id);
            var webhook = GetByID<WebHookData, WebHookResponse>(segments);
            return webhook;
        }

		public IEnumerable<WebHookData> GetCurrentList(out string previousList, out string nextList, IFilter filter = null)
		{
			var webhook = GetCurrentList<WebHookData, WebHooksResponse>(out previousList, out nextList, filter);
			return webhook;
		}
		#endregion
	}
}
