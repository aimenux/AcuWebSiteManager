using PX.Data;
using PX.Objects.PR;
using System;

namespace PX.Objects.PR
{
	[PXHidden]
	public partial class PRCalculationEngine : PXGraph<PRCalculationEngine>
	{
		protected struct ProjectDedBenPackageKey
		{
			public ProjectDedBenPackageKey(int? projectID, int? deductCodeID, int? laborItemID)
			{
				this.ProjectID = projectID;
				this.DeductCodeID = deductCodeID;
				this.LaborItemID = laborItemID;
			}

			public int? ProjectID;
			public int? DeductCodeID;
			public int? LaborItemID;
		}
	}
}
