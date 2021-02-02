using PX.Data;
using PX.Objects.PR;
using System;

namespace PX.Objects.PR
{
	[PXHidden]
	public partial class PRCalculationEngine : PXGraph<PRCalculationEngine>
	{
		protected struct FringeAmountInfo
		{
			public FringeAmountInfo(PRDeductCode deductCode, decimal totalProjectFringeAmount, decimal projectFringeAmountAsBenefit)
			{
				this.DeductCode = deductCode;
				this.TotalProjectFringeAmount = totalProjectFringeAmount;
				this.ProjectFringeAmountAsBenefit = projectFringeAmountAsBenefit;
			}

			public PRDeductCode DeductCode;
			public decimal TotalProjectFringeAmount;
			public decimal ProjectFringeAmountAsBenefit;
		}
	}
}
