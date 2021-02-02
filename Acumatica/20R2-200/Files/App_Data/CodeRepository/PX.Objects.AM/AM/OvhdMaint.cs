using PX.Data;

namespace PX.Objects.AM
{
    public class OvhdMaint : PXGraph<OvhdMaint>
    {
        public PXSelect<AMOverhead> OvhdRecords;
        public PXSetup<AMBSetup> ambsetup;
        public PXSavePerRow<AMOverhead> Save;

        protected virtual void AMOverhead_RowDeleting(PXCache sender, PXRowDeletingEventArgs e)
        {
            AMOverhead ovhdRec = (AMOverhead)e.Row;
            AMBomOvhd ambomoper = PXSelect<AMBomOvhd, Where<AMBomOvhd.ovhdID, Equal<Required<AMBomOvhd.ovhdID>>>>.Select(this, ovhdRec.OvhdID);
            if (ambomoper != null)
            {
                throw new PXException(Messages.GetLocal(Messages.Overhead_NotDeleted_OnBOM), ambomoper.BOMID);
            }

            AMProdOvhd amprodovhd = PXSelect<AMProdOvhd, Where<AMProdOvhd.ovhdID, Equal<Required<AMProdOvhd.ovhdID>>>>.Select(this, ovhdRec.OvhdID);
            if (amprodovhd != null)
            {
                throw new PXException(Messages.GetLocal(Messages.Overhead_NotDeleted_OnProd), 
                    amprodovhd.OrderType.TrimIfNotNullEmpty(), amprodovhd.ProdOrdID.TrimIfNotNullEmpty());
            }

            AMEstimateOvhd amestimateovhd = PXSelect<AMEstimateOvhd, Where<AMEstimateOvhd.ovhdID, Equal<Required<AMEstimateOvhd.ovhdID>>
                    >>.Select(this, ovhdRec.OvhdID);
            if (amestimateovhd != null)
            {
                throw new PXException(Messages.GetLocal(Messages.Overhead_NotDeleted_OnEstimate),
                    amestimateovhd.EstimateID, amestimateovhd.RevisionID);
            }
        }

        protected virtual void AMOverhead_RowUpdating(PXCache sender, PXRowUpdatingEventArgs e)
        {
            if (!sender.ObjectsEqual<AMOverhead.ovhdID>(e.Row, e.NewRow))
            {
                throw new PXException(Messages.GetLocal(Messages.OverheadIDCannotBeUpdated));
            }
        }
    }
}