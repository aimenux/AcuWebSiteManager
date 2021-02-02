using PX.Data;
using PX.Objects.CS;

namespace PX.Objects.AR
{
	public class CCTransactionsHistoryEnqVisibilityRestriction : PXGraphExtension<CCTransactionsHistoryEnq>
	{
		public static bool IsActive()
		{
			return PXAccess.FeatureInstalled<FeaturesSet.visibilityRestriction>();
		}

		public override void Initialize()
		{
			base.Initialize();

			Base.CCTransView.WhereAnd<Where<Customer.cOrgBAccountID, RestrictByUserBranches<Current<AccessInfo.userName>>>>();
		}
	}
}