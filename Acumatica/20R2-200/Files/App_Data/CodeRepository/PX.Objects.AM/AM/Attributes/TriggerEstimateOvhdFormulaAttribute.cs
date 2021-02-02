using PX.Data;

namespace PX.Objects.AM.Attributes
{
    /// <summary>
    /// Due to <see cref="PX.Data.Parent"/> not triggering a formula to update, add this to AMEstimateOper fields where the change of value should re-trigger estimate overhead row formulas
    /// </summary>
    public class TriggerEstimateOvhdFormulaAttribute : PXEventSubscriberAttribute, IPXFieldUpdatedSubscriber
    {
        public void FieldUpdated(PXCache cache, PXFieldUpdatedEventArgs e)
        {
            var amEstimateOper = (AMEstimateOper)e.Row;
            if (amEstimateOper == null)
            {
                return;
            }

            foreach (AMEstimateOvhd estimateOvhd in PXSelect<AMEstimateOvhd, Where<AMEstimateOvhd.estimateID,
                Equal<Required<AMEstimateOper.estimateID>>, And<AMEstimateOvhd.revisionID, Equal<Required<AMEstimateOper.revisionID>>,
                    And<AMEstimateOvhd.operationID, Equal<Required<AMEstimateOper.operationID>>
                    >>>>.Select(cache.Graph, amEstimateOper.EstimateID, amEstimateOper.RevisionID, amEstimateOper.OperationID))
            {
                cache.Graph.Caches<AMEstimateOvhd>().RaiseFieldUpdated<AMEstimateOvhd.overheadCostRate>(estimateOvhd, estimateOvhd.OverheadCostRate);
            }
            bool IsReadOnly = cache.GetStatus(e.Row) == PXEntryStatus.Notchanged;
            PXFormulaAttribute.CalcAggregate<AMEstimateOvhd.variableOvhdOperCost>(cache.Graph.Caches<AMEstimateOvhd>(), e.Row, IsReadOnly);
            cache.RaiseFieldUpdated<AMEstimateOper.variableOverheadCalcCost>(e.Row, null);
        }
    }
}