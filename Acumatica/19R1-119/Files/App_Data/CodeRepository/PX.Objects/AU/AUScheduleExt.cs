using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PX.Data;
using PX.SM;

namespace PX.Objects.AU
{
	public class AUScheduleExt : PXCacheExtension<AUSchedule>
	{
		#region BranchID
		public abstract class branchID : PX.Data.BQL.BqlInt.Field<branchID> { }
		[PXDBInt]
		[PXUIField(DisplayName = "Process with Branch", Visible = false)]
		[PXDimensionSelector("BIZACCT", typeof(Search<Branch.branchID, Where<MatchWithBranch<Branch.branchID>>>), typeof(Branch.branchCD), typeof(Branch.branchCD), typeof(Branch.roleName))]
		public virtual Int32? BranchID { get; set; }
		#endregion
	}
}
