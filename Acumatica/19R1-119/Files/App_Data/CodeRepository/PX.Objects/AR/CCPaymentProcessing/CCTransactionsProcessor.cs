using PX.CCProcessingBase;
using PX.Data;
using PX.Objects.AR.CCPaymentProcessing.Common;
using PX.Objects.AR.CCPaymentProcessing.Interfaces;

namespace PX.Objects.AR.CCPaymentProcessing
{
	public class CCTransactionsProcessor : ICCTransactionsProcessor
	{
		private ICCPaymentProcessor _processingClass;

		protected CCTransactionsProcessor(ICCPaymentProcessor processingClass)
		{
			_processingClass = processingClass;
		}

		public static ICCTransactionsProcessor GetCCTransactionsProcessor()
		{
			return new CCTransactionsProcessor(CCPaymentProcessing.GetCCPaymentProcessing());
		}

		public void ProcessCCTransaction(ICCPayment aDoc, CCProcTran refTran, CCTranType aTranType)
		{
			if (aDoc != null && aDoc.PMInstanceID != null && aDoc.CuryDocBal != null)
			{
				int tranID = 0;
				bool result = false;
				bool processed = false;
				if (aTranType == CCTranType.AuthorizeOnly || aTranType == CCTranType.AuthorizeAndCapture)
				{
					bool doCapture = (aTranType == CCTranType.AuthorizeAndCapture);
					result = _processingClass.Authorize(aDoc, doCapture, ref tranID);
					processed = true;
				}

				if (aTranType == CCTranType.PriorAuthorizedCapture)
				{
					if (refTran == null)
					{
						throw new PXException(Messages.ERR_CCTransactionMustBeAuthorizedBeforeCapturing);
					}
					result = _processingClass.Capture(aDoc.PMInstanceID, refTran.TranNbr, aDoc.CuryID, aDoc.CuryDocBal, ref tranID);
					processed = true;
				}

				if (aTranType == CCTranType.Void)
				{
					if (refTran == null)
					{
						throw new PXException(Messages.ERR_CCOriginalTransactionNumberIsRequiredForVoiding);
					}
					result = _processingClass.Void(aDoc.PMInstanceID, refTran.TranNbr, ref tranID);
					processed = true;
				}

				if (aTranType == CCTranType.VoidOrCredit)
				{
					if (refTran == null)
					{
						throw new PXException(Messages.ERR_CCOriginalTransactionNumberIsRequiredForVoidingOrCrediting);
					}
					result = _processingClass.VoidOrCredit(aDoc.PMInstanceID, refTran.TranNbr, ref tranID);
					processed = true;
				}

				if (aTranType == CCTranType.Credit)
				{
					if (refTran == null)
					{
						throw new PXException(Messages.ERR_CCOriginalTransactionNumberIsRequiredForCrediting);
					}
					if (refTran.TranNbr.HasValue)
					{
						result = _processingClass.Credit(aDoc, refTran.TranNbr.Value, ref tranID);
					}
					else
					{
						result = _processingClass.Credit(aDoc, refTran.PCTranNumber, ref tranID);
					}
					processed = true;
				}

				if (aTranType == CCTranType.CaptureOnly)
				{
					//Uses Authorization Number received from Processing center in a special way (for example, by phone)
					if (refTran == null || string.IsNullOrEmpty(refTran.AuthNumber))
					{
						throw new PXException(Messages.ERR_CCExternalAuthorizationNumberIsRequiredForCaptureOnlyTrans);
					}
					result = _processingClass.CaptureOnly(aDoc, refTran.AuthNumber, ref tranID);
					processed = true;
				}

				if (!processed)
				{
					throw new PXException(Messages.ERR_CCUnknownOperationType);
				}

				if (!result)
				{
					throw new PXException(Messages.ERR_CCTransactionWasNotAuthorizedByProcCenter, tranID);
				}

			}
		}

		public void ProcessCCTransaction(ICCPayment aDoc, ICCPaymentTransaction refTran, CCTranType aTranType)
		{
			if (aDoc != null && aDoc.PMInstanceID != null && aDoc.CuryDocBal != null)
			{
				int tranID = 0;
				bool result = false;
				bool processed = false;
				if (aTranType == CCTranType.AuthorizeOnly || aTranType == CCTranType.AuthorizeAndCapture)
				{
					bool doCapture = (aTranType == CCTranType.AuthorizeAndCapture);
					result = _processingClass.Authorize(aDoc, doCapture, ref tranID);
					processed = true;
				}

				if (aTranType == CCTranType.PriorAuthorizedCapture)
				{
					if (refTran == null)
					{
						throw new PXException(Messages.ERR_CCTransactionMustBeAuthorizedBeforeCapturing);
					}
					result = _processingClass.Capture(aDoc.PMInstanceID, refTran.TranNbr, aDoc.CuryID, aDoc.CuryDocBal, ref tranID);
					processed = true;
				}

				if (aTranType == CCTranType.Void)
				{
					if (refTran == null)
					{
						throw new PXException(Messages.ERR_CCOriginalTransactionNumberIsRequiredForVoiding);
					}
					result = _processingClass.Void(aDoc.PMInstanceID, refTran.TranNbr, ref tranID);
					processed = true;
				}

				if (aTranType == CCTranType.VoidOrCredit)
				{
					if (refTran == null)
					{
						throw new PXException(Messages.ERR_CCOriginalTransactionNumberIsRequiredForVoidingOrCrediting);
					}
					result = _processingClass.VoidOrCredit(aDoc.PMInstanceID, refTran.TranNbr, ref tranID);
					processed = true;
				}

				if (aTranType == CCTranType.Credit)
				{
					if (refTran == null)
					{
						throw new PXException(Messages.ERR_CCOriginalTransactionNumberIsRequiredForCrediting);
					}
					if (refTran.TranNbr.HasValue)
					{
						result = _processingClass.Credit(aDoc, refTran.TranNbr.Value, ref tranID);
					}
					else
					{
						result = _processingClass.Credit(aDoc, refTran.PCTranNumber, ref tranID);
					}
					processed = true;
				}

				if (aTranType == CCTranType.CaptureOnly)
				{
					//Uses Authorization Number received from Processing center in a special way (for example, by phone)
					if (refTran == null || string.IsNullOrEmpty(refTran.AuthNumber))
					{
						throw new PXException(Messages.ERR_CCExternalAuthorizationNumberIsRequiredForCaptureOnlyTrans);
					}
					result = _processingClass.CaptureOnly(aDoc, refTran.AuthNumber, ref tranID);
					processed = true;
				}

				if (!processed)
				{
					throw new PXException(Messages.ERR_CCUnknownOperationType);
				}

				if (!result)
				{
					throw new PXException(Messages.ERR_CCTransactionWasNotAuthorizedByProcCenter, tranID);
				}

			}
		}
	}
}
