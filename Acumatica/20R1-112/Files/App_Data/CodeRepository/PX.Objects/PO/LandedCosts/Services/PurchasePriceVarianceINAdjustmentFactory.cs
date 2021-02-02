using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PX.Data;
using PX.Objects.CS;
using PX.Objects.IN;

namespace PX.Objects.PO.LandedCosts
{
	public class PurchasePriceVarianceINAdjustmentFactory
	{
		private readonly INAdjustmentEntry _inGraph;

		public PurchasePriceVarianceINAdjustmentFactory(INAdjustmentEntry inGraph)
		{
			_inGraph = inGraph;
		}

		public virtual void CreateAdjustmentTran(List<AllocationServiceBase.POReceiptLineAdjustment> pOLinesToProcess, string ReasonCode)
		{
			foreach (AllocationServiceBase.POReceiptLineAdjustment poreceiptline in pOLinesToProcess)
			{
				INTran intran = new INTran();

				INTran origtran = PurchasePriceVarianceAllocationService.Instance.GetOriginalInTran(_inGraph, poreceiptline.ReceiptLine.ReceiptNbr, poreceiptline.ReceiptLine.LineNbr);
				bool isDropShip = (poreceiptline.ReceiptLine.LineType == POLineType.GoodsForDropShip || poreceiptline.ReceiptLine.LineType == POLineType.NonStockForDropShip);

				if (!isDropShip && origtran == null) throw new PXException(AP.Messages.CannotFindINReceipt, poreceiptline.ReceiptLine.ReceiptNbr);

				//Drop-Ships are considered non-stocks
				if (poreceiptline.ReceiptLine.IsStockItem())
				{
					intran.TranType = INTranType.ReceiptCostAdjustment;
				}
				else
				{
					//Landed cost and PPV for non-stock items are handled in special way in the inventory.
					//They should create a GL Batch, but for convinience and unforminty this functionality is placed into IN module
					//Review this part when the functionality is implemented in IN module
					intran.IsCostUnmanaged = true;
					intran.TranType = INTranType.Adjustment;
					intran.InvtMult = 0;
				}
				intran.InventoryID = poreceiptline.ReceiptLine.InventoryID;
				intran.SubItemID = poreceiptline.ReceiptLine.SubItemID;
				intran.SiteID = poreceiptline.ReceiptLine.SiteID;
				intran.BAccountID = poreceiptline.ReceiptLine.VendorID;
				intran.BranchID = poreceiptline.BranchID;


				if (isDropShip && intran.SiteID != null)
				{
					INSite invSite = INSite.PK.Find(_inGraph, intran.SiteID);
					if (invSite.DropShipLocationID == null)
					{
						throw new PXException(SO.Messages.NoDropShipLocation, invSite.SiteCD);
					}

					intran.LocationID = invSite.DropShipLocationID;
				}
				else
				{
					intran.LocationID = poreceiptline.ReceiptLine.LocationID ?? origtran.LocationID;
				}

				intran.LotSerialNbr = poreceiptline.ReceiptLine.LotSerialNbr;
				intran.POReceiptType = poreceiptline.ReceiptLine.ReceiptType;
				intran.POReceiptNbr = poreceiptline.ReceiptLine.ReceiptNbr;
				intran.POReceiptLineNbr = poreceiptline.ReceiptLine.LineNbr;

				//tran.Qty = poreceiptline.ReceiptQty;
				intran.TranDesc = poreceiptline.ReceiptLine.TranDesc;
				//tran.UnitCost = PXDBPriceCostAttribute.Round(inGraph.transactions.Cache, (decimal)(poreceiptline.ExtCost / poreceiptline.ReceiptQty));
				intran.TranCost = poreceiptline.AllocatedAmt;
				intran.ReasonCode = ReasonCode;
				if (origtran != null && origtran.DocType == INDocType.Issue)
				{
					intran.ARDocType = origtran.ARDocType;
					intran.ARRefNbr = origtran.ARRefNbr;
					intran.ARLineNbr = origtran.ARLineNbr;
				}
				if (!isDropShip)
				{
					intran.OrigDocType = origtran.DocType;
					intran.OrigTranType = origtran.TranType;
					intran.OrigRefNbr = origtran.RefNbr;
				}

				int? acctID = null;
				int? subID = null;

				//Set AcctID and SubID = POAccrual Acct/Sub from orig. INTran
				if (origtran != null)
				{
					intran.AcctID = origtran.AcctID;
					intran.SubID = origtran.SubID;
				}
				else if (isDropShip)
				{
					intran.AcctID = poreceiptline.ReceiptLine.POAccrualAcctID;
					intran.SubID = poreceiptline.ReceiptLine.POAccrualSubID;
				}
				ReasonCode reasonCode = CS.ReasonCode.PK.Find(_inGraph, ReasonCode);
				if (reasonCode == null)
					throw new PXException(AP.Messages.ReasonCodeCannotNotFound, ReasonCode);
				AP.APReleaseProcess.GetPPVAccountSub(ref acctID, ref subID, _inGraph, poreceiptline.ReceiptLine, reasonCode);

				intran.COGSAcctID = acctID;
				intran.COGSSubID = subID;

				intran = _inGraph.transactions.Insert(intran);
			}
		}
	}
}
