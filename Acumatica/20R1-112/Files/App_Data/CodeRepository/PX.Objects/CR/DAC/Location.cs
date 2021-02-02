using System;
using PX.Data;
using PX.Data.ReferentialIntegrity.Attributes;
using PX.Objects.AR;
using PX.Objects.CR.MassProcess;
using PX.Objects.GL;
using PX.Objects.PO;
using PX.Objects.TX;
using PX.Objects.CS;
using PX.Objects.IN;
using PX.Objects.SO;
using PX.Objects.AP;
using PX.Objects.CA;

namespace PX.Objects.CR
{
	[System.SerializableAttribute()]
	[PXPrimaryGraph(new Type[] {
					typeof(VendorLocationMaint),
					typeof(CustomerLocationMaint),	
                    typeof(BranchMaint),
					typeof(EP.EmployeeMaint),
					typeof(LocationMaint)},
				new Type[] {
					typeof(Select<Location, 
						Where<Location.bAccountID, Equal<Current<Location.bAccountID>>, And<Location.locationID, Equal<Current<Location.locationID>>, 
						And<Where<Current<Location.locType>, Equal<LocTypeList.vendorLoc>, Or<Current<Location.locType>, Equal<LocTypeList.combinedLoc>>>>>>>),
                    typeof(Select<Location, 
						Where<Location.bAccountID, Equal<Current<Location.bAccountID>>, And<Location.locationID, Equal<Current<Location.locationID>>, 
							And<Where<Current<Location.locType>, Equal<LocTypeList.customerLoc>, Or<Current<Location.locType>, Equal<LocTypeList.combinedLoc>>>>>>>),
					typeof(Select2<Branch, InnerJoin<BAccount, On<BAccount.bAccountID, Equal<Branch.bAccountID>>, 
						InnerJoin<Location, On<Location.bAccountID, Equal<BAccount.bAccountID>, And<Location.locationID, Equal<BAccount.defLocationID>>>>>, 
						Where<Location.bAccountID, Equal<Current<Location.bAccountID>>, And<Location.locationID, Equal<Current<Location.locationID>>, And<Current<Location.locType>, Equal<LocTypeList.companyLoc>>>>>),
					typeof(Select2<EP.EPEmployee, 
						InnerJoin<Location, On<Location.bAccountID, Equal<EP.EPEmployee.bAccountID>, And<Location.locationID, Equal<EP.EPEmployee.defLocationID>>>>, 
						Where<Location.bAccountID, Equal<Current<Location.bAccountID>>, And<Location.locationID, Equal<Current<Location.locationID>>, And<Current<Location.locType>, Equal<LocTypeList.employeeLoc>>>>>),
					typeof(Select<Location, 
						Where<Location.bAccountID, Equal<Current<Location.bAccountID>>, And<Location.locationID, Equal<Current<Location.locationID>>>>>)
					})]
	[PXCacheName(Messages.Location)]
	[PXProjection(typeof(Select2<Location, 
		LeftJoin<LocationAPAccountSub, On<LocationAPAccountSub.bAccountID, Equal<Location.bAccountID>, And<LocationAPAccountSub.locationID, Equal<Location.vAPAccountLocationID>>>,
		LeftJoin<LocationARAccountSub, On<LocationARAccountSub.bAccountID, Equal<Location.bAccountID>, And<LocationARAccountSub.locationID, Equal<Location.cARAccountLocationID>>>,
		LeftJoin<LocationAPPaymentInfo, On<LocationAPPaymentInfo.bAccountID, Equal<Location.bAccountID>, And<LocationAPPaymentInfo.locationID, Equal<Location.vPaymentInfoLocationID>>>,
		LeftJoin<BAccountR, On<BAccountR.bAccountID, Equal<Location.bAccountID>>>>>>>), Persistent = true)]
	public partial class Location : PX.Data.IBqlTable, IPaymentTypeDetailMaster, ILocation
	{
		#region BAccountID
		public abstract class bAccountID : PX.Data.BQL.BqlInt.Field<bAccountID> { }
		protected Int32? _BAccountID;
		[PXDBInt(IsKey = true)]
		[PXDBLiteDefault(typeof(BAccount.bAccountID))]
		[PXUIField(DisplayName = "Account ID", Visible = false, Enabled = false, Visibility = PXUIVisibility.Invisible, TabOrder = 0)]
		[PXParent(typeof(Select<BAccount,
			Where<BAccount.bAccountID,
			Equal<Current<Location.bAccountID>>>>)
			)]
		public virtual Int32? BAccountID
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
		protected Int32? _LocationID;
		[PXDBIdentity]
		[PXUIField(Visible = false, Enabled=false, Visibility = PXUIVisibility.Invisible)]
		[PXReferentialIntegrityCheck]
		public virtual Int32? LocationID
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
		#region LocationCD
		public abstract class locationCD : PX.Data.BQL.BqlString.Field<locationCD> { }
		protected String _LocationCD;
		[CS.LocationRaw(typeof(Where<Location.bAccountID, Equal<Current<Location.bAccountID>>>), IsKey = true, Visibility = PXUIVisibility.SelectorVisible, DisplayName = "Location ID")]
		[PXDefault(PersistingCheck =PXPersistingCheck.NullOrBlank)]
		public virtual String LocationCD
		{
			get
			{
				return this._LocationCD;
			}
			set
			{
				this._LocationCD = value;
			}
		}
		#endregion
		#region LocType
		public abstract class locType : PX.Data.BQL.BqlString.Field<locType> { }
		protected String _LocType;
		[PXDBString(2,IsFixed = true)]
		[LocTypeList.List()]
		[PXUIField(DisplayName = "Location Type", Visibility = PXUIVisibility.SelectorVisible)]
		public virtual String LocType
		{
			get
			{
				return this._LocType;
			}
			set
			{
				this._LocType = value;
			}
		}
		#endregion
		#region Descr
		public abstract class descr : PX.Data.BQL.BqlString.Field<descr> { }
		protected String _Descr;
		[PXDBString(60,IsUnicode = true)]
		[PXUIField(DisplayName = "Location Name", Visibility = PXUIVisibility.SelectorVisible)]
		public virtual String Descr
		{
			get
			{
				return this._Descr;
			}
			set
			{
				this._Descr = value;
			}
		}
		#endregion
		#region TaxRegistrationID
		public abstract class taxRegistrationID : PX.Data.BQL.BqlString.Field<taxRegistrationID> { }
		protected String _TaxRegistrationID;
		[PXDBString(50, IsUnicode = true)]
		[PXUIField(DisplayName = "Tax Registration ID")]
        [PXMassMergableField]
		[PXPersonalDataField]
		public virtual String TaxRegistrationID
		{
			get
			{
				return this._TaxRegistrationID;
			}
			set
			{
				this._TaxRegistrationID = value;
			}
		}
		#endregion
		#region DefAddressID
		public abstract class defAddressID : PX.Data.BQL.BqlInt.Field<defAddressID> { }
		protected Int32? _DefAddressID;
		[PXDBInt()]
		[PXDBChildIdentity(typeof(Address.addressID))]
		[PXUIField(DisplayName = "Default Address", Visibility = PXUIVisibility.Invisible)]
		[PXSelector(typeof(Search<Address.addressID>),DirtyRead = true)]
		public virtual Int32? DefAddressID
		{
			get
			{
				return this._DefAddressID;
			}
			set
			{
				this._DefAddressID = value;
			}
		}
		#endregion
		#region DefContactID
		public abstract class defContactID : PX.Data.BQL.BqlInt.Field<defContactID> { }
		protected Int32? _DefContactID;
		[PXDBInt()]
		[PXDBChildIdentity(typeof(Contact.contactID))]
		[PXUIField(DisplayName = "Default Contact")]
		[PXSelector(typeof(Search<Contact.contactID>), ValidateValue = false, DirtyRead = true)]
		public virtual Int32? DefContactID
		{
			get
			{
				return this._DefContactID;
			}
			set
			{
				this._DefContactID = value;
			}
		}
		#endregion
		#region NoteID
		public abstract class noteID : PX.Data.BQL.BqlGuid.Field<noteID> { }
		protected Guid? _NoteID;
		[PXNote()]
		public virtual Guid? NoteID
		{
			get
			{
				return this._NoteID;
			}
			set
			{
				this._NoteID = value;
			}
		}
		#endregion
		#region IsActive
		public abstract class isActive : PX.Data.BQL.BqlBool.Field<isActive> { }
		protected bool? _IsActive;
		[PXDBBool()]
		[PXDefault(true)]
		[PXUIField(DisplayName = "Active")]
		public virtual bool? IsActive
		{
			get
			{
				return this._IsActive;
			}
			set
			{
				this._IsActive = value;
			}
		}
		#endregion
		#region IsDefault
		public abstract class isDefault : PX.Data.BQL.BqlBool.Field<isDefault> { }

		[PXBool]
		[PXUIField(DisplayName = "Default", Enabled = false)]
		public virtual bool? IsDefault { get; set; }
		#endregion
		#region IsAPAccountSameAsMain
		public abstract class isAPAccountSameAsMain : PX.Data.BQL.BqlBool.Field<isAPAccountSameAsMain> { }
		protected bool? _IsAPAccountSameAsMain;
		[PXBool()]
		[PXUIField(DisplayName = "Same As Default Location's")]
		[PXFormula(typeof(Switch<Case<Where<locationID, Equal<vAPAccountLocationID>>, False>, True>))]
		public virtual bool? IsAPAccountSameAsMain
		{
			get
			{
				return this._IsAPAccountSameAsMain;
			}
			set
			{
				this._IsAPAccountSameAsMain = value;
			}
		}
		#endregion
		#region IsAPPaymentInfoSameAsMain
		public abstract class isAPPaymentInfoSameAsMain : PX.Data.BQL.BqlBool.Field<isAPPaymentInfoSameAsMain> { }
		protected bool? _IsAPPaymentInfoSameAsMain;
		[PXBool()]
		[PXUIField(DisplayName = "Same As Default Location's")]
		[PXFormula(typeof(Switch<Case<Where<locationID, Equal<vPaymentInfoLocationID>>, False>, True>))]
		public virtual bool? IsAPPaymentInfoSameAsMain
		{
			get
			{
				return this._IsAPPaymentInfoSameAsMain;
			}
			set
			{
				this._IsAPPaymentInfoSameAsMain = value;
			}
		}
		#endregion
		#region IsARAccountSameAsMain
		public abstract class isARAccountSameAsMain : PX.Data.BQL.BqlBool.Field<isARAccountSameAsMain> { }
		protected bool? _IsARAccountSameAsMain;
		[PXBool()]
		[PXUIField(DisplayName = "Same As Default Location's")]
		[PXFormula(typeof(Switch<Case<Where<locationID, Equal<cARAccountLocationID>>, False>, True>))]
		public virtual bool? IsARAccountSameAsMain
		{
			get
			{
				return this._IsARAccountSameAsMain;
			}
			set
			{
				this._IsARAccountSameAsMain = value;
			}
		}
		#endregion
		#region IsRemitAddressSameAsMain
		public abstract class isRemitAddressSameAsMain : PX.Data.BQL.BqlBool.Field<isRemitAddressSameAsMain> { }
		protected bool? _IsRemitAddressSameAsMain;
		[PXBool()]
		[PXUIField(DisplayName = "Same as Main")]
		public virtual bool? IsRemitAddressSameAsMain
		{
			get
			{
				return this._IsRemitAddressSameAsMain;
			}
			set
			{
				this._IsRemitAddressSameAsMain = value;
			}
		}
		#endregion
		#region IsRemitContactSameAsMain
		public abstract class isRemitContactSameAsMain : PX.Data.BQL.BqlBool.Field<isRemitContactSameAsMain> { }
		protected bool? _IsRemitContactSameAsMain;
		[PXBool()]
		[PXUIField(DisplayName = "Same as Main")]
		public virtual bool? IsRemitContactSameAsMain
		{
			get
			{
				return this._IsRemitContactSameAsMain;
			}
			set
			{
				this._IsRemitContactSameAsMain = value;
			}
		}
		#endregion

		//Customer Locaiton Properties
		#region CTaxZoneID
		public abstract class cTaxZoneID : PX.Data.BQL.BqlString.Field<cTaxZoneID> { }
		protected String _CTaxZoneID;
		[PXDBString(10, IsUnicode = true)]
		[PXUIField(DisplayName = "Tax Zone")]
		[PXDefault(PersistingCheck = PXPersistingCheck.Nothing)]
		[PXSelector(typeof(Search<TaxZone.taxZoneID>), DescriptionField = typeof(TaxZone.descr), CacheGlobal = true)]
		public virtual String CTaxZoneID
		{
			get
			{
				return this._CTaxZoneID;
			}
			set
			{
				this._CTaxZoneID = value;
			}
		}
		#endregion
		#region CTaxCalcMode
		public abstract class cTaxCalcMode : PX.Data.BQL.BqlString.Field<cTaxCalcMode> { }
		[PXDBString(1, IsFixed = true)]
		[PXDefault(TaxCalculationMode.TaxSetting, typeof(Search<CustomerClass.taxCalcMode, Where<CustomerClass.customerClassID, Equal<Current<CustomerClass.customerClassID>>>>))]
		[TaxCalculationMode.List]
		[PXUIField(DisplayName = "Tax Calculation Mode")]
		public virtual string CTaxCalcMode { get; set; }
		#endregion
		#region CAvalaraExemptionNumber
		public abstract class cAvalaraExemptionNumber : PX.Data.BQL.BqlString.Field<cAvalaraExemptionNumber> { }
		protected String _CAvalaraExemptionNumber;
		[PXDBString(30, IsUnicode = true)]
		[PXUIField(DisplayName = "Tax Exemption Number")]
		public virtual String CAvalaraExemptionNumber
		{
			get
			{
				return this._CAvalaraExemptionNumber;
			}
			set
			{
				this._CAvalaraExemptionNumber = value;
			}
		}
		#endregion
		#region CAvalaraCustomerUsageType
		public abstract class cAvalaraCustomerUsageType : PX.Data.BQL.BqlString.Field<cAvalaraCustomerUsageType> { }
		protected String _CAvalaraCustomerUsageType;
		[PXDBString(1, IsFixed = true)]
		[PXUIField(DisplayName = "Entity Usage Type")]
		[PXDefault(TXAvalaraCustomerUsageType.Default)]
		[TX.TXAvalaraCustomerUsageType.List]
		public virtual String CAvalaraCustomerUsageType
		{
			get
			{
				return this._CAvalaraCustomerUsageType;
			}
			set
			{
				this._CAvalaraCustomerUsageType = value;
			}
		}
		#endregion
		#region CCarrierID
		public abstract class cCarrierID : PX.Data.BQL.BqlString.Field<cCarrierID> { }
		protected String _CCarrierID;
		[PXDBString(15, IsUnicode = true, InputMask = ">aaaaaaaaaaaaaaa")]
		[PXUIField(DisplayName = "Ship Via")]
		[PXSelector(typeof(Search<Carrier.carrierID>),
			typeof(Carrier.carrierID), typeof(Carrier.description), typeof(Carrier.isExternal), typeof(Carrier.confirmationRequired),
			CacheGlobal = true,
			DescriptionField = typeof(Carrier.description))]
		public virtual String CCarrierID
		{
			get
			{
				return this._CCarrierID;
			}
			set
			{
				this._CCarrierID = value;
			}
		}
		#endregion
		#region CShipTermsID
		public abstract class cShipTermsID : PX.Data.BQL.BqlString.Field<cShipTermsID> { }
		protected String _CShipTermsID;
		[PXDBString(10, IsUnicode = true)]
		[PXUIField(DisplayName = "Shipping Terms")]
		[PXSelector(typeof(Search<ShipTerms.shipTermsID>),CacheGlobal=true,DescriptionField = typeof(ShipTerms.description))]
		public virtual String CShipTermsID
		{
			get
			{
				return this._CShipTermsID;
			}
			set
			{
				this._CShipTermsID = value;
			}
		}
		#endregion
		#region CShipZoneID
		public abstract class cShipZoneID : PX.Data.BQL.BqlString.Field<cShipZoneID> { }
		protected String _CShipZoneID;
		[PXDBString(15, IsUnicode = true, InputMask = ">aaaaaaaaaaaaaaa")]
		[PXUIField(DisplayName = "Shipping Zone")]
		[PXSelector(typeof(ShippingZone.zoneID), CacheGlobal = true, DescriptionField = typeof(ShippingZone.description))]
		public virtual String CShipZoneID
		{
			get
			{
				return this._CShipZoneID;
			}
			set
			{
				this._CShipZoneID = value;
			}
		}
		#endregion
		#region CFOBPointID
		public abstract class cFOBPointID : PX.Data.BQL.BqlString.Field<cFOBPointID> { }
		protected String _CFOBPointID;
		[PXDBString(15, IsUnicode = true)]
		[PXUIField(DisplayName = "FOB Point")]
		[PXSelector(typeof(FOBPoint.fOBPointID), CacheGlobal = true, DescriptionField = typeof(FOBPoint.description))]
		public virtual String CFOBPointID
		{
			get
			{
				return this._CFOBPointID;
			}
			set
			{
				this._CFOBPointID = value;
			}
		}
		#endregion
		#region CResedential
		public abstract class cResedential : PX.Data.BQL.BqlBool.Field<cResedential> { }
		protected Boolean? _CResedential;
		[PXDBBool()]
		[PXDefault(false)]
		[PXUIField(DisplayName = "Residential Delivery")]
		public virtual Boolean? CResedential
		{
			get
			{
				return this._CResedential;
			}
			set
			{
				this._CResedential = value;
			}
		}
		#endregion
		#region CSaturdayDelivery
		public abstract class cSaturdayDelivery : PX.Data.BQL.BqlBool.Field<cSaturdayDelivery> { }
		protected Boolean? _CSaturdayDelivery;
		[PXDBBool()]
		[PXDefault(false)]
		[PXUIField(DisplayName = "Saturday Delivery")]
		public virtual Boolean? CSaturdayDelivery
		{
			get
			{
				return this._CSaturdayDelivery;
			}
			set
			{
				this._CSaturdayDelivery = value;
			}
		}
		#endregion
		#region CGroundCollect
		public abstract class cGroundCollect : PX.Data.BQL.BqlBool.Field<cGroundCollect> { }
		protected Boolean? _CGroundCollect;
		[PXDBBool()]
		[PXDefault(false)]
		[PXUIField(DisplayName = "FedEx Ground Collect")]
		public virtual Boolean? CGroundCollect
		{
			get
			{
				return this._CGroundCollect;
			}
			set
			{
				this._CGroundCollect = value;
			}
		}
		#endregion
		#region CInsurance
		public abstract class cInsurance : PX.Data.BQL.BqlBool.Field<cInsurance> { }
		protected Boolean? _CInsurance;
		[PXDBBool()]
		[PXDefault(true)]
		[PXUIField(DisplayName = "Insurance")]
		public virtual Boolean? CInsurance
		{
			get
			{
				return this._CInsurance;
			}
			set
			{
				this._CInsurance = value;
			}
		}
		#endregion
		#region CLeadTime
		public abstract class cLeadTime : PX.Data.BQL.BqlShort.Field<cLeadTime> { }
		protected Int16? _CLeadTime;
		[PXDBShort(MinValue=0,MaxValue=100000)]
		[PXUIField(DisplayName = CR.Messages.LeadTimeDays)]
		public virtual Int16? CLeadTime
		{
			get
			{
				return this._CLeadTime;
			}
			set
			{
				this._CLeadTime = value;
			}
		}
		#endregion
		#region CBranchID
		public abstract class cBranchID : PX.Data.BQL.BqlInt.Field<cBranchID> { }
		protected Int32? _CBranchID;
		[Branch(useDefaulting: false, IsDetail = false, PersistingCheck = PXPersistingCheck.Nothing, DisplayName = "Shipping Branch", IsEnabledWhenOneBranchIsAccessible = true)]
		public virtual Int32? CBranchID
		{
			get
			{
				return this._CBranchID;
			}
			set
			{
				this._CBranchID = value;
			}
		}
		#endregion
		#region CSalesAcctID
		public abstract class cSalesAcctID : PX.Data.BQL.BqlInt.Field<cSalesAcctID> { }
		protected Int32? _CSalesAcctID;
		[Account(DisplayName = "Sales Account", Visibility = PXUIVisibility.Visible, DescriptionField = typeof(Account.description), Required = true, AvoidControlAccounts = true)]
		public virtual Int32? CSalesAcctID
		{
			get
			{
				return this._CSalesAcctID;
			}
			set
			{
				this._CSalesAcctID = value;
			}
		}
			  #endregion
		#region CSalesSubID
		public abstract class cSalesSubID : PX.Data.BQL.BqlInt.Field<cSalesSubID> { }
		protected Int32? _CSalesSubID;
		[SubAccount(typeof(Location.cSalesAcctID), DisplayName = "Sales Sub.", Visibility = PXUIVisibility.Visible, DescriptionField = typeof(Sub.description), Required = true)]
		public virtual Int32? CSalesSubID
		{
			get
			{
				return this._CSalesSubID;
			}
			set
			{
				this._CSalesSubID = value;
			}
		}
		#endregion
		#region CPriceClassID
		public abstract class cPriceClassID : PX.Data.BQL.BqlString.Field<cPriceClassID> { }
		protected String _CPriceClassID;
		[PXDBString(10, IsUnicode = true)]
		[PXSelector(typeof(AR.ARPriceClass.priceClassID))]
		[PXUIField(DisplayName = "Price Class", Visibility = PXUIVisibility.Visible)]
		public virtual String CPriceClassID
		{
			get
			{
				return this._CPriceClassID;
			}
			set
			{
				this._CPriceClassID = value;
			}
		}
		#endregion
		#region CSiteID
		public abstract class cSiteID : PX.Data.BQL.BqlInt.Field<cSiteID> { }
		protected Int32? _CSiteID;
		[PXDBInt()]
		[PXUIField(DisplayName = "Warehouse", Visibility = PXUIVisibility.Visible)]
		[PXDimensionSelector(SiteAttribute.DimensionName, typeof(INSite.siteID), typeof(INSite.siteCD), DescriptionField=typeof(INSite.descr))]
        [PXRestrictor(typeof(Where<INSite.active, Equal<True>>), IN.Messages.InactiveWarehouse, typeof(INSite.siteCD))]
        [PXRestrictor(typeof(Where<INSite.siteID, NotEqual<SiteAttribute.transitSiteID>>), IN.Messages.TransitSiteIsNotAvailable)]
		[PXForeignReference(typeof(Field<cSiteID>.IsRelatedTo<INSite.siteID>))]
		public virtual Int32? CSiteID
		{
			get
			{
				return this._CSiteID;
			}
			set
			{
				this._CSiteID = value;
			}
		}
		#endregion
		#region CDiscountAcctID
		public abstract class cDiscountAcctID : PX.Data.BQL.BqlInt.Field<cDiscountAcctID> { }
		protected Int32? _CDiscountAcctID;
		[Account(DisplayName = "Discount Account", Visibility = PXUIVisibility.Visible, DescriptionField = typeof(Account.description), Required=false, AvoidControlAccounts = true)]		
		public virtual Int32? CDiscountAcctID
		{
			get
			{
				return this._CDiscountAcctID;
			}
			set
			{
				this._CDiscountAcctID = value;
			}
		}
		#endregion
		#region CDiscountSubID
		public abstract class cDiscountSubID : PX.Data.BQL.BqlInt.Field<cDiscountSubID> { }
		protected Int32? _CDiscountSubID;
		[SubAccount(typeof(Location.cDiscountAcctID), DisplayName = "Discount Sub.", Visibility = PXUIVisibility.Visible, DescriptionField = typeof(Sub.description), Required = false)]		
		public virtual Int32? CDiscountSubID
		{
			get
			{
				return this._CDiscountSubID;
			}
			set
			{
				this._CDiscountSubID = value;
			}
		}
		#endregion
		#region CRetainageAcctID
		public abstract class cRetainageAcctID : PX.Data.BQL.BqlInt.Field<cRetainageAcctID> { }

		[Account(DisplayName = "Retainage Receivable Account", Visibility = PXUIVisibility.Visible, DescriptionField = typeof(Account.description), Required = false)]
		public virtual int? CRetainageAcctID
		{
			get;
			set;
		}
		#endregion
		#region CRetainageSubID
		public abstract class cRetainageSubID : PX.Data.BQL.BqlInt.Field<cRetainageSubID> { }

		[SubAccount(typeof(Location.cRetainageAcctID), DisplayName = "Retainage Receivable Sub.", Visibility = PXUIVisibility.Visible, DescriptionField = typeof(Sub.description), Required = false)]
		public virtual int? CRetainageSubID
		{
			get;
			set;
		}
		#endregion
		#region CFreightAcctID
		public abstract class cFreightAcctID : PX.Data.BQL.BqlInt.Field<cFreightAcctID> { }
		protected Int32? _CFreightAcctID;
		[Account(DisplayName = "Freight Account", Visibility = PXUIVisibility.Visible, DescriptionField = typeof(Account.description), Required = false, AvoidControlAccounts = true)]
		public virtual Int32? CFreightAcctID
		{
			get
			{
				return this._CFreightAcctID;
			}
			set
			{
				this._CFreightAcctID = value;
			}
		}
		#endregion
		#region CFreightSubID
		public abstract class cFreightSubID : PX.Data.BQL.BqlInt.Field<cFreightSubID> { }
		protected Int32? _CFreightSubID;
		[SubAccount(typeof(Location.cFreightAcctID), DisplayName = "Freight Sub.", Visibility = PXUIVisibility.Visible, DescriptionField = typeof(Sub.description), Required = false)]
		public virtual Int32? CFreightSubID
		{
			get
			{
				return this._CFreightSubID;
			}
			set
			{
				this._CFreightSubID = value;
			}
		}
		#endregion
		#region CShipComplete
		public abstract class cShipComplete : PX.Data.BQL.BqlString.Field<cShipComplete> { }
		protected String _CShipComplete;
		[PXDBString(1, IsFixed = true)]
		[PXDefault(SOShipComplete.CancelRemainder)]
		[SOShipComplete.List()]
		[PXUIField(DisplayName = "Shipping Rule")]
		public virtual String CShipComplete
		{
			get
			{
				return this._CShipComplete;
			}
			set
			{
				this._CShipComplete = value;
			}
		}
		#endregion
		#region COrderPriority
		public abstract class cOrderPriority : PX.Data.BQL.BqlShort.Field<cOrderPriority> { }
		protected Int16? _COrderPriority;
		[PXDBShort()]
		[PXDefault((short)0)]
		[PXUIField(DisplayName = "Order Priority")]
		public virtual Int16? COrderPriority
		{
			get
			{
				return this._COrderPriority;
			}
			set
			{
				this._COrderPriority = value;
			}
		}
		#endregion
		#region CCalendarID
		public abstract class cCalendarID : PX.Data.BQL.BqlString.Field<cCalendarID> { }
		protected String _CCalendarID;
		[PXDBString(10, IsUnicode = true)]
		[PXUIField(DisplayName = "Calendar", Visibility = PXUIVisibility.SelectorVisible)]
		[PXSelector(typeof(Search<CSCalendar.calendarID>), DescriptionField = typeof(CSCalendar.description))]
		public virtual String CCalendarID
		{
			get
			{
				return this._CCalendarID;
			}
			set
			{
				this._CCalendarID = value;
			}
		}
		#endregion
		#region CDefProject
		public abstract class cDefProjectID : PX.Data.BQL.BqlInt.Field<cDefProjectID> { }
		protected Int32? _CDefProjectID;
		[PM.Project(typeof(Where<PM.PMProject.customerID, Equal<Current<Location.bAccountID>>>), DisplayName = "Default Project")]
		public virtual Int32? CDefProjectID
		{
			get
			{
				return this._CDefProjectID;
			}
			set
			{
				this._CDefProjectID = value;
			}
		}
		#endregion

		#region CARAccountLocationID
		public abstract class cARAccountLocationID : PX.Data.BQL.BqlInt.Field<cARAccountLocationID> { }
		protected Int32? _CARAccountLocationID;
		[PXDBInt()]
		[PXDefault()]
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
        [Account(null, typeof(Search<Account.accountID,
                    Where2<Match<Current<AccessInfo.userName>>,
                         And<Account.active, Equal<True>,
                         And<Account.isCashAccount, Equal<False>,
                         And<Where<Current<GLSetup.ytdNetIncAccountID>, IsNull,
                          Or<Account.accountID, NotEqual<Current<GLSetup.ytdNetIncAccountID>>>>>>>>>), DisplayName = "AR Account", Required = true)]
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
		[SubAccount(typeof(Location.cARAccountID), DisplayName = "AP Sub.", DescriptionField = typeof(Sub.description), Required = true)]
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

		// Vendor Location Properties
		#region VTaxZoneID
		public abstract class vTaxZoneID : PX.Data.BQL.BqlString.Field<vTaxZoneID> { }
		protected String _VTaxZoneID;
		[PXDBString(10, IsUnicode = true)]
		[PXUIField(DisplayName = "Tax Zone")]
		[PXDefault(PersistingCheck = PXPersistingCheck.Nothing)]
		[PXSelector(typeof(Search<TaxZone.taxZoneID>), CacheGlobal = true)]
		public virtual String VTaxZoneID
		{
			get
			{
				return this._VTaxZoneID;
			}
			set
			{
				this._VTaxZoneID = value;
			}
		}
		#endregion
		#region VTaxCalcMode
		public abstract class vTaxCalcMode : PX.Data.BQL.BqlString.Field<vTaxCalcMode> { }
		[PXDBString(1, IsFixed = true)]
		[PXDefault(TaxCalculationMode.TaxSetting, typeof(Search<VendorClass.taxCalcMode, Where<VendorClass.vendorClassID, Equal<Current<Vendor.vendorClassID>>>>))]
		[TaxCalculationMode.List]
		[PXUIField(DisplayName = "Tax Calculation Mode")]
		public virtual string VTaxCalcMode { get; set; }
		#endregion
		#region VCarrierID
		public abstract class vCarrierID : PX.Data.BQL.BqlString.Field<vCarrierID> { }
		protected String _VCarrierID;
		[PXDBString(15, IsUnicode = true, InputMask = ">aaaaaaaaaaaaaaa")]
		[PXUIField(DisplayName = "Ship Via")]
		[PXSelector(typeof(Search<Carrier.carrierID>),
			typeof(Carrier.carrierID), typeof(Carrier.description), typeof(Carrier.isExternal), typeof(Carrier.confirmationRequired),
			CacheGlobal = true,
			DescriptionField = typeof(Carrier.description))]
		public virtual String VCarrierID
		{
			get
			{
				return this._VCarrierID;
			}
			set
			{
				this._VCarrierID = value;
			}
		}
		#endregion
		#region VShipTermsID
		public abstract class vShipTermsID : PX.Data.BQL.BqlString.Field<vShipTermsID> { }
		protected String _VShipTermsID;
		[PXDBString(10, IsUnicode = true)]
		[PXUIField(DisplayName = "Shipping Terms")]
		[PXSelector(typeof(Search<ShipTerms.shipTermsID>), CacheGlobal = true, DescriptionField = typeof(ShipTerms.description))]
		public virtual String VShipTermsID
		{
			get
			{
				return this._VShipTermsID;
			}
			set
			{
				this._VShipTermsID = value;
			}
		}
		#endregion
		#region VFOBPointID
		public abstract class vFOBPointID : PX.Data.BQL.BqlString.Field<vFOBPointID> { }
		protected String _VFOBPointID;
		[PXDBString(15, IsUnicode = true)]
		[PXUIField(DisplayName = "FOB Point")]
		[PXSelector(typeof(FOBPoint.fOBPointID), CacheGlobal = true, DescriptionField = typeof(FOBPoint.description))]
		public virtual String VFOBPointID
		{
			get
			{
				return this._VFOBPointID;
			}
			set
			{
				this._VFOBPointID = value;
			}
		}
		#endregion
		#region VLeadTime
		public abstract class vLeadTime : PX.Data.BQL.BqlShort.Field<vLeadTime> { }
		protected Int16? _VLeadTime;
		[PXDBShort(MinValue = 0, MaxValue = 100000)]
		[PXUIField(DisplayName = CR.Messages.LeadTimeDays)]
		public virtual Int16? VLeadTime
		{
			get
			{
				return this._VLeadTime;
			}
			set
			{
				this._VLeadTime = value;
			}
		}
		#endregion
		#region VBranchID
		public abstract class vBranchID : PX.Data.BQL.BqlInt.Field<vBranchID> { }
		protected Int32? _VBranchID;
		[Branch(useDefaulting: false, IsDetail = false, PersistingCheck = PXPersistingCheck.Nothing, DisplayName = "Receiving Branch", IsEnabledWhenOneBranchIsAccessible = true)]
		public virtual Int32? VBranchID
		{
			get
			{
				return this._VBranchID;
			}
			set
			{
				this._VBranchID = value;
			}
		}
		#endregion
		#region VExpenseAcctID
		public abstract class vExpenseAcctID : PX.Data.BQL.BqlInt.Field<vExpenseAcctID> { }
		protected Int32? _VExpenseAcctID;
		[Account(DisplayName = "Expense Account", Visibility = PXUIVisibility.Visible, DescriptionField = typeof(Account.description), AvoidControlAccounts = true)]
		public virtual Int32? VExpenseAcctID
		{
			get
			{
				return this._VExpenseAcctID;
			}
			set
			{
				this._VExpenseAcctID = value;
			}
		}
		#endregion
		#region VExpenseSubID
		public abstract class vExpenseSubID : PX.Data.BQL.BqlInt.Field<vExpenseSubID> { }
		protected Int32? _VExpenseSubID;
		[SubAccount(typeof(Location.vExpenseAcctID), DisplayName = "Expense Sub.", Visibility = PXUIVisibility.Visible, DescriptionField = typeof(Sub.description))]
		public virtual Int32? VExpenseSubID
		{
			get
			{
				return this._VExpenseSubID;
			}
			set
			{
				this._VExpenseSubID = value;
			}
		}
		#endregion
		#region VRetainageAcctID
		public abstract class vRetainageAcctID : PX.Data.BQL.BqlInt.Field<vRetainageAcctID> { }

		[Account(DisplayName = "Retainage Payable Account", Visibility = PXUIVisibility.Visible, DescriptionField = typeof(Account.description), Required = false)]
		public virtual int? VRetainageAcctID
		{
			get;
			set;
		}
		#endregion
		#region VRetainageSubID
		public abstract class vRetainageSubID : PX.Data.BQL.BqlInt.Field<vRetainageSubID> { }

		[SubAccount(typeof(Location.vRetainageAcctID), DisplayName = "Retainage Payable Sub.", Visibility = PXUIVisibility.Visible, DescriptionField = typeof(Sub.description), Required = false)]
		public virtual int? VRetainageSubID
		{
			get;
			set;
		}
		#endregion
		#region VFreightAcctID
		public abstract class vFreightAcctID : PX.Data.BQL.BqlInt.Field<vFreightAcctID> { }
		protected Int32? _VFreightAcctID;
		[Account(DisplayName = "Freight Account", Visibility = PXUIVisibility.Visible, DescriptionField = typeof(Account.description), Required = false)]
		public virtual Int32? VFreightAcctID
		{
			get
			{
				return this._VFreightAcctID;
			}
			set
			{
				this._VFreightAcctID = value;
			}
		}
		#endregion
		#region VFreightSubID
		public abstract class vFreightSubID : PX.Data.BQL.BqlInt.Field<vFreightSubID> { }
		protected Int32? _VFreightSubID;
		[SubAccount(typeof(Location.vFreightAcctID), DisplayName = "Freight Sub.", Visibility = PXUIVisibility.Visible, DescriptionField = typeof(Sub.description), Required = false)]
		public virtual Int32? VFreightSubID
		{
			get
			{
				return this._VFreightSubID;
			}
			set
			{
				this._VFreightSubID = value;
			}
		}
		#endregion		
        #region VDiscountAcctID
		public abstract class vDiscountAcctID : PX.Data.BQL.BqlInt.Field<vDiscountAcctID> { }

		[Account(DisplayName = "Discount Account", 
			Visibility = PXUIVisibility.Visible, 
			DescriptionField = typeof(Account.description), 
			Required = false,
			AvoidControlAccounts = true)]
		public virtual int? VDiscountAcctID
            {
			get;
			set;
        }
        #endregion
        #region VDiscountSubID
		public abstract class vDiscountSubID : PX.Data.BQL.BqlInt.Field<vDiscountSubID> { }

		[SubAccount(typeof(Location.vDiscountAcctID), 
			DisplayName = "Discount Sub.", 
			Visibility = PXUIVisibility.Visible, 
			DescriptionField = typeof(Sub.description), 
			Required = false)]
		public virtual int? VDiscountSubID
		{
			get;
			set;
        }
        #endregion

		#region VRcptQtyMin
		public abstract class vRcptQtyMin : PX.Data.BQL.BqlDecimal.Field<vRcptQtyMin> { }
		protected Decimal? _VRcptQtyMin;
		[PXDBDecimal(2, MinValue = 0.0, MaxValue = 999.0)]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = "Min. Receipt (%)")]
		public virtual Decimal? VRcptQtyMin
		{
			get
			{
				return this._VRcptQtyMin;
			}
			set
			{
				this._VRcptQtyMin = value;
			}
		}
		#endregion
		#region VRcptQtyMax
		public abstract class vRcptQtyMax : PX.Data.BQL.BqlDecimal.Field<vRcptQtyMax> { }
		protected Decimal? _VRcptQtyMax;
		[PXDBDecimal(2, MinValue = 0.0, MaxValue = 999.0)]
		[PXDefault(TypeCode.Decimal, "100.0")]
		[PXUIField(DisplayName = "Max. Receipt (%)")]
		public virtual Decimal? VRcptQtyMax
		{
			get
			{
				return this._VRcptQtyMax;
			}
			set
			{
				this._VRcptQtyMax = value;
			}
		}
		#endregion
		#region VRcptQtyThreshold
		public abstract class vRcptQtyThreshold : PX.Data.BQL.BqlDecimal.Field<vRcptQtyThreshold> { }
		protected Decimal? _VRcptQtyThreshold;
		[PXDBDecimal(2, MinValue = 0.0, MaxValue = 999.0)]
		[PXDefault(TypeCode.Decimal, "100.0")]
		[PXUIField(DisplayName = "Threshold Receipt (%)")]
		public virtual Decimal? VRcptQtyThreshold
		{
			get
			{
				return this._VRcptQtyThreshold;
			}
			set
			{
				this._VRcptQtyThreshold = value;
			}
		}
		#endregion
		#region VRcptQtyAction
		public abstract class vRcptQtyAction : PX.Data.BQL.BqlString.Field<vRcptQtyAction> { }
		protected String _VRcptQtyAction;
		[PXDBString(1, IsFixed = true)]
		[PXDefault(POReceiptQtyAction.AcceptButWarn)]
		[POReceiptQtyAction.List()]
		[PXUIField(DisplayName = "Receipt Action")]
		public virtual String VRcptQtyAction
		{
			get
			{
				return this._VRcptQtyAction;
			}
			set
			{
				this._VRcptQtyAction = value;
			}
		}
		#endregion
		#region VSiteID
		public abstract class vSiteID : PX.Data.BQL.BqlInt.Field<vSiteID> { }
		protected Int32? _VSiteID;
		[PXDBInt()]
		[PXUIField(DisplayName = "Warehouse", Visibility = PXUIVisibility.Visible)]
		[PXDimensionSelector(SiteAttribute.DimensionName, typeof(INSite.siteID), typeof(INSite.siteCD), DescriptionField=typeof(INSite.descr))]
        [PXRestrictor(typeof(Where<INSite.active, Equal<True>>), IN.Messages.InactiveWarehouse, typeof(INSite.siteCD))]
        [PXRestrictor(typeof(Where<INSite.siteID, NotEqual<SiteAttribute.transitSiteID>>), IN.Messages.TransitSiteIsNotAvailable)]
		[PXForeignReference(typeof(Field<vSiteID>.IsRelatedTo<INSite.siteID>))]
		public virtual Int32? VSiteID
		{
			get
			{
				return this._VSiteID;
			}
			set
			{
				this._VSiteID = value;
			}
		}
		#endregion
		#region VPrintOrder
		public abstract class vPrintOrder : PX.Data.BQL.BqlBool.Field<vPrintOrder> { }
		protected bool? _VPrintOrder;
		[PXDBBool]
		[PXDefault(false)]
		[PXUIField(DisplayName = "Print Order")]
		public virtual bool? VPrintOrder
		{
			get
			{
				return this._VPrintOrder;
			}
			set
			{
				this._VPrintOrder = value;
			}
		}
		#endregion
		#region VEmailOrder
		public abstract class vEmailOrder : PX.Data.BQL.BqlBool.Field<vEmailOrder> { }
		protected bool? _VEmailOrder;
		[PXDBBool]
		[PXDefault(false)]
		[PXUIField(DisplayName = "Email Order")]
		public virtual bool? VEmailOrder
		{
			get
			{
				return this._VEmailOrder;
			}
			set
			{
				this._VEmailOrder = value;
			}
		}
		#endregion
		#region VDefProjectID
		public abstract class vDefProjectID : PX.Data.BQL.BqlInt.Field<vDefProjectID> { }
		protected Int32? _VDefProjectID;
		[PM.Project(DisplayName = "Default Project")]
		public virtual Int32? VDefProjectID
		{
			get
			{
				return this._VDefProjectID;
			}
			set
			{
				this._VDefProjectID = value;
			}
		}
		#endregion
		#region VAPAccountLocationID
		public abstract class vAPAccountLocationID : PX.Data.BQL.BqlInt.Field<vAPAccountLocationID> { }
		protected Int32? _VAPAccountLocationID;
		[PXDBInt()]
		[PXDefault()]
		public virtual Int32? VAPAccountLocationID
		{
			get
			{
				return this._VAPAccountLocationID;
			}
			set
			{
				this._VAPAccountLocationID = value;
			}
		}
		#endregion
		#region VAPAccountID
		public abstract class vAPAccountID : PX.Data.BQL.BqlInt.Field<vAPAccountID> { }
		protected Int32? _VAPAccountID;
        [Account(null, typeof(Search<Account.accountID,
                    Where2<Match<Current<AccessInfo.userName>>,
                         And<Account.active, Equal<True>,
                         And<Where<Current<GLSetup.ytdNetIncAccountID>, IsNull,
                          Or<Account.accountID, NotEqual<Current<GLSetup.ytdNetIncAccountID>>>>>>>>), DisplayName = "AP Account", Required = true,
			ControlAccountForModule = ControlAccountModule.AP)]
        public virtual Int32? VAPAccountID
		{
			get
			{
				return this._VAPAccountID;
			}
			set
			{
				this._VAPAccountID = value;
			}
		}
		#endregion
		#region VAPSubID
		public abstract class vAPSubID : PX.Data.BQL.BqlInt.Field<vAPSubID> { }
		protected Int32? _VAPSubID;
		[SubAccount(typeof(Location.vAPAccountID), DisplayName = "AP Sub.", DescriptionField = typeof(Sub.description), Required = true)]
		public virtual Int32? VAPSubID
		{
			get
			{
				return this._VAPSubID;
			}
			set
			{
				this._VAPSubID = value;
			}
		}
		#endregion
		#region VPaymentInfoLocationID
		public abstract class vPaymentInfoLocationID : PX.Data.BQL.BqlInt.Field<vPaymentInfoLocationID> { }
		protected Int32? _VPaymentInfoLocationID;
		[PXDBInt()]
		[PXDefault()]
		public virtual Int32? VPaymentInfoLocationID
		{
			get
			{
				return this._VPaymentInfoLocationID;
			}
			set
			{
				this._VPaymentInfoLocationID = value;
			}
		}
		#endregion
		#region VRemitAddressID
		public abstract class vRemitAddressID : PX.Data.BQL.BqlInt.Field<vRemitAddressID> { }
		protected Int32? _VRemitAddressID;
		[PXDBInt()]
		[PXDBChildIdentity(typeof(Address.addressID))]
		public virtual Int32? VRemitAddressID
		{
			get
			{
				return this._VRemitAddressID;
			}
			set
			{
				this._VRemitAddressID = value;
			}
		}
		#endregion
		#region VRemitContactID
		public abstract class vRemitContactID : PX.Data.BQL.BqlInt.Field<vRemitContactID> { }
		protected Int32? _VRemitContactID;
		[PXDBInt()]
		[PXDBChildIdentity(typeof(Contact.contactID))]
		public virtual Int32? VRemitContactID
		{
			get
			{
				return this._VRemitContactID;
			}
			set
			{
				this._VRemitContactID = value;
			}
		}
		#endregion
		#region VPaymentMethodID
		public abstract class vPaymentMethodID : PX.Data.BQL.BqlString.Field<vPaymentMethodID> { }
		protected String _VPaymentMethodID;
		[PXDBString(10, IsUnicode = true)]
		[PXUIField(DisplayName = "Payment Method")]
		[PXSelector(typeof(Search<PaymentMethod.paymentMethodID,
							Where<PaymentMethod.useForAP, Equal<True>,
							And<PaymentMethod.isActive, Equal<True>>>>),					
							DescriptionField = typeof(PaymentMethod.descr))]
		[PXDefault(PersistingCheck = PXPersistingCheck.Nothing)]
		public virtual String VPaymentMethodID
		{
			get
			{
				return this._VPaymentMethodID;
			}
			set
			{
				this._VPaymentMethodID = value;
			}
		}
		#endregion
		#region VCashAccountID
		public abstract class vCashAccountID : PX.Data.BQL.BqlInt.Field<vCashAccountID> { }
		protected Int32? _VCashAccountID;
		[CashAccount(typeof(Location.vBranchID), typeof(Search2<CashAccount.cashAccountID,
						InnerJoin<PaymentMethodAccount, 
							On<PaymentMethodAccount.cashAccountID,Equal<CashAccount.cashAccountID>>>,
						Where2<Match<Current<AccessInfo.userName>>, 
							And<CashAccount.clearingAccount, Equal<False>,
							And<PaymentMethodAccount.paymentMethodID,Equal<Current<Location.vPaymentMethodID>>,
							And<PaymentMethodAccount.useForAP,Equal<True>>>>>>), 
							Visibility = PXUIVisibility.Visible)]		
		public virtual Int32? VCashAccountID
		{
			get
			{
				return this._VCashAccountID;
			}
			set
			{
				this._VCashAccountID = value;
			}
		}
		#endregion
		#region VPaymentLeadTime
		public abstract class vPaymentLeadTime : PX.Data.BQL.BqlShort.Field<vPaymentLeadTime> { }
		protected Int16? _VPaymentLeadTime;
		[PXDBShort(MinValue = -3660, MaxValue = 3660)]
		[PXDefault((short)0)]
		[PXUIField(DisplayName = "Payment Lead Time (Days)")]
		public Int16? VPaymentLeadTime
		{
			get
			{
				return this._VPaymentLeadTime;
			}
			set
			{
				this._VPaymentLeadTime = value;
			}
		}
		#endregion		
		#region VPaymentByType
		public abstract class vPaymentByType : PX.Data.BQL.BqlInt.Field<vPaymentByType> { }
		protected int? _VPaymentByType;
		[PXDBInt()]
		[PXDefault(APPaymentBy.DueDate)]
		[APPaymentBy.List]
		[PXUIField(DisplayName = "Payment By")]
		public int? VPaymentByType
		{
			get
			{
				return this._VPaymentByType;
			}
			set
			{
				this._VPaymentByType = value;
			}
		}
		#endregion
		#region VSeparateCheck
		public abstract class vSeparateCheck : PX.Data.BQL.BqlBool.Field<vSeparateCheck> { }
		protected Boolean? _VSeparateCheck;
		[PXDBBool()]
		[PXUIField(DisplayName = "Pay Separately")]
		[PXDefault(false)]
		public virtual Boolean? VSeparateCheck
		{
			get
			{
				return this._VSeparateCheck;
			}
			set
			{
				this._VSeparateCheck = value;
			}
		}
		#endregion
		#region VPrepaymentPct
		public abstract class vPrepaymentPct : Data.BQL.BqlDecimal.Field<vPrepaymentPct>
		{
		}
		[PXDBDecimal(6, MinValue = 0, MaxValue = 100)]
		[PXUIField(DisplayName = "Prepayment Percent")]
		[PXDefault(TypeCode.Decimal, "100.0")]
		public virtual decimal? VPrepaymentPct
		{
			get;
			set;
		}
		#endregion
		#region VAllowAPBillBeforeReceipt
		public abstract class vAllowAPBillBeforeReceipt : PX.Data.BQL.BqlBool.Field<vAllowAPBillBeforeReceipt> { }
		[PXDBBool]
		[PXUIField(DisplayName = "Allow AP Bill Before Receipt")]
		[PXDefault(false)]
		public virtual bool? VAllowAPBillBeforeReceipt
		{
			get;
			set;
		}
		#endregion

		//LocationAPAccountSub fields
		#region LocationAPAccountSubBAccountID
		public abstract class locationAPAccountSubBAccountID : PX.Data.BQL.BqlInt.Field<locationAPAccountSubBAccountID> { }
		protected Int32? _LocationAPAccountSubBAccountID;
		[PXDBInt(BqlField=typeof(LocationAPAccountSub.bAccountID))]
		[PXExtraKey()]
		public virtual Int32? LocationAPAccountSubBAccountID
		{
			get
			{
				return null;
			}
			set
			{
			}
		}
		#endregion
		#region APAccountID
		public abstract class aPAccountID : PX.Data.BQL.BqlInt.Field<aPAccountID> { }
		protected Int32? _APAccountID;
        [Account(null, typeof(Search<Account.accountID,
                    Where2<Match<Current<AccessInfo.userName>>,
                         And<Account.active, Equal<True>,
                         And<Account.isCashAccount, Equal<False>,
                         And<Where<Current<GLSetup.ytdNetIncAccountID>, IsNull,
                          Or<Account.accountID, NotEqual<Current<GLSetup.ytdNetIncAccountID>>>>>>>>>), DisplayName = "AP Account", BqlField = typeof(LocationAPAccountSub.vAPAccountID))]
		public virtual Int32? APAccountID
		{
			get
			{
				return this._APAccountID;
			}
			set
			{
				this._APAccountID = value;
			}
		}
		#endregion
		#region APSubID
		public abstract class aPSubID : PX.Data.BQL.BqlInt.Field<aPSubID> { }
		protected Int32? _APSubID;
		[SubAccount(typeof(Location.aPAccountID), BqlField = typeof(LocationAPAccountSub.vAPSubID), DisplayName = "AP Sub.", DescriptionField = typeof(Sub.description))]
		public virtual Int32? APSubID
		{
			get
			{
				return this._APSubID;
			}
			set
			{
				this._APSubID = value;
			}
		}
		#endregion
		#region APRetainageAcctID
		public abstract class aPRetainageAcctID : PX.Data.BQL.BqlInt.Field<aPRetainageAcctID> { }

		[Account(DisplayName = "Retainage Payable Account",
			Visibility = PXUIVisibility.Visible,
			DescriptionField = typeof(Account.description),
			BqlField = typeof(LocationAPAccountSub.vRetainageAcctID))]
		public virtual Int32? APRetainageAcctID
		{
			get;
			set;
		}
		#endregion
		#region APRetainageSubID
		public abstract class aPRetainageSubID : PX.Data.BQL.BqlInt.Field<aPRetainageSubID> { }
		[SubAccount(typeof(Location.vRetainageAcctID),
			DisplayName = "Retainage Payable Sub.",
			Visibility = PXUIVisibility.Visible,
			DescriptionField = typeof(Sub.description),
			BqlField = typeof(LocationAPAccountSub.vRetainageSubID))]
		public virtual Int32? APRetainageSubID
		{
			get;
			set;
		}
		#endregion

		//LocationARAccountSub fields
		#region LocationARAccountSubBAccountID
		public abstract class locationARAccountSubBAccountID : PX.Data.BQL.BqlInt.Field<locationARAccountSubBAccountID> { }
		protected Int32? _LocationARAccountSubBAccountID;
		[PXDBInt(BqlField = typeof(LocationARAccountSub.bAccountID))]
		[PXExtraKey()]
		public virtual Int32? LocationARAccountSubBAccountID
		{
			get
			{
				return null;
			}
			set
			{
			}
		}
		#endregion
		#region ARAccountID
		public abstract class aRAccountID : PX.Data.BQL.BqlInt.Field<aRAccountID> { }
		protected Int32? _ARAccountID;
        [Account(null, typeof(Search<Account.accountID,
                    Where2<Match<Current<AccessInfo.userName>>,
                         And<Account.active, Equal<True>,
                         And<Account.isCashAccount, Equal<False>,
                         And<Where<Current<GLSetup.ytdNetIncAccountID>, IsNull,
                          Or<Account.accountID, NotEqual<Current<GLSetup.ytdNetIncAccountID>>>>>>>>>), DisplayName = "AR Account", BqlField = typeof(LocationARAccountSub.cARAccountID))]
		public virtual Int32? ARAccountID
		{
			get
			{
				return this._ARAccountID;
			}
			set
			{
				this._ARAccountID = value;
			}
		}
		#endregion
		#region ARSubID
		public abstract class aRSubID : PX.Data.BQL.BqlInt.Field<aRSubID> { }
		protected Int32? _ARSubID;
		[SubAccount(typeof(Location.aRAccountID), BqlField = typeof(LocationARAccountSub.cARSubID), DisplayName = "AR Sub.", DescriptionField = typeof(Sub.description))]
		public virtual Int32? ARSubID
		{
			get
			{
				return this._ARSubID;
			}
			set
			{
				this._ARSubID = value;
			}
		}
		#endregion
		#region ARRetainageAcctID
		public abstract class aRRetainageAcctID : PX.Data.BQL.BqlInt.Field<aRRetainageAcctID> { }

		[Account(DisplayName = "Retainage Receivable Account", Visibility = PXUIVisibility.Visible, DescriptionField = typeof(Account.description), BqlField = typeof(LocationARAccountSub.cRetainageAcctID))]
		public virtual int? ARRetainageAcctID
		{
			get;
			set;
		}
		#endregion
		#region ARRetainageSubID
		public abstract class aRRetainageSubID : PX.Data.BQL.BqlInt.Field<aRRetainageSubID> { }

		[SubAccount(typeof(Location.aRRetainageAcctID), DisplayName = "Retainage Receivable Sub.", Visibility = PXUIVisibility.Visible, DescriptionField = typeof(Sub.description), BqlField = typeof(LocationARAccountSub.cRetainageSubID))]
		public virtual int? ARRetainageSubID
		{
			get;
			set;
		}
		#endregion
		//LocationAPPaymentInfo fields
		#region LocationAPPaymentInfoBAccountID
		public abstract class locationAPPaymentInfoBAccountID : PX.Data.BQL.BqlInt.Field<locationAPPaymentInfoBAccountID> { }
		protected Int32? _LocationAPPaymentInfoBAccountID;
		[PXDBInt(BqlField = typeof(LocationAPPaymentInfo.bAccountID))]
		[PXExtraKey()]
		public virtual Int32? LocationAPPaymentInfoBAccountID
		{
			get
			{
				return null;
			}
			set
			{
			}
		}
		#endregion
		#region RemitAddressID
		public abstract class remitAddressID : PX.Data.BQL.BqlInt.Field<remitAddressID> { }
		protected Int32? _RemitAddressID;
		[PXDBInt(BqlField = typeof(LocationAPPaymentInfo.vRemitAddressID))]
		public virtual Int32? RemitAddressID
		{
			get
			{
				return this._RemitAddressID;
			}
			set
			{
				this._RemitAddressID = value;
			}
		}
		#endregion
		#region RemitContactID
		public abstract class remitContactID : PX.Data.BQL.BqlInt.Field<remitContactID> { }
		protected Int32? _RemitContactID;
		[PXDBInt(BqlField = typeof(LocationAPPaymentInfo.vRemitContactID))]
		public virtual Int32? RemitContactID
		{
			get
			{
				return this._RemitContactID;
			}
			set
			{
				this._RemitContactID = value;
			}
		}
		#endregion
		#region PaymentMethodID
		public abstract class paymentMethodID : PX.Data.BQL.BqlString.Field<paymentMethodID> { }
		protected String _PaymentMethodID;
		[PXDBString(10, IsUnicode = true, BqlField = typeof(LocationAPPaymentInfo.vPaymentMethodID))]
		[PXUIField(DisplayName = "Payment Method")]
		public virtual String PaymentMethodID
		{
			get
			{
				return this._PaymentMethodID;
			}
			set
			{
				this._PaymentMethodID = value;
			}
		}
		#endregion
		#region CashAccountID
		public abstract class cashAccountID : PX.Data.BQL.BqlInt.Field<cashAccountID> { }
		protected Int32? _CashAccountID;
		[CashAccount(typeof(Search2<CashAccount.cashAccountID,
						InnerJoin<PaymentMethodAccount,
							On<PaymentMethodAccount.cashAccountID, Equal<CashAccount.cashAccountID>>>,
						Where2<Match<Current<AccessInfo.userName>>,
							And<CashAccount.clearingAccount, Equal<False>,
							And<PaymentMethodAccount.paymentMethodID, Equal<Current<Location.vPaymentMethodID>>,
							And<PaymentMethodAccount.useForAP, Equal<True>>>>>>), 
							BqlField = typeof(LocationAPPaymentInfo.vCashAccountID), 
							Visibility = PXUIVisibility.Visible)]
		public virtual Int32? CashAccountID
		{
			get
			{
				return this._CashAccountID;
			}
			set
			{
				this._CashAccountID = value;
			}
		}
		#endregion
		#region PaymentLeadTime
		public abstract class paymentLeadTime : PX.Data.BQL.BqlShort.Field<paymentLeadTime> { }
		protected Int16? _PaymentLeadTime;
		[PXDBShort(BqlField = typeof(LocationAPPaymentInfo.vPaymentLeadTime), MinValue = 0, MaxValue = 3660)]
		[PXUIField(DisplayName = "Payment Lead Time (Days)")]
		public Int16? PaymentLeadTime
		{
			get
			{
				return this._PaymentLeadTime;
			}
			set
			{
				this._PaymentLeadTime = value;
			}
		}
		#endregion
		#region SeparateCheck
		public abstract class separateCheck : PX.Data.BQL.BqlBool.Field<separateCheck> { }
		protected Boolean? _SeparateCheck;
		[PXDBBool(BqlField = typeof(LocationAPPaymentInfo.vSeparateCheck))]
		[PXUIField(DisplayName = "Pay Separately")]
		public virtual Boolean? SeparateCheck
		{
			get
			{
				return this._SeparateCheck;
			}
			set
			{
				this._SeparateCheck = value;
			}
		}
		#endregion
		#region PaymentByType
		public abstract class paymentByType : PX.Data.BQL.BqlInt.Field<paymentByType> { }
		protected int? _PaymentByType;
		[PXDBInt(BqlField = typeof(LocationAPPaymentInfo.vPaymentByType))]
		[PXDefault(APPaymentBy.DueDate)]
		[APPaymentBy.List]
		[PXUIField(DisplayName = "Payment By")]
		public int? PaymentByType
		{
			get
			{
				return this._PaymentByType;
			}
			set
			{
				this._PaymentByType = value;
			}
		}
		#endregion

		//BAccount fields
		#region BAccountBAccountID
		public abstract class bAccountBAccountID : PX.Data.BQL.BqlInt.Field<bAccountBAccountID> { }
		protected Int32? _BAccountBAccountID;
		//should be BAccount not BAccountR
		[PXDBInt(BqlField = typeof(BAccountR.bAccountID))]
		[PXExtraKey()]
		public virtual Int32? BAccountBAccountID
		{
			get
			{
				return null;
			}
			set
			{
			}
		}
		#endregion
		#region VDefAddressID
		public abstract class vDefAddressID : PX.Data.BQL.BqlInt.Field<vDefAddressID> { }
		protected Int32? _VDefAddressID;
		[PXDBInt(BqlField = typeof(BAccountR.defAddressID))]
		[PXDefault(typeof(Select<BAccount, Where<BAccount.bAccountID, Equal<Current<Location.bAccountID>>>>), SourceField = typeof(BAccount.defAddressID))]
		public virtual Int32? VDefAddressID
		{
			get
			{
				return this._VDefAddressID;
			}
			set
			{
				this._VDefAddressID = value;
			}
		}
		#endregion
		#region VDefContactID
		public abstract class vDefContactID : PX.Data.BQL.BqlInt.Field<vDefContactID> { }
		protected Int32? _VDefContactID;
		[PXDBInt(BqlField = typeof(BAccountR.defContactID))]
		[PXDefault(typeof(Select<BAccount, Where<BAccount.bAccountID, Equal<Current<Location.bAccountID>>>>), SourceField = typeof(BAccount.defContactID))]
		public virtual Int32? VDefContactID
		{
			get
			{
				return this._VDefContactID;
			}
			set
			{
				this._VDefContactID = value;
			}
		}
		#endregion

		//Company Location Properties
		#region CMPSalesSubID
		public abstract class cMPSalesSubID : PX.Data.BQL.BqlInt.Field<cMPSalesSubID> { }
		protected Int32? _CMPSalesSubID;
		[SubAccount(DisplayName = "Sales Sub.", Visibility = PXUIVisibility.Visible, DescriptionField = typeof(Sub.description))]
		public virtual Int32? CMPSalesSubID
		{
			get
			{
				return this._CMPSalesSubID;
			}
			set
			{
				this._CMPSalesSubID = value;
			}
		}
		#endregion
		#region CMPExpenseSubID
		public abstract class cMPExpenseSubID : PX.Data.BQL.BqlInt.Field<cMPExpenseSubID> { }
		protected Int32? _CMPExpenseSubID;
		[SubAccount(DisplayName = "Expense Sub.", Visibility = PXUIVisibility.Visible, DescriptionField = typeof(Sub.description))]
		public virtual Int32? CMPExpenseSubID
		{
			get
			{
				return this._CMPExpenseSubID;
			}
			set
			{
				this._CMPExpenseSubID = value;
			}
		}
		#endregion
		#region CMPFreightSubID
		public abstract class cMPFreightSubID : PX.Data.BQL.BqlInt.Field<cMPFreightSubID> { }
		protected Int32? _CMPFreightSubID;
		[SubAccount(DisplayName = "Freight Sub.", Visibility = PXUIVisibility.Visible, DescriptionField = typeof(Sub.description))]
		public virtual Int32? CMPFreightSubID
		{
			get
			{
				return this._CMPFreightSubID;
			}
			set
			{
				this._CMPFreightSubID = value;
			}
		}
		#endregion
		#region CMPDiscountSubID
		public abstract class cMPDiscountSubID : PX.Data.BQL.BqlInt.Field<cMPDiscountSubID> { }
		protected Int32? _CMPDiscountSubID;
		[SubAccount(DisplayName = "Discount Sub.", Visibility = PXUIVisibility.Visible, DescriptionField = typeof(Sub.description))]
		public virtual Int32? CMPDiscountSubID
		{
			get
			{
				return this._CMPDiscountSubID;
			}
			set
			{
				this._CMPDiscountSubID = value;
			}
		}
		#endregion
        #region CMPGainLossSubID
        public abstract class cMPGainLossSubID : PX.Data.BQL.BqlInt.Field<cMPGainLossSubID> { }
        protected Int32? _CMPGainLossSubID;
        [SubAccount(DisplayName = "Currency Gain/Loss Sub.", Visibility = PXUIVisibility.Visible, DescriptionField = typeof(Sub.description))]
        public virtual Int32? CMPGainLossSubID
        {
            get
            {
                return this._CMPGainLossSubID;
            }
            set
            {
                this._CMPGainLossSubID = value;
            }
        }
        #endregion
		#region CMPSiteID
		public abstract class cMPSiteID : PX.Data.BQL.BqlInt.Field<cMPSiteID> { }
		protected Int32? _CMPSiteID;
		[PXDBInt()]
		[PXUIField(DisplayName = "Warehouse", Visibility = PXUIVisibility.Visible)]
		[PXDimensionSelector(SiteAttribute.DimensionName, typeof(INSite.siteID), typeof(INSite.siteCD), DescriptionField = typeof(INSite.descr))]
        [PXRestrictor(typeof(Where<INSite.active, Equal<True>>), IN.Messages.InactiveWarehouse, typeof(INSite.siteCD))]
        public virtual Int32? CMPSiteID
		{
			get
			{
				return this._CMPSiteID;
			}
			set
			{
				this._CMPSiteID = value;
			}
		}
		#endregion

		#region tstamp
		public abstract class Tstamp : PX.Data.BQL.BqlByteArray.Field<Tstamp> { }
		protected Byte[] _tstamp;
		[PXDBTimestamp()]
		public virtual Byte[] tstamp
		{
			get
			{
				return this._tstamp;
			}
			set
			{
				this._tstamp = value;
			}
		}
		#endregion
		#region CreatedByID
		public abstract class createdByID : PX.Data.BQL.BqlGuid.Field<createdByID> { }
		protected Guid? _CreatedByID;
		[PXDBCreatedByID()]
		public virtual Guid? CreatedByID
		{
			get
			{
				return this._CreatedByID;
			}
			set
			{
				this._CreatedByID = value;
			}
		}
		#endregion
		#region CreatedByScreenID
		public abstract class createdByScreenID : PX.Data.BQL.BqlString.Field<createdByScreenID> { }
		protected String _CreatedByScreenID;
		[PXDBCreatedByScreenID()]
		public virtual String CreatedByScreenID
		{
			get
			{
				return this._CreatedByScreenID;
			}
			set
			{
				this._CreatedByScreenID = value;
			}
		}
		#endregion
		#region CreatedDateTime
		public abstract class createdDateTime : PX.Data.BQL.BqlDateTime.Field<createdDateTime> { }
		protected DateTime? _CreatedDateTime;
		[PXDBCreatedDateTime]
		[PXUIField(DisplayName = PXDBLastModifiedByIDAttribute.DisplayFieldNames.CreatedDateTime, Enabled = false, IsReadOnly = true)]
		public virtual DateTime? CreatedDateTime
		{
			get
			{
				return this._CreatedDateTime;
			}
			set
			{
				this._CreatedDateTime = value;
			}
		}
		#endregion
		#region LastModifiedByID
		public abstract class lastModifiedByID : PX.Data.BQL.BqlGuid.Field<lastModifiedByID> { }
		protected Guid? _LastModifiedByID;
		[PXDBLastModifiedByID()]
		public virtual Guid? LastModifiedByID
		{
			get
			{
				return this._LastModifiedByID;
			}
			set
			{
				this._LastModifiedByID = value;
			}
		}
		#endregion
		#region LastModifiedByScreenID
		public abstract class lastModifiedByScreenID : PX.Data.BQL.BqlString.Field<lastModifiedByScreenID> { }
		protected String _LastModifiedByScreenID;
		[PXDBLastModifiedByScreenID()]
		public virtual String LastModifiedByScreenID
		{
			get
			{
				return this._LastModifiedByScreenID;
			}
			set
			{
				this._LastModifiedByScreenID = value;
			}
		}
		#endregion
		#region LastModifiedDateTime
		public abstract class lastModifiedDateTime : PX.Data.BQL.BqlDateTime.Field<lastModifiedDateTime> { }
		protected DateTime? _LastModifiedDateTime;
		[PXDBLastModifiedDateTime]
		[PXUIField(DisplayName = PXDBLastModifiedByIDAttribute.DisplayFieldNames.LastModifiedDateTime, Enabled = false, IsReadOnly = true)]
		public virtual DateTime? LastModifiedDateTime
		{
			get
			{
				return this._LastModifiedDateTime;
			}
			set
			{
				this._LastModifiedDateTime = value;
			}
		}
		#endregion
		#region VSiteIDIsNull
		public abstract class vSiteIDIsNull : PX.Data.BQL.BqlShort.Field<vSiteIDIsNull> { }
		protected Int16? _VSiteIDIsNull;
		[PXShort()]
		[PXDBCalced(typeof(Switch<Case<Where<Location.vSiteID, IsNull>, shortMax>, short0>), typeof(short))]
		public virtual Int16? VSiteIDIsNull
		{
			get
			{
				return this._VSiteIDIsNull;
			}
			set
			{
				this._VSiteIDIsNull = value;
			}
		}
		#endregion

		#region IsAddressSameAsMain
		public abstract class isAddressSameAsMain : PX.Data.BQL.BqlBool.Field<isAddressSameAsMain> { }
		protected bool? _IsAddressSameAsMain;
		[PXBool()]
		[PXUIField(DisplayName = "Same as Main")]
		public virtual bool? IsAddressSameAsMain
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
		public abstract class isContactSameAsMain : PX.Data.BQL.BqlBool.Field<isContactSameAsMain> { }
		protected bool? _IsContactSameAsMain;
		[PXBool()]
		[PXUIField(DisplayName = "Same as Main")]
		public virtual bool? IsContactSameAsMain
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

		public static string GetKeyImage(int? baccountID, int? locationID)
		{
			return String.Format("{0}:{1}, {2}:{3}", typeof(Location.bAccountID).Name, baccountID,
																typeof(Location.locationID).Name, locationID);
		}

		public string GetKeyImage()
		{
			return GetKeyImage(BAccountID, LocationID);
		}

		public static string GetImage(int? baccountID, int? locationID)
		{
			return string.Format("{0}[{1}]",
								EntityHelper.GetFriendlyEntityName(typeof(Location)),
								GetKeyImage(baccountID, locationID));
		}

		public override string ToString()
		{
			return GetImage(BAccountID, LocationID);
		}
	}
	#region LocType Attribute
	public class LocTypeList
	{
		public class ListAttribute : PXStringListAttribute
		{
			public ListAttribute()
				: base(
				new string[] { CompanyLoc, VendorLoc, CustomerLoc, CombinedLoc, EmployeeLoc },
				new string[] { Messages.CompanyLoc, Messages.VendorLoc, Messages.CustomerLoc, Messages.CombinedLoc, Messages.EmployeeLoc }) { ; }
		}

		public const string CompanyLoc  = "CP";
		public const string VendorLoc   = "VE";
		public const string CustomerLoc = "CU";
		public const string CombinedLoc = "VC";
		public const string EmployeeLoc = "EP";

		public class companyLoc : PX.Data.BQL.BqlString.Constant<companyLoc>
		{
			public companyLoc() : base(CompanyLoc) { ;}
		}

		public class vendorLoc : PX.Data.BQL.BqlString.Constant<vendorLoc>
		{
			public vendorLoc() : base(VendorLoc) { ;}
		}

		public class customerLoc : PX.Data.BQL.BqlString.Constant<customerLoc>
		{
			public customerLoc() : base(CustomerLoc) { ;}
		}

		public class combinedLoc : PX.Data.BQL.BqlString.Constant<combinedLoc>
		{
			public combinedLoc() : base(CombinedLoc) { ;}
		}

		public class employeeLoc : PX.Data.BQL.BqlString.Constant<employeeLoc>
		{
			public employeeLoc() : base(EmployeeLoc) { ;}
		}
	}
	#endregion

	public interface ILocation
	{
		int? LocationID { get; set; }
		bool? IsActive { get; set; }

		int? CARAccountID { get; set; }
		int? CARSubID { get; set; }
		int? CSalesAcctID { get; set; }
		int? CSalesSubID { get; set; }
		bool? IsARAccountSameAsMain { get; set; }

		int? VAPAccountID { get; set; }
		int? VAPSubID { get; set; }
		int? VExpenseAcctID { get; set; }
		int? VExpenseSubID { get; set; }
		bool? IsAPAccountSameAsMain { get; set; }
		int? CDiscountAcctID { get; set; }
		int? CDiscountSubID { get; set; }
		int? CFreightAcctID { get; set; }
		int? CFreightSubID { get; set; }
	}
}

namespace PX.Objects.CR.Standalone
{
	using PX.Objects.CR;

	[Serializable()]
	public partial class Location : PX.Data.IBqlTable
	{
		#region BAccountID
		public abstract class bAccountID : PX.Data.BQL.BqlInt.Field<bAccountID> { }
		protected Int32? _BAccountID;
		[PXDBInt(IsKey = true)]
		[PXReferentialIntegrityCheck]
		public virtual Int32? BAccountID
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
		protected Int32? _LocationID;
		[PXDBIdentity()]
		public virtual Int32? LocationID
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
		#region LocationCD
		public abstract class locationCD : PX.Data.BQL.BqlString.Field<locationCD> { }
		protected String _LocationCD;
		[PXDBString(IsKey = true, IsUnicode = true)]
		public virtual String LocationCD
		{
			get
			{
				return this._LocationCD;
			}
			set
			{
				this._LocationCD = value;
			}
		}
		#endregion
		#region LocType
		public abstract class locType : PX.Data.BQL.BqlString.Field<locType> { }
		protected String _LocType;
		[PXDBString(2, IsFixed = true)]
		public virtual String LocType
		{
			get
			{
				return this._LocType;
			}
			set
			{
				this._LocType = value;
			}
		}
		#endregion
		#region Descr
		public abstract class descr : PX.Data.BQL.BqlString.Field<descr> { }
		protected String _Descr;
		[PXDBString(60, IsUnicode = true)]
		public virtual String Descr
		{
			get
			{
				return this._Descr;
			}
			set
			{
				this._Descr = value;
			}
		}
		#endregion
		#region TaxRegistrationID
		public abstract class taxRegistrationID : PX.Data.BQL.BqlString.Field<taxRegistrationID> { }
		protected String _TaxRegistrationID;
		[PXDBString(50, IsUnicode = true)]
		public virtual String TaxRegistrationID
		{
			get
			{
				return this._TaxRegistrationID;
			}
			set
			{
				this._TaxRegistrationID = value;
			}
		}
		#endregion
		#region DefAddressID
		public abstract class defAddressID : PX.Data.BQL.BqlInt.Field<defAddressID> { }
		protected Int32? _DefAddressID;
		[PXDBInt()]
		public virtual Int32? DefAddressID
		{
			get
			{
				return this._DefAddressID;
			}
			set
			{
				this._DefAddressID = value;
			}
		}
		#endregion
		#region DefContactID
		public abstract class defContactID : PX.Data.BQL.BqlInt.Field<defContactID> { }
		protected Int32? _DefContactID;
		[PXDBInt()]
		public virtual Int32? DefContactID
		{
			get
			{
				return this._DefContactID;
			}
			set
			{
				this._DefContactID = value;
			}
		}
		#endregion
		#region NoteID
		public abstract class noteID : PX.Data.BQL.BqlGuid.Field<noteID> { }
		protected Guid? _NoteID;
		[PXNote()]
		public virtual Guid? NoteID
		{
			get
			{
				return this._NoteID;
			}
			set
			{
				this._NoteID = value;
			}
		}
		#endregion
		#region IsActive
		public abstract class isActive : PX.Data.BQL.BqlBool.Field<isActive> { }
		protected bool? _IsActive;
		[PXDBBool()]
		public virtual bool? IsActive
		{
			get
			{
				return this._IsActive;
			}
			set
			{
				this._IsActive = value;
			}
		}
		#endregion

		//Customer Location Properties
		#region CTaxZoneID
		public abstract class cTaxZoneID : PX.Data.BQL.BqlString.Field<cTaxZoneID> { }
		protected String _CTaxZoneID;
		[PXDBString(10, IsUnicode = true)]
		public virtual String CTaxZoneID
		{
			get
			{
				return this._CTaxZoneID;
			}
			set
			{
				this._CTaxZoneID = value;
			}
		}
		#endregion
		#region CTaxCalcMode
		public abstract class cTaxCalcMode : PX.Data.BQL.BqlString.Field<cTaxCalcMode> { }
		[PXDBString(1, IsFixed = true)]
		[PXDefault(TaxCalculationMode.TaxSetting, typeof(Search<CustomerClass.taxCalcMode, Where<CustomerClass.customerClassID, Equal<Current<CustomerClass.customerClassID>>>>))]
		[TaxCalculationMode.List]
		[PXUIField(DisplayName = "Tax Calculation Mode")]
		public virtual string CTaxCalcMode { get; set; }
		#endregion
		#region CAvalaraExemptionNumber
		public abstract class cAvalaraExemptionNumber : PX.Data.BQL.BqlString.Field<cAvalaraExemptionNumber> { }
		protected String _CAvalaraExemptionNumber;
		[PXDBString(30, IsUnicode = true)]
		[PXUIField(DisplayName = "Tax Exemption Number")]
		public virtual String CAvalaraExemptionNumber
		{
			get
			{
				return this._CAvalaraExemptionNumber;
			}
			set
			{
				this._CAvalaraExemptionNumber = value;
			}
		}
		#endregion
		#region CAvalaraCustomerUsageType
		public abstract class cAvalaraCustomerUsageType : PX.Data.BQL.BqlString.Field<cAvalaraCustomerUsageType> { }
		protected String _CAvalaraCustomerUsageType;
		[PXDBString(1, IsFixed = true)]
		[PXUIField(DisplayName = "Entity Usage Type")]
		[PXDefault(TXAvalaraCustomerUsageType.Default)]
		[TX.TXAvalaraCustomerUsageType.List]
		public virtual String CAvalaraCustomerUsageType
		{
			get
			{
				return this._CAvalaraCustomerUsageType;
			}
			set
			{
				this._CAvalaraCustomerUsageType = value;
			}
		}
		#endregion
		#region CCarrierID
		public abstract class cCarrierID : PX.Data.BQL.BqlString.Field<cCarrierID> { }
		protected String _CCarrierID;
		[PXDBString(15, IsUnicode = true)]
		public virtual String CCarrierID
		{
			get
			{
				return this._CCarrierID;
			}
			set
			{
				this._CCarrierID = value;
			}
		}
		#endregion
		#region CShipTermsID
		public abstract class cShipTermsID : PX.Data.BQL.BqlString.Field<cShipTermsID> { }
		protected String _CShipTermsID;
		[PXDBString(10, IsUnicode = true)]
		public virtual String CShipTermsID
		{
			get
			{
				return this._CShipTermsID;
			}
			set
			{
				this._CShipTermsID = value;
			}
		}
		#endregion
		#region CShipZoneID
		public abstract class cShipZoneID : PX.Data.BQL.BqlString.Field<cShipZoneID> { }
		protected String _CShipZoneID;
		[PXDBString(15, IsUnicode = true)]
		public virtual String CShipZoneID
		{
			get
			{
				return this._CShipZoneID;
			}
			set
			{
				this._CShipZoneID = value;
			}
		}
		#endregion
		#region CFOBPointID
		public abstract class cFOBPointID : PX.Data.BQL.BqlString.Field<cFOBPointID> { }
		protected String _CFOBPointID;
		[PXDBString(15, IsUnicode = true)]
		public virtual String CFOBPointID
		{
			get
			{
				return this._CFOBPointID;
			}
			set
			{
				this._CFOBPointID = value;
			}
		}
		#endregion
		#region CResedential
		public abstract class cResedential : PX.Data.BQL.BqlBool.Field<cResedential> { }
		protected Boolean? _CResedential;
		[PXDBBool()]
		[PXDefault(false)]
		[PXUIField(DisplayName = "Residential Delivery")]
		public virtual Boolean? CResedential
		{
			get
			{
				return this._CResedential;
			}
			set
			{
				this._CResedential = value;
			}
		}
		#endregion
		#region CSaturdayDelivery
		public abstract class cSaturdayDelivery : PX.Data.BQL.BqlBool.Field<cSaturdayDelivery> { }
		protected Boolean? _CSaturdayDelivery;
		[PXDBBool()]
		[PXDefault(false)]
		[PXUIField(DisplayName = "Saturday Delivery")]
		public virtual Boolean? CSaturdayDelivery
		{
			get
			{
				return this._CSaturdayDelivery;
			}
			set
			{
				this._CSaturdayDelivery = value;
			}
		}
		#endregion
		#region CGroundCollect
		public abstract class cGroundCollect : PX.Data.BQL.BqlBool.Field<cGroundCollect> { }
		protected Boolean? _CGroundCollect;
		[PXDBBool()]
		[PXDefault(false)]
		[PXUIField(DisplayName = "Ground Collect")]
		public virtual Boolean? CGroundCollect
		{
			get
			{
				return this._CGroundCollect;
			}
			set
			{
				this._CGroundCollect = value;
			}
		}
		#endregion
		#region CInsurance
		public abstract class cInsurance : PX.Data.BQL.BqlBool.Field<cInsurance> { }
		protected Boolean? _CInsurance;
		[PXDBBool()]
		[PXDefault(true)]
		[PXUIField(DisplayName = "Insurance")]
		public virtual Boolean? CInsurance
		{
			get
			{
				return this._CInsurance;
			}
			set
			{
				this._CInsurance = value;
			}
		}
		#endregion
		#region CLeadTime
		public abstract class cLeadTime : PX.Data.BQL.BqlShort.Field<cLeadTime> { }
		protected Int16? _CLeadTime;
		[PXDBShort(MinValue = 0, MaxValue = 100000)]
		public virtual Int16? CLeadTime
		{
			get
			{
				return this._CLeadTime;
			}
			set
			{
				this._CLeadTime = value;
			}
		}
		#endregion
		#region CBranchID
		public abstract class cBranchID : PX.Data.BQL.BqlInt.Field<cBranchID> { }
		protected Int32? _CBranchID;
		[PXDBInt()]
		public virtual Int32? CBranchID
		{
			get
			{
				return this._CBranchID;
			}
			set
			{
				this._CBranchID = value;
			}
		}
		#endregion
		#region CSalesAcctID
		public abstract class cSalesAcctID : PX.Data.BQL.BqlInt.Field<cSalesAcctID> { }
		protected Int32? _CSalesAcctID;
		[PXDBInt()]
		public virtual Int32? CSalesAcctID
		{
			get
			{
				return this._CSalesAcctID;
			}
			set
			{
				this._CSalesAcctID = value;
			}
		}
		#endregion
		#region CSalesSubID
		public abstract class cSalesSubID : PX.Data.BQL.BqlInt.Field<cSalesSubID> { }
		protected Int32? _CSalesSubID;
		[PXDBInt()]
		public virtual Int32? CSalesSubID
		{
			get
			{
				return this._CSalesSubID;
			}
			set
			{
				this._CSalesSubID = value;
			}
		}
		#endregion
		#region CPriceClassID
		public abstract class cPriceClassID : PX.Data.BQL.BqlString.Field<cPriceClassID> { }
		protected String _CPriceClassID;
		[PXDBString(10, IsUnicode = true)]
		public virtual String CPriceClassID
		{
			get
			{
				return this._CPriceClassID;
			}
			set
			{
				this._CPriceClassID = value;
			}
		}
		#endregion
		#region CSiteID
		public abstract class cSiteID : PX.Data.BQL.BqlInt.Field<cSiteID> { }
		protected Int32? _CSiteID;
		[PXDBInt()]
		public virtual Int32? CSiteID
		{
			get
			{
				return this._CSiteID;
			}
			set
			{
				this._CSiteID = value;
			}
		}
		#endregion
		#region CDiscountAcctID
		public abstract class cDiscountAcctID : PX.Data.BQL.BqlInt.Field<cDiscountAcctID> { }
		protected Int32? _CDiscountAcctID;
		[PXDBInt()]
		public virtual Int32? CDiscountAcctID
		{
			get
			{
				return this._CDiscountAcctID;
			}
			set
			{
				this._CDiscountAcctID = value;
			}
		}
		#endregion
		#region CDiscountSubID
		public abstract class cDiscountSubID : PX.Data.BQL.BqlInt.Field<cDiscountSubID> { }
		protected Int32? _CDiscountSubID;
		[PXDBInt()]
		public virtual Int32? CDiscountSubID
		{
			get
			{
				return this._CDiscountSubID;
			}
			set
			{
				this._CDiscountSubID = value;
			}
		}
		#endregion
		#region CRetainageAcctID
		public abstract class cRetainageAcctID : PX.Data.BQL.BqlInt.Field<cRetainageAcctID> { }

		[PXDBInt()]
		public virtual int? CRetainageAcctID
		{
			get;
			set;
		}
		#endregion
		#region CRetainageSubID
		public abstract class cRetainageSubID : PX.Data.BQL.BqlInt.Field<cRetainageSubID> { }

		[PXDBInt()]
		public virtual int? CRetainageSubID
		{
			get;
			set;
		}
		#endregion
		#region CFreightAcctID
		public abstract class cFreightAcctID : PX.Data.BQL.BqlInt.Field<cFreightAcctID> { }
		protected Int32? _CFreightAcctID;
		[PXDBInt()]
		public virtual Int32? CFreightAcctID
		{
			get
			{
				return this._CFreightAcctID;
			}
			set
			{
				this._CFreightAcctID = value;
			}
		}
		#endregion
		#region CFreightSubID
		public abstract class cFreightSubID : PX.Data.BQL.BqlInt.Field<cFreightSubID> { }
		protected Int32? _CFreightSubID;
		[PXDBInt()]
		public virtual Int32? CFreightSubID
		{
			get
			{
				return this._CFreightSubID;
			}
			set
			{
				this._CFreightSubID = value;
			}
		}
		#endregion
		#region CShipComplete
		public abstract class cShipComplete : PX.Data.BQL.BqlString.Field<cShipComplete> { }
		protected String _CShipComplete;
		[PXDBString(1, IsFixed = true)]
		public virtual String CShipComplete
		{
			get
			{
				return this._CShipComplete;
			}
			set
			{
				this._CShipComplete = value;
			}
		}
		#endregion
		#region COrderPriority
		public abstract class cOrderPriority : PX.Data.BQL.BqlShort.Field<cOrderPriority> { }
		protected Int16? _COrderPriority;
		[PXDBShort()]
		public virtual Int16? COrderPriority
		{
			get
			{
				return this._COrderPriority;
			}
			set
			{
				this._COrderPriority = value;
			}
		}
		#endregion
		#region CCalendarID
		public abstract class cCalendarID : PX.Data.BQL.BqlString.Field<cCalendarID> { }
		protected String _CCalendarID;
		[PXDBString(10, IsUnicode = true)]
		public virtual String CCalendarID
		{
			get
			{
				return this._CCalendarID;
			}
			set
			{
				this._CCalendarID = value;
			}
		}
		#endregion
		#region CARAccountLocationID
		public abstract class cARAccountLocationID : PX.Data.BQL.BqlInt.Field<cARAccountLocationID> { }
		protected Int32? _CARAccountLocationID;
		[PXDBInt()]
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
		[PXDBInt()]
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
		[PXDBInt()]
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

		// Vendor Location Properties
		#region VTaxZoneID
		public abstract class vTaxZoneID : PX.Data.BQL.BqlString.Field<vTaxZoneID> { }
		protected String _VTaxZoneID;
		[PXDBString(10, IsUnicode = true)]
		public virtual String VTaxZoneID
		{
			get
			{
				return this._VTaxZoneID;
			}
			set
			{
				this._VTaxZoneID = value;
			}
		}
		#endregion
		#region VTaxCalcMode
		public abstract class vTaxCalcMode : PX.Data.BQL.BqlString.Field<vTaxCalcMode> { }
		[PXDBString(1, IsFixed = true)]
		[PXDefault(TaxCalculationMode.TaxSetting, typeof(Search<VendorClass.taxCalcMode, Where<VendorClass.vendorClassID, Equal<Current<Vendor.vendorClassID>>>>))]
		[TaxCalculationMode.List]
		[PXUIField(DisplayName = "Tax Calculation Mode")]
		public virtual string VTaxCalcMode { get; set; }
		#endregion
		#region VCarrierID
		public abstract class vCarrierID : PX.Data.BQL.BqlString.Field<vCarrierID> { }
		protected String _VCarrierID;
		[PXUIField(DisplayName = "Ship Via")]
		[PXDBString(15, IsUnicode = true, InputMask = ">aaaaaaaaaaaaaaa")]
		public virtual String VCarrierID
		{
			get
			{
				return this._VCarrierID;
			}
			set
			{
				this._VCarrierID = value;
			}
		}
		#endregion
		#region VShipTermsID
		public abstract class vShipTermsID : PX.Data.BQL.BqlString.Field<vShipTermsID> { }
		protected String _VShipTermsID;
		[PXDBString(10, IsUnicode = true)]
		[PXUIField(DisplayName = "Shipping Terms")]
		public virtual String VShipTermsID
		{
			get
			{
				return this._VShipTermsID;
			}
			set
			{
				this._VShipTermsID = value;
			}
		}
		#endregion
		#region VFOBPointID
		public abstract class vFOBPointID : PX.Data.BQL.BqlString.Field<vFOBPointID> { }
		protected String _VFOBPointID;
		[PXDBString(15, IsUnicode = true)]
		public virtual String VFOBPointID
		{
			get
			{
				return this._VFOBPointID;
			}
			set
			{
				this._VFOBPointID = value;
			}
		}
		#endregion
		#region VLeadTime
		public abstract class vLeadTime : PX.Data.BQL.BqlShort.Field<vLeadTime> { }
		protected Int16? _VLeadTime;
        [PXDBShort(MinValue = 0, MaxValue = 100000)]
        [PXUIField(DisplayName = CR.Messages.LeadTimeDays)]
        public virtual Int16? VLeadTime
		{
			get
			{
				return this._VLeadTime;
			}
			set
			{
				this._VLeadTime = value;
			}
		}
		#endregion
		#region VBranchID
		public abstract class vBranchID : PX.Data.BQL.BqlInt.Field<vBranchID> { }
		protected Int32? _VBranchID;
        [Branch(useDefaulting: false, IsDetail = false, PersistingCheck = PXPersistingCheck.Nothing, DisplayName = "Receiving Branch")]
        public virtual Int32? VBranchID
		{
			get
			{
				return this._VBranchID;
			}
			set
			{
				this._VBranchID = value;
			}
		}
		#endregion
		#region VExpenseAcctID
		public abstract class vExpenseAcctID : PX.Data.BQL.BqlInt.Field<vExpenseAcctID> { }
		protected Int32? _VExpenseAcctID;
		[PXDBInt()]
		public virtual Int32? VExpenseAcctID
		{
			get
			{
				return this._VExpenseAcctID;
			}
			set
			{
				this._VExpenseAcctID = value;
			}
		}
		#endregion
		#region VExpenseSubID
		public abstract class vExpenseSubID : PX.Data.BQL.BqlInt.Field<vExpenseSubID> { }
		protected Int32? _VExpenseSubID;
		[PXDBInt()]
		public virtual Int32? VExpenseSubID
		{
			get
			{
				return this._VExpenseSubID;
			}
			set
			{
				this._VExpenseSubID = value;
			}
		}
		#endregion
		#region VRetainageAcctID
		public abstract class vRetainageAcctID : PX.Data.BQL.BqlInt.Field<vRetainageAcctID> { }

		[PXDBInt()]
		public virtual int? VRetainageAcctID
		{
			get;
			set;
		}
		#endregion
		#region VRetainageSubID
		public abstract class vRetainageSubID : PX.Data.BQL.BqlInt.Field<vRetainageSubID> { }

		[PXDBInt()]
		public virtual int? VRetainageSubID
		{
			get;
			set;
		}
		#endregion
		#region VFreightAcctID
		public abstract class vFreightAcctID : PX.Data.BQL.BqlInt.Field<vFreightAcctID> { }
		protected Int32? _VFreightAcctID;
		[PXDBInt()]
		public virtual Int32? VFreightAcctID
		{
			get
			{
				return this._VFreightAcctID;
			}
			set
			{
				this._VFreightAcctID = value;
			}
		}
		#endregion
		#region VFreightSubID
		public abstract class vFreightSubID : PX.Data.BQL.BqlInt.Field<vFreightSubID> { }
		protected Int32? _VFreightSubID;
		[PXDBInt()]
		public virtual Int32? VFreightSubID
		{
			get
			{
				return this._VFreightSubID;
			}
			set
			{
				this._VFreightSubID = value;
			}
		}
		#endregion
        #region VDiscountAcctID
        public abstract class vDiscountAcctID : PX.Data.BQL.BqlInt.Field<vDiscountAcctID> { }
        protected Int32? _VDiscountAcctID;
        [PXDBInt()]
        public virtual Int32? VDiscountAcctID
        {
            get
            {
                return this._VDiscountAcctID;
            }
            set
            {
                this._VDiscountAcctID = value;
            }
        }
        #endregion
        #region VDiscountSubID
        public abstract class vDiscountSubID : PX.Data.BQL.BqlInt.Field<vDiscountSubID> { }
        protected Int32? _VDiscountSubID;
        [PXDBInt()]
        public virtual Int32? VDiscountSubID
        {
            get
            {
                return this._VDiscountSubID;
            }
            set
            {
                this._VDiscountSubID = value;
            }
        }
		#endregion

		#region VRcptQtyMin
		public abstract class vRcptQtyMin : PX.Data.BQL.BqlDecimal.Field<vRcptQtyMin> { }
		protected Decimal? _VRcptQtyMin;
		[PXDBDecimal(2, MinValue = 0.0, MaxValue = 999.0)]
		public virtual Decimal? VRcptQtyMin
		{
			get
			{
				return this._VRcptQtyMin;
			}
			set
			{
				this._VRcptQtyMin = value;
			}
		}
		#endregion
		#region VRcptQtyMax
		public abstract class vRcptQtyMax : PX.Data.BQL.BqlDecimal.Field<vRcptQtyMax> { }
		protected Decimal? _VRcptQtyMax;
		[PXDBDecimal(2, MinValue = 0.0, MaxValue = 999.0)]
		public virtual Decimal? VRcptQtyMax
		{
			get
			{
				return this._VRcptQtyMax;
			}
			set
			{
				this._VRcptQtyMax = value;
			}
		}
		#endregion
		#region VRcptQtyThreshold
		public abstract class vRcptQtyThreshold : PX.Data.BQL.BqlDecimal.Field<vRcptQtyThreshold> { }
		protected Decimal? _VRcptQtyThreshold;
		[PXDBDecimal(2, MinValue = 0.0, MaxValue = 999.0)]
		public virtual Decimal? VRcptQtyThreshold
		{
			get
			{
				return this._VRcptQtyThreshold;
			}
			set
			{
				this._VRcptQtyThreshold = value;
			}
		}
		#endregion
		#region VRcptQtyAction
		public abstract class vRcptQtyAction : PX.Data.BQL.BqlString.Field<vRcptQtyAction> { }
		protected String _VRcptQtyAction;
		[PXDBString(1, IsFixed = true)]
		public virtual String VRcptQtyAction
		{
			get
			{
				return this._VRcptQtyAction;
			}
			set
			{
				this._VRcptQtyAction = value;
			}
		}
		#endregion
		#region VSiteID
		public abstract class vSiteID : PX.Data.BQL.BqlInt.Field<vSiteID> { }
		protected Int32? _VSiteID;
        [PXDBInt()]
        [PXUIField(DisplayName = "Warehouse", Visibility = PXUIVisibility.Visible, FieldClass = SiteAttribute.DimensionName)]
        [PXDimensionSelector(SiteAttribute.DimensionName, typeof(INSite.siteID), typeof(INSite.siteCD), DescriptionField = typeof(INSite.descr))]
        [PXRestrictor(typeof(Where<INSite.active, Equal<True>>), IN.Messages.InactiveWarehouse, typeof(INSite.siteCD))]
        [PXRestrictor(typeof(Where<INSite.siteID, NotEqual<SiteAttribute.transitSiteID>>), IN.Messages.TransitSiteIsNotAvailable)]
        public virtual Int32? VSiteID
		{
			get
			{
				return this._VSiteID;
			}
			set
			{
				this._VSiteID = value;
			}
		}
		#endregion
		#region VPrintOrder
		public abstract class vPrintOrder : PX.Data.BQL.BqlBool.Field<vPrintOrder> { }
		protected bool? _VPrintOrder;
		[PXDBBool]
		public virtual bool? VPrintOrder
		{
			get
			{
				return this._VPrintOrder;
			}
			set
			{
				this._VPrintOrder = value;
			}
		}
		#endregion
		#region VEmailOrder
		public abstract class vEmailOrder : PX.Data.BQL.BqlBool.Field<vEmailOrder> { }
		protected bool? _VEmailOrder;
		[PXDBBool]
		public virtual bool? VEmailOrder
		{
			get
			{
				return this._VEmailOrder;
			}
			set
			{
				this._VEmailOrder = value;
			}
		}
		#endregion
		#region VAPAccountLocationID
		public abstract class vAPAccountLocationID : PX.Data.BQL.BqlInt.Field<vAPAccountLocationID> { }
		protected Int32? _VAPAccountLocationID;
		[PXDBInt()]
		public virtual Int32? VAPAccountLocationID
		{
			get
			{
				return this._VAPAccountLocationID;
			}
			set
			{
				this._VAPAccountLocationID = value;
			}
		}
		#endregion
		#region VAPAccountID
		public abstract class vAPAccountID : PX.Data.BQL.BqlInt.Field<vAPAccountID> { }
		protected Int32? _VAPAccountID;
		[PXDBInt()]
		public virtual Int32? VAPAccountID
		{
			get
			{
				return this._VAPAccountID;
			}
			set
			{
				this._VAPAccountID = value;
			}
		}
		#endregion
		#region VAPSubID
		public abstract class vAPSubID : PX.Data.BQL.BqlInt.Field<vAPSubID> { }
		protected Int32? _VAPSubID;
		[PXDBInt()]
		public virtual Int32? VAPSubID
		{
			get
			{
				return this._VAPSubID;
			}
			set
			{
				this._VAPSubID = value;
			}
		}
		#endregion
		#region VPaymentInfoLocationID
		public abstract class vPaymentInfoLocationID : PX.Data.BQL.BqlInt.Field<vPaymentInfoLocationID> { }
		protected Int32? _VPaymentInfoLocationID;
		[PXDBInt()]
		public virtual Int32? VPaymentInfoLocationID
		{
			get
			{
				return this._VPaymentInfoLocationID;
			}
			set
			{
				this._VPaymentInfoLocationID = value;
			}
		}
		#endregion
		#region VRemitAddressID
		public abstract class vRemitAddressID : PX.Data.BQL.BqlInt.Field<vRemitAddressID> { }
		protected Int32? _VRemitAddressID;
		[PXDBInt()]
		public virtual Int32? VRemitAddressID
		{
			get
			{
				return this._VRemitAddressID;
			}
			set
			{
				this._VRemitAddressID = value;
			}
		}
		#endregion
		#region VRemitContactID
		public abstract class vRemitContactID : PX.Data.BQL.BqlInt.Field<vRemitContactID> { }
		protected Int32? _VRemitContactID;
		[PXDBInt()]
		public virtual Int32? VRemitContactID
		{
			get
			{
				return this._VRemitContactID;
			}
			set
			{
				this._VRemitContactID = value;
			}
		}
		#endregion
		#region VPaymentMethodID
		public abstract class vPaymentMethodID : PX.Data.BQL.BqlString.Field<vPaymentMethodID> { }
		protected String _VPaymentMethodID;
		[PXDBString(10, IsUnicode = true)]
		public virtual String VPaymentMethodID
		{
			get
			{
				return this._VPaymentMethodID;
			}
			set
			{
				this._VPaymentMethodID = value;
			}
		}
		#endregion
		#region VCashAccountID
		public abstract class vCashAccountID : PX.Data.BQL.BqlInt.Field<vCashAccountID> { }
		protected Int32? _VCashAccountID;
		[PXDBInt()]
		public virtual Int32? VCashAccountID
		{
			get
			{
				return this._VCashAccountID;
			}
			set
			{
				this._VCashAccountID = value;
			}
		}
		#endregion
		#region VPaymentByType
		public abstract class vPaymentByType : PX.Data.BQL.BqlInt.Field<vPaymentByType> { }
		protected int? _VPaymentByType;
		[PXDBInt()]
		[PXDefault(APPaymentBy.DueDate)]
		[APPaymentBy.List]
		public int? VPaymentByType
		{
			get
			{
				return this._VPaymentByType;
			}
			set
			{
				this._VPaymentByType = value;
			}
		}
		#endregion
		#region VPaymentLeadTime
		public abstract class vPaymentLeadTime : PX.Data.BQL.BqlShort.Field<vPaymentLeadTime> { }
		protected Int16? _VPaymentLeadTime;
		[PXDBShort(MinValue = 0, MaxValue = 3660)]
		public Int16? VPaymentLeadTime
		{
			get
			{
				return this._VPaymentLeadTime;
			}
			set
			{
				this._VPaymentLeadTime = value;
			}
		}
		#endregion
		#region VSeparateCheck
		public abstract class vSeparateCheck : PX.Data.BQL.BqlBool.Field<vSeparateCheck> { }
		protected Boolean? _VSeparateCheck;
		[PXDBBool()]
		public virtual Boolean? VSeparateCheck
		{
			get
			{
				return this._VSeparateCheck;
			}
			set
			{
				this._VSeparateCheck = value;
			}
		}
		#endregion
		#region VPrepaymentPct
		public abstract class vPrepaymentPct : Data.BQL.BqlDecimal.Field<vPrepaymentPct>
		{
		}
		[PXDBDecimal(6)]
		public virtual decimal? VPrepaymentPct
		{
			get;
			set;
		}
		#endregion
		#region VAllowAPBillBeforeReceipt
		public abstract class vAllowAPBillBeforeReceipt : PX.Data.BQL.BqlBool.Field<vAllowAPBillBeforeReceipt> { }
		[PXDBBool]
		public virtual bool? VAllowAPBillBeforeReceipt
		{
			get;
			set;
		}
		#endregion

		//Company Location Properties
		#region CMPSalesSubID
		public abstract class cMPSalesSubID : PX.Data.BQL.BqlInt.Field<cMPSalesSubID> { }
		protected Int32? _CMPSalesSubID;
		[PXDBInt()]
		public virtual Int32? CMPSalesSubID
		{
			get
			{
				return this._CMPSalesSubID;
			}
			set
			{
				this._CMPSalesSubID = value;
			}
		}
		#endregion
		#region CMPExpenseSubID
		public abstract class cMPExpenseSubID : PX.Data.BQL.BqlInt.Field<cMPExpenseSubID> { }
		protected Int32? _CMPExpenseSubID;
		[PXDBInt()]
		public virtual Int32? CMPExpenseSubID
		{
			get
			{
				return this._CMPExpenseSubID;
			}
			set
			{
				this._CMPExpenseSubID = value;
			}
		}
		#endregion
		#region CMPFreightSubID
		public abstract class cMPFreightSubID : PX.Data.BQL.BqlInt.Field<cMPFreightSubID> { }
		protected Int32? _CMPFreightSubID;
		[PXDBInt()]
		public virtual Int32? CMPFreightSubID
		{
			get
			{
				return this._CMPFreightSubID;
			}
			set
			{
				this._CMPFreightSubID = value;
			}
		}
		#endregion
		#region CMPDiscountSubID
		public abstract class cMPDiscountSubID : PX.Data.BQL.BqlInt.Field<cMPDiscountSubID> { }
		protected Int32? _CMPDiscountSubID;
		[PXDBInt()]
		public virtual Int32? CMPDiscountSubID
		{
			get
			{
				return this._CMPDiscountSubID;
			}
			set
			{
				this._CMPDiscountSubID = value;
			}
		}
		#endregion
        #region CMPGainLossSubID
        public abstract class cMPGainLossSubID : PX.Data.BQL.BqlInt.Field<cMPGainLossSubID> { }
        protected Int32? _CMPGainLossSubID;
        [PXDBInt()]
        public virtual Int32? CMPGainLossSubID
        {
            get
            {
                return this._CMPGainLossSubID;
            }
            set
            {
                this._CMPGainLossSubID = value;
            }
        }
        #endregion
		#region CMPSiteID
		public abstract class cMPSiteID : PX.Data.BQL.BqlInt.Field<cMPSiteID> { }
		protected Int32? _CMPSiteID;
		[PXDBInt()]
		public virtual Int32? CMPSiteID
		{
			get
			{
				return this._CMPSiteID;
			}
			set
			{
				this._CMPSiteID = value;
			}
		}
		#endregion

		#region CreatedByID
		public abstract class createdByID : PX.Data.BQL.BqlGuid.Field<createdByID> { }
		protected Guid? _CreatedByID;
		[PXDBCreatedByID()]
		public virtual Guid? CreatedByID
		{
			get
			{
				return this._CreatedByID;
			}
			set
			{
				this._CreatedByID = value;
			}
		}
		#endregion
		#region CreatedByScreenID
		public abstract class createdByScreenID : PX.Data.BQL.BqlString.Field<createdByScreenID> { }
		protected String _CreatedByScreenID;
		[PXDBCreatedByScreenID()]
		public virtual String CreatedByScreenID
		{
			get
			{
				return this._CreatedByScreenID;
			}
			set
			{
				this._CreatedByScreenID = value;
			}
		}
		#endregion
		#region CreatedDateTime
		public abstract class createdDateTime : PX.Data.BQL.BqlDateTime.Field<createdDateTime> { }
		protected DateTime? _CreatedDateTime;
		[PXDBCreatedDateTime()]
		public virtual DateTime? CreatedDateTime
		{
			get
			{
				return this._CreatedDateTime;
			}
			set
			{
				this._CreatedDateTime = value;
			}
		}
		#endregion
		#region LastModifiedByID
		public abstract class lastModifiedByID : PX.Data.BQL.BqlGuid.Field<lastModifiedByID> { }
		protected Guid? _LastModifiedByID;
		[PXDBLastModifiedByID()]
		public virtual Guid? LastModifiedByID
		{
			get
			{
				return this._LastModifiedByID;
			}
			set
			{
				this._LastModifiedByID = value;
			}
		}
		#endregion
		#region LastModifiedByScreenID
		public abstract class lastModifiedByScreenID : PX.Data.BQL.BqlString.Field<lastModifiedByScreenID> { }
		protected String _LastModifiedByScreenID;
		[PXDBLastModifiedByScreenID()]
		public virtual String LastModifiedByScreenID
		{
			get
			{
				return this._LastModifiedByScreenID;
			}
			set
			{
				this._LastModifiedByScreenID = value;
			}
		}
		#endregion
		#region LastModifiedDateTime
		public abstract class lastModifiedDateTime : PX.Data.BQL.BqlDateTime.Field<lastModifiedDateTime> { }
		protected DateTime? _LastModifiedDateTime;
		[PXDBLastModifiedDateTime()]
		public virtual DateTime? LastModifiedDateTime
		{
			get
			{
				return this._LastModifiedDateTime;
			}
			set
			{
				this._LastModifiedDateTime = value;
			}
		}
		#endregion
		#region tstamp
		public abstract class Tstamp : PX.Data.BQL.BqlByteArray.Field<Tstamp> { }
		protected Byte[] _tstamp;
		[PXDBTimestamp()]
		public virtual Byte[] tstamp
		{
			get
			{
				return this._tstamp;
			}
			set
			{
				this._tstamp = value;
			}
		}
		#endregion
	}
}

