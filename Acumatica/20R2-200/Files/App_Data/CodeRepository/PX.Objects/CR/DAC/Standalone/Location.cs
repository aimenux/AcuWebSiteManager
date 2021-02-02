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
using PX.Objects.CR;


namespace PX.Objects.CR.Standalone
{
	[Serializable()]
	public partial class Location : PX.Data.IBqlTable, ILocation
	{
		#region Keys
		public new class PK : PrimaryKeyOf<Location>.By<locationID>
		{
			public static Location Find(PXGraph graph, int? locationID) => FindBy(graph, locationID);
		}
		#endregion

		#region LocationID
		public abstract class locationID : PX.Data.BQL.BqlInt.Field<locationID> { }

		[PXDBIdentity()]
		[PXUIField(Visible = false, Enabled = false, Visibility = PXUIVisibility.Invisible)]
		[PXReferentialIntegrityCheck]
		public virtual Int32? LocationID { get; set; }
		#endregion
		#region BAccountID
		public abstract class bAccountID : PX.Data.BQL.BqlInt.Field<bAccountID> { }

		[PXDBInt(IsKey = true)]
		[PXUIField(DisplayName = "Account ID", Visible = false, Enabled = false, Visibility = PXUIVisibility.Invisible, TabOrder = 0)]
		public virtual Int32? BAccountID { get; set; }
		#endregion
		#region LocationCD
		public abstract class locationCD : PX.Data.BQL.BqlString.Field<locationCD> { }

		[PXDBString(IsKey = true, IsUnicode = true)]
		public virtual String LocationCD { get; set; }
		#endregion

		#region LocType
		public abstract class locType : PX.Data.BQL.BqlString.Field<locType> { }

		[PXDBString(2, IsFixed = true)]
		[LocTypeList.List()]
		[PXUIField(DisplayName = "Location Type")]
		public virtual String LocType { get; set; }
		#endregion
		#region Descr
		public abstract class descr : PX.Data.BQL.BqlString.Field<descr> { }

		[PXDBString(60, IsUnicode = true)]
		[PXUIField(DisplayName = "Location Name")]
		public virtual String Descr { get; set; }
		#endregion
		#region TaxRegistrationID
		public abstract class taxRegistrationID : PX.Data.BQL.BqlString.Field<taxRegistrationID> { }

		[PXDBString(50, IsUnicode = true)]
		[PXUIField(DisplayName = "Tax Registration ID")]
		[PXPersonalDataField]
		public virtual String TaxRegistrationID { get; set; }
		#endregion
		#region DefAddressID
		public abstract class defAddressID : PX.Data.BQL.BqlInt.Field<defAddressID> { }

		[PXDBInt()]
		[PXUIField(DisplayName = "Default Address")]
		public virtual Int32? DefAddressID { get; set; }
		#endregion
		#region Override Address
		public abstract class overrideAddress : PX.Data.BQL.BqlBool.Field<overrideAddress> { }

		[PXBool]
		[PXUIField(DisplayName = "Override")]
		public virtual bool? OverrideAddress { get; set; }
		#endregion
		#region IsAddressSameAsMain
		[Obsolete("Use OverrideAddress instead")]
		public abstract class isAddressSameAsMain : PX.Data.BQL.BqlBool.Field<isAddressSameAsMain> { }

		[Obsolete("Use OverrideAddress instead")]
		[PXBool()]
		[PXUIField(DisplayName = "Same as Main")]
		public virtual bool? IsAddressSameAsMain
		{
			get { return OverrideAddress != null ? !this.OverrideAddress : null; }
		}
		#endregion
		#region DefContactID
		public abstract class defContactID : PX.Data.BQL.BqlInt.Field<defContactID> { }

		[PXDBInt()]
		[PXUIField(DisplayName = "Default Contact")]
		public virtual Int32? DefContactID { get; set; }
		#endregion
		#region Override Contact
		public abstract class overrideContact : PX.Data.BQL.BqlBool.Field<overrideContact> { }

		[PXBool]
		[PXUIField(DisplayName = "Override")]
		public virtual bool? OverrideContact { get; set; }
		#endregion
		#region IsContactSameAsMain
		[Obsolete("Use OverrideContact instead")]
		public abstract class isContactSameAsMain : PX.Data.BQL.BqlBool.Field<isContactSameAsMain> { }

		[Obsolete("Use OverrideContact instead")]
		[PXBool()]
		[PXUIField(DisplayName = "Same as Main")]
		public virtual bool? IsContactSameAsMain
		{
			get { return OverrideContact != null ? !this.OverrideContact : null; }
		}
		#endregion
		#region NoteID
		public abstract class noteID : PX.Data.BQL.BqlGuid.Field<noteID> { }

		[PXNote()]
		public virtual Guid? NoteID { get; set; }
		#endregion
		#region IsActive
		public abstract class isActive : PX.Data.BQL.BqlBool.Field<isActive> { }

		[PXDBBool()]
		[PXDefault(true)]
		[PXUIField(DisplayName = "Active")]
		public virtual bool? IsActive { get; set; }
		#endregion
		#region IsDefault
		public abstract class isDefault : PX.Data.BQL.BqlBool.Field<isDefault> { }

		[PXBool]
		[PXUIField(DisplayName = "Default", Enabled = false)]
		public virtual bool? IsDefault { get; set; }
		#endregion



		//Customer Location Properties
		#region CTaxZoneID
		public abstract class cTaxZoneID : PX.Data.BQL.BqlString.Field<cTaxZoneID> { }

		[PXDBString(10, IsUnicode = true)]
		[PXUIField(DisplayName = "Tax Zone")]
		[PXDefault(PersistingCheck = PXPersistingCheck.Nothing)]
		public virtual String CTaxZoneID { get; set; }
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

		[PXDBString(30, IsUnicode = true)]
		[PXUIField(DisplayName = "Tax Exemption Number")]
		public virtual String CAvalaraExemptionNumber { get; set; }
		#endregion
		#region CAvalaraCustomerUsageType
		public abstract class cAvalaraCustomerUsageType : PX.Data.BQL.BqlString.Field<cAvalaraCustomerUsageType> { }

		[PXDBString(1, IsFixed = true)]
		[PXUIField(DisplayName = "Entity Usage Type")]
		[PXDefault(TXAvalaraCustomerUsageType.Default)]
		[TX.TXAvalaraCustomerUsageType.List]
		public virtual String CAvalaraCustomerUsageType { get; set; }
		#endregion
		#region CCarrierID
		public abstract class cCarrierID : PX.Data.BQL.BqlString.Field<cCarrierID> { }

		[PXDBString(15, IsUnicode = true, InputMask = ">aaaaaaaaaaaaaaa")]
		[PXUIField(DisplayName = "Ship Via")]
		public virtual String CCarrierID { get; set; }
		#endregion
		#region CShipTermsID
		public abstract class cShipTermsID : PX.Data.BQL.BqlString.Field<cShipTermsID> { }

		[PXDBString(10, IsUnicode = true)]
		[PXUIField(DisplayName = "Shipping Terms")]
		public virtual String CShipTermsID { get; set; }
		#endregion
		#region CShipZoneID
		public abstract class cShipZoneID : PX.Data.BQL.BqlString.Field<cShipZoneID> { }

		[PXDBString(15, IsUnicode = true, InputMask = ">aaaaaaaaaaaaaaa")]
		[PXUIField(DisplayName = "Shipping Zone")]
		public virtual String CShipZoneID { get; set; }
		#endregion
		#region CFOBPointID
		public abstract class cFOBPointID : PX.Data.BQL.BqlString.Field<cFOBPointID> { }

		[PXDBString(15, IsUnicode = true)]
		[PXUIField(DisplayName = "FOB Point")]
		public virtual String CFOBPointID { get; set; }
		#endregion
		#region CResedential
		public abstract class cResedential : PX.Data.BQL.BqlBool.Field<cResedential> { }

		[PXDBBool()]
		[PXDefault(false)]
		[PXUIField(DisplayName = "Residential Delivery")]
		public virtual Boolean? CResedential { get; set; }
		#endregion
		#region CSaturdayDelivery
		public abstract class cSaturdayDelivery : PX.Data.BQL.BqlBool.Field<cSaturdayDelivery> { }

		[PXDBBool()]
		[PXDefault(false)]
		[PXUIField(DisplayName = "Saturday Delivery")]
		public virtual Boolean? CSaturdayDelivery { get; set; }
		#endregion
		#region CGroundCollect
		public abstract class cGroundCollect : PX.Data.BQL.BqlBool.Field<cGroundCollect> { }

		[PXDBBool()]
		[PXDefault(false)]
		[PXUIField(DisplayName = "Ground Collect")]
		public virtual Boolean? CGroundCollect { get; set; }
		#endregion
		#region CInsurance
		public abstract class cInsurance : PX.Data.BQL.BqlBool.Field<cInsurance> { }

		[PXDBBool()]
		[PXDefault(true)]
		[PXUIField(DisplayName = "Insurance")]
		public virtual Boolean? CInsurance { get; set; }
		#endregion
		#region CLeadTime
		public abstract class cLeadTime : PX.Data.BQL.BqlShort.Field<cLeadTime> { }

		[PXDBShort(MinValue = 0, MaxValue = 100000)]
		[PXUIField(DisplayName = CR.Messages.LeadTimeDays)]
		public virtual Int16? CLeadTime { get; set; }
		#endregion
		#region CBranchID
		public abstract class cBranchID : PX.Data.BQL.BqlInt.Field<cBranchID> { }

		[PXDBInt()]
		[PXUIField(DisplayName = "Shipping Branch")]
		public virtual Int32? CBranchID { get; set; }
		#endregion
		#region CSalesAcctID
		public abstract class cSalesAcctID : PX.Data.BQL.BqlInt.Field<cSalesAcctID> { }

		[PXDBInt()]
		[PXUIField(DisplayName = "Sales Account")]
		public virtual Int32? CSalesAcctID { get; set; }
		#endregion
		#region CSalesSubID
		public abstract class cSalesSubID : PX.Data.BQL.BqlInt.Field<cSalesSubID> { }

		[PXDBInt()]
		[PXUIField(DisplayName = "Sales Sub.")]
		public virtual Int32? CSalesSubID { get; set; }
		#endregion
		#region CPriceClassID
		public abstract class cPriceClassID : PX.Data.BQL.BqlString.Field<cPriceClassID> { }

		[PXDBString(10, IsUnicode = true)]
		[PXUIField(DisplayName = "Price Class")]
		public virtual String CPriceClassID { get; set; }
		#endregion
		#region CSiteID
		public abstract class cSiteID : PX.Data.BQL.BqlInt.Field<cSiteID> { }

		[PXDBInt()]
		[PXUIField(DisplayName = "Warehouse")]
		[PXForeignReference(typeof(Field<cSiteID>.IsRelatedTo<INSite.siteID>))]
		public virtual Int32? CSiteID { get; set; }
		#endregion
		#region CDiscountAcctID
		public abstract class cDiscountAcctID : PX.Data.BQL.BqlInt.Field<cDiscountAcctID> { }

		[PXDBInt()]
		[PXUIField(DisplayName = "Discount Account")]
		public virtual Int32? CDiscountAcctID { get; set; }
		#endregion
		#region CDiscountSubID
		public abstract class cDiscountSubID : PX.Data.BQL.BqlInt.Field<cDiscountSubID> { }

		[PXDBInt()]
		[PXUIField(DisplayName = "Discount Sub.")]
		public virtual Int32? CDiscountSubID { get; set; }
		#endregion
		#region CRetainageAcctID
		public abstract class cRetainageAcctID : PX.Data.BQL.BqlInt.Field<cRetainageAcctID> { }

		[PXDBInt()]
		[PXUIField(DisplayName = "Retainage Receivable Account")]
		public virtual int? CRetainageAcctID { get; set; }
		#endregion
		#region CRetainageSubID
		public abstract class cRetainageSubID : PX.Data.BQL.BqlInt.Field<cRetainageSubID> { }

		[PXDBInt()]
		[PXUIField(DisplayName = "Retainage Receivable Sub.")]
		public virtual int? CRetainageSubID { get; set; }
		#endregion
		#region CFreightAcctID
		public abstract class cFreightAcctID : PX.Data.BQL.BqlInt.Field<cFreightAcctID> { }

		[PXDBInt()]
		[PXUIField(DisplayName = "Freight Account")]
		public virtual Int32? CFreightAcctID { get; set; }
		#endregion
		#region CFreightSubID
		public abstract class cFreightSubID : PX.Data.BQL.BqlInt.Field<cFreightSubID> { }

		[PXDBInt()]
		[PXUIField(DisplayName = "Freight Sub.")]
		public virtual Int32? CFreightSubID { get; set; }
		#endregion
		#region CShipComplete
		public abstract class cShipComplete : PX.Data.BQL.BqlString.Field<cShipComplete> { }

		[PXDBString(1, IsFixed = true)]
		[PXDefault(SOShipComplete.CancelRemainder)]
		[SOShipComplete.List()]
		[PXUIField(DisplayName = "Shipping Rule")]
		public virtual String CShipComplete { get; set; }
		#endregion
		#region COrderPriority
		public abstract class cOrderPriority : PX.Data.BQL.BqlShort.Field<cOrderPriority> { }

		[PXDBShort()]
		[PXDefault((short)0)]
		[PXUIField(DisplayName = "Order Priority")]
		public virtual Int16? COrderPriority { get; set; }
		#endregion
		#region CCalendarID
		public abstract class cCalendarID : PX.Data.BQL.BqlString.Field<cCalendarID> { }

		[PXDBString(10, IsUnicode = true)]
		[PXUIField(DisplayName = "Calendar")]
		public virtual String CCalendarID { get; set; }
		#endregion
		#region CDefProject
		public abstract class cDefProjectID : PX.Data.BQL.BqlInt.Field<cDefProjectID> { }

		[PXDBInt]
		[PXUIField(DisplayName = "Default Project")]
		public virtual Int32? CDefProjectID { get; set; }
		#endregion
		#region CARAccountLocationID
		public abstract class cARAccountLocationID : PX.Data.BQL.BqlInt.Field<cARAccountLocationID> { }

		[PXDBInt()]
		[PXDefault()]
		public virtual Int32? CARAccountLocationID { get; set; }
		#endregion
		#region CARAccountID
		public abstract class cARAccountID : PX.Data.BQL.BqlInt.Field<cARAccountID> { }

		[PXDBInt()]
		[PXUIField(DisplayName = "AR Account")]
		public virtual Int32? CARAccountID { get; set; }
		#endregion
		#region CARSubID
		public abstract class cARSubID : PX.Data.BQL.BqlInt.Field<cARSubID> { }

		[PXDBInt()]
		[PXUIField(DisplayName = "AR Sub.")]
		public virtual Int32? CARSubID { get; set; }
		#endregion
		#region IsARAccountSameAsMain
		public abstract class isARAccountSameAsMain : PX.Data.BQL.BqlBool.Field<isARAccountSameAsMain> { }

		[PXBool()]
		[PXUIField(DisplayName = "Same As Default Location's")]
		[PXFormula(typeof(Switch<Case<Where<locationID, Equal<cARAccountLocationID>>, False>, True>))]
		public virtual bool? IsARAccountSameAsMain { get; set; }
		#endregion



		// Vendor Location Properties
		#region VTaxZoneID
		public abstract class vTaxZoneID : PX.Data.BQL.BqlString.Field<vTaxZoneID> { }

		[PXDBString(10, IsUnicode = true)]
		[PXUIField(DisplayName = "Tax Zone")]
		[PXDefault(PersistingCheck = PXPersistingCheck.Nothing)]
		public virtual String VTaxZoneID { get; set; }
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

		[PXUIField(DisplayName = "Ship Via")]
		[PXDBString(15, IsUnicode = true, InputMask = ">aaaaaaaaaaaaaaa")]
		public virtual String VCarrierID { get; set; }
		#endregion
		#region VShipTermsID
		public abstract class vShipTermsID : PX.Data.BQL.BqlString.Field<vShipTermsID> { }

		[PXDBString(10, IsUnicode = true)]
		[PXUIField(DisplayName = "Shipping Terms")]
		public virtual String VShipTermsID { get; set; }
		#endregion
		#region VFOBPointID
		public abstract class vFOBPointID : PX.Data.BQL.BqlString.Field<vFOBPointID> { }

		[PXDBString(15, IsUnicode = true)]
		[PXUIField(DisplayName = "FOB Point")]
		public virtual String VFOBPointID { get; set; }
		#endregion
		#region VLeadTime
		public abstract class vLeadTime : PX.Data.BQL.BqlShort.Field<vLeadTime> { }

		[PXDBShort(MinValue = 0, MaxValue = 100000)]
		[PXUIField(DisplayName = CR.Messages.LeadTimeDays)]
		public virtual Int16? VLeadTime { get; set; }
		#endregion
		#region VBranchID
		public abstract class vBranchID : PX.Data.BQL.BqlInt.Field<vBranchID> { }

		[PXDBInt()]
		[PXUIField(DisplayName = "Receiving Branch")]
		public virtual Int32? VBranchID { get; set; }
		#endregion
		#region VExpenseAcctID
		public abstract class vExpenseAcctID : PX.Data.BQL.BqlInt.Field<vExpenseAcctID> { }

		[PXDBInt()]
		[PXUIField(DisplayName = "Expense Account")]
		public virtual Int32? VExpenseAcctID { get; set; }
		#endregion
		#region VExpenseSubID
		public abstract class vExpenseSubID : PX.Data.BQL.BqlInt.Field<vExpenseSubID> { }

		[PXDBInt()]
		[PXUIField(DisplayName = "Expense Sub.")]
		public virtual Int32? VExpenseSubID { get; set; }
		#endregion
		#region VRetainageAcctID
		public abstract class vRetainageAcctID : PX.Data.BQL.BqlInt.Field<vRetainageAcctID> { }

		[PXDBInt()]
		[PXUIField(DisplayName = "Retainage Payable Account")]
		public virtual int? VRetainageAcctID { get; set; }
		#endregion
		#region VRetainageSubID
		public abstract class vRetainageSubID : PX.Data.BQL.BqlInt.Field<vRetainageSubID> { }

		[PXDBInt()]
		[PXUIField(DisplayName = "Retainage Payable Sub.")]
		public virtual int? VRetainageSubID { get; set; }
		#endregion
		#region VFreightAcctID
		public abstract class vFreightAcctID : PX.Data.BQL.BqlInt.Field<vFreightAcctID> { }

		[PXDBInt()]
		[PXUIField(DisplayName = "Freight Account")]
		public virtual Int32? VFreightAcctID { get; set; }
		#endregion
		#region VFreightSubID
		public abstract class vFreightSubID : PX.Data.BQL.BqlInt.Field<vFreightSubID> { }

		[PXDBInt()]
		[PXUIField(DisplayName = "Freight Sub.")]
		public virtual Int32? VFreightSubID { get; set; }
		#endregion
		#region VDiscountAcctID
		public abstract class vDiscountAcctID : PX.Data.BQL.BqlInt.Field<vDiscountAcctID> { }

		[PXDBInt()]
		[PXUIField(DisplayName = "Discount Account")]
		public virtual Int32? VDiscountAcctID { get; set; }
		#endregion
		#region VDiscountSubID
		public abstract class vDiscountSubID : PX.Data.BQL.BqlInt.Field<vDiscountSubID> { }

		[PXDBInt()]
		[PXUIField(DisplayName = "Discount Sub.")]
		public virtual Int32? VDiscountSubID { get; set; }
		#endregion
		#region VRcptQtyMin
		public abstract class vRcptQtyMin : PX.Data.BQL.BqlDecimal.Field<vRcptQtyMin> { }

		[PXDBDecimal(2, MinValue = 0.0, MaxValue = 999.0)]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = "Min. Receipt (%)")]
		public virtual Decimal? VRcptQtyMin { get; set; }
		#endregion
		#region VRcptQtyMax
		public abstract class vRcptQtyMax : PX.Data.BQL.BqlDecimal.Field<vRcptQtyMax> { }

		[PXDBDecimal(2, MinValue = 0.0, MaxValue = 999.0)]
		[PXDefault(TypeCode.Decimal, "100.0")]
		[PXUIField(DisplayName = "Max. Receipt (%)")]
		public virtual Decimal? VRcptQtyMax { get; set; }
		#endregion
		#region VRcptQtyThreshold
		public abstract class vRcptQtyThreshold : PX.Data.BQL.BqlDecimal.Field<vRcptQtyThreshold> { }

		[PXDBDecimal(2, MinValue = 0.0, MaxValue = 999.0)]
		[PXDefault(TypeCode.Decimal, "100.0")]
		[PXUIField(DisplayName = "Threshold Receipt (%)")]
		public virtual Decimal? VRcptQtyThreshold { get; set; }
		#endregion
		#region VRcptQtyAction
		public abstract class vRcptQtyAction : PX.Data.BQL.BqlString.Field<vRcptQtyAction> { }

		[PXDBString(1, IsFixed = true)]
		[PXDefault(POReceiptQtyAction.AcceptButWarn)]
		[POReceiptQtyAction.List()]
		[PXUIField(DisplayName = "Receipt Action")]
		public virtual String VRcptQtyAction { get; set; }
		#endregion
		#region VSiteID
		public abstract class vSiteID : PX.Data.BQL.BqlInt.Field<vSiteID> { }

		[PXDBInt()]
		[PXUIField(DisplayName = "Warehouse", Visibility = PXUIVisibility.Visible, FieldClass = SiteAttribute.DimensionName)]
		[PXDimensionSelector(SiteAttribute.DimensionName, typeof(INSite.siteID), typeof(INSite.siteCD), DescriptionField = typeof(INSite.descr))]
		[PXRestrictor(typeof(Where<INSite.active, Equal<True>>), IN.Messages.InactiveWarehouse, typeof(INSite.siteCD))]
		[PXRestrictor(typeof(Where<INSite.siteID, NotEqual<SiteAttribute.transitSiteID>>), IN.Messages.TransitSiteIsNotAvailable)]
		public virtual Int32? VSiteID { get; set; }
		#endregion
		#region VSiteIDIsNull
		public abstract class vSiteIDIsNull : PX.Data.BQL.BqlShort.Field<vSiteIDIsNull> { }

		[PXShort()]
		[PXDBCalced(typeof(Switch<Case<Where<Location.vSiteID, IsNull>, shortMax>, short0>), typeof(short))]
		public virtual Int16? VSiteIDIsNull { get; set; }
		#endregion
		#region VPrintOrder
		public abstract class vPrintOrder : PX.Data.BQL.BqlBool.Field<vPrintOrder> { }

		[PXDBBool]
		[PXDefault(false)]
		[PXUIField(DisplayName = "Print Order")]
		public virtual bool? VPrintOrder { get; set; }
		#endregion
		#region VEmailOrder
		public abstract class vEmailOrder : PX.Data.BQL.BqlBool.Field<vEmailOrder> { }

		[PXDBBool]
		[PXDefault(false)]
		[PXUIField(DisplayName = "Email Order")]
		public virtual bool? VEmailOrder { get; set; }
		#endregion
		#region VDefProjectID
		public abstract class vDefProjectID : PX.Data.BQL.BqlInt.Field<vDefProjectID> { }

		[PXDBInt]
		[PXUIField(DisplayName = "Default Project")]
		public virtual Int32? VDefProjectID { get; set; }
		#endregion
		#region VAPAccountLocationID
		public abstract class vAPAccountLocationID : PX.Data.BQL.BqlInt.Field<vAPAccountLocationID> { }

		[PXDBInt()]
		[PXDefault()]
		public virtual Int32? VAPAccountLocationID { get; set; }
		#endregion
		#region VAPAccountID
		public abstract class vAPAccountID : PX.Data.BQL.BqlInt.Field<vAPAccountID> { }

		[PXDBInt()]
		[PXUIField(DisplayName = "AP Account")]
		public virtual Int32? VAPAccountID { get; set; }
		#endregion
		#region VAPSubID
		public abstract class vAPSubID : PX.Data.BQL.BqlInt.Field<vAPSubID> { }

		[PXDBInt()]
		[PXUIField(DisplayName = "AP Sub.")]
		public virtual Int32? VAPSubID { get; set; }
		#endregion
		#region VPaymentInfoLocationID
		public abstract class vPaymentInfoLocationID : PX.Data.BQL.BqlInt.Field<vPaymentInfoLocationID> { }

		[PXDBInt()]
		[PXDefault()]
		public virtual Int32? VPaymentInfoLocationID { get; set; }
		#endregion
		#region OverrideRemitAddress
		public abstract class overrideRemitAddress : PX.Data.BQL.BqlBool.Field<overrideRemitAddress> { }

		[PXBool]
		[PXUIField(DisplayName = "Override")]
		public virtual bool? OverrideRemitAddress { get; set; }
		#endregion
		#region IsRemitAddressSameAsMain
		[Obsolete("Use OverrideRemitAddress instead")]
		public abstract class isRemitAddressSameAsMain : PX.Data.BQL.BqlBool.Field<isRemitAddressSameAsMain> { }
		protected bool? _IsRemitAddressSameAsMain;
		[Obsolete("Use OverrideRemitAddress instead")]
		[PXBool()]
		[PXUIField(DisplayName = "Same as Main")]
		public virtual bool? IsRemitAddressSameAsMain
		{
			get { return OverrideRemitAddress != null ? !this.OverrideRemitAddress : null; }
		}
		#endregion
		#region VRemitAddressID
		public abstract class vRemitAddressID : PX.Data.BQL.BqlInt.Field<vRemitAddressID> { }

		[PXDBInt()]
		public virtual Int32? VRemitAddressID { get; set; }
		#endregion
		#region OverrideRemitContact
		public abstract class overrideRemitContact : PX.Data.BQL.BqlBool.Field<overrideRemitContact> { }

		[PXBool]
		[PXUIField(DisplayName = "Override")]
		public virtual bool? OverrideRemitContact { get; set; }
		#endregion
		#region IsRemitContactSameAsMain
		[Obsolete("Use OverrideRemitContact instead")]
		public abstract class isRemitContactSameAsMain : PX.Data.BQL.BqlBool.Field<isRemitContactSameAsMain> { }
		protected bool? _IsRemitContactSameAsMain;
		[Obsolete("Use OverrideRemitContact instead")]
		[PXBool()]
		[PXUIField(DisplayName = "Same as Main")]
		public virtual bool? IsRemitContactSameAsMain
		{
			get { return OverrideRemitContact != null ? !this.OverrideRemitContact : null; }
		}
		#endregion
		#region VRemitContactID
		public abstract class vRemitContactID : PX.Data.BQL.BqlInt.Field<vRemitContactID> { }

		[PXDBInt()]
		public virtual Int32? VRemitContactID { get; set; }
		#endregion
		#region VPaymentMethodID
		public abstract class vPaymentMethodID : PX.Data.BQL.BqlString.Field<vPaymentMethodID> { }

		[PXDBString(10, IsUnicode = true)]
		[PXUIField(DisplayName = "Payment Method")]
		[PXDefault(PersistingCheck = PXPersistingCheck.Nothing)]
		public virtual String VPaymentMethodID { get; set; }
		#endregion
		#region VCashAccountID
		public abstract class vCashAccountID : PX.Data.BQL.BqlInt.Field<vCashAccountID> { }

		[PXDBInt()]
		[PXUIField(DisplayName = "Cash Account")]
		public virtual Int32? VCashAccountID { get; set; }
		#endregion
		#region VPaymentLeadTime
		public abstract class vPaymentLeadTime : PX.Data.BQL.BqlShort.Field<vPaymentLeadTime> { }

		[PXDBShort(MinValue = 0, MaxValue = 3660)]
		[PXDefault((short)0)]
		[PXUIField(DisplayName = "Payment Lead Time (Days)")]
		public Int16? VPaymentLeadTime { get; set; }
		#endregion
		#region VPaymentByType
		public abstract class vPaymentByType : PX.Data.BQL.BqlInt.Field<vPaymentByType> { }

		[PXDBInt()]
		[PXUIField(DisplayName = "Payment By")]
		[PXDefault(APPaymentBy.DueDate)]
		[APPaymentBy.List]
		public int? VPaymentByType { get; set; }
		#endregion
		#region VSeparateCheck
		public abstract class vSeparateCheck : PX.Data.BQL.BqlBool.Field<vSeparateCheck> { }

		[PXDBBool()]
		[PXUIField(DisplayName = "Pay Separately")]
		[PXDefault(false)]
		public virtual Boolean? VSeparateCheck { get; set; }
		#endregion
		#region VPrepaymentPct
		public abstract class vPrepaymentPct : Data.BQL.BqlDecimal.Field<vPrepaymentPct> { }

		[PXDBDecimal(6)]
		[PXUIField(DisplayName = "Prepayment Percent")]
		[PXDefault(TypeCode.Decimal, "100.0")]
		public virtual decimal? VPrepaymentPct { get; set; }
		#endregion
		#region VAllowAPBillBeforeReceipt
		public abstract class vAllowAPBillBeforeReceipt : PX.Data.BQL.BqlBool.Field<vAllowAPBillBeforeReceipt> { }
		[PXDBBool]
		[PXUIField(DisplayName = "Allow AP Bill Before Receipt")]
		[PXDefault(false)]
		public virtual bool? VAllowAPBillBeforeReceipt { get; set; }
		#endregion
		#region IsAPAccountSameAsMain
		public abstract class isAPAccountSameAsMain : PX.Data.BQL.BqlBool.Field<isAPAccountSameAsMain> { }

		[PXBool()]
		[PXUIField(DisplayName = "Same As Default Location's")]
		[PXFormula(typeof(Switch<Case<Where<locationID, Equal<vAPAccountLocationID>>, False>, True>))]
		public virtual bool? IsAPAccountSameAsMain { get; set; }
		#endregion
		#region IsAPPaymentInfoSameAsMain
		public abstract class isAPPaymentInfoSameAsMain : PX.Data.BQL.BqlBool.Field<isAPPaymentInfoSameAsMain> { }

		[PXBool()]
		[PXUIField(DisplayName = "Same As Default Location's")]
		[PXFormula(typeof(Switch<Case<Where<locationID, Equal<vPaymentInfoLocationID>>, False>, True>))]
		public virtual bool? IsAPPaymentInfoSameAsMain { get; set; }
		#endregion



		// Company Location Properties
		#region CMPSalesSubID
		public abstract class cMPSalesSubID : PX.Data.BQL.BqlInt.Field<cMPSalesSubID> { }

		[PXDBInt()]
		[PXUIField(DisplayName = "Sales Sub.")]
		public virtual Int32? CMPSalesSubID { get; set; }
		#endregion
		#region CMPExpenseSubID
		public abstract class cMPExpenseSubID : PX.Data.BQL.BqlInt.Field<cMPExpenseSubID> { }

		[PXDBInt()]
		[PXUIField(DisplayName = "Expense Sub.")]
		public virtual Int32? CMPExpenseSubID { get; set; }
		#endregion
		#region CMPFreightSubID
		public abstract class cMPFreightSubID : PX.Data.BQL.BqlInt.Field<cMPFreightSubID> { }

		[PXDBInt()]
		[PXUIField(DisplayName = "Freight Sub.")]
		public virtual Int32? CMPFreightSubID { get; set; }
		#endregion
		#region CMPDiscountSubID
		public abstract class cMPDiscountSubID : PX.Data.BQL.BqlInt.Field<cMPDiscountSubID> { }

		[PXDBInt()]
		[PXUIField(DisplayName = "Discount Sub.")]
		public virtual Int32? CMPDiscountSubID { get; set; }
		#endregion
		#region CMPGainLossSubID
		public abstract class cMPGainLossSubID : PX.Data.BQL.BqlInt.Field<cMPGainLossSubID> { }

		[PXDBInt()]
		[PXUIField(DisplayName = "Currency Gain/Loss Sub.")]
		public virtual Int32? CMPGainLossSubID { get; set; }
		#endregion
		#region CMPSiteID
		public abstract class cMPSiteID : PX.Data.BQL.BqlInt.Field<cMPSiteID> { }

		[PXDBInt()]
		[PXUIField(DisplayName = "Warehouse")]
		public virtual Int32? CMPSiteID { get; set; }
		#endregion



		// Audit Fields
		#region CreatedByID
		public abstract class createdByID : PX.Data.BQL.BqlGuid.Field<createdByID> { }

		[PXDBCreatedByID()]
		public virtual Guid? CreatedByID { get; set; }
		#endregion
		#region CreatedByScreenID
		public abstract class createdByScreenID : PX.Data.BQL.BqlString.Field<createdByScreenID> { }

		[PXDBCreatedByScreenID()]
		public virtual String CreatedByScreenID { get; set; }
		#endregion
		#region CreatedDateTime
		public abstract class createdDateTime : PX.Data.BQL.BqlDateTime.Field<createdDateTime> { }

		[PXDBCreatedDateTime()]
		public virtual DateTime? CreatedDateTime { get; set; }
		#endregion
		#region LastModifiedByID
		public abstract class lastModifiedByID : PX.Data.BQL.BqlGuid.Field<lastModifiedByID> { }

		[PXDBLastModifiedByID()]
		public virtual Guid? LastModifiedByID { get; set; }
		#endregion
		#region LastModifiedByScreenID
		public abstract class lastModifiedByScreenID : PX.Data.BQL.BqlString.Field<lastModifiedByScreenID> { }

		[PXDBLastModifiedByScreenID()]
		public virtual String LastModifiedByScreenID { get; set; }
		#endregion
		#region LastModifiedDateTime
		public abstract class lastModifiedDateTime : PX.Data.BQL.BqlDateTime.Field<lastModifiedDateTime> { }

		[PXDBLastModifiedDateTime()]
		public virtual DateTime? LastModifiedDateTime { get; set; }
		#endregion
		#region tstamp
		public abstract class Tstamp : PX.Data.BQL.BqlByteArray.Field<Tstamp> { }

		[PXDBTimestamp()]
		public virtual Byte[] tstamp { get; set; }
		#endregion
	}
}
