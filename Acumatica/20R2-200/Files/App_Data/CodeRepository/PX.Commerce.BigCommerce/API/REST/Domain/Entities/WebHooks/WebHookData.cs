using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using PX.Commerce.Core;

namespace PX.Commerce.BigCommerce.API.REST
{
    [JsonObject(Description = BCCaptions.WebHook)]
    public class WebHookData
	{
        [JsonProperty("id")]
        public virtual int? Id { get; set; }

        [JsonProperty("client_id")]
        public virtual string ClientID { get; set; }

        [JsonProperty("store_hash")]
        public virtual string StoreHash { get; set; }

        [JsonProperty("scope")]
        public virtual string Scope { get; set; }

        [JsonProperty("destination")]
        public virtual string Destination { get; set; }

        [JsonProperty("is_active")]
        public virtual bool IsActive { get; set; }

        [JsonProperty("phone")]
        public virtual string Phone { get; set; }

        [JsonIgnore]
        public virtual DateTime? DateCreated { get; set; }

        [JsonProperty("created_at")]
        public virtual string DateCreatedUT
        {
            get => DateCreated.TDToString();
            set => DateCreated = value.ToDate();
        }

        [JsonIgnore]
        public virtual DateTime? DateModified { get; set; }

        [JsonProperty("updated_at")]
        public virtual string DateModifiedUT
        {
            get => DateModified.TDToString();
            set => DateModified = value.ToDate();
        }

		[JsonProperty("headers")]
		public virtual Dictionary<String, String> Headers { get; set; }

		//Conditional Serialization
		public bool ShouldSerializeId()
        {
            return false;
        }
		public bool ShouldSerializeDateCreatedUT()
		{
			return false;
		}
		public bool ShouldSerializeDateModifiedUT()
		{
			return false;
		}
	}
}
