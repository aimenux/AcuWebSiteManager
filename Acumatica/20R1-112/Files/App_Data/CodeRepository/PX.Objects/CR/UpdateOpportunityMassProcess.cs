using PX.Data;
using PX.Data.BQL;
using PX.Objects.CR.MassProcess;
using PX.Objects.CS;
using PX.SM;

namespace PX.Objects.CR
{
	public class UpdateOpportunityMassProcess : CRBaseWorkflowUpdateProcess<UpdateOpportunityMassProcess, CROpportunity, PXMassUpdatableFieldAttribute, CROpportunity.classID>
	{
		[PXViewName(Messages.MatchingRecords)]
		[PXFilterable]
		[PXViewDetailsButton(typeof(CROpportunity))]
		[PXViewDetailsButton(typeof(CROpportunity),
				typeof(Select<BAccountCRM,
					Where<BAccountCRM.bAccountID, Equal<Current<CROpportunity.bAccountID>>>>),
				ActionName = "Items_BAccount_ViewDetails")]
		[PXViewDetailsButton(typeof(CROpportunity),
			typeof(Select<BAccountCRM,
				Where<BAccountCRM.bAccountID, Equal<Current<CROpportunity.parentBAccountID>>>>),
				ActionName = "Items_BAccountParent_ViewDetails")]
		public PXFilteredProcessingJoin<CROpportunity, CRWorkflowMassActionFilter,
				LeftJoin<BAccount, On<BAccount.bAccountID, Equal<CROpportunity.bAccountID>>,
				LeftJoin<BAccountParent, On<BAccountParent.bAccountID, Equal<CROpportunity.parentBAccountID>>,
				LeftJoin<CROpportunityProbability, On<CROpportunityProbability.stageCode, Equal<CROpportunity.stageID>>,
				LeftJoin<CRActivityStatistics, On<CROpportunity.noteID, Equal<CRActivityStatistics.noteID>>>>>>,
				Where<
					Brackets<BAccount.bAccountID.IsNull.Or<Match<BAccount, AccessInfo.userName.FromCurrent>>>
					.And<Brackets<
						CRWorkflowMassActionFilter.operation.FromCurrent.IsEqual<CRWorkflowMassActionOperation.updateSettings>
							.And<CROpportunity.isActive.IsEqual<True>>>
						.Or<WhereWorkflowActionEnabled<CROpportunity, CRWorkflowMassActionFilter.action>>>>>
			Items;

		protected override PXFilteredProcessing<CROpportunity, CRWorkflowMassActionFilter> ProcessingView => Items;

		#region CacheAttached
		[PXMergeAttributes(Method = MergeMethod.Merge)]
		[PXDBScalar(typeof(Search<CRActivityStatistics.lastActivityDate, Where<CRActivityStatistics.noteID, Equal<CROpportunity.noteID>>>))]
		protected virtual void CROpportunity_LastActivity_CacheAttached(PXCache sender)
		{
		}
		#endregion
	}
}
