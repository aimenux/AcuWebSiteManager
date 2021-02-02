using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using PX.Common;
using PX.Data;
using PX.Objects.AR;
using PX.Objects.CR.MassProcess;
using PX.Objects.CR.Extensions.CRCreateActions;
using PX.Objects.GL;
using PX.Objects.CS;
using PX.Objects.EP;
using PX.SM;
using PX.EP;
using PX.Objects.Extensions.ContactAddress;
using PX.Data.BQL;
using PX.Data.MassProcess;
using PX.Objects.CR.Extensions;
using PX.Objects.CR.Extensions.CRDuplicateEntities;
using PX.Objects.CR.Extensions.CRContactAccountDataSync;
using PX.Data.BQL.Fluent;

namespace PX.Objects.CR
{
	public class ContactMaint : PXGraph<ContactMaint, Contact, Contact.displayName>
	{
		#region Inner Types
		[Serializable]
        [PXHidden]
		public class CurrentUser : Users
		{
			public new abstract class pKID : PX.Data.BQL.BqlGuid.Field<pKID> { }
			public new abstract class guest : PX.Data.BQL.BqlBool.Field<guest> { }
		}
		#endregion

		#region Selects

		//TODO: need review
		[PXHidden]
		public PXSelect<BAccount>
			bAccountBasic;

		[PXHidden]
		public PXSetup<Company>
			company;

		[PXHidden]
        public IN.PXSetupOptional<CRSetup>
			Setup;

		[PXViewName(Messages.Contact)]
		[PXCopyPasteHiddenFields(typeof(Contact.duplicateStatus), typeof(Contact.duplicateFound))]
		public SelectContactEmailSync<Where<Contact.contactType, Equal<ContactTypesAttribute.person>, Or<Contact.contactType, Equal<ContactTypesAttribute.employee>>>>
			Contact;

		public PXSelect<Contact,
				Where<Contact.contactID, Equal<Current<Contact.contactID>>>>
			ContactCurrent;
		
		public PXSelect<Contact,
				Where<Contact.contactID, Equal<Current<Contact.contactID>>>>
			ContactCurrent2;

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
				CRLead.refContactID, Equal<Current<Contact.contactID>>>,
			OrderBy<
				Desc<CRLead.createdDateTime>>>
			Leads;

		[PXCopyPasteHiddenView]
		public PXSelect<CRActivityStatistics,
				Where<CRActivityStatistics.noteID, Equal<Current<Contact.noteID>>>>
			ContactActivityStatistics;

		[PXViewName(Messages.Address)]
		public AddressSelect<Contact.defAddressID, Contact.isAddressSameAsMain, Contact.bAccountID>
			AddressCurrent;
	    [PXCopyPasteHiddenView()]
		public PXSelectUsers<Contact, Where<Users.pKID, Equal<Current<Contact.userID>>>> User;
        [PXCopyPasteHiddenView()]
		public PXSelectUsersInRoles UserRoles;
        [PXCopyPasteHiddenView()]
		public PXSelectAllowedRoles Roles;

		[PXViewName(Messages.Answers)]
		public CRAttributeList<Contact>
			Answers;

		[PXViewName(Messages.Activities)]
		[PXFilterable]
		[CRDefaultMailTo]
		[CRReference(typeof(Contact.bAccountID),typeof(Contact.contactID))]
		public CRActivityList<Contact>
			Activities;
		
		public PXSelectJoin<EMailSyncAccount,
			InnerJoin<BAccount,
				On<BAccount.bAccountID, Equal<EMailSyncAccount.employeeID>>>,
			Where<BAccount.defContactID, Equal<Optional<Contact.contactID>>>> SyncAccount;

		public PXSelect<EMailAccount,
			Where<EMailAccount.emailAccountID, Equal<Optional<EMailSyncAccount.emailAccountID>>>> EMailAccounts;

		[PXCopyPasteHiddenView]
		[PXViewName(Messages.Relations)]
		[PXFilterable]
		public CRRelationsList<Contact.noteID>
			Relations;

		[PXHidden]
		public PXSelect<CROpportunityClass>
			CROpportunityClass;

		[PXViewName(Messages.Opportunities)]
		[PXFilterable]
		[PXViewDetailsButton(typeof(Contact))]
		public PXSelectReadonly2<CROpportunity,
			LeftJoin<CROpportunityProbability, On<CROpportunityProbability.stageCode, Equal<CROpportunity.stageID>>,
			LeftJoin<CROpportunityClass, On<CROpportunityClass.cROpportunityClassID, Equal<CROpportunity.classID>>>>,
			Where<CROpportunity.contactID, Equal<Current<Contact.contactID>>>>
			Opportunities;

		[PXFilterable]
		[PXViewDetailsButton(typeof(Contact))]
		public PXSelectReadonly<CRCase,
			Where<CRCase.contactID, Equal<Current<Contact.contactID>>,
            And<Where<Current<Contact.bAccountID>, IsNull,
                   Or<CRCase.customerID, Equal<Current<Contact.bAccountID>>>>>>>
			Cases;

        [PXCopyPasteHiddenView]
		[PXViewName(Messages.CampaignMember)]
		[PXFilterable]
		[PXViewDetailsButton(typeof(Contact), 
			typeof(Select<CRCampaign, 
				Where<CRCampaign.campaignID, Equal<Current<CRCampaignMembers.campaignID>>>>))]
		public PXSelectJoin<CRCampaignMembers,
			InnerJoin<CRCampaign, On<CRCampaignMembers.campaignID, Equal<CRCampaign.campaignID>>>,
			Where<CRCampaignMembers.contactID, Equal<Current<Contact.contactID>>>>
			Members;

		[PXHidden]
		public PXSelect<CRMarketingListMember>
			Subscriptions_stub;

		[PXCopyPasteHiddenView]
		[PXViewName(Messages.Subscriptions)]
		[PXFilterable]
		[PXViewDetailsButton(typeof(Contact),
			typeof(Select<CRMarketingList,
				Where<CRMarketingList.marketingListID, Equal<Current<CRMarketingListMember.marketingListID>>>>))]
		public CRMMarketingContactSubscriptions<Contact, Contact.contactID>
			Subscriptions;
		
		[PXViewName(Messages.Notifications)]
		public PXSelectJoin<ContactNotification,
			InnerJoin<NotificationSetup,
				On<NotificationSetup.setupID, Equal<ContactNotification.setupID>>>,
			Where<ContactNotification.contactID, Equal<Optional<Contact.contactID>>>>
			NWatchers;

		#endregion

		#region Ctors

		public ContactMaint()
		{
			PXUIFieldAttribute.SetRequired<Contact.lastName>(Contact.Cache, true);

		    // HACK graph can contain separate caches for BAccount and BAccountR, so force display names for BAccount cache
		    PXUIFieldAttribute.SetDisplayName<BAccount.acctCD>(Caches[typeof(BAccount)], Messages.BAccountCD);
		    PXUIFieldAttribute.SetDisplayName<BAccount.acctName>(Caches[typeof(BAccount)], Messages.BAccountName);

			PXUIFieldAttribute.SetDisplayName<BAccountR.acctCD>(Caches[typeof(BAccountR)], Messages.BAccountCD);
			PXUIFieldAttribute.SetDisplayName<BAccountR.acctName>(Caches[typeof(BAccountR)], Messages.BAccountName);

            Activities.GetNewEmailAddress =
				() =>
				{
					var contact = Contact.Current;
					return contact != null && !string.IsNullOrWhiteSpace(contact.EMail)
						? PXDBEmailAttribute.FormatAddressesWithSingleDisplayName(contact.EMail, contact.DisplayName)
						: String.Empty;
				};

			PXUIFieldAttribute.SetEnabled<EPLoginTypeAllowsRole.rolename>(Roles.Cache, null, false);
			Roles.Cache.AllowInsert = false;
			Roles.Cache.AllowDelete = false;
		    PXUIFieldAttribute.SetVisible<CRMarketingListMember.format>(Subscriptions.Cache, null, false);

			PXUIFieldAttribute.SetVisible<Contact.languageID>(ContactCurrent.Cache, null, PXDBLocalizableStringAttribute.HasMultipleLocales);
		}

	    public override void InitCacheMapping(Dictionary<Type, Type> map)
	    {
	        base.InitCacheMapping(map);
            Caches.AddCacheMappingsWithInheritance(this, typeof(BAccount));
	    }

	    #endregion

		#region Actions

		public PXMenuAction<Contact> Action;

		public PXDBAction<Contact> addOpportunity;
        [PXUIField(DisplayName = Messages.AddNewOpportunity, FieldClass = FeaturesSet.customerModule.FieldClass)]
		[PXButton(ImageKey = PX.Web.UI.Sprite.Main.AddNew)]
		public virtual void AddOpportunity()
		{
			var row = ContactCurrent.Current;
			if (row == null || row.ContactID == null) return;
		
			var graph = PXGraph.CreateInstance<OpportunityMaint>();
			var newOpportunity = graph.Opportunity.Insert();
            newOpportunity.BAccountID = row.BAccountID;

            newOpportunity.Source = row.Source;

			CRContactClass cls = PXSelect<
					CRContactClass,
				Where<
					CRContactClass.classID, Equal<Required<Contact.classID>>>>
				.Select(this, row.ClassID);

			if (cls?.TargetOpportunityClassID != null)
			{
				newOpportunity.ClassID = cls.TargetOpportunityClassID;
			}
			else
			{
				newOpportunity.ClassID = this.Setup.Current?.DefaultOpportunityClassID;
			}

			CROpportunityClass ocls = PXSelect<CROpportunityClass, Where<CROpportunityClass.cROpportunityClassID, Equal<Current<CROpportunity.classID>>>>
				.SelectSingleBound(this, new object[] { newOpportunity });
			if (ocls?.DefaultOwner == CRDefaultOwnerAttribute.Source)
			{
				newOpportunity.WorkgroupID = row.WorkgroupID;
				newOpportunity.OwnerID = row.OwnerID;
			}

			newOpportunity.ContactID = row.ContactID;
			graph.Opportunity.Update(newOpportunity);

			if (!this.IsContractBasedAPI)
				PXRedirectHelper.TryRedirect(graph, PXRedirectHelper.WindowMode.NewWindow);

			graph.Save.Press();
		}

		public PXDBAction<Contact> addCase;
        [PXUIField(DisplayName = Messages.AddNewCase, FieldClass = FeaturesSet.customerModule.FieldClass)]
		[PXButton(ImageKey = PX.Web.UI.Sprite.Main.AddNew)]
		public virtual void AddCase()
		{
		    var row = ContactCurrent.Current;
		    if (row == null || row.ContactID == null) return;

			var graph = PXGraph.CreateInstance<CRCaseMaint>();
			var newCase = (CRCase)graph.Case.Cache.CreateInstance();
            newCase = PXCache < CRCase >.CreateCopy(graph.Case.Insert(newCase));
			newCase.CustomerID = row.BAccountID;
			newCase.ContactID = row.ContactID;
            try
            {
                graph.Case.Update(newCase);
            }
            catch{}
			
			if (!this.IsContractBasedAPI)
				PXRedirectHelper.TryRedirect(graph, PXRedirectHelper.WindowMode.NewWindow);

			graph.Save.Press();
		}

		public PXAction<Contact> copyBAccountContactInfo;
		[PXButton(ImageKey = PX.Web.UI.Sprite.Main.ArrowDown, Tooltip = Messages.CopyFromCompany)]
		[PXUIField(DisplayName = Messages.CopyFromCompany)]
		public virtual void CopyBAccountContactInfo()
		{
			var row = ContactCurrent.Current as Contact;
			if (row == null || row.BAccountID == null) return;

			var acct = (BAccount)PXSelect<BAccount,
				Where<BAccount.bAccountID, Equal<Required<BAccount.bAccountID>>>>.
				Select(this, row.BAccountID);
			if (acct != null && acct.DefContactID != null)
			{
				var defContact = (Contact)PXSelect<Contact,
					Where<Contact.contactID, Equal<Required<Contact.contactID>>>>.
					Select(this, acct.DefContactID);
				if (defContact != null)
					CopyContactInfo(row, defContact);
				ContactCurrent.Update(row);
			}

			if (this.IsContractBasedAPI)
				this.Save.Press();
		}

        #endregion

        public PXAction<Contact> deleteMarketingList;
        [PXUIField(DisplayName = Messages.Delete)]
        [PXButton(ImageKey = PX.Web.UI.Sprite.Main.Remove)]
        public virtual void DeleteMarketingList()
        {
            CRMarketingListMember marketingListMember = Subscriptions.Cache.Current as CRMarketingListMember;
            if (marketingListMember == null) return;
            CRMarketingList marketingList = PXSelect<CRMarketingList, Where<CRMarketingList.marketingListID,
                Equal<Required<CRMarketingList.marketingListID>>>>.Select(this, marketingListMember.MarketingListID);

            if (marketingList == null) return;

            if (marketingList.IsDynamic == true)
                return;

                Subscriptions.Cache.Delete(marketingListMember);              
        }

        #region Event Handlers

        #region Contact

		[PXUIField(DisplayName = "Contact ID")]
        [ContactSelector(true, typeof(ContactTypesAttribute.person), typeof(ContactTypesAttribute.employee))]
		[PXMergeAttributes(Method = MergeMethod.Merge)]
		protected virtual void _(Events.CacheAttached<Contact.contactID> e) { }

		[PXUIField(DisplayName = "Contact Class")]
		[PXDefault(typeof(Search<CRSetup.defaultContactClassID>), PersistingCheck = PXPersistingCheck.Nothing)]
		[PXMergeAttributes(Method = MergeMethod.Merge)]
		protected virtual void _(Events.CacheAttached<Contact.classID> e) { }

		[ContactSynchronize]
		[PXMergeAttributes(Method = MergeMethod.Merge)]
		protected virtual void _(Events.CacheAttached<Contact.synchronize> e) { }

		[CRMBAccount(Visibility = PXUIVisibility.SelectorVisible)]
		[PXRestrictor(typeof(Where<Current<Contact.contactType>, NotEqual<ContactTypesAttribute.employee>, 
			Or<BAccount.type, Equal<BAccountType.branchType>,
			Or<BAccount.type, Equal<BAccountType.organizationType>, 
			Or<BAccount.type, Equal<BAccountType.organizationBranchCombinedType>>>>>), Messages.BAccountIsType, typeof(BAccount.type))]
		[PXRestrictor(typeof(Where<Current<Contact.contactType>, NotEqual<ContactTypesAttribute.person>, 
			Or<BAccount.type, Equal<BAccountType.prospectType>,
			Or<BAccount.type, Equal<BAccountType.customerType>,
			Or<BAccount.type, Equal<BAccountType.vendorType>,
			Or<BAccount.type, Equal<BAccountType.combinedType>>>>>>), Messages.BAccountIsType, typeof(BAccount.type))]
		[PopupMessage]
		[PXMergeAttributes(Method = MergeMethod.Replace)]
		protected virtual void _(Events.CacheAttached<Contact.bAccountID> e) { }

		[PXUIField(DisplayName = "Company Name", Visibility = PXUIVisibility.SelectorVisible)]
		[CRLeadFullName(typeof(Contact.bAccountID))]
		[PXMassMergableField]
		[PXPersonalDataField]
		[PXMergeAttributes(Method = MergeMethod.Replace)]
		protected virtual void _(Events.CacheAttached<Contact.fullName> e) { }

		[PXFormula(typeof(Selector<Contact.bAccountID, BAccount.parentBAccountID>))]
		[PXMergeAttributes(Method = MergeMethod.Merge)]
		protected virtual void _(Events.CacheAttached<Contact.parentBAccountID> e) { }

		[PXDBGuid(IsKey = true)]
		[PXDefault]
		[PXUIField(Visibility = PXUIVisibility.Invisible)]
		[PXParent(typeof(Select<Contact, Where<Contact.userID, Equal<Current<Users.pKID>>>>))]
		[PXMergeAttributes(Method = MergeMethod.Replace)]
		public virtual void Users_PKID_CacheAttached(PXCache sender){}

		[PXDBString(64, IsUnicode = true, InputMask = "" /*"AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA||.'@-_"*/)]
		[PXUIField(DisplayName = "Login")]
		[PXUIRequired(typeof(Where<Users.loginTypeID, IsNotNull, And<EntryStatus, Equal<EntryStatus.inserted>>>))]
		[PXMergeAttributes(Method = MergeMethod.Merge)]
		public virtual void Users_Username_CacheAttached(PXCache sender) { }

		[PXMergeAttributes(Method = MergeMethod.Append)]
		[PXSelector(typeof(Search2<Contact.contactID,
				LeftJoin<Users, On<Contact.userID, Equal<Users.pKID>>,
				LeftJoin<BAccount, On<BAccount.defContactID, Equal<Contact.contactID>>>>,
					Where<Current<Users.guest>, Equal<True>, And<Contact.contactType, Equal<ContactTypesAttribute.person>,
						Or<Current<Users.guest>, NotEqual<True>, And<Contact.contactType, Equal<ContactTypesAttribute.employee>, And<BAccount.bAccountID, IsNotNull>>>>>>),
			typeof(Contact.displayName),
			typeof(Contact.salutation),
			typeof(Contact.fullName),
			typeof(Contact.eMail),
			typeof(Users.username),
			DescriptionField = typeof(Contact.displayName))]
		[PXRestrictor(typeof(Where<Contact.userID, IsNull, Or<Contact.userID, Equal<Current<Users.pKID>>>>), PX.Objects.CR.Messages.ContactWithUser, typeof(Contact.displayName))]
		[PXDBScalar(typeof(Search<Contact.contactID, Where<Contact.userID, Equal<Users.pKID>>>))]
		protected virtual void Users_ContactID_CacheAttached(PXCache sender)
		{
		}

		//DONE: need to duplicate in User Maint
		[PXDBInt]
		[PXUIField(DisplayName = "User Type")]
		[PXRestrictor(typeof(Where<EPLoginType.entity, Equal<EPLoginType.entity.contact>>), Messages.NonContactLoginType, typeof(EPLoginType.loginTypeName))]
		[PXSelector(typeof(Search5<EPLoginType.loginTypeID, LeftJoin<EPManagedLoginType, On<EPLoginType.loginTypeID, Equal<EPManagedLoginType.loginTypeID>>,
								LeftJoin<Users, On<EPManagedLoginType.parentLoginTypeID, Equal<Users.loginTypeID>>,
								LeftJoin<CurrentUser, On<CurrentUser.pKID, Equal<Current<AccessInfo.userID>>>>>>,
								Where<Users.pKID, Equal<CurrentUser.pKID>, And<CurrentUser.guest, Equal<True>,
									Or<CurrentUser.guest, NotEqual<True>>>>, 
								Aggregate<GroupBy<EPLoginType.loginTypeID, GroupBy<EPLoginType.loginTypeName, GroupBy<EPLoginType.requireLoginActivation, GroupBy<EPLoginType.resetPasswordOnLogin>>>>>>), 
			SubstituteKey = typeof(EPLoginType.loginTypeName))]
		[PXDefault(PersistingCheck = PXPersistingCheck.Nothing)]
		protected virtual void Users_LoginTypeID_CacheAttached(PXCache sender) { }

		[PXUIField(DisplayName = "Guest Account")]
		[PXFormula(typeof(Switch<Case<Where<Selector<Users.loginTypeID, EPLoginType.entity>, Equal<EPLoginType.entity.contact>>, True>, False>))]
		[PXMergeAttributes(Method = MergeMethod.Merge)]
		protected virtual void Users_Guest_CacheAttached(PXCache sender) { }

		[PXFormula(typeof(Selector<Users.loginTypeID, EPLoginType.requireLoginActivation>))]
		[PXMergeAttributes(Method = MergeMethod.Merge)]
		protected virtual void Users_IsPendingActivation_CacheAttached(PXCache sender) { }

		[PXFormula(typeof(Switch<Case<Where<Selector<Users.loginTypeID, EPLoginType.resetPasswordOnLogin>, Equal<True>>, True>, False>))]
		[PXMergeAttributes(Method = MergeMethod.Merge)]
		protected virtual void Users_PasswordChangeOnNextLogin_CacheAttached(PXCache sender) { }

		[PXDefault(true, PersistingCheck = PXPersistingCheck.Nothing)]
		[PXMergeAttributes(Method = MergeMethod.Merge)]
		protected virtual void Users_GeneratePassword_CacheAttached(PXCache sender) { }

		[PXDBString(64, IsKey = true, IsUnicode = true, InputMask = "")]
		[PXDefault(typeof(Users.username))]
		[PXParent(typeof(Select<Users, Where<Users.username, Equal<Current<UsersInRoles.username>>>>))]
		[PXMergeAttributes(Method = MergeMethod.Replace)]	// delete PXSelector from DAC
		protected virtual void UsersInRoles_Username_CacheAttached(PXCache sender) { }

		public override void Persist()
		{
			if (Subscriptions?.Cache?.Inserted != null && Subscriptions.Cache.Inserted.Any_())
			{
				foreach (CRMarketingListMember insserted in Subscriptions.Cache.Inserted)
				{
					if (insserted.MarketingListID == 0 && insserted.CreatedByScreenID == "SM206036")
					{
						Subscriptions.Cache.SetStatus(insserted, PXEntryStatus.InsertedDeleted);
					}
				}
			}

			if (User.Current != null && User.Cache.GetStatus(User.Current) == PXEntryStatus.Inserted)
			{
				Users copy = PXCache<Users>.CreateCopy(User.Current);

				copy.OldPassword = User.Current.Password;
				copy.NewPassword = User.Current.Password;
				copy.ConfirmPassword = User.Current.Password;

				copy.FirstName = Contact.Current.FirstName;
				copy.LastName = Contact.Current.LastName;
				copy.Email = Contact.Current.EMail;

				copy.IsAssigned = true;

				User.Update(copy);
			}

			base.Persist();

			if (User.Current != null && User.Current.ContactID == null && Contact.Current != null) // for correct redirection to user after inserting
			{
				User.Current.ContactID = Contact.Current.ContactID;
			}
		}		

		protected virtual void Users_State_FieldSelecting(PXCache sender, PXFieldSelectingEventArgs e)
		{
			if (e.ReturnValue == null && (e.Row == null || sender.GetStatus(e.Row) == PXEntryStatus.Inserted))
			{
				e.ReturnValue = Users.state.NotCreated;
			}
		}

		protected virtual void Users_LoginTypeID_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
		{
			UserRoles.Cache.Clear();
			if (((Users) e.Row).LoginTypeID == null)
			{
				User.Cache.Clear();
				Contact.Current.UserID = null;
			}
		}
		
		protected virtual void Users_Username_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
		{
			Users user = (Users)e.Row;

			if (e.OldValue != null && user.Username != null && e.OldValue.ToString() != user.Username)
				User.Cache.RaiseExceptionHandling<Users.username>(User.Current, User.Current.Username, new PXSetPropertyException(Messages.LoginChangedError));
		}

        protected virtual void Users_Username_FieldVerifying(PXCache sender, PXFieldVerifyingEventArgs e)
        {
            Guid? restoredGuid = Access.GetGuidFromDeletedUser((string)e.NewValue);
            if (restoredGuid != null)
            {
                ((Users)e.Row).PKID = restoredGuid;
            }
        }
        protected virtual void Users_RowUpdated(PXCache sender, PXRowUpdatedEventArgs e)
        {
            Users user = (Users)e.Row;
            if (user == null || Contact.Current == null || ((Users)e.Row).LoginTypeID == null) return;				

			if (Contact.Current == null)
				Contact.Current = Contact.Select();
				Contact.Current.UserID = user.PKID;
			Contact.Cache.MarkUpdated(Contact.Current);

			if (this.IsContractBasedAPI) Roles.Select();
        }
		protected virtual void Users_RowInserted(PXCache sender, PXRowInsertedEventArgs e)
		{
			Users user = (Users) e.Row;

			EPLoginType ltype = PXSelect<EPLoginType, Where<EPLoginType.loginTypeID, Equal<Current<Users.loginTypeID>>>>.SelectSingleBound(this, new object[]{user});
			user.Username = ltype != null && ltype.EmailAsLogin == true ? Contact.Current.EMail : null;
			Guid? restoredGuid = Access.GetGuidFromDeletedUser(user.Username);
			if (restoredGuid != null)
			{
				user.PKID = restoredGuid;
			}

			if (Contact.Current.UserID == null)
			{
				Contact.Current.UserID = user.PKID;
			}
			else
			{
				User.Cache.Clear();
				UserRoles.Cache.Clear();
			}

		}

		protected virtual void _(Events.RowSelected<Contact> e)
		{
			Contact row = e.Row as Contact;
			if (row == null) return;

			var isNotInserted = e.Cache.GetStatus(row) != PXEntryStatus.Inserted;
            Contact.AllowDelete = row.ContactType == ContactTypesAttribute.Person;
		    PXUIFieldAttribute.SetEnabled<Contact.isActive>(Contact.Cache, row, row.ContactType == ContactTypesAttribute.Person);
            PXUIFieldAttribute.SetEnabled<Contact.classID>(Contact.Cache, row, row.ContactType == ContactTypesAttribute.Person);
            PXUIFieldAttribute.SetEnabled<Contact.isAddressSameAsMain>(Contact.Cache, row, row.ContactType == ContactTypesAttribute.Person);

            copyBAccountContactInfo.SetEnabled(row.ContactType == ContactTypesAttribute.Person);
			Answers.Cache.AllowInsert = row.ContactType == ContactTypesAttribute.Person;
			Answers.Cache.AllowUpdate = row.ContactType == ContactTypesAttribute.Person;
			Answers.Cache.AllowDelete = row.ContactType == ContactTypesAttribute.Person;

			Activities.Cache.AllowInsert = row.ContactType == ContactTypesAttribute.Person;
			Activities.Cache.AllowUpdate = row.ContactType == ContactTypesAttribute.Person;
			Activities.Cache.AllowDelete = row.ContactType == ContactTypesAttribute.Person;

			Relations.Cache.AllowInsert = row.ContactType == ContactTypesAttribute.Person && isNotInserted;
			Relations.Cache.AllowDelete = row.ContactType == ContactTypesAttribute.Person;

			Opportunities.Cache.AllowInsert = row.ContactType == ContactTypesAttribute.Person && isNotInserted;
			Opportunities.Cache.AllowUpdate = row.ContactType == ContactTypesAttribute.Person;
			Opportunities.Cache.AllowDelete = row.ContactType == ContactTypesAttribute.Person;

			Cases.Cache.AllowInsert = row.ContactType == ContactTypesAttribute.Person && isNotInserted;
			Cases.Cache.AllowUpdate = row.ContactType == ContactTypesAttribute.Person;
			Cases.Cache.AllowDelete = row.ContactType == ContactTypesAttribute.Person;

			Members.Cache.AllowInsert = row.ContactType == ContactTypesAttribute.Person && isNotInserted;
			Members.Cache.AllowUpdate = row.ContactType == ContactTypesAttribute.Person;
			Members.Cache.AllowDelete = row.ContactType == ContactTypesAttribute.Person;

			Subscriptions.Cache.AllowInsert = row.ContactType == ContactTypesAttribute.Person && isNotInserted;
			Subscriptions.Cache.AllowUpdate = row.ContactType == ContactTypesAttribute.Person;
			Subscriptions.Cache.AllowDelete = row.ContactType == ContactTypesAttribute.Person;

			NWatchers.Cache.AllowInsert = row.ContactType == ContactTypesAttribute.Person && isNotInserted;
			NWatchers.Cache.AllowUpdate = row.ContactType == ContactTypesAttribute.Person;
			NWatchers.Cache.AllowDelete = row.ContactType == ContactTypesAttribute.Person;

			User.Cache.AllowInsert = row.ContactType == ContactTypesAttribute.Person;
			User.Cache.AllowUpdate = row.ContactType == ContactTypesAttribute.Person;
			User.Cache.AllowDelete = row.ContactType == ContactTypesAttribute.Person;
			User.Cache.AllowSelect = row.ContactType == ContactTypesAttribute.Person;
			User.Cache.ClearQueryCacheObsolete();

			Roles.Cache.AllowInsert = row.ContactType == ContactTypesAttribute.Person;
			Roles.Cache.AllowUpdate = row.ContactType == ContactTypesAttribute.Person;
			Roles.Cache.AllowDelete = row.ContactType == ContactTypesAttribute.Person;
			Roles.Cache.AllowSelect = row.ContactType == ContactTypesAttribute.Person;
			Roles.Cache.ClearQueryCacheObsolete();

			UserRoles.Cache.AllowInsert = row.ContactType == ContactTypesAttribute.Person;
			UserRoles.Cache.AllowUpdate = row.ContactType == ContactTypesAttribute.Person;
			UserRoles.Cache.AllowDelete = row.ContactType == ContactTypesAttribute.Person;
			UserRoles.Cache.AllowSelect = row.ContactType == ContactTypesAttribute.Person;
			UserRoles.Cache.ClearQueryCacheObsolete();

			var bAccount = row.BAccountID.
				With<int?, BAccount>(_ => (BAccount)PXSelect<BAccount,
					Where<BAccount.bAccountID, Equal<Required<BAccount.bAccountID>>>>.
				Select(this, _));
			var isCustomerOrProspect = bAccount == null || 
				bAccount.Type == BAccountType.CustomerType ||
				bAccount.Type == BAccountType.ProspectType || 
				bAccount.Type == BAccountType.CombinedType;
			addOpportunity.SetEnabled(isNotInserted && isCustomerOrProspect);
			addCase.SetEnabled(isNotInserted && isCustomerOrProspect);

			PXUIFieldAttribute.SetEnabled<Contact.contactID>(e.Cache, row, true);
			PXUIFieldAttribute.SetEnabled<Contact.bAccountID>(e.Cache, row, row.ContactType == ContactTypesAttribute.Person);

			CRContactClass contactClass = row.ClassID.
				With(_ => (CRContactClass)PXSelectReadonly<CRContactClass,
					Where<CRContactClass.classID, Equal<Required<CRContactClass.classID>>>>.
					SelectSingleBound(this, null, _));
			if (contactClass != null)
			{
				Activities.DefaultEMailAccountId = contactClass.DefaultEMailAccountID;
			}

			bool isUserInserted = row.UserID == null || User.Cache.GetStatus(User.Current) == PXEntryStatus.Inserted;
			bool hasLoginType = isUserInserted && User.Current != null && User.Current.LoginTypeID != null;
			PXUIFieldAttribute.SetEnabled<Users.loginTypeID>(User.Cache, User.Current, isUserInserted && row.IsActive == true);
			PXUIFieldAttribute.SetEnabled<Users.username>(User.Cache, User.Current, this.IsContractBasedAPI || hasLoginType);
			PXUIFieldAttribute.SetEnabled<Users.generatePassword>(User.Cache, User.Current, this.IsContractBasedAPI || hasLoginType);
			PXUIFieldAttribute.SetEnabled<Users.password>(User.Cache, User.Current, this.IsContractBasedAPI || (hasLoginType && User.Current.GeneratePassword != true));

			var employeeHasUserAttached = row.ContactType == ContactTypesAttribute.Employee && User.Current != null;

			PXDefaultAttribute.SetPersistingCheck<Contact.eMail>(e.Cache, row, 
                employeeHasUserAttached || (hasLoginType && User.Current.Username != null )
                ? PXPersistingCheck.NullOrBlank 
                : PXPersistingCheck.Nothing);
			PXUIFieldAttribute.SetRequired<Contact.eMail>(e.Cache, employeeHasUserAttached || (hasLoginType && User.Current.Username != null));

			User.Current = (Users)User.View.SelectSingleBound(new[] { e.Row });

			PXUIFieldAttribute.SetEnabled<Address.isValidated>(e.Cache, row, false);

			PXUIFieldAttribute.SetEnabled<Contact.duplicateStatus>(e.Cache, row, false);

			PXUIFieldAttribute.SetEnabled<CRActivityStatistics.lastIncomingActivityDate>(e.Cache, row, false);
			PXUIFieldAttribute.SetEnabled<CRActivityStatistics.lastOutgoingActivityDate>(e.Cache, row, false);
		}

		protected virtual void _(Events.RowDeleting<Contact> e)
		{
			Contact contact = (Contact) e.Row;
			if(contact != null && contact.ContactType == ContactTypesAttribute.Employee)
				throw new PXSetPropertyException(Messages.CantDeleteEmployeeContact);
		}

        protected virtual void _(Events.RowUpdated<Contact> e)
        {
            Contact cont = (Contact)e.Row;
            if (cont != null && cont.ContactType == ContactTypesAttribute.Employee && !e.Cache.ObjectsEqual<Contact.displayName>(e.Row, e.OldRow))
            {
                BAccount emp =
                PXSelect<BAccount,
                    Where<BAccount.parentBAccountID, Equal<Current<Contact.bAccountID>>,
                    And<BAccount.defContactID, Equal<Current<CR.Contact.contactID>>>>>.SelectSingleBound(this, new object[]{cont});
                if (emp != null)
                {
                    emp = (BAccount)this.bAccountBasic.Cache.CreateCopy(emp);
                    this.bAccountBasic.Cache.SetValueExt<EPEmployee.acctName>(emp, cont.DisplayName);
                    this.bAccountBasic.Update(emp);
                }
            }
        }


		protected virtual void _(Events.FieldUpdated<Contact, Contact.isActive> e)
		{
			Contact c = (Contact) e.Row;

			Users user = PXSelect<Users, Where<Users.pKID, Equal<Current<Contact.userID>>>>.SelectSingleBound(this, new object[] { c });
			if (user != null)
			{
				user.IsApproved = c.IsActive == true;
				User.Update(user);
			}
		}

		protected virtual void _(Events.FieldUpdated<Contact, Contact.eMail> e)
		{
			Contact contact = (Contact)e.Row;
			
			foreach (EMailSyncAccount syncAccount in SyncAccount.Select(contact.ContactID)
					   .RowCast<EMailSyncAccount>()
					   .Select(account => (EMailSyncAccount)SyncAccount.Cache.CreateCopy(account)))
			{
				syncAccount.Address = contact.EMail;

				syncAccount.ContactsExportDate = null;
				syncAccount.ContactsImportDate = null;
				syncAccount.EmailsExportDate = null;
				syncAccount.EmailsImportDate = null;
				syncAccount.TasksExportDate = null;
				syncAccount.TasksImportDate = null;
				syncAccount.EventsExportDate = null;
				syncAccount.EventsImportDate = null;

				EMailAccount mailAccount = EMailAccounts.Select(syncAccount.EmailAccountID);
				mailAccount.Address = syncAccount.Address;

                EMailAccounts.Update(mailAccount);
                SyncAccount.Update(syncAccount);

			}
		}

		#endregion

		#region Lead

		[PXUIField(DisplayName = "Display Name", Visibility = PXUIVisibility.SelectorVisible, Enabled = false)]
		[PXMergeAttributes(Method = MergeMethod.Merge)]
		protected virtual void _(Events.CacheAttached<CRLead.memberName> e) { }

		#endregion

		#region CRCampaignMembers

		[PXDBLiteDefault(typeof(Contact.contactID))]
		[PXMergeAttributes(Method = MergeMethod.Merge)]
		protected virtual void CRCampaignMembers_ContactID_CacheAttached(PXCache sender)
		{

		}

		#endregion

		#region CRMarketingListMember

		[PXSelector(typeof(Search<CRMarketingList.marketingListID,
			Where<CRMarketingList.isDynamic, IsNull, Or<CRMarketingList.isDynamic, NotEqual<True>>>>),
			DescriptionField = typeof(CRMarketingList.mailListCode))]
		[PXMergeAttributes(Method = MergeMethod.Merge)]
		protected virtual void CRMarketingListMember_MarketingListID_CacheAttached(PXCache sender)
		{

		}

		[PXDBLiteDefault(typeof(Contact.contactID))]
		[PXMergeAttributes(Method = MergeMethod.Merge)]
		protected virtual void CRMarketingListMember_ContactID_CacheAttached(PXCache sender)
		{

		}

        protected virtual void CRMarketingListMember_RowSelected(PXCache sender, PXRowSelectedEventArgs e)
        {
            CRMarketingListMember row = e.Row as CRMarketingListMember;

            if (row == null) return;

            CRMarketingList _CRMarketingList = PXSelect<CRMarketingList, Where<CRMarketingList.marketingListID,
                Equal<Required<CRMarketingList.marketingListID>>>>.Select(this, row.MarketingListID);
            if(_CRMarketingList != null)
            {
                PXUIFieldAttribute.SetEnabled<CRMarketingList.marketingListID>(sender, row, _CRMarketingList.IsDynamic == false);
            }
        }
        #endregion

        #region CRPMTimeActivity

        [PXDBChildIdentity(typeof(Contact.contactID))]
		[PXSelector(typeof(Contact.contactID), DescriptionField = typeof(Contact.memberName), DirtyRead = true)]
        [PXMergeAttributes(Method = MergeMethod.Merge)]
        protected virtual void CRPMTimeActivity_ContactID_CacheAttached(PXCache sender) { }

        [PXDBDefault(typeof(Contact.bAccountID), PersistingCheck = PXPersistingCheck.Nothing)]
		[PXMergeAttributes(Method = MergeMethod.Merge)]
		protected virtual void CRPMTimeActivity_BAccountID_CacheAttached(PXCache sender) { }

		#endregion

		#region CROpportunityClass
		[PXUIField(DisplayName = "Class Description")]
		[PXMergeAttributes(Method = MergeMethod.Merge)]
		protected virtual void CROpportunityClass_Description_CacheAttached(PXCache sender)
		{
		}
		#endregion

		#region CRRelation

		[PXDBChildIdentity(typeof(Contact.contactID))]
		[PXMergeAttributes(Method = MergeMethod.Merge)]
		protected virtual void _(Events.CacheAttached<CRRelation.contactID> e) { }

		#endregion

		#endregion

		#region Private Methods

		protected void CopyContactInfo(Contact dest, Contact src)
		{
			if (!string.IsNullOrEmpty(src.FaxType)) dest.FaxType = src.FaxType;
			if (!string.IsNullOrEmpty(src.Phone1Type)) dest.Phone1Type = src.Phone1Type;
			if (!string.IsNullOrEmpty(src.Phone2Type)) dest.Phone2Type = src.Phone2Type;
			if (!string.IsNullOrEmpty(src.Phone3Type)) dest.Phone3Type = src.Phone3Type;

			dest.Fax = src.Fax;
			dest.Phone1 = src.Phone1;
			dest.Phone2 = src.Phone2;
			dest.Phone3 = src.Phone3;
			dest.WebSite = src.WebSite;
			dest.EMail = src.EMail;
		}

		#endregion

		#region Extensions

		/// <exclude/>
		public class DefaultContactOwnerGraphExt : CRDefaultDocumentOwner<
			ContactMaint, Contact,
			Contact.classID, Contact.ownerID, Contact.workgroupID>
		{ }

		/// <exclude/>
		public class CRDuplicateEntitiesForContactGraphExt : CRDuplicateContactEntities<ContactMaint, Contact>
		{
			#region Initialization

			protected override Type AdditionalConditions => typeof(

				Brackets<
					// do not show BA, that is currently attached to the Contact
					DuplicateDocument.bAccountID.FromCurrent.IsNotNull
					.And<Brackets<
						DuplicateContact.contactType.IsEqual<ContactTypesAttribute.bAccountProperty>
						.And<BAccountR.bAccountID.IsNotEqual<DuplicateDocument.bAccountID.FromCurrent>>
						.Or<DuplicateContact.contactType.IsNotEqual<ContactTypesAttribute.bAccountProperty>>
					>>
					.Or<DuplicateDocument.bAccountID.FromCurrent.IsNull>
				>
				.And<Brackets<
					// all Contact inside the single related BA or without related BA
					DuplicateDocument.bAccountID.FromCurrent.IsNotNull
					.And<Brackets<
						// it's a Lead with same BA
						DuplicateContact.bAccountID.IsEqual<DuplicateDocument.bAccountID.FromCurrent>
						.And<DuplicateContact.contactType.IsEqual<ContactTypesAttribute.lead>>

						// it's a Lead with no BA
						.Or<DuplicateContact.bAccountID.IsNull
							.And<DuplicateContact.contactType.IsEqual<ContactTypesAttribute.lead>>>

						// it's a Contact or BA
						.Or<DuplicateContact.contactType.IsIn<ContactTypesAttribute.person, ContactTypesAttribute.bAccountProperty>>
					>>
					.Or<DuplicateDocument.bAccountID.FromCurrent.IsNull>
				>>
				.And<Brackets<
					// Leads that are not linked to the current Contact
					Standalone.CRLead.refContactID.IsNotEqual<DuplicateDocument.contactID.FromCurrent>
					.Or<Standalone.CRLead.refContactID.IsNull>
				>>
				.And<
					DuplicateContact.isActive.IsEqual<True>.And<DuplicateContact.contactType.IsNotEqual<ContactTypesAttribute.bAccountProperty>>
					.Or<BAccountR.status.IsNotEqual<BAccountR.status.inactive>>
				>
			);

			protected override string WarningMessage => Messages.ContactHavePossibleDuplicates;

			public static bool IsActive()
			{
				return IsExtensionActive();
			}

			public override void Initialize()
			{
				base.Initialize();
			}

			protected override DocumentMapping GetDocumentMapping()
			{
				return new DocumentMapping(typeof(Contact)) { Key = typeof(Contact.contactID) };
			}

			protected override DuplicateDocumentMapping GetDuplicateDocumentMapping()
			{
				return new DuplicateDocumentMapping(typeof(Contact)) { Email = typeof(Contact.eMail) };
			}

			#endregion

			#region Events

			protected virtual void _(Events.FieldUpdated<Contact, Contact.isActive> e)
			{
				Contact row = e.Row as Contact;
				if (e.Row == null)
					return;

				if (row.IsActive == true && row.IsActive != (bool?)e.OldValue)
				{
					row.DuplicateStatus = DuplicateStatusAttribute.NotValidated;
				}
			}

			[CRDuplicateContactsSelector(typeof(MergeParams.sourceEntityID), SelectorMode = PXSelectorMode.DisplayModeText)]
			[PXMergeAttributes(Method = MergeMethod.Append)]
			protected virtual void _(Events.CacheAttached<MergeParams.targetEntityID> e) { }

			public virtual void _(Events.FieldSelecting<CRDuplicateRecord, CRDuplicateRecord.selected> e)
			{
				var doc = DuplicateDocuments.Current;

				if (e.Row == null || doc == null)
					return;

				bool isOfSameType = e.Row.DuplicateContactType == ContactTypesAttribute.Person;
				bool isOfSameParent = doc.BAccountID == null || e.Row.DuplicateBAccountID == null || e.Row.DuplicateBAccountID == doc.BAccountID;

				e.ReturnState = PXFieldState.CreateInstance(e.ReturnState, null, null, null, null, null, null, null, null, null, null,
					!isOfSameType || isOfSameParent
						? null
						: Messages.ContactBAccountDiff,
					!isOfSameType || isOfSameParent
						? PXErrorLevel.Undefined
						: PXErrorLevel.RowWarning,
					isOfSameType && isOfSameParent,
					null, null, PXUIVisibility.Undefined, null, null, null);
			}

			#endregion

			#region Overrides

			protected override Contact GetTargetEntity(int targetID)
			{
				return PXSelect<Contact, Where<Contact.contactID, Equal<Required<Contact.contactID>>>>.Select(Base, targetID);
			}

			protected override Contact GetTargetContact(Contact targetEntity)
			{
				return targetEntity as Contact;
			}

			protected override Address GetTargetAddress(Contact targetEntity)
			{
				return PXSelect<Address, Where<Address.addressID, Equal<Required<Address.addressID>>>>.Select(Base, targetEntity.DefAddressID);
			}

			protected override void MergeEntities(PXGraph targetGraph, Contact targetEntity, Contact duplicateContact)
			{
				PXCache Activities = targetGraph.Caches[typeof(CRPMTimeActivity)];
				foreach (CRPMTimeActivity activity in PXSelect<CRPMTimeActivity, Where<CRPMTimeActivity.contactID, Equal<Current<Contact.contactID>>>>
					.SelectMultiBound(targetGraph, new object[] { duplicateContact })
					.RowCast<CRPMTimeActivity>()
					.Select(cas => (CRPMTimeActivity)Activities.CreateCopy(cas)))
				{
					activity.ContactID = targetEntity.ContactID;
					activity.BAccountID = targetEntity.BAccountID;

					Activities.Update(activity);
				}

				PXCache Cases = targetGraph.Caches[typeof(CRCase)];
				foreach (CRCase cas in PXSelect<CRCase,
						Where<CRCase.contactID, Equal<Current<Contact.contactID>>>>
					.SelectMultiBound(targetGraph, new object[] { duplicateContact })
					.RowCast<CRCase>()
					.Select(cas => (CRCase)Cases.CreateCopy(cas)))
				{
					if (targetEntity.BAccountID != cas.CustomerID)
					{
						throw new PXException(Messages.ContactBAccountCase, duplicateContact.DisplayName, cas.CaseCD);
					}

					cas.ContactID = targetEntity.ContactID;

					Cases.Update(cas);
				}

				PXCache Opportunities = targetGraph.Caches[typeof(CROpportunity)];
				foreach (CROpportunity opp in PXSelect<CROpportunity,
						Where<CROpportunity.contactID, Equal<Current<Contact.contactID>>>>
					.SelectMultiBound(targetGraph, new object[] { duplicateContact })
					.RowCast<CROpportunity>()
					.Select(opp => (CROpportunity)Opportunities.CreateCopy(opp)))
				{
					if (targetEntity.BAccountID != opp.BAccountID)
					{
						throw new PXException(Messages.ContactBAccountForOpp, duplicateContact.DisplayName, duplicateContact.ContactID, opp.OpportunityID);
					}

					opp.ContactID = targetEntity.ContactID;

					Opportunities.Update(opp);
				}

				PXCache Relations = targetGraph.Caches[typeof(CRRelation)];
				foreach (CRRelation rel in PXSelectJoin<CRRelation,
						LeftJoin<CRRelation2, On<CRRelation.entityID, Equal<CRRelation2.entityID>,
							And<CRRelation.role, Equal<CRRelation2.role>,
								And<CRRelation2.refNoteID, Equal<Required<Contact.noteID>>>>>>,
						Where<CRRelation2.entityID, IsNull,
							And<CRRelation.refNoteID, Equal<Required<Contact.noteID>>>>>
					.Select(targetGraph, targetEntity.NoteID, duplicateContact.NoteID)
					.RowCast<CRRelation>()
					.Select(rel => (CRRelation)Relations.CreateCopy(rel)))
				{
					rel.RelationID = null;
					rel.RefNoteID = targetEntity.NoteID;

					Relations.Insert(rel);
				}

				PXCache Subscriptions = targetGraph.Caches[typeof(CRMarketingListMember)];
				foreach (CRMarketingListMember mmember in PXSelectJoin<CRMarketingListMember,
						LeftJoin<CRMarketingListMember2, On<CRMarketingListMember.marketingListID, Equal<CRMarketingListMember2.marketingListID>,
							And<CRMarketingListMember2.contactID, Equal<Required<Contact.contactID>>>>>,
						Where<CRMarketingListMember.contactID, Equal<Required<Contact.contactID>>,
							And<CRMarketingListMember2.marketingListID, IsNull>>>
					.Select(targetGraph, targetEntity.ContactID, duplicateContact.ContactID)
					.RowCast<CRMarketingListMember>()
					.Select(mmember => (CRMarketingListMember)Subscriptions.CreateCopy(mmember)))
				{
					mmember.ContactID = targetEntity.ContactID;

					Subscriptions.Insert(mmember);
				}

				PXCache Members = targetGraph.Caches[typeof(CRCampaignMembers)];
				foreach (CRCampaignMembers cmember in PXSelectJoin<CRCampaignMembers,
						LeftJoin<CRCampaignMembers2, On<CRCampaignMembers.campaignID, Equal<CRCampaignMembers2.campaignID>,
							And<CRCampaignMembers2.contactID, Equal<Required<Contact.contactID>>>>>,
						Where<CRCampaignMembers2.campaignID, IsNull,
							And<CRCampaignMembers.contactID, Equal<Required<Contact.contactID>>>>>
					.Select(targetGraph, targetEntity.ContactID, duplicateContact.ContactID)
					.RowCast<CRCampaignMembers>()
					.Select(cmember => (CRCampaignMembers)Members.CreateCopy(cmember)))
				{
					cmember.ContactID = targetEntity.ContactID;

					Members.Insert(cmember);
				}

				PXCache NWatchers = targetGraph.Caches[typeof(ContactNotification)];
				foreach (ContactNotification watcher in PXSelectJoin<ContactNotification,
						LeftJoin<ContactNotification2, On<ContactNotification.setupID, Equal<ContactNotification2.setupID>,
							And<ContactNotification2.contactID, Equal<Required<Contact.contactID>>>>>,
						Where<ContactNotification2.setupID, IsNull,
							And<ContactNotification.contactID, Equal<Required<Contact.contactID>>>>>
					.Select(targetGraph, targetEntity.ContactID, duplicateContact.ContactID)
					.RowCast<ContactNotification>()
					.Select(watcher => (ContactNotification)NWatchers.CreateCopy(watcher)))
				{
					watcher.NotificationID = null;
					watcher.ContactID = targetEntity.ContactID;

					NWatchers.Insert(watcher);
				}

				PXCache Leads = targetGraph.Caches[typeof(CRLead)];
				foreach (CRLead lead in PXSelect<
							CRLead,
						Where<
							CRLead.refContactID, Equal<Required<Contact.contactID>>>>
					.Select(targetGraph, duplicateContact.ContactID)
					.RowCast<CRLead>()
					.Select(lead => (CRLead)Leads.CreateCopy(lead)))
				{
					lead.RefContactID = targetEntity.ContactID;
					lead.BAccountID = targetEntity.BAccountID;

					Leads.Update(lead);
				}

				if (duplicateContact.UserID != null)
				{
					Users user = PXSelect<Users, Where<Users.pKID, Equal<Required<Contact.userID>>>>.Select(targetGraph, duplicateContact.UserID);
					if (user != null)
					{
						targetGraph.EnsureCachePersistence(typeof(Users));
						user.IsApproved = false;
						targetGraph.Caches[typeof(Users)].Update(user);
					}
				}
			}

			public override void GetAllProperties(List<FieldValue> values, HashSet<string> fieldNames)
			{
				int order = 0;

				values.AddRange(GetMarkedPropertiesOf<Contact>(Base, ref order).Where(fld => fieldNames.Add(fld.Name)));

				base.GetAllProperties(values, fieldNames);
			}

			protected override void DoDuplicateAttach(DuplicateDocument duplicateDocument)
			{
				var duplicateDecord = Duplicates.Cache.Current as CRDuplicateRecord;

				if (duplicateDecord == null)
					return;

				Contact duplicate = PXSelect<Contact,
						Where<Contact.contactID, Equal<Current<CRDuplicateRecord.duplicateContactID>>>>
					.SelectSingleBound(Base, new object[] { duplicateDecord });

				if (duplicate == null)
					return;

				if (duplicate.ContactType == ContactTypesAttribute.Lead)
				{
					CRLead lead = PXSelect<CRLead,
							Where<CRLead.contactID, Equal<Current<CRDuplicateRecord.duplicateContactID>>>>
						.SelectSingleBound(Base, new object[] { duplicateDecord });

					lead.RefContactID = duplicateDocument.ContactID;
					lead.BAccountID = null; // AC-149963: This will trigger filling contact info from contact (Lead Account will also be inherited from contact)

					Base.Leads.Update(lead);
				}
				else if (duplicate.ContactType == ContactTypesAttribute.BAccountProperty)
				{
					duplicateDocument.BAccountID = duplicate.BAccountID;
				}
				else
				{
					throw new PXException(Messages.AttachToAccountNotFound);
				}

				DuplicateDocuments.Update(duplicateDocument);
			}

			#endregion
		}

		/// <exclude/>
		public class ContactAccountDataReplacementGraphExt : CRContactAccountDataSync<ContactMaint>
		{
			public PXSelect<Address,
				Where<
					Address.addressID, Equal<Required<CRLead.defAddressID>>>>
				LeadAddress;

			public override void Initialize()
			{
				base.Initialize();

				Addresses = new PXSelectExtension<CR.Extensions.CRContactAccountDataSync.DocumentAddress>(LeadAddress);
			}

			protected override DocumentMapping GetDocumentMapping()
			{
				return new DocumentMapping(typeof(CRLead));
			}
			protected override DocumentContactMapping GetDocumentContactMapping()
			{
				return new DocumentContactMapping(typeof(CRLead)) { Email = typeof(CRLead.eMail) };
			}
			protected override DocumentAddressMapping GetDocumentAddressMapping()
			{
				return new DocumentAddressMapping(typeof(Address));
			}
		}

		/// <exclude/>
		public class UpdateRelatedContactInfoFromContactGraphExt : CRUpdateRelatedContactInfoGraphExt<ContactMaint>
		{
			private bool IsDefAddressIDChanged = false;

			protected virtual void _(Events.RowPersisted<Contact> e)
			{
				var row = e.Row;
				if (row == null || e.TranStatus != PXTranStatus.Open || e.Operation != PXDBOperation.Update)
					return;

				if (CheckIsDefAddressIDChanged(e.Cache, row))
				{
					Address address = Base.AddressCurrent.Current ?? Base.AddressCurrent.Select();

					Base.AddressCurrent.Cache.MarkUpdated(address);
				}

				if (!ContactWasUpdated(e.Cache, row))
				{
					return;
				}

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
					// Leads that are linked to the same Contact
					Standalone.CRLead.refContactID.IsEqual<@P.AsInt>
					.And<Standalone.CRLead.overrideRefContact.IsEqual<False>>>>
				.Update(Base, 
					// Set
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
					row.ContactID);
			}

			protected virtual void _(Events.RowPersisted<Address> e)
			{
				var row = e.Row;
				if (row == null || e.TranStatus != PXTranStatus.Open || (e.Operation != PXDBOperation.Update && e.Operation != PXDBOperation.Insert))
					return;

				Contact contact = Base.Contact.Current ?? PXSelect<
							Contact,
						Where<
							Contact.defAddressID, Equal<@P.AsInt>,
							And<Contact.contactType, Equal<ContactTypesAttribute.person>>>>
					.Select(Base, row.AddressID);

				if (contact == null)
					return;

				if (!(IsDefAddressIDChanged || CheckIsDefAddressIDChanged(Base.Contact.Cache, contact)) && !AddressWasUpdated(e.Cache, row))
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
					// Leads that are linked to the same Contact
					Standalone.CRLead.refContactID.IsEqual<@P.AsInt>
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
					contact.ContactID);
			}

			protected bool CheckIsDefAddressIDChanged(PXCache cache, Contact contact)
			{
				var origianl = cache.GetOriginal(contact) as Contact;

				IsDefAddressIDChanged = origianl.DefAddressID != contact.DefAddressID;

				return IsDefAddressIDChanged;
			}
		}

		/// <exclude/>
		public class CreateLeadFromContactGraphExt : CRCreateLeadAction<ContactMaint, Contact>
		{
			public override void Initialize()
			{
				base.Initialize();

				Addresses = new PXSelectExtension<CR.Extensions.CRCreateActions.DocumentAddress>(Base.AddressCurrent);
			}

			protected override DocumentContactMapping GetDocumentContactMapping()
			{
				return new DocumentContactMapping(typeof(Contact)) { Email = typeof(Contact.eMail) };
			}
			protected override DocumentAddressMapping GetDocumentAddressMapping()
			{
				return new DocumentAddressMapping(typeof(Address));
			}
		}

		/// <exclude/>
		public class CreateAccountFromContactGraphExt : CRCreateAccountAction<ContactMaint, Contact>
		{
			protected override string TargetType => CRTargetEntityType.Contact;

			public override void Initialize()
			{
				base.Initialize();

				Addresses = new PXSelectExtension<CR.Extensions.CRCreateActions.DocumentAddress>(Base.AddressCurrent);
			}

			protected override DocumentContactMapping GetDocumentContactMapping()
			{
				return new DocumentContactMapping(typeof(Contact)) { Email = typeof(Contact.eMail) };
			}
			protected override DocumentAddressMapping GetDocumentAddressMapping()
			{
				return new DocumentAddressMapping(typeof(Address));
			}

			protected override PXSelectBase<CRPMTimeActivity> Activities => Base.Activities;

			protected virtual void _(Events.FieldDefaulting<AccountsFilter, AccountsFilter.accountClass> e)
			{
				if (ExistingAccount.SelectSingle() is BAccount existingAccount)
				{
					e.NewValue = existingAccount.ClassID;
					e.Cancel = true;
					return;
				}

				Contact contact = Base.Contact.Current;
				if (contact == null) return;

				CRContactClass cls = PXSelect<
						CRContactClass,
					Where<
						CRContactClass.classID, Equal<Required<Contact.classID>>>>
					.Select(Base, contact.ClassID);

				if (cls?.TargetBAccountClassID != null)
				{
					e.NewValue = cls.TargetBAccountClassID;
				}
				else
				{
					e.NewValue = Base.Setup.Current?.DefaultCustomerClassID;
				}

				e.Cancel = true;
			}

			protected override void MapAddress(CR.Extensions.CRCreateActions.DocumentAddress docAddress, BAccount account, ref Address address)
			{
				// set address to account as is from contact
				// no need to check in release, should work properly, just to ensure
				System.Diagnostics.Debug.Assert(Base.Caches<Contact>().Current != null,
					"Random address will be used, there is no contact in currents");
				address = Base.AddressCurrent.View.SelectSingle() as Address ?? address;

				base.MapAddress(docAddress, account, ref address);
				account.DefAddressID = address.AddressID;
				address.BAccountID = account.BAccountID;
			}

		}

		#endregion
	}
}
