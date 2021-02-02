using System.Collections.Generic;
using System.Linq;
using PX.Data;
using PX.Data.BQL;
using PX.Data.BQL.Fluent;
using PX.Objects.AP;
using PX.Objects.CN.JointChecks.AP.DAC;
using PX.Objects.CN.JointChecks.AP.Services.DataProviders;

namespace PX.Objects.CN.Common.Services.DataProviders
{
    public class TransactionDataProvider
    {
        public static APTran GetTransaction(PXGraph graph, string transactionType,
            string referenceNumber, int? lineNumber)
        {
            return SelectFrom<APTran>
                .Where<APTran.refNbr.IsEqual<P.AsString>
                    .And<APTran.tranType.IsEqual<P.AsString>>
                    .And<APTran.lineNbr.IsEqual<P.AsInt>>>.View
                .Select(graph, referenceNumber, transactionType, lineNumber);
        }

        public static IEnumerable<APTran> GetTransactions(PXGraph graph, string transactionType,
            string referenceNumber, IEnumerable<int?> lineNumbers)
        {
            return SelectFrom<APTran>
                .Where<APTran.refNbr.IsEqual<P.AsString>
                    .And<APTran.tranType.IsEqual<P.AsString>>
                    .And<APTran.lineNbr.IsIn<P.AsInt>>>.View
                .Select(graph, referenceNumber, transactionType, lineNumbers.ToArray()).FirstTableItems;
        }

        public static IEnumerable<APTran> GetTransactions(PXGraph graph, string transactionType, string referenceNumber)
        {
            return SelectFrom<APTran>
                .Where<APTran.refNbr.IsEqual<P.AsString>
                    .And<APTran.tranType.IsEqual<P.AsString>>>.View
                .Select(graph, referenceNumber, transactionType).FirstTableItems;
        }

        public static IEnumerable<APTran> GetTransactions(PXGraph graph, string paymentReferenceNumber)
        {
            return SelectFrom<APTran>
                .InnerJoin<APAdjust>
                    .On<APTran.refNbr.IsEqual<APAdjust.adjdRefNbr>
                        .And<APTran.tranType.IsEqual<APAdjust.adjdDocType>
                        .And<Brackets<APTran.lineNbr.IsEqual<APAdjust.adjdLineNbr>
                            .Or<APAdjust.adjdLineNbr.IsEqual<Zero>>>>>>
                .Where<APAdjust.adjgRefNbr.IsEqual<P.AsString>>.View
                .Select(graph, paymentReferenceNumber).FirstTableItems;
        }

        public static APTran GetTransaction(PXGraph graph, JointPayee jointPayee)
        {
            var jointPayeePayment = JointPayeePaymentDataProvider.GetJointPayeePayment(graph, jointPayee.JointPayeeId);
            return GetTransaction(graph, jointPayeePayment.InvoiceDocType ?? jointPayeePayment.PaymentDocType,
                jointPayeePayment.InvoiceRefNbr ?? jointPayeePayment.PaymentRefNbr, jointPayee.BillLineNumber);
        }

        public static APTran GetOriginalTransaction(PXGraph graph, APTran transaction)
        {
            if (transaction.OrigLineNbr != null)
            {
                var originalInvoice =
                    InvoiceDataProvider.GetOriginalInvoice(graph, transaction.RefNbr, transaction.TranType);
                return GetTransaction(graph, originalInvoice.DocType, originalInvoice.RefNbr, transaction.OrigLineNbr);
            }
            return transaction;
        }
    }
}