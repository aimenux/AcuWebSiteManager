using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace PX.Commerce.BigCommerce.API.REST
{
    [JsonObject(Description = "Product -> Product Option -> Product Option Value")]
    public class ProductOptionValueData
    {
        public ProductOptionValueData()
        {
            ValueData = new List<object>();
        }

        [JsonProperty("id")]
        public int? Id { get; set; }
        
        [JsonProperty("label")]                                    
        public string Label { get; set; }                          
                                                                   
        [JsonProperty("sort_order")]                               
        public int? SortOrder { get; set; }                         
                                                                   
        //[JsonProperty("value_data")]
        //[JsonConverter(typeof(SingleValueArrayConverter<object>))]
        public List<object> ValueData { get; set; }

        [JsonProperty("is_default")]
        public bool IsDefault { get; set; }

		[JsonIgnore]
		public Guid? LocalID { get; set; }
	}
}
