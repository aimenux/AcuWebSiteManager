using System;
using System.Collections.Generic;
using PX.Commerce.BigCommerce.API.REST;
using PX.Commerce.Core.Model;

namespace PX.Commerce.BigCommerce.API.REST
{
    public class ProductVariantRestDataProvider : RestDataProviderV3, IChildRestDataProvider<ProductsVariantData>
    {
        protected override string GetListUrl { get; }   = "v3/catalog/products/{parent_id}/variants";
        protected override string GetSingleUrl { get; } = "v3/catalog/products/{parent_id}/variants/{id}";
        protected override string GetCountUrl { get; }  = "v3/catalog/categories/count";
        
        public ProductVariantRestDataProvider(IBigCommerceRestClient restClient) : base()
		{
            _restClient = restClient;
		}
        
        public List<ProductsVariantData> Get(string parentId)
        {
			var segments = MakeParentUrlSegments(parentId);
			return base.Get<ProductsVariantData, ProductVariantList>(null, segments).Data;
        }

        public int Count(string parentId)
        {
            var segments = MakeParentUrlSegments(parentId);
            var result = GetCount<ProductsVariantData, ProductVariantList>(segments);

            return result.Count;
        }

        public ProductsVariantData GetByID(string id, string parentId)
        {
            var segments = MakeUrlSegments(id, parentId);
            return GetByID<ProductsVariantData, ProductsVariant>(segments).Data;
        }

        public ProductsVariantData Create(ProductsVariantData productsVariantData, string parentId)
        {
            var productsVariant = new ProductsVariant { Data = productsVariantData };
            var segments = MakeParentUrlSegments(parentId);
            return Create<ProductsVariantData, ProductsVariant>(productsVariant, segments).Data;
        }
        
        public ProductsVariantData Update(ProductsVariantData productsVariantData, string id, string parentId)
        {
            var segments = MakeUrlSegments(id, parentId);
            var productVariant = new ProductsVariant {Data = productsVariantData};
            return Update<ProductsVariantData, ProductsVariant>(productVariant, segments).Data;
        }
        
        public bool Delete(string id, string parentId)
        {
            var segments = MakeUrlSegments(id, parentId);
            return base.Delete(segments);
        }
    }
	public class ProductVariantBatchRestDataProvider : ProductVariantRestDataProvider
	{
		protected override string GetListUrl { get; } = "v3/catalog/variants";
		public ProductVariantBatchRestDataProvider(IBigCommerceRestClient restClient) : base(restClient)
		{
		}

		public void UpdateAll(List<ProductsVariantData> productDatas, Action<ItemProcessCallback<ProductsVariantData>> callback)
		{
			var product = new ProductVariantList { Data = productDatas };
			UpdateAll<ProductsVariantData, ProductVariantList>(product, new UrlSegments(), callback);
		}
	}
}
