using PX.Common;
using PX.Data;
using PX.Objects.CS;
using PX.Objects.IN;
using PX.Objects.PO;
using System;
using System.Collections.Generic;
using System.Linq;

namespace PX.Objects.FS
{
    public class SM_POReceiptEntry : PXGraphExtension<POReceiptEntry>
    {
        public static bool IsActive()
        {
            return PXAccess.FeatureInstalled<FeaturesSet.serviceManagementModule>();
        }

        public override void Initialize()
        {
            base.Initialize();
            Base.onBeforeSalesOrderProcessPOReceipt = ProcessPOReceipt;
        }

        #region Views
        public PXSelect<FSServiceOrder> serviceOrderView;
        public PXSelect<FSSODetSplit> soDetSplitView;
        public PXSelect<FSSODet> soDetView;
        public PXSelect<FSAppointment> appointmentView;
        public PXSelect<FSAppointmentDet> apptDetView;
        public PXSelect<FSApptLineSplit> apptSplitView;
        #endregion

        #region  CacheAttached
        [PXDBString(IsKey = true, IsUnicode = true)]
        [PXParent(typeof(Select<FSServiceOrder, Where<FSServiceOrder.srvOrdType, Equal<Current<FSSODetSplit.srvOrdType>>, And<FSServiceOrder.refNbr, Equal<Current<FSSODetSplit.refNbr>>>>>))]
        [PXDefault()]
        protected virtual void FSSODetSplit_RefNbr_CacheAttached(PXCache sender)
        {
        }

        [PXDBDate()]
        [PXDefault()]
        protected virtual void FSSODetSplit_OrderDate_CacheAttached(PXCache sender)
        {
        }

        [PXDBLong()]
        [INItemPlanIDSimple()]
        protected virtual void FSSODetSplit_PlanID_CacheAttached(PXCache sender)
        {
        }

        [PXDBInt()]
        protected virtual void FSSODetSplit_SiteID_CacheAttached(PXCache sender)
        {
        }

        [PXDBInt()]
        protected virtual void FSSODetSplit_LocationID_CacheAttached(PXCache sender)
        {
        }

        // The selector is removed to avoid validation.
        #region Remove LotSerialNbr Selector
        [PXDBString]
        protected void _(Events.CacheAttached<FSSODetSplit.lotSerialNbr> e) { }

        [PXDBString]
        protected void _(Events.CacheAttached<FSAppointmentDet.lotSerialNbr> e) { }

        [PXDBString]
        protected void _(Events.CacheAttached<FSApptLineSplit.lotSerialNbr> e) { }
        #endregion

        // Attribute PXDBDefault is removed to prevent values ​​explicitly assigned by code from being changed in the Persist.
        #region Remove PXDBDefault
        [PXDBString(20, IsKey = true, IsUnicode = true, InputMask = "")]
        protected void _(Events.CacheAttached<FSApptLineSplit.apptNbr> e) { }

        [PXDBString(15, IsUnicode = true, InputMask = "")]
        protected void _(Events.CacheAttached<FSApptLineSplit.origSrvOrdNbr> e) { }

        [PXDBInt()]
        protected void _(Events.CacheAttached<FSApptLineSplit.origLineNbr> e) { }

        [PXDBDate()]
        protected void _(Events.CacheAttached<FSApptLineSplit.apptDate> e) { }
        #endregion

        #endregion

        #region Event Handlers
        protected virtual void _(Events.RowPersisted<POOrder> e)
        {
            if (e.TranStatus == PXTranStatus.Open && e.Operation == PXDBOperation.Update)
            {
                POOrder poOrderRow = (POOrder)e.Row;
                string poOrderOldStatus = (string)e.Cache.GetValueOriginal<POOrder.status>(poOrderRow);

                if (poOrderOldStatus != poOrderRow.Status)
                {
                    FSPOReceiptProcess.UpdateSrvOrdLinePOStatus(e.Cache.Graph, poOrderRow);
                }
            }
        }

        protected virtual void _(Events.RowPersisted<POLineUOpen> e)
        {
            if (e.TranStatus == PXTranStatus.Open && e.Operation == PXDBOperation.Update)
            {
                POLineUOpen POLineRow = e.Row;
                
                FSSODet fsSODetRow = PXSelect<FSSODet, 
                                        Where<FSSODet.poType, Equal<Required<FSSODet.poType>>,
                                        And<FSSODet.poNbr, Equal<Required<FSSODet.poNbr>>,
                                        And<FSSODet.poLineNbr, Equal<Required<FSSODet.poLineNbr>>>>>>
                                        .Select(Base, POLineRow.OrderType, POLineRow.OrderNbr, POLineRow.LineNbr);

                INItemPlan inItemPlanRow = PXSelect<INItemPlan, 
                                            Where<INItemPlan.supplyPlanID, Equal<Required<INItemPlan.supplyPlanID>>>>
                                            .Select(Base, POLineRow.PlanID);

                FSPOReceiptProcess.UpdatePOReferenceInSrvOrdLine(Base, soDetView, soDetSplitView, appointmentView, apptDetView,
                    fsSODetRow, Base.poOrderUPD.Current, POLineRow.LineNbr, POLineRow.Completed, null, inItemPlanRow, false);
            }
        }
        #endregion

        #region Methods

        public delegate void CopyOrig(POReceiptLine aDest, POLine aSrc, decimal aQtyAdj, decimal aBaseQtyAdj);

        [PXOverride]
        public virtual void Copy(POReceiptLine aDest, POLine aSrc, decimal aQtyAdj, decimal aBaseQtyAdj, CopyOrig del)
        {
            if (del != null)
            {
                del(aDest, aSrc, aQtyAdj, aBaseQtyAdj);
            }

            INItemPlan inItemPlanRow = PXSelect<INItemPlan,
                                       Where<
                                           INItemPlan.supplyPlanID, Equal<Required<INItemPlan.supplyPlanID>>,
                                           And<INItemPlan.planType, Equal<Required<INItemPlan.planType>>>>>
                                       .Select(Base, aSrc.PlanID, INPlanConstants.PlanF6);

            if (inItemPlanRow != null && inItemPlanRow.LocationID != null)
            {
                aDest.LocationID = inItemPlanRow.LocationID;
            }
                
        }

        public virtual List<PXResult<INItemPlan, INPlanType>> ProcessPOReceipt(PXGraph graph, IEnumerable<PXResult<INItemPlan, INPlanType>> list, string POReceiptType, string POReceiptNbr)
        {
            return FSPOReceiptProcess.ProcessPOReceipt(graph, list, POReceiptType, POReceiptNbr, stockItemProcessing: false);
        }
        #endregion
    }
}
