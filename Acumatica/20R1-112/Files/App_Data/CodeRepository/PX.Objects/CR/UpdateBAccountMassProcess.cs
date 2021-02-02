using PX.Data;
using PX.Objects.CR.MassProcess;
using PX.Objects.CS;
using PX.SM;

namespace PX.Objects.CR
{
	public class UpdateBAccountMassProcess : CRBaseUpdateProcess<UpdateBAccountMassProcess, BAccount, PXMassUpdatableFieldAttribute, BAccount.classID>
	{
		[PXViewName(Messages.MatchingRecords)]
		[PXViewDetailsButton(typeof(BAccount))]
		[PXViewDetailsButton(typeof(BAccount),
			typeof(Select<BAccountParent,
				Where<BAccountParent.bAccountID, Equal<Current<BAccount.parentBAccountID>>>>))]
		[PXViewDetailsButton(typeof(BAccount),
			typeof(Select<BAccountCRM,
				Where<BAccountCRM.bAccountID, Equal<Current<BAccount.bAccountID>>>>))]
		[PXFilterable]
		public PXProcessingJoin<BAccount,
			LeftJoin<Contact, On<Contact.bAccountID, Equal<BAccount.bAccountID>,
				And<Contact.contactID, Equal<BAccount.defContactID>>>,
			LeftJoin<Address, On<Address.bAccountID, Equal<BAccount.bAccountID>,
				And<Address.addressID, Equal<BAccount.defAddressID>>>,
			LeftJoin<BAccountParent, On<BAccountParent.bAccountID, Equal<BAccount.parentBAccountID>>,
			LeftJoin<Location, On<Location.bAccountID, Equal<BAccount.bAccountID>, And<Location.locationID, Equal<BAccount.defLocationID>>>,
			LeftJoin<State, On<State.countryID, Equal<Address.countryID>,
						And<State.stateID, Equal<Address.state>>>>>>>>,
			Where2<Where<BAccount.type, Equal<BAccountType.prospectType>,
					Or<BAccount.type, Equal<BAccountType.customerType>,
					Or<BAccount.type, Equal<BAccountType.combinedType>,
					Or<BAccount.type, Equal<BAccountType.vendorType>>>>>,
				And<Match<BAccount, Current<AccessInfo.userName>>> >,
			OrderBy<Asc<BAccount.acctName>>>
			Items;

		protected override PXGraph GetPrimaryGraph(BAccount item)
		{
			return new BusinessAccountMaint();
		}
	}
}
