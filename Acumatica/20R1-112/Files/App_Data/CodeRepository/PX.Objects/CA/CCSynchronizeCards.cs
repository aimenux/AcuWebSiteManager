using PX.Data;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using PX.Objects.AR.CCPaymentProcessing.Common;
using PX.Objects.AR.CCPaymentProcessing.Helpers;
using System.Text.RegularExpressions;
using PX.CCProcessingBase.Interfaces.V2;
using PX.Objects.AR.CCPaymentProcessing.CardsSynchronization;
using PX.Web.UI;
using PX.Objects.AR;

namespace PX.Objects.CA
{
	public class CCSynchronizeCards : PXGraph<CCSynchronizeCards>
	{
		public PXCancel<CreditCardsFilter> Cancel;
		public PXSave<CreditCardsFilter> Save;

		public PXAction<CreditCardsFilter> LoadCards;
		[PXProcessButton(CommitChanges = true)]
		[PXUIField(DisplayName = "Load Card Data")]
		protected virtual IEnumerable loadCards(PXAdapter adapter)
		{
			CreditCardsFilter filter = Filter.Current;
			filter.EnableCustomerPaymentDialog = false;
			CCProcessingHelper.CheckHttpsConnection();

			if (!ValidateLoadCardsAction())
			{
				int rowCnt = CustomerCardPaymentData.Select().Count;

				if (rowCnt == 0 || !adapter.ExternalCall
					|| Filter.Ask(CA.Messages.RelodCardDataDialogMsg, MessageButtons.YesNo) == WebDialogResult.Yes)
				{
					PXLongOperation.StartOperation(this, delegate
					{
						int newCardsCnt = GetCardsAllProfiles();
						if (newCardsCnt > 0)
						{
							this.Persist();
						}
					});
				}
			}
			return adapter.Get();
		}

		public PXAction<CreditCardsFilter> SetDefaultPaymentMethod;
		[PXButton(CommitChanges = true)]
		[PXUIField(DisplayName = "Set Payment Method")]
		protected virtual IEnumerable setDefaultPaymentMethod(PXAdapter adapter)
		{
			CreditCardsFilter filter = Filter.Current;

			if (!adapter.ExternalCall || PMFilter.AskExt((graph, vName) => filter.OverwritePaymentMethod = false) == WebDialogResult.OK)
			{
				string paymentMethod = filter.PaymentMethodId;
				PXFilterRow[] filterRows = CustomerCardPaymentData.View.GetExternalFilters();
				int startRow = 0;
				int totalRows = 0;
				IEnumerable<CCSynchronizeCard> retList = CustomerCardPaymentData.View.Select(null, null, null, null, null, filterRows, ref startRow, 0, ref totalRows).OfType<CCSynchronizeCard>();

				foreach (CCSynchronizeCard item in retList)
				{
					if (filter.OverwritePaymentMethod.GetValueOrDefault() == true || item.PaymentMethodID == null)
					{
						item.PaymentMethodID = paymentMethod;
						CustomerCardPaymentData.Update(item);
					}
				}
			}
			return adapter.Get();
		}

		public PXAction<CreditCardsFilter> ViewCustomer;
		[PXButton]
		protected virtual void viewCustomer()
		{
			CCSynchronizeCard syncCard = CustomerCardPaymentData.Current;
			CustomerMaint customer = CreateInstance<CustomerMaint>();
			PXSelectBase<Customer> customerQuery = new PXSelect<Customer,
				Where<Customer.bAccountID, Equal<Required<Customer.bAccountID>>>>(this);
			customer.CurrentCustomer.Current = customerQuery.SelectSingle(syncCard.BAccountID);

			if (customer.CurrentCustomer.Current != null)
			{
				throw new PXRedirectRequiredException(customer, true, string.Empty);
			}

		}

		public PXAction<CreditCardsFilter> GroupAction;
		[PXButton]
		[PXUIField(DisplayName = "Actions")]
		protected virtual IEnumerable groupAction(PXAdapter adapter, [PXString] string ActionName)
		{
			return adapter.Get();
		}

		public PXFilter<CreditCardsFilter> Filter;
		public PXFilter<CreditCardsFilter> PMFilter;

		public PXSelect<CustomerPaymentMethodDetail, Where<True, Equal<False>>> DummyCPMD;
		public CCSyncFilteredProcessing<CCSynchronizeCard, CreditCardsFilter,
			Where<CCSynchronizeCard.cCProcessingCenterID, Equal<Current<CreditCardsFilter.processingCenterId>>>,
			OrderBy<Asc<CCSynchronizeCard.customerCCPID>>> CustomerCardPaymentData;
		public IEnumerable customerCardPaymentData()
		{
			PXResultset<CCSynchronizeCard> resultSet = new PXResultset<CCSynchronizeCard>();
			CreditCardsFilter filter = Filter.Current;

			if (filter.ProcessingCenterId != null)
			{
				resultSet = GetSyncCardsByProcCenter(filter.ProcessingCenterId);
				foreach (CCSynchronizeCard res in resultSet)
				{
					yield return res;
				}
			}
		}

		public PXSelect<CustomerPaymentProfile> CustPaymentProfileForDialog;
		public IEnumerable custPaymentProfileForDialog()
		{
			foreach (CustomerPaymentProfile item in CustPaymentProfileForDialog.Cache.Cached)
			{
				yield return item;
			}
		}

		private const string maskedCardTmpl = "****-****-****-";
		private const string cachedCardDataKeyPostfix = "SyncInfoKey";

		List<CCSynchronizeCard> cacheRecordsSameCustomerCCPID;

		[Serializable]
		public class CustomerPaymentProfile : IBqlTable
		{
			public abstract class recordID : IBqlField
			{
			}
			[PXInt(IsKey = true)]
			public virtual int? RecordID { get; set; }

			public abstract class bAccountID : IBqlField
			{
			}
			[PXInt]
			[PXUIField(Visible = false)]
			public virtual int? BAccountID { get; set; }

			public abstract class customerCCPID : IBqlField
			{
			}
			[PXString]
			[PXUIField(DisplayName = "PC Cust. Profile ID", Enabled = false)]
			public virtual string CustomerCCPID { get; set; }

			public abstract class pCCustomerID : IBqlField
			{
			}
			[PXString]
			[PXUIField(DisplayName = "PC Cust. ID", Enabled = false)]
			public virtual string PCCustomerID { get; set; }

			public abstract class pCCustomerDescription : IBqlField
			{
			}
			[PXString]
			[PXUIField(DisplayName = "PC Cust. Descr.", Enabled = false)]
			public virtual string PCCustomerDescription { get; set; }

			public abstract class pCCustomerEmail : IBqlField
			{
			}
			[PXString]
			[PXUIField(DisplayName = "PC Cust. Email", Enabled = false)]
			public virtual string PCCustomerEmail { get; set; }

			public abstract class paymentCCPID : IBqlField
			{
			}
			[PXString]
			[PXUIField(DisplayName = "PC Payment Profile ID", Enabled = false)]
			public virtual string PaymentCCPID { get; set; }

			public abstract class paymentProfileFirstName : IBqlField
			{
			}
			[PXString]
			[PXUIField(DisplayName = "PC Payment Profile First Name", Enabled = false)]
			public virtual string PaymentProfileFirstName { get; set; }

			public abstract class paymentProfileLastName : IBqlField
			{
			}
			[PXString]
			[PXUIField(DisplayName = "PC Payment Profile Last Name", Enabled = false)]
			public virtual string PaymentProfileLastName { get; set; }

			public abstract class setPaymentProfile : IBqlField { }
			[PXBool]
			[PXUIField(DisplayName = "Selected")]
			public virtual bool? Selected { get; set; }

			public static CustomerPaymentProfile CreateFromSyncCard(CCSynchronizeCard syncCard)
			{
				CustomerPaymentProfile ret = new CustomerPaymentProfile()
				{
					RecordID = syncCard.RecordID,
					CustomerCCPID = syncCard.CustomerCCPID,
					PCCustomerDescription = syncCard.PCCustomerDescription,
					PCCustomerEmail = syncCard.PCCustomerEmail,
					PCCustomerID = syncCard.PCCustomerID,
					BAccountID = syncCard.BAccountID,
					PaymentProfileFirstName = syncCard.FirstName,
					PaymentProfileLastName = syncCard.LastName,
					PaymentCCPID = syncCard.PaymentCCPID
				};
				return ret;
			}
		}

		[Serializable]
		public class CreditCardsFilter : IBqlTable
		{
			public abstract class processingCenterId : IBqlField { }
			[PXString]
			[CCProcessingCenterSelector(CCProcessingFeature.ExtendedProfileManagement)]
			[PXUIField(DisplayName = "Processing Center")]
			public virtual string ProcessingCenterId { get; set; }

			public abstract class scheduledServiceSync : IBqlField { }
			[PXBool]
			[PXUnboundDefault(false)]
			[PXUIField(DisplayName = "Scheduled Sync")]
			public virtual bool? ScheduledServiceSync { get; set; }

			public abstract class loadExpiredCard : IBqlField { }
			[PXBool]
			[PXUnboundDefault(false)]
			[PXUIField(DisplayName = "Load Expired Card Data")]
			public virtual bool? LoadExpiredCards { get; set; }

			public abstract class paymentMethodId : IBqlField { }
			[PXString]
			[PXSelector(typeof(Search4<
				CCProcessingCenterPmntMethod.paymentMethodID,
				Where<CCProcessingCenterPmntMethod.processingCenterID, Equal<Current<CreditCardsFilter.processingCenterId>>>,
				Aggregate<
					GroupBy<CCProcessingCenterPmntMethod.paymentMethodID>>>))]
			[PXUIField(DisplayName = "Payment Method")]
			public virtual string PaymentMethodId { get; set; }

			public abstract class overwritePaymentMethod : IBqlField
			{
			}
			[PXBool]
			[PXUIField(DisplayName = "Overwrite Values")]
			public virtual bool? OverwritePaymentMethod { get; set; }

			public abstract class enableMultipleSettingCustomer : IBqlField { }
			[PXBool]
			public virtual bool? EnableCustomerPaymentDialog { get; set; }

			[PXBool]
			[PXUnboundDefault(false)]
			public virtual bool? IsScheduleProcess { get; set; }

			public abstract class customerName : IBqlField { }
			[PXUIField(DisplayName = "Select the payment profiles to be assigned to the customer", Enabled = false)]
			public virtual string CustomerName { get; set; }
		}
		public CCSynchronizeCards()
		{
			CustomerCardPaymentData.SetBeforeScheduleAddAction(() =>
				Filter.Current.ScheduledServiceSync = true
			);
			CustomerCardPaymentData.SetAfterScheduleAddAction(() =>
				Filter.Current.ScheduledServiceSync = false
			);
			CustomerCardPaymentData.SetBeforeScheduleProcessAllAction(() =>
				Filter.Current.IsScheduleProcess = true
			);
			CreditCardsFilter filter = Filter.Current;
			CustomerCardPaymentData.SetProcessDelegate((List<CCSynchronizeCard> items) =>
			{

				if (filter.IsScheduleProcess.GetValueOrDefault() == true
					&& filter.ScheduledServiceSync.GetValueOrDefault() == true)
				{
					DoLoadCards(filter);
				}
				else
				{
					DoImportCards(items);
				}
			});
		}

		private static void DoLoadCards(CreditCardsFilter filter)
		{
			CCSynchronizeCards graph = PXGraph.CreateInstance<CCSynchronizeCards>();
			filter.EnableCustomerPaymentDialog = false;
			graph.Filter.Current = filter;

			if (!graph.ValidateLoadCardsAction())
			{
				int newCardsCnt = graph.GetCardsAllProfiles();

				if (newCardsCnt > 0)
				{
					foreach (CCSynchronizeCard syncCard in graph.CustomerCardPaymentData.Cache.Inserted)
					{
						if (syncCard.NoteID.HasValue)
						{
							ProcessingInfo.AppendProcessingInfo(syncCard.NoteID.Value, Messages.LoadCardCompleted);
						}
					}

					try
					{
						graph.Persist();
					}
					catch
					{
						ProcessingInfo.ClearProcessingRows();
						throw;
					}
				}
			}
		}

		private static void DoImportCards(List<CCSynchronizeCard> items)
		{
			int index = 0;
			string procCenterId = procCenterId = items.First()?.CCProcessingCenterID;
			CCSynchronizeCards graph = PXGraph.CreateInstance<CCSynchronizeCards>();

			foreach (CCSynchronizeCard item in items)
			{
				if (graph.ValidateImportCard(item, index) && !graph.CheckCustomerPaymentProfileExists(item, index))
				{
					using (PXTransactionScope scope = new PXTransactionScope())
					{
						graph.CreateCustomerPaymentMethodRecord(item);
						graph.UpdateCCProcessingSyncronizeCardRecord(item);
						scope.Complete();
						PXProcessing<CCSynchronizeCard>.SetInfo(index, Messages.Completed);
					}
				}
				index++;
			}
		}

		public void CreditCardsFilter_RowSelected(PXCache sender, PXRowSelectedEventArgs args)
		{
			CreditCardsFilter filter = args.Row as CreditCardsFilter;
			if (filter == null) return;

			CustomerCardPaymentData.AllowInsert = false;
			CustomerCardPaymentData.AllowDelete = false;
			PXButtonState buttonState = (PXButtonState)Actions["Process"].GetState(Filter.Current);
			bool enabled = buttonState.Visible && buttonState.Enabled;
			SetDefaultPaymentMethod.SetEnabled(enabled);
			LoadCards.SetEnabled(enabled);
			Save.SetEnabled(string.IsNullOrEmpty(filter.ProcessingCenterId) ? false : true);
			PXCache cache = Caches[typeof(CustomerPaymentMethod)];
			PXUIFieldAttribute.SetVisible<CustomerPaymentMethod.customerCCPID>(cache, null, true);
			PXUIFieldAttribute.SetVisible<CreditCardsFilter.scheduledServiceSync>(sender, null, false);
		}

		public virtual void CCSynchronizeCard_RowSelected(PXCache sender, PXRowSelectedEventArgs args)
		{
			PXUIFieldAttribute.SetEnabled<CCSynchronizeCard.bAccountID>(sender, args.Row);
			PXUIFieldAttribute.SetEnabled<CCSynchronizeCard.paymentMethodID>(sender, args.Row);
			PXUIFieldAttribute.SetEnabled<CCSynchronizeCard.cashAccountID>(sender, args.Row);
		}

		public virtual void CCSynchronizeCard_BAccountID_FieldDefaulting(PXCache cache, PXFieldDefaultingEventArgs e)
		{
			CCSynchronizeCard syncCard = e.Row as CCSynchronizeCard;
			if (syncCard == null) return;

			string customerCCPID = syncCard.CustomerCCPID;
			PXSelectBase<CustomerProcessingCenterID> cpcQuery = new PXSelect<CustomerProcessingCenterID,
				Where<CustomerProcessingCenterID.customerCCPID, Equal<Required<CustomerProcessingCenterID.customerCCPID>>>, 
				OrderBy<Desc<CustomerProcessingCenterID.createdDateTime>>>(this);
			CustomerProcessingCenterID customerPaymentMethod = cpcQuery.SelectSingle(customerCCPID);

			if (customerPaymentMethod != null)
			{
				e.NewValue = customerPaymentMethod.BAccountID;
				return;
			}

			string customerID = syncCard.PCCustomerID;
			if (customerID == null) return;

			customerID = DeleteCustomerPrefix(customerID);
			PXSelectBase<Customer> cQuery = new PXSelect<Customer,
				Where<Customer.acctCD, Equal<Required<CCSynchronizeCard.pCCustomerID>>>>(this);
			Customer customer = cQuery.SelectSingle(customerID);

			if (customer != null)
			{
				e.NewValue = customer.BAccountID;
			}
		}

		public virtual void CCSynchronizeCard_PaymentMethodId_FieldUpdated(PXCache cache, PXFieldUpdatedEventArgs e)
		{
			CCSynchronizeCard syncCard = e.Row as CCSynchronizeCard;

			if(syncCard?.CardNumber != null)
			{
				string cardNum = syncCard.CardNumber.Substring(syncCard.CardNumber.IndexOf(maskedCardTmpl) + maskedCardTmpl.Length);
				FormatMaskedCardNum(syncCard, cardNum);
			}
		}

		public virtual void CCSynchronizeCard_BAccountID_FieldUpdated(PXCache cache, PXFieldUpdatedEventArgs e)
		{
			CCSynchronizeCard syncCard = e.Row as CCSynchronizeCard;
			if(syncCard == null || !e.ExternalCall || syncCard.BAccountID == null)
				return;

			if (Filter.Current.EnableCustomerPaymentDialog.GetValueOrDefault() == false)
			{
				Filter.Current.EnableCustomerPaymentDialog = true;
				Filter.Current.CustomerName = GetCustomerNameByID(syncCard.BAccountID);
				CustPaymentProfileForDialog.Cache.Clear();
				int insertedCnt = PopulatePaymentProfileForDialog(syncCard.PCCustomerID, syncCard.BAccountID);

				if (insertedCnt > 0)
				{
					CustPaymentProfileForDialog.AskExt();
				}
			}

			if (Filter.Current.EnableCustomerPaymentDialog.GetValueOrDefault() == true)
			{
				Filter.Current.EnableCustomerPaymentDialog = false;
			}
		}

		public virtual void CustomerPaymentProfile_RowUpdated(PXCache cache, PXRowUpdatedEventArgs e)
		{
			CustomerPaymentProfile row = e.Row as CustomerPaymentProfile;
			if(row?.Selected == true)
			{ 
				foreach(CCSynchronizeCard syncCard in GetRecordsWithSameCustomerCCPID(row.PCCustomerID))
				{
					if (syncCard.RecordID == row.RecordID && !syncCard.BAccountID.HasValue)
					{
						syncCard.BAccountID = row.BAccountID;
						CustomerCardPaymentData.Update(syncCard);
						CustomerCardPaymentData.View.RequestRefresh();
					}
				}
			}
		}

		private int PopulatePaymentProfileForDialog(string customerID, int? bAccountID)
		{
			int insertedRow = 0;
			customerID = DeleteCustomerPrefix(customerID);
			PXResultset<CCSynchronizeCard> results = CustomerCardPaymentData.Select();

			foreach (CCSynchronizeCard item in results)
			{
				string chkCustromerID = DeleteCustomerPrefix(item.PCCustomerID);
				if (!item.BAccountID.HasValue && chkCustromerID == customerID)
				{
					CustomerPaymentProfile cpp = CustomerPaymentProfile.CreateFromSyncCard(item);
					cpp.BAccountID = bAccountID;
					CustPaymentProfileForDialog.Insert(cpp);
					insertedRow++;
				}
			}

			return insertedRow;
		}

		[PXMergeAttributes(Method = MergeMethod.Append)]
		[SyncCardCustomerSelector(typeof(Customer.acctCD), typeof(Customer.acctName), ValidateValue = false)]
		protected virtual void CCSynchronizeCard_BAccountID_CacheAttached(PXCache sender) { }

		[PXMergeAttributes(Method = MergeMethod.Replace)]
		[PXDefault(PersistingCheck = PXPersistingCheck.Nothing)]
		[PXRSACryptStringWithMaskAttribute(1028, typeof(Search<
			PaymentMethodDetail.entryMask,
			Where<PaymentMethodDetail.paymentMethodID, Equal<Current<CustomerPaymentMethodDetail.paymentMethodID>>,
				And<PaymentMethodDetail.useFor, Equal<PaymentMethodDetailUsage.useForARCards>,
				And<PaymentMethodDetail.detailID, Equal<Current<CustomerPaymentMethodDetail.detailID>>>>>>), IsUnicode = true)]
		protected virtual void CustomerPaymentMethodDetail_Value_CacheAttached(PXCache sender) { }


		private string GetCustomerNameByID(int? bAccountId)
		{
			string ret = string.Empty;
			PXSelectBase<Customer> query = new PXSelect<Customer, Where<Customer.bAccountID, Equal<Required<Customer.bAccountID>>>>(this);
			Customer customer = query.SelectSingle(bAccountId);

			if (customer != null)
			{
				ret = customer.AcctName;
			}
			return ret;
		}

		private List<CCSynchronizeCard> GetRecordsWithSameCustomerCCPID(string customerID)
		{
			customerID = DeleteCustomerPrefix(customerID);

			if (cacheRecordsSameCustomerCCPID == null)
			{
				cacheRecordsSameCustomerCCPID = new List<CCSynchronizeCard>();

				foreach (CCSynchronizeCard syncCard in CustomerCardPaymentData.Select())
				{
					string chkCustomerID = DeleteCustomerPrefix(syncCard.PCCustomerID);
					if (chkCustomerID == customerID)
					{
						cacheRecordsSameCustomerCCPID.Add(syncCard);
					}
				}
			}
			return cacheRecordsSameCustomerCCPID;
		}

		private int GetCardsAllProfiles()
		{
			CreditCardsFilter filter = Filter.Current;
			string processingCenter = filter.ProcessingCenterId;
			CreditCardReceiverFactory factory = new CreditCardReceiverFactory(filter);
			CCSynchronizeCardManager syncronizeCardManager = new CCSynchronizeCardManager(this, processingCenter, factory);
			Dictionary<string, CustomerData> customerDatas = syncronizeCardManager.GetCustomerProfilesFromService();
			syncronizeCardManager.SetCustomerProfileIds(customerDatas.Select(i => i.Key));
			Dictionary<string, CustomerCreditCard> unsyncCustomerCreditCards = syncronizeCardManager.GetUnsynchronizedPaymentProfiles();
			int unsyncCardCnt = 0;

			foreach (var item in unsyncCustomerCreditCards)
			{
				List<CCSynchronizeCard> alreadyAdded = GetExistedSyncCardEntriesByCustomerCCPID(item.Key, processingCenter);
				CustomerCreditCard cards = item.Value;

				foreach (CreditCardData card in cards.CreditCards)
				{
					if (CheckNotImportedRecordExists(cards.CustomerProfileId, card.PaymentProfileID, alreadyAdded))
						continue;

					CCSynchronizeCard syncCard = new CCSynchronizeCard();
					CustomerData customerData = customerDatas[cards.CustomerProfileId];
					string cardNumber = card.CardNumber.Trim('X');
					FormatMaskedCardNum(syncCard, cardNumber);
					syncCard.CCProcessingCenterID = processingCenter;
					syncCard.CustomerCCPID = cards.CustomerProfileId;
					syncCard.CustomerCCPIDHash = CCSynchronizeCard.GetSha1HashString(syncCard.CustomerCCPID);
					syncCard.PaymentCCPID = card.PaymentProfileID;
					syncCard.PCCustomerID = customerData.CustomerCD;
					syncCard.PCCustomerDescription = customerData.CustomerName;
					syncCard.PCCustomerEmail = customerData.Email;

					if (card.CardExpirationDate != null)
					{
						syncCard.ExpirationDate = card.CardExpirationDate.Value;
					}

					if (card.AddressData != null)
					{
						AddressData addrData = card.AddressData;
						syncCard.FirstName = addrData.FirstName;
						syncCard.LastName = addrData.LastName;
					}

					CustomerCardPaymentData.Insert(syncCard);
					unsyncCardCnt++;
				}
			}

			return unsyncCardCnt;
		}

		private bool ValidateLoadCardsAction()
		{
			bool errorOccured = false;
			CreditCardsFilter filter = Filter.Current;
			string processingCenter = filter.ProcessingCenterId;

			if (processingCenter == null && filter.ScheduledServiceSync.GetValueOrDefault() == true)
			{
				throw new PXException(Messages.ProcessingCenterNotSelected);
			}

			if (processingCenter == null)
			{
				Filter.Cache.RaiseExceptionHandling<CreditCardsFilter.processingCenterId>(filter, filter.ProcessingCenterId,
					new PXSetPropertyException(Messages.ProcessingCenterNotSelected));
				errorOccured = true;
			}
			return errorOccured;
		}

		public bool ValidateImportCard(CCSynchronizeCard card, int cardIndex)
		{
			bool ret = true;
			if (card.BAccountID == null)
			{
				PXProcessing<CCSynchronizeCard>.SetError(cardIndex, CA.Messages.CustomerNotDefined);
				ret = false;
			}

			if (card.PaymentMethodID == null)
			{
				PXProcessing<CCSynchronizeCard>.SetError(cardIndex, Messages.PaymentMethodNotDefined);
				ret = false;
			}

			if (card.CashAccountID != null)
			{
				IEnumerable<CashAccount> availableCA = PXSelectorAttribute.SelectAll<CCSynchronizeCard.cashAccountID>(this.CustomerCardPaymentData.Cache,card)
					.RowCast<CashAccount>();
				bool exists = availableCA.Any(i => i.CashAccountID == card.CashAccountID);

				if (!exists)
				{
					PXProcessing<CCSynchronizeCard>.SetError(cardIndex, 
						PXMessages.LocalizeFormatNoPrefixNLA(AR.Messages.CashAccountIsNotConfiguredForPaymentMethodInAR, card.PaymentMethodID));
					ret = false;
				}
			}
			return ret;
		}

		public void CreateCustomerPaymentMethodRecord(CCSynchronizeCard item)
		{
			PXCache customerPaymentMethodCache = Caches[typeof(CustomerPaymentMethod)];
			CustomerPaymentMethod customerPM = customerPaymentMethodCache.CreateInstance() as CustomerPaymentMethod;
			customerPM.BAccountID = item.BAccountID;
			customerPM.CustomerCCPID = item.CustomerCCPID;
			customerPM.Descr = item.CardNumber;
			customerPM.PaymentMethodID = item.PaymentMethodID;
			customerPM.CashAccountID = item.CashAccountID;
			customerPM.CCProcessingCenterID = item.CCProcessingCenterID;

			if (item.ExpirationDate != null)
			{
				customerPaymentMethodCache.SetValueExt<CustomerPaymentMethod.expirationDate>(customerPM, item.ExpirationDate);
			}

			customerPaymentMethodCache.Insert(customerPM);
			customerPaymentMethodCache.Persist(PXDBOperation.Insert);
			customerPM = customerPaymentMethodCache.Current as CustomerPaymentMethod;
			CreateCustomerPaymentMethodDetailRecord(customerPM, item);
			CreateCustomerProcessingCenterRecord(customerPM, item);
		}

		public void UpdateCCProcessingSyncronizeCardRecord(CCSynchronizeCard item)
		{
			PXCache syncCardCache = Caches[typeof(CCSynchronizeCard)];
			item.Imported = true;
			syncCardCache.Update(item);
			syncCardCache.Persist(PXDBOperation.Update);
		}

		private void CreateCustomerProcessingCenterRecord(CustomerPaymentMethod customerPM, CCSynchronizeCard syncCard)
		{
			PXCache customerProcessingCenterCache = Caches[typeof(CustomerProcessingCenterID)];
			customerProcessingCenterCache.ClearQueryCacheObsolete();
			PXSelectBase<CustomerProcessingCenterID> checkRecordExist = new PXSelectReadonly<CustomerProcessingCenterID,
				Where<CustomerProcessingCenterID.cCProcessingCenterID, Equal<Required<CreditCardsFilter.processingCenterId>>,
					And<CustomerProcessingCenterID.bAccountID, Equal<Required<CustomerProcessingCenterID.bAccountID>>,
					And<CustomerProcessingCenterID.customerCCPID, Equal<Required<CustomerProcessingCenterID.customerCCPID>>>>>>(this);
			CustomerProcessingCenterID cProcessingCenter = checkRecordExist.SelectSingle(syncCard.CCProcessingCenterID, syncCard.BAccountID, syncCard.CustomerCCPID);

			if (cProcessingCenter == null)
			{
				cProcessingCenter = customerProcessingCenterCache.CreateInstance() as CustomerProcessingCenterID;
				cProcessingCenter.BAccountID = syncCard.BAccountID;
				cProcessingCenter.CCProcessingCenterID = syncCard.CCProcessingCenterID;
				cProcessingCenter.CustomerCCPID = syncCard.CustomerCCPID;
				customerProcessingCenterCache.Insert(cProcessingCenter);
				customerProcessingCenterCache.Persist(PXDBOperation.Insert);
			}
		}

		private void CreateCustomerPaymentMethodDetailRecord(CustomerPaymentMethod customerPM, CCSynchronizeCard syncCard)
		{
			PXResultset<PaymentMethodDetail> details = GetPaymentMethodDetailParams(customerPM.PaymentMethodID);
			PXCache customerPaymentMethodDetailCache = Caches[typeof(CustomerPaymentMethodDetail)];
			CustomerPaymentMethodDetail customerPaymentDetails;

			foreach (PaymentMethodDetail detail in details)
			{
				customerPaymentDetails = customerPaymentMethodDetailCache.CreateInstance() as CustomerPaymentMethodDetail;
				customerPaymentDetails.DetailID = detail.DetailID;
				customerPaymentDetails.PMInstanceID = customerPM.PMInstanceID;
				customerPaymentDetails.PaymentMethodID = customerPM.PaymentMethodID;

				if (customerPaymentDetails.DetailID == CreditCardAttributes.CardNumber)
				{
					Match match = new Regex("[\\d]+").Match(syncCard.CardNumber);
					if (match.Success)
					{
						string cardNum = match.Value.PadLeft(8, 'X');
						customerPaymentDetails.Value = cardNum;
					}
				}

				if (customerPaymentDetails.DetailID == CreditCardAttributes.CCPID)
				{
					customerPaymentDetails.Value = syncCard.PaymentCCPID;
				}

				customerPaymentMethodDetailCache.Insert(customerPaymentDetails);
				customerPaymentMethodDetailCache.Persist(PXDBOperation.Insert);
			}
		}

		private PXResultset<PaymentMethodDetail> GetPaymentMethodDetailParams(string paymentMethodId)
		{
			PXSelectBase<PaymentMethodDetail> query = new PXSelectReadonly<PaymentMethodDetail,
				Where<PaymentMethodDetail.paymentMethodID, Equal<Required<PaymentMethodDetail.paymentMethodID>>>>(this);
			PXResultset<PaymentMethodDetail> result = query.Select(paymentMethodId);
			return result;
		}

		private void FormatMaskedCardNum(CCSynchronizeCard syncCard, string cardNumber)
		{
			if (syncCard.PaymentMethodID == null)
			{
				syncCard.CardNumber = maskedCardTmpl + cardNumber;
			}
			else
			{
				syncCard.CardNumber = syncCard.PaymentMethodID + ":" + maskedCardTmpl + cardNumber;
			}
		}

		private PXResultset<CCSynchronizeCard> GetSyncCardsByProcCenter(string procCenterId)
		{
			CCProcessingHelper.CheckHttpsConnection();
			PXSelectBase<CCSynchronizeCard> select = new PXSelect<CCSynchronizeCard,
				Where<CCSynchronizeCard.cCProcessingCenterID, Equal<Required<CCSynchronizeCard.cCProcessingCenterID>>,
					And<CCSynchronizeCard.imported, Equal<False>>>>(this);
			PXResultset<CCSynchronizeCard> result = select.Select(procCenterId);
			return result;
		}

		private IEnumerable<PXResult<CustomerPaymentMethod, CustomerPaymentMethodDetail>> GetPaymentsProfilesByCustomer(string processingCenterID, string customerCCPID)
		{
			PXSelectBase<CustomerPaymentMethod> query = new PXSelectReadonly2<CustomerPaymentMethod, 
				InnerJoin<CustomerPaymentMethodDetail, On<CustomerPaymentMethod.pMInstanceID, Equal<CustomerPaymentMethodDetail.pMInstanceID>>,
				InnerJoin<PaymentMethodDetail, On<CustomerPaymentMethodDetail.detailID, Equal<PaymentMethodDetail.detailID>,
					And<CustomerPaymentMethodDetail.paymentMethodID, Equal<PaymentMethodDetail.paymentMethodID>>>>>,
				Where<CustomerPaymentMethod.cCProcessingCenterID, Equal<Required<CustomerPaymentMethod.cCProcessingCenterID>>,
					And<CustomerPaymentMethod.customerCCPID, Equal<Required<CustomerPaymentMethod.customerCCPID>>,
					And<PaymentMethodDetail.isCCProcessingID, Equal<True>,
					And<PaymentMethodDetail.useFor, Equal<PaymentMethodDetailUsage.useForARCards>>>>>>(this);
			var result = query.Select(processingCenterID, customerCCPID).Select(i => (PXResult<CustomerPaymentMethod, CustomerPaymentMethodDetail>)i);
			return result;
		}

		private List<CCSynchronizeCard> GetExistedSyncCardEntriesByCustomerCCPID(string customerCCPID, string processingCenterId)
		{
			string customerIdHash = CCSynchronizeCard.GetSha1HashString(customerCCPID);
			PXSelectBase<CCSynchronizeCard> query = new PXSelect<CCSynchronizeCard,
				Where<CCSynchronizeCard.customerCCPIDHash, Equal<Required<CCSynchronizeCard.customerCCPIDHash>>,
					And<CCSynchronizeCard.cCProcessingCenterID, Equal<Required<CCSynchronizeCard.cCProcessingCenterID>>>>>(this);
			var ret = query.Select(customerIdHash, processingCenterId).RowCast<CCSynchronizeCard>().ToList();
			return ret;
		}

		private bool CheckNotImportedRecordExists(string custCCPID, string paymentCCPID, List<CCSynchronizeCard> checkList)
		{
			bool ret = false;
			CCSynchronizeCard item = checkList.Where(i => i.CustomerCCPID == custCCPID && i.PaymentCCPID == paymentCCPID
				&& i.Imported.GetValueOrDefault() == false).FirstOrDefault();

			if (item != null)
			{
				ret = true;
			}
			return ret;
		}

		private bool CheckCustomerPaymentProfileExists(CCSynchronizeCard syncCard, int cardIndex)
		{
			var result = GetPaymentsProfilesByCustomer(syncCard.CCProcessingCenterID, syncCard.CustomerCCPID);
			string checkPaymentCCPID = syncCard.PaymentCCPID;

			foreach (CustomerPaymentMethodDetail cpmDetail in result.RowCast<CustomerPaymentMethodDetail>())
			{
				if (cpmDetail.Value == checkPaymentCCPID)
				{
					PXProcessing<CCSynchronizeCard>.SetError(cardIndex, Messages.RecordWithPaymentCCPIDExists);
					return true;
				}
			}
			return false;
		}

		private string DeleteCustomerPrefix(string customerID)
		{
			const string prefDelimeter = CCProcessingHelper.CustomerPrefix;
			int index = customerID.IndexOf(prefDelimeter);

			if (index >= 0)
			{
				customerID = customerID.Substring(index + prefDelimeter.Length);
			}
			return customerID;
		}

		public override void Persist()
		{
			CustPaymentProfileForDialog.Cache.Clear();
			base.Persist();
		}
	}
}


