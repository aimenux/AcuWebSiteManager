using System.Collections;
using System.Collections.Generic;
using PX.Data;
using PX.Objects.CT;
using PX.SM;

namespace PX.Objects.CR
{
	[DashboardType(PX.TM.OwnedFilter.DASHBOARD_TYPE, GL.TableAndChartDashboardTypeAttribute._AMCHARTS_DASHBOART_TYPE)]
	public class CaseEnq : PXGraph<CaseEnq>
	{
		#region BAccountContract
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

		#region Selects
		[PXHidden] 
		public PXSelect<BAccount> BAccount;

		[PXHidden] 
		public PXSelect<Contact> Contact;

		[PXHidden]
		public PXSelect<CRCase> CRCase;

		[PXViewName(Messages.Selection)]
		public PXFilter<OwnedFilter> 
			Filter;

		[PXViewName(Messages.Cases)]
		[PXFilterable]
        [PXViewDetailsButton(typeof(OwnedFilter), OnClosingPopup = PXSpecialButtonType.Refresh)]
		[PXViewDetailsButton(typeof(OwnedFilter),
			typeof(Select<BAccountCRM,
				Where<BAccountCRM.bAccountID, Equal<Current<CRCase.customerID>>>>),
            ActionName = "FilteredItems_BAccount_ViewDetails", OnClosingPopup = PXSpecialButtonType.Refresh)]
		[PXViewDetailsButton(typeof(OwnedFilter),
			typeof(Select2<BAccountCRM,
				InnerJoin<BAccountParent, On<BAccountParent.parentBAccountID, Equal<BAccountCRM.bAccountID>>>,
				Where<BAccountParent.bAccountID, Equal<Current<CRCase.customerID>>>>),
                ActionName = "FilteredItems_BAccountParent_ViewDetails", OnClosingPopup = PXSpecialButtonType.Refresh)]
		[PXViewDetailsButton(typeof(OwnedFilter),
			typeof(Select<Contact,
                Where<Contact.contactID, Equal<Current<CRCase.contactID>>>>), OnClosingPopup = PXSpecialButtonType.Refresh)]
		[PXViewDetailsButton(typeof(OwnedFilter),
			typeof(Select<Contract,
                Where<Contract.contractID, Equal<Current<CRCase.contractID>>>>), OnClosingPopup = PXSpecialButtonType.Refresh)]
		[PXViewDetailsButton(typeof(OwnedFilter),
			typeof(Select<Location,
                Where<Location.bAccountID, Equal<Current<CRCase.customerID>>, And<Location.locationID, Equal<Current<CRCase.locationID>>>>>), OnClosingPopup = PXSpecialButtonType.Refresh)]
		[PXViewDetailsButton(typeof(OwnedFilter),
			typeof(Select2<BAccount,
					InnerJoin<Contract, On<Contract.customerID, Equal<BAccount.bAccountID>>>, 								
					Where<Contract.contractID, Equal<Current<CRCase.contractID>>>>),
                ActionName = "FilteredItems_Contract_CustomerID_ViewDetails", OnClosingPopup = PXSpecialButtonType.Refresh)]
		public PXOwnerFilteredSelectReadonly<OwnedFilter,
		Select2<CRCase,
			LeftJoin<BAccount, On<BAccount.bAccountID, Equal<CRCase.customerID>>,
			LeftJoin<BAccountParent, On<BAccountParent.bAccountID, Equal<BAccount.parentBAccountID>>,
			LeftJoin<Contact, On<Contact.contactID, Equal<CRCase.contactID>>,
			LeftJoin<Contract, On<Contract.contractID, Equal<CRCase.contractID>>,	
			LeftJoin<BAccountContract, On<BAccountContract.bAccountID, Equal<Contract.customerID>>,
			LeftJoin<Location, On<Location.bAccountID, Equal<Current<CRCase.customerID>>, And<Location.locationID, Equal<CRCase.locationID>>>, 
			LeftJoin<CRActivityStatistics, On<CRCase.noteID, Equal<CRActivityStatistics.noteID>>>>>>>>>,
		Where<True, Equal<True>>,
		OrderBy<Desc<CRCase.caseCD>>>,
					CRCase.workgroupID, CRCase.ownerID>
			FilteredItems;

		#endregion

		#region Ctors

		public CaseEnq()
		{
			PXDBAttributeAttribute.Activate(FilteredItems.Cache);
			PXDBAttributeAttribute.Activate(this.Caches[typeof(BAccount)]);
			PXDBAttributeAttribute.Activate(this.Caches[typeof(BAccountParent)]);
			PXDBAttributeAttribute.Activate(this.Caches[typeof(Contact)]);
			PXDBAttributeAttribute.Activate(this.Caches[typeof(Contract)]);

			FilteredItems.NewRecordTarget = typeof(CRCaseMaint);

			var baccountCache = Caches[typeof(BAccount)];
			PXUIFieldAttribute.SetDisplayName<BAccount.acctCD>(baccountCache, Messages.BAccountCD);
			PXUIFieldAttribute.SetDisplayName<BAccount.acctName>(baccountCache, Messages.BAccountName);

			var parentBAccountCache = Caches[typeof(BAccountParent)];
			PXUIFieldAttribute.SetDisplayName<BAccountParent.acctCD>(parentBAccountCache, Messages.ParentAccount);
			PXUIFieldAttribute.SetDisplayName<BAccountParent.acctName>(parentBAccountCache, Messages.ParentAccountName);

			var contactCache = Caches[typeof(Contact)];
			PXUIFieldAttribute.SetDisplayName<Contact.displayName>(contactCache, Messages.Contact);
			
			var contractCache = Caches[typeof(Contract)];
			PXUIFieldAttribute.SetDisplayName<Contract.contractCD>(contractCache, Messages.Contract);
			PXUIFieldAttribute.SetDisplayName<Contract.description>(contractCache, Messages.ContractDescription);

            var caseCache = Caches[typeof(CRCase)];
            PXUIFieldAttribute.SetDisplayName<CRCase.locationID>(caseCache, Messages.LocationID);

			var contractBaccount = Caches[typeof(BAccountContract)];
			PXUIFieldAttribute.SetDisplayName<BAccount.acctCD>(contractBaccount,  Messages.Customer);
			PXUIFieldAttribute.SetDisplayName<BAccount.acctName>(contractBaccount, Messages.CustomerName);
		}

		#endregion

		#region Actions
		public PXCancel<OwnedFilter> Cancel;
		public PXSave<OwnedFilter> Save;

		public PXAction<OwnedFilter> takeCase;
		[PXUIField(DisplayName = "Take Case", MapEnableRights = PXCacheRights.Update, MapViewRights = PXCacheRights.Select)]
		[PXButton]
		public virtual void TakeCase()
		{
			CRCase c = ((CRCase) (FilteredItems.Cache.Current));
			c.OwnerID = EP.EmployeeMaint.GetCurrentEmployeeID(this);
			FilteredItems.Cache.Update(c);
			Save.Press();
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

		#region CacheAttached
		[PXMergeAttributes(Method = MergeMethod.Merge)]
		[PXDBDatetimeScalar(typeof(Search<CRActivityStatistics.lastActivityDate, Where<CRActivityStatistics.noteID, Equal<CRCase.noteID>>>), PreserveTime = true, UseTimeZone = true)]
		protected virtual void CRCase_LastActivity_CacheAttached(PXCache sender)
		{
		}
		#endregion
	}
}
