using PX.Data;
using PX.Objects.CS;

namespace PX.Objects.DR
{
	public class DRSetupMaint: PXGraph<DRSetupMaint>
	{
		public PXSelect<DRSetup> DRSetupRecord;
		public PXSave<DRSetup> Save;
		public PXCancel<DRSetup> Cancel;

		protected virtual void DRSetup_RowSelected(PXCache sender, PXRowSelectedEventArgs e)  // exclude in extension
		{
			DRSetup row = e.Row as DRSetup;
			if (row != null)
			{
				bool isMulticurrency = PXAccess.FeatureInstalled<FeaturesSet.multicurrency>();  // exclude in extension
				bool isASC606 = PXAccess.FeatureInstalled<FeaturesSet.aSC606>();  // exclude in extension
				PXUIFieldAttribute.SetVisible<DRSetup.useFairValuePricesInBaseCurrency>(sender, row, isMulticurrency && isASC606);  // exclude in extension
			}
		}
	}
}
