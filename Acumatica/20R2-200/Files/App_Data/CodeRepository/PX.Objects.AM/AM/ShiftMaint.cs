using PX.Data;

namespace PX.Objects.AM
{
    /// <summary>
    /// Manufacturing Shift Maintenance
    /// </summary>
    public class ShiftMaint : PXGraph<ShiftMaint>
    {
        public PXSelect<AMShiftMst> ShiftRecords;
        public PXSetup<AMBSetup> ambsetup;
        public PXSavePerRow<AMShiftMst> Save;

        protected virtual void AMShiftMst_RowDeleting(PXCache sender, PXRowDeletingEventArgs e)
        {
            var row = (AMShiftMst)e.Row;

            // Should not allow delete of shift in use
            var ex = CheckForReferences(row);
            if (ex != null)
            {
                e.Cancel = true;
                throw ex;
            }
        }

        protected virtual void AMShiftMst_ShiftID_FieldUpdating(PXCache sender, PXFieldUpdatingEventArgs e)
        {
            var row = (AMShiftMst)e.Row;
            if (row == null || string.IsNullOrWhiteSpace(row.ShiftID))
            {
                return;
            }

            var newValue = (string)e.NewValue;

            if (row.ShiftID.EqualsWithTrim(newValue))
            {
                return;
            }

            // Should not allow rename of shift in use
            var ex = CheckForReferences(row);
            if (ex != null)
            {
                e.NewValue = row.ShiftID;
                e.Cancel = true;
                throw ex;
            }
        }

        /// <summary>
        /// If the shift contains a reference then an exception is returned which indicates the shift is in use
        /// </summary>
        protected virtual PXSetPropertyException CheckForReferences(AMShiftMst shiftMst)
        {
            if (shiftMst == null)
            {
                return null;
            }

            AMShift workCenterShift = PXSelect<AMShift, Where<AMShift.shiftID, Equal<Required<AMShift.shiftID>>>>.Select(this, shiftMst.ShiftID);

            if (workCenterShift != null)
            {
                return new PXSetPropertyException(Messages.GetLocal(Messages.ShiftIDInUse), shiftMst.ShiftID.TrimIfNotNullEmpty(),
                    workCenterShift.WcID.TrimIfNotNullEmpty(), PXErrorLevel.Error);
            }

            return null;
        }

    }
}