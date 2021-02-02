using PX.Data;
using PX.Objects.AR;
using PX.Objects.CS;

namespace PX.Objects.CT
{
	public class ContractItemMaintVisibilityRestriction : PXGraphExtension<ContractItemMaint>
	{
		public static bool IsActive()
		{
			return PXAccess.FeatureInstalled<FeaturesSet.visibilityRestriction>();
		}

		public override void Initialize()
		{
			base.Initialize();

			Base.CurrentContracts.Join<LeftJoin<Customer, On<Customer.bAccountID, Equal<Contract.customerID>>>>();
			Base.CurrentContracts.WhereAnd<Where<Customer.cOrgBAccountID, RestrictByUserBranches<Current<AccessInfo.userName>>>>();
		}
	}
}