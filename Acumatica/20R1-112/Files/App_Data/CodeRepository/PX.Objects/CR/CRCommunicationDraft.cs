using PX.Data;

namespace PX.Objects.CR
{
	public class CRCommunicationDraft : CRCommunicationBase<
		Where<CRSMEmail.mpstatus, Equal<MailStatusListAttribute.draft>,
			And<Where<CRSMEmail.isArchived,Equal<False>,
				And<Where<CRSMEmail.isIncome, IsNull, 
					Or<CRSMEmail.isIncome, Equal<False>>>>>>>>
	{
		[PXDBString(100, InputMask = "")]
		[PXUIField(DisplayName = "From", Visibility = PXUIVisibility.SelectorVisible)]
		protected virtual void EMailAccount_Description_CacheAttached(PXCache sender) { }
	}
}
