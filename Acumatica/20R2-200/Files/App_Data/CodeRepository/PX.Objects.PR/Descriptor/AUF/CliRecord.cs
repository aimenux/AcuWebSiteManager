using System;

namespace PX.Objects.PR.AUF
{
	public class CliRecord : CalculationItem
	{
		public CliRecord(DateTime checkDate, int pimID, string state, DateTime? periodStart, DateTime? periodEnd) :
			base(AufRecordType.Cli, checkDate, pimID, state, periodStart, periodEnd) { }

		public override string ToString()
		{
			object[] lineData =
			{
				State,
				CheckDate,
				PimID,
				AccountNumber,
				TotalWagesAndTips,
				TaxableWagesAndTips,
				TaxableTips,
				WithholdingAmount,
				Rate,
				Hours,
				Days,
				Weeks,
				PeriodStart,
				PeriodEnd
			};

			return FormatLine(lineData);
		}

		public string AccountNumber { get; set; }
	}
}
