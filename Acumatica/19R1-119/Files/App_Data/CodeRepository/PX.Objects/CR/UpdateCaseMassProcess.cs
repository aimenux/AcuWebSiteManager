using PX.Data;
using PX.Objects.CR.MassProcess;
using PX.Objects.CS;
using PX.Objects.CT;
using PX.SM;

namespace PX.Objects.CR
{
	public class UpdateCaseMassProcess : CRBaseUpdateProcess<UpdateCaseMassProcess, CRCase, PXMassUpdatableFieldAttribute, CRCase.caseClassID>
	{
		[PXHidden]
		public PXSelect<Location> location;

		[PXViewName(Messages.MatchingRecords)]
		[PXFilterable]
		[PXViewDetailsButton(typeof(CRCase))]
		[PXViewDetailsButton(typeof(CRCase),
				typeof(Select<BAccountCRM,
					Where<BAccountCRM.bAccountID, Equal<Current<CRCase.customerID>>>>),
				ActionName = "Items_BAccount_ViewDetails")]
		[PXViewDetailsButton(typeof(CRCase),
			typeof(Select2<BAccountCRM,
				InnerJoin<BAccountParent, On<BAccountParent.parentBAccountID, Equal<BAccountCRM.bAccountID>>>,
				Where<BAccountParent.bAccountID, Equal<Current<CRCase.customerID>>>>),
				ActionName = "Items_BAccountParent_ViewDetails")]
		[PXViewDetailsButton(typeof(CRCase),
			typeof(Select<Contact,
				Where<Contact.contactID, Equal<Current<CRCase.contactID>>>>))]
		[PXViewDetailsButton(typeof(CRCase),
			typeof(Select<Contract,
				Where<Contract.contractID, Equal<Current<CRCase.contractID>>>>))]
		[PXViewDetailsButton(typeof(CRCase),
			typeof(Select<Location,
				Where<Location.bAccountID, Equal<Current<CRCase.customerID>>, And<Location.locationID, Equal<Current<CRCase.locationID>>>>>))]
		[PXViewDetailsButton(typeof(CRCase),
			typeof(Select2<BAccount,
					InnerJoin<Contract, On<Contract.customerID, Equal<BAccount.bAccountID>>>,
					Where<Contract.contractID, Equal<Current<CRCase.contractID>>>>),
				ActionName = "Items_Contract_CustomerID_ViewDetails")]
		public PXProcessingJoin<CRCase,
			LeftJoin<BAccount, On<BAccount.bAccountID, Equal<CRCase.customerID>>,
			LeftJoin<BAccountParent, On<BAccountParent.bAccountID, Equal<BAccount.parentBAccountID>>,
			LeftJoin<Contact, On<Contact.contactID, Equal<CRCase.contactID>>,
			LeftJoin<Contract, On<Contract.contractID, Equal<CRCase.contractID>>,
			LeftJoin<CaseEnq.BAccountContract, On<CaseEnq.BAccountContract.bAccountID, Equal<Contract.customerID>>,
			LeftJoin<Location, On<Location.bAccountID, Equal<CRCase.customerID>, And<Location.locationID, Equal<CRCase.locationID>>>,
			LeftJoin<CRActivityStatistics, On<CRCase.noteID, Equal<CRActivityStatistics.noteID>>>>>>>>>,
			Where<CRCase.majorStatus, NotEqual<CRCaseMajorStatusesAttribute.closed>,
				And<CRCase.majorStatus, NotEqual<CRCaseMajorStatusesAttribute.released>>>>
			Items;

		#region CacheAttached
		[PXMergeAttributes(Method = MergeMethod.Merge)]
		[PXDBScalar(typeof(Search<CRActivityStatistics.lastActivityDate, Where<CRActivityStatistics.noteID, Equal<CRCase.noteID>>>))]
		protected virtual void CRCase_LastActivity_CacheAttached(PXCache sender)
		{
		}
		#endregion
	}
}
