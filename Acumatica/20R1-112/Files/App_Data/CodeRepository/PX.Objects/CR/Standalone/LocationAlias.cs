using System;

using PX.Data;

namespace PX.Objects.CR.Standalone
{
	[Serializable]
	[PXHidden]
	public class LocationAlias : CR.Location
	{
		public new abstract class bAccountID : PX.Data.BQL.BqlInt.Field<bAccountID> { }
		public new abstract class locationID : PX.Data.BQL.BqlInt.Field<locationID> { }
		public new abstract class locationCD : PX.Data.BQL.BqlString.Field<locationCD> { }
		public new abstract class locType : PX.Data.BQL.BqlString.Field<locType> { }
		public new abstract class descr : PX.Data.BQL.BqlString.Field<descr> { }
		public new abstract class taxRegistrationID : PX.Data.BQL.BqlString.Field<taxRegistrationID> { }
		public new abstract class defAddressID : PX.Data.BQL.BqlInt.Field<defAddressID> { }
		public new abstract class defContactID : PX.Data.BQL.BqlInt.Field<defContactID> { }
		public new abstract class noteID : PX.Data.BQL.BqlGuid.Field<noteID> { }
		public new abstract class isActive : PX.Data.BQL.BqlBool.Field<isActive> { }
		public new abstract class isDefault : PX.Data.BQL.BqlBool.Field<isDefault> { }
		public new abstract class isAPAccountSameAsMain : PX.Data.BQL.BqlBool.Field<isAPAccountSameAsMain> { }
		public new abstract class isAPPaymentInfoSameAsMain : PX.Data.BQL.BqlBool.Field<isAPPaymentInfoSameAsMain> { }
		public new abstract class isARAccountSameAsMain : PX.Data.BQL.BqlBool.Field<isARAccountSameAsMain> { }
		public new abstract class isRemitAddressSameAsMain : PX.Data.BQL.BqlBool.Field<isRemitAddressSameAsMain> { }
		public new abstract class isRemitContactSameAsMain : PX.Data.BQL.BqlBool.Field<isRemitContactSameAsMain> { }
		public new abstract class cTaxZoneID : PX.Data.BQL.BqlString.Field<cTaxZoneID> { }
		public new abstract class cTaxCalcMode : PX.Data.BQL.BqlString.Field<cTaxCalcMode> { }
		public new abstract class cAvalaraExemptionNumber : PX.Data.BQL.BqlString.Field<cAvalaraExemptionNumber> { }
		public new abstract class cAvalaraCustomerUsageType : PX.Data.BQL.BqlString.Field<cAvalaraCustomerUsageType> { }
		public new abstract class cCarrierID : PX.Data.BQL.BqlString.Field<cCarrierID> { }
		public new abstract class cShipTermsID : PX.Data.BQL.BqlString.Field<cShipTermsID> { }
		public new abstract class cShipZoneID : PX.Data.BQL.BqlString.Field<cShipZoneID> { }
		public new abstract class cFOBPointID : PX.Data.BQL.BqlString.Field<cFOBPointID> { }
		public new abstract class cResedential : PX.Data.BQL.BqlBool.Field<cResedential> { }
		public new abstract class cSaturdayDelivery : PX.Data.BQL.BqlBool.Field<cSaturdayDelivery> { }
		public new abstract class cGroundCollect : PX.Data.BQL.BqlBool.Field<cGroundCollect> { }
		public new abstract class cInsurance : PX.Data.BQL.BqlBool.Field<cInsurance> { }
		public new abstract class cLeadTime : PX.Data.BQL.BqlShort.Field<cLeadTime> { }
		public new abstract class cBranchID : PX.Data.BQL.BqlInt.Field<cBranchID> { }
		public new abstract class cSalesAcctID : PX.Data.BQL.BqlInt.Field<cSalesAcctID> { }
		public new abstract class cSalesSubID : PX.Data.BQL.BqlInt.Field<cSalesSubID> { }
		public new abstract class cPriceClassID : PX.Data.BQL.BqlString.Field<cPriceClassID> { }
		public new abstract class cSiteID : PX.Data.BQL.BqlInt.Field<cSiteID> { }
		public new abstract class cDiscountAcctID : PX.Data.BQL.BqlInt.Field<cDiscountAcctID> { }
		public new abstract class cDiscountSubID : PX.Data.BQL.BqlInt.Field<cDiscountSubID> { }
		public new abstract class cFreightAcctID : PX.Data.BQL.BqlInt.Field<cFreightAcctID> { }
		public new abstract class cFreightSubID : PX.Data.BQL.BqlInt.Field<cFreightSubID> { }
		public new abstract class cRetainageAcctID : PX.Data.BQL.BqlInt.Field<cRetainageAcctID> { }
		public new abstract class cRetainageSubID : PX.Data.BQL.BqlInt.Field<cRetainageSubID> { }
		public new abstract class cShipComplete : PX.Data.BQL.BqlString.Field<cShipComplete> { }
		public new abstract class cOrderPriority : PX.Data.BQL.BqlShort.Field<cOrderPriority> { }
		public new abstract class cCalendarID : PX.Data.BQL.BqlString.Field<cCalendarID> { }
		public new abstract class cDefProjectID : PX.Data.BQL.BqlInt.Field<cDefProjectID> { }
		public new abstract class cARAccountLocationID : PX.Data.BQL.BqlInt.Field<cARAccountLocationID> { }
		public new abstract class cARAccountID : PX.Data.BQL.BqlInt.Field<cARAccountID> { }
		public new abstract class cARSubID : PX.Data.BQL.BqlInt.Field<cARSubID> { }
		public new abstract class vTaxZoneID : PX.Data.BQL.BqlString.Field<vTaxZoneID> { }
		public new abstract class vTaxCalcMode : PX.Data.BQL.BqlString.Field<vTaxCalcMode> { }
		public new abstract class vCarrierID : PX.Data.BQL.BqlString.Field<vCarrierID> { }
		public new abstract class vShipTermsID : PX.Data.BQL.BqlString.Field<vShipTermsID> { }
		public new abstract class vFOBPointID : PX.Data.BQL.BqlString.Field<vFOBPointID> { }
		public new abstract class vLeadTime : PX.Data.BQL.BqlShort.Field<vLeadTime> { }
		public new abstract class vBranchID : PX.Data.BQL.BqlInt.Field<vBranchID> { }
		public new abstract class vExpenseAcctID : PX.Data.BQL.BqlInt.Field<vExpenseAcctID> { }
		public new abstract class vExpenseSubID : PX.Data.BQL.BqlInt.Field<vExpenseSubID> { }
		public new abstract class vFreightAcctID : PX.Data.BQL.BqlInt.Field<vFreightAcctID> { }
		public new abstract class vFreightSubID : PX.Data.BQL.BqlInt.Field<vFreightSubID> { }
		public new abstract class vDiscountAcctID : PX.Data.BQL.BqlInt.Field<vDiscountAcctID> { }
		public new abstract class vDiscountSubID : PX.Data.BQL.BqlInt.Field<vDiscountSubID> { }
		public new abstract class vRetainageAcctID : PX.Data.BQL.BqlInt.Field<vRetainageAcctID> { }
		public new abstract class vRetainageSubID : PX.Data.BQL.BqlInt.Field<vRetainageSubID> { }
		public new abstract class vRcptQtyMin : PX.Data.BQL.BqlDecimal.Field<vRcptQtyMin> { }
		public new abstract class vRcptQtyMax : PX.Data.BQL.BqlDecimal.Field<vRcptQtyMax> { }
		public new abstract class vRcptQtyThreshold : PX.Data.BQL.BqlDecimal.Field<vRcptQtyThreshold> { }
		public new abstract class vRcptQtyAction : PX.Data.BQL.BqlString.Field<vRcptQtyAction> { }
		public new abstract class vSiteID : PX.Data.BQL.BqlInt.Field<vSiteID> { }
		public new abstract class vPrintOrder : PX.Data.BQL.BqlBool.Field<vPrintOrder> { }
		public new abstract class vEmailOrder : PX.Data.BQL.BqlBool.Field<vEmailOrder> { }
		public new abstract class vDefProjectID : PX.Data.BQL.BqlInt.Field<vDefProjectID> { }
		public new abstract class vAPAccountLocationID : PX.Data.BQL.BqlInt.Field<vAPAccountLocationID> { }
		public new abstract class vAPAccountID : PX.Data.BQL.BqlInt.Field<vAPAccountID> { }
		public new abstract class vAPSubID : PX.Data.BQL.BqlInt.Field<vAPSubID> { }
		public new abstract class vPaymentInfoLocationID : PX.Data.BQL.BqlInt.Field<vPaymentInfoLocationID> { }
		public new abstract class vRemitAddressID : PX.Data.BQL.BqlInt.Field<vRemitAddressID> { }
		public new abstract class vRemitContactID : PX.Data.BQL.BqlInt.Field<vRemitContactID> { }
		public new abstract class vPaymentMethodID : PX.Data.BQL.BqlString.Field<vPaymentMethodID> { }
		public new abstract class vCashAccountID : PX.Data.BQL.BqlInt.Field<vCashAccountID> { }
		public new abstract class vPaymentLeadTime : PX.Data.BQL.BqlShort.Field<vPaymentLeadTime> { }
		public new abstract class vPaymentByType : PX.Data.BQL.BqlInt.Field<vPaymentByType> { }
		public new abstract class vSeparateCheck : PX.Data.BQL.BqlBool.Field<vSeparateCheck> { }
		public new abstract class vPrepaymentPct : Data.BQL.BqlDecimal.Field<vPrepaymentPct> { }
		public new abstract class vAllowAPBillBeforeReceipt : PX.Data.BQL.BqlBool.Field<vAllowAPBillBeforeReceipt> { }
		public new abstract class locationAPAccountSubBAccountID : PX.Data.BQL.BqlInt.Field<locationAPAccountSubBAccountID> { }
		public new abstract class aPAccountID : PX.Data.BQL.BqlInt.Field<aPAccountID> { }
		public new abstract class aPSubID : PX.Data.BQL.BqlInt.Field<aPSubID> { }
		public new abstract class locationARAccountSubBAccountID : PX.Data.BQL.BqlInt.Field<locationARAccountSubBAccountID> { }
		public new abstract class aRAccountID : PX.Data.BQL.BqlInt.Field<aRAccountID> { }
		public new abstract class aRSubID : PX.Data.BQL.BqlInt.Field<aRSubID> { }
		public new abstract class locationAPPaymentInfoBAccountID : PX.Data.BQL.BqlInt.Field<locationAPPaymentInfoBAccountID> { }
		public new abstract class remitAddressID : PX.Data.BQL.BqlInt.Field<remitAddressID> { }
		public new abstract class remitContactID : PX.Data.BQL.BqlInt.Field<remitContactID> { }
		public new abstract class paymentMethodID : PX.Data.BQL.BqlString.Field<paymentMethodID> { }
		public new abstract class cashAccountID : PX.Data.BQL.BqlInt.Field<cashAccountID> { }
		public new abstract class paymentLeadTime : PX.Data.BQL.BqlShort.Field<paymentLeadTime> { }
		public new abstract class separateCheck : PX.Data.BQL.BqlBool.Field<separateCheck> { }
		public new abstract class paymentByType : PX.Data.BQL.BqlInt.Field<paymentByType> { }
		public new abstract class bAccountBAccountID : PX.Data.BQL.BqlInt.Field<bAccountBAccountID> { }
		public new abstract class vDefAddressID : PX.Data.BQL.BqlInt.Field<vDefAddressID> { }
		public new abstract class vDefContactID : PX.Data.BQL.BqlInt.Field<vDefContactID> { }
		public new abstract class cMPSalesSubID : PX.Data.BQL.BqlInt.Field<cMPSalesSubID> { }
		public new abstract class cMPExpenseSubID : PX.Data.BQL.BqlInt.Field<cMPExpenseSubID> { }
		public new abstract class cMPFreightSubID : PX.Data.BQL.BqlInt.Field<cMPFreightSubID> { }
		public new abstract class cMPDiscountSubID : PX.Data.BQL.BqlInt.Field<cMPDiscountSubID> { }
		public new abstract class cMPGainLossSubID : PX.Data.BQL.BqlInt.Field<cMPGainLossSubID> { }
		public new abstract class cMPSiteID : PX.Data.BQL.BqlInt.Field<cMPSiteID> { }
		public new abstract class Tstamp : PX.Data.BQL.BqlByteArray.Field<Tstamp> { }
		public new abstract class createdByID : PX.Data.BQL.BqlGuid.Field<createdByID> { }
		public new abstract class createdByScreenID : PX.Data.BQL.BqlString.Field<createdByScreenID> { }
		public new abstract class createdDateTime : PX.Data.BQL.BqlDateTime.Field<createdDateTime> { }
		public new abstract class lastModifiedByID : PX.Data.BQL.BqlGuid.Field<lastModifiedByID> { }
		public new abstract class lastModifiedByScreenID : PX.Data.BQL.BqlString.Field<lastModifiedByScreenID> { }
		public new abstract class lastModifiedDateTime : PX.Data.BQL.BqlDateTime.Field<lastModifiedDateTime> { }
		public new abstract class vSiteIDIsNull : PX.Data.BQL.BqlShort.Field<vSiteIDIsNull> { }
		public new abstract class isAddressSameAsMain : PX.Data.BQL.BqlBool.Field<isAddressSameAsMain> { }
		public new abstract class isContactSameAsMain : PX.Data.BQL.BqlBool.Field<isContactSameAsMain> { }

		public new abstract class aPRetainageAcctID : PX.Data.BQL.BqlInt.Field<aPRetainageAcctID> { }
		public new abstract class aPRetainageSubID : PX.Data.BQL.BqlInt.Field<aPRetainageSubID> { }
		public new abstract class aRRetainageAcctID : PX.Data.BQL.BqlInt.Field<aRRetainageAcctID> { }
		public new abstract class aRRetainageSubID : PX.Data.BQL.BqlInt.Field<aRRetainageSubID> { }

	}
}
