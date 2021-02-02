using PX.Data;
using PX.Objects.CS;

namespace PX.Objects.AR
{
	public sealed class PaymentFilterVisibilityRestriction : PXCacheExtension<ARPaymentsAutoProcessing.PaymentFilter>
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