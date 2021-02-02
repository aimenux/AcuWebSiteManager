using Newtonsoft.Json;

namespace PX.Commerce.BigCommerce.API.REST
{
    public class Brand
    {
        /// <summary>
        /// The ID of the brand.
        /// 
        /// Store ID
        /// </summary>
        [JsonProperty("id")]
        public virtual int Id { get; set; }

        /// <summary>
        /// The name of the brand. A brand's name must be unique.
        /// 
        /// string(255)
        /// </summary>
        [JsonProperty("name")]
        public virtual string Name { get; set; }

        /// <summary>
        /// The title shown in the browser when viewing the brand in 
        /// the store front.
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
        /// The URI to the image used for this brand.
        /// 
        /// string(255)
        /// </summary>
        [JsonProperty("image_file")]
        public virtual string ImageFile { get; set; }

        /// <summary>
        /// A comma separated list of keywords that can be used to locate 
        /// this brand when searching the store.
        /// 
        /// string(255)
        /// </summary>
        [JsonProperty("search_keywords")]
        public virtual string SearchKeywords { get; set; }
    }
}
