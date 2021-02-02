using PX.Data;
using System;
using System.Linq;
using System.Collections.Generic;
using PX.Objects.AR.CCPaymentProcessing.Common;
using PX.Objects.AR.CCPaymentProcessing.Helpers;
using PX.Objects.AR.CCPaymentProcessing.Interfaces;
using PX.Objects.AR.CCPaymentProcessing.Wrappers;
using PX.Objects.AR.CCPaymentProcessing.Repositories;
using PX.Objects.Extensions.PaymentTransaction;

namespace PX.Objects.AR.CCPaymentProcessing
{
	public delegate void AfterTranProcDelegate1(PXGraph graph, CCTranType tranType, bool success);
	public delegate void AfterTranProcDelegate(PXGraph graph, IBqlTable aTable, CCTranType tranType, bool success);
	public class CCPaymentEntry
	{
		PXGraph graph;

		ICCTransactionsProcessor transactionProcessor;

		public AfterProcessingManager AfterProcessingManager { get; set; }

		ICCTransactionsProcessor TransactionProcessor {
			get
			{
				if (transactionProcessor == null)
				{
					transactionProcessor = CCTransactionsProcessor.GetCCTransactionsProcessor();
				}
				return transactionProcessor;
			}
			set
			{
				transactionProcessor = value;
			}
		}

		public bool NeedPersistAfterRecord { get; set; } = true;

		public CCPaymentEntry(PXGraph graph)
		{
			this.graph = graph;
		}

		public void AuthorizeCCpayment(ICCPayment doc, IExternalTransactionAdapter paymentTransaction)
		{
			if (doc == null || doc.CuryDocBal == null)
				return;

			ExternalTransactionState state = ExternalTranHelper.GetActiveTransactionState(graph, paymentTransaction.Select());
			if (ExternalTranHelper.HasOpenCCProcTran(graph, state.ExternalTransaction))
			{
				throw new PXException(Messages.ERR_CCTransactionCurrentlyInProgress);
			}
			if (state.IsCaptured || state.IsPreAuthorized)
			{
				throw new PXException(Messages.ERR_CCPaymentAlreadyAuthorized);
			}
			if (doc.Released == false)
			{
				graph.Actions.PressSave();
			}

			ICCPayment toProc = graph.Caches[doc.GetType()].CreateCopy(doc) as ICCPayment;
			PXLongOperation.StartOperation(graph, delegate ()
			{
				bool success = true;
				try
				{
					TransactionProcessor.ProcessAuthorize(toProc, null);
				}
				catch
				{
					success = false;
					throw;
				}
				finally
				{
					if (AfterProcessingManager != null)
					{
						AfterProcessingManager.RunAuthorizeActions((IBqlTable)doc, success);
						AfterProcessingManager.PersistData();
					}
				}
			});
		}

		public void CaptureCCpayment(ICCPayment doc, IExternalTransactionAdapter paymentTransaction)
		{
			if (doc == null || doc.CuryDocBal == null)
				return;

			ExternalTransactionState state = ExternalTranHelper.GetActiveTransactionState(graph, paymentTransaction.Select());
			if (ExternalTranHelper.HasOpenCCProcTran(graph, state.ExternalTransaction))
			{
				throw new PXException(Messages.ERR_CCTransactionCurrentlyInProgress);
			}
			if (state.IsCaptured)
			{
				throw new PXException(Messages.ERR_CCAuthorizedPaymentAlreadyCaptured);
			}
			if (doc.Released == false)
			{
				graph.Actions.PressSave();
			}
	
			ICCPayment toProc = graph.Caches[doc.GetType()].CreateCopy(doc) as ICCPayment;
			IExternalTransaction tranCopy = null;
			if (state.IsPreAuthorized && !ExternalTranHelper.IsExpired(state.ExternalTransaction))
			{
				tranCopy = graph.Caches[state.ExternalTransaction.GetType()].CreateCopy(state.ExternalTransaction) as IExternalTransaction;
			}
			CCTranType operation = tranCopy != null ? CCTranType.PriorAuthorizedCapture : CCTranType.AuthorizeAndCapture;
			PXLongOperation.StartOperation(graph, delegate ()
			{
				bool success = true;
				try
				{
					if (operation == CCTranType.PriorAuthorizedCapture)
					{
						TransactionProcessor.ProcessPriorAuthorizedCapture(toProc, tranCopy);
					}
					else
					{
						TransactionProcessor.ProcessAuthorizeCapture(toProc, tranCopy);
					}
				}
				catch
				{
					success = false;
					throw;
				}
				finally
				{
					if (AfterProcessingManager != null)
					{
						if (operation == CCTranType.PriorAuthorizedCapture)
						{
							AfterProcessingManager.RunPriorAuthorizedCaptureActions((IBqlTable)doc, success);
						}
						else
						{
							AfterProcessingManager.RunCaptureActions((IBqlTable)doc, success);
						}
						AfterProcessingManager.PersistData();
					}
				}
			});
		}

		public void CaptureOnlyCCPayment(InputPaymentInfo paymentInfo, ICCPayment doc, IExternalTransactionAdapter paymentTransaction)
		{
			if (doc == null || doc.CuryDocBal == null)
			{
				return;
			}

			IExternalTransaction extTran = ExternalTranHelper.GetActiveTransaction(paymentTransaction.Select());
			if (ExternalTranHelper.HasOpenCCProcTran(graph, extTran))
			{
				throw new PXException(Messages.ERR_CCTransactionCurrentlyInProgress);
			}
			if (string.IsNullOrEmpty(paymentInfo.AuthNumber))
			{
				throw new PXException(Messages.ERR_CCExternalAuthorizationNumberIsRequiredForCaptureOnlyTrans);
			}
			if (doc.Released == false)
			{
				graph.Actions.PressSave();
			}
			ICCPayment toProc = graph.Caches[doc.GetType()].CreateCopy(doc) as ICCPayment;
			PXLongOperation.StartOperation(graph, delegate ()
			{
				bool success = true;
				try
				{
					IExternalTransaction tran = new ExternalTransaction();
					tran.AuthNumber = paymentInfo.AuthNumber;
					TransactionProcessor.ProcessCaptureOnly(toProc, tran);
				}
				catch
				{
					success = false;
					throw;
				}
				finally
				{
					if (AfterProcessingManager != null)
					{
						AfterProcessingManager.RunCaptureOnlyActions((IBqlTable)doc, success);
						AfterProcessingManager.PersistData();
					}
				}
			});
		}

		public void VoidCCPayment(ICCPayment doc, IExternalTransactionAdapter paymentTransaction)
		{
			if (doc == null || doc.CuryDocBal == null)
			{
				return;
			}
			ExternalTransactionState state = ExternalTranHelper.GetActiveTransactionState(graph, paymentTransaction.Select());
			if (ExternalTranHelper.HasOpenCCProcTran(graph, state.ExternalTransaction))
			{
				throw new PXException(Messages.ERR_CCTransactionCurrentlyInProgress);
			}
			if (!state.IsActive)
			{
				throw new PXException(Messages.ERR_CCNoTransactionToVoid);
			}

			if (state.IsRefunded)
			{
				throw new PXException(Messages.ERR_CCTransactionOfThisTypeInvalidToVoid);
			}

			if (ExternalTranHelper.IsExpired(state.ExternalTransaction))
			{
				throw new PXException(Messages.TransactionHasExpired);
			}
			if (doc.Released == false)
			{
				graph.Actions.PressSave();
			}
			ICCPayment toProc = graph.Caches[doc.GetType()].CreateCopy(doc) as ICCPayment;
			PXLongOperation.StartOperation(graph, delegate ()
			{
				bool success = true;
				try
				{
					TransactionProcessor.ProcessVoidOrCredit(toProc, state.ExternalTransaction);
				}
				catch
				{
					success = false;
					throw;
				}
				finally
				{
					if (AfterProcessingManager != null)
					{
						AfterProcessingManager.RunVoidActions((IBqlTable)doc, success);
						AfterProcessingManager.PersistData();
					}
				}
			});
		}

		public void CreditCCPayment(ICCPayment doc, IExternalTransactionAdapter paymentTransaction, string processingCenter)
		{
			if (doc == null || doc.CuryDocBal == null)
			{
				return;
			}
			IExternalTransaction extTran = ExternalTranHelper.GetActiveTransaction(paymentTransaction.Select());
			if (ExternalTranHelper.HasOpenCCProcTran(graph, extTran))
			{
				throw new PXException(Messages.ERR_CCTransactionCurrentlyInProgress);
			}
			if (doc.Released == false)
			{
				graph.Actions.PressSave();
			}
			ICCPayment toProc = graph.Caches[doc.GetType()].CreateCopy(doc) as ICCPayment;
			PXLongOperation.StartOperation(graph, delegate ()
			{
				bool success = true;
				try
				{
					IExternalTransaction tran = new ExternalTransaction();
					tran.TranNumber = doc.RefTranExtNbr;
					tran.ProcessingCenterID = processingCenter;
					TransactionProcessor.ProcessCredit(toProc, tran);
				}
				catch
				{
					success = false;
					throw;
				}
				finally
				{
					if (AfterProcessingManager != null)
					{
						AfterProcessingManager.RunCreditActions((IBqlTable)doc, success);
						AfterProcessingManager.PersistData();
					}
				}
			});
		}

		public void RecordVoid(ICCPayment doc, TranRecordData tranRecord)
		{
			bool success = true;
			try
			{
				var procGraph = PXGraph.CreateInstance<CCPaymentProcessingGraph>();
				var repo = new CCPaymentProcessingRepository(graph);
				repo.NeedPersist = this.NeedPersistAfterRecord;
				procGraph.Repository = repo;

				SetResponseTextIfNeeded(tranRecord);
				procGraph.RecordVoid(doc, tranRecord);
			}
			catch
			{
				success = false;
				throw;
			}
			finally
			{
				if (AfterProcessingManager != null)
				{
					AfterProcessingManager.RunVoidActions((IBqlTable)doc, success);
					if (NeedPersistAfterRecord)
					{
						AfterProcessingManager.PersistData();
					}
				}
			}
		}

		public void RecordUnknown(ICCPayment doc, TranRecordData tranRecord)
		{
			bool success = true; 
			try
			{
				var procGraph = PXGraph.CreateInstance<CCPaymentProcessingGraph>();
				var repo = new CCPaymentProcessingRepository(graph);
				repo.NeedPersist = this.NeedPersistAfterRecord;
				procGraph.Repository = repo;

				SetResponseTextIfNeeded(tranRecord);
				procGraph.RecordUnknown(doc, tranRecord);
			}
			catch
			{
				success = false;
				throw;
			}
			finally
			{
				if (AfterProcessingManager != null)
				{
					AfterProcessingManager.RunUnknownActions((IBqlTable)doc, success);
					if (NeedPersistAfterRecord)
					{
						AfterProcessingManager.PersistData();
					}
				}
			}
		}

		public void RecordPriorAuthCapture(ICCPayment doc, TranRecordData tranRecord)
		{
			bool success = true;
			try
			{
				var procGraph = PXGraph.CreateInstance<CCPaymentProcessingGraph>();
				var repo = new CCPaymentProcessingRepository(graph);
				repo.NeedPersist = this.NeedPersistAfterRecord;
				procGraph.Repository = repo;

				SetResponseTextIfNeeded(tranRecord);
				procGraph.RecordPriorAuthorizedCapture(doc, tranRecord);
			}
			catch
			{
				success = false;
				throw;
			}
			finally
			{
				if (AfterProcessingManager != null)
				{
					AfterProcessingManager.RunPriorAuthorizedCaptureActions((IBqlTable)doc, success);
					if (NeedPersistAfterRecord)
					{
						AfterProcessingManager.PersistData();
					}
				}
			}
		}

		public void RecordPriorAuthCapture(ICCPayment doc, TranRecordData tranRecord, IExternalTransactionAdapter paymentTransaction)
		{
			if (doc == null || doc.CuryDocBal == null)
			{
				return;
			}

			ExternalTransactionState state = ExternalTranHelper.GetActiveTransactionState(graph, paymentTransaction.Select());
			CommonRecordChecks(state, tranRecord);
			if (state.IsCaptured)
			{
				throw new PXException(Messages.ERR_CCAuthorizedPaymentAlreadyCaptured);
			}

			RecordPriorAuthCapture(doc, tranRecord);
		}

		public void RecordAuthCapture(ICCPayment doc, TranRecordData tranRecord)
		{
			bool success = true;
			try
			{
				var procGraph = PXGraph.CreateInstance<CCPaymentProcessingGraph>();
				var repo = new CCPaymentProcessingRepository(graph);
				repo.NeedPersist = this.NeedPersistAfterRecord;
				procGraph.Repository = repo;

				SetResponseTextIfNeeded(tranRecord);
				procGraph.RecordCapture(doc, tranRecord);
			}
			catch
			{
				success = false;
				throw;
			}
			finally
			{
				if (AfterProcessingManager != null)
				{
					AfterProcessingManager.RunCaptureActions((IBqlTable)doc, success);
					if (NeedPersistAfterRecord)
					{
						AfterProcessingManager.PersistData();
					}
				}
			}
		}

		public void RecordAuthCapture(ICCPayment doc, TranRecordData tranRecord, IExternalTransactionAdapter paymentTransaction)
		{
			if (doc == null || doc.CuryDocBal == null)
			{
				return;
			}

			ExternalTransactionState state = ExternalTranHelper.GetActiveTransactionState(graph, paymentTransaction.Select());
			CommonRecordChecks(state, tranRecord);
			if (state.IsCaptured)
			{
				throw new PXException(Messages.ERR_CCAuthorizedPaymentAlreadyCaptured);
			}

			RecordAuthCapture(doc, tranRecord);
		}

		public void RecordCaptureOnly(ICCPayment doc, TranRecordData tranRecord, IExternalTransactionAdapter paymentTransaction)
		{
			if (doc == null || doc.CuryDocBal == null)
			{
				return;
			}

			ExternalTransactionState state = ExternalTranHelper.GetActiveTransactionState(graph, paymentTransaction.Select());
			CommonRecordChecks(state, tranRecord);
			if (state.IsCaptured)
			{
				throw new PXException(Messages.ERR_CCAuthorizedPaymentAlreadyCaptured);
			}
			if (doc.Released == false)
			{
				graph.Actions.PressSave();
			}

			RecordCaptureOnly(doc, tranRecord);
		}

		public void RecordCaptureOnly(ICCPayment doc, TranRecordData tranRecord)
		{
			bool success = true;
			try
			{
				var procGraph = PXGraph.CreateInstance<CCPaymentProcessingGraph>();
				var repo = new CCPaymentProcessingRepository(graph);
				repo.NeedPersist = this.NeedPersistAfterRecord;
				procGraph.Repository = repo;

				SetResponseTextIfNeeded(tranRecord);
				procGraph.RecordCaptureOnly(doc, tranRecord);
			}
			catch
			{
				success = false;
				throw;
			}
			finally
			{
				if (AfterProcessingManager != null)
				{
					AfterProcessingManager.RunCaptureOnlyActions((IBqlTable)doc, success);
					if (NeedPersistAfterRecord)
					{
						AfterProcessingManager.PersistData();
					}
				}
			}
		}

		public void RecordAuthorization(ICCPayment doc, TranRecordData tranRecord)
		{
			bool success = true;
			try
			{
				var procGraph = PXGraph.CreateInstance<CCPaymentProcessingGraph>();
				var repo = new CCPaymentProcessingRepository(graph);
				repo.NeedPersist = this.NeedPersistAfterRecord;
				procGraph.Repository = repo;
				SetResponseTextIfNeeded(tranRecord);
				procGraph.RecordAuthorization(doc, tranRecord);
			}
			catch
			{
				success = false;
				throw;
			}
			finally
			{

				if (AfterProcessingManager != null)
				{
					AfterProcessingManager.RunAuthorizeActions((IBqlTable)doc, success);
					if (NeedPersistAfterRecord)
					{
						AfterProcessingManager.PersistData();
					}
				}
			}
		}

		public void RecordAuthorization(ICCPayment doc, TranRecordData tranRecord, IExternalTransactionAdapter paymentTransaction)
		{
			if (doc == null || doc.CuryDocBal == null)
			{
				return;
			}

			ExternalTransactionState state = ExternalTranHelper.GetActiveTransactionState(graph, paymentTransaction.Select());
			CommonRecordChecks(state, tranRecord);
			if (state.IsCaptured)
			{
				throw new PXException(Messages.ERR_CCAuthorizedPaymentAlreadyCaptured);
			}
			if (state.IsPreAuthorized)
			{
				throw new PXException(Messages.ERR_CCPaymentAlreadyAuthorized);
			}

			RecordAuthorization(doc, tranRecord);
		}

		public void RecordCCCredit(ICCPayment doc, TranRecordData tranRecord, IExternalTransactionAdapter paymentTransaction)
		{
			if (doc == null || doc.CuryDocBal == null)
			{
				return;
			}

			ExternalTransactionState state = ExternalTranHelper.GetActiveTransactionState(graph, paymentTransaction.Select());
			CommonRecordChecks(state, tranRecord);
			if (state.IsRefunded)
			{
				throw new PXException(Messages.ERR_CCPaymentIsAlreadyRefunded);
			}

			bool success = true;
			try
			{
				var procGraph = PXGraph.CreateInstance<CCPaymentProcessingGraph>();
				var repo = new CCPaymentProcessingRepository(graph);
				repo.NeedPersist = this.NeedPersistAfterRecord;
				procGraph.Repository = repo;
				tranRecord.RefExternalTranId = doc.RefTranExtNbr;
				SetResponseTextIfNeeded(tranRecord);
				procGraph.RecordCredit(doc, tranRecord);
			}
			catch
			{
				success = false;
				throw;
			}
			finally
			{
				if (AfterProcessingManager != null)
				{
					AfterProcessingManager.RunCreditActions((IBqlTable)doc, success);
					if (NeedPersistAfterRecord)
					{
						AfterProcessingManager.PersistData();
					}
				}
			}
		}

		private void CommonRecordChecks(ExternalTransactionState state, TranRecordData info)
		{
			if (ExternalTranHelper.HasOpenCCProcTran(graph, state.ExternalTransaction))
			{
				throw new PXException(Messages.ERR_CCTransactionCurrentlyInProgress);
			}
			if (string.IsNullOrEmpty(info.ExternalTranId))
			{
				throw new PXException(Messages.ERR_PCTransactionNumberOfTheOriginalPaymentIsRequired);
			}
		}

		private void SetResponseTextIfNeeded(TranRecordData recordData)
		{
			if (recordData.ResponseText == null)
			{
				recordData.ResponseText = Messages.ImportedExternalCCTransaction;
			}
		}
	}
}
