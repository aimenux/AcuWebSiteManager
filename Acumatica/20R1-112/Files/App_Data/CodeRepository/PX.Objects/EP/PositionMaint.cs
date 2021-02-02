using PX.SM;
using System;
using PX.Data;

namespace PX.Objects.EP
{
    public class PositionMaint : PXGraph<PositionMaint>
    {
        public PXSelect<EPPosition> EPPosition;
        public PXSavePerRow<EPPosition> Save;
        public PXCancel<EPPosition> Cancel;
        
        protected virtual void EPPosition_RowInserting(PXCache cache, PXRowInsertingEventArgs e)
        {
            string PositionID = ((EPPosition)e.Row).PositionID;
            EPPosition p = PXSelect<EPPosition, Where<EPPosition.positionID, Equal<Required<EPPosition.positionID>>>>.SelectWindowed(this, 0, 1, PositionID);
            if (p != null)
            {
                cache.RaiseExceptionHandling<EPPosition.positionID>(e.Row, PositionID, new PXException(Messages.RecordExists));
                e.Cancel = true;
            }

        }

        protected virtual void EPPosition_PositionID_FieldUpdating(PXCache cache, PXFieldUpdatingEventArgs e)
        {
            EPPosition row = e.Row as EPPosition;
            if (row != null)
            {
                if (e.NewValue != null && row.PositionID != null)
                {
					EPEmployeePosition employee =
					PXSelect<EPEmployeePosition, Where<EPEmployeePosition.positionID, Equal<Required<EPPosition.positionID>>>>.SelectWindowed(this, 0, 1, row.PositionID);
                    if (employee != null && employee.PositionID != e.NewValue.ToString())
                    {
                        throw new PXSetPropertyException(Messages.PositionInUse);
                    }
                }
            }
        }

        protected virtual void EPPosition_RowDeleting(PXCache cache, PXRowDeletingEventArgs e)
        {
            EPPosition row = e.Row as EPPosition;
            if (row != null)
            {
                EPEmployee p = PXSelectJoin<EPEmployee,
                    LeftJoin<EPEmployeePosition, On<EPEmployee.bAccountID, Equal<EPEmployeePosition.employeeID>>>, 
                    Where<EPEmployeePosition.positionID, Equal<Required<EPPosition.positionID>>>>.SelectSingleBound(this, null, row.PositionID);

                if (p != null)
                {
                    throw new PXException(Messages.PositionInUseForDelete, p.AcctCD);
                }
            }
        }
    }
}