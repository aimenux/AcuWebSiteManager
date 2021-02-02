using PX.Commerce.Core.Model;
using System.Collections.Generic;
using System.Linq;

namespace PX.Commerce.BigCommerce.API.REST
{
    public class ProductsOptionRestDataProvider : RestDataProviderV3, IChildRestDataProvider<ProductsOptionData>
    {
        protected override string GetListUrl { get; } = "v3/catalog/products/{parent_id}/options";
        protected override string GetSingleUrl { get; } = "v3/catalog/products/{parent_id}/options/{id}";
        protected override string GetCountUrl { get; } = string.Empty;

        public ProductsOptionRestDataProvider(IBigCommerceRestClient restClient) : base()
		{
            _restClient = restClient;
		}

        #region IChildRestDataProvider

        public ProductsOptionData Create(ProductsOptionData productsOptionData, string parentId)
        {
            var segments = MakeParentUrlSegments(parentId);
            var productsOption = new ProductsOption { Data = productsOptionData };
            return Create<ProductsOptionData, ProductsOption>(productsOption, segments).Data;
        }

        public ProductsOptionData Update(ProductsOptionData productsOptionData, string id, string parentId)
        {
            var segments = MakeUrlSegments(id, parentId);
            var productsOption = new ProductsOption { Data = productsOptionData };

            return Update<ProductsOptionData, ProductsOption>(productsOption, segments).Data;
        }

        public bool Delete(string id, string parentId)
        {
            var segments = MakeUrlSegments(id, parentId);
            return base.Delete(segments);
        }

        public int Count(string parentId)
        {
            var segments = MakeUrlSegments(parentId);
            var result = GetCount<ProductsOptionData, ProductsOptionList>(segments);

            return result.Count;
        }

        public List<ProductsOptionData> Get(string parentId)
        {
            var segments = MakeParentUrlSegments(parentId);
            return base.Get<ProductsOptionData, ProductsOptionList>(null, segments).Data;
        }

        public ProductsOptionData GetByID(string id, string parentId)
        {
            var segments = MakeUrlSegments(id, parentId);
            return GetByID<ProductsOptionData, ProductsOption>(segments).Data;
        }
        #endregion
    }
}
