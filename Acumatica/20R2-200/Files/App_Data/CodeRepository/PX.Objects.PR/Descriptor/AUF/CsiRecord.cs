using System;

namespace PX.Objects.PR.AUF
{
	public class CsiRecord : CalculationItem
	{
		public CsiRecord(DateTime checkDate, int pimID, string state, DateTime? periodStart, DateTime? periodEnd) :
			base(AufRecordType.Csi, checkDate, pimID, state, periodStart, periodEnd) { }

		public override string ToString()
		{
			object[] lineData =
			{
				State,
				CheckDate,
				PimID,
				AufConstants.UnusedField,
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
	}
}
