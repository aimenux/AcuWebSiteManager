using PX.Data;
using PX.Data.BQL;
using PX.Data.ReferentialIntegrity.Attributes;
using PX.Objects.CS;
using PX.Objects.PM;

namespace PX.Objects.PR
{
	public sealed class PMProjectExtension : PXCacheExtension<PMProject>
	{
		public static bool IsActive()
		{
			return PXAccess.FeatureInstalled<FeaturesSet.payrollModule>();
		}

		#region PayrollWorkLocationID
		public abstract class payrollWorkLocationID : BqlInt.Field<payrollWorkLocationID> { }
		[PXDBInt]
		[PXUIField(DisplayName = "Payroll Work Location", FieldClass = nameof(FeaturesSet.PayrollModule))]
		[PXSelector(typeof(PRLocation.locationID), SubstituteKey = typeof(PRLocation.locationCD))]
		public int? PayrollWorkLocationID { get; set; }
		#endregion
		#region WageAbovePrevailingAnnualizationException
		public abstract class wageAbovePrevailingAnnualizationException : BqlBool.Field<wageAbovePrevailingAnnualizationException> { }
		[PXDBBool]
		[PXUIField(DisplayName = "Excess Pay Rate Annualization Exception")]
		[PXDefault(true, PersistingCheck = PXPersistingCheck.Nothing)]
		public bool? WageAbovePrevailingAnnualizationException { get; set; }
		#endregion
		#region BenefitCodeReceivingFringeRate
		public abstract class benefitCodeReceivingFringeRate : BqlInt.Field<benefitCodeReceivingFringeRate> { }
		[PXDBInt]
		[PXUIField(DisplayName = "Benefit Code to Use for the Fringe Rate")]
		[DeductionActiveSelector(typeof(Where<PRDeductCode.contribType.IsNotEqual<ContributionType.employeeDeduction>
			.And<PRDeductCode.isCertifiedProject.IsEqual<True>>>))]
		[PXForeignReference(typeof(Field<benefitCodeReceivingFringeRate>.IsRelatedTo<PRDeductCode.codeID>))]
		public int? BenefitCodeReceivingFringeRate { get; set; }
		#endregion
		#region FileEmptyCertifiedReport
		public abstract class fileEmptyCertifiedReport : PX.Data.BQL.BqlBool.Field<fileEmptyCertifiedReport> { }
		[PXDBBool]
		[PXUIField(DisplayName = "File Empty Report")]
		[PXDefault(false, PersistingCheck = PXPersistingCheck.Nothing)]
		public bool? FileEmptyCertifiedReport { get; set; }
		#endregion
	}
}
