using PX.Data;
using PX.Objects.PR;
using System;

namespace PX.Objects.PR
{
	[PXHidden]
	public partial class PRCalculationEngine : PXGraph<PRCalculationEngine>
	{
		protected struct FringeBenefitDecreasingRateKey
		{
			public FringeBenefitDecreasingRateKey(int? projectID, int? deductCodeID, int? laborItemID, int? projectTaskID)
			{
				this.ProjectID = projectID;
				this.DeductCodeID = deductCodeID;
				this.LaborItemID = laborItemID;
				this.ProjectTaskID = projectTaskID;
			}

			public int? ProjectID;
			public int? DeductCodeID;
			public int? LaborItemID;
			public int? ProjectTaskID;
		}
	}
}
