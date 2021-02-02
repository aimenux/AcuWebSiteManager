using PX.Data;
using PX.Data.BQL.Fluent;
using PX.Data.ReferentialIntegrity.Attributes;
using PX.Objects.CS;
using PX.Objects.PM;
using System;

namespace PX.Objects.PR
{
	[PXCacheName(Messages.PRPaymentWCPremium)]
	[Serializable]
	public class PRPaymentWCPremium : IBqlTable
	{
		#region DocType
		public abstract class docType : PX.Data.BQL.BqlString.Field<docType> { }
		[PXDBString(3, IsFixed = true, IsKey = true)]
		[PXUIField(DisplayName = "Payment Doc. Type")]
		[PXDBDefault(typeof(PRPayment.docType))]
		public string DocType { get; set; }
		#endregion
		#region RefNbr
		public abstract class refNbr : PX.Data.BQL.BqlString.Field<refNbr> { }
		[PXDBString(15, IsUnicode = true, IsKey = true)]
		[PXUIField(DisplayName = "Payment Ref. Number")]
		[PXDBDefault(typeof(PRPayment.refNbr))]
		[PXParent(typeof(Select<PRPayment, Where<PRPayment.docType, Equal<Current<docType>>, And<PRPayment.refNbr, Equal<Current<refNbr>>>>>))]
		public String RefNbr { get; set; }
		#endregion
		#region WorkCodeID
		public abstract class workCodeID : PX.Data.BQL.BqlString.Field<workCodeID> { }
		[PMWorkCode(FieldClass = null, DisplayName = "WCC Code", IsKey = true)]
		[PXDefault]
		[PXForeignReference(typeof(Field<workCodeID>.IsRelatedTo<PMWorkCode.workCodeID>))]
		public string WorkCodeID { get; set; }
		#endregion
		#region DeductCodeID
		public abstract class deductCodeID : PX.Data.BQL.BqlInt.Field<deductCodeID> { }
		[PXDBInt(IsKey = true)]
		[PXUIField(DisplayName = "Deduction Code")]
		[PXSelector(typeof(SearchFor<PRDeductCode.codeID>
			.Where<PRDeductCode.isWorkersCompensation.IsEqual<True>>),
			SubstituteKey = typeof(PRDeductCode.codeCD), DescriptionField = typeof(PRDeductCode.description))]
		[PXDefault]
		[PXForeignReference(typeof(Field<deductCodeID>.IsRelatedTo<PRDeductCode.codeID>))]
		public int? DeductCodeID { get; set; }
		#endregion
		#region DeductionRate
		public abstract class deductionRate : PX.Data.BQL.BqlDecimal.Field<deductionRate> { }
		[PXDBDecimal(6, MinValue = 0)]
		[PXUIField(DisplayName = "Deduction Rate")]
		[PXUIVisible(typeof(Where<WCDeductionColumnVisibilityEvaluator, Equal<True>>))]
		[PXUIEnabled(typeof(Where<contribType.IsNotEqual<ContributionType.employerContribution>>))]
		public decimal? DeductionRate
		{ 
			[PXDependsOnFields(typeof(contribType))]
			get
			{
				if (ContribType != ContributionType.EmployerContribution)
				{
					return _DeductionRate;
				}
				return null;
			}
			set
			{
				_DeductionRate = value;
			}
		}
		private decimal? _DeductionRate;
		#endregion
		#region Rate
		public abstract class rate : PX.Data.BQL.BqlDecimal.Field<rate> { }
		[PXDBDecimal(6, MinValue = 0)]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = "Benefit Rate")]
		public decimal? Rate { get; set; }
		#endregion
		#region DeductionAmount
		public abstract class deductionAmount : PX.Data.BQL.BqlDecimal.Field<deductionAmount> { }
		[PRCurrency]
		[PXUIField(DisplayName = "Deduction Amount")]
		[PXUIEnabled(typeof(Where<contribType.IsNotEqual<ContributionType.employerContribution>>))]
		public decimal? DeductionAmount { get; set; }
		#endregion
		#region Amount
		public abstract class amount : PX.Data.BQL.BqlDecimal.Field<amount> { }
		[PRCurrency]
		[PXUIField(DisplayName = "Benefit Amount")]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIEnabled(typeof(Where<contribType.IsNotEqual<ContributionType.employeeDeduction>>))]
		public decimal? Amount { get; set; }
		#endregion
		#region RegularWageBaseAmount
		public abstract class regularWageBaseAmount : PX.Data.BQL.BqlDecimal.Field<regularWageBaseAmount> { }
		[PRCurrency]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = "Applicable Regular Wages")]
		public decimal? RegularWageBaseAmount { get; set; }
		#endregion
		#region OvertimeWageBaseAmount
		public abstract class overtimeWageBaseAmount : PX.Data.BQL.BqlDecimal.Field<overtimeWageBaseAmount> { }
		[PRCurrency]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = "Applicable Overtime Wages")]
		public decimal? OvertimeWageBaseAmount { get; set; }
		#endregion
		#region WageBaseAmount
		public abstract class wageBaseAmount : PX.Data.BQL.BqlDecimal.Field<wageBaseAmount> { }
		[PXDecimal]
		[PXFormula(typeof(Add<regularWageBaseAmount, overtimeWageBaseAmount>))]
		[PXUIField(DisplayName = "Total Applicable Wages", Enabled = false)]
		public decimal? WageBaseAmount { get; set; }
		#endregion
		#region RegularWageBaseHours
		public abstract class regularWageBaseHours : PX.Data.BQL.BqlDecimal.Field<regularWageBaseHours> { }
		[PXDBDecimal]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = "Applicable Regular Hours")]
		public decimal? RegularWageBaseHours { get; set; }
		#endregion
		#region OvertimeWageBaseHours
		public abstract class overtimeWageBaseHours : PX.Data.BQL.BqlDecimal.Field<overtimeWageBaseHours> { }
		[PXDBDecimal]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = "Applicable Overtime Hours")]
		public decimal? OvertimeWageBaseHours { get; set; }
		#endregion
		#region WageBaseHours
		public abstract class wageBaseHours : PX.Data.BQL.BqlDecimal.Field<wageBaseHours> { }
		[PXDecimal]
		[PXFormula(typeof(Add<regularWageBaseHours, overtimeWageBaseHours>))]
		[PXUIField(DisplayName = "Total Applicable Hours", Enabled = false)]
		public decimal? WageBaseHours { get; set; }
		#endregion

		#region ContribType
		public abstract class contribType : PX.Data.BQL.BqlString.Field<contribType> { }
		[PXString(3)]
		[PXUIField(DisplayName = "Contribution Type", Visible = false)]
		[PXFormula(typeof(Selector<deductCodeID, PRDeductCode.contribType>))]
		public string ContribType { get; set; }
		#endregion
		#region DeductionCalcType
		public abstract class deductionCalcType : PX.Data.BQL.BqlString.Field<deductionCalcType> { }
		[PXString(3)]
		[DedCntCalculationMethod.List]
		[PXUIField(DisplayName = "Deduction Calculation Method", Enabled = false)]
		[PXUIVisible(typeof(Where<WCDeductionColumnVisibilityEvaluator, Equal<True>>))]
		[PXFormula(typeof(Switch<Case<Where<contribType.IsNotEqual<ContributionType.employerContribution>>, Selector<deductCodeID, PRDeductCode.dedCalcType>>, Null>))]
		public string DeductionCalcType { get; set; }
		#endregion
		#region BenefitCalcType
		public abstract class benefitCalcType : PX.Data.BQL.BqlString.Field<benefitCalcType> { }
		[PXString(3)]
		[DedCntCalculationMethod.List]
		[PXUIField(DisplayName = "Benefit Calculation Method", Enabled = false)]
		[PXFormula(typeof(Selector<deductCodeID, PRDeductCode.cntCalcType>))]
		public string BenefitCalcType { get; set; }
		#endregion

		#region System Columns
		#region TStamp
		public abstract class tStamp : PX.Data.BQL.BqlByteArray.Field<tStamp> { }
		[PXDBTimestamp]
		public byte[] TStamp { get; set; }
		#endregion
		#region CreatedByID
		public abstract class createdByID : PX.Data.BQL.BqlGuid.Field<createdByID> { }
		[PXDBCreatedByID]
		public Guid? CreatedByID { get; set; }
		#endregion
		#region CreatedByScreenID
		public abstract class createdByScreenID : PX.Data.BQL.BqlString.Field<createdByScreenID> { }
		[PXDBCreatedByScreenID]
		public string CreatedByScreenID { get; set; }
		#endregion
		#region CreatedDateTime
		public abstract class createdDateTime : PX.Data.BQL.BqlDateTime.Field<createdDateTime> { }
		[PXDBCreatedDateTime]
		public DateTime? CreatedDateTime { get; set; }
		#endregion
		#region LastModifiedByID
		public abstract class lastModifiedByID : PX.Data.BQL.BqlGuid.Field<lastModifiedByID> { }
		[PXDBLastModifiedByID]
		public Guid? LastModifiedByID { get; set; }
		#endregion
		#region LastModifiedByScreenID
		public abstract class lastModifiedByScreenID : PX.Data.BQL.BqlString.Field<lastModifiedByScreenID> { }
		[PXDBLastModifiedByScreenID]
		public string LastModifiedByScreenID { get; set; }
		#endregion
		#region LastModifiedDateTime
		public abstract class lastModifiedDateTime : PX.Data.BQL.BqlDateTime.Field<lastModifiedDateTime> { }
		[PXDBLastModifiedDateTime]
		public DateTime? LastModifiedDateTime { get; set; }
		#endregion
		#endregion
	}
}
