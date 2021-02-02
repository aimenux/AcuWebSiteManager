using PX.Data;
using PX.Objects.CS;

namespace PX.Objects.AR
{
	public class ARChargeInvoiceVisibilityRestriction : PXGraphExtension<ARChargeInvoices>
	{
		public static bool IsActive()
		{
			return PXAccess.FeatureInstalled<FeaturesSet.visibilityRestriction>();
		}

		public override void Initialize()
		{
			base.Initialize();

			Base.ARDocumentListView.WhereAnd<Where<Customer.cOrgBAccountID, RestrictByBranch<Current<AccessInfo.branchID>>>>();
		}
	}
}