using PX.Data;
using PX.Objects.AP;
using PX.Objects.CN.Common.Services.DataProviders;
using PX.Objects.CN.JointChecks.AP.Services.DataProviders;

namespace PX.Objects.CN.JointChecks.AP.Services.CalculationServices
{
    public class InvoiceLineBalanceCalculationService : InvoiceBalanceCalculationService
    {
        public InvoiceLineBalanceCalculationService(PXGraph graph)
            : base(graph)
        {
        }

        public override decimal? GetInvoiceBalance(APAdjust adjustment)
        {
            var transaction = TransactionDataProvider.GetTransaction(Graph, adjustment.AdjdDocType,
                adjustment.AdjdRefNbr, adjustment.AdjdLineNbr);
            return transaction.CuryTranAmt;
        }
    }
}