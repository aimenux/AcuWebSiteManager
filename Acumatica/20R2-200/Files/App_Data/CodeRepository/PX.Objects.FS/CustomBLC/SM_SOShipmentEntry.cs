using PX.Data;
using PX.Objects.CS;
using PX.Objects.SO;
using System;

namespace PX.Objects.FS
{
    public class SM_SOShipmentEntry : PXGraphExtension<SOShipmentEntry>
    {
        public static bool IsActive()
        {
            return PXAccess.FeatureInstalled<FeaturesSet.serviceManagementModule>();
        }

        public delegate void CreateShipmentDelegate(SOOrder order,
                                                    int? SiteID,
                                                    DateTime? ShipDate,
                                                    bool? useOptimalShipDate,
                                                    string operation,
                                                    DocumentList<SOShipment> list,
                                                    PXQuickProcess.ActionFlow quickProcessFlow);

        [PXOverride]
        public virtual void CreateShipment(SOOrder order,
                                           int? SiteID,
                                           DateTime? ShipDate,
                                           bool? useOptimalShipDate,
                                           string operation,
                                           DocumentList<SOShipment> list,
                                           PXQuickProcess.ActionFlow quickProcessFlow,
                                           CreateShipmentDelegate del)
        {
            ValidatePostBatchStatus(PXDBOperation.Update, ID.Batch_PostTo.SO, order.OrderType, order.RefNbr);
            del(order, SiteID, ShipDate, useOptimalShipDate, operation, list, quickProcessFlow);
        }

        #region Validations
        public virtual void ValidatePostBatchStatus(PXDBOperation dbOperation, string postTo, string createdDocType, string createdRefNbr)
        {
            DocGenerationHelper.ValidatePostBatchStatus<SOShipment>(Base, dbOperation, postTo, createdDocType, createdRefNbr);
        }
        #endregion
    }
}