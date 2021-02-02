using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PX.Objects.TX.Data
{
	public class TaxYearWithPeriods<TTaxYear, TTaxPeriod>
		where TTaxYear : TaxYear
		where TTaxPeriod : TaxPeriod
	{
		public TTaxYear TaxYear { get; set; }

		public List<TTaxPeriod> TaxPeriods { get; set; }

		public TaxYearWithPeriods()
		{
			TaxPeriods = new List<TTaxPeriod>();
		}
	}
}
