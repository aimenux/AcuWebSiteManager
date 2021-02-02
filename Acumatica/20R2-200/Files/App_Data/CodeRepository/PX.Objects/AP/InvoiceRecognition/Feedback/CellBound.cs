using Newtonsoft.Json;
using System.Collections.Generic;

namespace PX.Objects.AP.InvoiceRecognition.Feedback
{
	internal class CellBound
	{
		[JsonProperty("page")]
		public short Page { get; set; }

		[JsonProperty("table")]
		public short Table { get; set; }

		[JsonProperty("columns")]
		public List<short> Columns { get; set; }

		[JsonProperty("row")]
		public short Row { get; set; }

		[JsonProperty("detailColumn")]
		public string DetailColumn { get; set; }

		[JsonProperty("detailRow")]
		public short DetailRow { get; set; }
	}
}
