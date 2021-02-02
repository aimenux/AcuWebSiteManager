using PX.Data;
using PX.Objects.CR;
using PX.Objects.CS;

namespace PX.Objects.AR
{
	public sealed class LocationMaintVisibilityRestriction : PXGraphExtension<LocationMaint>
	{
		public static bool IsActive()
		{
			return PXAccess.FeatureInstalled<FeaturesSet.visibilityRestriction>();
		}

		public PXSetup<BAccountR, Where<BAccountR.bAccountID, Equal<Current<Location.bAccountID>>>> currentBAccount;
		
	}
}
