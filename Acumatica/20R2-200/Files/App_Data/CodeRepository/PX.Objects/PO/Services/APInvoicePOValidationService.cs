using PX.Data;
using PX.Objects.AP;
using System;
using System.Collections.Generic;
using System.Linq;
using POLineDTO = PX.Objects.PO.GraphExtensions.APInvoiceEntryExt.APInvoicePOValidation.POLineDTO;

namespace PX.Objects.PO
{
	public class APInvoicePOValidationService
	{
		protected Lazy<POSetup> _poSetup;

		public APInvoicePOValidationService(Lazy<POSetup> poSetup)
		{
			_poSetup = poSetup;
		}

		public virtual bool IsLineValidationRequired(APTran tran)
		{
			return tran != null && tran.TranType == APDocType.Invoice && tran.Sign > 0m
				&& tran.POOrderType != null && tran.PONbr != null && tran.POLineNbr != null
				&& _poSetup.Value.APInvoiceValidation == APInvoiceValidationMode.Warning;
		}

		public virtual bool ShouldCreateRevision(PXCache cache, APTran tran, string apTranCuryID, POLineDTO poLine)
		{
			return IsLineValidationRequired(tran) && (IsAPTranQtyExceedsPOLineUnbilledQty(tran, poLine)
				|| IsAPTranUnitCostExceedsPOLineUnitCost(cache, tran, apTranCuryID, poLine)
				|| IsAPTranAmountExceedsPOLineUnbilledAmount(tran, apTranCuryID, poLine));
		}

		public virtual bool IsAPTranQtyExceedsPOLineUnbilledQty(APTran tran, POLineDTO poLine)
		{
			bool isSameUom = tran.UOM == poLine.UOM;

			decimal apTranQty = isSameUom ? tran.Qty ?? 0m : tran.BaseQty ?? 0m;
			decimal poLineUnbilledQty = isSameUom ? poLine.UnbilledQty ?? 0m : poLine.BaseUnbilledQty ?? 0m;

			return apTranQty > poLineUnbilledQty;
		}

		public virtual bool IsAPTranUnitCostExceedsPOLineUnitCost(PXCache cache, APTran tran, string apTranCuryID, POLineDTO poLine)
		{
			bool isSameUom = tran.UOM == poLine.UOM;
			bool isSameCury = apTranCuryID == poLine.CuryID;

			decimal apTranUnitCost = isSameCury && isSameUom ? tran.CuryUnitCost ?? 0m
				: IN.INUnitAttribute.ConvertFromBase(cache, tran.InventoryID, tran.UOM, tran.UnitCost ?? 0m, IN.INPrecision.UNITCOST);
			decimal poLineUnitCost = isSameCury && isSameUom ? poLine.CuryUnitCost ?? 0m
				: IN.INUnitAttribute.ConvertFromBase(cache, tran.InventoryID, poLine.UOM, poLine.UnitCost ?? 0m, IN.INPrecision.UNITCOST);

			return apTranUnitCost > poLineUnitCost;
		}

		public virtual bool IsAPTranAmountExceedsPOLineUnbilledAmount(APTran tran, string apTranCuryID, POLineDTO poLine)
		{
			bool isSameCury = apTranCuryID == poLine.CuryID;

			decimal apTranAmt = isSameCury ? tran.CuryTranAmt ?? 0m : tran.TranAmt ?? 0m;
			decimal poLineUnbilledAmt = isSameCury ? poLine.CuryUnbilledAmt ?? 0m : poLine.UnbilledAmt ?? 0m;

			return apTranAmt > poLineUnbilledAmt;
		}
	}
}
