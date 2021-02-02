using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PX.Data;
using PX.Dashboards.Widgets;
using PX.Objects.GL;
using PX.Olap;

namespace PX.Objects.SM
{
	public class ChartSettingsMaintExt : PXGraphExtension<ChartSettingsMaint>
	{
		[PXOverride]
		public virtual SortType DetermineSortType(string field, Func<string, SortType> del)
		{
			if (PivotMaintExt.TryDetermineSortType(Base.DataScreen, field, out var sortType))
				return sortType;

			return del(field);
		}

		public virtual void ChartSettings_RowUpdated(PXCache sender, PXRowUpdatedEventArgs e)
		{
			if (!e.ExternalCall) return;

			ChartSettings row = (ChartSettings)e.Row, oldRow = (ChartSettings)e.OldRow;

			if (row.CategoryField != oldRow.CategoryField
			    && PivotMaintExt.IsFinPeriod(Base.DataScreen, row.CategoryField))
			{
				sender.SetValue<ChartSettings.categorySortType>(row, SortTypeListAttribute.Legend);
				sender.SetValue<ChartSettings.categorySortOrder>(row, 0); // ascending
			}

			if (row.SeriesField != oldRow.SeriesField
				&& PivotMaintExt.IsFinPeriod(Base.DataScreen, row.SeriesField))
			{
				sender.SetValue<ChartSettings.seriesSortType>(row, SortTypeListAttribute.Legend);
				sender.SetValue<ChartSettings.seriesSortOrder>(row, 0); // ascending
			}
		}
	}
}
