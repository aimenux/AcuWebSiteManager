using PX.CCProcessingBase;
using PX.Common;
using PX.Data;
using PX.Objects.Common;
using System;
using System.Collections.Generic;
using System.Text;
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

		public void AuthorizeCCpayment(ICCPayment doc, ICCPaymentTransactionAdapter paymentTransaction)
		{
			if (doc == null || doc.PMInstanceID == null || doc.PMInstanceID == PaymentTranExtConstants.NewPaymentProfile 
				|| doc.CuryDocBal == null)
				return;

			IEnumerable<ICCPaymentTransaction> trans = paymentTransaction.Select();
			if (CCProcTranHelper.HasOpenCCTran(paymentTransaction.Select()))
			{
				throw new PXException(Messages.ERR_CCTransactionCurrentlyInProgress);
			}
			CCPaymentState paymentState = CCProcTranHelper.ResolveCCPaymentState(trans);
			if (paymentState.HasFlag(CCPaymentState.Captured | CCPaymentState.PreAuthorized))
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
					TransactionProcessor.ProcessCCTransaction(toProc, (ICCPaymentTransaction)null, CCTranType.AuthorizeOnly);
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

		public void CaptureCCpayment(ICCPayment doc, ICCPaymentTransactionAdapter paymentTransaction)
		{
			if (doc == null || doc.PMInstanceID == null || doc.PMInstanceID == PaymentTranExtConstants.NewPaymentProfile 
				|| doc.CuryDocBal == null)
				return;
			IEnumerable<ICCPaymentTransaction> trans = paymentTransaction.Select();
			if (CCProcTranHelper.HasOpenCCTran(trans))
				throw new PXException(Messages.ERR_CCTransactionCurrentlyInProgress);
			CCPaymentState paymentState = CCProcTranHelper.ResolveCCPaymentState(trans);
			if (paymentState.HasFlag(CCPaymentState.Captured))
			{
				throw new PXException(Messages.ERR_CCAuthorizedPaymentAlreadyCaptured);
			}
			if (doc.Released == false)
			{
				graph.Actions.PressSave();
			}
			ICCPaymentTransaction authTran = CCProcTranHelper.FindCCPreAuthorizing(trans);
			ICCPayment toProc = graph.Caches[doc.GetType()].CreateCopy(doc) as ICCPayment;
			ICCPaymentTransaction authTranCopy = null;
			if (authTran != null && !CCProcTranHelper.IsExpired(authTran))
				authTranCopy = graph.Caches[authTran.GetType()].CreateCopy(authTran) as ICCPaymentTransaction;
			CCTranType operation = (authTranCopy) != null ? CCTranType.PriorAuthorizedCapture : CCTranType.AuthorizeAndCapture;
			PXLongOperation.StartOperation(graph, delegate ()
			{
				bool success = true;
				try
				{
					TransactionProcessor.ProcessCCTransaction(toProc, authTranCopy, operation);
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

		public void CaptureOnlyCCPayment(InputPaymentInfo paymentInfo, ICCPayment doc, ICCPaymentTransactionAdapter paymentTransaction)
		{
			if (doc == null || doc.PMInstanceID == null || doc.PMInstanceID == PaymentTranExtConstants.NewPaymentProfile 
				|| doc.CuryDocBal == null)
			{
				return;
			}
			IEnumerable<ICCPaymentTransaction> trans = paymentTransaction.Select();
			if (CCProcTranHelper.HasOpenCCTran(trans))
				throw new PXException(Messages.ERR_CCTransactionCurrentlyInProgress);
			if (string.IsNullOrEmpty(paymentInfo.AuthNumber))
				throw new PXException(Messages.ERR_CCExternalAuthorizationNumberIsRequiredForCaptureOnlyTrans);
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
					ICCPaymentTransaction refTran = new CCProcTran();
					refTran.AuthNumber = paymentInfo.AuthNumber;
					TransactionProcessor.ProcessCCTransaction(toProc, refTran, CCTranType.CaptureOnly);
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

		public void VoidCCPayment(ICCPayment doc, ICCPaymentTransactionAdapter paymentTransaction)
		{
			if (doc == null || doc.PMInstanceID == null || doc.PMInstanceID == PaymentTranExtConstants.NewPaymentProfile 
				|| doc.CuryDocBal == null)
			{
				return;
			}
			IEnumerable<ICCPaymentTransaction> trans = paymentTransaction.Select();
			if (CCProcTranHelper.HasOpenCCTran(trans))
				throw new PXException(Messages.ERR_CCTransactionCurrentlyInProgress);
			ICCPaymentTransaction toVoid = CCProcTranHelper.FindCCLastSuccessfulTran(trans);
			if (toVoid == null)
			{
				throw new PXException(Messages.ERR_CCNoTransactionToVoid);
			}
			else if (toVoid.TranType == CCTranTypeCode.VoidTran || toVoid.TranType == CCTranTypeCode.Credit)
			{
				throw new PXException(Messages.ERR_CCTransactionOfThisTypeInvalidToVoid);
			}

			if (CCProcTranHelper.IsExpired(toVoid))
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
					TransactionProcessor.ProcessCCTransaction(toProc, toVoid, CCTranType.VoidOrCredit);
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

		public void CreditCCPayment(ICCPayment doc, ICCPaymentTransactionAdapter paymentTransaction)
		{
			if (doc == null || doc.PMInstanceID == null || doc.PMInstanceID == PaymentTranExtConstants.NewPaymentProfile 
				|| doc.CuryDocBal == null)
			{
				return;
			}
			IEnumerable<ICCPaymentTransaction> trans = paymentTransaction.Select();
			if (CCProcTranHelper.HasOpenCCTran(trans))
				throw new PXException(Messages.ERR_CCTransactionCurrentlyInProgress);
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
					ICCPaymentTransaction refTran = new CCProcTran();
					refTran.TranNbr = null;
					refTran.PCTranNumber = doc.RefTranExtNbr;
					TransactionProcessor.ProcessCCTransaction(toProc, refTran, CCTranType.Credit);
				}
				catch
				{
					success = false;
					throw;
				}
				finally
				{
					RunCallbacks((IBqlTable)doc,CCTranType.VoidOrCredit, success);
				}
			});
		}

		public void RecordCCpayment(ICCPayment doc, ICCManualInputPaymentInfo info, ICCPaymentTransactionAdapter paymentTransaction)
		{
			if (doc == null || doc.PMInstanceID == null || doc.PMInstanceID == PaymentTranExtConstants.NewPaymentProfile 
				|| doc.CuryDocBal == null)
			{
				return;
			}
			IEnumerable<ICCPaymentTransaction> trans = paymentTransaction.Select();
			if (CCProcTranHelper.HasOpenCCTran(trans))
				throw new PXException(Messages.ERR_CCTransactionCurrentlyInProgress);

			if (string.IsNullOrEmpty(info.PCTranNumber))
				throw new PXException(Messages.ERR_PCTransactionNumberOfTheOriginalPaymentIsRequired);
			CCPaymentState paymentState = CCProcTranHelper.ResolveCCPaymentState(trans);
			if (paymentState.HasFlag(CCPaymentState.Captured))
			{
				throw new PXException(Messages.ERR_CCAuthorizedPaymentAlreadyCaptured);
			}
			if (doc.Released == false)
			{
				graph.Actions.PressSave();
			}
			ICCPayment toProc = graph.Caches[doc.GetType()].CreateCopy(doc) as ICCPayment;
			ICCPaymentTransaction authTran = CCProcTranHelper.FindCCPreAuthorizing(trans);
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
					RunCallbacks((IBqlTable)doc,operation,success);
				}
			});
		}

		public void RecordCCCredit(ICCPayment doc, ICCManualInputPaymentInfo info, ICCPaymentTransactionAdapter paymentTransaction)
		{
			if (doc == null || doc.PMInstanceID == null || doc.PMInstanceID == PaymentTranExtConstants.NewPaymentProfile 
				|| doc.CuryDocBal == null)
			{
				return;
			}
			IEnumerable<ICCPaymentTransaction> trans = paymentTransaction.Select();
			if (CCProcTranHelper.HasOpenCCTran(trans))
			{
				throw new PXException(Messages.ERR_CCTransactionCurrentlyInProgress);
			}
			if (string.IsNullOrEmpty(info.PCTranNumber))
			{
				throw new PXException(Messages.ERR_PCTransactionNumberOfTheOriginalPaymentIsRequired);
			}
			CCPaymentState paymentState = CCProcTranHelper.ResolveCCPaymentState(trans);
			if (paymentState.HasFlag(CCPaymentState.Refunded))
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
					int? tranID;
					procGraph.RecordCredit(doc, toProc.RefTranExtNbr, info.PCTranNumber, info.AuthNumber, out tranID);
				}
				catch
				{
					success = false;
					throw;
				}
				finally
				{ 
					RunCallbacks((IBqlTable)doc,operation,success);
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
