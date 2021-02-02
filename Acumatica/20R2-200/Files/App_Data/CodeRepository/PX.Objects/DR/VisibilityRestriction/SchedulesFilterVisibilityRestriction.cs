using PX.Data;
using PX.Objects.AR;
using PX.Objects.CR;
using PX.Objects.CS;
using System.Collections;

namespace PX.Objects.DR
{
	public sealed class SchedulesFilterVisibilityRestriction : PXCacheExtension<DRDraftScheduleProc.SchedulesFilter>
	{
		public static bool IsActive()
		{
			return PXAccess.FeatureInstalled<FeaturesSet.visibilityRestriction>();
		}

		#region BAccountID
		[PXMergeAttributes(Method = MergeMethod.Append)]
		[RestrictCustomerByBranch(typeof(BAccountR.cOrgBAccountID), branchID: typeof(DRDraftScheduleProc.SchedulesFilter.branchID),
			ResetCustomer = true)]
		public int? BAccountID { get; set; }
		#endregion
	}
}