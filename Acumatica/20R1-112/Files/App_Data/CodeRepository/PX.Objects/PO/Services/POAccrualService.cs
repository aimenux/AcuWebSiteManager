using PX.Common;
using PX.Data;
using System;
using System.Collections.Generic;
using System.Linq;

namespace PX.Objects.PO
{
	public class POAccrualService
	{
		public class POAccrualRecord
		{
			public string ReceivedUOM { get; set; }
			public decimal? ReceivedQty { get; set; } = 0m;
			public decimal? BaseReceivedQty { get; set; } = 0m;
			public decimal? ReceivedCost { get; set; } = 0m;
			public string BilledUOM { get; set; }
			public decimal? BilledQty { get; set; } = 0m;
			public decimal? BaseBilledQty { get; set; } = 0m;
			public string BillCuryID { get; set; }
			public decimal? CuryBilledAmt { get; set; } = 0m;
			public decimal? BilledAmt { get; set; } = 0m;
			public decimal? CuryBilledCost { get; set; } = 0m;
			public decimal? BilledCost { get; set; } = 0m;
			public decimal? CuryBilledDiscAmt { get; set; } = 0m;
			public decimal? BilledDiscAmt { get; set; } = 0m;
			public decimal? PPVAmt { get; set; } = 0m;

			public static POAccrualRecord FromPOAccrualStatus(POAccrualStatus s)
			{
				return new POAccrualRecord
				{
					ReceivedUOM = s.ReceivedUOM,
					ReceivedQty = s.ReceivedQty,
					BaseReceivedQty = s.BaseReceivedQty,
					ReceivedCost = s.ReceivedCost,
					BilledUOM = s.BilledUOM,
					BilledQty = s.BilledQty,
					BaseBilledQty = s.BaseBilledQty,
					BillCuryID = s.BillCuryID,
					CuryBilledAmt = s.CuryBilledAmt,
					BilledAmt = s.BilledAmt,
					CuryBilledCost = s.CuryBilledCost,
					BilledCost = s.BilledCost,
					CuryBilledDiscAmt = s.CuryBilledDiscAmt,
					BilledDiscAmt = s.BilledDiscAmt,
					PPVAmt = s.PPVAmt,
				};
			}
		}

		protected PXGraph _graph;

		public POAccrualService(PXGraph graph)
		{
			_graph = graph ?? throw new ArgumentNullException(nameof(graph));
		}

		public virtual POAccrualRecord GetAccrualStatusSummary(POLineUOpen poLine)
		{
			string orderType = poLine.OrderType;
			string orderNbr = poLine.OrderNbr;
			int? orderLineNbr = poLine.LineNbr;
			string poAccrualType = poLine.POAccrualType;

			if (!poAccrualType.IsIn(POAccrualType.Order, POAccrualType.Receipt))
				return new POAccrualRecord();

			// works for both order-based and receipt-based billing
			var accrualRecords = PXSelect<POAccrualStatus,
				Where<POAccrualStatus.orderType, Equal<Required<POAccrualStatus.orderType>>,
					And<POAccrualStatus.orderNbr, Equal<Required<POAccrualStatus.orderNbr>>,
					And<POAccrualStatus.orderLineNbr, Equal<Required<POAccrualStatus.orderLineNbr>>>>>>
				.Select(_graph, orderType, orderNbr, orderLineNbr)
				.RowCast<POAccrualStatus>()
				.Select(POAccrualRecord.FromPOAccrualStatus)
				.ToList();

			return this.Accumulate(accrualRecords);
		}

		protected virtual POAccrualRecord Accumulate(IEnumerable<POAccrualRecord> records)
		{
			var result = new POAccrualRecord();
			foreach (var r in records)
			{
				bool nulloutReceivedQty = result.ReceivedQty == null || r.ReceivedQty == null
					|| result.ReceivedUOM != null && r.ReceivedUOM != null && !string.Equals(result.ReceivedUOM, r.ReceivedUOM, StringComparison.OrdinalIgnoreCase);
				if (nulloutReceivedQty)
				{
					result.ReceivedUOM = null;
					result.ReceivedQty = null;
				}
				else if (r.ReceivedQty != 0m)
				{
					result.ReceivedUOM = r.ReceivedUOM;
					result.ReceivedQty += r.ReceivedQty;
				}
				result.BaseReceivedQty += r.BaseReceivedQty;
				result.ReceivedCost += r.ReceivedCost;
				bool nulloutBilledQty = result.BilledQty == null || r.BilledQty == null
					|| result.BilledUOM != null && r.BilledUOM != null && !string.Equals(result.BilledUOM, r.BilledUOM, StringComparison.OrdinalIgnoreCase);
				if (nulloutBilledQty)
				{
					result.BilledUOM = null;
					result.BilledQty = null;
				}
				else if (r.BilledQty != 0m)
				{
					result.BilledUOM = r.BilledUOM;
					result.BilledQty += r.BilledQty;
				}
				result.BaseBilledQty += r.BaseBilledQty;
				bool nulloutBilledCuryAmt = result.CuryBilledAmt == null || r.CuryBilledAmt == null
					|| result.BillCuryID != null && r.BillCuryID != null && !string.Equals(result.BillCuryID, r.BillCuryID, StringComparison.OrdinalIgnoreCase);
				if (nulloutBilledCuryAmt)
				{
					result.BillCuryID = null;
					result.CuryBilledAmt = null;
					result.CuryBilledCost = null;
					result.CuryBilledDiscAmt = null;
				}
				else if (r.CuryBilledAmt != 0m)
				{
					result.BillCuryID = r.BillCuryID;
					result.CuryBilledAmt += r.CuryBilledAmt;
					result.CuryBilledCost += r.CuryBilledCost;
					result.CuryBilledDiscAmt += r.CuryBilledDiscAmt;
				}
				result.BilledAmt += r.BilledAmt;
				result.BilledCost += r.BilledCost;
				result.BilledDiscAmt += r.BilledDiscAmt;
				result.PPVAmt += r.PPVAmt;
			}
			return result;
		}

		public static void SetIfNotNull<TField>(PXCache cache, POAccrualStatus row, object value)
			where TField : IBqlField
		{
			if (value != null)
			{
				cache.SetValue<TField>(row, value);
			}
		}

		public static void SetIfNotEmpty<TField>(PXCache cache, POAccrualStatus row, decimal? value)
			where TField : IBqlField
		{
			if (value != null && value != 0m)
			{
				cache.SetValue<TField>(row, value);
			}
		}

		public virtual void UpdateOrdersStatus(IEnumerable<Tuple<string, string>> orderCheckClosed)
		{
			foreach (Tuple<string, string> orderNbr in orderCheckClosed)
			{
				POLineUOpen minStatusLine =
					PXSelect<POLineUOpen,
					Where<POLineUOpen.orderType, Equal<Required<POLineUOpen.orderType>>,
						And<POLineUOpen.orderNbr, Equal<Required<POLineUOpen.orderNbr>>,
						And<POLineUOpen.lineType, NotEqual<POLineType.description>>>>,
					OrderBy<Asc<POLineUOpen.completed, Asc<POLineUOpen.closed>>>>
					.Select(_graph, orderNbr.Item1, orderNbr.Item2);

				string newStatus =
					(minStatusLine == null || minStatusLine.Closed == true) ? POOrderStatus.Closed
					: (minStatusLine.Completed == true) ? POOrderStatus.Completed
					: POOrderStatus.Open;

				POOrder order = PXSelect<POOrder,
					Where<POOrder.orderType, Equal<Required<POOrder.orderType>>,
						And<POOrder.orderNbr, Equal<Required<POOrder.orderNbr>>>>>
					.Select(_graph, orderNbr.Item1, orderNbr.Item2);

				if (order != null && order.Status != newStatus && order.Hold != true)
				{
					PXCache orderCache = _graph.Caches[typeof(POOrder)];
					POOrder upd = (POOrder)orderCache.CreateCopy(order);

					upd.Status = newStatus;
					orderCache.Update(upd);
				}
			}
		}
	}
}
