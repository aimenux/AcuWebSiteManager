using PX.Data;
using PX.Objects.AR;
using PX.Objects.CR;
using PX.Objects.CS;

namespace PX.Objects.CA
{
	public sealed class CABankTranVisibilityRestriction : PXCacheExtension<CABankTran>
	{
		public static bool IsActive()
		{
			return PXAccess.FeatureInstalled<FeaturesSet.visibilityRestriction>();
		}

		#region PayeeBAccountID
		[PXMergeAttributes(Method = MergeMethod.Append)]
		[RestrictCustomerByUserBranches(typeof(BAccountR.cOrgBAccountID))]
		public int? PayeeBAccountID { get; set; }
		#endregion
	}
}