using PX.Data;
using PX.Objects.IN;

namespace PX.Objects.AM
{
    public class MPSType : PXGraph<MPSType>
    {
        public PXSelect<AMMPSType> AMMPSTypeRecords;
        public PXSavePerRow<AMMPSType> Save;

        protected virtual void AMMPSType_RowDeleting(PXCache sender, PXRowDeletingEventArgs e)
        {
            var row = (AMMPSType) e.Row;
            if (row?.MPSTypeID == null)
            {
                return;
            }

            var sb = new System.Text.StringBuilder();

            //Check for default MPS...
            AMRPSetup setup = PXSelect<AMRPSetup>.Select(this);
            if (setup != null && setup.DefaultMPSTypeID.EqualsWithTrim(row.MPSTypeID))
            {
                sb.AppendFormat(AM.Messages.MPSTypeReferencedSetup, row.MPSTypeID);
            }

            //Check for active MPS...
            foreach (PXResult<AMMPS, InventoryItem> result in PXSelectJoinGroupBy<AMMPS, 
                InnerJoin<InventoryItem, On<AMMPS.inventoryID, Equal<InventoryItem.inventoryID>>>,
                Where<AMMPS.activeFlg, Equal<True>,
                And<AMMPS.mPSTypeID, Equal<Required<AMMPS.mPSTypeID>>>>,
                Aggregate<GroupBy<AMMPS.inventoryID>>
                >.SelectWindowed(this, 0, 5, row.MPSTypeID))
            {
                var inventoryItem = (InventoryItem) result;

                if (inventoryItem?.InventoryCD == null)
                {
                    return;
                }

                sb.AppendFormat(AM.Messages.MPSTypeReferencedActiveMPS, row.MPSTypeID, inventoryItem.InventoryCD.TrimIfNotNullEmpty());
            }

            if (sb.Length <= 0)
            {
                return;
            }

            e.Cancel = true;
            throw new PXSetPropertyException(sb.ToString(), PXErrorLevel.Error);
        }
    }
}