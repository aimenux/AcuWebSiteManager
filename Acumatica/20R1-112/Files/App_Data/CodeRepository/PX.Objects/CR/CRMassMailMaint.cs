using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using PX.Common;
using PX.Data;
using PX.Data.EP;
using PX.Objects.EP;
using PX.SM;
using PX.TM;
using PX.Objects.CS;
using System.Linq;
using PX.Data.BQL;
using PX.Data.BQL.Fluent;

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
			public Recipient(
				Contact contact,
				string format,
				object entity = null,
				int? bAccountID = null,
				int? contactID = null,
				Guid? refNoteID = null,
				Guid? documentNoteID = null)
			{
				Contact = contact;
				Format = format;
				Entity = entity;
				BAccountID = bAccountID;
				ContactID = contactID;
				RefNoteID = refNoteID;
				DocumentNoteID = documentNoteID;
			}

			public static Recipient SmartCreate(EntityHelper helper, Contact contact,
				object entity = null,
				CRLead lead = null, // the same as contact (contact is lead)
				string format = NotificationFormat.Html)
			{
				entity = entity ?? contact;
				return lead != null && lead.ContactID != null
					?
						new Recipient(
							contact,
							format,
							entity: entity,
							bAccountID: contact.BAccountID,
							contactID: lead.RefContactID,
							refNoteID: helper.GetNoteIDAndEnsureNoteExists(contact),
							documentNoteID: helper.GetNoteIDAndEnsureNoteExists(entity)
						)
					:
						new Recipient(
							contact,
							format,
							entity: entity,
							bAccountID: contact.BAccountID,
							contactID: contact.ContactID,
							documentNoteID: helper.GetNoteIDAndEnsureNoteExists(entity)
						);
			}

			public Contact Contact { get; }
			public object Entity { get; }
			public string Format { get; }
			public Guid? DocumentNoteID { get; }
			public Guid? RefNoteID { get; }
			public int? BAccountID { get; }
			public int? ContactID { get; }

			public TemplateNotificationGenerator GetSender(CRMassMail massMail)
			{
				var sender = TemplateNotificationGenerator.Create(Contact);

				sender.MailAccountId = massMail.MailAccountID ?? MailAccountManager.DefaultMailAccountID;
				sender.To = massMail.MailTo;
				sender.Cc = massMail.MailCc;
				sender.Bcc = massMail.MailBcc;
				sender.Body = massMail.MailContent ?? string.Empty;
				sender.Subject = massMail.MailSubject;
				sender.AttachmentsID = massMail.NoteID;

				sender.BAccountID = BAccountID;
				sender.BodyFormat = Format;
				sender.RefNoteID = RefNoteID;
				sender.ContactID = ContactID;
				sender.DocumentNoteID = DocumentNoteID;

				return sender;
			}
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
		public PXSelect<CRMassMail>
			MassMails;

		[PXViewName(Messages.History)]
		[PXFilterable]
		[PXCopyPasteHiddenView]
		[PXViewDetailsButton]
		[PXViewDetailsButton(typeof(CRMassMail),
			typeof(Select<Contact,
				Where<Contact.contactID, Equal<Current<CRSMEmail.contactID>>>>),
			ActionName = "History_Contact_ViewDetails")]
		[PXViewDetailsButton(typeof(CRMassMail),
			typeof(Select<BAccountCRM,
				Where<BAccountCRM.bAccountID, Equal<Current<CRSMEmail.bAccountID>>>>),
			ActionName = "History_BAccount_ViewDetails")]
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
		//public PXSelectJoin<Contact,
		//	LeftJoin<BAccount, On<BAccount.bAccountID, Equal<Contact.bAccountID>>,
		//	LeftJoin<Address, On<Address.addressID, Equal<Contact.defAddressID>>,
		//	LeftJoin<CRMassMailMember,
		//		On<CRMassMailMember.contactID, Equal<Contact.contactID>,
		//			And<CRMassMailMember.massMailID, Equal<Current<CRMassMail.massMailID>>>>>>>,
		//	Where2<
		//		Where<Contact.noMassMail, IsNull, Or<Contact.noMassMail, NotEqual<True>>>,
		//		And2<Where<Contact.noEMail, IsNull, Or<Contact.noEMail, NotEqual<True>>>,
		//		And<Where<Contact.noMarketing, IsNull, Or<Contact.noMarketing, NotEqual<True>>>>>>,
		//	OrderBy<Asc<Contact.displayName, Asc<Contact.contactID>>>>
		//	Leads;
		public PXSelectJoin<Contact,
			LeftJoin<BAccount, On<BAccount.bAccountID, Equal<Contact.bAccountID>>,
			LeftJoin<Address, On<Address.addressID, Equal<Contact.defAddressID>>,
			LeftJoin<CRMassMailMember,
				On<CRMassMailMember.contactID, Equal<Contact.contactID>,
					And<CRMassMailMember.massMailID, Equal<Current<CRMassMail.massMailID>>>>,
			LeftJoin<CRLead, On<CRLead.contactID, Equal<Contact.contactID>>>>>>,
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
			foreach (PXResult row in Leads.View.QuickSelect())
			{
				var contact = row.GetItem<Contact>();
				var mailLead = row.GetItem<CRMassMailMember>();
				if (contact.Selected != true && mailLead.ContactID != null &&
					Leads.Cache.GetStatus(contact) != PXEntryStatus.Updated)
				{
					contact.Selected = true;
				}
				yield return new PXResult<Contact, BAccount, Address, CRLead>(
					contact,
					row.GetItem<BAccount>(),
					row.GetItem<Address>(),
					row.GetItem<CRLead>()
				);
			}
		}

		#endregion

		#region Event Handlers

		[PXSelector(typeof(Contact.contactID), DescriptionField = typeof(Contact.memberName))]
		[PXMergeAttributes(Method = MergeMethod.Merge)]
		protected virtual void _(Events.CacheAttached<CRSMEmail.contactID> e) { }

		[PXSelector(typeof(BAccount.bAccountID), DescriptionField = typeof(BAccount.acctName))]
		[PXMergeAttributes(Method = MergeMethod.Merge)]
		protected virtual void _(Events.CacheAttached<CRSMEmail.bAccountID> e) { }

		[PXFormula(typeof(EntityDescription<CRSMEmail.documentNoteID>))]
		[PXMergeAttributes(Method = MergeMethod.Merge)]
		protected virtual void _(Events.CacheAttached<CRSMEmail.documentSource> e) { }

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
			if (Preview.AskExt((graph, name) =>
				{
					Preview.Current.MailAccountID = MassMails.Current.MailAccountID ?? MailAccountManager.DefaultMailAccountID;
					Preview.Current.MailTo = MailAccountManager.GetDefaultEmailAccount(false)?.Address;
				})
				== WebDialogResult.OK)
			{
				Preview.View.Answer = WebDialogResult.OK;
				CheckFields(Preview.Cache, Preview.Current,
							typeof(CRMassMailPreview.mailAccountID),
							typeof(CRMassMailPreview.mailTo));
				Preview.View.Answer = WebDialogResult.None;

				// todo: probably should be IQueryable to optimize FirstOrDefault request
				var recipient = EnumerateRecipientsForSending().FirstOrDefault();
				if (recipient == null) throw new PXException(Messages.RecipientsNotFound);

				var massMail = PXCache<CRMassMail>.CreateCopy(MassMails.Current);
				massMail.MailAccountID = Preview.Current.MailAccountID;
				massMail.MailTo = Preview.Current.MailTo;
				massMail.MailCc = null;
				massMail.MailBcc = null;

				AddSendedMessages(
					massMail,
					new Recipient(recipient.Contact, recipient.Format).GetSender(massMail).Send()
				);

				Actions.PressSave();
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

		public PXAction<SMEmail> ViewEntity;
		[PXUIField(DisplayName = "", Visible = false)]
		[PXButton]
		protected virtual IEnumerable viewEntity(PXAdapter adapter)
		{
			var row = History.Current;
			if (row != null)
			{
				new EntityHelper(this).NavigateToRow(row.RefNoteID, PXRedirectHelper.WindowMode.NewWindow);
			}

			return adapter.Get();
		}

		public PXAction<SMEmail> ViewDocument;
		[PXUIField(DisplayName = "", Visible = false)]
		[PXButton]
		protected virtual IEnumerable viewDocument(PXAdapter adapter)
		{
			var row = History.Current;
			if (row != null)
			{
				new EntityHelper(this).NavigateToRow(row.DocumentNoteID, PXRedirectHelper.WindowMode.NewWindow);
			}

			return adapter.Get();
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
			if (MassMails.Current == null
				|| MassMails.Current.Status == CRMassMailStatusesAttribute.Send)
				return;

			if (MassMails.View.Answer == WebDialogResult.No)
			{
				this.Caches<CRLead>().Clear();
				this.Caches<Contact>().Clear();
				this.Caches<BAccount>().Clear();
				this.Caches<EPEmployee>().Clear();
				this.Caches<CRMarketingListMember>().Clear();
				this.Caches<CRMarketingList>().Clear();
				this.Caches<CRCampaign>().Clear();
				return;
			}

			// TODO: need to get rid of 2 selects. Now it happens twice because of Ask

			if(MassMails.View.Answer == WebDialogResult.Yes)
			{
				var massMail = PXCache<CRMassMail>.CreateCopy(MassMails.Current);
				PXLongOperation.StartOperation(this, () => ProcessMassMailEmails(MassMails.Current, GetRecipientsForSendingDistinctByEmail()));
			}
			else
			{
				var mails = GetRecipientsForSendingDistinctByEmail();
				if (mails.Count == 0) throw new PXException(Messages.RecipientsNotFound);
				this.Caches<Note>().Clear();

				var confirmMessage = PXMessages.LocalizeFormatNoPrefix(Messages.MassMailSend, mails.Count);

				MassMails.Ask(Messages.Confirmation, confirmMessage, MessageButtons.YesNo);
			}
		}

		private static void ProcessMassMailEmails(CRMassMail massMail, IEnumerable<Recipient> recipients)
		{
			var processor = PXGraph.CreateInstance<CRMassMailMaint>();
			processor.EnsureCachePersistence(typeof(Note));

			foreach (Recipient recipient in recipients)
			{
				var cache = processor.Caches[recipient.Entity.GetType()];
				cache.SetStatus(recipient.Entity, PXEntryStatus.Notchanged);

				IEnumerable<CRSMEmail> messages;
				try
				{
					messages = recipient.GetSender(massMail).Send();
				}
				catch (Exception e)
				{
					PXTrace.WriteError(new PXException(Messages.FailedToSendMassEmail, e));

					continue;
				}

				processor.AddSendedMessages(massMail, messages);
			}

			processor.MassMails.Current = massMail;
			processor.MassMails.Current.Status = CRMassMailStatusesAttribute.Send;
			processor.MassMails.Current.SentDateTime = DateTime.Now;
			processor.MassMails.UpdateCurrent();
			processor.Actions.PressSave();
		}

		private void AddSendedMessages(CRMassMail massMail, IEnumerable<CRSMEmail> messages)
		{
			foreach (CRSMEmail message in messages)
			{
				SendedMessages.Insert(
					new CRMassMailMessage
					{
						MassMailID = massMail.MassMailID,
						MessageID = message.ImcUID,
					});
			}
		}

		private List<Recipient> GetRecipientsForSendingDistinctByEmail()
		{
			return EnumerateRecipientsForSending()
				.GroupBy(i => i.Contact.EMail)
				.Select(i => i.OrderByDescending(c => c.Contact.ContactPriority /*leads first*/).First())
				.ToList();
		}

		private IEnumerable<Recipient> EnumerateRecipientsForSending()
		{
			var helper = new EntityHelper(this);
			IEnumerable<Recipient> recipients;
			switch (MassMails.Current.Source)
			{
				case CRMassMailSourcesAttribute.MailList:
					recipients = EnumerateRecipientsForMailList(helper);
					break;

				case CRMassMailSourcesAttribute.Campaign:
					recipients = EnumerateRecipientsForCampaign(helper);
					break;

				case CRMassMailSourcesAttribute.Lead:
					recipients = EnumerateRecipientsForLeads(helper);
					break;

				default:
					yield break;
			}
			foreach (var recipient in recipients)
			{
				if (string.IsNullOrEmpty(recipient.Contact.EMail))
					continue;
				yield return recipient;
			}
		}

		private IEnumerable<Recipient> EnumerateRecipientsForMailList(EntityHelper helper)
		{
			ResetView(MailListsExt.View);
			foreach (CRMarketingList list in MailLists.Select().RowCast<CRMarketingList>().Where(l => l.Selected == true))
			{
				var marketingGraph = PXGraph.CreateInstance<CRMarketingListMaint>();
				marketingGraph.MailLists.Current = list;

				foreach (PXResult row in marketingGraph.MailRecipients.Select())
				{
					var contact = row.GetItem<Contact>();
					var subscription = row.GetItem<CRMarketingListMember>();
					var lead = row.GetItem<CRLead>();

					if (subscription?.IsSubscribed != true
						|| (contact.NoMassMail | contact.NoEMail | contact.NoMarketing) == true)
						continue;

					yield return Recipient.SmartCreate(helper, contact, entity: list, lead: lead, format: subscription?.Format ?? NotificationFormat.Html);
				}
			}
		}

		private IEnumerable<Recipient> EnumerateRecipientsForCampaign(EntityHelper helper)
		{
			ResetView(Campaigns.View);
			foreach (CRCampaign campaign in Campaigns.Select().RowCast<CRCampaign>().Where(c => c.Selected == true))
			{
				Campaigns.Cache.Current = campaign;

				foreach (var (contact, _, lead) in
					SelectFrom<Contact>
					.InnerJoin<CRCampaignMembers>
						.On<CRCampaignMembers.contactID.IsEqual<Contact.contactID>>
					.LeftJoin<CRLead>
						.On<Contact.contactType.IsEqual<ContactTypesAttribute.lead>.And<CRLead.contactID.IsEqual<Contact.contactID>>>
					.Where<
						CRCampaignMembers.campaignID.IsEqual<@P.AsString>
						.And<
							Brackets<
								GDPR.ContactExt.pseudonymizationStatus.IsEqual<PXPseudonymizationStatusListAttribute.notPseudonymized>
								.Or<GDPR.ContactExt.pseudonymizationStatus.IsNull>
							>
						>
						.And<True.IsNotIn<
							Contact.noMassMail,
							Contact.noEMail,
							Contact.noMarketing>>
						.And<
							Brackets<
								@P.AsString.IsNotEqual<SendFilterSourcesAttribute.neverSent>
								.Or<CRCampaignMembers.emailSendCount.IsEqual<Zero>>>
							>
						>
					.View
					.ReadOnly
					.Select(this, campaign.CampaignID, campaign.SendFilter)
					.Cast<PXResult<Contact, CRCampaignMembers, CRLead>>())
				{
					yield return Recipient.SmartCreate(helper, contact, entity: campaign, lead: lead);
				}
			}
		}

		private IEnumerable<Recipient> EnumerateRecipientsForLeads(EntityHelper helper)
		{
			// emails only for selected (should be cached)
			foreach (var (contact, lead) in Leads
				.Select()
				.ToList()
				.Select(i => (contact: i.GetItem<Contact>(), lead: i.GetItem<CRLead>()))
				.Where(i => i.contact.Selected == true))
			{
				yield return Recipient.SmartCreate(helper, contact, lead: lead);
			}
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
