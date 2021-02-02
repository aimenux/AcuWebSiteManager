using PX.Commerce.Core.Model;
using System;
using System.Collections.Generic;

namespace PX.Commerce.BigCommerce.API.REST
{
	class ProductVideoDataProvider : RestDataProviderV3, IChildRestDataProvider<ProductsVideo>
	{
		protected override string GetListUrl { get; } = "v3/catalog/products/{parent_id}/videos";

		protected override string GetSingleUrl { get; } = "v3/catalog/products/{parent_id}/videos/{id}";

		protected override string GetCountUrl { get; } = string.Empty;

		public ProductVideoDataProvider(IBigCommerceRestClient restClient) : base()
		{
			_restClient = restClient;
		}

		public ProductsVideo Create(ProductsVideo productsVideo, string parentId)
		{
			var segments = MakeParentUrlSegments(parentId);
			var productVideo = new ProductVideoData { Data = productsVideo };
			return Create<ProductsVideo, ProductVideoData>(productVideo, segments).Data;
		}

		//public ProductsVideo Create(ProductsVideo productsVideo, string parentId)
		//{
		//	var segments = MakeParentUrlSegments(parentId);
		//	return Create(productsVideo, segments);
		//}

		#region Not Implemented
		public ProductsVideo Update(ProductsVideo productsVideo, string id, string parentId)
		{
			throw new NotImplementedException();
		}

		public int Count(string parentId)
		{
			throw new NotImplementedException();
		}


		public bool Delete(string id, string parentId)
		{
			throw new NotImplementedException();
		}

		public List<ProductsVideo> Get(string parentId)
		{
			throw new NotImplementedException();
		}

		public ProductsVideo GetByID(string id, string parentId)
		{
			throw new NotImplementedException();
		}

		#endregion
	}
}
