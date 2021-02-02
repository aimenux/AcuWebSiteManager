using PX.Data;
using PX.Data.BQL;
using PX.Data.ReferentialIntegrity.Attributes;
using PX.Objects.IN;
using PX.Objects.PM;
using System;

namespace PX.Objects.PR
{
	[PXCacheName(Messages.PRDeductionAndBenefitProjectPackage)]
	[Serializable]
	public class PRDeductionAndBenefitProjectPackage : IBqlTable
	{
		#region RecordID
		public abstract class recordID : BqlInt.Field<recordID> { }
		[PXDBIdentity(IsKey = true)]
		public virtual int? RecordID { get; set; }
		#endregion
		#region ProjectID
		public abstract class projectID : BqlInt.Field<projectID> { }
		[PXDBInt]
		[PXDBDefault(typeof(PMProject.contractID))]
		[PXParent(typeof(Select<PMProject, Where<PMProject.contractID, Equal<projectID>>>))]
		public int? ProjectID { get; set; }
		#endregion
		#region DeductionAndBenefitCodeID
		public abstract class deductionAndBenefitCodeID : BqlInt.Field<deductionAndBenefitCodeID> { }
		[PXDBInt]
		[PXDefault]
		[PXUIField(DisplayName = "Deduction And Benefit Code")]
		[DeductionActiveSelector(typeof(Where<PRDeductCode.isCertifiedProject.IsEqual<True>>))]
		[PXRestrictor(
			typeof(Where<Brackets<PRDeductCode.contribType.IsEqual<ContributionType.employerContribution>
					.Or<PRDeductCode.dedCalcType.IsNotEqual<DedCntCalculationMethod.percentOfNet>>>
				.And<PRDeductCode.contribType.IsEqual<ContributionType.employeeDeduction>
					.Or<PRDeductCode.cntCalcType.IsNotEqual<DedCntCalculationMethod.percentOfNet>>>>),
			Messages.PercentOfNetInCertifiedProject)]
		[DedAndBenPackageEventSubscriber(typeof(deductionAmount), typeof(deductionRate), typeof(benefitAmount), typeof(benefitRate), typeof(effectiveDate))]
		[PXForeignReference(typeof(Field<deductionAndBenefitCodeID>.IsRelatedTo<PRDeductCode.codeID>))]
		public int? DeductionAndBenefitCodeID { get; set; }
		#endregion
		#region DeductionAmount
		public abstract class deductionAmount : BqlDecimal.Field<deductionAmount> { }
		[PXDBDecimal(MinValue = 0)]
		[DefaultDedBenValue(
			typeof(deductionAndBenefitCodeID),
			typeof(PRDeductCode.dedCalcType),
			new string[] { DedCntCalculationMethod.FixedAmount, DedCntCalculationMethod.AmountPerHour },
			typeof(PRDeductCode.contribType),
			new string[] { ContributionType.EmployeeDeduction, ContributionType.BothDeductionAndContribution },
			typeof(PRDeductCode.dedAmount))]
		[PXUIField(DisplayName = "Deduction Amount")]
		public decimal? DeductionAmount { get; set; }
		#endregion
		#region DeductionRate
		public abstract class deductionRate : BqlDecimal.Field<deductionRate> { }
		[PXDBDecimal(MinValue = 0)]
		[DefaultDedBenValue(
			typeof(deductionAndBenefitCodeID),
			typeof(PRDeductCode.dedCalcType),
			new string[] { DedCntCalculationMethod.PercentOfGross, DedCntCalculationMethod.PercentOfCustom },
			typeof(PRDeductCode.contribType),
			new string[] { ContributionType.EmployeeDeduction, ContributionType.BothDeductionAndContribution },
			typeof(PRDeductCode.dedPercent))]
		[PXUIField(DisplayName = "Deduction Percent")]
		public decimal? DeductionRate { get; set; }
		#endregion
		#region BenefitAmount
		public abstract class benefitAmount : BqlDecimal.Field<benefitAmount> { }
		[PXDBDecimal(MinValue = 0)]
		[DefaultDedBenValue(
			typeof(deductionAndBenefitCodeID),
			typeof(PRDeductCode.cntCalcType),
			new string[] { DedCntCalculationMethod.FixedAmount, DedCntCalculationMethod.AmountPerHour },
			typeof(PRDeductCode.contribType),
			new string[] { ContributionType.EmployerContribution, ContributionType.BothDeductionAndContribution },
			typeof(PRDeductCode.cntAmount))]
		[PXUIField(DisplayName = "Contribution Amount")]
		public decimal? BenefitAmount { get; set; }
		#endregion
		#region BenefitRate
		public abstract class benefitRate : BqlDecimal.Field<benefitRate> { }
		[PXDBDecimal(MinValue = 0)]
		[DefaultDedBenValue(
			typeof(deductionAndBenefitCodeID),
			typeof(PRDeductCode.cntCalcType),
			new string[] { DedCntCalculationMethod.PercentOfGross, DedCntCalculationMethod.PercentOfCustom },
			typeof(PRDeductCode.contribType),
			new string[] { ContributionType.EmployerContribution, ContributionType.BothDeductionAndContribution },
			typeof(PRDeductCode.cntPercent))]
		[PXUIField(DisplayName = "Contribution Percent")]
		public decimal? BenefitRate { get; set; }
		#endregion
		#region EffectiveDate
		public abstract class effectiveDate : BqlDateTime.Field<effectiveDate> { }
		[PXDefault]
		[PXDBDate]
		[PXUIField(DisplayName = "Effective Date")]
		[PXCheckUnique(typeof(projectID), typeof(deductionAndBenefitCodeID), typeof(laborItemID), ClearOnDuplicate = false)]
		public virtual DateTime? EffectiveDate { get; set; }
		#endregion
		#region LaborItemID
		public abstract class laborItemID : Data.BQL.BqlInt.Field<laborItemID> { }
		[PMLaborItem(typeof(projectID), null, null)]
		[PXForeignReference(typeof(Field<laborItemID>.IsRelatedTo<InventoryItem.inventoryID>))]
		public virtual int? LaborItemID { get; set; }
		#endregion
		#region System Columns
		#region TStamp
		public class tStamp : IBqlField { }
		[PXDBTimestamp()]
		public byte[] TStamp { get; set; }
		#endregion
		#region CreatedByID
		public class createdByID : IBqlField { }
		[PXDBCreatedByID()]
		public Guid? CreatedByID { get; set; }
		#endregion
		#region CreatedByScreenID
		public class createdByScreenID : IBqlField { }
		[PXDBCreatedByScreenID()]
		public string CreatedByScreenID { get; set; }
		#endregion
		#region CreatedDateTime
		public class createdDateTime : IBqlField { }
		[PXDBCreatedDateTime()]
		public DateTime? CreatedDateTime { get; set; }
		#endregion
		#region LastModifiedByID
		public class lastModifiedByID : IBqlField { }
		[PXDBLastModifiedByID()]
		public Guid? LastModifiedByID { get; set; }
		#endregion
		#region LastModifiedByScreenID
		public class lastModifiedByScreenID : IBqlField { }
		[PXDBLastModifiedByScreenID()]
		public string LastModifiedByScreenID { get; set; }
		#endregion
		#region LastModifiedDateTime
		public class lastModifiedDateTime : IBqlField { }
		[PXDBLastModifiedDateTime()]
		public DateTime? LastModifiedDateTime { get; set; }
		#endregion
		#endregion
	}
}
