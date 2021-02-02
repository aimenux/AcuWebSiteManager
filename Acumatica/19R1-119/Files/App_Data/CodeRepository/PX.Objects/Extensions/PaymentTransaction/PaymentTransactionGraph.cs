using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using PX.Data;
using PX.Objects.AR;
using PX.Objects.AR.CCPaymentProcessing.Interfaces;
using PX.Objects.AR.CCPaymentProcessing.Helpers;
using PX.Objects.AR.CCPaymentProcessing.Common;
using PX.Objects.AR.CCPaymentProcessing.Wrappers;
using PX.Objects.AR.CCPaymentProcessing;
using PX.CCProcessingBase;
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
		public PXSelectExtension<PaymentTransactionDetail> PaymentTransaction;
		public PXSelectExtension<Payment> PaymentDoc;
		public PXSetup<ARSetup> ARSetup;
		public PXFilter<InputPaymentInfo> InputPaymentInfo;

		public bool ReleaseAfterAuthorize { get; set; }
		public bool ReleaseAfterCapture { get; set; }
		public bool ReleaseAfterVoid { get; set; }
		public bool ReleaseAfterCredit { get; set; }
		public bool ReleaseAfterCaptureOnly { get; set; }
		public bool ReleaseAfterRecord { get; set; }

		protected string SelectedProcessingCenter;
		protected CCPaymentProcessing paymentProcessing;

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
				var afterAuthorizeActions = GetAfterAuthorizeActions();
				CCPaymentEntry paymentEntry = GetCCPaymentEntry();
				foreach (var item in afterAuthorizeActions)
				{
					paymentEntry.AddAfterProcessCallback(item);
				}
				if (ReleaseAfterAuthorize)
				{
					paymentEntry.AddAfterProcessCallback(ReleaseARDocument);
				}
				paymentEntry.AuthorizeCCpayment(pDoc,new GenericCCPaymentTransactionAdapter<PaymentTransactionDetail>(PaymentTransaction));
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
				BeforeCapturePayment(doc);
				CheckHeldForReviewTranStatus(pDoc);
				var tranAdapter = new GenericCCPaymentTransactionAdapter<PaymentTransactionDetail>(PaymentTransaction);
				var afterCaptureActions = GetAfterCaptureActions();
				CCPaymentEntry paymentEntry = GetCCPaymentEntry();
				foreach (var item in afterCaptureActions)
				{
					paymentEntry.AddAfterProcessCallback(item);
				}
				if (ReleaseAfterCapture)
				{
					paymentEntry.AddAfterProcessCallback(ReleaseARDocument);
				}
				if (adapter.MassProcess)
				{
					paymentEntry.AddAfterProcessCallback(CheckForHeldForReviewStatusAfterProc);
				}
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
				BeforeVoidPayment(doc);
				var tranAdapter = new GenericCCPaymentTransactionAdapter<PaymentTransactionDetail>(PaymentTransaction);
				var afterVoidActions = GetAfterVoidActions();
				CCPaymentEntry paymentEntry = GetCCPaymentEntry();
				foreach (var item in afterVoidActions)
				{
					paymentEntry.AddAfterProcessCallback(item);
				}
				if (ReleaseAfterVoid)
				{
					paymentEntry.AddAfterProcessCallback(ReleaseARDocument);
				}
				paymentEntry.VoidCCPayment(pDoc,tranAdapter);
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
				var tranAdapter = new GenericCCPaymentTransactionAdapter<PaymentTransactionDetail>(PaymentTransaction);
				if (String.IsNullOrEmpty(pDoc.RefTranExtNbr))
				{
					PaymentDoc.Cache.RaiseExceptionHandling<Payment.refTranExtNbr>(pDoc, pDoc.RefTranExtNbr, new PXSetPropertyException(AR.Messages.ERR_PCTransactionNumberOfTheOriginalPaymentIsRequired));
					continue;
				}

				var afterCreditActions = GetAfterCreditActions();
				CCPaymentEntry paymentEntry = GetCCPaymentEntry();
				foreach (var item in afterCreditActions)
				{
					paymentEntry.AddAfterProcessCallback(item);
				}
				if (ReleaseAfterCredit)
				{
					paymentEntry.AddAfterProcessCallback(ReleaseARDocument);
				}
				paymentEntry.CreditCCPayment(pDoc,tranAdapter);
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
			var parameters = InputPaymentInfo.Current;
			if (parameters == null)
				return adapter.Get();
			if (string.IsNullOrEmpty(parameters.AuthNumber))
			{
				if (InputPaymentInfo.Cache.RaiseExceptionHandling<InputPaymentInfo.authNumber>(parameters,
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
				var tranAdapter = new GenericCCPaymentTransactionAdapter<PaymentTransactionDetail>(PaymentTransaction);
				var afterActions = GetAfterCaptureOnlyActions();
				CCPaymentEntry paymentEntry = GetCCPaymentEntry();
				foreach (var item in afterActions)
				{
					paymentEntry.AddAfterProcessCallback(item);
				}
				if (ReleaseAfterCaptureOnly)
				{
					paymentEntry.AddAfterProcessCallback(ReleaseARDocument);
				}
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
					ICCPaymentTransaction tran = CCProcTranHelper.FindCCLastSuccessfulTran(GetTrans());
					if (tran != null)
					{
						CheckTranStatus(pDoc, tran.TranNbr);
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
			InputPaymentInfo parameters = InputPaymentInfo.Current;
			if (parameters == null)
				return adapter.Get();
			bool failed = false;
			if (string.IsNullOrEmpty(parameters.PCTranNumber))
			{
				if (InputPaymentInfo.Cache.RaiseExceptionHandling<InputPaymentInfo.pCTranNumber>(parameters,
					null, new PXSetPropertyException(ErrorMessages.FieldIsEmpty, typeof(InputPaymentInfo.pCTranNumber).Name)))
					throw new PXRowPersistingException(typeof(InputPaymentInfo.pCTranNumber).Name, null, ErrorMessages.FieldIsEmpty, typeof(InputPaymentInfo.pCTranNumber).Name);
				failed = true;
			}

			if (string.IsNullOrEmpty(parameters.AuthNumber))
			{
				if (InputPaymentInfo.Cache.RaiseExceptionHandling<InputPaymentInfo.authNumber>(parameters,
					null, new PXSetPropertyException(ErrorMessages.FieldIsEmpty, typeof(InputPaymentInfo.authNumber).Name)))
					throw new PXRowPersistingException(typeof(InputPaymentInfo.authNumber).Name, null, ErrorMessages.FieldIsEmpty, typeof(InputPaymentInfo.authNumber).Name);
				failed = true;
			}
			if (failed)
				return adapter.Get();

			List<TPrimary> list = new List<TPrimary>();
			foreach (TPrimary doc in adapter.Get<TPrimary>())
			{
				CheckDocumentUpdatedInDb(doc);
				ICCPayment pDoc = GetPaymentDoc(doc);
				PXTrace.WriteInformation($"{methodName}. RefNbr:{pDoc.RefNbr}; UserName:{info.UserName}");
				list.Add(doc);
				BeforeRecordPayment(doc);
				
				var tranAdapter = new GenericCCPaymentTransactionAdapter<PaymentTransactionDetail>(PaymentTransaction);
				var afterActions = GetAfterRecordActions();
				CCPaymentEntry paymentEntry = GetCCPaymentEntry();
				foreach (var item in afterActions)
				{
					paymentEntry.AddAfterProcessCallback(item);
				}
				if (ReleaseAfterRecord)
				{
					paymentEntry.AddAfterProcessCallback(ReleaseARDocument);
				}
				if (pDoc.DocType == ARDocType.Payment || pDoc.DocType == ARDocType.Prepayment)
				{
					paymentEntry.RecordCCpayment(pDoc, InputPaymentInfo.Current, tranAdapter);
				}
				else if (pDoc.DocType == ARDocType.Refund)
				{
					paymentEntry.RecordCCCredit(pDoc,InputPaymentInfo.Current, tranAdapter);
				}
			}
			return list;
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
			InputPaymentInfo filter = InputPaymentInfo.Current;
			filter.PCTranNumber = filter.AuthNumber = null;
		}

		public virtual void initAuthCCInfo(PXGraph aGraph, string ViewName)
		{
			InputPaymentInfo filter = InputPaymentInfo.Current;
			filter.PCTranNumber = filter.AuthNumber = null;
			PXUIFieldAttribute.SetVisible<InputPaymentInfo.pCTranNumber>(InputPaymentInfo.Cache, filter, false);
		}

		protected virtual CCPaymentEntry GetCCPaymentEntry()
		{
			return new CCPaymentEntry(this.Base);
		}

		public override void Initialize()
		{
			base.Initialize();
			paymentProcessing = new CCPaymentProcessing(Base);
			MapViews(Base);
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

		public int? CheckTranStatus(ICCPayment doc, int? innerTranNbr)
		{
			int? retInnerTranNum = null;
			ICCPaymentTransaction storedTran = GetTrans().Where(i => i.TranNbr == innerTranNbr).FirstOrDefault();
			if (storedTran != null && storedTran.ProcessingCenterID != null)
			{
				bool supported = IsFeatureSupported(storedTran.ProcessingCenterID, CCProcessingFeature.TransactionGetter);
				if (supported)
				{
					V2.TransactionData tranData = paymentProcessing.GetTransactionById(storedTran.PCTranNumber, storedTran.ProcessingCenterID);
					SelectedProcessingCenter = storedTran.ProcessingCenterID;
					if (CCTranStatusCode.GetCode(V1ProcessingDTOGenerator.ToV1(tranData.TranStatus)) != storedTran.TranStatus)
					{
						if (tranData.TranType == V2.CCTranType.AuthorizeOnly)
						{
							retInnerTranNum = GetInnerTranNum(doc, tranData, RecordAuth);
						}
						if (tranData.TranType == V2.CCTranType.PriorAuthorizedCapture)
						{
							retInnerTranNum = GetInnerTranNum(doc, tranData, RecordCapture);
						}
						if (tranData.TranType == V2.CCTranType.AuthorizeAndCapture)
						{
							retInnerTranNum = GetInnerTranNum(doc, tranData, RecordCapture);
						}
						if (tranData.TranType == V2.CCTranType.Void)
						{
							retInnerTranNum = GetInnerTranNum(doc, tranData, RecordVoid);
						}
					}
				}
			}
			return retInnerTranNum;
		}

		private int? GetInnerTranNum(ICCPayment doc, V2.TransactionData tranData, Action<ICCPayment,V2.TransactionData> action)
		{
			action(doc, tranData);
			PaymentTransaction.Cache.ClearQueryCache();
			int? retInnerTranNum = GetTrans().Where(i => i.PCTranNumber == tranData.TranID).OrderByDescending(i => i.TranNbr).Select(i => i.TranNbr).FirstOrDefault();
			return retInnerTranNum;
		}

		public void CheckHeldForReviewTranStatus(ICCPayment doc)
		{
			ICCPaymentTransaction storedTran = CCProcTranHelper.FindOpenForReviewTran(GetTrans());
			if (storedTran != null)
			{
				int? newRecordNum = CheckTranStatus(doc, storedTran.TranNbr);
				if (newRecordNum != null)
				{
					ICCPaymentTransaction tran = GetTrans().Where(i => i.TranNbr == newRecordNum).FirstOrDefault();
					if (tran != null && tran.TranType == CCTranTypeCode.VoidTran)
					{
						PaymentDoc.Cache.Clear();
						PaymentDoc.Cache.ClearQueryCache();
						PaymentTransaction.Cache.Clear();
						PaymentTransaction.Cache.ClearQueryCache();
						throw new PXException(AR.Messages.CCProcessingAuthTranDeclined);
					}
					if (tran != null && tran.TranStatus == CCTranStatusCode.Declined)
					{
						PaymentDoc.Cache.Clear();
						PaymentDoc.Cache.ClearQueryCache();
						PaymentTransaction.Cache.Clear();
						PaymentTransaction.Cache.ClearQueryCache();
						throw new PXException(AR.Messages.CCProcessingAuthTranDeclined);
					}
				}
				else
				{
					if (IsFeatureSupported(storedTran.ProcessingCenterID, CCProcessingFeature.TransactionGetter))
					{
						throw new PXException(AR.Messages.CCProcessingARPaymentTranHeldWarning);
					}
					else
					{
						throw new PXException(AR.Messages.CCProcessingApprovalHoldingTranNotSupported, storedTran.ProcessingCenterID);
					}
				}
			}
		}

		public PaymentState GetPaymentState()
		{
			if (PaymentTransaction == null)
			{
				return new PaymentState();
			}
			IEnumerable<ICCPaymentTransaction> trans = GetTrans();
			PaymentState paymentState = new PaymentState(trans);
			return paymentState;
		}

		public IEnumerable<ICCPaymentTransaction> GetTrans()
		{
			if (PaymentTransaction == null)
				yield break;
			IEnumerable<ICCPaymentTransaction> trans = PaymentTransaction.Select().RowCast<PaymentTransactionDetail>().Cast<ICCPaymentTransaction>();
			foreach (ICCPaymentTransaction tran in trans)
			{
				yield return tran;
			}
		}

		protected CCProcessingCenter GetProcessingCenterById(string id)
		{
			CCProcessingCenter procCenter = PXSelect<CCProcessingCenter,
				Where<CCProcessingCenter.processingCenterID, Equal<Required<CCProcessingCenter.processingCenterID>>>>.Select(Base, id);
			return procCenter;
		}

		protected virtual bool IsFeatureSupported(string procCenterId, CCProcessingFeature feature)
		{
			bool ret = CCProcessingFeatureHelper.IsFeatureSupported(GetProcessingCenterById(procCenterId), CCProcessingFeature.PaymentHostedForm);
			return ret;
		}

		protected virtual void RecordAuth(ICCPayment doc, V2.TransactionData tranData)
		{
			ICCPaymentTransactionAdapter tranAdapter = new GenericCCPaymentTransactionAdapter<PaymentTransactionDetail>(PaymentTransaction);
			var procGraph = PXGraph.CreateInstance<CCPaymentProcessingGraph>();
			DateTime expirationDate = PXTimeZoneInfo.Now.AddDays(tranData.ExpireAfterDays.Value);
			TranRecordData tranRecordData = FormatTranRecord(tranData);
			tranRecordData.ExpirationDate = expirationDate;
			bool res = procGraph.RecordAuthorization(doc, tranRecordData);
			foreach (AfterTranProcDelegate callback in GetAfterAuthorizeActions())
			{
				callback((IBqlTable)doc, CCTranType.AuthorizeOnly, true);
			}
			if (ReleaseAfterAuthorize)
			{
				ReleaseARDocument((IBqlTable)doc, CCTranType.AuthorizeOnly, true);
			}
		}

		protected virtual void RecordVoid(ICCPayment doc, V2.TransactionData tranData)
		{
			ICCPaymentTransactionAdapter tranAdapter = new GenericCCPaymentTransactionAdapter<PaymentTransactionDetail>(PaymentTransaction);
			var procGraph = PXGraph.CreateInstance<CCPaymentProcessingGraph>();
			TranRecordData tranRecordData = FormatTranRecord(tranData);
			tranRecordData.AuthCode = tranData.AuthCode;
			bool res = procGraph.RecordVoid(doc, tranRecordData);
			foreach (AfterTranProcDelegate callback in GetAfterVoidActions())
			{
				callback((IBqlTable)doc, CCTranType.Void, true);
			}
			if (ReleaseAfterVoid)
			{
				ReleaseARDocument((IBqlTable)doc, CCTranType.Void, true);
			}
		}

		protected virtual void RecordCapture(ICCPayment doc, V2.TransactionData tranData)
		{
			ICCPaymentTransactionAdapter tranAdapter = new GenericCCPaymentTransactionAdapter<PaymentTransactionDetail>(PaymentTransaction);
			var procGraph = PXGraph.CreateInstance<CCPaymentProcessingGraph>();
			TranRecordData tranRecordData = FormatTranRecord(tranData);
			CCTranType tranType = CCTranType.CaptureOnly;
			if (tranData.TranType == V2.CCTranType.AuthorizeAndCapture)
			{
				tranType = CCTranType.AuthorizeAndCapture;
				procGraph.RecordCapture(doc, tranRecordData);
			}
			else
			{
				procGraph.RecordCaptureOnly(doc, tranRecordData);
			}
			 
			foreach (AfterTranProcDelegate callback in GetAfterCaptureActions())
			{
				callback((IBqlTable)doc, tranType, true);
			}
			if (ReleaseAfterCapture)
			{
				ReleaseARDocument((IBqlTable)doc, tranType, true);
			}
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
			tranRecordData.TranStatus = CCTranStatusCode.GetCode(V1ProcessingDTOGenerator.ToV1(tranData.TranStatus));
			string cvvCode = CVVVerificationStatusCode.GetCCVCode(V1ProcessingDTOGenerator.ToV1(tranData.CcvVerificationStatus));
			tranRecordData.CvvVerificationCode = cvvCode;
			return tranRecordData;
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

		protected virtual void BeforeRecordPayment(TPrimary doc)
		{

		}

		protected virtual IEnumerable<AfterTranProcDelegate> GetAfterAuthorizeActions()
		{
			yield break;
		}

		protected virtual IEnumerable<AfterTranProcDelegate> GetAfterCaptureActions()
		{
			yield break;
		}

		protected virtual IEnumerable<AfterTranProcDelegate> GetAfterVoidActions()
		{
			yield break;
		}

		protected virtual IEnumerable<AfterTranProcDelegate> GetAfterCreditActions()
		{
			yield break;
		}

		protected virtual IEnumerable<AfterTranProcDelegate> GetAfterCaptureOnlyActions()
		{
			yield break;
		}

		protected virtual IEnumerable<AfterTranProcDelegate> GetAfterRecordActions()
		{
			yield break;
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
				IEnumerable<ICCPaymentTransaction> storedTrans = GetTrans();
				if (e.Row.TranStatus == CCTranStatusCode.HeldForReview)
				{
					ICCPaymentTransaction searchTran = CCProcTranHelper.FindOpenForReviewTran(storedTrans);
					if (searchTran != null && searchTran.TranNbr == row.TranNbr)
					{
						PaymentTransaction.Cache.RaiseExceptionHandling<CCProcTran.tranNbr>(row, row.TranNbr,
							new PXSetPropertyException(AR.Messages.CCProcessingARPaymentTranHeldWarning, PXErrorLevel.RowWarning));
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

		protected abstract PaymentTransactionDetailMapping GetPaymentTransactionMapping();

		protected abstract PaymentMapping GetPaymentMapping();

		protected class PaymentMapping : IBqlMapping
		{
			public Type PMInstanceID = typeof(Payment.pMInstanceID);
			public Type CuryDocBal = typeof(Payment.curyDocBal);
			public Type CuryID = typeof(Payment.curyID);
			public Type DocType = typeof(Payment.docType);
			public Type RefNbr = typeof(Payment.refNbr);
			public Type OrigDocType = typeof(Payment.origDocType);
			public Type OrigRefNbr = typeof(Payment.origRefNbr);
			public Type RefTranExtNbr = typeof(Payment.refTranExtNbr);
			public Type Released = typeof(Payment.released);
			
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

		public static void CheckForHeldForReviewStatusAfterProc(IBqlTable aTable, CCTranType procTran, bool success)
		{
			ICCPayment doc = aTable as ICCPayment;
			if (doc != null && success)
			{
				var graph = PXGraph.CreateInstance<ARPaymentEntry>();
				var query = new PXSelect<CCProcTran, Where<CCProcTran.docType, Equal<Required<CCProcTran.docType>>,
					And<CCProcTran.refNbr, Equal<Required<CCProcTran.refNbr>>>>, OrderBy<Desc<CCProcTran.tranNbr>>>(graph);
				var result = query.Select(doc.DocType, doc.RefNbr);
				var openTran = CCProcTranHelper.FindOpenForReviewTran(result.RowCast<CCProcTran>());
				if (openTran != null)
				{
					throw new PXSetPropertyException(AR.Messages.CCProcessingARPaymentTranHeldWarning, PXErrorLevel.RowWarning);
				}
			}
		}

		public static void ReleaseARDocument(IBqlTable aTable, CCTranType procTran, bool success)
		{
			ARRegister doc = aTable as ARRegister;
			if (doc != null && success)
			{
				var paymentGraph = PXGraph.CreateInstance<ARPaymentEntry>();
				var query = new PXSelect<CCProcTran, Where<CCProcTran.docType, Equal<Required<CCProcTran.docType>>,
					And<CCProcTran.refNbr, Equal<Required<CCProcTran.refNbr>>>>, OrderBy<Desc<CCProcTran.tranNbr>>>(paymentGraph);
				var result = query.Select(doc.DocType,doc.RefNbr);

				PaymentState ps = new PaymentState(result);
				if (!ps.isOpenForReview && ps.lastTran?.TranStatus != CCTranStatusCode.Declined)
				{
					ReleaseARDocument(aTable);
				}
			}
		}

		public static void ReleaseARDocument(IBqlTable aTable)
		{
			ARRegister toProc = (ARRegister)aTable;
			if (!(toProc.Released ?? false))
			{
				List<ARRegister> list = new List<ARRegister>(1);
				list.Add(toProc);
				ARDocumentRelease.ReleaseDoc(list, false);
			}
		}
	}
}
