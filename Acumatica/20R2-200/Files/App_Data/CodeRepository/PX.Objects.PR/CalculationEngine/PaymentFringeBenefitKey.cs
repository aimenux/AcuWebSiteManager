using PX.Data;
using PX.Objects.PR;
using System;

namespace PX.Objects.PR
{
	[PXHidden]
	public partial class PRCalculationEngine : PXGraph<PRCalculationEngine>
	{
		protected struct PaymentFringeBenefitKey
		{
			public PaymentFringeBenefitKey(PRPaymentFringeBenefit paymentFringeBenefit) : this(
				paymentFringeBenefit.ProjectID,
				paymentFringeBenefit.LaborItemID,
				paymentFringeBenefit.ProjectTaskID)
			{ }

			public PaymentFringeBenefitKey(int? projectID, int? laborItemID, int? projectTaskID)
			{
				this.ProjectID = projectID;
				this.LaborItemID = laborItemID;
				this.ProjectTaskID = projectTaskID;
			}

			public int? ProjectID;
			public int? LaborItemID;
			public int? ProjectTaskID;
		}
	}
}
