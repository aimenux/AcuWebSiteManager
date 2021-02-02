using Newtonsoft.Json;
using PX.CloudServices.DocumentRecognition;
using System.Collections.Generic;

namespace PX.Objects.AP.InvoiceRecognition.Feedback
{
	internal class VersionedFeedback
	{
		internal static readonly JsonSerializerSettings _settings = new JsonSerializerSettings
		{
			NullValueHandling = NullValueHandling.Ignore
		};

		[JsonProperty("$version")]
		public byte Version { get; set; } = 1;

		[JsonProperty("documents")]
		public List<Document> Documents { get; set; }

		public override string ToString()
		{
			return JsonConvert.SerializeObject(this, _settings);
		}
	}
}
