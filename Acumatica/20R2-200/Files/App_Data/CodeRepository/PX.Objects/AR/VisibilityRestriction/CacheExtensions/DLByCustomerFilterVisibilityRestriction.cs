using PX.Data;
using PX.Objects.CS;

namespace PX.Objects.AR
{
	public sealed class DLByCustomerFilterVisibilityRestriction : PXCacheExtension<ARDunningLetterByCustomerEnq.DLByCustomerFilter>
	{
		public static bool IsActive()
		{
			return PXAccess.FeatureInstalled<FeaturesSet.visibilityRestriction>();
		}

		#region BAccountID
		[PXMergeAttributes(Method = MergeMethod.Append)]
		[RestrictCustomerByUserBranches]
		public int? BAccountID { get; set; }
		#endregion
	}
}
