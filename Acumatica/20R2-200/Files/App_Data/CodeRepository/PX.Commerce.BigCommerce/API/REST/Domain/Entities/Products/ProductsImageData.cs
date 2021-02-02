using Newtonsoft.Json;
using PX.Commerce.BigCommerce.API.WebDAV;
using PX.Commerce.Core;

namespace PX.Commerce.BigCommerce.API.REST
{
	[JsonObject(Description = "Product-> ProductImage")]
	public class ProductsImageData : BCAPIEntity, IWebDAVEntity
	{
		[JsonProperty("is_thumbnail")]
		public bool IsThumbnail { get; set; }

		[JsonProperty("sort_order")]
		public int SortOrder { get; set; }

		[JsonProperty("description")]
		public string Description { get; set; }

		[JsonProperty("id")]
		public int Id { get; set; }

		[JsonProperty("image_file")]
		public string ImageFile { get; set; }

		[JsonProperty("url_zoom")]
		public string UrlZoom { get; set; }

		[JsonProperty("url_standard")]
		public string UrlStandard { get; set; }

		[JsonProperty("url_thumbnail")]
		public string UrlThumbnail { get; set; }

		[JsonProperty("url_tiny")]
		public string UrlTiny { get; set; }

		[JsonProperty("date_modified")]
		public string DateModified { get; set; }

		[JsonProperty("image_url")]

		public string ImageUrl { get; set; }

	}
}