using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using PX.Common;
using PX.Common.Mail;
using PX.Common.MIME;
using PX.Data;
using PX.Data.EP;
using PX.SM;
using PX.Objects.CR;
using Email = PX.Common.Mail.Email;
using System.Collections.Concurrent;

namespace PX.Objects.EP
{
	public sealed class CommonMailReceiveProvider : IMailReceiveProvider, IMessageProccessor, IOriginalMailProvider
	{
		#region UploadFile
		[Serializable]
        [PXHidden]
		public class UploadFile : IBqlTable
		{
			#region FileID
			public abstract class fileID : PX.Data.BQL.BqlGuid.Field<fileID> { }

			[PXDBGuid(IsKey = true)]
			public virtual Guid? FileID { get; set; }
			#endregion

			#region Name
			public abstract class name : PX.Data.BQL.BqlString.Field<name> { }
			[PXDBString(InputMask = "", IsUnicode = true)]
			public virtual string Name { get; set; }
			#endregion

			#region Versioned
			public abstract class versioned : PX.Data.BQL.BqlBool.Field<versioned> { }

			[PXDBBool]
			public virtual Boolean? Versioned { get; set; }
			#endregion

			#region IsPublic
			public abstract class isPublic : PX.Data.BQL.BqlBool.Field<isPublic> { }

			[PXDBBool]
			public virtual Boolean? IsPublic { get; set; }
			#endregion

			#region CreatedByID
			public abstract class createdByID : PX.Data.BQL.BqlGuid.Field<createdByID> { }
			[PXDBCreatedByID]
			public virtual Guid? CreatedByID { get; set; }
			#endregion

			#region CreatedDateTime
			public abstract class createdDateTime : PX.Data.BQL.BqlDateTime.Field<createdDateTime> { }
			[PXDBCreatedDateTime]
			public virtual DateTime? CreatedDateTime { get; set; }
			#endregion

			#region LastRevisionID
			public abstract class lastRevisionID : PX.Data.BQL.BqlInt.Field<lastRevisionID> { }
			[PXDBInt]
			[PXDefault(0)]
			public virtual int? LastRevisionID { get; set; }
			#endregion

			#region PrimaryScreenID
			public abstract class primaryScreenID : PX.Data.BQL.BqlString.Field<primaryScreenID> { }
			[PXDBString]
			public virtual string PrimaryScreenID { get; set; }
			#endregion

			#region tstamp
			public abstract class Tstamp : PX.Data.BQL.BqlByteArray.Field<Tstamp> { }
			[PXDBTimestamp]
			public virtual Byte[] tstamp { get; set; }
			#endregion

			#region NoteID
			public abstract class noteID : PX.Data.BQL.BqlGuid.Field<noteID> { }
			[PXNote]
			public virtual Guid? NoteID { get; set; }
			#endregion
		}
		#endregion

		#region ImageReplacer

		// todo: unify with EmbeddedImagesExtractorExtension
		public sealed class ImageReplacer : Dictionary<string, Guid>
		{
			private static readonly Regex rHref = new Regex("<a href=\\\"cid:.*?>\\s*(?<RefContent>.*?)</a>",
				RegexOptions.IgnoreCase | RegexOptions.Multiline | RegexOptions.Singleline | RegexOptions.Compiled);
			private static readonly Regex rImg = new Regex("<img\\s+(?<Content>[^>]*?)>",
				RegexOptions.IgnoreCase | RegexOptions.Multiline | RegexOptions.Singleline | RegexOptions.Compiled);
			private static readonly Regex rContent = new Regex("(?<Item>.*?)\\s*=\\s*(?<Value>(\\\".*?\\\")|(\\d+))\\s*",
				RegexOptions.IgnoreCase | RegexOptions.Multiline | RegexOptions.Singleline | RegexOptions.Compiled);

			public string Replace(string source)
			{
				string result = rHref.Replace(source, ReplaceLink);
				return rImg.Replace(result, ReplaceImage);
			}

			private string ReplaceLink(Match refMatch)
			{
				Match match = rImg.Match(refMatch.Groups["RefContent"].Value);
				return ReplaceImage(match)/*.Replace("|noclick", string.Empty)*/;
			}

			private string ReplaceImage(Match match)
			{
				if (!string.IsNullOrEmpty(match.Groups["Content"].Value))
				{
					Guid fileID = Guid.Empty;
					//bool border = false;
					int width = 0, height = 0;
					string title = null;
					foreach (Match item in rContent.Matches(match.Groups["Content"].Value))
					{
						string value = item.Groups["Value"].Value;
						if (value.StartsWith("\"") && value.EndsWith("\""))
							value = value.Substring(1, value.Length - 2);
						switch (item.Groups["Item"].Value.ToLower())
						{
							case "border":
								//if (value != "0") border = true;
								break;
							case "width":
								int.TryParse(value, out width);
								break;
							case "height":
								int.TryParse(value, out height);
								break;
							case "alt":
								title = value;
								break;
							case "src":
								if (fileID == Guid.Empty && value.Length > 4)
								{
									var cid = "<" + value.Substring(4) + ">";
									this.TryGetValue(cid, out fileID);
								}
								break;
						}
					}
					if (fileID != Guid.Empty)
					{
						/*string result = "[Image:" + fileID;
						if (border) result += "|border";
						if (width != 0 && height != 0)
							result += "|" + width + "x" + height;
						else if (width != 0)
							result += "|" + width;
						if (title != null)
							result += "|" + title;
						return result + "|noclick]";*/
						var result = "<img src=\"~/Frames/GetFile.ashx?fileID=" + fileID + "\" ";
						//TODO: need review
						if (width != 0)
							result += "width=\"" + width + "\" ";
						if (height != 0)
							result += "height=\"" + height + "\" ";
						result += " />";
						return result;
					}
				}
				return match.Value;
			}
		}

		#endregion

		#region MailProcessor

		private sealed class MailProcessor
		{
            private static readonly Regex extRegex = new Regex(@"\w+", RegexOptions.Multiline | RegexOptions.Compiled);

			private readonly Regex _tRegex;
			private readonly PXGraph _graph;
			private readonly Email _email;
			private readonly EMailAccount _account;

			private CRSMEmail _activityMessage;
			private CRActivity _activity;
			private SMEmail _message;

			private MailProcessor(PXGraph graph, EMailAccount account)
			{
				if (graph == null) throw new ArgumentNullException("graph");
				if (account == null) throw new ArgumentNullException("account");

				string emailTagPrefix = @"\[";
				string emailTagSuffix = @"\]";

				foreach (PreferencesEmail curacc in PXSelect<PreferencesEmail>.Select(graph).RowCast<PreferencesEmail>().Where(curacc => curacc.DefaultEMailAccountID != null))
				{
					emailTagPrefix = curacc.EmailTagPrefix;
					emailTagSuffix = curacc.EmailTagSuffix;
					foreach (char c in "[]()^|?*.-+")
					{
						string enforcedSymbol = @"\" + c.ToString();
						emailTagPrefix = emailTagPrefix.Replace(c.ToString(), enforcedSymbol);
						emailTagSuffix = emailTagSuffix.Replace(c.ToString(), enforcedSymbol);
					}
				}

				string reqegx = emailTagPrefix + @"(?<Ticket>\d+?)" + emailTagSuffix;

				_tRegex = new Regex(reqegx, RegexOptions.Multiline | RegexOptions.Compiled);
				_graph = graph;
				_account = account;

				PXCache scache = _graph.Caches[typeof(CRActivityStatistics)];
				_graph.Views.Caches.Remove(typeof(CRActivityStatistics));
				_graph.Views.Caches.Add(typeof(CRActivityStatistics));
			}

			private MailProcessor(PXGraph graph, EMailAccount account, Email email)
				: this(graph, account)
			{
				if (email == null) throw new ArgumentNullException("email");

				_email = email;
			}

			private MailProcessor(PXGraph graph, EMailAccount account, SMEmail message)
				: this(graph, account)
			{
				if (message == null) throw new ArgumentNullException("message");

				_message = message;
			}

			public static void Proccess(PXGraph graph, EMailAccount account, Email email)
			{
				var obj = new MailProcessor(graph, account, email);
				obj.Proccess();
			}

			public static void Proccess(PXGraph graph, EMailAccount account, SMEmail email)
			{
				var obj = new MailProcessor(graph, account, email);
				obj.Proccess();
			}

			private void Proccess()
			{
				using (new PXScreenIDScope("CR306015"))
				{
					try
					{
						if (CreateMessage()) ProcessMessage();
					}
					finally
					{
						_graph.Clear();
						_activity = null;
						if (_activityMessage != null)
						{
							_message.Exception = _activityMessage.Exception;
							_message.MPStatus = _activityMessage.MPStatus;
						}
						_message = null;
						_activityMessage = null;
					}
				}
			}

			private bool CreateMessage()
			{
				if (_message != null)
				{
					_activityMessage = PXSelect<CRSMEmail,
						Where<CRSMEmail.noteID, Equal<Required<SMEmail.refNoteID>>>>.Select(_graph, _message.RefNoteID);

					return true;
				}

				try
				{
					if (_email.Message.From == null || _email.Message.From.Count() == 0)
						return false;

					using (PXTransactionScope ts = new PXTransactionScope())
					{
						// Only two functions in scope so as not to create activity without email,
						// but also to prevent repetition of processing messages with bad attachments
					CreateActivity();
					CreateEmail();

						ts.Complete();
					}

					if (_message == null) return false;
					
					_activityMessage = PXSelect<CRSMEmail,
						Where<CRSMEmail.noteID, Equal<Required<SMEmail.refNoteID>>>>.Select(_graph, _message.RefNoteID);
					
					AppendAttachments();
					PersistAM();
					return true;
				}
				catch (Exception ex)
				{
					PXTrace.WriteError(ex);
					PersistException(ex);
					return false;
				}
			}

			private void AppendAttachments()
			{
				var allowedExtansions = AllowedFileExtentions(_account.IncomingAttachmentType);
				var images = CreateAttachments(allowedExtansions);
				ReplaceInlineImages(images);

			}

			private void ReplaceInlineImages(IEnumerable<KeyValuePair<string, Guid>> createAttachments)
			{
				var imageReplacer = new ImageReplacer();
				foreach (KeyValuePair<string, Guid> pair in createAttachments)
					if (!imageReplacer.ContainsKey(pair.Key))
						imageReplacer.Add(pair.Key, pair.Value);
				if (imageReplacer.Count > 0)
				{
					_activityMessage.Body = imageReplacer.Replace(_activityMessage.Body);
					UpdateAM();
				}
			}

			private IEnumerable<KeyValuePair<string, Guid>> CreateAttachments(ICollection<string> allowedExtensions)
			{
				var images = new List<KeyValuePair<string, Guid>>();
				foreach (Entity c in _email.Message.Attachments)
				{
					var newFileId = Guid.NewGuid();
					var name = c.ContentDisposition?.Param_FileName ?? c.ContentType?.Param_Name;
					if (name == null) continue;

					name = name.Trim();
					if (string.IsNullOrEmpty(name)) name = "untitled";

					string ext;
					try
					{
						ext = Path.GetExtension(name);
					}
					catch (ArgumentException ae)
					{
						PXTrace.WriteError(new Exception("Cannot parse extension of the attachment name. Name contains invalid symbols.", ae));
						ext = null;
					}
					if (!string.IsNullOrEmpty(ext)) ext = ext.Substring(1).ToLower();
					if (allowedExtensions != null && !allowedExtensions.Contains(ext))
					{
						continue;
					}

					var content = ((BodySinglepartBase)c.Body).Data;					
					CreateFile(_activityMessage.NoteID.Value, newFileId, name, content);

					if (!string.IsNullOrEmpty(c.ContentID))
						images.Add(new KeyValuePair<string, Guid>(c.ContentID, newFileId));
				}
				return images;
			}

			private ICollection<string> AllowedFileExtentions(string allowedTypes)
			{
				var allowedExtansionsStr = allowedTypes.With(s => s.Trim().ToLower());
				List<string> allowedExtansions = null;
				var allowAny = allowedExtansionsStr == "all";
				if (!allowAny && !string.IsNullOrEmpty(allowedExtansionsStr))
					foreach (Match word in extRegex.Matches(allowedExtansionsStr))
					{
						if (allowedExtansions == null)
							allowedExtansions = new List<string>();
						allowedExtansions.Add(word.Value.ToLower());
					}
				return allowedExtansions;
			}

			// todo: unify with AttachmentsHandlerExtension
			private void CreateFile(Guid noteId, Guid newFileId, string name, byte[] content)
			{
				var noteDocCache = _graph.Caches[typeof(NoteDoc)];
				var noteDoc = (NoteDoc)noteDocCache.CreateInstance();
				noteDoc.NoteID = noteId;
				noteDoc.FileID = newFileId;
				noteDocCache.Insert(noteDoc);
				_graph.EnsureCachePersistence(typeof(NoteDoc));

				var uploadFileCache = _graph.Caches[typeof(UploadFile)];
				var uploadFile = (UploadFile)uploadFileCache.CreateInstance();
				uploadFile.FileID = newFileId;
				uploadFile.LastRevisionID = 1;
				uploadFile.Versioned = true;
				uploadFile.IsPublic = false;
				if (name.Length > 200) name = name.Substring(0, 200);
				uploadFile.Name = newFileId + @"\" + name;
				uploadFile.PrimaryScreenID = "CR306015"; //TODO: need review
				uploadFileCache.Insert(uploadFile);
				_graph.EnsureCachePersistence(typeof(UploadFile));

				var fileRevisionCache = _graph.Caches[typeof(UploadFileRevision)];
				var fileRevision = (UploadFileRevision)fileRevisionCache.CreateInstance();
				fileRevision.FileID = newFileId;
				fileRevision.FileRevisionID = 1;
				fileRevision.Data = content;
				fileRevision.Size = UploadFileHelper.BytesToKilobytes(content.Length);
				fileRevisionCache.Insert(fileRevision);
				_graph.EnsureCachePersistence(typeof(UploadFileRevision));
			}

			private void PersistAM()
			{
				var cached = _graph.Caches[_activityMessage.GetType()].Locate(_activityMessage);
				_graph.Persist();
				_graph.SelectTimeStamp();
				var cache = _graph.Caches[_activityMessage.GetType()];
				_activityMessage = (CRSMEmail)cache.CreateCopy(cached);
			}
			
			private int? DecodeTicket(string str, out string clearStr)
			{
				if (!string.IsNullOrEmpty(str) && _tRegex.IsMatch(str))
				{
					var ticket = _tRegex.Match(str).Groups["Ticket"];
					int res;
					if (int.TryParse(ticket.Value.Trim(), out res)) //TODO: need extract only longest substring of digits
					{
					    clearStr = _tRegex.Replace(str, String.Empty);
						return res;
					}
				}
				clearStr = str;
				return null;
			}

			private void CreateEmail()
			{
				PXCache cache = _graph.Caches[typeof(SMEmail)];

				_message = (SMEmail) cache.CreateCopy(cache.Insert());
				_message.RefNoteID = _activity.NoteID;
				_message.MailAccountID = _account.EmailAccountID;				

				var mimeMessage = _email.Message;
				_message.MailFrom = mimeMessage.From.With(_ =>_.ToString()) ?? string.Empty;
				_message.MailReply = mimeMessage.ReplyTo.With(_ => _.ToString()) ?? string.Empty;

				_message.MailTo = mimeMessage.To.With(_ => _.ToString()) ?? string.Empty;
				_message.MailCc = mimeMessage.Cc.With(_ =>_.ToString()) ?? string.Empty;
				_message.MailBcc = mimeMessage.Bcc.With(_ => _.ToString()) ?? string.Empty;
                _message.Subject = mimeMessage.Subject.With(_ => _.ToString()) ?? " ";

				if (!string.IsNullOrEmpty(_email.UID))
				{
					if (_account.IncomingHostProtocol == IncomingMailProtocolsAttribute._IMAP)
						_message.ImapUID = int.Parse(_email.UID);
					else
						_message.Pop3UID = _email.UID;
				}

				_message.IsIncome = true;
				_message.MessageId = mimeMessage.MessageID;
				_message.MPStatus = MailStatusListAttribute.PreProcess;//TODO: need move into Automation steps

				var body = mimeMessage.BodyHtmlText;
				if (body == null)
				{
					if (mimeMessage.BodyText != null)
					{
						body = PX.Common.Tools.ConvertSimpleTextToHtml(mimeMessage.BodyText);
					}
				}
				else
				{
					body = mimeMessage.BodyHtmlText.
						Replace(Environment.NewLine, " ").
						Replace("<spanstyle", "<span style"); //NOTE: ms outlook style conflicts with our system style

                    Regex baseTag = new Regex(@"<base(.|\n)*?>");
				    body = baseTag.Replace(body, String.Empty);
				}
				_message.Body = body ?? string.Empty;

			    string clearedSubject;

                var ticket = DecodeTicket(_message.Subject, out clearedSubject);
                if (ticket != null) _message.Ticket = CorrectTicket(ticket);

                _message.Subject = clearedSubject;
				
				_message = (SMEmail)cache.CreateCopy(cache.Update(_message));

				_graph.EnsureCachePersistence(_message.GetType());

				var cached = _graph.Caches[_message.GetType()].Locate(_message);
				_graph.Persist();
				_graph.SelectTimeStamp();
				_message = (SMEmail)cache.CreateCopy(cached);
			}

			private void CreateActivity()
			{
				PXCache cache = _graph.Caches[typeof(CRActivity)];

				_activity = (CRActivity)cache.CreateCopy(cache.Insert());

				_activity.ClassID = CRActivityClass.Email;
				_activity.Type = null;

				_activity.Subject = _email.Message.Subject.With(_ => _.ToString()) ?? " ";

				string clearedSubject;
				DecodeTicket(_activity.Subject, out clearedSubject);
				_activity.Subject = clearedSubject;

				_activity.StartDate = _email.Message.Date == DateTime.MinValue ? PXTimeZoneInfo.Now : PXTimeZoneInfo.ConvertTimeFromUtc(_email.Message.Date.ToUniversalTime(), LocaleInfo.GetTimeZone());
				
				_activity = (CRActivity)cache.CreateCopy(cache.Update(_activity));

				_graph.EnsureCachePersistence(_activity.GetType());

				var cached = _graph.Caches[_activity.GetType()].Locate(_activity);
				_graph.Persist();
				_graph.SelectTimeStamp();
				_activity = (CRActivity)cache.CreateCopy(cached);
			}

			private void UpdateAM()
			{
				Type type = _activityMessage.GetType();
				var cache = _graph.Caches[type];
				_activityMessage = (CRSMEmail)cache.CreateCopy(cache.Update(_activityMessage));
				
				_graph.EnsureCachePersistence(type);
			}
			
			private void ProcessMessage()
			{
				using (PXTransactionScope sc = new PXTransactionScope())
				{
					try
					{
						PreProcessActivity();
					}
					catch (Exception)
					{
						//Unable to change status - leave message alone.
						return;
					}					
					try
					{						
						ProcessActivity();
						PostProcessActivity();
					}
					catch (Exception ex)
					{
						PersistException(ex);
					}					
					sc.Complete();
				}
			}

			private void PreProcessActivity()
			{
				_activityMessage.MPStatus = MailStatusListAttribute.InProcess;
				_activityMessage.Exception = null;
				UpdateAM();
				PersistAM();
			}

			private void PostProcessActivity()
			{
				var isDeleted = _graph.Caches[_activityMessage.GetType()].GetStatus(_activityMessage) == PXEntryStatus.Deleted;

				if (_email != null && _email.Exception != null && _activityMessage.Exception == null)
					_activityMessage.Exception = _email.Exception.Message;

				_activityMessage.MPStatus = MailStatusListAttribute.Processed;
				_activityMessage.UIStatus = ActivityStatusAttribute.Completed;
				UpdateAM();
				PersistAM();

				if (isDeleted)
				{
					_graph.Caches[_activityMessage.GetType()].SetStatus(_activityMessage, PXEntryStatus.Deleted);
					PersistAM();
				}
			}
			private void PersistException(Exception ex)
			{
				this._graph.Clear();
				if (_activityMessage == null || _graph.Caches[_activityMessage.GetType()].GetStatus(_activityMessage) == PXEntryStatus.Inserted) return;

				_activityMessage = PXSelect<CRSMEmail, Where<CRSMEmail.noteID, Equal<Required<CRSMEmail.noteID>>>>.SelectWindowed(_graph, 0, 1, _activityMessage.NoteID);
				if (_activityMessage != null)
				{
					_activityMessage = (CRSMEmail)_graph.Caches[_activityMessage.GetType()].CreateCopy(_activityMessage);
					_activityMessage.Exception = ex.Message;
					_activityMessage.MPStatus = MailStatusListAttribute.Failed;
					UpdateAM();
					PersistAM();
				}
			}

			private void ProcessActivity()
			{
				var result = EmailProcessorManager.Handlers.GetEnumerator();
				
				var args = new EmailProcessEventArgs(_graph, _account, _activityMessage);
				while (result.MoveNext())
				{
					var processor = result.Current;
					if (processor != null)
						try
						{
							processor.Process(args);
						}
						catch (Exception ex)
						{
							args.IsSuccessful = false;
							_activityMessage.Exception += ex.Message;
							_activityMessage.Exception += Environment.NewLine + AddUIErrorsInfo(ex);                           
						}
				}
				if (_email != null && _email.Exception != null)
					_activityMessage.Exception = _email.Exception.Message;
				//else if (!args.IsSuccessful && string.IsNullOrEmpty(_activityMessage.Exception))
				//	_activityMessage.Exception = Messages.NotPossibleProcessMessage;

				if (_activityMessage.Exception != null && !_activityMessage.Exception.Equals(PXMessages.LocalizeNoPrefix(Common.Messages.CannotDecodeBase64ContentExplicit) + "\r\n"))
					throw new PXException(_activityMessage.Exception);
			}

			private string AddUIErrorsInfo(Exception source)
			{
				var ex = source as PXOuterException;
				if (ex == null || ex.Row == null) return string.Empty;

				var res = new System.Text.StringBuilder();
				var cache = _graph.Caches[ex.Row.GetType()];
				var errors = PXUIFieldAttribute.GetErrors(cache, ex.Row);
				if (errors != null)
					try
					{
                        res.AppendLine(string.Format("=== {0} ===", PXMessages.LocalizeNoPrefix(Messages.Errors)));
                        foreach (KeyValuePair<string, string> error in errors)
						{
							res.Append(error.Key);
							res.Append(": ");
							res.AppendLine(error.Value);
						}
						res.AppendLine(string.Format("=== {0} ===", PXMessages.LocalizeNoPrefix(Messages.ChangesetDetails)));
						foreach (string field in cache.Fields)
						{
							var fieldName = field;
							var state = cache.GetStateExt(ex.Row, field) as PXFieldState;
							if (state != null && !string.IsNullOrWhiteSpace(state.DisplayName))
								fieldName = state.DisplayName;

							res.Append(fieldName);
							res.Append(": ");
							var value = cache.GetValue(ex.Row, field);
							if (value != null) res.AppendLine(value.ToString());
							else res.AppendLine();
						}
					}
					catch (StackOverflowException) { throw; }
					catch (OutOfMemoryException) { throw; }
					catch { }
				return res.ToString();
			}

			private int? CorrectTicket(int? originalVal)
			{
				var act = (CRSMEmail)PXSelectJoin<CRActivity, 
					InnerJoin<CRSMEmail,
						On<CRSMEmail.parentNoteID, Equal<CRActivity.noteID>>>,
					Where<CRSMEmail.classID, Equal<CRActivityClass.oldEmails>,
						And<CRSMEmail.ticket, Equal<Required<CRSMEmail.ticket>>>>>.
					Select(_graph, originalVal);
				return act != null && act.ID != null 
					? act.ID 
					: originalVal;
			}
		}

		#endregion

		#region UIDsCache

		private class UIDsCache
		{
			private class ListEnumerator : IEnumerator, IEnumerable
			{
				private readonly IList _items;
				private readonly int _startIndex;
				private int _currentIndex;
				private object _current;

				public ListEnumerator(IList items, int startIndex)
				{
					if (items == null) throw new ArgumentNullException("items");
					if (startIndex < 0) throw new ArgumentOutOfRangeException("startIndex");

					_items = items;
					_startIndex = startIndex;
					Reset();
				}

				public bool MoveNext()
				{
					_current = null;

					while (_current == null)
					{
						if (_currentIndex >= _items.Count - 1) 
							return false;

						_currentIndex++;
						_current = _items[_currentIndex];
					}
					return true;
				}

				public void Reset()
				{
					_currentIndex = _startIndex - 1;
					MoveNext();
				}

				public object Current
				{
					get { return _current; }
				}

				public IEnumerator GetEnumerator()
				{
					return this;
				}
			}

			private class UIDsCacheInterface : MailReceiver.IReadUIDsCollection
			{
				private readonly UIDsCache _cache;
				private readonly bool _isImap;

				public UIDsCacheInterface(UIDsCache cache, bool isImap)
				{
					if (cache == null) throw new ArgumentNullException("cache");
					_cache = cache;
					_isImap = isImap;
				}

				public MailReceiver.ReadUIDs Get(int marker)
				{
					var items = _isImap ? _cache._imap : _cache._pop3;
					var iterator = new ListEnumerator(items, marker);
					return new MailReceiver.ReadUIDs(iterator, items.Count);
				}

				public void Add(object uid)
				{
					if (uid == null) throw new ArgumentNullException("uid");
					var pop3 = _isImap ? null : uid;
					var imap = _isImap ? uid : null;
					_cache.Add(pop3, imap);
				}
			}

			private void Add(object pop3, object imap)
			{
				if (pop3 != null) _pop3.Add(pop3.ToString());
				int imapInt;
				if (imap != null && int.TryParse(imap.ToString(), out imapInt))
					_imap.Add(imapInt);
			}

			private static readonly IDictionary<int, UIDsCache> _items;

			private readonly IList _pop3;
			private readonly IList _imap;

			private int _accountId;
			private DateTime _lastCreated;

			static UIDsCache()
			{
				_items = new ConcurrentDictionary<int, UIDsCache>();
			}

			private UIDsCache()
			{
				_pop3 = new List<string>();
				_imap = new List<int>();
			}

			public static MailReceiver.IReadUIDsCollection Get(int accountId, bool isImap)
			{
				UIDsCache res;
				if (!_items.TryGetValue(accountId, out res))
				{
					res = new UIDsCache();
					res._accountId = accountId;
					res._lastCreated = new DateTime(1970, 01, 01);
					_items[accountId] = res;
				}
				res.Prefetch();
				return new UIDsCacheInterface(res, isImap);
			}

			private void Prefetch()
			{
				var _sqlBatchSize = 100000;
				var graph = new PXGraph();
				var select = new PXSelect<SMEmail,
								Where<SMEmail.mailAccountID, Equal<Required<SMEmail.mailAccountID>>,
									And<SMEmail.createdDateTime, Greater<Required<SMEmail.createdDateTime>>>>,
								OrderBy<Asc<SMEmail.createdDateTime>>>(graph);

				using (new PXFieldScope(select.View,
						typeof(SMEmail.noteID),
						typeof(SMEmail.mailAccountID),
						typeof(SMEmail.createdDateTime),
						typeof(SMEmail.pop3UID),
						typeof(SMEmail.imapUID)))
				using (new PXReadDeletedScope())
				{
					var hasMore = true;
					while (hasMore)
					{
						hasMore = false;
						foreach (SMEmail record in select.SelectWindowed(0, _sqlBatchSize, _accountId, _lastCreated))
						{
							hasMore = true;
							var createdDate = record.CreatedDateTime;
							if (createdDate == null) continue;

							var pop3Uid = record.Pop3UID;
							var imapUid = record.ImapUID;

							_lastCreated = (DateTime)createdDate;

							if (!string.IsNullOrEmpty(pop3Uid))
								_pop3.Add(pop3Uid);

							if (imapUid != null)
								_imap.Add((int)imapUid);
						}
					}
				}
			}
		}

		#endregion

		#region Fields

		private readonly PXGraph _graph;

		private static readonly Dictionary<string, MailReceiver.Context> _contexts;

		#endregion

		#region Ctors

		static CommonMailReceiveProvider()
		{
			_contexts = new Dictionary<string, MailReceiver.Context>();
		}

		public CommonMailReceiveProvider() { }

		public CommonMailReceiveProvider(PXGraph graph)
			: this()
		{
			if (graph == null) throw new ArgumentNullException("graph");
			_graph = graph;
		}

		#endregion

		#region Public methods

		public void Receive(int accountId)
		{
			var graph = GetGraph();
			if (MailAccountManager.IsMailProcessingOff) 
                throw new PXException(Messages.MailProcessingIsTurnedOff);

			var account = GetAccount(graph, accountId);
			var isImap = account.IncomingHostProtocol == IncomingMailProtocolsAttribute._IMAP;
			var context = GetContext(accountId, isImap);

			using (MailReceiver mailer = GetReceiver(account))
			{
			    foreach (Email mail in mailer.Receive(context))
				//if (!mail.Message.IsFailed)
			    {
			        var messageId = mail.Message.MessageID;

			        if (IsMessageExists(accountId, messageId))
			        {
			            if (TryUpdateActivity(isImap, accountId, messageId, mail.UID))
							context.ReadUIDs.Add(mail.UID);		// because can be ignored by _lastTaskId in prefetch
			        }
			        else
			        {
			            MailProcessor.Proccess(graph, account, mail);

						context.ReadUIDs.Add(mail.UID);			// because can be ignored by _lastTaskId in prefetch
			        }

			    }
			}
		}

	    private MailReceiver.Context GetContext(int accountId, bool isImap)
		{
			MailReceiver.Context res = null;
			var key = accountId + "_" + isImap;			
			var cache = UIDsCache.Get(accountId, isImap);

			lock (_contexts)
			{
				MailReceiver.Context oldContext;
				res = _contexts.TryGetValue(key, out oldContext)
					? new MailReceiver.Context(oldContext, cache)
					: new MailReceiver.Context(cache);
				_contexts[key] = res;
			}
			return res;
		}

		public void Process(object message)
		{
			var activity = message as SMEmail;
			if (activity == null) throw new ArgumentException("message");

			var graph = GetGraph();
			var account = GetAccount(graph, (int)activity.MailAccountID);
			MailProcessor.Proccess(graph, account, activity);			
		}

		public void ProcessMessage(EMailAccount account, object message)
		{
			if (account == null) throw new ArgumentNullException("account");

			var activity = message as SMEmail;
			if (activity == null) throw new ArgumentException("message");

			var graph = GetGraph();
			MailProcessor.Proccess(graph, account, activity);
		}

		public Email GetMail(object message)
		{
			var activity = message as SMEmail;
			if (activity == null || activity.IsIncome != true)
				throw new PXException(Messages.InvalidEmailForOperation);
			
			var graph = GetGraph();
			var account = activity.MailAccountID.With(_ => GetAccount(graph, _));
			if (account == null)
				throw new PXException(Messages.MailAccountNotSpecified);

			var isImap = account.IncomingHostProtocol == IncomingMailProtocolsAttribute._IMAP;
			object uid = null;
			if (isImap)
			{
				if (activity.ImapUID == null || activity.ImapUID < 0)
					throw new PXException(Messages.InvalidEmailForOperation);
				uid = activity.ImapUID;
			}
			else
			{
				if (string.IsNullOrEmpty(activity.Pop3UID))
					throw new PXException(Messages.InvalidEmailForOperation);
				uid = activity.Pop3UID;
			}

			var receiver = MailAccountManager.GetReceiver(account);
			if (receiver == null)
				throw new PXException(ErrorMessages.IncomingAccountNotConfigurated);

			return receiver.ReceiveMail(uid);
		}

		#endregion

		#region Private Methods

		private PXGraph GetGraph()
		{
			var graph = _graph ?? new PXGraph();
			graph.SelectTimeStamp();
			graph.FieldVerifying.AddHandler<CRActivity.workgroupID>(delegate(PXCache sender, PXFieldVerifyingEventArgs e) { e.Cancel = true; });
            graph.FieldVerifying.AddHandler<CRActivity.uistatus>(delegate(PXCache sender, PXFieldVerifyingEventArgs e) { e.Cancel = true; });
			return graph;
		}


		private static MailReceiver GetReceiver(EMailAccount account)
		{
			var receiver = MailAccountManager.GetReceiver(account);
			if (receiver == null)
				throw new PXException(ErrorMessages.IncomingAccountNotConfigurated);
			return receiver;
		}

		private static EMailAccount GetAccount(PXGraph graph, int accountId)
		{
			EMailAccount account =
				PXSelect<EMailAccount,
					Where<EMailAccount.emailAccountID, Equal<Required<EMailAccount.emailAccountID>>>>.
					SelectWindowed(graph, 0, 1, accountId);
			if (account == null 
				|| string.IsNullOrEmpty(account.With(_ => _.Address).With(_ => _.Trim())) 
				|| string.IsNullOrEmpty(account.IncomingHostName) && account.EmailAccountType == EmailAccountTypesAttribute.Standard
				|| account.IncomingHostProtocol == IncomingMailProtocolsAttribute._IMAP && string.IsNullOrEmpty(account.ImapRootFolder)
			)
			{
				throw new PXException(ErrorMessages.IncomingAccountNotConfigurated);
			}
			return account;
		}

		private static bool IsMessageExists(int accountId, string messageId)
		{
			using (var record = PXDatabase.SelectSingle<SMEmail>(
				new PXDataField(typeof(SMEmail.noteID).Name),
				new PXDataFieldValue(typeof(SMEmail.mailAccountID).Name, accountId),
				new PXDataFieldValue(typeof(SMEmail.messageId).Name, messageId)))
			{
				if (record != null) return true;
			}
			return false;
		}

		private static bool TryUpdateActivity(bool isImap, int accountId, string messageId, string uid)
		{
			var isUpdated = PXDatabase.Update<SMEmail>(
		        new PXDataFieldAssign(isImap ? typeof(SMEmail.imapUID).Name : typeof(SMEmail.pop3UID).Name, uid),
		        new PXDataFieldRestrict(typeof(SMEmail.mailAccountID).Name, accountId), 
		        new PXDataFieldRestrict(typeof(SMEmail.messageId).Name, messageId),
		        isImap 
                    ? new PXDataFieldRestrict(typeof(SMEmail.imapUID).Name, PXDbType.Int, 4, null, PXComp.ISNULL)
		            : new PXDataFieldRestrict(typeof(SMEmail.pop3UID).Name, PXDbType.NVarChar, 150, null, PXComp.ISNULL)
				);

			return isUpdated;
		}

		#endregion
	}
}
