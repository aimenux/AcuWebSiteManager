using System.Collections.Generic;
using System.Linq;
using PX.Data;
using PX.Objects.AP;
using PX.Objects.CN.JointChecks.AP.CacheExtensions;
using PX.Objects.CN.JointChecks.AP.DAC;

namespace PX.Objects.CN.JointChecks.AP.Services.CalculationServices
{
    public class AmountToPayPerLineCalculationService : AmountToPayCalculationService
    {
        public AmountToPayPerLineCalculationService(PXGraph graph, IEnumerable<JointPayeePayment> jointPayeePayments)
            : base(graph, jointPayeePayments)
        {
        }

        public override decimal? GetJointAmountToPay(int? lineNbr)
        {
            return JointPayeePayments.Where(jpp => jpp.BillLineNumber == lineNbr)
                .Sum(jpp => jpp.JointAmountToPay);
        }

        protected override decimal? GetVendorAmountToPay(APAdjust adjustment)
        {
            var adjustmentExtension = PXCache<APAdjust>.GetExtension<ApAdjustExt>(adjustment);
            return adjustmentExtension.AmountToPayPerLine;
        }

        protected override decimal? GetTotalJointAmountToPay(JointPayeePayment jointPayeePayment, int? lineNbr)
        {
            return JointPayeePayments
                .Where(jpp => jpp.JointPayeePaymentId != jointPayeePayment.JointPayeePaymentId &&
                    jpp.BillLineNumber == lineNbr)
                .Sum(jpp => jpp.JointAmountToPay);
        }
    }
}