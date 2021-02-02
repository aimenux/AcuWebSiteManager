using PX.Data;
using PX.Objects.CS;

namespace PX.Objects.AR
{
	public class ARSalesPriceMaintVisibilityRestriction : PXGraphExtension<ARSalesPriceMaint>
	{
		public static bool IsActive()
		{
			return PXAccess.FeatureInstalled<FeaturesSet.visibilityRestriction>();
		}

		public PXSelect<CR.BAccount,
			Where<CR.BAccount.cOrgBAccountID, RestrictByUserBranches<Current<AccessInfo.userName>>,
				And<Where<CR.BAccount.type, Equal<CR.BAccountType.customerType>,
					Or<CR.BAccount.type, Equal<CR.BAccountType.combinedType>>>>>>
			CustomerCode;

		public override void Initialize()
		{
			base.Initialize();

			Base.Records.WhereAnd<Where<CR.BAccount.cOrgBAccountID, RestrictByUserBranches<Current<AccessInfo.userName>>,
									Or<ARSalesPrice.customerID, IsNull>>>();
		}
	}
}