using PX.Data;
using PX.Objects.CS;

namespace PX.Objects.FS
{
    public class EquipmentSetupMaint : PXGraph<EquipmentSetupMaint>
    {
        public PXSave<FSEquipmentSetup> Save;
        public PXCancel<FSEquipmentSetup> Cancel;
        public PXSelect<FSEquipmentSetup> EquipmentSetupRecord;

        public EquipmentSetupMaint()
            : base()
        {
            FSEquipmentSetup setup = PXSelectReadonly<FSEquipmentSetup>.Select(this);
            if (setup == null)
            {
                throw new PXSetupNotEnteredException(ErrorMessages.SetupNotEntered, typeof(FSEquipmentSetup), DACHelper.GetDisplayName(typeof(FSSetup)));
            }
        }

        #region Event Handlers

        #region FieldSelecting
        #endregion
        #region FieldDefaulting
        #endregion
        #region FieldUpdating
        #endregion
        #region FieldVerifying
        protected virtual void _(Events.FieldVerifying<FSEquipmentSetup, FSEquipmentSetup.contractPostTo> e)
        {
            if (e.Row == null)
            {
                return;
            }

            FSEquipmentSetup fsEquipmentSetupRow = (FSEquipmentSetup)e.Row;
            if ((string)e.NewValue == ID.Contract_PostTo.SALES_ORDER_INVOICE)
            {
                e.Cache.RaiseExceptionHandling<FSEquipmentSetup.contractPostTo>(fsEquipmentSetupRow, null, new PXSetPropertyException(TX.Error.SOINVOICE_FROM_CONTRACT_NOT_IMPLEMENTED, PXErrorLevel.Error));
            }
        }
        #endregion
        #region FieldUpdated
        protected virtual void _(Events.FieldUpdated<FSEquipmentSetup, FSEquipmentSetup.contractPostTo> e)
        {
            if (e.Row == null)
            {
                return;
            }

            FSEquipmentSetup fsEquipmentSetupRow = (FSEquipmentSetup)e.Row;

            if (fsEquipmentSetupRow.ContractPostTo != (string)e.OldValue)
            {
                SharedFunctions.ValidatePostToByFeatures<FSEquipmentSetup.contractPostTo>(e.Cache, fsEquipmentSetupRow, fsEquipmentSetupRow.ContractPostTo);
            }
        }
        #endregion

        protected virtual void _(Events.RowSelecting<FSEquipmentSetup> e)
        {
        }

        protected virtual void _(Events.RowSelected<FSEquipmentSetup> e)
        {
            if (e.Row == null)
            {
                return;
            }

            FSEquipmentSetup fsEquipmentSetupRow = (FSEquipmentSetup)e.Row;

            EnableDisable_Document(e.Cache, fsEquipmentSetupRow);
            FSPostTo.SetLineTypeList<FSEquipmentSetup.contractPostTo>(e.Cache, e.Row);
        }

        protected virtual void _(Events.RowInserting<FSEquipmentSetup> e)
        {
        }

        protected virtual void _(Events.RowInserted<FSEquipmentSetup> e)
        {
        }

        protected virtual void _(Events.RowUpdating<FSEquipmentSetup> e)
        {
        }

        protected virtual void _(Events.RowUpdated<FSEquipmentSetup> e)
        {
        }

        protected virtual void _(Events.RowDeleting<FSEquipmentSetup> e)
        {
        }

        protected virtual void _(Events.RowDeleted<FSEquipmentSetup> e)
        {
        }

        protected virtual void _(Events.RowPersisting<FSEquipmentSetup> e)
        {
            if (e.Row == null)
            {
                return;
            }

            FSEquipmentSetup fsEquipmentSetupRow = (FSEquipmentSetup)e.Row;

            if (fsEquipmentSetupRow.ContractPostTo == ID.Contract_PostTo.SALES_ORDER_INVOICE)
            {
                e.Cache.RaiseExceptionHandling<FSEquipmentSetup.contractPostTo>(fsEquipmentSetupRow, null, new PXSetPropertyException(TX.Error.SOINVOICE_FROM_CONTRACT_NOT_IMPLEMENTED, PXErrorLevel.Error));
            }

            SharedFunctions.ValidatePostToByFeatures<FSEquipmentSetup.contractPostTo>(e.Cache, fsEquipmentSetupRow, fsEquipmentSetupRow.ContractPostTo);
        }

        protected virtual void _(Events.RowPersisted<FSEquipmentSetup> e)
        {
        }
        
        #endregion

        public static void EnableDisable_Document(PXCache cache, FSSetup fsEquipmentSetupRow)
        {
            bool isDistributionModuleInstalled = PXAccess.FeatureInstalled<FeaturesSet.distributionModule>();

            PXUIFieldAttribute.SetVisible<FSEquipmentSetup.contractPostOrderType>(cache, fsEquipmentSetupRow, isDistributionModuleInstalled && fsEquipmentSetupRow.ContractPostTo == ID.SrvOrdType_PostTo.SALES_ORDER_MODULE);

            if (fsEquipmentSetupRow.ContractPostTo == ID.SrvOrdType_PostTo.SALES_ORDER_MODULE)
            {
                PXDefaultAttribute.SetPersistingCheck<FSEquipmentSetup.contractPostOrderType>(cache, fsEquipmentSetupRow, PXPersistingCheck.NullOrBlank);
            }
            else
            {
                PXDefaultAttribute.SetPersistingCheck<FSEquipmentSetup.contractPostOrderType>(cache, fsEquipmentSetupRow, PXPersistingCheck.Nothing);
            }
        }
    }
}