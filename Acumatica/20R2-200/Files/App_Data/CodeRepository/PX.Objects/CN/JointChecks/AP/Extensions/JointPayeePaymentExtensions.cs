using PX.Objects.CN.JointChecks.AP.DAC;

namespace PX.Objects.CN.JointChecks.AP.Extensions
{
    public static class JointPayeePaymentExtensions
    {
        public static bool IsRelatedToSameInvoice(this JointPayeePayment jointPayeePayment,
            JointPayeePayment jointPayeePaymentToCompare)
        {
            return jointPayeePayment.InvoiceRefNbr == jointPayeePaymentToCompare.InvoiceRefNbr &&
                jointPayeePayment.InvoiceDocType == jointPayeePaymentToCompare.InvoiceDocType;
        }

        public static bool IsPaymentByline(this JointPayeePayment jointPayeePayment)
        {
            return jointPayeePayment.BillLineNumber != null && jointPayeePayment.BillLineNumber != decimal.Zero;
        }
    }
}
