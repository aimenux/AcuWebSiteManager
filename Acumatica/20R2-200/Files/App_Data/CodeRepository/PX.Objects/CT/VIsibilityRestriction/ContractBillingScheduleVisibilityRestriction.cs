using PX.Data;
using PX.Objects.AR;
using PX.Objects.CS;

namespace PX.Objects.CT
{
	public sealed class ContractBillingScheduleVisibilityRestriction : PXCacheExtension<ContractBillingSchedule>
	{
		public static bool IsActive()
		{
			return PXAccess.FeatureInstalled<FeaturesSet.visibilityRestriction>();
		}

		[PXMergeAttributes(Method = MergeMethod.Append)]
		[RestrictCustomerByUserBranches]
		public int? AccountID { get; set; }
	}
}
