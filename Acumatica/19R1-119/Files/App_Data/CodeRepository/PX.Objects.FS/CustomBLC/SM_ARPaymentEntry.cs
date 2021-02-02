using PX.Data;
using PX.Objects.AR;
using PX.Objects.CS;
using PX.Objects.CM;
using System.Collections;
using System;

namespace PX.Objects.FS
{
    public class SM_ARPaymentEntry : PXGraphExtension<ARPaymentEntry>
    {
        public static bool IsActive()
        {
            return PXAccess.FeatureInstalled<FeaturesSet.serviceManagementModule>();
        }

        #region Views
        [PXCopyPasteHiddenView]
        public PXSelectJoin<FSAdjust,
                    LeftJoin<FSServiceOrder,
                        On<FSServiceOrder.srvOrdType, Equal<FSAdjust.adjdOrderType>,
                            And<FSServiceOrder.refNbr, Equal<FSAdjust.adjdOrderNbr>>>>,
                    Where<
                        FSAdjust.adjgDocType, Equal<Current<ARPayment.docType>>,
                        And<FSAdjust.adjgRefNbr, Equal<Current<ARPayment.refNbr>>>>>
                    FSAdjustments;
        #endregion

        #region Actions
        public PXAction<ARPayment> viewFSDocumentToApply;
        [PXUIField(
            DisplayName = "View Service Order",
            MapEnableRights = PXCacheRights.Select,
            MapViewRights = PXCacheRights.Select,
            Visible = false)]
        [PXLookupButton]
        public virtual IEnumerable ViewFSDocumentToApply(PXAdapter adapter)
        {
            FSAdjust fsAdjustRow = FSAdjustments.Current;

            if (fsAdjustRow != null 
                && !(String.IsNullOrEmpty(fsAdjustRow.AdjdOrderType) 
                        || String.IsNullOrEmpty(fsAdjustRow.AdjdOrderNbr)))
            {
                ServiceOrderEntry graphServiceOrderEntry = PXGraph.CreateInstance<ServiceOrderEntry>();
                graphServiceOrderEntry.ServiceOrderRecords.Current = graphServiceOrderEntry.ServiceOrderRecords.Search<FSServiceOrder.refNbr>(fsAdjustRow.AdjdOrderNbr, fsAdjustRow.AdjdOrderType);

                if (graphServiceOrderEntry.ServiceOrderRecords.Current != null)
                {
                    throw new PXRedirectRequiredException(graphServiceOrderEntry, true, "View Service Order") { Mode = PXBaseRedirectException.WindowMode.NewWindow };
                }
            }
            return adapter.Get();
        }
        
        public PXAction<ARPayment> viewFSAppointmentSource;
        [PXUIField(
            DisplayName = "View Appointment Source",
            MapEnableRights = PXCacheRights.Select,
            MapViewRights = PXCacheRights.Select,
            Visible = false)]
        [PXLookupButton]
        public virtual IEnumerable ViewFSAppointmentSource(PXAdapter adapter)
        {
            FSAdjust fsAdjustRow = FSAdjustments.Current;

            if (fsAdjustRow != null 
                && !(String.IsNullOrEmpty(fsAdjustRow.AdjdOrderType) 
                        || String.IsNullOrEmpty(fsAdjustRow.AdjdAppRefNbr)))
            {
                AppointmentEntry graphAppointmentEntry = PXGraph.CreateInstance<AppointmentEntry>();
                graphAppointmentEntry.AppointmentRecords.Current = graphAppointmentEntry.AppointmentRecords.Search<FSAppointment.refNbr>(fsAdjustRow.AdjdAppRefNbr, fsAdjustRow.AdjdOrderType);

                if (graphAppointmentEntry.AppointmentRecords.Current != null)
                {
                    throw new PXRedirectRequiredException(graphAppointmentEntry, true, "View Appointment Source") { Mode = PXBaseRedirectException.WindowMode.NewWindow };
                }
            }
            return adapter.Get();
        }
        #endregion

        #region ARPaymentEventHandlers
        protected void ARPayment_RowSelected(PXCache cache, PXRowSelectedEventArgs e)
        {
            if (e.Row == null)
            {
                return;
            }

            ARPayment arPaymentRow = (ARPayment)e.Row;
            PXUIFieldAttribute.SetVisible<FSxARPayment.serviceContractID>(cache, arPaymentRow, PXAccess.FeatureInstalled<FeaturesSet.equipmentManagementModule>() == true && arPaymentRow.DocType == ARPaymentType.Prepayment);

            bool atleastoneFSAdjust = FSAdjustments.SelectWindowed(0, 1).Count > 0;

            FSAdjustments.Cache.AllowInsert = !(atleastoneFSAdjust);
            FSAdjustments.Cache.AllowDelete = false;
            FSAdjustments.AllowSelect = arPaymentRow.CreatedByScreenID == ID.ScreenID.APPOINTMENT
                                            || arPaymentRow.CreatedByScreenID == ID.ScreenID.SERVICE_ORDER
                                                || atleastoneFSAdjust;
        }
        #endregion

        #region FSAdjustEventHandlers
        protected virtual void FSAdjust_AdjdOrderNbr_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
        {
            try
            {
                FSAdjust fsAdjustRow = (FSAdjust)e.Row;

                foreach (PXResult<FSServiceOrder, CurrencyInfo> result in PXSelectJoin<FSServiceOrder,
                    InnerJoin<CurrencyInfo, On<CurrencyInfo.curyInfoID, Equal<FSServiceOrder.curyInfoID>>>,
                    Where<FSServiceOrder.srvOrdType, Equal<Required<FSServiceOrder.srvOrdType>>, 
                    And<FSServiceOrder.refNbr, Equal<Required<FSServiceOrder.refNbr>>>>>.Select(Base, fsAdjustRow.AdjdOrderType, fsAdjustRow.AdjdOrderNbr))
                {
                    FSAdjust_AdjdOrderNbr_FieldUpdated<FSServiceOrder>(result, fsAdjustRow);
                    return;
                }
            }
            catch (PXSetPropertyException ex)
            {
                throw new PXException(ex.Message);
            }
        }

        protected virtual void FSAdjust_RowSelecting(PXCache cache, PXRowSelectingEventArgs e)
        {
            if (e.Row == null)
            {
                return;
            }

            FSAdjust fsAdjustRow = (FSAdjust)e.Row;

            using (new PXConnectionScope())
            {
                fsAdjustRow.SOCuryCompletedBillableTotal = ServiceOrderCore.GetServiceOrderBillableTotal(cache.Graph, fsAdjustRow.AdjdOrderType, fsAdjustRow.AdjdOrderNbr);
            }
        }

        protected virtual void FSAdjust_RowPersisting(PXCache cache, PXRowPersistingEventArgs e)
        {
            //If there is not header
            if (Base.Document.Current == null || e.Row == null)
            {
                return;
            }

            FSAdjust fsAdjustRow = (FSAdjust)e.Row;

            FSServiceOrder fsServiceOrderRow = PXSelect<FSServiceOrder,
                                               Where<
                                                   FSServiceOrder.srvOrdType, Equal<Required<FSServiceOrder.srvOrdType>>,
                                                   And<FSServiceOrder.refNbr, Equal<Required<FSServiceOrder.refNbr>>>>>
                                              .SelectWindowed(Base, 0, 1, fsAdjustRow.AdjdOrderType, fsAdjustRow.AdjdOrderNbr);

            if (fsServiceOrderRow != null && string.Equals(fsServiceOrderRow.CuryID, Base.Document.Current.CuryID) == false)
            { 
                cache.RaiseExceptionHandling<FSAdjust.adjdOrderNbr>(fsAdjustRow, fsAdjustRow.AdjdOrderNbr, new PXSetPropertyException("DIFERENTE CURY ID", PXErrorLevel.Error));
            }
        }

        protected void FSAdjust_AdjdOrderNbr_FieldUpdated<T>(PXResult<T, CurrencyInfo> res, FSAdjust adj)
            where T : FSServiceOrder, new()
        {
            T fsServiceOrderRow = PXCache<T>.CreateCopy((T)res);

            adj.CustomerID = Base.Document.Current.CustomerID;
            adj.AdjgDocDate = Base.Document.Current.AdjDate;
            adj.AdjgCuryInfoID = Base.Document.Current.CuryInfoID;
            adj.AdjdCuryInfoID = fsServiceOrderRow.CuryInfoID;
            adj.AdjdOrigCuryInfoID = fsServiceOrderRow.CuryInfoID;
            adj.AdjdOrderDate = fsServiceOrderRow.OrderDate > Base.Document.Current.AdjDate
                ? Base.Document.Current.AdjDate
                : fsServiceOrderRow.OrderDate;
            adj.Released = false;

            if (Base.Document.Current != null && string.IsNullOrEmpty(Base.Document.Current.DocDesc))
            {
                Base.Document.Current.DocDesc = fsServiceOrderRow.DocDesc;
            }
        }
        #endregion
    }
}