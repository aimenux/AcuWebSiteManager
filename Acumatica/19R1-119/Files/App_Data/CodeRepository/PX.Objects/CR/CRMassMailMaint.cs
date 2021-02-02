using System;
using System.Collections;
using System.Collections.Generic;
using PX.Common;
using PX.Data;
using PX.Data.EP;
using PX.Objects.EP;
using PX.SM;
using PX.TM;
using PX.Objects.CS;

namespace PX.Objects.CR
{
	#region CRMassMailPreview

	[PXCacheName(Messages.PreviewSettings)]
	[Serializable]
	[PXHidden]
	public partial class CRMassMailPreview : IBqlTable
	{
		#region MailAccountID
		public abstract class mailAccountID : PX.Data.BQL.BqlInt.Field<mailAccountID> { }

		[PXDBInt]
		[PXUIField(DisplayName = "From")]
		[PXEMailAccountIDSelectorAttribute]
		[PXDefault]
		public virtual int? MailAccountID { get; set; }
		#endregion

		#region MailTo
		public abstract class mailTo : PX.Data.BQL.BqlString.Field<mailTo> { }
		protected string _MailTo;
		[PXString(255)]
		[PXUIField(DisplayName = "To")]
		[PXDefault]
		public virtual string MailTo
		{
			get
			{
				return this._MailTo;
			}
			set
			{
				this._MailTo = value;
			}
		}
		#endregion
	}

	#endregion

	public class CRMassMailMaint : PXGraph<CRMassMailMaint, CRMassMail>
	{
		#region Recipient

		private class Recipient
		{
		    public Recipient(Contact contact, string format)
            {
                Entity = contact;
                Contact = contact;
                Format = format;
            }

            public Recipient(object entity, Contact contact, string format)
			{
				Entity = entity;
			    Contact = contact;
                Format = format;
			}

            public Contact Contact { get; }
		    public object Entity { get; }
		    public string Format { get; }
		}

		#endregion		

		#region Constants

		private const string _MAILTO_DEFAULT = "((Email))";

		#endregion

		#region Selects

		[PXHidden]
		[PXCopyPasteHiddenView]
		public PXSetup<CRSetup>
			Setup;

		[PXHidden]
		[PXCopyPasteHiddenView]
		public PXSelect<Contact>
			BaseContacts;

        [PXHidden]
        [PXCopyPasteHiddenView]
        public PXSelect<BAccount>
            BAccount;

		[PXViewName(Messages.MassMailSummary)]
		[CRMassEmailLoadTemplate(typeof(CRMassMail),
			ContentField = typeof(CRMassMail.mailContent))]
		[PXCopyPasteHiddenFields(typeof(CRMassMail.mailContent))]
		public PXSelect<CRMassMail>
			MassMails;

		[PXViewName(Messages.History)]
		[PXFilterable]
		[PXCopyPasteHiddenView]
        [PXViewDetailsButton]
		public PXSelectJoin<CRSMEmail,
			InnerJoin<CRMassMailMessage,
				On<CRMassMailMessage.messageID, Equal<CRSMEmail.imcUID>>>,
			Where<CRMassMailMessage.massMailID, Equal<Optional<CRMassMail.massMailID>>>>
			History;

		[PXHidden]
		[PXCopyPasteHiddenView]
		public PXSelect<CRMassMailMessage, 
			Where<CRMassMailMessage.massMailID, Equal<Current<CRMassMail.massMailID>>>>
			SendedMessages;

		[PXViewName(Messages.EntityFields)]
		[PXCopyPasteHiddenView]
		public PXSelectOrderBy<CacheEntityItem,
			OrderBy<Asc<CacheEntityItem.number>>> 
			EntityItems;

		[PXViewName(Messages.MailLists)]
		[PXFilterable]
		[PXCopyPasteHiddenView]
		public PXSelectJoin<CRMarketingList,
			LeftJoin<CRMassMailMarketingList,
				On<CRMassMailMarketingList.mailListID, Equal<CRMarketingList.marketingListID>,
					And<CRMassMailMarketingList.massMailID, Equal<Current<CRMassMail.massMailID>>>>>,
            Where<CRMarketingList.isActive, Equal<boolTrue>>,
			OrderBy<Asc<CRMarketingList.name>>> 
			MailLists;

	    [PXCopyPasteHiddenView]
	    public PXSelectReadonly2<CRMarketingList,
	            LeftJoin<CRMassMailMarketingList,
	                On<CRMassMailMarketingList.mailListID, Equal<CRMarketingList.marketingListID>,
	                    And<CRMassMailMarketingList.massMailID, Equal<Current<CRMassMail.massMailID>>>>>,
	            Where<CRMarketingList.isActive, Equal<boolTrue>>,
	            OrderBy<Asc<CRMarketingList.name>>>
	        MailListsExt;

        [PXViewName(Messages.Campaigns)]
		[PXFilterable]
		[PXCopyPasteHiddenView]
		public PXSelectJoin<CRCampaign,
			LeftJoin<CRMassMailCampaign,
				On<CRMassMailCampaign.campaignID, Equal<CRCampaign.campaignID>, 
					And<CRMassMailCampaign.massMailID, Equal<Current<CRMassMail.massMailID>>>>>,
            Where<CRCampaign.isActive, Equal<boolTrue>>, 
			OrderBy<Asc<CRCampaign.campaignName>>>
			Campaigns;

		[PXViewName(Messages.LeadsAndContacts)]
		[PXFilterable]
		[PXCopyPasteHiddenView]
		public PXSelectJoin<Contact,
			LeftJoin<BAccount, On<BAccount.bAccountID, Equal<Contact.bAccountID>>,
			LeftJoin<Address, On<Address.addressID, Equal<Contact.defAddressID>>, 
			LeftJoin<CRMassMailMember, 
				On<CRMassMailMember.contactID, Equal<Contact.contactID>,
					And<CRMassMailMember.massMailID, Equal<Current<CRMassMail.massMailID>>>>>>>,
			Where2<
				Where<Contact.noMassMail, IsNull, Or<Contact.noMassMail, NotEqual<True>>>, 
				And2<Where<Contact.noEMail, IsNull, Or<Contact.noEMail, NotEqual<True>>>,
				And<Where<Contact.noMarketing, IsNull, Or<Contact.noMarketing, NotEqual<True>>>>>>,
			OrderBy<Asc<Contact.displayName, Asc<Contact.contactID>>>>
			Leads;

		[PXViewName(Messages.Preview)]
		[PXCopyPasteHiddenView]
		public PXFilter<CRMassMailPreview> 
			Preview;

		[PXHidden]
		[PXCopyPasteHiddenView]
		public PXSelect<CRMassMailMarketingList,
			Where<CRMassMailMarketingList.massMailID, Equal<Required<CRMassMail.massMailID>>>> 
			selectedMailList;

		[PXHidden]
		[PXCopyPasteHiddenView]
		public PXSelect<CRMassMailCampaign,
			Where<CRMassMailCampaign.massMailID, Equal<Required<CRMassMail.massMailID>>>>
			selectedCampaigns;

		[PXHidden]
		[PXCopyPasteHiddenView]
		public PXSelect<CRMassMailMember,
			Where<CRMassMailMember.massMailID, Equal<Required<CRMassMail.massMailID>>>>
			selectedLeads;

		[PXHidden]
		[PXCopyPasteHiddenView]
		public PXSelectJoin<Contact,
			InnerJoin<CRMarketingListMember,
				On<CRMarketingListMember.contactID, Equal<Contact.contactID>>>> DynamicSourceList;

        [PXViewName(Messages.Activities)]
        [PXFilterable]
        public CRActivityList<CRMassMail>
            Activities;	   
        #endregion

        #region Ctors

        public CRMassMailMaint()
		{
			if (string.IsNullOrEmpty(Setup.Current.MassMailNumberingID))
				throw new PXSetPropertyException(Messages.NumberingIDIsNull, Messages.CRSetup);
            
            
            PXUIFieldAttribute.SetEnabled(Campaigns.Cache, null, false);
			PXUIFieldAttribute.SetEnabled<CRCampaign.selected>(Campaigns.Cache, null, true);
            PXUIFieldAttribute.SetEnabled<CRCampaign.sendFilter>(Campaigns.Cache, null, true);
			PXUIFieldAttribute.SetDisplayName<Contact.fullName>(Leads.Cache, Messages.CompanyName);
			PXUIFieldAttribute.SetDisplayName<BAccount.classID>(Caches[typeof(BAccount)], Messages.CompanyClass);
			PXDBAttributeAttribute.Activate(BaseContacts.Cache);
		}

        #endregion

        #region Data Handlers	    
        protected virtual IEnumerable dynamicSourceList([PXInt] int mailListID)
		{
			return CRSubscriptionsSelect.Select(this, mailListID);
		}
		protected virtual IEnumerable entityItems([PXString] string parent)
		{
			if (MassMails.Current == null) return new CacheEntityItem[0];
            
            return EMailSourceHelper.TemplateEntity(this, parent, typeof(Contact).FullName, null);
		}

		protected virtual IEnumerable mailLists()
		{
			foreach (PXResult row in MailListsExt.Select())
			{
				var rec = (CRMarketingList)row[typeof(CRMarketingList)];
				var mailList = (CRMassMailMarketingList)row[typeof(CRMassMailMarketingList)];
			    CRMarketingList cache = (CRMarketingList)this.MailLists.Cache.Locate(rec);

			    if (rec.Selected != true && mailList.MailListID != null)			    
			        rec.Selected = true;		
                
                if (cache != null)
			    {
			        bool? selected = cache.Selected;
                    MailLists.Cache.RestoreCopy(cache, rec);
			        cache.Selected = selected;
			        rec = cache;
			    }			                    
				yield return new PXResult<CRMarketingList>(rec);
			}
		}

		protected virtual IEnumerable campaigns()
		{
			foreach (PXResult row in Campaigns.View.QuickSelect())
			{
				var campaign = (CRCampaign)row[typeof(CRCampaign)];
				var mailCampaign = (CRMassMailCampaign)row[typeof(CRMassMailCampaign)];
				if (campaign.Selected != true && mailCampaign.CampaignID != null &&
					Campaigns.Cache.GetStatus(campaign) != PXEntryStatus.Updated)
				{
					campaign.Selected = true;					
				}
				yield return new PXResult<CRCampaign>(campaign);
			}
		}

		protected virtual IEnumerable leads()
		{
			foreach(PXResult row in Leads.View.QuickSelect())
			{
				var contact = (Contact)row[typeof(Contact)];
				var mailLead = (CRMassMailMember)row[typeof(CRMassMailMember)];
				if (contact.Selected != true && mailLead.ContactID != null && 
					Leads.Cache.GetStatus(contact) != PXEntryStatus.Updated)
				{
					contact.Selected = true;
				}
				var bAccount = (BAccount)row[typeof(BAccount)];
				var address = (Address)row[typeof(Address)];
				yield return new PXResult<Contact, BAccount, Address>(contact, bAccount, address);
			}
		}

		#endregion

		#region Event Handlers

		[PXUIField(DisplayName = "Display Name", Visibility = PXUIVisibility.SelectorVisible, Enabled = false)]
		[PXDBString(255, IsUnicode = true)]
		[PXFieldDescription]
		[PXNavigateSelector(typeof(Search<Contact.displayName>))]
		protected virtual void Contact_DisplayName_CacheAttached(PXCache sender)
		{

		}

		[PXDBGuid]
		[PXOwnerSelector(typeof(Contact.workgroupID))]
		[PXChildUpdatable(AutoRefresh = true, TextField = "AcctName", ShowHint = false)]
		[PXUIField(DisplayName = "Owner", Visibility = PXUIVisibility.SelectorVisible)]
		protected virtual void Contact_OwnerID_CacheAttached(PXCache sender)
		{

		}

		[PXDBString(10, IsUnicode = true, InputMask = ">aaaaaaaaaa")]
		[PXUIField(DisplayName = "Class ID")]
		[PXNavigateSelector(typeof(CRContactClass.classID), DescriptionField = typeof(CRContactClass.description), CacheGlobal = true)]
		protected virtual void Contact_ClassID_CacheAttached(PXCache sender)
		{

		}

		[PXDBString(1)]
		[PXUIField(DisplayName = "Source")]
		[CRMSources]
		protected virtual void Contact_Source_CacheAttached(PXCache sender)
		{

		}

		[PXDBString(1)]
		[PXUIField(DisplayName = "Status", Visibility = PXUIVisibility.SelectorVisible)]
		[LeadStatuses]
		protected virtual void Contact_Status_CacheAttached(PXCache sender)
		{
			
		}

		[PXDBString(2, IsFixed = true)]
		[PXUIField(DisplayName = "Reason")]
		[LeadResolutions]
		protected virtual void Contact_Resolution_CacheAttached(PXCache sender)
		{

		}

        protected virtual void CRCampaign_Selected_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
        {
            CRCampaign row = (CRCampaign)e.Row;
            if (row != null && !(bool)e.OldValue && (bool)row.Selected)
            {
                foreach (CRCampaign item in Campaigns.Select())
                {
                    if (item.Selected == true && item != row)
                    {
                        sender.SetValue<CRCampaign.selected>(item, false);
                        sender.SetStatus(item, PXEntryStatus.Updated);
                    }
                }
                Campaigns.View.RequestRefresh();
            }
        }


        protected virtual void CRMassMail_RowSelected(PXCache cache, PXRowSelectedEventArgs e)
		{
			var row = (CRMassMail)e.Row;
			if (row == null) return;

			CorrectUI(cache, row);
		}

		protected virtual void CRMassMail_RowInserted(PXCache cache, PXRowInsertedEventArgs e)
		{
			((CRMassMail)e.Row).MailTo = _MAILTO_DEFAULT;
		}

		protected virtual void CRMarketingList_RowPersisting(PXCache cache, PXRowPersistingEventArgs e)
		{
			e.Cancel = true;
		}		

		protected virtual void Contact_RowPersisting(PXCache cache, PXRowPersistingEventArgs e)
		{
			e.Cancel = true;
		}

		protected virtual void CRCampaign_RowPersisting(PXCache cache, PXRowPersistingEventArgs e)
		{
			e.Cancel = true;
		}

		protected virtual void CRMassMailPreview_RowPersisting(PXCache cache, PXRowPersistingEventArgs e)
		{
			e.Cancel = true;
		}

		#endregion

		#region Overrides

		public override void Persist()
		{
			saveMailLists();
			saveCampaigns();
			saveLeads();

			base.Persist();

			CorrectUI(MassMails.Cache, MassMails.Current);
		}

		#endregion

		#region Actions

		public PXAction<CRMassMail> PreviewMail;
		[PXUIField(DisplayName = Messages.PreviewMessage)]
		[PXButton]
		public virtual IEnumerable previewMail(PXAdapter a)
		{
			if (Preview.AskExt(
				(graph, name) =>
					{
						Preview.Current.MailAccountID = MassMails.Current.With(current => current.MailAccountID)
							?? MailAccountManager.DefaultMailAccountID;
						Preview.Current.MailTo = MailAccountManager.GetDefaultEmailAccount(false).With(_ => _.Address);
					}) == WebDialogResult.OK)
			{
				Preview.View.Answer = WebDialogResult.OK;
				CheckFields(Preview.Cache, Preview.Current,
							typeof(CRMassMailPreview.mailAccountID),
							typeof(CRMassMailPreview.mailTo));
				Preview.View.Answer = WebDialogResult.None;

				var mails = GetMailsForSending(true);
				if (mails.Count == 0) throw new PXException(Messages.RecipientsNotFound);

				var mailTo = Preview.Current.MailTo;
				var accountId = Preview.Current.MailAccountID;

				SendMassMail(accountId, mailTo, null, null, mails, false, this, MassMails.Current, true);
				Save.Press();
			}

			yield return MassMails.Current;
		}

		public PXAction<CRMassMail> Send;
		[PXUIField(DisplayName = Messages.Send)]
		[PXSendMailButton]
		public virtual IEnumerable send(PXAdapter a)
		{
			CheckFields(MassMails.Cache, MassMails.Current,
						typeof(CRMassMail.mailAccountID),
						typeof(CRMassMail.mailSubject),
						typeof(CRMassMail.mailTo),
						typeof(CRMassMail.plannedDate));

			SendMails();
			
			yield return MassMails.Current;
		}

		public PXAction<CRMassMail> MessageDetails;
		[PXUIField(Visible = false)]
		[PXButton]
		public virtual IEnumerable messageDetails(PXAdapter a)
		{
			PXRedirectHelper.TryOpenPopup(History.Cache, History.Current, string.Empty);
			
			yield return MassMails.Current;
		}

		#endregion

		#region Private Methods

		private void CorrectUI(PXCache cache, CRMassMail row)
		{
            if (row == null) return;

			var isEnabled = row.Status != CRMassMailStatusesAttribute.Send;
			PXUIFieldAttribute.SetEnabled<CRMassMail.massMailID>(MassMails.Cache, row);
			PXUIFieldAttribute.SetEnabled<CRMassMail.mailAccountID>(MassMails.Cache, row, isEnabled);
			PXUIFieldAttribute.SetEnabled<CRMassMail.mailSubject>(MassMails.Cache, row, isEnabled);
			PXUIFieldAttribute.SetEnabled<CRMassMail.mailTo>(MassMails.Cache, row, isEnabled);
			PXUIFieldAttribute.SetEnabled<CRMassMail.mailCc>(MassMails.Cache, row, isEnabled);
			PXUIFieldAttribute.SetEnabled<CRMassMail.mailBcc>(MassMails.Cache, row, isEnabled);
			PXUIFieldAttribute.SetEnabled<CRMassMail.mailContent>(MassMails.Cache, row, isEnabled);
			PXUIFieldAttribute.SetEnabled<CRMassMail.source>(MassMails.Cache, row, isEnabled);
			PXUIFieldAttribute.SetEnabled<CRMassMail.sourceType>(MassMails.Cache, row, isEnabled);
			PXUIFieldAttribute.SetEnabled<CRMassMail.plannedDate>(MassMails.Cache, row, isEnabled);
			PXUIFieldAttribute.SetEnabled<CRMassMail.status>(MassMails.Cache, row, isEnabled);
			PXUIFieldAttribute.SetEnabled<CRMassMail.sentDateTime>(MassMails.Cache, row, false);
			MailLists.Cache.AllowUpdate = isEnabled;
			Leads.Cache.AllowUpdate = isEnabled;
			Campaigns.Cache.AllowUpdate = isEnabled;

			var isNotInserted = cache.GetStatus(row) != PXEntryStatus.Inserted;
			Send.SetEnabled(isEnabled && isNotInserted);
			PreviewMail.SetEnabled(isEnabled && isNotInserted);
		}

		private void SendMails()
		{
			if (MassMails.Current == null || MassMails.Current.Status == CRMassMailStatusesAttribute.Send)
				return;

			var mails = GetMailsForSending(false);
			if (mails.Count == 0) throw new PXException(Messages.RecipientsNotFound);

			var confirmMessage = PXMessages.LocalizeFormatNoPrefix(Messages.MassMailSend, mails.Count);
			if (MassMails.Ask(Messages.Confirmation, confirmMessage, MessageButtons.YesNo) != WebDialogResult.Yes)
			{
				return;
			}

			var mailAccountId = MassMails.Current.MailAccountID;
			var mailTo = MassMails.Current.MailTo;
			var mailCc = MassMails.Current.MailCc;
			var mailBcc = MassMails.Current.MailBcc;

			PXLongOperation.StartOperation(this, () => SendMassMail(mailAccountId, mailTo, mailCc, mailBcc, mails, true, this, MassMails.Current));
		}

		private static void SendMassMail(int? accountId, string mailTo, string mailCc, string mailBcc, IEnumerable<Recipient> recievers, bool linkToEntity, CRMassMailMaint graph, CRMassMail current, bool previewMessage = false)
		{
			PXGraph proc = new PXGraph();
            EntityHelper entityHelper = new EntityHelper(proc);
			foreach (Recipient item in recievers)
			{
				var entity = item.Entity;
                PXCache cache = proc.Caches[entity.GetType()];
				cache.SetStatus(entity, PXEntryStatus.Notchanged);
			    var id = entityHelper.GetEntityNoteID(entity, true);

				var sender = TemplateNotificationGenerator.Create(item.Contact);
				sender.MailAccountId = accountId ?? MailAccountManager.DefaultMailAccountID;
                sender.To = mailTo;
                sender.Cc = mailCc;
                sender.Bcc = mailBcc;
				sender.Body = graph.MassMails.Current.MailContent ?? string.Empty;
				sender.Subject = graph.MassMails.Current.MailSubject;
				sender.BodyFormat = item.Format;
				sender.AttachmentsID = graph.MassMails.Current.NoteID;			    
                
                if (linkToEntity)
			    {
                    sender.RefNoteID = id;
                    sender.BAccountID = item.Contact.BAccountID;
			        sender.ContactID = item.Contact.ContactID;
			    }

			    var messages = sender.Send();

				foreach (CRSMEmail message in messages)
				{
					var log = (CRMassMailMessage)graph.SendedMessages.Cache.CreateInstance();
					log.MassMailID = graph.MassMails.Current.MassMailID;
					log.MessageID =message.ImcUID;
					graph.SendedMessages.Insert(log);
				}
			}

			if (!previewMessage)
			{
			graph.MassMails.Current.Status = CRMassMailStatusesAttribute.Send;
			graph.MassMails.Current.SentDateTime = DateTime.Now;
			graph.MassMails.Update(graph.MassMails.Current);
			}

			graph.Save.Press();
		}		

		private List<Recipient> GetMailsForSending(bool onlyFirst)
		{
			var mailKeys = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
			var mails = new List<Recipient>();

			switch (MassMails.Current.Source)
			{
				case CRMassMailSourcesAttribute.MailList:
				    ResetView(MailListsExt.View);
                    foreach (CRMarketingList list in MailLists.Select())
						if (list.Selected == true)
						{
						    var marketingGraph = PXGraph.CreateInstance<CRMarketingListMaint>();
						    marketingGraph.MailLists.Current = list;                            
                            IEnumerable rows = marketingGraph.MailRecipients.Select();							
							foreach (PXResult row in rows)
							{
								var contact = PXResult.Unwrap<Contact>(row);
								if (contact.NoMassMail == true
									|| contact.NoEMail == true
									|| contact.NoMail == true
									|| contact.NoCall == true
									|| contact.NoFax == true
									|| contact.NoMarketing == true)
								{
									continue;
								}

								var subscription = PXResult.Unwrap<CRMarketingListMember>(row);

								if (subscription?.IsSubscribed != true)
									continue;

								var address = contact.EMail;
								if (address != null && (address = address.Trim()) != string.Empty && !mailKeys.Contains(address))
								{
									mailKeys.Add(address);
									mails.Add(new Recipient(list, contact, subscription.Return(s => s.Format, NotificationFormat.Html)));
									if (onlyFirst) break;
								}
							}
							if (mails.Count > 0 && onlyFirst) break;
						}
					break;
				case CRMassMailSourcesAttribute.Campaign:
				    ResetView(Campaigns.View);
                    foreach (CRCampaign list in Campaigns.Select())
						if (list.Selected == true)
						{
						    Campaigns.Cache.Current = list;

							var select = new PXSelectJoin<
									Contact,
								InnerJoin<CRCampaignMembers, 
									On<CRCampaignMembers.contactID, Equal<Contact.contactID>>>,
								Where<
									Where2<
										Where<GDPR.ContactExt.pseudonymizationStatus, Equal<PXPseudonymizationStatusListAttribute.notPseudonymized>,
										Or<GDPR.ContactExt.pseudonymizationStatus, IsNull>>,
									And<Contact.noMassMail, NotEqual<True>,
									And<Contact.noEMail, NotEqual<True>,
									And<Contact.noMarketing, NotEqual<True>,
									And<CRCampaignMembers.campaignID, Equal<Required<CRCampaignMembers.campaignID>>>>>>>>>(this);

							if (list.SendFilter == SendFilterSourcesAttribute._NEVERSENT)
							{
								select.WhereAnd<Where<CRCampaignMembers.emailSendCount, Equal<Zero>>>();
							}

							foreach (PXResult<Contact, CRCampaignMembers> item in select.Select(list.CampaignID))
							{
								var contact = (Contact)item;
								var address = contact.EMail;
								if (address != null && (address = address.Trim()) != string.Empty && !mailKeys.Contains(address))
								{
									mailKeys.Add(address);
									mails.Add(new Recipient(list, contact, NotificationFormat.Html));
									if (onlyFirst)
										break;
								}
							}

							if (mails.Count > 0 && onlyFirst)
								break;
						}
					break;
				case CRMassMailSourcesAttribute.Lead:
					foreach (Contact contact in Leads.Select())
						if (contact.Selected == true)
						{
							var address = contact.EMail;
							if (address != null && (address = address.Trim()) != string.Empty && !mailKeys.Contains(address))
							{
								mailKeys.Add(address);
								mails.Add(new Recipient(contact, NotificationFormat.Html));
								if (onlyFirst) break;
							}
						}
					break;
			}
			return mails;
		}
		
	    private void ResetView(PXView view)
	    {
	        view.Cache.Current = null;            
            view.Clear();
	    }
		private void saveLeads()
		{
			if (MassMails.Current != null && MassMails.Current.MassMailID != null)
			{
				var massMailID = (int)MassMails.Current.MassMailID;
				selectedLeads.View.Clear();
				if (MassMails.Current.Source == CRMassMailSourcesAttribute.Lead)
					foreach (Contact batch in Leads.Cache.Updated)
					{
						if (batch == null || batch.ContactID == null) continue;

						var item = (CRMassMailMember)PXSelect<CRMassMailMember>.
							Search<CRMassMailMember.massMailID, CRMassMailMember.contactID>(this, massMailID, batch.ContactID);

						if (batch.Selected != true && item != null)
							selectedLeads.Delete(item);

						if (batch.Selected == true && item == null)
						{
							item = new CRMassMailMember();
							item.MassMailID = massMailID;
							item.ContactID = batch.ContactID;
							selectedLeads.Insert(item);
						}
					}
				else
					foreach (CRMassMailMember item in selectedLeads.Select(massMailID))
						selectedLeads.Delete(item);
			}
		}

		private void saveCampaigns()
		{
			if (MassMails.Current != null && MassMails.Current.MassMailID != null)
			{
				var massMailID = (int)MassMails.Current.MassMailID;
				selectedCampaigns.View.Clear();
				if (MassMails.Current.Source == CRMassMailSourcesAttribute.Campaign)
					foreach (CRCampaign batch in Campaigns.Cache.Updated)
					{
						if (batch == null || batch.CampaignID == null) continue;

						var item = (CRMassMailCampaign)PXSelect<CRMassMailCampaign>.
							Search<CRMassMailCampaign.massMailID, CRMassMailCampaign.campaignID>(this, massMailID, batch.CampaignID);

						if (batch.Selected != true && item != null)
							selectedCampaigns.Delete(item);

						if (batch.Selected == true)
						{
							if (item == null)
							{
								item = new CRMassMailCampaign();
								item.MassMailID = massMailID;
								item.CampaignID = batch.CampaignID;
								selectedCampaigns.Insert(item);
							}
							else
							{
								selectedCampaigns.Update(item);
							}
						}
					}
				else
					foreach (CRMassMailCampaign item in selectedCampaigns.Select(massMailID))
						selectedCampaigns.Delete(item);
			}
		}

		private void saveMailLists()
		{
			if (MassMails.Current != null && MassMails.Current.MassMailID != null)
			{
				var massMailID = (int)MassMails.Current.MassMailID;
				selectedMailList.View.Clear();
				if (MassMails.Current.Source == CRMassMailSourcesAttribute.MailList)
					foreach (CRMarketingList batch in MailLists.Cache.Updated)
					{
						if (batch == null || !batch.MarketingListID.HasValue) continue;

						var item = (CRMassMailMarketingList)PXSelect<CRMassMailMarketingList>.
							Search<CRMassMailMarketingList.massMailID, CRMassMailMarketingList.mailListID>(this, massMailID, batch.MarketingListID);

						if (batch.Selected != true && item != null)
							selectedMailList.Delete(item);

						if (batch.Selected == true && item == null)
						{
							item = new CRMassMailMarketingList();
							item.MassMailID = massMailID;
							item.MailListID = batch.MarketingListID;
							selectedMailList.Insert(item);
						}
					}
				else
					foreach (CRMassMailMarketingList item in selectedMailList.Select(massMailID))
						selectedMailList.Delete(item);

			}
		}

		private void CheckFields(PXCache cache, object row, params Type[] fields)
		{
			var errors = new Dictionary<string, string>(fields.Length);
			foreach (Type field in fields)
			{
				var value = cache.GetValue(row, field.Name);
				if (value == null || (value is string && string.IsNullOrEmpty(value as string)))
				{
					var state = cache.GetValueExt(row, field.Name) as PXFieldState;
					var fieldDisplayName = state == null || string.IsNullOrEmpty(state.DisplayName) 
						? field.Name 
						: state.DisplayName;
					var errorMessage = PXMessages.LocalizeFormatNoPrefix(Messages.EmptyValueErrorFormat, fieldDisplayName);
					var fieldName = cache.GetField(field);
					errors.Add(fieldName, errorMessage);
					PXUIFieldAttribute.SetError(cache, row, fieldName, errorMessage);
				}
			}
			if (errors.Count > 0)
				throw new PXOuterException(errors, GetType(), row, ErrorMessages.RecordRaisedErrors, null, cache.GetItemType().Name);
		}

		#endregion
	}
}
