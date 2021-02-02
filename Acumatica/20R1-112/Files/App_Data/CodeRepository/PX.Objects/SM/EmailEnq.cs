using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PX.Data;
using PX.Objects.CR;
using PX.Objects.AR;
using PX.Objects.AP;
using PX.Objects.EP;
using PX.Objects.SO;
using PX.SM;
using PX.TM;
//using Messages = PX.Objects.CS.Messages;

namespace PX.Objects.SM
{
	public class EmailEnq : PXGraph<EmailEnq>
	{
	[PXHidden]
		public PXSelect<BAccount> BAccounts;
		[PXHidden]
		public PXSelect<Customer> Customers;
		[PXHidden]
		public PXSelect<Vendor> Vendors;
		[PXHidden]
		public PXSelect<EPEmployee> Employees;

		[PXHidden]
		public PXSelect<CRSMEmail> crEmail;


		[PXFilterable]
		[PXViewDetailsButton(typeof(SMEmail), OnClosingPopup = PXSpecialButtonType.Refresh)]
		public PXSelectJoinOrderBy<SMEmail,
				LeftJoin<EMailAccount,
					On<EMailAccount.emailAccountID, Equal<SMEmail.mailAccountID>>,
				LeftJoin<CRActivity, 
					On<CRActivity.noteID, Equal<SMEmail.refNoteID>>>>,
               OrderBy<Desc<SMEmail.createdDateTime>>> Emails;

		public PXAction<SMEmail> AddNew;
		[PXUIField(DisplayName = "")]
		[PXInsertButton(Tooltip = CR.Messages.AddEmail, CommitChanges = true)]
		public virtual void addNew()
		{
			CREmailActivityMaint graph = CreateInstance<CREmailActivityMaint>();
			graph.Message.Current = graph.Message.Insert();
			graph.Message.Cache.IsDirty = false;
			PXRedirectHelper.TryRedirect(graph, PXRedirectHelper.WindowMode.InlineWindow);
		}

		public PXAction<SMEmail> DoubleClick;
		[PXUIField(DisplayName = "", MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select, Visible = false)]
		public virtual IEnumerable doubleClick(PXAdapter adapter)
		{
			return viewEMail(adapter);
		}

		public PXAction<SMEmail> EditRecord;
		[PXUIField(DisplayName = "")]
		[PXEditDetailButton(OnClosingPopup = PXSpecialButtonType.Refresh)]
		public virtual void editRecord()
		{
			SMEmail item = Emails.Current;
			if (item == null) return;

			if (item.RefNoteID != item.NoteID)
			{
				CREmailActivityMaint graph = CreateInstance<CREmailActivityMaint>();
				graph.Message.Current = graph.Message.Search<CRSMEmail.noteID>(item.RefNoteID);
				PXRedirectHelper.TryRedirect(graph, PXRedirectHelper.WindowMode.InlineWindow);
			}
			else
			{
				CRSMEmailMaint graph = CreateInstance<CRSMEmailMaint>();
				graph.Email.Current = graph.Email.Search<SMEmail.noteID>(item.NoteID);
				PXRedirectHelper.TryRedirect(graph, PXRedirectHelper.WindowMode.InlineWindow);
			}
		}


		public PXAction<SMEmail> Delete;
		[PXUIField(DisplayName = " ")]
		[PXButton(ImageKey = PX.Web.UI.Sprite.Main.Remove)]
		public virtual IEnumerable delete(PXAdapter adapter)
		{
			PXLongOperation.StartOperation(this, () => DeleteMessage(SelectedList().RowCast<SMEmail>()));
			return adapter.Get();
		}

		public PXAction<SMEmail> ViewEMail;
		[PXUIField(DisplayName = "", Visible = false)]
		protected virtual IEnumerable viewEMail(PXAdapter adapter)
		{
			editRecord();
			return adapter.Get();
		}

		public PXAction<SMEmail> ViewEntity;
		[PXUIField(DisplayName = "", Visible = false)]
		protected virtual IEnumerable viewEntity(PXAdapter adapter)
		{
			var row = Emails.Current;
			if (row != null)
			{
				CRActivity activity = PXSelect<CRActivity, Where<CRActivity.noteID, Equal<Required<CRActivity.noteID>>>>.SelectSingleBound(this, null, row.RefNoteID);
				if (activity != null)
				{
					new EntityHelper(this).NavigateToRow(activity.RefNoteID, PXRedirectHelper.WindowMode.New);
				}
			}
			return adapter.Get();
		}

		static void DeleteMessage(IEnumerable<SMEmail> messages)
		{
			foreach (SMEmail message in messages)
			{
				if (message.RefNoteID != message.NoteID)
				{
					CREmailActivityMaint graphCR = CreateInstance<CREmailActivityMaint>();
					graphCR.Message.Current = graphCR.Message.Search<CRSMEmail.noteID>(message.RefNoteID);
					graphCR.Delete.Press();
				}
				else
				{
					CRSMEmailMaint graphSM = CreateInstance<CRSMEmailMaint>();
					graphSM.Email.Current = graphSM.Email.Search<SMEmail.noteID>(message.NoteID);
					graphSM.Delete.Press();
				}
			}
		}
		
		protected virtual IEnumerable<SMEmail> SelectedList()
		{
			List<SMEmail> selected = Emails.Cache.Updated.Cast<SMEmail>().Where(a => a.Selected == true).ToList();

			return selected;
		}


		[PXString(IsUnicode = true)]
		[PXUIField(DisplayName = "Related Entity Description", Enabled = false)]
		[PXFormula(typeof (EntityDescription<CRActivity.refNoteID>))]
		protected virtual void SMEmail_Source_CacheAttached(PXCache sender) { }

	}

}
