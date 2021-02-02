using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using PX.Common;
using PX.Data;
using PX.Data.EP;
using PX.Objects.AP;
using PX.Objects.AR;
using PX.Objects.EP;
using PX.SM;
using PX.TM;

namespace PX.Objects.CR
{
	public class CRCommunicationBase<TWhere> : PXGraph<CRCommunicationBase<TWhere>>
		where TWhere : IBqlWhere, new()
	{
		#region Select

		[PXHidden]
		public PXSelect<BAccount> BAccounts;
		[PXHidden]
		public PXSelect<Customer> Customers;
		[PXHidden]
		public PXSelect<Vendor> Vendors;
		[PXHidden]
		public PXSelect<EPEmployee> Employees;
			
		[PXFilterable]
		[PXViewDetailsButton(typeof(Select<CRActivity, Where<CRActivity.noteID, Equal<Current<CRSMEmail.noteID>>>>), OnClosingPopup = PXSpecialButtonType.Refresh, ActionName = "Emails_ViewDetails")]
		public PXSelectJoin<CRSMEmail,
			LeftJoin<EMailAccount, 
				On<EMailAccount.emailAccountID, Equal<CRSMEmail.mailAccountID>>,
			LeftJoin<EPView, 
				On<CRSMEmail.noteID, Equal<EPView.noteID>, And<CRSMEmail.ownerID, Equal<EPView.userID>>>>>,
			Where<CRSMEmail.classID, Equal<CRActivityClass.email>,
				And2<Where<CRSMEmail.createdByID, Equal<Current<AccessInfo.userID>>,
					Or<CRSMEmail.ownerID, Equal<Current<AccessInfo.userID>>,
					Or<CRSMEmail.ownerID, OwnedUser<Current<AccessInfo.userID>>,
					Or<CRSMEmail.workgroupID, InMember<Current<AccessInfo.userID>>>>>>, 
				And<TWhere>>>,
			OrderBy<Desc<CRSMEmail.startDate>>>
			Emails;

		[PXHidden]
		public PXSelect<EMailAccount> account;
		#endregion

		#region Ctors

		public CRCommunicationBase():this(new ButtonMenu[0]){}

		protected CRCommunicationBase(params ButtonMenu[] menus)
		{
			List<ButtonMenu> all = new List<ButtonMenu>
			{
				new ButtonMenu("reply", Messages.Reply, String.Empty),
				new ButtonMenu("replyAll", Messages.ReplyAll, String.Empty),
				new ButtonMenu("forward", Messages.Forward, String.Empty),
			};
			all.AddRange(menus);
			ScreenActions.SetMenu(all.ToArray());
			ScreenActions.MenuAutoOpen = true;
			this.Emails.Cache.AllowUpdate = true;
			this.Emails.AllowUpdate = true;
		}
		#endregion

		#region Event Handlers

		[PXUIField(DisplayName = "Subject", Visibility = PXUIVisibility.SelectorVisible)]
		[PXMergeAttributes(Method = MergeMethod.Merge)]
		protected virtual void CRSMEmail_Subject_CacheAttached(PXCache sender){ }

		[EPStartDate(DisplayName = "Date", DisplayNameDate = "Date", DisplayNameTime = "Time", BqlField = typeof(CRSMEmail.startDate))]
		[PXUIField(DisplayName = "Date")]
		[PXMergeAttributes(Method = MergeMethod.Merge)]
		protected virtual void CRSMEmail_StartDate_CacheAttached(PXCache sender) { }
		
		[PXString(IsUnicode = true)]
		protected virtual void EMailAccount_Password_CacheAttached(PXCache sender) { }
		
		#endregion

		#region Actions

		public PXSave<CRSMEmail> Save;
		public PXCancel<CRSMEmail> Cancel;

		public PXAction<CRSMEmail> ViewEntity;
		[PXUIField(MapEnableRights = PXCacheRights.Select, Visible = false)]
		[PXButton]
		protected void viewEntity()
		{
			if (Emails.Current != null && Emails.Current.RefNoteID != null)
			{
				new EntityHelper(this).NavigateToRow(Emails.Current.RefNoteID, PXRedirectHelper.WindowMode.New);
			}
		}

		public PXAction<CRSMEmail> AddNew;
		[PXUIField(DisplayName = "")]
		[PXInsertButton(Tooltip = Messages.CreateEmail, CommitChanges = true)]
		public virtual void addNew()
		{
			CREmailActivityMaint graph = CreateInstance<CREmailActivityMaint>();
			graph.Message.Current = graph.Message.Insert();
			graph.Message.Cache.IsDirty = false;
			PXRedirectHelper.TryRedirect(graph, PXRedirectHelper.WindowMode.InlineWindow);
		}

		public PXAction<CRSMEmail> EditRecord;
		[PXUIField(DisplayName = "")]
		[PXEditDetailButton(OnClosingPopup = PXSpecialButtonType.Refresh)]
		public virtual void editRecord()
		{
			CRSMEmail item = Emails.Current;
			if (item == null) return;

			CREmailActivityMaint graph = CreateInstance<CREmailActivityMaint>();
			graph.Message.Current = graph.Message.Search<CRSMEmail.noteID>(item.NoteID);
			PXRedirectHelper.TryRedirect(graph, PXRedirectHelper.WindowMode.InlineWindow);
		}

		public PXAction<CRSMEmail> Delete;
		[PXUIField(DisplayName = " ")]
		[PXButton(ImageKey = PX.Web.UI.Sprite.Main.Remove)]
		public virtual IEnumerable delete(PXAdapter adapter)
		{			
			PXLongOperation.StartOperation(this, () => DeleteMessage(SelectedList().RowCast<CRSMEmail>()));	
			return adapter.Get();
		}

		public PXAction<CRSMEmail> ScreenActions;
		[PXUIField(DisplayName = "Actions")]
		[PXButton(MenuAutoOpen = true, SpecialType = PXSpecialButtonType.ActionsFolder)]
		public virtual IEnumerable screenActions(PXAdapter adapter)
		{
			return adapter.Get();
		}

		public PXAction<CRSMEmail> Reply;
		[PXUIField(DisplayName = Messages.Reply)]
		[PXReplyMailButton(OnClosingPopup = PXSpecialButtonType.Refresh)]
		protected void reply()
		{
			CRSMEmail item = Emails.Current;
			if (item == null) return;

			CREmailActivityMaint graph = CreateInstance<CREmailActivityMaint>();
			graph.Message.Current = graph.Message.Search<CRSMEmail.noteID>(item.NoteID);
			graph.Reply.Press();
		}

		public PXAction<CRSMEmail> ReplyAll;
		[PXUIField(DisplayName = Messages.ReplyAll)]
		[PXReplyMailButton(OnClosingPopup = PXSpecialButtonType.Refresh)]
		public virtual void replyAll()
		{
			CRSMEmail item = Emails.Current;
			if (item == null) return;

			CREmailActivityMaint graph = CreateInstance<CREmailActivityMaint>();
			graph.Message.Current = graph.Message.Search<CRSMEmail.noteID>(item.NoteID);
			graph.ReplyAll.Press();
		}

		public PXAction<CRSMEmail> Forward;
		[PXUIField(DisplayName = Messages.Forward)]
		[PXForwardMailButton(OnClosingPopup = PXSpecialButtonType.Refresh)]
		protected void forward()
		{
			CRSMEmail item = Emails.Current;
			if (item == null) return;

			CREmailActivityMaint graph = CreateInstance<CREmailActivityMaint>();
			graph.Message.Current = graph.Message.Search<CRSMEmail.noteID>(item.NoteID);
			graph.Forward.Press();
		}

		public PXAction<CRSMEmail> ViewEMail;
		[PXUIField(DisplayName = "", Visible = false)]
		protected virtual IEnumerable viewEMail(PXAdapter adapter)
		{
			editRecord();
			return adapter.Get();
		}

		public PXAction<CRSMEmail> DoubleClick;
		[PXUIField(DisplayName = "", MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select, Visible = false)]
		[PXLookupButton]
		public virtual IEnumerable doubleClick(PXAdapter adapter)
		{
			return viewEMail(adapter);
		}

		public override bool IsDirty
		{
			get
			{
				TimeSpan span;
                Exception exeption;
				PXLongRunStatus status = PXLongOperation.GetStatus(this.UID, out span, out exeption);
				if (status == PXLongRunStatus.Completed || status == PXLongRunStatus.Aborted)
				{
					return false;
				}

				return this.Caches<CRSMEmail>().Deleted.Count() > 0;
			}
		}

		private static void DeleteMessage(IEnumerable<CRSMEmail> messages)
		{
			CREmailActivityMaint graph = CreateInstance<CREmailActivityMaint>();
			foreach (CRSMEmail message in messages)
			{
				graph.Message.Current = graph.Message.Search<CRSMEmail.noteID>(message.NoteID);
				graph.Delete.Press();
			}
		}

		#endregion

		public override void Persist()
		{
			CREmailActivityMaint graph = CreateInstance<CREmailActivityMaint>();
			foreach (CRSMEmail item in Emails.Cache.Deleted)
			{
				graph.Message.Current = graph.Message.Search<CRSMEmail.noteID>(item.NoteID);
				graph.Delete.Press();
				Emails.Cache.SetStatus(item, PXEntryStatus.Notchanged);
			}
			base.Persist();
		}

		private PXResult<CRSMEmail, EMailAccount, EPView> GetActivityResult(CRSMEmail a)
		{
			EMailAccount acc = PXSelectReadonly<EMailAccount, 
				Where<EMailAccount.emailAccountID, Equal<Required<CRSMEmail.mailAccountID>>>>.Select(this, a.MailAccountID);

			EPView v = PXSelectReadonly<EPView, 
				Where<Required<CRSMEmail.noteID>, Equal<EPView.noteID>, 
					And<Required<CRSMEmail.ownerID>, Equal<EPView.userID>>>>.Select(this, a.NoteID, a.OwnerID);

			return new PXResult<CRSMEmail, EMailAccount, EPView>(a, acc ?? new EMailAccount{EmailAccountID = a.MailAccountID}, v ?? new EPView{NoteID = a.NoteID, UserID = a.OwnerID});
		}

		protected virtual IEnumerable<PXResult<CRSMEmail, EMailAccount, EPView>> SelectedList()
		{
			List<PXResult<CRSMEmail, EMailAccount, EPView>> selected = Emails.Cache.Updated
                .Cast<CRSMEmail>()
                .Where(a => a.Selected == true)
                .Select(GetActivityResult)
                .ToList();

			if (!selected.Any() && Emails.Current != null)
			{
				Emails.Current.Selected = true;
				selected.Add(GetActivityResult(Emails.Current));
			}
			return selected;
		}
	}
}
