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
		protected int? SelectedBAccount;
		protected string SelectedPaymentMethod;
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
					CheckPaymentTranForceSync(doc);
					RemoveSyncLock(doc);
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
			System.Web.HttpRequest request = System.Web.HttpContext.Current.Request;
			bool isCancel = false;
			var cancelStr = request.Form.Get("__CLOSECCHFORM");
			if (cancelStr != null && bool.TryParse(cancelStr, out isCancel) && isCancel)
			{
				RemoveSyncLock(doc);
				return adapter.Get();
			}
			var tranResponseStr = request.Form.Get("__TRANID");
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

			PXLongOperation.StartOperation(Base, delegate
			{
				SyncPaymentTransactionById(doc, new List<string>() { tranId });
			});
			return adapter.Get();
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
			if (doc != null && adapter.ExternalCall && LockExists(doc))
			{
				WebDialogResult result = PaymentTransaction.Ask(AR.Messages.CCProcessingARPaymentAlreadyProcessed, MessageButtons.OKCancel);
				if (result == WebDialogResult.No)
				{
					throw new PXException(AR.Messages.CCProcessingOperationCancelled);
				}
			}
		}

		private void CheckPaymentTranForceSync(TPrimary doc)
		{
			CheckPaymentTransaction(doc);
			IExternalTransaction storedTran = GetExtTrans().FirstOrDefault();
			bool needSyncUnsettled = false;
			if (storedTran != null && ExternalTranHelper.GetTransactionState(Base, storedTran).IsActive)
			{
				SyncPaymentTransactionById(doc, new List<string>() { storedTran.TranNumber });
				ClearTransactionСaches();
				storedTran = GetExtTrans().FirstOrDefault();
				if (storedTran?.Active == false)
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
				IEnumerable<string> result = PrepeareTransactionIds(GetTransByDoc(pDoc, trans));
				SyncPaymentTransactionById(doc, result);
			}
		}

		private void CheckPaymentTransaction(TPrimary doc)
		{
			if (!IsFeatureSupported(SelectedProcessingCenter, CCProcessingFeature.TransactionGetter))
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
				IEnumerable<string> result = PrepeareTransactionIds(GetTransByDoc(pDoc, trans));
				SyncPaymentTransactionById(doc, result);
			}
			ClearTransactionСaches();

			IExternalTransaction tran = ExternalTranHelper.GetActiveTransaction(GetExtTrans());
			if (tran != null)
			{
				TranStatusChanged(pDoc, tran.TransactionID);
			}
		}

		public virtual void SyncPaymentTransactionById(TPrimary doc, IEnumerable<string> tranIds)
		{
			if (!IsFeatureSupported(SelectedProcessingCenter, CCProcessingFeature.PaymentHostedForm))
				return;
			ICCPayment pDoc = GetPaymentDoc(doc);
			using (PXTransactionScope scope = new PXTransactionScope())
			{
				foreach (string tranId in tranIds)
				{
					IEnumerable<PaymentTransactionDetail> existsTran = base.PaymentTransaction.Select().RowCast<PaymentTransactionDetail>();
					PaymentTransactionDetail storedTran = existsTran.FirstOrDefault(i => i.PCTranNumber == tranId);
					if (storedTran != null && storedTran.TranStatus == CCTranStatusCode.Approved)
					{
						continue;
					}
					var tranData = GetTranData(tranId);
					if (storedTran != null && CCTranStatusCode.GetCode(V2Converter.ConvertTranStatus(tranData.TranStatus)) == storedTran.TranStatus)
					{
						continue;
					}
					if (tranData?.CustomerId != null && !SuitableCustomerProfileId(tranData?.CustomerId))
					{
						continue;
					}

					try
					{
						PXTrace.WriteInformation($"Synchronize tran. TranId = {tranData.TranID}, TranType = {tranData.TranType}, DocNum = {tranData.DocNum}, " +
							$"SubmitTime = {tranData.SubmitTime}, Amount = {tranData.Amount}, PCCustomerID = {tranData.CustomerId}, PCCustomerPaymentID = {tranData.PaymentId}");
					}
					catch (Exception ex)
					{
						var v = ex;
					}
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
							GetOrCreatePaymentProfilrByTran(tranData, pDoc);
						}
						RemoveSyncLock(doc);
						RecordAuth(pDoc, tranData);
					}

					if (tranType == V2.CCTranType.AuthorizeAndCapture)
					{
						if (tranData.TranStatus == V2.CCTranStatus.Approved)
						{
							GetOrCreatePaymentProfilrByTran(tranData, pDoc);
						}
						RemoveSyncLock(doc);
						RecordCapture(pDoc, tranData);
					}

					if (tranType == V2.CCTranType.CaptureOnly)
					{
						if (tranData.TranStatus == V2.CCTranStatus.Approved)
						{
							GetOrCreatePaymentProfilrByTran(tranData, pDoc);
						}
						RemoveSyncLock(doc);
						RecordCapture(pDoc, tranData);
					}
					scope.Complete();
				}
			}
		}

		protected virtual bool SuitableCustomerProfileId(string customerId)
		{
			bool ret = true;
			if (customerId != null)
			{
				var query = new PXSelect<CustomerPaymentMethod,Where<CustomerPaymentMethod.customerCCPID,Equal<Required<CustomerPaymentMethod.customerCCPID>>,
					And<CustomerPaymentMethod.cCProcessingCenterID,Equal<Required<CustomerPaymentMethod.cCProcessingCenterID>>>>>(Base);
				CustomerPaymentMethod cpm = query.SelectSingle(customerId, this.SelectedProcessingCenter);
				if (cpm != null && cpm.BAccountID != SelectedBAccount)
				{
					ret = false;
				}
			}
			return ret;
		}

		protected virtual int? GetOrCreatePaymentProfilrByTran(V2.TransactionData tranData, ICCPayment pDoc)
		{
			int? instanceID;
			try
			{
				V2.TranProfile profile = null;
				if (tranData.CustomerId != null && tranData.PaymentId != null)
				{
					profile = new V2.TranProfile()
					{ CustomerProfileId = tranData.CustomerId, PaymentProfileId = tranData.PaymentId };
					ICCPaymentProfile cpm = null;
					try
					{
						cpm = PrepeareCpmRecord();
					}
					catch (Exception ex)
					{
						var v = ex;
					}
					cpm.CustomerCCPID = tranData.CustomerId;
				}

				ExternalTransactionState state = GetActiveTransactionState();
				if (profile == null && state.IsPreAuthorized)
				{
					profile = GetCustomerProfileFromDoc(pDoc);
				}

				if (profile == null)
				{
					profile = GetOrCreateCustomerProfileByTranId(tranData.TranID);
				}

				instanceID = GetInstanceId(profile);

				using (PXTransactionScope tran = new PXTransactionScope())
				{
					if (instanceID == PaymentTranExtConstants.NewPaymentProfile)
					{
						instanceID = CreatePaymentProfile(profile);
					}
					CreateCustomerProcessingCenterRecord(profile);
					tran.Complete();
				}
			}
			finally
			{
				NewCpm.Cache.Clear();
				NewCpmd.Cache.Clear();
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

		protected int? CreatePaymentProfile(V2.TranProfile input)
		{
			var cpmSelect = NewCpm;
			var cpmdSelect = NewCpmd;
			var pmdSelect = PaymentMethodDet;
			foreach (PaymentMethodDetail item in pmdSelect.Select())
			{
				CustomerPaymentMethodDetail cpmd = new CustomerPaymentMethodDetail();
				cpmd.PaymentMethodID = SelectedPaymentMethod;
				cpmd.DetailID = item.DetailID;
				if (item.IsCCProcessingID == true)
				{
					cpmd.Value = input.PaymentProfileId;
				}
				cpmdSelect.Insert(cpmd);
			}

			CCCustomerInformationManagerGraph infoManagerGraph = PXGraph.CreateInstance<CCCustomerInformationManagerGraph>();
			GenericCCPaymentProfileAdapter<CustomerPaymentMethod> cpmAdapter = new GenericCCPaymentProfileAdapter<CustomerPaymentMethod>(cpmSelect);
			var cpmdAdapter = new GenericCCPaymentProfileDetailAdapter<CustomerPaymentMethodDetail, PaymentMethodDetail>(cpmdSelect, pmdSelect);
			infoManagerGraph.GetPaymentProfile(Base, cpmAdapter, cpmdAdapter);
			var cardIdent = pmdSelect.Select().RowCast<PaymentMethodDetail>().Where(i => i.IsIdentifier == true).First();
			var detailWithCardNum = cpmdSelect.Select().RowCast<CustomerPaymentMethodDetail>().Where(i => i.DetailID == cardIdent.DetailID).First();
			Match match = new Regex("[\\d]+").Match(detailWithCardNum.Value);

			CustomerPaymentMethod cpm = cpmSelect.Select();
			if (match.Success)
			{
				cpm.Descr = SelectedPaymentMethod + ":" + maskedCardTmpl + match.Value;
			}

			cpmSelect.Cache.Persist(PXDBOperation.Insert);
			cpmdSelect.Cache.Persist(PXDBOperation.Insert);
			return cpm.PMInstanceID;
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
			PXSelectBase<CustomerPaymentMethod> query = new PXSelectReadonly2<CustomerPaymentMethod,
				InnerJoin<CustomerPaymentMethodDetail, On<CustomerPaymentMethod.pMInstanceID, Equal<CustomerPaymentMethodDetail.pMInstanceID>>,
				InnerJoin<PaymentMethodDetail, On<CustomerPaymentMethodDetail.detailID, Equal<PaymentMethodDetail.detailID>,
					And<CustomerPaymentMethodDetail.paymentMethodID, Equal<PaymentMethodDetail.paymentMethodID>>>>>,
				Where<CustomerPaymentMethod.cCProcessingCenterID, Equal<Required<CustomerPaymentMethod.cCProcessingCenterID>>,
				And<CustomerPaymentMethod.customerCCPID, Equal<Required<CustomerPaymentMethod.customerCCPID>>,
					And<PaymentMethodDetail.isCCProcessingID, Equal<True>, And<PaymentMethodDetail.useFor, Equal<PaymentMethodDetailUsage.useForARCards>>>>>>(Base);

			PXResultset<CustomerPaymentMethod> queryResult = query.Select(SelectedProcessingCenter, input.CustomerProfileId);

			foreach (PXResult<CustomerPaymentMethod, CustomerPaymentMethodDetail> item in queryResult)
			{
				CustomerPaymentMethodDetail checkCpmd = (CustomerPaymentMethodDetail)item;

				if (checkCpmd.Value == input.PaymentProfileId)
				{
					instanceID = checkCpmd.PMInstanceID;
					break;
				}
			}
			CustomerPaymentMethod cpm = NewCpm.Select();
			if (cpm != null && cpm.PMInstanceID != null && cpm.PMInstanceID >= 0)
			{
				instanceID = cpm.PMInstanceID.Value;
			}
			return instanceID;
		}

		protected V2.TranProfile GetOrCreateCustomerProfileByTranId(string tranId)
		{
			PXSelectBase<CustomerPaymentMethod> query = new PXSelectReadonly<CustomerPaymentMethod,
				Where<CustomerPaymentMethod.bAccountID, Equal<Required<CustomerPaymentMethod.bAccountID>>,
					And<CustomerPaymentMethod.cCProcessingCenterID, Equal<Required<CustomerPaymentMethod.cCProcessingCenterID>>>>,
				OrderBy<Desc<CustomerPaymentMethod.createdDateTime>>>(Base);

			IEnumerable<CustomerPaymentMethod> cpmRes = query.Select(SelectedBAccount, SelectedProcessingCenter).RowCast<CustomerPaymentMethod>();
			CustomerPaymentMethod searchCpm = cpmRes.FirstOrDefault();
			ICCPaymentProfile cpm = PrepeareCpmRecord();
			if (searchCpm != null)
			{
				cpm.CustomerCCPID = searchCpm.CustomerCCPID;
			}

			CCCustomerInformationManagerGraph infoManagerGraph = PXGraph.CreateInstance<CCCustomerInformationManagerGraph>();
			GenericCCPaymentProfileAdapter<CustomerPaymentMethod> cpmAdapter = 
				new GenericCCPaymentProfileAdapter<CustomerPaymentMethod>(NewCpm);
			V2.TranProfile ret = infoManagerGraph.GetOrCreatePaymentProfileByTran(Base, cpmAdapter, tranId);
			cpm.CustomerCCPID = ret.CustomerProfileId;
			return ret;
		}

		protected V2.TranProfile GetCustomerProfileFromDoc(ICCPayment pDoc)
		{
			V2.TranProfile ret = null;
			if(pDoc.PMInstanceID > 0)
			{
				var query = new PXSelectJoin<CustomerPaymentMethod,
					InnerJoin<CustomerPaymentMethodDetail,On<CustomerPaymentMethodDetail.pMInstanceID,Equal<CustomerPaymentMethod.pMInstanceID>>,
					InnerJoin<PaymentMethodDetail,On<PaymentMethodDetail.detailID,Equal<CustomerPaymentMethodDetail.detailID>>>>,
					Where<CustomerPaymentMethod.pMInstanceID,Equal<Required<CustomerPaymentMethod.pMInstanceID>>,
						And<PaymentMethodDetail.isCCProcessingID,Equal<True>>>>(Base);
				PXResultset<CustomerPaymentMethod> result = query.Select(pDoc.PMInstanceID);

				foreach (PXResult<CustomerPaymentMethod, CustomerPaymentMethodDetail> item in result)
				{
					CustomerPaymentMethod cpm = (CustomerPaymentMethod)item;
					CustomerPaymentMethodDetail cpmDet = (CustomerPaymentMethodDetail)item;
					ret = new V2.TranProfile() { CustomerProfileId = cpm.CustomerCCPID, PaymentProfileId = cpmDet.Value };
					break;
				}
			}
			return ret;
		}

		public PXSelect<CustomerPaymentMethod> NewCpm;
		public virtual IEnumerable newCpm()
		{
			PXCache cache = Base.Caches[typeof(CustomerPaymentMethod)];
			foreach (object item in cache.Cached)
			{
				CustomerPaymentMethod cpm = item as CustomerPaymentMethod;
				if (cache.GetStatus(item) == PXEntryStatus.Inserted && cpm.PMInstanceID < 0)
				{
					yield return item;
				}
			}
			yield break;
		}

		public PXSelect<CustomerPaymentMethodDetail> NewCpmd;
		public virtual IEnumerable newCpmd()
		{
			PXCache cache = Base.Caches[typeof(CustomerPaymentMethodDetail)];
			foreach (object item in cache.Cached)
			{
				CustomerPaymentMethodDetail cpm = item as CustomerPaymentMethodDetail;
				if (cache.GetStatus(item) == PXEntryStatus.Inserted)
				{
					yield return item;
				}
			}
			yield break;
		}

		public PXSelect<PaymentMethodDetail> PaymentMethodDet;
		public virtual IEnumerable paymentMethodDet()
		{
			PXSelectBase<PaymentMethodDetail> query = new PXSelect<PaymentMethodDetail,
				Where<PaymentMethodDetail.paymentMethodID,Equal<Required<PaymentMethodDetail.paymentMethodID>>>>(Base);
			var result = query.Select(SelectedPaymentMethod);
			return result;
		}

		protected CustomerPaymentMethod PrepeareCpmRecord()
		{
			CustomerPaymentMethod cpm = NewCpm.Select();
			if (cpm == null)
			{
				cpm = new CustomerPaymentMethod();
				cpm.PaymentMethodID = SelectedPaymentMethod;
				cpm.CCProcessingCenterID = SelectedProcessingCenter;
				cpm = NewCpm.Insert(cpm);
			}
			return cpm;
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

		protected bool IsSupportPaymentHostedForm(string processingCenterId)
		{
			if (processingCenterId != checkedProcessingCenter)
			{
				CCProcessingCenter procCenter = GetProcessingCenterById(processingCenterId);
				checkedProcessingCenterResult = IsFeatureSupported(processingCenterId, CCProcessingFeature.PaymentHostedForm);
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

		private IEnumerable<string> PrepeareTransactionIds(List<V2.TransactionData> list)
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

		protected virtual TPrimary GetDocWithoutChanges(TPrimary input)
		{
			return null;
		}
	}
}