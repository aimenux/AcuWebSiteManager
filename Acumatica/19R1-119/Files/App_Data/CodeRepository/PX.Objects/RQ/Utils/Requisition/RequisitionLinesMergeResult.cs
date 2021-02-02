using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PX.Objects.RQ
{
	/// <summary>
	/// The DTO with the results from the merge of the requistion lines.
	/// </summary>
	public class RequisitionLinesMergeResult
	{
		public bool Merged { get; } 

		public RQRequisitionLine ResultLine { get; }

		public RequisitionLinesMergeResult(bool merged, RQRequisitionLine resultLine)
		{
			Merged = merged;
			ResultLine = resultLine;
		}
	}
}
