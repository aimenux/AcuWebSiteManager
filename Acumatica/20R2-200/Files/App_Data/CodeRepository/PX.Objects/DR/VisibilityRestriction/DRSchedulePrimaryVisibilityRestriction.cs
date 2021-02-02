using PX.Data;
using PX.Objects.CR;
using PX.Objects.CS;

namespace PX.Objects.DR
{
	public class DRSchedulePrimaryVisibilityRestriction : PXGraphExtension<DRSchedulePrimary>
	{
		public static bool IsActive()
		{
			return PXAccess.FeatureInstalled<FeaturesSet.visibilityRestriction>();
		}

		public override void Initialize()
		{
			base.Initialize();

			Base.Items.Join<LeftJoin<BAccountR, On<BAccountR.bAccountID, Equal<DRSchedule.bAccountID>>>>();
			Base.Items.WhereAnd<Where<BAccountR.cOrgBAccountID, RestrictByUserBranches<Current<AccessInfo.userName>>>>();
		}
	}
}
