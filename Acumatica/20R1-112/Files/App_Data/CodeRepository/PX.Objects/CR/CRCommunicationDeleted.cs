using System;
using System.Collections;
using System.Collections.Generic;
using PX.Data;

namespace PX.Objects.CR
{
	public class CRCommunicationDeleted : CRCommunicationBase<
		Where<CRSMEmail.mpstatus, Equal<MailStatusListAttribute.deleted>,
			And<Where<CRSMEmail.isArchived, Equal<False>>>>>
	{
	    public CRCommunicationDeleted():
			base(new ButtonMenu("restore", Messages.Restore, String.Empty))
	    {
	    }

		[PXDBString(100, InputMask = "")]
		[PXUIField(DisplayName = "Account", Visibility = PXUIVisibility.SelectorVisible)]
		protected virtual void EMailAccount_Description_CacheAttached(PXCache sender) { }

		public PXAction<CRSMEmail> Restore;
		[PXUIField(DisplayName = "Restore")]
		[PXButton]
		public virtual IEnumerable restore(PXAdapter adapter)
		{
			PXLongOperation.StartOperation(this, () => RestoreMessages(SelectedList().RowCast<CRSMEmail>()));
			return adapter.Get();
		}

		private static void RestoreMessages(IEnumerable<CRSMEmail> messages)
		{
			CREmailActivityMaint graph = CreateInstance<CREmailActivityMaint>();
			foreach (CRSMEmail message in messages)
			{
				graph.Message.Current = graph.Message.Search<CRSMEmail.noteID>(message.NoteID);
				graph.Restore.Press();
			}
		}
	}
}
