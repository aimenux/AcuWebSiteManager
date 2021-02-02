using System.Collections.Generic;
using System.Linq;
using PX.Data;
using PX.Objects.AP;
using PX.Objects.CN.Common.Services.DataProviders;
using PX.Objects.CN.JointChecks.AP.CacheExtensions;
using PX.Objects.CN.JointChecks.AP.DAC;

namespace PX.Objects.CN.JointChecks.AP.Services.CalculationServices
{
    public class AmountToPayCalculationService : CalculationServiceBase
    {
        protected IEnumerable<JointPayeePayment> JointPayeePayments;

        public AmountToPayCalculationService(PXGraph graph, IEnumerable<JointPayeePayment> jointPayeePayments)
            : base(graph)
        {
            JointPayeePayments = jointPayeePayments;
        }

        public virtual decimal? GetTotalAmountToPay(JointPayeePayment jointPayeePayment, APAdjust adjustment)
        {
            var vendorAmountPaid = GetVendorAmountToPay(adjustment);
            var totalJointAmountToPay = GetTotalJointAmountToPay(jointPayeePayment, adjustment.AdjdLineNbr);
            return vendorAmountPaid + jointPayeePayment.JointAmountToPay + totalJointAmountToPay;
        }

        public virtual decimal? GetJointAmountToPay(int? lineNbr)
        {
            return JointPayeePayments.Sum(jpp => jpp.JointAmountToPay);
        }

        protected virtual decimal? GetVendorAmountToPay(APAdjust adjustment)
        {
            var invoice = InvoiceDataProvider.GetInvoice(Graph, adjustment.AdjdDocType, adjustment.AdjdRefNbr);
            var invoiceExtension = PXCache<APInvoice>.GetExtension<APInvoiceJCExt>(invoice);
            return invoiceExtension.AmountToPay;
        }

        protected virtual decimal? GetTotalJointAmountToPay(JointPayeePayment jointPayeePayment, int? lineNbr)
        {
            return JointPayeePayments
                .Where(jpp => jpp.JointPayeePaymentId != jointPayeePayment.JointPayeePaymentId)
                .Sum(jpp => jpp.JointAmountToPay);
        }
    }
}