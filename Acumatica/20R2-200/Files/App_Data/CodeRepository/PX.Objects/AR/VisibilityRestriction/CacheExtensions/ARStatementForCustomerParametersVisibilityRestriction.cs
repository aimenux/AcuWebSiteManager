using PX.Data;
using PX.Objects.CS;

namespace PX.Objects.AR
{
	public sealed class ARStatementForCustomerParametersVisibilityRestriction : PXCacheExtension<ARStatementForCustomer.ARStatementForCustomerParameters>
	{
		public static bool IsActive()
		{
			return PXAccess.FeatureInstalled<FeaturesSet.visibilityRestriction>();
		}

		#region CustomerID
		[PXMergeAttributes(Method = MergeMethod.Append)]
		[RestrictCustomerByBranch(branchID: typeof(ARStatementForCustomer.ARStatementForCustomerParameters.branchID), ResetCustomer = true)]
		public int? CustomerID { get; set; }
		#endregion
	}
}