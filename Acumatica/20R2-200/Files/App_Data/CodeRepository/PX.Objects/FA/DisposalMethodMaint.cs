using PX.Data;

namespace PX.Objects.FA
{
	public class DisposalMethodMaint : PXGraph<DisposalMethodMaint>
	{
        public PXSavePerRow<FADisposalMethod, FADisposalMethod.disposalMethodID> Save;
		public PXCancel<FADisposalMethod> Cancel;

		#region Selects
		public PXSelect<FADisposalMethod> DisposalMethods;
	    public PXSetup<FASetup> fasetup;
	    #endregion

        #region Ctor
        public DisposalMethodMaint()
        {
            FASetup setup = fasetup.Current;
        }
        #endregion
	}
}