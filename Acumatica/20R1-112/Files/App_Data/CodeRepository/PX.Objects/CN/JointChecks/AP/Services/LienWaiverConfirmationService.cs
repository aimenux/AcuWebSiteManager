using PX.Data;
using PX.Objects.AP;
using PX.Objects.CN.Compliance.Descriptor;

namespace PX.Objects.CN.JointChecks.AP.Services
{
    public class LienWaiverConfirmationService : ILienWaiverConfirmationService
    {
        public bool IsCreationOfAdditionalLienWaiversConfirmed(PXSelectBase<APPayment> view)
        {
            return view.Ask(Common.Descriptor.SharedMessages.Warning,
                ComplianceMessages.LienWaiver.ManuallyCreatedLienWaiverIsReferredToApCheck,
                MessageButtons.YesNo).IsPositive();
        }
    }
}