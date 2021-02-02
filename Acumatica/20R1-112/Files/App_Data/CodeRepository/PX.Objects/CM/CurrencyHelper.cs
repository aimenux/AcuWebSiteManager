using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PX.Data;

namespace PX.Objects.CM
{
	public class CurrencyHelper
	{
		public static bool IsSameCury(long? CuryInfoIDA, long? CuryInfoIDB, CurrencyInfo curyInfoA, CurrencyInfo curyInfoB)
		{
			return CuryInfoIDA == CuryInfoIDB || curyInfoA != null && curyInfoB != null && curyInfoA.CuryID == curyInfoB.CuryID;
		}

		public static bool IsSameCury(PXGraph graph, long? curyInfoIDA, long? curyInfoIDB)
		{
			CurrencyInfo curyInfoA = PXSelect<CurrencyInfo,
				Where<CurrencyInfo.curyInfoID, Equal<Required<CurrencyInfo.curyInfoID>>>>.SelectSingleBound(graph, null, curyInfoIDA);
			CurrencyInfo curyInfoB = PXSelect<CurrencyInfo,
				Where<CurrencyInfo.curyInfoID, Equal<Required<CurrencyInfo.curyInfoID>>>>.SelectSingleBound(graph, null, curyInfoIDB);
			return CurrencyHelper.IsSameCury(curyInfoIDA, curyInfoIDB, curyInfoA, curyInfoB);
		}
	}
}
