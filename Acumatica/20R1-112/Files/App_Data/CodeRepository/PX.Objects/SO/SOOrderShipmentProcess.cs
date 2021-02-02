using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using PX.Data;
using PX.Objects.AR;
using PX.Objects.CS;

namespace PX.Objects.SO
{
	public class SOOrderShipmentProcess : PXGraph<SOOrderShipmentProcess, SOOrderShipment>
	{
        public PXSelect<SOOrder> Orders;
        public PXSelect<SOShipment> Shipments;
		public PXSelect<SOMiscLine2,
			Where<SOMiscLine2.orderType, Equal<Required<SOOrder.orderType>>,
				And<SOMiscLine2.orderNbr, Equal<Required<SOOrder.orderNbr>>,
				And<Where<SOMiscLine2.unbilledQty, NotEqual<decimal0>,
					Or<SOMiscLine2.curyUnbilledAmt, NotEqual<decimal0>>>>>>> UnbilledMiscLines;
		public PXSelect<ARBalances> Arbalances;

		public PXAction<SOShipment> flow;
		[PXUIField(DisplayName = "Flow")]
		[PXButton(ImageKey = PX.Web.UI.Sprite.Main.DataEntryF)]
		protected virtual IEnumerable Flow(PXAdapter adapter)
		{
			Save.Press();
			return adapter.Get();
		}

		public PXSelectJoin<SOOrderShipment,
			InnerJoin<SOOrder, 
				On<SOOrder.orderType, Equal<SOOrderShipment.orderType>, 
				And<SOOrder.orderNbr, Equal<SOOrderShipment.orderNbr>>>,
			InnerJoin<ARInvoice,
				On<ARInvoice.docType, Equal<SOOrderShipment.invoiceType>,
					And<ARInvoice.refNbr, Equal<SOOrderShipment.invoiceNbr>,
						And<ARInvoice.released, Equal<boolTrue>>>>>>,
			Where<SOOrderShipment.invoiceType, Equal<Current<ARInvoice.docType>>, 
				And<SOOrderShipment.invoiceNbr, Equal<Current<ARInvoice.refNbr>>>>> Items;
		public PXSelect<SOAdjust,
				Where<SOAdjust.adjdOrderType, Equal<Required<SOAdjust.adjdOrderType>>,
					And<SOAdjust.adjdOrderNbr, Equal<Required<SOAdjust.adjdOrderNbr>>>>> Adjustments;

        protected virtual void SOOrder_RowPersisting(PXCache sender, PXRowPersistingEventArgs e)
        {
            SOOrder doc = (SOOrder)e.Row;
            if (e.Operation == PXDBOperation.Update)
            {
                if (doc.ShipmentCntr < 0 || doc.OpenShipmentCntr < 0 || doc.ShipmentCntr < doc.BilledCntr + doc.ReleasedCntr && doc.Behavior == SOBehavior.SO)
                {
                    throw new PXSetPropertyException(Messages.InvalidShipmentCounters);
                }
            } 
        }

		public SOOrderShipmentProcess()
		{
		}

		public virtual void CompleteSOLinesAndSplits(ARRegister ardoc, List<PXResult<SOOrderShipment, SOOrder>> orderShipments)
		{
			if (ardoc.IsCancellation == true || ardoc.IsCorrection == true) return;

			foreach (PXResult<SOOrderShipment, SOOrder> orderShipment in orderShipments)
			{
				SOOrder order = orderShipment;
				SOOrderType orderType = SOOrderType.PK.Find(this, order.OrderType);
				if (orderType.RequireShipping == false)
				{
					PXDatabase.Update<SOLine>(
						new PXDataFieldAssign<SOLine.completed>(true),
						new PXDataFieldRestrict<SOLine.completed>(false),
						new PXDataFieldRestrict<SOLine.orderType>(PXDbType.VarChar, 2, order.OrderType, PXComp.EQ),
						new PXDataFieldRestrict<SOLine.orderNbr>(PXDbType.NVarChar, 15, order.OrderNbr, PXComp.EQ));
					PXDatabase.Update<SOLineSplit>(
						new PXDataFieldAssign<SOLineSplit.completed>(true),
						new PXDataFieldRestrict<SOLineSplit.completed>(false),
						new PXDataFieldRestrict<SOLineSplit.orderType>(PXDbType.VarChar, 2, order.OrderType, PXComp.EQ),
						new PXDataFieldRestrict<SOLineSplit.orderNbr>(PXDbType.NVarChar, 15, order.OrderNbr, PXComp.EQ));
				}
			}
			PXUpdateJoin<
				Set<SOLine.completed, True>,
				SOLine,
					InnerJoin<ARTran, On<ARTran.sOOrderType, Equal<SOLine.orderType>,
						And<ARTran.sOOrderNbr, Equal<SOLine.orderNbr>,
						And<ARTran.sOOrderLineNbr, Equal<SOLine.lineNbr>>>>>,
				Where<SOLine.lineType, Equal<SOLineType.miscCharge>,
					And<SOLine.completed, Equal<False>,
					And<ARTran.tranType, Equal<Required<ARTran.tranType>>,
					And<ARTran.refNbr, Equal<Required<ARTran.refNbr>>>>>>>
			.Update(this,
				ardoc.DocType,
				ardoc.RefNbr);
		}

		public virtual List<PXResult<SOOrderShipment, SOOrder>> UpdateOrderShipments(ARRegister arDoc, HashSet<object> processed)
		{
			bool isCancellationInvoice = (arDoc.IsCancellation == true);
			bool isCorrectionInvoice = (arDoc.IsCorrection == true);
			var boundInvoice = isCancellationInvoice
				? ARInvoice.PK.Find(this, arDoc.OrigDocType, arDoc.OrigRefNbr)
				: arDoc;

			var orderShipments = this.Items.View.SelectMultiBound(new object[] { boundInvoice })
				.Cast<PXResult<SOOrderShipment, SOOrder>>()
				.ToList();
			foreach (var ordershipment in orderShipments)
			{
				SOOrderShipment copy = PXCache<SOOrderShipment>.CreateCopy(ordershipment);
				if (isCancellationInvoice)
				{
					ARInvoice existingCorrectionInvoice = PXSelect<ARInvoice,
						Where<ARInvoice.origDocType, Equal<Current<ARInvoice.origDocType>>,
							And<ARInvoice.origRefNbr, Equal<Current<ARInvoice.origRefNbr>>,
							And<ARInvoice.isCorrection, Equal<True>>>>>
						.SelectSingleBound(this, new[] { arDoc });
					copy.InvoiceType = existingCorrectionInvoice?.DocType;
					copy.InvoiceNbr = existingCorrectionInvoice?.RefNbr;
				}
				copy.InvoiceReleased = !isCancellationInvoice;

				this.Items.Update(copy);

				if (!isCancellationInvoice && !isCorrectionInvoice)
				{
					SOOrder order = ordershipment;
					SOOrderType otype = SOOrderType.PK.Find(this, order.OrderType);
					if ((order.Completed == true || otype.RequireShipping == false) && order.BilledCntr <= 1 && order.ShipmentCntr <= order.BilledCntr + order.ReleasedCntr)
					{
						foreach (SOAdjust adj in this.Adjustments.Select(order.OrderType, order.OrderNbr))
						{
							SOAdjust adjcopy = PXCache<SOAdjust>.CreateCopy(adj);
							adjcopy.CuryAdjdAmt = 0m;
							adjcopy.CuryAdjgAmt = 0m;
							adjcopy.AdjAmt = 0m;
							this.Adjustments.Update(adjcopy);
						}

						ResetUnbilledMiscLines(order);
					}
				}
				processed.Add(arDoc);
			}
			return orderShipments;
		}

		protected virtual void ResetUnbilledMiscLines(SOOrder order)
		{
			PXRowUpdated updateBalance = (s, e) =>
			{
				if (!(e.Row is SOOrder row))
					return;

				if (e.OldRow is SOOrder oldRow)
				{
					ARReleaseProcess.UpdateARBalances(this, oldRow, -oldRow.UnbilledOrderTotal, -oldRow.OpenOrderTotal);
				}
				ARReleaseProcess.UpdateARBalances(this, row, row.UnbilledOrderTotal, row.OpenOrderTotal);
			};

			RowUpdated.AddHandler<SOOrder>(updateBalance);

			try
			{
				foreach (SOMiscLine2 line in UnbilledMiscLines.Select(order.OrderType, order.OrderNbr))
				{
					line.CuryUnbilledAmt = 0m;
					line.UnbilledQty = 0m;
					line.Completed = true;
					UnbilledMiscLines.Update(line);
				}
			}
			finally
			{
				RowUpdated.RemoveHandler<SOOrder>(updateBalance);
			}
		}
	}
}
