using Newtonsoft.Json;

namespace PX.Commerce.BigCommerce.API.REST
{
    public class OptionSet
    {
        /// <summary>
        /// The ID of the option set.
        /// 
        /// int
        /// </summary>
        [JsonProperty("id")]
        public virtual int Id { get; set; }

        /// <summary>
        /// The name of the option set.
        /// 
        /// int
        /// </summary>
        [JsonProperty("name")]
        public virtual string Name { get; set; }

        /// <summary>
        /// A link to the options which make up the option set. See the Option Set Options page for details.
        /// 
        /// resource
        /// </summary>
        [JsonProperty("options")]
        public virtual Resource ResourceOptions { get; set; }
    }
}