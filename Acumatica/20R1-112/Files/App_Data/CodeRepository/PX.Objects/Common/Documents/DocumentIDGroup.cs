using System.Collections.Generic;
using System.Linq;

using PX.Objects.Common.Extensions;

namespace PX.Objects.Common.Documents
{
	public class DocumentIDGroup
	{
		public string Module { get; set; }

		public string DocumentType
		{
			get { return DocumentTypes.SingleOrDefault(); }
			set { DocumentTypes = value.SingleToList(); }
		}

		public List<string> DocumentTypes { get; set; }

		public List<string> RefNbrs { get; set; }

		public DocumentIDGroup()
		{
			DocumentTypes = new List<string>();
			RefNbrs = new List<string>();
		}
	}
}
