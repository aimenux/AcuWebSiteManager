using PX.Data;

namespace PX.Objects.FA
{
	public class SetupMaint : PXGraph<SetupMaint>
	{
		public PXSelect<FASetup> FASetupRecord;
		public PXSave<FASetup> Save;
		public PXCancel<FASetup> Cancel;
	}
}