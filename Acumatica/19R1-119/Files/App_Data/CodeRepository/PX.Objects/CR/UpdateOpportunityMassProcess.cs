using PX.Data;
using PX.Objects.CR.MassProcess;
using PX.Objects.CS;
using PX.SM;

namespace PX.Objects.CR
{
	public class UpdateOpportunityMassProcess : CRBaseUpdateProcess<UpdateOpportunityMassProcess, CROpportunity, PXMassUpdatableFieldAttribute, CROpportunity.classID>
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
		public PXProcessingJoin<CROpportunity,
					LeftJoin<BAccount, On<BAccount.bAccountID, Equal<CROpportunity.bAccountID>>,
					LeftJoin<BAccountParent, On<BAccountParent.bAccountID, Equal<CROpportunity.parentBAccountID>>,
					LeftJoin<CROpportunityProbability, On<CROpportunityProbability.stageCode, Equal<CROpportunity.stageID>>,
					LeftJoin<CRActivityStatistics, On<CROpportunity.noteID, Equal<CRActivityStatistics.noteID>>>>>>,
					Where<CROpportunity.majorStatus, NotEqual<OpportunityMajorStatusesAttribute.closed>>>
			Items;

		#region CacheAttached
		[PXMergeAttributes(Method = MergeMethod.Merge)]
		[PXDBScalar(typeof(Search<CRActivityStatistics.lastActivityDate, Where<CRActivityStatistics.noteID, Equal<CROpportunity.noteID>>>))]
		protected virtual void CROpportunity_LastActivity_CacheAttached(PXCache sender)
		{
		}
		#endregion
	}
}
