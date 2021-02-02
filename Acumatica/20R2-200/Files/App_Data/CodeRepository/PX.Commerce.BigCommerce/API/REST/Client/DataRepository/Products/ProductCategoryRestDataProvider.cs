using PX.Commerce.Core.Model;
using System.Collections.Generic;

namespace PX.Commerce.BigCommerce.API.REST
{
    public class ProductCategoryRestDataProvider : RestDataProviderV3, IParentRestDataProvider<ProductCategoryData>
    {
        private const string id_string = "id";

        protected override string GetListUrl { get; }   = "v3/catalog/categories";
        protected override string GetSingleUrl { get; } = "v3/catalog/categories/{id}";
        protected override string GetCountUrl { get; }  = "v3/catalog/categories";

        public ProductCategoryRestDataProvider(IBigCommerceRestClient restClient) : base()
		{
            _restClient = restClient;
		}

        #region  IParentRestDataProvider  

        public ItemCount Count()
        {
            return GetCount<ProductCategoryData, ProductCategoryList>();
        }
        public ItemCount Count(IFilter filter)
        {
            return GetCount<ProductCategoryData, ProductCategoryList>(filter);
        }

        public List<ProductCategoryData> Get(IFilter filter = null)
        {
            return Get<ProductCategoryData, ProductCategoryList>(filter).Data;
        }

        public IEnumerable<ProductCategoryData> GetAll(IFilter filter = null)
        {
            return GetAll<ProductCategoryData, ProductCategoryList>(filter);
        }
        
        public ProductCategoryData GetByID(string id)
        {
            var segments = MakeUrlSegments(id);
            return GetByID<ProductCategoryData, ProductCategory>(segments).Data;
        }

        public bool Delete(ProductCategoryData productCategoryData, int id)
        {
            return Delete(id);
        }

        public bool Delete(int id)
        {
            var segments = MakeUrlSegments(id.ToString());
            return base.Delete(segments);
        }

        public ProductCategoryData Create(ProductCategoryData category)
        {
            var productCategory  = new ProductCategory{Data = category};
            return Create<ProductCategoryData, ProductCategory>(productCategory).Data;
        }

        public ProductCategoryData Update(ProductCategoryData category, int id)
        {
            var segments = MakeUrlSegments(id.ToString());
            return Update<ProductCategoryData, ProductCategory>(category, segments).Data;
        }
        #endregion

    }
}
