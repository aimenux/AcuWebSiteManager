using System.Collections.Generic;
using Newtonsoft.Json;

namespace PX.Commerce.BigCommerce.API.REST
{
    public class Category
    {
        /// <summary>
        /// The ID of the category.
        /// </summary>
        [JsonProperty("id")]
        public virtual int Id { get; set; }

        /// <summary>
        /// The ID of the parent category the category belongs to.
        /// </summary>
        [JsonProperty("parent_id")]
        public virtual int ParentId { get; set; }

        /// <summary>
        /// The name of the category.
        /// 
        /// string(50)
        /// </summary>
        [JsonProperty("name")]
        public virtual string Name { get; set; }

        /// <summary>
        /// A description of the category.
        /// 
        /// text
        /// </summary>
        [JsonProperty("description")]
        public virtual string Description { get; set; }

        /// <summary>
        /// The sort order of the category.
        /// </summary>
        [JsonProperty("sort_order")]
        public virtual int SortOrder { get; set; }

        /// <summary>
        /// The title shown in the browser when viewing the category in the store front.
        /// 
        /// string(250)
        /// </summary>
        [JsonProperty("page_title")]
        public virtual string PageTitle { get; set; }

        /// <summary>
        /// A comma separated list of meta keywords to include in the html.
        /// 
        /// text
        /// </summary>
        [JsonProperty("meta_keywords")]
        public virtual string MetaKeywords { get; set; }

        /// <summary>
        /// A meta description to include in the html.
        /// 
        /// text
        /// </summary>
        [JsonProperty("meta_description")]
        public virtual string MetaDescription { get; set; }

        /// <summary>
        /// The layout template file used to render this category.
        /// 
        /// string(50)
        /// </summary>
        [JsonProperty("layout_file")]
        public virtual string LayoutFile { get; set; }

        /// <summary>
        /// A list of all the ancestor category ID's.
        /// </summary>
        [JsonProperty("parent_category_list")]
        public virtual List<int> ParentCategoryList { get; set; }

        /// <summary>
        /// The URI to the image used for this category.
        /// 
        /// string(255)
        /// </summary>
        [JsonProperty("image_file")]
        public virtual string ImageFile { get; set; }

        /// <summary>
        /// Indicates whether the category is visible in the store.
        /// </summary>
        [JsonProperty("is_visible")]
        public virtual bool IsVisible { get; set; }

        /// <summary>
        /// A comma separated list of keywords that can be used to locate this 
        /// category when searching the store.
        /// 
        /// string(255)
        /// </summary>
        [JsonProperty("search_keywords")]
        public virtual string SearchKeywords { get; set; }
    }
}
