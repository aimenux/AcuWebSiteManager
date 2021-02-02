using System;
using System.Collections;
using PX.Data;
using PX.Data.Wiki.Parser;
using PX.Objects.EP;
using PX.SM;

namespace PX.Objects.CR
{
	public class SendKBArticleMaint : PXGraph<SendKBArticleMaint>
	{
		#region EmailMessage

		[Serializable]
		public partial class EmailMessage : IBqlTable
		{
			#region MailAccountID
			public abstract class mailAccountID : PX.Data.BQL.BqlInt.Field<mailAccountID> { }

			[PXInt]
			[PXUIField(DisplayName = "From")]
			[PXEMailAccountIDSelectorAttribute]
			[PXDefault]
			public virtual int? MailAccountID { get; set; }
			#endregion

			#region MailTo

			public abstract class mailTo : PX.Data.BQL.BqlString.Field<mailTo> { }

			[PXString(255)]
			[PXUIField(DisplayName = "To")]
			[PXDefault]
			public virtual string MailTo { get; set; }

			#endregion

			#region MailCc

			public abstract class mailCc : PX.Data.BQL.BqlString.Field<mailCc> { }

			[PXString(255)]
			[PXUIField(DisplayName = "CC")]
			public virtual string MailCc { get; set; }

			#endregion

			#region MailBcc

			public abstract class mailBcc : PX.Data.BQL.BqlString.Field<mailBcc> { }

			[PXString(255)]
			[PXUIField(DisplayName = "BCC")]
			public virtual string MailBcc { get; set; }

			#endregion

			#region Subject

			public abstract class subject : PX.Data.BQL.BqlString.Field<subject> { }

			[PXString(255)]
			[PXDefault]
			[PXUIField(DisplayName = "Subject")]
			public virtual string Subject { get; set; }

			#endregion

			#region MailUser

			public abstract class mailUser : PX.Data.BQL.BqlGuid.Field<mailUser> { }

			[PXDBGuid]
			public virtual Guid? MailUser { get; set; }

			#endregion

			#region WikiText
			public abstract class wikiText : PX.Data.BQL.BqlString.Field<wikiText> { }

			[PXString]
			public virtual string WikiText { get; set; }
			#endregion

			#region WikiID

			public abstract class wikiID : PX.Data.BQL.BqlGuid.Field<wikiID> { }

			[PXGuid]
			public virtual Guid? WikiID { get; set; }

			#endregion
		}

		#endregion

		#region ArticleSelectorAttribute

		public sealed class ArticleSelectorAttribute : PXCustomSelectorAttribute
		{
			public PXSelectBase<WikiPage> _select;
			public SendKBArticleMaint _graph;

			public ArticleSelectorAttribute() 
				: base(typeof(WikiPage.pageID))
			{
			}

			public override void CacheAttached(PXCache sender)
			{
				base.CacheAttached(sender);

				_graph = sender.Graph as SendKBArticleMaint;
				if (_graph != null)
					_select = new PXSelect<WikiPage,
							Where<WikiPage.name, NotLike<TemplateLeftLike>,
								And<WikiPage.name, NotLike<GenTemplateLeftLike>,
								And<WikiPage.name, NotLike<ContainerTemplateLeftLike>,
								And<WikiPage.wikiID, Equal<Required<WikiPage.wikiID>>>>>>>(_graph);
			}

			public IEnumerable GetRecords()
			{
				if (_select == null) yield break;

				foreach (PXResult<WikiPage> record in _select.Select(_graph.Message.Current.WikiID))
					yield return (WikiPage)record;
			}
		}

		#endregion

		#region Article

		[Serializable]
		public partial class Article : IBqlTable
		{
			#region ArticleID

			public abstract class articleID : PX.Data.BQL.BqlGuid.Field<articleID> { }

			[PXGuid]
			[PXUIField(DisplayName = "Article")]
			[ArticleSelector]
			public virtual Guid? ArticleID { get; set; }

			#endregion

			#region Content

			public abstract class content : PX.Data.BQL.BqlString.Field<content> { }

			[PXString]
			[PXUIField(DisplayName = "Wiki Text", IsReadOnly = true)]
			public virtual String Content { get; set; }

			#endregion

			#region ParsedContent

			public abstract class parsedContent : PX.Data.BQL.BqlString.Field<parsedContent> { }

			[PXString]
			[PXUIField(Visible = false)]
			public virtual String ParsedContent { get; set; }

			#endregion
		}

		#endregion

		#region Selects

		public PXFilter<EmailMessage> Message;

		public PXFilter<Article> InsertArticleFilter;

		#endregion

		#region Actions

		public PXCancel<EmailMessage> Cancel;

		public PXAction<EmailMessage> Send;

		[PXButton(Tooltip = Messages.SendMail, ImageKey = PX.Web.UI.Sprite.Main.MailSend)]
		[PXUIField(DisplayName = Messages.Send)]
		public virtual IEnumerable send(PXAdapter adapter)
		{
			var emailMessage = Message.Current;

			CheckValue(emailMessage.MailAccountID, "From");
			CheckText(emailMessage.MailTo, "To");
			CheckText(emailMessage.Subject, "Subject");

			WikiDescriptorExt record = PXSelect<WikiDescriptorExt,
					Where<WikiDescriptorExt.pageID, Equal<Required<WikiDescriptorExt.pageID>>>>.
				Select(this, emailMessage.WikiID);
			var wikiSettings = WikiSettings;
			var newSettings = wikiSettings == null ? new PXSettings() : new PXSettings(wikiSettings);
			newSettings.ExternalRootUrl = record == null ? null : record.PubVirtualPath;

			var sender = new NotificationGenerator(this)
							{
								MailAccountId = emailMessage.MailAccountID,
								Subject = emailMessage.Subject,
								To = emailMessage.MailTo,
								Cc = emailMessage.MailCc,
								Bcc = emailMessage.MailBcc,
								Body = emailMessage.WikiText,
								//WikiSettings = newSettings,
								//WikiID = emailMessage.WikiID
							};
			sender.Send();

			return adapter.Get();
		}

		public PXAction<EmailMessage> insertArticle;

		[PXButton(ImageKey = PX.Web.UI.Sprite.Main.AddArticle,
			Tooltip = Messages.InsertArticleToolTip)]
		[PXUIField(DisplayName = Messages.InsertArticle)]
		public virtual IEnumerable InsertArticle(PXAdapter adapter)
		{
			if (InsertArticleFilter.AskExt("InsertArticleFilter", InitializeInsertArticleFilter, true) == WebDialogResult.OK)
			{
				Message.Current.WikiText += InsertArticleFilter.Current.ParsedContent;
			}
			return adapter.Get();
		}

		#endregion

		#region Event Handlers

		protected virtual void Article_RowUpdated(PXCache cache, PXRowUpdatedEventArgs e)
		{
			var row = e.Row as Article;
			if (row == null) return;

			string newContent = null;
			if (row.ArticleID != null) 
			{
				var reader = PXGraph.CreateInstance<WikiReader>();
				reader.Filter.Current.PageID = row.ArticleID;
				var page = reader.ReadPage();
				if (page != null) newContent = page.Content;
				
			}
			row.Content = newContent;
		}

		protected virtual void EmailMessage_RowInserted(PXCache cache, PXRowInsertedEventArgs e)
		{
			var row = e.Row as EmailMessage;
			if (row == null) return;

			EMailAccount mailAccount = PXSelectJoin<EMailAccount,
				LeftJoin<PreferencesEmail, On<PreferencesEmail.defaultEMailAccountID, Equal<EMailAccount.emailAccountID>>>,
				Where<Match<Current<AccessInfo.userName>>>,
				OrderBy<Desc<PreferencesEmail.defaultEMailAccountID, Asc<EMailAccount.address>>>>
						.Select(this);
			if (mailAccount != null)
				row.MailAccountID = mailAccount.EmailAccountID;
		}

		#endregion

		#region Public Methods

		public ISettings WikiSettings { get; set; }

		#endregion

		#region Private Methods

		private void InitializeInsertArticleFilter(PXGraph graph, string name)
		{
			InsertArticleFilter.Current.ArticleID = null;
			InsertArticleFilter.Current.Content = null;
		}

		private static void CheckValue(object val, string fieldName)
		{
			if (val == null)
				throw new PXSetPropertyException(PXMessages.LocalizeFormatNoPrefixNLA(Messages.IncorrectDataInField, fieldName));
		}

		private static void CheckText(string subject, string fieldName)
		{
			if (string.IsNullOrEmpty(subject))
				throw new PXSetPropertyException(PXMessages.LocalizeFormatNoPrefixNLA(Messages.IncorrectDataInField, fieldName));
		}

		#endregion
	}
}
