using PX.Data;
using PX.Objects.EP;
using PX.Objects.PR;
using System;

namespace PX.Objects.PR
{
	[PXHidden]
	public partial class PRCalculationEngine : PXGraph<PRCalculationEngine>
	{
		protected class PackageDedBenCalculation
		{
			public decimal RegularHours { get; private set; }
			public decimal OvertimeHours { get; private set; }
			public decimal RegularHoursAmount { get; private set; }
			public decimal OvertimeHoursAmount { get; private set; }
			public decimal? DeductionAmount { get; set; } = 0m;
			public decimal? BenefitAmount { get; set; } = 0m;

			public PackageDedBenCalculation(PREarningDetail earning, EPEarningType earningType)
			{
				if (earningType.IsOvertime == true)
				{
					OvertimeHours = earning.Hours.GetValueOrDefault();
					OvertimeHoursAmount = earning.Amount.GetValueOrDefault();
					RegularHours = 0m;
					RegularHoursAmount = 0m;
				}
				else
				{
					RegularHours = earning.Hours.GetValueOrDefault();
					RegularHoursAmount = earning.Amount.GetValueOrDefault();
					OvertimeHours = 0m;
					OvertimeHoursAmount = 0m;
				}
			}

			public void Add(PackageDedBenCalculation other)
			{
				RegularHours += other.RegularHours;
				OvertimeHours += other.OvertimeHours;
				RegularHoursAmount += other.RegularHoursAmount;
				OvertimeHoursAmount += other.OvertimeHoursAmount;
				DeductionAmount += other.DeductionAmount;
				BenefitAmount += other.BenefitAmount;
			}
		}
	}
}
