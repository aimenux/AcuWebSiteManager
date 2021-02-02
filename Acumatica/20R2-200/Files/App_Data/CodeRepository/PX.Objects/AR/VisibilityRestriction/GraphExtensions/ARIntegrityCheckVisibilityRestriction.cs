using PX.Data;
using PX.Objects.CS;
using System.Collections;

namespace PX.Objects.AR
{
	public class ARIntegrityCheckVisibilityRestriction : PXGraphExtension<ARIntegrityCheck>
	{
		public static bool IsActive()
		{
			return PXAccess.FeatureInstalled<FeaturesSet.visibilityRestriction>();
		}

		[PXMergeAttributes(Method = MergeMethod.Append)]
		[RestrictCustomerClassByUserBranches]
		protected virtual void ARIntegrityCheckFilter_CustomerClassID_CacheAttached(PXCache sender) { }

		public PXFilteredProcessing<Customer, ARIntegrityCheckFilter,
			Where<Customer.cOrgBAccountID, RestrictByUserBranches<Current<AccessInfo.userName>>,
			And<Match<Current<AccessInfo.userName>>>>> ARCustomerList;

		public PXSelect<Customer,
			Where<Customer.customerClassID, Equal<Current<ARIntegrityCheckFilter.customerClassID>>,
			And<Customer.cOrgBAccountID, RestrictByUserBranches<Current<AccessInfo.userName>>,
			And<Match<Current<AccessInfo.userName>>>
			>>> Customer_ClassID;

		protected IEnumerable arcustomerlist()
		{
			if (Base.Filter.Current != null && Base.Filter.Current.CustomerClassID != null)
			{
				return Customer_ClassID.Select();
			}
			return null;
		}
	}
}