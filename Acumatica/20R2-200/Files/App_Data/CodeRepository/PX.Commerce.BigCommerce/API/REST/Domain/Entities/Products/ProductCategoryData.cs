
using Newtonsoft.Json;
using PX.Commerce.Core;
using System.ComponentModel;

namespace PX.Commerce.BigCommerce.API.REST
{
    [JsonObject(Description = "ProductCategory")]
	[Description(BigCommerceCaptions.ProductCategoryData)]
    public class ProductCategoryData : BCAPIEntity
	{

        [JsonProperty("id")]
        public int? Id { get; set; }

        [JsonProperty("parent_id")]
		[Description(BigCommerceCaptions.ParentCategory)]
		public int? ParentId { get; set; }
        [JsonProperty("name")]
		[Description(BigCommerceCaptions.CategoryName)]
		[ValidateRequired()]
        public string Name { get; set; }

        [JsonProperty("description")]
		[Description(BigCommerceCaptions.CategoryDescription)]
		public string Description { get; set; }

        [JsonProperty("views")]
        public int? Views { get; set; }

        [JsonProperty("sort_order")]
		[Description(BigCommerceCaptions.SortOrder)]
		public int? SortOrder { get; set; }

        [JsonProperty("page_title")]
		[Description(BigCommerceCaptions.PageTitle)]
		public string PageTitle { get; set; }

        [JsonProperty("meta_keywords")]
		//[Description(BigCommerceCaptions.Meta Keywords")]
		public string[] MetaKeywords { get; set; }

        [JsonProperty("meta_description")]
		[Description(BigCommerceCaptions.MetaDescription)]
		public string MetaDescription { get; set; }

        [JsonProperty("layout_file")]
		[Description(BigCommerceCaptions.LayoutFile)]
		public string LayoutFile { get; set; }

        [JsonProperty("image_url")]
		[Description(BigCommerceCaptions.ImageUrl)]
		public string ImageUrl { get; set; }

        [JsonProperty("is_visible")]
        public bool? IsVisible { get; set; }

        [JsonProperty("search_keywords")]
		[Description(BigCommerceCaptions.SearchKeywords)]
		public string SearchKeywords { get; set; }

        [JsonProperty("default_product_sort")]
		[Description(BigCommerceCaptions.DefaultProductSort)]
		public string DefaultProductSort { get; set; }

        [JsonProperty("custom_url")]
		[Description(BigCommerceCaptions.CustomUrl)]
		public ProductCustomUrl CustomUrl { get; set; }

        public override string ToString()
        {
            return $"{Name}, ID: {Id}, ParentID: {ParentId} ";
        }
	}
}
