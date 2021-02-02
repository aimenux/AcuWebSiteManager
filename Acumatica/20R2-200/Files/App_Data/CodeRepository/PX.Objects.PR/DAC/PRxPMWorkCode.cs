using PX.Data;
using PX.Objects.CS;
using PX.Objects.PM;

namespace PX.Objects.PR
{
	[PXPrimaryGraph(typeof(PRWorkCodeMaint))]
	public sealed class PRxPMWorkCode : PXCacheExtension<PMWorkCode>
	{
		public static bool IsActive()
		{
			return PXAccess.FeatureInstalled<FeaturesSet.payrollModule>();
		}
	}
}
