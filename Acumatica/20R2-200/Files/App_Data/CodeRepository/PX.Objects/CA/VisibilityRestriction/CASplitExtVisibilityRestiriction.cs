using PX.Data;
using PX.Objects.CS;
using PX.Objects.AR;
using PX.Objects.CR;

namespace PX.Objects.CA
{
	public sealed class CASplitExtVisibilityRestiriction : PXCacheExtension<CASplitExt>
	{
		public static bool IsActive()
		{
			return PXAccess.FeatureInstalled<FeaturesSet.visibilityRestriction>();
		}

		#region ReferenceID
		[PXMergeAttributes(Method = MergeMethod.Append)]
		[RestrictCustomerByBranch(typeof(BAccountR.cOrgBAccountID), branchID: typeof(AccessInfo.branchID),
			ResetCustomer = true)]
		public int? ReferenceID { get; set; }
		#endregion
	}
}