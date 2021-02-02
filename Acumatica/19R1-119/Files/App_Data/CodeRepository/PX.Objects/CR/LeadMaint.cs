using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using PX.Common;
using PX.Objects.AR;
using PX.Objects.CR.MassProcess;
using PX.Objects.CS;
using PX.Objects.GL;
using PX.Data;
using PX.Objects.IN;
using PX.SM;

namespace PX.Objects.CR
{
    public class LeadMaint : PXGraph<LeadMaint, Contact, Contact.displayName>
	{

		#region AccountsFilter
		[Serializable]
		[PXHidden]
		public partial class AccountsFilter : IBqlTable
		{
			#region BAccountID
			public abstract class bAccountID : PX.Data.BQL.BqlString.Field<bAccountID> { }

			[PXDefault]
			[PXDBString(30, IsUnicode = true, InputMask = ">CCCCCCCCCCCCCCCCCCCCCCCCCCCCCC")]
			[PXUIField(DisplayName = "Business Account ID", Required = true)]
			public virtual string BAccountID { get; set; }
			#endregion

			#region AccountName
			public abstract class accountName : PX.Data.BQL.BqlString.Field<accountName> { }

			[PXDefault(typeof(Contact.fullName))]
			[PXDBString(60, IsUnicode = true)]
			[PXUIField(DisplayName = "Company Name", Required = true)]
			public virtual string AccountName { get; set; }
			#endregion

			#region AccountClass
			public abstract class accountClass : PX.Data.BQL.BqlString.Field<accountClass> { }

			[PXDefault(typeof(CRSetup.defaultCustomerClassID), PersistingCheck = PXPersistingCheck.Nothing)]
			[PXDBString(10, IsUnicode = true, InputMask = ">aaaaaaaaaa")]
            [PXUIField(DisplayName = "Business Account Class", Required = true)]
			[PXSelector(typeof(CRCustomerClass.cRCustomerClassID))]
			public virtual string AccountClass { get; set; }
            #endregion

            #region NoteID
            public abstract class noteID : PX.Data.BQL.BqlGuid.Field<noteID> { }

		    [PXNote]
		    public virtual Guid? NoteID { get; set; }
		    #endregion

        }

        #endregion

        #region OppotunityFilter
        [Serializable]
		[PXHidden]
		public class OpportunityFilter : IBqlTable
		{

			#region OpportunityID
			public abstract class opportunityID : PX.Data.BQL.BqlString.Field<opportunityID> { }

			[PXDBString(CROpportunity.OpportunityIDLength, IsUnicode = true, InputMask = ">CCCCCCCCCC")]
			[PXUIField(DisplayName = "Opportunity ID", Required = true)]
			[PXDefault]
			public virtual String OpportunityID { get; set; }
			#endregion

		}
        #endregion


        #region Selects
        

        //TODO: need review
        [PXHidden]
		public PXSelect<BAccount>
			bAccountBasic;

        [PXHidden]
		[PXCheckCurrent]
		public PXSetup<Company>
			company;

		[PXHidden]
		[PXCheckCurrent]
		public PXSetup<CRSetup>
			Setup;
			
		[PXViewName(Messages.Lead)]
		[PXCopyPasteHiddenFields(typeof(Contact.status), typeof(Contact.duplicateStatus), typeof(Contact.resolution))]
		public PXSelect<Contact,
			Where<Contact.contactType, Equal<ContactTypesAttribute.lead>, 
            Or<Where<Contact.contactType, Equal<ContactTypesAttribute.person>, And<Contact.status, Equal<LeadStatusesAttribute.converted>>>>>>
			Lead;

		[PXHidden]
		public PXSelect<Contact,
			Where<Contact.contactID, Equal<Current<Contact.contactID>>>>
			LeadCurrent;

		[PXCopyPasteHiddenView]
		public PXSelect<CRActivityStatistics,
				Where<CRActivityStatistics.noteID, Equal<Current<Contact.noteID>>>>
			LeadActivityStatistics;

		[PXHidden]
		public PXSelect<Contact,
			Where<Contact.contactID, Equal<Current<Contact.contactID>>>>
			LeadCurrent2;

		[PXViewName(Messages.Address)]
		public AddressSelect<Contact.defAddressID, Contact.isAddressSameAsMain, Contact.bAccountID>
			AddressCurrent;

		[PXViewName(Messages.Answers)]
		public CRAttributeList<Contact>
			Answers;

		[PXViewName(Messages.Activities)]
		[PXFilterable]
		[CRDefaultMailTo]
        [CRReference(typeof(Contact.bAccountID), typeof(Contact.contactID))]
        public CRActivityList<Contact>
			Activities;
		
		[PXCopyPasteHiddenView]
		[PXViewName(Messages.Relations)]
		[PXFilterable]
		public CRRelationsList<Contact.noteID>
			Relations;

		[PXViewName(Messages.Opportunities)]
		[PXFilterable]
		[PXViewDetailsButton(typeof(Contact))]
		[PXViewDetailsButton(typeof(Contact), 
			typeof(Select<Contact, 
				Where<Contact.contactID, Equal<Current<CROpportunity.contactID>>>>))]
		public PXSelectReadonly2<CROpportunity,
			LeftJoin<Contact, On<Contact.contactID, Equal<CROpportunity.contactID>>, 
			LeftJoin<CROpportunityProbability, On<CROpportunityProbability.stageCode, Equal<CROpportunity.stageID>>>>,
			Where<Current<Contact.contactID>, Greater<Zero>,
 			And<Where<CROpportunity.bAccountID, Equal<Current<Contact.bAccountID>>,
					   Or<CROpportunity.contactID, Equal<Current<Contact.contactID>>>>>>>
			Opportunities;

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

		[PXViewName(Messages.Subscriptions)]
		[PXFilterable]
		[PXViewDetailsButton(typeof(Contact),
			typeof(Select<CRMarketingList,
				Where<CRMarketingList.marketingListID, Equal<Current<CRMarketingListMember.marketingListID>>>>))]
		public CRMMarketingContactSubscriptions
			Subscriptions;

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



		#region Delegates

		#endregion

		#region Create Account

		[PXViewName(Messages.CreateAccount)]
		public PXFilter<AccountsFilter> AccountInfo;

		protected virtual void AccountsFilter_BAccountID_FieldDefaulting(PXCache sender, PXFieldDefaultingEventArgs e)
		{
			if (this.IsDimensionAutonumbered(CustomerAttribute.DimensionName))
			{
				e.NewValue = this.GetDimensionAutonumberingNewValue(CustomerAttribute.DimensionName);
			}
		}

		protected virtual void AccountsFilter_AccountName_FieldDefaulting(PXCache sender, PXFieldDefaultingEventArgs e)
		{
			if (LeadCurrent.Current != null && LeadCurrent.Current.FullName != null)
			{
				e.NewValue = LeadCurrent.Current.FullName;
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
			PXUIFieldAttribute.SetEnabled<AccountsFilter.bAccountID>(sender, e.Row, !this.IsDimensionAutonumbered(CustomerAttribute.DimensionName));
		}
		#endregion

		#region Create Opportunity

		[PXViewName(Messages.CreateOpportunity)]
		public PXFilter<OpportunityFilter> OpportunityInfo;

		#endregion

		#region Ctors

		public LeadMaint()
		{
			PXUIFieldAttribute.SetRequired<Contact.status>(Lead.Cache, true);
			PXUIFieldAttribute.SetRequired<Contact.lastName>(Lead.Cache, true);

			PXUIFieldAttribute.SetDisplayName<BAccount.acctCD>(bAccountBasic.Cache, Messages.BAccountCD);
			PXUIFieldAttribute.SetDisplayName<BAccount.acctName>(bAccountBasic.Cache, Messages.BAccountName);

            PXUIFieldAttribute.SetEnabled<Contact.assignDate>(Lead.Cache, Lead.Cache.Current, false);

            Activities.GetNewEmailAddress =
				() =>
				{
					var contact = Lead.Current;
					return contact != null && !string.IsNullOrWhiteSpace(contact.EMail)
						? PXDBEmailAttribute.FormatAddressesWithSingleDisplayName(contact.EMail, contact.DisplayName)
						: String.Empty;
				};
			if (this.IsImport && !this.IsExport)
			{
				Lead.WhereNew<Where<Contact.contactType, Equal<ContactTypesAttribute.lead>>>();				
				Lead.OrderByNew<OrderBy<Asc<Contact.majorStatus, Desc<Contact.duplicateStatus, Asc<Contact.contactID>>>>>();
			}
		    PXUIFieldAttribute.SetVisible<CRMarketingListMember.format>(Subscriptions.Cache,null, false);
        }

		#endregion

		#region Actions

		public PXMenuAction<Contact> Action;

		public PXAction<Contact> convertToContact;
		[PXUIField(DisplayName = Messages.ConvertToContact)]
		[PXButton(ImageKey = PX.Web.UI.Sprite.Main.Process)]
		public virtual IEnumerable ConvertToContact(PXAdapter adapter)
		{
			Save.Press();
			List<Contact> contacts = new List<Contact>(adapter.Get().Cast<Contact>());
			foreach (Contact lead in contacts)
			{
				PXLongOperation.StartOperation(this, () =>
				{
                    
					ContactMaint graph = CreateInstance<ContactMaint>();
					lead.DuplicateFound = false;
					lead.DuplicateStatus = DuplicateStatusAttribute.NotValidated;
                    lead.QualificationDate = PXTimeZoneInfo.Now;
				    lead.ConvertedBy = Accessinfo.UserID;
					graph.Contact.Current = lead;
				    graph.Contact.Current.Status = LeadStatusesAttribute.Converted;
				    graph.Contact.Current.MajorStatus = LeadMajorStatusesAttribute._CONVERTED;

					graph.Contact.Cache.SetStatus(lead, PXEntryStatus.Updated);
					graph.Save.Press();
					
					if (adapter.ExternalCall && !this.IsContractBasedAPI)
						throw new PXRedirectRequiredException(graph, "Contact");
				});
			}
			return contacts;
		}

		public PXAction<Contact> convertToOpportunity;
		[PXUIField(DisplayName = Messages.ConvertToOpportunity)]
		[PXButton(ImageKey = PX.Web.UI.Sprite.Main.Process)]
		public virtual IEnumerable ConvertToOpportunity(PXAdapter adapter)
		{
			Save.Press();
			bool isOpportunityAutoNumberOn = this.IsNumberingAutonumbered(Setup.Current.OpportunityNumberingID);
			List<Contact> contacts = new List<Contact>(adapter.Get().Cast<Contact>());
			foreach (Contact l in contacts)
			{

				OpportunityMaint opportunityMaint = CreateInstance<OpportunityMaint>();
				CROpportunity opportunity = (CROpportunity)opportunityMaint.Opportunity.Cache.Insert();

				if (!isOpportunityAutoNumberOn)
				{
					if (OpportunityInfo.AskExt() != WebDialogResult.OK || !OpportunityInfo.VerifyRequired()) return contacts;
					CROpportunity existing = PXSelect<CROpportunity, Where<CROpportunity.opportunityID, Equal<Required<CROpportunity.opportunityID>>>>.SelectSingleBound(this, null, OpportunityInfo.Current.OpportunityID);
					if (existing != null)
					{
						OpportunityInfo.Cache.RaiseExceptionHandling<OpportunityFilter.opportunityID>(OpportunityInfo.Current, OpportunityInfo.Current.OpportunityID, new PXSetPropertyException(Messages.OpportunityAlreadyExists, OpportunityInfo.Current.OpportunityID));
						return contacts;
					}

					object cd = OpportunityInfo.Current.OpportunityID;
					opportunityMaint.Opportunity.Cache.RaiseFieldUpdating<CROpportunity.opportunityID>(null, ref cd);
					opportunity.OpportunityID = (string) cd;
				}
				Contact lead = l;
				PXLongOperation.StartOperation(this, delegate()
				{
					CRContactClass cls = PXSelect<CRContactClass, Where<CRContactClass.classID, Equal<Current<Contact.classID>>>>.SelectSingleBound(this, new object[] { lead });
					if (cls != null && cls.OwnerToOpportunity == true)
					{
						opportunity.WorkgroupID = lead.WorkgroupID;
						opportunity.OwnerID = lead.OwnerID;
					}
				    if (cls != null && cls.TargetOpportunityClassID != null)
				    {
				        opportunity.ClassID = cls.TargetOpportunityClassID;
				    }

					if (lead.BAccountID != null)					
						opportunity.BAccountID = lead.BAccountID;

					if(lead.ParentBAccountID != null)
						opportunity.ParentBAccountID = lead.ParentBAccountID;

				    if (lead.CampaignID != null)
				        opportunity.CampaignSourceID = lead.CampaignID;
					opportunity.ContactID = lead.ContactID;
					opportunity.ConvertedLeadID = lead.ContactID;
					opportunity.Subject = string.IsNullOrEmpty(lead.FullName) ? lead.DisplayName : lead.FullName;
					opportunity = (CROpportunity)opportunityMaint.Opportunity.Cache.Update(opportunity);

					ContactMaint contactGraph = CreateInstance<ContactMaint>();
					lead.ContactType = ContactTypesAttribute.Person;
                    lead.QualificationDate = PXTimeZoneInfo.Now;
                    lead.ConvertedBy = Accessinfo.UserID;
					lead = contactGraph.Contact.Update(lead);
					
					opportunityMaint.Opportunity.Search<CROpportunity.opportunityID>(opportunity.OpportunityID);
					lead = opportunityMaint.Leads.Update(lead);

                    // Copy Note text and Files references
                    CRSetup setup = PXSetupOptional<CRSetup>.Select(opportunityMaint);
					PXNoteAttribute.CopyNoteAndFiles(opportunityMaint.Leads.Cache, lead, opportunityMaint.OpportunityCurrent.Cache, opportunity, setup);
					
					if (!this.IsContractBasedAPI)
						throw new PXRedirectRequiredException(opportunityMaint, "Opportunity", true);
					
					opportunityMaint.Save.Press();
				});
			}
			return contacts;
		}

		public PXAction<Contact> convertToBAccount;
		[PXUIField(DisplayName = Messages.ConvertToBAccount, MapEnableRights = PXCacheRights.Update, MapViewRights = PXCacheRights.Select)]
		[PXButton(ImageKey = PX.Web.UI.Sprite.Main.Process)]
		public virtual IEnumerable ConvertToBAccount(PXAdapter adapter)
		{
			Save.Press();
			List<Contact> contacts = new List<Contact>(adapter.Get().Cast<Contact>());
			foreach (Contact l in contacts)
			{
				if (AccountInfo.AskExt((graph, view) => graph.Views[view].Cache.Clear(), true) != WebDialogResult.Yes) return contacts;
				bool empty_required = !AccountInfo.VerifyRequired();
				BAccount existing = PXSelect<BAccount, Where<BAccount.acctCD, Equal<Required<BAccount.acctCD>>>>.SelectSingleBound(this, null, AccountInfo.Current.BAccountID);
				if (existing != null)
				{
					AccountInfo.Cache.RaiseExceptionHandling<AccountsFilter.bAccountID>(AccountInfo.Current, AccountInfo.Current.BAccountID, new PXSetPropertyException(Messages.BAccountAlreadyExists, AccountInfo.Current.BAccountID));
					return contacts; 
				}
				if (empty_required) return contacts;

				Save.Press();
				Contact lead = l;
                lead.ConvertedBy = Accessinfo.UserID;
				PXLongOperation.StartOperation(this, () => ConvertToAccount(lead, AccountInfo.Current, this.IsContractBasedAPI));
			}
			return contacts;
		}

		public static void ConvertToAccount(Contact contact, AccountsFilter param, bool IsContractBasedAPI)
		{
			BusinessAccountMaint accountMaint = CreateInstance<BusinessAccountMaint>();
			object cd = param.BAccountID;
			accountMaint.BAccount.Cache.RaiseFieldUpdating<BAccount.acctCD>(null, ref cd);
			BAccount account = new BAccount
			{
				AcctCD = (string) cd,
				AcctName = param.AccountName,
				Type = BAccountType.ProspectType,
				ClassID = param.AccountClass,
                ParentBAccountID = contact.ParentBAccountID,
				CampaignSourceID = contact.CampaignID,
			};

			#region Set Contact and Address fields
			CRContactClass cls = PXSelect<CRContactClass, Where<CRContactClass.classID, Equal<Current<Contact.classID>>>>.SelectSingleBound(accountMaint, new object[] { contact });
			if (cls != null && cls.OwnerToBAccount == true)
			{
				account.WorkgroupID = contact.WorkgroupID;
				account.OwnerID = contact.OwnerID;
			}

			try
			{
				object newValue = account.OwnerID;
				accountMaint.BAccount.Cache.RaiseFieldVerifying<BAccount.ownerID>(account, ref newValue);
			}
			catch (PXSetPropertyException)
			{
				account.OwnerID = null;
			}
	
			account = accountMaint.BAccount.Insert(account);

			accountMaint.Answers.CopyAllAttributes(account, contact);

			Contact defContact = PXCache<Contact>.CreateCopy(PXSelect<Contact, Where<Contact.contactID, Equal<Current<BAccount.defContactID>>>>.SelectSingleBound(accountMaint, new object[] { account }));
		    var defContactNoteID = defContact.NoteID;
            PXCache<Contact>.RestoreCopy(defContact, contact);
			defContact.ContactType = ContactTypesAttribute.BAccountProperty;
			defContact.FullName = account.AcctName;
			defContact.ContactID = account.DefContactID;			
			defContact.BAccountID = account.BAccountID;
			defContact.DuplicateStatus = DuplicateStatusAttribute.NotValidated;
			defContact.DuplicateFound = false;
		    defContact.WorkgroupID = null;
		    defContact.OwnerID = null;
            defContact.ClassID = null;
            defContact.NoteID = defContactNoteID;
            defContact = accountMaint.DefContact.Update(defContact);

			Address contactAddress = PXSelect<Address, Where<Address.addressID, Equal<Required<Contact.defAddressID>>>>.Select(accountMaint, contact.DefAddressID );
			if (contactAddress == null)
				throw new PXException(Messages.DefAddressNotExists, contact.DisplayName);
			contactAddress.BAccountID = account.BAccountID;
			accountMaint.AddressCurrent.Cache.Clear();

			defContact.DefAddressID = contactAddress.AddressID;
			defContact = accountMaint.DefContact.Update(defContact);
			
			contactAddress = accountMaint.AddressCurrent.Update(contactAddress);

			account.DefAddressID = contactAddress.AddressID;
			accountMaint.BAccount.Update(account);

			contact.BAccountID = account.BAccountID;
			contact.DuplicateStatus = DuplicateStatusAttribute.NotValidated;
			contact.DuplicateFound = false;
            if(contact.QualificationDate == null)
		        contact.QualificationDate = PXTimeZoneInfo.Now;
			accountMaint.Contacts.Cache.SetStatus(contact, PXEntryStatus.Updated);
			CR.Location location = accountMaint.DefLocation.Select();
			location.DefAddressID = contactAddress.AddressID;
			accountMaint.DefLocation.Update(location);

			account.NoteID = PXNoteAttribute.GetNoteID<CRActivity.noteID>(accountMaint.CurrentBAccount.Cache, account);
			foreach (CRPMTimeActivity a in PXSelect<CRPMTimeActivity, Where<CRPMTimeActivity.contactID, Equal<Required<Contact.contactID>>>>.Select(accountMaint, contact.ContactID))
			{
				a.BAccountID = account.BAccountID;
				accountMaint.Activities.Cache.Update(a);
			}
			#endregion

			// Copy Note text and Files references
			CRSetup setup = PXSetupOptional<CRSetup>.Select(accountMaint);
			PXNoteAttribute.CopyNoteAndFiles(accountMaint.Contacts.Cache, contact, accountMaint.CurrentBAccount.Cache, account, setup);
			
			if (!IsContractBasedAPI)
				throw new PXRedirectRequiredException(accountMaint, "Business Account");
					
			accountMaint.Save.Press();
		}


		public PXAction<Contact> checkForDuplicates;
		[PXUIField(DisplayName = Messages.CheckForDuplicates)]
		[PXButton(ImageKey = PX.Web.UI.Sprite.Main.Process)]
		public virtual IEnumerable CheckForDuplicates(PXAdapter adapter)
		{
			foreach (Contact rec in adapter.Get())
			{
				Lead.Current = rec;
				Contact orig = rec;
				Contact lead = rec;

				if (lead.MajorStatus != LeadMajorStatusesAttribute._CLOSED && 
					(adapter.ExternalCall || rec.DuplicateStatus == DuplicateStatusAttribute.NotValidated))
				{
					lead = Lead.Cache.CreateCopy(Lead.Current) as Contact;
					lead.DuplicateStatus = DuplicateStatusAttribute.NotValidated;					
					Lead.Update(lead);

					Lead.Current.DuplicateFound = true;
					Duplicates.View.Clear();
					var result = Duplicates.Select();
					Lead.Current.DuplicateFound = (result != null && result.Count > 0);

					lead = Lead.Cache.CreateCopy(Lead.Current) as Contact;
					lead.DuplicateStatus = DuplicateStatusAttribute.Validated;

					Decimal? score = 0;
					foreach (PXResult<CRDuplicateRecord, Contact, CRLeadContactValidationProcess.Contact2> r in result)
					{
						CRLeadContactValidationProcess.Contact2 duplicate = r;
						CRDuplicateRecord contactScore = r;

						int duplicateWeight = ContactMaint.GetContactWeight(duplicate);
						int currentWeight = ContactMaint.GetContactWeight(Lead.Current);
						if (duplicateWeight > currentWeight ||
							(duplicateWeight == currentWeight &&
						     duplicate.ContactID < Lead.Current.ContactID))
						{
							lead.DuplicateStatus = DuplicateStatusAttribute.PossibleDuplicated;
							if (contactScore.Score > score)
								score = contactScore.Score;
						}
					}
					if(orig.DuplicateStatus != lead.DuplicateStatus)
						lead = Lead.Update(lead);

					if (Lead.Current.DuplicateFound == false && adapter.ExternalCall)
						Lead.Cache.RaiseExceptionHandling<Contact.duplicateStatus>(lead, null,
						                                                           new PXSetPropertyException(
																					   Messages.NoPossibleDuplicates, 
                                                                                       PXErrorLevel.RowInfo));
				}				
				yield return lead;
			}			

			if(Lead.Cache.IsDirty)			
				Save.Press();			
		}	
		
		#endregion

		#region Event Handlers

		#region Contact

		[PXUIField(DisplayName = "Lead ID", Visibility = PXUIVisibility.Invisible)]
		[LeadSelector]
		[PXMergeAttributes(Method = MergeMethod.Merge)]
		public virtual void Contact_ContactID_CacheAttached(PXCache sender) {}

		[PXDefault(ContactTypesAttribute.Lead)]
		[PXMergeAttributes(Method = MergeMethod.Merge)]
		public virtual void Contact_ContactType_CacheAttached(PXCache sender) {}

		[PXRestrictor(typeof(Where<BAccount.type, Equal<BAccountType.prospectType>,
			Or<BAccount.type, Equal<BAccountType.customerType>,
			Or<BAccount.type, Equal<BAccountType.combinedType>>>>), Messages.CannotAttachLeadTo, typeof(BAccountR.type))]
		[CustomerAndProspect(DisplayName = "Business Account", Visibility = PXUIVisibility.SelectorVisible)]
		public virtual void Contact_BAccountID_CacheAttached(PXCache sender) {}

		[PXDefault(LeadMajorStatusesAttribute._JUST_CREATED, PersistingCheck = PXPersistingCheck.Nothing)]
		[PXUIField(Visible = true)]
		[PXMergeAttributes(Method = MergeMethod.Merge)]
		public virtual void Contact_MajorStatus_CacheAttached(PXCache sender) {}

		[PXUIField(DisplayName = "Status", Visibility = PXUIVisibility.SelectorVisible)]
		[PXDefault(LeadStatusesAttribute.New)]
		[PXUIRequired(typeof(Where<Contact.contactType, Equal<ContactTypesAttribute.lead>>))]
		[PXMergeAttributes(Method = MergeMethod.Merge)]
		public virtual void Contact_Status_CacheAttached(PXCache sender) {}

		[PXUIField(DisplayName = "Reason", Visibility = PXUIVisibility.SelectorVisible)]
		[CRDropDownAutoValue(typeof(Contact.status))]
		[PXMergeAttributes(Method = MergeMethod.Merge)]
		public virtual void Contact_Resolution_CacheAttached(PXCache sender) {}

		[PXUIField(DisplayName = "Company Name", Visibility = PXUIVisibility.SelectorVisible)]
		[PXMassMergableField]
		[CRLeadFullName(typeof(Contact.bAccountID))]
		[PXMergeAttributes(Method = MergeMethod.Replace)]
		public virtual void Contact_FullName_CacheAttached(PXCache sender) {}

		[PXUIField(DisplayName = "Lead Class")]
		[PXDefault(typeof(Search<CRSetup.defaultLeadClassID>), PersistingCheck = PXPersistingCheck.Nothing)]
		[PXMergeAttributes(Method = MergeMethod.Merge)]
		public virtual void Contact_ClassID_CacheAttached(PXCache sender) { }

        
        protected virtual void Contact_RowSelected(PXCache sender, PXRowSelectedEventArgs e)
		{
			Contact row = e.Row as Contact;
			if (row == null) return;
            PXUIFieldAttribute.SetEnabled<Contact.contactID>(sender, row, true);

			CRContactClass leadClass = row.ClassID.
				With(_ => (CRContactClass)PXSelectReadonly<CRContactClass,
					Where<CRContactClass.classID, Equal<Required<CRContactClass.classID>>>>.
					Select(this, _));
			if (leadClass != null)
			{
				Activities.DefaultEMailAccountId = leadClass.DefaultEMailAccountID;
			}

			if (row.DuplicateStatus == DuplicateStatusAttribute.PossibleDuplicated || row.DuplicateFound == true)
			{
				sender.RaiseExceptionHandling<Contact.duplicateStatus>(row, 
					null, new PXSetPropertyException(Messages.LeadHavePossibleDuplicates, PXErrorLevel.Warning, row.ContactID));
			}
		}

		  protected virtual void Contact_RowPersisting(PXCache sender, PXRowPersistingEventArgs e)
		  {
			  Contact row = e.Row as Contact;
			  if (row != null && IsImport && !IsExport)
			  {
				  bool needupdate = false;
				  foreach (var field in Caches[typeof(Contact)].Fields)
				  {
					  object oldfieldvalue = Caches[typeof(Contact)].GetValueOriginal(row, field);
					  object newfieldvalue = Caches[typeof(Contact)].GetValue(row, field);
					  if (!Equals(oldfieldvalue, newfieldvalue))
					  {
						  needupdate = true;
						  break;
					  }
				  }
				  if (!needupdate)
					  e.Cancel = true;
			  }

		  }

        protected virtual void Contact_RowPersisted(PXCache sender, PXRowPersistedEventArgs e)
		{
			Contact row = e.Row as Contact;

            if (row == null || e.TranStatus != PXTranStatus.Open) return;
			if (CRGrammProcess.PersistGrams(this, row))
				row.DuplicateStatus = DuplicateStatusAttribute.NotValidated;
		}

		protected virtual void Contact_FullName_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
		{
			Contact row = (Contact) e.Row;
			if (row != null)
			{
				AccountInfo.Cache.SetDefaultExt<AccountsFilter.accountName>(AccountInfo.Current);
			}
		}

		public override void Persist()
		{
			base.Persist();
			if (PXAccess.FeatureInstalled<FeaturesSet.contactDuplicate>() && Setup.Current.ValidateContactDuplicatesOnEntry == true)
			{
				checkForDuplicates.Press();				
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
        #endregion

        #region CRDuplicateRecord

        protected virtual void CRDuplicateRecord_RowUpdated(PXCache sender, PXRowUpdatedEventArgs e)
		{
			sender.IsDirty = false;
		}

        #endregion

        #region CRPMTimeActivity

        [PXDBChildIdentity(typeof(Contact.contactID))]
        [PXMergeAttributes(Method = MergeMethod.Merge)]
        protected virtual void CRPMTimeActivity_ContactID_CacheAttached(PXCache sender) { }

        #endregion

        #endregion

        #region Protected Methods

        protected bool CanBeMerged(Contact row)
		{
			return row.QualificationDate == null;
		}

		#endregion

		#region Private Methods
		
		#endregion
	}

	public static class PXAutonumberingInfo
	{
		public static bool IsDimensionAutonumbered(this PXGraph graph, string dimension)
		{
			return PXSelect<Segment, Where<Segment.dimensionID, Equal<Required<Segment.dimensionID>>>>.Select(graph, dimension)
				.RowCast<Segment>()
				.Any(segment => segment.AutoNumber == true);
		}

		public static bool IsNumberingAutonumbered(this PXGraph graph, string numbering)
		{
			return PXSelect<Numbering, Where<Numbering.numberingID, Equal<Required<Numbering.numberingID>>>>.Select(graph, numbering)
				.RowCast<Numbering>()
				.Any(n => n.UserNumbering != true);
		}

		public static string GetDimensionAutonumberingNewValue(this PXGraph graph, string dimension)
		{
			Numbering n = (PXResult<Dimension, Numbering>)PXSelectJoin<Dimension,
				LeftJoin<Numbering, On<Dimension.numberingID, Equal<Numbering.numberingID>>>,
				Where<Dimension.dimensionID, Equal<Required<Dimension.dimensionID>>,
					And<Numbering.userNumbering, NotEqual<True>>>>
				.SelectSingleBound(graph, null, dimension);

			return n.With(_ => _.NewSymbol) ?? Messages.New;
		}
	}
}
