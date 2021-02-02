using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PX.Data;
using PX.Objects;
using PX.Objects.AR;
using PX.Objects.AR.CCPaymentProcessing.Common;
using PX.Objects.AR.CCPaymentProcessing.Helpers;
using PX.Objects.AR.CCPaymentProcessing.Repositories;
using PX.Objects.AR.CCPaymentProcessing.Interfaces;
using PX.Objects.AR.CCPaymentProcessing;
using PX.Objects.AR.Standalone;

namespace PX.Objects.Extensions.PaymentTransaction
{
	public class ARCashSaleAfterProcessingManager : AfterProcessingManager
	{
		public bool ReleaseDoc { get; set; }

		public ARCashSaleEntry Graph { get; set; }

		private ARCashSaleEntry graphWithOriginDoc;

		private IBqlTable inputTable;

		public override void RunAuthorizeActions(IBqlTable table, bool success)
		{
			inputTable = table;
			ARCashSaleEntry graph = CreateGraphIfNeeded(table);
			ChangeDocProcessingStatus(graph, CCTranType.AuthorizeOnly, success);
			RestoreCopy();
		}

		public override void RunCaptureActions(IBqlTable table, bool success)
		{
			RunCaptureActions(table, CCTranType.AuthorizeAndCapture, success);
		}

		public override void RunPriorAuthorizedCaptureActions(IBqlTable table, bool success)
		{
			RunCaptureActions(table, CCTranType.PriorAuthorizedCapture, success);
		}

		public override void RunVoidActions(IBqlTable table, bool success)
		{
			inputTable = table;
			CCTranType tranType = CCTranType.VoidOrCredit;
			ARCashSaleEntry graph = CreateGraphIfNeeded(table);
			ChangeDocProcessingStatus(graph, tranType, success);
			UpdateCashSale(graph, tranType, success);
			ARCashSale cashSale = Graph.Document.Current;
			if (ReleaseDoc = cashSale.VoidAppl == true && NeedRelease(cashSale))
			{
				ReleaseDocument(graph, tranType, success);
			}
			RestoreCopy();
		}

		public override void RunCreditActions(IBqlTable table,  bool success)
		{
			inputTable = table;
			CCTranType type = CCTranType.VoidOrCredit;
			ARCashSaleEntry graph = CreateGraphIfNeeded(table);
			ChangeDocProcessingStatus(graph, type, success);
			UpdateCashSale(graph, type, success);
			ARCashSale cashSale = Graph.Document.Current;
			if (NeedRelease(cashSale))
			{
				ReleaseDocument(graph, type, success);
			}
			RestoreCopy();
		}

		private void RunCaptureActions(IBqlTable table, CCTranType tranType, bool success)
		{
			inputTable = table;
			ARCashSaleEntry graph = CreateGraphIfNeeded(table);
			ChangeDocProcessingStatus(graph, tranType, success);
			UpdateCashSale(graph, tranType, success);
			ARCashSale cashSale = Graph.Document.Current;
			if (NeedRelease(cashSale))
			{
				ReleaseDocument(graph, tranType, success);
			}
			RestoreCopy();
		}

		private bool NeedRelease(ARCashSale cashSale)
		{
			return ReleaseDoc && cashSale.Released == false && Graph.arsetup.Current.IntegratedCCProcessing == true;
		}

		public void UpdateCashSale(ARCashSaleEntry graph, CCTranType tranType, bool success)
		{
			if (!success)
				return;
			ARCashSaleEntry cashSaleGraph = graph as ARCashSaleEntry;
			ARCashSale toProc = (ARCashSale)cashSaleGraph.Document.Current;
			IExternalTransaction currTran = cashSaleGraph.ExternalTran.SelectSingle();
			if (currTran != null)
			{
				toProc.DocDate = currTran.LastActivityDate.Value.Date;
				toProc.AdjDate = currTran.LastActivityDate.Value.Date;

				ExternalTransactionState tranState = ExternalTranHelper.GetTransactionState(cashSaleGraph, currTran);
				if (tranState.IsActive)
				{
					toProc.ExtRefNbr = currTran.TranNumber;
				}
				else if (toProc.DocType != ARDocType.CashReturn && (tranState.IsVoided || tranState.IsDeclined))
				{
					toProc.ExtRefNbr = null;
				}

				cashSaleGraph.Document.Update(toProc);
			}
		}

		public void ReleaseDocument(ARCashSaleEntry cashSaleGraph, CCTranType procTran, bool success) 
		{
			AR.ARRegister doc = cashSaleGraph.Document.Current as AR.ARRegister;
			cashSaleGraph.Actions.PressSave();
			if (doc != null && success)
			{
				var tran = cashSaleGraph.ExternalTran.SelectSingle(doc.DocType, doc.RefNbr);
				if (tran != null)
				{
					ExternalTransactionState state = ExternalTranHelper.GetTransactionState(cashSaleGraph, tran);
					if (!state.IsDeclined && !state.IsOpenForReview)
					{
						PaymentTransactionGraph<ARCashSaleEntry, ARCashSale>.ReleaseARDocument(doc);
					}
				}
			}
		}

		private void ChangeDocProcessingStatus(ARCashSaleEntry cashSaleGraph, CCTranType tranType, bool success)
		{
			ARCashSale cashSale = (ARCashSale)cashSaleGraph.Document.Current; ;
			var extTran = cashSaleGraph.ExternalTran.SelectSingle();
			if (extTran == null) return;
			ExternalTransactionState state = ExternalTranHelper.GetTransactionState(cashSaleGraph, extTran);
			ChangeCaptureFailedFlag(state, cashSale);
			if (success)
			{
				bool pendingProcessing = true;
				if (extTran != null)
				{
					if (state.IsCaptured && !state.IsOpenForReview)
					{
						pendingProcessing = false;
					}
					if (cashSale.DocType == ARDocType.CashReturn && (state.IsVoided || state.IsRefunded)
						&& !state.IsOpenForReview)
					{
						pendingProcessing = false;
					}
					ChangeDocProcessingFlags(state, cashSale, tranType);
				}
				cashSale.PendingProcessing = pendingProcessing;
			}
			ChangeOriginDocProcessingStatus(cashSaleGraph, tranType, success);
			cashSaleGraph.Document.Update(cashSale);
		}

		private void ChangeOriginDocProcessingStatus(ARCashSaleEntry cashSaleGraph, CCTranType tranType, bool success)
		{
			IExternalTransaction tran = cashSaleGraph.ExternalTran.SelectSingle();
			ARCashSale cashSale = cashSaleGraph.Document.Current;
			ExternalTransactionState tranState = ExternalTranHelper.GetTransactionState(cashSaleGraph, tran);
			var oCashSaleGraph = GetGraphWithOriginDoc(cashSaleGraph, tranType);
			if (oCashSaleGraph != null)
			{
				var oExtTran = oCashSaleGraph.ExternalTran.SelectSingle();
				ARCashSale oCashSale = oCashSaleGraph.Document.Current;
				if (oExtTran.TransactionID == tran.TransactionID)
				{
					ChangeCaptureFailedFlag(tranState, oCashSale);
					if (success)
					{
						ChangeDocProcessingFlags(tranState, oCashSale, tranType);
					}
				}
				cashSaleGraph.Caches[typeof(ARCashSale)].Update(oCashSale);
			}
		}

		private void ChangeDocProcessingFlags(ExternalTransactionState tranState, ARCashSale doc, CCTranType tranType)
		{
			if (tranState.HasErrors) return;
			doc.IsCCAuthorized = doc.IsCCCaptured = doc.IsCCRefunded = false;
			if (!tranState.IsDeclined && !tranState.IsOpenForReview && !ExternalTranHelper.IsExpired(tranState.ExternalTransaction))
			{
				switch (tranType)
				{
					case CCTranType.AuthorizeAndCapture: doc.IsCCCaptured = true; break;
					case CCTranType.CaptureOnly: doc.IsCCCaptured = true; break;
					case CCTranType.PriorAuthorizedCapture: doc.IsCCCaptured = true; break;
					case CCTranType.AuthorizeOnly: doc.IsCCAuthorized = true; break;
					case CCTranType.Credit: doc.IsCCRefunded = true; break;
				}
				if (tranType == CCTranType.VoidOrCredit && tranState.IsRefunded)
				{
					doc.IsCCRefunded = true;
				}
			}
		}

		private void ChangeCaptureFailedFlag(ExternalTransactionState state, ARCashSale doc)
		{
			if (doc.IsCCCaptureFailed == false
				&& (state.ProcessingStatus == ProcessingStatus.CaptureFail || state.ProcessingStatus == ProcessingStatus.CaptureDecline))
			{
				doc.IsCCCaptureFailed = true;
			}
			else if (doc.IsCCCaptureFailed == true && (state.IsCaptured || state.IsVoided 
				|| (state.IsPreAuthorized && !state.HasErrors && CheckCaptureFailedExists(state))))
			{
				doc.IsCCCaptureFailed = false;
			}
		}

		private bool CheckCaptureFailedExists(ExternalTransactionState state)
		{
			bool ret = false;
			var repo = GetPaymentProcessingRepository();
			IEnumerable<CCProcTran> procTrans = repo.GetCCProcTranByTranID(state.ExternalTransaction.TransactionID);
			if (!CCProcTranHelper.HasCaptureFailed(procTrans))
			{
				ret = true;
			}
			return ret;
		}

		private ARCashSaleEntry GetGraphByDocTypeRefNbr(string docType, string refNbr)
		{
			var cashSaleGraph = PXGraph.CreateInstance<ARCashSaleEntry>();
			cashSaleGraph.Document.Current = PXSelect<ARCashSale, Where<ARCashSale.docType, Equal<Required<ARCashSale.docType>>,
				And<ARCashSale.refNbr, Equal<Required<ARCashSale.refNbr>>>>>
					.SelectWindowed(cashSaleGraph, 0, 1, docType, refNbr);
			return cashSaleGraph;
		}

		public override void PersistData()
		{
			ARCashSale doc = Graph?.Document.Current;
			if (doc != null)
			{
				PXEntryStatus status = Graph.Document.Cache.GetStatus(doc);
				if (status != PXEntryStatus.Notchanged)
				{
					Graph.Save.Press();
				}
			}
			RestoreCopy();
		}

		private ARCashSaleEntry GetGraphWithOriginDoc(ARCashSaleEntry graph, CCTranType tranType)
		{
			if (graphWithOriginDoc != null)
			{
				return graphWithOriginDoc;
			}

			IExternalTransaction tran = graph.ExternalTran.SelectSingle();
			ARCashSale cashSale = graph.Document.Current;
			ExternalTransactionState tranState = ExternalTranHelper.GetTransactionState(graph, tran);
			if (tranType == CCTranType.VoidOrCredit
				&& tran.DocType == cashSale.OrigDocType && tran.RefNbr == cashSale.OrigRefNbr
				&& tran.DocType == ARDocType.CashSale)
			{
				graphWithOriginDoc = GetGraphByDocTypeRefNbr(tran.DocType, tran.RefNbr);
			}
			return graphWithOriginDoc;
		}

		protected virtual ARCashSaleEntry CreateGraphIfNeeded(IBqlTable table)
		{
			if (Graph == null)
			{
				ARCashSale cashSale = table as ARCashSale;
				Graph = GetGraphByDocTypeRefNbr(cashSale.DocType, cashSale.RefNbr);
				Graph.Document.Update(cashSale);
			}
			return Graph;
		}

		protected virtual ICCPaymentProcessingRepository GetPaymentProcessingRepository()
		{
			ICCPaymentProcessingRepository repository = new CCPaymentProcessingRepository(Graph);
			return repository;
		}

		public override PXGraph GetGraph()
		{
			return Graph;
		}

		private void RestoreCopy()
		{
			ARCashSale doc = Graph?.Document.Current;
			if (doc != null && inputTable != null)
			{
				Graph.Document.Cache.RestoreCopy(inputTable, doc);
			}
		}
	}
}
