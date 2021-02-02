using PX.Data;

namespace PX.Objects.PR
{
	[PXCacheName(Messages.PRAcaUpdateCompanyMonthFilter)]
	public class PRAcaUpdateCompanyMonthFilter : IBqlTable
	{
		#region UpdateCertificationOfEligibility
		public abstract class updateCertificationOfEligibility : PX.Data.BQL.BqlBool.Field<updateCertificationOfEligibility> { }
		[PXBool]
		[PXUIField(DisplayName = "Certification of Eligibility")]
		public bool? UpdateCertificationOfEligibility { get; set; }
		#endregion
		#region CertificationOfEligibility
		public abstract class certificationOfEligibility : PX.Data.BQL.BqlString.Field<certificationOfEligibility> { }
		[PXString(3)]
		[AcaCertificationOfEligibility.List]
		[PXUIEnabled(typeof(Where<PRAcaUpdateCompanyMonthFilter.updateCertificationOfEligibility.IsEqual<True>>))]
		[PXUIField(DisplayName = "")]
		public string CertificationOfEligibility { get; set; }
		#endregion

		#region UpdateSelfInsured
		public abstract class updateSelfInsured : PX.Data.BQL.BqlBool.Field<updateSelfInsured> { }
		[PXBool]
		[PXUIField(DisplayName = "Self-Insured")]
		public bool? UpdateSelfInsured { get; set; }
		#endregion
		#region SelfInsured
		public abstract class selfInsured : PX.Data.BQL.BqlBool.Field<selfInsured> { }
		[PXBool]
		[PXUnboundDefault(false)]
		[PXUIEnabled(typeof(Where<PRAcaUpdateCompanyMonthFilter.updateSelfInsured.IsEqual<True>>))]
		[PXUIField(DisplayName = "")]
		public bool? SelfInsured { get; set; }
		#endregion
	}
}
