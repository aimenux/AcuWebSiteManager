using PX.Data;
using PX.Data.BQL;
using PX.Data.BQL.Fluent;
using PX.Data.SQLTree;
using PX.Objects.CA;
using PX.Objects.CM;
using PX.Objects.CS;
using PX.Objects.GL;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PX.Objects.GL
{
	/// <summary>
	/// Contains single record representing Current Branch of the company.
	/// Can be used in Generic Inquiries for filtering.
	/// </summary>
	[PXProjection(typeof(Select<GL.Branch,
		Where<Branch.branchID, Equal<CurrentValue<AccessInfo.branchID>>>>), Persistent = false)]
	[PXCacheName(CS.Messages.CurrentBranch)]
	public partial class CurrentBranch : GL.Branch
	{

	}
}