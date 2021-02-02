using PX.Data;
using System.Collections.Generic;
using PX.Objects.AR.CCPaymentProcessing.Common;
using PX.Objects.AR.CCPaymentProcessing.Helpers;
using PX.Objects.AR.CCPaymentProcessing.Interfaces;
using PX.Objects.Extensions.PaymentTransaction;

namespace PX.Objects.AR.CCPaymentProcessing
{
	public delegate void AfterTranProcDelegate(IBqlTable aTable, CCTranType tranType, bool success);
	public class CCPaymentEntry
	{
		PXGraph graph;
		List<AfterTranProcDelegate> afterProcessCallbacks;

		ICCTransactionsProcessor transactionProcessor;

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

		public CCPaymentEntry(PXGraph graph)
		{
			this.graph = graph;
			afterProcessCallbacks = new List<AfterTranProcDelegate>();
		}

		public void AddAfterProcessCallback(AfterTranProcDelegate callback)
		{
			if (callback != null)
			{
				afterProcessCallbacks.Add(callback);
			}
		}

		public void ClearAfterProcessCallbacks()
		{
			afterProcessCallbacks.Clear();
		}

		public IEnumerable<AfterTranProcDelegate> GetAfterProcessCallbacks()
		{
			foreach (var callback in afterProcessCallbacks)
				yield return callback;
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
					RunCallbacks((IBqlTable)doc, CCTranType.AuthorizeOnly, success);
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
					RunCallbacks((IBqlTable)doc, operation, success);
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
					RunCallbacks((IBqlTable)toProc, CCTranType.CaptureOnly, success);
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
					RunCallbacks((IBqlTable)doc, CCTranType.VoidOrCredit, success);
				}
			});
		}

		public void CreditCCPayment(ICCPayment doc, IExternalTransactionAdapter paymentTransaction)
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
					TransactionProcessor.ProcessCredit(toProc, tran);
				}
				catch
				{
					success = false;
					throw;
				}
				finally
				{
					RunCallbacks((IBqlTable)doc, CCTranType.VoidOrCredit, success);
				}
			});
		}

		public void RecordCCpayment(ICCPayment doc, ICCManualInputPaymentInfo info, IExternalTransactionAdapter paymentTransaction)
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
			if (string.IsNullOrEmpty(info.PCTranNumber))
			{
				throw new PXException(Messages.ERR_PCTransactionNumberOfTheOriginalPaymentIsRequired);
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
			CCTranType operation = CCTranType.AuthorizeAndCapture;
			PXLongOperation.StartOperation(graph, delegate ()
			{
				bool success = true;
				try
				{
					var procGraph = PXGraph.CreateInstance<CCPaymentProcessingGraph>();
					TranRecordData tranRecord = new TranRecordData();
					tranRecord.ExternalTranId = info.PCTranNumber;
					tranRecord.AuthCode = info.AuthNumber;
					tranRecord.ResponseText = Messages.ImportedExternalCCTransaction;
					procGraph.RecordCapture(doc,tranRecord);
				}
				catch
				{
					success = false;
					throw;
				}
				finally
				{
					RunCallbacks((IBqlTable)doc, operation, success);
				}
			});
		}

		public void RecordCCCredit(ICCPayment doc, ICCManualInputPaymentInfo info, IExternalTransactionAdapter paymentTransaction)
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
			if (string.IsNullOrEmpty(info.PCTranNumber))
			{
				throw new PXException(Messages.ERR_PCTransactionNumberOfTheOriginalPaymentIsRequired);
			}
			if (state.IsRefunded)
			{
				throw new PXException(Messages.ERR_CCPaymentIsAlreadyRefunded);
			}

			if (doc.Released == false)
			{
				graph.Actions.PressSave();
			}
			ICCPayment toProc = graph.Caches[doc.GetType()].CreateCopy(doc) as ICCPayment;
			CCTranType operation = CCTranType.Credit;
			PXLongOperation.StartOperation(graph, delegate ()
			{
				bool success = true;
				try
				{
					var procGraph = PXGraph.CreateInstance<CCPaymentProcessingGraph>();
					TranRecordData recordData = new TranRecordData();
					recordData.RefExternalTranId = toProc.RefTranExtNbr;
					recordData.ExternalTranId = info.PCTranNumber;
					recordData.AuthCode = info.AuthNumber;
					procGraph.RecordCredit(doc, recordData);
				}
				catch
				{
					success = false;
					throw;
				}
				finally
				{ 
					RunCallbacks((IBqlTable)doc, operation, success);
				}
			});
		}

		private void RunCallbacks(IBqlTable table, CCTranType tranType, bool success)
		{
			foreach (var callback in afterProcessCallbacks)
			{
				callback(table, tranType, success);
			}
		}
	}
}
