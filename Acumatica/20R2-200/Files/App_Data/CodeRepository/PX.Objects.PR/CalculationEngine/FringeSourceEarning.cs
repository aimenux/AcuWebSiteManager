using PX.Data;
using PX.Objects.PM;
using PX.Objects.PR;
using System;

namespace PX.Objects.PR
{
	[PXHidden]
	public partial class PRCalculationEngine : PXGraph<PRCalculationEngine>
	{
		protected struct FringeSourceEarning
		{
			public FringeSourceEarning(PREarningDetail earning, PMProject project, decimal calculatedFringeRate, decimal setupFringeRate)
			{
				this.Earning = earning;
				this.Project = project;
				this.CalculatedFringeRate = calculatedFringeRate;
				this.SetupFringeRate = setupFringeRate;
			}

			public PREarningDetail Earning;
			public PMProject Project;
			public decimal CalculatedFringeRate;
			public decimal SetupFringeRate;
		}
	}
}
