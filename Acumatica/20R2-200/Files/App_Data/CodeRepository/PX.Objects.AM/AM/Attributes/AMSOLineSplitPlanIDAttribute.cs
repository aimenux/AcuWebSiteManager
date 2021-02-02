using System;
using PX.Data;
using PX.Objects.AM.CacheExtensions;
using PX.Objects.IN;
using PX.Objects.SO;

namespace PX.Objects.AM.Attributes
{
    /// <summary>
    /// Manufacturing extension to SOLineSplitPlanIDAttribute.
    /// Implements "SO to Production" allocation
    /// </summary>
    public class AMSOLineSplitPlanIDAttribute : SOLineSplitPlanIDAttribute
    {
        public const string SoToProductionPlanType = INPlanConstants.PlanM8;

        public AMSOLineSplitPlanIDAttribute(Type ParentNoteID, Type ParentHoldEntry, Type ParentOrderDate) : base(ParentNoteID, ParentHoldEntry, ParentOrderDate)
        {
        }

        protected override bool InitPlanRequired(PXCache cache, PXRowUpdatedEventArgs e)
        {
            return base.InitPlanRequired(cache, e) || !cache.ObjectsEqual<SOLineSplitExt.aMProdCreate>(e.Row, e.OldRow);
        }

        protected override bool IsLineLinked(SOLineSplit soLineSplit)
        {
            return base.IsLineLinked(soLineSplit) || PXCache<SOLineSplit>.GetExtension<SOLineSplitExt>(soLineSplit)?.AMProdOrdID != null;
        }

        protected override string CalcPlanType(INItemPlan plan, SOLineSplit splitRow, SOOrderType ordertype, bool isOrderOnHold)
        {
            var planType = base.CalcPlanType(plan, splitRow, ordertype, isOrderOnHold);

            var splitRowExt = PXCache<SOLineSplit>.GetExtension<SOLineSplitExt>(splitRow);
            if (splitRow?.IsAllocated == false && splitRowExt?.AMProdCreate == true)
            {
                planType = SoToProductionPlanType;
            }

            return planType;
        }

        public override INItemPlan DefaultValues(PXCache sender, INItemPlan planRow, object origRow)
        {
            var splitRow = (SOLineSplit)origRow;
            var splitRowExt = PXCache<SOLineSplit>.GetExtension<SOLineSplitExt>(splitRow);
            var isProductionLinked = splitRowExt != null && splitRowExt.AMProdCreate.GetValueOrDefault() && !string.IsNullOrWhiteSpace(splitRowExt.AMProdOrdID);

            var planRowReturn = base.DefaultValues(sender, planRow, origRow);

            if (planRowReturn == null || splitRowExt == null)
            {
                return planRowReturn;
            }

            if (INPlanTypeHelper.IsMfgPlanType(planRowReturn.PlanType) || isProductionLinked)
            {
                //It is possible during production creation the order gets marked as linked row however...
                //  this doesn't give the plan type enough time to set as M8 due to IsLineLinked(SOLineSplit) reporting back a linked row
                planRowReturn.PlanType = SoToProductionPlanType;

                planRowReturn.FixedSource = INReplenishmentSource.Manufactured;
                planRowReturn.PlanQty = splitRow.BaseQty.GetValueOrDefault() - splitRow.BaseReceivedQty.GetValueOrDefault() - splitRow.BaseShippedQty.GetValueOrDefault();

                if (planRowReturn.PlanQty.GetValueOrDefault() <= 0)
                {
                    return null;
                }
            }
            
            return planRowReturn;
        }
    }
}