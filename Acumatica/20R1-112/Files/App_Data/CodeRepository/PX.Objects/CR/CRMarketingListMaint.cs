using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Web.Compilation;
using PX.Common;
using PX.Data;
using PX.Objects.Common.Extensions;
using PX.Objects.CS;
using PX.Objects.EP;
using PX.SM;

namespace PX.Objects.CR
{
	public class CRMarketingListMaint : PXGraph<CRMarketingListMaint, CRMarketingList>, PXImportAttribute.IPXPrepareItems
	{
		#region Old Implementation

		//#region MailSubscriptionInfo

		//[Serializable]
		//[PXHidden]
		//public partial class MailSubscriptionInfo : IBqlTable
		//{
		//	#region Email

		//	public abstract class email : PX.Data.BQL.BqlString.Field<email> { }

		//	[PXString]
		//	[PXDefault]
		//	[PXUIField(DisplayName = "Email")]
		//	public virtual String Email { get; set; }

		//	#endregion

		//	#region FirstName

		//	public abstract class firstName : PX.Data.BQL.BqlString.Field<firstName> { }

		//	[PXString]
		//	[PXDefault]
		//	[PXUIField(DisplayName = "First Name")]
		//	public virtual String FirstName { get; set; }

		//	#endregion

		//	#region LastName

		//	public abstract class lastName : PX.Data.BQL.BqlString.Field<lastName> { }

		//	[PXString]
		//	[PXDefault]
		//	[PXUIField(DisplayName = "Last Name")]
		//	public virtual String LastName { get; set; }

		//	#endregion

		//	#region CompanyName

		//	public abstract class companyName : PX.Data.BQL.BqlString.Field<companyName> { }

		//	[PXString]
		//	[PXUIField(DisplayName = "Company Name")]
		//	public virtual String CompanyName { get; set; }

		//	#endregion

		//	#region ActivationID

		//	public abstract class activationID : PX.Data.BQL.BqlString.Field<activationID> { }

		//	[PXString]
		//	[PXUIField(DisplayName = "Activation ID")]
		//	public virtual String ActivationID { get; set; }

		//	#endregion

		//	#region Salutation

		//	public abstract class salutation : PX.Data.BQL.BqlString.Field<salutation> { }

		//	[PXDBString(255, IsUnicode = true)]
		//	[PXUIField(DisplayName = Messages.Position, Visibility = PXUIVisibility.SelectorVisible)]
		//	public virtual String Salutation { get; set; }
		//	#endregion

		//	#region Source
		//	public abstract class source : PX.Data.BQL.BqlString.Field<source> { }

		//	[PXDBString(1, IsFixed = true)]
		//	[PXUIField(DisplayName = "Lead Source", Visibility = PXUIVisibility.SelectorVisible)]
		//	[CRMSources]
		//	[PXDefault(CRMSourcesAttribute._WEB)]
		//	public virtual String Source { get; set; }
		//	#endregion

		//	#region Status
		//	public abstract class status : PX.Data.BQL.BqlString.Field<status> { }

		//	[PXDBString(1)]
		//	[PXUIField(DisplayName = "Status", Visibility = PXUIVisibility.SelectorVisible)]
		//	[LeadStatuses]
		//	[PXDefault(LeadStatusesAttribute.New)]
		//	public virtual String Status { get; set; }
		//	#endregion

		//	#region Phone
		//	public abstract class phone : PX.Data.BQL.BqlString.Field<phone> { }

		//	[PXDBString(50)]
		//	[PXUIField(DisplayName = "Phone", Visibility = PXUIVisibility.SelectorVisible)]
		//	[PhoneValidation]
		//	public virtual String Phone { get; set; }
		//	#endregion

		//	#region ClassID
		//	public abstract class classID : PX.Data.BQL.BqlString.Field<classID> { }

		//	[PXDBString(10, IsUnicode = true, InputMask = ">aaaaaaaaaa")]
		//	[PXUIField(DisplayName = "Class ID")]
		//	[PXSelector(typeof(CRContactClass.classID), DescriptionField = typeof(CRContactClass.description))]
		//	public virtual String ClassID { get; set; }
		//	#endregion

		//	#region NoCall
		//	public abstract class noCall : PX.Data.BQL.BqlBool.Field<noCall> { }

		//	[PXDBBool]
		//	[PXDefault(false)]
		//	[PXUIField(DisplayName = "Do Not Call")]
		//	public virtual bool? NoCall { get; set; }
		//	#endregion
		//}

		//#endregion

		//#region MailUnsubscriptionInfo

		//[Serializable]
		//[PXHidden]
		//public partial class MailUnsubscriptionInfo : IBqlTable
		//{
		//	#region Email

		//	public abstract class email : PX.Data.BQL.BqlString.Field<email> { }

		//	[PXString]
		//	[PXDefault]
		//	[PXUIField(DisplayName = "Email")]
		//	public virtual String Email { get; set; }

		//	#endregion
		//}

		//#endregion

		//#region MailListInfo

		//[Serializable]
		//[PXHidden]
		//[PXProjection(typeof(Select<CRMarketingList, Where<CRMarketingList.isActive, Equal<True>>>), Persistent = true)]
		//public partial class MarketingListInfo : CRMarketingList
		//{
		//}

		//#endregion

		//#region Views

		//[PXHidden]
		//public PXSelect<Contact>
		//	Leads;

		//[PXHidden]
		//public PXFilter<MailSubscriptionInfo>
		//	Subscriptions;

		//[PXHidden]
		//public PXFilter<MailUnsubscriptionInfo>
		//	Unsubscriptions;

		//[PXHidden]
		//public PXSelect<MarketingListInfo>
		//	MailListsSubscription;

		//[PXHidden]
		//public PXSelect<CRMarketingListMember>
		//	NewMailSubscriptions;

		//[PXHidden]
		//public PXSelect<Contact, Where<Contact.contactID, Equal<Required<Contact.contactID>>>>
		//	ContactForListMember;

		//#endregion

		//#region Actions

		//public PXAction<Contact> details;
		//[PXButton(Tooltip = Messages.ViewDetailsTooltip)]
		//[PXUIField(DisplayName = Messages.ViewDetails, Visible = false)]
		//public virtual IEnumerable Details(PXAdapter adapter)
		//{
		//	if (FilteredItems.Current != null)
		//	{
		//		var recipient = (Contact)PXSelect<Contact>.
		//			Search<Contact.contactID>(this, FilteredItems.Current.ContactID);
		//		if (recipient != null)
		//			PXRedirectHelper.TryOpenPopup(Caches[typeof(Contact)], recipient, string.Empty);
		//	}
		//	return adapter.Get();
		//}

		//public PXAction<Contact> generateActivation;
		//[PXUIField(Visible = false)]
		//[PXButton]
		//public virtual IEnumerable GenerateActivation(PXAdapter adapter)
		//{
		//	var recipient = Leads.Current;
		//	var minutes = 10;
		//	if (adapter.Parameters != null && adapter.Parameters.Length != 0)
		//		int.TryParse(adapter.Parameters[0].ToString(), out minutes);

		//	if (recipient != null)
		//	{
		//		var wasUpdated = false;
		//		var cache = Caches[typeof(CRMarketingListMember)];
		//		foreach (MarketingListInfo listInfo in MailListsSubscription.Select())
		//			if (listInfo.MarketingListID != null && listInfo.Selected == true)
		//				foreach (CRMarketingListMember item in PXSelect<CRMarketingListMember,
		//						Where<CRMarketingListMember.contactID, Equal<Required<CRMarketingListMember.contactID>>,
		//							And<CRMarketingListMember.marketingListID, Equal<Required<CRMarketingListMember.marketingListID>>>>>.
		//					Select(this, recipient.ContactID, listInfo.MarketingListID))
		//				{
		//					item.Subscribed = false;
		//					cache.Update(item);
		//					wasUpdated = true;
		//				}
		//		if (wasUpdated) Save.Press();

		//		//recipient.ActivationID = System.Web.HttpUtility.UrlEncode(recipient.ContactID.ToString());//TODO: need remove
		//	}
		//	return adapter.Get();
		//}

		//public PXAction<CRMarketingList> removeRecipient;
		//[PXUIField(Visible = false)]
		//[PXButton]
		//public virtual IEnumerable RemoveRecipient(PXAdapter adapter)
		//{
		//	foreach (MailUnsubscriptionInfo item in Unsubscriptions.Select())
		//	{
		//		foreach (MarketingListInfo list in MailListsSubscription.Select())
		//			foreach (PXResult<CRMarketingListMember> row in
		//				PXSelectJoin<CRMarketingListMember,
		//						InnerJoin<Contact, On<Contact.contactID, Equal<CRMarketingListMember.contactID>>>,
		//						Where<CRMarketingListMember.marketingListID, Equal<Required<CRMarketingList.marketingListID>>,
		//							And<Contact.eMail, Equal<Required<Contact.eMail>>>>>.
		//					Select(this, list.MarketingListID, item.Email))
		//			{
		//				var sub = (CRMarketingListMember)row;
		//				sub.Subscribed = false;
		//				MailRecipients.Update(sub);
		//			}
		//	}
		//	Save.Press();
		//	SelectTimeStamp();
		//	return adapter.Get();
		//}

		//#endregion

		//public override void Persist()
		//{
		//	PersistSubscription();
		//	PersistUnsubscription();

		//	base.Persist();
		//}

		//private void PersistSubscription()
		//{
		//	var current = Subscriptions.Current;
		//	if (string.IsNullOrEmpty(current.Email)) return;

		//	var lead = (Contact)Leads.Search<Contact.eMail>(current.Email);
		//	if (lead == null)
		//	{
		//		lead = (Contact)Leads.Cache.Insert();
		//		lead.ContactType = ContactTypesAttribute.Lead;
		//		lead.FirstName = current.FirstName;
		//		lead.LastName = current.LastName;
		//		lead.EMail = current.Email;
		//		lead.FullName = current.CompanyName;
		//		lead.Salutation = current.Salutation;
		//		lead.Source = current.Source;
		//		lead.Status = current.Status;
		//		lead.Phone1 = current.Phone;
		//		lead.ClassID = current.ClassID;
		//		lead.NoCall = current.NoCall;
		//		Leads.Cache.Update(lead);
		//	}

		//	foreach (MarketingListInfo list in MailListsSubscription.Select())
		//	{
		//		if (list.Selected != true) continue;

		//		if (Leads.Cache.GetStatus(lead) != PXEntryStatus.Inserted)
		//		{
		//			var existSubscriptions = NewMailSubscriptions.
		//				Search<CRMarketingListMember.contactID, CRMarketingListMember.marketingListID>(lead.ContactID, list.MarketingListID);
		//			if (existSubscriptions != null && existSubscriptions.Count > 0) continue;
		//		}

		//		var subscr = (CRMarketingListMember)NewMailSubscriptions.Cache.CreateInstance();
		//		subscr.ContactID = lead.ContactID;
		//		subscr.MarketingListID = list.MarketingListID;
		//		subscr.Subscribed = false;
		//		NewMailSubscriptions.Cache.Update(subscr);
		//	}
		//}

		//private void PersistUnsubscription()
		//{
		//	Contact lead;
		//	if (string.IsNullOrEmpty(Unsubscriptions.Current.Email) ||
		//		(lead = (Contact)Leads.Search<Contact.eMail>(Unsubscriptions.Current.Email)) == null ||
		//		Leads.Cache.GetStatus(lead) == PXEntryStatus.Inserted)
		//	{
		//		return;
		//	}

		//	foreach (MarketingListInfo list in MailListsSubscription.Select())
		//	{
		//		if (list.Selected == true) continue;

		//		var existSubscriptions = NewMailSubscriptions.
		//			Search<CRMarketingListMember.contactID, CRMarketingListMember.marketingListID>(lead.ContactID, list.MarketingListID);
		//		if (existSubscriptions == null || existSubscriptions.Count == 0) continue;

		//		var subscr = (CRMarketingListMember)existSubscriptions;
		//		subscr.Subscribed = false;
		//		NewMailSubscriptions.Cache.Update(subscr);
		//	}
		//}

		//public static void DoActivate(string activationID)
		//{
		//	int contactId;
		//	if (!int.TryParse(System.Web.HttpUtility.UrlDecode(activationID), out contactId))
		//	{
		//		return;
		//	}

		//	var graph = new PXGraph();
		//	var cache = graph.Caches[typeof(CRMarketingListMember)];
		//	graph.Views.Caches.Add(typeof(CRMarketingListMember));
		//	var wasUpdated = false;
		//	foreach (CRMarketingListMember item in PXSelect<CRMarketingListMember,
		//			Where<CRMarketingListMember.contactID, Equal<Required<CRMarketingListMember.contactID>>>>.
		//		Select(graph, contactId))
		//	{
		//		item.Subscribed = true;
		//		cache.Update(item);
		//		wasUpdated = true;
		//	}
		//	if (wasUpdated) graph.Actions.PressSave();
		//}

		//protected virtual void Contact_RowPersisted(PXCache cache, PXRowPersistedEventArgs e)
		//{
		//	var row = e.Row as Contact;
		//	if (row == null || e.TranStatus != PXTranStatus.Open) return;

		//	foreach (CRMarketingListMember item in NewMailSubscriptions.Cache.Inserted)
		//		if (item.ContactID < 0) item.ContactID = row.ContactID;
		//}




		#endregion

		#region Strange Code

		//protected virtual void PerformAction(List<Contact> list)
		//{
		//	if (Operations.Current.Action == REMOVE_ACTION)
		//	{
		//		if (MailLists.Current != null
		//		    && Operations.Ask(
		//			    Messages.AskConfirmation,
		//			    string.Format(PXLocalizer.Localize(Messages.ConfirmRemoving, typeof(Messages).ToString()), list.Count),
		//			    MessageButtons.YesNoCancel) == WebDialogResult.Yes)
		//		{
		//			try
		//			{
		//				foreach (Contact item in list)
		//				{
		//					if (item.Selected == true)
		//					{
		//						var newItem = (CRMarketingListMember)MailRecipients.Cache.CreateInstance();
		//						newItem.MarketingListID = MailLists.Current.MarketingListID;
		//						newItem.ContactID = item.ContactID;
		//						MailRecipients.Cache.Delete(newItem);
		//					}
		//				}
		//				Save.Press();
		//				MailRecipients.Cache.Clear();
		//			}
		//			finally
		//			{
		//				MailRecipients.Cache.Clear();
		//			}
		//		}
		//	}
		//	else
		//	{
		//		if (MailLists.Current != null)
		//		{
		//			try
		//			{
		//				foreach (Contact item in list)
		//				{
		//					if (item.Selected == true)
		//					{
		//						var newItem = (CRMarketingListMember)MailRecipients.Cache.CreateInstance();
		//						newItem.MarketingListID = MailLists.Current.MarketingListID;
		//						newItem.ContactID = item.ContactID;
		//						MailRecipients.Cache.Insert(newItem);
		//					}
		//				}
		//				Save.Press();
		//				FilteredItems.Cache.Clear();
		//			}
		//			finally
		//			{
		//				MailRecipients.Cache.Clear();
		//			}
		//		}
		//	}
		//	this.MailRecipients.View.RequestRefresh();
		//}

		#endregion

		

		#region Constants

		public const string REMOVE_ACTION = "Remove Members";
		public const string ADD_ACTION = "Add Members";

		#endregion

		#region Views

		[PXHidden]
		public PXSelect<BAccount> BAccount;

		[PXHidden]
		public PXSelect<Address>
			Addresses;

		[PXViewName(Messages.MarketingList)]
		public PXSelect<CRMarketingList>
			MailLists;

        [PXHidden]
		public PXSelect<CRMarketingList, Where<CRMarketingList.marketingListID, Equal<Current<CRMarketingList.marketingListID>>>>
			MailListsCurrent;

		[PXViewName(Messages.Filter)]
		public PXFilter<OperationParam> Operations;

		[PXViewName(Messages.MailRecipients)]
		[PXImport(typeof(CRMarketingList))]
		[PXCopyPasteHiddenView]
		public CRMarketingListMembersList MailRecipients;

		public PXSelect<CRFixedFilterRow,
					Where<CRFixedFilterRow.refNoteID, Equal<Current<CRMarketingList.noteID>>>> SelectionCriteria;

		[PXViewName(Messages.Activities)]
        [PXFilterable]
        public CRActivityList<CRMarketingList> Activities;

		[PXHidden]
		public PXSelectJoin<Contact,
				InnerJoin<CRMarketingListMember,
					On<CRMarketingListMember.contactID, Equal<Contact.contactID>>,
				LeftJoin<BAccount,
					On<BAccount.bAccountID, Equal<Contact.bAccountID>>,
				LeftJoin<BAccountParent,
					On<BAccountParent.bAccountID, Equal<Contact.parentBAccountID>>,
				LeftJoin<Address,
					On<Address.addressID, Equal<Contact.defAddressID>>,
				LeftJoin<State,
					On<State.countryID, Equal<Address.countryID>,
					And<State.stateID, Equal<Address.state>>>,
				LeftJoin<CRLead,
					On<CRLead.contactID.IsEqual<Contact.contactID>>>>>>>>,
				Where<CRMarketingListMember.marketingListID, Equal<Current<CRMarketingList.marketingListID>>>>
			RemoveItems;

		[PXViewName(Messages.SelectionPreview)]
		[CRFixedFilterable(typeof(CRMarketingList.noteID))]
		public PXSelectReadonly2<Contact,
				LeftJoin<CRMarketingListMember,
					On<CRMarketingListMember.contactID, Equal<Contact.contactID>,
					And<CRMarketingListMember.marketingListID, Equal<Current<CRMarketingList.marketingListID>>>>,
				LeftJoin<BAccount,
					On<BAccount.bAccountID, Equal<Contact.bAccountID>>,
				LeftJoin<BAccountParent,
					On<BAccountParent.bAccountID, Equal<Contact.parentBAccountID>>,
				LeftJoin<Address,
					On<Address.addressID, Equal<Contact.defAddressID>>,
				LeftJoin<State,
					On<State.countryID, Equal<Address.countryID>,
					And<State.stateID, Equal<Address.state>>>,
				LeftJoin<CRLead,
					On<CRLead.contactID.IsEqual<Contact.contactID>>>>>>>>,
				Where<CRMarketingListMember.contactID, IsNull>>
			FilteredItems;

		protected virtual IEnumerable filteredItems([PXString] string action)
		{
			// MailRecipients.View.RequestRefresh();

			if (!String.Equals(Operations.Current.Action, action, StringComparison.OrdinalIgnoreCase))
				FilteredItems.Cache.Clear();
			Operations.Current.Action = action;

			var mailListID = MailLists.Current.With(ml => ml.MarketingListID);

			//Dinamic)
			if (MailLists.Current.With(ml => ml.IsDynamic == true))
				return CRSubscriptionsSelect.Select(this, mailListID);

			using (ReadOnlyScope s = new ReadOnlyScope(this.Caches<CRMarketingList>()))
			{
				CRSubscriptionsSelect.MergeFilters(this, mailListID);
			}

			//Remove
			if (string.Equals(Operations.Current.Action, REMOVE_ACTION, StringComparison.OrdinalIgnoreCase))
				return this.QuickSelect(RemoveItems.View.BqlSelect, PXView.Filters);

			return this.QuickSelect(FilteredItems.View.BqlSelect, PXView.Filters);
		}

		#endregion

		#region Ctors

		public CRMarketingListMaint()
		{            
			var contactCache = Caches[typeof(Contact)];
			PXUIFieldAttribute.SetDisplayName<Contact.fullName>(contactCache, Messages.ContactFullName);
			PXDBAttributeAttribute.Activate(FilteredItems.Cache);

			var parentBAccountCache = Caches[typeof(BAccount)];
			parentBAccountCache.DisplayName = Messages.ParentAccount;
			PXUIFieldAttribute.SetDisplayName<BAccount.acctName>(parentBAccountCache, Messages.ParentAccountNameShort);
			PXDBAttributeAttribute.Activate(parentBAccountCache);

			PXUIFieldAttribute.SetVisible<CRMarketingListMember.marketingListID>(MailRecipients.Cache, null, false);
			PXUIFieldAttribute.SetVisibility<Contact.fullName>(this.Caches[typeof(Contact)], null, PXUIVisibility.Visible);
		    PXUIFieldAttribute.SetEnabled(this.Caches[typeof(Contact)], null, null, false);
		    PXUIFieldAttribute.SetEnabled<Contact.selected>(this.Caches[typeof(Contact)], null, true);

        }

		#endregion

		#region CacheAttached        
        
        [PXDimensionSelector("BIZACCT",
            typeof(Search2<BAccount.acctCD,
                    LeftJoin<Contact, On<Contact.bAccountID, Equal<BAccount.bAccountID>, And<Contact.contactID, Equal<BAccount.defContactID>>>,
                    LeftJoin<Address, On<Address.bAccountID, Equal<BAccount.bAccountID>, And<Address.addressID, Equal<BAccount.defAddressID>>>>>,
                Where2<Where<BAccount.type, Equal<BAccountType.customerType>,
                    Or<BAccount.type, Equal<BAccountType.prospectType>,
                    Or<BAccount.type, Equal<BAccountType.combinedType>,
                    Or<BAccount.type, Equal<BAccountType.vendorType>>>>>,
                    And<Match<Current<AccessInfo.userName>>>>>),
            typeof(BAccount.acctCD),
            typeof(BAccount.acctCD), typeof(BAccount.acctName), typeof(BAccount.type), typeof(BAccount.classID), typeof(BAccount.status), typeof(Contact.phone1), typeof(Address.city), typeof(Address.countryID), typeof(Contact.eMail))]
        [PXDBString(30, IsUnicode = true, IsKey = true, InputMask = "")]
        [PXUIField(DisplayName = "Account ID", Enabled = false)]
        public virtual void BAccount_AcctCD_CacheAttached(PXCache sender)
	    {
	    }
		
		[PXDBString(255, IsUnicode = true)]
		[PXUIField(DisplayName = "Company Name", Visibility = PXUIVisibility.SelectorVisible, Enabled = false, Visible = true)]
		protected void Contact_FullName_CacheAttached(PXCache sender)
		{
		}

		[PXDBString(1, IsFixed = true)]
		[CRContactMethods]
		[PXUIField(DisplayName = "Contact Method")]
		protected virtual void Contact_Method_CacheAttached(PXCache sender)
		{
		}

		[PXDBBool]
		[PXDefault(true, PersistingCheck = PXPersistingCheck.Nothing)]
		[PXUIField(DisplayName = "Active")]
		protected virtual void Contact_IsActive_CacheAttached(PXCache sender)
		{
		}

		[PXDBInt]
		[AddressRevisionID]
		protected virtual void Contact_RevisionID_CacheAttached(PXCache sender)
		{
		}

		[PXDBString(2, IsFixed = true)]
		[PXUIField(DisplayName = "Type")]
		[ContactTypes]
		[PXDefault(ContactTypesAttribute.Lead, PersistingCheck = PXPersistingCheck.Nothing)]
		protected virtual void Contact_ContactType_CacheAttached(PXCache sender)
		{
		}

		[PXMergeAttributes(Method = MergeMethod.Merge)]
		[PXUIField(DisplayName = "Business Account", Visibility = PXUIVisibility.SelectorVisible)]
        [PXSelector(typeof(BAccount.bAccountID), SubstituteKey = typeof(BAccount.acctCD), DescriptionField = typeof(BAccount.acctCD), DirtyRead = true)]
        protected virtual void Contact_BAccountID_CacheAttached(PXCache sender)
		{
		}

        [PXDBString(2, IsFixed = true)]
		[PXUIField(DisplayName = "Business Account Type", Visibility = PXUIVisibility.SelectorVisible, Enabled = false)]
		[BAccountType.List()]
	    protected virtual void BAccount_Type_CacheAttached(PXCache sender)
	    {
	    }

		[PXMergeAttributes(Method = MergeMethod.Merge)]
		[PXUIField(DisplayName = "List Name", Visibility = PXUIVisibility.SelectorVisible)]
		protected virtual void CRMarketingList_Name_CacheAttached(PXCache sender)
		{
		}

		[PXSelector(typeof(State.stateID), DescriptionField = typeof(State.name))]
		[PXMergeAttributes(Method = MergeMethod.Merge)]
		protected virtual void Address_State_CacheAttached(PXCache sender)
		{
		}

		[PXMergeAttributes(Method = MergeMethod.Merge)]
		[PXUIField(DisplayName = "Member Since", Enabled = false)]
		protected virtual void CRMarketingListMember_CreatedDateTime_CacheAttached(PXCache cache)
		{
		}

		#endregion

		#region Event Handlers

		protected virtual void CRMarketingList_RowSelected(PXCache sender, PXRowSelectedEventArgs e)
		{
			var row = e.Row as CRMarketingList;
			if (row == null) return;

			var isStatic = row.IsDynamic != true;
			if (row.IsDynamic != true)
				row.GIDesignID = null;

			PXUIFieldAttribute.SetEnabled(MailRecipients.Cache, null, null, isStatic);
			MailRecipients.View.AllowInsert = isStatic;
			MailRecipients.View.AllowDelete = isStatic;
			MailRecipients.View.AllowUpdate = true;

			PXUIFieldAttribute.SetEnabled<CRMarketingListMember.isSubscribed>(Caches[typeof(CRMarketingListMember)], null, true);

			PXUIFieldAttribute.SetEnabled<CRMarketingList.gIDesignID>(sender, row, row.IsDynamic == true);
			PXUIFieldAttribute.SetEnabled<CRMarketingList.sharedGIFilter>(sender, row, row.IsDynamic == true && row.GIDesignID != null);
			PXUIFieldAttribute.SetVisible<CRMarketingListMember.selected>(Caches[typeof(CRMarketingListMember)], null, isStatic);
			FilteredItems.Cache.AllowInsert = isStatic;
			FilteredItems.Cache.AllowDelete = isStatic;
			FilteredItems.Cache.AllowUpdate = true;
		}

		protected virtual void CRMarketingList_GIDesignID_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
		{
			sender.SetValue<CRMarketingList.sharedGIFilter>(sender.Current, null);
		}

		protected virtual void CRMarketingList_RowPersisting(PXCache sender, PXRowPersistingEventArgs e)
		{
			CRMarketingList row = e.Row as CRMarketingList;

			if (row == null || row.IsDynamic == true)
				return;
			
			var origDynamic = (bool?)sender.GetValueOriginal<CRMarketingList.isDynamic>(row);
			if (origDynamic != true)
				return;

			MailRecipients
				.Select().AsEnumerable()
				.Where(_ => ((CRMarketingListMember)_).IsSubscribed != true)
				.ForEach(_ => this.Caches[typeof(CRMarketingListMember)].Delete(_));
		}

		protected virtual void CRMarketingListMember_IsSubscribed_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
		{
			CRMarketingListMember row = e.Row as CRMarketingListMember;
			CRMarketingList list = MailLists.Current;
			if (row == null || list == null || list.IsDynamic == false || row.IsSubscribed != true)
				return;

			sender.SetStatus(row, PXEntryStatus.Deleted);
		}

		#endregion

		#region CRFixedFilterRow Events		
		public PXSelect<CRFixedFilterRow, 
            Where<CRFixedFilterRow.refNoteID, Equal<Current<CRMarketingList.noteID>>>> SelectrionCriteria;

	    [PXDefault(typeof(CRMarketingList.noteID))]	    
	    [PXMergeAttributes(Method = MergeMethod.Append)]
	    protected virtual void CRFixedFilterRow_RefNoteID_CacheAttached(PXCache sender)
	    {

	    }
        [PXDefault(true)]
	    [PXDBBool]
	    [PXUIField(DisplayName = "Active")]
	    protected virtual void CRFixedFilterRow_IsUsed_CacheAttached(PXCache sender)
	    {

	    }
	    
	    [PXDBByte]
	    [PXUIField(DisplayName = "Condition")]
	    protected virtual void CRFixedFilterRow_Condition_CacheAttached(PXCache sender)
	    {

	    }

	    protected virtual void CRFixedFilterRow_DataField_FieldSelecting(PXCache sender, PXFieldSelectingEventArgs e)
	    {
	        e.ReturnState = PXStringState.CreateInstance(e.ReturnState, null, null, "DataField",
	            null, null, null, PropertyNames, PropertyLabels, true, null);
	    }

	    protected virtual void CRFixedFilterRow_DataField_FieldUpdating(PXCache cache, PXFieldUpdatingEventArgs e)
	    {
	        if (e.NewValue == null) return;

	        if (CheckProperty(e.NewValue)) return;

	        cache.RaiseExceptionHandling<CRFixedFilterRow.dataField>(e.Row, e.NewValue, GetPropertyException("DataField"));
	        e.NewValue = null;
	    }

	    protected virtual void CRFixedFilterRow_OpenBrackets_FieldSelecting(PXCache sender, PXFieldSelectingEventArgs e)
	    {
	        CRFixedFilterRow row = e.Row as CRFixedFilterRow;

	        if (IsImport && row != null)
	        {
	            if (row.OpenBrackets == null && FilterRow.OpenBracketsAttribute.Values != null)
	            {
	                row.OpenBrackets = FilterRow.OpenBracketsAttribute.Values.First();
	            }

	            if (row.CloseBrackets == null && FilterRow.CloseBracketsAttribute.Values != null)
	            {
	                row.CloseBrackets = FilterRow.CloseBracketsAttribute.Values.First();
	            }
	        }
	    }

	    protected virtual void CRFixedFilterRow_ValueSt_FieldSelecting(PXCache cache, PXFieldSelectingEventArgs e)
	    {
	        Values_FieldSelecting_Handler(e, "ValueSt", false);
	    }

	    protected virtual void CRFixedFilterRow_ValueSt2_FieldSelecting(PXCache cache, PXFieldSelectingEventArgs e)
	    {
	        Values_FieldSelecting_Handler(e, "ValueSt2", true);
	    }

	    protected virtual void CRFixedFilterRow_Condition_FieldSelecting(PXCache cache, PXFieldSelectingEventArgs e)
	    {
	        string[] labels = null;
	        int[] values = null;
	        var row = e.Row as CRFixedFilterRow;
	        if (row != null)
	            GetConditions(row.DataField, out values, out labels);
	        string[] localizedLabels = LocalizeFilterConditionLabels(labels);
	        e.ReturnState = PXIntState.CreateInstance(row == null ? null : row.Condition, typeof(CRFixedFilterRow.condition).Name, false, null, null, null, values, localizedLabels, null, null);
	    }

	    protected virtual void CRFixedFilterRow_Condition_FieldUpdating(PXCache cache, PXFieldUpdatingEventArgs e)
	    {
	        var row = e.Row as CRFixedFilterRow;
	        if (row != null && e.NewValue != null)
	        {
	            if (!CheckCondition(e.NewValue, row.DataField))
	                e.NewValue = null;
	        }
	    }

	    protected virtual void CRFixedFilterRow_RowUpdated(PXCache cache, PXRowUpdatedEventArgs e)
	    {
	        var row = e.Row as CRFixedFilterRow;
	        var oldRow = e.OldRow as CRFixedFilterRow;
	        if (row == null || oldRow == null) return;
	        if (row.Condition == null || row.DataField == null)
	            row.Condition = row.DataField != null? (byte?)0 : null;

            if (!Equals(row.DataField, oldRow.DataField) || !IsConditionWithValue(row.Condition))
	        {
	            row.ValueSt = null;
	            row.ValueSt2 = null;
	        }
	        else if (!IsConditionWithTwoValue(row.Condition))
	        {
	            row.ValueSt2 = null;
	        }
	    }
        #endregion

	    #region Private Methods
	    private string[] LocalizeFilterConditionLabels(string[] labels)
	    {
	        if (labels != null)
	        {
	            string[] localizedLabels = new string[labels.Length];
	            for (int i = 0; i < labels.Length; i++)
	            {
	                localizedLabels[i] = PXLocalizer.Localize(labels[i], typeof(InfoMessages).FullName);
	            }
	            return localizedLabels;
	        }
	        return null;
	    }

	    private void Values_FieldSelecting_Handler(PXFieldSelectingEventArgs e, string fieldName, bool secondValue)
	    {
	        var row = e.Row as CRFixedFilterRow;
	        if (row == null || string.IsNullOrEmpty(row.DataField)) return;

	        var enabled = true;
	        if (!IsConditionWithValue(row.Condition) || secondValue && !IsConditionWithTwoValue(row.Condition))
	        {
	            enabled = false;
	            e.ReturnValue = null;
	        }

	        var index = Array.IndexOf(PropertyNames, row.DataField);
	        if (index > -1)
	        {
	            var state = PropertyStates[index].CreateInstance(null, null, null, null, null, null, null, fieldName,
	                null, null, null, PXErrorLevel.Undefined, enabled, null,
	                false, PXUIVisibility.Undefined, null, null, null);
	            state.Value = e.ReturnValue;
	            e.ReturnState = state;
	        }
	    }

	    private void GetConditions(string dataField, out int[] values, out string[] labels)
	    {
	        values = new int[0];
	        labels = new string[0];

	        if (string.IsNullOrEmpty(dataField)) return;

	        ReadProperties();
	        if (_numFields.Contains(dataField))
	        {
	            values = _numberConditionsValues;
	            labels = _numberConditionsLabels;
	        }
	        else if (_dateFields.Contains(dataField))
	        {
	            values = _dateConditionsValues;
	            labels = _dateConditionsLabels;
	        }
	        else
	        {
	            values = _commonConditionsValues;
	            labels = _commonConditionsLabels;
	        }
	    }

	    private string[] PropertyNames
	    {
	        get
	        {
	            ReadProperties();
	            return _propertyNames;
	        }
	    }

	    private string[] PropertyLabels
	    {
	        get
	        {
	            ReadProperties();
	            return _propertyLabels;
	        }
	    }

	    private PXFieldState[] PropertyStates
	    {
	        get
	        {
                if(_propertyStates == null)
	                ReadProperties();
	            return _propertyStates;
	        }
	    }

	    private void ReadProperties()
	    {

	        _propertyNames = new string[0];
	        _propertyLabels = new string[0];
	        _propertyStates = new PXFieldState[0];
	        _numFields.Clear();
	        _dateFields.Clear();
	        _stringFields.Clear();

	        var fieldStates = 
                PXFieldState.GetFields(this, new[] {typeof(Contact)}, false)
                .Where(s => s.Name.EndsWith("_Attributes") || !s.Name.Contains('_')).ToList();
	        fieldStates.AddRange(PXFieldState.GetFields(this, new[] {typeof(Address)}, false)
                .Where(state => !state.Name.Contains('_') && !fieldStates.Exists(e => e.Name == state.Name || e.DisplayName == state.DisplayName))
                .Select(state => PXFieldState.CreateInstance(state, null, null, null, null, null, null, null, typeof(Address).Name + "__" + state.Name, null, null, null, PXErrorLevel.Undefined, null, null, null, PXUIVisibility.Undefined, null, null, null)));

	        var properties = new List<PXFieldState>();
	        foreach (var state in fieldStates)
	        {
	            string fieldName = state.Name.Replace(typeof(Address).Name + "__", string.Empty);                  

                if (fieldName.EndsWith("_Attributes") || 
                    (state.Visibility != PXUIVisibility.Invisible &&
                    state.Visible && !string.IsNullOrEmpty(state.DisplayName) &&	                
                    !fieldName.Contains('_') && 
                    !properties.Exists(item => item.Name == fieldName || item.DisplayName == state.DisplayName )))
	            {
	                properties.Add(state);

					if (!string.IsNullOrEmpty(state.ViewName) && !Views.ContainsKey(state.ViewName))
						Views.Add(state.ViewName, this.FilteredItems.View);
					switch (Type.GetTypeCode(state.DataType))
	                {
	                    case TypeCode.Byte:
	                    case TypeCode.Int16:
	                    case TypeCode.Int32:
	                    case TypeCode.Int64:
	                    case TypeCode.UInt16:
	                    case TypeCode.UInt32:
	                    case TypeCode.UInt64:
	                    case TypeCode.Single:
	                    case TypeCode.Double:
	                    case TypeCode.Decimal:
	                        _numFields.Add(state.Name);
	                        break;
	                    case TypeCode.DateTime:
	                        _dateFields.Add(state.Name);
	                        break;
	                    case TypeCode.String:
	                    case TypeCode.Char:
	                        _stringFields.Add(state.Name);
	                        break;
	                }
	            }
	        }
	        _propertyNames = new string[properties.Count];
	        _propertyLabels = new string[properties.Count];
	        _propertyStates = new PXFieldState[properties.Count];
	        //properties.Sort((fs1, fs2) => String.Compare(String.Concat(fs1.DisplayName, "$", fs1.Name), String.Concat(fs2.DisplayName, "$", fs2.Name)));
	        for (int i = 0; i < properties.Count; i++)
	        {
	            PXFieldState state = properties[i];
	            _propertyNames[i] = state.Name;
	            _propertyLabels[i] = state.DisplayName;
	            _propertyStates[i] = state;
	        }

	        MakeEqualLabelsDistinct();
	    }

	    private void MakeEqualLabelsDistinct()
	    {
	        int masLength = _propertyLabels.Length;
	        bool isEqualExist;
	        bool[] processedItems = new bool[masLength];

	        for (int i = 0; i < masLength - 1; i++)
	        {
	            //Search equal labels for not processed items
	            if (!processedItems[i])
	            {
	                string currentLabel = _propertyLabels[i];
	                isEqualExist = false;

	                for (int j = i + 1; j < masLength; j++)
	                {
	                    //Make distinct only not processed items
	                    if (!processedItems[j] && currentLabel == _propertyLabels[j])
	                    {
	                        isEqualExist = true;
	                        processedItems[j] = true;
	                        _propertyLabels[j] = string.Format("{0} ({1})", _propertyLabels[j], _propertyNames[j]);
	                    }
	                }

	                if (isEqualExist)
	                {
	                    processedItems[i] = true;
	                    _propertyLabels[i] = string.Format("{0} ({1})", _propertyLabels[i], _propertyNames[i]);
	                }
	            }
	        }
	    }

	    private static bool IsConditionWithTwoValue(int? condition)
	    {
	        if (condition == null) return false;

	        var typedCondition = (PXCondition)condition.Value;
	        return typedCondition == PXCondition.BETWEEN;
	    }

	    private static bool IsConditionWithValue(int? condition)
	    {
	        if (condition == null) return true;

	        var typedCondition = (PXCondition)condition.Value;
	        return typedCondition != PXCondition.TOMMOROW &&
	               typedCondition != PXCondition.TODAY_OVERDUE &&
	               typedCondition != PXCondition.TODAY &&
	               typedCondition != PXCondition.THIS_WEEK &&
	               typedCondition != PXCondition.THIS_MONTH &&
	               typedCondition != PXCondition.OVERDUE &&
	               typedCondition != PXCondition.NEXT_WEEK &&
	               typedCondition != PXCondition.NEXT_MONTH;
	    }

	    private bool CheckCondition(object condition, string dataField)
	    {
	        if (condition == null) return true;

	        int[] availableValues;
	        string[] availableLabels;
	        GetConditions(dataField, out availableValues, out availableLabels);

	        var conditionInt = Convert.ToInt32(condition);
	        return availableValues.Any(val => val == conditionInt);
	    }

	    private bool CheckProperty(object propertyName)
	    {
	        return PropertyNames.Any(value => Equals(value, propertyName));
	    }

	    private static PXSetPropertyException GetPropertyException(string field)
	    {
	        return new PXSetPropertyException(PXMessages.LocalizeFormat(ErrorMessages.ElementDoesntExist, field));
	    }
	   
	
	    private string[] _propertyNames;
	    private string[] _propertyLabels;
	    private PXFieldState[] _propertyStates;
	    private readonly List<string> _numFields = new List<string>();
	    private readonly List<string> _dateFields = new List<string>();
	    private readonly List<string> _stringFields = new List<string>();

	    private static readonly int[] _commonConditionsValues;
	    private static readonly string[] _commonConditionsLabels;
	    private static readonly int[] _numberConditionsValues;
	    private static readonly string[] _numberConditionsLabels;
	    private static readonly int[] _dateConditionsValues;
	    private static readonly string[] _dateConditionsLabels;
	    static CRMarketingListMaint()
	    {
	        var commonValuesList = new List<int>();
	        var commonLabelsList = new List<string>();
	        var numberValuesList = new List<int>();
	        var numberLabelsList = new List<string>();
	        var dateValuesList = new List<int>();
	        var dateLabelsList = new List<string>();

	        foreach (var info in PXEnumDescriptionAttribute.GetFullInfo(typeof(PXCondition)))
	        {
	            var key = (byte)(int)Enum.Parse(typeof(PXCondition), info.Key.ToString());
	            switch (info.Value.Key)
	            {
	                case "NUMBER":
	                    numberValuesList.Add(key);
	                    numberLabelsList.Add(info.Value.Value);
	                    break;
	                case "DATETIME":
	                    dateValuesList.Add(key);
	                    dateLabelsList.Add(info.Value.Value);
	                    break;
	                case "COMMON":
	                    numberValuesList.Add(key);
	                    numberLabelsList.Add(info.Value.Value);
	                    dateValuesList.Add(key);
	                    dateLabelsList.Add(info.Value.Value);
	                    commonValuesList.Add(key);
	                    commonLabelsList.Add(info.Value.Value);
	                    break;
	                case "HIDDEN":
	                    break;
	                default:
	                    commonValuesList.Add(key);
	                    commonLabelsList.Add(info.Value.Value);
	                    break;
	            }
	        }
	        _commonConditionsValues = commonValuesList.ToArray();
	        _commonConditionsLabels = commonLabelsList.ToArray();
	        _numberConditionsValues = numberValuesList.ToArray();
	        _numberConditionsLabels = numberLabelsList.ToArray();
	        _dateConditionsValues = dateValuesList.ToArray();
	        _dateConditionsLabels = dateLabelsList.ToArray();
	    }
        #endregion
	    #region Implementation of IPXPrepareItems

	    public virtual bool PrepareImportRow(string viewName, IDictionary keys, IDictionary values)
	    {
	        if (string.Compare(viewName, nameof(MailRecipients), true) == 0)
	        {
	            if (values.Contains("ContactID"))
	            {

	                Contact contact;
	                int contactID;
	                if (int.TryParse(values["ContactID"].ToString(), out contactID))
	                {
	                    contact = PXSelect<Contact, Where<Contact.contactID, Equal<Required<Contact.contactID>>, And<
	                        Where<Contact.contactType, Equal<ContactTypesAttribute.lead>,
	                            Or<Contact.contactType, Equal<ContactTypesAttribute.person>>>>>>.Select(this, contactID);
	                }
	                else
	                {
	                    string contactDisplayName = values["ContactID"].ToString();
	                    contact = PXSelect<Contact, Where<Contact.memberName, Equal<Required<Contact.memberName>>, And<
	                            Where<Contact.contactType, Equal<ContactTypesAttribute.lead>,
	                                Or<Contact.contactType, Equal<ContactTypesAttribute.person>,
	                                    Or<Contact.contactType, Equal<ContactTypesAttribute.bAccountProperty>>>>>>,
	                        OrderBy<Asc<Contact.contactPriority>>>.Select(this, contactDisplayName);
	                }

	                if (contact != null)
	                {
	                    if (values.Contains("MarketingListID"))
	                        values["MarketingListID"] = this.MailLists.Current.MarketingListID;
	                    else
	                        values.Add("MarketingListID", this.MailLists.Current.MarketingListID);
	                    keys["MarketingListID"] = this.MailLists.Current.MarketingListID;

	                    if (values.Contains("ContactID"))
	                        values["ContactID"] = contact.ContactID;
	                    else
	                        values.Add("ContactID", contact.ContactID);
	                    keys["ContactID"] = contact.ContactID;
	                }
	            }
	            else
	            {
	                return false;
	            }
	        }

	        return true;
	    }

	    public bool RowImporting(string viewName, object row)
	    {
	        return row == null;
	    }

	    public bool RowImported(string viewName, object row, object oldRow)
	    {
	        return oldRow == null;
	    }

	    public virtual void PrepareItems(string viewName, IEnumerable items)
	    {
	    }

	    #endregion
    }
}
