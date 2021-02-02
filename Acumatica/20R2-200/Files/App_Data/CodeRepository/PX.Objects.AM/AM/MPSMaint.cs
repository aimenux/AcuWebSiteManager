using System;
using PX.Common;
using PX.Data;
using PX.Objects.CS;

namespace PX.Objects.AM
{
    public class MPSMaint : PXGraph<MPSMaint>
    {
        public PXSave<AMMPS> Save;
        public PXCancel<AMMPS> Cancel;
        public PXInsert<AMMPS> Insert;
        public PXDelete<AMMPS> Delete;
        public PXCopyPasteAction<AMMPS> CopyPaste;

        [PXFilterable]
        [PXImport(typeof(AMMPS))]
        public PXSelect<AMMPS> AMMPSRecords;
        public PXSetup<AMRPSetup> setup;

        public MPSMaint()
        {
            PXUIFieldAttribute.SetDisplayName<AMBomItemActive.descr>(this.Caches<AMBomItemActive>(), "BOM Description");
        }

        protected virtual void AMMPS_PlanDate_FieldVerifying(PXCache cache, PXFieldVerifyingEventArgs e)
        {
            var ammps = (AMMPS)e.Row;
            var mpsDate = Common.Current.BusinessDate(this).AddDays(setup.Current.MPSFence.GetValueOrDefault());
 
            if ((DateTime)e.NewValue < mpsDate)
            {
                cache.RaiseExceptionHandling<AMMPS.planDate>(ammps, ammps.PlanDate, new PXSetPropertyException(AM.Messages.GetLocal(AM.Messages.MpsMaintPlanDateWarning), PXErrorLevel.Warning));
            }
        }

        protected virtual void AMMPS_RowPersisting(PXCache sender, PXRowPersistingEventArgs e)
        {
            if ((AMMPS) e.Row != null && ((AMMPS) e.Row).Qty.GetValueOrDefault() <= 0)
            {
                //prevents the records from saving with a quantity default of zero
                object qty = ((AMMPS)e.Row).Qty.GetValueOrDefault();
                sender.RaiseFieldVerifying<AMMPS.qty>((AMMPS)e.Row, ref qty);
            }
        }

        protected virtual void AMMPS_Qty_FieldVerifying(PXCache cache, PXFieldVerifyingEventArgs e)
        {
            var ammps = (AMMPS)e.Row;
            if ((decimal)e.NewValue <= 0)
            {
                cache.RaiseExceptionHandling<AMMPS.qty>(e.Row, ammps.Qty, new PXSetPropertyException(AM.Messages.GetLocal(AM.Messages.QuantityGreaterThanZero)));
            }
        }

        protected virtual void AMMPS_BOMID_FieldDefaulting(PXCache cache, PXFieldDefaultingEventArgs e)
        {
            if (IsImport || IsContractBasedAPI)
            {
                return;
            }

            e.NewValue = GetBomID((AMMPS)e.Row);
        }

        protected virtual string GetBomID(AMMPS mps)
        {
            if (mps?.InventoryID == null || mps.SiteID == null)
            {
                return null;
            }

            var id = new PrimaryBomIDManager(this)
            {
                BOMIDType = PrimaryBomIDManager.BomIDType.Planning
            };

            var planBomId = id.GetPrimaryAllLevels(mps.InventoryID, mps.SiteID, mps.SubItemID);
            if (!string.IsNullOrWhiteSpace(planBomId))
            {
                return planBomId;
            }

            id.BOMIDType = PrimaryBomIDManager.BomIDType.Default;
            return id.GetPrimaryAllLevels(mps.InventoryID, mps.SiteID, mps.SubItemID);
        }

        protected virtual void AMMPS_RowSelected(PXCache sender, PXRowSelectedEventArgs e)
        {
            var ammps = (AMMPS)e.Row;

            if (ammps == null)
            {
                return;
            }

            // Get the Numbering for the Row
            Numbering numbering = PXSelectJoin<Numbering, InnerJoin<AMMPSType, On<AMMPSType.mpsNumberingID, Equal<Numbering.numberingID>>>,
                Where<AMMPSType.mPSTypeID, Equal<Required<AMMPSType.mPSTypeID>>>>.Select(this, ammps.MPSTypeID);

            var userNumbering = numbering?.UserNumbering == true;

            PXUIFieldAttribute.SetVisible<AMMPS.mPSID>(sender, ammps, userNumbering);
            PXUIFieldAttribute.SetEnabled<AMMPS.mPSID>(sender, ammps, userNumbering);
        }

        protected virtual void AMMPS_RowInserting(PXCache cache, PXRowInsertingEventArgs e)
        {
            // Temp key to allow multiple inserts before persisting. When persisting and auto number it will swap the value for us.
            var insertedCounter = cache.Inserted.Count() + 1;
            ((AMMPS)e.Row).MPSID = $"-{insertedCounter}";
        }
    }
}