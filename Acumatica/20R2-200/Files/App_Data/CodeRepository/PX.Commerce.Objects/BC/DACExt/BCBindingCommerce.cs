using System;
using PX.Data;
using PX.Objects.AR;
using PX.Objects.IN;
using PX.Objects.SO;
using PX.Objects.TX;
using PX.Objects.GL;
using PX.Objects.CS;
using System.Collections.Generic;
using PX.Commerce.Core;
using PX.Objects.CA;
using PX.Objects.CR;

namespace PX.Commerce.Objects
{
	[Serializable]
	[PXNonInstantiatedExtension]
	public class BCBindingCommerce : PXCacheExtension<BCBinding>
	{
		#region Keys
		public static class FK
		{
			public class BindingsBranch : Branch.PK.ForeignKeyOf<BCBinding>.By<branchID> { }
		}
		#endregion

		#region BranchID
		[PXMergeAttributes(Method = MergeMethod.Replace)]
		[Branch(typeof(Branch.branchID))]
		[PXDefault(typeof(AccessInfo.branchID))]
		public virtual int? BranchID { get; set; }
		public abstract class branchID : IBqlField { }
		#endregion
	}
}