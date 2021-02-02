using System.Collections.Generic;
using System.Linq;
using PX.Common;
using PX.Data;
using PX.Objects.AP;
using PX.Objects.CN.Common.Services.DataProviders;
using PX.Objects.CN.JointChecks.AP.DAC;
using PX.Objects.Common;
using PX.Objects.Common.Abstractions;
using PX.Objects.Common.Extensions;

namespace PX.Objects.CN.JointChecks.AP.Services.DataProviders
{
	public class LienWaiverProjectDataProvider
	{
		public static IEnumerable<int?> GetProjectIds(APAdjust adjustment, PXGraph graph)
		{
			FinDocumentExtKey actualDocLineKey = InvoiceDataProvider.GetSourceEntityKeyByRetainage(graph,
				adjustment.AdjdDocType, adjustment.AdjdRefNbr, adjustment.AdjdLineNbr);

			var transactions = adjustment.AdjdLineNbr == 0
				? TransactionDataProvider.GetTransactions(graph, actualDocLineKey.Type, actualDocLineKey.RefNbr)
				: TransactionDataProvider.GetTransaction(graph, actualDocLineKey.Type, actualDocLineKey.RefNbr, actualDocLineKey.LineNbr).SingleToList();

			return transactions.Select(tran => tran.ProjectID);
		}

		public static IEnumerable<int?> GetProjectIds(PXGraph graph, IEnumerable<APAdjust> adjustments)
		{
			return adjustments.Select(adjustment =>
					InvoiceDataProvider.GetOriginalInvoice(graph, adjustment.AdjdRefNbr, adjustment.AdjdDocType))
				.SelectMany(inv => TransactionDataProvider
					.GetTransactions(graph, inv.DocType, inv.RefNbr).Select(t => t.ProjectID)).Distinct().ToList();
		}

		public static IEnumerable<int?> GetProjectIds(APInvoiceEntry graph, JointPayee jointPayee)
		{
			var originalBill = InvoiceDataProvider.GetOriginalInvoice(graph, graph.Document.Current);
			var transactions = TransactionDataProvider.GetTransactions(graph, originalBill.DocType, originalBill.RefNbr)
				.ToList();
			if (IsSingleProjectPerDocumentContext(graph.APSetup.Current, transactions))
			{
				return graph.Document.Current.ProjectID.AsSingleEnumerable();
			}
			if (jointPayee != null && graph.Document.Current.PaymentsByLinesAllowed == true)
			{
				transactions = transactions.Where(tran => tran.LineNbr == jointPayee.BillLineNumber).ToList();
			}
			return transactions.Select(tran => tran.ProjectID).Distinct();
		}

		public static IEnumerable<int?> GetProjectIds(APInvoiceEntry graph)
		{
			return GetProjectIds(graph, null);
		}

		public static IEnumerable<int?> GetProjectIds(APPaymentEntry graph, JointPayee jointPayee)
		{
			var adjustments = graph.Adjustments.SelectMain();
			var jointPayeeAdjustments = jointPayee.BillLineNumber == 0
				? adjustments.SelectMany(adjustment => GetTransactions(graph, adjustment))
				: adjustments.Select(adjustment => GetTransaction(graph, jointPayee, adjustment));
			return jointPayeeAdjustments.Select(tran => tran.ProjectID);
		}

		public static int? GetProjectId(APPaymentEntry graph, APTran transaction)
		{
			var originalTransaction = TransactionDataProvider.GetOriginalTransaction(graph, transaction);
			return originalTransaction.ProjectID;
		}

		private static APTran GetTransaction(PXGraph graph, JointPayee jointPayee, IDocumentAdjustment adjustment)
		{
			var originalBill =
				InvoiceDataProvider.GetOriginalInvoice(graph, adjustment.AdjdRefNbr, adjustment.AdjdDocType);
			return TransactionDataProvider.GetTransaction(graph, originalBill.DocType, originalBill.RefNbr,
				jointPayee.BillLineNumber);
		}

		private static IEnumerable<APTran> GetTransactions(PXGraph graph, IDocumentAdjustment adjustment)
		{
			var originalBill =
				InvoiceDataProvider.GetOriginalInvoice(graph, adjustment.AdjdRefNbr, adjustment.AdjdDocType);
			return TransactionDataProvider.GetTransactions(graph, originalBill.DocType, originalBill.RefNbr);
		}

		private static bool IsSingleProjectPerDocumentContext(APSetup accountPayableSetup,
			IEnumerable<APTran> transactions)
		{
			return accountPayableSetup.RequireSingleProjectPerDocument == true && transactions.IsEmpty();
		}
	}
}