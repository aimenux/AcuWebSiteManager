using PX.Data;
using PX.Objects.CS;

namespace PX.Objects.FS
{
    public class RouteSetupMaint : PXGraph<RouteSetupMaint>
    {
        public PXSave<FSRouteSetup> Save;
        public PXCancel<FSRouteSetup> Cancel;
        public PXSelect<FSRouteSetup> RouteSetupRecord;
        public PXSelect<FSSetup> SetupRecord;

        public RouteSetupMaint()
            : base()
        {
            FSSetup setup = PXSelectReadonly<FSSetup>.Select(this);
            if (setup == null)
            {
                throw new PXSetupNotEnteredException(ErrorMessages.SetupNotEntered, typeof(FSSetup), PXMessages.LocalizeNoPrefix(TX.ScreenName.SERVICE_PREFERENCES));
            }
        }

        public virtual void FSRouteSetup_RowSelected(PXCache cache, PXRowSelectedEventArgs e)
        {
            if (e.Row == null)
            {
                return;
            }

            if (SetupRecord.Current == null)
            {
                SetupRecord.Current = SetupRecord.Select();
            }
        }

        public virtual void FSSetup_RowSelected(PXCache cache, PXRowSelectedEventArgs e)
        {
            if (e.Row == null)
            {
                return;
            }

            FSSetup fsSetupRow = (FSSetup)e.Row;

            EquipmentSetupMaint.EnableDisable_Document(cache, fsSetupRow);
            SharedFunctions.ValidatePostToByFeatures<FSSetup.contractPostTo>(cache, fsSetupRow, fsSetupRow.ContractPostTo);
        }

        public virtual void FSSetup_ContractPostTo_FieldUpdated(PXCache cache, PXFieldUpdatedEventArgs e)
        {
            if (e.Row == null)
            {
                return;
            }

            FSSetup fsSetupRow = (FSSetup)e.Row;

            if (fsSetupRow.ContractPostTo != (string)e.OldValue)
            {
                SharedFunctions.ValidatePostToByFeatures<FSSetup.contractPostTo>(cache, fsSetupRow, fsSetupRow.ContractPostTo);
            }
        }

        public virtual void FSSetup_ContractPostTo_FieldVerifying(PXCache cache, PXFieldVerifyingEventArgs e)
        {
            if (e.Row == null)
            {
                return;
            }

            FSSetup fsSetupRow = (FSSetup)e.Row;
            if ((string)e.NewValue == ID.Contract_PostTo.SALES_ORDER_INVOICE)
            {
                cache.RaiseExceptionHandling<FSSetup.contractPostTo>(fsSetupRow, null, new PXSetPropertyException(TX.Error.CONTRACT_SO_INVOICE_NOT_IMPLEMENTED, PXErrorLevel.Error));
            }
        }

        public virtual void FSSetup_RowPersisting(PXCache cache, PXRowPersistingEventArgs e)
        {
            if (e.Row == null)
            {
                return;
            }

            FSSetup fsSetupRow = (FSSetup)e.Row;

            if (fsSetupRow.ContractPostTo == ID.Contract_PostTo.SALES_ORDER_INVOICE)
            {
                cache.RaiseExceptionHandling<FSSetup.contractPostTo>(fsSetupRow, null, new PXSetPropertyException(TX.Error.CONTRACT_SO_INVOICE_NOT_IMPLEMENTED, PXErrorLevel.Error));
            }

            SharedFunctions.ValidatePostToByFeatures<FSSetup.contractPostTo>(cache, fsSetupRow, fsSetupRow.ContractPostTo);
        }
    }
}