using PX.Data;
using PX.Objects.CR.MassProcess;
using PX.Objects.CS;
using PX.SM;

namespace PX.Objects.CR
{
	public class UpdateContactMassProcess : CRBaseUpdateProcess<UpdateContactMassProcess, Contact, PXMassUpdatableFieldAttribute, Contact.classID>
	{
		[PXUIField(DisplayName = "Company Name")]
		[CRLeadFullName(typeof(Contact.bAccountID))]
		protected virtual void Contact_FullName_CacheAttached(PXCache sender) { }

		[PXViewName(Messages.MatchingRecords)]
		[PXFilterable]
		[PXViewDetailsButton(typeof(Contact))]
		[PXViewDetailsButton(typeof(Contact),
			typeof(Select<BAccountCRM,
				Where<BAccountCRM.bAccountID, Equal<Current<Contact.bAccountID>>>>),
			ActionName = "Items_BAccount_ViewDetails")]
		[PXViewDetailsButton(typeof(Contact),
			typeof(Select<BAccountCRM,
				Where<BAccountCRM.bAccountID, Equal<Current<Contact.parentBAccountID>>>>),
				ActionName = "Items_BAccountParent_ViewDetails")]
		public PXProcessingJoin<Contact,
			LeftJoin<Address, On<Address.addressID, Equal<Contact.defAddressID>>,
			LeftJoin<BAccount, On<BAccount.bAccountID, Equal<Contact.bAccountID>>,
			LeftJoin<BAccountParent, On<BAccountParent.bAccountID, Equal<Contact.parentBAccountID>>,
			LeftJoin<State,
				On<State.countryID, Equal<Address.countryID>,
					And<State.stateID, Equal<Address.state>>>,
			LeftJoin<CRActivityStatistics, On<Contact.noteID, Equal<CRActivityStatistics.noteID>>>>>>>,
			Where<Contact.contactType, Equal<ContactTypesAttribute.person>,
				And<Contact.isActive, Equal<True>>>,
			OrderBy<Asc<Contact.displayName>>>
			Items;
	}
}
