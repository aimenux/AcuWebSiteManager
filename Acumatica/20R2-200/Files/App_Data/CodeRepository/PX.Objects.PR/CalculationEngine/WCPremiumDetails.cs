using PX.Data;
using PX.Objects.PR;
using System;

namespace PX.Objects.PR
{
	[PXHidden]
	public partial class PRCalculationEngine : PXGraph<PRCalculationEngine>
	{
		protected class WCPremiumDetails
		{
			public void Add(WCPremiumDetails other)
			{
				this.RegularHours += other.RegularHours;
				this.RegularWageBase += other.RegularWageBase;
				this.OvertimeHours += other.OvertimeHours;
				this.OvertimeWageBase += other.OvertimeWageBase;
				this.ApplicableEarningAmountForDed += other.ApplicableEarningAmountForDed;
				this.ApplicableEarningAmountForBen += other.ApplicableEarningAmountForBen;
				this.ApplicableEarningHoursForDed += other.ApplicableEarningHoursForDed;
				this.ApplicableEarningHoursForBen += other.ApplicableEarningHoursForBen;
			}

			public decimal RegularHours = 0m;
			public decimal RegularWageBase = 0m;
			public decimal OvertimeHours = 0m;
			public decimal OvertimeWageBase = 0m;
			public decimal ApplicableEarningAmountForDed = 0m;
			public decimal ApplicableEarningAmountForBen = 0m;
			public decimal ApplicableEarningHoursForDed = 0m;
			public decimal ApplicableEarningHoursForBen = 0m;
		}
	}
}
