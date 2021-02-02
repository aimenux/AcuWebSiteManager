using System;

using PX.Data;

using PX.Objects.SO;
using PX.Objects.GL;

namespace PX.Objects.AR.Standalone
{
	/// <summary>
	/// An alias descendant version of <see cref="ARRegister"/>. Can be 
	/// used e.g. to avoid behaviour when <see cref="PXSubstituteAttribute"/> 
	/// substitutes <see cref="ARRegister"/> for derived classes. Can also be used
	/// as a table alias if <see cref="ARRegister"/> is joined multiple times in BQL.
	/// </summary>
	/// <remarks>
	/// Contains all BQL fields from the base class, which is enforced by unit tests.
	/// This class should NOT override any properties. If you need such behaviour,
	/// derive from this class, do not alter it.
	/// </remarks>
	[Serializable]
	[PXHidden]
	public partial class ARRegisterAlias2 : AR.ARRegister
	{
		public new abstract class selected : PX.Data.BQL.BqlBool.Field<selected> { }
		public new abstract class branchID : PX.Data.BQL.BqlInt.Field<branchID> { }
		public new abstract class docType : PX.Data.BQL.BqlString.Field<docType> { }
		public new abstract class printDocType : PX.Data.BQL.BqlString.Field<printDocType> { }
		public new abstract class refNbr : PX.Data.BQL.BqlString.Field<refNbr> { }
		public new abstract class origModule : PX.Data.BQL.BqlString.Field<origModule> { }
		public new abstract class docDate : PX.Data.BQL.BqlDateTime.Field<docDate> { }
		public new abstract class origDocDate : PX.Data.BQL.BqlDateTime.Field<origDocDate> { }
		public new abstract class dueDate : PX.Data.BQL.BqlDateTime.Field<dueDate> { }
		public new abstract class tranPeriodID : PX.Data.BQL.BqlString.Field<tranPeriodID> { }
		public new abstract class finPeriodID : PX.Data.BQL.BqlString.Field<finPeriodID> { }
		public new abstract class customerID : PX.Data.BQL.BqlInt.Field<customerID> { }
		public new abstract class customerID_Customer_acctName : PX.Data.BQL.BqlString.Field<customerID_Customer_acctName> { }
		public new abstract class customerLocationID : PX.Data.BQL.BqlInt.Field<customerLocationID> { }
		public new abstract class curyID : PX.Data.BQL.BqlString.Field<curyID> { }
		public new abstract class aRAccountID : PX.Data.BQL.BqlInt.Field<aRAccountID> { }
		public new abstract class aRSubID : PX.Data.BQL.BqlInt.Field<aRSubID> { }
		public new abstract class lineCntr : PX.Data.BQL.BqlInt.Field<lineCntr> { }
		public new abstract class adjCntr : PX.Data.BQL.BqlInt.Field<adjCntr> { }
		public new abstract class drSchedCntr : PX.Data.BQL.BqlInt.Field<drSchedCntr> { }
		public new abstract class curyInfoID : PX.Data.BQL.BqlLong.Field<curyInfoID> { }
		public new abstract class curyOrigDocAmt : PX.Data.BQL.BqlDecimal.Field<curyOrigDocAmt> { }
		public new abstract class origDocAmt : PX.Data.BQL.BqlDecimal.Field<origDocAmt> { }
		public new abstract class curyDocBal : PX.Data.BQL.BqlDecimal.Field<curyDocBal> { }
		public new abstract class docBal : PX.Data.BQL.BqlDecimal.Field<docBal> { }
		public new abstract class curyOrigDiscAmt : PX.Data.BQL.BqlDecimal.Field<curyOrigDiscAmt> { }
		public new abstract class origDiscAmt : PX.Data.BQL.BqlDecimal.Field<origDiscAmt> { }
		public new abstract class curyDiscTaken : PX.Data.BQL.BqlDecimal.Field<curyDiscTaken> { }
		public new abstract class discTaken : PX.Data.BQL.BqlDecimal.Field<discTaken> { }
		public new abstract class curyDiscBal : PX.Data.BQL.BqlDecimal.Field<curyDiscBal> { }
		public new abstract class discBal : PX.Data.BQL.BqlDecimal.Field<discBal> { }
		public new abstract class docDisc : PX.Data.BQL.BqlDecimal.Field<docDisc> { }
		public new abstract class curyDocDisc : PX.Data.BQL.BqlDecimal.Field<curyDocDisc> { }
		public new abstract class curyChargeAmt : PX.Data.BQL.BqlDecimal.Field<curyChargeAmt> { }
		public new abstract class chargeAmt : PX.Data.BQL.BqlDecimal.Field<chargeAmt> { }
		public new abstract class docDesc : PX.Data.BQL.BqlString.Field<docDesc> { }
		public new abstract class taxCalcMode : PX.Data.BQL.BqlString.Field<taxCalcMode> { }
		public new abstract class createdByID : PX.Data.BQL.BqlGuid.Field<createdByID> { }
		public new abstract class createdByScreenID : PX.Data.BQL.BqlString.Field<createdByScreenID> { }
		public new abstract class createdDateTime : PX.Data.BQL.BqlDateTime.Field<createdDateTime> { }
		public new abstract class lastModifiedByID : PX.Data.BQL.BqlGuid.Field<lastModifiedByID> { }
		public new abstract class lastModifiedByScreenID : PX.Data.BQL.BqlString.Field<lastModifiedByScreenID> { }
		public new abstract class lastModifiedDateTime : PX.Data.BQL.BqlDateTime.Field<lastModifiedDateTime> { }
		public new abstract class Tstamp : PX.Data.BQL.BqlByteArray.Field<Tstamp> { }
		public new abstract class docClass : PX.Data.BQL.BqlString.Field<docClass> { }
		public new abstract class batchNbr : PX.Data.BQL.BqlString.Field<batchNbr> { }
		public new abstract class batchSeq : PX.Data.BQL.BqlShort.Field<batchSeq> { }
		public new abstract class released : PX.Data.BQL.BqlBool.Field<released> { }
		/// <exclude/>
		public new abstract class releasedToVerify : PX.Data.BQL.BqlBool.Field<releasedToVerify> { }
		public new abstract class openDoc : PX.Data.BQL.BqlBool.Field<openDoc> { }
		public new abstract class hold : PX.Data.BQL.BqlBool.Field<hold> { }
		public new abstract class scheduled : PX.Data.BQL.BqlBool.Field<scheduled> { }
		public new abstract class voided : PX.Data.BQL.BqlBool.Field<voided> { }
		public new abstract class pendingProcessing : PX.Data.BQL.BqlBool.Field<pendingProcessing> { }
		public new abstract class selfVoidingDoc : PX.Data.BQL.BqlBool.Field<selfVoidingDoc> { }
		public new abstract class noteID : PX.Data.BQL.BqlGuid.Field<noteID> { }
		public new abstract class closedDate : PX.Data.BQL.BqlDateTime.Field<closedDate> { }
		public new abstract class refNoteID : PX.Data.BQL.BqlGuid.Field<refNoteID> { }
		public new abstract class closedFinPeriodID : PX.Data.BQL.BqlString.Field<closedFinPeriodID> { }
		public new abstract class closedTranPeriodID : PX.Data.BQL.BqlString.Field<closedTranPeriodID> { }
		public new abstract class rGOLAmt : PX.Data.BQL.BqlDecimal.Field<rGOLAmt> { }
		public new abstract class curyRoundDiff : PX.Data.BQL.BqlDecimal.Field<curyRoundDiff> { }
        public new abstract class roundDiff : PX.Data.BQL.BqlDecimal.Field<roundDiff> { }
		public new abstract class scheduleID : PX.Data.BQL.BqlString.Field<scheduleID> { }
		public new abstract class impRefNbr : PX.Data.BQL.BqlString.Field<impRefNbr> { }
		public new abstract class statementDate : PX.Data.BQL.BqlDateTime.Field<statementDate> { }
		public new abstract class salesPersonID : PX.Data.BQL.BqlInt.Field<salesPersonID> { }
		public new abstract class isTaxValid : PX.Data.BQL.BqlBool.Field<isTaxValid> { }
		public new abstract class isTaxPosted : PX.Data.BQL.BqlBool.Field<isTaxPosted> { }
		public new abstract class isTaxSaved : PX.Data.BQL.BqlBool.Field<isTaxSaved> { }
		public new abstract class nonTaxable : PX.Data.BQL.BqlBool.Field<nonTaxable> { }
		public new abstract class origDocType : PX.Data.BQL.BqlString.Field<origDocType> { }
		public new abstract class origRefNbr : PX.Data.BQL.BqlString.Field<origRefNbr> { }
		public new abstract class status : PX.Data.BQL.BqlString.Field<status> { }
		public new abstract class curyDiscountedDocTotal : PX.Data.BQL.BqlDecimal.Field<curyDiscountedDocTotal> { }
		public new abstract class discountedDocTotal : PX.Data.BQL.BqlDecimal.Field<discountedDocTotal> { }
		public new abstract class curyDiscountedTaxableTotal : PX.Data.BQL.BqlDecimal.Field<curyDiscountedTaxableTotal> { }
		public new abstract class discountedTaxableTotal : PX.Data.BQL.BqlDecimal.Field<discountedTaxableTotal> { }
		public new abstract class curyDiscountedPrice : PX.Data.BQL.BqlDecimal.Field<curyDiscountedPrice> { }
		public new abstract class discountedPrice : PX.Data.BQL.BqlDecimal.Field<discountedPrice> { }
		public new abstract class hasPPDTaxes : PX.Data.BQL.BqlBool.Field<hasPPDTaxes> { }
		public new abstract class pendingPPD : PX.Data.BQL.BqlBool.Field<pendingPPD> { }
		public new abstract class curyInitDocBal : PX.Data.BQL.BqlDecimal.Field<curyInitDocBal> { }
		public new abstract class initDocBal : PX.Data.BQL.BqlDecimal.Field<initDocBal> { }
		public new abstract class displayCuryInitDocBal : PX.Data.BQL.BqlDecimal.Field<displayCuryInitDocBal> { }
		public new abstract class isMigratedRecord : PX.Data.BQL.BqlBool.Field<isMigratedRecord> { }
		public new abstract class paymentsByLinesAllowed : PX.Data.BQL.BqlBool.Field<paymentsByLinesAllowed> { }
		public new abstract class approverID : PX.Data.BQL.BqlGuid.Field<approverID> { }
		public new abstract class approverWorkgroupID : PX.Data.BQL.BqlInt.Field<approverWorkgroupID> { }
		public new abstract class approved : PX.Data.BQL.BqlBool.Field<approved> { }
		public new abstract class rejected : PX.Data.BQL.BqlBool.Field<rejected> { }
		public new abstract class dontApprove : PX.Data.BQL.BqlBool.Field<dontApprove> { }

		public new abstract class retainageAcctID : PX.Data.BQL.BqlInt.Field<retainageAcctID> { }
		public new abstract class retainageSubID : PX.Data.BQL.BqlInt.Field<retainageSubID> { }
		public new abstract class retainageApply : PX.Data.BQL.BqlBool.Field<retainageApply> { }
		public new abstract class isRetainageDocument : PX.Data.BQL.BqlBool.Field<isRetainageDocument> { }
		public new abstract class defRetainagePct : PX.Data.BQL.BqlDecimal.Field<defRetainagePct> { }
		public new abstract class curyLineRetainageTotal : PX.Data.BQL.BqlDecimal.Field<curyLineRetainageTotal> { }
		public new abstract class lineRetainageTotal : PX.Data.BQL.BqlDecimal.Field<lineRetainageTotal> { }
		public new abstract class curyRetainageTotal : PX.Data.BQL.BqlDecimal.Field<curyRetainageTotal> { }
		public new abstract class retainageTotal : PX.Data.BQL.BqlDecimal.Field<retainageTotal> { }
		public new abstract class curyRetainageUnreleasedAmt : PX.Data.BQL.BqlDecimal.Field<curyRetainageUnreleasedAmt> { }
		public new abstract class retainageUnreleasedAmt : PX.Data.BQL.BqlDecimal.Field<retainageUnreleasedAmt> { }
		public new abstract class curyRetainageReleased : PX.Data.BQL.BqlDecimal.Field<curyRetainageReleased> { }
		public new abstract class retainageReleased : PX.Data.BQL.BqlDecimal.Field<retainageReleased> { }
		public new abstract class curyRetainedTaxTotal : PX.Data.BQL.BqlDecimal.Field<curyRetainedTaxTotal> { }
		public new abstract class retainedTaxTotal : PX.Data.BQL.BqlDecimal.Field<retainedTaxTotal> { }
		public new abstract class curyRetainedDiscTotal : PX.Data.BQL.BqlDecimal.Field<curyRetainedDiscTotal> { }
		public new abstract class retainedDiscTotal : PX.Data.BQL.BqlDecimal.Field<retainedDiscTotal> { }
		public new abstract class curyRetainageUnpaidTotal : PX.Data.BQL.BqlDecimal.Field<curyRetainageUnpaidTotal> { }
		public new abstract class retainageUnpaidTotal : PX.Data.BQL.BqlDecimal.Field<retainageUnpaidTotal> { }
		public new abstract class curyRetainagePaidTotal : PX.Data.BQL.BqlDecimal.Field<curyRetainagePaidTotal> { }
		public new abstract class retainagePaidTotal : PX.Data.BQL.BqlDecimal.Field<retainagePaidTotal> { }
		public new abstract class curyOrigDocAmtWithRetainageTotal : PX.Data.BQL.BqlDecimal.Field<curyOrigDocAmtWithRetainageTotal> { }
		public new abstract class origDocAmtWithRetainageTotal : PX.Data.BQL.BqlDecimal.Field<origDocAmtWithRetainageTotal> { }

		/// <exclude/>
		public new abstract class isCancellation : Data.BQL.BqlBool.Field<isCancellation> { }
		/// <exclude/>
		public new abstract class isCorrection : Data.BQL.BqlBool.Field<isCorrection> { }
		/// <exclude/>
		public new abstract class isUnderCorrection : Data.BQL.BqlBool.Field<isUnderCorrection> { }
		/// <exclude/>
		public new abstract class canceled : Data.BQL.BqlBool.Field<canceled> { }
	}
}
