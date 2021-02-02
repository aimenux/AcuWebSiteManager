using PX.Data;
using PX.Objects.AP;
using PX.Objects.CN.Compliance.CL.DAC;
using PX.Objects.CN.JointChecks.AP.Services.LienWaiverValidationServices;
using PX.Objects.CS;

namespace PX.Objects.CN.JointChecks.AP.GraphExtensions
{
    public class ApPrintChecksExt : PXGraphExtension<APPrintChecks>
    {
        public PXSetup<LienWaiverSetup> LienWaiverSetup;
        private LienWaiverProcessingValidationService lienWaiverProcessingValidationService;

        public delegate void AssignNumbersDelegate(APPaymentEntry paymentEntry, APPayment payment,
            ref string nextCheckNumber, bool skipStubs = false);

        public static bool IsActive()
        {
            return PXAccess.FeatureInstalled<FeaturesSet.construction>();
        }

        public override void Initialize()
        {
            lienWaiverProcessingValidationService = new LienWaiverProcessingValidationService(Base);
        }

        [PXOverride]
        public virtual void AssignNumbers(APPaymentEntry paymentEntry, APPayment payment, ref string nextCheckNumber,
            bool skipStubs = false, AssignNumbersDelegate baseHandler = null)
        {
            lienWaiverProcessingValidationService.ValidateLienWaivers(payment,
                LienWaiverSetup.Current?.ShouldStopPayments);
            baseHandler?.Invoke(paymentEntry, payment, ref nextCheckNumber, skipStubs);
        }
    }
}