using PX.Data;
using PX.Objects.AP;
using PX.Objects.CN.Common.Services.DataProviders;

namespace PX.Objects.CN.JointChecks.AP.Services.CalculationServices
{
    public class InvoiceBalanceCalculationService : CalculationServiceBase
    {
        public InvoiceBalanceCalculationService(PXGraph graph)
            : base(graph)
        {
        }

        public virtual decimal? GetInvoiceBalance(APAdjust adjustment)
        {
            var invoice = InvoiceDataProvider.GetInvoice(Graph, adjustment.AdjdDocType, adjustment.AdjdRefNbr);
            return invoice.CuryDocBal;
        }
    }
}