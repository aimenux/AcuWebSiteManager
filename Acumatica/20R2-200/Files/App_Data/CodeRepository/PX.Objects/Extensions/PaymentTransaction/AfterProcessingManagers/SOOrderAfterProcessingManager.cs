using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PX.Data;
using PX.Objects;
using PX.Objects.AR;
using PX.Objects.SO;
using PX.Objects.AR.CCPaymentProcessing.Common;
using PX.Objects.AR.CCPaymentProcessing.Helpers;
using PX.Objects.AR.CCPaymentProcessing.Interfaces;
using PX.Objects.AR.CCPaymentProcessing;

namespace PX.Objects.Extensions.PaymentTransaction
{
	class SOOrderAfterProcessingManager : AfterProcessingManager
	{
		SOOrderEntry Graph { get; set; }

		public override void RunAuthorizeActions(IBqlTable table, bool success)
		{
			var graph = CreateGraphIfNeeded(table);
			UpdateSOOrderState(graph, CCTranType.AuthorizeOnly, success);
		}

		public override void RunCaptureActions(IBqlTable table, bool success)
		{
			var graph = CreateGraphIfNeeded(table);
			UpdateSOOrderState(graph, CCTranType.AuthorizeAndCapture, success);
		}

		public override void RunPriorAuthorizedCaptureActions(IBqlTable table, bool success)
		{
			var graph = CreateGraphIfNeeded(table);
			UpdateSOOrderState(graph, CCTranType.PriorAuthorizedCapture, success);
		}

		public override void RunVoidActions(IBqlTable table, bool success)
		{
			var graph = CreateGraphIfNeeded(table);
			UpdateSOOrderState(graph, CCTranType.VoidOrCredit, success);
		}

		public override void RunCreditActions(IBqlTable table, bool success)
		{
			var graph = CreateGraphIfNeeded(table);
			UpdateSOOrderState(graph, CCTranType.VoidOrCredit, success);
		}

		public void UpdateSOOrderState(SOOrderEntry soGraph, CCTranType lastOperation, bool success)
		{
			if (!success) return;
			SOOrder olddoc = soGraph.Document.Current as SOOrder;
			if (olddoc == null) return;
			UpdateDocState(soGraph, olddoc, lastOperation);
		}

		public void UpdateDocState(SOOrderEntry graph, SOOrder olddoc, CCTranType lastOperation)
		{
			SOOrderEntry orderEntry = graph as SOOrderEntry;
			orderEntry.Document.Current = orderEntry.Document.Search<SOOrder.orderNbr>(olddoc.OrderNbr, olddoc.OrderType);

			SOOrder doc = (SOOrder)orderEntry.Document.Cache.CreateCopy(orderEntry.Document.Current);
			orderEntry.ExternalTran.Cache.Clear();
			IExternalTransaction extTran = orderEntry.ExternalTran.SelectSingle();
			ExternalTransactionState state = ExternalTranHelper.GetTransactionState(orderEntry, extTran);
			bool needUpdate = ExternalTranHelper.UpdateCCPaymentState(doc, state);

			if (doc.IsCCCaptured == true || doc.IsCCAuthorized == true)
			{
				doc.ExtRefNbr = extTran.TranNumber;
				doc = orderEntry.Document.Update(doc);
			}
			else if (doc.IsCCCaptured == false && doc.IsCCAuthorized == false && (state.IsVoided || state.IsDeclined))
			{
				doc.ExtRefNbr = null;
				doc = orderEntry.Document.Update(doc);
			}

			bool needsPersist = false;
			if (needUpdate)
			{
				doc.PreAuthTranNumber = null;
				doc = orderEntry.Document.Update(doc);
				orderEntry.Document.Search<SOOrder.orderNbr>(doc.OrderNbr, doc.OrderType);
				needsPersist = true;
			}
			try
			{
				if (orderEntry.soordertype.Current.CanHaveApplications)
				{
					if ((state.IsVoided || state.IsRefunded)
						&& orderEntry.arsetup.Current.IntegratedCCProcessing == true)
					{
						PXSetPropertyException message = null;
						ARPaymentEntry docgraph = PXGraph.CreateInstance<ARPaymentEntry>();
						ARPayment payment = PXSelect<ARPayment, Where<ARPayment.docType, Equal<Required<ARPayment.docType>>,
							And<ARPayment.refNbr, Equal<Required<ARPayment.refNbr>>>>>.Select(docgraph, extTran.DocType, extTran.RefNbr);
						if (payment != null && payment.Voided == false)
						{
							docgraph.VoidCheckProcExt(payment);

							ARPayment voidPayment = docgraph.Document.Current;
							if (voidPayment?.DocType == ARDocType.VoidPayment)
							{
								voidPayment.ExtRefNbr = extTran.TranNumber;
							}

							docgraph.Save.Press();
							PaymentTransactionGraph<SOOrderEntry, SOOrder>.ReleaseARDocument(docgraph.Document.Current);
							needsPersist = true;
						}
						else if (payment != null)
						{
							message = new PXSetPropertyException(SO.Messages.CouldNotVoidCCTranPayment, PXErrorLevel.Warning, extTran.RefNbr);
						}

						if (message != null)
						{
							PXLongOperation.SetCustomInfo(new SOOrderEntry.SOOrderMessageDisplay(message));
						}
					}
					if (state.IsCaptured)
					{
						orderEntry.Save.Press();
						// CC transactions linked directly to SO Order are no more supported
						//PXGraph target;
						//orderEntry.CreatePaymentProc(orderEntry.Document.Current, out target);
						needsPersist = true;
					}
				}
			}
			finally
			{
				if (needsPersist)
				{
					orderEntry.Save.Press();
				}
			}
		}

		public override void PersistData()
		{
			SOOrder doc = Graph?.Document.Current;
			if (doc != null)
			{
				PXEntryStatus status = Graph.Document.Cache.GetStatus(doc);
				if (status != PXEntryStatus.Notchanged)
				{
					Graph.Save.Press();
				}
			}
		}

		public override PXGraph GetGraph()
		{
			return Graph;
		}

		protected virtual SOOrderEntry CreateGraphIfNeeded(IBqlTable table)
		{
			if (Graph == null)
			{
				var soGraph = PXGraph.CreateInstance<SOOrderEntry>();
				var order = table as SOOrder;
				soGraph.Document.Current = soGraph.Document.Search<SOOrder.orderNbr>(order.OrderNbr, order.OrderType);
				Graph = soGraph;
			}
			return Graph;
		}
	}
}
