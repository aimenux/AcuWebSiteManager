using PX.Commerce.Core.Model;
using System.Collections.Generic;

namespace PX.Commerce.BigCommerce.API.REST
{
    class ProductModifierValueRestDataProvider : RestDataProviderV3, ISubChildRestDataProvider<ProductModifierValueData>
    {
        protected override string GetListUrl { get; } = "v3/catalog/products/{product_id}/modifiers/{option_id}/values";
        protected override string GetSingleUrl { get; } = "v3/catalog/products/{product_id}/modifiers/{option_id}/values/{value_id}";
        protected override string GetCountUrl { get; } = "v3/catalog/products/{product_id}/modifiers/{option_id}/values";

        private const string product_id = "product_id";
        private const string option_id = "option_id";
        private const string value_id = "value_id";

        public ProductModifierValueRestDataProvider(IBigCommerceRestClient restClient) : base()
		{
            _restClient = restClient;
		}

        #region ISubChildRestDataProvider  

        public int Count(string productId, string subId)
        {
            var segments = new UrlSegments();
            segments.Add(product_id, productId);
            segments.Add(option_id, subId);

            return GetCount<ProductModifierValueData, ProductsModifierValueList>(segments).Count;
        }

        public List<ProductModifierValueData> Get(string productId, string optionId)
        {
            var segments = new UrlSegments();
            segments.Add(product_id, productId);
            segments.Add(option_id, optionId);

            return base.Get<ProductModifierValueData, ProductsModifierValueList>(null, segments).Data;

        }

        public ProductModifierValueData GetByID(string productId, string optionId, string valueId)
        {
            var segments = new UrlSegments();
            segments.Add(product_id, productId);
            segments.Add(option_id, optionId);
            segments.Add(value_id, valueId);

            return base.GetByID<ProductModifierValueData, ProductsModifierValue>(segments).Data;
        }

        public ProductModifierValueData Create(ProductModifierValueData productsModifierValueData, string productId, string optionId)
        {
            var segments = new UrlSegments();
            segments.Add(product_id, productId);
            segments.Add(option_id, optionId);
            var productsModifierValue = new ProductsModifierValue { Data = productsModifierValueData };

            return base.Create<ProductModifierValueData, ProductsModifierValue>(productsModifierValue, segments).Data;
        }

        public ProductModifierValueData Update(ProductModifierValueData productsModifierValueData, string productId, string optionId, string valueId)
        {
            var segments = new UrlSegments();
            segments.Add(product_id, productId);
            segments.Add(option_id, optionId);
            segments.Add(value_id, valueId);
            var productsModifierValue = new ProductsModifierValue { Data = productsModifierValueData };
            return Update<ProductModifierValueData, ProductsModifierValue>(productsModifierValue, segments).Data;
        }

        public bool Delete(string productId, string optionId, string valueId)
        {
            var segments = new UrlSegments();
            segments.Add(product_id, productId);
            segments.Add(option_id, optionId);
            segments.Add(value_id, valueId);

            return base.Delete(segments);
        }

        #endregion
    }
}
