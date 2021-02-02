using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Web.Compilation;
using PX.Data;
using PX.Data.EP;
using PX.Objects.CS;
using PX.Objects.EP;
using PX.Objects.SO;
using PX.SM;

namespace PX.Objects.CR
{
	[DashboardType((int)DashboardTypeAttribute.Type.Default)]
    [Serializable]
	public class CRCommunicationInbox : CRCommunicationBase<Where<CRSMEmail.isIncome, Equal<True>, 
                                    And<CRSMEmail.mpstatus, NotEqual<MailStatusListAttribute.deleted>,
                                    And<CRSMEmail.isArchived, NotEqual<True>>>>>
	{
		public PXFilter<RelatedEntity> entityFilter;
		public PXSelect<EPView> viewStatus;

		public CRCommunicationInbox() :
			base(new ButtonMenu("markAsRead", Messages.MarkAsRead, String.Empty),
				new ButtonMenu("markAsUnread", Messages.MarkAsUnread, String.Empty),
				new ButtonMenu("relate", Messages.LinkTo, String.Empty),
				new ButtonMenu("archive", Messages.Archive, String.Empty))
		{
			var actionSource =
		        PXAccess.FeatureInstalled<FeaturesSet.customerModule>() 
                ? (PXStringListAttribute)new CREmailActivityMaint.EntityList() 
                : new CREmailActivityMaint.EntityListSimple();

			Create.SetMenu(actionSource.ValueLabelDic.Select(entity => new ButtonMenu(entity.Key, PXMessages.LocalizeFormatNoPrefix(entity.Value), null) { OnClosingPopup = PXSpecialButtonType.Cancel }).ToArray());
		}

		[PXDBString(100, InputMask = "")]
		[PXUIField(DisplayName = "To", Visibility = PXUIVisibility.SelectorVisible)]
		protected virtual void EMailAccount_Description_CacheAttached(PXCache sender ){ }

		protected virtual void CRSMEmail_RefNoteID_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
		{
			CRSMEmail email = (CRSMEmail)e.Row;
			if (email != null && email.IsIncome == true)
			{
				email.Exception = null;
			}
		}
        
        public PXAction<CRSMEmail> Relate;
		[PXUIField(DisplayName = "Link To")]
		[PXButton]
		public virtual IEnumerable relate(PXAdapter adapter)
		{
            bool lint  = entityFilter.AskExt() == WebDialogResult.OK && entityFilter.VerifyRequired();
			if (lint)
			{
				PXCache cache = this.Caches<CRSMEmail>();
				RelatedEntity relatedEntity = entityFilter.Current;
				EntityHelper helper = new EntityHelper(this);
				Type entityType = PXBuildManager.GetType(relatedEntity.Type, false);
				object row = helper.GetEntityRow(entityType, relatedEntity.RefNoteID);
				Type graphType = helper.GetPrimaryGraphType(row, false);
				Type actualEntityType = PXSubstManager.Substitute(entityType, graphType);
				object actualRow = helper.GetEntityRow(actualEntityType, relatedEntity.RefNoteID);
				PXGraph graph = PXGraph.CreateInstance(graphType);
				graph.Caches[actualEntityType].Current = actualRow;
				foreach (PXResult<CRSMEmail, EMailAccount, EPView> email in SelectedList())
				{
					((CRSMEmail) email).Selected = false;
					CRSMEmail copy = PXCache<CRSMEmail>.CreateCopy(email);
                    CRActivity newActivity = (CRActivity)graph.Caches[typeof(CRActivity)].Insert();

                    copy.BAccountID = newActivity.BAccountID;
                    copy.ContactID = newActivity.ContactID;
                    copy.RefNoteID = newActivity.RefNoteID;
					copy.MPStatus = MailStatusListAttribute.Processed;
					copy.Exception = null;
				    PXRefNoteSelectorAttribute.EnsureNotePersistence(this, entityFilter.Current.Type, entityFilter.Current.RefNoteID);
                    copy = (CRSMEmail)cache.Update(copy);                    
                }
				Save.Press();
			}
			else
			{
				entityFilter.Ask(Messages.Warning, Messages.SelectRecord, MessageButtons.OK);
			}

			Emails.Cache.IsDirty = false;
			Emails.Cache.Clear();
			Emails.Cache.ClearQueryCacheObsolete();
			Emails.View.RequestRefresh();
			return adapter.Get();
		}

		public PXAction<CRSMEmail> Create;
		[PXUIField(DisplayName = "Create")]
        [PXButton(MenuAutoOpen = true)]
		public virtual IEnumerable create(PXAdapter adapter)
		{
            CREmailActivityMaint graph = CreateInstance<CREmailActivityMaint>();
			PXAdapter a = new PXAdapter(new PXView.Dummy(graph, new Select<CRSMEmail>(), SelectedList().Cast<object>().ToList()))
			{
				MassProcess = true,
				Menu = adapter.Menu
			};

		    foreach (object item in graph.Create.Press(a))
		    {
		    }
			return adapter.Get();
		}

		protected virtual IEnumerable MarkAs(PXAdapter adapter, int status, IEnumerable<PXResult<CRSMEmail, EMailAccount, EPView>> messages)
		{            
            foreach (PXResult<CRSMEmail, EMailAccount, EPView> r in messages)
			{
				CRSMEmail email = r;
				EPView vstatus = r;

				email.Selected = false;
				object owner = Emails.Cache.GetValueExt<CRSMEmail.ownerID>(email);
				if (owner is PXFieldState) owner = ((PXFieldState) owner).Value;

				if ((owner == null || email.OwnerID != Accessinfo.UserID) && vstatus.Status != status)
				{
					Emails.Cache.RaiseExceptionHandling<CRSMEmail.selected>(email, null, new PXSetPropertyException(Messages.OnlyOwnerSetReadStatus, PXErrorLevel.RowWarning, new EPViewStatusAttribute().ValueLabelDic[status]));
				}
				else if (status == EPViewStatusAttribute.VIEWED
						        ? vstatus.Status != EPViewStatusAttribute.VIEWED
						        : vstatus.Status == EPViewStatusAttribute.VIEWED)
				{
					vstatus = PXCache<EPView>.CreateCopy(vstatus);
					vstatus.UserID = email.OwnerID;
					vstatus.NoteID = email.NoteID;
					vstatus.Status = status;
					viewStatus.Update(vstatus);
				}
			}

		    viewStatus.Cache.Persist(PXDBOperation.Insert);
		    viewStatus.Cache.Persist(PXDBOperation.Update);

            Emails.Cache.IsDirty = false;
			return adapter.Get();
		}

		public PXAction<CRSMEmail> MarkAsRead;
		[PXUIField(DisplayName = "Mark as Read")]
		[PXButton]
		public virtual IEnumerable markAsRead(PXAdapter adapter)
		{
			PXLongOperation.StartOperation(this, () => MarkAs(adapter, EPViewStatusAttribute.VIEWED, SelectedList()));
			return adapter.Get();
		}

		public PXAction<CRSMEmail> MarkAsUnread;
		[PXUIField(DisplayName = "Mark as Unread")]
		[PXButton]
		public virtual IEnumerable markAsUnread(PXAdapter adapter)
		{
			PXLongOperation.StartOperation(this, () => MarkAs(adapter, EPViewStatusAttribute.NOTVIEWED, SelectedList()));
			return adapter.Get();
		}

        public PXAction<CRSMEmail> Archive;
        [PXUIField(DisplayName = "Archive")]
        [PXButton]
        public virtual IEnumerable archive(PXAdapter adapter)
        {
            PXLongOperation.StartOperation(this, () => ArchiveMessages(SelectedList().RowCast<CRSMEmail>()));
            return adapter.Get();
        }

        private static void ArchiveMessages(IEnumerable<CRSMEmail> messages)
        {
            CREmailActivityMaint graph = CreateInstance<CREmailActivityMaint>();
            foreach (CRSMEmail message in messages)
            {
                graph.Message.Current = graph.Message.Search<CRSMEmail.noteID>(message.NoteID);
                graph.Archive.Press();
            }
        }
	}
}
