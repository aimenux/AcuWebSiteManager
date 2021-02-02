using PX.Data;
using PX.Objects.AR;
using PX.Objects.CS;
using PX.Objects.CT;

namespace PX.Objects.CR
{
	public sealed class CaseFilterVisibilityRestriction : PXCacheExtension<CaseFilter>
	{
		public static bool IsActive()
		{
			return PXAccess.FeatureInstalled<FeaturesSet.visibilityRestriction>();
		}

		#region ContractID
		[PXMergeAttributes(Method = MergeMethod.Append)]
		[RestrictCustomerByUserBranches(typeof(BAccountR.cOrgBAccountID))]
		public int? ContractID { get; set; }
		#endregion
	}
}
