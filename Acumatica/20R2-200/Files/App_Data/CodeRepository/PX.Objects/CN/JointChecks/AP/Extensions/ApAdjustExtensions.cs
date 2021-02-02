using PX.Objects.AP;
using PX.Objects.CN.JointChecks.AP.DAC;

namespace PX.Objects.CN.JointChecks.AP.Extensions
{
    public static class ApAdjustExtensions
    {
        public static bool IsRelatedJointPayeePayment(this APAdjust adjustment, JointPayeePayment jointPayeePayment)
        {
            return adjustment.AdjgRefNbr == jointPayeePayment.PaymentRefNbr &&
                adjustment.AdjgDocType == jointPayeePayment.PaymentDocType &&
                adjustment.AdjdRefNbr == jointPayeePayment.InvoiceRefNbr &&
                adjustment.AdjdDocType == jointPayeePayment.InvoiceDocType &&
                adjustment.AdjNbr == jointPayeePayment.AdjustmentNumber &&
                adjustment.AdjdLineNbr == jointPayeePayment.BillLineNumber;
        }
    }
}
