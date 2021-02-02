using PX.Data;
using PX.Objects.CS;

namespace PX.Objects.AR
{
	public sealed class ARDunningLetterRecordsParametersVisibilityRestriction : PXCacheExtension<ARDunningLetterProcess.ARDunningLetterRecordsParameters>
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