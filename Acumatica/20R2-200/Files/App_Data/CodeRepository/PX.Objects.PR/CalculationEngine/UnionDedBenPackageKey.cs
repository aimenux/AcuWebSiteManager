using PX.Data;
using PX.Objects.PR;
using System;

namespace PX.Objects.PR
{
	[PXHidden]
	public partial class PRCalculationEngine : PXGraph<PRCalculationEngine>
	{
		protected struct UnionDedBenPackageKey
		{
			public UnionDedBenPackageKey(string unionID, int? deductCodeID, int? laborItemID)
			{
				this.UnionID = unionID;
				this.DeductCodeID = deductCodeID;
				this.LaborItemID = laborItemID;
			}

			public string UnionID;
			public int? DeductCodeID;
			public int? LaborItemID;
		}
	}
}
