using PX.Common;
using PX.Data;
using PX.Objects.AP;
using PX.Objects.CN.JointChecks.AP.Services.DataProviders;

namespace PX.Objects.CN.JointChecks.AP.Services
{
    public class LienWaiverHoldPaymentService
    {
        private readonly LienWaiverDataProvider lienWaiverDataProvider;
        private readonly PXGraph graph;

        public LienWaiverHoldPaymentService(PXGraph graph)
        {
            lienWaiverDataProvider = new LienWaiverDataProvider(graph);
            this.graph = graph;
        }

        public void HoldPaymentsIfNeeded(string invoiceReferenceNumber, bool? shouldStopPayments)
        {
            var payments = PaymentDataProvider.GetGeneratedPayments(graph, invoiceReferenceNumber);
            payments.ForEach(payment => HoldPaymentIfNeeded(payment, shouldStopPayments));
        }

        public void HoldPaymentIfNeeded(APRegister payment, bool? shouldStopPayments)
        {
            payment.Hold = shouldStopPayments == true
                && lienWaiverDataProvider.DoesAnyOutstandingComplianceExist(payment);
            graph.Caches<APPayment>().Update(payment);
        }
    }
}