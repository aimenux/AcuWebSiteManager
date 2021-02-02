using PX.Data;
using PX.Objects.AR;
using PX.Objects.CS;

namespace PX.Objects.CT
{
	public class RenewContractsVisibilityRestriction : PXGraphExtension<RenewContracts>
	{
		public static bool IsActive()
		{
			return PXAccess.FeatureInstalled<FeaturesSet.visibilityRestriction>();
		}

		public override void Initialize()
		{
			base.Initialize();

			Base.ItemsInitialCommand = BqlCommand.AppendJoin<LeftJoin<Customer, On<Customer.bAccountID, Equal<RenewContracts.ContractsList.customerID>>>>(Base.ItemsInitialCommand);
			Base.ItemsInitialCommand = Base.ItemsInitialCommand.WhereAnd<Where<Customer.cOrgBAccountID, RestrictByUserBranches<Current<AccessInfo.userName>>>>();
		}
	}

	public sealed class RenewalContractFilterVisibilityRestriction : PXCacheExtension<RenewContracts.RenewalContractFilter>
	{
		public static bool IsActive()
		{
			return PXAccess.FeatureInstalled<FeaturesSet.visibilityRestriction>();
		}

		[PXMergeAttributes(Method = MergeMethod.Append)]
		[RestrictCustomerClassByUserBranches]
		public string CustomerClassID { get; set; }
	}
}
