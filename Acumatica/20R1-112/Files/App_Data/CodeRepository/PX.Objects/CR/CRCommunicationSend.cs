using System;
using System.Collections;
using System.Collections.Generic;
using PX.Data;
using PX.SM;

namespace PX.Objects.CR
{
	public class CRCommunicationSend : CRCommunicationBase<
        Where<CRSMEmail.mpstatus, Equal<MailStatusListAttribute.processed>, 
			And<Where<CRSMEmail.isArchived, Equal<False>,
				And<CRSMEmail.isIncome, Equal<False>>>>>>
	{
        public PXSelect<PreferencesEmail> Preferences;

	    public CRCommunicationSend() :
	        base(new ButtonMenu("archive", Messages.Archive, String.Empty))
	    {
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

        [PXDBString(100, InputMask = "")]
        [PXUIField(DisplayName = "From", Visibility = PXUIVisibility.SelectorVisible)]
        protected virtual void EMailAccount_Description_CacheAttached(PXCache sender) { }
	}
}
