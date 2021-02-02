using PX.Data;
using PX.Objects.AR;
using PX.Objects.CS;

namespace PX.Objects.CT
{
	public class ContractPriceUpdateVisibilityRestriction : PXGraphExtension<ContractPriceUpdate>
	{
		public static bool IsActive()
		{
			return PXAccess.FeatureInstalled<FeaturesSet.visibilityRestriction>();
		}

		public override void Initialize()
		{
			base.Initialize();

			Base.ItemsView.WhereAnd<Where<Customer.cOrgBAccountID, RestrictByUserBranches<Current<AccessInfo.userName>>>>();
		}
	}
}