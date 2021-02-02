using PX.Commerce.Core.Model;
using System.Collections.Generic;

namespace PX.Commerce.BigCommerce.API.REST
{
    public class ProductsModifierRestDataProvider : RestDataProviderV3, IChildRestDataProvider<ProductsModifierData>
    {
        protected override string GetListUrl { get; } = "v3/catalog/products/{parent_id}/modifiers";
        protected override string GetSingleUrl { get; } = "v3/catalog/products/{parent_id}/modifiers/{id}";
        protected override string GetCountUrl { get; } = string.Empty;

        public ProductsModifierRestDataProvider(IBigCommerceRestClient restClient) : base()
		{
            _restClient = restClient;
		}

        public int Count(string parentId)
        {
            var segments = MakeUrlSegments(parentId);
            var result = GetCount<ProductsModifierData, ProductsModifierList>(segments);

            return result.Count;
        }

        public ProductsModifierData Create(ProductsModifierData productsModifierData, string parentId)
        {
            var segments = MakeParentUrlSegments(parentId);
            var productsModifier = new ProductsModifier { Data = productsModifierData };
            return Create<ProductsModifierData, ProductsModifier>(productsModifier, segments).Data;
        }

        public bool Delete(string id, string parentId)
        {
            var segments = MakeUrlSegments(id, parentId);
            return base.Delete(segments);
        }

        public List<ProductsModifierData> Get(string parentId)
        {
            var segments = MakeParentUrlSegments(parentId);
            return base.Get<ProductsModifierData, ProductsModifierList>(null, segments).Data;
        }

        public ProductsModifierData GetByID(string id, string parentId)
        {
            var segments = MakeUrlSegments(id, parentId);
            return GetByID<ProductsModifierData, ProductsModifier>(segments).Data;
        }

        public ProductsModifierData Update(ProductsModifierData productsModifierData, string id, string parentId)
        {
            var segments = MakeUrlSegments(id, parentId);
            var productsOption = new ProductsModifier { Data = productsModifierData };

            return Update<ProductsModifierData, ProductsModifier>(productsOption, segments).Data;
        }
    }
}
