using PX.Commerce.Core;
using PX.Data;
using System;
using System.Collections.Generic;
using System.Net;

namespace PX.Commerce.BigCommerce.API.REST
{
	public class ProductRestDataProvider : RestDataProviderV3
	{
		private const string id_string = "id";
		protected override string GetListUrl { get; } = "v3/catalog/products";
		//protected override string GetFullListUrl { get; } = "v3/catalog/products?include=variants,images,custom_fields,primary_image,bulk_pricing_rules";
		protected override string GetSingleUrl { get; } = "v3/catalog/products/{id}";
		protected override string GetCountUrl { get; } = "v3/catalog/products";

		public ProductRestDataProvider(IBigCommerceRestClient restClient) : base()
		{
			_restClient = restClient;
		}

		#region IParentRestDataProvider

		public ItemCount Count()
		{
			return GetCount<ProductData, ProductList>();
		}

		public ItemCount Count(IFilter filter)
		{
			return GetCount<ProductData, ProductList>(filter);
		}

		public IEnumerable<ProductData> GetAll(IFilter filter = null)
		{
			return GetAll<ProductData, ProductList>(filter);
		}

		public List<ProductData> Get(IFilter filter = null)
		{
			return base.Get<ProductData, ProductList>(filter).Data;
		}

		public ProductData GetByID(string id,IFilter filter=null)
		{
			var segments = MakeUrlSegments(id);
			var result = GetByID<ProductData, Product>(segments,filter);
			return result.Data;
		}

		public ProductData Create(ProductData productData)
		{
			try
			{
				var product = new Product { Data = productData };
				var result = base.Create<ProductData, Product>(product);
				return result.Data;
			}
			catch (RestException ex)
			{
				if (ex.ResponceStatusCode.Equals(((HttpStatusCode)HttpStatusCode.Conflict).ToString()))
					throw new PXException(BCMessages.MultipleEntitiesWithUniqueField, BCCaptions.SyncDirectionExport,
						BCCaptions.StockItem, String.IsNullOrEmpty(productData.Name)? productData.Sku : productData.Sku + ", " + productData.Name);
				throw ex;
			}
		}

		public bool Delete(ProductData productData, int id)
		{
			return Delete(id);
		}

		public bool Delete(int id)
		{
			var segments = MakeUrlSegments(id.ToString());
			return Delete(segments);
		}
		public ProductData Update(ProductData productData, int id)
		{
			var segments = MakeUrlSegments(id.ToString());
			var result = Update<ProductData, Product>(productData, segments);
			return result.Data;
		}

		public void UpdateAllQty(List<ProductQtyData> productDatas, Action<ItemProcessCallback<ProductQtyData>> callback)
		{
			var product = new ProductQtyList { Data = productDatas };
			UpdateAll<ProductQtyData, ProductQtyList>(product, new UrlSegments(), callback);
		}
		public void UpdateAllRelations(List<RelatedProductsData> productDatas, Action<ItemProcessCallback<RelatedProductsData>> callback)
		{
			var product = new RelatedProductsList { Data = productDatas };
			UpdateAll<RelatedProductsData, RelatedProductsList>(product, new UrlSegments(), callback);
		}
		#endregion
	}
}
