using PX.Data;
using System;

namespace PX.Objects.PR
{
	[PXCacheName(Messages.PRAcaDeductCode)]
	[Serializable]
	[PXTable(typeof(PRDeductCode.codeID), IsOptional = true)]
	public sealed class PRAcaDeductCode : PXCacheExtension<PRDeductCode>
	{
		#region AcaApplicable
		public abstract class acaApplicable : PX.Data.BQL.BqlBool.Field<acaApplicable> { }
		[PXDBBool]
		[PXDefault(false, PersistingCheck = PXPersistingCheck.Nothing)]
		[PXUIField(DisplayName = "ACA Applicable", Visible = false)]
		[PXUIEnabled(typeof(Where<PRDeductCode.isWorkersCompensation.IsEqual<False>.And<PRDeductCode.isPayableBenefit.IsEqual<False>>>))]
		public bool? AcaApplicable { get; set; }
		#endregion
		#region MinimumIndividualContribution
		public abstract class minimumIndividualContribution : PX.Data.BQL.BqlDecimal.Field<minimumIndividualContribution> { }
		[PRCurrency(MinValue = 0)]
		[PXDefault(TypeCode.Decimal, "0.0", PersistingCheck = PXPersistingCheck.Nothing)]
		[PXUIField(DisplayName = "Minimum Individual Contribution")]
		[PXUIRequired(typeof(Where<PRAcaDeductCode.acaApplicable.IsEqual<True>>))]
		public decimal? MinimumIndividualContribution { get; set; }
		#endregion
	}
}
