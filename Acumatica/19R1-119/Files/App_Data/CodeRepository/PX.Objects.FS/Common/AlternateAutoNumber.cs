using PX.Data;
using PX.Objects.CS;
using System;

namespace PX.Objects.FS
{
    public abstract class AlternateAutoNumberAttribute : AutoNumberAttribute
    {
        private string originalRefNbr = null;

        public AlternateAutoNumberAttribute(Type setupField, Type dateField)
            : base(setupField, dateField)
        {
        }

        public AlternateAutoNumberAttribute()
            : base(typeof(FSSetup.empSchdlNumberingID), typeof(AccessInfo.businessDate))
        {
        }

        public override void RowPersisted(PXCache sender, PXRowPersistedEventArgs e)
        {
            if (e.Row == null)
            {
                return;
            }

            if (e.TranStatus == PXTranStatus.Aborted && e.Operation == PXDBOperation.Insert)
            {
                if (originalRefNbr != null)
                {
                    sender.SetValue(e.Row, _FieldName, originalRefNbr);
                    sender.Normalize();
                }
            }

            if (e.TranStatus == PXTranStatus.Aborted || e.TranStatus == PXTranStatus.Completed)
            {
                originalRefNbr = null;
            }
        }

        public override void RowPersisting(PXCache sender, PXRowPersistingEventArgs e)
        {
            if (e.Row == null)
            {
                return;
            }

            if (e.Operation == PXDBOperation.Insert)
            {
                originalRefNbr = (string)sender.GetValue(e.Row, _FieldName);

                if (SetRefNbr(sender, e.Row) == false)
                {
                    throw new AutoNumberException();
                }
                else 
                { 
                    sender.Normalize();
                }
            }
        }

        protected abstract bool SetRefNbr(PXCache cache, object row);
    }
}