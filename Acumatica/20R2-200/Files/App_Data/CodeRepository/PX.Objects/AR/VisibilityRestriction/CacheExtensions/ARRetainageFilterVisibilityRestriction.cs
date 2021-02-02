using PX.Data;
using PX.Objects.CS;

namespace PX.Objects.AR
{
	public sealed class ARRetainageFilterVisibilityRestriction : PXCacheExtension<ARRetainageFilter>
	{
		public static bool IsActive()
		{
			return PXAccess.FeatureInstalled<FeaturesSet.visibilityRestriction>();
		}

		#region CustomerID
		[PXMergeAttributes(Method = MergeMethod.Append)]
		[RestrictCustomerByBranch(branchID: typeof(ARRetainageFilter.branchID), ResetCustomer = true)]
		public int? CustomerID { get; set; }
		#endregion
	}
}