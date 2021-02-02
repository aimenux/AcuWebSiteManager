using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PX.Data;
using PX.Objects;
using PX.Objects.AR;
using PX.Objects.SO;
using PX.Objects.AR.Repositories;
using PX.Objects.AR.CCPaymentProcessing.Common;
using PX.Objects.AR.CCPaymentProcessing.Helpers;
using PX.Objects.AR.CCPaymentProcessing.Interfaces;
using PX.Objects.AR.CCPaymentProcessing;

namespace PX.Objects.Extensions.PaymentTransaction
{
	class SOInvoiceAfterProcessingManager : AfterProcessingManager
	{
		SOInvoiceEntry Graph { get; set; }
		public override void RunAuthorizeActions(IBqlTable table, bool success)
		{
			var graph = CreateGraphIfNeeded(table);
			UpdateSOInvoiceState(graph, CCTranType.AuthorizeOnly, success);
		}

		public override void RunCaptureActions(IBqlTable table, bool success)
		{
			var graph = CreateGraphIfNeeded(table);
			UpdateSOInvoiceState(graph, CCTranType.AuthorizeAndCapture, success);
		}

		public override void RunPriorAuthorizedCaptureActions(IBqlTable table, bool success)
		{
			var graph = CreateGraphIfNeeded(table);
			UpdateSOInvoiceState(graph, CCTranType.PriorAuthorizedCapture, success);
		}

		public override void RunVoidActions(IBqlTable table, bool success)
		{
			var graph = CreateGraphIfNeeded(table);
			UpdateSOInvoiceState(graph, CCTranType.VoidOrCredit, success);
		}

		public override void RunCreditActions(IBqlTable table, bool success)
		{
			var graph = CreateGraphIfNeeded(table);
			UpdateSOInvoiceState(graph, CCTranType.VoidOrCredit, success);
		}

		public void UpdateSOInvoiceState(SOInvoiceEntry invoiceEntry, CCTranType lastOperation, bool success)
		{
			if (!success) return;
			SOInvoice doc = invoiceEntry.SODocument.Select();

			IExternalTransaction lastTran = invoiceEntry.ExternalTran.SelectSingle();
			ExternalTransactionState lastTranState = ExternalTranHelper.GetTransactionState(invoiceEntry, lastTran);
			bool needUpdate = ExternalTranHelper.UpdateCapturedState(doc, lastTranState);

			if (doc.IsCCCaptured == true && lastTran?.TranNumber != null)
			{
				doc.ExtRefNbr = lastTran.TranNumber;
			}
			else if (doc.IsCCCaptured == false)
			{
				if (lastTranState.IsVoided || lastTranState.IsDeclined)
				{
					doc.ExtRefNbr = null;
				}

				invoiceEntry.UpdateRelatedSOOrderCapturedAmount(lastOperation, invoiceEntry.ExternalTran.SelectSingle(), lastTranState);

				if (lastTranState.ProcessingStatus == ProcessingStatus.VoidSuccess)
				{
					needUpdate = true;
				}
			}

			if (needUpdate)
			{
				doc = invoiceEntry.SODocument.Update(doc);
				invoiceEntry.Document.Search<ARInvoice.refNbr>(doc.RefNbr, doc.DocType);
				CCProcTranRepository repo = new CCProcTranRepository(invoiceEntry);
				if (doc.IsCCCaptured == true)
				{
					foreach (ExternalTransaction extTran in invoiceEntry.ExternalTran.Select())
					{
						if (string.IsNullOrEmpty(extTran.RefNbr) || string.IsNullOrEmpty(extTran.DocType))
						{
							extTran.DocType = doc.DocType;
							extTran.RefNbr = doc.RefNbr;
							invoiceEntry.ExternalTran.Update(extTran);
							foreach (CCProcTran tran in repo.GetCCProcTranByTranID(extTran.TransactionID))
							{
								if (string.IsNullOrEmpty(tran.RefNbr) || string.IsNullOrEmpty(tran.DocType))
								{
									tran.DocType = doc.DocType;
									tran.RefNbr = doc.RefNbr;
									invoiceEntry.ccProcTran.Update(tran);
								}
							}
						}
						invoiceEntry.UpdateRelatedSOOrderPreAuthAmount(lastOperation, extTran, lastTranState);
					}
				}
				invoiceEntry.Save.Press();
			}
		}

		public override void PersistData()
		{
			var doc = Graph?.Document.Current;
			if (doc != null)
			{
				PXEntryStatus status = Graph.Document.Cache.GetStatus(doc);
				if (status != PXEntryStatus.Notchanged)
				{
					Graph.Save.Press();
				}
			}
		}

		protected virtual SOInvoiceEntry CreateGraphIfNeeded(IBqlTable table)
		{
			if (Graph == null)
			{
				SOInvoice doc = table as SOInvoice;
				SOInvoiceEntry graph = PXGraph.CreateInstance<SO.SOInvoiceEntry>();
				graph.Document.Current = graph.Document.Search<ARInvoice.refNbr>(doc.RefNbr, doc.DocType);
				Graph = graph;
			}
			return Graph;
		}

		public override PXGraph GetGraph()
		{
			return Graph;
		}
	}
}
