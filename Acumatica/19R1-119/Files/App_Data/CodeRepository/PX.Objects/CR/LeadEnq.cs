using System;
using PX.Data;
using PX.Objects.CS;
using PX.SM;

namespace PX.Objects.CR
{
	[DashboardType(PX.TM.OwnedFilter.DASHBOARD_TYPE, GL.TableAndChartDashboardTypeAttribute._AMCHARTS_DASHBOART_TYPE)]
	public class LeadEnq : PXGraph<LeadEnq>
	{
		#region Selects

		[PXHidden]
		public PXSelect<BAccount>
			BaseAccounts;

		[PXHidden]
		public PXSelect<Contact>
			BaseContacts;

		[PXViewName(Messages.Selection)]
		public PXFilter<OwnedFilter> 
			Filter;

		[PXViewName(Messages.Leads)]
		[PXFilterable]
		[PXViewDetailsButton(typeof(OwnedFilter), OnClosingPopup = PXSpecialButtonType.Cancel)]
		[PXViewDetailsButton(typeof(OwnedFilter),
				typeof(Select<BAccountCRM,
					Where<BAccountCRM.bAccountID, Equal<Current<Contact.bAccountID>>>>),
                ActionName = "FilteredItems_BAccount_ViewDetails", OnClosingPopup = PXSpecialButtonType.Cancel)]
		[PXViewDetailsButton(typeof(OwnedFilter),
			typeof(Select<BAccountCRM,
				Where<BAccountCRM.bAccountID, Equal<Current<Contact.parentBAccountID>>>>),
                ActionName = "FilteredItems_BAccountParent_ViewDetails", OnClosingPopup = PXSpecialButtonType.Cancel)]
		public PXOwnerFilteredSelectReadonly<OwnedFilter, 
			Select2<Contact,
				LeftJoin<BAccount, On<BAccount.bAccountID, Equal<Contact.bAccountID>>,
				LeftJoin<BAccountParent, On<BAccountParent.bAccountID, Equal<Contact.parentBAccountID>>,
				LeftJoin<Address, On<Address.addressID, Equal<Contact.defAddressID>>, 
				LeftJoin<State, 
					On<State.countryID, Equal<Address.countryID>, 
						And<State.stateID, Equal<Address.state>>>,
				LeftJoin<CRActivityStatistics, On<Contact.noteID, Equal<CRActivityStatistics.noteID>>>>>>>,
				Where<Contact.contactType, Equal<ContactTypesAttribute.lead>>>,
			Contact.workgroupID, Contact.ownerID> 
			FilteredItems;

		#endregion

		#region Ctors

		public LeadEnq()
		{
			PXDBAttributeAttribute.Activate(FilteredItems.Cache);
			PXDBAttributeAttribute.Activate(BaseAccounts.Cache);
			PXDBAttributeAttribute.Activate(this.Caches[typeof(BAccountParent)]);
			
			FilteredItems.NewRecordTarget = typeof(LeadMaint);


			var bAccountCache = Caches[typeof(BAccount)];
			bAccountCache.DisplayName = Messages.Customer;
			PXUIFieldAttribute.SetDisplayName<BAccount.acctCD>(bAccountCache, Messages.BAccountCD);
			PXUIFieldAttribute.SetDisplayName<BAccount.acctName>(bAccountCache, Messages.BAccountName);

			var parentBAccountCache = Caches[typeof(BAccountParent)];
			parentBAccountCache.DisplayName = Messages.ParentAccount;
			PXUIFieldAttribute.SetDisplayName<BAccountParent.acctCD>(parentBAccountCache, Messages.ParentAccountID);
			PXUIFieldAttribute.SetDisplayName<BAccountParent.acctName>(parentBAccountCache, Messages.ParentAccountName);

			var stateCache = Caches[typeof(State)];
			PXUIFieldAttribute.SetDisplayName<State.name>(stateCache, Messages.State);
		}

		#endregion

		#region Actions

		public PXCancel<OwnedFilter> Cancel;

		#endregion

		#region Event Handlers

		[PXUIField(DisplayName = "Company Name")]
		[CRLeadFullName(typeof(Contact.bAccountID))]
		public virtual void Contact_FullName_CacheAttached(PXCache sender)
		{

		}

		[PXDBString(2, IsFixed = true)]
		[PXDefault(ContactTypesAttribute.Lead)]
		protected virtual void Contact_ContactType_CacheAttached(PXCache sender)
		{
			
		}

		#endregion

		#region Overrides

		public override bool IsDirty
		{
			get
			{
				return false;
			}
		}

		#endregion
	}
}
