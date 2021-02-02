using PX.Commerce.Core.Model;
using System.Collections.Generic;

namespace PX.Commerce.BigCommerce.API.REST
{
    public class ProductBrandRestDataProvider : RestDataProviderV3, IParentRestDataProvider<ProductBrandData>
    {
        private const string id_string = "id";
        protected override string GetListUrl   { get; } = "v3/catalog/brands";
        protected override string GetSingleUrl { get; } = "v3/catalog/brands/{id}";
        protected override string GetCountUrl  { get; } = "v3/catalog/brands";

        public ProductBrandRestDataProvider(IBigCommerceRestClient restClient) : base()
		{
            _restClient = restClient;
		}

        #region IParentRestDataProviderIDataRestClient  

        public ItemCount Count()
        {
            return GetCount<ProductBrandData, ProductBrandDataList>(); 
        }
        public ItemCount Count(IFilter filter)
        {            
            return GetCount<ProductBrandData, ProductBrandDataList>(filter); 
        }

        public List<ProductBrandData> Get(IFilter filter = null)
        {
            return Get<ProductBrandData, ProductBrandDataList>().Data;
        }
        public IEnumerable<ProductBrandData> GetAll(IFilter filter = null)
        {
            return base.GetAll<ProductBrandData, ProductBrandDataList>(filter);
        }
        
        public ProductBrandData GetByID(string id)
        {
            var segments = MakeUrlSegments(id);
            return base.GetByID<ProductBrandData, ProductBrand>(segments).Data;
        }

        public bool Delete(ProductBrandData productBrandData, int id)
        {
            return Delete(id);
        }

        public bool Delete( int id)
        {
            var segments = MakeUrlSegments(id.ToString());
            return base.Delete(segments);
        }

        public ProductBrandData Create(ProductBrandData productBrandData)
        {
            var productBrand = new ProductBrand{Data = productBrandData};
            return Create<ProductBrandData, ProductBrand>(productBrand).Data;
        }

        public ProductBrandData Update(ProductBrandData productBrandData, int id)
        {
            var productBrand = new ProductBrand{Data = productBrandData};
            var segments = MakeUrlSegments(productBrandData.Id.ToString());
            return Update<ProductBrandData, ProductBrand>(productBrand, segments).Data;
        }
		#endregion
	}
}
