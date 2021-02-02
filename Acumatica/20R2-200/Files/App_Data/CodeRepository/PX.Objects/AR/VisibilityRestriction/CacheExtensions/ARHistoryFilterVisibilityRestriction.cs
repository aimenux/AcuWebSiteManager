using PX.Data;
using PX.Objects.CS;

namespace PX.Objects.AR
	{
		public sealed class ARHistoryFilterVisibilityRestriction : PXCacheExtension<ARCustomerBalanceEnq.ARHistoryFilter>
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
	}
}