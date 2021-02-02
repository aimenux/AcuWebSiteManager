using System;
using System.Collections.Generic;
using System.Net;
using System.Text;
using System.Linq;
using V2 = PX.CCProcessingBase.Interfaces.V2;
using PX.Common;
using PX.Data;
using PX.Objects.AR.CCPaymentProcessing.Common;
using PX.Objects.AR.CCPaymentProcessing.Helpers;
using PX.Objects.CA;
using PX.Objects.Extensions.PaymentTransaction;
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
			if (contextGraph == null)
			{
				throw new ArgumentNullException(nameof(contextGraph));
			}
			_repository = new CCPaymentProcessingRepository(contextGraph);
			_transactionProcessingWrapper = (plugin, context) => CardTransactionProcessingWrapper.GetTransactionProcessingWrapper(plugin, new CardProcessingReadersProvider(context));
		}

		public CCPaymentProcessing(ICCPaymentProcessingRepository repo)
		{
			_repository = repo ?? throw new ArgumentNullException(nameof(repo));
			_transactionProcessingWrapper = (plugin, context) => CardTransactionProcessingWrapper.GetTransactionProcessingWrapper(plugin, new CardProcessingReadersProvider(context));
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

		public virtual void ValidateSettings(PXGraph callerGraph, string processingCenterID, PluginSettingDetail settingDetail)
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
			CCError result = processor.ValidateSettings(settingDetail);
			if (result.source != CCError.CCErrorSource.None)
			{
				throw new PXSetPropertyException(result.ErrorMessage, PXErrorLevel.Error);
			}
		}

		public virtual IList<PluginSettingDetail> ExportSettings(PXGraph callerGraph, string processingCenterID)
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
			List<PluginSettingDetail> processorSettings = new List<PluginSettingDetail>();
			processor.ExportSettings(processorSettings);
			return processorSettings;
		}
		#endregion
		#region Public Processing Functions

		public virtual TranOperationResult Authorize(ICCPayment payment, bool aCapture)
		{
			Customer customer;
			IsCCValidForProcessing(payment.PMInstanceID);
			CustomerPaymentMethod pmInstance = this.findPMInstance(payment.PMInstanceID, out customer);
			if (pmInstance == null)
			{
				throw new PXException(Messages.CreditCardWithID_0_IsNotDefined, payment.PMInstanceID);
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
			
			CCTranType tranType = aCapture ? CCTranType.AuthorizeAndCapture : CCTranType.AuthorizeOnly;
			CCProcTran tran = new CCProcTran();
			tran.Copy(payment);
			return this.DoTransaction(tranType, tran, null, customer.AcctCD);
		}

		public virtual TranOperationResult Capture(ICCPayment payment, int? transactionId)
		{
			ExternalTransaction extTran = _repository.GetExternalTransaction(transactionId);
			Customer customer;
			IsCCValidForProcessing(payment.PMInstanceID);
			CustomerPaymentMethod pmInstance = this.findPMInstance(payment.PMInstanceID, out customer);
			if (extTran == null)
			{
				throw new PXException(Messages.ERR_CCAuthorizationTransactionIsNotFound, transactionId);
			}
			if (!extTran.Active.GetValueOrDefault())
			{
				throw new PXException(Messages.ERR_CCProcessingReferensedTransactionNotAuthorized, transactionId);
			}
			if (ExternalTranHelper.IsExpired(extTran))
			{
				throw new PXException(Messages.ERR_CCAuthorizationTransactionHasExpired, transactionId);
			}
			
			CCProcessingCenter procCenter = _repository.GetCCProcessingCenter(pmInstance.CCProcessingCenterID);
			if (procCenter == null)
			{
				throw new PXException(Messages.ERR_CCProcessingCenterUsedForAuthIsNotValid, pmInstance.CCProcessingCenterID, transactionId);
			}
			if (!(procCenter.IsActive ?? false))
			{
				throw new PXException(Messages.ERR_CCProcessingCenterUsedForAuthIsNotActive, procCenter, transactionId);
			}

			CCProcTran sTran = GetSuccessfulCCProcTran(extTran);
			CCProcTran procTran = new CCProcTran
			{
				PMInstanceID = payment.PMInstanceID,
				RefTranNbr = sTran.TranNbr,
				TransactionID = extTran.TransactionID,
				DocType = extTran.DocType,
				RefNbr = extTran.RefNbr,
				CuryID = payment.CuryID,
				Amount = payment.CuryDocBal,
				OrigDocType = extTran.OrigDocType,
				OrigRefNbr = extTran.OrigRefNbr
			};

			return this.DoTransaction(CCTranType.PriorAuthorizedCapture, procTran, extTran.TranNumber, customer.AcctCD);
		}

		public virtual TranOperationResult CaptureOnly(ICCPayment aPmtInfo, string authNbr)
		{
			Customer customer;
			IsCCValidForProcessing(aPmtInfo.PMInstanceID);
			CustomerPaymentMethod pmInstance = this.findPMInstance(aPmtInfo.PMInstanceID, out customer);
			if (string.IsNullOrEmpty(authNbr))
			{
				throw new PXException(Messages.ERR_CCExternalAuthorizationNumberIsRequiredForCaptureOnlyTrans, authNbr);
			}
			CCProcessingCenter procCenter = _repository.GetCCProcessingCenter(pmInstance.CCProcessingCenterID);
			if (procCenter == null)
			{
				throw new PXException(Messages.ERR_CCProcessingCenterUsedForAuthIsNotValid, pmInstance.CCProcessingCenterID, 0);
			}
			if (!(procCenter.IsActive ?? false))
			{
				throw new PXException(Messages.ERR_CCProcessingCenterUsedForAuthIsNotActive, procCenter, 0);
			}
			CCProcTran tran = new CCProcTran();
			tran.Copy(aPmtInfo);
			return this.DoTransaction(CCTranType.CaptureOnly, tran, authNbr, customer.AcctCD);
		}

		public virtual TranOperationResult Void(int? aPMInstanceID, int? transactionId)
		{
			ExternalTransaction extTran = _repository.GetExternalTransaction(transactionId);
			Customer customer;
			IsCCValidForProcessing(aPMInstanceID);
			CustomerPaymentMethod pmInstance = this.findPMInstance(aPMInstanceID, out customer);
			if (extTran == null)
			{
				throw new PXException(Messages.ERR_CCTransactionToVoidIsNotFound, transactionId);
			}
			CCProcTran sTran = GetSuccessfulCCProcTran(extTran);
			if (!MayBeVoided(sTran))
			{
				throw new PXException(Messages.ERR_CCProcessingTransactionMayNotBeVoided, transactionId);
			}
			if (!extTran.Active.GetValueOrDefault())
			{
				throw new PXException(Messages.ERR_CCProcessingReferensedTransactionNotAuthorized, transactionId);
			}
			CCProcessingCenter procCenter = _repository.GetCCProcessingCenter(pmInstance.CCProcessingCenterID);
			if (procCenter == null)
			{
				throw new PXException(Messages.ERR_CCProcessingCenterUsedInReferencedTranNotFound, pmInstance.CCProcessingCenterID, transactionId);
			}
			if (!(procCenter.IsActive ?? false))
			{
				throw new PXException(Messages.ERR_CCProcessingCenterUsedInReferencedTranNotFound, procCenter, transactionId);
			}

			CCProcTran tran = new CCProcTran
			{
				PMInstanceID = aPMInstanceID,
				RefTranNbr = sTran.TranNbr,
				TransactionID = extTran.TransactionID,
				DocType = extTran.DocType,
				RefNbr = extTran.RefNbr,
				CuryID = sTran.CuryID,
				Amount = extTran.Amount,
				OrigDocType = extTran.OrigDocType,
				OrigRefNbr = extTran.OrigRefNbr
			};

			return this.DoTransaction(CCTranType.Void, tran, extTran.TranNumber, customer.AcctCD);
		}

		public virtual TranOperationResult VoidOrCredit(int? aPMInstanceID, int? transactionId)
		{
			TranOperationResult result = this.Void(aPMInstanceID, transactionId);
			if(!result.Success)
			{
				ExternalTransaction extTran = _repository.GetExternalTransaction(transactionId);
				CCProcTran tran = GetSuccessfulCCProcTran(extTran);
				if (MayBeCredited(tran))
					result = this.Credit(aPMInstanceID, transactionId, null, null);
			}
			return result;
		}

		public virtual TranOperationResult Credit(ICCPayment aPmtInfo, string aExtRefTranNbr)
		{
			IsCCValidForProcessing(aPmtInfo.PMInstanceID);
			Customer customer;
			CustomerPaymentMethod pmInstance = this.findPMInstance(aPmtInfo.PMInstanceID, out customer);
			CCProcTran refTran = _repository.FindReferencedCCProcTran(aPmtInfo.PMInstanceID, aExtRefTranNbr);
			if (refTran != null)
			{
				return Credit(aPmtInfo, refTran.TransactionID.Value);
			}
			else
			{
				CCProcTran tran = new CCProcTran();
				tran.Copy(aPmtInfo);
				tran.RefTranNbr = null;
				tran.RefPCTranNumber = aExtRefTranNbr;
				return this.DoTransaction(CCTranType.Credit, tran, aExtRefTranNbr, customer.AcctCD);
			}
		}

		public virtual TranOperationResult Credit(ICCPayment aPmtInfo, int? transactionId)
		{
			Customer customer;
			IsCCValidForProcessing(aPmtInfo.PMInstanceID);
			CustomerPaymentMethod pmInstance = this.findPMInstance(aPmtInfo.PMInstanceID, out customer);
			ExternalTransaction extTran = _repository.GetExternalTransaction(transactionId);
			if (extTran == null)
			{
				throw new PXException(Messages.ERR_CCTransactionToCreditIsNotFound, transactionId);
			}

			CCProcTran sTran = GetSuccessfulCCProcTran(extTran);
			if (!MayBeCredited(sTran))
			{
				throw new PXException(Messages.ERR_CCProcessingTransactionMayNotBeCredited, sTran.TranType);
			}

			if (!extTran.Active.GetValueOrDefault())
			{
				throw new PXException(Messages.ERR_CCProcessingReferensedTransactionNotAuthorized, transactionId);
			}

			CCProcessingCenter procCenter = _repository.GetCCProcessingCenter(pmInstance.CCProcessingCenterID);
			if (procCenter == null)
			{
				throw new PXException(Messages.ERR_CCProcessingCenterUsedInReferencedTranNotFound, pmInstance.CCProcessingCenterID, transactionId);
			}

			if (!(procCenter.IsActive ?? false))
			{
				throw new PXException(Messages.ERR_CCProcessingCenterUsedInReferencedTranNotActive, pmInstance.CCProcessingCenterID, transactionId);
			}
			CCProcTran tran = new CCProcTran();
			tran.Copy(aPmtInfo);
			tran.RefTranNbr = sTran.TranNbr;

			if (!aPmtInfo.CuryDocBal.HasValue)
			{
				tran.CuryID = sTran.CuryID;
				tran.Amount = sTran.Amount;
			}
			return this.DoTransaction(CCTranType.Credit, tran, extTran.TranNumber, customer.AcctCD);
		}

		public virtual TranOperationResult Credit(int? aPMInstanceID, int? transactionId, string aCuryID, decimal? aAmount)
		{
			ExternalTransaction extTran = _repository.GetExternalTransaction(transactionId);
			Customer customer;
			IsCCValidForProcessing(aPMInstanceID);
			CustomerPaymentMethod pmInstance = this.findPMInstance(aPMInstanceID, out customer);
			if (extTran == null)
			{
				throw new PXException(Messages.ERR_CCTransactionToCreditIsNotFound, transactionId);
			}

			CCProcTran sTran = GetSuccessfulCCProcTran(extTran);
			if (!MayBeCredited(sTran))
			{
				throw new PXException(Messages.ERR_CCProcessingTransactionMayNotBeCredited, sTran.TranType);
			}

			if (!extTran.Active.GetValueOrDefault())
			{
				throw new PXException(Messages.ERR_CCProcessingReferensedTransactionNotAuthorized, transactionId);
			}

			CCProcessingCenter procCenter = _repository.GetCCProcessingCenter(pmInstance.CCProcessingCenterID);
			if (procCenter == null)
			{
				throw new PXException(Messages.ERR_CCProcessingCenterUsedInReferencedTranNotFound, pmInstance.CCProcessingCenterID, transactionId);
			}

			if (!(procCenter.IsActive ?? false))
			{
				throw new PXException(Messages.ERR_CCProcessingCenterUsedInReferencedTranNotActive, pmInstance.CCProcessingCenterID, transactionId);
			}
			CCProcTran tran = new CCProcTran
			{
				PMInstanceID = aPMInstanceID,
				DocType = extTran.DocType,
				RefNbr = extTran.RefNbr,
				OrigDocType = extTran.OrigDocType,
				OrigRefNbr = extTran.OrigRefNbr,
				RefTranNbr = sTran.TranNbr
			};
			if (aAmount.HasValue)
			{
				tran.CuryID = aCuryID;
				tran.Amount = aAmount;
			}
			else
			{
				tran.CuryID = sTran.CuryID;
				tran.Amount = sTran.Amount;
			}
			return this.DoTransaction(CCTranType.Credit, tran, extTran.TranNumber, customer.AcctCD);
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

		private void ValidateRelativelyRefTran(ICCPayment payment, TranRecordData recordData, CCProcTran refProcTran)
		{
			string docType = GetTrimValue(refProcTran.DocType);
			string refNbr = GetTrimValue(refProcTran.RefNbr);
			string origDocType = GetTrimValue(refProcTran.OrigDocType);
			string origRefNbr = GetTrimValue(refProcTran.OrigRefNbr);
		
			string pDocType = GetTrimValue(payment.DocType);
			string pRefNbr = GetTrimValue(payment.RefNbr);
			string pOrigDocType = GetTrimValue(payment.OrigDocType);
			string pOrigRefNbr = GetTrimValue(payment.OrigRefNbr);
			CustomerPaymentMethod cpm = null;

			if (payment.PMInstanceID != PaymentTranExtConstants.NewPaymentProfile)
			{
				cpm = _repository.GetCustomerPaymentMethod(payment.PMInstanceID);
			}

			if (docType != pDocType || refNbr != pRefNbr
				|| (pOrigDocType != null && (origDocType != pOrigDocType || origRefNbr != pOrigRefNbr))
				|| (payment.PMInstanceID != refProcTran.PMInstanceID && refProcTran.PMInstanceID != PaymentTranExtConstants.NewPaymentProfile))
			{
				string doc = docType != null ? docType + refNbr : origDocType + origRefNbr;
				throw new PXException(Messages.ERR_TransactionIsRecordedForAnotherDoc, recordData.ExternalTranId, cpm?.Descr, doc);
			}
		}

		private string GetTrimValue(string input)
		{
			return input != null ? input.Trim() : null;
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

		/// <summary>
		/// After successful operation the property <see cref="TranRecordData.InnerTranId" /> stores <see cref="ExternalTransaction.TransactionID" /> value. 
		/// This value is not used in the client code, but could be processed by customizations.
		/// </summary>
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

		/// <summary>
		/// After successful operation the property <see cref="TranRecordData.InnerTranId" /> stores <see cref="ExternalTransaction.TransactionID" /> value. 
		/// This value is not used in the client code, but could be processed by customizations.
		/// </summary>
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

		/// <summary>
		/// After successful operation the property <see cref="TranRecordData.InnerTranId" /> stores <see cref="ExternalTransaction.TransactionID" /> value. 
		/// This value is not used in the client code, but could be processed by customizations.
		/// </summary>
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

		/// <summary>
		/// After successful operation the property <see cref="TranRecordData.InnerTranId" /> stores <see cref="ExternalTransaction.TransactionID" /> value. 
		/// This value is not used in the client code, but could be processed by customizations.
		/// </summary>
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

		/// <summary>
		/// After successful operation the property <see cref="TranRecordData.InnerTranId" /> stores <see cref="ExternalTransaction.TransactionID" /> value. 
		/// This value is not used in the client code, but could be processed by customizations.
		/// </summary>
		public virtual bool RecordCaptureOnly(ICCPayment payment, TranRecordData recordData)
		{
			bool ret = false;
			int? innerTranId = null;
			CCProcTran tran = PrepeareRecord(payment, recordData);
			this.RecordTransaction(ref innerTranId, CCTranType.CaptureOnly, tran);
			if (innerTranId != null)
			{
				recordData.InnerTranId = innerTranId;
				ret = true;
			}
			return ret;
		}

		/// <summary>
		/// After successful operation the property <see cref="TranRecordData.InnerTranId" /> stores <see cref="ExternalTransaction.TransactionID" /> value. 
		/// This value is not used in the client code, but could be processed by customizations.
		/// </summary>
		public virtual bool RecordCredit(ICCPayment aPmtInfo, TranRecordData recordData)
		{
			IsCCValidForProcessing(aPmtInfo.PMInstanceID);
			CCProcTran tran = new CCProcTran();
			tran.Copy(aPmtInfo);
			CCProcTran origCCTran = null;
			if (!string.IsNullOrEmpty(recordData.RefExternalTranId))
			{
				origCCTran = _repository.FindReferencedCCProcTran(aPmtInfo.PMInstanceID, recordData.RefExternalTranId);
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
				if (!string.IsNullOrEmpty(recordData.RefExternalTranId))
				{
					tran.RefPCTranNumber = recordData.RefExternalTranId;
				}
			}
			tran.PCTranNumber = recordData.ExternalTranId;
			tran.AuthNumber = recordData.AuthCode;
			tran.TranType = CCTranTypeCode.GetTypeCode(CCTranType.Credit);
			FillRecordedTran(tran);
			tran = _repository.InsertTransaction(tran);
			recordData.InnerTranId = tran.TranNbr;
			return true;
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
				var query = new PXSelect<CCProcTran,
					Where<CCProcTran.pCTranNumber, Equal<Required<CCProcTran.pCTranNumber>>>,
					OrderBy<Desc<CCProcTran.tranNbr>>>(_repository.Graph);
				var trans = query.Select(recordData.ExternalTranId).RowCast<CCProcTran>();
				CCProcTran sTran = null;
				if (payment.PMInstanceID != PaymentTranExtConstants.NewPaymentProfile)
				{
					sTran = trans.Where(i => i.PMInstanceID == payment.PMInstanceID).FirstOrDefault();
				}
				if (sTran == null && trans.Count() > 0 
					&& trans.All(i => i.PMInstanceID == PaymentTranExtConstants.NewPaymentProfile))
				{
					sTran = trans.First();
				}
				
				if (sTran != null)
				{
					string authCode = GetTrimValue(sTran.AuthNumber);
					string recAuthCode = GetTrimValue(recordData.AuthCode);
					if (string.IsNullOrEmpty(authCode) || string.IsNullOrEmpty(recAuthCode) || authCode == recAuthCode)
					{
						ValidateRelativelyRefTran(payment, recordData, sTran);
						tran.RefTranNbr = sTran?.TranNbr;
					}
				}
			}

			if (tran.RefTranNbr != null)
			{
				CCProcTran refProcTran = _repository.GetCCProcTran(tran.RefTranNbr);
				tran.TransactionID = refProcTran.TransactionID;
				tran.OrigDocType = refProcTran.OrigDocType;
				tran.OrigRefNbr = refProcTran.OrigRefNbr;
			}
			return tran;
		}

		private void CheckProcessingCenter(CCProcessingCenter procCenter)
		{
			CCPluginTypeHelper.GetPluginTypeWithCheck(procCenter);
		}
		#endregion

		#region Public Static Functions
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
			if (!apiResponse.isSucess && apiResponse.ErrorSource != CCError.CCErrorSource.None)
			{
				StringBuilder stringBuilder = new StringBuilder();
				foreach (KeyValuePair<string, string> kvp in apiResponse.Messages)
				{
					stringBuilder.Append(kvp.Key);
					stringBuilder.Append(": ");
					stringBuilder.Append(kvp.Value);
					stringBuilder.Append(". ");
				}
				throw new PXException(Messages.CardProcessingError, CCError.GetDescription(apiResponse.ErrorSource), stringBuilder.ToString());
			}
		}

		protected virtual TranOperationResult DoTransaction(CCTranType aTranType, CCProcTran aTran, string origRefNbr, string customerCd)
		{
			TranOperationResult ret = new TranOperationResult();
			CCProcessingCenter procCenter = _repository.FindProcessingCenter(aTran.PMInstanceID, aTran.CuryID);
			CheckProcessingCenter(procCenter);
			
			if (procCenter == null || string.IsNullOrEmpty(procCenter.ProcessingTypeName))
			{
				throw new PXException(Messages.ERR_ProcessingCenterForCardNotConfigured);
			}
			CashAccount cashAccount = _repository.GetCashAccount(procCenter.CashAccountID);
			if (cashAccount.CuryID != aTran.CuryID)
			{
				throw new PXException(Messages.ProcessingCenterCurrencyDoesNotMatch, aTran.CuryID, cashAccount.CuryID);
			}

			aTran.ProcessingCenterID = procCenter.ProcessingCenterID;
			aTran.TranType = CCTranTypeCode.GetTypeCode(aTranType);
			aTran.ProcStatus = CCProcStatus.Opened;
			aTran.CVVVerificationStatus = CVVVerificationStatusCode.SkippedDueToPriorVerification;
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
					aTran.CVVVerificationStatus = CVVVerificationStatusCode.RequiredButNotVerified;
			}
			aTran = this.StartTransaction(aTran, procCenter.OpenTranTimeout);
			ret.TransactionId = aTran.TransactionID.Value;

			TranProcessingInput inputData = new TranProcessingInput();
			Copy(inputData, aTran);
			if (!string.IsNullOrEmpty(customerCd))
			{
				inputData.CustomerCD = customerCd;
			}
			if (!string.IsNullOrEmpty(origRefNbr))
			{
				inputData.OrigRefNbr = origRefNbr;
			}

			if (needCvvVerification)
			{
				inputData.VerifyCVV = !cvvVerified;
			}

			CCProcessingContext context = new CCProcessingContext()
			{
				callerGraph = _repository.Graph,
				processingCenter = procCenter,
				aCustomerCD = customerCd,
				aPMInstanceID = inputData.PMInstanceID,
				aDocType = inputData.DocType,
				aRefNbr = inputData.DocRefNbr,
			};
			var procWrapper = GetProcessingWrapper(context);
			TranProcessingResult result = new TranProcessingResult();
			bool hasError = false;
			try
			{
				result = procWrapper.DoTransaction(aTranType, inputData);
				PXTrace.WriteInformation($"CCPaymentProcessing.DoTransaction. PCTranNumber:{result.PCTranNumber}; PCResponseCode:{result.PCResponseCode}; PCResponseReasonCode:{result.PCResponseReasonCode}; PCResponseReasonText:{result.PCResponseReasonText}; ErrorText:{result.ErrorText}");
			}
			catch (V2.CCProcessingException procException)
			{
				if (procException.Reason == V2.CCProcessingException.ExceptionReason.TranDeclined)
				{
					result.TranStatus = CCTranStatus.Declined;
					hasError = false;
				}
				else
				{
					hasError = true;
				}
				result.ErrorSource = CCError.CCErrorSource.ProcessingCenter;
				string errorMessage = string.Empty;
				if (procException.Message.Equals(procException.InnerException?.Message) 
					|| procException.InnerException == null)
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
				result.ErrorSource = CCError.CCErrorSource.Network;
				result.ErrorText = webExn.Message;
				PXTrace.WriteInformation($"CCPaymentProcessing.DoTransaction.WebException. ErrorSource:{result.ErrorSource}; ErrorText:{result.ErrorText}");
			}
			catch (Exception exn)
			{
				hasError = true;
				result.ErrorSource = CCError.CCErrorSource.Internal;
				result.ErrorText = exn.Message;
				throw new PXException(Messages.ERR_CCPaymentProcessingInternalError, aTran.TranNbr, exn.Message);
			}
			finally
			{
				CCProcTran tran = this.EndTransaction(aTran.TranNbr.Value, result, (hasError ? CCProcStatus.Error : CCProcStatus.Finalized));
				if (!hasError)
				{
					this.ProcessTranResult(tran, result);
				}
			}
			ret.Success = result.Success;
			return ret;
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
			IHostedPaymentFormProcessingWrapper wrapper = GetHostedPaymentFormProcessingWrapper(procCenterId, provider);
			var generator = new V2ProcessingInputGenerator(provider) { FillCardData = false, FillCustomerData = false, FillAdressData = true };
			V2.ProcessingInput v2Input = generator.GetProcessingInput(tranType, paymentDoc);
			wrapper.GetPaymentForm(v2Input);
		}

		public virtual HostedFormResponse ParsePaymentFormResponse(string response, string procCenterId)
		{
			CCProcessingContext context = new CCProcessingContext();
			context.callerGraph = _repository.Graph;
			context.processingCenter = _repository.GetCCProcessingCenter(procCenterId);
			ICardProcessingReadersProvider provider = new CardProcessingReadersProvider(context);
			IHostedPaymentFormProcessingWrapper wrapper = GetHostedPaymentFormProcessingWrapper(procCenterId, provider);
			return wrapper.ParsePaymentFormResponse(response);
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
			aTran = _repository.InsertOrUpdateTransaction(aTran);
			return aTran;
		}

		protected virtual CCProcTran EndTransaction(int aTranID, TranProcessingResult aRes, string aProcStatus)
		{
			CCProcTran tran = _repository.GetCCProcTran(aTranID);
			Copy(tran, aRes);
			tran.ProcStatus = aProcStatus;
			tran.EndTime = PXTimeZoneInfo.Now;
			if (aRes.ExpireAfterDays.HasValue)
				tran.ExpirationDate = tran.EndTime.Value.AddDays(aRes.ExpireAfterDays.Value);
			else
				tran.ExpirationDate = null;
			tran = _repository.UpdateTransaction(tran);
			this.UpdateCvvVerificationStatus(tran);
			return tran;
		}

		protected virtual void ProcessTranResult(CCProcTran aTran, TranProcessingResult aResult)
		{

		}

		protected virtual void RecordTransaction(ref int? extTranId, CCTranType aTranType, CCProcTran aTran)
		{
			//Add later - use ProcessCenter to fill ExpirationDate
			//aTran.ProcessingCenterID = aProcCenter.ProcessingCenterID;
			aTran.TranType = CCTranTypeCode.GetTypeCode(aTranType);
			if(string.IsNullOrEmpty(aTran.CVVVerificationStatus))
			{ 
				aTran.CVVVerificationStatus = CVVVerificationStatusCode.SkippedDueToPriorVerification;
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
						aTran.CVVVerificationStatus = CVVVerificationStatusCode.RequiredButNotVerified;
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
			aTran = _repository.InsertOrUpdateTransaction(aTran);
			extTranId = aTran.TransactionID;
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
			    aTran.CVVVerificationStatus == CVVVerificationStatusCode.Matched &&
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

			object processor = null;

			try
			{
				processor = CCPluginTypeHelper.CreatePluginInstance(processingCenter);
			}
			catch (PXException)
			{
				throw;
			}
			catch
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

		protected IHostedPaymentFormProcessingWrapper GetHostedPaymentFormProcessingWrapper(string procCenterId, ICardProcessingReadersProvider provider)
		{
			CCProcessingCenter procCenter = _repository.GetCCProcessingCenter(procCenterId);
			object processor = GetProcessor(procCenter);
			
			return HostedPaymnetFormProcessingWrapper(processor, provider);
		}

		protected virtual void IsCCValidForProcessing(int? pmInstanceId)
		{
			Customer customer;
			if (pmInstanceId == null)
			{
				throw new PXException(Messages.CreditCardIsNotDefined);
			}
			CustomerPaymentMethod pmInstance = this.findPMInstance(pmInstanceId, out customer);
			if (pmInstance == null)
			{
				throw new PXException(Messages.CreditCardWithID_0_IsNotDefined, pmInstanceId);
			}
			if (pmInstance != null && pmInstance.IsActive == false)
			{
				throw new PXException(Messages.InactiveCreditCardMayNotBeProcessed, pmInstance.Descr);
			}
			if (IsExpired(pmInstance))
			{
				throw new PXException(Messages.ERR_CCCreditCardHasExpired, pmInstance.ExpirationDate.Value.ToString("d"), customer.AcctCD);
			}
		}

		private CCProcTran GetSuccessfulCCProcTran(ExternalTransaction extTran)
		{
			var items = _repository.GetCCProcTranByTranID(extTran.TransactionID);
			CCProcTran procTran = CCProcTranHelper.FindCCLastSuccessfulTran(items) as CCProcTran;
			if (procTran == null)
			{
				throw new Exception("Could not get CCProcTran record by TransactionId.");
			}
			return procTran;
		}
		#endregion

		#region  Utility Funcions

		public static void Copy(TranProcessingInput aDst, CCProcTran aSrc)
		{
			aDst.TranID = aSrc.TranNbr.Value;
			aDst.PMInstanceID = aSrc.PMInstanceID.Value;
			bool useOrigDoc = string.IsNullOrEmpty(aSrc.DocType);
			aDst.DocType = useOrigDoc ? aSrc.OrigDocType : aSrc.DocType;
			aDst.DocRefNbr = useOrigDoc ? aSrc.OrigRefNbr : aSrc.RefNbr;
			aDst.Amount = aSrc.Amount.Value;
			aDst.CuryID = aSrc.CuryID;
		}

		public static void Copy(CCProcTran aDst, TranProcessingResult aSrc)
		{
			aDst.PCTranNumber = aSrc.PCTranNumber;
			aDst.PCResponseCode = aSrc.PCResponseCode;
			aDst.PCResponseReasonCode = aSrc.PCResponseReasonCode;
			aDst.AuthNumber = aSrc.AuthorizationNbr;
			aDst.PCResponse = aSrc.PCResponse;
			aDst.PCResponseReasonText = aSrc.PCResponseReasonText;
			aDst.CVVVerificationStatus = CVVVerificationStatusCode.GetCCVCode(aSrc.CcvVerificatonStatus);
			aDst.TranStatus = CCTranStatusCode.GetCode(aSrc.TranStatus);
			if (aSrc.ErrorSource != CCError.CCErrorSource.None)
			{
				aDst.ErrorSource = CCError.GetCode(aSrc.ErrorSource);
				aDst.ErrorText = aSrc.ErrorText;
				if (aSrc.ErrorSource != CCError.CCErrorSource.ProcessingCenter)
				{
					aDst.PCResponseReasonText = aSrc.ErrorText;
				}
			}
		}

		#endregion
	}
}
	

