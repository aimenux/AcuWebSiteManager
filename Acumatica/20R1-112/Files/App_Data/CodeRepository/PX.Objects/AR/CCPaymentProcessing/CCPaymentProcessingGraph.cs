using System;
using System.Collections.Generic;
using PX.CCProcessingBase;
using PX.Data;
using PX.Objects.AR.CCPaymentProcessing.Common;
using PX.Objects.AR.CCPaymentProcessing.Wrappers;
namespace PX.Objects.AR.CCPaymentProcessing
{
	public class CCPaymentProcessingGraph : PXGraph<CCPaymentProcessingGraph>, ICCPaymentProcessor
	{
		#region Public Auxiliary Functions
		public virtual bool TestCredentials(PXGraph callerGraph, string processingCenterID)
		{
			return new CCPaymentProcessing(callerGraph).TestCredentials(callerGraph, processingCenterID);
		}	

		public virtual void ValidateSettings(PXGraph callerGraph, string processingCenterID, PluginSettingDetail settingDetail)
		{
			new CCPaymentProcessing(callerGraph).ValidateSettings(callerGraph, processingCenterID, settingDetail);
		}

		public virtual IList<PluginSettingDetail> ExportSettings(PXGraph callerGraph, string processingCenterID)
		{
			return new CCPaymentProcessing(callerGraph).ExportSettings(callerGraph, processingCenterID);
		}
		
		#endregion
		#region Public Processing Functions

		public virtual TranOperationResult Authorize(ICCPayment aPmtInfo, bool aCapture)
		{
			return GetPaymentProcessing().Authorize(aPmtInfo, aCapture);
		}

		public virtual TranOperationResult Capture(ICCPayment payment, int? transactionId)
		{
			return GetPaymentProcessing().Capture(payment, transactionId);
		}

		public virtual TranOperationResult CaptureOnly(ICCPayment aPmtInfo, string aAuthorizationNbr)
		{
			return GetPaymentProcessing().CaptureOnly(aPmtInfo, aAuthorizationNbr);
		}

		public virtual TranOperationResult Void(int? aPMInstanceID, int? aRefTranNbr)
		{
			return GetPaymentProcessing().Void(aPMInstanceID, aRefTranNbr);
		}

		public virtual TranOperationResult VoidOrCredit(int? aPMInstanceID, int? transactionId)
		{
			return GetPaymentProcessing().VoidOrCredit(aPMInstanceID, transactionId);
		}

		public virtual TranOperationResult Credit(ICCPayment aPmtInfo, string aExtRefTranNbr)
		{
			return GetPaymentProcessing().Credit(aPmtInfo, aExtRefTranNbr);
		}

		public virtual TranOperationResult Credit(ICCPayment aPmtInfo, int? transactionId)
		{
			return GetPaymentProcessing().Credit(aPmtInfo, transactionId);
		}

		public virtual TranOperationResult Credit(int? aPMInstanceID, int? transactionId, string aCuryID, decimal? aAmount)
		{
			return GetPaymentProcessing().Credit(aPMInstanceID, transactionId, aCuryID, aAmount);
		}

		public virtual bool RecordAuthorization(ICCPayment payment, TranRecordData data)
		{
			return GetPaymentProcessing().RecordAuthorization(payment, data);
		}

		public virtual bool RecordCapture(ICCPayment payment, TranRecordData data)
		{
			return GetPaymentProcessing().RecordCapture(payment, data);
		}

		public virtual bool RecordPriorAuthorizedCapture(ICCPayment payment, TranRecordData data)
		{
			return GetPaymentProcessing().RecordPriorAuthorizedCapture(payment, data);
		}

		public virtual bool RecordCaptureOnly(ICCPayment payment, TranRecordData data)
		{
			return GetPaymentProcessing().RecordCaptureOnly(payment, data);
		}

		public virtual bool RecordVoid(ICCPayment payment, TranRecordData data)
		{
			return GetPaymentProcessing().RecordVoid(payment, data);
		}

		public virtual bool RecordCredit(ICCPayment aPmtInfo, TranRecordData data)
		{
			return GetPaymentProcessing().RecordCredit(aPmtInfo, data);
		}

		protected virtual CCPaymentProcessing GetPaymentProcessing()
		{
			return new CCPaymentProcessing();
		}
		#endregion
	}
}