using PX.Data;
using PX.Data.BQL.Fluent;
using PX.Objects.AP;
using PX.Objects.EP;
using System;

namespace PX.Objects.PR
{
	[PXCacheName(Messages.PREmployeeDeduct)]
	[Serializable]
	public class PREmployeeDeduct : IBqlTable
	{
		#region BAccountID
		public abstract class bAccountID : PX.Data.BQL.BqlInt.Field<bAccountID> { }
		[PXDBInt(IsKey = true)]
		[PXDefault(typeof(PREmployee.bAccountID))]
		[PXParent(typeof(Select<PREmployee, Where<PREmployee.bAccountID, Equal<Current<PREmployeeDeduct.bAccountID>>>>))]
		public int? BAccountID { get; set; }
		#endregion
		#region LineNbr
		public abstract class lineNbr : PX.Data.BQL.BqlInt.Field<lineNbr> { }
		[PXDBInt(IsKey = true)]
		[PXLineNbr(typeof(PREmployee.lineCntr))]
		[PXUIField(DisplayName = "Line Nbr.", Visible = false)]
		public virtual int? LineNbr { get; set; }
		#endregion
		#region CodeID
		public abstract class codeID : PX.Data.BQL.BqlInt.Field<codeID> { }
		[PXDBInt]
		[PXDefault]
		[PXUIField(DisplayName = "Deduction Code")]
		[DeductionActiveSelector(typeof(Where<PRDeductCode.isWorkersCompensation.IsEqual<False>
			.And<PRDeductCode.isCertifiedProject.IsEqual<False>>
			.And<PRDeductCode.isUnion.IsEqual<False>>>))]
		public int? CodeID { get; set; }
		#endregion
		#region ContribType
		public abstract class contribType : PX.Data.BQL.BqlString.Field<contribType> { }
		[PXString(3, IsFixed = true)]
		[PXUIField(DisplayName = "Contribution Type")]
		[ContributionType.List]
		[PXFormula(typeof(Selector<codeID, PRDeductCode.contribType>))]
		public string ContribType { get; set; }
		#endregion
		#region CntCalcType
		public abstract class cntCalcType : PX.Data.BQL.BqlString.Field<cntCalcType> { }
		[PXString(3, IsFixed = true)]
		[PXUIField(DisplayName = "Calculation Method")]
		[DedCntCalculationMethod.List]
		[PXFormula(typeof(Selector<codeID, PRDeductCode.cntCalcType>))]
		public string CntCalcType { get; set; }
		#endregion
		#region CntMaxFreqType
		public abstract class cntMaxFreqType : PX.Data.BQL.BqlString.Field<cntMaxFreqType> { }
		[PXDBString(3, IsFixed = true)]
		[PXUIField(DisplayName = "Contribution Maximum Frequency")]
		[DeductionMaxFrequencyType.List]
		[PXDefault]
		[PXUIEnabled(typeof(Where<PREmployeeDeduct.contribType, NotEqual<ContributionType.employeeDeduction>>))]
		[PXUIRequired(typeof(Where<PREmployeeDeduct.contribType, NotEqual<ContributionType.employeeDeduction>,
			And<PREmployeeDeduct.cntUseDflt, Equal<False>>>))]
		[UseDefaultValue(typeof(PREmployeeDeduct.codeID), typeof(PRDeductCode.cntMaxFreqType), typeof(PREmployeeDeduct.dedUseDflt), ManageUIEnabled = false)]
		public string CntMaxFreqType { get; set; }
		#endregion
		#region DedCalcType
		public abstract class dedCalcType : PX.Data.BQL.BqlString.Field<dedCalcType> { }
		[PXString(3, IsFixed = true)]
		[PXUIField(DisplayName = "Calculation Method")]
		[DedCntCalculationMethod.List]
		[PXFormula(typeof(Selector<codeID, PRDeductCode.dedCalcType>))]
		public string DedCalcType { get; set; }
		#endregion
		#region DedMaxFreqType
		public abstract class dedMaxFreqType : PX.Data.BQL.BqlString.Field<dedMaxFreqType> { }
		[PXDBString(3, IsFixed = true)]
		[PXUIField(DisplayName = "Deduction Maximum Frequency")]
		[DeductionMaxFrequencyType.List]
		[PXDefault]
		[PXUIEnabled(typeof(Where<PREmployeeDeduct.contribType, NotEqual<ContributionType.employerContribution>>))]
		[PXUIRequired(typeof(Where<PREmployeeDeduct.contribType, NotEqual<ContributionType.employerContribution>,
			And<PREmployeeDeduct.dedUseDflt, Equal<False>>>))]
		[UseDefaultValue(typeof(PREmployeeDeduct.codeID), typeof(PRDeductCode.dedMaxFreqType), typeof(PREmployeeDeduct.dedUseDflt), ManageUIEnabled = false)]
		public string DedMaxFreqType { get; set; }
		#endregion
		#region StartDate
		public abstract class startDate : PX.Data.BQL.BqlDateTime.Field<startDate> { }
		[PXDBDate]
		[PXUIField(DisplayName = "Start")]
		[PXDBDefault]
		public DateTime? StartDate { get; set; }
		#endregion
		#region EndDate
		public abstract class endDate : PX.Data.BQL.BqlDateTime.Field<endDate> { }
		[PXDBDate]
		[PXUIField(DisplayName = "End")]
		public DateTime? EndDate { get; set; }
		#endregion
		#region IsActive
		public abstract class isActive : PX.Data.BQL.BqlBool.Field<isActive> { }
		[PXDBBool]
		[PXDefault(true)]
		[PXUIField(DisplayName = "Active")]
		public bool? IsActive { get; set; }
		#endregion
		#region DedAmount
		public abstract class dedAmount : PX.Data.BQL.BqlDecimal.Field<dedAmount> { }
		[PRCurrency(MinValue = 0)]
		[PXUIField(DisplayName = "Deduction Amount")]
		[PXDefault]
		[PXUIEnabled(typeof(Where<PREmployeeDeduct.contribType, NotEqual<ContributionType.employerContribution>,
			And<PREmployeeDeduct.dedCalcType, NotEqual<DedCntCalculationMethod.percentOfGross>,
			And<PREmployeeDeduct.dedCalcType, NotEqual<DedCntCalculationMethod.percentOfCustom>,
			And<PREmployeeDeduct.dedCalcType, NotEqual<DedCntCalculationMethod.percentOfNet>,
			And<PREmployeeDeduct.dedCalcType, IsNotNull>>>>>))]
		[PXUIRequired(typeof(Where<PREmployeeDeduct.contribType, NotEqual<ContributionType.employerContribution>,
			And<PREmployeeDeduct.dedCalcType, NotEqual<DedCntCalculationMethod.percentOfGross>,
			And<PREmployeeDeduct.dedCalcType, NotEqual<DedCntCalculationMethod.percentOfCustom>,
			And<PREmployeeDeduct.dedCalcType, NotEqual<DedCntCalculationMethod.percentOfNet>,
			And<PREmployeeDeduct.dedCalcType, IsNotNull,
			And<PREmployeeDeduct.dedUseDflt, Equal<False>>>>>>>))]
		[UseDefaultValue(typeof(PREmployeeDeduct.codeID), typeof(PRDeductCode.dedAmount), typeof(PREmployeeDeduct.dedUseDflt), ManageUIEnabled = false)]
		public Decimal? DedAmount { get; set; }
		#endregion
		#region DedPercent
		public abstract class dedPercent : PX.Data.BQL.BqlDecimal.Field<dedPercent> { }
		[PXDBDecimal(MinValue = 0)]
		[PXUIField(DisplayName = "Deduction Percent")]
		[PXDefault]
		[PXUIEnabled(typeof(Where<PREmployeeDeduct.contribType, NotEqual<ContributionType.employerContribution>,
			And<PREmployeeDeduct.dedCalcType, NotEqual<DedCntCalculationMethod.fixedAmount>,
			And<PREmployeeDeduct.dedCalcType, NotEqual<DedCntCalculationMethod.amountPerHour>,
			And<PREmployeeDeduct.dedCalcType, IsNotNull>>>>))]
		[PXUIRequired(typeof(Where<PREmployeeDeduct.contribType, NotEqual<ContributionType.employerContribution>,
			And<PREmployeeDeduct.dedCalcType, NotEqual<DedCntCalculationMethod.fixedAmount>,
			And<PREmployeeDeduct.dedCalcType, NotEqual<DedCntCalculationMethod.amountPerHour>,
			And<PREmployeeDeduct.dedCalcType, IsNotNull,
			And<PREmployeeDeduct.dedUseDflt, Equal<False>>>>>>))]
		[UseDefaultValue(typeof(PREmployeeDeduct.codeID), typeof(PRDeductCode.dedPercent), typeof(PREmployeeDeduct.dedUseDflt), ManageUIEnabled = false)]
		public Decimal? DedPercent { get; set; }
		#endregion
		#region DedMaxAmount
		public abstract class dedMaxAmount : PX.Data.BQL.BqlDecimal.Field<dedMaxAmount> { }
		[PRCurrency(MinValue = 0)]
		[PXUIField(DisplayName = "Deduction Max")]
		[PXDefault]
		[PXUIEnabled(typeof(Where<PREmployeeDeduct.contribType, NotEqual<ContributionType.employerContribution>,
			And<PREmployeeDeduct.dedMaxFreqType, NotEqual<DeductionMaxFrequencyType.noMaximum>>>))]
		[PXUIRequired(typeof(Where<PREmployeeDeduct.contribType, NotEqual<ContributionType.employerContribution>,
			And<PREmployeeDeduct.dedMaxFreqType, NotEqual<DeductionMaxFrequencyType.noMaximum>,
			And<PREmployeeDeduct.dedUseDflt, Equal<False>>>>))]
		[UseDefaultValue(typeof(PREmployeeDeduct.codeID), typeof(PRDeductCode.dedMaxAmount), typeof(PREmployeeDeduct.dedUseDflt), ManageUIEnabled = false)]
		public Decimal? DedMaxAmount { get; set; }
		#endregion
		#region DedUseDflt
		public abstract class dedUseDflt : PX.Data.BQL.BqlBool.Field<dedUseDflt> { }
		[PXDBBool]
		[PXUIField(DisplayName = "Use Deduction Defaults")]
		[PXDefault(true)]
		[PXUIEnabled(typeof(Where<PREmployeeDeduct.contribType, NotEqual<ContributionType.employerContribution>>))]
		public bool? DedUseDflt { get; set; }
		#endregion
		#region CntAmount
		public abstract class cntAmount : PX.Data.BQL.BqlDecimal.Field<cntAmount> { }
		[PRCurrency(MinValue = 0)]
		[PXUIField(DisplayName = "Contribution Amount")]
		[PXDefault]
		[PXUIEnabled(typeof(Where<PREmployeeDeduct.contribType, NotEqual<ContributionType.employeeDeduction>,
			And<PREmployeeDeduct.cntCalcType, NotEqual<DedCntCalculationMethod.percentOfGross>,
			And<PREmployeeDeduct.dedCalcType, NotEqual<DedCntCalculationMethod.percentOfCustom>,
			And<PREmployeeDeduct.cntCalcType, NotEqual<DedCntCalculationMethod.percentOfNet>,
			And<PREmployeeDeduct.cntCalcType, IsNotNull>>>>>))]
		[PXUIRequired(typeof(Where<PREmployeeDeduct.contribType, NotEqual<ContributionType.employeeDeduction>,
			And<PREmployeeDeduct.cntCalcType, NotEqual<DedCntCalculationMethod.percentOfGross>,
			And<PREmployeeDeduct.dedCalcType, NotEqual<DedCntCalculationMethod.percentOfCustom>,
			And<PREmployeeDeduct.cntCalcType, NotEqual<DedCntCalculationMethod.percentOfNet>,
			And<PREmployeeDeduct.cntCalcType, IsNotNull,
			And<PREmployeeDeduct.cntUseDflt, Equal<False>>>>>>>))]
		[UseDefaultValue(typeof(PREmployeeDeduct.codeID), typeof(PRDeductCode.cntAmount), typeof(PREmployeeDeduct.cntUseDflt), ManageUIEnabled = false)]
		public Decimal? CntAmount { get; set; }
		#endregion
		#region CntPercent
		public abstract class cntPercent : PX.Data.BQL.BqlDecimal.Field<cntPercent> { }
		[PXDBDecimal(MinValue = 0)]
		[PXUIField(DisplayName = "Contribution Percent")]
		[PXDefault]
		[PXUIEnabled(typeof(Where<PREmployeeDeduct.contribType, NotEqual<ContributionType.employeeDeduction>,
			And<PREmployeeDeduct.cntCalcType, NotEqual<DedCntCalculationMethod.fixedAmount>,
			And<PREmployeeDeduct.cntCalcType, NotEqual<DedCntCalculationMethod.amountPerHour>,
			And<PREmployeeDeduct.cntCalcType, IsNotNull>>>>))]
		[PXUIRequired(typeof(Where<PREmployeeDeduct.contribType, NotEqual<ContributionType.employeeDeduction>,
			And<PREmployeeDeduct.cntCalcType, NotEqual<DedCntCalculationMethod.fixedAmount>,
			And<PREmployeeDeduct.cntCalcType, NotEqual<DedCntCalculationMethod.amountPerHour>,
			And<PREmployeeDeduct.cntCalcType, IsNotNull,
			And<PREmployeeDeduct.cntUseDflt, Equal<False>>>>>>))]
		[UseDefaultValue(typeof(PREmployeeDeduct.codeID), typeof(PRDeductCode.cntPercent), typeof(PREmployeeDeduct.cntUseDflt), ManageUIEnabled = false)]
		public Decimal? CntPercent { get; set; }
		#endregion
		#region CntMaxAmount
		public abstract class cntMaxAmount : PX.Data.BQL.BqlDecimal.Field<cntMaxAmount> { }
		[PRCurrency(MinValue = 0)]
		[PXUIField(DisplayName = "Contribution Max")]
		[PXDefault]
		[PXUIEnabled(typeof(Where<PREmployeeDeduct.contribType, NotEqual<ContributionType.employeeDeduction>,
			And<PREmployeeDeduct.cntMaxFreqType, NotEqual<DeductionMaxFrequencyType.noMaximum>>>))]
		[PXUIRequired(typeof(Where<PREmployeeDeduct.contribType, NotEqual<ContributionType.employeeDeduction>,
			And<PREmployeeDeduct.cntMaxFreqType, NotEqual<DeductionMaxFrequencyType.noMaximum>,
			And<PREmployeeDeduct.cntUseDflt, Equal<False>>>>))]
		[UseDefaultValue(typeof(PREmployeeDeduct.codeID), typeof(PRDeductCode.cntMaxAmount), typeof(PREmployeeDeduct.cntUseDflt), ManageUIEnabled = false)]
		public Decimal? CntMaxAmount { get; set; }
		#endregion
		#region CntUseDflt
		public abstract class cntUseDflt : PX.Data.BQL.BqlBool.Field<cntUseDflt> { }
		[PXDBBool]
		[PXDefault(true)]
		[PXUIField(DisplayName = "Use Contribution Defaults")]
		[PXUIEnabled(typeof(Where<PREmployeeDeduct.contribType, NotEqual<ContributionType.employeeDeduction>>))]
		public bool? CntUseDflt { get; set; }
		#endregion
		#region Sequence
		public abstract class sequence : PX.Data.BQL.BqlInt.Field<sequence> { }
		[PXDBInt]
		[PXDefault]
		[PXUIField(DisplayName = "Sequence")]
		public int? Sequence { get; set; }
		#endregion
		#region IsGarnishment
		public abstract class isGarnishment : PX.Data.BQL.BqlBool.Field<isGarnishment> { }
		[PXBool]
		[PXFormula(typeof(Selector<codeID, PRDeductCode.isGarnishment>))]
		[PXUIField(DisplayName = "Is Garnish", Enabled = false)]
		public bool? IsGarnishment { get; set; }
		#endregion
		#region GarnBAccountID
		public abstract class garnBAccountID : PX.Data.BQL.BqlInt.Field<garnBAccountID> { }
		[VendorActive]
		[PXUIEnabled(typeof(isGarnishment))]
		public int? GarnBAccountID { get; set; }
		#endregion
		#region VndInvDescr
		public abstract class vndInvDescr : PX.Data.BQL.BqlString.Field<vndInvDescr> { }
		[PXDBString(60, IsUnicode = true)]
		[PXUIField(DisplayName = "Vendor Invoice Description")]
		[PXUIEnabled(typeof(isGarnishment))]
		public string VndInvDescr { get; set; }
		#endregion
		#region GarnCourtDate
		public abstract class garnCourtDate : PX.Data.BQL.BqlDateTime.Field<garnCourtDate> { }
		[PXDBDate]
		[PXUIField(DisplayName = "Court Date")]
		[PXUIEnabled(typeof(isGarnishment))]
		public DateTime? GarnCourtDate { get; set; }
		#endregion
		#region GarnCourtName
		public abstract class garnCourtName : PX.Data.BQL.BqlString.Field<garnCourtName> { }
		[PXDBString(60, IsUnicode = true)]
		[PXUIField(DisplayName = "Court Name")]
		[PXUIEnabled(typeof(isGarnishment))]
		public string GarnCourtName { get; set; }
		#endregion
		#region GarnDocRefNbr
		public abstract class garnDocRefNbr : PX.Data.BQL.BqlString.Field<garnDocRefNbr> { }
		[PXDBString(30, IsUnicode = true)]
		[PXUIField(DisplayName = "Document ID")]
		[PXUIEnabled(typeof(isGarnishment))]
		public string GarnDocRefNbr { get; set; }
		#endregion
		#region GarnOrigAmount
		public abstract class garnOrigAmount : PX.Data.BQL.BqlDecimal.Field<garnOrigAmount> { }
		[PRCurrency(MinValue = 0)]
		[PXUIField(DisplayName = "Original Amount")]
		[PXUIEnabled(typeof(isGarnishment))]
		public Decimal? GarnOrigAmount { get; set; }
		#endregion
		#region GarnPaidAmount
		public abstract class garnPaidAmount : PX.Data.BQL.BqlDecimal.Field<garnPaidAmount> { }
		[PRCurrency]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = "Amount Paid", Enabled = false)]
		public Decimal? GarnPaidAmount { get; set; }
		#endregion
		#region System Columns
		#region TStamp
		public class tStamp : IBqlField { }
		[PXDBTimestamp]
		public byte[] TStamp { get; set; }
		#endregion
		#region CreatedByID
		public class createdByID : IBqlField { }
		[PXDBCreatedByID]
		public Guid? CreatedByID { get; set; }
		#endregion
		#region CreatedByScreenID
		public class createdByScreenID : IBqlField { }
		[PXDBCreatedByScreenID]
		public string CreatedByScreenID { get; set; }
		#endregion
		#region CreatedDateTime
		public class createdDateTime : IBqlField { }
		[PXDBCreatedDateTime]
		public DateTime? CreatedDateTime { get; set; }
		#endregion
		#region LastModifiedByID
		public class lastModifiedByID : IBqlField { }
		[PXDBLastModifiedByID]
		public Guid? LastModifiedByID { get; set; }
		#endregion
		#region LastModifiedByScreenID
		public class lastModifiedByScreenID : IBqlField { }
		[PXDBLastModifiedByScreenID]
		public string LastModifiedByScreenID { get; set; }
		#endregion
		#region LastModifiedDateTime
		public class lastModifiedDateTime : IBqlField { }
		[PXDBLastModifiedDateTime]
		public DateTime? LastModifiedDateTime { get; set; }
		#endregion
		#endregion
	}
}