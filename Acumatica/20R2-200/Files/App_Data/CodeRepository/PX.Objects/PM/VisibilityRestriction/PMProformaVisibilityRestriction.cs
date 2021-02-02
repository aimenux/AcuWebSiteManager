using PX.Data;
using PX.Objects.AR;
using PX.Objects.CS;
using System;

namespace PX.Objects.PM
{
	public sealed class PMProformaVisibilityRestriction : PXCacheExtension<PMProforma>
	{
		public static bool IsActive()
		{
			return PXAccess.FeatureInstalled<FeaturesSet.visibilityRestriction>();
		}

		#region CustomerID
		[PXMergeAttributes(Method = MergeMethod.Append)]
		[RestrictCustomerByBranch(branchID: typeof(PMProforma.branchID))]
		public int? CustomerID { get; set; }
		#endregion
	}
}
