using PX.Data;
using PX.Objects.AR;
using PX.Objects.CS;

namespace PX.Objects.CT
{
	public class ContractMaintVisibilityRestriction : PXGraphExtension<ContractMaint>
	{
		public static bool IsActive()
		{
			return PXAccess.FeatureInstalled<FeaturesSet.visibilityRestriction>();
		}

		public override void Initialize()
		{
			base.Initialize();

			Base.Contracts.WhereAnd<Where<Customer.cOrgBAccountID, RestrictByUserBranches<Current<AccessInfo.userName>>>>();
		}

		[PXMergeAttributes(Method = MergeMethod.Append)]
		[RestrictCustomerByUserBranches]
		public void _(Events.CacheAttached<Contract.customerID> e)
		{
		}
	}
}