using PX.Data;
using PX.Objects.CS;

namespace PX.Objects.AR
{
	public class ARDiscountSequenceMaintVisibilityRestriction : PXGraphExtension<ARDiscountSequenceMaint>
	{
		public static bool IsActive()
		{
			return PXAccess.FeatureInstalled<FeaturesSet.visibilityRestriction>();
		}

		public override void Initialize()
		{
			base.Initialize();

			Base.Customers.WhereAnd<Where<Customer.cOrgBAccountID, RestrictByUserBranches<Current<AccessInfo.userName>>>>();
		}
	}

	public sealed class DiscountCustomerVisibilityRestriction : PXCacheExtension<DiscountCustomer>
	{
		public static bool IsActive()
		{
			return PXAccess.FeatureInstalled<FeaturesSet.visibilityRestriction>();
		}

		#region CustomerID
		[PXMergeAttributes(Method = MergeMethod.Append)]
		[RestrictCustomerByUserBranches]
		public int? CustomerID { get; set; }
		#endregion
	}
}