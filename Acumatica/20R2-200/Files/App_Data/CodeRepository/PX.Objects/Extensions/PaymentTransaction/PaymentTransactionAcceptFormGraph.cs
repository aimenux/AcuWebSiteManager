using System.Collections;
using System.Collections.Generic;
using System.Linq;
using PX.Data;
using PX.Objects.AR;
using PX.Objects.CA;
using PX.Objects.AR.CCPaymentProcessing.Helpers;
using PX.Objects.AR.CCPaymentProcessing.Common;
using PX.Objects.AR.CCPaymentProcessing.Interfaces;
using PX.Objects.AR.CCPaymentProcessing.Wrappers;
using PX.Objects.AR.CCPaymentProcessing;
using V2 = PX.CCProcessingBase.Interfaces.V2;
using Newtonsoft.Json.Linq;
using PX.CCProcessingBase;
using System.Text.RegularExpressions;
using System;
using PX.Common;

namespace PX.Objects.Extensions.PaymentTransaction
{
	public abstract class PaymentTransactionAcceptFormGraph<TGraph, TPrimary> : PaymentTransactionGraph<TGraph, TPrimary>
		where TGraph : PXGraph
		where TPrimary : class, IBqlTable, new()
	{
		private const string maskedCardTmpl = "****-****-****-";

		protected bool UseAcceptHostedForm;
		protected Guid? DocNoteId;
		protected bool EnableMobileMode;
		protected bool CheckSyncLockOnPersist;

		private string checkedProcessingCenter = null;
		private bool checkedProcessingCenterResult;
		private RetryPolicy<IEnumerable<V2.TransactionData>> retryUnsettledTran;

		[PXUIField(DisplayName = "Authorize CC Payment", MapEnableRights = PXCacheRights.Update, MapViewRights = PXCacheRights.Update)]
		[PXProcessButton]
		[ARMigrationModeDependentActionRestriction(
		restrictInMigrationMode: true,
		restrictForRegularDocumentInMigrationMode: true,
		restrictForUnreleasedMigratedDocumentInNormalMode: true)]
		public override IEnumerable AuthorizeCCPayment(PXAdapter adapter)
		{
			IEnumerable ret;
			string methodName = GetClassMethodName();
			PXTrace.WriteInformation($"{methodName} started.");
			ShowProcessingWarnIfLock(adapter);
			if (!UseAcceptHostedForm)
			{
				ret = base.AuthorizeCCPayment(adapter);
			}
			else
			{
				if (!IsSupportPaymentHostedForm(SelectedProcessingCenter))
				{
					throw new PXException(AR.Messages.ERR_ProcessingCenterNotSupportAcceptPaymentForm);
				}
				ret = AuthorizeThroughForm(adapter);
			}
			return ret;
		}

		[PXUIField(DisplayName = "Capture CC Payment", MapEnableRights = PXCacheRights.Update, MapViewRights = PXCacheRights.Update)]
		[PXProcessButton]
		[ARMigrationModeDependentActionRestriction(
			restrictInMigrationMode: true,
			restrictForRegularDocumentInMigrationMode: true,
			restrictForUnreleasedMigratedDocumentInNormalMode: true)]
		public override IEnumerable CaptureCCPayment(PXAdapter adapter)
		{
			IEnumerable ret;
			string methodName = GetClassMethodName();
			PXTrace.WriteInformation($"{methodName} started.");
			ShowProcessingWarnIfLock(adapter);
			if (!UseAcceptHostedForm)
			{
				ret = base.CaptureCCPayment(adapter);
			}
			else
			{
				if (!IsSupportPaymentHostedForm(SelectedProcessingCenter))
				{
					throw new PXException(AR.Messages.ERR_ProcessingCenterNotSupportAcceptPaymentForm);
				}
				ret = CaptureThroughForm(adapter);
			}
			return ret;
		}

		[PXUIField(DisplayName = "Validate CC Payment", MapEnableRights = PXCacheRights.Update, MapViewRights = PXCacheRights.Update, Visible = true)]
		[PXButton]
		public override IEnumerable ValidateCCPayment(PXAdapter adapter)
		{
			string methodName = GetClassMethodName();
			PXTrace.WriteInformation($"{methodName} started.");
			ShowProcessingWarnIfLock(adapter);
			Base.Actions.PressCancel();
			List<TPrimary> list = new List<TPrimary>();
			foreach (TPrimary doc in adapter.Get<TPrimary>())
			{
				list.Add(doc);
				PXLongOperation.StartOperation(Base, delegate
				{
					if (!RunPendingOperations(doc))
					{
						CheckPaymentTransaction(doc);
						IExternalTransaction storedTran = GetExtTrans().FirstOrDefault();
						bool needSyncUnsettled = false;
						if (storedTran != null)
						{
							if (!ExternalTranHelper.GetTransactionState(Base, storedTran).IsActive)
							{
								needSyncUnsettled = true;
							}
						}
						else
						{
							needSyncUnsettled = true;
						}

						if (needSyncUnsettled)
						{
							ICCPayment pDoc = GetPaymentDoc(doc);
							IEnumerable<V2.TransactionData> trans = GetPaymentProcessing().GetUnsettledTransactions(SelectedProcessingCenter);
							IEnumerable<string> result = PrepareTransactionIds(GetTransByDoc(pDoc, trans));

							SyncPaymentTransactionById(doc, result);
						}
					}
					if (LockExists(doc))
					{
						RemoveSyncLock(doc);
					}
				});
			}
			return list;
		}

		public PXAction<TPrimary> syncPaymentTransaction;
		[PXUIField(MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select, Visible = false)]
		[PXButton]
		public virtual IEnumerable SyncPaymentTransaction(PXAdapter adapter)
		{
			string methodName = GetClassMethodName();
			PXTrace.WriteInformation($"{methodName} started.");
			TPrimary doc = adapter.Get<TPrimary>().First<TPrimary>();
			bool isCancel = false;
			var cancelStr = this.GetStringFromContext("__CLOSECCHFORM");
			if (cancelStr != null && bool.TryParse(cancelStr, out isCancel) && isCancel)
			{
				RemoveSyncLock(doc);
				return adapter.Get();
			}
			var tranResponseStr = this.GetStringFromContext("__TRANID");
			if (tranResponseStr == null)
			{
				throw new PXException(AR.Messages.ERR_AcceptHostedFormResponseNotFound);
			}

			var response = GetPaymentProcessing().ParsePaymentFormResponse(tranResponseStr, SelectedProcessingCenter);
			string tranId = response?.TranID;
			if (string.IsNullOrEmpty(tranId))
			{
				throw new PXException(AR.Messages.ERR_CouldNotGetTransactionIdFromResponse);
			}

			PXLongOperation.StartOperation(Base, () =>
				SyncPaymentTransactionById(doc, new List<string>() { tranId }));

			return adapter.Get();
		}

		protected virtual string GetStringFromContext(string key)
		{
			var request = System.Web.HttpContext.Current?.Request;
			return request != null
				? request.Form.Get(key)
				: PXContext.Session[key] as string;
		}

		private IEnumerable AuthorizeThroughForm(PXAdapter adapter)
		{
			List<TPrimary> list = new List<TPrimary>();
			foreach (TPrimary doc in adapter.Get<TPrimary>())
			{
				CheckDocumentUpdatedInDb(doc);
				ICCPayment pDoc = GetPaymentDoc(doc);
				if (pDoc.CuryDocBal <= 0)
				{
					throw new PXException(AR.Messages.ERR_CCAmountMustBePositive);
				}
				if (pDoc.Released == false)
				{
					Base.Actions.PressSave();
					BeforeAuthorizePayment(doc);
				}
				CheckPaymentTransaction(doc);
				list.Add(doc);
				if (EnableMobileMode)
				{
					Dictionary<string, string> appendParams = new Dictionary<string, string>();
					appendParams.Add("NoteId", DocNoteId.ToString());
					appendParams.Add("DocType", pDoc.DocType);
					appendParams.Add("TranType", AR.CCPaymentProcessing.Common.CCTranType.AuthorizeOnly.ToString());
					appendParams.Add("CompanyName", Base.Accessinfo.CompanyName);
					string redirectUrl = V2.CCServiceEndpointHelper.GetUrl(V2.CCServiceAction.GetAcceptPaymentForm, appendParams);
					if (redirectUrl == null)
						throw new PXException(AR.Messages.ERR_CCProcessingCouldNotGenerateRedirectUrl);

					if (pDoc.PMInstanceID == PaymentTranExtConstants.NewPaymentProfile && !TranHeldForReview())
					{
						SetSyncLock(doc);
						PXTrace.WriteInformation($"Redirect to endpoint. Url: {redirectUrl}");
						throw new PXRedirectToUrlException(redirectUrl, PXBaseRedirectException.WindowMode.New, true, "Redirect:" + redirectUrl);
					}
					RemoveSyncLock(doc);
				}
				else
				{
					PXPaymentRedirectException redirectEx = null;
					try
					{
						GetPaymentProcessing().ShowAcceptPaymentForm(V2.CCTranType.AuthorizeOnly, pDoc, SelectedProcessingCenter, SelectedBAccount);
					}
					catch (PXPaymentRedirectException ex)
					{
						redirectEx = ex;
					}

					PXLongOperation.StartOperation(Base, () =>
					{
						CheckPaymentTransaction(doc);
						if (pDoc.PMInstanceID == PaymentTranExtConstants.NewPaymentProfile && !TranHeldForReview() && redirectEx != null)
						{
							SetSyncLock(doc);
							throw redirectEx;
						}
						RemoveSyncLock(doc);
					});
				}
			}
			return list;
		}

		private IEnumerable CaptureThroughForm(PXAdapter adapter)
		{
			List<TPrimary> list = new List<TPrimary>();
			foreach (TPrimary doc in adapter.Get<TPrimary>())
			{
				CheckDocumentUpdatedInDb(doc);
				ICCPayment pDoc = GetPaymentDoc(doc);
				if (pDoc.CuryDocBal <= 0)
				{
					throw new PXException(AR.Messages.ERR_CCAmountMustBePositive);
				}
				if (pDoc.Released == false)
				{
					Base.Actions.PressSave();
					BeforeCapturePayment(doc);
				}
				CheckPaymentTransaction(doc);
				list.Add(doc);
				if (EnableMobileMode)
				{
					if (FindPreAuthorizing())
						continue;
					Dictionary<string, string> appendParams = new Dictionary<string, string>();
					appendParams.Add("NoteId", DocNoteId.ToString());
					appendParams.Add("DocType", pDoc.DocType);
					appendParams.Add("TranType", V2.CCTranType.AuthorizeAndCapture.ToString());
					appendParams.Add("CompanyName", Base.Accessinfo.CompanyName);
					string redirectUrl = V2.CCServiceEndpointHelper.GetUrl(V2.CCServiceAction.GetAcceptPaymentForm, appendParams);
					if (redirectUrl == null)
						throw new PXException(AR.Messages.ERR_CCProcessingCouldNotGenerateRedirectUrl);
					if (pDoc.PMInstanceID == PaymentTranExtConstants.NewPaymentProfile && !TranHeldForReview())
					{
						SetSyncLock(doc);
						PXTrace.WriteInformation($"Redirect to endpoint. Url: {redirectUrl}");
						throw new PXRedirectToUrlException(redirectUrl, PXBaseRedirectException.WindowMode.New, true, "Redirect:" + redirectUrl);
					}
					RemoveSyncLock(doc);
				}
				else
				{
					PXPaymentRedirectException redirectEx = null;
					try
					{
						GetPaymentProcessing().ShowAcceptPaymentForm(V2.CCTranType.AuthorizeAndCapture, pDoc, SelectedProcessingCenter, SelectedBAccount);
					}
					catch (PXPaymentRedirectException ex)
					{
						redirectEx = ex;
					}

					PXLongOperation.StartOperation(Base, () =>
					{
						CheckPaymentTransaction(doc);
						if (pDoc.PMInstanceID == PaymentTranExtConstants.NewPaymentProfile && !TranHeldForReview() && !FindPreAuthorizing() && redirectEx != null)
						{
							SetSyncLock(doc);
							throw redirectEx;
						}
						RemoveSyncLock(doc);
					});
				}
			}
			return list;
		}

		private void ShowProcessingWarnIfLock(PXAdapter adapter)
		{
			TPrimary doc = adapter.Get<TPrimary>().FirstOrDefault();
			IExternalTransaction extTran = ExternalTranHelper.GetActiveTransaction(GetExtTrans());
			if (doc != null && adapter.ExternalCall && LockExists(doc) && extTran == null)
			{
				WebDialogResult result = PaymentTransaction.Ask(AR.Messages.CCProcessingARPaymentAlreadyProcessed, MessageButtons.OKCancel);
				if (result == WebDialogResult.No)
				{
					throw new PXException(AR.Messages.CCProcessingOperationCancelled);
				}
			}
		}

		protected override bool RunPendingOperations(TPrimary doc)
		{
			bool supported = IsFeatureSupported(SelectedProcessingCenter, CCProcessingFeature.TransactionGetter, false);
			if (supported)
			{
				IExternalTransaction extTran = ExternalTranHelper.GetActiveTransaction(GetExtTrans());

				if (extTran == null || extTran.NeedSync == false) return false;

				V2.TransactionData tranData = GetPaymentProcessing().GetTransactionById(extTran.TranNumber, SelectedProcessingCenter);
				using (PXTransactionScope scope = new PXTransactionScope())
				{
					if (LockExists(doc))
					{
						RemoveSyncLock(doc);
					}
					ValidateTran(doc, tranData);
					SyncProfile(doc, tranData);
					UpdateNeedSyncDoc(doc, tranData);
					scope.Complete();
				}
				return true;
			}
			return false;
		}

		private void CheckPaymentTransaction(TPrimary doc)
		{
			if (!IsFeatureSupported(SelectedProcessingCenter, CCProcessingFeature.TransactionGetter, false))
				return;
			ICCPayment pDoc = GetPaymentDoc(doc);
			IEnumerable<V2.TransactionData> trans = null;

			if (LockExists(doc))
			{
				retryUnsettledTran.HandleError(i => GetTransByDoc(pDoc, i).Count > 0 ? true : false);
				try
				{
					trans = retryUnsettledTran.Execute(() => GetPaymentProcessing().GetUnsettledTransactions(SelectedProcessingCenter));
				}
				catch (InvalidOperationException)
				{ }
			}

			if (trans != null)
			{
				IEnumerable<string> result = PrepareTransactionIds(GetTransByDoc(pDoc, trans));
				SyncPaymentTransactionById(doc, result);
			}
			else
			{
				IExternalTransaction tran = ExternalTranHelper.GetActiveTransaction(GetExtTrans());
				if (tran != null)
				{
					SyncPaymentTransactionById(doc, new List<string>() { tran.TranNumber });
				}
			}
		}

		public virtual void SyncPaymentTransactionById(TPrimary doc, IEnumerable<string> tranIds)
		{
			if (!IsFeatureSupported(SelectedProcessingCenter, CCProcessingFeature.PaymentHostedForm, false))
				return;
			ICCPayment pDoc = GetPaymentDoc(doc);
			using (PXTransactionScope scope = new PXTransactionScope())
			{
				foreach (string tranId in tranIds)
				{
					IExternalTransaction storedExtTran = GetExtTrans().FirstOrDefault(i => i.TranNumber == tranId);
					var tranData = GetTranData(tranId);
					string newProcStatus = GetProcessingStatus(tranData);
					if (storedExtTran != null && storedExtTran.ProcStatus == newProcStatus)
					{
						continue;
					}
					if (tranData?.CustomerId != null && !SuitableCustomerProfileId(tranData?.CustomerId))
					{
						continue;
					}

					PXTrace.WriteInformation($"Synchronize tran. TranId = {tranData.TranID}, TranType = {tranData.TranType}, DocNum = {tranData.DocNum}, " +
						$"SubmitTime = {tranData.SubmitTime}, Amount = {tranData.Amount}, PCCustomerID = {tranData.CustomerId}, PCCustomerPaymentID = {tranData.PaymentId}");

					V2.CCTranType tranType = tranData.TranType.Value;

					if (tranType == V2.CCTranType.Void)
					{
						RemoveSyncLock(doc);
						RecordVoid(pDoc, tranData);
					}

					if (tranType == V2.CCTranType.AuthorizeOnly)
					{
						if (tranData.TranStatus == V2.CCTranStatus.Approved)
						{
							GetOrCreatePaymentProfileByTran(tranData, pDoc);
						}
						RemoveSyncLock(doc);
						RecordAuth(pDoc, tranData);
					}

					if (tranType == V2.CCTranType.PriorAuthorizedCapture)
					{
						if (tranData.TranStatus == V2.CCTranStatus.Approved)
						{
							GetOrCreatePaymentProfileByTran(tranData, pDoc);
						}
						RemoveSyncLock(doc);
						RecordCapture(pDoc, tranData);
					}

					if (tranType == V2.CCTranType.AuthorizeAndCapture)
					{
						if (tranData.TranStatus == V2.CCTranStatus.Approved)
						{
							GetOrCreatePaymentProfileByTran(tranData, pDoc);
						}
						RemoveSyncLock(doc);
						RecordCapture(pDoc, tranData);
					}

					if (tranType == V2.CCTranType.CaptureOnly)
					{
						if (tranData.TranStatus == V2.CCTranStatus.Approved)
						{
							GetOrCreatePaymentProfileByTran(tranData, pDoc);
						}
						RemoveSyncLock(doc);
						RecordCapture(pDoc, tranData);
					}
				}

				//Finalize not found transactions in processing center, but esists in acumatica
				FinalizeNotFoundTran();
				scope.Complete();
			}
		}

		private void FinalizeNotFoundTran()
		{
			IEnumerable<IExternalTransaction> externalTransactions = GetExtTrans();

			foreach (IExternalTransaction extTran in externalTransactions)
			{
				ExternalTransaction externalTransaction = GetPaymentRepository().GetExternalTransaction(extTran.TransactionID);
				IEnumerable<CCProcTran> ccProcTransactions = GetPaymentRepository().GetCCProcTranByTranID(extTran.TransactionID);

				foreach (CCProcTran ccProcTran in ccProcTransactions)
				{
					if(ccProcTran.ProcStatus == CCProcStatus.Opened)
					{
						ccProcTran.ProcStatus = CCProcStatus.Finalized;
						ccProcTran.TranStatus = CCTranStatusCode.Error;
						ccProcTran.PCResponseReasonText = AR.Messages.ERR_CCProcessingCenterPCResponseReasonNotExists;
						ccProcTran.ErrorText = AR.Messages.ERR_CCProcessingCenterNotContainsTranWithID;
						GetPaymentRepository().UpdateTransaction(ccProcTran, externalTransaction);
					}
				}
			}
		}

		protected virtual bool SuitableCustomerProfileId(string customerId)
		{
			bool ret = true;
			if (customerId != null)
			{
				var query = new PXSelect<CustomerPaymentMethod, Where<CustomerPaymentMethod.customerCCPID, Equal<Required<CustomerPaymentMethod.customerCCPID>>,
					And<CustomerPaymentMethod.cCProcessingCenterID, Equal<Required<CustomerPaymentMethod.cCProcessingCenterID>>>>>(Base);
				CustomerPaymentMethod cpm = query.SelectSingle(customerId, this.SelectedProcessingCenter);
				if (cpm != null && cpm.BAccountID != SelectedBAccount)
				{
					ret = false;
				}
			}
			return ret;
		}

		protected virtual int? GetOrCreatePaymentProfileByTran(V2.TransactionData tranData, ICCPayment pDoc)
		{
			if (pDoc.PMInstanceID != PaymentTranExtConstants.NewPaymentProfile)
			{
				return pDoc.PMInstanceID;
			}

			int? instanceID = PaymentTranExtConstants.NewPaymentProfile;

			V2.TranProfile profile = null;
			if (tranData.CustomerId != null && tranData.PaymentId != null)
			{
				profile = new V2.TranProfile()
				{ CustomerProfileId = tranData.CustomerId, PaymentProfileId = tranData.PaymentId };
			}

			if (!NeedSaveCard() || !CheckAllowSavingCards())
			{
				if (profile != null)
				{
					instanceID = GetInstanceId(profile);
				}
				pDoc.PMInstanceID = instanceID;
				return instanceID;
			}

			var creator = GetPaymentProfileCreator();
			try
			{
				CustomerPaymentMethod cpm = creator.PrepeareCpmRecord();

				if (profile == null)
				{
					profile = GetOrCreateCustomerProfileByTranId(cpm, tranData.TranID);
				}

				instanceID = GetInstanceId(profile);

				if (instanceID == PaymentTranExtConstants.NewPaymentProfile)
				{
					instanceID = creator.CreatePaymentProfile(profile);
				}
				creator.CreateCustomerProcessingCenterRecord(profile);
			}
			finally
			{
				creator.ClearCaches();
			}
			pDoc.PMInstanceID = instanceID;
			return instanceID;
		}

		public override void Initialize()
		{
			base.Initialize();
			CheckSyncLockOnPersist = true;
			retryUnsettledTran = new RetryPolicy<IEnumerable<V2.TransactionData>>();
			retryUnsettledTran.RetryCnt = 1;
			retryUnsettledTran.StaticSleepDuration = 6000;
		}

		protected void CreateCustomerProcessingCenterRecord(V2.TranProfile input)
		{
			PXCache customerProcessingCenterCache = Base.Caches[typeof(CustomerProcessingCenterID)];
			customerProcessingCenterCache.ClearQueryCacheObsolete();
			PXSelectBase<CustomerProcessingCenterID> checkRecordExist = new PXSelectReadonly<CustomerProcessingCenterID,
				Where<CustomerProcessingCenterID.cCProcessingCenterID, Equal<Required<CustomerProcessingCenterID.cCProcessingCenterID>>,
				And<CustomerProcessingCenterID.bAccountID, Equal<Required<CustomerProcessingCenterID.bAccountID>>,
				And<CustomerProcessingCenterID.customerCCPID, Equal<Required<CustomerProcessingCenterID.customerCCPID>>>>>>(Base);

			CustomerProcessingCenterID cProcessingCenter = checkRecordExist.SelectSingle(SelectedProcessingCenter, SelectedBAccount, input.CustomerProfileId);

			if (cProcessingCenter == null)
			{
				cProcessingCenter = customerProcessingCenterCache.CreateInstance() as CustomerProcessingCenterID;
				cProcessingCenter.BAccountID = SelectedBAccount;
				cProcessingCenter.CCProcessingCenterID = SelectedProcessingCenter;
				cProcessingCenter.CustomerCCPID = input.CustomerProfileId;
				customerProcessingCenterCache.Insert(cProcessingCenter);
				customerProcessingCenterCache.Persist(PXDBOperation.Insert);
			}
		}

		protected int? GetInstanceId(V2.TranProfile input)
		{
			int? instanceID = PaymentTranExtConstants.NewPaymentProfile;
			PXCache cpmCache = Base.Caches[typeof(CustomerPaymentMethod)];
			cpmCache.ClearQueryCacheObsolete();
			var repo = GetPaymentProcessing().Repository;
			var result = repo.GetCustomerPaymentMethodWithProfileDetail(SelectedProcessingCenter, input.CustomerProfileId, input.PaymentProfileId);
			if (result != null && result.Item1.BAccountID == SelectedBAccount)
			{
				instanceID = result.Item1.PMInstanceID;
			}
			return instanceID;
		}

		protected V2.TranProfile GetOrCreateCustomerProfileByTranId(CustomerPaymentMethod cpm, string tranId)
		{
			PXSelectBase<CustomerPaymentMethod> query = new PXSelectReadonly<CustomerPaymentMethod,
				Where<CustomerPaymentMethod.bAccountID, Equal<Required<CustomerPaymentMethod.bAccountID>>,
					And<CustomerPaymentMethod.cCProcessingCenterID, Equal<Required<CustomerPaymentMethod.cCProcessingCenterID>>>>,
				OrderBy<Desc<CustomerPaymentMethod.createdDateTime>>>(Base);

			IEnumerable<CustomerPaymentMethod> cpmRes = query.Select(SelectedBAccount, SelectedProcessingCenter).RowCast<CustomerPaymentMethod>();
			CustomerPaymentMethod searchCpm = cpmRes.FirstOrDefault();
			if (searchCpm != null)
			{
				cpm.CustomerCCPID = searchCpm.CustomerCCPID;
			}

			PXSelect<CustomerPaymentMethod> cpmNew = new PXSelect<CustomerPaymentMethod>(Base);
			V2.TranProfile ret = null;
			try
			{
				cpmNew.Insert(cpm);
				CCCustomerInformationManagerGraph infoManagerGraph = PXGraph.CreateInstance<CCCustomerInformationManagerGraph>();
				GenericCCPaymentProfileAdapter<CustomerPaymentMethod> cpmAdapter =
				new GenericCCPaymentProfileAdapter<CustomerPaymentMethod>(cpmNew);
				ret = infoManagerGraph.GetOrCreatePaymentProfileByTran(Base, cpmAdapter, tranId);
			}
			finally
			{
				cpmNew.Cache.Clear();
			}
			return ret;
		}

		protected V2.TransactionData GetTranData(string tranId)
		{
			V2.TransactionData tranData = GetPaymentProcessing().GetTransactionById(tranId, SelectedProcessingCenter);
			return tranData;
		}

		protected Customer GetCustomerByAccountId(int? id)
		{
			Customer customer = PXSelect<Customer, Where<Customer.bAccountID, Equal<Required<Customer.bAccountID>>>>.Select(Base, id);
			return customer;
		}

		protected virtual bool IsSupportPaymentHostedForm(string processingCenterId)
		{
			if (processingCenterId != checkedProcessingCenter)
			{
				CCProcessingCenter procCenter = GetProcessingCenterById(processingCenterId);
				checkedProcessingCenterResult = IsFeatureSupported(processingCenterId, CCProcessingFeature.PaymentHostedForm, true);
				checkedProcessingCenter = processingCenterId;
			}
			return checkedProcessingCenterResult;
		}

		private List<V2.TransactionData> GetTransByDoc(ICCPayment payment, IEnumerable<V2.TransactionData> trans)
		{
			string searchDocNum = payment.DocType + payment.RefNbr;
			List<V2.TransactionData> targetTran = trans.Where(i => i.DocNum == searchDocNum).ToList();
			return targetTran;
		}

		private IEnumerable<string> PrepareTransactionIds(List<V2.TransactionData> list)
		{
			return list.OrderBy(i => i.SubmitTime).Select(i => i.TranID);
		}

		private bool FindPreAuthorizing()
		{
			ExternalTransactionState state = GetActiveTransactionState();
			return state.IsPreAuthorized ? true : false;
		}

		private bool TranHeldForReview()
		{ 
			ExternalTransactionState state = GetActiveTransactionState();
			return state.IsOpenForReview ? true : false;
		}

		protected override void RowSelected(Events.RowSelected<TPrimary> e)
		{
			base.RowSelected(e);
			var doc = PaymentDoc.Current;
			if (doc?.ProcessingCenterID != null && ExternalTranHelper.GetActiveTransaction(GetExtTrans()) == null)
			{
				CCProcessingCenter procCenter = GetProcessingCenterById(doc.ProcessingCenterID);
				if (procCenter != null && procCenter.UseAcceptPaymentForm == false)
				{
					PaymentDoc.Cache.RaiseExceptionHandling<Payment.processingCenterID>(doc, doc.ProcessingCenterID,
						new PXSetPropertyException(CA.Messages.AcceptPaymentFromNewCardDisabledWarning, PXErrorLevel.Warning, doc.ProcessingCenterID));
				}
			}
		}

		protected virtual TPrimary GetDocWithoutChanges(TPrimary input)
		{
			return null;
		}
	}
}