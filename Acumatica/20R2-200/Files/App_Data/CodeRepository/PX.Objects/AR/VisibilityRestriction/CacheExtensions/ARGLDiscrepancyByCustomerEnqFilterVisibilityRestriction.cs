using PX.Data;
using PX.Objects.AR;
using PX.Objects.CS;

namespace ReconciliationTools
{
	public sealed class
		ARGLDiscrepancyByCustomerEnqFilterVisibilityRestriction : PXCacheExtension<ARGLDiscrepancyByCustomerEnqFilter>
	{
		public static bool IsActive()
		{
			return PXAccess.FeatureInstalled<FeaturesSet.visibilityRestriction>();
		}

		#region CustomerID

		[PXMergeAttributes(Method = MergeMethod.Append)]
		[RestrictCustomerByBranch(typeof(ARGLDiscrepancyByCustomerEnqFilter.branchID), ResetCustomer = true)]
		public int? CustomerID { get; set; }
		#endregion
	}
}
