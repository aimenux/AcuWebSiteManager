using PX.Data;
using PX.Objects.CS;

namespace PX.Objects.AR
{
	public sealed class ARWriteOffFilterVisibilityRestriction : PXCacheExtension<ARWriteOffFilter>
	{
		public static bool IsActive()
		{
			return PXAccess.FeatureInstalled<FeaturesSet.visibilityRestriction>();
		}

		#region CustomerID
		[PXMergeAttributes(Method = MergeMethod.Append)]
		[RestrictCustomerByBranch(branchID: typeof(ARWriteOffFilter.branchID), ResetCustomer = true)]
		public int? CustomerID { get; set; }
		#endregion
	}
}
