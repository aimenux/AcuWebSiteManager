using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using PX.CloudServices.DocumentRecognition;
using System;

namespace PX.Objects.AP.InvoiceRecognition.Feedback
{
	internal class TypedField : Field
	{
		[JsonConverter(typeof(StringEnumConverter))]
		[JsonProperty("type")]
		public FieldTypes? Type { get; set; }
	}
}
