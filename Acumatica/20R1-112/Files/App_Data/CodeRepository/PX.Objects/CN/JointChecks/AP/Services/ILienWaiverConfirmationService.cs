using PX.Data;
using PX.Objects.AP;

namespace PX.Objects.CN.JointChecks.AP.Services
{
    public interface ILienWaiverConfirmationService
    {
        bool IsCreationOfAdditionalLienWaiversConfirmed(PXSelectBase<APPayment> view);
    }
}