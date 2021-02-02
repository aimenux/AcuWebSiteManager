using System;

namespace PX.Objects.Common.Abstractions
{
	public class DocumentKey : Tuple<string, string>
	{
		public DocumentKey(IDocumentKey record)
			: base(record.DocType, record.RefNbr)
		{ }

		public DocumentKey(string docType, string refNbr)
			: base(docType, refNbr)
		{ }

		public string DocType => Item1;
		public string RefNbr => Item2;
	}
}
