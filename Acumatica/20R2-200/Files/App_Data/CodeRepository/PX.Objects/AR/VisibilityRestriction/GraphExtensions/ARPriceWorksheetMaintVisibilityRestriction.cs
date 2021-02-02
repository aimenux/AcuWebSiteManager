using PX.Data;
using PX.Objects.CR;
using PX.Objects.CS;

namespace PX.Objects.AR
{
	public class ARPriceWorksheetMaintVisibilityRestriction : PXGraphExtension<ARPriceWorksheetMaint>
	{
		public static bool IsActive()
		{
			return PXAccess.FeatureInstalled<FeaturesSet.visibilityRestriction>();
		}

		public PXSelect<BAccount,
			Where<BAccount.cOrgBAccountID, RestrictByUserBranches<Current<AccessInfo.userName>>,
				And<Where<BAccount.type, Equal<BAccountType.customerType>,
					Or<BAccount.type, Equal<BAccountType.combinedType>>>>>>
			CustomerCode;

		public override void Initialize()
		{
			base.Initialize();

			Base.Details.WhereAnd<Where<BAccount.cOrgBAccountID, RestrictByUserBranches<Current<AccessInfo.userName>>,
									Or<ARPriceWorksheetDetail.customerID, IsNull>>>();
		}
	}
}
