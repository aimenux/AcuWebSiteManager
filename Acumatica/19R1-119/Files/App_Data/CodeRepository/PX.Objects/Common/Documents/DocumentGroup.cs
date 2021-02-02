using System.Collections.Generic;

namespace PX.Objects.Common.Documents
{
	public class DocumentGroup<TDocument>
	{
		public string Module { get; set; }

		public string DocumentType { get; set; }

		public IDictionary<string, TDocument> DocumentsByRefNbr { get; set; }
	}
}