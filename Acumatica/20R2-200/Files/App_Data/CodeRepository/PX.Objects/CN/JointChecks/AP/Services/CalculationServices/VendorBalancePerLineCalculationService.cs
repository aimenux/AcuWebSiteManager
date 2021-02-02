using System;
using System.Collections.Generic;
using System.Linq;
using PX.Common;
using PX.Data;
using PX.Objects.AP;
using PX.Objects.CN.Common.Extensions;
using PX.Objects.CN.Common.Services.DataProviders;
using PX.Objects.CN.JointChecks.AP.DAC;
using PX.Objects.CN.JointChecks.AP.Services.DataProviders;

namespace PX.Objects.CN.JointChecks.AP.Services.CalculationServices
{
    public class VendorBalancePerLineCalculationService : VendorBalanceCalculationService
    {
        public VendorBalancePerLineCalculationService(PXGraph graph, IEnumerable<JointPayee> jointPayees)
            : base(graph, jointPayees)
        {
        }

        public decimal? GetVendorBalancePerLine(APAdjust adjustment)
        {
            var jointPayeesTotalBalanceForLine = JointPayees
                .Where(jp => jp.BillLineNumber == adjustment.AdjdLineNbr)
                .Sum(jp => jp.JointBalance);
            var transactionBalance = TransactionDataProvider.GetTransaction(
                Graph, adjustment.AdjdDocType, adjustment.AdjdRefNbr, adjustment.AdjdLineNbr).CuryTranBal;

            APInvoice invoice = InvoiceDataProvider.GetInvoice(Graph, adjustment.AdjdDocType, adjustment.AdjdRefNbr);

            if (invoice.IsRetainageDocument == true)
            {
                var openBalancePerLine = GetOpenBalanceByAllBillsFromRetainageGroupPerLine(adjustment, transactionBalance);
                var vendorBalancePerLine = openBalancePerLine - jointPayeesTotalBalanceForLine;
                return Math.Min(transactionBalance.GetValueOrDefault(), vendorBalancePerLine.GetValueOrDefault());
            }
            else
            {
                return transactionBalance - jointPayeesTotalBalanceForLine;
            }
        }

        protected override decimal? GetOpenBalanceByAllBillsFromRetainageGroup(APInvoice invoice)
        {
            var originalInvoice = InvoiceDataProvider.GetOriginalInvoice(Graph, invoice.RefNbr, invoice.DocType);
            var billLineNumbers = JointPayees.Select(jp => jp.BillLineNumber).ToList();
            var unreleasedRetainageAmount = GetUnreleasedRetainageAmount(originalInvoice, billLineNumbers);
            var releasedAmountFromRetainageGroup = GeReleasedAmountFromRetainageGroup(invoice, billLineNumbers);
            return invoice.CuryDocBal + unreleasedRetainageAmount + releasedAmountFromRetainageGroup;
        }

        private decimal? GetOpenBalanceByAllBillsFromRetainageGroupPerLine(
            APAdjust adjustment, decimal? transactionBalance)
        {
            var invoice = InvoiceDataProvider.GetInvoice(Graph, adjustment.AdjdDocType, adjustment.AdjdRefNbr);
            var originalInvoice = InvoiceDataProvider.GetOriginalInvoice(Graph, invoice);
            var unreleasedRetainageAmount =
                GetUnreleasedRetainageAmount(originalInvoice, adjustment.AdjdLineNbr.CreateArray());
            var releasedAmountFromRetainageGroup =
                GeReleasedAmountFromRetainageGroup(invoice, adjustment.AdjdLineNbr.CreateArray());
            return transactionBalance + unreleasedRetainageAmount + releasedAmountFromRetainageGroup;
        }

        private decimal? GetUnreleasedRetainageAmount(APInvoice originalInvoice, IEnumerable<int?> billLineNumbers)
        {
            return TransactionDataProvider
                .GetTransactions(Graph, originalInvoice.DocType, originalInvoice.RefNbr, billLineNumbers)
                .Sum(t => t.CuryRetainageBal);
        }

        private decimal? GeReleasedAmountFromRetainageGroup(APInvoice invoice, IEnumerable<int?> billLineNumbers)
        {
            var allReleasedBillsFromRetainageGroup = InvoiceDataProvider
                .GetAllBillsFromRetainageGroup(Graph, invoice.RefNbr, invoice.DocType)
                .Except(invoice).Where(bill => bill.Released == true);
            return allReleasedBillsFromRetainageGroup.SelectMany(bill => TransactionDataProvider
                    .GetTransactions(Graph, bill.DocType, bill.RefNbr, billLineNumbers))
                .Sum(transaction => transaction.CuryTranBal);
        }
    }
}