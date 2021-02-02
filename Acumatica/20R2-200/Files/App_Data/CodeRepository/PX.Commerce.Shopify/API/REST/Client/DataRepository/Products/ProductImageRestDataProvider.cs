using PX.Commerce.Core.Model;
using System;
using System.Collections.Generic;
using System.Linq;

namespace PX.Commerce.Shopify.API.REST
{
	public class ProductImageRestDataProvider : RestDataProviderBase, IChildRestDataProvider<ProductImageData>
	{
		protected override string GetListUrl { get; } = "products/{parent_id}/images.json";
		protected override string GetSingleUrl { get; } = "products/{parent_id}/images/{id}.json";
		protected override string GetCountUrl { get; } = "products/{parent_id}/images/count.json";
		protected override string GetSearchUrl => throw new NotImplementedException();
		private string GetMetafieldsUrl { get; } = "metafields.json?metafield[owner_id]={0}&metafield[owner_resource]=product_image";

		public ProductImageRestDataProvider(IShopifyRestClient restClient) : base()
		{
			ShopifyRestClient = restClient;
		}

		public ProductImageData Create(ProductImageData entity, string productId)
		{
			var segments = MakeParentUrlSegments(productId);
			return base.Create<ProductImageData, ProductImageResponse>(entity, segments);
		}

		public ProductImageData Update(ProductImageData entity, string productId, string imageId)
		{
			var segments = MakeUrlSegments(imageId, productId);
			return Update<ProductImageData, ProductImageResponse>(entity, segments);
		}

		public bool Delete(string productId, string imageId)
		{
			var segments = MakeUrlSegments(imageId, productId);
			return Delete(segments);
		}

		public int Count(string productId)
		{
			var segments = MakeParentUrlSegments(productId);
			return GetCount(segments).Count;
		}

		public IEnumerable<ProductImageData> GetCurrentList(string productId, out string previousList, out string nextList, IFilter filter = null)
		{
			var segments = MakeParentUrlSegments(productId);
			var imageList = GetCurrentList<ProductImageData, ProductImagesResponse>(out previousList, out nextList, filter, segments);
			if (imageList != null && imageList.Count() > 0)
			{
                foreach (var oneItem in imageList) 
                {
                    oneItem.Metafields = GetMetafieldsByImageId(oneItem.Id.ToString());
                }
			}
			return imageList;
		}

		public IEnumerable<ProductImageData> GetAll(string productId, IFilter filter = null)
		{
			var segments = MakeParentUrlSegments(productId);
			var imageList = GetAll<ProductImageData, ProductImagesResponse>(filter, segments);
			if (imageList != null && imageList.Count() > 0)
			{
                foreach (var oneItem in imageList)
                {
                    oneItem.Metafields = GetMetafieldsByImageId(oneItem.Id.ToString());
                    yield return oneItem;
                }
			}
			yield break;
		}

		public ProductImageData GetByID(string productId, string imageId)
		{
			var segments = MakeUrlSegments(imageId, productId);
			var image = GetByID<ProductImageData, ProductImageResponse>(segments);
			if (image != null) image.Metafields = GetMetafieldsByImageId(imageId);
			return image;
		}

		public IEnumerable<ProductImageData> GetAllWithoutParent(IFilter filter = null)
		{
			throw new NotImplementedException();
		}

		public List<MetafieldData> GetMetafieldsByImageId(string imageId)
		{
			var request = BuildRequest(string.Format(GetMetafieldsUrl, imageId), nameof(GetMetafieldsByImageId), null, null);
			return ShopifyRestClient.GetAll<MetafieldData, MetafieldsResponse>(request).ToList();
		}
	}
}
