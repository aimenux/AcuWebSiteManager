using PX.Data;
using PX.Data.BQL;
using PX.Objects.CR.MassProcess;
using PX.Objects.CS;
using PX.SM;

namespace PX.Objects.CR
{
	public class UpdateLeadMassProcess : CRBaseWorkflowUpdateProcess<UpdateLeadMassProcess, CRLead, PXMassUpdatableFieldAttribute, CRLead.classID>
	{

		[PXMassUpdatableField]
		[PXDefault(typeof(Search<CRLeadClass.defaultSource, Where<CRLeadClass.classID, Equal<Current<CRLead.classID>>>>),
			PersistingCheck = PXPersistingCheck.Nothing)]
		[PXMergeAttributes(Method = MergeMethod.Merge)]
		protected virtual void _(Events.CacheAttached<CRLead.source> e) { }

		[PXMassUpdatableField]
		[PXMergeAttributes(Method = MergeMethod.Merge)]
		protected virtual void _(Events.CacheAttached<CRLead.campaignID> e) { }

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
		public PXFilteredProcessingJoin<CRLead, CRWorkflowMassActionFilter,
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
				CRLead.contactType.IsEqual<ContactTypesAttribute.lead>
				.And<Brackets<BAccount.bAccountID.IsNull.Or<Match<BAccount, AccessInfo.userName.FromCurrent>>>>
				.And<Brackets<
						CRWorkflowMassActionFilter.operation.FromCurrent.IsEqual<CRWorkflowMassActionOperation.updateSettings>
							.And<CRLead.isActive.IsEqual<True>>>
					.Or<WhereWorkflowActionEnabled<CRLead, CRWorkflowMassActionFilter.action>>>>,
			OrderBy<
				Asc<CRLead.displayName>>>
			Items;

		protected override PXFilteredProcessing<CRLead, CRWorkflowMassActionFilter> ProcessingView => Items;
	}
}
