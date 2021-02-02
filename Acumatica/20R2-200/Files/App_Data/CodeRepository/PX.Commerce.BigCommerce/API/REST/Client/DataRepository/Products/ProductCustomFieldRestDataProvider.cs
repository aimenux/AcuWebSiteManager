using PX.Commerce.Core.Model;
using System.Collections.Generic;

namespace PX.Commerce.BigCommerce.API.REST
{
    public class ProductCustomFieldRestDataProvider : RestDataProviderV3, IChildRestDataProvider<ProductsCustomFieldData>
    {
        private const string id_string = "id";

        protected override string GetListUrl { get; } = "v3/catalog/products/{parent_id}/custom-fields";

        protected override string GetSingleUrl { get; } = "v3/catalog/products/{parent_id}/custom-fields/{id}";

        protected override string GetCountUrl { get; } = string.Empty;

        public ProductCustomFieldRestDataProvider(IBigCommerceRestClient restClient) : base()
		{
            _restClient = restClient;
		}

        #region IChildRestDataProvider

        public ProductsCustomFieldData Create(ProductsCustomFieldData productsCustomFieldData, string parentId)
        {
            var segments = MakeParentUrlSegments(parentId);
            var productsCustomField = new ProductsCustomField { Data = productsCustomFieldData };
            return Create<ProductsCustomFieldData, ProductsCustomField>(productsCustomField, segments).Data;
        }

        public ProductsCustomFieldData Update(ProductsCustomFieldData productsCustomFieldData, string id, string parentId)
        {
            var segments = MakeUrlSegments(id, parentId);
            var productsCustomField = new ProductsCustomField { Data = productsCustomFieldData };

            return Update<ProductsCustomFieldData, ProductsCustomField>(productsCustomField, segments).Data;
        }

        public bool Delete(string id, string parentId)
        {
            var segments = MakeUrlSegments(id, parentId);
            return base.Delete(segments);
        }

        public int Count(string parentId)
        {
            var segments = MakeUrlSegments(parentId);
            var result = GetCount<ProductsCustomFieldData, ProductsCustomFieldList>(segments);

            return result.Count;
        }

        public List<ProductsCustomFieldData> Get(string parentId)
        {
            var segments = MakeParentUrlSegments(parentId);
            return base.Get<ProductsCustomFieldData, ProductsCustomFieldList>(null, segments).Data;
        }

        public ProductsCustomFieldData GetByID(string id, string parentId)
        {
            var segments = MakeUrlSegments(id, parentId);
            return GetByID<ProductsCustomFieldData, ProductsCustomField>(segments).Data;
        }

        #endregion
    }
}
