using System.Collections.Generic;
using System.Linq;
using PX.Data;
using PX.Objects.AP;
using PX.Objects.CN.JointChecks.AP.DAC;

namespace PX.Objects.CN.JointChecks.AP.Services.LienWaiverCreationServices
{
    public class LienWaiverAmountCalculationService
    {
        public static decimal? GetAmountPaid(IEnumerable<APAdjust> adjustments)
        {
            return adjustments.Sum(adjust => adjust.CuryAdjgAmt);
        }

        public static decimal? GetJointAmountToPay(IEnumerable<PXResult<JointPayeePayment>> jointPayees)
        {
            return jointPayees.Sum(jpp => jpp.GetItem<JointPayeePayment>().JointAmountToPay);
        }

        public static decimal? GetBillAmount(IEnumerable<APTran> transactions)
        {
            return transactions.Sum(tran => tran.CuryTranAmt);
        }

        public static decimal? GetJointAmountOwed(IEnumerable<PXResult<JointPayeePayment>> jointPayees)
        {
            return jointPayees.Sum(jp => jp.GetItem<JointPayee>().JointAmountOwed);
        }
    }
}