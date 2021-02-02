using PX.Data;

namespace PX.Objects.AM
{
    public class BOMSetup : PXGraph<BOMSetup>
    {
        public PXSave<AMBSetup> Save;
        public PXCancel<AMBSetup> Cancel;

        public PXSelect<AMBSetup> AMBSetupRecord;

        public PXSelect<AMECRSetupApproval> ECRSetupApproval;
        public PXSelect<AMECOSetupApproval> ECOSetupApproval;

        protected virtual void AMBSetup_ECRRequestApproval_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
        {
            // Same Logic used on SOSetupMaint (19R1) - but using FieldUpdated
            PXCache cache = this.Caches[typeof(AMECRSetupApproval)];
            PXResultset<AMECRSetupApproval> setups = PXSelect<AMECRSetupApproval>.Select(sender.Graph, null);
            foreach (AMECRSetupApproval setup in setups)
            {
                setup.IsActive = ((AMBSetup)e.Row)?.ECRRequestApproval ?? false;
                cache.Update(setup);
            }
        }

        protected virtual void AMBSetup_ECORequestApproval_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
        {
            // Same Logic used on SOSetupMaint (19R1) - but using FieldUpdated
            PXCache cache = this.Caches[typeof(AMECOSetupApproval)];
            PXResultset<AMECOSetupApproval> setups = PXSelect<AMECOSetupApproval>.Select(sender.Graph, null);
            foreach (AMECOSetupApproval setup in setups)
            {
                setup.IsActive = ((AMBSetup)e.Row)?.ECORequestApproval ?? false;
                cache.Update(setup);
            }
        }
    }
}