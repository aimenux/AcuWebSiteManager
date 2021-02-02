using Newtonsoft.Json;
using PX.Commerce.Core;
using PX.Commerce.Core.Model;
using RestSharp.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;

namespace PX.Commerce.Shopify.API.REST
{
	public class CustomerRestDataProvider : RestDataProviderBase, IParentRestDataProvider<CustomerData>
	{
		protected override string GetListUrl { get; } = "customers.json";
		protected override string GetSingleUrl { get; } = "customers/{id}.json";
		protected override string GetCountUrl { get; } = "customers/count.json";
		protected override string GetSearchUrl { get; } = "customers/search.json";
		private string GetAccountActivationUrl { get; } = "customers/{id}/account_activation_url.json";
		private string GetSendInviteUrl { get; } = "customers/{id}/send_invite.json";
        private string GetMetafieldsUrl { get; } = "customers/{id}/metafields.json";

        public CustomerRestDataProvider(IShopifyRestClient restClient) : base()
		{
			ShopifyRestClient = restClient;
		}

		public CustomerData Create(CustomerData entity)
		{
			return base.Create<CustomerData, CustomerResponse>(entity);
		}

		public CustomerData Update(CustomerData entity) => Update(entity, entity.Id.ToString());
		public CustomerData Update(CustomerData entity, string customerId)
		{
			var segments = MakeUrlSegments(customerId);
			return base.Update<CustomerData, CustomerResponse>(entity, segments);
		}

		public bool Delete(CustomerData entity, string customerId) => Delete(customerId);

		public bool Delete(string customerId)
		{
			var segments = MakeUrlSegments(customerId);
			return Delete(segments);
		}

		public IEnumerable<CustomerData> GetCurrentList(out string previousList, out string nextList, IFilter filter = null)
		{
			return GetCurrentList<CustomerData, CustomersResponse>(out previousList, out nextList, filter);
		}

		public IEnumerable<CustomerData> GetAll(IFilter filter = null)
		{
			return GetAll<CustomerData, CustomersResponse>(filter);
		}

        public CustomerData GetByID(string id) => GetByID(id, true);

        public CustomerData GetByID(string customerId, bool includedMetafields = true)
		{
			var segments = MakeUrlSegments(customerId);
            var entity = base.GetByID<CustomerData, CustomerResponse>(segments);
            if(entity != null && includedMetafields == true)
            {
                entity.Metafields = GetMetafieldsById(customerId);
            }
            return entity;
		}

		public ItemCount Count()
		{
			return base.GetCount();
		}

		public ItemCount Count(IFilter filter)
		{
			return base.GetCount(filter);
		}

		public IEnumerable<CustomerData> GetByQuery(string fieldName, string value)
		{
			var url = GetSearchUrl;
			var property = typeof(CustomerData).GetProperty(fieldName, System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.IgnoreCase);
			if (property != null)
			{
				var attr = property.GetAttribute<JsonPropertyAttribute>();
				if (attr == null) throw new KeyNotFoundException();
				String key = attr.PropertyName;
				url += $"?query={attr.PropertyName}:{value}";
			}
			else
				throw new KeyNotFoundException();
			var request = BuildRequest(url, nameof(this.GetByQuery), null, null);
			return ShopifyRestClient.GetAll<CustomerData, CustomersResponse>(request);
		}

		public bool ActivateAccount(string customerId)
		{
			var request = BuildRequest(GetAccountActivationUrl, nameof(this.ActivateAccount), MakeUrlSegments(customerId), null);
			return ShopifyRestClient.Post(request);
		}

        public List<MetafieldData> GetMetafieldsById(string id)
        {
            var request = BuildRequest(GetMetafieldsUrl, nameof(GetMetafieldsById), MakeUrlSegments(id), null);
            return ShopifyRestClient.GetAll<MetafieldData, MetafieldsResponse>(request).ToList();
        }
    }
}
