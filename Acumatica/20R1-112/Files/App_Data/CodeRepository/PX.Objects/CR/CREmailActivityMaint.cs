using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Web.Compilation;
using PX.Common;
using PX.Common.Mail;
using PX.CS;
using PX.Data;
using PX.Data.EP;
using PX.Data.Wiki.Parser;
using PX.Objects.CS;
using PX.Objects.EP;
using PX.SM;
using PX.TM;
using FileInfo = PX.SM.FileInfo;
using PX.Data.RichTextEdit;
using PX.Web.UI;
using System.Net.Mail;
using PX.Objects.Common;
using PX.Objects.Common.GraphExtensions.Abstract;

namespace PX.Objects.CR
{
	public class CREmailActivityMaint : CRBaseActivityMaint<CREmailActivityMaint, CRSMEmail>
	{

		#region Extensions

		public class EmbeddedImagesExtractor
			: EmbeddedImagesExtractorExtension<CREmailActivityMaint, CRSMEmail, CRSMEmail.body>
				.WithFieldForExceptionPersistence<CRSMEmail.exception>
		{
		}

		private readonly EmbeddedImagesExtractor _extractor;

		#endregion

		public class TemplateSourceType
		{
			public class ListAttribute : PXStringListAttribute
			{
				public ListAttribute()
					: base(
						 new string[] { Notification, Activity, KnowledgeBase },
						 new string[] { Messages.EmailNotificationTemplate, Messages.EmailActivityTemplate, Messages.KnowledgeBaseArticle }
					) { }
			}
			public class ShortListAttribute : PXStringListAttribute
			{
				public ShortListAttribute()
					: base(
						 new string[] { KnowledgeBase },
						 new string[] { Messages.KnowledgeBaseArticle }
					) { }
			}

			public const string Notification = "NO";
			public const string Activity = "AC";
			public const string KnowledgeBase = "KB";

			public class notification : PX.Data.BQL.BqlString.Constant<notification>
			{
				public notification() : base(Notification) { }
			}
			public class activity : PX.Data.BQL.BqlString.Constant<activity>
			{
				public activity() : base(Activity) { }
			}
		}

		#region NotificatonFilter
		[Serializable]
		[PXHidden]
		public partial class NotificationFilter : IBqlTable
		{

			#region Type
			public abstract class type : PX.Data.BQL.BqlString.Field<type> { }
			[PXString(2, IsFixed = true)]
			[PXDefault(TemplateSourceType.Notification)]
			[TemplateSourceType.List]
			[PXUIField(DisplayName = "Source Type", Visibility = PXUIVisibility.Visible, Enabled = true)]
			public virtual string Type { get; set; }
			#endregion

			#region NotificationName
			public abstract class notificationName : PX.Data.BQL.BqlString.Field<notificationName> { }

			[PXSelector(typeof(Notification.name), DescriptionField = typeof(Notification.name))]
			[PXString(255, InputMask = "", IsUnicode = true)]
			[PXUIField(DisplayName = "Template")]
			public virtual string NotificationName { get; set; }
			#endregion

			#region TemplateActivity
			public abstract class templateActivity : PX.Data.BQL.BqlInt.Field<templateActivity> { }

			[PXGuid]
			[PXUIField(DisplayName = "Activity", Visibility = PXUIVisibility.SelectorVisible)]
			[PXSelector(typeof(Search<CRActivity.noteID,
				 Where<CRActivity.refNoteID, Equal<Current<CRSMEmail.refNoteID>>,
					And<CRActivity.classID, Equal<CRActivityClass.email>>>>),
				 typeof(CRActivity.subject),
				 SubstituteKey = typeof(CRActivity.subject))]
			public virtual Guid? TemplateActivity { get; set; }
			#endregion

			#region PageID
			public abstract class pageID : PX.Data.BQL.BqlGuid.Field<pageID> { }

			protected Guid? _PageID;
			[PXDBGuid(IsKey = true)]
			[PXUIField(DisplayName = "Page ID", Visibility = PXUIVisibility.SelectorVisible)]
			public virtual Guid? PageID { get; set; }
			#endregion

			#region PageName
			public abstract class pageName : PX.Data.BQL.BqlString.Field<pageName> { }
			protected String _PageName;
			[PXString()]
			[PXUIField(Visibility = PXUIVisibility.SelectorVisible, DisplayName = "Article ID")]
			public virtual String PageName
			{
				get
				{
					return this._PageName;
				}
				set
				{
					this._PageName = value;
				}
			}
			#endregion

			#region AppendText
			public abstract class appendText : PX.Data.BQL.BqlBool.Field<appendText> { }

			[PXBool]
			[PXUIField(DisplayName = "Append", Visibility = PXUIVisibility.SelectorVisible)]
			[PXDefault(true)]
			public virtual bool? AppendText { get; set; }
			#endregion
		}
		#endregion

		#region SendEmailParams
		public class SendEmailParams
		{
			private readonly IList<FileInfo> _attachments;

			public SendEmailParams()
			{
				_attachments = new List<FileInfo>();
			}

			public IList<FileInfo> Attachments
			{
				get { return _attachments; }
			}

			public string From { get; set; }

			public string To { get; set; }

			public string Cc { get; set; }

			public string Bcc { get; set; }

			public string Subject { get; set; }

			public string Body { get; set; }

			public object Source { get; set; }

			public object ParentSource { get; set; }

			public string TemplateID { get; set; }
		}
		#endregion

		#region Selects
		
		[PXRefNoteSelector(typeof(CRSMEmail), typeof(CRSMEmail.refNoteID))]
		[PXCopyPasteHiddenFields(typeof(CRSMEmail.body))]
		public PXSelect<CRSMEmail>
			Message;

		public PXSelect<PMTimeActivity>
			TAct;

		public PXSelect<CRSMEmail, Where<CRSMEmail.noteID, Equal<Current<CRSMEmail.noteID>>>>
			CurrentMessage;

		[PXHidden]
		[Obsolete(InternalMessages.FieldIsObsoleteAndWillBeRemoved2019R2)]
		public PXSelect<CRActivity,
			Where<CRActivity.noteID, Equal<Current<CRSMEmail.noteID>>>>
			CRAct;

		[PXHidden]
		public PXSelect<SMEmail,
			Where<SMEmail.refNoteID, Equal<Current<CRSMEmail.noteID>>>>
			SMMail;

		public PMTimeActivityList<CRSMEmail>
			TimeActivity;
		
		[PXHidden]
		public PXSetup<CRSetup>
			crSetup;

		[PXHidden]
		public PXSelect<CT.Contract>
			 BaseContract;

		public PXSelect<Notification> Notification;

		public PX.Objects.SM.SPWikiCategoryMaint.PXSelectWikiFoldersTree Folders;

		[PXViewName(Messages.NotificationTemplate)]
		public PXFilter<NotificationFilter> NotificationInfo;

		#endregion

		#region Ctors

		public CREmailActivityMaint()
		{
			PXStringListAttribute actionSource =
				PXAccess.FeatureInstalled<FeaturesSet.customerModule>() ? (PXStringListAttribute)new EntityList() : new EntityListSimple();

			Create.SetMenu(actionSource.ValueLabelDic.Select(entity => new ButtonMenu(entity.Key, PXMessages.LocalizeFormatNoPrefix(entity.Value), null)).ToArray());

			FieldVerifying.AddHandler(typeof(UploadFile), typeof(UploadFile.name).Name, UploadFileNameFieldVerifying);

			CRCaseActivityHelper.Attach(this);

			PXView relEntityView = new PXView(this, true, new Select<CRSMEmail>(), new PXSelectDelegate(GetRelatedEntity));
			Views.Add("RelatedEntity", relEntityView);
			
			Action.AddMenuAction(ReplyAll);
			Action.AddMenuAction(Forward);
			Action.AddMenuAction(LoadEmailSource);
			Action.AddMenuAction(process);
			Action.AddMenuAction(CancelSending);
			Action.AddMenuAction(DownloadEmlFile);
			Action.AddMenuAction(Archive);
			Action.AddMenuAction(RestoreArchive);
			Action.AddMenuAction(Restore);
			_extractor = GetExtension<EmbeddedImagesExtractor>();
		}

		#endregion

		#region Actions

		public class PXEMailActivityDelete<TNode> : PXDelete<TNode>
			where TNode : class, IBqlTable, new()
		{
			public PXEMailActivityDelete(PXGraph graph, string name)
				: base(graph, name)
			{
			}

			[PXUIField(DisplayName = ActionsMessages.Delete, MapEnableRights = PXCacheRights.Delete, MapViewRights = PXCacheRights.Delete)]
			[PXDeleteButton(ConfirmationMessage = ActionsMessages.ConfirmDeleteExplicit)]
			protected override IEnumerable Handler(PXAdapter adapter)
			{
				CREmailActivityMaint graph = (CREmailActivityMaint)adapter.View.Graph;
				foreach (CRSMEmail record in adapter.Get())
				{
					CRSMEmail newMessage = (CRSMEmail)graph.Message.Cache.CreateCopy(record);
					graph.TryCorrectMailDisplayNames(newMessage);
					if (newMessage.MPStatus != MailStatusListAttribute.Deleted)
					{
						newMessage.MPStatus = MailStatusListAttribute.Deleted;
						newMessage.UIStatus = ActivityStatusAttribute.Canceled;
						newMessage = graph.Message.Update(newMessage);
						foreach (Type field in graph.Message.Cache.BqlFields.Where(f => f != typeof(CRSMEmail.mpstatus)
							&& f != typeof(CRSMEmail.noteID)
							&& f != typeof(CRSMEmail.emailNoteID)
							&& f != typeof(CRSMEmail.uistatus)
							&& f != typeof(CRActivity.noteID)
							&& f != typeof(CRActivity.uistatus)
							&& f != typeof(SMEmail.noteID)
							))
						{
							graph.CommandPreparing.AddHandler(typeof(CRSMEmail), field.Name,
								 (sender, args) => { if (args.Operation == PXDBOperation.Update) args.Cancel = true; });
						}
					}
					else
						newMessage = graph.Message.Delete(newMessage);
					graph.Actions.PressSave();
					yield return newMessage;
				}
			}
		}

		public PXEMailActivityDelete<CRSMEmail> Delete;

		public PXAction<CRSMEmail> Send;
		[PXUIField(DisplayName = Messages.Send, MapEnableRights = PXCacheRights.Select)]
		[PXSendMailButton]
		protected virtual IEnumerable send(PXAdapter adapter)
		{
			var activity = Message.Current;
			if (activity == null) return new CRSMEmail[0];

			var res = new[] { activity };
			if (activity.MPStatus != ActivityStatusListAttribute.Draft &&
					activity.MPStatus != MailStatusListAttribute.Failed)
			{
				return res;
			}

			if (!VerifyEmailFields(activity))
			{
				throw new PXException();
			}
			Actions.PressSave();

			var newMessage = (CRSMEmail)Message.Cache.CreateCopy(activity);
			TryCorrectMailDisplayNames(newMessage);
			newMessage.MPStatus = MailStatusListAttribute.PreProcess;
			newMessage.RetryCount = 0;
			newMessage = (CRSMEmail)Message.Cache.Update(newMessage);
			this.SaveClose.Press();

			return new[] { newMessage };
		}

		public PXAction<CRSMEmail> Forward;
		[PXUIField(DisplayName = Messages.Forward, MapEnableRights = PXCacheRights.Select)]
		[PXForwardMailButton]
		protected void forward()
		{
			var targetGraph = CreateTargetMail(Message.Current);
			throw new PXRedirectRequiredException(targetGraph, true, "Forward")
			{
				Mode = PXBaseRedirectException.WindowMode.NewWindow
			};
		}

		public PXAction<CRSMEmail> Reply;

		[PXUIField(DisplayName = Messages.Reply, MapEnableRights = PXCacheRights.Select)]
		[PXReplyMailButton]
		protected IEnumerable reply(PXAdapter adapter)
		{
			foreach (CRSMEmail oldMessage in adapter.Get())
			{
				var targetGraph = CreateTargetMail(oldMessage, GetReplyAddress(oldMessage));

				PXRedirectHelper.TryRedirect(targetGraph, PXRedirectHelper.WindowMode.Same);
				yield return oldMessage;
			}
		}

		public PXAction<CRSMEmail> ReplyAll;
		[PXUIField(DisplayName = Messages.ReplyAll, MapEnableRights = PXCacheRights.Select)]
		[PXReplyMailButton]
		protected void replyAll()
		{
			var oldMessage = Message.Current;
			var mailAccountAddress = GetMailAccountAddress(oldMessage);
			var targetGraph = CreateTargetMail(oldMessage,
					GetReplyAllAddress(oldMessage, mailAccountAddress),
					GetReplyAllCCAddress(oldMessage, mailAccountAddress),
					GetReplyAllBCCAddress(oldMessage, mailAccountAddress)
				);

			PXRedirectHelper.TryRedirect(targetGraph, PXRedirectHelper.WindowMode.NewWindow);
		}

		public override IEnumerable ExecuteSelect(string viewName, object[] parameters, object[] searches, string[] sortcolumns,
			bool[] descendings, PXFilterRow[] filters, ref int startRow, int maximumRows, ref int totalRows)
		{			
			return base.ExecuteSelect(viewName, parameters, searches, sortcolumns, descendings, filters, ref startRow, maximumRows, ref totalRows);
		}

		public virtual CREmailActivityMaint CreateTargetMail(CRSMEmail oldMessage, string replyTo = null, string cc = null, string bcc = null)
		{
			var targetGraph = PXGraph.CreateInstance<CREmailActivityMaint>();
			var message = targetGraph.Message.Insert();

			if (IsAccountAccessable(oldMessage.MailAccountID))
				message.MailAccountID = oldMessage.MailAccountID;

			message.RefNoteID = oldMessage.RefNoteID;
			message.BAccountID = oldMessage.BAccountID;
			message.ContactID = oldMessage.ContactID;
			message.ParentNoteID = oldMessage.NoteID;
			message.IsIncome = false;
			message.Subject = GetSubjectPrefix(oldMessage.Subject, replyTo == null);
			message.MailTo = replyTo;
			message.MailCc = cc;
			message.MailBcc = bcc;

			message.MessageId = "<" + Guid.NewGuid() + "_acumatica" + GetMessageIDAppendix(this, message) + ">";

			message.Body = CreateReplyBody(oldMessage.MailFrom, oldMessage.MailTo, oldMessage.MailCc, oldMessage.Subject,
				oldMessage.Body, (DateTime)oldMessage.LastModifiedDateTime);

			if (PXOwnerSelectorAttribute.BelongsToWorkGroup(this, oldMessage.WorkgroupID, this.Accessinfo.UserID))
				message.WorkgroupID = oldMessage.WorkgroupID;

			targetGraph.Message.Update(message);
			message.NoteID = PXNoteAttribute.GetNoteID<CRSMEmail.noteID>(targetGraph.Message.Cache, message);
			//message.ProjectID = oldMessage.ProjectID;
			//message.ProjectTaskID = oldMessage.ProjectTaskID;
			targetGraph.Message.Update(message);

			CopyAttachments(targetGraph, oldMessage, message, replyTo != null);

			targetGraph.Message.Cache.IsDirty = false;
			return targetGraph;
		}

		public PXAction<CRSMEmail> process;
		[PXUIField(DisplayName = "Process", MapEnableRights = PXCacheRights.Select)]
		[PXButton(ImageUrl = "PX.Web.UI.Images.Data.pinionStart.gif", DisabledImageUrl = "PX.Web.UI.Images.Data.pinionStartD.gif")]
		protected void Process()
		{			
			var message = SMMail.SelectSingle();
			message.RetryCount = MailAccountManager.GetEmailPreferences().RepeatOnErrorSending;
			PXLongOperation.StartOperation(this, delegate() { ProcessMessage(message); });			
		}

		public static void ProcessMessage(SMEmail message)
		{
			if (MailAccountManager.IsMailProcessingOff) throw new PXException(EP.Messages.MailProcessingIsTurnedOff);

			if (message != null &&
				(message.MPStatus == MailStatusListAttribute.PreProcess ||
				message.MPStatus == MailStatusListAttribute.Failed))
			{
				if (message.IsIncome == true)
					EMailMessageReceiver.ProcessMessage(message);
				else
					MailSendProvider.SendMessage(message);
				if (!string.IsNullOrEmpty(message.Exception))
					throw new PXException(message.Exception);
			}
		}

		public PXAction<CRSMEmail> CancelSending;
		[PXUIField(DisplayName = EP.Messages.CancelSending, MapEnableRights = PXCacheRights.Select)]
		[PXButton(ImageUrl = "~/Icons/Cancel_Active.gif",
			DisabledImageUrl = "~/Icons/Cancel_NotActive.gif",
			Tooltip = EP.Messages.CancelSendingTooltip)]
		public virtual void cancelSending()
		{
			var message = Message.Current;
			if (message != null && message.MPStatus == MailStatusListAttribute.PreProcess)
			{
				var newMessage = (CRSMEmail)Message.Cache.CreateCopy(message);
				newMessage.MPStatus = ActivityStatusAttribute.Draft;
				Message.Cache.Update(newMessage);
				Actions.PressSave();
			}
		}

		public PXAction<CRSMEmail> DownloadEmlFile;
		[PXUIField(DisplayName = EP.Messages.DownloadEmlFile)]
		[PXButton(ImageUrl = "~/Icons/Eml_Active.gif",
			DisabledImageUrl = "~/Icons/Eml_NotActive.gif",
			Tooltip = EP.Messages.DownloadEmlFileTooltip)]
		public virtual void downloadEmlFile()
		{
			var message = SMMail.SelectSingle();
			if (message != null && message.IsIncome == true)
			{
				var mail = EMailMessageReceiver.GetOriginalMail(message);
				throw PXExportHandlerEml.GenerateException(mail);
			}
		}

		public PXAction<CRSMEmail> LoadEmailSource;
		[PXUIField(DisplayName = "Select Source", MapEnableRights = PXCacheRights.Select)]
		[PXButton(Tooltip = Messages.SelectTemplateTooltip)]
		public virtual void loadEmailSource()
		{
			WebDialogResult res = NotificationInfo.AskExt();
			if (res == WebDialogResult.OK)
			{
				if (NotificationInfo.Current.Type == TemplateSourceType.Notification)
				{
					Notification notification = PXSelect<Notification,
						 Where<Notification.name, Equal<Required<Notification.name>>>>.
						 Select(this, NotificationInfo.Current.NotificationName);
					if (notification == null) return;

					object row = new EntityHelper(this).GetEntityRow(Message.Current.RefNoteID);
					if (row == null) return;
					Type graphType;
					var type = row.GetType();
					PXPrimaryGraphAttribute.FindPrimaryGraph(this.Caches[type], true, ref row, out graphType);

					var templateGraph = PXGraph.CreateInstance(graphType);
					var keys = GetKeys(row, templateGraph.Caches[type]);
					EntityHelper eh = new EntityHelper(templateGraph);
					templateGraph.Caches[type].Current = eh.GetEntityRow(type, keys);
					Notification upd = PXCache<PX.SM.Notification>.CreateCopy(notification);
					upd.Subject = PXTemplateContentParser.Instance.Process(notification.Subject, templateGraph, type, null);
					upd.Body = PXTemplateContentParser.Instance.Process(notification.Body, templateGraph, type, null);
					upd.NTo = PXTemplateContentParser.Instance.Process(notification.NTo, templateGraph, type, null);
					upd.NCc = PXTemplateContentParser.Instance.Process(notification.NCc, templateGraph, type, null);
					upd.NBcc = PXTemplateContentParser.Instance.Process(notification.NBcc, templateGraph, type, null);

					List<Guid> _attachments = new List<Guid>();
					foreach (NoteDoc noteDoc in PXSelect<NoteDoc, Where<NoteDoc.noteID, Equal<Required<NoteDoc.noteID>>>>.Select(this, upd.NoteID))
					{
						_attachments.Add((Guid)noteDoc.FileID);
					}
					PXNoteAttribute.SetFileNotes(Message.Cache, Message.Current, _attachments.ToArray());

					if (upd.NFrom.HasValue && IsAccountAccessable(upd.NFrom))
					{
						Message.Current.MailAccountID = upd.NFrom.Value;
						Message.Current.MailFrom = FillMailFrom(this, Message.Current, true);
						Message.Current.MailReply = FillMailReply(this, Message.Current);
					}

					if (NotificationInfo.Current.AppendText == true)
					{
						Message.Current.MailTo = PXDBEmailAttribute.AppendAddresses(Message.Current.MailTo, upd.NTo);
						Message.Current.MailCc = PXDBEmailAttribute.AppendAddresses(Message.Current.MailCc, upd.NCc);
						Message.Current.MailBcc = PXDBEmailAttribute.AppendAddresses(Message.Current.MailBcc, upd.NBcc);
						Message.Current.Subject += upd.Subject;
						Message.Current.Body = upd.Body + Message.Current.Body;
					}
					else
					{
						Message.Current.MailTo = upd.NTo;
						Message.Current.MailCc = upd.NCc;
						Message.Current.MailBcc = upd.NBcc;
						Message.Current.Body = upd.Body;
						Message.Current.Subject = upd.Subject;
					}
				}
				if (NotificationInfo.Current.Type == TemplateSourceType.Activity)
				{
					SMEmail email = PXSelect<SMEmail,
						 Where<SMEmail.refNoteID, Equal<Required<SMEmail.refNoteID>>>>.
						 Select(this, NotificationInfo.Current.TemplateActivity);
					if (email == null) return;

					if (NotificationInfo.Current.AppendText == true)
					{
						Message.Current.Body = Message.Current.Body + email.Body;
					}
					else
					{
						Message.Current.Body = email.Body;
					}
				}
				if (NotificationInfo.Current.Type == TemplateSourceType.KnowledgeBase)
				{
					PXResult<WikiPage, WikiPageLanguage, WikiRevision> result = (PXResult<WikiPage, WikiPageLanguage, WikiRevision>)PXSelectJoin<WikiPage,
						InnerJoin<WikiPageLanguage, 
							On<WikiPageLanguage.pageID, Equal<WikiPage.pageID>>,
						InnerJoin<WikiRevision, 
							On<WikiRevision.pageID, Equal<WikiPage.pageID>>>>,
						Where<WikiPage.name, Equal<Required<WikiPage.name>>>,
						OrderBy<Desc<WikiRevision.pageRevisionID>>>.SelectSingleBound(new PXGraph(), null, NotificationInfo.Current.PageName);

					if (result == null) return;

					WikiRevision wr = result[typeof(WikiRevision)] as WikiRevision;

					if (wr == null) return;

					if (NotificationInfo.Current.AppendText == true)
					{
						Message.Current.Body = Message.Current.Body + PXWikiParser.Parse(wr.Content);
					}
					else
					{
						Message.Current.Body = PXWikiParser.Parse(wr.Content);
					}
				}
			}
            Message.Current.Body = PX.Web.UI.PXRichTextConverter.NormalizeHtml(Message.Current.Body);

			if (this.IsContractBasedAPI)
				this.Save.Press();
		}

		public PXMenuAction<CRSMEmail> Action;

		public PXAction<CRSMEmail> Create;
		[PXUIField(DisplayName = "Create")]
		[PXButton(MenuAutoOpen = true)]
		public virtual IEnumerable create(PXAdapter adapter)
		{
			if (string.IsNullOrEmpty(adapter.Menu))
				return adapter.Get();

			if (adapter.Menu == ExpenseReceipt &&
				!EmployeeMaint.GetCurrentEmployeeID(this).HasValue)
			{
				throw new PXException(Messages.MustBeEmployee, Accessinfo.DisplayName);
			}

			PXGraph graph = CreateInstance(GraphTypes[adapter.Menu]);
			PXCache cache = graph.Views[graph.PrimaryView].Cache;
			object entity = cache.Insert();
			Contact contact = entity as Contact;
            CRCase newCase = entity as CRCase;
            CROpportunity opp = entity as CROpportunity;
			CRActivity activity = entity as CRActivity;

			Type listType = graph.GetType().GetFields().Where(f => typeof(IActivityList).IsAssignableFrom(f.FieldType)).Select(f => f.FieldType).FirstOrDefault();
			while (listType != null && !string.Equals(listType.Name, typeof(CRActivityListBase<,>).Name))
			{
				listType = listType.BaseType;
			}
			List<CRSMEmail> sourceemails = adapter.Get().RowCast<CRSMEmail>().ToList();
			if (listType == null) return sourceemails;

			int pos = listType.GetGenericTypeDefinition().GetGenericArguments().Where(t => string.Equals(t.Name, "TActivity")).Select(t => t.GenericParameterPosition).FirstOrDefault();
			Type activityType = listType.GetGenericArguments()[pos];
			PXCache activityCache = graph.Caches[activityType];

			HashSet<EmailAddress> names = new HashSet<EmailAddress>();

			List<CRSMEmail> emails = adapter.MassProcess ? sourceemails.Where(e => e.Selected == true).ToList() : sourceemails;

			CRSMEmail mainEmail = null;

			var view = new PXView(this, false,
				BqlCommand.CreateInstance(
					BqlCommand.Compose(
						typeof(Select<,>), activityType,
						typeof(Where<,>), activityType.GetNestedType(typeof(CRActivity.noteID).Name),
						typeof(Equal<>), typeof(Required<>), activityType.GetNestedType(typeof(CRActivity.noteID).Name))));

			foreach (CRSMEmail emailFrame in emails)
			{
				if (mainEmail == null)
			        mainEmail = emailFrame;

				CRActivity email = (CRActivity)view.SelectSingle(emailFrame.NoteID);

				if (activity != null && activity.ClassID != CRActivityClass.Task && activity.ClassID != CRActivityClass.Event)
				{
					email.ContactID = activity.ContactID;
                    email.BAccountID = activity.BAccountID;
                    email.RefNoteID = activity.RefNoteID;
                }
				else
				{
					email.RefNoteID = PXNoteAttribute.GetNoteID(cache, entity, EntityHelper.GetNoteField(cache.GetItemType()));
				    email.ContactID = contact?.ContactID;
				}

				//if (email.MPStatus == MailStatusListAttribute.Failed)
				//	email.MPStatus = MailStatusListAttribute.Processed;
				//email.Exception = null;

				object updated = email;
				if (activityType != email.GetType())
				{ 
					updated = activityCache.CreateInstance();
					activityCache.RestoreCopy(updated, email);
				}

				updated = activityCache.Update(updated);
			    if (activity != null)
			    {
					activity.Subject = mainEmail.Subject;
					cache.Update(activity);
					activityCache.SetValue<CRActivity.parentNoteID>(updated, activity.NoteID);
			    }

			    EmailAddress name;
                if ((contact != null || opp != null) && names.Count <= 1 && (name = ParseNames(emailFrame.MailFrom)) != null)
                {
                    names.Add(name);
				}
			}
			if (contact != null && names.Count == 1)
			{
				contact.LastName = string.IsNullOrEmpty(names.ToArray()[0].LastName) ? names.ToArray()[0].Email : names.ToArray()[0].LastName;
				contact.FirstName = names.ToArray()[0].FirstName;
				contact.EMail = names.ToArray()[0].Email;
				cache.Update(contact);
			}
		    if (newCase != null && mainEmail!= null)
		    {
		        newCase.Subject = mainEmail.Subject;
		        newCase.Description = mainEmail.Body;
		        cache.Update(newCase);
		    }
            if (opp != null && mainEmail != null)
            {
                opp.Subject = mainEmail.Subject;
                opp.Details = mainEmail.Body;
                cache.Update(opp);

                PXCache CRContactCache = graph.Caches[typeof(CRContact)];
                if (CRContactCache != null && names.Count == 1)
                {
                    CRContact crcontact = graph.Views["Opportunity_Contact"].SelectSingle() as CRContact;
                    if (crcontact != null)
                    {
                        opp.AllowOverrideContactAddress = true;
                        crcontact.LastName = names.ToArray()[0].LastName;
                        crcontact.FirstName = names.ToArray()[0].FirstName;
                        crcontact.Email = names.ToArray()[0].Email;
                    }
                    CRContactCache.Update(crcontact);
                    cache.Update(opp);
                }
            }
			
			if (!this.IsContractBasedAPI)
				PXRedirectHelper.TryRedirect(graph, !adapter.ExternalCall ? PXRedirectHelper.WindowMode.InlineWindow : PXRedirectHelper.WindowMode.NewWindow);

			graph.Actions.PressSave();

			return emails;
		}

		public PXAction<CRSMEmail> Archive;
		[PXUIField(DisplayName = "Archive", MapEnableRights = PXCacheRights.Update)]
		[PXButton]
		protected virtual IEnumerable archive(PXAdapter adapter)
		{
			foreach (CRSMEmail newMessage in adapter.Get().Cast<CRSMEmail>())
			{
				if (!(newMessage.IsArchived ?? false))
				{
					newMessage.IsArchived = true;
					Message.Update(newMessage);
				}
				Actions.PressSave();
				yield return newMessage;
			}
		}

		public PXAction<CRSMEmail> RestoreArchive;
		[PXUIField(DisplayName = "Restore from Archive", MapEnableRights = PXCacheRights.Update)]
		[PXButton]
		protected virtual IEnumerable restoreArchive(PXAdapter adapter)
		{
			foreach (CRSMEmail newMessage in adapter.Get().Cast<CRSMEmail>())
			{
				if (newMessage.IsArchived ?? false)
				{
					newMessage.IsArchived = false;
					Message.Update(newMessage);
				}
				Actions.PressSave();
				yield return newMessage;
			}
		}

		public PXAction<CRSMEmail> Restore;
		[PXUIField(DisplayName = "Restore Deleted", MapEnableRights = PXCacheRights.Update)]
		[PXButton]
		protected virtual IEnumerable restore(PXAdapter adapter)
		{
			foreach (CRSMEmail newMessage in adapter.Get().Cast<CRSMEmail>().Select(record => (CRSMEmail)Message.Cache.CreateCopy(record)))
			{
				CRSMEmail msg = newMessage;
				if (newMessage.MPStatus == MailStatusListAttribute.Deleted)
				{
					newMessage.MPStatus = newMessage.IsIncome == true
						? MailStatusListAttribute.Processed
						: MailStatusListAttribute.Draft;
					msg = Message.Update(newMessage);
				}
				Actions.PressSave();
				yield return msg;
			}
		}
		#endregion

		#region Data Handlers

		public IEnumerable GetRelatedEntity()
		{
			var current = Message.Current;
			if (current != null && current.RefNoteID != null)
			{
				var row = new EntityHelper(this).GetEntityRow(current.RefNoteID);
				if (row != null) yield return row;
			}
		}

		public override void Persist()
		{
			using (PXTransactionScope ts = new PXTransactionScope())
			{
				base.Persist();

				CorrectFileNames();

				ts.Complete();
			}

			this.Actions.PressCancel();
		}

		#endregion

		#region Event Handlers


		[PXUIField(DisplayName = "Parent", Enabled = false)]
		[PXMergeAttributes(Method = MergeMethod.Merge)]
		protected virtual void CRSMEmail_ParentNoteID_CacheAttached(PXCache cache) { }

		[PXEMailAccountIDSelector()]
		[PXRestrictor(typeof(Where<
			EMailAccount.emailAccountType, Equal<EmailAccountTypesAttribute.standard>,
			Or<Where<
				EMailAccount.emailAccountType, Equal<EmailAccountTypesAttribute.exchange>,
				And<EMailAccount.defaultOwnerID, Equal<Current<AccessInfo.userID>>>>>>), ErrorMessages.RestrictedEmailAccount, typeof(EMailAccount.description))]
		[PXDefault]
		[PXMergeAttributes(Method = MergeMethod.Merge)]
		protected void CRSMEmail_MailAccountID_CacheAttached(PXCache sender) { }

		[PXUIField(DisplayName = "Incoming")]
		[PXMergeAttributes(Method = MergeMethod.Merge)]
		protected void CRSMEmail_IsIncome_CacheAttached(PXCache sender) { }

		[CREmailSelector]
		[PXDefault( PersistingCheck = PXPersistingCheck.NullOrBlank)]
		[PXMergeAttributes(Method = MergeMethod.Merge)]
		protected void CRSMEmail_MailTo_CacheAttached(PXCache sender) { }

		[CREmailSelector]
		[PXMergeAttributes(Method = MergeMethod.Merge)]
		protected void CRSMEmail_MailCc_CacheAttached(PXCache sender) { }

		[CREmailSelector]
		[PXMergeAttributes(Method = MergeMethod.Merge)]
		protected void CRSMEmail_MailBcc_CacheAttached(PXCache sender) { }

		[PXDefault(MailStatusListAttribute.Draft)]
		[PXMergeAttributes(Method = MergeMethod.Merge)]
		protected virtual void CRSMEmail_MPStatus_CacheAttached(PXCache cache) { }

		[PXUIField(DisplayName = "Subject", Visibility = PXUIVisibility.SelectorVisible)]
		[PXMergeAttributes(Method = MergeMethod.Merge)]
		protected virtual void CRSMEmail_Subject_CacheAttached(PXCache sender) { }
		
		[PXParent(typeof(Select<CRSMEmail, Where<CRSMEmail.noteID, Equal<Current<PMTimeActivity.refNoteID>>>>), ParentCreate = true)]
		[PXMergeAttributes(Method = MergeMethod.Merge)]
		protected void PMTimeActivity_RefNoteID_CacheAttached(PXCache sender) { }

		[PXSubordinateGroupSelector]
		[PXMergeAttributes(Method = MergeMethod.Merge)]
		protected void PMTimeActivity_WorkgroupID_CacheAttached(PXCache sender) { }

		[PXFormula(typeof(Switch<
			Case<Where<PMTimeActivity.trackTime, Equal<True>>, ActivityStatusAttribute.open,
			Case<Where<PMTimeActivity.released, Equal<True>>, ActivityStatusAttribute.released,
			Case<Where<PMTimeActivity.approverID, IsNotNull>, ActivityStatusAttribute.pendingApproval>>>,
			ActivityStatusAttribute.completed>))]
		[PXMergeAttributes(Method = MergeMethod.Merge)]
		protected void _(Events.CacheAttached<PMTimeActivity.approvalStatus> e) { }

		protected virtual void NotificationFilter_RowSelected(PXCache cache, PXRowSelectedEventArgs e)
		{
			NotificationFilter row = (NotificationFilter)e.Row;
			if (row == null) return;

			bool isNotification = row.Type.Equals(TemplateSourceType.Notification);
			bool isKBArticle = row.Type.Equals(TemplateSourceType.KnowledgeBase);
			bool isActivity = row.Type.Equals(TemplateSourceType.Activity);
			PXUIFieldAttribute.SetVisible<NotificationFilter.templateActivity>(cache, row, isActivity);
			PXUIFieldAttribute.SetVisible<NotificationFilter.notificationName>(cache, row, isNotification);
			PXUIFieldAttribute.SetVisible<NotificationFilter.pageID>(cache, row, isKBArticle);
			PXUIFieldAttribute.SetVisible<NotificationFilter.pageName>(cache, row, isKBArticle);
		}

		protected virtual void CRSMEmail_Body_FieldDefaulting(PXCache cache, PXFieldDefaultingEventArgs e)
		{
			var signature = GetSignature(true);
			if (!string.IsNullOrEmpty(signature))
				e.NewValue = PX.Web.UI.PXRichTextConverter.NormalizeHtml(Tools.AppendToHtmlBody(e.NewValue as string, "<br />" + signature));
		}

		protected virtual void CRSMEmail_RowSelected(PXCache cache, PXRowSelectedEventArgs e)
		{
			CRSMEmail row = (CRSMEmail)e.Row;
			if (row == null)
			{
				//It's need for correct working outlook plugin.
				Forward.SetEnabled(true);
				Reply.SetEnabled(true);
				ReplyAll.SetEnabled(true);
				Forward.SetVisible(true);
				Reply.SetVisible(true);
				ReplyAll.SetVisible(true);
				return;
			}

			var tAct = (PMTimeActivity)TimeActivity.SelectSingle();
			var tActCache = TimeActivity.Cache;

			
			var wasUsed = !string.IsNullOrEmpty(tAct?.TimeCardCD) || tAct?.Billed == true;
			if (wasUsed)
				PXUIFieldAttribute.SetEnabled(cache, row, false);

			var showMinutes = EPSetupCurrent.RequireTimes == true;			
			PXDBDateAndTimeAttribute.SetTimeVisible<CRSMEmail.startDate>(cache, row, true);
			PXDBDateAndTimeAttribute.SetTimeEnabled<CRSMEmail.startDate>(cache, row, true);
			PXDBDateAndTimeAttribute.SetTimeVisible<CRSMEmail.endDate>(cache, row, showMinutes && tAct?.TrackTime == true);

			string origStatus =
				(string)this.Message.Cache.GetValueOriginal<CRSMEmail.uistatus>(row) ?? ActivityStatusListAttribute.Open;

			bool? oringTrackTime =
				(bool?)this.TimeActivity.Cache.GetValueOriginal<PMTimeActivity.trackTime>(tAct) ?? false;

			if (origStatus == ActivityStatusAttribute.Completed && oringTrackTime != true)
				origStatus = ActivityStatusAttribute.Open;

			if (row.IsLocked == true)
				origStatus = ActivityStatusAttribute.Completed;

			PXUIFieldAttribute.SetEnabled(cache, row, row.MPStatus == MailStatusListAttribute.Draft || row.MPStatus == MailStatusListAttribute.Failed);

			if (origStatus == ActivityStatusListAttribute.Open)
			{
				PXUIFieldAttribute.SetEnabled<CRSMEmail.isExternal>(cache, row, true);
			}
			else
			{
				PXUIFieldAttribute.SetEnabled<CRSMEmail.isExternal>(cache, row, false);
			}

			PXUIFieldAttribute.SetEnabled<CRSMEmail.parentNoteID>(cache, row, IsContractBasedAPI);
			PXUIFieldAttribute.SetEnabled<CRSMEmail.mpstatus>(cache, row, IsContractBasedAPI);
			PXUIFieldAttribute.SetEnabled<CRSMEmail.startDate>(cache, row, IsContractBasedAPI);
			PXUIFieldAttribute.SetVisible<CRSMEmail.uistatus>(cache, row, false);
			PXUIFieldAttribute.SetEnabled<CRSMEmail.source>(cache, row, false);
			PXUIFieldAttribute.SetEnabled<CRSMEmail.type>(cache, row, false);
			PXUIFieldAttribute.SetEnabled<CRSMEmail.isPrivate>(cache, row, true);
			PXUIFieldAttribute.SetEnabled<CRSMEmail.ownerID>(cache, row, (origStatus == ActivityStatusListAttribute.Open && row.MPStatus == MailStatusListAttribute.Draft && row.IsLocked != true) || row.IsIncome == true);
			PXUIFieldAttribute.SetEnabled<CRSMEmail.workgroupID>(cache, row, (origStatus == ActivityStatusListAttribute.Open && row.MPStatus == MailStatusListAttribute.Draft && row.IsLocked != true) || row.IsIncome == true);

			row.EntityDescription = CacheUtility.GetErrorDescription(row.Exception) + GetEntityDescription(row);

			GotoParentActivity.SetEnabled(row.ParentNoteID != null);

			var isIncome = row.IsIncome == true;

			// TODO: clear redundant Enables
			Create.SetEnabled(isIncome);
			Create.SetVisible(isIncome);
			Send.SetVisible(!isIncome && (row.MPStatus == MailStatusListAttribute.Failed || row.MPStatus == MailStatusListAttribute.Draft));

			Action.SetVisible(nameof(loadEmailSource), row.IsIncome != true);
			Action.SetVisible(nameof(downloadEmlFile), isIncome);

			Action.SetVisible(nameof(cancelSending), !isIncome && row.MPStatus == MailStatusListAttribute.PreProcess);
			CancelSending.SetEnabled(!isIncome && row.MPStatus == MailStatusListAttribute.PreProcess);

			var account = PXSelectorAttribute.Select<CRSMEmail.mailAccountID>(cache, row) as EMailAccount;
			this.Action.SetVisible(nameof(Process), account?.EmailAccountType != EmailAccountTypesAttribute.Exchange);
			process.SetEnabled(row.MPStatus == MailStatusListAttribute.PreProcess || (isIncome && row.MPStatus == MailStatusListAttribute.Failed));

			var isInserted = cache.GetStatus(row) == PXEntryStatus.Inserted;

			Forward.SetEnabled(!isInserted);
			Reply.SetEnabled(!isInserted);
			ReplyAll.SetEnabled(!isInserted);
			DownloadEmlFile.SetEnabled(isIncome);
			LoadEmailSource.SetEnabled(!isIncome && row.MPStatus == MailStatusListAttribute.Draft);

			PXUIFieldAttribute.SetRequired<CRSMEmail.ownerID>(cache, !this.UnattendedMode && tAct?.TrackTime == true && row.MPStatus != MailStatusListAttribute.Deleted);

            if(row.IsIncome == true)
			    MarkAs(cache, row, Accessinfo.UserID, EPViewStatusAttribute.VIEWED);

			Archive.SetEnabled(row.IsArchived == false);
			Action.SetVisible(nameof(archive), row.IsArchived == false);
			RestoreArchive.SetEnabled(row.IsArchived == true);
			Action.SetVisible(nameof(restoreArchive), row.IsArchived == true);

			Restore.SetEnabled(row.MPStatus == MailStatusListAttribute.Deleted);
			Action.SetVisible(nameof(restore), row.MPStatus == MailStatusListAttribute.Deleted);

			if (row.ClassID == -2)
			{
				PXUIFieldAttribute.SetEnabled(cache, row, false);
				PXUIFieldAttribute.SetEnabled<CRSMEmail.isPrivate>(cache, row, true);
			}

			PXStringListAttribute.SetList<NotificationFilter.type>(
				 NotificationInfo.Cache,
				 null,
			  row.RefNoteID == null
					? (PXStringListAttribute)new TemplateSourceType.ShortListAttribute()
					: new TemplateSourceType.ListAttribute());
			PXDefaultAttribute.SetDefault<NotificationFilter.type>(NotificationInfo.Cache,
				 null,
				 row.RefNoteID == null ? TemplateSourceType.KnowledgeBase : TemplateSourceType.Notification);
		}

		protected virtual void PMTimeActivity_RowSelected(PXCache cache, PXRowSelectedEventArgs e)
		{
			PMTimeActivity row = (PMTimeActivity)e.Row;

			var email = this.CurrentMessage.Current;
			if (email == null) return;

			bool isPmVisible = PM.ProjectAttribute.IsPMVisible(GL.BatchModule.TA);
			PXUIFieldAttribute.SetVisible<PMTimeActivity.trackTime>(cache, null, email.IsIncome != true);
			PXUIFieldAttribute.SetVisible<PMTimeActivity.approvalStatus>(cache, null, row?.TrackTime == true && email.IsIncome != true);
			PXUIFieldAttribute.SetVisible<PMTimeActivity.timeSpent>(cache, null, row?.TrackTime == true);
			PXUIFieldAttribute.SetVisible<PMTimeActivity.earningTypeID>(cache, null, row?.TrackTime == true);
			PXUIFieldAttribute.SetVisible<PMTimeActivity.isBillable>(cache, null, row?.TrackTime == true);
			PXUIFieldAttribute.SetVisible<PMTimeActivity.released>(cache, null, row?.TrackTime == true);
			PXUIFieldAttribute.SetVisible<PMTimeActivity.timeBillable>(cache, null, row?.IsBillable == true && row?.TrackTime == true);
			PXUIFieldAttribute.SetVisible<PMTimeActivity.overtimeBillable>(cache, null, row?.IsBillable == true && row?.TrackTime == true);
			PXUIFieldAttribute.SetVisible<PMTimeActivity.approverID>(cache, null, row?.TrackTime == true);
			PXUIFieldAttribute.SetVisible<PMTimeActivity.overtimeSpent>(cache, null, false);
			PXUIFieldAttribute.SetVisible<PMTimeActivity.overtimeBillable>(cache, null, false);
			PXUIFieldAttribute.SetVisible<PMTimeActivity.projectID>(cache, null, row?.TrackTime == true && isPmVisible);
			PXUIFieldAttribute.SetVisible<PMTimeActivity.certifiedJob>(cache, null, row?.TrackTime == true && isPmVisible);
			PXUIFieldAttribute.SetVisible<PMTimeActivity.projectTaskID>(cache, null, row?.TrackTime == true && isPmVisible);
			PXUIFieldAttribute.SetVisible<PMTimeActivity.costCodeID>(cache, null, row?.TrackTime == true && isPmVisible);
			PXUIFieldAttribute.SetVisible<PMTimeActivity.labourItemID>(cache, null, row?.TrackTime == true && isPmVisible);
			PXUIFieldAttribute.SetVisible<PMTimeActivity.unionID>(cache, null, row?.TrackTime == true);
			PXUIFieldAttribute.SetVisible<PMTimeActivity.workCodeID>(cache, null, row?.TrackTime == true);

			string origStatus =
				(string)this.Message.Cache.GetValueOriginal<CRSMEmail.uistatus>(email)
				?? ActivityStatusListAttribute.Open;

			bool? oringTrackTime =
				(bool?)this.TimeActivity.Cache.GetValueOriginal<PMTimeActivity.trackTime>(row)
				?? false;

			string origTimeStatus =
				(string)this.TimeActivity.Cache.GetValueOriginal<PMTimeActivity.approvalStatus>(row)
				?? ActivityStatusListAttribute.Open;

			if (origStatus == ActivityStatusAttribute.Completed && oringTrackTime != true)
				origStatus = ActivityStatusAttribute.Open;

			if (email.IsLocked == true)
				origStatus = ActivityStatusAttribute.Completed;

			if (origStatus != ActivityStatusListAttribute.Open)
			{
				PXUIFieldAttribute.SetEnabled<PMTimeActivity.trackTime>(cache, row, email.IsLocked != true);
			}

			var wasUsed = !string.IsNullOrEmpty(row?.TimeCardCD) || row?.Billed == true;

			Delete.SetEnabled(!wasUsed && row?.Released != true);

			// TimeActivity
			if (row?.Released == true)
				origTimeStatus = ActivityStatusAttribute.Completed;

			if (origTimeStatus == ActivityStatusListAttribute.Open)
			{
				PXUIFieldAttribute.SetEnabled(cache, row, true);

				PXUIFieldAttribute.SetEnabled<PMTimeActivity.timeBillable>(cache, row, !wasUsed && row?.IsBillable == true);
				PXUIFieldAttribute.SetEnabled<PMTimeActivity.overtimeBillable>(cache, row, !wasUsed && row?.IsBillable == true);
			}
			else
			{
				PXUIFieldAttribute.SetEnabled(cache, row, false);
			}

			PXUIFieldAttribute.SetEnabled<PMTimeActivity.approvalStatus>(cache, row, row != null && row.TrackTime == true && !wasUsed);
			PXUIFieldAttribute.SetEnabled<PMTimeActivity.released>(cache, row, false);

		}

		protected override void TMasterFieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
		{
			CRSMEmail email = (CRSMEmail)e.Row;
			if (email != null && email.IsIncome == true)
			{
				email.Exception = null;
			}
		}

		protected virtual void CRSMEmail_RefNoteID_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
		{
			CRSMEmail email = (CRSMEmail)e.Row;
			if (email == null) return;

			if (Caches[typeof(RelatedEntity)].Current != null)
			{
				var related = Views["RelatedEntity"].SelectSingle();
				if (related == null) return;

				var relatedType = related.GetType();

				if (typeof(Contact).IsAssignableFrom(relatedType))
				{
					Contact contact = related as Contact;

					if (contact?.ContactType != ContactTypesAttribute.Person)
						return;

					email.ContactID = contact?.ContactID;
					email.BAccountID = contact?.BAccountID;
				}
				else if (typeof(BAccount).IsAssignableFrom(relatedType))
				{
					BAccount contact = related as BAccount;
					email.ContactID = null;
					email.BAccountID = contact?.BAccountID;
				}
			}
		}

		protected virtual void CRSMEmail_OwnerID_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
		{
			CRSMEmail email = (CRSMEmail)e.Row;
			if (email != null && email.OwnerID != null && email.OwnerID != (Guid?)e.OldValue && email.IsIncome == true)
			{
				MarkAs(sender, email, (Guid)email.OwnerID, EPViewStatusAttribute.NOTVIEWED);
			}
		}

		protected virtual void CRSMEmail_MailAccountID_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
		{
			var row = e.Row as CRSMEmail;
			if (row == null) return;

			row.MessageId = "<" + Guid.NewGuid() + "_acumatica" + GetMessageIDAppendix(this, row) + ">";
			row.MailFrom = FillMailFrom(this, row, true);
            row.MailReply = FillMailReply(this, row);
        }

        protected virtual void CRSMEmail_UIStatus_FieldVerifying(PXCache sender, PXFieldVerifyingEventArgs e)
		{
			e.Cancel = true;
		}
		protected virtual void CRSMEmail_RowInserted(PXCache cache, PXRowInsertedEventArgs e)
		{
			var row = e.Row as CRSMEmail;
			if (row == null) return;

			row.ClassID = CRActivityClass.Email;

			if (row.OwnerID == null)
			{
				var newOwner = EmployeeMaint.GetCurrentEmployeeID(this);
				if (PXOwnerSelectorAttribute.BelongsToWorkGroup(this, row.WorkgroupID, newOwner))
					row.OwnerID = newOwner;
			}

			if (row.MailAccountID == null && row.IsIncome != true && row.ClassID != CRActivityClass.EmailRouting)
				row.MailAccountID = GetDefaultAccountId(row.OwnerID);

			if (row.IsIncome != true && row.ClassID != CRActivityClass.EmailRouting)
				row.MailFrom = FillMailFrom(this, row, true);

			row.MailFrom = FillMailFrom(this, row, true);
			row.MailReply = FillMailReply(this, row);
			
			row.MessageId = "<" + Guid.NewGuid() + "_acumatica" + GetMessageIDAppendix(this, row) + ">";
		}

		protected virtual void CRSMEmail_RowUpdated(PXCache cache, PXRowUpdatedEventArgs e)
		{
			var row = e.Row as CRSMEmail;
			var oldRow = e.OldRow as CRSMEmail;
			if (row == null || oldRow == null) return;

            row.ClassID = CRActivityClass.Email;

			if (row.IsIncome != true && row.ClassID != CRActivityClass.EmailRouting && oldRow.OwnerID != row.OwnerID)
				row.MailFrom = FillMailFrom(this, row, true);

			if (row.IsIncome != true && row.ClassID != CRActivityClass.EmailRouting && oldRow.OwnerID != row.OwnerID || row.MailAccountID == null)
				row.MailAccountID = GetDefaultAccountId(row.OwnerID);
			if(row.MessageId == null)
                row.MessageId = "<" + Guid.NewGuid() + "_acumatica" + GetMessageIDAppendix(this, row) + ">";
        }

		protected virtual void CRSMEmail_RowDeleting(PXCache cache, PXRowDeletingEventArgs e)
		{
			var row = (CRSMEmail)e.Row;
			if (row == null) return;

			var timeAct = TimeActivity.Current;

			if (timeAct != null)
			{
				if (timeAct.Billed == true || !string.IsNullOrEmpty(timeAct.TimeCardCD))
				{
				cache.SetStatus(e.Row, PXEntryStatus.Notchanged);
				throw new PXException(TM.Messages.EmailActivityCannotBeDeleted);
			}

				TimeActivity.Cache.Delete(timeAct);
		}
		}


		protected virtual void UploadFileNameFieldVerifying(PXCache sender, PXFieldVerifyingEventArgs e)
		{
			e.Cancel = true;
		}

		protected virtual void CRSMEmail_RowPersisted(PXCache cache, PXRowPersistedEventArgs e)
		{
			var row = (CRSMEmail)e.Row;
			var tAct = (PMTimeActivity)TimeActivity.SelectSingle();

			PXDefaultAttribute.SetPersistingCheck<CRSMEmail.ownerID>(cache, row, !this.UnattendedMode && tAct?.TrackTime == true 
				&& row.MPStatus != MailStatusListAttribute.Deleted ? PXPersistingCheck.Null : PXPersistingCheck.Nothing);

			if (e.Operation != PXDBOperation.Insert || e.TranStatus != PXTranStatus.Completed)
			{
				return;
			}
			using (PXDataRecord msg =
				PXDatabase.SelectSingle<SMEmail>(
					new PXDataField(typeof(SMEmail.id).Name),
					new PXDataFieldValue(typeof(SMEmail.noteID).Name, row.EmailNoteID)))
			{
				if (msg != null)
				{
					cache.SetValue<CRSMEmail.id>(e.Row, msg.GetInt32(0));
				}
			}
		}

		protected virtual void _(Events.FieldUpdated<PMTimeActivity, PMTimeActivity.ownerID> e)
		{
			e.Cache.SetDefaultExt<PMTimeActivity.labourItemID>(e.Row);
		}
		protected virtual void _(Events.FieldUpdated<PMTimeActivity, PMTimeActivity.costCodeID> e)
		{
			e.Cache.SetDefaultExt<PMTimeActivity.workCodeID>(e.Row);
		}
		protected virtual void _(Events.FieldUpdated<PMTimeActivity, PMTimeActivity.projectID> e)
		{
			e.Cache.SetDefaultExt<PMTimeActivity.unionID>(e.Row);
			e.Cache.SetDefaultExt<PMTimeActivity.certifiedJob>(e.Row);
			e.Cache.SetDefaultExt<PMTimeActivity.labourItemID>(e.Row);
		}
		#endregion

		#region SendEmail


		public static void SendEmail(SendEmailParams sendEmailParams)
		{
            var graph = PXGraph.CreateInstance<CREmailActivityMaint>();
            graph.SendEmail(sendEmailParams, null);
		}

        protected virtual void SendEmail(SendEmailParams sendEmailParams, Action<CRSMEmail> handler)
		{
			var cache = Message.Cache;
			var activityCache = cache;
			var activityIsDirtyOld = activityCache.IsDirty;
			var newEmail = cache.NonDirtyInsert<CRSMEmail>(null);
			var owner = EP.EmployeeMaint.GetCurrentEmployeeID(this);

			newEmail.MailTo = sendEmailParams.To;
			newEmail.MailCc = sendEmailParams.Cc;
			newEmail.MailBcc = sendEmailParams.Bcc;
			newEmail.Subject = sendEmailParams.Subject;

			newEmail.MPStatus = ActivityStatusListAttribute.Draft;
			activityCache.IsDirty = activityIsDirtyOld;
			cache.Current = newEmail;

			var sourceType = sendEmailParams.Source.With(s => s.GetType());
			var sourceCache = sourceType.With(type => this.Caches[type]);
			Guid? refNoteId = sourceCache.With(c => PXNoteAttribute.GetNoteID(c, sendEmailParams.Source, EntityHelper.GetNoteField(sourceType)));
			newEmail.RefNoteID = refNoteId;

			var parentSourceType = sendEmailParams.ParentSource.With(s => s.GetType());
			var parentSourceCache = parentSourceType.With(type => Activator.CreateInstance(BqlCommand.Compose(typeof(PXCache<>), type), this) as PXCache);

			EntityHelper helper = new EntityHelper(this);
			if (parentSourceCache != null)
			{
				var parentSource = helper.GetEntityRow(parentSourceCache.GetItemType(),
					 helper.GetEntityRowKeys(parentSourceCache.GetItemType(), sendEmailParams.ParentSource));

				var fieldName = EntityHelper.GetIDField(parentSourceType);
				int? bAccountId = (int?)this.Caches[parentSourceType].GetValue(parentSource, fieldName);
				
				newEmail.BAccountID = bAccountId;
			}
			newEmail.Type = null;
			newEmail.IsIncome = false;
			var newBody = sendEmailParams.Body;
			newEmail.OwnerID = owner;
			newEmail.Subject = newEmail.Subject;
			if (!string.IsNullOrEmpty(sendEmailParams.TemplateID))
			{
				TemplateNotificationGenerator generator =
					 TemplateNotificationGenerator.Create(sendEmailParams.Source,
						  sendEmailParams.TemplateID);
				var template = generator.ParseNotification();

				if (string.IsNullOrEmpty(newBody))
					newBody = template.Body;
				if (string.IsNullOrEmpty(newEmail.Subject))
					newEmail.Subject = template.Subject;
				if (string.IsNullOrEmpty(newEmail.MailTo))
					newEmail.MailTo = template.To;
				if (string.IsNullOrEmpty(newEmail.MailCc))
					newEmail.MailCc = template.Cc;
				if (string.IsNullOrEmpty(newEmail.MailBcc))
					newEmail.MailBcc = template.Bcc;

				newEmail.MailAccountID = template.MailAccountId ?? MailAccountManager.DefaultMailAccountID;
				
				newEmail.MessageId = "<" + Guid.NewGuid() + "_acumatica" + GetMessageIDAppendix(cache.Graph, newEmail) + ">";

				if (template.AttachmentsID != null)
				{
					List<Guid> _attachmentLinks = new List<Guid>();

					foreach (NoteDoc doc in
						PXSelect<NoteDoc, Where<NoteDoc.noteID, Equal<Required<NoteDoc.noteID>>>>.Select(this, template.AttachmentsID))
					{
						if (doc.FileID != null && !_attachmentLinks.Contains(doc.FileID.Value))
							_attachmentLinks.Add(doc.FileID.Value);
					}

					PXNoteAttribute.SetFileNotes(activityCache, newEmail, _attachmentLinks.ToArray());
				}
			}
			else
			{
				if (!IsHtml(newBody))
					newBody = Tools.ConvertSimpleTextToHtml(newBody);

				var html = new StringBuilder();
				html.AppendLine("<html><body>");
				html.Append(Tools.RemoveHeader(newBody));
				html.Append("<br/>");
				html.Append(Tools.RemoveHeader(newEmail.Body));
				html.Append("</body></html>");
				newBody = html.ToString();
			}

			newEmail.MailFrom = FillMailFrom(this, newEmail);
			newEmail.MailReply = FillMailReply(this, newEmail);
			newEmail.Body = PX.Web.UI.PXRichTextConverter.NormalizeHtml(newBody);

			if (sendEmailParams.Attachments.Count > 0)
			{
				AttachFiles(newEmail, refNoteId, cache, sendEmailParams.Attachments);
			}
			if (handler != null) handler(newEmail);
			Caches[newEmail.GetType()].RaiseRowSelected(newEmail);
			throw new PXPopupRedirectException(this, this.GetType().Name, true);
		}

		private static string FillMailReply(PXGraph graph, CRSMEmail message)
		{
			string defaultDisplayName = null;
			string defaultAddress = null;

			if (message.MailAccountID != null)
			{
				EMailAccount account =
					PXSelectReadonly<EMailAccount,
						Where<EMailAccount.emailAccountID, Equal<Required<CRSMEmail.mailAccountID>>>>.Select(graph, message.MailAccountID);


				if (account != null)
				{
					defaultDisplayName = account.Description.With(_ => _.Trim());
					defaultAddress = account.ReplyAddress.With(_ => _.Trim());
					if (string.IsNullOrEmpty(defaultAddress))
						defaultAddress = account.Address.With(_ => _.Trim());

					if (account.SenderDisplayNameSource == SenderDisplayNameSourceAttribute.Account)
						return string.IsNullOrEmpty(account.AccountDisplayName)
							? defaultAddress
							: new MailAddress(defaultAddress, account.AccountDisplayName).ToString();
				}
				else
				{
					MailAddress mailReply;
					EmailParser.TryParse(message.MailFrom, out mailReply);

					defaultAddress = mailReply?.Address;
				}
			}

			return GenerateBackAddress(graph,
				message,
				defaultDisplayName,
				defaultAddress,
				true);
		}


		private static string FillMailFrom(PXGraph graph, CRSMEmail message, bool allowUseCurrentUser = false)
		{
			string defaultDisplayName = null;
			string defaultAddress = null;

			if (message.MailAccountID != null)
			{
				EMailAccount account =
					PXSelectReadonly<EMailAccount,
						Where<EMailAccount.emailAccountID, Equal<Required<CRSMEmail.mailAccountID>>>>.Select(graph, message.MailAccountID);

				MailAddress mailFrom;

				EmailParser.TryParse(message.MailFrom, out mailFrom);

				defaultDisplayName = account?.Description.With(_ => _.Trim());
				defaultAddress = account != null 
					? account.Address.With(_ => _.Trim()) 
					: mailFrom?.Address;

				if (account != null && account.SenderDisplayNameSource == SenderDisplayNameSourceAttribute.Account)
					return string.IsNullOrEmpty(account.AccountDisplayName)
						? defaultAddress
						: new MailAddress(defaultAddress, account.AccountDisplayName).ToString();
			}

			return GenerateBackAddress(graph, 
				message,
				defaultDisplayName,
				defaultAddress,
				allowUseCurrentUser);
		}

		private static string GenerateBackAddress(PXGraph graph, CRSMEmail message, string defaultDisplayName, string defaultAddress, bool allowUseCurrentUser)
		{
			string result = null;

			if (message != null)
			{
				if (message.OwnerID == null || message.ClassID == CRActivityClass.EmailRouting)
			{
					return string.IsNullOrEmpty(defaultDisplayName)
					? defaultAddress
					: new MailAddress(defaultAddress, defaultDisplayName).ToString();
				}

				var results = PXSelectReadonly2<Users,
					LeftJoin<Contact,
						On<Contact.userID, Equal<Users.pKID>>>,
				Where<Users.pKID, Equal<Required<Users.pKID>>>>.
					SelectWindowed(graph, 0, 1, message.OwnerID);

				if (results == null || results.Count == 0) return defaultAddress;

				var contact = (Contact)results[0][typeof(Contact)];
				var user = (Users)results[0][typeof(Users)];

				string displayName = null;
				string address = defaultAddress;
				if (user != null && user.PKID != null)
				{
					var userDisplayName = user.FullName.With(_ => _.Trim());
					if (!string.IsNullOrEmpty(userDisplayName))
						displayName = userDisplayName;
				}
				if (contact != null && contact.BAccountID != null)
				{
					var contactDisplayName = contact.DisplayName.With(_ => _.Trim());
					if (!string.IsNullOrEmpty(contactDisplayName))
						displayName = contactDisplayName;
				}

				result = string.IsNullOrEmpty(displayName) || string.IsNullOrEmpty(address)
					? address
					: new MailAddress(address, displayName).ToString();
			}

			if (string.IsNullOrEmpty(result) && allowUseCurrentUser)
			{
				return graph.Accessinfo.UserID.
					With(id => (Users)PXSelect<Users>.Search<Users.pKID>(graph, id)).
					With(u => PXDBEmailAttribute.FormatAddressesWithSingleDisplayName(u.Email, u.FullName));
			}

			return result;
		}

		private static bool IsHtml(string text)
		{
			if (!string.IsNullOrEmpty(text))
			{
				var htmlIndex = text.IndexOf("<html", StringComparison.CurrentCultureIgnoreCase);
				var bodyIndex = text.IndexOf("<body", StringComparison.CurrentCultureIgnoreCase);
				return htmlIndex > -1 && bodyIndex > -1 && bodyIndex > htmlIndex;
			}
			return false;
		}

		private static object[] GetKeys(object e, PXCache cache)
		{
			var keys = new List<object>();

			foreach (Type t in cache.BqlKeys)
				keys.Add(cache.GetValue(e, t.Name));

			return keys.ToArray();
		}
		#endregion

		#region Private Methods

		private bool VerifyEmailFields(CRSMEmail row)
		{
			var res = true;

			//From
			Message.Cache.RaiseExceptionHandling<CRSMEmail.mailAccountID>(row, null, null);
			if (row.MailAccountID == null)
			{
				var exception = new PXSetPropertyException(ErrorMessages.FieldIsEmpty);
				Message.Cache.RaiseExceptionHandling<CRSMEmail.mailAccountID>(row, null, exception);
				PXUIFieldAttribute.SetError<CRSMEmail.mailAccountID>(Message.Cache, row, exception.Message);
				res = false;
			}

			//To
			Message.Cache.RaiseExceptionHandling<CRSMEmail.mailTo>(row, null, null);
			if (string.IsNullOrWhiteSpace(row.MailTo))
			{
				var exception = new PXSetPropertyException(ErrorMessages.FieldIsEmpty);
				Message.Cache.RaiseExceptionHandling<CRSMEmail.mailTo>(row, null, exception);
				PXUIFieldAttribute.SetError<CRSMEmail.mailTo>(Message.Cache, row, exception.Message);
				res = false;
			}

			return res;
		}

		private Int32? GetDefaultAccountId(Guid? owner)
		{
			if (owner != null)
			{
				int? result = MailAccountManager.GetDefaultMailAccountID((Guid)owner, true);
				if (result != null && IsAccountAccessable(result))
					return result;
			}

			if(IsAccountAccessable(MailAccountManager.DefaultAnyMailAccountID))
			return MailAccountManager.DefaultAnyMailAccountID;
			return null;
		}

		public virtual string GetReplyAddress(CRSMEmail oldMessage)
		{
			var newAddressList = new List<MailAddress>();

			if (oldMessage.MailReply != null)
			{
				foreach (var item in EmailParser.ParseAddresses(oldMessage.MailReply))
				{
					string displayName = null;
					if (String.IsNullOrWhiteSpace(item.DisplayName) && oldMessage.MailFrom != null)
					{
						foreach (var item1 in EmailParser.ParseAddresses(oldMessage.MailCc)
							.Where(_ => string.Equals(_.Address, item.Address, StringComparison.OrdinalIgnoreCase)))
						{
							displayName = item1.DisplayName;
						}
					}
					else
					{
						displayName = item.DisplayName;
					}
					var newitem = new MailAddress(item.Address, displayName);
					newAddressList.Add(newitem);
				}
			}

			if (newAddressList.Count == 0 && oldMessage.MailFrom != null)
			{
				foreach (var item in EmailParser.ParseAddresses(oldMessage.MailFrom))
				{
					newAddressList.Add(new MailAddress(item.Address, item.DisplayName));
				}
			}

			return PXDBEmailAttribute.ToString(newAddressList);
		}

		public string GetMailAccountAddress(CRSMEmail oldMessage)
		{
			return oldMessage.MailAccountID.
				With(_ => (EMailAccount)PXSelect<EMailAccount,
					Where<EMailAccount.emailAccountID, Equal<Required<EMailAccount.emailAccountID>>>>.
				Select(this, _.Value)).
				With(_ => _.Address);
		}

		private static string GetMessageIDAppendix(PXGraph graph, CRSMEmail message)
		{
			if (message.MailAccountID == null) return ""; 

			var hostName = message.MailAccountID.
				With(_ => (EMailAccount) PXSelect<EMailAccount,
					Where<EMailAccount.emailAccountID, Equal<Required<EMailAccount.emailAccountID>>>>.
					Select(graph, _.Value)).
				With(_ => _.OutcomingHostName);

			if (hostName == null) return "";

			return "@" + hostName;
		}

		public virtual string GetReplyAllCCAddress(CRSMEmail oldMessage, string mailAccountAddress)
		{
			var newAddressList = new List<MailAddress>();

			if (oldMessage.MailCc != null)
			{
				foreach (var item in EmailParser.ParseAddresses(oldMessage.MailCc)
					.Where(_ => !string.Equals(_.Address, mailAccountAddress, StringComparison.OrdinalIgnoreCase)))
				{
					newAddressList.Add(new MailAddress(item.Address, item.DisplayName));
				}
			}
			
			return PXDBEmailAttribute.ToString(newAddressList);
		}

		public virtual string GetReplyAllBCCAddress(CRSMEmail oldMessage, string mailAccountAddress)
		{
			var newAddressList = new List<MailAddress>();

			if (oldMessage.MailBcc != null)
			{
				foreach (var item in EmailParser.ParseAddresses(oldMessage.MailBcc)
					.Where(_ => !string.Equals(_.Address, mailAccountAddress, StringComparison.OrdinalIgnoreCase)))
				{
					newAddressList.Add(new MailAddress(item.Address, item.DisplayName));
				}
			}
			
			return PXDBEmailAttribute.ToString(newAddressList);
		}

		public virtual string GetReplyAllAddress(CRSMEmail oldMessage, string mailAccountAddress)
		{
			var newAddressList = new List<MailAddress>();

			if (oldMessage.MailReply != null)
			{
				foreach (var item in EmailParser.ParseAddresses(oldMessage.MailReply))
				{
					string displayName = null;
					if (String.IsNullOrEmpty(item.DisplayName) && oldMessage.MailFrom != null)
					{
						foreach (var item1 in EmailParser.ParseAddresses(oldMessage.MailTo)
							.Where(_ => string.Equals(_.Address, item.Address, StringComparison.OrdinalIgnoreCase)))
						{
							displayName = item1.DisplayName;
						}
					}
					else
					{
						displayName = item.DisplayName;
					}
					newAddressList.Add(new MailAddress(item.Address, displayName));
				}
			}

			if (newAddressList.Count == 0 && oldMessage.MailFrom != null)
			{
				foreach (var item in EmailParser.ParseAddresses(oldMessage.MailFrom))
				{
					newAddressList.Add(new MailAddress(item.Address, item.DisplayName));
				}
			}

			if (oldMessage.MailTo != null)
			{
				foreach (var item in EmailParser.ParseAddresses(oldMessage.MailTo)
					.Where(_ => !string.Equals(_.Address, mailAccountAddress, StringComparison.OrdinalIgnoreCase)))
				{
					newAddressList.Add(new MailAddress(item.Address, item.DisplayName));
				}
			}
			
			return PXDBEmailAttribute.ToString(newAddressList);
		}

		private static string GetSubjectPrefix(string subject, bool forward)
		{
			bool startWith;
			if (subject != null)
				do
				{
					startWith = false;
					if (subject.ToUpper().StartsWith("RE: ") || subject.ToUpper().StartsWith("FW: "))
					{
						subject = subject.Substring(4);
						startWith = true;
					}
					if (subject.ToUpper().StartsWith(PXMessages.LocalizeNoPrefix(Messages.EmailReplyPrefix).ToUpper()))
					{
						subject = subject.Substring(Messages.EmailReplyPrefix.Length);
						startWith = true;
					}
					if (subject.ToUpper().StartsWith(PXMessages.LocalizeNoPrefix(Messages.EmailForwardPrefix).ToUpper()))
					{
						subject = subject.Substring(Messages.EmailForwardPrefix.Length);
						startWith = true;
					}
				} while (startWith);
			return (forward ? "FW: " : "RE: ") + subject;
		}
		
		private bool IsAccountAccessable(int? accountID)
		{
			EMailAccount acc = PXSelect<EMailAccount,
				Where<EMailAccount.emailAccountID, Equal<Required<EMailAccount.emailAccountID>>,
				And<Match<Current<AccessInfo.userName>>>>>.SelectSingleBound(this, null, accountID);
		    return acc != null && (acc.EmailAccountType != EmailAccountTypesAttribute.Exchange ||
		            acc.DefaultOwnerID == this.Accessinfo.UserID);
		}

		protected virtual string CreateReplyBody(string mailFrom, string mailTo, string subject, string message, DateTime lastModifiedDateTime)
		{
			return CreateReplyBody(mailFrom, mailTo, null, subject, message, lastModifiedDateTime);
		}

		private string CreateReplyBody(string mailFrom, string mailTo, string mailCc, string subject, string message, DateTime lastModifiedDateTime)
		{
			var wikiTitle =
				 "<br/><br/><div class=\"wiki\" style=\"border-top:solid 1px black;padding:2px 0px;line-height:1.5em;\">" +
				 "\r\n<b>From:</b> " + mailFrom +
				 "<br/>\r\n<b>Sent:</b> " + lastModifiedDateTime +
				 "<br/>\r\n<b>To:</b> " + mailTo +
				 (string.IsNullOrEmpty(mailCc) ? "" : "<br/>\r\n<b>Cc:</b> " + mailCc) +
				 "<br/>\r\n<b>Subject:</b> " + subject +
				 "<br/><br/>\r\n</div>";

            return PXRichTextConverter.NormalizeHtml(GetSignature(false) + wikiTitle + message);
		}

		// TODO: seems that there's no need in this method anymore after the addresses redesign
		private void TryCorrectMailDisplayNames(CRSMEmail message)
		{
			string ownerEmail = FillMailFrom(this, message);
			string ownerDisplayName = null;

			MailAddress fromBox;
			if (ownerEmail != null && EmailParser.TryParse(ownerEmail, out fromBox))
				ownerDisplayName = fromBox.DisplayName;

			if (ownerDisplayName == null)
			{
				ownerDisplayName =
				message.With(id => (EMailAccount)PXSelect<EMailAccount>.
					Search<EMailAccount.emailAccountID>(this, id.MailAccountID)).
				With(a => a.Description);
			}

			//from			
			var fromAddress = message.MailFrom;
			if (!string.IsNullOrEmpty(fromAddress) &&
				EmailParser.TryParse(fromAddress, out fromBox))
			{
				message.MailFrom = new MailAddress(fromBox.Address, ownerDisplayName).ToString();
				Caches[message.GetType()].Update(message);
			}

			//reply
			MailAddress replyBox;
			var replyAddress = message.MailReply;
			if (!string.IsNullOrEmpty(replyAddress) &&
				EmailParser.TryParse(replyAddress, out replyBox) &&
				!object.Equals(replyBox.DisplayName, ownerDisplayName))
			{
				message.MailReply = new MailAddress(replyBox.Address, ownerDisplayName).ToString();
				Caches[message.GetType()].Update(message);
			}
		}
		
		private void CorrectFileNames()
		{
			var noteId = Message.Current.With(m => m.NoteID);
			var actNoteId = Message.Current.With(act => act.NoteID);
			if (noteId == null || actNoteId == null) return;

			var searchText = "[" + Message.Current.MessageId + "]";
			var replaceText = "[" + Message.Current.NoteID + "]";
			var cache = Caches[typeof(UploadFile)];
			PXSelectJoin<UploadFile,
					InnerJoin<NoteDoc, On<NoteDoc.fileID, Equal<UploadFile.fileID>>>,
					Where<NoteDoc.noteID, Equal<Required<NoteDoc.noteID>>>>.
					Clear(this);
			foreach (UploadFile file in
				PXSelectJoin<UploadFile,
					InnerJoin<NoteDoc, On<NoteDoc.fileID, Equal<UploadFile.fileID>>>,
					Where<NoteDoc.noteID, Equal<Required<NoteDoc.noteID>>>>.
				Select(this, noteId))
			{
				if (!string.IsNullOrEmpty(file.Name) && file.Name.Contains(searchText))
				{
					file.Name = file.Name.Replace(searchText, replaceText);
					cache.PersistUpdated(file);
				}
			}
		}

		private string GetEntityDescription(CRActivity row)
		{
			string res = string.Empty;
			var helper = new EntityHelper(this);
			var entity = row.RefNoteID.With(_ => helper.GetEntityRow(_.Value, true));

			if (entity != null)
			{
				res = CacheUtility.GetDescription(helper, entity, entity.GetType());
			}

			return res;
		}

		protected static void AttachFiles(CRSMEmail newEmail, Guid? refNoteId, PXCache cache, IEnumerable<FileInfo> files)
		{
			var uploadFile = PXGraph.CreateInstance<UploadFileMaintenance>();
			var filesID = new List<Guid>();
			uploadFile.IgnoreFileRestrictions = true;
			foreach (FileInfo file in files)
			{
				var separator = file.FullName.IndexOf('\\') > -1 ? string.Empty : "\\";
				file.FullName = string.Format("[{0}] {2}{3}", newEmail.ImcUID, refNoteId, separator, file.FullName);
				uploadFile.SaveFile(file, FileExistsAction.CreateVersion);
				var uid = (Guid)file.UID;
				if (!filesID.Contains(uid))
					filesID.Add(uid);
			}
			cache.SetValueExt(newEmail, "NoteFiles", filesID.ToArray());
		}

		private string GetSignature(bool isNewMessage)
		{
			var userPref = ((UserPreferences)PXSelect<UserPreferences>.
				 Search<UserPreferences.userID>(this, PXAccess.GetUserID()));
			string signature = userPref.With(pref => pref.MailSignature);
			if (signature != null && (signature = signature.Trim()) != string.Empty)
			{
				if (isNewMessage)
				{
					if (userPref.SignatureToNewEmail != null && userPref.SignatureToNewEmail.Value)
						return signature;
				}
				else
				{
					if (userPref.SignatureToReplyAndForward != null && userPref.SignatureToReplyAndForward.Value)
						return signature;
				}
			}
			return string.Empty;
		}

		private void CopyAttachments(PXGraph targetGraph, CRSMEmail message, CRSMEmail newMessage, bool isReply)
		{
			if (message == null || newMessage == null) return;

			var filesIDs = PXNoteAttribute.GetFileNotes(Message.Cache, message);
			if (filesIDs == null || filesIDs.Length == 0) return;

			if (isReply && !String.IsNullOrEmpty(message.Body))
			{
				var t = PXSelectJoin<UploadFile,
						InnerJoin<NoteDoc, On<NoteDoc.fileID, Equal<UploadFile.fileID>>>,
						Where<NoteDoc.noteID, Equal<Required<NoteDoc.noteID>>>>.
					Select(this, message.NoteID).RowCast<UploadFile>().ToList();

				var extractor = new ImageExtractor();
				if(!extractor.FindImages(message.Body, t, out var found))
				{
					return;
				}
				filesIDs = found.ToArray();
			}

			PXNoteAttribute.SetFileNotes(targetGraph.Caches<CRSMEmail>(), newMessage, filesIDs);
		}

		private EPSetup EPSetupCurrent
		{
			get
			{
				return (EPSetup)PXSelect<EPSetup>.SelectWindowed(this, 0, 1) ?? EmptyEpSetup;
			}
		}
		private static readonly EPSetup EmptyEpSetup = new EPSetup();

		public class EmailAddress
		{
			public string FirstName { get; set; }
			public string LastName { get; set; }
			public string Email { get; set; }
		}

		public static EmailAddress ParseNames(string source)
		{
			List<string> patterns = new List<string>
			{
				@"'(?<first>[\w.@]+)\s+(?<last>[\w.@]+).*'\s+<(?<email>[^<>]+)>",
				@"'(?<last>[\w.@]+),\s+(?<first>[\w.@]+).*'\s+<(?<email>[^<>]+)>",
				@"""(?<first>[\w.@]+)\s+(?<last>[\w.@]+).*""\s+<(?<email>[^<>]+)>",
				@"""(?<last>[\w.@]+),\s+(?<first>[\w.@]+).*""\s+<(?<email>[^<>]+)>",
				@"(?<first>[\w.@]+)\s+(?<last>[\w.@]+).*\s+<(?<email>[^<>]+)>",
				@"(?<last>[\w.@]+),\s+(?<first>[\w.@]+).*\s+<(?<email>[^<>]+)>",
				@"'(?<last>[\w.@]+)'\s+<(?<email>[^<>]+)>",
				@"""(?<last>[\w.@]+)""\s+<(?<email>[^<>]+)>",
				@"(?<last>[\w.@]+)\s+<(?<email>[^<>]+)>",
				@"(?<email>[\w@.-]+)",
			};
			return (patterns.Select(pattern => new Regex(pattern, RegexOptions.IgnoreCase | RegexOptions.Singleline))
								 .Select(re => re.Match(source).Groups)
								 .Select(groups => new { groups, email_grp = groups["email"] })
								 .Where(@t => @t.email_grp.Success)
								 .Select(@t => new EmailAddress()
								 {
									 FirstName = @t.groups["first"].Value,
									 LastName = @t.groups["last"].Value,
									 Email = @t.email_grp.Value
								 })).FirstOrDefault();
		}

		public class EntityList : PXStringListAttribute
		{
			public EntityList()
				: base(
					 new string[] { Event, Task, Lead, Contact, Case, Opportunity, ExpenseReceipt },
					 new string[] { EP.Messages.Event, EP.Messages.Task, Messages.Lead, Messages.Contact, Messages.Case, Messages.Opportunity, EP.Messages.ExpenseReceipt }) { }
		}

		public class EntityListSimple : PXStringListAttribute
		{
			public EntityListSimple()
				: base(
					 new string[] { Event, Task },
					 new string[] { EP.Messages.Event, EP.Messages.Task }) { }
		}

		public const string Event = "E";
		public const string Task = "T";
		public const string Lead = "L";
		public const string Contact = "C";
		public const string Case = "S";
		public const string Opportunity = "O";
		public const string ExpenseReceipt = "R";

		protected Dictionary<string, Type> GraphTypes = new Dictionary<string, Type>()
			{
				{Event, typeof(EPEventMaint)},
				{Task,typeof(CRTaskMaint)}, 
				{Lead, typeof(LeadMaint)},
				{Contact, typeof(ContactMaint)},
				{Case, typeof(CRCaseMaint)},
				{Opportunity, typeof(OpportunityMaint)},
                {ExpenseReceipt, typeof(ExpenseClaimDetailEntry)}
			};
		#endregion

		#region Helpers

		internal void InsertFile(FileDto file)
		{
			_extractor.InsertFile(file);
		}

		#endregion
	}
}
