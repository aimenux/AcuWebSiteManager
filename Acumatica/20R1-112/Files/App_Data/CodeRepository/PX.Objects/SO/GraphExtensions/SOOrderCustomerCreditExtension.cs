using System;
using PX.Data;
using PX.Objects.AR;
using PX.Objects.Extensions.CustomerCreditHold;

namespace PX.Objects.SO.GraphExtensions
{
	/// <summary>A mapped generic graph extension that defines the SO credit helper functionality.</summary>
	public class SOOrderCustomerCreditExtension : CustomerCreditExtension<SOOrderEntry>
	{
		#region Mapping	

		protected override DocumentMapping GetDocumentMapping()
		{
			return new DocumentMapping(typeof(SOOrder))
			{
				CustomerID = typeof(SOOrder.customerID),
				Hold = typeof(SOOrder.creditHold),
				Status = typeof(SOOrder.status)
			};
		}

		#endregion

		#region Implementation

		protected override bool? GetReleasedValue(PXCache sender, object Row)
		{
			SOOrder row = Document.Cache.GetMain((Document)Row) as SOOrder;

			return row?.Cancelled == true || row?.Completed == true;
		}

		protected override bool? GetHoldValue(PXCache sender, object Row)
		{
			SOOrder row = Document.Cache.GetMain((Document)Row) as SOOrder;

			return (row?.Hold == true || row?.CreditHold == true || row?.InclCustOpenOrders == false);
		}

		protected override bool? GetCreditCheckError(PXCache sender, object Row)
		{
			return SOOrderType.PK.Find(sender.Graph, (Document.Cache.GetMain((Document)Row) as SOOrder).OrderType)?.CreditHoldEntry ?? false;
		}

		private bool IsLongOperationProcessing(PXGraph graph) => PXLongOperation.Exists(graph);
		public override void Verify(PXCache sender, object Row, EventArgs e)
		{
			if (!IsLongOperationProcessing(sender.Graph) || VerifyOnLongRun(sender, Row, e))
				base.Verify(sender, Row, e);
		}

		public virtual bool VerifyOnLongRun(PXCache sender, object Row, EventArgs e)
		{
			var rowUpdatedArgs = e as PXRowUpdatedEventArgs;
			if (rowUpdatedArgs == null)
				return false;

			return GetHoldValue(sender, Row) != GetHoldValue(sender, rowUpdatedArgs.OldRow);
		}

		protected virtual void _(Events.RowInserted<Document> e)
		{
			if (e.Row == null) return;

			UpdateARBalances(e.Cache, e.Row, null);
		}

		protected override void _(Events.RowUpdated<Document> e)
		{
			SOOrder row = Document.Cache.GetMain((Document)e.Row) as SOOrder;
			SOOrder oldRow = Document.Cache.GetMain((Document)e.OldRow) as SOOrder;
			if (row != null && oldRow != null)
				UpdateARBalances(e.Cache, e.Row, e.OldRow);

			if (_InternalCall)
			{
				return;
			}

			if (oldRow != null && row.CustomerID == oldRow.CustomerID &&
				row.CreditHold != oldRow.CreditHold &&
				row.CreditHold == false &&
				row.Hold == false ||
				oldRow != null && row.IsCCAuthorized != oldRow.IsCCAuthorized &&
			 	row.IsCCAuthorized == true ||
				oldRow != null && row.IsCCCaptured != oldRow.IsCCCaptured &&
				row.IsCCCaptured == true)
			{
				e.Cache.SetValue<SOOrder.approvedCredit>(row, true);
				e.Cache.SetValue<SOOrder.approvedCreditAmt>(row, row.OrderTotal);
			}

			if (oldRow != null && row.Hold != oldRow.Hold && row.Hold == true)
			{
				e.Cache.SetValueExt<SOOrder.creditHold>(row, false);
			}

			base._(e);
		}

		protected virtual void _(Events.RowDeleted<Document> e)
		{
			if (e.Row == null) return;

			UpdateARBalances(e.Cache, null, e.Row);
		}

		protected override decimal? GetDocumentBalance(PXCache cache, object Row)
		{
			SOOrder row = Document.Cache.GetMain((Document)Row) as SOOrder;

			decimal? DocumentBal = base.GetDocumentBalance(cache, row);

			if (DocumentBal > 0m && row.ApprovedCredit == true)
			{
				if (row.ApprovedCreditAmt >= row.OrderTotal)
				{
					DocumentBal = 0m;
				}
			}

			return DocumentBal;
		}

		protected override void PlaceOnHold(PXCache sender, object Row, bool OnAdminHold)
		{
			SOOrder row = Document.Cache.GetMain((Document)Row) as SOOrder;

			if (OnAdminHold)
			{
				sender.RaiseExceptionHandling<SOOrder.hold>(row, true,
					new PXSetPropertyException(AR.Messages.AdminHoldEntry, PXErrorLevel.Warning));

				object oldRow = sender.CreateCopy(row);
				sender.SetValueExt<SOOrder.status>(row, null);
				sender.SetValueExt<SOOrder.creditHold>(row, false);
				sender.SetValueExt<SOOrder.hold>(row, true);
				sender.RaiseRowUpdated(row, oldRow);
			}
			else
			{
				sender.SetValueExt<SOOrder.status>(row, null);
				base.PlaceOnHold(sender, row, false);
			}

			sender.SetValue<SOOrder.approvedCredit>(row, false);
			sender.SetValue<SOOrder.approvedCreditAmt>(row, 0m);
		}

		public override void UpdateARBalances(PXCache cache, Object newRow, Object oldRow)
		{
			if (oldRow != null)
			{
				SOOrder oldSORow = Document.Cache.GetMain((Document)oldRow) as SOOrder;
				ARReleaseProcess.UpdateARBalances(cache.Graph, oldSORow, -(oldSORow).UnbilledOrderTotal, -(oldSORow.OpenOrderTotal));
			}

			if (newRow != null)
			{
				SOOrder newSORow = Document.Cache.GetMain((Document)newRow) as SOOrder;
				ARReleaseProcess.UpdateARBalances(cache.Graph, newSORow, (newSORow).UnbilledOrderTotal, newSORow.OpenOrderTotal);
			}
		}

		#endregion
	}
}