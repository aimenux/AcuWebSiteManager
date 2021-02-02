using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PX.Common;
using PX.Data;
using PX.Data.BQL;
using PX.Data.BQL.Fluent;
using PX.Objects;
using PX.Objects.AR;
using PX.Objects.AR.CCPaymentProcessing.Common;
using PX.Objects.AR.CCPaymentProcessing.Helpers;
using PX.Objects.AR.CCPaymentProcessing.Interfaces;
using PX.Objects.AR.CCPaymentProcessing;
using PX.Objects.AR.CCPaymentProcessing.Repositories;
using PX.Objects.CA;
using PX.Objects.SO;

namespace PX.Objects.Extensions.PaymentTransaction
{
	public class ARPaymentAfterProcessingManager : AfterProcessingManager
	{
		public bool ReleaseDoc { get; set; }

		public bool RaisedVoidForReAuthorization { get; set; }

		public ARPaymentEntry Graph { get; set; }

		private ARPaymentEntry graphWithOriginDoc;

		private IBqlTable inputTable;
		
		public override void RunAuthorizeActions(IBqlTable table, bool success)
		{
			inputTable = table;
			CCTranType tranType = CCTranType.AuthorizeOnly;
			ARPaymentEntry graph = CreateGraphIfNeeded(table);
			ChangeDocProcessingStatus(graph, tranType, success);
			if (this.RaisedVoidForReAuthorization)
			{
				UpdateDocReAuthFieldsAfterValidationByVoidForReAuth(graph);
			}
			else
			{
				UpdateDocReAuthFieldsAfterAuthorize(graph);
			}

			UpdateARPayment(graph, tranType, success);
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
			ARPaymentEntry graph = CreateGraphIfNeeded(table);
			ChangeDocProcessingStatus(graph, tranType, success);
			if (this.RaisedVoidForReAuthorization)
			{
				UpdateDocReAuthFieldsAfterVoidForReAuth(graph);
			}
			else
			{
				UpdateDocReAuthFieldsAfterVoid(graph);
			}

			UpdateARPayment(graph, tranType, success);
			ARPayment doc = graph.Document.Current;
			if (ReleaseDoc && NeedRelease(doc) && ARPaymentType.VoidAppl(doc.DocType) == true)
			{
				ReleaseDocument(graph, tranType, success);
			}
			RestoreCopy();
		}

		public override void RunCreditActions(IBqlTable table, bool success)
		{
			inputTable = table; 
			CCTranType type = CCTranType.VoidOrCredit;
			ARPaymentEntry graph = CreateGraphIfNeeded(table);
			ChangeDocProcessingStatus(graph, type, success);
			UpdateARPaymentAndSetWarning(graph, type, success);
			ARPayment doc = Graph.Document.Current;
			if (ReleaseDoc && NeedRelease(doc))
			{
				ReleaseDocument(graph, type, success);
			}
			RestoreCopy();
		}

		public override void RunCaptureOnlyActions(IBqlTable table, bool success)
		{
			inputTable = table;
			CCTranType tranType = CCTranType.CaptureOnly;
			ARPaymentEntry graph = CreateGraphIfNeeded(table);
			ChangeDocProcessingStatus(graph, tranType, success);
			UpdateDocReAuthFieldsAfterCapture(graph);
			UpdateExtRefNbrARPayment(graph, tranType, success);
			ARPayment doc = Graph.Document.Current;
			if (ReleaseDoc && NeedRelease(doc))
			{
				ReleaseDocument(graph, tranType, success);
			}
			RestoreCopy();
		}

		public override void RunUnknownActions(IBqlTable table, bool success)
		{
			inputTable = table;
			ARPaymentEntry graph = CreateGraphIfNeeded(table);
			UpdateARPayment(graph, CCTranType.Unknown, success);
			RestoreCopy();
		}

		public bool NeedReleaseForCapture(ARPayment doc)
		{
			bool ret = false;
			try
			{
				ARPaymentEntry.CheckValidPeriodForCCTran(Graph, doc);
				ret = NeedRelease(doc);
			}
			catch { }
			return ret;
		}

		private void RunCaptureActions(IBqlTable table, CCTranType tranType, bool success)
		{
			inputTable = table;
			ARPaymentEntry graph = CreateGraphIfNeeded(table);
			ChangeDocProcessingStatus(graph, tranType, success);
			UpdateDocReAuthFieldsAfterCapture(graph);
			UpdateARPaymentAndSetWarning(graph, tranType, success);
			ARPayment doc = graph.Document.Current;
			if (ReleaseDoc && NeedReleaseForCapture(doc))
			{
				ReleaseDocument(graph, tranType, success);
			}
			if (IsMassProcess)
			{
				CheckForHeldForReviewStatusAfterProc(graph, tranType, success);
			}
			RestoreCopy();
		}

		private bool NeedRelease(ARPayment doc)
		{
			return doc.Released == false && Graph.arsetup.Current.IntegratedCCProcessing == true;
		}

		public void CheckForHeldForReviewStatusAfterProc(ARPaymentEntry paymentEntry, CCTranType procTran, bool success)
		{
			if (success)
			{
				var doc = paymentEntry.Document.Current;
				var query = new PXSelect<ExternalTransaction, Where<ExternalTransaction.docType, Equal<Required<ExternalTransaction.docType>>,
					And<ExternalTransaction.refNbr, Equal<Required<ExternalTransaction.refNbr>>>>, OrderBy<Desc<ExternalTransaction.transactionID>>>(paymentEntry);
				var result = query.Select(doc.DocType, doc.RefNbr);
				ExternalTransactionState state = ExternalTranHelper.GetActiveTransactionState(paymentEntry, result.RowCast<ExternalTransaction>());
				if (state.IsOpenForReview)
				{
					throw new PXSetPropertyException(AR.Messages.CCProcessingTranHeldWarning, PXErrorLevel.RowWarning);
				}
			}
		}

		public void ReleaseDocument(ARPaymentEntry paymentGraph, CCTranType tranType, bool success)
		{
			var doc = paymentGraph.Document.Current;
			paymentGraph.Actions.PressSave();
			if (doc != null && success)
			{
				var tran = paymentGraph.ExternalTran.SelectSingle(doc.DocType, doc.RefNbr);
				if (tran != null)
				{
					ExternalTransactionState state = ExternalTranHelper.GetTransactionState(paymentGraph, tran);
					if (!state.IsDeclined && !state.IsOpenForReview && !state.IsExpired)
					{
						PaymentTransactionGraph<ARPaymentEntry, ARPayment>.ReleaseARDocument(doc);
					}
				}
			}
		}

		public void UpdateARPaymentAndSetWarning(ARPaymentEntry paymentGraph, CCTranType tranType, bool success)
		{
			var toProc = paymentGraph.Document.Current;
			if (success && toProc.Released == false)
			{
				IExternalTransaction currTran = paymentGraph.ExternalTran.SelectSingle();
				ExternalTransactionState state = ExternalTranHelper.GetTransactionState(paymentGraph, currTran);
				if (currTran != null)
				{
					if (state.IsActive)
					{
						if (toProc.AdjDate != null && !(PXLongOperation.GetCustomInfo() is PXProcessingInfo))
						{
							PXLongOperation.SetCustomInfo(new DocDateWarningDisplay(toProc.AdjDate.Value));
						}
						toProc.DocDate = currTran.LastActivityDate.Value.Date;
						toProc.AdjDate = currTran.LastActivityDate.Value.Date;
					}

					SetExtRefNbrValue(paymentGraph, toProc, currTran, state);

					paymentGraph.Document.Update(toProc);
				}
			}
		}

		public void UpdateARPayment(ARPaymentEntry paymentGraph, CCTranType tranType, bool success)
		{
			ARPayment toProc = paymentGraph.Document.Current;
			if (success && toProc.Released == false)
			{
				IExternalTransaction currTran = paymentGraph.ExternalTran.SelectSingle();
				if (currTran != null)
				{
					ExternalTransactionState state = ExternalTranHelper.GetTransactionState(paymentGraph, currTran);
					toProc.DocDate = currTran.LastActivityDate.Value.Date;
					toProc.AdjDate = currTran.LastActivityDate.Value.Date;

					SetExtRefNbrValue(paymentGraph, toProc, currTran, state);

					paymentGraph.Document.Update(toProc);
				}
			}
		}


		public void ChangeDocProcessingStatus(ARPaymentEntry paymentGraph, CCTranType tranType, bool success)
		{
			var extTran = paymentGraph.ExternalTran.SelectSingle();
			ARPayment payment = paymentGraph.Document.Current;
			if (extTran == null) return;
			
			ExternalTransactionState state = ExternalTranHelper.GetTransactionState(paymentGraph, extTran);
			ChangeCaptureFailedFlag(state, payment);
			if (success)
			{
				bool pendingProcessing = true;
				bool syncNotNeeded = !state.IsOpenForReview && !state.NeedSync && !state.CreateProfile;
				if (state.IsCaptured && syncNotNeeded)
				{
					pendingProcessing = false;
				}
				if ((payment.DocType == ARDocType.VoidPayment || payment.DocType == ARDocType.Refund)
					&& (state.IsVoided || state.IsRefunded) && syncNotNeeded)
				{
					pendingProcessing = false;
				}
				ChangeDocProcessingFlags(state, payment, tranType);
				payment.PendingProcessing = pendingProcessing;
			}
			ChangeOriginDocProcessingStatus(paymentGraph, tranType, success);
			payment = SyncActualExternalTransation(paymentGraph, payment);
			paymentGraph.Document.Update(payment);
		}

		private void ChangeOriginDocProcessingStatus(ARPaymentEntry paymentGraph, CCTranType tranType, bool success)
		{
			IExternalTransaction tran = paymentGraph.ExternalTran.SelectSingle();
			ARPayment payment = paymentGraph.Document.Current;
			ExternalTransactionState tranState = ExternalTranHelper.GetTransactionState(paymentGraph, tran);
			var oPaymentGraph = GetGraphWithOriginDoc(paymentGraph, tranType);
			if (oPaymentGraph != null)
			{
				var oExtTran = oPaymentGraph.ExternalTran.SelectSingle();
				ARPayment oPayment = oPaymentGraph.Document.Current;
				if (oExtTran.TransactionID == tran.TransactionID)
				{
					ChangeCaptureFailedFlag(tranState, oPayment);
					if (success)
					{
						ChangeDocProcessingFlags(tranState, oPayment, tranType);
					}
				}
				paymentGraph.Caches[typeof(ARPayment)].Update(oPayment);
			}
		}

		private void UpdateExtRefNbrARPayment(PXGraph graph, CCTranType tranType, bool success)
		{
			var paymentGraph = graph as ARPaymentEntry;
			ARPayment doc = (ARPayment)paymentGraph.Document.Current;
			if (success && doc.Released == false)
			{
				IExternalTransaction currTran = paymentGraph.ExternalTran.SelectSingle();
				ExternalTransactionState state = ExternalTranHelper.GetTransactionState(paymentGraph, currTran);
				if (currTran != null)
				{
					SetExtRefNbrValue(paymentGraph, doc, currTran, state);
				}
				paymentGraph.Document.Update(doc);
			}
		}

		private void SetExtRefNbrValue(ARPaymentEntry graph, ARPayment doc, IExternalTransaction currTran, ExternalTransactionState state)
		{
			if (state.IsActive)
			{
				graph.Document.Cache.SetValue<ARPayment.extRefNbr>(doc, currTran.TranNumber);
			}
		}

		private void ChangeCaptureFailedFlag(ExternalTransactionState state, ARPayment doc)
		{
			if (doc.IsCCCaptureFailed == false
				&& (state.ProcessingStatus == ProcessingStatus.CaptureFail || state.ProcessingStatus == ProcessingStatus.CaptureDecline))
			{
				doc.IsCCCaptureFailed = true;
			}
			else if (doc.IsCCCaptureFailed == true && (state.IsCaptured || state.IsVoided || 
				(state.IsPreAuthorized && !state.HasErrors && CheckCaptureFailedExists(state))))
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

		private void ChangeDocProcessingFlags(ExternalTransactionState tranState, ARPayment doc, CCTranType tranType)
		{
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

		private ARPaymentEntry GetGraphWithOriginDoc(ARPaymentEntry graph, CCTranType tranType)
		{
			if (graphWithOriginDoc != null)
			{
				return graphWithOriginDoc; 
			}
			IExternalTransaction tran = graph.ExternalTran.SelectSingle();
			ARPayment payment = graph.Document.Current;
			if (tranType == CCTranType.VoidOrCredit
				&& tran.DocType == payment.OrigDocType && tran.RefNbr == payment.OrigRefNbr
				&& (tran.DocType == ARDocType.Payment || tran.DocType == ARDocType.Prepayment))
			{
				graphWithOriginDoc = GetGraphByDocTypeRefNbr(tran.DocType, tran.RefNbr);
			}
			return graphWithOriginDoc;
		}

		private ARPaymentEntry GetGraphByDocTypeRefNbr(string docType, string refNbr)
		{
			var paymentGraph = PXGraph.CreateInstance<ARPaymentEntry>();

			paymentGraph.RowSelecting.RemoveHandler<ARPayment>(paymentGraph.ARPayment_RowSelecting);
			paymentGraph.FieldUpdating.RemoveHandler<SOAdjust.curyDocBal>(paymentGraph.SOAdjust_CuryDocBal_FieldUpdating);

			paymentGraph.Document.Current = PXSelect<ARPayment, Where<ARPayment.docType, Equal<Required<ARPayment.docType>>,
				And<ARPayment.refNbr, Equal<Required<ARPayment.refNbr>>>>>
					.Select(paymentGraph, docType, refNbr);

			return paymentGraph;
		}

		public override void PersistData()
		{
			ARPayment doc = Graph?.Document.Current;
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

		public override PXGraph GetGraph()
		{
			return Graph;
		}

		protected virtual ARPaymentEntry CreateGraphIfNeeded(IBqlTable table)
		{
			if (Graph == null)
			{
				ARPayment doc = table as ARPayment;
				Graph = GetGraphByDocTypeRefNbr(doc.DocType, doc.RefNbr);
				Graph.Document.Update(doc);
			}
			return Graph;
		}

		protected virtual ICCPaymentProcessingRepository GetPaymentProcessingRepository()
		{
			ICCPaymentProcessingRepository repository = new CCPaymentProcessingRepository(Graph);
			return repository;
		}

		private ARPayment SyncActualExternalTransation(ARPaymentEntry paymentGraph, ARPayment payment)
		{
			ExternalTransaction lastExtTran = paymentGraph.ExternalTran.SelectSingle();

			if (payment.CCActualExternalTransactionID == null)
			{
				payment.CCActualExternalTransactionID = lastExtTran.TransactionID;
				return payment;
			}

			if (lastExtTran.TransactionID > payment.CCActualExternalTransactionID)
			{
				ExternalTransaction currentActualExtTran = paymentGraph.ExternalTran
																				.Select()
																				.SingleOrDefault(t =>
																				((ExternalTransaction)t).TransactionID == payment.CCActualExternalTransactionID);

				if(lastExtTran == null || currentActualExtTran == null) return payment;

				ExternalTransactionState stateOfLastExtTran = ExternalTranHelper.GetTransactionState(paymentGraph, lastExtTran);
				ExternalTransactionState stateOfActualExtTran = ExternalTranHelper.GetTransactionState(paymentGraph, currentActualExtTran);

				if (!stateOfActualExtTran.IsActive || stateOfLastExtTran.IsActive)
				{
					payment.CCActualExternalTransactionID = lastExtTran.TransactionID;
				}
			}

			return payment;
		}

		private void RestoreCopy()
		{
			ARPayment doc = Graph?.Document.Current;
			if (doc != null && inputTable != null)
			{
				Graph.Document.Cache.RestoreCopy(inputTable, doc);
			}
		}

		#region Re-authorization of expired transactions 
		private void UpdateDocReAuthFieldsAfterAuthorize(ARPaymentEntry paymentGraph)
		{
			ARPayment payment = paymentGraph.Document.Current;
			ExternalTransactionState tranState = GetStateOfLastExternalTransaction(paymentGraph);

			if (tranState?.IsPreAuthorized == true || payment.CCReauthDate == null)
			{
				ExcludeFromReAuthProcess(paymentGraph, payment);
			}
			else
			{
				HandleUnsuccessfulAttemptOfReauth(paymentGraph, payment);
			}
		}

		private void UpdateDocReAuthFieldsAfterCapture(ARPaymentEntry paymentGraph)
		{
			ExternalTransactionState tranState = GetStateOfLastExternalTransaction(paymentGraph);
			
			if (tranState?.IsCaptured == true)
			{
				ARPayment payment = paymentGraph.Document.Current;
				ExcludeFromReAuthProcess(paymentGraph, payment);
			}
		}

		private void UpdateDocReAuthFieldsAfterVoid(ARPaymentEntry graph)
		{
			ExternalTransactionState tranState = GetStateOfLastExternalTransaction(graph);
			
			if (tranState?.IsVoided == true)
			{
				ARPayment payment = graph.Document.Current;
				ExcludeFromReAuthProcess(graph, payment);
			}
		}
		
		private void UpdateDocReAuthFieldsAfterValidationByVoidForReAuth(ARPaymentEntry paymentGraph)
		{
			ExternalTransactionState tranState = GetStateOfLastExternalTransaction(paymentGraph);

			if (tranState?.IsActive == false)
			{
				ARPayment payment = paymentGraph.Document.Current;
				IncludeToReAuthProcess(paymentGraph, payment);
			}
		}

		private void UpdateDocReAuthFieldsAfterVoidForReAuth(ARPaymentEntry paymentGraph)
		{
			ExternalTransactionState tranState = GetStateOfLastExternalTransaction(paymentGraph);
			
			if (tranState?.IsVoided == true)
			{
				ARPayment payment = paymentGraph.Document.Current;
				IncludeToReAuthProcess(paymentGraph, payment);
			}
		}

		private ExternalTransactionState GetStateOfLastExternalTransaction(ARPaymentEntry paymentGraph)
		{
			IExternalTransaction tran = paymentGraph.ExternalTran.SelectSingle();
			if (tran == null) return null;
			
			ExternalTransactionState tranState = ExternalTranHelper.GetTransactionState(paymentGraph, tran);
			return tranState;
		}
		
		private void HandleUnsuccessfulAttemptOfReauth(ARPaymentEntry paymentGraph, ARPayment payment)
		{
			ICCPaymentProcessingRepository repository = CCPaymentProcessingRepository.GetCCPaymentProcessingRepository();
			var processingCenter = repository.GetCCProcessingCenter(payment.ProcessingCenterID);
			
			payment.CCReauthTriesLeft -= 1;
			
			if (payment.CCReauthTriesLeft > 0)
			{
				payment.CCReauthDate = PXTimeZoneInfo.Now.AddHours(processingCenter.ReauthRetryDelay.Value);
				paymentGraph.Document.Update(payment);
			}
			else
			{
				ExcludeFromReAuthProcess(paymentGraph, payment);
			}
		}
		
		private void ExcludeFromReAuthProcess(ARPaymentEntry paymentGraph, ARPayment payment)
		{
			payment.CCReauthDate = null;
			payment.CCReauthTriesLeft = 0;
			paymentGraph.Document.Update(payment);
		}
		
		private void IncludeToReAuthProcess(ARPaymentEntry paymentGraph, ARPayment payment)
		{
			ICCPaymentProcessingRepository repository = CCPaymentProcessingRepository.GetCCPaymentProcessingRepository();
			var processingCenter = repository.GetCCProcessingCenter(payment.ProcessingCenterID);
			var processingCenterPmntMethod = GetProcessingCenterPmntMethod(paymentGraph, payment);

			payment.CCReauthDate = PXTimeZoneInfo.Now.AddHours(processingCenterPmntMethod.ReauthDelay.Value);
			payment.CCReauthTriesLeft = processingCenter.ReauthRetryNbr;
			paymentGraph.Document.Update(payment);
		}
		
		private CCProcessingCenterPmntMethod GetProcessingCenterPmntMethod(ARPaymentEntry paymentGraph, ARPayment payment)
		{
			var query = new SelectFrom<CCProcessingCenterPmntMethod>
							.Where<CCProcessingCenterPmntMethod.paymentMethodID.IsEqual<@P.AsString>
								.And<CCProcessingCenterPmntMethod.processingCenterID.IsEqual<@P.AsString>>>
							.View(paymentGraph);
			
			var result = query.Select(payment.PaymentMethodID, payment.ProcessingCenterID);
			
			foreach (PXResult<CCProcessingCenterPmntMethod> processingCenterPmntMethod in result)
			{
				return processingCenterPmntMethod;
			}
			return null;
		}
		#endregion
	}
}
