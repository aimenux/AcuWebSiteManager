using PX.Data;
using PX.Objects.CS;
using PX.SM;

namespace PX.Objects.CR
{
	public class AssignLeadMassProcess : CRBaseAssignProcess<AssignLeadMassProcess, CRLead, CRSetup.leaddefaultAssignmentMapID>
	{
		[PXViewName(Messages.MatchingRecords)]
		[PXFilterable]
		[PXViewDetailsButton(typeof(CRLead))]
		[PXViewDetailsButton(typeof(CRLead),
			typeof(Select<BAccountCRM,
				Where<BAccountCRM.bAccountID, Equal<Current<CRLead.bAccountID>>>>),
			ActionName = "Items_BAccount_ViewDetails")]
		[PXViewDetailsButton(typeof(CRLead),
			typeof(Select<BAccountCRM,
				Where<BAccountCRM.bAccountID, Equal<Current<CRLead.parentBAccountID>>>>),
				ActionName = "Items_BAccountParent_ViewDetails")]
		public PXProcessingJoin<CRLead,
			LeftJoin<Address, 
				On<Address.addressID, Equal<CRLead.defAddressID>>,
			LeftJoin<BAccount, 
				On<BAccount.bAccountID, Equal<CRLead.bAccountID>>,
			LeftJoin<BAccountParent, 
				On<BAccountParent.bAccountID, Equal<CRLead.parentBAccountID>>,
			LeftJoin<State,
				On<State.countryID, Equal<Address.countryID>,
					And<State.stateID, Equal<Address.state>>>,
			LeftJoin<CRActivityStatistics, 
				On<CRLead.noteID, Equal<CRActivityStatistics.noteID>>>>>>>,
			Where<
				CRLead.isActive.IsEqual<True>
				.And<CRLead.contactType.IsEqual<ContactTypesAttribute.lead>>
				.And<BAccount.bAccountID.IsNull.Or<Match<BAccount, AccessInfo.userName.FromCurrent>>>>,
			OrderBy<
				Asc<CRLead.displayName>>>

			Items;
	}
}
