using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using PX.Common;
using PX.Data;
using PX.Objects.AR;
using PX.Objects.CR.MassProcess;
using PX.Objects.GL;
using PX.Objects.CS;
using PX.Objects.EP;
using PX.SM;

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
		public SelectContactEmailSync<Where<Contact.contactType, Equal<ContactTypesAttribute.person>, Or<Contact.contactType, Equal<ContactTypesAttribute.employee>>>>
			Contact;

		public PXSelect<Contact,
				Where<Contact.contactID, Equal<Current<Contact.contactID>>>>
			ContactCurrent;
		
		public PXSelect<Contact,
				Where<Contact.contactID, Equal<Current<Contact.contactID>>>>
			ContactCurrent2;

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

		[PXViewName(Messages.Opportunities)]
		[PXFilterable]
		[PXViewDetailsButton(typeof(Contact))]
		public PXSelectReadonly2<CROpportunity,
			LeftJoin<CROpportunityProbability, On<CROpportunityProbability.stageCode, Equal<CROpportunity.stageID>>>,
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
		public CRMMarketingContactSubscriptions
			Subscriptions;
		
		[PXViewName(Messages.Notifications)]
		public PXSelectJoin<ContactNotification,
			InnerJoin<NotificationSetup,
				On<NotificationSetup.setupID, Equal<ContactNotification.setupID>>>,
			Where<ContactNotification.contactID, Equal<Optional<Contact.contactID>>>>
			NWatchers;

		[PXViewDetailsButton(typeof(Contact),
			typeof(Select<Contact,
			Where<Contact.contactID, Equal<Current<CRDuplicateRecord.duplicateContactID>>>>))]
		[PXViewDetailsButton(typeof(Contact),
			typeof(Select2<BAccountCRM,
				InnerJoin<Contact, On<Contact.bAccountID, Equal<BAccountCRM.bAccountID>>>,
				Where<Contact.contactID, Equal<Current<CRDuplicateRecord.duplicateContactID>>>>),
				ActionName = "Duplicates_BAccount_ViewDetails")]
		public CRDuplicateContactList Duplicates;

		#endregion

		#region Create Account

		[PXViewName(Messages.CreateAccount)]
		public PXFilter<LeadMaint.AccountsFilter> AccountInfo;

		protected virtual void AccountsFilter_BAccountID_FieldDefaulting(PXCache sender, PXFieldDefaultingEventArgs e)
		{
			if (this.IsDimensionAutonumbered(CustomerAttribute.DimensionName))
			{
				e.NewValue = this.GetDimensionAutonumberingNewValue(CustomerAttribute.DimensionName);
			}
		}

		protected virtual void AccountsFilter_AccountClass_FieldDefaulting(PXCache sender, PXFieldDefaultingEventArgs e)
		{
            Contact contact = sender.Graph.Caches<Contact>().Current as Contact;
            if (contact == null) return;
            CRContactClass ctcls = PXSelect<CRContactClass,
                                        Where<CRContactClass.classID,
                                        Equal<Required<Contact.classID>>>>.Select(this, contact.ClassID);
            if (ctcls != null)
            {
                if (ctcls.TargetBAccountClassID == null)
                {
                    CRCustomerClass cls = PXSelect<CRCustomerClass, Where<CRCustomerClass.cRCustomerClassID, Equal<Required<Contact.classID>>>>.Select(this, contact.ClassID);
                    if (cls != null)
                    {
                        e.NewValue = cls.CRCustomerClassID;
                    }
                }
                else
                {
                    e.NewValue = ctcls.TargetBAccountClassID;
                }
            }
		}

		protected virtual void AccountsFilter_RowSelected(PXCache sender, PXRowSelectedEventArgs e)
		{
			PXUIFieldAttribute.SetEnabled<LeadMaint.AccountsFilter.bAccountID>(sender, e.Row, !this.IsDimensionAutonumbered(CustomerAttribute.DimensionName));
		}
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

            CRContactClass ctcls = PXSelect<CRContactClass, Where<CRContactClass.classID, Equal<Required<Contact.classID>>>>.Select(this, row.ClassID);
			var graph = PXGraph.CreateInstance<OpportunityMaint>();
			var newOpportunity = graph.Opportunity.Insert();
            newOpportunity.BAccountID = row.BAccountID;
            if (ctcls != null && ctcls.TargetOpportunityClassID != null)
            {
                newOpportunity.ClassID = ctcls.TargetOpportunityClassID;
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

		public PXAction<Contact> checkForDuplicates;
		[PXUIField(DisplayName = Messages.CheckForDuplicates)]
		[PXButton(ImageKey = PX.Web.UI.Sprite.Main.Process)]
		public virtual IEnumerable CheckForDuplicates(PXAdapter adapter)
		{
			foreach (Contact rec in adapter.Get())
			{
				Contact.Current = rec;
				Contact contact = rec;

				if (adapter.ExternalCall || rec.DuplicateStatus == DuplicateStatusAttribute.NotValidated)
				{					
					contact = Contact.Cache.CreateCopy(Contact.Current) as Contact;					
					contact.DuplicateStatus = DuplicateStatusAttribute.NotValidated;
					Contact.Update(contact);

					Contact.Current.DuplicateFound = true;
					Duplicates.View.Clear();
					var result = Duplicates.Select();					


					contact = Contact.Cache.CreateCopy(Contact.Current) as Contact;
					contact.DuplicateFound = (result != null && result.Count > 0);
					contact.DuplicateStatus = DuplicateStatusAttribute.Validated;

					Decimal? score = 0;
					foreach (PXResult<CRDuplicateRecord, Contact, CRLeadContactValidationProcess.Contact2> r in result)
					{
						CRLeadContactValidationProcess.Contact2 duplicate = r;
						CRDuplicateRecord contactScore = r;
						int duplicateWeight = GetContactWeight(duplicate);
						int currentWeight = GetContactWeight(Contact.Current);
						if (duplicateWeight > currentWeight ||
								(duplicateWeight == currentWeight &&
								 duplicate.ContactID < Contact.Current.ContactID))
						{
							contact.DuplicateStatus = DuplicateStatusAttribute.PossibleDuplicated;
							if (contactScore.Score > score)
								score = contactScore.Score;
						}
					}
					contact = Contact.Update(contact);

					if (Contact.Current.DuplicateFound == false)
					{
						if (adapter.ExternalCall)
							Contact.Cache.RaiseExceptionHandling<Contact.duplicateStatus>(contact, null,
								new PXSetPropertyException(
									Messages.NoPossibleDuplicates,
									PXErrorLevel.RowInfo));
						else
							Contact.Cache.RaiseExceptionHandling<Contact.duplicateStatus>(contact, null, null);
					}
				}
				yield return contact;
			}

			if (Contact.Cache.IsDirty)
				Save.Press();
		}	

		public PXAction<Contact> convertToBAccount;
		[PXUIField(DisplayName = Messages.ConvertToBAccount, MapEnableRights = PXCacheRights.Update, MapViewRights = PXCacheRights.Select)]
		[PXButton(ImageKey = PX.Web.UI.Sprite.Main.Process)]
		public virtual IEnumerable ConvertToBAccount(PXAdapter adapter)
		{
			List<Contact> contacts = new List<Contact>(adapter.Get().Cast<Contact>());
			foreach (Contact contact in contacts.Where(contact => contact != null))
			{
				if (AccountInfo.AskExt((graph, view) => graph.Views[view].Cache.Clear(), true) != WebDialogResult.Yes) return contacts;
				bool empty_required = !AccountInfo.VerifyRequired();
				BAccount existing = PXSelect<BAccount, Where<BAccount.acctCD, Equal<Required<BAccount.acctCD>>>>.SelectSingleBound(this, null, AccountInfo.Current.BAccountID);
				if (existing != null)
				{
					AccountInfo.Cache.RaiseExceptionHandling<LeadMaint.AccountsFilter.bAccountID>(AccountInfo.Current, AccountInfo.Current.BAccountID, new PXSetPropertyException(Messages.BAccountAlreadyExists, AccountInfo.Current.BAccountID));
					return contacts;
				}
				if (empty_required) return contacts;

				Save.Press();
				Contact c = contact;
				PXLongOperation.StartOperation(this, () => LeadMaint.ConvertToAccount(c, AccountInfo.Current, this.IsContractBasedAPI));
			}
			return contacts;
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
		public virtual void Contact_ContactID_CacheAttached(PXCache sender) { }

		[PXUIField(DisplayName = "Contact Class")]
		[PXDefault(typeof(Search<CRSetup.defaultContactClassID>), PersistingCheck = PXPersistingCheck.Nothing)]
		[PXMergeAttributes(Method = MergeMethod.Merge)]
		public virtual void Contact_ClassID_CacheAttached(PXCache sender) { }

		[PXDBString(1, IsFixed = true)]
		[PXUIField(DisplayName = "Source")]
		[CRMSources]
		[PXMassMergableField]
		[PXMergeAttributes(Method = MergeMethod.Replace)]	// delete PXFormula from DAC
		public virtual void Contact_Source_CacheAttached(PXCache sender) { }

		[PXUIField(DisplayName = "Synchronize")]
		[ContactSynchronize]
		[PXMergeAttributes(Method = MergeMethod.Merge)]
		public virtual void Contact_Synchronize_CacheAttached(PXCache sender) { }

		[BAccount(Visibility = PXUIVisibility.SelectorVisible)]
		[PXRestrictor(typeof(Where<Current<Contact.contactType>, NotEqual<ContactTypesAttribute.employee>, 
			Or<BAccount.type, Equal<BAccountType.branchType>,
			Or<BAccount.type, Equal<BAccountType.organizationType>, 
			Or<BAccount.type, Equal<BAccountType.organizationBranchCombinedType>>>>>), Messages.BAccountIsType, typeof(BAccount.type))]
		[PXRestrictor(typeof(Where<Current<Contact.contactType>, NotEqual<ContactTypesAttribute.person>, 
			Or<BAccount.type, Equal<BAccountType.prospectType>,
			Or<BAccount.type, Equal<BAccountType.customerType>,
			Or<BAccount.type, Equal<BAccountType.vendorType>,
			Or<BAccount.type, Equal<BAccountType.combinedType>>>>>>), Messages.BAccountIsType, typeof(BAccount.type))]
		[PXMergeAttributes(Method = MergeMethod.Replace)]
		public virtual void Contact_BAccountID_CacheAttached(PXCache sender) { }

		[PXUIField(DisplayName = "Company Name", Visibility = PXUIVisibility.SelectorVisible)]
		[CRLeadFullName(typeof(Contact.bAccountID))]
		[PXMassMergableField]
		[PXPersonalDataField]
		[PXMergeAttributes(Method = MergeMethod.Replace)]
		public virtual void Contact_FullName_CacheAttached(PXCache sender){}

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
			if (PXAccess.FeatureInstalled<FeaturesSet.contactDuplicate>() && Setup.Current.ValidateContactDuplicatesOnEntry == true)
			{
				checkForDuplicates.Press();
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

		protected virtual void Contact_RowSelected(PXCache sender, PXRowSelectedEventArgs e)
		{
			Contact row = e.Row as Contact;
			if (row == null) return;

			var isNotInserted = sender.GetStatus(row) != PXEntryStatus.Inserted;
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
			Relations.Cache.AllowUpdate = row.ContactType == ContactTypesAttribute.Person;
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
			User.Cache.ClearQueryCache();

			Roles.Cache.AllowInsert = row.ContactType == ContactTypesAttribute.Person;
			Roles.Cache.AllowUpdate = row.ContactType == ContactTypesAttribute.Person;
			Roles.Cache.AllowDelete = row.ContactType == ContactTypesAttribute.Person;
			Roles.Cache.AllowSelect = row.ContactType == ContactTypesAttribute.Person;
			Roles.Cache.ClearQueryCache();

			UserRoles.Cache.AllowInsert = row.ContactType == ContactTypesAttribute.Person;
			UserRoles.Cache.AllowUpdate = row.ContactType == ContactTypesAttribute.Person;
			UserRoles.Cache.AllowDelete = row.ContactType == ContactTypesAttribute.Person;
			UserRoles.Cache.AllowSelect = row.ContactType == ContactTypesAttribute.Person;
			UserRoles.Cache.ClearQueryCache();

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

			PXUIFieldAttribute.SetEnabled<Contact.contactID>(sender, row, true);
			PXUIFieldAttribute.SetEnabled<Contact.bAccountID>(sender, row, row.ContactType == ContactTypesAttribute.Person);

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

			PXDefaultAttribute.SetPersistingCheck<Contact.eMail>(sender, row, 
                employeeHasUserAttached || (hasLoginType && User.Current.Username != null )
                ? PXPersistingCheck.NullOrBlank 
                : PXPersistingCheck.Nothing);
			PXUIFieldAttribute.SetRequired<Contact.eMail>(sender, employeeHasUserAttached || (hasLoginType && User.Current.Username != null));

			User.Current = (Users)User.View.SelectSingleBound(new[] { e.Row });

			// Lead History
			PXUIFieldAttribute.SetEnabled<Contact.source>(sender, row, false);
			PXUIFieldAttribute.SetEnabled<Contact.campaignID>(sender, row, false);
			PXUIFieldAttribute.SetEnabled<Contact.status>(sender, row, false);
			PXUIFieldAttribute.SetEnabled<Contact.resolution>(sender, row, false);
			PXUIFieldAttribute.SetEnabled<Contact.convertedBy>(sender, row, false);
			PXUIFieldAttribute.SetEnabled<Contact.qualificationDate>(sender, row, false);

			PXUIFieldAttribute.SetEnabled<Address.isValidated>(sender, row, false);

			PXUIFieldAttribute.SetEnabled<Contact.duplicateStatus>(sender, row, false);

			PXUIFieldAttribute.SetEnabled<CRActivityStatistics.lastIncomingActivityDate>(sender, row, false);
			PXUIFieldAttribute.SetEnabled<CRActivityStatistics.lastOutgoingActivityDate>(sender, row, false);


			if (row.DuplicateStatus == DuplicateStatusAttribute.PossibleDuplicated || row.DuplicateFound == true)
			{
				sender.RaiseExceptionHandling<Contact.duplicateStatus>(row,
					null, new PXSetPropertyException(Messages.ContactHavePossibleDuplicates, PXErrorLevel.Warning, row.ContactID));
			}
		}

		protected virtual void Contact_RowDeleting(PXCache sender, PXRowDeletingEventArgs e)
		{
			Contact contact = (Contact) e.Row;
			if(contact != null && contact.ContactType == ContactTypesAttribute.Employee)
				throw new PXSetPropertyException(Messages.CantDeleteEmployeeContact);
		}
        protected virtual void Contact_RowUpdated(PXCache cache, PXRowUpdatedEventArgs e)
        {
            Contact cont = (Contact)e.Row;
            if (cont != null && cont.ContactType == ContactTypesAttribute.Employee && !cache.ObjectsEqual<Contact.displayName>(e.Row, e.OldRow))
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


		protected virtual void Contact_IsActive_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
		{
			Contact c = (Contact) e.Row;
			if (c.IsActive == true && c.IsActive != (bool?)e.OldValue)
			{
				c.DuplicateStatus = DuplicateStatusAttribute.NotValidated;
			}

			Users user = PXSelect<Users, Where<Users.pKID, Equal<Current<Contact.userID>>>>.SelectSingleBound(this, new object[] { c });
			if (user != null)
			{
				user.IsApproved = c.IsActive == true;
				User.Update(user);
			}
		}

		protected virtual void Contact_RowPersisted(PXCache sender, PXRowPersistedEventArgs e)
		{
			Contact row = e.Row as Contact;
			if (row == null || e.TranStatus != PXTranStatus.Open) return;
			if (CRGrammProcess.PersistGrams(this, row))
				row.DuplicateStatus = DuplicateStatusAttribute.NotValidated;
		}

		protected virtual void Contact_Email_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
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
        [PXMergeAttributes(Method = MergeMethod.Merge)]
        protected virtual void CRPMTimeActivity_ContactID_CacheAttached(PXCache sender) { }

        [PXDBDefault(typeof(Contact.bAccountID), PersistingCheck = PXPersistingCheck.Nothing)]
		[PXMergeAttributes(Method = MergeMethod.Merge)]
		protected virtual void CRPMTimeActivity_BAccountID_CacheAttached(PXCache sender) { }

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

		public static int GetContactWeight(Contact contact)
		{
		    if (contact.ContactType == ContactTypesAttribute.Employee)
		        return 4;
		    if (contact.ContactType == ContactTypesAttribute.BAccountProperty)
		    {
		        BAccount bAccount =
		            PXSelect<BAccount, Where<BAccount.bAccountID, Equal<Required<Contact.bAccountID>>>>.Select(
		                new PXGraph(), contact.BAccountID);

		        if (bAccount != null &&
		            (bAccount.Type == BAccountType.CustomerType || bAccount.Type == BAccountType.VendorType))
		            return 3;
                return 2;
		    }
				
			if (contact.ContactType == ContactTypesAttribute.Person)
				return 1;
			return 0;
		}
		#endregion
	}
}
