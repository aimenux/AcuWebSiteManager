using System;
using System.Collections.Generic;
using System.Net;
using System.Text;
using System.Web.Compilation;
using PX.CCProcessingBase;
using V2 = PX.CCProcessingBase.Interfaces.V2;
using PX.Common;
using PX.Data;
using PX.Objects.AR.CCPaymentProcessing.Common;
using PX.Objects.AR.CCPaymentProcessing.Helpers;
using PX.Objects.CA;
using PX.Objects.AR.CCPaymentProcessing.Repositories;
using PX.Objects.AR.CCPaymentProcessing.Wrappers;

namespace PX.Objects.AR.CCPaymentProcessing
{
	public class CCPaymentProcessing
	{
		#region Private Members		

		private ICCPaymentProcessingRepository _repository;

		public ICCPaymentProcessingRepository Repository => _repository;

		private Func<object, CCProcessingContext, ICardTransactionProcessingWrapper> _transactionProcessingWrapper;

		private Func<object, ICardProcessingReadersProvider, IHostedPaymentFormProcessingWrapper> _hostedPaymentFormProcessingWrapper;
		
		public Func<object, ICardProcessingReadersProvider, IHostedPaymentFormProcessingWrapper> HostedPaymnetFormProcessingWrapper
		{
			get
			{
				if (_hostedPaymentFormProcessingWrapper == null)
				{
					return (plugin, provider) => HostedFromProcessingWrapper.GetPaymentFormProcessingWrapper(plugin, provider, null);
				}
				return _hostedPaymentFormProcessingWrapper;
			}
			set
			{
				_hostedPaymentFormProcessingWrapper = value;
			}
		}
		
		#endregion
		#region Public Members
		public CCPaymentProcessing()
		{
			_repository = CCPaymentProcessingRepository.GetCCPaymentProcessingRepository();
			_transactionProcessingWrapper = (plugin, context) => CardTransactionProcessingWrapper.GetTransactionProcessingWrapper(plugin, new CardProcessingReadersProvider(context));
		}

		public CCPaymentProcessing(PXGraph contextGraph)
		{
			_repository = CCPaymentProcessingRepository.GetCCPaymentProcessingRepository(contextGraph);
			_transactionProcessingWrapper = (plugin, context) => CardTransactionProcessingWrapper.GetTransactionProcessingWrapper(plugin, new CardProcessingReadersProvider(context));
		}

		public CCPaymentProcessing(ICCPaymentProcessingRepository repository,
			Func<object, CCProcessingContext, ICardTransactionProcessingWrapper> processingWrapper)
		{
			_repository = repository;
			_transactionProcessingWrapper = processingWrapper;
		}
		public static ICCPaymentProcessor GetCCPaymentProcessing()
		{
			return PXGraph.CreateInstance<CCPaymentProcessingGraph>();
		}
		#endregion

		#region Public Auxiliary Functions
		public virtual bool TestCredentials(PXGraph callerGraph, string processingCenterID)
		{
			CCProcessingContext context = new CCProcessingContext()
			{
				callerGraph = callerGraph
			};
			CCProcessingCenter processingCenter = _repository.GetCCProcessingCenter(processingCenterID);
			CCProcessingFeatureHelper.CheckProcessing(processingCenter, CCProcessingFeature.Base, context);
			if (context.processingCenter == null)
			{
				throw new PXException(Messages.ERR_CCProcessingCenterNotFound);
			}
			var processor = GetProcessingWrapper(context);
			APIResponse apiResponse = new APIResponse();
			processor.TestCredentials(apiResponse);
			ProcessAPIResponse(apiResponse);
			return apiResponse.isSucess;
		}

		public virtual void ValidateSettings(PXGraph callerGraph, string processingCenterID, ISettingsDetail settingDetail)
		{
			CCProcessingContext context = new CCProcessingContext()
			{
				callerGraph = callerGraph
			};
			CCProcessingCenter processingCenter = _repository.GetCCProcessingCenter(processingCenterID);
			CCProcessingFeatureHelper.CheckProcessing(processingCenter, CCProcessingFeature.Base, context);
			if (context.processingCenter == null)
			{
				throw new PXException(Messages.ERR_CCProcessingCenterNotFound);
			}
			var processor = GetProcessingWrapper(context);
			CCErrors result = processor.ValidateSettings(settingDetail);
			if (result.source != CCErrors.CCErrorSource.None)
			{
				throw new PXSetPropertyException(result.ErrorMessage, PXErrorLevel.Error);
			}
		}

		public virtual IList<ISettingsDetail> ExportSettings(PXGraph callerGraph, string processingCenterID)
		{
			CCProcessingContext context = new CCProcessingContext()
			{
				callerGraph = callerGraph
			};
			CCProcessingCenter processingCenter = _repository.GetCCProcessingCenter(processingCenterID);
			CCProcessingFeatureHelper.CheckProcessing(processingCenter, CCProcessingFeature.Base, context);
			if (context.processingCenter == null)
			{
				throw new PXException(Messages.ERR_CCProcessingCenterNotFound);
			}
			var processor = GetProcessingWrapper(context);
			List<ISettingsDetail> processorSettings = new List<ISettingsDetail>();
			processor.ExportSettings(processorSettings);
			return processorSettings;
		}
		#endregion
		#region Public Processing Functions

		public virtual bool Authorize(ICCPayment aPmtInfo, bool aCapture, ref int aTranNbr)
		{
			Customer customer;
			CustomerPaymentMethod pmInstance = this.findPMInstance(aPmtInfo.PMInstanceID, out customer);
			if (pmInstance == null)
			{
				throw new PXException(Messages.CreditCardWithID_0_IsNotDefined, aPmtInfo.PMInstanceID);
			}
			CCProcessingCenter procCenter = _repository.FindProcessingCenter(pmInstance.PMInstanceID, null);
			if (procCenter == null)
			{
				throw new PXException(Messages.ERR_CCProcessingCenterIsNotSpecified, pmInstance.Descr);
			}
			if (!(procCenter.IsActive ?? false))
			{
				throw new PXException(Messages.ERR_CCProcessingIsInactive, procCenter.ProcessingCenterID);
			}
			if (!IsValidForProcessing(pmInstance, customer)) return false;

			CCTranType tranType = aCapture ? CCTranType.AuthorizeAndCapture : CCTranType.AuthorizeOnly;
			CCProcTran tran = new CCProcTran();
			tran.Copy(aPmtInfo);
			return this.DoTransaction(ref aTranNbr, tranType, tran, null, customer.AcctCD, procCenter);
		}

		public virtual bool Capture(int? aPMInstanceID, int? aAuthTranNbr, string aCuryID, decimal? aAmount, ref int aTranNbr)
		{
			CCProcTran authTran = _repository.GetCCProcTran(aAuthTranNbr);
			Customer customer;
			CustomerPaymentMethod pmInstance = this.findPMInstance(aPMInstanceID, out customer);

			if (authTran == null)
			{
				throw new PXException(Messages.ERR_CCAuthorizationTransactionIsNotFound, aAuthTranNbr);
			}
			if (!IsAuthorized(authTran))
			{
				throw new PXException(Messages.ERR_CCProcessingReferensedTransactionNotAuthorized, aAuthTranNbr);
			}

			if (authTran.ExpirationDate.HasValue && authTran.ExpirationDate.Value < PXTimeZoneInfo.Now)
			{
				throw new PXException(Messages.ERR_CCAuthorizationTransactionHasExpired, aAuthTranNbr);
			}
			if (!IsCCValidForProcessing(aPMInstanceID)) return false;

			CCProcessingCenter procCenter = _repository.GetCCProcessingCenter(pmInstance.CCProcessingCenterID);
			if (procCenter == null)
			{
				throw new PXException(Messages.ERR_CCProcessingCenterUsedForAuthIsNotValid, authTran.ProcessingCenterID, aAuthTranNbr);
			}
			if (!(procCenter.IsActive ?? false))
			{
				throw new PXException(Messages.ERR_CCProcessingCenterUsedForAuthIsNotActive, procCenter, aAuthTranNbr);
			}
			CCProcTran tran = new CCProcTran
			{
				PMInstanceID = aPMInstanceID,
				RefTranNbr = aAuthTranNbr,
				DocType = authTran.DocType,
				RefNbr = authTran.RefNbr,
				CuryID = aCuryID,
				Amount = aAmount,
				OrigDocType = authTran.OrigDocType,
				OrigRefNbr = authTran.OrigRefNbr
			};

			return this.DoTransaction(ref aTranNbr, CCTranType.PriorAuthorizedCapture, tran, authTran, customer.AcctCD, procCenter);
		}

		public virtual bool CaptureOnly(ICCPayment aPmtInfo, string aAuthorizationNbr, ref int aTranNbr)
		{
			Customer customer;
			CustomerPaymentMethod pmInstance = this.findPMInstance(aPmtInfo.PMInstanceID, out customer);
			if (string.IsNullOrEmpty(aAuthorizationNbr))
			{
				throw new PXException(Messages.ERR_CCExternalAuthorizationNumberIsRequiredForCaptureOnlyTrans, aAuthorizationNbr);
			}
			CCProcessingCenter procCenter = _repository.GetCCProcessingCenter(pmInstance.CCProcessingCenterID);
			if (procCenter == null)
			{
				throw new PXException(Messages.ERR_CCProcessingCenterUsedForAuthIsNotValid, pmInstance.CCProcessingCenterID, aTranNbr);
			}
			if (!(procCenter.IsActive ?? false))
			{
				throw new PXException(Messages.ERR_CCProcessingCenterUsedForAuthIsNotActive, procCenter, aTranNbr);
			}
			if (!IsCCValidForProcessing(aPmtInfo.PMInstanceID)) return false;
			CCProcTran tran = new CCProcTran();
			tran.Copy(aPmtInfo);
			CCProcTran refTran = new CCProcTran {AuthNumber = aAuthorizationNbr};
			return this.DoTransaction(ref aTranNbr, CCTranType.CaptureOnly, tran, refTran, customer.AcctCD, procCenter);
		}

		public virtual bool Void(int? aPMInstanceID, int? aRefTranNbr, ref int aTranNbr)
		{
			CCProcTran refTran = _repository.GetCCProcTran(aRefTranNbr);
			Customer customer;
			CustomerPaymentMethod pmInstance = this.findPMInstance(aPMInstanceID, out customer);
			if (refTran == null)
			{
				throw new PXException(Messages.ERR_CCTransactionToVoidIsNotFound, aRefTranNbr);
			}
			if (!MayBeVoided(refTran))
			{
				throw new PXException(Messages.ERR_CCProcessingTransactionMayNotBeVoided, refTran.TranType);
			}
			if (!IsAuthorized(refTran))
			{
				throw new PXException(Messages.ERR_CCProcessingReferensedTransactionNotAuthorized, aRefTranNbr);
			}
			if (!IsCCValidForProcessing(aPMInstanceID)) return false;
			CCProcessingCenter procCenter = _repository.GetCCProcessingCenter(pmInstance.CCProcessingCenterID);
			if (procCenter == null)
			{
				throw new PXException(Messages.ERR_CCProcessingCenterUsedInReferencedTranNotFound, refTran.ProcessingCenterID, aRefTranNbr);
			}
			if (!(procCenter.IsActive ?? false))
			{
				throw new PXException(Messages.ERR_CCProcessingCenterUsedInReferencedTranNotFound, procCenter, aRefTranNbr);
			}

			CCProcTran tran = new CCProcTran
			{
				PMInstanceID = aPMInstanceID,
				RefTranNbr = aRefTranNbr,
				DocType = refTran.DocType,
				RefNbr = refTran.RefNbr,
				CuryID = refTran.CuryID,
				Amount = refTran.Amount,
				OrigDocType = refTran.OrigDocType,
				OrigRefNbr = refTran.OrigRefNbr
			};

			return this.DoTransaction(ref aTranNbr, CCTranType.Void, tran, refTran, customer.AcctCD, procCenter);
		}

		public virtual bool VoidOrCredit(int? aPMInstanceID, int? aRefTranNbr, ref int aTranNbr)
		{
			if (!this.Void(aPMInstanceID, aRefTranNbr, ref aTranNbr))
			{
				CCProcTran refTRan = _repository.GetCCProcTran(aRefTranNbr);
				if (MayBeCredited(refTRan))
					return this.Credit(aPMInstanceID, aRefTranNbr, null, null, ref aTranNbr);
			}
			return true;
		}

		public virtual bool Credit(ICCPayment aPmtInfo, string aExtRefTranNbr, ref int aTranNbr)
		{
			if (!IsCCValidForProcessing(aPmtInfo.PMInstanceID)) return false;

			Customer customer;
			CustomerPaymentMethod pmInstance = this.findPMInstance(aPmtInfo.PMInstanceID, out customer);
			CCProcTran refTran = _repository.FindReferencedCCProcTran(aPmtInfo.PMInstanceID, aExtRefTranNbr);
			if (refTran != null)
			{
				return Credit(aPmtInfo, refTran.TranNbr.Value, ref aTranNbr);
			}
			else
			{
				CCProcTran tran = new CCProcTran();
				tran.Copy(aPmtInfo);
				tran.RefTranNbr = null;
				tran.RefPCTranNumber = aExtRefTranNbr;
				refTran = new CCProcTran();
				refTran.PCTranNumber = aExtRefTranNbr;
				return this.DoTransaction(ref aTranNbr, CCTranType.Credit, tran, refTran, customer.AcctCD, null);
			}
		}

		public virtual bool Credit(ICCPayment aPmtInfo, int aRefTranNbr, ref int aTranNbr)
		{
			Customer customer;
			CustomerPaymentMethod pmInstance = this.findPMInstance(aPmtInfo.PMInstanceID, out customer);
			CCProcTran authTran = _repository.GetCCProcTran(aRefTranNbr);
			if (authTran == null)
			{
				throw new PXException(Messages.ERR_CCTransactionToCreditIsNotFound, aRefTranNbr);
			}
			if (!MayBeCredited(authTran))
			{
				throw new PXException(Messages.ERR_CCProcessingTransactionMayNotBeCredited, authTran.TranType);
			}

			if (!IsAuthorized(authTran))
			{
				throw new PXException(Messages.ERR_CCProcessingReferensedTransactionNotAuthorized, aRefTranNbr);
			}

			if (!IsCCValidForProcessing(aPmtInfo.PMInstanceID)) return false;

			CCProcessingCenter procCenter = _repository.GetCCProcessingCenter(pmInstance.CCProcessingCenterID);
			if (procCenter == null)
			{
				throw new PXException(Messages.ERR_CCProcessingCenterUsedInReferencedTranNotFound, authTran.ProcessingCenterID, aRefTranNbr);
			}

			if (!(procCenter.IsActive ?? false))
			{
				throw new PXException(Messages.ERR_CCProcessingCenterUsedInReferencedTranNotActive, authTran.ProcessingCenterID, aRefTranNbr);
			}
			CCProcTran tran = new CCProcTran();
			tran.Copy(aPmtInfo);
			tran.RefTranNbr = authTran.TranNbr;

			if (!aPmtInfo.CuryDocBal.HasValue)
			{
				tran.CuryID = authTran.CuryID;
				tran.Amount = authTran.Amount;
			}
			return this.DoTransaction(ref aTranNbr, CCTranType.Credit, tran, authTran, customer.AcctCD, procCenter);

		}

		public virtual bool Credit(int? aPMInstanceID, int? aRefTranNbr, string aCuryID, decimal? aAmount, ref int aTranNbr)
		{
			CCProcTran authTran = _repository.GetCCProcTran(aRefTranNbr);
			Customer customer;
			CustomerPaymentMethod pmInstance = this.findPMInstance(aPMInstanceID, out customer);
			if (authTran == null)
			{
				throw new PXException(Messages.ERR_CCTransactionToCreditIsNotFound, aRefTranNbr);
			}
			if (!MayBeCredited(authTran))
			{
				throw new PXException(Messages.ERR_CCProcessingTransactionMayNotBeCredited, authTran.TranType);
			}

			if (!IsAuthorized(authTran))
			{
				throw new PXException(Messages.ERR_CCProcessingReferensedTransactionNotAuthorized, aRefTranNbr);
			}
			if (!IsCCValidForProcessing(aPMInstanceID)) return false;

			CCProcessingCenter procCenter = _repository.GetCCProcessingCenter(pmInstance.CCProcessingCenterID);
			if (procCenter == null)
			{
				throw new PXException(Messages.ERR_CCProcessingCenterUsedInReferencedTranNotFound, authTran.ProcessingCenterID, aRefTranNbr);
			}

			if (!(procCenter.IsActive ?? false))
			{
				throw new PXException(Messages.ERR_CCProcessingCenterUsedInReferencedTranNotActive, authTran.ProcessingCenterID, aRefTranNbr);
			}
			CCProcTran tran = new CCProcTran
			{
				PMInstanceID = aPMInstanceID,
				DocType = authTran.DocType,
				RefNbr = authTran.RefNbr,
				OrigDocType = authTran.OrigDocType,
				OrigRefNbr = authTran.OrigRefNbr,
				RefTranNbr = authTran.TranNbr
			};
			if (aAmount.HasValue)
			{
				tran.CuryID = aCuryID;
				tran.Amount = aAmount;
			}
			else
			{
				tran.CuryID = authTran.CuryID;
				tran.Amount = authTran.Amount;
			}
			return this.DoTransaction(ref aTranNbr, CCTranType.Credit, tran, authTran, customer.AcctCD, procCenter);
		}

		private void ValidateRecordTran(ICCPayment payment)
		{
			CCProcessingCenter procCenter = _repository.FindProcessingCenter(payment.PMInstanceID, payment.CuryID);

			if (procCenter == null || string.IsNullOrEmpty(procCenter.ProcessingTypeName))
			{
				throw new PXException(Messages.ERR_ProcessingCenterForCardNotConfigured);
			}

			CashAccount cashAccount = _repository.GetCashAccount(procCenter.CashAccountID);
			if (cashAccount.CuryID != payment.CuryID)
			{
				throw new PXException(Messages.ProcessingCenterCurrencyDoesNotMatch, payment.CuryID, cashAccount.CuryID);
			}
		}

		private CCProcTran FormatCCProcTran(ICCPayment payment, TranRecordData recordData)
		{
			CCProcTran tran = new CCProcTran
			{
				PMInstanceID = payment.PMInstanceID,
				OrigDocType = payment.OrigDocType,
				OrigRefNbr = payment.OrigRefNbr,
				DocType = payment.DocType,
				TranStatus = recordData.TranStatus,
				RefNbr = payment.RefNbr,
				AuthNumber = recordData.AuthCode,
				CuryID = payment.CuryID,
				Amount = recordData.Amount ?? payment.CuryDocBal,
				CVVVerificationStatus = recordData.CvvVerificationCode,
				PCResponseReasonText = recordData.ResponseText,
				PCTranNumber = recordData.ExternalTranId
			};
			return tran;
		}

		public virtual bool RecordAuthorization(ICCPayment payment, TranRecordData recordData)
		{
			bool ret = false;
			int? innerTranId = null;
			CCProcTran tran = PrepeareRecord(payment, recordData);
			tran.ExpirationDate = recordData.ExpirationDate;
			this.RecordTransaction(ref innerTranId, CCTranType.AuthorizeOnly, tran);
			if (innerTranId != null)
			{
				recordData.InnerTranId = innerTranId;
				ret = true;
			}
			return ret;
		}

		public virtual bool RecordCapture(ICCPayment payment, TranRecordData recordData)
		{
			bool ret = false;
			int? innerTranId = null;
		
			CCProcTran tran = PrepeareRecord(payment, recordData);
			this.RecordTransaction(ref innerTranId, CCTranType.AuthorizeAndCapture, tran);
			if (innerTranId != null)
			{
				recordData.InnerTranId = innerTranId;
				ret = true;
			}
			return ret;
		}

		public virtual bool RecordPriorAuthorizedCapture(ICCPayment payment, TranRecordData recordData)
		{
			bool ret = false;
			int? innerTranId = null;
			CCProcTran tran = PrepeareRecord(payment, recordData);
			this.RecordTransaction(ref innerTranId, CCTranType.PriorAuthorizedCapture, tran);
			if (innerTranId != null)
			{
				recordData.InnerTranId = innerTranId;
				ret = true;
			}
			return ret;
		}

		public virtual bool RecordVoid(ICCPayment payment, TranRecordData recordData)
		{
			bool ret = false;
			int? innerTranId = null;
			CCProcTran tran = PrepeareRecord(payment, recordData);

			CCProcTran refTran = new PXSelect<CCProcTran,
				Where2<
					Where<CCProcTran.tranStatus, Equal<CCTranStatusCode.approved>,
						Or<CCProcTran.tranStatus,Equal<CCTranStatusCode.heldForReview>>>,
					And<CCProcTran.pCTranNumber, Equal<Required<CCProcTran.pCTranNumber>>>>>(_repository.Graph).SelectSingle(tran.PCTranNumber);

			if (refTran != null)
			{
				tran.RefTranNbr = refTran.TranNbr;
			}
			this.RecordTransaction(ref innerTranId, CCTranType.Void, tran);
			if (innerTranId != null)
			{
				recordData.InnerTranId = innerTranId;
				ret = true;
			}
			return ret;
		}

		public virtual bool RecordCaptureOnly(ICCPayment payment, TranRecordData recordData)
		{
			bool ret = false;
			int? innerTranId = null;
			CCProcTran tran = PrepeareRecord(payment, recordData);			this.RecordTransaction(ref innerTranId, CCTranType.CaptureOnly, tran);
			if (innerTranId != null)
			{
				recordData.InnerTranId = innerTranId;
				ret = true;
			}
			return ret;
		}

		public virtual CCProcTran PrepeareRecord(ICCPayment payment, TranRecordData recordData)
		{
			if (recordData.ValidateDoc)
			{
				IsCCValidForProcessing(payment.PMInstanceID);
				ValidateRecordTran(payment);
			}

			CCProcessingCenter procCenter;
			if (payment.PMInstanceID >= 0)
			{
				procCenter = _repository.FindProcessingCenter(payment.PMInstanceID, payment.CuryID);
			}
			else
			{
				procCenter = _repository.GetCCProcessingCenter(recordData.ProcessingCenterId);
			}
			CCProcTran tran = FormatCCProcTran(payment, recordData);
			tran.ProcessingCenterID = procCenter.ProcessingCenterID;

			if (recordData.ExternalTranId != null)
			{ 
				CCProcTran sTran = new PXSelect<CCProcTran,
					Where<CCProcTran.pCTranNumber, Equal<Required<CCProcTran.pCTranNumber>>>,
					OrderBy<Desc<CCProcTran.tranNbr>>>(_repository.Graph).SelectSingle(recordData.ExternalTranId); 
				tran.RefTranNbr = sTran?.TranNbr;
			}
			return tran;
		}

		public virtual void RecordVoidingTran(CCProcTran aOrigTran, string aExtTranRef, out int aTranNbr)
		{
			CCProcTran tran = new CCProcTran
			{
				PMInstanceID = aOrigTran.PMInstanceID,
				ProcessingCenterID = aOrigTran.ProcessingCenterID,
				OrigDocType = aOrigTran.OrigDocType,
				OrigRefNbr = aOrigTran.OrigRefNbr,
				DocType = aOrigTran.DocType,
				RefNbr = aOrigTran.RefNbr,
				CuryID = aOrigTran.CuryID,
				Amount = aOrigTran.Amount,
				RefTranNbr = aOrigTran.TranNbr,
				PCTranNumber = aExtTranRef,
				TranType = CCTranTypeCode.GetTypeCode(CCTranType.Void)
			};
			FillRecordedTran(tran, Messages.LostExpiredTranVoided);
			tran = _repository.InsertCCProcTran(tran);
			_repository.Save();
			aTranNbr = tran.TranNbr.Value;
		}

		public virtual bool RecordCredit(ICCPayment aPmtInfo, string aRefPCTranNbr, string aExtTranRef, string aExtAuthNbr, out int? aTranNbr)
		{
			aTranNbr = null;
			if (!IsCCValidForProcessing(aPmtInfo.PMInstanceID)) return false;
			CCProcTran tran = new CCProcTran();
			tran.Copy(aPmtInfo);
			CCProcTran origCCTran = null;
			if (!String.IsNullOrEmpty(aRefPCTranNbr))
			{
				origCCTran = _repository.FindReferencedCCProcTran(aPmtInfo.PMInstanceID, aRefPCTranNbr);
			}

			if (origCCTran != null && (aPmtInfo.PMInstanceID == origCCTran.PMInstanceID))
			{
				//Override Orig Doc Ref by those from orig CC Tran if any
				tran.RefTranNbr = origCCTran.TranNbr;
				tran.ProcessingCenterID = origCCTran.ProcessingCenterID;
			}
			else
			{
				CCProcessingCenter procCenter = _repository.FindProcessingCenter(aPmtInfo.PMInstanceID, aPmtInfo.CuryID);
				if (procCenter == null || string.IsNullOrEmpty(procCenter.ProcessingTypeName))
				{
					throw new PXException(Messages.ERR_ProcessingCenterForCardNotConfigured);
				}
				CashAccount cashAccount = _repository.GetCashAccount(procCenter.CashAccountID);
				if (cashAccount.CuryID != aPmtInfo.CuryID)
				{
					throw new PXException(Messages.ProcessingCenterCurrencyDoesNotMatch, aPmtInfo.CuryID, cashAccount.CuryID);
				}
				tran.ProcessingCenterID = procCenter.ProcessingCenterID;
				tran.RefPCTranNumber = aRefPCTranNbr;
			}
			tran.PCTranNumber = aExtTranRef;
			tran.AuthNumber = aExtAuthNbr;
			tran.TranType = CCTranTypeCode.GetTypeCode(CCTranType.Credit);
			FillRecordedTran(tran);
			tran = _repository.InsertCCProcTran(tran);
			_repository.Save();
			aTranNbr = tran.TranNbr;
			return true;
		}
		#endregion

		#region Public Static Functions

		public static bool IsAuthorized(CCProcTran aTran)
		{
			return (aTran.TranStatus == CCTranStatusCode.Approved || aTran.TranStatus == CCTranStatusCode.HeldForReview) && (aTran.ProcStatus == CCProcStatus.Finalized);
		}

		public static bool MayBeVoided(CCProcTran aOrigTran)
		{
			switch (aOrigTran.TranType)
			{
				case CCTranTypeCode.Authorize:
				case CCTranTypeCode.AuthorizeAndCapture:
				case CCTranTypeCode.PriorAuthorizedCapture:
				case CCTranTypeCode.CaptureOnly:
					return true;
			}
			return false;
		}

		public static bool MayBeCredited(CCProcTran aOrigTran)
		{
			switch (aOrigTran.TranType)
			{
				case CCTranTypeCode.AuthorizeAndCapture:
				case CCTranTypeCode.PriorAuthorizedCapture:
				case CCTranTypeCode.CaptureOnly:
					return true;
			}
			return false;
		}

		public static bool IsExpired(CustomerPaymentMethod aPMInstance)
		{
			return (aPMInstance.ExpirationDate.HasValue && aPMInstance.ExpirationDate.Value < DateTime.Now);
		}

		protected static void FillRecordedTran(CCProcTran aTran, string aReasonText = Messages.ImportedExternalCCTransaction)
		{
			aTran.PCResponseReasonText = aReasonText;
			aTran.TranNbr = null;
			aTran.StartTime = aTran.EndTime = PXTimeZoneInfo.Now;
			aTran.ExpirationDate = null;
			aTran.TranStatus = CCTranStatusCode.Approved;
			aTran.ProcStatus = CCProcStatus.Finalized;
		}

		#endregion

		#region Internal Processing Functions
		protected virtual void ProcessAPIResponse(APIResponse apiResponse)
		{
			if (!apiResponse.isSucess && apiResponse.ErrorSource != CCErrors.CCErrorSource.None)
			{
				StringBuilder stringBuilder = new StringBuilder();
				foreach (KeyValuePair<string, string> kvp in apiResponse.Messages)
				{
					stringBuilder.Append(kvp.Key);
					stringBuilder.Append(": ");
					stringBuilder.Append(kvp.Value);
					stringBuilder.Append(". ");
				}
				throw new PXException(Messages.CardProcessingError, CCErrors.GetDescription(apiResponse.ErrorSource), stringBuilder.ToString());
			}
		}

		protected virtual bool DoTransaction(ref int aTranNbr, CCTranType aTranType, CCProcTran aTran, CCProcTran aRefTran, string aCustomerCD, CCProcessingCenter aProcCenter)
		{

			if (aProcCenter == null)
			{
				aProcCenter = _repository.FindProcessingCenter(aTran.PMInstanceID, aTran.CuryID);
			}

			if (aProcCenter == null || string.IsNullOrEmpty(aProcCenter.ProcessingTypeName))
			{
				throw new PXException(Messages.ERR_ProcessingCenterForCardNotConfigured);
			}
			CashAccount cashAccount = _repository.GetCashAccount(aProcCenter.CashAccountID);
			if (cashAccount.CuryID != aTran.CuryID)
			{
				throw new PXException(Messages.ProcessingCenterCurrencyDoesNotMatch, aTran.CuryID, cashAccount.CuryID);
			}

			aTran.ProcessingCenterID = aProcCenter.ProcessingCenterID;
			aTran.TranType = CCTranTypeCode.GetTypeCode(aTranType);

			aTran.CVVVerificationStatus = CVVVerificationStatusCode.RelyOnPriorVerification;
			bool cvvVerified = false;
			bool needCvvVerification = isCvvVerificationRequired(aTranType);
			if (needCvvVerification)
			{
				bool isStored;
				CCProcTran verifyTran = this.findCVVVerifyingTran(aTran.PMInstanceID, out isStored);
				if (verifyTran != null)
				{
					cvvVerified = true;
					if (!isStored)
						this.UpdateCvvVerificationStatus(verifyTran);
				}
				if (!cvvVerified)
					aTran.CVVVerificationStatus = CVVVerificationStatusCode.NotVerified;
			}
			aTran = this.StartTransaction(aTran, aProcCenter.OpenTranTimeout);
			aTranNbr = aTran.TranNbr.Value;

			ProcessingInput inputData = new ProcessingInput();
			Copy(inputData, aTran);
			if (!string.IsNullOrEmpty(aCustomerCD))
			{
				inputData.CustomerCD = aCustomerCD;
			}
			if (aRefTran != null)
			{
				inputData.OrigRefNbr = (aTranType == CCTranType.CaptureOnly) ? aRefTran.AuthNumber : aRefTran.PCTranNumber;
			}

			if (needCvvVerification)
			{
				inputData.VerifyCVV = !cvvVerified;
			}

			CCProcessingContext context = new CCProcessingContext()
			{
				callerGraph = _repository.Graph,
				processingCenter = aProcCenter,
				aCustomerCD = aCustomerCD,
				aPMInstanceID = inputData.PMInstanceID,
				aDocType = inputData.DocType,
				aRefNbr = inputData.DocRefNbr,
			};
			var processor = GetProcessingWrapper(context);
			ProcessingResult result = new ProcessingResult();
			bool hasError = false;
			try
			{
				result = processor.DoTransaction(aTranType, inputData);
				PXTrace.WriteInformation($"CCPaymentProcessing.DoTransaction. PCTranNumber:{result.PCTranNumber}; PCResponseCode:{result.PCResponseCode}; PCResponseReasonCode:{result.PCResponseReasonCode}; PCResponseReasonText:{result.PCResponseReasonText}; ErrorText:{result.ErrorText}");
			}
			catch (V2.CCProcessingException procException)
			{
				hasError = true;
				result.ErrorSource = CCErrors.CCErrorSource.ProcessingCenter;
				string errorMessage = String.Empty;
				if (procException.Message.Equals(procException?.InnerException?.Message))
				{
					errorMessage = procException.Message;
				}
				else
				{
					errorMessage = procException.Message + "; " + procException?.InnerException?.Message;
				}
				result.ErrorText = errorMessage;
				result.PCResponseReasonText += errorMessage;
				PXTrace.WriteInformation($"CCPaymentProcessing.DoTransaction.V2.CCProcessingException. ErrorSource:{result.ErrorSource}; ErrorText:{result.ErrorText}");
			}
			catch (WebException webExn)
			{
				hasError = true;
				result.ErrorSource = CCErrors.CCErrorSource.Network;
				result.ErrorText = webExn.Message;
				PXTrace.WriteInformation($"CCPaymentProcessing.DoTransaction.WebException. ErrorSource:{result.ErrorSource}; ErrorText:{result.ErrorText}");
			}
			catch (Exception exn)
			{
				hasError = true;
				result.ErrorSource = CCErrors.CCErrorSource.Internal;
				result.ErrorText = exn.Message;
				throw new PXException(Messages.ERR_CCPaymentProcessingInternalError, aTranNbr, exn.Message);
			}
			finally
			{
				CCProcTran tran = this.EndTransaction(aTranNbr, result, (hasError ? CCProcStatus.Error : CCProcStatus.Finalized));
				if (!hasError)
				{
					this.ProcessTranResult(tran, aRefTran, result);
				}
			}
			return result.isAuthorized;
		}

		public virtual void ShowAcceptPaymentForm(V2.CCTranType tranType, ICCPayment paymentDoc, string procCenterId, int? bAccountId)
		{
			var context = new CCProcessingContext();
			context.processingCenter = _repository.GetCCProcessingCenter(procCenterId);
			context.callerGraph = _repository.Graph;
			context.aRefNbr = paymentDoc.RefNbr;
			context.aDocType = paymentDoc.DocType;
			context.aCustomerID = bAccountId;
			ICardProcessingReadersProvider provider = new CardProcessingReadersProvider(context);
			HostedPaymnetFormProcessingWrapper = (p, w) => { return HostedFromProcessingWrapper.GetPaymentFormProcessingWrapper(p, w, context); };
			IHostedPaymentFormProcessingWrapper wrapper = GetHostedPaymnetFormProcessingWrapper(procCenterId, provider);
			var generator = new V2ProcessingInputGenerator(provider) { FillCardData = false, FillCustomerData = false, FillAdressData = true };
			V2.ProcessingInput v2Input = generator.GetProcessingInput(tranType, paymentDoc);
			wrapper.GetPaymentForm(v2Input);
		}

		public virtual V2.TransactionData GetTransactionById(string transactionId, string processingCenterId)
		{
			var context = new CCProcessingContext();
			context.callerGraph = _repository.Graph;
			context.processingCenter = _repository.GetCCProcessingCenter(processingCenterId);
			ICardTransactionProcessingWrapper wrapper = GetProcessingWrapper(context);
			V2.TransactionData ret = wrapper.GetTransaction(transactionId);
			return ret;
		}

		public virtual IEnumerable<V2.TransactionData> GetUnsettledTransactions(string processingCenterId, V2.TransactionSearchParams searchParams = null)
		{
			var context = new CCProcessingContext();
			context.callerGraph = _repository.Graph;
			context.processingCenter = _repository.GetCCProcessingCenter(processingCenterId);
			ICardTransactionProcessingWrapper wrapper = GetProcessingWrapper(context);
			IEnumerable<V2.TransactionData> ret = wrapper.GetUnsettledTransactions(searchParams);
			return ret;
		}

		protected virtual CCProcTran StartTransaction(CCProcTran aTran, int? aAutoExpTimeout)
		{
			aTran.TranNbr = null;
			aTran.StartTime = PXTimeZoneInfo.Now;
			if (aAutoExpTimeout.HasValue)
			{
				aTran.ExpirationDate = aTran.StartTime.Value.AddSeconds(aAutoExpTimeout.Value);
			}
			aTran = _repository.InsertCCProcTran(aTran);
			_repository.Save();
			return aTran;
		}

		protected virtual CCProcTran EndTransaction(int aTranID, ProcessingResult aRes, string aProcStatus)
		{
			CCProcTran tran = _repository.GetCCProcTran(aTranID);
			Copy(tran, aRes);
			tran.ProcStatus = aProcStatus;
			tran.EndTime = PXTimeZoneInfo.Now;
			if (aRes.ExpireAfterDays.HasValue)
				tran.ExpirationDate = tran.EndTime.Value.AddDays(aRes.ExpireAfterDays.Value);
			else
				tran.ExpirationDate = null;
			tran = _repository.UpdateCCProcTran(tran);
			_repository.Save();
			this.UpdateCvvVerificationStatus(tran);
			return tran;
		}

		protected virtual void ProcessTranResult(CCProcTran aTran, CCProcTran aRefTran, ProcessingResult aResult)
		{
			if (aTran.TranType == CCTranTypeCode.GetTypeCode(CCTranType.Void) &&
			    aRefTran.IsManuallyEntered())
			{
				if (aResult.TranStatus != CCTranStatus.Approved
				    && ((aResult.ResultFlag & (CCResultFlag.OrigTransactionNotFound | CCResultFlag.OrigTransactionExpired)) != CCResultFlag.None))
				{
					//Force Void over Missed of Expired Authorization transaction;
					int voidingTranNbr;
					RecordVoidingTran(aRefTran, null, out voidingTranNbr);
					aResult.isAuthorized = true;
				}
			}
		}

		protected virtual void RecordTransaction(ref int? aTranNbr, CCTranType aTranType, CCProcTran aTran)
		{
			//Add later - use ProcessCenter to fill ExpirationDate
			//aTran.ProcessingCenterID = aProcCenter.ProcessingCenterID;
			aTran.TranType = CCTranTypeCode.GetTypeCode(aTranType);
			if(string.IsNullOrEmpty(aTran.CVVVerificationStatus))
			{ 
				aTran.CVVVerificationStatus = CVVVerificationStatusCode.RelyOnPriorVerification;
				bool cvvVerified = false;
				bool needCvvVerification = isCvvVerificationRequired(aTranType);
				if (needCvvVerification)
				{
					bool isStored;
					CCProcTran verifyTran = this.findCVVVerifyingTran(aTran.PMInstanceID, out isStored);
					if (verifyTran != null)
					{
						cvvVerified = true;
					}
					if (!cvvVerified)
						aTran.CVVVerificationStatus = CVVVerificationStatusCode.NotVerified;
				}
			}
			aTran.TranNbr = null;
			aTran.StartTime = aTran.EndTime = PXTimeZoneInfo.Now;
			if (aTranType != CCTranType.AuthorizeOnly)
			{
				aTran.ExpirationDate = null;
			}
			if (aTran.TranStatus == null)
			{
				aTran.TranStatus = CCTranStatusCode.Approved;
			}
			aTran.ProcStatus = CCProcStatus.Finalized;
			aTran = _repository.InsertCCProcTran(aTran);
			_repository.Save();
			aTranNbr = aTran.TranNbr;
		}

		#endregion

		#region Internal Reading Functions

		protected static bool isCvvVerificationRequired(CCTranType aType)
		{
			return (aType == CCTranType.AuthorizeOnly || aType == CCTranType.AuthorizeAndCapture);
		}

		protected virtual CCProcTran findCVVVerifyingTran(int? aPMInstanceID, out bool aIsStored)
		{
			CustomerPaymentMethod pmInstance = _repository.GetCustomerPaymentMethod(aPMInstanceID);
			if (pmInstance.CVVVerifyTran.HasValue)
			{
				aIsStored = true;
				return _repository.GetCCProcTran(pmInstance.CVVVerifyTran);
			}
			else
			{
				aIsStored = false;
				CCProcTran verifyingTran = _repository.FindVerifyingCCProcTran(aPMInstanceID);
				return verifyingTran;
			}
		}

		protected virtual CustomerPaymentMethod findPMInstance(int? aPMInstanceID, out Customer aCustomer)
		{
			PXResult<CustomerPaymentMethod, Customer> res = _repository.FindCustomerAndPaymentMethod(aPMInstanceID);
			if (res != null)
			{
				aCustomer = (Customer) res;
				return (CustomerPaymentMethod) res;
			}
			aCustomer = null;
			return null;
		}

		protected virtual void UpdateCvvVerificationStatus(CCProcTran aTran)
		{
			if (aTran.TranStatus == CCTranStatusCode.Approved &&
			    aTran.CVVVerificationStatus == CVVVerificationStatusCode.Match &&
			    (aTran.TranType == CCTranTypeCode.AuthorizeAndCapture || aTran.TranType == CCTranTypeCode.Authorize))
			{
				CustomerPaymentMethod pmInstance = _repository.GetCustomerPaymentMethod(aTran.PMInstanceID);
				if (!pmInstance.CVVVerifyTran.HasValue)
				{
					pmInstance.CVVVerifyTran = aTran.TranNbr;
					CustomerPaymentMethodDetail cvvDetail = _repository.GetCustomerPaymentMethodDetail(aTran.PMInstanceID, CreditCardAttributes.CVV);
					if (cvvDetail != null)
						_repository.DeletePaymentMethodDetail(cvvDetail);
					_repository.UpdateCustomerPaymentMethod(pmInstance);
					_repository.Save();
				}
			}
		}

		protected object GetProcessor(CCProcessingCenter processingCenter)
		{
			if (processingCenter == null)
			{
				throw new PXException(Messages.ERR_CCProcessingCenterNotFound);
			}
			object processor;
			try
			{
				Type processorType = PXBuildManager.GetType(processingCenter.ProcessingTypeName, true);
				processor = Activator.CreateInstance(processorType);
			}
			catch (Exception)
			{
				throw new PXException(Messages.ERR_ProcessingCenterTypeInstanceCreationFailed,
					processingCenter.ProcessingTypeName,
					processingCenter.ProcessingCenterID);
			}
			return processor;
		}

		protected ICardTransactionProcessingWrapper GetProcessingWrapper(CCProcessingContext context)
		{
			object processor = GetProcessor(context.processingCenter);
			return _transactionProcessingWrapper(processor, context);
		}

		protected IHostedPaymentFormProcessingWrapper GetHostedPaymnetFormProcessingWrapper(string procCenterId, ICardProcessingReadersProvider provider)
		{
			CCProcessingCenter procCenter = _repository.GetCCProcessingCenter(procCenterId);
			object processor = GetProcessor(procCenter);
			
			return HostedPaymnetFormProcessingWrapper(processor, provider);
		}

		protected virtual bool IsCCValidForProcessing(int? aPMInstanceID)
		{
			Customer customer;
			CustomerPaymentMethod pmInstance = this.findPMInstance(aPMInstanceID, out customer);
			if (pmInstance == null)
			{
				throw new PXException(Messages.CreditCardWithID_0_IsNotDefined, aPMInstanceID);
			}
			return IsValidForProcessing(pmInstance, customer);
		}

		protected virtual bool IsValidForProcessing(CustomerPaymentMethod pmInstance, Customer customer)
		{
			if (pmInstance == null)
			{
				throw new PXException(Messages.CreditCardWithID_0_IsNotDefined);
			}
			if (pmInstance != null && pmInstance.IsActive == false)
			{
				throw new PXException(Messages.InactiveCreditCardMayNotBeProcessed, pmInstance.Descr);
			}
			if (IsExpired(pmInstance))
			{
				throw new PXException(Messages.ERR_CCCreditCardHasExpired, pmInstance.ExpirationDate.Value.ToString("d"), customer.AcctCD);
			}
			return true;
		}

		#endregion

		#region  Utility Funcions

		public static void Copy(ProcessingInput aDst, CCProcTran aSrc)
		{
			aDst.TranID = aSrc.TranNbr.Value;
			aDst.PMInstanceID = aSrc.PMInstanceID.Value;
			bool useOrigDoc = String.IsNullOrEmpty(aSrc.DocType);
			aDst.DocType = useOrigDoc ? aSrc.OrigDocType : aSrc.DocType;
			aDst.DocRefNbr = useOrigDoc ? aSrc.OrigRefNbr : aSrc.RefNbr;
			aDst.Amount = aSrc.Amount.Value;
			aDst.CuryID = aSrc.CuryID;
		}

		public static void Copy(CCProcTran aDst, ProcessingResult aSrc)
		{
			aDst.PCTranNumber = aSrc.PCTranNumber;
			aDst.PCResponseCode = aSrc.PCResponseCode;
			aDst.PCResponseReasonCode = aSrc.PCResponseReasonCode;
			aDst.AuthNumber = aSrc.AuthorizationNbr;
			aDst.PCResponse = aSrc.PCResponse;
			aDst.PCResponseReasonText = aSrc.PCResponseReasonText;
			aDst.CVVVerificationStatus = CVVVerificationStatusCode.GetCCVCode(aSrc.CcvVerificatonStatus);
			aDst.TranStatus = CCTranStatusCode.GetCode(aSrc.TranStatus);
			if (aSrc.ErrorSource != CCErrors.CCErrorSource.None)
			{
				aDst.ErrorSource = CCErrors.GetCode(aSrc.ErrorSource);
				aDst.ErrorText = aSrc.ErrorText;
				if (aSrc.ErrorSource != CCErrors.CCErrorSource.ProcessingCenter)
				{
					aDst.PCResponseReasonText = aSrc.ErrorText;
				}
			}
		}

		#endregion
	}
}
	

