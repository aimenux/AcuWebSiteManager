using PX.Data;
using PX.Objects.AR;
using PX.Objects.CR;
using PX.Objects.CS;
using static PX.Objects.GL.Reclassification.UI.ReclassifyTransactionsProcess;

namespace PX.Objects.GL
{
	public sealed class LoadOptionsVisibilityRestriction : PXCacheExtension<LoadOptions>
	{
		public static bool IsActive()
		{
			return PXAccess.FeatureInstalled<FeaturesSet.visibilityRestriction>();
		}

		#region ReferenceID
		[PXMergeAttributes(Method = MergeMethod.Append)]
		[RestrictCustomerByBranch(typeof(BAccountR.cOrgBAccountID), branchID: typeof(LoadOptions.branchID),
			ResetCustomer = true)]
		public int? ReferenceID { get; set; }
		#endregion
	}
}
