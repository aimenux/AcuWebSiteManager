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
		public class UnionDeductionQuery : SelectFrom<PREarningDetail>
			   .InnerJoin<PRDeductionAndBenefitUnionPackage>.On<PRDeductionAndBenefitUnionPackage.unionID.IsEqual<PREarningDetail.unionID>>
			   .InnerJoin<PRDeductCode>.On<PRDeductCode.codeID.IsEqual<PRDeductionAndBenefitUnionPackage.deductionAndBenefitCodeID>>
			   .InnerJoin<EPEarningType>.On<EPEarningType.typeCD.IsEqual<PREarningDetail.typeCD>>
			   .Where<PREarningDetail.paymentDocType.IsEqual<PRPayment.docType.FromCurrent>
				   .And<PREarningDetail.paymentRefNbr.IsEqual<PRPayment.refNbr.FromCurrent>>
				   .And<PREarningDetail.isFringeRateEarning.IsNotEqual<True>>
				   .And<PRDeductionAndBenefitUnionPackage.deductionAndBenefitCodeID.IsEqual<P.AsInt>>
				   .And<PRDeductionAndBenefitUnionPackage.effectiveDate.IsLessEqual<PRPayment.transactionDate.FromCurrent>>
				   .And<PRDeductCode.isActive.IsEqual<True>>
				   .And<PREarningDetail.labourItemID.IsEqual<PRDeductionAndBenefitUnionPackage.laborItemID>
					   .Or<PRDeductionAndBenefitUnionPackage.laborItemID.IsNull
						   .And<PREarningDetail.labourItemID.IsNull
							   .Or<PREarningDetail.labourItemID.IsNotInSubselect<SearchFor<PRDeductionAndBenefitUnionPackage.laborItemID>
								   .Where<PRDeductionAndBenefitUnionPackage.unionID.IsEqual<PREarningDetail.unionID>
									   .And<PRDeductionAndBenefitUnionPackage.laborItemID.IsNotNull>
									   .And<PRDeductionAndBenefitUnionPackage.deductionAndBenefitCodeID.IsEqual<PRDeductCode.codeID>>
									   .And<PRDeductionAndBenefitUnionPackage.effectiveDate.IsLessEqual<PRPayment.transactionDate.FromCurrent>>>>>>>>>.View
		{
			public UnionDeductionQuery(PXGraph graph) : base(graph) { }
		}
	}
}
