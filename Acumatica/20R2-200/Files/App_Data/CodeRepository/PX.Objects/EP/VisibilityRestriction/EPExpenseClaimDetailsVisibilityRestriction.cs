using PX.Data;
using PX.Objects.AR;
using PX.Objects.CS;

namespace PX.Objects.EP
{
	public sealed class EPExpenseClaimDetailsVisibilityRestriction : PXCacheExtension<EPExpenseClaimDetails>
	{
		public static bool IsActive()
		{
			return PXAccess.FeatureInstalled<FeaturesSet.visibilityRestriction>();
		}

		#region CustomerID
		[PXMergeAttributes(Method = MergeMethod.Append)]
		[RestrictCustomerByBranch(branchID: typeof(EPExpenseClaimDetails.branchID))]
		public int? CustomerID { get; set; }
		#endregion
	}
}
