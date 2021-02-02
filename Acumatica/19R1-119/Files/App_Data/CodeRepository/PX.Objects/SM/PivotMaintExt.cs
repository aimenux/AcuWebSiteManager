using PX.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PX.Objects.GL;
using PX.Olap;
using PX.Olap.Maintenance;

namespace PX.Objects.SM
{
	public class PivotMaintExt : PXGraphExtension<PivotMaint>
	{
		public static bool TryDetermineSortType(DataScreenBase dataScreen, string field, out SortType sortType)
		{
			if (IsFinPeriod(dataScreen, field))
			{
				sortType = SortType.ByValue;
				return true;
			}

			sortType = SortType.ByDisplayValue;
			return false;
		}

		public static bool IsFinPeriod(DataScreenBase dataScreen, string field)
		{
			if (!String.IsNullOrEmpty(field))
			{
				var attr = dataScreen?.View.Cache
					.GetAttributesReadonly(field, true)
					.OfType<FinPeriodIDFormattingAttribute>()
					.FirstOrDefault();

				if (attr != null)
				{
					return true;
				}
			}

			return false;
		}

		[PXOverride]
		public virtual SortType DetermineSortType(string field, Func<string, SortType> del)
		{
			if (TryDetermineSortType(Base.DataScreen, field, out var sortType))
				return sortType;

			return del(field);
		}
	}
}
