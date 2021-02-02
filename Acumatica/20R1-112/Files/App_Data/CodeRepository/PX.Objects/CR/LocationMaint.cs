using System;
using PX.Data;
using System.Collections;
using PX.Objects.AP;
using PX.Objects.AR;
using PX.Objects.Common;
using PX.Objects.GL;
using PX.Objects.CS;
using CRLocation = PX.Objects.CR.Standalone.Location;
using Branch = PX.Objects.GL.Branch;
using PX.SM;
using PX.Objects.GL.Helpers;
using PX.Objects.TX;

namespace PX.Objects.CR
{
	[PXSubstitute(GraphType = typeof(LocationMaint))]
    [Serializable]
    [PXHidden]
	public partial class SelectedLocation : Location
	{
		#region BAccountID
		public new abstract class bAccountID : PX.Data.BQL.BqlInt.Field<bAccountID> { }
		[PXDBInt(IsKey = true)]
		[PXDefault(typeof(SelectedLocation.bAccountID))]
		[PXUIField(DisplayName = "Business Account", TabOrder = 0)]
		[PXDimensionSelector("BIZACCT", typeof(Search2<BAccount.bAccountID,
			LeftJoin<Contact, On<Contact.bAccountID, Equal<BAccount.bAccountID>, And<Contact.contactID, Equal<BAccount.defContactID>>>,
			LeftJoin<Address, On<Address.bAccountID, Equal<BAccount.bAccountID>, And<Address.addressID, Equal<BAccount.defAddressID>>>>>,
			Where<BAccount.type, Equal<BAccountType.customerType>,
				Or<BAccount.type, Equal<BAccountType.prospectType>,
				Or<BAccount.type, Equal<BAccountType.vendorType>,
                Or<BAccount.type, Equal<BAccountType.combinedType>>>>>>), typeof(BAccount.acctCD),
			typeof(BAccount.acctCD),
			typeof(BAccount.acctName), typeof(BAccount.type), typeof(BAccount.classID), typeof(BAccount.status), typeof(Contact.phone1), typeof(Address.city), typeof(Address.countryID),
			DescriptionField = typeof(BAccount.acctName))]
		[PXParent(typeof(Select<BAccount,
			Where<BAccount.bAccountID,
			Equal<Current<Location.bAccountID>>>>)
			)]
		public override Int32? BAccountID
		{
			get
			{
				return base._BAccountID;
			}
			set
			{
				base._BAccountID = value;
			}
		}
		#endregion
		#region IsAddressSameAsMain
		public new abstract class isAddressSameAsMain : PX.Data.BQL.BqlBool.Field<isAddressSameAsMain> { }
		[PXBool()]
		[PXUIField(DisplayName = "Same As Main")]
		public override bool? IsAddressSameAsMain
		{
			get
			{
				return this._IsAddressSameAsMain;
			}
			set
			{
				this._IsAddressSameAsMain = value;
			}
		}
		#endregion
		#region IsContactSameAsMain
		public new abstract class isContactSameAsMain : PX.Data.BQL.BqlBool.Field<isContactSameAsMain> { }
		[PXBool()]
		[PXUIField(DisplayName = "Same As Main")]
		public override bool? IsContactSameAsMain
		{
			get
			{
				return this._IsContactSameAsMain;
			}
			set
			{
				this._IsContactSameAsMain = value;
			}
		}
		#endregion
	}

	[PXProjection(typeof(Select<CRLocation>), Persistent = false)]
    [PXCacheName(Messages.LocationARAccountSub)]
    [Serializable]
	public partial class LocationARAccountSub : IBqlTable
	{
		#region BAccountID
		public abstract class bAccountID : PX.Data.BQL.BqlInt.Field<bAccountID> { }
		protected int? _BAccountID;
		[PXDBInt(BqlField = typeof(CRLocation.bAccountID), IsKey = true)]
		public virtual int? BAccountID
		{
			get
			{
				return this._BAccountID;
			}
			set
			{
				this._BAccountID = value;
			}
		}
		#endregion
		#region LocationID
		public abstract class locationID : PX.Data.BQL.BqlInt.Field<locationID> { }
		protected int? _LocationID;
		[PXDBInt(BqlField = typeof(CRLocation.locationID), IsKey = true)]
		public virtual int? LocationID
		{
			get
			{
				return this._LocationID;
			}
			set
			{
				this._LocationID = value;
			}
		}
		#endregion
		#region CARAccountLocationID
		public abstract class cARAccountLocationID : PX.Data.BQL.BqlInt.Field<cARAccountLocationID> { }
		protected Int32? _CARAccountLocationID;
		[PXDBInt(BqlField = typeof(CRLocation.cARAccountLocationID))]
		public virtual Int32? CARAccountLocationID
		{
			get
			{
				return this._CARAccountLocationID;
			}
			set
			{
				this._CARAccountLocationID = value;
			}
		}
		#endregion
		#region CARAccountID
		public abstract class cARAccountID : PX.Data.BQL.BqlInt.Field<cARAccountID> { }
		protected Int32? _CARAccountID;
		[Account(BqlField = typeof(CRLocation.cARAccountID), DisplayName = "AR Account", DescriptionField = typeof(Account.description), Required = true, ControlAccountForModule = ControlAccountModule.AR)]
		public virtual Int32? CARAccountID
		{
			get
			{
				return this._CARAccountID;
			}
			set
			{
				this._CARAccountID = value;
			}
		}
		#endregion
		#region CARSubID
		public abstract class cARSubID : PX.Data.BQL.BqlInt.Field<cARSubID> { }
		protected Int32? _CARSubID;
		[SubAccount(typeof(LocationARAccountSub.cARAccountID), BqlField = typeof(CRLocation.cARSubID), DisplayName = "AR Sub.", DescriptionField = typeof(Sub.description), Required = true)]
		public virtual Int32? CARSubID
		{
			get
			{
				return this._CARSubID;
			}
			set
			{
				this._CARSubID = value;
			}
		}
		#endregion
		#region CRetainageAcctID
		public abstract class cRetainageAcctID : PX.Data.BQL.BqlInt.Field<cRetainageAcctID> { }

		[Account(BqlField = typeof(CRLocation.cRetainageAcctID),
			DisplayName = "Retainage Receivable Account",
			DescriptionField = typeof(Account.description),
			Required = true,
			ControlAccountForModule = ControlAccountModule.AR)]
		public virtual int? CRetainageAcctID
		{
			get;
			set;
		}
		#endregion
		#region CRetainageSubID
		public abstract class cRetainageSubID : PX.Data.BQL.BqlInt.Field<cRetainageSubID> { }

		[SubAccount(typeof(LocationARAccountSub.cRetainageAcctID),
			BqlField = typeof(CRLocation.cRetainageSubID),
			DisplayName = "Retainage Receivable Sub.",
			DescriptionField = typeof(Sub.description),
			Required = true)]
		public virtual int? CRetainageSubID
		{
			get;
			set;
		}
		#endregion
	}

	public class LocationMaint : LocationMaintBase<Location, Location, Where<Location.bAccountID, Equal<Optional<Location.bAccountID>>>>
	{
		#region Buttons

		public PXSave<Location> Save;
		public PXAction<Location> cancel;
		public PXInsert<Location> Insert;
		public PXDelete<Location> Delete;
		public PXFirst<Location> First;
		public PXPrevious<Location> previous;
		public PXNext<Location> next;
		public PXLast<Location> Last;
		public PXAction<Location> viewOnMap;
		public PXAction<Location> validateAddresses;		
		#endregion

		#region ButtonDelegates

		[PXUIField(DisplayName = ActionsMessages.Cancel, MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select)]
		[PXCancelButton]
		protected virtual IEnumerable Cancel(PXAdapter adapter)
		{
			int? acctid = Location.Current != null ? Location.Current.BAccountID : null;
			foreach (object res in (new PXCancel<Location>(this, "Cancel")).Press(adapter))
			{
				var loc = res is PXResult ? (Location)(res as PXResult)[0] : (Location)res;

				if (!IsImport && Location.Cache.GetStatus(loc) == PXEntryStatus.Inserted
						&& (acctid != loc.BAccountID || string.IsNullOrEmpty(loc.LocationCD)))
				{
					foreach (object resFirst in First.Press(adapter))
					{
						Location first = resFirst is PXResult ? (Location)(resFirst as PXResult)[0] : (Location)resFirst;
						return new object[] { first };
					}
					loc.LocationCD = null;
					return new object[] { loc };
				}
				else
				{
					return new object[] { loc };
				}
			}
			return new object[0];
		}

		[PXUIField(DisplayName = ActionsMessages.Previous, MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select)]
		[PXPreviousButton]
		protected virtual IEnumerable Previous(PXAdapter adapter)
		{
			foreach (object res in (new PXPrevious<Location>(this, "Prev")).Press(adapter))
			{
				var loc = res is PXResult ? (Location)(res as PXResult)[0] : (Location)res;

				if (Location.Cache.GetStatus(loc) == PXEntryStatus.Inserted)
				{
					return Last.Press(adapter);
				}
				else
				{
					return new object[] { loc };
				}
			}
			return new object[0];
		}

		[PXUIField(DisplayName = ActionsMessages.Next, MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select)]
		[PXNextButton]
		protected virtual IEnumerable Next(PXAdapter adapter)
		{
			foreach (object res in (new PXNext<Location>(this, "Next")).Press(adapter))
			{
				var loc = res is PXResult ? (Location)(res as PXResult)[0] : (Location)res;

				if (Location.Cache.GetStatus(loc) == PXEntryStatus.Inserted)
				{
					return First.Press(adapter);
				}
				else
				{
					return new object[] { loc };
				}
			}
			return new object[0];
		}

		[PXUIField(DisplayName = Messages.ViewOnMap, MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select)]
		[PXButton()]
		public virtual IEnumerable ViewOnMap(PXAdapter adapter)
		{

			BAccountUtility.ViewOnMap(this.Address.Current);
			return adapter.Get();
		}

		[PXUIField(DisplayName = CS.Messages.ValidateAddresses, MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select, FieldClass = CS.Messages.ValidateAddress)]
		[PXButton]
		public virtual IEnumerable ValidateAddresses(PXAdapter adapter)
		{
			Location primary = this.LocationCurrent.Current;
			if (primary != null)
			{
				BAccount acct = BAccountUtility.FindAccount(this, primary.BAccountID);
				bool isSameAsMain = (acct != null && acct.DefAddressID == primary.DefAddressID);
				Address address = this.Address.Current;
				if (address != null && isSameAsMain == false && address.IsValidated == false)
				{
					PXAddressValidator.Validate<Address>(this, address, true, true);
				}
			}
			return adapter.Get();
		}

		#endregion

		[PXFilterable]
		[PXViewDetailsButton(typeof(Location))]
		[PXViewDetailsButton(typeof(Location),
			typeof(Select<Contact,
				Where<Contact.contactID, Equal<Current<CROpportunity.contactID>>>>))]
		public PXSelectJoin<CROpportunity, 
			LeftJoin<Contact, On<Contact.contactID, Equal<CROpportunity.contactID>>, 
			LeftJoin<CROpportunityProbability, On<CROpportunityProbability.stageCode, Equal<CROpportunity.stageID>>>>,
			Where<CROpportunity.bAccountID, Equal<Current<Location.bAccountID>>,
				And<CROpportunity.locationID, Equal<Current<Location.locationID>>>>> 
			Opportunities;

		[PXFilterable]
		[PXViewDetailsButton(typeof(Location))]
		[PXViewDetailsButton(typeof(Location),
			typeof(Select<Contact,
				Where<Contact.contactID, Equal<Current<CRCase.contactID>>>>))]
		public PXSelect<CRCase, 
			Where<CRCase.customerID, Equal<Current<Location.bAccountID>>,
				And<CRCase.locationID, Equal<Current<Location.locationID>>>>> 
			Cases;

		public PXSelect<LocationARAccountSub, Where<LocationARAccountSub.bAccountID, Equal<Current<Location.bAccountID>>, And<LocationARAccountSub.locationID, Equal<Current<Location.cARAccountLocationID>>>>> ARAccountSubLocation;

		[PXHidden]
		public PXSelect<CustSalesPeople> Salesperson;

		public LocationMaint()
		{
			Location.Join<LeftJoin<BAccount, On<BAccount.bAccountID, Equal<Location.bAccountID>>>>();

			Location.WhereAnd<Where<
				BAccount.bAccountID, Equal<Location.bAccountID>,
				And<BAccount.bAccountID, IsNotNull,
				And<Match<BAccount, Current<AccessInfo.userName>>>>>>();

			if (Company.Current.BAccountID.HasValue == false)
			{
                throw new PXSetupNotEnteredException(ErrorMessages.SetupNotEntered, typeof(Branch), PXMessages.LocalizeNoPrefix(CS.Messages.BranchMaint));
			}
			PXUIFieldAttribute.SetDisplayName<Location.locationCD>(Location.Cache, Messages.LocationID);
			PXUIFieldAttribute.SetDisplayName<Location.cLeadTime>(LocationCurrent.Cache, Messages.LeadTimeDays);

			var bAccountCache = Caches[typeof(BAccount)];
			PXUIFieldAttribute.SetDisplayName<BAccount.acctCD>(bAccountCache, Messages.BAccountCD);
			PXUIFieldAttribute.SetDisplayName<BAccount.acctName>(bAccountCache, Messages.BAccountName);
		}

		public override void Persist()
		{
			var arAccountSub = ARAccountSubLocation.Current;

			if (arAccountSub != null)
			{
				ValidationHelper.SetErrorEmptyIfNull<LocationARAccountSub.cARAccountID>(ARAccountSubLocation.Cache, arAccountSub, arAccountSub.CARAccountID);
				ValidationHelper.SetErrorEmptyIfNull<LocationARAccountSub.cARSubID>(ARAccountSubLocation.Cache, arAccountSub, arAccountSub.CARSubID);
			}

			base.Persist();
			this.ARAccountSubLocation.Cache.Clear();
		}

		public PXSetup<GL.Branch> Company;

		#region Location Events

		/*[CustomerProspectVendor(DisplayName = "Buisness Account", IsKey = true)]
		protected virtual void Location_BAccountID_CacheAttached(PXCache sender, PXRowSelectedEventArgs e)
		{
			
		}*/

		protected override void Location_RowSelected(PXCache sender, PXRowSelectedEventArgs e)
		{
			BAccount baccount = (BAccount)PXParentAttribute.SelectParent(sender, e.Row, typeof(BAccount));

			PXUIFieldAttribute.SetEnabled<CR.Location.isARAccountSameAsMain>(sender, e.Row, baccount!=null && !object.Equals(baccount.DefLocationID, ((Location)e.Row).LocationID));

			base.Location_RowSelected(sender, e);
		}

		protected virtual void Location_RowUpdated(PXCache sender, PXRowUpdatedEventArgs e)
		{
			SelectedLocation row = e.Row as SelectedLocation;
			if (row != null)
			{
				Address addr = PXSelect<Address, Where<Address.addressID, Equal<Required<Address.addressID>>>>.Select(this, row.DefAddressID);
				if (addr != null && Address.Cache.GetStatus(addr) == PXEntryStatus.Notchanged)
				{
					//LocationExtAddress projection is sensitive to Address.tstamp only
					Address.Cache.SetStatus(addr, PXEntryStatus.Updated);
				}
			}
		}

		protected virtual void Location_IsARAccountSameAsMain_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
		{
			CR.Location record = (CR.Location)e.Row;

			if (record.IsARAccountSameAsMain == false)
			{
				LocationARAccountSub mainloc = ARAccountSubLocation.Select();
				record.CARAccountID = mainloc.CARAccountID;
				record.CARSubID = mainloc.CARSubID;
				record.CRetainageAcctID = mainloc.CRetainageAcctID;
				record.CRetainageSubID = mainloc.CRetainageSubID;
				record.CARAccountLocationID = record.LocationID;

				LocationARAccountSub copyloc = new LocationARAccountSub();
				copyloc.BAccountID = record.BAccountID;
				copyloc.LocationID = record.LocationID;
				copyloc.CARAccountID = record.CARAccountID;
				copyloc.CARSubID = record.CARSubID;
				copyloc.CRetainageAcctID = record.CRetainageAcctID;
				copyloc.CRetainageSubID = record.CRetainageSubID;

				BusinessAccount.Cache.Current = (BAccount)PXParentAttribute.SelectParent(sender, e.Row, typeof(BAccount));
				ARAccountSubLocation.Insert(copyloc);
			}
			if (record.IsARAccountSameAsMain == true)
			{
				record.CARAccountID = null;
				record.CARSubID = null;
				record.CRetainageAcctID = null;
				record.CRetainageSubID = null;
				BAccount baccount = (BAccount)PXParentAttribute.SelectParent(sender, e.Row, typeof(BAccount));
				if (baccount != null)
				{
					record.CARAccountLocationID = baccount.DefLocationID;
				}
			}
		}

		protected virtual void Location_VBranchID_FieldDefaulting(PXCache sender, PXFieldDefaultingEventArgs e)
		{
			e.NewValue = null;
			e.Cancel = true;
		}
		#endregion

		#region LocationARAccountSub Events

		protected virtual void LocationARAccountSub_RowSelected(PXCache sender, PXRowSelectedEventArgs e)
		{
			if (Location.Current != null)
			{
				PXUIFieldAttribute.SetEnabled(sender, e.Row, object.Equals(Location.Current.LocationID, Location.Current.CARAccountLocationID));
			}
		}

		protected virtual void LocationARAccountSub_RowUpdated(PXCache sender, PXRowUpdatedEventArgs e)
		{
			LocationARAccountSub record = (LocationARAccountSub)e.Row;

			if (!sender.ObjectsEqual<LocationARAccountSub.cARAccountID, LocationARAccountSub.cARSubID>(e.Row, e.OldRow))
			{
				Location mainloc = Location.Current;
				var state = (PXFieldState)sender.GetStateExt<LocationARAccountSub.cARAccountID>(e.Row);
				if (state.ErrorLevel < PXErrorLevel.Error)
					mainloc.CARAccountID = record.CARAccountID;
				mainloc.CARSubID = record.CARSubID;

				Location.Cache.MarkUpdated(mainloc);

				Address addr = PXSelect<Address, Where<Address.addressID, Equal<Required<Address.addressID>>>>.Select(this, mainloc.DefAddressID);
				if (addr != null && Address.Cache.GetStatus(addr) == PXEntryStatus.Notchanged)
				{
					//LocationExtAddress projection is sensitive to Address.tstamp only
					Address.Cache.SetStatus(addr, PXEntryStatus.Updated);
				}
			}

			if (!sender.ObjectsEqual<LocationARAccountSub.cRetainageAcctID, LocationARAccountSub.cRetainageSubID>(e.Row, e.OldRow))
			{
				Location mainloc = Location.Current;
				var state = (PXFieldState)sender.GetStateExt<LocationARAccountSub.cRetainageAcctID>(e.Row);
				if (state.ErrorLevel < PXErrorLevel.Error)
					mainloc.CRetainageAcctID = record.CRetainageAcctID;
				mainloc.CRetainageSubID = record.CRetainageSubID;

				if (Location.Cache.GetStatus(mainloc) == PXEntryStatus.Notchanged)
				{
					Location.Cache.SetStatus(mainloc, PXEntryStatus.Updated);
				}
			}
		}

		#endregion

		}

	public class LocationMaintBase<Base, Primary, Where> : PXGraph<LocationMaintBase<Base, Primary, Where>>
		where Base : Location, new()
		where Primary : class, IBqlTable, new()
		where Where : class, IBqlWhere, new()
	{

		#region Public Selects

		public PXSelect<BAccount> BusinessAccount;

		public PXSelect<Base, Where> Location;


		public PXSelect<Location, Where<Location.bAccountID, Equal<Current<Location.bAccountID>>, And<Location.locationID, Equal<Current<Location.locationID>>>>> LocationCurrent;
		public PXSelect<Address, Where<Address.bAccountID, Equal<Current<Location.bAccountID>>,
										And<Address.addressID, Equal<Current<Location.defAddressID>>>>> Address;
		public PXSelect<Contact, Where<Contact.bAccountID, Equal<Current<Location.bAccountID>>,
										And<Contact.contactID, Equal<Current<Location.defContactID>>>>> Contact;

		public LocationMaintBase()
		{
            PXUIFieldAttribute.SetDisplayName<Contact.salutation>(Contact.Cache, CR.Messages.Attention);

			LocationValidator = new LocationValidator();
		}

		#endregion

		protected LocationValidator LocationValidator;

		#region Main Record Events

		protected virtual void Location_RowSelected(PXCache cache, PXRowSelectedEventArgs e)
		{
			SelectedLocation row = e.Row as SelectedLocation;
			if (row != null)
			{
				BAccount acct = BAccountUtility.FindAccount(this, row.BAccountID);
				if (acct != null)
				{
					if (!row.DefAddressID.HasValue)
					{
						row.DefAddressID = acct.DefAddressID; //Set default value
					}
					row.IsAddressSameAsMain = (row.DefAddressID == acct.DefAddressID);
					if (!row.DefContactID.HasValue)
					{
						row.DefContactID = acct.DefContactID;
					}
					row.IsContactSameAsMain = (row.DefContactID == acct.DefContactID);
				}

				bool VendorDetailsVisible = false;
				bool CustomerDetailsVisible = (row.LocType == LocTypeList.CustomerLoc || row.LocType == LocTypeList.CombinedLoc);
				bool CompanyDetailsVisible = (row.LocType == LocTypeList.CompanyLoc);

				if (row.LocType == LocTypeList.VendorLoc || row.LocType == LocTypeList.CombinedLoc)
				{
					VendorDetailsVisible = true;

					var vendor = VendorMaint.FindByID(this, row.BAccountID);
					if (vendor != null)
					{
						bool expenseAcctFieldsShouldBeRequired = vendor.TaxAgency == true;

						PXUIFieldAttribute.SetRequired<Location.vExpenseAcctID>(cache, expenseAcctFieldsShouldBeRequired);
						PXUIFieldAttribute.SetRequired<Location.vExpenseSubID>(cache, expenseAcctFieldsShouldBeRequired);
					}
				}

				PXUIFieldAttribute.SetEnabled<Location.vTaxZoneID>(cache, null, VendorDetailsVisible || CompanyDetailsVisible);
				PXUIFieldAttribute.SetEnabled<Location.vExpenseAcctID>(cache, null, VendorDetailsVisible);
				PXUIFieldAttribute.SetEnabled<Location.vExpenseSubID>(cache, null, VendorDetailsVisible);
				PXUIFieldAttribute.SetEnabled<Location.vRetainageAcctID>(cache, null, VendorDetailsVisible);
				PXUIFieldAttribute.SetEnabled<Location.vRetainageSubID>(cache, null, VendorDetailsVisible);
				PXUIFieldAttribute.SetEnabled<Location.vDiscountAcctID>(cache, null, VendorDetailsVisible);
				PXUIFieldAttribute.SetEnabled<Location.vDiscountSubID>(cache, null, VendorDetailsVisible);
				PXUIFieldAttribute.SetEnabled<Location.vLeadTime>(cache, null, VendorDetailsVisible);
				PXUIFieldAttribute.SetEnabled<Location.vBranchID>(cache, null, VendorDetailsVisible);
				PXUIFieldAttribute.SetEnabled<Location.vCarrierID>(cache, null, VendorDetailsVisible);
				PXUIFieldAttribute.SetEnabled<Location.vFOBPointID>(cache, null, VendorDetailsVisible);
				PXUIFieldAttribute.SetEnabled<Location.vShipTermsID>(cache, null, VendorDetailsVisible);

				PXUIFieldAttribute.SetEnabled<Location.cTaxZoneID>(cache, null, CustomerDetailsVisible);
				PXUIFieldAttribute.SetEnabled<Location.cAvalaraCustomerUsageType>(cache, null, CustomerDetailsVisible);
				PXUIFieldAttribute.SetEnabled<Location.cSalesAcctID>(cache, null, CustomerDetailsVisible);
				PXUIFieldAttribute.SetEnabled<Location.cSalesSubID>(cache, null, CustomerDetailsVisible);
				PXUIFieldAttribute.SetEnabled<Location.cSalesAcctID>(cache, null, CustomerDetailsVisible);
				PXUIFieldAttribute.SetEnabled<Location.cRetainageAcctID>(cache, null, CustomerDetailsVisible);
				PXUIFieldAttribute.SetEnabled<Location.cRetainageSubID>(cache, null, CustomerDetailsVisible);
				PXUIFieldAttribute.SetEnabled<Location.cDiscountAcctID>(cache, null, CustomerDetailsVisible);
				PXUIFieldAttribute.SetEnabled<Location.cDiscountSubID>(cache, null, CustomerDetailsVisible);
				PXUIFieldAttribute.SetEnabled<Location.cFreightAcctID>(cache, null, CustomerDetailsVisible);
				PXUIFieldAttribute.SetEnabled<Location.cFreightSubID>(cache, null, CustomerDetailsVisible);
				PXUIFieldAttribute.SetEnabled<Location.cBranchID>(cache, null, CustomerDetailsVisible);
				PXUIFieldAttribute.SetEnabled<Location.cCarrierID>(cache, null, CustomerDetailsVisible);
				
				PXUIFieldAttribute.SetEnabled<Location.cFOBPointID>(cache, null, CustomerDetailsVisible);
				PXUIFieldAttribute.SetEnabled<Location.cShipTermsID>(cache, null, CustomerDetailsVisible);

				PXUIFieldAttribute.SetEnabled<Location.cMPSalesSubID>(cache, null, CompanyDetailsVisible);
				PXUIFieldAttribute.SetEnabled<Location.cMPExpenseSubID>(cache, null, CompanyDetailsVisible);

				bool isGroundCollectVisible = false;

				if ( row.CCarrierID != null)
				{
					Carrier carrier = Carrier.PK.Find(this, row.CCarrierID);

					if (carrier != null && carrier.IsExternal == true && !string.IsNullOrEmpty(carrier.CarrierPluginID))
					{
						isGroundCollectVisible = CarrierPluginMaint.GetCarrierPluginAttributes(this, carrier.CarrierPluginID).Contains("COLLECT");
					}
				}

				PXUIFieldAttribute.SetVisible<Location.cGroundCollect>(cache, row, isGroundCollectVisible);

				EstablishCTaxZoneRule((field, isRequired) => PXUIFieldAttribute.SetRequired(Location.Cache, field.Name, isRequired));
				EstablishVTaxZoneRule((field, isRequired) => PXUIFieldAttribute.SetRequired(Location.Cache, field.Name, isRequired));
			}
		}

		protected virtual void Location_RowInserted(PXCache cache, PXRowInsertedEventArgs e)
		{
			Location row = (Location)e.Row;
			if (row != null)
			{
				BAccount account = BAccountUtility.FindAccount(this, row.BAccountID);
				if (account != null)
				{
					row.DefAddressID = account.DefAddressID;
					row.DefContactID = account.DefContactID;
				}
				else
				{
					//Inserting Address record
					Address addr = new Address();
					addr = this.Address.Insert(addr);
					row.DefAddressID = addr.AddressID;
					this.Address.Cache.IsDirty = false;
					//Inserting Contact record
					Contact cont = new Contact();
					cont = this.Contact.Insert(cont);
					row.DefContactID = cont.ContactID;
					this.Contact.Cache.IsDirty = false;
				}
				//Updating inserted record status
				this.Location.Cache.IsDirty = false;
			}
		}

		protected virtual void Location_RowDeleting(PXCache cache, PXRowDeletingEventArgs e)
		{
			Location row = e.Row as Location;
			BAccount acct = BAccountUtility.FindAccount(this, row.BAccountID); ;
			if (acct != null)
			{
				if (acct.DefLocationID == row.LocationID)
				{
					throw new PXException(Messages.CannotDeleteDefaultLoc);
				}
			}
		}

		protected virtual void Location_RowDeleted(PXCache cache, PXRowDeletedEventArgs e)
		{
			Location row = e.Row as Location;
			BAccount acct = BAccountUtility.FindAccount(this, row.BAccountID); ;
			if (acct != null)
			{
				if (row.DefAddressID.HasValue && row.DefAddressID != acct.DefAddressID)
				{
					Address addr = (Address)this.Address.SelectSingle();
					if (row.DefAddressID == addr.AddressID)
						this.Address.Delete(addr);
				}
				if (row.DefContactID.HasValue && row.DefContactID != acct.DefContactID)
				{
                    Contact cnt = (Contact)this.Contact.SelectSingle();
					if (row.DefContactID == cnt.ContactID)
						this.Contact.Delete(cnt);
				}
			}
		}

		protected virtual void Location_LocType_FieldDefaulting(PXCache cache, PXFieldDefaultingEventArgs e)
		{
			Location row = e.Row as Location;
			if (row != null)
			{
				BAccount acct = BAccountUtility.FindAccount(this, row.BAccountID);
				if (acct != null)
				{
					switch (acct.Type)
					{
						case BAccountType.VendorType:
							e.NewValue = LocTypeList.VendorLoc;
							break;
						case BAccountType.CustomerType:
						case BAccountType.EmpCombinedType:
							e.NewValue = LocTypeList.CustomerLoc;
							break;
						case BAccountType.CombinedType:
							e.NewValue = LocTypeList.CombinedLoc;
							break;
						default:
							e.NewValue = LocTypeList.CompanyLoc;
							break;
					}
				}
			}
		}

		protected virtual void Location_DefAddressID_FieldDefaulting(PXCache cache, PXFieldDefaultingEventArgs e)
		{
			Location row = e.Row as Location;
			if (row != null)
			{
				BAccount acct = BAccountUtility.FindAccount(this, row.BAccountID);
				if (acct != null)
				{
					e.NewValue = acct.DefAddressID;
				}
			}
		}

		protected virtual void Location_DefContactID_FieldDefaulting(PXCache cache, PXFieldDefaultingEventArgs e)
		{
			Location row = e.Row as Location;
			if (row != null)
			{
				BAccount acct = BAccountUtility.FindAccount(this, row.BAccountID);
				if (acct != null)
				{
					e.NewValue = acct.DefContactID;
				}
			}
		}

		protected virtual void Location_CARAccountLocationID_FieldDefaulting(PXCache cache, PXFieldDefaultingEventArgs e)
		{
			Location row = e.Row as Location;
			if (row != null)
			{
				BAccount acct = BAccountUtility.FindAccount(this, row.BAccountID);
				if (acct != null)
				{
					e.NewValue = acct.DefLocationID;
				}
			}
		}

		protected virtual void Location_VAPAccountLocationID_FieldDefaulting(PXCache cache, PXFieldDefaultingEventArgs e)
		{
			Location row = e.Row as Location;
			if (row != null)
			{
				BAccount acct = BAccountUtility.FindAccount(this, row.BAccountID);
				if (acct != null)
				{
					e.NewValue = acct.DefLocationID;
				}
			}
		}

		protected virtual void Location_VPaymentInfoLocationID_FieldDefaulting(PXCache cache, PXFieldDefaultingEventArgs e)
		{
			Location row = e.Row as Location;
			if (row != null)
			{
				BAccount acct = BAccountUtility.FindAccount(this, row.BAccountID);
				if (acct != null)
				{
					e.NewValue = acct.DefLocationID;
				}
			}
		}

		protected virtual void Location_VRemitAddressID_FieldDefaulting(PXCache cache, PXFieldDefaultingEventArgs e)
		{
			Location row = e.Row as Location;
			if (row != null)
			{
				BAccount acct = BAccountUtility.FindAccount(this, row.BAccountID);
				if (acct != null)
				{
					e.NewValue = acct.DefAddressID;
				}
			}
		}

		protected virtual void Location_VRemitContactID_FieldDefaulting(PXCache cache, PXFieldDefaultingEventArgs e)
		{
			Location row = e.Row as Location;
			if (row != null)
			{
				BAccount acct = BAccountUtility.FindAccount(this, row.BAccountID);
				if (acct != null)
				{
					e.NewValue = acct.DefContactID;
				}
			}
		}

		protected virtual void Location_VExpenseAcctID_FieldDefaulting(PXCache cache, PXFieldDefaultingEventArgs e)
		{
			Location loc = e.Row as Location;
			if (loc != null && loc.BAccountID != null)
			{
				BAccount acct = BAccountUtility.FindAccount(this, loc.BAccountID);
				if (acct != null &&
						 acct.DefLocationID != null &&
						 (acct.Type == BAccountType.VendorType || acct.Type == BAccountType.CombinedType) &&
						 loc.LocationID != acct.DefLocationID)
				{
					Location defLocation = PXSelect<Location>.Search<Location.locationID, Location.bAccountID>(this, acct.DefLocationID, acct.BAccountID);
					if (defLocation != null && defLocation.VExpenseAcctID != null)
					{
						e.NewValue = defLocation.VExpenseAcctID;
						e.Cancel = true;
					}
				}
			}
		}

		protected virtual void Location_VExpenseSubID_FieldDefaulting(PXCache cache, PXFieldDefaultingEventArgs e)
		{
			Location loc = e.Row as Location;
			if (loc != null && loc.BAccountID != null)
			{
				BAccount acct = BAccountUtility.FindAccount(this, loc.BAccountID);
				if (acct != null &&
						 acct.DefLocationID != null &&
						 (acct.Type == BAccountType.VendorType || acct.Type == BAccountType.CombinedType) &&
						 loc.LocationID != acct.DefLocationID)
				{
					Location defLocation = PXSelect<Location>.Search<Location.locationID, Location.bAccountID>(this, acct.DefLocationID, acct.BAccountID);
					if (defLocation != null && defLocation.VExpenseSubID != null)
					{
						e.NewValue = defLocation.VExpenseSubID;
						e.Cancel = true;
					}
				}
			}
		}

		protected virtual void Location_CSalesAcctID_FieldDefaulting(PXCache cache, PXFieldDefaultingEventArgs e)
		{
			Location loc = e.Row as Location;
			if (loc != null && loc.BAccountID != null)
			{
				BAccount acct = BAccountUtility.FindAccount(this, loc.BAccountID);
				if (acct != null &&
						 acct.DefLocationID != null &&
						 (acct.Type == BAccountType.CustomerType || acct.Type == BAccountType.CombinedType) &&
						 loc.LocationID != acct.DefLocationID)
				{
					Location defLocation = PXSelect<Location>.Search<Location.locationID, Location.bAccountID>(this, acct.DefLocationID, acct.BAccountID);
					if (defLocation != null && defLocation.CSalesAcctID != null)
					{
						e.NewValue = defLocation.CSalesAcctID;
						e.Cancel = true;
					}
				}
			}
		}

		protected virtual void Location_CSalesSubID_FieldDefaulting(PXCache cache, PXFieldDefaultingEventArgs e)
		{
			Location loc = e.Row as Location;
			if (loc != null && loc.BAccountID != null)
			{
				BAccount acct = BAccountUtility.FindAccount(this, loc.BAccountID);
				if (acct != null &&
						 acct.DefLocationID != null &&
						 (acct.Type == BAccountType.CustomerType || acct.Type == BAccountType.CombinedType) &&
						 loc.LocationID != acct.DefLocationID)
				{
					Location defLocation = PXSelect<Location>.Search<Location.locationID, Location.bAccountID>(this, acct.DefLocationID, acct.BAccountID);
					if (defLocation != null && defLocation.CSalesSubID != null)
					{
						e.NewValue = defLocation.CSalesSubID;
						e.Cancel = true;
					}
				}
			}
		}
		protected virtual void Location_CPriceClassID_FieldDefaulting(PXCache cache, PXFieldDefaultingEventArgs e)
		{
			Location loc = e.Row as Location;
			if (loc != null && loc.BAccountID != null)
			{
				BAccount acct = BAccountUtility.FindAccount(this, loc.BAccountID);
				if (acct != null &&
						 acct.DefLocationID != null &&
						 (acct.Type == BAccountType.CustomerType || acct.Type == BAccountType.CombinedType) &&
						 loc.LocationID != acct.DefLocationID)
				{
					Location defLocation = PXSelect<Location>.Search<Location.locationID, Location.bAccountID>(this, acct.DefLocationID, acct.BAccountID);
					if (defLocation != null && defLocation.CPriceClassID != null)
					{
						e.NewValue = defLocation.CPriceClassID;
						e.Cancel = true;
					}
				}
			}
		}
		protected virtual void Location_VTaxZoneID_FieldDefaulting(PXCache cache, PXFieldDefaultingEventArgs e)
		{
			Location loc = e.Row as Location;
			if (loc != null && loc.BAccountID != null)
			{
				BAccount acct = BAccountUtility.FindAccount(this, loc.BAccountID);
				if (acct != null &&
						 acct.DefLocationID != null &&
						 (acct.Type == BAccountType.VendorType || acct.Type == BAccountType.CombinedType) &&
						 loc.LocationID != acct.DefLocationID)
				{
					Vendor vendor = PXSelect<Vendor>.Search<Vendor.bAccountID>(this, acct.BAccountID);
					VendorClass vClass = PXSelect<VendorClass>.Search<VendorClass.vendorClassID>(this, vendor.VendorClassID);
					if (vClass != null && vClass.TaxZoneID != null)
					{
						e.NewValue = vClass.TaxZoneID;
						e.Cancel = true;
					}
				}
			}
		}
		protected virtual void Location_VTaxCalcMode_FieldDefaulting(PXCache cache, PXFieldDefaultingEventArgs e)
		{
			Location loc = e.Row as Location;
			if (loc != null && loc.BAccountID != null)
			{
				BAccount acct = BAccountUtility.FindAccount(this, loc.BAccountID);
				if (acct != null &&
						 acct.DefLocationID != null &&
						 (acct.Type == BAccountType.VendorType || acct.Type == BAccountType.CombinedType) &&
						 loc.LocationID != acct.DefLocationID)
				{
					Vendor vendor = PXSelect<Vendor>.Search<Vendor.bAccountID>(this, acct.BAccountID);
					VendorClass vClass = PXSelect<VendorClass>.Search<VendorClass.vendorClassID>(this, vendor.VendorClassID);
					if (vClass != null && vClass.TaxCalcMode != null)
					{
						e.NewValue = vClass.TaxCalcMode;
						e.Cancel = true;
					}
				}
			}

		}
		protected virtual void Location_CTaxZoneID_FieldDefaulting(PXCache cache, PXFieldDefaultingEventArgs e)
		{
			Location loc = e.Row as Location;
			if (loc != null && loc.BAccountID != null)
			{
				BAccount acct = BAccountUtility.FindAccount(this, loc.BAccountID);
				if (acct != null &&
						 acct.DefLocationID != null &&
						 (acct.Type == BAccountType.CustomerType || acct.Type == BAccountType.CombinedType) &&
						 loc.LocationID != acct.DefLocationID)
				{
					Customer customer = PXSelect<Customer>.Search<Customer.bAccountID>(this, acct.BAccountID);
					CustomerClass cClass = PXSelect<CustomerClass>.Search<CustomerClass.customerClassID>(this, customer.CustomerClassID);
					if (cClass != null && cClass.TaxZoneID != null)
					{
						e.NewValue = cClass.TaxZoneID;
						e.Cancel = true;
					}
				}
			}
		}

		protected virtual void Location_CAvalaraCustomerUsageType_FieldDefaulting(PXCache cache, PXFieldDefaultingEventArgs e)
		{
			Location loc = e.Row as Location;
			if (loc != null && loc.BAccountID != null)
			{
				BAccount acct = BAccountUtility.FindAccount(this, loc.BAccountID);
				if (acct != null &&
						 acct.DefLocationID != null &&
						 (acct.Type == BAccountType.CustomerType || acct.Type == BAccountType.CombinedType) &&
						 loc.LocationID != acct.DefLocationID)
				{
					Customer customer = PXSelect<Customer>.Search<Customer.bAccountID>(this, acct.BAccountID);
					CustomerClass cClass = PXSelect<CustomerClass>.Search<CustomerClass.customerClassID>(this, customer.CustomerClassID);
					if (cClass != null && cClass.AvalaraCustomerUsageType != null)
					{
						e.NewValue = cClass.AvalaraCustomerUsageType;
						e.Cancel = true;
						return;
					}
				}

				e.NewValue = TXAvalaraCustomerUsageType.Default;
				e.Cancel = true;
			}
		}

		protected virtual void Location_CShipComplete_FieldDefaulting(PXCache cache, PXFieldDefaultingEventArgs e)
		{
			Location row = e.Row as Location;
			if (row != null)
			{
				BAccount acct = BAccountUtility.FindAccount(this, row.BAccountID);
				if (acct != null)
				{
					LocationExtAddress DefLocation = PXSelect<LocationExtAddress, Where<LocationExtAddress.locationBAccountID, Equal<Required<BAccount.bAccountID>>,
					And<LocationExtAddress.locationID, Equal<Required<BAccount.defLocationID>>>>>.Select(this, acct.BAccountID, acct.DefLocationID);
					if (DefLocation != null && DefLocation.CShipComplete != null)
						e.NewValue = DefLocation.CShipComplete;
				}
			}
		}

		protected virtual void Location_RowPersisting(PXCache cache, PXRowPersistingEventArgs e)
		{
			Location loc = e.Row as Location;
			if (loc == null || loc.BAccountID == null) return;
			
			BAccount acct = BAccountUtility.FindAccount(this, loc.BAccountID);
			if (acct != null)
			{
				EstablishCTaxZoneRule((field, isRequired) => PXDefaultAttribute.SetPersistingCheck(cache, field.Name, loc, isRequired ? PXPersistingCheck.Null : PXPersistingCheck.Nothing));
				EstablishVTaxZoneRule((field, isRequired) => PXDefaultAttribute.SetPersistingCheck(cache, field.Name, loc, isRequired ? PXPersistingCheck.Null : PXPersistingCheck.Nothing));

				if (acct.Type == BAccountType.CustomerType || acct.Type == BAccountType.CombinedType)
				{
					LocationValidator.ValidateCustomerLocation(cache, acct, loc);
					VerifyAvalaraUsageType(loc);
				}
				if (acct.Type == BAccountType.VendorType || acct.Type == BAccountType.CombinedType)
				{
					var vendor = VendorMaint.GetByID(this, acct.BAccountID);
					LocationValidator.ValidateVendorLocation(cache, vendor, loc);
				}
			}
		}

		object _KeyToAbort = null;

		protected virtual void Location_RowPersisted(PXCache sender, PXRowPersistedEventArgs e)
		{
			if (e.Operation == PXDBOperation.Insert)
			{
				if (e.TranStatus == PXTranStatus.Open)
				{
					if ((int?)sender.GetValue<CR.Location.vAPAccountLocationID>(e.Row) < 0)
					{
						_KeyToAbort = sender.GetValue<Location.locationID>(e.Row);

						PXDatabase.Update<Location>(
							new PXDataFieldAssign("VAPAccountLocationID", _KeyToAbort),
							new PXDataFieldRestrict("LocationID", _KeyToAbort),
							PXDataFieldRestrict.OperationSwitchAllowed);

						sender.SetValue<CR.Location.vAPAccountLocationID>(e.Row, _KeyToAbort);
					}

					if ((int?)sender.GetValue<CR.Location.vPaymentInfoLocationID>(e.Row) < 0)
					{
						_KeyToAbort = sender.GetValue<Location.locationID>(e.Row);

						PXDatabase.Update<Location>(
							new PXDataFieldAssign("VPaymentInfoLocationID", _KeyToAbort),
							new PXDataFieldRestrict("LocationID", _KeyToAbort),
							PXDataFieldRestrict.OperationSwitchAllowed);

						sender.SetValue<CR.Location.vPaymentInfoLocationID>(e.Row, _KeyToAbort);
					}

					if ((int?)sender.GetValue<CR.Location.cARAccountLocationID>(e.Row) < 0)
					{
						_KeyToAbort = sender.GetValue<Location.locationID>(e.Row);

						PXDatabase.Update<Location>(
							new PXDataFieldAssign("CARAccountLocationID", _KeyToAbort),
							new PXDataFieldRestrict("LocationID", _KeyToAbort),
							PXDataFieldRestrict.OperationSwitchAllowed);

						sender.SetValue<CR.Location.cARAccountLocationID>(e.Row, _KeyToAbort);
					}
				}
				else
				{
					if (e.TranStatus == PXTranStatus.Aborted)
					{
						if (object.Equals(_KeyToAbort, sender.GetValue<CR.Location.vAPAccountLocationID>(e.Row)))
						{
							object KeyAborted = sender.GetValue<Location.locationID>(e.Row);
							sender.SetValue<CR.Location.vAPAccountLocationID>(e.Row, KeyAborted);
						}

						if (object.Equals(_KeyToAbort, sender.GetValue<CR.Location.vPaymentInfoLocationID>(e.Row)))
						{
							object KeyAborted = sender.GetValue<Location.locationID>(e.Row);
							sender.SetValue<CR.Location.vPaymentInfoLocationID>(e.Row, KeyAborted);
						}

						if (object.Equals(_KeyToAbort, sender.GetValue<CR.Location.cARAccountLocationID>(e.Row)))
						{
							object KeyAborted = sender.GetValue<Location.locationID>(e.Row);
							sender.SetValue<CR.Location.cARAccountLocationID>(e.Row, KeyAborted);
						}
					}
					_KeyToAbort = null;
				}
			}
		}

		public virtual void Location_IsAddressSameAsMain_FieldUpdated(PXCache cache, PXFieldUpdatedEventArgs e)
		{
			SelectedLocation row = (SelectedLocation)e.Row;
			if (row != null && row.BAccountID != null)
			{
				PXResult<BAccount, Address> res = (PXResult<BAccount, Address>)PXSelectJoin<BAccount, LeftJoin<Address, On<Address.addressID, Equal<BAccount.defAddressID>>>,
														Where<BAccount.bAccountID, Equal<Required<BAccount.bAccountID>>>>.Select(this, row.BAccountID);
				if (res != null)
				{
					Address defAddress = res;
					BAccount account = res;
					if (row.IsAddressSameAsMain == true)
					{
						if (row.DefAddressID != account.DefAddressID)
						{
							Address addr = this.Address.Current;
							if (addr == null || addr.AddressID != row.DefAddressID)
								addr = PXSelect<Address, Where<Address.addressID, Equal<Required<Address.addressID>>>>.Select(this, row.DefAddressID);
							if (addr != null && addr.AddressID == row.DefAddressID)
							{
								this.Address.Cache.Delete(addr);
							}
							row.DefAddressID = account.DefAddressID;
							//this.Location.View.RequestRefresh();							
						}
					}
					else
					{
						Address addr = PXCache<Address>.CreateCopy(defAddress);
						addr.BAccountID = row.BAccountID;
						addr.AddressID = null;
						//Copy from default here
						addr = (Address)this.Address.Cache.Insert(addr);
						row.DefAddressID = addr.AddressID;
						this.Location.View.RequestRefresh();
					}
				}
			}
		}

		public virtual void Location_IsContactSameAsMain_FieldUpdated(PXCache cache, PXFieldUpdatedEventArgs e)
		{
			SelectedLocation row = (SelectedLocation)e.Row;
			//BAccount account = BAccountUtility.FindAccount(this, row.BAccountID); ;
			if (row != null && row.BAccountID != null)
			{
				PXResult<BAccount, Contact> res = (PXResult<BAccount, Contact>)PXSelectJoin<BAccount, LeftJoin<Contact, On<Contact.contactID, Equal<BAccount.defContactID>>>,
														   Where<BAccount.bAccountID, Equal<Required<BAccount.bAccountID>>>>.Select(this, row.BAccountID);
				if (res != null)
				{
					Contact defContact = res;
					BAccount account = res;
					if (row.IsContactSameAsMain == true)
					{
						if (row.DefContactID != account.DefContactID)
						{
							Contact cont = this.Contact.Current;
							if (cont == null || cont.ContactID != row.DefContactID)
								cont = PXSelect<Contact, Where<Contact.contactID, Equal<Required<Contact.contactID>>>>.Select(this, row.DefContactID);
							if (cont.ContactID == row.DefContactID)
							{
								this.Contact.Cache.Delete(cont);
							}
							row.DefContactID = account.DefContactID;
							this.Location.View.RequestRefresh();
						}
					}
					else
					{
						Contact cont = new Contact();
						if (defContact != null && defContact.ContactID.HasValue)
							PXCache<Contact>.RestoreCopy(cont, defContact);
						cont.ContactType = ContactTypesAttribute.BAccountProperty;
						cont.BAccountID = account.BAccountID;
						cont.ContactID = null;
					    cont.NoteID = null;
						//Copy from default here
						cont = (Contact)this.Contact.Cache.Insert(cont);
						row.DefContactID = cont.ContactID;
						this.Location.View.RequestRefresh();
					}
				}
			}
		}

		#endregion

		#region Address Events

		protected virtual void Address_RowSelected(PXCache cache, PXRowSelectedEventArgs e)
		{
            if (e.Row == null) return;

            Address row = e.Row as Address;
			BAccount acct = BAccountUtility.FindAccount(this, row.BAccountID); ;
			bool isSameAsMain = false;
			if (acct != null)
			{
				isSameAsMain = (row.AddressID == acct.DefAddressID);
			}
			PXUIFieldAttribute.SetEnabled(cache, row, !isSameAsMain);
			PXUIFieldAttribute.SetEnabled<Address.isValidated>(cache, row, false);
		}

		#endregion

		#region Contact Events

		protected virtual void Contact_RowSelected(PXCache cache, PXRowSelectedEventArgs e)
		{
            if (e.Row == null) return;

            Contact row = e.Row as Contact;
			BAccount acct = BAccountUtility.FindAccount(this, row.BAccountID); ;
			bool isSameAsMain = false;
			if (acct != null)
			{
				isSameAsMain = (row.ContactID == acct.DefContactID);
			}
			PXUIFieldAttribute.SetEnabled(cache, row, !isSameAsMain);
		}
		#endregion

		private CustomerClass ReadCustomerClass()
		{
			CustomerClass customerClass = 
				(PXResult<AR.Customer, AR.CustomerClass>)PXSelectJoin<AR.Customer,
					InnerJoin<AR.CustomerClass, On<AR.Customer.customerClassID, Equal<AR.CustomerClass.customerClassID>>>,
					Where<AR.Customer.bAccountID, Equal<Optional<Location.bAccountID>>>>.Select(this);

			return customerClass;
		}

		protected virtual void VerifyAvalaraUsageType(Location loc)
		{
			CustomerClass customerClass = ReadCustomerClass();
			if (customerClass == null || customerClass.RequireAvalaraCustomerUsageType != true)
				return;

			if (loc.CAvalaraCustomerUsageType == TXAvalaraCustomerUsageType.Default)
				throw new PXRowPersistingException(typeof(Location.cAvalaraCustomerUsageType).Name,
					loc.CAvalaraCustomerUsageType, Common.Messages.NonDefaultAvalaraUsageType);
		}

		protected virtual void EstablishCTaxZoneRule(Action<Type, bool> setCheck)
		{
			CustomerClass customerClass = ReadCustomerClass();

			bool isTaxZoneRequired = customerClass != null && customerClass.RequireTaxZone == true;
			setCheck(typeof(Location.cTaxZoneID), isTaxZoneRequired);
		}

		protected virtual void EstablishVTaxZoneRule(Action<Type, bool> setCheck)
		{
			PXResult<AP.Vendor, AP.VendorClass> res = (PXResult<AP.Vendor, AP.VendorClass>)PXSelectJoin<AP.Vendor, InnerJoin<AP.VendorClass, On<AP.Vendor.vendorClassID, Equal<AP.VendorClass.vendorClassID>>>,
										Where<AP.Vendor.bAccountID, Equal<Optional<Location.bAccountID>>>>.Select(this);

			AP.VendorClass vendorClass = res;

			bool isTaxZoneRequred = vendorClass != null && vendorClass.RequireTaxZone == true;
			setCheck(typeof(Location.vTaxZoneID), isTaxZoneRequred);
		}
	}

	public class LocationValidator
	{
		public virtual bool ValidateCustomerLocation(PXCache cache, BAccount baccount, ILocation location)
		{
			bool res = ValidateCommon(cache, baccount, location);

			var validationHelper = new AccountAndSubValidationHelper(cache, location);

			res &= validationHelper.SetErrorEmptyIfNull<Location.cSalesAcctID>(location.CSalesAcctID)
				.SetErrorEmptyIfNull<Location.cSalesSubID>(location.CSalesSubID)
				.SetErrorIfInactiveAccount<Location.cARAccountID>(location.CARAccountID)
				.SetErrorIfInactiveSubAccount<Location.cARSubID>(location.CARSubID)
				.SetErrorIfInactiveAccount<Location.cSalesAcctID>(location.CSalesAcctID)
				.SetErrorIfInactiveSubAccount<Location.cSalesSubID>(location.CSalesSubID)
				.SetErrorIfInactiveAccount<Location.cDiscountAcctID>(location.CDiscountAcctID)
				.SetErrorIfInactiveSubAccount<Location.cDiscountSubID>(location.CDiscountSubID)
				.SetErrorIfInactiveAccount<Location.cFreightAcctID>(location.CFreightAcctID)
				.SetErrorIfInactiveSubAccount<Location.cFreightSubID>(location.CFreightSubID)
				.IsValid;

			if (location.IsARAccountSameAsMain != true)
			{
				res &= validationHelper.SetErrorEmptyIfNull<Location.cARAccountID>(location.CARAccountID)
					.SetErrorEmptyIfNull<Location.cARSubID>(location.CARSubID)
					.IsValid;
			}

			return res;
		}

		public virtual bool ValidateVendorLocation(PXCache cache, Vendor vendor, ILocation location)
		{
			bool res = ValidateCommon(cache, vendor, location);

			var validationHelper = new ValidationHelper(cache, location);

			if (vendor.TaxAgency == true)
			{
				res &= validationHelper.SetErrorEmptyIfNull<Location.vExpenseAcctID>(location.VExpenseAcctID)
					.SetErrorEmptyIfNull<Location.vExpenseSubID>(location.VExpenseSubID)
					.IsValid;
			}

			if (location.IsAPAccountSameAsMain != true)
			{
				res &= validationHelper.SetErrorEmptyIfNull<Location.vAPAccountID>(location.VAPAccountID)
					.SetErrorEmptyIfNull<Location.vAPSubID>(location.VAPSubID)
					.IsValid;
			}

			return res;
		}

		protected virtual bool ValidateCommon(PXCache cache, BAccount acct, ILocation loc)
		{
			if (loc.LocationID == acct.DefLocationID && loc.IsActive != true)
			{
				cache.RaiseExceptionHandling<Location.isActive>(loc, null, new PXSetPropertyException(Messages.DefaultLocationCanNotBeNotActive, typeof(Location.isActive).Name));
				return false;
			}

			return true;
		}
	}
}
