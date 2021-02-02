using PX.Data;
using PX.Objects.CS;

namespace PX.Objects.AR.VisibilityRestriction.CacheExtensions
{
	public sealed class ARDocumentFilterVisibilityRestriction : PXCacheExtension<ARDocumentEnq.ARDocumentFilter>
	{
		public static bool IsActive()
		{
			return PXAccess.FeatureInstalled<FeaturesSet.visibilityRestriction>();
		}

		#region CustomerID
		[PXMergeAttributes(Method = MergeMethod.Append)]
		[RestrictCustomerByOrganization(orgBAccountID: typeof(ARDocumentEnq.ARDocumentFilter.orgBAccountID))]
		public int? CustomerID { get; set; }
		#endregion
	}
}