using Newtonsoft.Json;
using PX.Commerce.Core;
using PX.Commerce.Core.Model;
using RestSharp.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;

namespace PX.Commerce.Shopify.API.REST
{
	public class ProductRestDataProvider : RestDataProviderBase, IParentRestDataProvider<ProductData>
	{
		protected override string GetListUrl { get; } = "products.json";
		protected override string GetSingleUrl { get; } = "products/{id}.json";
		protected override string GetCountUrl { get; } = "products/count.json";
		protected override string GetSearchUrl => throw new NotImplementedException();
		private string GetMetafieldsUrl { get; } = "products/{id}/metafields.json";

		public ProductRestDataProvider(IShopifyRestClient restClient) : base()
		{
			ShopifyRestClient = restClient;
		}

		public ProductData Create(ProductData entity)
		{
			return base.Create<ProductData, ProductResponse>(entity);
		}

		public ProductData Update(ProductData entity) => Update(entity, entity.Id.ToString());
		public ProductData Update(ProductData entity, string productId)
		{
			var segments = MakeUrlSegments(productId);
			return base.Update<ProductData, ProductResponse>(entity, segments);
		}

		public bool Delete(ProductData entity, string productId) => Delete(productId);

		public bool Delete(string productId)
		{
			var segments = MakeUrlSegments(productId);
			return Delete(segments);
		}

		public IEnumerable<ProductData> GetCurrentList(out string previousList, out string nextList, IFilter filter = null)
		{
			return GetCurrentList<ProductData, ProductsResponse>(out previousList, out nextList, filter);
		}

		public IEnumerable<ProductData> GetAll(IFilter filter = null)
		{
			return GetAll<ProductData, ProductsResponse>(filter);
		}

		public ProductData GetByID(string productId)
		{
			var segments = MakeUrlSegments(productId);
			var entity = base.GetByID<ProductData, ProductResponse>(segments);
            if(entity != null)
			    entity.Metafields = GetMetafieldsByProductId(productId);
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

		public List<MetafieldData> GetMetafieldsByProductId(string productId)
		{
			var request = BuildRequest(GetMetafieldsUrl, nameof(GetMetafieldsByProductId), MakeUrlSegments(productId), null);
            return ShopifyRestClient.GetAll<MetafieldData, MetafieldsResponse>(request).ToList();
		}
	}
}
