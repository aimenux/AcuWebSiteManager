using PX.Objects.TX;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PX.Objects.AP
{
	public static class TaxClassExtension
	{
		public static bool IsRegularInclusiveTax(this Tax tax)
		{
			return
				tax != null &&
				tax.TaxCalcLevel == CSTaxCalcLevel.Inclusive &&
				tax.TaxType != CSTaxType.Withholding &&
				tax.TaxType != CSTaxType.PerUnit &&
				tax.ReverseTax != true;
		}
	}
}
