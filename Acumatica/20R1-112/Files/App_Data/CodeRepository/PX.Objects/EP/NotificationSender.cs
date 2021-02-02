using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Web.Compilation;
using System.Xml;
using PX.Common;
using PX.Common.Mail;
using PX.Data;
using PX.Data.EP;
using PX.Data.Reports;
using PX.Data.Wiki.Parser;
using PX.Data.Wiki.Parser.Html;
using PX.Data.Wiki.Parser.Txt;
using PX.Objects.CR;
using PX.Objects.SM;
using PX.Reports;
using PX.Reports.Controls;
using PX.Reports.Data;
using PX.Reports.Mail;
using PX.SM;
using FileInfo = PX.SM.FileInfo;
using System.Net.Mail;
using Message = PX.Reports.Mail.Message;
using PX.Objects.CS;

namespace PX.Objects.EP
{

    #region NotificationProvider

    public class NotificationProvider : INotificationSenderWithActivityLink
    {
        public void Notify(int? accountId, string mailTo, string mailCc, string mailBcc, string subject, string body)
        {
            var sender = new NotificationGenerator
            {
                MailAccountId = accountId,
                To = mailTo,
                Cc = mailCc,
                Bcc = mailBcc,
                Subject = subject,
                Body = body
            };
            sender.Send();
        }

        public void Notify(int? accountId, string mailTo, string mailCc, string mailBcc, string subject, string body, int? bAccountId,
            int? contactId, Guid? refNoteId, Tuple<string, byte[]>[] attachments)
        {
            var sender = new NotificationGenerator
            {
                MailAccountId = accountId,
                To = mailTo,
                Cc = mailCc,
                Bcc = mailBcc,
                Subject = subject,
                Body = body,
                BAccountID = bAccountId,
                ContactID = contactId,
                RefNoteID = refNoteId
            };
            foreach (var attachment in attachments)
            {
                sender.AddAttachment(attachment.Item1, attachment.Item2);
            }
            sender.Send();
        }
    }

    #endregion

    #region NotificationSender

    public class NotificationGenerator
    {
        #region Constants

        private static readonly Regex _HtmlRegex = new Regex(
            @"<(.|\n)*?>",
            RegexOptions.IgnoreCase | RegexOptions.Compiled);

        #endregion

        #region Fields

        private readonly List<Guid> _attachmentLinks = new List<Guid>();
        private readonly List<FileInfo> _newAttachments = new List<FileInfo>();

        private readonly PXGraph _graph;
        private Guid? _owner;
		
		protected string _localeName;
        #endregion

        #region Ctors

        public NotificationGenerator()
            : this(new PXGraph())
        {
        }

        public NotificationGenerator(PXGraph graph, AccessInfo accessinfo = null)
        {
            _graph = graph;
	        _graph.Caches[typeof(AccessInfo)].Current = accessinfo;
	        MassProcessMode = true;
        }

        #endregion

        #region Public Methods

        public string BodyFormat { get; set; }

        public string Body { get; set; }

        public string Subject { get; set; }

        public string From { get; set; }

        public string Bcc { get; set; }

        public string Cc { get; set; }

        public string To { get; set; }

        public int? MailAccountId { get; set; }

        public string Reply { get; set; }

        public Guid? RefNoteID { get; set; }

		public Guid? DocumentNoteID { get; set; }

        public int? BAccountID { get; set; }

        public int? ContactID { get; set; }

        public Guid? ParentNoteID { get; set; }

        public Guid? Owner
        {
            get { return _owner ?? EP.EmployeeMaint.GetCurrentEmployeeID(_graph); }
            set { _owner = value; }
        }

        public IEnumerable<NotificationRecipient> Watchers { get; set; }

        public Guid? AttachmentsID { get; set; }

		public bool MassProcessMode { get; set; }
		public void AddAttachment(string name, byte[] content)
		{
			AddAttachment(name, content, null);
		}


		public void AddAttachment(string name, byte[] content, string cid)
		{
			if (name == null) throw new ArgumentNullException(nameof(name));
			if (content == null) throw new ArgumentNullException(nameof(content));
			var id = string.IsNullOrEmpty(cid) ? Guid.NewGuid() : new Guid(cid);
			var file = new FileInfo(id, id + @"\" + name, null, content);
			_newAttachments.Add(file);
			_attachmentLinks.Add(id);
		}

		public void AddAttachmentLink(Guid Value)
        {
            _attachmentLinks.Add(Value);
        }

        public IEnumerable<CRSMEmail> Send()
        {
            IEnumerable<CRSMEmail> messages = CreateMessages();
			CRSMEmail[] activities = messages as CRSMEmail[] ?? messages.ToArray();
            PersistMessages(activities);
            return activities;
        }

        public void Send(IEnumerable<CRSMEmail> messages)
        {
            CRSMEmail[] activities = messages as CRSMEmail[] ?? messages.ToArray();
            PersistMessages(activities);
        }

        public IEnumerable<CRSMEmail> MailMessages()
        {            
            return CreateMessages();
        }
       
        #endregion

        #region Protected Methods

        private PXGraph Graph
        {
            get { return _graph; }
        }

        protected PXGraph GetEmailGraph()
        {
            return _graph;
        }

        protected void PersistMessages(IEnumerable<CRSMEmail> messages)
        {
            using (PXTransactionScope ts = new PXTransactionScope())
            {
                if (_newAttachments.Count > 0)
                {
                    var upload = PXGraph.CreateInstance<UploadFileMaintenance>();
                    upload.IgnoreFileRestrictions = true;
                    foreach (FileInfo file in _newAttachments)
                    {                                            
                        upload.SaveFile(file);
                        if (file.UID == null) throw new Exception(string.Format("Cannot save file '{0}'" + file.Name));
                    }
                }

                foreach (CRSMEmail item in messages)
                {
	                try
	                {
		                var activityCache = Graph.Caches[item.GetType()];
		                PXDBDefaultAttribute.SetSourceType<CRSMEmail.refNoteID>(activityCache, item, null);
		                activityCache.PersistInserted(item);
	                }
	                catch (Exception e)
	                {
		                if (!MassProcessMode)
			                throw;
							 PXTrace.WriteInformation(e);
	                }
                }
                Graph.Caches<CRActivityStatistics>().Persist(PXDBOperation.Insert);
                Graph.Caches<CRActivityStatistics>().Persist(PXDBOperation.Update);

                ts.Complete();
            }
        }

	    protected IEnumerable<CRSMEmail> CreateMessages()
	    {
		    var result = new List<CRSMEmail>();
		    CRSMEmail main = CreateMessage();
		    result.Add(main);
		    if (Watchers != null && Watchers.Any())
		    {
			    string mailCc = null;
			    string mailBcc = null;

			    foreach (NotificationRecipient n in Watchers)
			    {
				    if (n.Hidden == true)
				    {
					    mailBcc += (mailBcc == null ? string.Empty : "; ") + n.Email;
				    }
				    else
				    {
					    mailCc += (mailCc == null ? string.Empty : "; ") + n.Email;
				    }
			    }
			    main.MailCc = MergeAddressList(main, mailCc, main.MailCc);
			    main.MailBcc = MergeAddressList(main, mailBcc, main.MailBcc);
		    }

		    for (int i = result.Count - 1; i >= 0; i--)
		    {
			    var item = result[i];
			    if (item.MailAccountID == null)
			    {
				    Graph.Caches[typeof (CRSMEmail)].Delete(item);
				    result.RemoveAt(i);
				    if (!MassProcessMode && result.Count == 0)
					    throw new Exception(Messages.MailFromUndefined);
				    PXTrace.WriteInformation(Messages.MailFromUndefined);
			    }

			    var mail = item;
			    if (string.IsNullOrEmpty(mail.MailTo))
			    {
				    if (!string.IsNullOrEmpty(mail.MailCc))
				    {
					    string list = mail.MailCc;
					    mail.MailTo = GetFirstMail(ref list);
					    if (mail.MailTo != null) mail.MailCc = list;
				    }
				    if (!string.IsNullOrEmpty(mail.MailBcc))
				    {
					    string list = mail.MailBcc;
					    mail.MailTo = GetFirstMail(ref list);
					    if (mail.MailTo != null) mail.MailBcc = list;
				    }

				    if (string.IsNullOrEmpty(mail.MailTo))
				    {
					    Graph.Caches[typeof (CRSMEmail)].Delete(item);
					    result.RemoveAt(i);
					    if (!MassProcessMode && result.Count == 0)
						    throw new Exception(Messages.MailToUndefined);
					    PXTrace.WriteInformation(Messages.MailToUndefined);
				    }
			    }
		    }
		    return result;
	    }

	    #endregion

        #region Private Methods

        protected virtual CRSMEmail CreateMessage()
        {
			return CreateMessage(false);
		}
        protected CRSMEmail CreateMessage(bool isTemplate)
        {
            var activityCache = Graph.Caches[typeof (CRSMEmail)];
            var act = (CRSMEmail) activityCache.Insert();

			//CRActivity
            act.ClassID = CRActivityClass.Email;
            act.Type = null;
			act.OwnerID = Owner;
			act.StartDate = PXTimeZoneInfo.Now;
			act.BAccountID = BAccountID;
            if(act.ContactID == null)
                act.ContactID = ContactID;
            act.RefNoteID = RefNoteID;
            act.DocumentNoteID = DocumentNoteID;
			act.ParentNoteID = ParentNoteID;
			
			//SMEmail
			var accountId = MailAccountId ?? MailAccountManager.DefaultMailAccountID;
			act.MailAccountID = accountId;

			var account = (EMailAccount)PXSelect<EMailAccount,
			 Where<EMailAccount.emailAccountID, Equal<Required<EMailAccount.emailAccountID>>>>.
			 Select(_graph, accountId);
			if (account != null)
			{
				if (account.SenderDisplayNameSource == SenderDisplayNameSourceAttribute.Account)
					act.MailFrom = string.IsNullOrEmpty(account.AccountDisplayName)
						? account.Address
						: new MailAddress(account.Address, account.AccountDisplayName).ToString();
				else
					act.MailFrom = $"{TextUtils.QuoteString(account.Description)} <{account.Address}>";
			}

			if (!isTemplate)
			{

			act.MailTo = MergeAddressList(act, To, act.MailTo);
			act.MailCc = MergeAddressList(act, Cc, act.MailCc);
			act.MailBcc = MergeAddressList(act, Bcc, act.MailBcc);
			act.MailReply = string.IsNullOrEmpty(Reply) ? act.MailFrom : Reply;
			act.Subject = Subject;
			act.Body = BodyFormat == null || BodyFormat == EmailFormatListAttribute.Html
                ? CreateHtmlBody(Body)
                : CreateTextBody(Body);
			}

			act.IsIncome = false;
			act.MPStatus = MailStatusListAttribute.PreProcess;
			act.Format = BodyFormat ?? EmailFormatListAttribute.Html;

			if (AttachmentsID != null)
			{
				foreach (NoteDoc doc in
						PXSelect<NoteDoc, Where<NoteDoc.noteID, Equal<Required<NoteDoc.noteID>>>>.Select(Graph, AttachmentsID))
				{
					if (doc.FileID != null && !_attachmentLinks.Contains(doc.FileID.Value))
						_attachmentLinks.Add(doc.FileID.Value);
				}
			}

            if (_attachmentLinks.Count > 0)
                PXNoteAttribute.SetFileNotes(activityCache, act, _attachmentLinks.ToArray());

			act = (CRSMEmail)activityCache.Update(act);

			return act;
        }

        private static bool IsHtml(string text)
        {
            return !string.IsNullOrEmpty(text) && _HtmlRegex.IsMatch(text);
        }

        private static string CreateHtmlBody(string text)
        {
            return IsHtml(text) ? text : Tools.ConvertSimpleTextToHtml(text);
        }

        private static string CreateTextBody(string text)
        {
            return IsHtml(text) ? Tools.ConvertHtmlToSimpleText(text) : text;
        }

        private static string GetFirstMail(ref string addressList)
        {
			MailAddress address;
			if (EmailParser.TryParse(addressList, out address))
				return address.ToString();
			
                string result = addressList;
                addressList = null;
                return result;
            }

        protected static string MergeAddressList(CRSMEmail email, string addressList, string sourceList)
        {
            if (string.IsNullOrEmpty(addressList)) return sourceList;

            List<MailAddress> result = new List<MailAddress>();
            var index = new HashSet<string>();
            foreach (MailAddress address in EmailParser.ParseAddresses(addressList))
            {
                if (index.Contains(address.Address)) continue;
                if (email.MailTo != null && email.MailTo.Contains(address.Address, StringComparison.InvariantCultureIgnoreCase)) continue;
                if (email.MailCc != null && email.MailCc.Contains(address.Address, StringComparison.InvariantCultureIgnoreCase)) continue;
                if (email.MailBcc != null && email.MailBcc.Contains(address.Address, StringComparison.InvariantCultureIgnoreCase)) continue;
                index.Add(address.Address);

                result.Add(address);
            }
            return result.Count == 0
                ? sourceList
                : string.IsNullOrEmpty(sourceList)
                    ? PXDBEmailAttribute.ToString(result)
                    : PXDBEmailAttribute.AppendAddresses(sourceList, PXDBEmailAttribute.ToString(result));
        }

        #endregion
    }

    #endregion

    #region TemplateNotificationSender

    public class TemplateNotificationGenerator : NotificationGenerator
    {
        #region Fields

        private Guid? _refNoteId;
        private readonly object _entity;
        private readonly PXGraph _graph;
        
        #endregion

        #region Ctors

	    private TemplateNotificationGenerator(PXGraph graph, object row, Notification t = null)
		    : this(new PXGraph(), graph)
	    {
			_entity = row;
			_refNoteId = new EntityHelper(Graph).GetEntityNoteID(_entity);
			if (t != null)
				InitializeTemplate(t);
	    }

	    private TemplateNotificationGenerator(object row, string graphType = null)
			: this(new PXGraph(), InitializeGraph(graphType))
        {
			if (row != null)
			{
				EntityHelper helper = new EntityHelper(this.Graph);
				Type rowType = row.GetType();
				if (!string.IsNullOrEmpty(this.Graph.PrimaryView))
				{
				    Type primaryType = this.Graph.Views[this.Graph.PrimaryView].Cache.GetItemType();
				    if (rowType.IsSubclassOf(primaryType))
				        rowType = primaryType;
				}
				row = helper.GetEntityRow(rowType, helper.GetEntityKey(row.GetType(), row));
				this.Graph.Caches[rowType].Current = row;
				_entity = row;
				_refNoteId = new EntityHelper(Graph).GetEntityNoteID(_entity);
			}
        }

	    private TemplateNotificationGenerator(PXGraph baseGraph, PXGraph graph)
		    : base(baseGraph, graph.Accessinfo)
	    {
			_graph = graph;
	    }


	    private TemplateNotificationGenerator(object row, Notification t)
            : this(row, PXPageIndexingService.GetGraphTypeByScreenID(t.ScreenID))
        {
            InitializeTemplate(t);
        }

        public static TemplateNotificationGenerator Create(object row, int? notificationId)
        {
            Notification notification = null;

            if (notificationId != null)
            {
                var maint = PXGraph.CreateInstance<SMNotificationMaint>();
                notification = maint.PublishedNotification.
                    Search<Notification.notificationID>(notificationId);
                if (notification == null)
                    throw new PXException(Messages.NotificationTemplateNotFound);
            }
            return notification == null
                ? new TemplateNotificationGenerator(row)
                : new TemplateNotificationGenerator(row, notification);
        }

        public static TemplateNotificationGenerator Create(object row, Notification notification)
        {
            return new TemplateNotificationGenerator(row, notification);
        }

        public static TemplateNotificationGenerator Create(object row, string notificationCD = null)
        {
            Notification notification = null;
            if (notificationCD != null)
            {
                var maint = PXGraph.CreateInstance<SMNotificationMaint>();
                notification = maint.PublishedNotification.
                    Search<Notification.name>(notificationCD);
                if (notification == null)
                    throw new PXException(Messages.NotificationTemplateCDNotFound, notificationCD);
            }
            return notification == null
                ? new TemplateNotificationGenerator(row)
                : new TemplateNotificationGenerator(row, notification);
        }

        public static TemplateNotificationGenerator Create(PXGraph graph, object row, int notificationId)
        {
            var maint = PXGraph.CreateInstance<SMNotificationMaint>();
            Notification notification = maint.PublishedNotification.
                Search<Notification.notificationID>(notificationId);
            if (notification == null)
                throw new PXException(Messages.NotificationTemplateNotFound);

            return new TemplateNotificationGenerator(graph, row, notification);
        }

        public static TemplateNotificationGenerator Create(PXGraph graph, object row, Notification notification)
        {
            return new TemplateNotificationGenerator(graph, row, notification);
        }

        public static TemplateNotificationGenerator Create(PXGraph graph, object row, string notificationCD = null)
        {
            Notification notification = null;
            if (notificationCD != null)
            {
                var maint = PXGraph.CreateInstance<SMNotificationMaint>();
                notification = maint.PublishedNotification.
                    Search<Notification.name>(notificationCD);
                if (notification == null)
                    throw new PXException(Messages.NotificationTemplateCDNotFound, notificationCD);
            }
            return notification == null
                ? new TemplateNotificationGenerator(graph, row)
                : new TemplateNotificationGenerator(graph, row, notification);
        }

        #endregion

        #region Public Methods

        public bool LinkToEntity { get; set; }

        public virtual NotificationGenerator ParseNotification()
        {
			PXCultureScope culture = !String.IsNullOrEmpty(_localeName) ? new PXCultureScope(new System.Globalization.CultureInfo(_localeName)) : null;

			using (culture)
			{
				return new NotificationGenerator(this.Graph)
				{
					MailAccountId = MailAccountId,
					To = ParseMailAddressExpression(To),
					Cc = ParseMailAddressExpression(Cc),
					Bcc = ParseMailAddressExpression(Bcc),
					Subject = ParseExpression(Subject),
					Body = ParseExpression(Body),
					AttachmentsID = AttachmentsID
				};
			} 
        }

        #endregion

        #region Override Methods

        protected override CRSMEmail CreateMessage()
        {
			PXCultureScope culture = !String.IsNullOrEmpty(_localeName) ? new PXCultureScope(new System.Globalization.CultureInfo(_localeName)) : null;

			using (culture)
			{
				var message = base.CreateMessage(true);

				if (LinkToEntity && _refNoteId != null)
					message.RefNoteID = _refNoteId;

				if (_entity != null)
				{
					var contact = _entity as Contact;
					message.ContactID = contact != null && contact.ContactType != ContactTypesAttribute.Lead
						? contact.ContactID 
						: message.ContactID;

					message.MailTo = message.MailCc = message.MailBcc = null;

					message.MailTo = MergeAddressList(message, ParseMailAddressExpression(To), null);
					message.MailCc = MergeAddressList(message, ParseMailAddressExpression(Cc), null);
					message.MailBcc = MergeAddressList(message, ParseMailAddressExpression(Bcc), null);
					message.MailReply = string.IsNullOrEmpty(Reply) ? message.MailFrom : ParseMailAddressExpression(Reply);
					message.Subject = ParseExpression(Subject);
					message.Body = PX.Web.UI.PXRichTextConverter.NormalizeHtml(ParseExpression(Body));
				}

				return message;
			}
        }

        private Type EntityType
        {
            get { return _entity.GetType(); }
        }

        #endregion

        #region Private Methods

        private PXGraph Graph
        {
            get { return _graph; }
        }

        private void InitializeTemplate(Notification t)
        {
            MailAccountId = t.NFrom ?? MailAccountManager.DefaultAnyMailAccountID;
            var mailFrom = (MailAccountId).
                With(_ => (EMailAccount) PXSelect<EMailAccount,
                    Where<EMailAccount.emailAccountID, Equal<Required<EMailAccount.emailAccountID>>>>.
                    Select(Graph, _.Value)).
                With(_ => _.Address);
            From = mailFrom;
            Reply = mailFrom;
            To = t.NTo;
            Cc = t.NCc;
            Bcc = t.NBcc;
            Body = t.Body;
            Subject = t.Subject;
			AttachmentsID = t.NoteID;

            foreach (
                NoteDoc res in
                    PXSelect<NoteDoc, Where<NoteDoc.noteID, Equal<Required<NoteDoc.noteID>>>>.Select(Graph, t.NoteID))
            {
                AddAttachmentLink((Guid) res.FileID);
            }

			_localeName = t.LocaleName;
            PXSelectBase GeneralInfo;
            if (!this.Graph.Views.ContainsKey(nameof(GeneralInfo)))
            {
                GeneralInfo = new GeneralInfoSelect(this.Graph);
                this.Graph.Views.Add(nameof(GeneralInfo), GeneralInfo.View);
            }
        }
		
		public string ParseMailAddressExpression(string field)
		{
			if (string.IsNullOrEmpty(field)) return string.Empty;
			if (_entity == null) return field;

			string[] mailsTo = field.Split(';', ',');
			StringBuilder res = new StringBuilder();

			for (int m = 0; m < mailsTo.Length; m++)
			{
				var mail = ParseExpression(mailsTo[m]);

				if (!string.IsNullOrEmpty(mail)) res.Append(mail).Append(';');
			}

			return res.ToString();
		}

		public string ParseExpression(string field)
        {
			return _entity == null ? field : PXTemplateContentParser.Instance.Process(field, Graph, EntityType, null);
        }

        private static PXGraph InitializeGraph(string graphType)
        {
            Type type = null;
            if (graphType != null)
            {
                type = PXBuildManager.GetType(graphType, false);
                if (type == null)
                    throw new PXException(PXMessages.LocalizeFormatNoPrefixNLA(Messages.TypeCannotBeFound, graphType));
                if (!typeof (PXGraph).IsAssignableFrom(type))
                    throw new PXException(PXMessages.LocalizeFormatNoPrefixNLA(Messages.IsNotGraphSubclass, graphType));
            }
            PXGraph graph = type != null ? PXGraph.CreateInstance(type) : null;
            return graph ?? new PXGraph();
        }

        #endregion
    }

    #endregion

/*#if !AZURE
	[System.Security.Permissions.RegistryPermission(System.Security.Permissions.SecurityAction.Assert, Unrestricted = true)]
	[System.Security.Permissions.FileIOPermission(System.Security.Permissions.SecurityAction.Assert, Unrestricted = true)]
	[System.Security.Permissions.SecurityPermission(System.Security.Permissions.SecurityAction.Assert, Unrestricted = true)]
#endif*/

    public class ReportNotificationGenerator
    {
        private readonly Report _report;
        private IDictionary<string, string> _parameters;

        public ReportNotificationGenerator(string reportId)
        {
            if (string.IsNullOrEmpty(reportId))
                throw new ArgumentNullException("reportId");

            _report = PXReportTools.LoadReport(reportId, null);

            if (_report == null)
                throw new ArgumentException(PXMessages.LocalizeFormatNoPrefixNLA(Messages.ReportCannotBeFound, reportId), "reportId");
        }

        public int? MailAccountId { get; set; }

        public IDictionary<string, string> Parameters
        {
            get { return _parameters ?? (_parameters = new Dictionary<string, string>()); }
            set { _parameters = value; }
        }

        public IEnumerable<NotificationRecipient> AdditionalRecipents { get; set; }

        public string Format { get; set; }
        public Int32? NotificationID { get; set; }

        public IEnumerable<CRSMEmail> Send()
        {
            PXReportTools.InitReportParameters(_report, _parameters, SettingsProvider.Instance.Default);

            ReportNode reportNode = ReportProcessor.ProcessReport(_report);
            reportNode.SendMailMode = true;

            return SendMessages(MailAccountId, reportNode, Format, AdditionalRecipents, NotificationID);
        }

        public static IEnumerable<CRSMEmail> Send(string reportId, IDictionary<string, string> reportParams)
        {
            var sender = new ReportNotificationGenerator(reportId) {Parameters = reportParams};
            return sender.Send();
        }

        private static IEnumerable<CRSMEmail> SendMessages(int? accountId, ReportNode report, string format,
            IEnumerable<NotificationRecipient> additionalRecipents, Int32? TemplateID)
        {
            List<CRSMEmail> result = new List<CRSMEmail>();
            Exception ex = null;
            foreach (Message message in GetMessages(report))
            {
                try
                {
                    result.AddRange(Send(accountId, message, format, additionalRecipents, TemplateID));
                }
                catch (Exception e)
                {
                    PXTrace.WriteError(e);
                    ex = e;
                }
            }
            if (ex != null) throw ex;
            return result;
        }

        private static IEnumerable<Message> GetMessages(ReportNode report)
        {
            {
                var mailSource = new Dictionary<GroupMessage, PX.Reports.Mail.Message>();
                foreach (MailSettings message in report.Groups.Select(g => g.MailSettings))
                {
                    if (message != null &&
                        message.ShouldSerialize() &&
                        !mailSource.ContainsKey(message))
                    {
                        mailSource.Add(message, new PX.Reports.Mail.Message(message, report, message));
                    }
                }
                return mailSource.Values;
            }
        }

        private static IEnumerable<CRSMEmail> Send(int? accountId, Message message, string format,
            IEnumerable<NotificationRecipient> additionalRecipients, Int32? TemplateID)
        {
            var graph = new PXGraph();

            var activitySource = message.Relationship.ActivitySource;
            Guid? refNoteID = GetEntityNoteID(graph, activitySource);
			
	        var bacc = message.Relationship.ParentSource as BAccount;
			int? bAccountID = bacc != null ? bacc.BAccountID : null;

            NotificationGenerator sender = null;
            if (TemplateID != null)
            {
                sender = TemplateNotificationGenerator.Create(activitySource, (int) TemplateID);
            }
            else if (!string.IsNullOrEmpty(message.TemplateID))
            {
                sender = TemplateNotificationGenerator.Create(activitySource, message.TemplateID);
            }
            if (sender == null)
                sender = new NotificationGenerator(graph);

            sender.Body = string.IsNullOrEmpty(sender.Body) ? message.Content.Body : sender.Body;
            sender.Subject = string.IsNullOrEmpty(sender.Subject) ? message.Content.Subject : sender.Subject;
            sender.MailAccountId = accountId;
            sender.Reply = accountId.
                With(_ => (EMailAccount) PXSelect<EMailAccount,
                    Where<EMailAccount.emailAccountID, Equal<Required<EMailAccount.emailAccountID>>>>.
                    Select(graph, _.Value)).
                With(_ => _.Address);
            sender.To = message.Addressee.To;
            sender.Cc = message.Addressee.Cc;
            sender.Bcc = message.Addressee.Bcc;
            sender.RefNoteID = refNoteID;
            sender.BAccountID = bAccountID;
            sender.BodyFormat = PXNotificationFormatAttribute.ValidBodyFormat(format);

            List<NotificationRecipient> watchers = new List<NotificationRecipient>();
            if (sender.Watchers != null)
                watchers.AddRange(sender.Watchers);
            if (additionalRecipients != null)
                watchers.AddRange(additionalRecipients);
            sender.Watchers = watchers;

            foreach (ReportStream attachment in message.Attachments)
                sender.AddAttachment(attachment.Name, attachment.GetBytes());

            return sender.Send();
        }

        private static Guid? GetEntityNoteID(PXGraph graph, object row)
        {
            var helper = new EntityHelper(graph);
            Guid? refNoteId = null;
            if (row != null)
            {
                var cacheType = row.GetType();
                var graphType = helper.GetPrimaryGraphType(cacheType, row, false);
                if (graphType != null)
                {
                    var primaryGraph = graph.GetType() != graphType
                        ? (PXGraph) PXGraph.CreateInstance(graphType)
                        : graph;

                    var primaryCache = primaryGraph.Caches[cacheType];
                    refNoteId = PXNoteAttribute.GetNoteID(primaryCache,
                        row, EntityHelper.GetNoteField(cacheType));
                }
            }
            return refNoteId;
        }

        public static IEnumerable<GroupMessage> GetWatchers(GroupMessage source, string defaultFormat,
            RecipientList watchers)
        {
            if (watchers != null)
            {
                GroupMessage msg = null;
                bool sourceAdded = false;

                if (defaultFormat != null)
                {
                    var format = ConvertFormat(defaultFormat);

                    msg = new GroupMessage(source.From, source.UID, source.Addressee,
                        source.Content, source.TemplateID,
                        format, source.Relationship, source.Report);
                    sourceAdded = true;
                }

                //Redefine message format;
                foreach (NotificationRecipient n in watchers)
                {
                    if (string.Compare(n.Email, source.Addressee.To, true) == 0)
                    {
                        var format = ConvertFormat(n.Format);
                        msg = new GroupMessage(source.From, source.UID, source.Addressee,
                            source.Content, source.TemplateID,
                            format, source.Relationship, source.Report);
                        sourceAdded = true;
                        break;
                    }
                }

                foreach (NotificationRecipient n in watchers)
                {
                    string format = ConvertFormat(n.Format);
                    if (msg != null && msg.Format != format)
                    {
                        yield return msg;
                        msg = null;
                    }

                    if (msg == null)
                    {
                        if (format == source.Format)
                        {
                            msg = new GroupMessage(source);
                            sourceAdded = true;
                        }
                        else
                        {
                            msg = new GroupMessage(source.From, source.UID,
                                PX.Common.Mail.MailSender.MessageAddressee.Empty,
                                source.Content, source.TemplateID,
                                format, source.Relationship, source.Report);
                        }
                    }

                    if (n.Hidden == true)
                    {
                        if (!string.IsNullOrEmpty(msg.Addressee.Bcc) && msg.Addressee.Bcc.Contains(n.Email)) continue;
                        var bcc = (msg.Addressee.Bcc != null ? msg.Addressee.Bcc + ';' : string.Empty) + n.Email;
                        var addresse = new PX.Common.Mail.MailSender.MessageAddressee(
                            msg.Addressee.To, msg.Addressee.Reply, msg.Addressee.Cc, bcc);
                        msg = new GroupMessage(msg.From, msg.UID, addresse,
                            msg.Content, msg.TemplateID,
                            msg.Format, msg.Relationship, msg.Report);
                    }
                    else
                    {
                        if (string.IsNullOrEmpty(msg.Addressee.To))
                        {
                            var addresse = new PX.Common.Mail.MailSender.MessageAddressee(
                                n.Email, msg.Addressee.Reply, msg.Addressee.Cc, msg.Addressee.Bcc);
                            msg = new GroupMessage(msg.From, msg.UID, addresse,
                                msg.Content, msg.TemplateID,
                                msg.Format, msg.Relationship, msg.Report);
                        }
                        else
                        {
                            if (msg.Addressee.To == n.Email) continue;
                            if (!string.IsNullOrEmpty(msg.Addressee.Cc) && msg.Addressee.Cc.Contains(n.Email)) continue;

                            var cc = (msg.Addressee.Cc != null ? msg.Addressee.Cc + ';' : string.Empty) + n.Email;

                            var addresse = new PX.Common.Mail.MailSender.MessageAddressee(
                                msg.Addressee.To, msg.Addressee.Reply, cc, msg.Addressee.Bcc);
                            msg = new GroupMessage(msg.From, msg.UID, addresse,
                                msg.Content, msg.TemplateID,
                                msg.Format, msg.Relationship, msg.Report);
                        }
                    }
                }
                if (msg != null) yield return msg;

                if (!sourceAdded) yield return source;

                yield break;
            }
            yield return source;
        }

        public static string ConvertFormat(string notificationFormat)
        {
            switch (notificationFormat)
            {
                case NotificationFormat.PDF:
                    return ReportProcessor.FilterPdf;
                case NotificationFormat.Excel:
                    return ReportProcessor.FilterExcel;
                default:
                    return ReportProcessor.FilterHtml;
            }
        }
    }

    public class RecipientList : IEnumerable<NotificationRecipient>
    {
        public RecipientList()
        {
            items = new SortedList<string, NotificationRecipient>();
        }

        public void Add(NotificationRecipient item)
        {
            string key = item.Format + '.' + items.Count;
            items.Add(key, item);
        }

        private readonly SortedList<string, NotificationRecipient> items;

        #region IEnumerable<NotificationRecipient> Members

        public IEnumerator<NotificationRecipient> GetEnumerator()
        {
            foreach (KeyValuePair<string, NotificationRecipient> item in items)
                yield return item.Value;
        }

        #endregion

        #region IEnumerable Members

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            foreach (KeyValuePair<string, NotificationRecipient> item in items)
                yield return item.Value;
        }

        #endregion
    }
}
