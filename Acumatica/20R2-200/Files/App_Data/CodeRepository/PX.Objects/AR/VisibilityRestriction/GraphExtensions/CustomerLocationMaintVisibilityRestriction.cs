using PX.Data;
using PX.Objects.CR;
using PX.Objects.CS;

namespace PX.Objects.AR
{
	public sealed class CustomerLocationMaintVisibilityRestriction : PXGraphExtension<CustomerLocationMaint>
	{
		public static bool IsActive()
		{
			return PXAccess.FeatureInstalled<FeaturesSet.visibilityRestriction>();
		}

		[PXMergeAttributes(Method = MergeMethod.Append)]
		[RestrictCustomerByUserBranches]
		public void Location_BAccountID_CacheAttached(PXCache sender) { }
	}
}