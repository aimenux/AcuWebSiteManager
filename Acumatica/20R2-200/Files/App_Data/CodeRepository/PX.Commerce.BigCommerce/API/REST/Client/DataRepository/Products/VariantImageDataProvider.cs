using System.Collections.Generic;

namespace PX.Commerce.BigCommerce.API.REST
{
	public class VariantImageDataProvider : RestDataProviderV3
	{
		protected override string GetListUrl { get; } = "v3/catalog/products/{parent_id}/variants/{id}/image";
		protected override string GetSingleUrl { get; } = string.Empty;
		protected override string GetCountUrl { get; } = string.Empty;

		public VariantImageDataProvider(IBigCommerceRestClient restClient) : base()
		{
			_restClient = restClient;
		}

		public ProductsImageData Create(ProductsImageData productsImageData, string parentId, string id)
		{
			var segments = MakeUrlSegments(id,parentId);
			var productsImage = new ProductsImage { Data = productsImageData };
			return Create<ProductsImageData, ProductsImage>(productsImage, segments).Data;
		}
		
	}
}