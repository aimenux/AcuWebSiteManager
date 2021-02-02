using PX.Data;
using PX.Objects.PR;
using System;

namespace PX.Objects.PR
{
	[PXHidden]
	public partial class PRCalculationEngine : PXGraph<PRCalculationEngine>
	{
		protected struct FringeEarningDecreasingRateKey
		{
			public FringeEarningDecreasingRateKey(int? projectID, string earningTypeCD, int? laborItemID, int? projectTaskID, bool isWageAbovePrevailing)
			{
				this.ProjectID = projectID;
				this.EarningTypeCD = earningTypeCD;
				this.LaborItemID = laborItemID;
				this.ProjectTaskID = projectTaskID;
				this.IsWageAbovePrevailing = isWageAbovePrevailing;
			}

			public int? ProjectID;
			public string EarningTypeCD;
			public int? LaborItemID;
			public int? ProjectTaskID;
			public bool IsWageAbovePrevailing;
		}
	}
}
