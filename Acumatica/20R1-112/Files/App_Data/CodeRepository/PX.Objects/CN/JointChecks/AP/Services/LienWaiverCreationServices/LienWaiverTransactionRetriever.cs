using System.Collections.Generic;
using System.Linq;
using PX.Common;
using PX.Data;
using PX.Objects.AP;
using PX.Objects.CM;
using PX.Objects.CN.Common.Extensions;
using PX.Objects.CN.Common.Services;
using PX.Objects.CN.Common.Services.DataProviders;
using PX.Objects.CN.JointChecks.AP.Models;
using PX.Objects.CN.JointChecks.AP.Services.DataProviders;
using PX.Objects.CN.JointChecks.AP.Services.PreparePaymentsServices;
using PX.Objects.CN.Subcontracts.AP.CacheExtensions;
using PX.Objects.Common;

namespace PX.Objects.CN.JointChecks.AP.Services.LienWaiverCreationServices
{
    public class LienWaiverTransactionRetriever : ILienWaiverTransactionRetriever
    {
        private readonly APPaymentEntry graph;

        public LienWaiverTransactionRetriever(PXGraph graph)
        {
            this.graph = (APPaymentEntry) graph;
        }

        /// <summary>
        /// Returns transactions related to adjustments from the current payment in cache.
        /// Returns empty collection in case an adjustment relates to multiple transactions with references to
        /// different projects or commitments. It is possible for a bill without Pay per line feature.
        /// </summary>
        public IEnumerable<APTran> GetTransactions()
        {
            var insertedAdjustments = graph.Caches<APAdjust>().Inserted.RowCast<APAdjust>().ToList();
            var adjustments = SiteMapExtension.IsPreparePaymentsScreenId()
                ? PreparePaymentsAdjustmentsCache.GetStoredAdjustments().Where(adjust =>
                    IsRelatedAdjustment(insertedAdjustments.SingleOrDefault(), adjust))
                : graph.Adjustments.Select().FirstTableItems.Concat(insertedAdjustments).ToList();
            var transactions = adjustments.Select(GetTransaction).ToList();
            return transactions.Any(t => t == null)
                ? Enumerable.Empty<APTran>()
                : transactions;
        }

        /// <summary>
        /// Returns transactions related to adjustments from the current payment in cache, that are referenced to
        /// to project and commitment from <see cref="LienWaiverGenerationKey"/>.
        /// </summary>
        public IEnumerable<APTran> GetTransactions(LienWaiverGenerationKey generationKey)
        {
            return GetTransactions()
                .Where(tran => IsRelatedToCommitment(tran, generationKey.ProjectId, generationKey.OrderNumber));
        }

        private APTran GetTransaction(APAdjust adjustment)
        {
            return adjustment.AdjdLineNbr == 0
                ? GetTransactionIfAdjustmentIsRelatedToSingleCommitment(adjustment)
                : TransactionDataProvider.GetTransaction(graph, adjustment.AdjdDocType, adjustment.AdjdRefNbr,
                    adjustment.AdjdLineNbr);
        }

        private APTran GetTransactionIfAdjustmentIsRelatedToSingleCommitment(IDocumentAdjustment adjustment)
        {
            var (retainageTransaction, originalTransactions) = GetOriginalBillTransactionsWithRetainageKey(adjustment);
            var transaction = originalTransactions.First();
            var subcontractNumber = GetSubcontractNumber(transaction);
            return originalTransactions.All(tran =>
                IsRelatedToCommitment(tran, transaction.ProjectID, subcontractNumber ?? transaction.PONbr))
                ? GetOriginalTransactionWithRetainageBillReference(transaction, retainageTransaction)
                : null;
        }

        private static APTran GetOriginalTransactionWithRetainageBillReference(APTran originalTransaction,
            IDocumentTran retainageTransaction)
        {
            if (retainageTransaction == null)
            {
                return originalTransaction;
            }
            originalTransaction.RefNbr = retainageTransaction.RefNbr;
            originalTransaction.CuryTranAmt = retainageTransaction.CuryTranAmt;
            return originalTransaction;
        }

        private (APTran retainageTransaction, List<APTran> originalTransactions)
            GetOriginalBillTransactionsWithRetainageKey(IDocumentAdjustment adjustment)
        {
            var billTransactions = TransactionDataProvider
                .GetTransactions(graph, adjustment.AdjdDocType, adjustment.AdjdRefNbr).ToList();
            var singleTransaction = billTransactions.SingleOrNull();
            if (singleTransaction?.OrigLineNbr != 0)
            {
                return (null, billTransactions);
            }
            var originalInvoice = InvoiceDataProvider
                .GetOriginalInvoice(graph, singleTransaction.RefNbr, singleTransaction.TranType);
            var originalTransactions = TransactionDataProvider.GetTransactions(graph, originalInvoice.DocType,
                originalInvoice.RefNbr).ToList();
            return (singleTransaction, originalTransactions);
        }

        private bool IsRelatedAdjustment(IDocumentAdjustment insertedAdjustment, IDocumentAdjustment adjustment)
        {
            insertedAdjustment = insertedAdjustment ?? graph.Caches<APAdjust>().Current as APAdjust;
            var payment = graph.Document.Current;
            return graph.APSetup.Current.HoldEntry == true
                ? adjustment.AdjgRefNbr == payment.RefNbr &&
                adjustment.AdjgDocType == payment.DocType
                : adjustment.AdjdRefNbr == insertedAdjustment?.AdjdRefNbr &&
                adjustment.AdjdDocType == insertedAdjustment?.AdjdDocType;
        }

        private bool IsRelatedToCommitment(APTran transaction, int? projectId, string commitmentNumber)
        {
            var originalTransaction = TransactionDataProvider.GetOriginalTransaction(graph, transaction);
            var subcontractNumber = GetSubcontractNumber(originalTransaction);
            return originalTransaction.ProjectID == projectId &&
                commitmentNumber.IsIn(originalTransaction.PONbr, subcontractNumber);
        }

        private static string GetSubcontractNumber(APTran transaction)
        {
            return PXCache<APTran>.GetExtension<ApTranExt>(transaction).SubcontractNbr;
        }
    }
}