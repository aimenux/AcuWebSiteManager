using PX.Data;
using PX.Data.BQL;
using PX.Data.BQL.Fluent;
using PX.Objects.EP;
using PX.Objects.PR;
using System;

namespace PX.Objects.PR
{
	[PXHidden]
	public partial class PRCalculationEngine : PXGraph<PRCalculationEngine>
	{
		public class ProjectDeductionQuery : SelectFrom<PREarningDetail>
			.InnerJoin<PRDeductionAndBenefitProjectPackage>.On<PRDeductionAndBenefitProjectPackage.projectID.IsEqual<PREarningDetail.projectID>>
			.InnerJoin<PRDeductCode>.On<PRDeductCode.codeID.IsEqual<PRDeductionAndBenefitProjectPackage.deductionAndBenefitCodeID>>
			.InnerJoin<EPEarningType>.On<EPEarningType.typeCD.IsEqual<PREarningDetail.typeCD>>
			.Where<PREarningDetail.paymentDocType.IsEqual<PRPayment.docType.FromCurrent>
				.And<PREarningDetail.paymentRefNbr.IsEqual<PRPayment.refNbr.FromCurrent>>
				.And<PREarningDetail.isFringeRateEarning.IsNotEqual<True>>
				.And<PRDeductionAndBenefitProjectPackage.deductionAndBenefitCodeID.IsEqual<P.AsInt>>
				.And<PRDeductionAndBenefitProjectPackage.effectiveDate.IsLessEqual<PRPayment.transactionDate.FromCurrent>>
				.And<PRDeductCode.isActive.IsEqual<True>>
				.And<PREarningDetail.labourItemID.IsEqual<PRDeductionAndBenefitProjectPackage.laborItemID>
					.Or<PRDeductionAndBenefitProjectPackage.laborItemID.IsNull
						.And<PREarningDetail.labourItemID.IsNull
							.Or<PREarningDetail.labourItemID.IsNotInSubselect<SearchFor<PRDeductionAndBenefitProjectPackage.laborItemID>
								.Where<PRDeductionAndBenefitProjectPackage.projectID.IsEqual<PREarningDetail.projectID>
									.And<PRDeductionAndBenefitProjectPackage.laborItemID.IsNotNull>
									.And<PRDeductionAndBenefitProjectPackage.deductionAndBenefitCodeID.IsEqual<PRDeductCode.codeID>>
									.And<PRDeductionAndBenefitProjectPackage.effectiveDate.IsLessEqual<PRPayment.transactionDate.FromCurrent>>>>>>>>>.View
		{
			public ProjectDeductionQuery(PXGraph graph) : base(graph) { }
		}
	}
}
