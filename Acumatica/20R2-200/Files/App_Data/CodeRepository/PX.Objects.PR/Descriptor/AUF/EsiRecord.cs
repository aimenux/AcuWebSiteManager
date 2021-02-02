using System;

namespace PX.Objects.PR.AUF
{
	public class EsiRecord : CalculationItem
	{
		public EsiRecord(DateTime checkDate, int pimID, string state, DateTime? periodStart, DateTime? periodEnd) :
			base(AufRecordType.Esi, checkDate, pimID, state, periodStart, periodEnd) { }

		public override string ToString()
		{
			bool isPuertoRico = State == LocationConstants.PuertoRicoStateAbbr;

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
				PeriodEnd,
				isPuertoRico ? Commissions : new decimal?(),
				isPuertoRico ? Allowances : new decimal?(),
				AccountNumber
			};

			return FormatLine(lineData);
		}

		public virtual decimal? Commissions { get; set; }
		public virtual decimal? Allowances { get; set; }
		public virtual string AccountNumber { get; set; }
	}
}
