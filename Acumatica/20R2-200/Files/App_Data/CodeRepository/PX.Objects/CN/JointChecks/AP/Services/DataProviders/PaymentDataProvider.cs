using System.Collections.Generic;
using System.Linq;
using PX.Data;
using PX.Data.BQL;
using PX.Data.BQL.Fluent;
using PX.Objects.AP;

namespace PX.Objects.CN.JointChecks.AP.Services.DataProviders
{
    public class PaymentDataProvider
    {
        public static IEnumerable<APPayment> GetGeneratedPayments(PXGraph graph, string invoiceReferenceNumber)
        {
            var query = new PXSelectJoin<APPayment,
                InnerJoin<APAdjust,
                    On<APAdjust.adjgRefNbr, Equal<APPayment.refNbr>,
                        And<APAdjust.adjgDocType, Equal<APPayment.docType>>>,
                    InnerJoin<APInvoice,
                        On<APAdjust.adjdRefNbr, Equal<APInvoice.refNbr>,
                            And<APAdjust.adjdDocType, Equal<APInvoice.docType>>>>>,
                Where<APInvoice.refNbr, Equal<Required<APInvoice.refNbr>>>>(graph);
            return query.SelectMain(invoiceReferenceNumber);
        }

        public static APPayment GetPayment(PXGraph graph, string referenceNumber, string documentType)
        {
            return SelectFrom<APPayment>
                .Where<APPayment.refNbr.IsEqual<P.AsString>
                    .And<APPayment.docType.IsEqual<P.AsString>>>.View.Select(graph, referenceNumber, documentType);
        }

        public static bool DoesCheckContainPaymentForJointVendors(PXGraph graph, APRegister payment)
        {
            return GetJointAmountsToPaySum(graph, payment) > 0;
        }

        public static bool DoesCheckContainPaymentForPrimaryVendor(PXGraph graph, APRegister payment)
        {
            return payment.CuryOrigDocAmt > GetJointAmountsToPaySum(graph, payment);
        }

        private static decimal? GetJointAmountsToPaySum(PXGraph graph, APRegister payment)
        {
            var jointPayeePayments = JointPayeePaymentDataProvider.GetJointPayeePayments(graph, payment);
            return jointPayeePayments.Sum(jpp => jpp.JointAmountToPay);
        }
    }
}