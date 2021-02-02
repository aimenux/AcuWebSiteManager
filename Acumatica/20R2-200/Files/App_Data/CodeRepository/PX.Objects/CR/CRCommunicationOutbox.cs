using System.Collections;
using System.Collections.Generic;
using System.Linq;
using PX.Data;

namespace PX.Objects.CR
{
	public class CRCommunicationOutbox : CRCommunicationBase<
		Where2<Where<CRSMEmail.mpstatus, Equal<MailStatusListAttribute.preProcess>,
				Or<CRSMEmail.mpstatus, Equal<MailStatusListAttribute.inProcess>,
					Or<CRSMEmail.mpstatus, Equal<MailStatusListAttribute.failed>>>>,
			And<CRSMEmail.isArchived,Equal<False>,
			And<Where<CRSMEmail.isIncome, IsNull,
				Or<CRSMEmail.isIncome, Equal<False>>>>>>>
	{
		[PXDBString(100, InputMask = "")]
		[PXUIField(DisplayName = "From", Visibility = PXUIVisibility.SelectorVisible)]
		protected virtual void EMailAccount_Description_CacheAttached(PXCache sender) { }

	    public PXAction<CRSMEmail> Resubmit;
	    [PXUIField(DisplayName = "Resubmit")]
	    [PXButton]
	    public virtual IEnumerable resubmit(PXAdapter adapter)
	    {
		    PXLongOperation.StartOperation(this, delegate() { ResubmitMessages(SelectedList().RowCast<CRSMEmail>().ToList()); });
	        return adapter.Get();
	    }
        
        private static void ResubmitMessages(IEnumerable<CRSMEmail> messages)
        {
            foreach (CRSMEmail message in messages)
            {
                CREmailActivityMaint graph = CreateInstance<CREmailActivityMaint>();
                graph.Message.Current = graph.Message.Search<CRSMEmail.noteID>(message.NoteID);
                graph.Message.Current.MPStatus = ActivityStatusListAttribute.Draft;
                graph.Send.Press();
            }
        }
	}
}
