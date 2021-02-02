using System.Collections.Generic;
using System.Linq;
using PX.Objects.AP;
using PX.Objects.CN.Compliance.CL.DAC;

namespace PX.Objects.CN.JointChecks.AP.Models
{
    public class LienWaiverGenerationContext
    {
        public IEnumerable<ComplianceDocument> LienWaiversForPayment
        {
            get;
            set;
        }

        public IEnumerable<APTran> Transactions
        {
            get;
            set;
        }

        public bool PaymentHasGeneratedLienWaivers =>
            LienWaiversForPayment.Any(cd => cd.IsCreatedAutomatically == true);

        public bool PaymentHasManuallyCreatedLienWaivers =>
            LienWaiversForPayment.Any(cd => cd.IsCreatedAutomatically != true);
    }
}