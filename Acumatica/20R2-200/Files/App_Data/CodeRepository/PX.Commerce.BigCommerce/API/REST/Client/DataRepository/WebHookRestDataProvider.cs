using PX.Commerce.Core.Model;
using System;
using System.Collections.Generic;

namespace PX.Commerce.BigCommerce.API.REST
{
    public class WebHookRestDataProvider : RestDataProviderV2, IParentRestDataProvider<WebHookData>
    {
        protected override string GetListUrl { get; } = "v2/hooks";
        protected override string GetSingleUrl { get; } = "v2/hooks/{id}";
        protected override string GetCountUrl { get; } = "v2/hooks/count";

        public WebHookRestDataProvider(IBigCommerceRestClient restClient) : base()
		{
            _restClient = restClient;
		}

        #region IParentDataRestClient

        public ItemCount Count()
        {
			throw new NotImplementedException();
        }

        public ItemCount Count(IFilter filter)
        {
			throw new NotImplementedException();
		}

        public WebHookData Create(WebHookData webhook)
        {
            var newwebhook = base.Create(webhook);
            return newwebhook;
        }

        public WebHookData Update(WebHookData customer, int id)
        {
			throw new NotImplementedException();
		}
    
        public bool Delete(WebHookData order, int id)
        {
            return Delete(id);
        }
      
        public bool Delete(int id)
        {
            var segments = MakeUrlSegments(id.ToString());
            return Delete(segments);
        }

        public List<WebHookData> Get(IFilter filter = null)
        {
            var webhook = Get<WebHookData>(filter);
            return webhook;
        }

        public IEnumerable<WebHookData> GetAll(IFilter filter = null)
        {
			return base.GetAll<WebHookData>(filter);
        }

        public WebHookData GetByID(string webhookId)
        {
            var segments = MakeUrlSegments(webhookId);
            var customer = GetByID<WebHookData>(segments);
            return customer;
        }
        #endregion
    }
}
