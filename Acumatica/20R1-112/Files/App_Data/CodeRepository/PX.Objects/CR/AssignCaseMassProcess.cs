using PX.Data;
using PX.Data.BQL;
using PX.Objects.CT;
using PX.SM;

namespace PX.Objects.CR
{
	public class AssignCaseMassProcess : CRBaseAssignProcess<AssignCaseMassProcess, CRCase, CRSetup.defaultCaseAssignmentMapID>
	{
		#region BAccountContract
		/// <exclude/>
		[PXCacheName(Messages.Customer)]
		public sealed class BAccountContract : BAccount
		{
			public new abstract class bAccountID : PX.Data.BQL.BqlInt.Field<bAccountID> { }

			public new abstract class acctCD : PX.Data.BQL.BqlString.Field<acctCD> { }

			public new abstract class acctName : PX.Data.BQL.BqlString.Field<acctName> { }

			public new abstract class acctReferenceNbr : PX.Data.BQL.BqlString.Field<acctReferenceNbr> { }

			public new abstract class parentBAccountID : PX.Data.BQL.BqlInt.Field<parentBAccountID> { }

			public new abstract class ownerID : PX.Data.BQL.BqlGuid.Field<ownerID> { }

			public new abstract class type : PX.Data.BQL.BqlString.Field<type> { }

			public new abstract class defContactID : PX.Data.BQL.BqlInt.Field<defContactID> { }

			public new abstract class defLocationID : PX.Data.BQL.BqlInt.Field<defLocationID> { }
		}

		#endregion

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
			LeftJoin<BAccountContract, On<BAccountContract.bAccountID, Equal<Contract.customerID>>,
			LeftJoin<Location, On<Location.bAccountID, Equal<CRCase.customerID>, And<Location.locationID, Equal<CRCase.locationID>>>,
			LeftJoin<CRActivityStatistics, On<CRCase.noteID, Equal<CRActivityStatistics.noteID>>>>>>>>>,
			Where<CRCase.isActive.IsEqual<True>
				.And<Brackets<BAccount.bAccountID.IsNull.Or<Match<BAccount, Current<AccessInfo.userName>>>>>>>
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
