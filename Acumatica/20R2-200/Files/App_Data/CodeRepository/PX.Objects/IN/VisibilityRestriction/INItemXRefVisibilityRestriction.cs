using PX.Data;
using PX.Objects.AR;
using PX.Objects.CR;
using PX.Objects.CS;

namespace PX.Objects.IN
{
	public sealed class INItemXRefVisibilityRestriction : PXCacheExtension<INItemXRef>
	{
		public static bool IsActive()
		{
			return PXAccess.FeatureInstalled<FeaturesSet.visibilityRestriction>();
		}

		#region BAccountID
		[PXMergeAttributes(Method = MergeMethod.Append)]
		[RestrictCustomerByUserBranches(typeof(BAccountR.cOrgBAccountID), SuppressVerify = true)]
		public int? BAccountID { get; set; }
		#endregion
	}
}