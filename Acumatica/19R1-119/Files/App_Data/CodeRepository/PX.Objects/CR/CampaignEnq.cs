using System;
using System.Collections;
using PX.Data;
using PX.Objects.CS;
using PX.SM;


namespace PX.Objects.CR
{
	[DashboardType(PX.TM.OwnedFilter.DASHBOARD_TYPE, GL.TableAndChartDashboardTypeAttribute._AMCHARTS_DASHBOART_TYPE)]
	public class CampaignEnq : PXGraph<CampaignEnq>
	{
		#region CampaignFilter

		[Serializable]
		public partial class CampaignFilter : OwnedFilter
		{
			#region CampaignID
			public abstract class campaignID : PX.Data.BQL.BqlString.Field<campaignID> { }
			protected String _CampaignID;
			[PXString]
			[PXUIField(DisplayName = "Campaign")]
			[PXSelector(typeof(CRCampaign.campaignID), DescriptionField = typeof(CRCampaign.campaignName))]
			public virtual String CampaignID
			{
				get
				{
					return _CampaignID;
				}
				set
				{
					_CampaignID = value;
				}
			}
			#endregion			
		}
		#endregion

		#region Selects

		[PXHidden]
		public PXSelect<BAccount>
			BaseAccounts;

		[PXHidden]
		public PXSelect<Contact>
			BaseContacts;

		[PXHidden]
		public PXSelect<CRCampaign>
			BaseCRCampaign;

		[PXViewName(Messages.Selection)]
		public PXFilter<CampaignFilter>
			Filter;

		[PXViewName(Messages.CampaignMembers)]
		[PXFilterable]
        [PXViewDetailsButton(typeof(CampaignFilter), OnClosingPopup = PXSpecialButtonType.Refresh)]
		public PXOwnerFilteredSelectReadonly<CampaignFilter,
			Select2<Contact,
			InnerJoin<CRCampaignMembers, On<CRCampaignMembers.contactID, Equal<Contact.contactID>>,
			LeftJoin<CRCampaign, On<CRCampaign.campaignID, Equal<CRCampaignMembers.campaignID>>,
			LeftJoin<BAccount, On<BAccount.bAccountID, Equal<Contact.bAccountID>>,
			LeftJoin<Address, On<Address.addressID, Equal<Contact.defAddressID>>,
			LeftJoin<State,
					On<State.countryID, Equal<Address.countryID>,
						And<State.stateID, Equal<Address.state>>>>>>>>,			
			Where<Current<CampaignFilter.campaignID>, IsNull,
					Or<Current<CampaignFilter.campaignID>, Equal<CRCampaign.campaignID>>>>,				
			Contact.workgroupID, Contact.ownerID>
			FilteredItems;

		#endregion

		#region Ctors

		public CampaignEnq()
		{
			FilteredItems.NewRecordTarget = typeof(ContactMaint);
			Actions.Move("FilteredItems_AddNew", "AddNewLead");
			Actions["FilteredItems_AddNew"].SetCaption("New Contact");

			PXUIFieldAttribute.SetRequired<Contact.displayName>(FilteredItems.Cache, false);

			var bAccountCache = Caches[typeof(BAccount)];
			bAccountCache.DisplayName = Messages.Customer;
			PXUIFieldAttribute.SetDisplayName<BAccount.acctCD>(bAccountCache, Messages.BAccountCD);
			PXUIFieldAttribute.SetDisplayName<BAccount.acctName>(bAccountCache, Messages.BAccountName);


			var stateCache = Caches[typeof(State)];
			PXUIFieldAttribute.SetDisplayName<State.name>(stateCache, Messages.State);

			var campaignCache = Caches[typeof(CRCampaign)];
			PXUIFieldAttribute.SetRequired<CRCampaign.campaignID>(campaignCache, false);
			PXUIFieldAttribute.SetRequired<CRCampaign.campaignName>(campaignCache, false);

		}

		#endregion

		#region Actions

		public PXCancel<CampaignFilter> Cancel;

		public PXAction<CampaignFilter> addNewLead;
		[PXUIField(DisplayName = Messages.AddNewLead)]
        [PXButton(Tooltip = Messages.AddNewRecordToolTip, CommitChanges = true, OnClosingPopup = PXSpecialButtonType.Cancel)]
		public virtual void AddNewLead()
		{
			var target = PXGraph.CreateInstance<LeadMaint>();
			var targetCache = target.Lead.Cache;
			var row = targetCache.Insert();
			var newRow = (Contact)targetCache.CreateCopy(row);
			newRow.WorkgroupID = Filter.Current.WorkGroupID;
			newRow.OwnerID = Filter.Current.OwnerID;
			targetCache.Update(newRow);
			PXRedirectHelper.TryRedirect(target, PXRedirectHelper.WindowMode.NewWindow);
		}

		#endregion

		#region Event Handlers

		[PXMergeAttributes(Method = MergeMethod.Merge)]
		[PXUIField(DisplayName = Messages.CampaignStatus)]
		public virtual void CRCampaign_Status_CacheAttached(PXCache sender) { }


		[PXUIField(DisplayName = "Company Name")]
		[CRLeadFullName(typeof(Contact.bAccountID))]
		public virtual void Contact_FullName_CacheAttached(PXCache sender)
		{

		}

		[PXDBString(2, IsFixed = true)]
		[PXDefault(ContactTypesAttribute.Person, PersistingCheck = PXPersistingCheck.Nothing)]
		[ContactTypes]
		[PXUIField(DisplayName = "Type", Enabled = false)]
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
