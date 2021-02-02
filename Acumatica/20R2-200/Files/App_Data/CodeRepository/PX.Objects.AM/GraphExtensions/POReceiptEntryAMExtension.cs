using System.Collections.Generic;
using System.Linq;
using PX.Data;
using PX.Objects.AM.Attributes;
using PX.Objects.IN;
using PX.Objects.PO;

namespace PX.Objects.AM.GraphExtensions
{
    public class POReceiptEntryAMExtension : PXGraphExtension<POReceiptEntry>
    {
        public static bool IsActive()
        {
            return PXAccess.FeatureInstalled<CS.FeaturesSet.manufacturing>();
        }

        [PXMergeAttributes(Method = MergeMethod.Merge)]
        [POReceiptLineTypeListMfg(typeof(POReceiptLine.inventoryID))]
        protected virtual void POReceiptLine_LineType_CacheAttached(PXCache sender)
        {
        }

        //Used for Add PO Lines Panel. Adding 2 MFG line types to list
        [PXStringList(
            new string[]
            {
                POLineType.GoodsForInventory, POLineType.GoodsForSalesOrder, POLineType.GoodsForReplenishment,
                POLineType.GoodsForDropShip, POLineType.NonStockForDropShip, POLineType.NonStockForSalesOrder,
                POLineType.NonStock, POLineType.Service, POLineType.Freight, POLineType.Description,
                POLineType.GoodsForManufacturing, POLineType.NonStockForManufacturing,
            },
            new string[]
            {
                PX.Objects.PO.Messages.GoodsForInventory, PX.Objects.PO.Messages.GoodsForSalesOrder, PX.Objects.PO.Messages.GoodsForReplenishment,
                PX.Objects.PO.Messages.GoodsForDropShip, PX.Objects.PO.Messages.NonStockForDropShip, PX.Objects.PO.Messages.NonStockForSalesOrder,
                PX.Objects.PO.Messages.NonStockItem, PX.Objects.PO.Messages.Service, PX.Objects.PO.Messages.Freight, PX.Objects.PO.Messages.Description,
                Messages.GoodsForManufacturing, Messages.NonStockForManufacturing,
            }
        )]
        [PXMergeAttributes(Method = MergeMethod.Merge)]
        public void POLineS_LineType_CacheAttached(PXCache sender) { }

        public delegate bool TryProcessReceiptLinkedAllocationDelegate(INTran newline, POReceipt receiptDoc, POLineUOpen poLine, HashSet<PXResult<INItemPlan, INPlanType>> podemand, List<INItemPlan> posupply);

        [PXOverride]
        public virtual bool TryProcessReceiptLinkedAllocation(INTran newline, POReceipt receiptDoc, POLineUOpen poLine, HashSet<PXResult<INItemPlan, INPlanType>> podemand, List<INItemPlan> posupply, TryProcessReceiptLinkedAllocationDelegate del)
        {
            var isMfgNonStock = poLine != null && poLine.LineType == POLineType.NonStockForManufacturing;
            if (isMfgNonStock)
            {
                var planselect = new PXSelect<INItemPlan>(Base);
                var nonstockplantype = new INPlanType { ReplanOnEvent = INPlanConstants.Plan60 };

                var planlist = podemand?.ToList();
                planlist?.AddRange(posupply.ConvertAll<PXResult<INItemPlan, INPlanType>>(_ => new PXResult<INItemPlan, INPlanType>(planselect.Insert(_), nonstockplantype)));
#if DEBUG
                //TODO: process to handle non stock MFG receipt from PO to allocate to the production order (similar to SOOrderEntry.ProcessPOReceipt)
                //SOOrderEntry.ProcessPOReceipt(Base, planlist, receiptDoc.ReceiptType, receiptDoc.ReceiptNbr); 
#endif

                return true;
            }

            return del?.Invoke(newline, receiptDoc, poLine, podemand, posupply) ?? false;
        }
    }
}
