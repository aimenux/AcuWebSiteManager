using PX.Data;
using PX.Objects.CS;
using PX.SM;

namespace PX.Objects.CR
{
	public class AssignContactMassProcess : CRBaseAssignProcess<AssignContactMassProcess, Contact, CRSetup.contactdefaultAssignmentMapID>
	{
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
			Where<Contact.isActive, Equal<True>,
					And<Contact.contactType, Equal<ContactTypesAttribute.person>,
					And<Where<BAccount.bAccountID, IsNull, Or<Match<BAccount, Current<AccessInfo.userName>>>>> >>,
			OrderBy<Asc<Contact.displayName>>>
			Items;
	}
}
