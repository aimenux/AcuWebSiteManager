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
		[GL.Attributes.BranchTree(DisplayName = "Process with Branch", Required = false, PersistingCheck = PXPersistingCheck.Nothing)]
		public virtual Int32? BranchID { get; set; }
		#endregion
	}
}
