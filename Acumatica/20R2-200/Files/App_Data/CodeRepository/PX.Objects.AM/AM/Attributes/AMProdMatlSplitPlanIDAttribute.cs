using PX.Data;
using System;
using PX.Objects.IN;

namespace PX.Objects.AM.Attributes
{
    /// <summary>
    /// Handles the Allocation of Material for Production orders
    /// </summary>
    public class AMProdMatlSplitPlanIDAttribute : INItemPlanIDAttribute
    {
        #region Ctor
        public AMProdMatlSplitPlanIDAttribute(Type ParentNoteID, Type ParentHoldEntry)
            : base(ParentNoteID, ParentHoldEntry)
        {
        }
        #endregion

        #region Implementation
        bool InitVendor = false;
        bool ResetSupplyPlanID = false;

        protected virtual bool IsLineLinked(AMProdMatlSplit split)
        {
            return split != null && (split.POOrderNbr != null || split.AMProdOrdID != null || split.IsAllocated.GetValueOrDefault());
        }

        public override void RowUpdated(PXCache sender, PXRowUpdatedEventArgs e)
        {
            var isLinked = IsLineLinked((AMProdMatlSplit)e.Row);
            InitVendor = !sender.ObjectsEqual<AMProdMatlSplit.siteID, AMProdMatlSplit.subItemID, AMProdMatlSplit.vendorID, AMProdMatlSplit.pOCreate>(e.Row, e.OldRow) &&
                         !isLinked;
            ResetSupplyPlanID = !isLinked;

            try
            {
                base.RowUpdated(sender, e);
            }
            finally
            {
                InitVendor = false;
                ResetSupplyPlanID = false;
            }
        }

        /// <summary>
        /// Determine the correct material <c>INPlanConstants</c> plan type
        /// </summary>
        /// <param name="prodMatl">Production Material Row</param>
        /// <param name="prodMatlSplit">Production Material Allocation Row</param>
        /// <param name="prodItem">Parent Production Item Row</param>
        /// <returns>A <c>INPlanConstants</c> otherwise null if not applicable</returns>
        protected virtual string GetMaterialPlanType(AMProdMatl prodMatl, AMProdMatlSplit prodMatlSplit, AMProdItem prodItem)
        {
            if (prodMatl == null || prodMatlSplit == null || prodItem == null || ProductionStatus.IsClosedOrCanceled(prodItem))
            {
                return null;
            }

            var isMaterialSupply = prodMatl.IsByproduct.GetValueOrDefault() || prodItem.Function == OrderTypeFunction.Disassemble;
            if (!isMaterialSupply && prodMatlSplit.IsAllocated.GetValueOrDefault())
            {
                return INPlanConstants.PlanM7; /* Production Allocated */
            }

            if (prodMatlSplit.POCreate.GetValueOrDefault())
            {
                return INPlanConstants.PlanM9; /* Production to Purchase */
            }

            if (prodMatlSplit.ProdCreate.GetValueOrDefault())
            {
                return INPlanConstants.PlanMA; /* Production to Production */
            }

            if (!prodItem.Hold.GetValueOrDefault() && prodItem.StatusID != ProductionOrderStatus.Planned)
            {
                return isMaterialSupply
                    ? INPlanConstants.PlanM2 /* Production Supply */
                    : INPlanConstants.PlanM6; /* Production Demand */
            }

            // Prepared types as default
            return isMaterialSupply
                ? INPlanConstants.PlanM1 /* Production Supply Prepared */
                : INPlanConstants.PlanM5; /* Production Demand Prepared */
        }

        protected virtual AMProdItem GetParentProdItem(PXCache sender, AMProdMatlSplit split)
        {
            return (AMProdItem)PXParentAttribute.SelectParent(sender, split, typeof(AMProdItem));
        }

        protected virtual AMProdMatl GetParentProdMatl(PXCache sender, AMProdMatlSplit split)
        {
            return (AMProdMatl)PXParentAttribute.SelectParent(sender, split, typeof(AMProdMatl));
        }

        protected virtual AMProdOper GetParentProdOper(PXCache sender, AMProdMatlSplit split)
        {
            return (AMProdOper)PXParentAttribute.SelectParent(sender, split, typeof(AMProdOper));
        }

        public override INItemPlan DefaultValues(PXCache sender, INItemPlan planRow, object origRow)
        {
            var splitRow = (AMProdMatlSplit)origRow;
            var parent = GetParentProdMatl(sender, splitRow);

            if (parent?.MaterialType == AMMaterialType.Subcontract &&
                parent?.SubcontractSource == AMSubcontractSource.VendorSupplied)
            {
                // No plan record for vendor supplied materail. This will hide from allocation details and MRP
                return null;
            }

            var parentItem = GetParentProdItem(sender, splitRow);
            var parentOper = GetParentProdOper(sender, splitRow);

            planRow.PlanType = GetMaterialPlanType(parent, splitRow, parentItem);
            planRow.Hold = parentItem?.Hold == true;
            planRow.BAccountID = parentItem?.CustomerID;
            planRow.InventoryID = splitRow.InventoryID;
            planRow.SubItemID = splitRow.SubItemID;
            planRow.SiteID = splitRow.SiteID;
            planRow.LocationID = splitRow.LocationID;
            planRow.LotSerialNbr = splitRow.LotSerialNbr;
            if(parent.ProdCreate == true)
            {
                planRow.FixedSource = INReplenishmentSource.Manufactured;
            }

            if (ResetSupplyPlanID)
            {
                planRow.SupplyPlanID = null;
            }

            planRow.VendorID = splitRow.VendorID;

            if (InitVendor || splitRow.POCreate == true && splitRow.VendorID != null && planRow.VendorLocationID == null)
            {
                planRow.VendorLocationID =
                    PX.Objects.PO.POItemCostManager.FetchLocation(
                        sender.Graph,
                        splitRow.VendorID,
                        splitRow.InventoryID,
                        splitRow.SubItemID,
                        splitRow.SiteID);
            }

            planRow.PlanDate = splitRow.TranDate ?? parentOper?.StartDate ?? parentItem?.StartDate;
            planRow.PlanQty = (splitRow.BaseQty - splitRow.BaseQtyReceived).NotLessZero();
            planRow.RefNoteID = parentOper?.NoteID ?? parent?.NoteID;

            return string.IsNullOrEmpty(planRow.PlanType) || planRow.PlanQty.GetValueOrDefault() == 0 ? null : planRow;
        }

        #endregion
    }
}
