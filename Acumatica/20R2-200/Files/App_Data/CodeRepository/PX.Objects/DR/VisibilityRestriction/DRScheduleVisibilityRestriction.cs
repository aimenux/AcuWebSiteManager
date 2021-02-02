using PX.Data;
using PX.Objects.AR;
using PX.Objects.CR;
using PX.Objects.CS;

namespace PX.Objects.DR
{
	public sealed class DRScheduleVisibilityRestriction : PXCacheExtension<DRSchedule>
	{
		public static bool IsActive()
		{
			return PXAccess.FeatureInstalled<FeaturesSet.visibilityRestriction>();
		}

		#region BAccountID
		[PXMergeAttributes(Method = MergeMethod.Append)]
		[RestrictCustomerByUserBranches(typeof(BAccountR.cOrgBAccountID))]
		public int? BAccountID { get; set; }
		#endregion
	}
}