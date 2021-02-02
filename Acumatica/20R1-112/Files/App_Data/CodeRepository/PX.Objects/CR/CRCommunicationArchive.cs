using System.Collections;
using System.Collections.Generic;
using PX.Data;

namespace PX.Objects.CR
{
    public class CRCommunicationArchive : CRCommunicationBase<Where<CRSMEmail.isArchived, Equal<True>>>
    {
		[PXDBString(100, InputMask = "")]
		[PXUIField(DisplayName = "From", Visibility = PXUIVisibility.SelectorVisible)]
		protected virtual void EMailAccount_Description_CacheAttached(PXCache sender) { }

        public PXAction<CRSMEmail> RestoreFromArchive;
        [PXUIField(DisplayName = "Restore from Archive")]
        [PXButton]
        public virtual IEnumerable restoreFromArchive(PXAdapter adapter)
        {
            PXLongOperation.StartOperation(this, () => RestoreFromArchiveMessages(SelectedList().RowCast<CRSMEmail>()));
            return adapter.Get();
        }

        private static void RestoreFromArchiveMessages(IEnumerable<CRSMEmail> messages)
        {
            CREmailActivityMaint graph = CreateInstance<CREmailActivityMaint>();
            foreach (CRSMEmail message in messages)
            {
                graph.Message.Current = graph.Message.Search<CRSMEmail.noteID>(message.NoteID);
                graph.RestoreArchive.Press();
            }
        }
    }
}
