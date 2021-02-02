using PX.Data;
using PX.Objects.AR;
using PX.Objects.CR;
using PX.Objects.CS;
using static PX.Objects.CT.UsageMaint;

namespace PX.Objects.CT
{
	public sealed class UsageFilterVisibilityRestriction : PXCacheExtension<UsageFilter>
	{
		public static bool IsActive()
		{
			return PXAccess.FeatureInstalled<FeaturesSet.visibilityRestriction>();
		}

		#region ContractID
		[PXMergeAttributes(Method = MergeMethod.Merge)]
		[PXSelector(typeof(Search2<Contract.contractID,
					LeftJoin<BAccountR, On<Contract.customerID, Equal<BAccountR.bAccountID>>>,
					Where<Contract.baseType, Equal<CTPRType.contract>,
						And<Contract.status, NotEqual<Contract.status.draft>,
						And<Contract.status, NotEqual<Contract.status.inApproval>>>>>),
			SubstituteKey = typeof(Contract.contractCD), DescriptionField = typeof(Contract.description))]
		[RestrictCustomerByUserBranches(typeof(BAccountR.cOrgBAccountID))]
		public int? ContractID { get; set; }
		#endregion
	}
}
