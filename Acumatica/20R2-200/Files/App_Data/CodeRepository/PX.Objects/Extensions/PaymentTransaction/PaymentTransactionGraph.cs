using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using PX.Data;
using PX.Objects.AR;
using PX.Objects.AR.Standalone;
using PX.Objects.AR.CCPaymentProcessing.Interfaces;
using PX.Objects.AR.CCPaymentProcessing.Helpers;
using PX.Objects.AR.CCPaymentProcessing.Common;
using PX.Objects.AR.CCPaymentProcessing.Wrappers;
using PX.Objects.AR.CCPaymentProcessing.Repositories;
using PX.Objects.AR.CCPaymentProcessing;
using System.Diagnostics;
using PX.Objects.CA;
using V2 = PX.CCProcessingBase.Interfaces.V2;
using PX.Common;
namespace PX.Objects.Extensions.PaymentTransaction
{
	public abstract class PaymentTransactionGraph<TGraph, TPrimary> : PXGraphExtension<TGraph>
		where TGraph : PXGraph
		where TPrimary : class, IBqlTable, new()
	{
		public PXSelectExtension<ExternalTransactionDetail> ExternalTransaction;
		public PXSelectExtension<PaymentTransactionDetail> PaymentTransaction;
		public PXSelectExtension<Payment> PaymentDoc;
		public PXSetup<ARSetup> ARSetup;
		public PXFilter<InputPaymentInfo> InputPmtInfo;

		public bool ReleaseDoc { get; set; }
		public string TranHeldwarnMsg { get; set; } = AR.Messages.CCProcessingTranHeldWarning;
		protected string SelectedProcessingCenter { get; set; }
		protected int? SelectedBAccount { get; set; }
		protected string SelectedPaymentMethod { get; set; }
		protected CCPaymentProcessing paymentProcessing;

		private const string maskedCardTmpl = "****-****-****-";

		public PXAction<TPrimary> authorizeCCPayment;
		[PXUIField(DisplayName = "Authorize CC Payment", MapEnableRights = PXCacheRights.Update, MapViewRights = PXCacheRights.Update)]
		[PXProcessButton]
		[ARMigrationModeDependentActionRestriction(
			restrictInMigrationMode: true,
			restrictForRegularDocumentInMigrationMode: true,
			restrictForUnreleasedMigratedDocumentInNormalMode: true)]
		public virtual IEnumerable AuthorizeCCPayment(PXAdapter adapter)
		{
			var methodName = GetClassMethodName();
			AccessInfo info = this.Base.Accessinfo;
			PXTrace.WriteInformation($"{methodName} started.");
			List<TPrimary> list = new List<TPrimary>();
			foreach (TPrimary doc in adapter.Get<TPrimary>())
			{
				CheckDocumentUpdatedInDb(doc);
				PXCache cache = this.Base.Caches[typeof(TPrimary)];
				bool prevAllowUpdateState = cache.AllowUpdate;
				cache.AllowUpdate = true;
				ICCPayment pDoc = GetPaymentDoc(doc);
				PXTrace.WriteInformation($"{methodName}. RefNbr:{pDoc.RefNbr}; UserName:{info.UserName}");
				list.Add(doc);
				BeforeAuthorizePayment(doc);
				CCPaymentEntry paymentEntry = GetCCPaymentEntry();
				paymentEntry.AuthorizeCCpayment(pDoc,new GenericExternalTransactionAdapter<ExternalTransactionDetail>(ExternalTransaction));
				cache.AllowUpdate = prevAllowUpdateState;
			}
			return list;
		}

		public PXAction<TPrimary> captureCCPayment;
		[PXUIField(DisplayName = "Capture CC Payment", MapEnableRights = PXCacheRights.Update, MapViewRights = PXCacheRights.Update)]
		[PXProcessButton]
		[ARMigrationModeDependentActionRestriction(
			restrictInMigrationMode: true,
			restrictForRegularDocumentInMigrationMode: true,
			restrictForUnreleasedMigratedDocumentInNormalMode: true)]
		public virtual IEnumerable CaptureCCPayment(PXAdapter adapter)
		{ 
			var methodName = GetClassMethodName();
			AccessInfo info = this.Base.Accessinfo;
			PXTrace.WriteInformation($"{methodName} started.");
			List<TPrimary> list = new List<TPrimary>();
			foreach (TPrimary doc in adapter.Get<TPrimary>())
			{
				CheckDocumentUpdatedInDb(doc);
				PXCache cache = this.Base.Caches[typeof(TPrimary)];
				bool prevAllowUpdateState = cache.AllowUpdate;
				cache.AllowUpdate = true;
				ICCPayment pDoc = GetPaymentDoc(doc);
				PXTrace.WriteInformation($"{methodName}. RefNbr:{pDoc.RefNbr}; UserName:{info.UserName}");
				list.Add(doc);
				RunPendingOperations(doc);
				BeforeCapturePayment(doc);
				CheckHeldForReviewTranStatus(pDoc);
				var tranAdapter = new GenericExternalTransactionAdapter<ExternalTransactionDetail>(ExternalTransaction);
				CCPaymentEntry paymentEntry = GetCCPaymentEntry();
				paymentEntry.CaptureCCpayment(pDoc, tranAdapter);				
				cache.AllowUpdate = prevAllowUpdateState;
			}
			return list;
		}

		public PXAction<TPrimary> voidCCPayment;
		[PXUIField(DisplayName = "Void CC Payment", MapEnableRights = PXCacheRights.Update, MapViewRights = PXCacheRights.Update)]
		[PXProcessButton]
		[ARMigrationModeDependentActionRestriction(
			restrictInMigrationMode: true,
			restrictForRegularDocumentInMigrationMode: true,
			restrictForUnreleasedMigratedDocumentInNormalMode: true)]
		public virtual IEnumerable VoidCCPayment(PXAdapter adapter)
		{
			var methodName = GetClassMethodName();
			AccessInfo info = this.Base.Accessinfo;
			PXTrace.WriteInformation($"{methodName} started.");
			List<TPrimary> list = new List<TPrimary>();
			foreach (TPrimary doc in adapter.Get<TPrimary>())
			{
				CheckDocumentUpdatedInDb(doc);
				PXCache cache = this.Base.Caches[typeof(TPrimary)];
				bool prevAllowUpdateState = cache.AllowUpdate;
				cache.AllowUpdate = true;
				ICCPayment pDoc = GetPaymentDoc(doc);
				PXTrace.WriteInformation($"{methodName}. RefNbr:{pDoc.RefNbr}; UserName:{info.UserName}");
				list.Add(doc);
				RunPendingOperations(doc);
				BeforeVoidPayment(doc);
				var tranAdapter = new GenericExternalTransactionAdapter<ExternalTransactionDetail>(ExternalTransaction);
				CCPaymentEntry paymentEntry = GetCCPaymentEntry();
				paymentEntry.VoidCCPayment(pDoc, tranAdapter);
				cache.AllowUpdate = prevAllowUpdateState;
			}
			return list;
		}

		public PXAction<TPrimary> creditCCPayment;
		[PXUIField(DisplayName = "Refund CC Payment", MapEnableRights = PXCacheRights.Update, MapViewRights = PXCacheRights.Update)]
		[PXProcessButton]
		[ARMigrationModeDependentActionRestriction(
			restrictInMigrationMode: true,
			restrictForRegularDocumentInMigrationMode: true,
			restrictForUnreleasedMigratedDocumentInNormalMode: true)]
		public virtual IEnumerable CreditCCPayment(PXAdapter adapter)
		{
			var methodName = GetClassMethodName();
			AccessInfo info = this.Base.Accessinfo;
			PXTrace.WriteInformation($"{methodName} started.");
			List<TPrimary> list = new List<TPrimary>();
			foreach (TPrimary doc in adapter.Get<TPrimary>())
			{
				CheckDocumentUpdatedInDb(doc);
				ICCPayment pDoc = GetPaymentDoc(doc);
				PXTrace.WriteInformation($"{methodName}. RefNbr:{pDoc.RefNbr}; UserName:{info.UserName}");
				list.Add(doc);
				BeforeCreditPayment(doc);
				var tranAdapter = new GenericExternalTransactionAdapter<ExternalTransactionDetail>(ExternalTransaction);
				if (String.IsNullOrEmpty(pDoc.RefTranExtNbr))
				{
					PaymentDoc.Cache.RaiseExceptionHandling<Payment.refTranExtNbr>(pDoc, pDoc.RefTranExtNbr, new PXSetPropertyException(AR.Messages.ERR_PCTransactionNumberOfTheOriginalPaymentIsRequired));
					continue;
				}
				CCPaymentEntry paymentEntry = GetCCPaymentEntry();
				paymentEntry.CreditCCPayment(pDoc, tranAdapter, SelectedProcessingCenter);
			}
			return list;
		}

		public PXAction<TPrimary> captureOnlyCCPayment;
		[PXUIField(DisplayName = "Extern. Authorized CC Payment", MapEnableRights = PXCacheRights.Update, MapViewRights = PXCacheRights.Update)]
		[PXProcessButton]
		[ARMigrationModeDependentActionRestriction(
			restrictInMigrationMode: true,
			restrictForRegularDocumentInMigrationMode: true,
			restrictForUnreleasedMigratedDocumentInNormalMode: true)]
		public virtual IEnumerable CaptureOnlyCCPayment(PXAdapter adapter)
		{
			var methodName = GetClassMethodName();
			PXTrace.WriteInformation($"{methodName} started.");
			AccessInfo info = this.Base.Accessinfo;
			var parameters = InputPmtInfo.Current;
			if (parameters == null)
				return adapter.Get();
			if (string.IsNullOrEmpty(parameters.AuthNumber))
			{
				if (InputPmtInfo.Cache.RaiseExceptionHandling<InputPaymentInfo.authNumber>(parameters,
					null, new PXSetPropertyException(ErrorMessages.FieldIsEmpty, typeof(InputPaymentInfo.authNumber).Name)))
					throw new PXRowPersistingException(typeof(InputPaymentInfo.authNumber).Name, null, ErrorMessages.FieldIsEmpty, typeof(InputPaymentInfo.authNumber).Name);
				return adapter.Get();
			}
			List<TPrimary> list = new List<TPrimary>();
			foreach (TPrimary doc in adapter.Get<TPrimary>())
			{
				CheckDocumentUpdatedInDb(doc);
				ICCPayment pDoc = GetPaymentDoc(doc);
				PXTrace.WriteInformation($"{methodName}. RefNbr:{pDoc.RefNbr}; UserName:{info.UserName}");
				list.Add(doc);
				BeforeCaptureOnlyPayment(doc);
				var tranAdapter = new GenericExternalTransactionAdapter<ExternalTransactionDetail>(ExternalTransaction);
				CCPaymentEntry paymentEntry = GetCCPaymentEntry();
				paymentEntry.CaptureOnlyCCPayment(parameters,pDoc,tranAdapter);
			}
			return list;
		}

		public PXAction<TPrimary> validateCCPayment;
		[PXUIField(DisplayName = "Validate CC Payment", MapEnableRights = PXCacheRights.Update, MapViewRights = PXCacheRights.Update)]
		[PXProcessButton]
		public virtual IEnumerable ValidateCCPayment(PXAdapter adapter)
		{
			string methodName = GetClassMethodName();
			PXTrace.WriteInformation($"{methodName} started.");
			List<TPrimary> list = new List<TPrimary>();
			foreach (TPrimary doc in adapter.Get<TPrimary>())
			{
				list.Add(doc);
				ICCPayment pDoc = GetPaymentDoc(doc);
				PXLongOperation.StartOperation(Base, delegate
				{
					IExternalTransaction tran = ExternalTranHelper.GetActiveTransaction(GetExtTrans());
					if (tran != null)
					{
						TranStatusChanged(pDoc, tran.TransactionID);
					}
				});
			}
			return list;
		}

		public PXAction<TPrimary> recordCCPayment;
		[PXUIField(DisplayName = "Record CC Payment", MapEnableRights = PXCacheRights.Update, MapViewRights = PXCacheRights.Update)]
		[PXProcessButton]
		[ARMigrationModeDependentActionRestriction(
			restrictInMigrationMode: true,
			restrictForRegularDocumentInMigrationMode: true,
			restrictForUnreleasedMigratedDocumentInNormalMode: true)]
		public virtual IEnumerable RecordCCPayment(PXAdapter adapter)
		{
			var methodName = GetClassMethodName();
			PXTrace.WriteInformation($"{methodName} started.");
			AccessInfo info = this.Base.Accessinfo;
			InputPaymentInfo parameters = InputPmtInfo.Current;
			if (parameters == null)
				return adapter.Get();

			bool validated = ValidateRecordedInfo(parameters);
			if (!validated)
				return adapter.Get();

			InputPmtInfo.Cache.Clear();
			List<TPrimary> list = new List<TPrimary>();
			foreach (TPrimary doc in adapter.Get<TPrimary>())
			{
				CheckDocumentUpdatedInDb(doc);
				ICCPayment pDoc = GetPaymentDoc(doc);
				PXTrace.WriteInformation($"{methodName}. RefNbr:{pDoc.RefNbr}; UserName:{info.UserName}");
				list.Add(doc);
				if (!CCProcessingFeatureHelper.IsFeatureSupported(GetProcessingCenterById(SelectedProcessingCenter), 
					CCProcessingFeature.TransactionGetter))
				{
					throw new PXException(AR.Messages.ERR_RecordTranNotSupportedWithoutTranGetterFeature, SelectedProcessingCenter);
				}
				var details = GetTranDetails(parameters.PCTranNumber.Trim());
				ValidateTransactionData(pDoc, details);

				if (details.TranType == V2.CCTranType.Credit && details.RefTranID != null && pDoc.RefTranExtNbr == null)
				{
					var extDoc = PaymentDoc.Current;
					extDoc.RefTranExtNbr = details.RefTranID;
					PaymentDoc.Update(extDoc);
				}

				if (pDoc.Released == false)
				{
					Base.Actions.PressSave();
				}

				CheckSaveCardOption(details);
				PXLongOperation.StartOperation(Base, delegate ()
				{
					var newGraph = CreateNewGraph();
					CCPaymentEntry paymentEntry = new CCPaymentEntry(newGraph);
					RecordTransaction(doc, details, paymentEntry);
				});
			}
			return list;
		}

		public void CheckSaveCardOption(V2.TransactionData details)
		{
			if (NeedSaveCard() && CheckAllowSavingCards() && details.CustomerId != null && details.PaymentId != null)
			{
				using (PXTransactionScope scope = new PXTransactionScope())
				{
					int? pmInstanceId = CreateProfileIfNeeded(details);
					PaymentDoc.Current.PMInstanceID = pmInstanceId;
					PaymentDoc.Update(PaymentDoc.Current);
					Base.Actions.PressSave();
					scope.Complete();
				}
			}
		}

		public virtual void RecordTransaction(TPrimary doc, V2.TransactionData details, CCPaymentEntry paymentEntry)
		{
			paymentEntry.AfterProcessingManager = GetAfterProcessingManager();
			ICCPayment pDoc = GetPaymentDoc(doc);
			CCTranType tranType = V2Converter.ConvertTranType(details.TranType.Value);
			var tranAdapter = new GenericExternalTransactionAdapter<ExternalTransactionDetail>(ExternalTransaction);
			TranRecordData recordData = FormatRecordData(details);
			RaiseBeforeAction(tranType, doc);

			if (pDoc.DocType == ARDocType.Payment || pDoc.DocType == ARDocType.Prepayment)
			{
				if (tranType == CCTranType.AuthorizeAndCapture)
				{
					paymentEntry.RecordAuthCapture(pDoc, recordData, tranAdapter);
				}
				else if (tranType == CCTranType.PriorAuthorizedCapture)
				{
					paymentEntry.RecordPriorAuthCapture(pDoc, recordData, tranAdapter);
				}
				else if (tranType == CCTranType.AuthorizeOnly)
				{
					paymentEntry.RecordAuthorization(pDoc, recordData, tranAdapter);
				}
				else if (tranType == CCTranType.CaptureOnly)
				{
					paymentEntry.RecordCaptureOnly(pDoc, recordData, tranAdapter);
				}
			}
			else if (pDoc.DocType == ARDocType.Refund && tranType == CCTranType.Credit)
			{
				paymentEntry.RecordCCCredit(pDoc, recordData, tranAdapter);
			}
		}

		private TranRecordData FormatRecordData(V2.TransactionData info)
		{
			TranRecordData tranRecord = new TranRecordData();
			tranRecord.ExternalTranId = info.TranID;
			tranRecord.AuthCode = info.AuthCode;
			tranRecord.TransactionDate = info.SubmitTime;
			tranRecord.ProcessingCenterId = SelectedProcessingCenter;
			tranRecord.TranStatus = CCTranStatusCode.GetCode(V2Converter.ConvertTranStatus(info.TranStatus));
			tranRecord.Imported = true;
			return tranRecord;
		}

		protected virtual bool ValidateRecordedInfo(InputPaymentInfo info)
		{
			bool ret = true;

			if (string.IsNullOrEmpty(info.PCTranNumber))
			{
				var ex = new PXSetPropertyException(ErrorMessages.FieldIsEmpty, nameof(InputPaymentInfo.PCTranNumber));
				if (InputPmtInfo.Cache.RaiseExceptionHandling<InputPaymentInfo.pCTranNumber>(info, null, ex))
					throw ex;
				ret = false;
			}
			return ret;
		}

		protected V2.TransactionData GetTranDetails(string transactionId)
		{
			var details = GetPaymentProcessing().GetTransactionById(transactionId, SelectedProcessingCenter);
			return details;
		}

		protected virtual bool RunPendingOperations(TPrimary doc)
		{
			return true;
		}

		protected virtual void ValidateTransactionData(ICCPayment doc, V2.TransactionData tranData)
		{
			TranValidationHelper.CheckRecordedTranStatus(tranData);
			if (tranData.TranType == V2.CCTranType.Void)
			{
				throw new PXException(AR.Messages.ERR_IncorrectVoidTranType, tranData.TranID);
			}
			if (doc.DocType != ARDocType.Refund && tranData.TranType == V2.CCTranType.Credit)
			{
				throw new PXException(AR.Messages.ERR_IncorrectRefundTranType, tranData.TranType);
			}
			if (doc.DocType == ARDocType.Refund && tranData.TranType != V2.CCTranType.Credit)
			{
				throw new PXException(AR.Messages.ERR_IncorrectTranType, tranData.TranType);
			}
			if (doc.DocType == ARDocType.Refund && tranData.RefTranID != null && doc.RefTranExtNbr != null
				&& tranData.RefTranID != doc.RefTranExtNbr)
			{
				throw new PXException(AR.Messages.ERR_RefundTranNotLinkedWithOrigTran, tranData.TranID, doc.RefTranExtNbr);
			}
			ValidateCustomerProfile(doc, tranData);

			var prms = new TranValidationHelper.AdditionalParams();
			prms.ProcessingCenter = SelectedProcessingCenter;
			prms.PMInstanceId = doc.PMInstanceID;
			prms.Repo = GetPaymentRepository();
			TranValidationHelper.CheckTranAlreadyRecorded(tranData, prms);

			if (PaymentDoc.Current.CuryDocBal != tranData.Amount)
			{
				throw new PXException(AR.Messages.ERR_IncorrectTranAmount, tranData.TranID);
			}
		}

		private void ValidateCustomerProfile(ICCPayment doc, V2.TransactionData tranData)
		{
			var repo = GetPaymentRepository();
			var prms = new TranValidationHelper.AdditionalParams();
			prms.CustomerID = SelectedBAccount;
			prms.PMInstanceId = doc.PMInstanceID;
			prms.ProcessingCenter = SelectedProcessingCenter;
			prms.Repo = repo;
			TranValidationHelper.CheckPaymentProfile(tranData, prms);
		}

		protected string GetClassMethodName()
		{
			StackTrace sTrace = new StackTrace();
			string mName = sTrace.GetFrame(1).GetMethod().Name;
			string className = sTrace.GetFrame(1).GetMethod().ReflectedType.Name;
			int index = className.IndexOf('`');
			if (index >= 0)
			{
				className = className.Substring(0,index);
			}
			return className + "." + mName;
		}

		public virtual void clearCCInfo()
		{
			InputPaymentInfo filter = InputPmtInfo.Current;
			filter.PCTranNumber = filter.AuthNumber = null;
		}

		public virtual void initAuthCCInfo(PXGraph aGraph, string ViewName)
		{
			InputPaymentInfo filter = InputPmtInfo.Current;
			filter.PCTranNumber = filter.AuthNumber = null;
			PXUIFieldAttribute.SetVisible<InputPaymentInfo.pCTranNumber>(InputPmtInfo.Cache, filter, false);
		}

		protected virtual CCPaymentEntry GetCCPaymentEntry()
		{
			var paymentEntry = new CCPaymentEntry(this.Base);
			paymentEntry.AfterProcessingManager = GetAfterProcessingManager();
			return paymentEntry;
		}

		public override void Initialize()
		{
			base.Initialize();
			MapViews(Base);
		}

		public virtual CCPaymentProcessing GetPaymentProcessing()
		{
			if (paymentProcessing == null)
			{
				paymentProcessing = new CCPaymentProcessing(Base);
			}
			return paymentProcessing;
		}

		public virtual ICCPaymentProcessingRepository GetPaymentRepository()
		{
			if (paymentProcessing == null)
			{
				paymentProcessing = new CCPaymentProcessing(Base);
			}
			return paymentProcessing.Repository;
		}

		protected virtual void CheckDocumentUpdatedInDb(TPrimary doc)
		{
			PXEntryStatus status = Base.Caches[typeof(TPrimary)].GetStatus(doc);
			if (status == PXEntryStatus.Notchanged)
			{
				EntityHelper entityHelper = new EntityHelper(this.Base);
				object[] keys = entityHelper.GetEntityKey(typeof(TPrimary), doc);
				object storedRow = entityHelper.GetEntityRow(typeof(TPrimary), keys);
				if (storedRow != null)
				{
					byte[] ts = Base.Caches[typeof(TPrimary)].GetValue(doc, "tstamp") as byte[];
					byte[] storedTs = Base.Caches[typeof(TPrimary)].GetValue(storedRow, "tstamp") as byte[];
					if (PXDBTimestampAttribute.compareTimestamps(ts, storedTs) < 0)
					{
						throw new PXException(ErrorMessages.RecordUpdatedByAnotherProcess, typeof(TPrimary).Name);
					}
				}
			}
		}

		protected ICCPayment GetPaymentDoc(TPrimary doc)
		{
			ICCPayment pDoc = doc as ICCPayment;
			if (pDoc == null)
			{
				pDoc = PaymentDoc.View.SelectSingleBound(new object[] { doc }) as ICCPayment;
			}
			if (pDoc == null)
			{
				throw new PXException(NotLocalizableMessages.ERR_CCProcessingNotImplementedICCPayment);
			}
			return pDoc;
		}

		public void SyncProfile(TPrimary doc, V2.TransactionData tranData)
		{
			IExternalTransaction extTran = GetExtTrans().FirstOrDefault(i=>i.TranNumber == tranData.TranID);
			if (extTran == null) return;
			if (SelectedProcessingCenter != null && SelectedBAccount != null && extTran.SaveProfile == true)
			{
				int? pmInstanceId = null;
				ICCPaymentProcessingRepository repo = GetPaymentRepository();
				var tranFromBase = repo.GetExternalTransaction(extTran.TransactionID);
				if (CheckAllowSavingCards())
				{
					ICCPayment pDoc = GetPaymentDoc(doc);
					if (tranData.CustomerId != null && tranData.PaymentId != null && pDoc.PMInstanceID == PaymentTranExtConstants.NewPaymentProfile)
					{
						pmInstanceId = CreateProfileIfNeeded(tranData);
					}
					if (pmInstanceId != PaymentTranExtConstants.NewPaymentProfile && pDoc.Released == false)
					{
						PaymentDoc.Current.PMInstanceID = pmInstanceId;
						PaymentDoc.Update(PaymentDoc.Current);
						tranFromBase.PMInstanceID = pmInstanceId;
						repo.UpdateExternalTransaction(tranFromBase);
					}
				}
				tranFromBase.SaveProfile = false;
				repo.UpdateExternalTransaction(tranFromBase);
				Base.Actions.PressSave();
			}
		}

		public int? CreateProfileIfNeeded(V2.TransactionData details)
		{
			string custId = details.CustomerId;
			string paymentId = details.PaymentId;
			var repo = GetPaymentRepository();
			var result = repo.GetCustomerPaymentMethodWithProfileDetail(SelectedProcessingCenter, custId, paymentId);
			int? pmInstanceId = null;

			if (result != null)
			{
				var cpm = result.Item1;
				TranValidationHelper.CheckCustomer(details, SelectedBAccount, cpm);
				pmInstanceId = cpm.PMInstanceID;
			}
			else
			{
				var tranProfile = new V2.TranProfile() { CustomerProfileId = custId, PaymentProfileId = paymentId };
				var creator = GetPaymentProfileCreator();
				creator.PrepeareCpmRecord();
				pmInstanceId = creator.CreatePaymentProfile(tranProfile);
				creator.CreateCustomerProcessingCenterRecord(tranProfile);
				creator.ClearCaches();
			}
			return pmInstanceId;
		}

		public bool TranStatusChanged(ICCPayment doc, int? tranId)
		{
			bool ret = false;
			IExternalTransaction storedExtTran = GetExtTrans().Where(i => i.TransactionID == tranId).FirstOrDefault();

			if (SelectedProcessingCenter == null)
			{
				CustomerPaymentMethod cpm = GetPaymentRepository().GetCustomerPaymentMethod(doc.PMInstanceID);
				SelectedProcessingCenter = cpm.CCProcessingCenterID;
			}

			if (storedExtTran != null && SelectedProcessingCenter != null)
			{
				bool supported = IsFeatureSupported(SelectedProcessingCenter, CCProcessingFeature.TransactionGetter, false);
				if (supported)
				{
					V2.TransactionData tranData = GetPaymentProcessing().GetTransactionById(storedExtTran.TranNumber, SelectedProcessingCenter);
					string newProcStatus = GetProcessingStatus(tranData);
					if (storedExtTran.ProcStatus != newProcStatus)
					{
						if (tranData.TranType == V2.CCTranType.AuthorizeOnly)
						{
							RecordTran(doc, tranData, RecordAuth);
							ret = true;
						}
						if (tranData.TranType == V2.CCTranType.PriorAuthorizedCapture)
						{
							RecordTran(doc, tranData, RecordCapture);
							ret = true;
						}
						if (tranData.TranType == V2.CCTranType.AuthorizeAndCapture)
						{
							RecordTran(doc, tranData, RecordCapture);
							ret = true;
						}
						if (tranData.TranType == V2.CCTranType.Void)
						{
							RecordTran(doc, tranData, RecordVoid);
							ret = true;
						}
					}
				}
			}
			return ret;
		}

		private void RaiseBeforeAction(CCTranType tranType, TPrimary doc)
		{
			if (tranType == CCTranType.AuthorizeOnly)
			{
				BeforeAuthorizePayment(doc);
			}
			else if (tranType == CCTranType.AuthorizeAndCapture || tranType == CCTranType.PriorAuthorizedCapture)
			{
				BeforeCapturePayment(doc);
			}
			else if (tranType == CCTranType.Credit)
			{
				BeforeCreditPayment(doc);
			}
			else if (tranType == CCTranType.CaptureOnly)
			{
				BeforeCaptureOnlyPayment(doc);
			}
		}
	
		private void RecordTran(ICCPayment doc, V2.TransactionData tranData, Action<ICCPayment,V2.TransactionData> action)
		{
			ExternalTransaction.Cache.ClearQueryCache();
			PaymentTransaction.Cache.ClearQueryCache();
			action(doc, tranData);
		}

		public void CheckHeldForReviewTranStatus(ICCPayment doc)
		{
			ExternalTransactionState state = GetActiveTransactionState();
			if (state.IsOpenForReview)
			{
				int? tranID = state.ExternalTransaction.TransactionID;
				bool changed = TranStatusChanged(doc, tranID);
				if (changed)
				{
					IExternalTransaction affectedTran = GetExtTrans().Where(i=>i.TransactionID == tranID).FirstOrDefault();
					if (affectedTran != null && affectedTran.ProcStatus == ExtTransactionProcStatusCode.VoidSuccess || 
						affectedTran.Active == false)
					{
						PaymentDoc.Cache.Clear();
						PaymentDoc.Cache.ClearQueryCache();
						ClearTransactionСaches();
						throw new PXException(AR.Messages.CCProcessingAuthTranDeclined);
					}
				}
				else
				{
					if (IsFeatureSupported(SelectedProcessingCenter, CCProcessingFeature.TransactionGetter, true))
					{
						throw new PXException(TranHeldwarnMsg);
					}
					else
					{
						throw new PXException(AR.Messages.CCProcessingApprovalHoldingTranNotSupported, SelectedProcessingCenter);
					}
				}
			}
		}

		public ExternalTransactionState GetActiveTransactionState()
		{
			var trans = GetExtTrans();
			var ret = ExternalTranHelper.GetActiveTransactionState(Base, trans);
			return ret;
		}

		public string GetLastTransactionDescription()
		{
			string ret = null;
			var tran = GetExtTrans().FirstOrDefault();
			if (tran != null)
			{
				if (!ExternalTranHelper.HasVoidPreAuthorizedOrAfterReviewInHistory(Base, tran))
				{
					ret = ExternalTranHelper.GetTransactionState(Base, tran).Description;
				}
			}
			return ret;
		}

		public IEnumerable<ICCPaymentTransaction> GetProcTrans()
		{
			if (PaymentTransaction == null)
				yield break;
			IEnumerable<ICCPaymentTransaction> trans = PaymentTransaction.Select().RowCast<PaymentTransactionDetail>().Cast<ICCPaymentTransaction>();
			foreach (ICCPaymentTransaction tran in trans)
			{
				yield return tran;
			}
		}

		public IEnumerable<IExternalTransaction> GetExtTrans()
		{
			if (ExternalTransaction == null)
				yield break;
			foreach (IExternalTransaction tran in ExternalTransaction.Select().RowCast<ExternalTransactionDetail>())
			{
				yield return tran;
			}
		}

		protected void ValidateTran(TPrimary doc, V2.TransactionData tranData)
		{
			IExternalTransaction storedTran = GetExtTrans().FirstOrDefault(i => i.TranNumber == tranData.TranID);
			if (storedTran == null) return;

			if (storedTran.NeedSync == true)
			{
				ICCPayment pDoc = GetPaymentDoc(doc);
				string status = GetProcessingStatus(tranData);
				if (status == storedTran.ProcStatus)
				{
					if (tranData.Amount != storedTran.Amount)
					{
						throw new PXException(AR.Messages.ERR_IncorrectTranAmount, tranData.TranID);
					}
				}
				ValidateCustomerProfile(pDoc, tranData);
			}
		}

		protected void UpdateNeedSyncDoc(TPrimary doc, V2.TransactionData tranData)
		{
			IExternalTransaction storedTran = GetExtTrans().FirstOrDefault(i => i.TranNumber == tranData.TranID);
			if (storedTran == null) return;

			if (storedTran.NeedSync == true)
			{
				ICCPaymentProcessingRepository repo = GetPaymentRepository();
				var tranFromBase = repo.GetExternalTransaction(storedTran.TransactionID);
				tranFromBase.NeedSync = false;
				repo.UpdateExternalTransaction(tranFromBase);
				repo.Save();
				string status = GetProcessingStatus(tranData);
				if (status == storedTran.ProcStatus)
				{
					CCTranType tranType = V2Converter.ConvertTranType(tranData.TranType.Value);
					if (tranType == CCTranType.AuthorizeOnly)
					{
						RunAuthCallbacks(doc);
					}
					if (tranType == CCTranType.AuthorizeAndCapture || tranType == CCTranType.PriorAuthorizedCapture)
					{
						RunCaptureCallbacks(doc, tranType);
					}
				}
				else
				{
					TranStatusChanged(GetPaymentDoc(doc), tranFromBase.TransactionID);
				}
			}
		}

		protected string GetProcessingStatus(V2.TransactionData tranData)
		{
			string tranStatusCode = CCTranStatusCode.GetCode(V2Converter.ConvertTranStatus(tranData.TranStatus));
			string tranTypeCode = CCTranTypeCode.GetTypeCode(V2Converter.ConvertTranType(tranData.TranType.Value));
			string procStatus = ExtTransactionProcStatusCode.GetStatusByTranStatusTranType(tranStatusCode, tranTypeCode);
			return procStatus;
		}

		protected CCProcessingCenter GetProcessingCenterById(string id)
		{
			CCProcessingCenter procCenter = PXSelect<CCProcessingCenter,
				Where<CCProcessingCenter.processingCenterID, Equal<Required<CCProcessingCenter.processingCenterID>>>>.Select(Base, id);
			return procCenter;
		}

		protected virtual PaymentProfileCreator GetPaymentProfileCreator()
		{
			return new PaymentProfileCreator(PXGraph.CreateInstance<CCPaymentHelperGraph>(), SelectedPaymentMethod, SelectedProcessingCenter, SelectedBAccount);
		}

		protected virtual bool IsFeatureSupported(string procCenterId, CCProcessingFeature feature, bool throwOnError)
		{
			bool ret = CCProcessingFeatureHelper.IsFeatureSupported(GetProcessingCenterById(procCenterId), CCProcessingFeature.PaymentHostedForm, throwOnError);
			return ret;
		}

		[Obsolete(Common.InternalMessages.MethodIsObsoleteAndWillBeRemoved2021R1)]
		protected virtual bool IsFeatureSupported(string procCenterId, CCProcessingFeature feature)
		{
			bool ret = CCProcessingFeatureHelper.IsFeatureSupported(GetProcessingCenterById(procCenterId), CCProcessingFeature.PaymentHostedForm);
			return ret;
		}

		protected virtual void RecordAuth(ICCPayment doc, V2.TransactionData tranData)
		{
			TranRecordData tranRecordData = FormatTranRecord(tranData);
			if (tranData.ExpireAfterDays != null)
			{
				DateTime expirationDate = PXTimeZoneInfo.Now.AddDays(tranData.ExpireAfterDays.Value);
				tranRecordData.ExpirationDate = expirationDate;
			}
			var paymentEntry = GetCCPaymentEntry();
			paymentEntry.RecordAuthorization(doc, tranRecordData);
		}

		private void RunAuthCallbacks(IBqlTable doc)
		{
			var afterProcessingMngr = GetAfterProcessingManager();
			afterProcessingMngr.RunAuthorizeActions(doc, true);
			afterProcessingMngr.PersistData();
		}

		protected virtual void RecordVoid(ICCPayment doc, V2.TransactionData tranData)
		{
			var paymentEntry = GetCCPaymentEntry();
			TranRecordData tranRecordData = FormatTranRecord(tranData);
			tranRecordData.AuthCode = tranData.AuthCode;
			paymentEntry.RecordVoid(doc, tranRecordData);
		}

		protected virtual void RecordCapture(ICCPayment doc, V2.TransactionData tranData)
		{
			var paymentEntry = GetCCPaymentEntry();
			TranRecordData tranRecordData = FormatTranRecord(tranData);
			if (tranData.TranType == V2.CCTranType.AuthorizeAndCapture)
			{
				paymentEntry.RecordAuthCapture(doc, tranRecordData);
			}
			else if (tranData.TranType == V2.CCTranType.PriorAuthorizedCapture)
			{
				paymentEntry.RecordPriorAuthCapture(doc, tranRecordData);
			}
			else
			{
				paymentEntry.RecordCaptureOnly(doc, tranRecordData);
			}
		}

		private void RunCaptureCallbacks(IBqlTable doc, CCTranType tranType)
		{
			var afterProcessingMngr = GetAfterProcessingManager();
			if (tranType == CCTranType.PriorAuthorizedCapture)
			{
				afterProcessingMngr.RunPriorAuthorizedCaptureActions(doc, true);
			}
			else
			{
				afterProcessingMngr.RunCaptureActions(doc, true);
			}
			afterProcessingMngr.PersistData();
		}

		protected bool CheckAllowSavingCards()
		{
			CCProcessingCenter procCenter = GetProcessingCenterById(SelectedProcessingCenter);
			if (procCenter.AllowSaveProfile == false)
			{
				return false;
			}
			return true;
		}

		protected TranRecordData FormatTranRecord(V2.TransactionData tranData)
		{
			TranRecordData tranRecordData = new TranRecordData();
			tranRecordData.ExternalTranId = tranData.TranID;
			tranRecordData.Amount = tranData.Amount;
			tranRecordData.AuthCode = tranData.AuthCode;
			tranRecordData.ResponseText = tranData.ResponseReasonText;
			tranRecordData.ProcessingCenterId = SelectedProcessingCenter;
			tranRecordData.ValidateDoc = false;
			tranRecordData.TranStatus = CCTranStatusCode.GetCode(V2Converter.ConvertTranStatus(tranData.TranStatus));
			string cvvCode = CVVVerificationStatusCode.GetCCVCode(V2Converter.ConvertCardVerificationStatus(tranData.CcvVerificationStatus));
			tranRecordData.CvvVerificationCode = cvvCode;
			return tranRecordData;
		}

		protected void ClearTransactionСaches()
		{
			PaymentTransaction.Cache.Clear();
			PaymentTransaction.Cache.ClearQueryCache();
			PaymentTransaction.View.Clear();
			ExternalTransaction.Cache.Clear();
			ExternalTransaction.Cache.ClearQueryCache();
			ExternalTransaction.View.Clear();
		}

		protected bool NeedSaveCard()
		{
			return PaymentDoc?.Current.SaveCard == true;
		}

		protected virtual void MapViews(TGraph graph)
		{

		}

		protected virtual void BeforeAuthorizePayment(TPrimary doc)
		{

		}

		protected virtual void BeforeCapturePayment(TPrimary doc)
		{

		}

		protected virtual void BeforeVoidPayment(TPrimary doc)
		{
			
		}

		protected virtual void BeforeCreditPayment(TPrimary doc)
		{

		}

		protected virtual void BeforeCaptureOnlyPayment(TPrimary doc)
		{

		}

		protected virtual void SetSyncLock(TPrimary doc)
		{

		}

		protected virtual void RemoveSyncLock(TPrimary doc)
		{

		}

		protected virtual bool LockExists(TPrimary doc)
		{
			return false;
		}

		protected virtual AfterProcessingManager GetAfterProcessingManager()
		{
			return null;
		}

		protected virtual TGraph CreateNewGraph()
		{
			return null;
		}

		protected virtual void RowSelected(Events.RowSelected<TPrimary> e)
		{

		}

		protected virtual void RowSelected(Events.RowSelected<PaymentTransactionDetail> e)
		{
			e.Cache.AllowInsert = false;
			e.Cache.AllowUpdate = false;
			e.Cache.AllowDelete = false;
			PaymentTransactionDetail row = e?.Row;
			if (row != null)
			{
				IEnumerable<ICCPaymentTransaction> storedTrans = GetProcTrans();
				if (e.Row.TranStatus == CCTranStatusCode.HeldForReview)
				{
					ICCPaymentTransaction searchTran = CCProcTranHelper.FindOpenForReviewTran(storedTrans);
					if (searchTran != null && searchTran.TranNbr == row.TranNbr)
					{
						PaymentTransaction.Cache.RaiseExceptionHandling<CCProcTran.tranNbr>(row, row.TranNbr,
							new PXSetPropertyException(TranHeldwarnMsg, PXErrorLevel.RowWarning));
					}
				}
				IEnumerable<ICCPaymentTransaction> activeAuthCapture = CCProcTranHelper.FindAuthCaptureActiveTrans(storedTrans);
				if (activeAuthCapture.Count() > 1 && activeAuthCapture.Where(i => i.TranNbr == row.TranNbr).Any())
				{
					PaymentTransaction.Cache.RaiseExceptionHandling<CCProcTran.tranNbr>(row, row.TranNbr,
						new PXSetPropertyException(AR.Messages.CCProcessingARPaymentMultipleActiveTranWarning, PXErrorLevel.RowWarning));
				}
			}
		}

		protected virtual void RowPersisting(Events.RowPersisting<TPrimary> e)
		{
			
		}

		protected abstract ExternalTransactionDetailMapping GetExternalTransactionMapping();

		protected abstract PaymentTransactionDetailMapping GetPaymentTransactionMapping();

		protected abstract PaymentMapping GetPaymentMapping();

		protected class PaymentMapping : IBqlMapping
		{
			public Type PMInstanceID = typeof(Payment.pMInstanceID);
			public Type ProcessingCenterID = typeof(Payment.processingCenterID);
			public Type CuryDocBal = typeof(Payment.curyDocBal);
			public Type CuryID = typeof(Payment.curyID);
			public Type DocType = typeof(Payment.docType);
			public Type RefNbr = typeof(Payment.refNbr);
			public Type OrigDocType = typeof(Payment.origDocType);
			public Type OrigRefNbr = typeof(Payment.origRefNbr);
			public Type RefTranExtNbr = typeof(Payment.refTranExtNbr);
			public Type Released = typeof(Payment.released);
			public Type SaveCard = typeof(Payment.saveCard);
			public Type Table { get; private set; }
			public Type Extension => typeof(Payment);
			public PaymentMapping(Type table)
			{
				Table = table;
			}
		}

		protected class PaymentTransactionDetailMapping : IBqlMapping
		{
			public Type TranNbr = typeof(PaymentTransactionDetail.tranNbr);
			public Type PMInstanceID = typeof(PaymentTransactionDetail.pMInstanceID);
			public Type processingCenterID = typeof(PaymentTransactionDetail.processingCenterID);
			public Type DocType = typeof(PaymentTransactionDetail.docType);
			public Type OrigDocType = typeof(PaymentTransactionDetail.origDocType);
			public Type OrigRefNbr = typeof(PaymentTransactionDetail.origRefNbr);
			public Type RefNbr = typeof(PaymentTransactionDetail.refNbr);
			public Type ExpirationDate = typeof(PaymentTransactionDetail.expirationDate);
			public Type ProcStatus = typeof(PaymentTransactionDetail.procStatus);
			public Type TranStatus = typeof(PaymentTransactionDetail.tranStatus);
			public Type TranType = typeof(PaymentTransactionDetail.tranType);
			public Type PCTranNumber = typeof(PaymentTransactionDetail.pCTranNumber);
			public Type AuthNumber = typeof(PaymentTransactionDetail.authNumber);
			public Type PCResponseReasonText = typeof(PaymentTransactionDetail.pCResponseReasonText);
			public Type Amount = typeof(PaymentTransactionDetail.amount);

			public Type Extension => typeof(PaymentTransactionDetail);
			public Type Table { get; private set; }

			public PaymentTransactionDetailMapping(Type table)
			{
				Table = table;
			}
		}

		protected class ExternalTransactionDetailMapping : IBqlMapping
		{
			public Type TransactionID = typeof(ExternalTransactionDetail.transactionID);
			public Type PMInstanceID = typeof(ExternalTransactionDetail.pMInstanceID);
			public Type DocType = typeof(ExternalTransactionDetail.docType);
			public Type RefNbr = typeof(ExternalTransactionDetail.refNbr);
			public Type OrigDocType = typeof(ExternalTransactionDetail.origDocType);
			public Type OrigRefNbr = typeof(ExternalTransactionDetail.origRefNbr);
			public Type TranNumber = typeof(ExternalTransactionDetail.tranNumber);
			public Type AuthNumber = typeof(ExternalTransactionDetail.authNumber);
			public Type Amount = typeof(ExternalTransactionDetail.amount);
			public Type ProcessingStatus = typeof(ExternalTransactionDetail.procStatus);
			public Type LastActivityDate = typeof(ExternalTransactionDetail.lastActivityDate);
			public Type Direction = typeof(ExternalTransactionDetail.direction);
			public Type Active = typeof(ExternalTransactionDetail.active);
			public Type Completed = typeof(ExternalTransactionDetail.completed);
			public Type ParentTranID = typeof(ExternalTransactionDetail.parentTranID);
			public Type ExpirationDate = typeof(ExternalTransactionDetail.expirationDate);
			public Type cVVVerification = typeof(ExternalTransactionDetail.cVVVerification);
			public Type Extension => typeof(ExternalTransactionDetail);
			public Type Table { get; private set; }

			public ExternalTransactionDetailMapping(Type table)
			{
				Table = table;
			}
		}

		protected class InputPaymentInfoMapping : IBqlMapping
		{
			public Type Table { get; private set; }

			public Type Extension => typeof(InputPaymentInfo);

			public Type AuthNumber = typeof(InputPaymentInfo.authNumber);
			public Type PCTranNumber = typeof(InputPaymentInfo.pCTranNumber);

			public InputPaymentInfoMapping(Type table)
			{
				Table = table;
			}
		}

		public static void CheckForHeldForReviewStatusAfterProc(PXGraph graph, IBqlTable aTable, CCTranType procTran, bool success)
		{
			ICCPayment doc = aTable as ICCPayment;
			if (doc != null && success)
			{
				var paymentEntry = graph as ARPaymentEntry;
				var query = new PXSelect<ExternalTransaction, Where<ExternalTransaction.docType, Equal<Required<ExternalTransaction.docType>>,
					And<ExternalTransaction.refNbr, Equal<Required<ExternalTransaction.refNbr>>>>, OrderBy<Desc<ExternalTransaction.transactionID>>>(graph);
				var result = query.Select(doc.DocType, doc.RefNbr);
				ExternalTransactionState state = ExternalTranHelper.GetActiveTransactionState(graph, result.RowCast<ExternalTransaction>());
				if (state.IsOpenForReview)
				{
					throw new PXSetPropertyException(AR.Messages.CCProcessingTranHeldWarning, PXErrorLevel.RowWarning);
				}
			}
		}

		public static void ReleaseARDocument(IBqlTable aTable)
		{
			AR.ARRegister toProc = (AR.ARRegister)aTable;
			using (PXTimeStampScope scope = new PXTimeStampScope(null))
			{
				if (!(toProc.Released ?? false))
				{
					List<AR.ARRegister> list = new List<AR.ARRegister>(1);
					list.Add(toProc);
					ARDocumentRelease.ReleaseDoc(list, false);
				}
			}
		}
	}
}
