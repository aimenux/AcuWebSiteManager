using PX.Common;
using PX.Data;
using PX.Objects.Common;
using PX.Objects.AP;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PX.Data.BQL.Fluent;

namespace PX.Objects.PO.GraphExtensions.APInvoiceEntryExt
{
	public class APInvoicePOValidation : PXGraphExtension<APInvoiceEntry>
	{
		#region Internal Items
		public class POLineDTO
		{
			protected POLine _poLine;
			protected string _poLineCuryID;
			protected POLineBillingRevision _poLineRevision;

			public virtual String OrderType => _poLineRevision == null ? _poLine.OrderType : _poLineRevision.OrderType;
			public virtual String OrderNbr => _poLineRevision == null ? _poLine.OrderNbr : _poLineRevision.OrderNbr;
			public virtual Int32? OrderLineNbr => _poLineRevision == null ? _poLine.LineNbr : _poLineRevision.OrderLineNbr;
			public virtual String CuryID => _poLineRevision == null ? _poLineCuryID : _poLineRevision.CuryID;
			public virtual String UOM => _poLineRevision == null ? _poLine.UOM : _poLineRevision.UOM;
			public virtual Decimal? OrderQty => _poLineRevision == null ? _poLine.OrderQty : _poLineRevision.OrderQty;
			public virtual Decimal? BaseOrderQty => _poLineRevision == null ? _poLine.BaseOrderQty : _poLineRevision.BaseOrderQty;
			public virtual Decimal? ReceivedQty => _poLineRevision == null ? _poLine.ReceivedQty : _poLineRevision.ReceivedQty;
			public virtual Decimal? BaseReceivedQty => _poLineRevision == null ? _poLine.BaseReceivedQty : _poLineRevision.BaseReceivedQty;
			public virtual Decimal? RcptQtyMax => _poLineRevision == null ? _poLine.RcptQtyMax : _poLineRevision.RcptQtyMax;
			public virtual Decimal? UnbilledQty => _poLineRevision == null ? _poLine.UnbilledQty : _poLineRevision.UnbilledQty;
			public virtual Decimal? BaseUnbilledQty => _poLineRevision == null ? _poLine.BaseUnbilledQty : _poLineRevision.BaseUnbilledQty;
			public virtual Decimal? CuryUnbilledAmt => _poLineRevision == null ? _poLine.CuryUnbilledAmt : _poLineRevision.CuryUnbilledAmt;
			public virtual Decimal? UnbilledAmt => _poLineRevision == null ? _poLine.UnbilledAmt : _poLineRevision.UnbilledAmt;
			public virtual Decimal? CuryUnitCost => _poLineRevision == null ? _poLine.CuryUnitCost : _poLineRevision.CuryUnitCost;
			public virtual Decimal? UnitCost => _poLineRevision == null ? _poLine.UnitCost : _poLineRevision.UnitCost;

			#region ctors
			public POLineDTO(POLine poLine, string curyID)
			{
				_poLine = poLine;
				_poLineCuryID = curyID;
			}

			public POLineDTO(POLineBillingRevision poLineRevision)
			{
				_poLineRevision = poLineRevision;
			}
			#endregion
		}
		#endregion

		public static bool IsActive()
		{
			return PXAccess.FeatureInstalled<CS.FeaturesSet.distributionModule>();
		}

		private APInvoicePOValidationService _validationService;
		public virtual APInvoicePOValidationService GetValidationService(Lazy<POSetup> poSetup)
		{
			if (_validationService == null)
				_validationService = new APInvoicePOValidationService(poSetup);
			return _validationService;
		}

		protected virtual void _(Events.RowUpdated<APTran> e)
		{
			// We need this as after discounts recalculation in RowUpdated there is no RowSelected call.
			Validate(e.Row, e.Cache);
		}

		protected virtual void _(Events.RowSelected<APTran> e)
		{
			Validate(e.Row, e.Cache);
		}

		protected virtual void Validate(APTran tran, PXCache cache)
		{
			APInvoicePOValidationService validationService = GetValidationService(Lazy.By(() => Base.posetup.Current));
			if (!validationService.IsLineValidationRequired(tran) || Base.IsImport || Base.IsExport)
				return;

			Prefetch(Base.Document.Current);
			POLineDTO poLine = ReadLinkedPOLine(tran);
			if (poLine == null)
				return;

			bool isReceivedMoreThenOrdered = poLine.ReceivedQty > poLine.OrderQty * poLine.RcptQtyMax / 100m;
			string tabMessage = poLine.OrderType == POOrderType.RegularSubcontract ? Messages.BillLinesDifferFromRSLines : Messages.BillLinesDifferFromPOLines;
			string message = poLine.OrderType == POOrderType.RegularSubcontract ?
				isReceivedMoreThenOrdered ? Messages.APInvoiceRSValidationWarningWithReceivedQty : Messages.APInvoiceRSValidationWarning
				: isReceivedMoreThenOrdered ? Messages.APInvoicePOValidationWarningWithReceivedQty : Messages.APInvoicePOValidationWarning;

			bool isAnyWarningOnLine = false;
			bool qtyFieldError = PXUIFieldAttribute.GetErrorOnly<APTran.qty>(cache, tran) != null;
			if (validationService.IsAPTranQtyExceedsPOLineUnbilledQty(tran, poLine) && !qtyFieldError)
			{
				ShowWarningWithErrorFallback<APTran.qty>(cache, message, tran, poLine);
				isAnyWarningOnLine = true;
			}
			else if (!validationService.IsAPTranQtyExceedsPOLineUnbilledQty(tran, poLine) && !qtyFieldError)
			{
				PXUIFieldAttribute.SetWarning<APTran.qty>(cache, tran, null);
			}

			bool unitCostFieldError = PXUIFieldAttribute.GetErrorOnly<APTran.curyUnitCost>(cache, tran) != null;
			if (validationService.IsAPTranUnitCostExceedsPOLineUnitCost(cache, tran, Base.Document.Current.CuryID, poLine) && !unitCostFieldError)
			{
				ShowWarningWithErrorFallback<APTran.curyUnitCost>(cache, message, tran, poLine);
				isAnyWarningOnLine = true;
			}
			else if (!validationService.IsAPTranUnitCostExceedsPOLineUnitCost(cache, tran, Base.Document.Current.CuryID, poLine) && !unitCostFieldError)
			{
				PXUIFieldAttribute.SetWarning<APTran.curyUnitCost>(cache, tran, null);
			}

			bool tranAmtFieldError = PXUIFieldAttribute.GetErrorOnly<APTran.curyTranAmt>(cache, tran) != null;
			if (validationService.IsAPTranAmountExceedsPOLineUnbilledAmount(tran, Base.Document.Current.CuryID, poLine) && !tranAmtFieldError)
			{
				ShowWarningWithErrorFallback<APTran.curyTranAmt>(cache, message, tran, poLine);
				isAnyWarningOnLine = true;
			}
			else if (!validationService.IsAPTranAmountExceedsPOLineUnbilledAmount(tran, Base.Document.Current.CuryID, poLine) && !tranAmtFieldError)
			{
				PXUIFieldAttribute.SetWarning<APTran.curyTranAmt>(cache, tran, null);
			}

			if (isAnyWarningOnLine)
			{
				cache.RaiseExceptionHandling<APTran.inventoryID>(tran, tran.InventoryID,
					new PXSetPropertyKeepPreviousException(tabMessage, PXErrorLevel.RowWarning));
			}
			else
			{
				PXUIFieldAttribute.SetWarning<APTran.inventoryID>(cache, tran, null);
			}
		}

		public PXSelectReadonly2<POLine,
			InnerJoin<POOrder, On<POLine.FK.Order>>,
			Where<POLine.orderType, Equal<Required<POLine.orderType>>,
				And<POLine.orderNbr, Equal<Required<POLine.orderNbr>>,
				And<POLine.lineNbr, Equal<Required<POLine.lineNbr>>>>>> POLineQuery;

		public PXSelectReadonly<POLineBillingRevision,
			Where<POLineBillingRevision.apDocType, Equal<Required<APTran.tranType>>,
				And<POLineBillingRevision.apRefNbr, Equal<Required<APTran.refNbr>>,
				And<POLineBillingRevision.orderType, Equal<Required<APTran.pOOrderType>>,
				And<POLineBillingRevision.orderNbr, Equal<Required<APTran.pONbr>>,
				And<POLineBillingRevision.orderLineNbr, Equal<Required<APTran.pOLineNbr>>>>>>>> POLineRevisionQuery;

		protected virtual Type[] GetPOLinesQueryFieldScope() => new[] { typeof(POLine), typeof(POOrder.curyID) };

		protected string _prefetchedDoc = null;
		protected virtual void Prefetch(APInvoice doc)
		{
			var docKey = $"{doc.DocType}-{doc.RefNbr}-{doc.Released}";
			if (string.Equals(docKey, _prefetchedDoc, StringComparison.Ordinal))
				return;

			if (doc.Released != true)
			{
				var poLinesQuery = new PXSelectReadonly2<POLine,
					InnerJoin<POOrder, On<POLine.FK.Order>,
					InnerJoin<APTran,
						On<APTran.pOOrderType, Equal<POLine.orderType>,
							And<APTran.pONbr, Equal<POLine.orderNbr>,
							And<APTran.pOLineNbr, Equal<POLine.lineNbr>>>>>>,
					Where<APTran.tranType, Equal<Required<APInvoice.docType>>,
						And<APTran.refNbr, Equal<Required<APInvoice.refNbr>>>>>(Base);
				using (var fieldScope = new PXFieldScope(poLinesQuery.View, GetPOLinesQueryFieldScope()))
				{
					var restrictedFields = Lazy.By(() => fieldScope.Fields.Select(f => f.Field).ToArray());
					foreach (PXResult<POLine, POOrder, APTran> record in poLinesQuery.View.SelectMulti(doc.DocType, doc.RefNbr))
					{
						POLine poLine = record;
						POLineQuery.StoreCached(
							new PXCommandKey(new object[] { poLine.OrderType, poLine.OrderNbr, poLine.LineNbr }, null, null, null, 0, 1, null, false,
								restrictedFields: restrictedFields.Value),
							new List<object> { record });
					}
				}
			}
			else
			{
				foreach (PXResult<APTran, POLineBillingRevision> record in PXSelectReadonly2<APTran,
					LeftJoin<POLineBillingRevision, On<POLineBillingRevision.apDocType, Equal<APTran.tranType>,
						And<POLineBillingRevision.apRefNbr, Equal<APTran.refNbr>,
						And<POLineBillingRevision.orderType, Equal<APTran.pOOrderType>,
						And<POLineBillingRevision.orderNbr, Equal<APTran.pONbr>,
						And<POLineBillingRevision.orderLineNbr, Equal<APTran.pOLineNbr>>>>>>>,
					Where<APTran.tranType, Equal<Required<APInvoice.docType>>,
						And<APTran.refNbr, Equal<Required<APInvoice.refNbr>>,
						And<APTran.pONbr, IsNotNull>>>>
					.Select(Base, doc.DocType, doc.RefNbr))
				{
					APTran tran = record;
					POLineBillingRevision revision = record;
					POLineRevisionQuery.StoreCached(
						new PXCommandKey(new object[] { tran.TranType, tran.RefNbr, tran.POOrderType, tran.PONbr, tran.POLineNbr }, true),
						new List<object> { revision.OrderNbr != null ? new PXResult<POLineBillingRevision>(revision) : null });
				}
			}

			_prefetchedDoc = docKey;
		}

		public virtual POLineDTO ReadLinkedPOLine(APTran tran)
		{
			if (tran.Released != true)
			{
				using (new PXFieldScope(POLineQuery.View, GetPOLinesQueryFieldScope()))
				{
					var poRecord = (PXResult<POLine, POOrder>)POLineQuery.View.SelectSingle(tran.POOrderType, tran.PONbr, tran.POLineNbr);
					return new POLineDTO(poRecord, poRecord.GetItem<POOrder>().CuryID);
				}
			}
			else
			{
				POLineBillingRevision poLineRevision = POLineRevisionQuery
					.SelectWindowed(0, 1, tran.TranType, tran.RefNbr, tran.POOrderType, tran.PONbr, tran.POLineNbr);
				if (poLineRevision == null)
					return null;

				return new POLineDTO(poLineRevision);
			}
		}

		public virtual void ShowWarningWithErrorFallback<TField>(PXCache cache, string message, APTran tran, POLineDTO poLine)
			where TField : IBqlField
		{
			decimal unbilledQty = IN.PXDBQuantityAttribute.Round(poLine.UnbilledQty);
			decimal curyUnitCost = IN.PXDBPriceCostAttribute.Round(poLine.CuryUnitCost ?? 0m);
			decimal curyUnbilledAmt = CM.PXDBCurrencyAttribute.BaseRound(Base, poLine.CuryUnbilledAmt ?? 0m);

			if (cache.RaiseExceptionHandling<TField>(tran, cache.GetValueExt<TField>(tran),
				new PXSetPropertyException<TField>(message, PXErrorLevel.Warning,
					poLine.UOM, poLine.CuryID, unbilledQty, curyUnitCost, curyUnbilledAmt)))
			{
				throw new PXSetPropertyException<TField>(message, poLine.UOM, poLine.CuryID, unbilledQty, curyUnitCost, curyUnbilledAmt);
			}
		}

		protected virtual void _(Events.RowPersisting<APTran> e)
		{
			APTran tran = e.Row;
			if (tran == null || tran.Released == true || e.Operation.Command().IsNotIn(PXDBOperation.Insert, PXDBOperation.Update))
				return;

			string lineType = null;
			decimal? aPTranQty = tran.Qty;

			switch (tran.POAccrualType)
			{
				case POAccrualType.Receipt:
					POReceiptLine rctLine = PXSelectReadonly<POReceiptLine,
						Where<POReceiptLine.receiptNbr, Equal<Required<POReceiptLine.receiptNbr>>,
							And<POReceiptLine.lineNbr, Equal<Required<POReceiptLine.lineNbr>>>>>
						.SelectWindowed(Base, 0, 1, tran.ReceiptNbr, tran.ReceiptLineNbr);
					lineType = rctLine.LineType;
					
					decimal? pOReceiptLineUnbilledQty = rctLine.UnbilledQty;

					if (tran.UOM != rctLine.UOM)
					{
						aPTranQty = tran.BaseQty;
						pOReceiptLineUnbilledQty = rctLine.BaseUnbilledQty;
					}

					if (aPTranQty > pOReceiptLineUnbilledQty && tran.Sign == ((rctLine.InvtMult < 0) ? -1m : 1m))
					{
						if (e.Cache.RaiseExceptionHandling<APTran.qty>(tran, tran.Qty,
							new PXSetPropertyException(Messages.QuantityBilledIsGreaterThenPOReceiptQuantity, PXErrorLevel.Error)))
						{
							throw new PXSetPropertyException(Messages.QuantityBilledIsGreaterThenPOReceiptQuantity);
						}
					}
					break;
				case POAccrualType.Order:
					POLine poLine = PXSelectReadonly<POLine,
						Where<POLine.orderType, Equal<Required<POLine.orderType>>, And<POLine.orderNbr, Equal<Required<POLine.orderNbr>>,
							And<POLine.lineNbr, Equal<Required<POLine.lineNbr>>>>>>
						.SelectWindowed(Base, 0, 1, tran.POOrderType, tran.PONbr, tran.POLineNbr);
					lineType = poLine.LineType;
					if (tran.Sign < 0) break; // Skip bills for returns.
					
					decimal? pOLineBilledQty = poLine.BilledQty;
					decimal? maxRcptQty = poLine.OrderQty * poLine.RcptQtyMax / 100m;
					
					if (tran.UOM != poLine.UOM)
					{
						aPTranQty = tran.BaseQty;
						pOLineBilledQty = poLine.BaseBilledQty;
						maxRcptQty = poLine.BaseOrderQty * poLine.RcptQtyMax / 100m;
					}

					if (poLine.RcptQtyAction.IsIn(POReceiptQtyAction.Reject, POReceiptQtyAction.AcceptButWarn)
						&& aPTranQty + pOLineBilledQty > maxRcptQty)
					{
						PXErrorLevel errorLevel = (poLine.RcptQtyAction == POReceiptQtyAction.Reject) ? PXErrorLevel.Error : PXErrorLevel.Warning;
						if (e.Cache.RaiseExceptionHandling<APTran.qty>(tran, tran.Qty,
								new PXSetPropertyException(Messages.QuantityBilledIsGreaterThenPOQuantity, errorLevel))
							&& errorLevel == PXErrorLevel.Error)
						{
							throw new PXSetPropertyException(Messages.QuantityBilledIsGreaterThenPOQuantity);
						}
					}
					break;
			}

			if (tran.Qty == 0m && tran.CuryTranAmt != 0m && POLineType.UsePOAccrual(lineType))
			{
				e.Cache.RaiseExceptionHandling<APTran.qty>(tran, tran.Qty,
					new PXSetPropertyException(CS.Messages.Entry_NE, PXErrorLevel.Error, 0m));
			}
		}
	}
}
