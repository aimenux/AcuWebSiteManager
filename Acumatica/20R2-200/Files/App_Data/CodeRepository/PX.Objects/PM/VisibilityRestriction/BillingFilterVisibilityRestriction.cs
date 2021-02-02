using PX.Data;
using PX.Objects.AR;
using PX.Objects.CS;
using static PX.Objects.PM.BillingProcess;

namespace PX.Objects.PM
{
	public sealed class BillingFilterVisibilityRestriction : PXCacheExtension<BillingFilter>
	{
		public static bool IsActive()
		{
			return PXAccess.FeatureInstalled<FeaturesSet.visibilityRestriction>();
		}

		#region CustomerClassID
		[PXMergeAttributes(Method = MergeMethod.Append)]
		[RestrictCustomerClassByUserBranches]
		public string CustomerClassID { get; set; }
		#endregion

		#region CustomerID
		[PXMergeAttributes(Method = MergeMethod.Append)]
		[RestrictCustomerByUserBranches]
		public int? CustomerID { get; set; }
		#endregion
	}
}
