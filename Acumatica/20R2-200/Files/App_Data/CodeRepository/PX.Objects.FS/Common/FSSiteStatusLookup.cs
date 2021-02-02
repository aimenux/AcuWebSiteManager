using PX.Common;
using PX.Data;
using System;
using PX.Objects.IN;
using PX.Objects.CS;
using PX.Objects.CR;
using PX.Objects.SO;
using PX.Objects.Common.Bql;
using PX.Objects.CM;
using PX.Objects.Common;

namespace PX.Objects.FS
{
	public class FSSiteStatusLookup<Status, StatusFilter> : INSiteStatusLookup<Status, StatusFilter>
		where Status : class, IBqlTable, new()
		where StatusFilter : FSSiteStatusFilter, new()
	{
		#region Ctor
		public FSSiteStatusLookup(PXGraph graph)
			: base(graph)
		{
			graph.RowSelecting.AddHandler(typeof(FSSiteStatusSelected), OnRowSelecting);
		}

		public FSSiteStatusLookup(PXGraph graph, Delegate handler)
			: base(graph, handler)
		{
			graph.RowSelecting.AddHandler(typeof(FSSiteStatusSelected), OnRowSelecting);
		}
		#endregion
		protected virtual void OnRowSelecting(PXCache sender, PXRowSelectingEventArgs e)
		{
			if (sender.Fields.Contains(typeof(FSSiteStatusSelected.curyID).Name) &&
					sender.GetValue<FSSiteStatusSelected.curyID>(e.Row) == null)
			{
				PXCache srvOrdCache = sender.Graph.Caches[typeof(FSServiceOrder)];
				sender.SetValue<FSSiteStatusSelected.curyID>(e.Row,
					srvOrdCache.GetValue<FSServiceOrder.curyID>(srvOrdCache.Current));
				sender.SetValue<FSSiteStatusSelected.curyInfoID>(e.Row,
					srvOrdCache.GetValue<FSServiceOrder.curyInfoID>(srvOrdCache.Current));
			}

			if (sender.Graph is RouteServiceContractScheduleEntry
						|| sender.Graph is ServiceContractScheduleEntry)
			{
				PX.Objects.GL.Company row = (PX.Objects.GL.Company)sender.Graph.Caches[typeof(PX.Objects.GL.Company)].Current;

				CurrencyInfo currencyInfo = new CurrencyInfo();
				currencyInfo.BaseCuryID = currencyInfo.CuryID = sender.Graph.Accessinfo.BaseCuryID ?? row?.BaseCuryID;
				currencyInfo.CuryRate = 1;
				currencyInfo.CuryMultDiv = CuryMultDivType.Mult;
				currencyInfo.CuryEffDate = sender.Graph.Accessinfo.BusinessDate;

				sender.SetValue<FSSiteStatusSelected.curyID>(e.Row, currencyInfo.CuryID);
				sender.SetValue<FSSiteStatusSelected.curyInfoID>(e.Row, currencyInfo.CuryInfoID);
			}
		}

		protected override void OnFilterSelected(PXCache sender, PXRowSelectedEventArgs e)
		{
			base.OnFilterSelected(sender, e);
			FSSiteStatusFilter filter = (FSSiteStatusFilter)e.Row;
			PXUIFieldAttribute.SetVisible<FSSiteStatusFilter.historyDate>(sender, null, filter.Mode == SOAddItemMode.ByCustomer);

			PXCache status = sender.Graph.Caches[typeof(FSSiteStatusSelected)];
			PXUIFieldAttribute.SetVisible<FSSiteStatusSelected.qtyLastSale>(status, null, filter.Mode == SOAddItemMode.ByCustomer);
			PXUIFieldAttribute.SetVisible<FSSiteStatusSelected.curyID>(status, null, filter.Mode == SOAddItemMode.ByCustomer);
			PXUIFieldAttribute.SetVisible<FSSiteStatusSelected.curyUnitPrice>(status, null, filter.Mode == SOAddItemMode.ByCustomer);
			PXUIFieldAttribute.SetVisible<FSSiteStatusSelected.lastSalesDate>(status, null, filter.Mode == SOAddItemMode.ByCustomer);

			if (filter.HistoryDate == null)
				filter.HistoryDate = sender.Graph.Accessinfo.BusinessDate.GetValueOrDefault().AddMonths(-3);
		}

		protected override void OnRowSelected(PXCache sender, PXRowSelectedEventArgs e)
		{
			base.OnRowSelected(sender, e);
			PXUIFieldAttribute.SetEnabled<FSSiteStatusSelected.durationSelected>(sender, e.Row, true);
		}
	}
}

