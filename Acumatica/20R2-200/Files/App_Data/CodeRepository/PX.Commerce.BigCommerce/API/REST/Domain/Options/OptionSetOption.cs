using Newtonsoft.Json;

namespace PX.Commerce.BigCommerce.API.REST
{
    public class OptionSetOption
    {
        /// <summary>
        /// The unique ID of the Option that has been applied to the Option Set.
        /// 
        /// int
        /// </summary>
        [JsonProperty("id")]
        public virtual int Id { get; set; }

        /// <summary>
        /// The ID of that Option that this option set option refers to. 
        /// 
        /// int
        /// </summary>
        [JsonProperty("option_id")]
        public virtual int OptionId { get; set; }

        /// <summary>
        /// The ID of the Option Set that the Option was applied to. 
        /// 
        /// int
        /// </summary>
        [JsonProperty("option_set_id")]
        public virtual int OptionSetId { get; set; }

        /// <summary>
        /// The name to use for the option for this option set. This must be unique for the Option Set. 
        /// 
        /// string(255) 
        /// </summary>
        [JsonProperty("display_name")]
        public virtual string DisplayName { get; set; }

        /// <summary>
        /// The order in which the Option is displayed on the product's page.  
        /// 
        /// int
        /// </summary>
        [JsonProperty("sort_order")]
        public virtual int SortOrder { get; set; }

        /// <summary>
        /// Specifies whether the customer is required to enter or pick a value for this option before they can add the product to their cart. 
        /// 
        /// boolean 
        /// </summary>
        [JsonProperty("is_required")]
        public virtual bool IsRequired { get; set; }

        /// <summary>
        /// Resource link to the option this option set option is derived from. 
        /// 
        /// resource
        /// </summary>
        [JsonProperty("option")]
        public virtual Resource ResourceOption { get; set; }
    }
}
