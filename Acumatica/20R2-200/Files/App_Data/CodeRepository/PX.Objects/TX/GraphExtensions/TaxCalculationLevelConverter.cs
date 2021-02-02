using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PX.Data;
using PX.TaxProvider;

namespace PX.Objects.TX.GraphExtensions
{
	public static class TaxCalculationLevelConverter
	{
		public static string ToCSTaxCalcLevel(this TaxCalculationLevel target)
		{
			switch (target)
			{
				case TaxCalculationLevel.Inclusive:
					return CSTaxCalcLevel.Inclusive;
				case TaxCalculationLevel.CalcOnItemAmt:
					return CSTaxCalcLevel.CalcOnItemAmt;
				case TaxCalculationLevel.CalcOnItemAmtPlusTaxAmt:
					return CSTaxCalcLevel.CalcOnItemAmtPlusTaxAmt;
				default:
					return CSTaxCalcLevel.Inclusive;
			}
		}
	}
}
