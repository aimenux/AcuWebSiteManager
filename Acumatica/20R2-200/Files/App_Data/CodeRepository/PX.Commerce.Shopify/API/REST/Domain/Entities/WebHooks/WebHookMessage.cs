using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using PX.Commerce.Core;

namespace PX.Commerce.Shopify.API.REST
{
    [JsonObject(Description = BCCaptions.WebHookMessage)]
    public class WebHookMessage
    {
		[JsonProperty("id")]
		public virtual long? Id { get; set; }

		/// <summary>
		/// Date and time when the data was created. The API returns this value in ISO 8601 format.
		/// </summary>
		[JsonProperty("created_at")]
		[ShouldNotSerialize]
		public DateTime? DateCreatedAt { get; set; }

		/// <summary>
		/// Date and time when the data was updated. The API returns this value in ISO 8601 format.
		/// </summary>
		[JsonProperty("updated_at")]
		[ShouldNotSerialize]
		public DateTime? DateModifiedAt { get; set; }
	}
}
