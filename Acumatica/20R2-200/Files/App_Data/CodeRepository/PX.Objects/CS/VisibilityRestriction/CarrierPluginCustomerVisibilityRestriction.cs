using PX.Data;
using PX.Objects.AR;

namespace PX.Objects.CS
{
	public sealed class CarrierPluginCustomerVisibilityRestriction : PXCacheExtension<CarrierPluginCustomer>
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