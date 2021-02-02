using Autofac;
using System;
using System.Collections;
using PX.Common;
using PX.Data;
using PX.Data.BQL.Fluent;
using PX.Data.BQL;
using PX.Data.EP;
using PX.Objects.AP;
using PX.Objects.CS;
using PX.Objects.CT;
using PX.Objects.SO;
using PX.SM;
using PX.Objects.CR.DAC;
using PX.Objects.CA;
using PX.Objects.CR.MassProcess;
using PX.Data.MassProcess;
using PX.Objects.CR.Extensions;
using PX.Objects.CR.Extensions.CRDuplicateEntities;
using PX.Objects.CR.Extensions.CRCreateActions;
using System.Linq;
using System.Collections.Generic;
using PX.Objects.CR.Extensions.Relational;
using PX.CS.Contracts.Interfaces;
using PX.Objects.GDPR;
using PX.Objects.GraphExtensions.ExtendBAccount;
using PX.Objects.AR;
using System.Web.Compilation;
using PX.Data.DependencyInjection;
using CRLocation = PX.Objects.CR.Standalone.Location;

namespace PX.Objects.CR
{
	public class BusinessAccountMaint : PXGraph<BusinessAccountMaint, BAccount>
	{
		#region Selects

		[PXHidden]
		public PXSelect<BAccount>
			BaseBAccounts;

        [PXHidden]
        public PXSelect<BAccountCRM>
            BaseBAccountsCRM;

		[PXHidden]
		public PXSetup<GL.Branch>
			Branches;

		[PXHidden]
		[PXCheckCurrent]
		public CM.CMSetupSelect 
			CMSetup;

		[PXHidden]
		[PXCheckCurrent]
		public PXSetup<GL.Company>
			cmpany;

		[PXHidden]
		[PXCheckCurrent]
		public PXSetup<CRSetup> 
			Setup;

		[PXHidden]
		public PXSelect<CRLocation>
			BaseLocations;

		[PXViewName(Messages.BAccount)]
		public PXSelectJoin<BAccount, 
			LeftJoin<Contact, 
				On<Contact.contactID, Equal<BAccount.defContactID>>>,
			Where2<Match<Current<AccessInfo.userName>>, 
			And<Where<BAccount.type, Equal<BAccountType.customerType>,
				Or<BAccount.type, Equal<BAccountType.prospectType>,
				Or<BAccount.type, Equal<BAccountType.combinedType>,
				Or<BAccount.type, Equal<BAccountType.vendorType>>>>>>>>
			BAccount;

		[PXHidden]
		[PXCopyPasteHiddenFields(typeof(BAccount.primaryContactID))]
		public PXSelectJoin<BAccount, 
			LeftJoin<Contact, 
				On<Contact.contactID, Equal<BAccount.defContactID>>>,
			Where<BAccount.bAccountID, Equal<Current<BAccount.bAccountID>>>>
			CurrentBAccount;

		[PXCopyPasteHiddenView]
		public PXSelect<CRActivityStatistics,
				Where<CRActivityStatistics.noteID, Equal<Current<BAccount.noteID>>>>
			BAccountActivityStatistics;

		[PXHidden]
		public PXSelect<Address>
			AddressDummy;

		[PXHidden]
		public PXSelect<Contact>
			ContactDummy;

		[PXViewName(Messages.Leads)]
		[PXFilterable]
		[PXViewDetailsButton(typeof(Contact))]
		public PXSelectJoin<
				CRLead,
			InnerJoin<Address,
				On<Address.addressID, Equal<Contact.defAddressID>>,
			LeftJoin<CRActivityStatistics,
				On<CRActivityStatistics.noteID, Equal<CRLead.noteID>>>>,
			Where<
				CRLead.bAccountID, Equal<Current<BAccount.bAccountID>>>,
			OrderBy<
				Desc<CRLead.createdDateTime>>>
			Leads;

		[PXViewName(Messages.Answers)]
		public CRAttributeList<BAccount>
			Answers;

		[PXViewName(Messages.Activities)]
		[PXFilterable]
        [CRReference(typeof(BAccount.bAccountID), Persistent = true)]
		public CRActivityList<BAccount> 
			Activities;
		
        [PXHidden]
		public PXFilter<ActivityContactFilter> 
			ActivityContacts;

		[PXHidden]
		public PXSelect<CROpportunityClass>
			CROpportunityClass;

		[PXViewName(Messages.Opportunities)]
		[PXFilterable]
		[PXViewDetailsButton(typeof(BAccount))]
		[PXViewDetailsButton(typeof(BAccount),
			typeof(Select<BAccount,
				Where<BAccount.bAccountID, Equal<Current<CROpportunity.bAccountID>>>>))]
		[PXViewDetailsButton(typeof(BAccount),
			typeof(Select<Contact,
				Where<Contact.contactID, Equal<Current<CROpportunity.contactID>>>>))]
		public PXSelectJoin<CROpportunity,			
			LeftJoin<Contact, On<Contact.contactID, Equal<CROpportunity.contactID>>, 
			LeftJoin<CROpportunityProbability, On<CROpportunityProbability.stageCode, Equal<CROpportunity.stageID>>,
			LeftJoin<BAccount, On<BAccount.bAccountID, Equal<CROpportunity.bAccountID>>,
			LeftJoin<CROpportunityClass, On<CROpportunityClass.cROpportunityClassID, Equal<CROpportunity.classID>>>>>>,
			Where<BAccount.bAccountID, Equal<Current<BAccount.bAccountID>>,
					Or<BAccount.parentBAccountID, Equal<Current<BAccount.bAccountID>>>>> 
			Opportunities;

		[PXHidden]
		public PXSelect<CROpportunity> OpportunityLink;
		
		[PXCopyPasteHiddenView]
		[PXViewName(Messages.Relations)]
		[PXFilterable]
		public CRRelationsList<BAccount.noteID>
			Relations;

		[PXViewName(Messages.Cases)]
		[PXFilterable]
		[PXViewDetailsButton(typeof(BAccount))]
		[PXViewDetailsButton(typeof(BAccount),
			typeof(Select<Contact,
				Where<Contact.contactID, Equal<Current<CRCase.contactID>>>>))]
		public PXSelectReadonly2<CRCase,
			LeftJoin<Contact, On<Contact.contactID, Equal<CRCase.contactID>>>, 
			Where<CRCase.customerID, Equal<Current<BAccount.bAccountID>>>> 
			Cases;

		[PXViewName(Messages.Contracts)]
		[PXFilterable]
		[PXViewDetailsButton(typeof(BAccount))]
		[PXViewDetailsButton(typeof(BAccount),
			typeof(Select<Location,
				Where<Location.bAccountID, Equal<Current<Contract.customerID>>, And<Location.locationID, Equal<Current<Contract.locationID>>>>>))]
		[PXViewDetailsButton(typeof(BAccount),
			typeof(Select<BAccount,
				Where<BAccount.bAccountID, Equal<Current<Contract.customerID>>>>))]
		public PXSelectReadonly2<Contract,
			LeftJoin<ContractBillingSchedule, On<ContractBillingSchedule.contractID, Equal<Contract.contractID>>, 
			LeftJoin<BAccount, On<BAccount.bAccountID, Equal<Contract.customerID>>>>,
			Where<Contract.baseType, Equal<CTPRType.contract>, 
			  And<Where<BAccount.bAccountID, Equal<Current<BAccount.bAccountID>>,
							 Or<ContractBillingSchedule.accountID, Equal<Current<BAccount.bAccountID>>>>>>>
			Contracts;

		[PXViewName(Messages.Orders)]
		[PXFilterable]
		[PXViewDetailsButton(typeof(BAccount))]
		public PXSelectReadonly<SOOrder,
			Where<SOOrder.customerID, Equal<Current<BAccount.bAccountID>>>>
			Orders;

        [PXCopyPasteHiddenView]
		[PXViewName(Messages.CampaignMember)]
		[PXFilterable]
		[PXViewDetailsButton(typeof(BAccount),
			typeof(Select<CRCampaign,
				Where<CRCampaign.campaignID, Equal<Current<CRCampaignMembers.campaignID>>>>))]
		public PXSelectJoin<CRCampaignMembers,
			InnerJoin<CRCampaign, On<CRCampaignMembers.campaignID, Equal<CRCampaign.campaignID>>, 
			InnerJoin<Contact, On<Contact.contactID, Equal<CRCampaignMembers.contactID>>>>,
			Where<Contact.bAccountID, Equal<Current<BAccount.bAccountID>>>>
			Members;

		[PXHidden]
		public PXSelect<CRMarketingListMember>
			Subscriptions_stub;

		[PXViewName(Messages.Subscriptions)]
		[PXFilterable]
		[PXViewDetailsButton(typeof(BAccount),
			typeof(Select<CRMarketingList,
				Where<CRMarketingList.marketingListID, Equal<Current<CRMarketingListMember.marketingListID>>>>))]
		[PXViewDetailsButton(typeof(BAccount),
			typeof(Select<Contact,
				Where<Contact.contactID, Equal<Current<CRMarketingListMember.contactID>>>>), ActionName = "Subscriptions_Contact_ViewDetails")]
		public CRMMarketingBAccountSubscriptions
			Subscriptions;

        public CRNotificationSourceList<BAccount, BAccount.classID, CRNotificationSource.bAccount> NotificationSources;

        public CRNotificationRecipientList<BAccount, BAccount.classID> NotificationRecipients;

        #endregion

        #region Ctors
        public BusinessAccountMaint()
		{
			if (Branches.Current.BAccountID.HasValue == false) //TODO: need review
                throw new PXSetupNotEnteredException(ErrorMessages.SetupNotEntered, typeof(GL.Branch), PXMessages.LocalizeNoPrefix(CS.Messages.BranchMaint));

			PXUIFieldAttribute.SetDisplayName<BAccount.acctCD>(BAccount.Cache, Messages.BAccountCD);
			PXUIFieldAttribute.SetDisplayName<Carrier.description>(Caches[typeof(Carrier)], Messages.CarrierDescription);

			Activities.GetNewEmailAddress =
				() =>
				{
					var contact = (Contact)PXSelect<Contact,
						Where<Contact.contactID, Equal<Current<BAccount.defContactID>>>>.
						Select(this);

					return contact != null && !string.IsNullOrWhiteSpace(contact.EMail)
						? PXDBEmailAttribute.FormatAddressesWithSingleDisplayName(contact.EMail, contact.DisplayName)
						: String.Empty;
				};

			PXUIFieldAttribute.SetRequired<BAccount.status>(BAccount.Cache, true);

			Action.AddMenuAction(ChangeID);

            PXUIFieldAttribute.SetVisible<CRMarketingListMember.format>(Subscriptions.Cache, null, false);
		}

		#endregion

		#region Actions

		public PXMenuAction<BAccount> Action;

		public PXDBAction<BAccount> addOpportunity;
		[PXUIField(DisplayName = Messages.AddNewOpportunity)]
		[PXButton(ImageKey = PX.Web.UI.Sprite.Main.AddNew)]
		public virtual void AddOpportunity()
		{
			var row = CurrentBAccount.Current;
			if (row == null || row.BAccountID == null) return;

			var graph = PXGraph.CreateInstance<OpportunityMaint>();
            var newOpportunity = graph.Opportunity.Insert();
			newOpportunity.BAccountID = row.BAccountID;
			newOpportunity.LocationID = row.DefLocationID;

			CROpportunityClass ocls = PXSelect<CROpportunityClass, Where<CROpportunityClass.cROpportunityClassID, Equal<Current<CROpportunity.classID>>>>
				.SelectSingleBound(this, new object[] { newOpportunity });
			if (ocls?.DefaultOwner == CRDefaultOwnerAttribute.Source)
			{
				newOpportunity.WorkgroupID = row.WorkgroupID;
				newOpportunity.OwnerID = row.OwnerID;
			}

			//TODO: need calculate default contact
            newOpportunity = graph.Opportunity.Update(newOpportunity);
            graph.Opportunity.SetValueExt<CROpportunity.bAccountID>(newOpportunity, row.BAccountID);
            graph.Answers.CopyAllAttributes(newOpportunity, row);
			
			if (!this.IsContractBasedAPI)
				PXRedirectHelper.TryRedirect(graph, PXRedirectHelper.WindowMode.NewWindow);

			graph.Save.Press();
		}

		public PXDBAction<BAccount> addCase;
		[PXUIField(DisplayName = Messages.AddNewCase)]
		[PXButton(ImageKey = PX.Web.UI.Sprite.Main.AddNew)]
		public virtual void AddCase()
		{
			var row = CurrentBAccount.Current;
			if (row == null || row.BAccountID == null) return;

            var graph = PXGraph.CreateInstance<CRCaseMaint>();

            var newCase = (CRCase)graph.Case.Cache.Insert();
            newCase.CustomerID = row.BAccountID;
            newCase.LocationID = row.DefLocationID;

            //TODO: need calculate default contact
            newCase = graph.Case.Update(newCase);
            graph.Answers.CopyAllAttributes(newCase, row);
			
			if (!this.IsContractBasedAPI)
				PXRedirectHelper.TryRedirect(graph, PXRedirectHelper.WindowMode.NewWindow);

			graph.Save.Press();
		}

		public PXChangeID<BAccount, BAccount.acctCD> ChangeID;
        #endregion

        #region Cache Attached
        #region NotificationSource
        [PXSelector(typeof(Search<NotificationSetup.setupID,
            Where<NotificationSetup.sourceCD, Equal<CRNotificationSource.bAccount>>>),
			DescriptionField = typeof(NotificationSetup.notificationCD),
			SelectorMode = PXSelectorMode.DisplayModeText | PXSelectorMode.NoAutocomplete)]
        [PXUIField(DisplayName = "Mailing ID")]
		[PXMergeAttributes(Method = MergeMethod.Merge)]
        protected virtual void NotificationSource_SetupID_CacheAttached(PXCache sender)
        {
        }
        [PXDBString(10, IsUnicode = true)]
		[PXMergeAttributes(Method = MergeMethod.Replace)]
        protected virtual void NotificationSource_ClassID_CacheAttached(PXCache sender)
        {
        }
        [PXCheckUnique(typeof(NotificationSource.setupID), IgnoreNulls = false,
            Where = typeof(Where<NotificationSource.refNoteID, Equal<Current<NotificationSource.refNoteID>>>))]
		[PXMergeAttributes(Method = MergeMethod.Merge)]
        protected virtual void NotificationSource_NBranchID_CacheAttached(PXCache sender)
        {

        }
        [PXUIField(DisplayName = "Report")]
        [PXSelector(typeof(Search<SiteMap.screenID,
            Where<SiteMap.url, Like<urlReports>,
                And<SiteMap.screenID, Like<PXModule.cr_>>>,
            OrderBy<Asc<SiteMap.screenID>>>), typeof(SiteMap.screenID), typeof(SiteMap.title),
            Headers = new string[] { CA.Messages.ReportID, CA.Messages.ReportName },
            DescriptionField = typeof(SiteMap.title))]
		[PXMergeAttributes(Method = MergeMethod.Merge)]
        protected virtual void NotificationSource_ReportID_CacheAttached(PXCache sender)
        {
        }
        #endregion

        #region NotificationRecipient
        [PXDBDefault(typeof(NotificationSource.sourceID))]
		[PXMergeAttributes(Method = MergeMethod.Merge)]
        protected virtual void NotificationRecipient_SourceID_CacheAttached(PXCache sender)
        {
        }
        [PXDefault]
        [CRMContactType.List]
        [PXCheckUnique(typeof(NotificationRecipient.contactID),
            Where = typeof(Where<NotificationRecipient.sourceID, Equal<Current<NotificationRecipient.sourceID>>,
            And<NotificationRecipient.refNoteID, Equal<Current<BAccount.noteID>>>>))]
		[PXMergeAttributes(Method = MergeMethod.Merge)]
        protected virtual void NotificationRecipient_ContactType_CacheAttached(PXCache sender)
        {
        }
        [PXUIField(DisplayName = "Contact ID")]
        [PXNotificationContactSelector(typeof(NotificationRecipient.contactType), DirtyRead = true)]
		[PXMergeAttributes(Method = MergeMethod.Merge)]
        protected virtual void NotificationRecipient_ContactID_CacheAttached(PXCache sender)
        {
        }
        [PXDBString(10, IsUnicode = true)]
		[PXMergeAttributes(Method = MergeMethod.Replace)]	// remove PXDefault from DAC
        protected virtual void NotificationRecipient_ClassID_CacheAttached(PXCache sender)
        {
        }
        [PXUIField(DisplayName = "Email", Enabled = false)]
		[PXMergeAttributes(Method = MergeMethod.Merge)]
        protected virtual void NotificationRecipient_Email_CacheAttached(PXCache sender)
        {
        }
        #endregion

        #region CROpportunityClass
        [PXUIField(DisplayName = "Class Description")]
        [PXMergeAttributes(Method = MergeMethod.Merge)]
        protected virtual void CROpportunityClass_Description_CacheAttached(PXCache sender)
        {
        }
        #endregion

        #endregion

        #region Event Handlers
        #region NotificationRecipient Events
        protected virtual void NotificationRecipient_RowSelected(PXCache cache, PXRowSelectedEventArgs e)
        {
            NotificationRecipient row = (NotificationRecipient)e.Row;
            if (row == null) return;
            Contact contact = PXSelectorAttribute.Select<NotificationRecipient.contactID>(cache, row) as Contact;
            if (contact == null)
            {
                switch (row.ContactType)
                {
                    case CRMContactType.Primary:
                        var defContactAddress = this.GetExtension<DefContactAddressExt>();
                        contact = defContactAddress.DefContact.SelectWindowed(0, 1);
                        break;
                    case CRMContactType.Shipping:
                        var defLocationExt = this.GetExtension<DefLocationExt>();
                        contact = defLocationExt.DefLocationContact.View.SelectSingle(new object[] { BAccount.Cache.Current }) as Contact;
                        break;
                }
            }
            if (contact != null)
                row.Email = contact.EMail;
        }
        #endregion  

        #region SOOrder

		[SOOrderStatus.ListWithoutOrders()]
		[PXMergeAttributes(Method = MergeMethod.Merge)]
		protected virtual void SOOrder_Status_CacheAttached(PXCache sender)
		{

		}

		#endregion

		#region BAccount

		[PXDimensionSelector("BIZACCT",
			typeof(Search2<
				BAccountCRM.bAccountID,
			LeftJoin<Contact,
				On<Contact.bAccountID, Equal<BAccountCRM.bAccountID>,
				And<Contact.contactID, Equal<BAccountCRM.defContactID>>>,
			LeftJoin<Address,
				On<Address.bAccountID, Equal<BAccountCRM.bAccountID>,
				And<Address.addressID, Equal<BAccountCRM.defAddressID>>>>>,
			Where<
				Current<BAccount.bAccountID>, NotEqual<BAccountCRM.bAccountID>,
				And2<Where<
					BAccountCRM.type, Equal<BAccountType.customerType>,
					Or<BAccountCRM.type, Equal<BAccountType.prospectType>,
					Or<BAccountCRM.type, Equal<BAccountType.combinedType>,
					Or<BAccountCRM.type, Equal<BAccountType.vendorType>>>>>,
				And<Match<Current<AccessInfo.userName>>>>>>),
			substituteKey: typeof(BAccountCRM.acctCD),
			fieldList: new[]
			{
				typeof(BAccountCRM.acctCD),
				typeof(BAccountCRM.acctName),
				typeof(BAccountCRM.type),
				typeof(BAccountCRM.classID),
				typeof(BAccountCRM.status),
				typeof(Contact.phone1),
				typeof(Address.city),
				typeof(Address.countryID),
				typeof(Contact.eMail)
			},
			DescriptionField = typeof(BAccountCRM.acctName))]
		[PXMergeAttributes(Method = MergeMethod.Merge)]
		protected virtual void _(Events.CacheAttached<BAccount.parentBAccountID> e) { }

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
			typeof(BAccount.acctCD), typeof(BAccount.acctName), typeof(BAccount.type), typeof(BAccount.classID), typeof(BAccount.status), typeof(Contact.phone1), 
			typeof(Address.city), typeof(Address.countryID), typeof(Contact.eMail))]
		[PXUIField(DisplayName = "Business Account ID", Visibility = PXUIVisibility.SelectorVisible)]
		[PXMergeAttributes(Method = MergeMethod.Merge)]
		protected virtual void BAccount_AcctCD_CacheAttached(PXCache cache)
		{
			
		}

		protected virtual void BAccount_ClassID_FieldDefaulting(PXCache cache, PXFieldDefaultingEventArgs e)
		{
			e.NewValue = Setup.Current.DefaultCustomerClassID;
		}

		protected virtual void BAccount_Type_FieldDefaulting(PXCache cache, PXFieldDefaultingEventArgs e)
		{
			e.NewValue = BAccountType.ProspectType;
		}

	    protected virtual void BAccountCRM_RowSelected(PXCache cache, PXRowSelectedEventArgs e)
	    {
            //Used for corrrect navigation
	        this.BAccount.Current = (BAccount)e.Row;
	    }

	    protected virtual void BAccount_RowSelected(PXCache cache, PXRowSelectedEventArgs e)
		{
			var row = (BAccount)e.Row;
			if (row == null) return;

			var isNotInserted = cache.GetStatus(row) != PXEntryStatus.Inserted;
			Relations.Cache.AllowInsert = isNotInserted;
			Opportunities.Cache.AllowInsert = isNotInserted;
			Cases.Cache.AllowInsert = isNotInserted;
			Members.Cache.AllowInsert = isNotInserted;
			Subscriptions.Cache.AllowInsert = isNotInserted;

			var isCustomerOrCombined = row.Type == BAccountType.CustomerType || row.Type == BAccountType.CombinedType;
			var isVendorOrCombined = row.Type == BAccountType.VendorType || row.Type == BAccountType.CombinedType;
			var isCustomerOrProspect = row.Type == BAccountType.CustomerType || row.Type == BAccountType.ProspectType;
			var isCustomerOrProspectOrCombined = isCustomerOrProspect || row.Type == BAccountType.CombinedType;

			addOpportunity.SetEnabled(isNotInserted && isCustomerOrProspectOrCombined);
			addCase.SetEnabled(isNotInserted && isCustomerOrProspectOrCombined);

			ChangeID.SetEnabled(row.IsBranch != true);

			PXUIFieldAttribute.SetEnabled<BAccount.parentBAccountID>(cache, row, isCustomerOrCombined == false || PXAccess.FeatureInstalled<FeaturesSet.parentChildAccount>() == false);
			
		    CRCustomerClass customerClass = row.ClassID.
		        With(_ => (CRCustomerClass)PXSelectReadonly<CRCustomerClass,
		                Where<CRCustomerClass.cRCustomerClassID, Equal<Required<CRCustomerClass.cRCustomerClassID>>>>.
		            Select(this, _));
		    if (customerClass != null)
		    {
		        Activities.DefaultEMailAccountId = customerClass.DefaultEMailAccountID;
		    }
		}

		protected virtual void BAccount_RowPersisting(PXCache cache, PXRowPersistingEventArgs e)
		{
			BAccount row = e.Row as BAccount;

			if (row != null && (row.Type == BAccountType.CustomerType || row.Type == BAccountType.CombinedType))
			{
				AR.Customer customer =
					SelectFrom<AR.Customer>
					.Where<AR.Customer.acctCD.IsEqual<@P.AsString>>
					.View
					.Select(this, row.AcctCD);
				AR.CustomerMaint.VerifyParentBAccountID<BAccount.parentBAccountID>(this, cache, customer, row);
			}
		}

		#endregion

		#region Contact

		[PXDefault(ContactTypesAttribute.BAccountProperty)]
		[PXMergeAttributes(Method = MergeMethod.Merge)]
		protected virtual void _(Events.CacheAttached<Contact.contactType> e) { }

		[PXUIField(DisplayName = "Business Account", Visibility = PXUIVisibility.SelectorVisible)]
		[PXDBDefault(typeof(BAccount.bAccountID), PersistingCheck = PXPersistingCheck.Nothing)]
		[PXSelector(typeof(BAccount.bAccountID), SubstituteKey = typeof(BAccount.acctCD), DirtyRead = true)]
		[PXParent(typeof(Select<BAccount, Where<BAccount.bAccountID, Equal<Current<Contact.bAccountID>>>>))]
		[PXMergeAttributes(Method = MergeMethod.Merge)]
		public virtual void _(Events.CacheAttached<CRLead.bAccountID> e) { }

		[PXUIField(DisplayName = "Business Account", Visibility = PXUIVisibility.SelectorVisible)]
		[PXDBDefault(typeof(BAccount.bAccountID), PersistingCheck = PXPersistingCheck.Nothing)]
		[PXSelector(typeof(BAccount.bAccountID), SubstituteKey = typeof(BAccount.acctCD), DirtyRead = true)]
		[PXParent(typeof(Select<BAccount, Where<BAccount.bAccountID, Equal<Current<Contact.bAccountID>>>>))]
		[PXMergeAttributes(Method = MergeMethod.Merge)]
		public virtual void _(Events.CacheAttached<Contact.bAccountID> e) { }

		[PXMergeAttributes(Method = MergeMethod.Merge)]
		[PXUIField(DisplayName = "Member Name", Visibility = PXUIVisibility.SelectorVisible, Visible = true, Enabled = false)]
		public virtual void _(Events.CacheAttached<Contact.memberName> e) { }

		[PXDBInt(BqlField = typeof(Standalone.CROpportunityRevision.bAccountID))]
		[PXUIField(DisplayName = "Business Account", Visibility = PXUIVisibility.SelectorVisible)]
		[PXDBDefault(typeof(BAccount.bAccountID))]
		[PXSelector(typeof(BAccount.bAccountID), SubstituteKey = typeof(BAccount.acctCD), DirtyRead = true)]
		[PXMergeAttributes(Method = MergeMethod.Replace)]   // replace single attribute from DAC
		public virtual void _(Events.CacheAttached<CROpportunity.bAccountID> e) { }

        [PXUIField(DisplayName = "Location")]                
        [PXDBDefault(typeof(CRLocation.locationID))]
        [LocationID(typeof(Where<Location.bAccountID, Equal<Current<CROpportunity.bAccountID>>>),
            DisplayName = "Location", 
            DescriptionField = typeof(Location.descr),
            BqlField = typeof(Standalone.CROpportunityRevision.locationID),
            ValidateValue = false)]
		[PXMergeAttributes(Method = MergeMethod.Replace)]
		public virtual void _(Events.CacheAttached<CROpportunity.locationID> e) { }

		#endregion

		#region Lead

		[PXUIField(DisplayName = "Display Name", Visibility = PXUIVisibility.SelectorVisible, Enabled = false)]
		[PXMergeAttributes(Method = MergeMethod.Merge)]
		protected virtual void _(Events.CacheAttached<CRLead.memberName> e) { }

		#endregion

		#region Address

		[PXDBDefault(typeof(BAccount.bAccountID), PersistingCheck = PXPersistingCheck.Nothing)]
		[PXParent(typeof(Select<BAccount, Where<BAccount.bAccountID, Equal<Current<Address.bAccountID>>>>))]
		[PXMergeAttributes(Method = MergeMethod.Merge)]
		protected virtual void Address_BAccountID_CacheAttached(PXCache sender) { }

		#endregion

		#region CRCampaignMembers


		[PXDBInt(IsKey = true)]
		[PXDBDefault(typeof(BAccount.defContactID))]
		[PXMergeAttributes(Method = MergeMethod.Replace)]
		protected virtual void CRCampaignMembers_ContactID_CacheAttached(PXCache sender)
		{
		}

		[PXMergeAttributes(Method = MergeMethod.Merge)]
		[PXCheckUnique(typeof(CRCampaignMembers.contactID))]
		protected virtual void CRCampaignMembers_CampaignID_CacheAttached(PXCache sender)
		{
		}

		#endregion

		#region CRMarketingListMember

		[PXDBInt(IsKey = true)]
		[PXDBDefault(typeof(BAccount.defContactID), DefaultForUpdate = false)]
		[PXUIField(DisplayName = "Name")]
		[PXMergeAttributes(Method = MergeMethod.Replace)]
		protected virtual void CRMarketingListMember_ContactID_CacheAttached(PXCache sender)
		{
		}

		#endregion

		#region CRPMTimeActivity

		[PXSelector(typeof(Contact.contactID), DescriptionField = typeof(Contact.memberName))]
		[PXMergeAttributes(Method = MergeMethod.Merge)]
		protected virtual void CRPMTimeActivity_ContactID_CacheAttached(PXCache sender) { }

		[PXDBDefault(typeof(BAccount.bAccountID), PersistingCheck = PXPersistingCheck.Nothing)]
        [PXDBChildIdentity(typeof(BAccount.bAccountID))]
        [PXMergeAttributes(Method = MergeMethod.Merge)]
        protected virtual void CRPMTimeActivity_BAccountID_CacheAttached(PXCache sender) { }

        #endregion

        #region CRRelation

        [PXDBChildIdentity(typeof(BAccount.bAccountID))]
        [PXMergeAttributes(Method = MergeMethod.Merge)]
        protected virtual void _(Events.CacheAttached<CRRelation.entityID> e) { }

        #endregion

        #endregion

		#region Extensions

		#region Details

		/// <exclude/>
		// Acuminator disable once PX1016 ExtensionDoesNotDeclareIsActiveMethod extension should be constantly active
		public class DefContactAddressExt : DefContactAddressExt<BusinessAccountMaint, BAccount, BAccount.acctName> { }

		/// <exclude/>
		// Acuminator disable once PX1016 ExtensionDoesNotDeclareIsActiveMethod extension should be constantly active
		public class DefLocationExt : DefLocationExt<BusinessAccountMaint, DefContactAddressExt, LocationDetailsExt, BAccount, BAccount.bAccountID, BAccount.defLocationID>
			.WithUIExtension
		{
			#region State

			public override void Initialize()
			{
				base.Initialize();

				/* HACK:
				 * optimized export use dummy BaseLocations (sometimes) as
				 * view for Location.Current, that leads to invalid join:
				 * left join Location on 1 = 1
				*/
				if (Base.IsContractBasedAPI)
					Base.Views[nameof(Base.BaseLocations)] =
						new PXView(Base, false, DefLocation.View.BqlSelect);
			}

			public override List<Type> InitLocationFields => new List<Type>
			{
				typeof(CRLocation.vTaxCalcMode),
				typeof(CRLocation.vRetainageAcctID),
				typeof(CRLocation.vRetainageSubID),
			};

			#endregion
		}

		/// <exclude/>
		// Acuminator disable once PX1016 ExtensionDoesNotDeclareIsActiveMethod extension should be constantly active
		public class ContactDetailsExt : BusinessAccountContactDetailsExt<BusinessAccountMaint, CreateContactFromAccountGraphExt, BAccount, BAccount.bAccountID> { }

		/// <exclude/>
		// Acuminator disable once PX1016 ExtensionDoesNotDeclareIsActiveMethod extension should be constantly active
		public class LocationDetailsExt : LocationDetailsExt<BusinessAccountMaint, DefContactAddressExt, BAccount, BAccount.bAccountID> { }

		/// <exclude/>
		// Acuminator disable once PX1016 ExtensionDoesNotDeclareIsActiveMethod extension should be constantly active
		public class PrimaryContactGraphExt : CRPrimaryContactGraphExt<
			BusinessAccountMaint, ContactDetailsExt,
			BAccount, BAccount.bAccountID, BAccount.primaryContactID>
		{
			protected override PXView ContactsView => Base1.Contacts.View;

			#region Events

			[PXVerifySelector(typeof(
				SelectFrom<Contact>
				.Where<
					Contact.bAccountID.IsEqual<BAccount.bAccountID.FromCurrent>
					.And<Contact.contactType.IsEqual<ContactTypesAttribute.person>>
					.And<Contact.isActive.IsEqual<True>>
				>
				.SearchFor<Contact.contactID>),
				fieldList: new[]
				{
					typeof(Contact.displayName),
					typeof(Contact.salutation),
					typeof(Contact.phone1),
					typeof(Contact.eMail)
				},
				VerifyField = false,
				DescriptionField = typeof(Contact.displayName)
			)]
			[PXUIField(DisplayName = "Name")]
			[PXMergeAttributes(Method = MergeMethod.Merge)]
			protected virtual void _(Events.CacheAttached<BAccount.primaryContactID> e) { }

			#endregion
		}

		#endregion

		#region Address Lookup Extension

		/// <exclude/>
		// Acuminator disable once PX1016 ExtensionDoesNotDeclareIsActiveMethod extension should be constantly active
		public class BusinessAccountMaintAddressLookupExtension : CR.Extensions.AddressLookupExtension<BusinessAccountMaint, BAccount, Address>
		{
			protected override string AddressView => nameof(DefContactAddressExt.DefAddress);
			protected override string ViewOnMap => nameof(DefContactAddressExt.ViewMainOnMap);
		}

		/// <exclude/>
		// Acuminator disable once PX1016 ExtensionDoesNotDeclareIsActiveMethod extension should be constantly active
		public class BusinessAccountMaintDefLocationAddressLookupExtension : CR.Extensions.AddressLookupExtension<BusinessAccountMaint, BAccount, Address>
		{
			protected override string AddressView => nameof(DefLocationExt.DefLocationAddress);
			protected override string ViewOnMap => nameof(DefLocationExt.ViewDefLocationAddressOnMap);
		}

		#endregion

		/// <exclude/>
		// Acuminator disable once PX1016 ExtensionDoesNotDeclareIsActiveMethod extension should be constantly active
		public class ExtendToCustomer : ExtendToCustomerGraph<BusinessAccountMaint, BAccount>
		{
			#region Initialization 

			public override void Initialize()
			{
				Base.Actions.Move(nameof(Base.Action), nameof(this.viewCustomer));
			}

			protected override SourceAccountMapping GetSourceAccountMapping()
			{
				return new SourceAccountMapping(typeof(BAccount));
			}

			#endregion

			#region Overrides

			protected override void _(Events.RowSelected<BAccount> e)
			{
				base._(e);

				viewCustomer.SetVisible(viewCustomer.GetEnabled());
			}

			#endregion
		}

		/// <exclude/>
		// Acuminator disable once PX1016 ExtensionDoesNotDeclareIsActiveMethod extension should be constantly active
		public class ExtendToVendor : ExtendToVendorGraph<BusinessAccountMaint, BAccount>
		{
			#region Initialization 

			public override void Initialize()
			{
				Base.Actions.Move(nameof(Base.Action), nameof(this.viewVendor));
			}

			protected override SourceAccountMapping GetSourceAccountMapping()
			{
				return new SourceAccountMapping(typeof(BAccount));
			}

			#endregion

			#region Overrides

			protected override void _(Events.RowSelected<BAccount> e)
			{
				base._(e);

				viewVendor.SetVisible(viewVendor.GetEnabled());
			}

			#endregion
		}

		/// <exclude/>
		// Acuminator disable once PX1016 ExtensionDoesNotDeclareIsActiveMethod extension should be constantly active
		public class DefaultAccountOwner : CRDefaultDocumentOwner<
			BusinessAccountMaint, BAccount,
			BAccount.classID, BAccount.ownerID, BAccount.workgroupID>
		{ }

		/// <exclude/>
		// Acuminator disable once PX1016 ExtensionDoesNotDeclareIsActiveMethod extension should be constantly active
		public class CRDuplicateEntitiesForBAccountGraphExt : CRDuplicateEntities<BusinessAccountMaint, BAccount>
		{
			#region Initialization 

			protected override Type AdditionalConditions => typeof(
				
				DuplicateContact.contactType.IsEqual<ContactTypesAttribute.bAccountProperty>
				.And<BAccountR.status.IsNotEqual<BAccountR.status.inactive>>
			);

			protected override string WarningMessage => Messages.BAccountHavePossibleDuplicates;

			public static bool IsActive()
			{
				return IsExtensionActive();
			}

			public override void Initialize()
			{
				base.Initialize();

				DuplicateDocuments = new PXSelectExtension<DuplicateDocument>(Base.GetExtension<DefContactAddressExt>().DefContact);
			}

			protected override DocumentMapping GetDocumentMapping()
			{
				return new DocumentMapping(typeof(BAccount)) { Key = typeof(BAccount.bAccountID) };
			}

			protected override DuplicateDocumentMapping GetDuplicateDocumentMapping()
			{
				return new DuplicateDocumentMapping(typeof(Contact)) { Email = typeof(Contact.eMail) };
			}

			#endregion

			#region Events

			[CRDuplicateBAccountSelector(typeof(MergeParams.sourceEntityID), SelectorMode = PXSelectorMode.DisplayModeHint)]
			[PXMergeAttributes(Method = MergeMethod.Append)]
			protected virtual void _(Events.CacheAttached<MergeParams.targetEntityID> e) { }

			protected virtual void _(Events.FieldUpdated<BAccount, BAccount.status> e)
			{
				BAccount row = e.Row as BAccount;
				if (e.Row == null)
					return;

				if (row.Status != CR.BAccount.status.Inactive && row.Status != (string)e.OldValue)
				{
					var dupDoc = base.DuplicateDocuments.Current;

					base.DuplicateDocuments.SetValueExt<DuplicateDocument.duplicateStatus>(dupDoc, DuplicateStatusAttribute.NotValidated);
				}
			}

			public virtual void _(Events.FieldDefaulting<MergeParams, MergeParams.targetEntityID> e)
			{
				BAccount current = (BAccount)Base.Caches<BAccount>().Current;

				List<BAccount> duplicates = PXSelectorAttribute.SelectAll<MergeParams.targetEntityID>(e.Cache, e.Row)
					.RowCast<BAccount>()
					.Where(a => a.Type == BAccountType.CustomerType || a.Type == BAccountType.VendorType || a.Type == BAccountType.CombinedType)
					.ToList();

				e.NewValue = (current.Type == BAccountType.CustomerType || current.Type == BAccountType.VendorType || current.Type == BAccountType.CombinedType) 
							&& current.Status != CR.BAccount.status.Inactive
							|| duplicates.Count == 0
					? current.BAccountID
					: duplicates
						.OrderBy(duplicate => duplicate?.BAccountID)
						.FirstOrDefault()
						?.BAccountID;
			}

			public virtual void _(Events.FieldVerifying<MergeParams, MergeParams.targetEntityID> e)
			{
				int? targetID = e.NewValue as int?;

				if (PXSelectorAttribute.SelectAll<MergeParams.targetEntityID>(e.Cache, e.Row)
					.RowCast<BAccount>()
					.Any(account => account.BAccountID != targetID && account.Type != BAccountType.ProspectType))
				{
					BAccount acct = PXSelect<BAccount, Where<BAccount.bAccountID, Equal<Required<MergeParams.targetEntityID>>>>.Select(Base, e.Row.TargetEntityID);

					PXUIFieldAttribute.SetError<MergeParams.targetEntityID>(e.Cache, e.Row, Messages.OnlyBAccountMergeSources, acct.AcctCD);
				}
			}

			public virtual void _(Events.FieldSelecting<MergeParams, MergeParams.targetEntityID> e)
			{
				if (e.Row == null)
					return;

				int? targetID = e.Row.TargetEntityID;

				if (PXSelectorAttribute.SelectAll<MergeParams.targetEntityID>(e.Cache, e.Row)
					.RowCast<BAccount>()
					.Any(account => account.BAccountID != targetID && account.Type != BAccountType.ProspectType))
				{
					BAccount acct = PXSelect<BAccount, Where<BAccount.bAccountID, Equal<Required<MergeParams.targetEntityID>>>>.Select(Base, e.Row.TargetEntityID);

					PXUIFieldAttribute.SetError<MergeParams.targetEntityID>(e.Cache, e.Row, Messages.OnlyBAccountMergeSources, acct.AcctCD);

					return;
				}

				PXUIFieldAttribute.SetError<MergeParams.targetEntityID>(e.Cache, e.Row, null);
			}

			public override void _(Events.RowSelected<Extensions.CRDuplicateEntities.Document> e)
			{
				base._(e);

				if (e.Row == null) return;

				DuplicateAttach.SetVisible(false);
			}

			public virtual void _(Events.FieldSelecting<CRDuplicateRecord, CRDuplicateRecord.selected> e)
			{
				if (e.Row == null)
					return;

				e.ReturnState = PXFieldState.CreateInstance(e.ReturnState, null, null, null, null, null, null, null, null, null, null, null, PXErrorLevel.Undefined,
					e.Row.DuplicateContactType == ContactTypesAttribute.BAccountProperty,
					null, null, PXUIVisibility.Undefined, null, null, null);
			}

			#endregion

			#region Actions

			[PXUIField(DisplayName = Messages.MarkAsValidated)]
			[PXButton]
			public override IEnumerable markAsValidated(PXAdapter adapter)
			{
				base.markAsValidated(adapter);

				foreach (PXResult<BAccount, Contact> pxresult in adapter.Get())
				{
					var account = (BAccount)pxresult;

					var defContactAddress = Base.GetExtension<DefContactAddressExt>();
					Contact defContact = defContactAddress.DefContact.View.SelectSingleBound(new object[] { account }) as Contact;

					if (defContact != null)
					{
						defContact = (Contact)defContactAddress.DefContact.Cache.CreateCopy(defContact);

						defContact.DuplicateStatus = DuplicateStatusAttribute.Validated;
						defContact.DuplicateFound = false;

						defContactAddress.DefContact.Update(defContact);

						if (Base.IsContractBasedAPI)
							Base.Save.Press();
					}
				}

				return adapter.Get();
			}

			[PXUIField(DisplayName = Messages.CloseAsDuplicate)]
			[PXButton]
			public override IEnumerable closeAsDuplicate(PXAdapter adapter)
			{
				base.closeAsDuplicate(adapter);

				foreach (PXResult<BAccount, Contact> pxresult in adapter.Get())
				{
					var account = (BAccount)pxresult;

					var defContactAddress = Base.GetExtension<DefContactAddressExt>();
					Contact defContact = defContactAddress.DefContact.View.SelectSingleBound(new object[] { account }) as Contact;

					if (defContact != null)
					{
						defContact = (Contact)defContactAddress.DefContact.Cache.CreateCopy(defContact);

						defContact.DuplicateStatus = DuplicateStatusAttribute.Duplicated;

						defContactAddress.DefContact.Update(defContact);

						if (Base.IsContractBasedAPI)
							Base.Save.Press();
					}
				}

				return adapter.Get();
			}

			[PXOverride]
			public virtual void Persist(Action del)
			{
				del();

				var doc = Documents.Current;
				if (doc == null)
					return;

				BAccount ba = Documents.Cache.GetMain(doc) as BAccount;

				if (Setup.Current?.ValidateAccountDuplicatesOnEntry == true && ba?.Status != CR.BAccount.status.Inactive)
				{
					CheckForDuplicates.Press();
				}
			}

			#endregion

			#region Overrides

			protected override BAccount GetTargetEntity(int targetID)
			{
				return PXSelect<BAccount, Where<BAccount.bAccountID, Equal<Required<BAccount.bAccountID>>>>.Select(Base, targetID);
			}

			protected override Contact GetTargetContact(BAccount targetEntity)
			{
				return PXSelect<Contact, Where<Contact.contactID, Equal<Required<BAccount.defContactID>>>>.Select(Base, targetEntity.DefContactID);
			}

			protected override Address GetTargetAddress(BAccount targetEntity)
			{
				return PXSelect<Address, Where<Address.addressID, Equal<Required<BAccount.defAddressID>>>>.Select(Base, targetEntity.DefAddressID);
			}

			protected override void MergeEntities(PXGraph targetGraph, BAccount targetEntity, BAccount duplicateAccount)
			{
				int? defContactID = duplicateAccount.DefContactID;
				PXCache Contacts = targetGraph.Caches[typeof(Contact)];
				foreach (Contact contact in PXSelect<
							Contact, 
						Where<
							Contact.bAccountID, Equal<Required<BAccount.bAccountID>>>>
					.Select(targetGraph, duplicateAccount.BAccountID)
					.RowCast<Contact>()
					.Where(c => c.ContactID != defContactID)
					.Select(c => (Contact)Contacts.CreateCopy(c)))
				{
					contact.BAccountID = targetEntity.BAccountID;

					Contacts.Update(contact);
				}

				PXCache Activities = targetGraph.Caches[typeof(CRPMTimeActivity)];
				foreach (CRPMTimeActivity activity in PXSelect<
							CRPMTimeActivity, 
						Where<
							CRPMTimeActivity.bAccountID, Equal<Required<BAccount.bAccountID>>>>
					.Select(targetGraph, duplicateAccount.BAccountID)
					.RowCast<CRPMTimeActivity>()
					.Select(cas => (CRPMTimeActivity)Activities.CreateCopy(cas)))
				{
					if (activity.BAccountID == duplicateAccount.BAccountID)
					{
						activity.BAccountID = targetEntity.BAccountID;
					}
					activity.BAccountID = targetEntity.BAccountID;

					Activities.Update(activity);
				}

				PXCache Cases = targetGraph.Caches[typeof(CRCase)];
				foreach (CRCase cas in PXSelect<
							CRCase, 
						Where<
							CRCase.customerID, Equal<Required<BAccount.bAccountID>>>>
					.Select(targetGraph, duplicateAccount.BAccountID)
					.RowCast<CRCase>()
					.Select(cas => (CRCase)Cases.CreateCopy(cas)))
				{
					cas.CustomerID = targetEntity.BAccountID;

					Cases.Update(cas);
				}

				PXCache Opportunities = targetGraph.Caches[typeof(CROpportunity)];
				foreach (CROpportunity opp in PXSelect<
							CROpportunity, 
						Where<
							CROpportunity.bAccountID, Equal<Required<BAccount.bAccountID>>>>
					.Select(targetGraph, duplicateAccount.BAccountID)
					.RowCast<CROpportunity>()
					.Select(opp => (CROpportunity)Opportunities.CreateCopy(opp)))
				{
					opp.BAccountID = targetEntity.BAccountID;
					opp.LocationID = targetEntity.DefLocationID;

					Opportunities.Update(opp);
				}

				PXCache Relations = targetGraph.Caches[typeof(CRRelation)];
				foreach (CRRelation rel in PXSelectJoin<CRRelation,
					LeftJoin<CRRelation2,
						On<CRRelation.entityID, Equal<CRRelation2.entityID>,
						And<CRRelation.role, Equal<CRRelation2.role>,
						And<CRRelation2.refNoteID, Equal<Required<BAccount.noteID>>>>>>,
					Where<CRRelation2.entityID, IsNull,
						And<CRRelation.refNoteID, Equal<Required<BAccount.noteID>>>>>
					.Select(targetGraph, targetEntity.NoteID, duplicateAccount.NoteID)
					.RowCast<CRRelation>()
					.Select(rel => (CRRelation)Relations.CreateCopy(rel)))
				{
					rel.RelationID = null;
					rel.RefNoteID = targetEntity.NoteID;

					Relations.Insert(rel);
				}

				PXCache Leads = targetGraph.Caches[typeof(CRLead)];
				foreach (CRLead lead in PXSelect<
							CRLead,
						Where<
							CRLead.bAccountID, Equal<Required<BAccount.bAccountID>>>>
					.Select(targetGraph, duplicateAccount.BAccountID)
					.RowCast<CRLead>()
					.Select(lead => (CRLead)Leads.CreateCopy(lead)))
				{
					// do it silently to not trigger other events
					Leads.SetValue<CRLead.bAccountID>(lead, targetEntity.BAccountID);
				}
			}

			protected override bool CheckIsActive()
			{
				BAccount account = Base.BAccount.Current;

				if (account == null)
					return false;

				return account.Status != CR.BAccount.status.Inactive;
			}

			public override void GetAllProperties(List<FieldValue> values, HashSet<string> fieldNames)
			{
				int order = 0;

				values.AddRange(GetMarkedPropertiesOf<BAccount>(Base, ref order).Where(fld => fieldNames.Add(fld.Name)));
				values.AddRange(GetMarkedPropertiesOf<Contact>(Base, ref order).Where(fld => fieldNames.Add(fld.Name)));

				base.GetAllProperties(values, fieldNames);
			}

			protected override void DoDuplicateAttach(DuplicateDocument duplicateDocument)
			{
				return;
			}

			#endregion
		}

		/// <exclude/>
		// Acuminator disable once PX1016 ExtensionDoesNotDeclareIsActiveMethod extension should be constantly active
		public class BAccountLocationSharedContactOverrideGraphExt : SharedChildOverrideGraphExt<BusinessAccountMaint, BAccountLocationSharedContactOverrideGraphExt>
		{
			#region Initialization 

			public override bool ViewHasADelegate => true;

			protected override DocumentMapping GetDocumentMapping()
			{
				return new DocumentMapping(typeof(CRLocation))
				{
					RelatedID = typeof(CRLocation.bAccountID),
					ChildID = typeof(CRLocation.defContactID),
					IsOverrideRelated = typeof(CRLocation.overrideContact)
				};
			}

			protected override RelatedMapping GetRelatedMapping()
			{
				return new RelatedMapping(typeof(BAccount))
				{
					RelatedID = typeof(BAccount.bAccountID),
					ChildID = typeof(BAccount.defContactID)
				};
			}

			protected override ChildMapping GetChildMapping()
			{
				return new ChildMapping(typeof(Contact))
				{
					ChildID = typeof(Contact.contactID),
					RelatedID = typeof(Contact.bAccountID),
				};
			}

			#endregion
		}

		/// <exclude/>
		// Acuminator disable once PX1016 ExtensionDoesNotDeclareIsActiveMethod extension should be constantly active
		public class BAccountLocationSharedAddressOverrideGraphExt : SharedChildOverrideGraphExt<BusinessAccountMaint, BAccountLocationSharedAddressOverrideGraphExt>
		{
			#region Initialization 

			public override bool ViewHasADelegate => true;

			protected override DocumentMapping GetDocumentMapping()
			{
				return new DocumentMapping(typeof(CRLocation))
				{
					RelatedID = typeof(CRLocation.bAccountID),
					ChildID = typeof(CRLocation.defAddressID),
					IsOverrideRelated = typeof(CRLocation.overrideAddress)
				};
			}

			protected override RelatedMapping GetRelatedMapping()
			{
				return new RelatedMapping(typeof(BAccount))
				{
					RelatedID = typeof(BAccount.bAccountID),
					ChildID = typeof(BAccount.defAddressID)
				};
			}

			protected override ChildMapping GetChildMapping()
			{
				return new ChildMapping(typeof(Address))
				{
					ChildID = typeof(Address.addressID),
					RelatedID = typeof(Address.bAccountID),
				};
			}

			#endregion
		}

		/// <exclude/>
		// Acuminator disable once PX1016 ExtensionDoesNotDeclareIsActiveMethod extension should be constantly active
		public class CreateLeadFromAccountGraphExt : CRCreateLeadAction<BusinessAccountMaint, BAccount>
		{
			#region Initialization

			public override void Initialize()
			{
				base.Initialize();

				Addresses = new PXSelectExtension<DocumentAddress>(Base.GetExtension<DefContactAddressExt>().DefAddress);
				Contacts = new PXSelectExtension<DocumentContact>(Base.GetExtension<DefContactAddressExt>().DefContact);
			}

			protected override DocumentContactMapping GetDocumentContactMapping()
			{
				return new DocumentContactMapping(typeof(Contact)) { Email = typeof(Contact.eMail) };
			}
			protected override DocumentAddressMapping GetDocumentAddressMapping()
			{
				return new DocumentAddressMapping(typeof(Address));
			}

			#endregion
		}

		/// <exclude/>
		// Acuminator disable once PX1016 ExtensionDoesNotDeclareIsActiveMethod extension should be constantly active
		public class CreateContactFromAccountGraphExt : CRCreateContactActionBase<BusinessAccountMaint, BAccount>
		{
			#region Initialization

			protected override PXSelectBase<CRPMTimeActivity> Activities => Base.Activities;

			public override void Initialize()
			{
				base.Initialize();

				Addresses = new PXSelectExtension<DocumentAddress>(Base.GetExtension<DefContactAddressExt>().DefAddress);
				Contacts = new PXSelectExtension<DocumentContact>(Base.GetExtension<DefContactAddressExt>().DefContact);
			}

			protected override DocumentContactMapping GetDocumentContactMapping()
			{
				return new DocumentContactMapping(typeof(Contact)) { Email = typeof(Contact.eMail) };
			}

			protected override DocumentAddressMapping GetDocumentAddressMapping()
			{
				return new DocumentAddressMapping(typeof(Address));
			}

			#endregion

			#region Events

			public virtual void _(Events.RowSelected<ContactFilter> e)
			{
				PXUIFieldAttribute.SetReadOnly<ContactFilter.fullName>(e.Cache, e.Row, true);
			}

			public virtual void _(Events.FieldDefaulting<ContactFilter, ContactFilter.fullName> e)
			{
				e.NewValue = Contacts.SelectSingle()?.FullName;
			}

			#endregion

			#region Overrides

			protected override void FillRelations<TNoteField>(CRRelationsList<TNoteField> relations, Contact target)
			{
			}

			protected override IConsentable MapConsentable(DocumentContact source, IConsentable target)
			{
				return target;
			}

			#endregion
		}

		/// <exclude/>
		// Acuminator disable once PX1016 ExtensionDoesNotDeclareIsActiveMethod extension should be constantly active
		public class UpdateRelatedContactInfoFromAccountGraphExt : CRUpdateRelatedContactInfoGraphExt<BusinessAccountMaint>
		{
			#region Events 

			protected virtual void _(Events.RowPersisted<Contact> e)
			{
				var row = e.Row;
				if (row == null || e.TranStatus != PXTranStatus.Open || e.Operation != PXDBOperation.Update || !ContactWasUpdated(e.Cache, row))
					return;

				BAccount account = Base.BAccount.Current ?? PXSelect<
						BAccount,
						Where<
							BAccount.bAccountID.IsEqual<@P.AsInt>
							.And<BAccount.defContactID.IsEqual<@P.AsInt>>>>
					.Select(Base, row.BAccountID, row.ContactID);

				if (account == null)
					return;

				PXUpdateJoin<
					Set<Contact.firstName, Required<Contact.firstName>,
					Set<Contact.lastName, Required<Contact.lastName>,
					Set<Contact.fullName, Required<Contact.fullName>,
					Set<Contact.salutation, Required<Contact.salutation>,
					Set<Contact.eMail, Required<Contact.eMail>,
					Set<Contact.webSite, Required<Contact.webSite>,
					Set<Contact.phone1Type, Required<Contact.phone1Type>,
					Set<Contact.phone1, Required<Contact.phone1>,
					Set<Contact.phone2Type, Required<Contact.phone2Type>,
					Set<Contact.phone2, Required<Contact.phone2>,
					Set<Contact.phone3Type, Required<Contact.phone3Type>,
					Set<Contact.phone3, Required<Contact.phone3>,
					Set<Contact.faxType, Required<Contact.faxType>,
					Set<Contact.fax, Required<Contact.fax>,
					Set<Contact.grammValidationDateTime, Required<Contact.grammValidationDateTime>,
					Set<Contact.consentAgreement, Required<Contact.consentAgreement>,
					Set<Contact.consentDate, Required<Contact.consentDate>,
					Set<Contact.consentExpirationDate, Required<Contact.consentExpirationDate>
					>>>>>>>>>>>>>>>>>>,
				Contact,
				LeftJoin<Standalone.CRLead,
					On<Standalone.CRLead.contactID.IsEqual<Contact.contactID>>>,
				Where<
					// Leads that are linked to the same Account
					Contact.bAccountID.IsEqual<@P.AsInt>
					.And<Standalone.CRLead.overrideRefContact.IsEqual<False>>>>
				.Update(Base,
					//Set
					row.FirstName,
					row.LastName,
					row.FullName,
					row.Salutation,
					row.EMail,
					row.WebSite,
					row.Phone1Type,
					row.Phone1,
					row.Phone2Type,
					row.Phone2,
					row.Phone3Type,
					row.Phone3,
					row.FaxType,
					row.Fax,
					new DateTime(1900, 1, 1),
					row.ConsentAgreement,
					row.ConsentDate,
					row.ConsentExpirationDate,

					// Where
					account.BAccountID);
			}

			protected virtual void _(Events.RowPersisted<Address> e)
			{
				var row = e.Row;
				if (row == null || e.TranStatus != PXTranStatus.Open || e.Operation != PXDBOperation.Update || !AddressWasUpdated(e.Cache, row))
					return;

				BAccount account = Base.BAccount.Current ?? PXSelect<
						BAccount,
						Where<
							BAccount.bAccountID.IsEqual<@P.AsInt>
							.And<BAccount.defAddressID.IsEqual<@P.AsInt>>>>
					.Select(Base, row.BAccountID, row.AddressID);

				if (account == null || row.AddressID != account.DefAddressID)
					return;

				PXUpdateJoin<
					Set<Address.addressLine1, Required<Address.addressLine1>,
					Set<Address.addressLine2, Required<Address.addressLine2>,
					Set<Address.city, Required<Address.city>,
					Set<Address.state, Required<Address.state>,
					Set<Address.postalCode, Required<Address.postalCode>,
					Set<Address.countryID, Required<Address.countryID>>>>>>>,
				Address,
				InnerJoin<Contact,
					On<Contact.defAddressID.IsEqual<Address.addressID>>,
				LeftJoin<Standalone.CRLead,
					On<Standalone.CRLead.contactID.IsEqual<Contact.contactID>>>>,
				Where<
					// Leads that are linked to the same Account
					Contact.bAccountID.IsEqual<@P.AsInt>
					.And<Standalone.CRLead.overrideRefContact.IsEqual<False>>>>
				.Update(Base,
					//Set
					row.AddressLine1,
					row.AddressLine2,
					row.City,
					row.State,
					row.PostalCode,
					row.CountryID,

					// Where
					account.BAccountID);
			}

			#endregion
		}

		/// <exclude/>
		// Acuminator disable once PX1016 ExtensionDoesNotDeclareIsActiveMethod extension should be constantly active
		public class LastNameOrCompanyNameRequiredGraphExt : PXGraphExtension<BusinessAccountMaint>
		{
			#region Overrides

			[PXRemoveBaseAttribute(typeof(PXUIRequiredAttribute))]
			protected virtual void _(Events.CacheAttached<CRLead.displayName> e) { }

			[PXRemoveBaseAttribute(typeof(CRLastNameDefaultAttribute))]
			protected virtual void _(Events.CacheAttached<CRLead.lastName> e) { }

			protected virtual void _(Events.RowPersisting<CRLead> e)
			{
				var row = e.Row;
				if (row == null) return;

				if (row.LastName == null && row.FullName == null)
					throw new PXSetPropertyException(Messages.LastNameOrFullNameReqired);
			}

			#endregion
		}

		/// <exclude/>
		public class ExtensionSort
			: SortExtensionsBy<ExtensionOrderFor<BusinessAccountMaint>
				.FilledWith<
					DefContactAddressExt,
					CreateLeadFromAccountGraphExt,
					CreateContactFromAccountGraphExt
				>>
		{ }

		#endregion
	}
}
