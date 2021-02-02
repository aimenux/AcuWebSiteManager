using PX.Objects.AM.Attributes;
using PX.Data;

namespace PX.Objects.AM
{
    public class LaborCodeMaint : PXGraph<LaborCodeMaint>
    {
        public PXSelect<AMLaborCode> LaborCodeRecords;
        public PXSavePerRow<AMLaborCode> Save;

        protected virtual void AMLaborCode_RowSelected(PXCache sender, PXRowSelectedEventArgs e)
        {
            AMLaborCode amLaborCode = (AMLaborCode) e.Row;
            if (amLaborCode == null)
            {
                return;
            }

            PXUIFieldAttribute.SetEnabled<AMLaborCode.overheadAccountID>(sender, e.Row, amLaborCode.LaborType == AMLaborType.Indirect);
            PXUIFieldAttribute.SetEnabled<AMLaborCode.overheadSubID>(sender, e.Row, amLaborCode.LaborType == AMLaborType.Indirect);
        }

        protected virtual void AMLaborCode_LaborType_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
        {
            var amLaborCode = (AMLaborCode)e.Row;
            if (amLaborCode == null || string.IsNullOrWhiteSpace(amLaborCode.LaborType))
            {
                return;
            }

            if (amLaborCode.LaborType == AMLaborType.Direct && (amLaborCode.OverheadAccountID != null || amLaborCode.OverheadSubID != null))
            {
                amLaborCode.OverheadAccountID = null;
                amLaborCode.OverheadSubID = null;
            }
        }

        protected virtual void AMLaborCode_RowPersisting(PXCache sender, PXRowPersistingEventArgs e)
        {
            var amLaborCode = (AMLaborCode) e.Row;
            if (amLaborCode != null && amLaborCode.LaborType == AMLaborType.Indirect)
            {
                if (amLaborCode.OverheadAccountID == null)
                {
                    sender.RaiseExceptionHandling<AMLaborCode.overheadAccountID>(amLaborCode,
                        amLaborCode.OverheadAccountID,
                        new PXSetPropertyException(Messages.OverheadAccountRequiredIndirectLabor, PXErrorLevel.Error));
                }
                if (amLaborCode.OverheadSubID == null)
                {
                    sender.RaiseExceptionHandling<AMLaborCode.overheadSubID>(amLaborCode, amLaborCode.OverheadSubID,
                        new PXSetPropertyException(Messages.OverheadAccountRequiredIndirectLabor, PXErrorLevel.Error));
                }
            }
        }

        protected virtual void AMLaborCode_RowDeleting(PXCache sender, PXRowDeletingEventArgs e)
        {
            var amLaborCode = (AMLaborCode) e.Row;
            if (amLaborCode == null)
            {
                return;
            }

            // Check to see if the Direct Labor code is used in a work center
            AMShift amShift = PXSelect<AMShift, Where<AMShift.laborCodeID, Equal<Required<AMShift.laborCodeID>>>>.Select(this, amLaborCode.LaborCodeID);
            if (amShift != null)
            {
                e.Cancel = true;
                throw new PXException(Messages.GetLocal(Messages.LaborCodeNoDeleteUsedInWC), amShift.WcID, amShift.ShiftID);
            }
        }
    }
}
