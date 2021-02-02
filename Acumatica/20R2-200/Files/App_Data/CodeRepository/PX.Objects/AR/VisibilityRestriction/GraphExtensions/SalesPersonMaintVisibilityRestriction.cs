using PX.Data;
using PX.Objects.CS;

namespace PX.Objects.AR
{
	public class SalesPersonMaintVisibilityRestriction : PXGraphExtension<SalesPersonMaint>
	{
		public static bool IsActive()
		{
			return PXAccess.FeatureInstalled<FeaturesSet.visibilityRestriction>();
		}

		public PXSelectJoin<CustSalesPeople,
			InnerJoin<Customer, On<Customer.bAccountID, Equal<CustSalesPeople.bAccountID>>>,
			Where<CustSalesPeople.salesPersonID, Equal<Current<SalesPerson.salesPersonID>>,
				And<Customer.cOrgBAccountID, RestrictByUserBranches<Current<AccessInfo.userName>>,
				And<Match<Customer, Current<AccessInfo.userName>>>>>> SPCustomers;
	}

	public sealed class CustSalesPeopleVisibilityRestriction : PXCacheExtension<CustSalesPeople>
	{
		public static bool IsActive()
		{
			return PXAccess.FeatureInstalled<FeaturesSet.visibilityRestriction>();
		}

		#region BAccountID
		[PXMergeAttributes(Method = MergeMethod.Append)]
		[RestrictCustomerByUserBranches]
		public int? BAccountID { get; set; }
		#endregion
	}
}
