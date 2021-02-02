using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PX.Commerce.BigCommerce.API.REST
{
	public class ProductBulkPricingRestDataProvider : RestDataProviderV3
	{
		protected override string GetListUrl { get; } = "v3/catalog/products/{parent_id}/bulk-pricing-rules";

		protected override string GetSingleUrl { get; } = "v3/catalog/products/{parent_id}/bulk-pricing-rules/{id}";

		protected override string GetCountUrl => throw new NotImplementedException();
		public ProductBulkPricingRestDataProvider(IBigCommerceRestClient client)
		{
			_restClient = client;
		}
		public ProductsBulkPricingRules Create(ProductsBulkPricingRules pricingRules, string parentId)
		{
			var segments = MakeParentUrlSegments(parentId);
			return Create<ProductsBulkPricingRules, BulkPricing>(pricingRules, segments).Data;
		}
		public IEnumerable<ProductsBulkPricingRules> GetAll( string parentId)
		{
			var segments = MakeParentUrlSegments(parentId);
			return GetAll<ProductsBulkPricingRules, BulkPricingList>(urlSegments: segments);
		}

		public bool Delete(string id,string parentId)
		{
			var segments = MakeUrlSegments(id, parentId);
			return Delete(urlSegments: segments);
		}

		public ProductsBulkPricingRules Update(ProductsBulkPricingRules productData, string id)
		{
			var segments = MakeUrlSegments(id, productData.ParentId.ToString());
			var result = Update<ProductsBulkPricingRules, BulkPricing>(productData, segments);
			return result.Data;
		}

	}

	public class ProductBatchBulkRestDataProvider : ProductBulkPricingRestDataProvider
	{
		protected override string GetListUrl { get; } = "v3/catalog/products?include=bulk_pricing_rules";
		public ProductBatchBulkRestDataProvider(IBigCommerceRestClient restClient) : base(restClient)
		{
		}

		public void UpdateAll(List<BulkPricingWithSalesPrice> productDatas, Action<ItemProcessCallback<BulkPricingWithSalesPrice>> callback)
		{
			var product = new BulkPricingListWithSalesPrice { Data = productDatas };
			UpdateAll<BulkPricingWithSalesPrice, BulkPricingListWithSalesPrice>(product, new UrlSegments(), callback);
		}
	}
}
