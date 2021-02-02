using PX.Data;
using System;
using PX.Objects.IN;

namespace PX.Objects.AM.Attributes
{
    /// <summary>
    /// Handles the Allocation of Production orders - The manufactured item
    /// </summary>
    public class AMProdItemSplitPlanIDAttribute : INItemPlanIDAttribute
    {
        #region Ctor
        public AMProdItemSplitPlanIDAttribute(Type ParentNoteID, Type ParentHoldEntry)
            : base(ParentNoteID, ParentHoldEntry)
        {
        }
        #endregion

        #region Implementation

        protected virtual string GetPlanType(AMProdItem parent, bool hold)
        {
            // Determine the Plan Type based on Parent Status and the Supply type
            if (parent.StatusID == ProductionOrderStatus.Cancel || parent.StatusID == ProductionOrderStatus.Closed || parent.StatusID == ProductionOrderStatus.Completed)
            {
                return null;
            }

            if (parent.Function == OrderTypeFunction.Disassemble)
            {
                return hold || parent.StatusID == ProductionOrderStatus.Planned ? INPlanConstants.PlanM5 : INPlanConstants.PlanM6;
            }

            switch (parent.SupplyType)
            {
                case ProductionSupplyType.Inventory:
                    return hold || parent.StatusID == ProductionOrderStatus.Planned ? INPlanConstants.PlanM1 : INPlanConstants.PlanM2;
                case ProductionSupplyType.Production:
                    return hold || parent.StatusID == ProductionOrderStatus.Planned ? INPlanConstants.PlanMB : INPlanConstants.PlanMC;
                case ProductionSupplyType.SalesOrder:
                    return hold || parent.StatusID == ProductionOrderStatus.Planned ? INPlanConstants.PlanMD : INPlanConstants.PlanME;
            }

            return null;
        }

        protected virtual AMProdItem GetParentProdItem(PXCache sender, AMProdItemSplit split)
        {
            return (AMProdItem) PXParentAttribute.SelectParent(sender, split, typeof(AMProdItem));
        }

        public override INItemPlan DefaultValues(PXCache sender, INItemPlan planRow, object origRow)
        {
            var splitRow = (AMProdItemSplit)origRow;
            var parent = GetParentProdItem(sender, splitRow);

            planRow.Hold = parent?.Hold ?? false;
            planRow.PlanType = GetPlanType(parent, planRow.Hold.GetValueOrDefault());
            planRow.BAccountID = parent?.CustomerID;
            planRow.InventoryID = splitRow.InventoryID;
            planRow.SubItemID = splitRow.SubItemID;
            planRow.SiteID = splitRow.SiteID;
            planRow.LocationID = splitRow.LocationID;
            planRow.LotSerialNbr = splitRow.LotSerialNbr;
            planRow.PlanDate = parent?.EndDate;
            planRow.PlanQty = splitRow.BaseQty;
            planRow.RefNoteID = parent?.NoteID;

            if (planRow.RefNoteID == Guid.Empty)
            {
                planRow.RefNoteID = null;
            }

            return string.IsNullOrEmpty(planRow.PlanType) || planRow.PlanQty.GetValueOrDefault() == 0 ? null : planRow;
        }
                
        #endregion
    }
}
