using PX.Data;
using PX.Data.BQL.Fluent;
using PX.Objects.EP;
using PX.Objects.PM;
using PX.Objects.PR;
using System;

namespace PX.Objects.PR
{
	[PXHidden]
	public partial class PRCalculationEngine : PXGraph<PRCalculationEngine>
	{
		public class FringeBenefitApplicableEarningsQuery : SelectFrom<PREarningDetail>
			   .InnerJoin<PRProjectFringeBenefitRate>.On<PRProjectFringeBenefitRate.projectID.IsEqual<PREarningDetail.projectID>
				   .And<PRProjectFringeBenefitRate.laborItemID.IsEqual<PREarningDetail.labourItemID>>
				   .And<PRProjectFringeBenefitRate.projectTaskID.IsEqual<PREarningDetail.projectTaskID>
					   .Or<PRProjectFringeBenefitRate.projectTaskID.IsNull>>>
			   .InnerJoin<PMProject>.On<PMProject.contractID.IsEqual<PREarningDetail.projectID>>
			   .InnerJoin<EPEarningType>.On<EPEarningType.typeCD.IsEqual<PREarningDetail.typeCD>>
			   .InnerJoin<PREmployee>.On<PREmployee.bAccountID.IsEqual<PREarningDetail.employeeID>>
			   .Where<PREarningDetail.paymentDocType.IsEqual<PRPayment.docType.FromCurrent>
				   .And<PREarningDetail.paymentRefNbr.IsEqual<PRPayment.refNbr.FromCurrent>>
				   .And<PRProjectFringeBenefitRate.effectiveDate.IsLessEqual<PREarningDetail.date>>
				   .And<PREarningDetail.isFringeRateEarning.IsEqual<False>>
				   .And<PREmployee.exemptFromCertifiedReporting.IsNotEqual<True>>>
			   .OrderBy<PRProjectFringeBenefitRate.projectTaskID.Desc, PRProjectFringeBenefitRate.effectiveDate.Desc>.View
		{
			public FringeBenefitApplicableEarningsQuery(PXGraph graph) : base(graph) { }
		}
	}
}
