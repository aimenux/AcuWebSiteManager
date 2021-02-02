using System.Collections.Generic;
using System.Linq;
using PX.Data;
using PX.Data.BQL;
using PX.Data.BQL.Fluent;
using PX.Objects.AP;
using PX.Objects.CN.JointChecks.AP.Services.DataProviders;
using PX.Objects.Common.Abstractions;

namespace PX.Objects.CN.Common.Services.DataProviders
{
	public class InvoiceDataProvider
	{
		public static APInvoice GetInvoice(PXGraph graph, string documentType, string referenceNumber)
		{
			return SelectFrom<APInvoice>
				.Where<APInvoice.refNbr.IsEqual<P.AsString>
					.And<APInvoice.docType.IsEqual<P.AsString>>>.View
				.Select(graph, referenceNumber, documentType);
		}

		public static IEnumerable<APInvoice> GetRelatedRetainageBills(PXGraph graph, string referenceNumber,
			string documentType)
		{
			return new PXSelectJoin<APInvoice,
					InnerJoin<APRetainageInvoice, On<APInvoice.docType, Equal<APRetainageInvoice.docType>,
						And<APInvoice.refNbr, Equal<APRetainageInvoice.refNbr>>>>,
					Where<APRetainageInvoice.isRetainageDocument, Equal<True>,
						And<APRetainageInvoice.origDocType, Equal<Required<APInvoice.docType>>,
						And<APRetainageInvoice.origRefNbr, Equal<Required<APInvoice.refNbr>>>>>>(graph)
				.Select(documentType, referenceNumber).FirstTableItems;
		}

		public static IEnumerable<APInvoice> GetAllBillsFromRetainageGroup(PXGraph graph, string referenceNumber,
			string documentType)
		{
			var originalBill = GetOriginalInvoice(graph, referenceNumber, documentType);
			var allRelatedBills = GetRelatedRetainageBills(graph, originalBill.RefNbr, originalBill.DocType);
			return allRelatedBills.Append(originalBill);
		}

		public static APInvoice GetOriginalInvoice(PXGraph graph, APInvoice invoice)
		{
			return invoice?.IsRetainageDocument == true
				? GetInvoice(graph, invoice.OrigDocType, invoice.OrigRefNbr)
				: invoice;
		}

		public static APInvoice GetOriginalInvoice(PXGraph graph, string referenceNumber, string documentType)
		{
			var invoice = GetInvoice(graph, documentType, referenceNumber);
			return GetOriginalInvoice(graph, invoice);
		}

		public static IEnumerable<APInvoice> GetInvoices(PXGraph graph, IEnumerable<APTran> transactions)
		{
			return transactions.Select(tran => GetInvoice(graph, tran.TranType, tran.RefNbr))
				.Distinct();
		}

		public static IEnumerable<APInvoice> GetReversedRetainageDebitAdjustments(PXGraph graph, string referenceNumber)
		{
			return SelectFrom<APInvoice>
				.Where<APRegister.origRefNbr.IsEqual<P.AsString>
					.And<APRegister.origDocType.IsEqual<APDocType.invoice>>
					.And<APInvoice.isRetainageDocument.IsEqual<True>>
					.And<APInvoice.docType.IsEqual<APDocType.debitAdj>>>.View
				.Select(graph, referenceNumber).FirstTableItems;
		}

		public static IEnumerable<APTran> GetReversedRetainageDebitAdjustmentLines(PXGraph graph, string referenceNumber, int? lineNbr)
		{
			return SelectFrom<APTran>
				.InnerJoin<APRetainageInvoice>
					.On<APRetainageInvoice.docType.IsEqual<APTran.tranType>
						.And<APRetainageInvoice.refNbr.IsEqual<APTran.refNbr>>>
				.Where<APRetainageInvoice.origRefNbr.IsEqual<P.AsString>
					   .And<APTran.origLineNbr.IsEqual<P.AsInt>>
						.And<APRetainageInvoice.origDocType.IsEqual<APDocType.invoice>>
						.And<APRetainageInvoice.isRetainageDocument.IsEqual<True>>
						.And<APRetainageInvoice.docType.IsEqual<APDocType.debitAdj>>>.View
					.Select(graph, referenceNumber, lineNbr).FirstTableItems;
		}

		public static FinDocumentExtKey GetSourceEntityKeyByRetainage(PXGraph graph, string docType, string refNbr, int? lineNbr)
		{
			FinDocumentExtKey key = new FinDocumentExtKey();

			APInvoice bill = GetInvoice(graph, docType, refNbr);

			if (bill.IsRetainageDocument == true)
			{
				key.Type = bill.OrigDocType;
				key.RefNbr = bill.OrigRefNbr;

				if (lineNbr != 0)
				{
					key.LineNbr = TransactionDataProvider.GetTransaction(graph, bill.DocType, bill.RefNbr, lineNbr)
						.OrigLineNbr;
				}
				else
				{
					key.LineNbr = lineNbr;
				}
			}
			else
			{
				key.Type = docType;
				key.RefNbr = refNbr;
				key.LineNbr = lineNbr;
			}

			return key;
		}
	}
}