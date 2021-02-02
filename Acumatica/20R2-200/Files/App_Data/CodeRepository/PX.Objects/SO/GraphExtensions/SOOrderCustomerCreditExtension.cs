using System;
using PX.Data;
using PX.Objects.AR;
using PX.Objects.Common.Extensions;
using PX.Objects.Extensions.CustomerCreditHold;
using PX.SM;

namespace PX.Objects.SO.GraphExtensions
{
	/// <summary>A mapped generic graph extension that defines the SO credit helper functionality.</summary>
	public abstract class SOOrderCustomerCreditExtension<TGraph> : CustomerCreditExtension<
			TGraph,
			SOOrder,
			SOOrder.customerID,
			SOOrder.creditHold,
			SOOrder.completed,
			SOOrder.status>
		where TGraph : PXGraph
	{
		#region Implementation

		protected override bool? GetReleasedValue(PXCache sender, SOOrder row)
		{
			return row?.Cancelled == true || row?.Completed == true;
		}

		protected override bool? GetHoldValue(PXCache sender, SOOrder row)
		{
			return (row?.Hold == true || row?.CreditHold == true || row?.InclCustOpenOrders == false);
		}

		protected override bool? GetCreditCheckError(PXCache sender, SOOrder row)
		{
			return SOOrderType.PK.Find(sender.Graph, row.OrderType)?.CreditHoldEntry ?? false;
		}

		private bool IsLongOperationProcessing(PXGraph graph) => PXLongOperation.Exists(graph);
		public override void Verify(PXCache sender, SOOrder Row, EventArgs e)
		{
			if (!IsLongOperationProcessing(sender.Graph) || VerifyOnLongRun(sender, Row, e))
				base.Verify(sender, Row, e);
		}

		public virtual bool VerifyOnLongRun(PXCache sender, SOOrder Row, EventArgs e)
		{
			var rowUpdatedArgs = e as PXRowUpdatedEventArgs;
			if (rowUpdatedArgs == null)
				return false;

			return GetHoldValue(sender, Row) != GetHoldValue(sender, (SOOrder)rowUpdatedArgs.OldRow);
		}

		protected virtual void _(Events.RowInserted<SOOrder> e)
		{
			if (e.Row != null)
				UpdateARBalances(e.Cache, e.Row, null);
		}

		protected override void _(Events.RowUpdated<SOOrder> e)
		{
			if (e.Row != null && e.OldRow != null)
				UpdateARBalances(e.Cache, e.Row, e.OldRow);

			if (_InternalCall == false)
				base._(e);
		}

		protected virtual void _(Events.RowDeleted<SOOrder> e)
		{
			if (e.Row != null)
				UpdateARBalances(e.Cache, null, e.Row);
		}

		protected override decimal? GetDocumentBalance(PXCache cache, SOOrder row)
		{
			decimal? DocumentBal = base.GetDocumentBalance(cache, row);

			if (DocumentBal > 0m && IsFullAmountApproved(row))
			{
				DocumentBal = 0m;
			}

			return DocumentBal;
		}

		protected override void PlaceOnHold(PXCache cache, SOOrder order, bool onAdminHold)
		{
			if (onAdminHold)
			{
				cache.RaiseExceptionHandling<SOOrder.creditHold>(order, true,
					new PXSetPropertyException(AR.Messages.AdminHoldEntry, PXErrorLevel.Warning));

				cache.LiteUpdate(order, (c, r) =>
				{
					c.SetValueExt<SOOrder.creditHold>(r, false);
					c.SetValueExt<SOOrder.inclCustOpenOrders>(r, false);
					c.SetValueExt<SOOrder.hold>(r, true);
				});
			}
			else
			{
				SOOrder.Events.Select(e => e.CreditLimitViolated).FireOn(Base, order);

				base.PlaceOnHold(cache, order, false);
			}

			cache.SetValue<SOOrder.approvedCredit>(order, false);
			cache.SetValue<SOOrder.approvedCreditAmt>(order, 0m);
		}

		public override void UpdateARBalances(PXCache cache, SOOrder newRow, SOOrder oldRow)
		{
			if (oldRow == null || newRow == null ||
				!cache.ObjectsEqualBy<BqlFields.FilledWith<
					SOOrder.unbilledOrderTotal,
					SOOrder.openOrderTotal,
					SOOrder.inclCustOpenOrders,
					SOOrder.hold,
					SOOrder.creditHold,
					SOOrder.customerID,
					SOOrder.customerLocationID,
					SOOrder.aRDocType,
					SOOrder.branchID,
					SOOrder.shipmentCntr,
					SOOrder.cancelled>>(oldRow, newRow))
			{
				if (oldRow != null)
					ARReleaseProcess.UpdateARBalances(cache.Graph, oldRow, -oldRow.UnbilledOrderTotal, -oldRow.OpenOrderTotal);

				if (newRow != null)
					ARReleaseProcess.UpdateARBalances(cache.Graph, newRow, newRow.UnbilledOrderTotal, newRow.OpenOrderTotal);
			}
		}

		protected virtual bool IsFullAmountApproved(SOOrder row)
			=> row.ApprovedCredit == true && row.ApprovedCreditAmt >= row.OrderTotal;
		#endregion
	}
}