using PX.Data;
using PX.Objects.AR;
using PX.Objects.CS;

namespace PX.Objects.EP
{
	public sealed class EPExpenseClaimVisibilityRestriction : PXCacheExtension<EPExpenseClaim>
	{
		public static bool IsActive()
		{
			return PXAccess.FeatureInstalled<FeaturesSet.visibilityRestriction>();
		}

		#region CustomerID
		[PXMergeAttributes(Method = MergeMethod.Append)]
		[RestrictCustomerByBranch(branchID: typeof(EPExpenseClaim.branchID))]
		public int? CustomerID { get; set; }
		#endregion
	}
}