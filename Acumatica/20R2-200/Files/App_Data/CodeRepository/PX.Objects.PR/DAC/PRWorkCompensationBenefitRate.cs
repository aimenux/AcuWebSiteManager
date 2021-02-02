using PX.Data;
using PX.Data.BQL;
using PX.Data.BQL.Fluent;
using PX.Data.ReferentialIntegrity.Attributes;
using PX.Objects.PM;
using System;

namespace PX.Objects.PR
{
	[PXCacheName(Messages.PRWorkCompensationBenefitRate)]
	[Serializable]
	public class PRWorkCompensationBenefitRate : IBqlTable
	{
		#region WorkCodeID
		public abstract class workCodeID : PX.Data.BQL.BqlString.Field<workCodeID> { }
		[PMWorkCode(FieldClass = null, DisplayName = "WCC Code", IsKey = true)]
		[PXDefault]
		public string WorkCodeID { get; set; }
		#endregion
		#region DeductCodeID
		public abstract class deductCodeID : PX.Data.BQL.BqlInt.Field<deductCodeID> { }
		[PXDBInt(IsKey = true)]
		[PXDefault]
		[PXUIField(DisplayName = "Deduction Code")]
		[DeductionActiveSelector(typeof(Where<PRDeductCode.isWorkersCompensation.IsEqual<True>>))]
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
		#region EffectiveDate
		public abstract class effectiveDate : PX.Data.BQL.BqlDateTime.Field<effectiveDate> { }
		[PXDBDate(IsKey = true)]
		[PXDefault(typeof(AccessInfo.businessDate))]
		[PXUIField(DisplayName = "Effective Date")]
		public virtual DateTime? EffectiveDate { get; set; }
		#endregion
		#region IsActive
		public abstract class isActive : PX.Data.BQL.BqlBool.Field<isActive> { }
		[EffectiveDateActive(
			typeof(effectiveDate),
			typeof(SelectFrom<PRDeductCode>
				.Where<PRDeductCode.codeID.IsEqual<P.AsInt>
					.And<PRDeductCode.isActive.IsEqual<True>>>),
			typeof(deductCodeID))]
		public virtual bool? IsActive { get; set; }
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

		#region System Columns
		#region CreatedByID
		[PXDBCreatedByID()]
		public virtual Guid? CreatedByID { get; set; }
		public abstract class createdByID : PX.Data.BQL.BqlGuid.Field<createdByID> { }
		#endregion
		#region CreatedByScreenID
		[PXDBCreatedByScreenID()]
		public virtual string CreatedByScreenID { get; set; }
		public abstract class createdByScreenID : PX.Data.BQL.BqlString.Field<createdByScreenID> { }
		#endregion
		#region CreatedDateTime
		[PXDBCreatedDateTime()]
		public virtual DateTime? CreatedDateTime { get; set; }
		public abstract class createdDateTime : PX.Data.BQL.BqlDateTime.Field<createdDateTime> { }
		#endregion
		#region LastModifiedByID
		[PXDBLastModifiedByID()]
		public virtual Guid? LastModifiedByID { get; set; }
		public abstract class lastModifiedByID : PX.Data.BQL.BqlGuid.Field<lastModifiedByID> { }
		#endregion
		#region LastModifiedByScreenID
		[PXDBLastModifiedByScreenID()]
		public virtual string LastModifiedByScreenID { get; set; }
		public abstract class lastModifiedByScreenID : PX.Data.BQL.BqlString.Field<lastModifiedByScreenID> { }
		#endregion
		#region LastModifiedDateTime
		[PXDBLastModifiedDateTime()]
		public virtual DateTime? LastModifiedDateTime { get; set; }
		public abstract class lastModifiedDateTime : PX.Data.BQL.BqlDateTime.Field<lastModifiedDateTime> { }
		#endregion
		#endregion System Columns
	}
}