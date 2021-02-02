using PX.Data;
using PX.Objects.AP;
using PX.Objects.CS;
using PX.Objects.PO;
using System;
using System.Collections.Generic;

namespace PX.Objects.FS
{
    public class SM_APReleaseProcess : PXGraphExtension<APReleaseProcess>
    {
        public static bool IsActive()
        {
            return PXAccess.FeatureInstalled<FeaturesSet.serviceManagementModule>();
        }

        [PXHidden]
        public PXSelect<FSServiceOrder> serviceOrderView;

        #region Overrides
        [PXOverride]
        public void VerifyStockItemLineHasReceipt(APRegister arRegisterRow, Action<APRegister> del)
        {
            if (arRegisterRow.CreatedByScreenID != ID.ScreenID.INVOICE_BY_APPOINTMENT
                    && arRegisterRow.CreatedByScreenID != ID.ScreenID.INVOICE_BY_SERVICE_ORDER)
            {
                if (del != null)
                {
                    del(arRegisterRow);
                }
            }
        }

        public delegate APRegister OnBeforeReleaseDelegate(APRegister apdoc);

        [PXOverride]
        public virtual APRegister OnBeforeRelease(APRegister apdoc, OnBeforeReleaseDelegate del)
        {
            ValidatePostBatchStatus(PXDBOperation.Update, ID.Batch_PostTo.AP, apdoc.DocType, apdoc.RefNbr);

            if (del != null)
            {
                return del(apdoc);
            }

            return null;
        }
        #endregion

        #region Event Handlers
        protected virtual void _(Events.RowPersisted<POOrder> e)
        {
            if (e.TranStatus == PXTranStatus.Open && e.Operation == PXDBOperation.Update)
            {
                POOrder poOrderRow = (POOrder)e.Row;
                string poOrderOldStatus = (string)e.Cache.GetValueOriginal<POOrder.status>(poOrderRow);

                bool updateLines = false;
                List<POLine> poLineUpdatedList = new List<POLine>();

                foreach (object row in Base.poOrderLineUPD.Cache.Updated)
                {
                    if ((bool?)Base.poOrderLineUPD.Cache.GetValue<POLineUOpen.completed>(row) != false)
                    {
                        updateLines = true;
                    }

                    poLineUpdatedList.Add(SharedFunctions.ConvertToPOLine((POLineUOpen)row));
                }

                if (poOrderOldStatus != poOrderRow.Status || updateLines == true)
                {
                    SharedFunctions.UpdateFSSODetReferences(e.Cache.Graph, serviceOrderView.Cache, poOrderRow, poLineUpdatedList);
                }
            }
        }
        #endregion

        #region Validations
        public virtual void ValidatePostBatchStatus(PXDBOperation dbOperation, string postTo, string createdDocType, string createdRefNbr)
        {
            DocGenerationHelper.ValidatePostBatchStatus<APRegister>(Base, dbOperation, postTo, createdDocType, createdRefNbr);
        }
        #endregion
    }
}
