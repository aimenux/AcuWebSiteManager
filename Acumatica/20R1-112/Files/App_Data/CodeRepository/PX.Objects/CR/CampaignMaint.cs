using System;
using PX.Data;
using System.Collections.Generic;
using System.Collections;
using PX.Objects.CR.Standalone;
using PX.Objects.CS;
using System.Linq;
using PX.Objects.AP;
using PX.Objects.AR;
using PX.Objects.SO;
using PX.Objects.GL;
using PX.Objects.PM;

namespace PX.Objects.CR
{
	#region CampaignMaint
	public class CampaignMaint : PXGraph<CampaignMaint, CRCampaign>, PXImportAttribute.IPXPrepareItems
	{
		[PXHidden]
		public PXSetup<CRSetup>
			crSetup;

		[PXHidden]
		public PXSelect<Contact>
			BaseContacts;

		[PXHidden]
		public PXSelect<APInvoice>
			APInvoicies;

		[PXHidden]
		public PXSelect<ARInvoice>
			ARInvoicies;

		public PXSelect<Contact, Where<Contact.contactID, Equal<Required<Contact.contactID>>>>
			ContactByContactId;

		[PXViewName(Messages.Campaign)]
		public PXSelect<CRCampaign>
			Campaign;

		[PXHidden]
		public PXSelect<CROpportunityClass>
			CROpportunityClass;

		[PXViewName(Messages.Opportunities)]
		public PXSelectJoin<CROpportunity,
			LeftJoin<CROpportunityProbability, On<CROpportunity.stageID, Equal<CROpportunityProbability.stageCode>>,
			LeftJoin<CROpportunityClass, On<CROpportunityClass.cROpportunityClassID, Equal<CROpportunity.classID>>>>,
			Where<CROpportunity.campaignSourceID, Equal<Current<CRCampaign.campaignID>>>>
			Opportunities;

		[PXHidden]
		[PXCopyPasteHiddenFields(typeof(CRCampaign.projectID), typeof(CRCampaign.projectTaskID))]
		public PXSelect<CRCampaign,
			Where<CRCampaign.campaignID, Equal<Current<CRCampaign.campaignID>>>>
			CampaignCurrent;

		[PXHidden]
		public PXSelect<DAC.Standalone.CRCampaign,
			Where<DAC.Standalone.CRCampaign.campaignID, Equal<Current<CRCampaign.campaignID>>>>
			CalcCampaignCurrent;


		public PXFilter<OperationParam> Operations;

		[PXViewName(Messages.CampaignMembers)]
		[PXImport(typeof(CRCampaign))]
		[PXFilterable]
		public CRCampaignMembersList CampaignMembers;

		[PXHidden]
		public PXSelect<CRCampaignMembers> CampaignMembersHidden;

		[PXViewName(Messages.Answers)]
		public CRAttributeList<CRCampaign>
			Answers;


		[PXViewName(Messages.Leads)]
		[PXFilterable]
		[PXCopyPasteHiddenView]
		public PXSelectJoin<CRLead,
			LeftJoin<BAccount,
				On<BAccount.bAccountID, Equal<CRLead.bAccountID>>,
			LeftJoin<Address,
				On<Address.addressID, Equal<CRLead.defAddressID>>>>,
			Where<CRLead.campaignID, Equal<Current<CRCampaign.campaignID>>>,
			OrderBy<
				Asc<CRLead.displayName, Asc<CRLead.contactID>>>>
			Leads;

		[PXViewName(Messages.Activities)]
		[PXFilterable]
		public CRCampaignMembersActivityList<CRCampaign>
			Activities;

		[PXHidden]
		public PXSelect<CRPMTimeActivity, Where<True, Equal<False>>> _hiddenActivities;

		public CampaignMaint()
		{
			PXDBAttributeAttribute.Activate(this.Caches[typeof(Contact)]);
			PXDBAttributeAttribute.Activate(Opportunities.Cache);
			PXUIFieldAttribute.SetEnabled<DAC.Standalone.CRCampaign.leadsGenerated>(CampaignCurrent.Cache, null, false);
			PXUIFieldAttribute.SetEnabled<DAC.Standalone.CRCampaign.leadsConverted>(CampaignCurrent.Cache, null, false);
			PXUIFieldAttribute.SetEnabled<DAC.Standalone.CRCampaign.contacts>(CampaignCurrent.Cache, null, false);
			PXUIFieldAttribute.SetEnabled<DAC.Standalone.CRCampaign.opportunities>(CampaignCurrent.Cache, null, false);
			PXUIFieldAttribute.SetEnabled<DAC.Standalone.CRCampaign.closedOpportunities>(CampaignCurrent.Cache, null, false);
			PXUIFieldAttribute.SetEnabled<DAC.Standalone.CRCampaign.opportunitiesValue>(CampaignCurrent.Cache, null, false);
			PXUIFieldAttribute.SetEnabled<DAC.Standalone.CRCampaign.closedOpportunitiesValue>(CampaignCurrent.Cache, null, false);

			PXUIFieldAttribute.SetRequired<CRCampaign.startDate>(CampaignCurrent.Cache, true);
			PXUIFieldAttribute.SetRequired<CRCampaign.status>(CampaignCurrent.Cache, true);


			var cache = Caches[typeof(Contact)];
			PXDBAttributeAttribute.Activate(cache);
			PXUIFieldAttribute.SetVisible<Contact.title>(cache, null, false);
			PXUIFieldAttribute.SetVisible<Contact.workgroupID>(cache, null, false);
			PXUIFieldAttribute.SetVisible<Contact.firstName>(cache, null, false);
			PXUIFieldAttribute.SetVisible<Contact.midName>(cache, null, false);
			PXUIFieldAttribute.SetVisible<Contact.lastName>(cache, null, false);
			PXUIFieldAttribute.SetVisible<Contact.phone2>(cache, null, false);
			PXUIFieldAttribute.SetVisible<Contact.phone3>(cache, null, false);
			PXUIFieldAttribute.SetVisible<Contact.fax>(cache, null, false);
			PXUIFieldAttribute.SetVisible<Contact.webSite>(cache, null, false);
			PXUIFieldAttribute.SetVisible<Contact.dateOfBirth>(cache, null, false);
			PXUIFieldAttribute.SetVisible<Contact.createdByID>(cache, null, false);
			PXUIFieldAttribute.SetVisible<Contact.createdDateTime>(cache, null, false);
			PXUIFieldAttribute.SetVisible<Contact.lastModifiedByID>(cache, null, false);
			PXUIFieldAttribute.SetVisible<Contact.lastModifiedDateTime>(cache, null, false);

			PXUIFieldAttribute.SetVisible<Address.addressLine1>(Caches[typeof(Address)], null, false);
			PXUIFieldAttribute.SetVisible<Address.addressLine2>(Caches[typeof(Address)], null, false);

			PXUIFieldAttribute.SetVisible<Contact.classID>(cache, null, false);
			PXUIFieldAttribute.SetVisible<Contact.source>(cache, null, false);
			PXUIFieldAttribute.SetVisible<Contact.status>(cache, null, false);

			PXUIFieldAttribute.SetVisibility<Contact.contactPriority>(cache, null, PXUIVisibility.Invisible);

			PXUIFieldAttribute.SetDisplayName<BAccount.acctName>(Caches[typeof(BAccount)], Messages.CustomerName);
			cache = Caches[typeof(CRLead)];
			PXUIFieldAttribute.SetVisible<CRLead.title>(cache, null, false);
			PXUIFieldAttribute.SetVisible<CRLead.firstName>(cache, null, false);
			PXUIFieldAttribute.SetVisible<CRLead.midName>(cache, null, false);
			PXUIFieldAttribute.SetVisible<CRLead.lastName>(cache, null, false);
			PXUIFieldAttribute.SetVisible<CRLead.phone1>(cache, null, false);
			PXUIFieldAttribute.SetVisible<CRLead.phone2>(cache, null, false);
			PXUIFieldAttribute.SetVisible<CRLead.phone3>(cache, null, false);
			PXUIFieldAttribute.SetVisible<CRLead.fax>(cache, null, false);
			PXUIFieldAttribute.SetVisible<CRLead.eMail>(cache, null, false);
			PXUIFieldAttribute.SetVisible<CRLead.webSite>(cache, null, false);
			PXUIFieldAttribute.SetVisible<CRLead.dateOfBirth>(cache, null, false);
			PXUIFieldAttribute.SetVisible<CRLead.createdByID>(cache, null, false);
			PXUIFieldAttribute.SetVisible<CRLead.createdDateTime>(cache, null, false);
			PXUIFieldAttribute.SetVisible<CRLead.lastModifiedByID>(cache, null, false);
			PXUIFieldAttribute.SetVisible<CRLead.lastModifiedDateTime>(cache, null, false);
			PXUIFieldAttribute.SetEnabled(this.Caches[typeof(Contact)], null, null, false);
			PXUIFieldAttribute.SetEnabled<Contact.selected>(this.Caches[typeof(Contact)], null, true);
		}

		public override void Clear(PXClearOption option)
		{
			if (this.Caches.ContainsKey(typeof(CRCampaignMembers)))
			{
				this.Caches[typeof(CRCampaignMembers)].ClearQueryCache();
			}

			base.Clear(option);
		}

		#region CacheAttached

		[PXMergeAttributes(Method = MergeMethod.Replace)]
		[PXDBString(10, IsUnicode = true, IsKey = true)]
		[PXDBLiteDefault(typeof(CRCampaign.campaignID))]
		[PXUIField(DisplayName = Messages.CampaignID)]
		[PXParent(typeof(Select<CRCampaign, Where<CRCampaign.campaignID, Equal<Current<CRCampaignMembers.campaignID>>>>))]
		protected virtual void CRCampaignMembers_CampaignID_CacheAttached(PXCache cache)
		{
		}

		[PXMergeAttributes(Method = MergeMethod.Merge)]
		[PXUIField(DisplayName = "Member Since", Enabled = false)]
		protected virtual void CRCampaignMembers_CreatedDateTime_CacheAttached(PXCache cache)
		{
		}

		[PXMergeAttributes(Method = MergeMethod.Merge)]
		[PXUIField(DisplayName = "Contact", Visibility = PXUIVisibility.SelectorVisible, Enabled = false)]
		protected void Contact_DisplayName_CacheAttached(PXCache sender)
		{
		}

		[PXMergeAttributes(Method = MergeMethod.Merge)]
		[PXDefault("")]
		protected void Contact_ContactType_CacheAttached(PXCache sender)
		{
		}

		[PXMergeAttributes(Method = MergeMethod.Merge)]
		[PXUIField(DisplayName = "Account ID", Visibility = PXUIVisibility.SelectorVisible, Enabled = false)]
		[PXSelector(typeof(BAccount.bAccountID), SubstituteKey = typeof(BAccount.acctCD), DescriptionField = typeof(BAccount.acctCD), DirtyRead = true)]
		protected void Contact_BAccountID_CacheAttached(PXCache sender)
		{
		}

		[PXMergeAttributes(Method = MergeMethod.Merge)]
		[PXSelector(typeof(BAccountR.bAccountID), SubstituteKey = typeof(BAccountR.acctCD), DescriptionField = typeof(BAccountR.acctCD))]
		protected void CRPMTimeActivity_BAccountID_CacheAttached(PXCache sender)
		{

		}
		[PXMergeAttributes(Method = MergeMethod.Merge)]
		[PXSelector(typeof(Contact.contactID), DescriptionField = typeof(Contact.displayName))]
		protected void CRPMTimeActivity_ContactID_CacheAttached(PXCache sender)
		{

		}

		#region CROpportunityClass
		[PXUIField(DisplayName = "Class Description")]
		[PXMergeAttributes(Method = MergeMethod.Merge)]
		protected virtual void CROpportunityClass_Description_CacheAttached(PXCache sender)
		{
		}
		#endregion

		#endregion

		#region Actions

		public PXDBAction<CRCampaign> addOpportunity;
		[PXUIField(DisplayName = Messages.AddNewOpportunity, FieldClass = FeaturesSet.customerModule.FieldClass)]
		[PXButton(ImageKey = PX.Web.UI.Sprite.Main.AddNew, OnClosingPopup = PXSpecialButtonType.Refresh)]
		public virtual void AddOpportunity()
		{
			var row = CampaignCurrent.Current;
			if (row == null || row.CampaignID == null) return;

			var graph = PXGraph.CreateInstance<OpportunityMaint>();

			var newOpportunity = graph.Opportunity.Insert();

			newOpportunity.CampaignSourceID = row.CampaignID;

			if (row.ProjectID != null)
				newOpportunity.ProjectID = row.ProjectID;

			CROpportunityClass ocls = PXSelect<CROpportunityClass, Where<CROpportunityClass.cROpportunityClassID, Equal<Current<CROpportunity.classID>>>>
				.SelectSingleBound(this, new object[] { newOpportunity });
			if (ocls?.DefaultOwner == CRDefaultOwnerAttribute.Source)
			{
				newOpportunity.WorkgroupID = row.WorkgroupID;
				newOpportunity.OwnerID = row.OwnerID;
			}

			graph.Opportunity.Update(newOpportunity);
			PXRedirectHelper.TryRedirect(graph, PXRedirectHelper.WindowMode.NewWindow);
		}

		public PXDBAction<CRCampaign> addContact;
		[PXUIField(DisplayName = Messages.AddNewLead, FieldClass = FeaturesSet.customerModule.FieldClass)]
		[PXButton(ImageKey = PX.Web.UI.Sprite.Main.AddNew, OnClosingPopup = PXSpecialButtonType.Refresh)]
		public virtual void AddContact()
		{
			var row = CampaignCurrent.Current;
			if (row?.CampaignID == null) return;

			var graph = PXGraph.CreateInstance<LeadMaint>();

			var lead = graph.Lead.Insert();

			lead.CampaignID = row.CampaignID;

			CRLeadClass ocls = PXSelect<CRLeadClass, Where<CRLeadClass.classID, Equal<Current<Contact.classID>>>>
				.SelectSingleBound(this, new object[] { lead });
			if (ocls?.DefaultOwner == CRDefaultOwnerAttribute.Source)
			{
				lead.WorkgroupID = row.WorkgroupID;
				lead.OwnerID = row.OwnerID;
			}

			lead = graph.Lead.Update(lead);

			PXRedirectHelper.TryRedirect(graph, PXRedirectHelper.WindowMode.NewWindow);
		}

		public PXAction<CROpportunity> addNewProjectTask;
		[PXUIField(Visible = false)]
		[PXButton()]
		public virtual IEnumerable AddNewProjectTask(PXAdapter adapter)
		{
			var campaign = CampaignCurrent.Current;
			if (campaign != null && campaign.ProjectID.HasValue)
			{
				var graph = PXGraph.CreateInstance<ProjectTaskEntry>();
				graph.Clear();
				var task = new PMTask();
				graph.Task.Cache.SetValue<PMTask.projectID>(task, campaign.ProjectID);

				object taskID = campaign.CampaignID;
				graph.Task.Cache.RaiseFieldUpdating<PMTask.taskCD>(task, ref taskID);
				graph.Task.Cache.SetValue<PMTask.taskCD>(task, taskID);


				task = (PMTask)graph.Task.Cache.CreateCopy(graph.Task.Insert(task));

				graph.Task.Cache.SetValue<PMTask.description>(task, campaign.CampaignName);
				graph.Task.Cache.SetValue<PMTask.plannedStartDate>(task, campaign.StartDate);
				graph.Task.Cache.SetValue<PMTask.startDate>(task, campaign.StartDate);
				graph.Task.Cache.SetValue<PMTask.plannedEndDate>(task, campaign.EndDate);
				graph.Task.Cache.SetValue<PMTask.endDate>(task, campaign.EndDate);
				graph.Task.Update(task);

				PXRedirectHelper.TryRedirect(graph, task, PXRedirectHelper.WindowMode.NewWindow);
			}
			return adapter.Get();
		}

		#endregion

		#region Events

		protected virtual void OperationParam_ContactGI_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
		{
			this.Operations.Cache.SetValue<OperationParam.sharedGIFilter>(this.Operations.Current, null);
		}

		protected virtual void CRCampaign_RowDeleting(PXCache sender, PXRowDeletingEventArgs e)
		{
			CRCampaign row = e.Row as CRCampaign;
			DAC.Standalone.CRCampaign dacRow = CalcCampaignCurrent.Select();
			if (row != null)
			{
				if (CanBeDeleted(row, dacRow) == false)
				{
					e.Cancel = true;
					throw new PXException(Messages.CampaignIsReferenced);
				}
			}
		}

		protected virtual void CRCampaign_Status_FieldDefaulting(PXCache sender, PXFieldDefaultingEventArgs e)
		{
			CRCampaign campaign = (CRCampaign)e.Row;
			var state = (PXStringState)this.Caches<CRCampaign>().GetStateExt<CRCampaign.status>(campaign);
			campaign.Status = state.AllowedValues.FirstOrDefault();
		}

		private bool CanBeDeleted(CRCampaign campaign, DAC.Standalone.CRCampaign dacCampaign)
		{
			foreach (var f in new string[]
			{
				nameof(CRCampaign.mailsSent)
			})
			{
				var state = CampaignCurrent.Cache.GetStateExt(campaign, f);
				if (((PXIntState)state).Value != null && (int)((PXIntState)state).Value > 0)
					return false;
			}

			foreach (var f in new string[]
			{
				nameof(DAC.Standalone.CRCampaign.closedOpportunities),
				nameof(DAC.Standalone.CRCampaign.contacts),
				nameof(DAC.Standalone.CRCampaign.leadsConverted),
				nameof(DAC.Standalone.CRCampaign.leadsGenerated),
				nameof(DAC.Standalone.CRCampaign.opportunities),
			})
			{
				var state = CalcCampaignCurrent.Cache.GetStateExt(dacCampaign, f);
				if (((PXIntState)state).Value != null && (int)((PXIntState)state).Value > 0)
					return false;
			}

			if (PXSelectGroupBy<PMTask,
				Where<PMTask.projectID, Equal<Current<CRCampaign.projectID>>,
				And<PMTask.taskID, Equal<Current<CRCampaign.projectTaskID>>>>,
				Aggregate<Count>>.Select(this).RowCount > 0)
				return false;

			return true;
		}

		protected virtual void CRCampaign_RowSelected(PXCache cache, PXRowSelectedEventArgs e)
		{
			CRCampaign row = e.Row as CRCampaign;
			if (row == null) return;

			var isNotInserted = cache.GetStatus(row) != PXEntryStatus.Inserted;
			addOpportunity.SetEnabled(isNotInserted);
			this.Actions[CRCampaignMembersList.addAction].SetEnabled(isNotInserted);

			PXUIFieldAttribute.SetEnabled<CRCampaign.projectTaskID>(CampaignCurrent.Cache, row, row.ProjectID.HasValue);
			PXUIFieldAttribute.SetRequired<CRCampaign.projectTaskID>(CampaignCurrent.Cache, row.ProjectID.HasValue);
		}

		protected virtual void CRCampaign_ProjectID_FieldUpdated(PXCache cache, PXFieldUpdatedEventArgs e)
		{
			CRCampaign row = e.Row as CRCampaign;
			if (row == null) return;
			this.CampaignCurrent.Cache.SetValue<CRCampaign.projectTaskID>(row, null);
		}

		protected virtual void CRCampaign_RowPersisting(PXCache cache, PXRowPersistingEventArgs e)
		{
			CRCampaign row = (CRCampaign)e.Row;
			if (row != null)
			{
				if (row.StartDate.HasValue == false)
				{
					if (cache.RaiseExceptionHandling<CRCampaign.startDate>(e.Row, null, new PXSetPropertyException(ErrorMessages.FieldIsEmpty, typeof(CRCampaign.startDate).Name)))
					{
						throw new PXRowPersistingException(typeof(CRCampaign.startDate).Name, null, ErrorMessages.FieldIsEmpty, typeof(CRCampaign.startDate).Name);
					}
				}

				if (row.ProjectID.HasValue && !row.ProjectTaskID.HasValue)
				{
					if (cache.RaiseExceptionHandling<CRCampaign.projectTaskID>(e.Row, null, new PXSetPropertyException(ErrorMessages.FieldIsEmpty, typeof(CRCampaign.projectTaskID).Name)))
					{
						throw new PXRowPersistingException(typeof(CRCampaign.projectTaskID).Name, null, ErrorMessages.FieldIsEmpty, typeof(CRCampaign.projectTaskID).Name);
					}
				}

				if (row.ProjectTaskID.HasValue)
				{
					if (PXSelectGroupBy<CRCampaign,
						Where<CRCampaign.projectID, Equal<Required<CRCampaign.projectID>>,
							And<CRCampaign.projectTaskID, Equal<Required<CRCampaign.projectTaskID>>,
							And<CRCampaign.campaignID, NotEqual<Required<CRCampaign.campaignID>>>>>,
						Aggregate<Count>>.Select(this, new object[] { row.ProjectID, row.ProjectTaskID, row.CampaignID }).RowCount > 0)
					{
						throw new PXRowPersistingException(typeof(CRCampaign.projectTaskID).Name, row.ProjectTaskID, Messages.TaskIsAlreadyLinkedToCampaign, typeof(CRCampaign.projectTaskID).Name);
					}
				}
			}
		}

		#endregion

		#region Implementation of IPXPrepareItems

		public virtual bool PrepareImportRow(string viewName, IDictionary keys, IDictionary values)
		{
			if (string.Compare(viewName, "CampaignMembers", true) == 0)
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
						if (values.Contains("CampaignID"))
							values["CampaignID"] = Campaign.Current.CampaignID;
						else
							values.Add("CampaignID", Campaign.Current.CampaignID);
						keys["CampaignID"] = Campaign.Current.CampaignID;

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
	#endregion
}
