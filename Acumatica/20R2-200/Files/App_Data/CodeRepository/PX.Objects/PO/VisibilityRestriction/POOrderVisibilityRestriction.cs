using PX.Data;
using PX.Objects.AR;
using PX.Objects.CR;
using PX.Objects.CS;

namespace PX.Objects.PO
{
	public sealed class POOrderVisibilityRestriction : PXCacheExtension<POOrder>
	{
		public static bool IsActive()
		{
			return PXAccess.FeatureInstalled<FeaturesSet.visibilityRestriction>();
		}

		#region BAccountID
		[PXMergeAttributes(Method = MergeMethod.Append)]
		[RestrictCustomerByUserBranches(typeof(BAccount2.cOrgBAccountID))]
		public int? ShipToBAccountID { get; set; }
		#endregion
	}
}