using PX.Data;
using PX.Objects.CS;

namespace PX.Objects.FS
{
    public class EquipmentSetupMaint : PXGraph<EquipmentSetupMaint>
    {
        public PXSave<FSSetup> Save;
        public PXCancel<FSSetup> Cancel;
        public PXSelect<FSSetup> SetupRecord;

        public EquipmentSetupMaint()
            : base()
        {
            FSSetup setup = PXSelectReadonly<FSSetup>.Select(this);
            if (setup == null)
            {
                throw new PXSetupNotEnteredException(ErrorMessages.SetupNotEntered, typeof(FSSetup), PXMessages.LocalizeNoPrefix(TX.ScreenName.SERVICE_PREFERENCES));
            }
        }

        public static void EnableDisable_Document(PXCache cache, FSSetup fsSetupRow)
        {
            bool isDistributionModuleInstalled = PXAccess.FeatureInstalled<FeaturesSet.distributionModule>();

            PXUIFieldAttribute.SetVisible<FSSetup.contractPostOrderType>(cache, fsSetupRow, isDistributionModuleInstalled && fsSetupRow.ContractPostTo == ID.SrvOrdType_PostTo.SALES_ORDER_MODULE);

            if (fsSetupRow.ContractPostTo == ID.SrvOrdType_PostTo.SALES_ORDER_MODULE)
            {
                PXDefaultAttribute.SetPersistingCheck<FSSetup.contractPostOrderType>(cache, fsSetupRow, PXPersistingCheck.NullOrBlank);
            }
            else
            {
                PXDefaultAttribute.SetPersistingCheck<FSSetup.contractPostOrderType>(cache, fsSetupRow, PXPersistingCheck.Nothing);
            }
        }

        public virtual void FSSetup_RowSelected(PXCache cache, PXRowSelectedEventArgs e)
        {
            if (e.Row == null)
            {
                return;
            }

            FSSetup fsSetupRow = (FSSetup)e.Row;

            EnableDisable_Document(cache, fsSetupRow);
        }

        public virtual void FSSetup_ContractPostTo_FieldVerifying(PXCache cache, PXFieldVerifyingEventArgs e)
        {
            if (e.Row == null)
            {
                return;
            }

            FSSetup fsSetupRow = (FSSetup)e.Row;
            if((string)e.NewValue == ID.Contract_PostTo.SALES_ORDER_INVOICE)
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

        [System.Obsolete("Remove for 2019R2")]
        public virtual void FSSetup_ContractPostTo_FieldDefaulting(PXCache cache, PXFieldDefaultingEventArgs e)
        {
            if (e.Row == null)
            {
                return;
            }

            e.NewValue = ID.Contract_PostTo.ACCOUNTS_RECEIVABLE_MODULE;
        }
    }
}