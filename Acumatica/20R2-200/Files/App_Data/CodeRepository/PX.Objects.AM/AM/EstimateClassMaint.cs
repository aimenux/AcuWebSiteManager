using PX.Data;

namespace PX.Objects.AM
{
    public class EstimateClassMaint : PXGraph<EstimateClassMaint, AMEstimateClass>
    {
        public PXSelect<AMEstimateClass> EstimateClassRecords;

        protected virtual void AMEstimateClass_RowDeleting(PXCache sender, PXRowDeletingEventArgs e)
        {
            var estimateClass = (AMEstimateClass)e.Row;
            if (estimateClass == null)
            {
                return;
            }

            AMEstimateItem amEstimateItem = PXSelect<AMEstimateItem, 
                Where<AMEstimateItem.estimateClassID, Equal<Required<AMEstimateClass.estimateClassID>>>>.Select(this, estimateClass.EstimateClassID);
            if (amEstimateItem == null)
            {
                return;
            }
            sender.RaiseExceptionHandling<AMEstimateClass.estimateClassID>(
                estimateClass,
                estimateClass.EstimateClassID,
                new PXSetPropertyException(Messages.GetLocal(Messages.EstimateClass_NotDeleted,
                    amEstimateItem.EstimateID, amEstimateItem.RevisionID), PXErrorLevel.Error)
            );
            e.Cancel = true;
        }
    }
}