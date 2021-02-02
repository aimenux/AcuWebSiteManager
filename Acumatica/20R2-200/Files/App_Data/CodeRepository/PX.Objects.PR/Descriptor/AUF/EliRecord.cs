using System;

namespace PX.Objects.PR.AUF
{
	public class EliRecord : CalculationItem
	{
		public EliRecord(DateTime checkDate, int pimID, string state, DateTime? periodStart, DateTime? periodEnd) :
			base(AufRecordType.Eli, checkDate, pimID, state, periodStart, periodEnd) { }

		public override string ToString()
		{
			object[] lineData =
			{
				State,
				CheckDate,
				PimID,
				TotalWagesAndTips,
				TaxableWagesAndTips,
				TaxableTips,
				WithholdingAmount,
				Hours,
				Days,
				Weeks,
				PeriodStart,
				PeriodEnd
			};

			return FormatLine(lineData);
		}
	}
}
