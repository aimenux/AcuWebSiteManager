using PX.Data;
using PX.Objects.AR;
using PX.Objects.CS;

namespace PX.Objects.PM
{
	public class ProjectEntryVisibilityRestriction : PXGraphExtension<ProjectEntry>
	{
		public static bool IsActive()
		{
			return PXAccess.FeatureInstalled<FeaturesSet.visibilityRestriction>();
		}

		[PXRemoveBaseAttribute(typeof(RestrictCustomerByUserBranches))]
		public void _(Events.CacheAttached<PMProject.contractCD> e)
		{
		}
	}
}