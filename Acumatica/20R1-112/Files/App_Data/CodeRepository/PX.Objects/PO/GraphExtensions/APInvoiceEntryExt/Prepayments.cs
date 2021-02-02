using System;
using System.Collections.Generic;
using System.Linq;
using PX.Data;
using PX.Objects.AP;
using PX.Objects.CM;
using PX.Objects.Common.Discount.Attributes;
using PX.Objects.CS;
using PX.Objects.GL;
using PX.Objects.TX;

namespace PX.Objects.PO.GraphExtensions.APInvoiceEntryExt
{
	public class Prepayments : PXGraphExtension<APInvoiceEntry>
	{
		public PXSelect<POLine> POLines;
		public PXSelect<POOrder,
			Where<POOrder.orderType, Equal<Optional<POOrderPrepayment.orderType>>, And<POOrder.orderNbr, Equal<Optional<POOrderPrepayment.orderNbr>>>>>
			POOrders;

		public PXSelect<POOrderPrepayment,
			Where<POOrderPrepayment.aPDocType, Equal<Current<APInvoice.docType>>,
				And<POOrderPrepayment.aPRefNbr, Equal<Current<APInvoice.refNbr>>,
				And<Current<APInvoice.docType>, Equal<APDocType.prepayment>>>>>
			PrepaidOrders;

		[PXMergeAttributes(Method = MergeMethod.Append)]
		[PXParent(typeof(Select<POOrderPrepayment,
			Where<POOrderPrepayment.orderType, Equal<Current<APTran.pOOrderType>>, And<POOrderPrepayment.orderNbr, Equal<Current<APTran.pONbr>>,
				And<POOrderPrepayment.aPDocType, Equal<Current<APTran.tranType>>, And<POOrderPrepayment.aPRefNbr, Equal<Current<APTran.refNbr>>,
				And<Current<APTran.tranType>, Equal<APDocType.prepayment>>>>>>>), ParentCreate = true)]
		protected virtual void _(Events.CacheAttached<APTran.pONbr> e)
		{
		}

		[PXMergeAttributes(Method = MergeMethod.Append)]
		[PXRemoveBaseAttribute(typeof(ManualDiscountMode))]
		[PrepaymentDiscount(typeof(APTran.curyDiscAmt), typeof(APTran.curyTranAmt),
			typeof(APTran.discPct), typeof(APTran.freezeManualDisc), DiscountFeatureType.VendorDiscount)]
		protected virtual void _(Events.CacheAttached<APTran.manualDisc> e)
		{
		}

		protected virtual void _(Events.RowSelected<APInvoice> e)
		{
			PXFormulaAttribute.SetAggregate<APTran.curyTranAmt>(Base.Transactions.Cache,
				(e.Row.DocType == APDocType.Prepayment) ? typeof(SumCalc<POOrderPrepayment.curyLineTotal>) : null);
		}

		protected virtual void _(Events.RowInserted<POOrderPrepayment> e)
		{
			if (Base.Document.Current?.OrigModule == BatchModule.AP)
			{
				Base.Document.Current.OrigModule = BatchModule.PO;
				Base.Document.Cache.MarkUpdated(Base.Document.Current);
			}
		}

		protected virtual void _(Events.RowDeleted<POOrderPrepayment> e)
		{
			if (Base.Document.Current?.OrigModule == BatchModule.PO
				&& !PrepaidOrders.Select().AsEnumerable().Any())
			{
				Base.Document.Current.OrigModule = BatchModule.AP;
				Base.Document.Cache.MarkUpdated(Base.Document.Current);
			}
		}

		protected virtual void _(Events.FieldDefaulting<APTran, APTran.prepaymentPct> e)
		{
			if (e.Row.TranType != APDocType.Prepayment)
			{
				e.NewValue = 0m;
				e.Cancel = true;
				return;
			}

			if (!string.IsNullOrEmpty(e.Row.POOrderType) && !string.IsNullOrEmpty(e.Row.PONbr))
			{
				POOrder order = PXSelectReadonly<POOrder,
					Where<POOrder.orderType, Equal<Required<POOrder.orderType>>,
						And<POOrder.orderNbr, Equal<Required<POOrder.orderNbr>>>>>
					.Select(Base, e.Row.POOrderType, e.Row.PONbr);
				if (order?.PrepaymentPct != null)
				{
					e.NewValue = order.PrepaymentPct;
					e.Cancel = true;
					return;
				}
			}

			if (e.Row.InventoryID != null)
			{
				POVendorInventory vendorInventory = PXSelectReadonly<POVendorInventory,
					Where<POVendorInventory.inventoryID, Equal<Required<POVendorInventory.inventoryID>>,
						And<POVendorInventory.vendorID, Equal<Current<APInvoice.vendorID>>,
						And<POVendorInventory.vendorLocationID, Equal<Current<APInvoice.vendorLocationID>>>>>>
					.Select(Base, e.Row.InventoryID);
				if (vendorInventory?.PrepaymentPct != null)
				{
					e.NewValue = vendorInventory.PrepaymentPct;
					e.Cancel = true;
					return;
				}
			}

			e.NewValue = Base.location.Current?.VPrepaymentPct ?? 0m;
		}

		protected virtual void _(Events.FieldUpdated<APTran, APTran.inventoryID> e)
		{
			if (e.Row.TranType == APDocType.Prepayment)
			{
				e.Cache.SetDefaultExt<APTran.prepaymentPct>(e.Row);
			}
		}

		protected virtual void _(Events.FieldUpdated<APTran, APTran.pONbr> e)
		{
			if (e.Row.TranType == APDocType.Prepayment)
			{
				e.Cache.SetDefaultExt<APTran.prepaymentPct>(e.Row);
			}
		}

		protected virtual void _(Events.RowDeleted<APTran> e)
		{
			if (e.Row.TranType != APDocType.Prepayment || string.IsNullOrEmpty(e.Row.POOrderType) || string.IsNullOrEmpty(e.Row.PONbr))
				return;
			var prepaidOrder = PXParentAttribute.SelectParent<POOrderPrepayment>(Base.Transactions.Cache, e.Row);
			if (prepaidOrder != null
				&& !PXParentAttribute.SelectChildren(Base.Transactions.Cache, prepaidOrder, typeof(POOrderPrepayment)).Any())
			{
				PrepaidOrders.Delete(prepaidOrder);
			}
		}

		public virtual void AddPOOrderProc(POOrder order, bool createNew)
		{
			APInvoice prepayment;
			if (createNew)
			{
				prepayment = Base.Document.Insert(new APInvoice { DocType = APDocType.Prepayment });
				prepayment.DocDesc = order.OrderDesc;
				if (PXAccess.FeatureInstalled<FeaturesSet.vendorRelations>())
				{
					prepayment.VendorID = order.PayToVendorID;
					prepayment.VendorLocationID = (order.VendorID == order.PayToVendorID) ? order.VendorLocationID : null;
					prepayment.SuppliedByVendorID = order.VendorID;
					prepayment.SuppliedByVendorLocationID = order.VendorLocationID;
				}
				else
				{
					prepayment.VendorID =
					prepayment.SuppliedByVendorID = order.VendorID;
					prepayment.VendorLocationID =
					prepayment.SuppliedByVendorLocationID = order.VendorLocationID;
				}
				prepayment.CuryID = order.CuryID;
				Base.Document.Update(prepayment);
				prepayment.TaxCalcMode = order.TaxCalcMode;
				prepayment.InvoiceNbr = order.OrderNbr;
				prepayment.DueDate = order.OrderDate;
				prepayment.TaxZoneID = order.TaxZoneID;
				Base.Document.Update(prepayment);
			}
			else
			{
				prepayment = Base.Document.Current;
			}

			TaxBaseAttribute.SetTaxCalc<APTran.taxCategoryID, TaxAttribute>(Base.Transactions.Cache, null, TaxCalc.ManualCalc);

			var orderLines = PXSelectReadonly<POLineRS,
				Where<POLineRS.orderType, Equal<Required<POOrder.orderType>>,
					And<POLineRS.orderNbr, Equal<Required<POOrder.orderNbr>>>>,
				OrderBy<Asc<POLineRS.sortOrder, Asc<POLineRS.lineNbr>>>>
				.Select(Base, order.OrderType, order.OrderNbr)
				.RowCast<POLineRS>()
				.ToList();

			bool hasAdded = AddPOOrderLines(orderLines);
			if (!hasAdded)
			{
				throw new PXException(Messages.APInvoicePOOrderCreation_NoApplicableLinesFound);
			}

			Base.AddOrderTaxes(order);

			TaxBaseAttribute.SetTaxCalc<APTran.taxCategoryID, TaxAttribute>(Base.Transactions.Cache, null, TaxCalc.ManualLineCalc);
		}

		public virtual bool AddPOOrderLines(IEnumerable<POLineRS> lines)
		{
			bool hasAdded = false;
			foreach (POLineRS line in lines.Where(l =>
				(l.CuryExtCost + l.CuryRetainageAmt > l.CuryReqPrepaidAmt)
				&& (l.Billed == false || l.LineType == POLineType.Service && l.Closed == false)))
			{
				var tran = new APTran
				{
					InventoryID = line.InventoryID,
					ProjectID = line.ProjectID,
					TaskID = line.TaskID,
					CostCodeID = line.CostCodeID,
					TaxID = line.TaxID,
					TaxCategoryID = line.TaxCategoryID,
					TranDesc = line.TranDesc,
					UOM = line.UOM,
					CuryUnitCost = line.CuryUnitCost,
					DiscPct = line.DiscPct,
					ManualPrice = true,
					ManualDisc = true,
					FreezeManualDisc = true,
					DiscountID = line.DiscountID,
					DiscountSequenceID = line.DiscountSequenceID,
					RetainagePct = line.RetainagePct,
					POOrderType = line.OrderType,
					PONbr = line.OrderNbr,
					POLineNbr = line.LineNbr,
				};

				decimal? billedAndPrepaidQty = line.ReqPrepaidQty + line.OrderBilledQty;
				tran.Qty = (line.OrderQty <= billedAndPrepaidQty) ? line.OrderQty : line.OrderQty - billedAndPrepaidQty;

				decimal? billedAndPrepaidAmt = line.CuryReqPrepaidAmt + line.CuryOrderBilledAmt;
				if (billedAndPrepaidAmt == 0m)
				{
					tran.CuryLineAmt = line.CuryLineAmt;
					tran.CuryDiscAmt = line.CuryDiscAmt;
				}
				else if (line.CuryExtCost + line.CuryRetainageAmt <= billedAndPrepaidAmt)
				{
					tran.CuryLineAmt = 0m;
					tran.CuryDiscAmt = 0m;
					tran.CuryRetainageAmt = 0m;
					tran.CuryTranAmt = 0m;
				}
				else
				{
					decimal? prepaymentRatio = (line.CuryExtCost + line.CuryRetainageAmt - billedAndPrepaidAmt) / (line.CuryExtCost + line.CuryRetainageAmt);
					tran.CuryLineAmt = PXCurrencyAttribute.Round(Base.Transactions.Cache, tran, (prepaymentRatio * line.CuryLineAmt) ?? 0m, CMPrecision.TRANCURY);
					tran.CuryDiscAmt =  PXCurrencyAttribute.Round(Base.Transactions.Cache, tran, (prepaymentRatio * line.CuryDiscAmt) ?? 0m, CMPrecision.TRANCURY);
				}

				Base.Transactions.Insert(tran);
				hasAdded = true;
			}
			Base.AutoRecalculateDiscounts();
			return hasAdded;
		}

		[PXOverride]
		public virtual void VoidPrepayment(APRegister doc, Action<APRegister> baseMethod)
		{
			foreach (PXResult<APTran, POLine> poLineRes in Base.TransactionsPOLine.Select())
			{
				POLine line = poLineRes;
				if (line?.OrderNbr == null) continue;

				POLine updLine = PXCache<POLine>.CreateCopy(line);
				APTran tran = poLineRes;
				updLine.ReqPrepaidQty -= tran.Qty;
				updLine.CuryReqPrepaidAmt -= tran.CuryTranAmt + tran.CuryRetainageAmt;
				updLine = this.POLines.Update(updLine);
			}

			POOrderPrepayment prepay = PrepaidOrders.Select();
			if (prepay != null)
			{
				POOrderPrepayment updPrepay = PXCache<POOrderPrepayment>.CreateCopy(prepay);
				updPrepay.CuryAppliedAmt -= doc.CuryOrigDocAmt;
				updPrepay = this.PrepaidOrders.Update(updPrepay);

				POOrder order = POOrders.Select(prepay.OrderType, prepay.OrderNbr);
				POOrder updOrder = PXCache<POOrder>.CreateCopy(order);
				updOrder.CuryPrepaidTotal -= doc.CuryDocBal;
				updOrder = this.POOrders.Update(updOrder);
			}

			baseMethod(doc);
		}

		[PXOverride]
		public virtual void CheckQtyFromPO(PXCache sender, APTran tran, Action<PXCache, APTran> baseMethod)
		{
			baseMethod(sender, tran);

			if (tran.TranType == APDocType.Prepayment && !string.IsNullOrEmpty(tran.PONbr) && tran.POLineNbr != null)
			{
				POLine poLine = PXSelectReadonly<POLine,
					Where<POLine.orderType, Equal<Required<POLine.orderType>>, And<POLine.orderNbr, Equal<Required<POLine.orderNbr>>,
						And<POLine.lineNbr, Equal<Required<POLine.lineNbr>>>>>>
					.SelectWindowed(Base, 0, 1, tran.POOrderType, tran.PONbr, tran.POLineNbr);
				if (tran.Qty > poLine.OrderQty)
				{
					sender.RaiseExceptionHandling<APTran.qty>(tran, tran.Qty, new PXSetPropertyException(Messages.PrepaidQtyCantExceedPOLine));
				}
				if ((poLine.CuryReqPrepaidAmt > poLine.CuryBilledAmt ? poLine.CuryReqPrepaidAmt : poLine.CuryBilledAmt)
					+ tran.CuryTranAmt + tran.CuryRetainageAmt
					> poLine.CuryExtCost + poLine.CuryRetainageAmt)
				{
					sender.RaiseExceptionHandling<APTran.curyTranAmt>(tran, tran.CuryTranAmt,
						new PXSetPropertyException(Messages.PrepaidAmtCantExceedPOLine));
				}
				else if (poLine.CuryReqPrepaidAmt + poLine.CuryBilledAmt
					+ tran.CuryTranAmt + tran.CuryRetainageAmt
					> poLine.CuryExtCost + poLine.CuryRetainageAmt)
				{
					sender.RaiseExceptionHandling<APTran.curyTranAmt>(tran, tran.CuryTranAmt,
						new PXSetPropertyException(Messages.PrepaidAmtMayExceedPOLine, PXErrorLevel.Warning, poLine.OrderNbr));
				}
			}
		}
	}
}
