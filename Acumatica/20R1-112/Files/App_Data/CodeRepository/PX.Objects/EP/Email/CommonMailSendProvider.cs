using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Mail;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;
using PX.Common;
using PX.Common.Mail;
using PX.Data;
using PX.Data.EP;
using PX.Data.Wiki.Parser;
using PX.Objects.CR;
using PX.SM;
using Group = System.Text.RegularExpressions.Group;

namespace PX.Objects.EP
{
	public sealed class CommonMailSendProvider : IMailSendProvider
	{
		#region AttachmentCollection

		private class AttachmentCollection : IEnumerable<Attachment>
		{
			#region File

			private sealed class File
			{
				private readonly Guid _id;
				private readonly string _name;
				private readonly byte[] _data;

				private string _ext;

				public File(Guid id, string name, byte[] data)
				{
					_id = id;
					_name = name;
					_data = data;
				}

				public Guid Id
				{
					get { return _id; }
				}

				public string Name
				{
					get { return _name; }
				}

				public byte[] Data
				{
					get { return _data; }
				}

				public string Extension
				{
					get
					{
						return _ext ?? (_ext = MimeTypes.GetMimeType(Path.GetExtension(_name)));
					}
				}
			}

			#endregion

			#region Fields

			private readonly List<File> _items = new List<File>();
			private readonly PXGraph _graph;

			#endregion

			#region Ctors

			public AttachmentCollection(PXGraph graph)
			{
				_graph = graph;
			}

			#endregion

			public void Add(Guid id, string name, byte[] data)
			{
				if (_items.Any(e => e.Id == id))
				{
					return;
				}

				_items.Add(new File(id, name, data));
			}

			public void Add(Guid id)
			{
				if (_items.Any(e => e.Id == id))
				{
					return;
				}

				var f = ReadFile(id);
				if (f == null) return;

				var filename = f.Name.Split('\\');
				var name = filename[filename.Length - 1].Replace('/', '_').Replace('\\', '_');
				var a = new File(id, name, f.Data);

				_items.Add(a);
			}

			public static string CreateLink(Guid id)
			{
				return "cid:" + id;
			}

			public bool ResizeImage(Guid id, int width, int height)
			{
				var res = false;
				foreach (File att in _items)
					if (att.Id == id)
					{
						var result = Drawing.ScaleImageFromBytes(att.Data, width, height);
						if (result != null)
						{
							var replacer = new File(att.Id, att.Name, result);
							_items.Remove(att);
							_items.Add(replacer);
							res = true;
						}
						break;
					}
				return res;
			}

			private UploadFile ReadFile(Guid? id)
			{
				var result = (PXResult<UploadFile, UploadFileRevision>)
					PXSelectJoin<UploadFile,
					InnerJoin<UploadFileRevision,
						On<UploadFile.fileID, Equal<UploadFileRevision.fileID>,
							And<UploadFile.lastRevisionID, Equal<UploadFileRevision.fileRevisionID>>>>,
					Where<UploadFile.fileID, Equal<Required<UploadFile.fileID>>>>.
					Select(_graph, id);
				if (result == null) return null;
				var file = (UploadFile)result[typeof(UploadFile)];
				var revision = (UploadFileRevision)result[typeof(UploadFileRevision)];
				if (file != null && revision != null)
					file.Data = revision.Data;
				return file;
			}

			public IEnumerator<Attachment> GetEnumerator()
			{
				foreach (File item in _items)
					yield return
						new System.Net.Mail.Attachment(new MemoryStream(item.Data),
									   item.Name, item.Extension) { ContentId = item.Id.ToString() };
			}

			IEnumerator IEnumerable.GetEnumerator()
			{
				return GetEnumerator();
			}
		}

		#endregion

		#region Message

		private sealed class Message
		{
			private readonly string _from;
			private readonly string _to;
			private readonly string _subject;

			public Message(string @from, string to, string subject)
			{
				if (string.IsNullOrEmpty(from)) throw new ArgumentNullException("from");
				if (string.IsNullOrEmpty(to)) throw new ArgumentNullException("to");

				_from = from;
				_to = to;
				_subject = subject;
			}

			public string From
			{
				get { return _from; }
			}

			public string To
			{
				get { return _to; }
			}

			public string Reply { get; set; }

			public string Cc { get; set; }

			public string Bcc { get; set; }

			public string Subject
			{
				get { return _subject; }
			}

			public string Body { get; set; }

			public IEnumerable<Attachment> Files { get; set; }

			public bool Html { get; set; }

			public string UID { get; set; }
		}

		#endregion

		#region ImageExtractor

		#endregion

		#region ImageInfo

		#endregion
		
		#region MessageProcessor

		private class MessageProcessor : IDisposable
		{
			#region Constants

			private const string _FILEID_REGEX_GROUP = "fileid";
			private const string _SRC_REGEX_GROUP = "src";
			private const string _SRC_ATT_PREFIX = "src=\"";
			private const string _SRC_ATT_POSTFIX = "\"";

			private static readonly Regex _imagesRegex =
				new Regex("<img [^<>]*" + _SRC_ATT_PREFIX + "(?<" + _SRC_REGEX_GROUP + ">[^<>\"]*?)/getfile.ashx" +
						"\\?([^<>\"]*&)*fileid=(?<" + _FILEID_REGEX_GROUP + ">[^\\.<>\"&]*?)(\\.[^<>&\"]{1,}){0,1}(&[^<>\"]*)*" + _SRC_ATT_POSTFIX + "[^<>]*>",
						RegexOptions.Multiline | RegexOptions.Singleline | RegexOptions.Compiled | RegexOptions.IgnoreCase);

            private static readonly Regex _imagesRegexnewRTE =
				new Regex("<img [^<>]* src=\"([^<>]*)\">",
						RegexOptions.Multiline | RegexOptions.Singleline | RegexOptions.Compiled | RegexOptions.IgnoreCase);

            

            #endregion

			private readonly PXGraph _graph;			
			private EMailAccount _account;
			private MailSender _mailer;
			
			public MessageProcessor(int? accountID)
			{
				_graph = new PXGraph();
				_graph.SelectTimeStamp();
				ReadAccount(accountID);
				PXSelect<PreferencesEmail>.Clear(_graph);				
			}
			
			public void ProcessAll()
			{
                bool success = true;
				foreach (SMEmail message in
				PXSelect<SMEmail,
					Where<SMEmail.mailAccountID, Equal<Required<SMEmail.mailAccountID>>,
						And<SMEmail.isIncome, NotEqual<True>,
						And<SMEmail.mpstatus, Equal<MailStatusListAttribute.preProcess>>>>>.
				SelectWindowed(_graph, 0, _account.SendGroupMails.GetValueOrDefault(), _account.EmailAccountID))
				{
					Process(message);
                    if (success && message.MPStatus == MailStatusListAttribute.Failed)
                        success = false;
				}
				if(!success)
					throw new PXException(ErrorMessages.SeveralItemsFailed);
			}

			public void Process(SMEmail message)
			{				
				using (PXTransactionScope sc = new PXTransactionScope())
				{
					try
					{
						PreProcessMessage(message);
					}
					catch (Exception)
					{
						//Unable to change status - leave message alone.
						return;
					}
					try
					{
						ProcessMessage(message);
						PostProcessMessage(message);
					}
					catch (Exception ex)
					{
						this._graph.Clear();
						if (message == null || _graph.Caches[message.GetType()].GetStatus(message) == PXEntryStatus.Inserted) return;

						message = PXSelect<SMEmail, Where<SMEmail.noteID, Equal<Required<SMEmail.noteID>>>>.SelectWindowed(_graph, 0, 1, message.NoteID);
						if (message != null)
						{
							var messageCopy = (SMEmail)_graph.Caches[message.GetType()].CreateCopy(message);

							messageCopy.Exception = ex.Message;
							messageCopy.MPStatus = MailStatusListAttribute.Failed;
							UpdateMessage(messageCopy);

							_graph.Caches[message.GetType()].RestoreCopy(message, messageCopy);
						}
					}
					sc.Complete();
				}
			}

			private void ReadAccount(int? accountID)
			{
				int? smtpAccount = accountID;
				EMailAccount account = null;
				if (smtpAccount != null)
					account = ReadAccountSettings(smtpAccount);
				if (account == null)
					account = ReadDefaultAccountSettings();
				if (account == null)
					throw new PXException(ErrorMessages.EmailNotConfigured);
				if (string.IsNullOrEmpty(account.Address.With(_ => _.Trim())))
					throw new PXException(Messages.EmptyEmailAccountAddress);

				var mailer = MailAccountManager.GetSender(account);
				if (mailer == null)
					throw new PXException(ErrorMessages.EmailNotConfigured);

				_account = account;
				_mailer = mailer;
			}

			private void PreProcessMessage(SMEmail message)
			{                
				var messageCopy = (SMEmail)_graph.Caches[message.GetType()].CreateCopy(message);

				messageCopy.MPStatus = MailStatusListAttribute.InProcess;
				messageCopy.Exception = null;
			    if (messageCopy.MessageId == null)
					messageCopy.MessageId = "<" + Guid.NewGuid() + "_acumatica@" + _account.OutcomingHostName + ">";                
				UpdateMessage(messageCopy);

				_graph.Caches[message.GetType()].RestoreCopy(message, messageCopy);
			}

			private void PostProcessMessage(SMEmail message)
			{
				PXDatabase.Update<CRActivity>(
					new PXDataFieldAssign<CRActivity.startDate>(PXTimeZoneInfo.UtcNow),
					new PXDataFieldAssign<CRActivity.uistatus>(message.Exception == null 
						? ActivityStatusAttribute.Completed
						: ActivityStatusAttribute.Open),
					new PXDataFieldRestrict<CRActivity.noteID>(PXDbType.UniqueIdentifier, 16, message.RefNoteID, PXComp.EQ));

				if (message.Exception == null)
				{
					PXUpdate<Set<PMTimeActivity.approvalStatus,
						Switch<Case<Where<PMTimeActivity.approverID, IsNull>, 
							ActivityStatusAttribute.completed>, 
							ActivityStatusAttribute.pendingApproval>>,
						PMTimeActivity,
						Where<PMTimeActivity.refNoteID, Equal<Required<PMTimeActivity.refNoteID>>,
							And<PMTimeActivity.isCorrected, Equal<Zero>>>>
						.Update(_graph, message.RefNoteID);						
				}

				if (message.Exception == null)
                {
                    message.RetryCount = 0;
                    message.MPStatus = MailStatusListAttribute.Processed;
                }
                else
			    {
			        message.RetryCount += 1;
			        if (message.Exception.StartsWith("5") ||
			            message.RetryCount >= MailAccountManager.GetEmailPreferences().RepeatOnErrorSending)
			        {
			            message.RetryCount = 0;
			            message.MPStatus = MailStatusListAttribute.Failed;
			        }
			        else
			        {
			            message.MPStatus = MailStatusListAttribute.PreProcess;
			        }
			    }

			    UpdateMessage(message);
			}

			private void ProcessMessage(SMEmail message)
			{
				try
				{					
					SendMail(CreateMail(message));
					message.Exception = null;
				}
				catch (OutOfMemoryException) { throw; }
				catch (StackOverflowException) { throw; }
				catch (Exception e)
				{
					message.Exception = e.Message;
				}
			}

			private Message CreateMail(SMEmail message)
			{
				var subject = GenerateSubject(message);
				var fs = ReadAttachments(message);
                ReadTemplateAttachments(message);
				var content = ExtractInlineImages(message,fs);

				Message mail = new Message(message.MailFrom, message.MailTo, subject)
								{
									Reply = message.MailReply,
									Cc = message.MailCc,
									Bcc = message.MailBcc,
									Body = GenerateBody(content, fs),
									Html = message.Format == null || message.Format == EmailFormatListAttribute.Html,
									UID = message.MessageId,
									Files = fs
								};

				return mail;
			}

			private void UpdateMessage(SMEmail message)
			{
				var emailType = message.GetType();
				var cache = _graph.Caches[emailType];
				message = (SMEmail)cache.Update(message);
				_graph.EnsureCachePersistence(emailType);				
				var cached = _graph.Caches[message.GetType()].Locate(message);
				_graph.Persist();
				_graph.SelectTimeStamp();
				message = (SMEmail)cache.CreateCopy(cached);
			}

            private string ReadTemplateAttachments(SMEmail message)
            {
                if (message.Body != null && message.Body.IndexOf("embedded=\"true\"") > -1)
                {
                    Regex _tempimagesRegex = new Regex("(<(img data-convert=\"view\")[^<>]*(src=\"([^\"]*)\" ([^<>]*)>))");
                    Regex _tempimagesRegex2 = new Regex("(<img[^<>]*src=(\"[^<>]*GetFile.*;file=([^\"]*)\") ([^<>]*)>)");
                    List<string> changeimage = new List<string>();
                    List<int> start = new List<int>();
                    List<int> length = new List<int>();
                    foreach (Match match in _tempimagesRegex.Matches(message.Body))
                    {
                        if (match.Groups[4].Value != null && match.Groups[1].Value.IndexOf("embedded=\"true\"") > -1)
                        {
                            foreach (var result in
                            PXSelectJoin<UploadFile,
                            InnerJoin<UploadFileRevision,
                                On<UploadFile.fileID, Equal<UploadFileRevision.fileID>,
                                    And<UploadFile.lastRevisionID, Equal<UploadFileRevision.fileRevisionID>>>>,
                            Where<UploadFile.name, Equal<Required<UploadFile.name>>>>.
                            SelectWindowed(new PXGraph(), 0, 1, HttpUtility.UrlDecode(match.Groups[4].Value)))
                            {
                                UploadFileRevision filedata = result[typeof(UploadFileRevision)] as UploadFileRevision;
                                if (filedata != null)
                                {
                                    string tempchangeimage = "<img title=\"\" src=\"data:image/jpeg;base64," + Convert.ToBase64String(filedata.Data) + "\">";
                                    message.Body = message.Body.Replace(match.Groups[1].Value, tempchangeimage);
                                }
                            }
                        }
                    }

                    foreach (Match match in _tempimagesRegex2.Matches(message.Body))
                    {
                        if (match.Groups[3].Value != null && match.Groups[1].Value.IndexOf("embedded=\"true\"") > -1)
                        {
                            foreach (var result in
                            PXSelectJoin<UploadFile,
                            InnerJoin<UploadFileRevision,
                                On<UploadFile.fileID, Equal<UploadFileRevision.fileID>,
                                    And<UploadFile.lastRevisionID, Equal<UploadFileRevision.fileRevisionID>>>>,
                            Where<UploadFile.name, Equal<Required<UploadFile.name>>>>.
                            SelectWindowed(new PXGraph(), 0, 1, HttpUtility.UrlDecode(match.Groups[3].Value)))
                            {
                                UploadFileRevision filedata = result[typeof(UploadFileRevision)] as UploadFileRevision;
                                if (filedata != null)
                                {
                                    string tempchangeimage = "<img title=\"\" src=\"data:image/jpeg;base64," + Convert.ToBase64String(filedata.Data) + "\">";
                                    message.Body = message.Body.Replace(match.Groups[1].Value, tempchangeimage);
                                }
                            }
                        }
                    }
                }
                return null;
            }

		    private string ExtractInlineImages(SMEmail message, AttachmentCollection fs)
			{
				string res;
				ICollection<PX.Data.ImageExtractor.ImageInfo> files;
				if (message.Body != null && new ImageExtractor().Extract(message.Body, out res, out files))
				{
					foreach (PX.Data.ImageExtractor.ImageInfo imageInfo in files)
						fs.Add(imageInfo.ID, imageInfo.Name, imageInfo.Bytes);
					return res;
				}
				return message.Body;
			}

			private void SendMail(Message mail)
			{
				var mailerReply = _account.With(_ => _.ReplyAddress).With(_ => _.Trim());
				var correctReply = string.IsNullOrEmpty(mailerReply) ? mail.Reply : mailerReply;

				var from = mail.From.With(_ => _.TrimEnd(';'));
				var reply = correctReply;
				var to = mail.To;
				var addressee = new MailSender.MessageAddressee(to, reply, mail.Cc, mail.Bcc);
				var content = new MailSender.MessageContent(mail.Subject, mail.Html, mail.Body);
				var msg = new MailSender.MailMessageT(from, mail.UID, addressee, content);
				_mailer.Send(msg, mail.Files.ToArray());
			}

			private EMailAccount ReadDefaultAccountSettings()
			{
				PXSelect<PreferencesEmail>.Clear(_graph);
				var defAddress = ((PreferencesEmail)PXSelect<PreferencesEmail>.SelectWindowed(_graph, 0, 1)).With(_ => _.DefaultEMailAccountID);
				return ReadAccountSettings(defAddress);
			}

			private EMailAccount ReadAccountSettings(int? accountId)
			{
				if (accountId == null) return null;

				PXSelect<EMailAccount,
					Where<EMailAccount.emailAccountID, Equal<Required<EMailAccount.emailAccountID>>>>.
					Clear(_graph);

				var account = (EMailAccount)PXSelect<EMailAccount,
					Where<EMailAccount.emailAccountID, Equal<Required<EMailAccount.emailAccountID>>>>.
					Select(_graph, accountId);

				return account;
			}
			
			private string GenerateBody(string content, AttachmentCollection fs)
			{
				var body = content ?? string.Empty;
				MatchCollection images = _imagesRegex.Matches(body);
                MatchCollection images1 = _imagesRegexnewRTE.Matches(body);

			    if (images.Count > 0)
			    {
                    var sb = new StringBuilder();
			        var currentIndex = 0;
				    foreach (Match match in images)
					{
						Group fileid = match.Groups[_FILEID_REGEX_GROUP];
						Group src = match.Groups[_SRC_REGEX_GROUP];
						Guid imgId;
						if (GUID.TryParse(fileid.Value, out imgId))
						{
							fs.Add(imgId);
							sb.Append(body.Substring(currentIndex, src.Index - currentIndex));
							sb.Append(AttachmentCollection.CreateLink(imgId));
							sb.Append(_SRC_ATT_POSTFIX);
							currentIndex = body.IndexOf(_SRC_ATT_POSTFIX, fileid.Index + fileid.Length) + _SRC_ATT_POSTFIX.Length;
						}
						else
						{
							var newIndex = src.Index + src.Length + _SRC_ATT_POSTFIX.Length;
							sb.Append(body.Substring(currentIndex, newIndex - currentIndex));
							currentIndex = newIndex;
						}
					}
					if (currentIndex < body.Length - 1)
						sb.Append(body.Substring(currentIndex));
					body = sb.ToString();
			    }
                // ToDo for RTE (old and new links)
			    else
			    {
			        if (images1.Count > 0)
			        {
			            var sb = new StringBuilder();
			            var currentIndex = 0;
			            foreach (Match match in images1)
			            {
			                Group filename = match.Groups[1];
			                Group src = match.Groups[1];

			                foreach (var result in
			                    PXSelect<UploadFile,
			                        Where<UploadFile.name, Equal<Required<UploadFile.name>>>>.
			                        SelectWindowed(new PXGraph(), 0, 1, HttpUtility.UrlDecode(filename.Value)))
			                {
			                    UploadFile uf = result[typeof (UploadFile)] as UploadFile;
			                    if (uf != null)
			                    {
			                        Guid imgId = (Guid) uf.FileID;
			                        fs.Add(imgId);
			                        sb.Append(body.Substring(currentIndex, src.Index - currentIndex));
			                        sb.Append(AttachmentCollection.CreateLink(imgId));
			                        sb.Append(_SRC_ATT_POSTFIX);
			                        currentIndex = body.IndexOf(_SRC_ATT_POSTFIX, filename.Index + filename.Length) +
			                                       _SRC_ATT_POSTFIX.Length;
			                    }
			                    else
			                    {
			                        var newIndex = src.Index + src.Length + _SRC_ATT_POSTFIX.Length;
			                        sb.Append(body.Substring(currentIndex, newIndex - currentIndex));
			                        currentIndex = newIndex;
			                    }
			                }
			            }
			            if (currentIndex < body.Length - 1)
			                sb.Append(body.Substring(currentIndex));
			            body = sb.ToString();
			        }
			    }
			    return body;
			}

			private string GenerateSubject(SMEmail message)
			{
				var subject = message.Subject;
                if (message.Ticket == null) message.Ticket = message.ID;
			    if (message.Ticket != null)
			    {
			        EMailAccount account =
			            PXSelect<EMailAccount, Where<EMailAccount.emailAccountID, Equal<Required<EMailAccount.emailAccountID>>>>.Select(_graph,
			                message.MailAccountID);			        
			        if (account != null && account.IncomingProcessing == true)
			        {
			            var ticket = EncodeTicket(message.ID.GetValueOrDefault());
			            subject += " " + ticket;
			        }
			    }
			    return subject;
			}

			private string EncodeTicket(int id)
			{
			    string emailTagPrefix = "[";
			    string emailTagSuffix = "]";
                foreach (PreferencesEmail curacc in PXSelect<PreferencesEmail>.Select(_graph))
                {
                    if (curacc.DefaultEMailAccountID != null)
                    {
                        emailTagPrefix = curacc.EmailTagPrefix;
                        emailTagSuffix = curacc.EmailTagSuffix;
                    }
                }
				return emailTagPrefix + id + emailTagSuffix;
			}

			private AttachmentCollection ReadAttachments(SMEmail message)
			{
				var fs = new AttachmentCollection(_graph);

				foreach (NoteDoc notes in
					PXSelect<NoteDoc,
						Where<NoteDoc.fileID, IsNotNull,
							And<NoteDoc.noteID, Equal<Required<NoteDoc.noteID>>>>>.
						Select(_graph, message.RefNoteID))
				{
					fs.Add((Guid)notes.FileID);
				}

				return fs;
			}

			public void Dispose()
			{
				_mailer.Dispose();
			}
		}

		#endregion

		public void Send(int accountId)
		{
			var graph = new PXGraph();
			graph.SelectTimeStamp();

			if (MailAccountManager.IsMailProcessingOff) throw new PXException(Messages.MailProcessingIsTurnedOff);

			using (var processor = new MessageProcessor(accountId))
			{
				processor.ProcessAll();
			}
		}

		public void SendMessage(object message)
		{
			if (message == null) throw new ArgumentNullException(nameof(message), PXMessages.LocalizeNoPrefix(Messages.Message));

			if (!(message is SMEmail))
			{
				string errorText = PXMessages.LocalizeFormatNoPrefixNLA(Messages.CanNotProcessMessage, message.GetType().Name,
					typeof(SMEmail).Name);
				throw new ArgumentException(errorText, PXMessages.LocalizeNoPrefix(Messages.Message));
			}

			SMEmail email = message as SMEmail;

			if (email.MailAccountID != null && PX.Objects.CS.Email.PXEmailSyncHelper.IsExchange(email.MailAccountID.Value))
			{
				CRSMEmail emailProjection =
						PXSelect<CRSMEmail, Where<CRSMEmail.noteID, Equal<Required<SMEmail.refNoteID>>>>.Select(new PXGraph(), email.RefNoteID);

				CS.Email.PXEmailSyncHelper.SendMessage(emailProjection);
			}
			else
				using (var processor = new MessageProcessor(email.MailAccountID))
				{
					processor.Process(email);
				}
		}
	}
}
