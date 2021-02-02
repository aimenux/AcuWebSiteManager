using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using PX.Common;
using PX.Objects.AR;
using PX.Objects.CR.MassProcess;
using PX.Objects.CR.Extensions.CRCreateActions;
using PX.Objects.CS;
using PX.Objects.GL;
using PX.Data;
using PX.Data.BQL;
using PX.Data.BQL.Fluent;
using PX.Data.EP;
using PX.Objects.IN;
using PX.SM;
using PX.TM;
using PX.Data.MassProcess;
using PX.Objects.CR.Extensions;
using PX.Objects.CR.Extensions.CRDuplicateEntities;
using PX.Objects.CR.Extensions.CRContactAccountDataSync;

namespace PX.Objects.CR
{
    public class LeadMaint : PXGraph<LeadMaint, CRLead, CRLead.displayName>
	{
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

		[PXViewName(Messages.Address)]
		public AddressSelect<CRLead.defAddressID, CRLead.isAddressSameAsMain, CRLead.bAccountID>
			AddressCurrent;

		[PXViewName(Messages.Lead)]
		[PXCopyPasteHiddenFields(typeof(CRLead.status), typeof(CRLead.resolution), typeof(CRLead.duplicateStatus), typeof(CRLead.duplicateFound))]
		public PXSelect<CRLead,
			Where<CRLead.contactType, Equal<ContactTypesAttribute.lead>>>
			Lead;

		[PXHidden]
		public PXSelect<CRLead,
			Where<CRLead.contactID, Equal<Current<CRLead.contactID>>>>
			LeadCurrent;

		[PXCopyPasteHiddenView]
		public PXSelect<CRActivityStatistics,
				Where<CRActivityStatistics.noteID, Equal<Current<CRLead.noteID>>>>
			LeadActivityStatistics;

		[PXHidden]
		public PXSelect<CRLead,
			Where<CRLead.contactID, Equal<Current<CRLead.contactID>>>>
			LeadCurrent2;

		[PXViewName(Messages.Answers)]
		public CRAttributeList<CRLead>
			Answers;

		[PXViewName(Messages.Activities)]
		[PXFilterable]
		[CRDefaultMailTo]
		[CRReference(typeof(CRLead.bAccountID), typeof(CRLead.refContactID))]
		public LeadActivities
			Activities;
		
		[PXCopyPasteHiddenView]
		[PXViewName(Messages.Relations)]
		[PXFilterable]
		public CRRelationsList<CRLead.noteID>
			Relations;

		[PXViewName(Messages.Opportunities)]
		[PXCopyPasteHiddenView]
		public SelectFrom<CROpportunity>
					.InnerJoin<CRLead>
						.On<CROpportunity.leadID.IsEqual<CRLead.noteID>>
					.LeftJoin<CROpportunityClass>
						.On<CROpportunityClass.cROpportunityClassID.IsEqual<CROpportunity.classID>>
					.Where<
						CRLead.contactID.IsEqual<CRLead.contactID.FromCurrent>
						.And<CRLead.contactType.IsEqual<ContactTypesAttribute.lead>>
					>
				.View
			Opportunities;

		[PXViewName(Messages.CampaignMember)]
		[PXFilterable]
		[PXViewDetailsButton(typeof(CRLead),
			typeof(Select<CRCampaign,
				Where<CRCampaign.campaignID, Equal<Current<CRCampaignMembers.campaignID>>>>))]
		public PXSelectJoin<CRCampaignMembers,
			InnerJoin<CRCampaign, On<CRCampaignMembers.campaignID, Equal<CRCampaign.campaignID>>>,
			Where<CRCampaignMembers.contactID, Equal<Current<CRLead.contactID>>>>
			Members;
		
		[PXHidden]
		public PXSelect<CRMarketingListMember>
			Subscriptions_stub;

		[PXViewName(Messages.Subscriptions)]
		[PXFilterable]
		[PXViewDetailsButton(typeof(CRLead),
			typeof(Select<CRMarketingList,
				Where<CRMarketingList.marketingListID, Equal<Current<CRMarketingListMember.marketingListID>>>>))]
		public CRMMarketingContactSubscriptions<CRLead, CRLead.contactID>
			Subscriptions;

		[PXHidden]
		public PXSelectReadonly<CRLeadClass, Where<CRLeadClass.classID, Equal<Current<CRLead.classID>>>>
			LeadClass;

		#endregion

		#region Delegates

		#endregion

		#region Ctors

		public LeadMaint()
		{
			PXUIFieldAttribute.SetDisplayName<BAccountCRM.acctCD>(bAccountBasic.Cache, Messages.BAccountCD);

            PXUIFieldAttribute.SetEnabled<CRLead.assignDate>(Lead.Cache, Lead.Cache.Current, false);

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
				Lead.WhereNew<Where<CRLead.contactType, Equal<ContactTypesAttribute.lead>>>();
				Lead.OrderByNew<OrderBy<Desc<CRLead.isActive, Desc<CRLead.duplicateStatus, Asc<CRLead.contactID>>>>>();
			}
			PXUIFieldAttribute.SetVisible<CRMarketingListMember.format>(Subscriptions.Cache,null, false);

			PXUIFieldAttribute.SetVisible<Contact.languageID>(LeadCurrent.Cache, null, PXDBLocalizableStringAttribute.HasMultipleLocales);
		}

		#endregion

		#region Actions

		public PXMenuAction<CRLead> Action;

		#endregion

		#region Event Handlers

		#region Lead

		protected virtual void _(Events.RowSelected<CRLead> e)
		{
			CRLead row = e.Row as CRLead;
			if (row == null) return;
            PXUIFieldAttribute.SetEnabled<CRLead.contactID>(e.Cache, row, true);
			ConfigureAddressSectionUI();

			CRLeadClass leadClass = row.ClassID.
				With(_ => (CRLeadClass)PXSelectReadonly<CRLeadClass,
					Where<CRLeadClass.classID, Equal<Required<CRLeadClass.classID>>>>.
					Select(this, _));
			if (leadClass != null)
			{
				Activities.DefaultEMailAccountId = leadClass.DefaultEMailAccountID;
			}
		}

		protected virtual void _(Events.RowPersisting<CRLead> e)
		{
			CRLead row = e.Row as CRLead;
			if (row != null && IsImport && !IsExport)
			{
				bool needupdate = false;
				foreach (var field in Caches[typeof(CRLead)].Fields)
				{
					object oldfieldvalue = Caches[typeof(CRLead)].GetValueOriginal(row, field);
					object newfieldvalue = Caches[typeof(CRLead)].GetValue(row, field);
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

		#endregion

		#region Address

		protected virtual void _(Events.RowSelected<Address> e)
		{
			Address row = e.Row as Address;
			if (row == null) return;

			PXUIFieldAttribute.SetEnabled<Address.isValidated>(e.Cache, row, false);
			ConfigureAddressSectionUI();
		}

		[PopupMessage]
		[PXSelector(typeof(
			SelectFrom<BAccountCRM>
			.LeftJoin<Contact>
				.On<Contact.contactID.IsEqual<BAccountCRM.defContactID>>
			.LeftJoin<Address>
				.On<Address.addressID.IsEqual<BAccountCRM.defAddressID>>
			.SearchFor<BAccountCRM.bAccountID>),
			fieldList: new[]
			{
				typeof(BAccountCRM.bAccountID),
				typeof(BAccountCRM.acctCD),
				typeof(BAccountCRM.acctName),
				typeof(BAccountCRM.type),
				typeof(Contact.phone1),
				typeof(Address.city),
				typeof(Address.state),
				typeof(BAccountCRM.status)
			}, 
			SubstituteKey = typeof(BAccountCRM.acctCD),
			DescriptionField = typeof(BAccountCRM.acctName),
			DirtyRead = true)]
		[PXMergeAttributes(Method = MergeMethod.Merge)]
		protected virtual void _(Events.CacheAttached<CRLead.bAccountID> e) { }
		#endregion

		#region CRCampaignMembers

		[PXDBLiteDefault(typeof(CRLead.contactID))]
		[PXMergeAttributes(Method = MergeMethod.Merge)]
		protected virtual void _(Events.CacheAttached<CRCampaignMembers.contactID> e) { }

		#endregion

		#region CRMarketingListMember

		[PXSelector(typeof(Search<CRMarketingList.marketingListID,
			Where<CRMarketingList.isDynamic, IsNull, Or<CRMarketingList.isDynamic, NotEqual<True>>>>),
			DescriptionField = typeof(CRMarketingList.mailListCode))]
		[PXMergeAttributes(Method = MergeMethod.Merge)]
		protected virtual void _(Events.CacheAttached<CRMarketingListMember.marketingListID> e) { }

		[PXDBLiteDefault(typeof(CRLead.contactID))]
		[PXDBInt(IsKey = true)]
		[PXUIField(DisplayName = "Name")]
		[PXSelector(typeof(Search<CRLead.contactID>),
			typeof(CRLead.fullName),
			typeof(CRLead.displayName),
			typeof(CRLead.eMail),
			typeof(CRLead.phone1),
			typeof(CRLead.bAccountID),
			typeof(CRLead.salutation),
			typeof(CRLead.contactType),
			typeof(CRLead.isActive),
			typeof(CRLead.memberName),
			DescriptionField = typeof(CRLead.memberName),
			Filterable = true,
			DirtyRead = true)]
		[PXParent(typeof(Select<CRLead, Where<CRLead.contactID, Equal<Current<CRMarketingListMember.contactID>>>>))]
		[PXMergeAttributes(Method = MergeMethod.Replace)]
		protected virtual void _(Events.CacheAttached<CRMarketingListMember.contactID> e) { }

		#endregion

		#region CRPMTimeActivity

		[PXDBChildIdentity(typeof(CRLead.contactID))]
		[PXMergeAttributes(Method = MergeMethod.Merge)]
		protected virtual void _(Events.CacheAttached<CRPMTimeActivity.contactID> e) { }

		[PopupMessage]
		[PXDBDefault(typeof(CRLead.bAccountID), PersistingCheck = PXPersistingCheck.Nothing)]
		[PXMergeAttributes(Method = MergeMethod.Merge)]
		protected virtual void _(Events.CacheAttached<CRPMTimeActivity.bAccountID> e) { }

		#endregion

		private void ConfigureAddressSectionUI()
		{
			PXCache leadCache = Caches[typeof(CRLead)];
			CRLead thisLead = leadCache.Current as CRLead;

			PXCache addressCache = Caches[typeof(Address)];
			Address thisAddress = addressCache.Current as Address;

			if (thisLead == null || thisAddress == null) return;

			string warningAboutAccountAddressChange = "";
			bool addressEnabled;
			if (thisLead.OverrideRefContact == true || thisLead.RefContactID == null)
			{
				addressEnabled = true;
			}
			else
			{
				var results = SelectFrom<Contact>
					.LeftJoin<BAccount>
						.On<BAccount.bAccountID.IsEqual<Contact.bAccountID>>
					.Where<
						Contact.contactID.IsEqual<CRLead.refContactID.FromCurrent>
					>.View.ReadOnly.SelectSingleBound(this, new object[] { });

				BAccount refContactAccount = null;
				Contact refContact = null;

				foreach (PXResult<Contact, BAccount> result in results)
				{
					refContact = (Contact)result;
					refContactAccount = (BAccount)result;
				}

				bool refContactAccountIsProspect = refContactAccount != null && refContactAccount.Type == BAccountType.ProspectType;
				bool refContactAddressLinkedToAccount = refContact != null && refContactAccount != null && refContact.DefAddressID == refContactAccount.DefAddressID;

				addressEnabled = refContactAccount == null || refContactAccountIsProspect || !refContactAddressLinkedToAccount;
				warningAboutAccountAddressChange = refContactAccountIsProspect && refContactAddressLinkedToAccount ? Messages.WarningAboutAccountAddressChange : "";
			}

			PXUIFieldAttribute.SetWarning<CRLead.overrideRefContact>(Caches[typeof(CRLead)], thisLead, warningAboutAccountAddressChange);

			PXUIFieldAttribute.SetEnabled<Address.addressLine1>(Caches[typeof(Address)], thisAddress, addressEnabled);
			PXUIFieldAttribute.SetEnabled<Address.addressLine2>(Caches[typeof(Address)], thisAddress, addressEnabled);
			PXUIFieldAttribute.SetEnabled<Address.city>(Caches[typeof(Address)], thisAddress, addressEnabled);
			PXUIFieldAttribute.SetEnabled<Address.state>(Caches[typeof(Address)], thisAddress, addressEnabled);
			PXUIFieldAttribute.SetEnabled<Address.postalCode>(Caches[typeof(Address)], thisAddress, addressEnabled);
			PXUIFieldAttribute.SetEnabled<Address.countryID>(Caches[typeof(Address)], thisAddress, addressEnabled);
		}


		#endregion

		#region Extensions

		/// <exclude/>
		public class DefaultLeadOwnerGraphExt : CRDefaultDocumentOwner<
			LeadMaint, CRLead,
			CRLead.classID, CRLead.ownerID, CRLead.workgroupID>
		{ }

		/// <exclude/>
		public class CRDuplicateEntitiesForLeadGraphExt : CRDuplicateContactEntities<LeadMaint, CRLead>
		{
			#region Initialization 

			protected override Type AdditionalConditions => typeof(

				Brackets<
					// do not show contact, that is currently attached to the Lead
					DuplicateDocument.refContactID.FromCurrent.IsNotNull
					.And<CRDuplicateGrams.entityID.IsNotEqual<DuplicateDocument.refContactID.FromCurrent>>
					.Or<DuplicateDocument.refContactID.FromCurrent.IsNull>
				>
				.And<Brackets<
					// do not show BA, that is currently attached to the Lead
					DuplicateDocument.bAccountID.FromCurrent.IsNotNull
					.And<Brackets<
						DuplicateContact.contactType.IsEqual<ContactTypesAttribute.bAccountProperty>
							.And<BAccountR.bAccountID.IsNotEqual<DuplicateDocument.bAccountID.FromCurrent>>
						.Or<DuplicateContact.contactType.IsNotEqual<ContactTypesAttribute.bAccountProperty>>
					>>
					.Or<DuplicateDocument.bAccountID.FromCurrent.IsNull>
				>>
				.And<
					DuplicateContact.isActive.IsEqual<True>.And<DuplicateContact.contactType.IsNotEqual<ContactTypesAttribute.bAccountProperty>>
					.Or<BAccountR.status.IsNotEqual<BAccountR.status.inactive>>
				>
				);

			protected override string WarningMessage => Messages.LeadHavePossibleDuplicates;

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
				return new DocumentMapping(typeof(CRLead)) { Key = typeof(CRLead.contactID) };
			}

			protected override DuplicateDocumentMapping GetDuplicateDocumentMapping()
			{
				return new DuplicateDocumentMapping(typeof(CRLead)) { Email = typeof(CRLead.eMail) };
			}

			#endregion

			#region Events

			protected virtual void _(Events.FieldUpdated<CRLead, CRLead.isActive> e)
			{
				CRLead row = e.Row as CRLead;
				if (e.Row == null)
					return;

				if (row.IsActive == true && row.IsActive != (bool?)e.OldValue)
				{
					row.DuplicateStatus = DuplicateStatusAttribute.NotValidated;
				}
			}

			[CRDuplicateLeadsSelector(typeof(MergeParams.sourceEntityID), SelectorMode = PXSelectorMode.DisplayModeText)]
			[PXMergeAttributes(Method = MergeMethod.Append)]
			protected virtual void _(Events.CacheAttached<MergeParams.targetEntityID> e) { }

			public virtual void _(Events.FieldSelecting<CRDuplicateRecord, CRDuplicateRecord.selected> e)
			{
				var doc = DuplicateDocuments.Current;

				if (e.Row == null || doc == null)
					return;

				bool isOfSameType = e.Row.DuplicateContactType == ContactTypesAttribute.Lead;
				bool isOfSameParent =
					(doc.RefContactID == null || e.Row.DuplicateRefContactID == null || e.Row.DuplicateRefContactID == doc.RefContactID)
					&& (doc.BAccountID == null || e.Row.DuplicateBAccountID == null || e.Row.DuplicateBAccountID == doc.BAccountID);

				e.ReturnState = PXFieldState.CreateInstance(e.ReturnState, null, null, null, null, null, null, null, null, null, null,
					!isOfSameType || isOfSameParent
						? null
						: Messages.LeadBAccountDiff,
					!isOfSameType || isOfSameParent
						? PXErrorLevel.Undefined
						: PXErrorLevel.RowWarning,
					isOfSameType && isOfSameParent,
					null, null, PXUIVisibility.Undefined, null, null, null);
			}

			#endregion

			#region Overrides

			protected override CRLead GetTargetEntity(int targetID)
			{
				return PXSelect<CRLead, Where<CRLead.contactID, Equal<Required<CRLead.contactID>>>>.Select(Base, targetID);
			}

			protected override Contact GetTargetContact(CRLead targetEntity)
			{
				return targetEntity as Contact;
			}

			protected override Address GetTargetAddress(CRLead targetEntity)
			{
				return PXSelect<Address, Where<Address.addressID, Equal<Required<Address.addressID>>>>.Select(Base, targetEntity.DefAddressID);
			}
			protected override void MergeEntities(PXGraph targetGraph, CRLead targetLead, CRLead duplicateLead)
			{
				PXCache Activities = targetGraph.Caches[typeof(CRPMTimeActivity)];
				foreach (CRPMTimeActivity activity in PXSelect<CRPMTimeActivity, Where<CRPMTimeActivity.refNoteID, Equal<Current<CRLead.noteID>>>>
					.SelectMultiBound(targetGraph, new object[] { duplicateLead })
					.RowCast<CRPMTimeActivity>()
					.Select(cas => (CRPMTimeActivity)Activities.CreateCopy(cas)))
				{
					activity.RefNoteID = targetLead.NoteID;
					activity.ContactID = targetLead.RefContactID;
					activity.BAccountID = targetLead.BAccountID;

					Activities.Update(activity);
				}

				PXCache Opportunities = targetGraph.Caches[typeof(CROpportunity)];
				foreach (CROpportunity opp in PXSelect<CROpportunity,Where<CROpportunity.leadID, Equal<Current<CRLead.noteID>>>>
					.SelectMultiBound(targetGraph, new object[] { duplicateLead })
					.RowCast<CROpportunity>()
					.Select(opp => (CROpportunity)Opportunities.CreateCopy(opp)))
				{
					if (targetLead.BAccountID != opp.BAccountID)
					{
						throw new PXException(Messages.ContactBAccountForOpp, duplicateLead.DisplayName, duplicateLead.ContactID, opp.OpportunityID);
					}

					opp.ContactID = targetLead.ContactID;

					Opportunities.Update(opp);
				}

				PXCache Relations = targetGraph.Caches[typeof(CRRelation)];
				foreach (CRRelation rel in PXSelectJoin<CRRelation,
						LeftJoin<CRRelation2, On<CRRelation.entityID, Equal<CRRelation2.entityID>,
							And<CRRelation.role, Equal<CRRelation2.role>,
								And<CRRelation2.refNoteID, Equal<Required<CRLead.noteID>>>>>>,
						Where<CRRelation2.entityID, IsNull,
							And<CRRelation.refNoteID, Equal<Required<CRLead.noteID>>>>>
					.Select(targetGraph, targetLead.NoteID, duplicateLead.NoteID)
					.RowCast<CRRelation>()
					.Select(rel => (CRRelation)Relations.CreateCopy(rel)))
				{
					rel.RelationID = null;
					rel.RefNoteID = targetLead.NoteID;

					Relations.Insert(rel);
				}

				PXCache Subscriptions = targetGraph.Caches[typeof(CRMarketingListMember)];
				foreach (CRMarketingListMember mmember in PXSelectJoin<CRMarketingListMember,
						LeftJoin<CRMarketingListMember2, On<CRMarketingListMember.marketingListID, Equal<CRMarketingListMember2.marketingListID>,
							And<CRMarketingListMember2.contactID, Equal<Required<CRLead.contactID>>>>>,
						Where<CRMarketingListMember.contactID, Equal<Required<CRLead.contactID>>,
							And<CRMarketingListMember2.marketingListID, IsNull>>>
					.Select(targetGraph, targetLead.ContactID, duplicateLead.ContactID)
					.RowCast<CRMarketingListMember>()
					.Select(mmember => (CRMarketingListMember)Subscriptions.CreateCopy(mmember)))
				{
					mmember.ContactID = targetLead.ContactID;

					Subscriptions.Insert(mmember);
				}

				PXCache Members = targetGraph.Caches[typeof(CRCampaignMembers)];
				foreach (CRCampaignMembers cmember in PXSelectJoin<CRCampaignMembers,
						LeftJoin<CRCampaignMembers2, On<CRCampaignMembers.campaignID, Equal<CRCampaignMembers2.campaignID>,
							And<CRCampaignMembers2.contactID, Equal<Required<CRLead.contactID>>>>>,
						Where<CRCampaignMembers2.campaignID, IsNull,
							And<CRCampaignMembers.contactID, Equal<Required<CRLead.contactID>>>>>
					.Select(targetGraph, targetLead.ContactID, duplicateLead.ContactID)
					.RowCast<CRCampaignMembers>()
					.Select(cmember => (CRCampaignMembers)Members.CreateCopy(cmember)))
				{
					cmember.ContactID = targetLead.ContactID;

					Members.Insert(cmember);
				}

				PXCache NWatchers = targetGraph.Caches[typeof(ContactNotification)];
				foreach (ContactNotification watcher in PXSelectJoin<ContactNotification,
						LeftJoin<ContactNotification2, On<ContactNotification.setupID, Equal<ContactNotification2.setupID>,
							And<ContactNotification2.contactID, Equal<Required<CRLead.contactID>>>>>,
						Where<ContactNotification2.setupID, IsNull,
							And<ContactNotification.contactID, Equal<Required<CRLead.contactID>>>>>
					.Select(targetGraph, targetLead.ContactID, duplicateLead.ContactID)
					.RowCast<ContactNotification>()
					.Select(watcher => (ContactNotification)NWatchers.CreateCopy(watcher)))
				{
					watcher.NotificationID = null;
					watcher.ContactID = targetLead.ContactID;

					NWatchers.Insert(watcher);
				}
			}

			public override void GetAllProperties(List<FieldValue> values, HashSet<string> fieldNames)
			{
				int order = 0;

				values.AddRange(GetMarkedPropertiesOf<CRLead>(Base, ref order).Where(fld => fieldNames.Add(fld.Name)));

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

				if (duplicate.ContactType == ContactTypesAttribute.BAccountProperty)
				{
					DuplicateDocuments.Cache.SetValue<DuplicateDocument.refContactID>(duplicateDocument, null);
					DuplicateDocuments.Cache.SetValueExt<DuplicateDocument.bAccountID>(duplicateDocument, duplicate.BAccountID);
				}
				else if (duplicate.ContactType == ContactTypesAttribute.Person)
				{
					DuplicateDocuments.Cache.SetValue<DuplicateDocument.bAccountID>(duplicateDocument, duplicate.BAccountID);
					DuplicateDocuments.Cache.SetValueExt<DuplicateDocument.refContactID>(duplicateDocument, duplicate.ContactID);
				}
				else
				{
					throw new PXException(Messages.CanAttachToContactOrBAccount);
				}

				DuplicateDocuments.Update(duplicateDocument);
			}

			protected override void ValidateEntitiesBeforeMerge(List<CRLead> duplicateEntities)
			{
				int? firstRefContactID = null;
				foreach (CRLead lead in duplicateEntities)
				{
					if (lead.RefContactID != null)
					{
						if (firstRefContactID == null)
						{
							firstRefContactID = lead.RefContactID;
						}
						else if (firstRefContactID != lead.RefContactID)
						{
							throw new PXException(Messages.DuplicatesMergeProhibitedDueToDifferentContacts);
						}
					}
				}
			}

			#endregion
		}

		/// <exclude/>
		public class ContactAccountDataReplacementGraphExt : CRContactAccountDataSync<LeadMaint>
		{
			public override void Initialize()
			{
				base.Initialize();

				Addresses = new PXSelectExtension<CR.Extensions.CRContactAccountDataSync.DocumentAddress>(Base.AddressCurrent);
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
		public class CreateAccountFromLeadGraphExt : CRCreateAccountAction<LeadMaint, CRLead>
		{
			protected override string TargetType => CRTargetEntityType.Lead;

			public override void Initialize()
			{
				base.Initialize();

				Addresses = new PXSelectExtension<CR.Extensions.CRCreateActions.DocumentAddress>(Base.AddressCurrent);
			}

			protected override DocumentContactMapping GetDocumentContactMapping()
			{
				return new DocumentContactMapping(typeof(CRLead)) { Email = typeof(CRLead.eMail) };
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

				CRLead lead = Base.Lead.Current;
				if (lead == null) return;

				CRLeadClass cls = PXSelect<
						CRLeadClass,
					Where<
						CRLeadClass.classID,
						Equal<Required<CRLead.classID>>>>
					.Select(Base, lead.ClassID);

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
		}

		/// <exclude/>
		public class CreateContactFromLeadGraphExt : CRCreateContactAction<LeadMaint, CRLead>
		{
			protected override string TargetType => CRTargetEntityType.Lead;

			public override void Initialize()
			{
				base.Initialize();

				Addresses = new PXSelectExtension<CR.Extensions.CRCreateActions.DocumentAddress>(Base.AddressCurrent);
			}

			protected override DocumentContactMapping GetDocumentContactMapping()
			{
				return new DocumentContactMapping(typeof(CRLead)) { Email = typeof(CRLead.eMail) };
			}
			protected override DocumentContactMethodMapping GetDocumentContactMethodMapping()
			{
				return new DocumentContactMethodMapping(typeof(CRLead));
			}
			protected override DocumentAddressMapping GetDocumentAddressMapping()
			{
				return new DocumentAddressMapping(typeof(Address));
			}

			protected override PXSelectBase<CRPMTimeActivity> Activities => Base.Activities;

			protected virtual void _(Events.FieldDefaulting<ContactFilter, ContactFilter.contactClass> e)
			{
				if (ExistingContact.SelectSingle() is Contact existingContact)
				{
					e.NewValue = existingContact.ClassID;
					e.Cancel = true;
					return;
				}

				CRLead lead = Base.Lead.Current;
				if (lead == null) return;

				CRLeadClass cls = PXSelect<
						CRLeadClass,
						Where<
							CRLeadClass.classID,
							Equal<Required<CRLead.classID>>>>
					.Select(Base, lead.ClassID);

				if (cls?.TargetContactClassID != null)
				{
					e.NewValue = cls.TargetContactClassID;
				}
				else
				{
					e.NewValue = Base.Setup.Current?.DefaultContactClassID;
				}

				e.Cancel = true;
			}
		}

		/// <exclude/>
		public class CreateOpportunityFromLeadGraphExt : CRCreateOpportunityAction<LeadMaint, CRLead>
		{
			protected override string TargetType => CRTargetEntityType.Lead;

			public override void Initialize()
			{
				base.Initialize();

				Addresses = new PXSelectExtension<CR.Extensions.CRCreateActions.DocumentAddress>(Base.AddressCurrent);
			}

			protected override DocumentContactMapping GetDocumentContactMapping()
			{
				return new DocumentContactMapping(typeof(CRLead)) { Email = typeof(CRLead.eMail) };
			}
			protected override DocumentAddressMapping GetDocumentAddressMapping()
			{
				return new DocumentAddressMapping(typeof(Address));
			}

			protected override PXSelectBase<CRPMTimeActivity> Activities => Base.Activities;

			public virtual void _(Events.FieldDefaulting<OpportunityFilter, OpportunityFilter.opportunityClass> e)
			{
				e.NewValue = Base
					.LeadClass
					.SelectSingle()
					?.TargetOpportunityClassID is string oppCls
						? oppCls
						: Base.Setup.Current?.DefaultOpportunityClassID;
				e.Cancel = true;
			}

			protected override CROpportunity CreateMaster(OpportunityMaint graph, OpportunityConversionOptions options)
			{
				var opp =  base.CreateMaster(graph, options);
				if (Base.LeadClass.SelectSingle()?.TargetOpportunityStage is string stage)
				{
					opp.StageID = stage;
				}
				return graph.Opportunity.Update(opp);
			}
		}

		/// <exclude/>
		public class CreateBothAccountAndContactFromLeadGraphExt : CRCreateBothContactAndAccountAction<LeadMaint, CRLead, CreateAccountFromLeadGraphExt, CreateContactFromLeadGraphExt> { }

		/// <exclude/>
		public class CreateOpportunityAllFromLeadGraphExt : CRCreateOpportunityAllAction<LeadMaint, CRLead, CreateOpportunityFromLeadGraphExt, CreateAccountFromLeadGraphExt, CreateContactFromLeadGraphExt> { }

		/// <exclude/>
		public class UpdateRelatedContactInfoFromLeadGraphExt : CRUpdateRelatedContactInfoGraphExt<LeadMaint>
		{
			protected virtual void _(Events.RowPersisted<CRLead> e)
			{
				var row = e.Row;
				if (row == null
					|| e.TranStatus != PXTranStatus.Open
					|| (e.Operation != PXDBOperation.Update && e.Operation != PXDBOperation.Insert)
					|| row.OverrideRefContact == true
					|| !ContactWasUpdated(e.Cache, row))
				{
					return;
				}

				if (row.RefContactID != null)
				{
					PXUpdateJoin<
						Set<Contact.displayName, Required<Contact.displayName>,
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
						>>>>>>>>>>>>>>>>>>>,
					Contact,
					LeftJoin<Standalone.CRLead,
						On<Standalone.CRLead.contactID.IsEqual<Contact.contactID>>>,
					Where<
						// Leads that are linked to the same Contact
						Standalone.CRLead.refContactID.IsEqual<@P.AsInt>
						.And<Standalone.CRLead.overrideRefContact.IsEqual<False>>

						// Contact itself
						.Or<Contact.contactID.IsEqual<@P.AsInt>>>>
					.Update(Base,
						// Set
						row.DisplayName,
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
						new DateTime(1900,1,1),
						row.ConsentAgreement,
						row.ConsentDate,
						row.ConsentExpirationDate,

						// Where
						row.RefContactID, 
						row.RefContactID);
					PXSelectorAttribute.ClearGlobalCache<Contact>();
					Base.Caches<Contact>().Clear();
				}
				else if (row.BAccountID != null)
				{
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
						On<Standalone.CRLead.contactID.IsEqual<Contact.contactID>>,
					LeftJoin<BAccount,
						On<BAccount.defContactID.IsEqual<Contact.contactID>>>>,
					Where<
						// Leads that are linked to the same Account
						Contact.bAccountID.IsEqual<@P.AsInt>
							.And<Standalone.CRLead.overrideRefContact.IsEqual<False>>

						// Account's Contact info itself
						.Or<BAccount.bAccountID.IsEqual<@P.AsInt>
							.And<BAccount.type.IsEqual<BAccountType.prospectType>>>>>
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
						row.BAccountID,
						row.BAccountID);
				}
			}

			protected virtual void _(Events.RowPersisted<Address> e)
			{
				var row = e.Row;
				if (row == null
					|| e.TranStatus != PXTranStatus.Open
					|| (e.Operation != PXDBOperation.Update && e.Operation != PXDBOperation.Insert)
					|| !(AddressWasUpdated(e.Cache, row) || e.Operation == PXDBOperation.Insert))
				{
					return;
				}

				CRLead lead = Base.Lead.Current ?? PXSelect<
						CRLead,
						Where<CRLead.defAddressID.IsEqual<@P.AsInt>>>
					.Select(Base, row.AddressID);

				if (lead == null || lead.OverrideRefContact == true)
					return;

				if (lead.RefContactID != null)
				{
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
						On<Standalone.CRLead.contactID.IsEqual<Contact.contactID>>,
					LeftJoin<BAccount,
						On<BAccount.bAccountID.IsEqual<Contact.bAccountID>>>>>,
					Where<
						// Leads that are linked to the same Contact
						Standalone.CRLead.refContactID.IsEqual<@P.AsInt>
							.And<Standalone.CRLead.overrideRefContact.IsEqual<False>>

						// linked to BA
						.Or<BAccount.bAccountID.IsNotNull>
							.And<Brackets<

								// unlinked Contacts of Customers and Vendors
								BAccount.type.IsIn<BAccountType.customerType, BAccountType.vendorType, BAccountType.combinedType>>
									.And<Contact.defAddressID.IsNotEqual<BAccount.defAddressID>>

								// Contact of Prospect
								.Or<BAccount.type.IsEqual<BAccountType.prospectType>>>
							.And<Contact.contactID.IsEqual<@P.AsInt>>

						// Contact without BA
						.Or<Contact.bAccountID.IsNull>
							.And<Contact.contactID.IsEqual<@P.AsInt>>
					>>
					.Update(Base,
						//Set
						row.AddressLine1,
						row.AddressLine2,
						row.City,
						row.State,
						row.PostalCode,
						row.CountryID,

						// Where
						lead.RefContactID,
						lead.RefContactID,
						lead.RefContactID);
				}
				else if (lead.BAccountID != null)
				{
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
						On<Standalone.CRLead.contactID.IsEqual<Contact.contactID>>,
					LeftJoin<BAccount,
						On<BAccount.defContactID.IsEqual<Contact.contactID>>>>>,
					Where<
						// Leads that are linked to the same Account
						Contact.bAccountID.IsEqual<@P.AsInt>
							.And<Standalone.CRLead.overrideRefContact.IsEqual<False>>


						// Account's Contact info itself
						.Or<BAccount.bAccountID.IsEqual<@P.AsInt>
							.And<BAccount.type.IsEqual<BAccountType.prospectType>>>>>
					.Update(Base,
						//Set
						row.AddressLine1,
						row.AddressLine2,
						row.City,
						row.State,
						row.PostalCode,
						row.CountryID,

						// Where
						lead.BAccountID,
						lead.BAccountID);
				}
			}
		}

		/// <exclude/>
		public class LastNameOrCompanyNameRequiredGraphExt : PXGraphExtension<LeadMaint>
		{
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
		}

		/// <exclude/>
		public class ExtensionSort
			: SortExtensionsBy<ExtensionOrderFor<LeadMaint>
				.FilledWith<
					DefaultLeadOwnerGraphExt,
					ContactAccountDataReplacementGraphExt,
					UpdateRelatedContactInfoFromLeadGraphExt,
					LastNameOrCompanyNameRequiredGraphExt,
					CreateContactFromLeadGraphExt,
					CreateAccountFromLeadGraphExt,
					CreateOpportunityFromLeadGraphExt,
					CreateBothAccountAndContactFromLeadGraphExt,
					CreateOpportunityAllFromLeadGraphExt>> { }
		#endregion
    }
}
