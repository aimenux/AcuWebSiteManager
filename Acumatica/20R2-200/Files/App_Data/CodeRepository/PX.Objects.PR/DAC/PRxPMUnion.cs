using PX.Data;
using PX.Objects.CS;
using PX.Objects.PM;

namespace PX.Objects.PR
{
	[PXPrimaryGraph(typeof(PRUnionMaint))]
	public sealed class PRxPMUnion : PXCacheExtension<PMUnion>
	{
		public static bool IsActive()
		{
			return PXAccess.FeatureInstalled<FeaturesSet.payrollModule>();
		}
	}
}
