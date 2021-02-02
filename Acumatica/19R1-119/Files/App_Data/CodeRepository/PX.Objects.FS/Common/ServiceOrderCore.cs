using PX.Data;
using PX.Objects.AR;
using PX.Objects.CM.Extensions;
using PX.Objects.CR;
using PX.Objects.CS;
using PX.Objects.CT;
using PX.Objects.Extensions.MultiCurrency;
using PX.Objects.IN;
using PX.Objects.PM;
using PX.Objects.SO;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace PX.Objects.FS
{
    public static class ServiceOrderCore
    {
        public class AttendeeInfo
        {
            public string CustomerContactName = null;
            public string EMail = null;
            public string Phone1 = null;
        }

        [Serializable]
        public class RelatedServiceOrder : FSServiceOrder
        {
            public new abstract class srvOrdType : PX.Data.BQL.BqlString.Field<srvOrdType> { }

            public new abstract class refNbr : PX.Data.BQL.BqlString.Field<refNbr> { }

            public new abstract class sOID : PX.Data.BQL.BqlInt.Field<sOID> { }

            #region CuryInfoID
            public new abstract class curyInfoID : PX.Data.BQL.BqlLong.Field<curyInfoID> { }
            [PXDBLong]
            public override Int64? CuryInfoID { get; set; }
            #endregion
        }

        #region Selects

        public class ServiceOrderTypeRouteRecords_View : PXSelect<FSSrvOrdType,
                                                Where<
                                                    FSSrvOrdType.behavior, Equal<FSSrvOrdType.behavior.RouteAppointment>>>
        {
            public ServiceOrderTypeRouteRecords_View(PXGraph graph) : base(graph)
            {
            }

            public ServiceOrderTypeRouteRecords_View(PXGraph graph, Delegate handler) : base(graph, handler)
            {
            }
        }

        public class ServiceOrderRecords_View : PXSelectJoin<FSServiceOrder,
                LeftJoin<Customer,
                    On<Customer.bAccountID, Equal<FSServiceOrder.customerID>>>,
                Where2<
                                                Where<
                        FSServiceOrder.srvOrdType, Equal<Optional<FSServiceOrder.srvOrdType>>>,
                    And<Where<
                        Customer.bAccountID, IsNull,
                        Or<Match<Customer, Current<AccessInfo.userName>>>>>>>
        {
            public ServiceOrderRecords_View(PXGraph graph) : base(graph)
            {
            }

            public ServiceOrderRecords_View(PXGraph graph, Delegate handler) : base(graph, handler)
            {
            }
        }

        public class CurrentServiceOrder_View : PXSelect<FSServiceOrder,
                                                    Where<
                                                        FSServiceOrder.srvOrdType, Equal<Current<FSServiceOrder.srvOrdType>>,
                                                        And<FSServiceOrder.refNbr, Equal<Current<FSServiceOrder.refNbr>>>>>
        {
            public CurrentServiceOrder_View(PXGraph graph) : base(graph)
            {
            }

            public CurrentServiceOrder_View(PXGraph graph, Delegate handler) : base(graph, handler)
            {
            }
        }
        public class ServiceOrderAppointments_View : PXSelect<FSAppointment,
                                                        Where<
                                                            FSAppointment.sOID, Equal<Current<FSServiceOrder.sOID>>>,
                                                        OrderBy<Asc<FSAppointment.refNbr>>>
        {
            public ServiceOrderAppointments_View(PXGraph graph) : base(graph)
            {
            }

            public ServiceOrderAppointments_View(PXGraph graph, Delegate handler) : base(graph, handler)
            {
            }
        }

        public class ServiceOrderEmployees_View : PXSelectJoin<FSSOEmployee,
                                                       InnerJoin<BAccount,
                                                                On<
                                                                    FSSOEmployee.employeeID, Equal<BAccount.bAccountID>>,
                                                        LeftJoin<FSSODetEmployee,
                                                                On<
                                                                    FSSODetEmployee.lineRef, Equal<FSSOEmployee.serviceLineRef>,
                                                                    And<FSSODetEmployee.sOID, Equal<FSSOEmployee.sOID>>>>>,
                                                        Where<
                                                                FSSOEmployee.sOID, Equal<Current<FSServiceOrder.sOID>>>,
                                                        OrderBy<
                                                                Asc<FSSOEmployee.lineRef>>>
        {
            public ServiceOrderEmployees_View(PXGraph graph) : base(graph)
            {
            }

            public ServiceOrderEmployees_View(PXGraph graph, Delegate handler) : base(graph, handler)
            {
            }
        }

        public class ServiceOrderEquipment_View : PXSelectJoin<FSSOResource,
                                                        LeftJoin<FSEquipment,
                                                            On<
                                                                FSEquipment.SMequipmentID, Equal<FSSOResource.SMequipmentID>>>,
                                                        Where<
                                                            FSSOResource.sOID, Equal<Current<FSServiceOrder.sOID>>>>
        {
            public ServiceOrderEquipment_View(PXGraph graph) : base(graph)
            {
            }

            public ServiceOrderEquipment_View(PXGraph graph, Delegate handler) : base(graph, handler)
            {
            }
        }

        public class ServiceOrderAttendees_View : PXSelect<FSSOAttendee,
                                                        Where<
                                                            FSSOAttendee.sOID, Equal<Current<FSServiceOrder.sOID>>>>
        {
            public ServiceOrderAttendees_View(PXGraph graph) : base(graph)
            {
            }

            public ServiceOrderAttendees_View(PXGraph graph, Delegate handler) : base(graph, handler)
            {
            }
        }

        public class RelatedServiceOrders_View : PXSelectReadonly<RelatedServiceOrder,
                                                    Where<
                                                        RelatedServiceOrder.sourceDocType, Equal<Current<FSServiceOrder.srvOrdType>>,
                                                        And<RelatedServiceOrder.sourceRefNbr, Equal<Current<FSServiceOrder.refNbr>>>>>
        {
            public RelatedServiceOrders_View(PXGraph graph) : base(graph)
            {
            }

            public RelatedServiceOrders_View(PXGraph graph, Delegate handler) : base(graph, handler)
            {
            }
        }

        public class FSContact_View : PXSelect<FSContact,
                                            Where<FSContact.contactID, Equal<Current<FSServiceOrder.serviceOrderContactID>>>>
        {
            public FSContact_View(PXGraph graph) : base(graph)
            {
            }

            public FSContact_View(PXGraph graph, Delegate handler) : base(graph, handler)
            {
            }
        }

        public class FSAddress_View : PXSelect<FSAddress, 
                                            Where<FSAddress.addressID, Equal<Current<FSServiceOrder.serviceOrderAddressID>>>>
        {
            public FSAddress_View(PXGraph graph) : base(graph)
            {
            }

            public FSAddress_View(PXGraph graph, Delegate handler) : base(graph, handler)
            {
            }
        }

        #endregion

        #region Action Handlers

        /// <summary>
        /// Closes all appointments belonging to <c>fsServiceOrderRow</c>, in case an error occurs with any appointment,
        /// the service order will not be closed and a message will be displayed alerting the user about the appointment's issue.
        /// The row of the appointment having problems is marked with its error.
        /// </summary>
        public static bool CloseAppointmentsInServiceOrder(ServiceOrderEntry graph, FSServiceOrder fsServiceOrderRow, WebDialogResult wdr)
        {
            bool closingActionStatus = true;

            PXResultset<FSAppointment> bqlResultSet =
                                        PXSelect<
                                            FSAppointment,
                                        Where<
                                            FSAppointment.sOID, Equal<Required<FSServiceOrder.sOID>>,
                                        And<
                                            FSAppointment.status, NotEqual<FSAppointment.status.Closed>,
                                        And<
                                            FSAppointment.status, NotEqual<FSAppointment.status.Canceled>>>>>
                                        .Select(graph, fsServiceOrderRow.SOID);

            if (bqlResultSet.Count > 0)
            {
                if (wdr != WebDialogResult.No)
                {
                    Dictionary<FSAppointment, string> appWithErrors = SharedFunctions.CloseAppointments(bqlResultSet);

                    if (appWithErrors.Count > 0)
                    {
                        foreach (KeyValuePair<FSAppointment, string> kvp in appWithErrors)
                        {
                            graph.ServiceOrderAppointments.Cache.RaiseExceptionHandling<FSAppointment.refNbr>(
                                                                                        kvp.Key,
                                                                                        kvp.Key.RefNbr,
                                                                                        new PXSetPropertyException(kvp.Value, PXErrorLevel.RowError));
                        }

                        throw new PXException(PXMessages.LocalizeFormatNoPrefix(TX.Error.SERVICE_ORDER_CANT_BE_CLOSED_APPOINTMENTS_HAVE_ISSUES));
                    }
                }
                else
                {
                    closingActionStatus = false;
                }
            }

            return closingActionStatus;
        }

        /// <summary>
        /// Completes all appointments belonging to <c>fsServiceOrderRow</c>, in case an error occurs with any appointment,
        /// the service order will not be completed and a message will be displayed alerting the user about the appointment's issue.
        /// The row of the appointment having problems is marked with its error.
        /// </summary>
        public static void CompleteAppointmentsInServiceOrder(ServiceOrderEntry graph, FSServiceOrder fsServiceOrderRow)
        {
            PXResultset<FSAppointmentInRoute> bqlResultSet = PXSelect<
                                                            FSAppointmentInRoute,
                                                        Where<
                                                            FSAppointmentInRoute.sOID, Equal<Required<FSAppointmentInRoute.sOID>>,
                                                        And<
                                                            FSAppointmentInRoute.status, NotEqual<FSAppointmentInRoute.status.Closed>,
                                                        And<
                                                            FSAppointmentInRoute.status, NotEqual<FSAppointmentInRoute.status.Canceled>,
                                                        And<
                                                            FSAppointmentInRoute.status, NotEqual<FSAppointmentInRoute.status.Completed>>>>>>
                                                        .Select(graph, fsServiceOrderRow.SOID);

            if (bqlResultSet.Count > 0)
            {
                Dictionary<FSAppointment, string> appWithErrors = SharedFunctions.CompleteAppointments(graph, bqlResultSet);

                if (appWithErrors.Count > 0)
                {
                    foreach (KeyValuePair<FSAppointment, string> kvp in appWithErrors)
                    {
                        graph.ServiceOrderAppointments.Cache.RaiseExceptionHandling<FSAppointment.refNbr>(
                                                                                    kvp.Key,
                                                                                    kvp.Key.RefNbr,
                                                                                    new PXSetPropertyException(kvp.Value, PXErrorLevel.RowError));
                    }

                    throw new PXException(PXMessages.LocalizeFormatNoPrefix(TX.Error.SERVICE_ORDER_CANT_BE_COMPLETED_APPOINTMENTS_HAVE_ISSUES));
                }
            }
        }

        /// <summary>
        /// Cancels all appointments belonging to <c>fsServiceOrderRow</c>, in case an error occurs with any appointment,
        /// the service order will not be canceled and a message will be displayed alerting the user about the appointment's issue.
        /// The row of the appointment having problems is marked with its error.
        /// </summary>
        public static void CancelAppointmentsInServiceOrder(ServiceOrderEntry graph, FSServiceOrder fsServiceOrderRow)
        {
            PXResultset<FSAppointment> bqlResultSet =
                                        PXSelect<
                                            FSAppointment,
                                        Where<
                                            FSAppointment.sOID, Equal<Required<FSServiceOrder.sOID>>,
                                            And<FSAppointment.status, NotEqual<FSAppointment.status.Canceled>>>>
                                        .Select(graph, fsServiceOrderRow.SOID);

            if (bqlResultSet.Count > 0)
            {
                Dictionary<FSAppointment, string> appWithErrors = SharedFunctions.CancelAppointments(graph, bqlResultSet);

                if (appWithErrors.Count > 0)
                {
                    foreach (KeyValuePair<FSAppointment, string> kvp in appWithErrors)
                    {
                        graph.ServiceOrderAppointments.Cache.RaiseExceptionHandling<FSAppointment.refNbr>(
                                                                                    kvp.Key,
                                                                                    kvp.Key.RefNbr,
                                                                                    new PXSetPropertyException(kvp.Value, PXErrorLevel.RowError));
                    }

                    throw new PXException(PXMessages.LocalizeFormatNoPrefix(TX.Error.SERVICE_ORDER_CANT_BE_CANCELED_APPOINTMENTS_HAVE_ISSUES));
                }
            }
        }

        public static void ShowAsk(ServiceOrderEntry graph, PXView view, string newStatus, out WebDialogResult wdr)
        {
            wdr = WebDialogResult.None;

            switch (newStatus)
            {
                case ID.Status_ServiceOrder.COMPLETED:
                    break;
                case ID.Status_ServiceOrder.CLOSED:

                    bool displayAlert = graph.SetupRecord.Current.AlertBeforeCloseServiceOrder == true
                                            && graph.ServiceOrderRecords.Current.IsCalledFromQuickProcess != true;

                    if (displayAlert == true 
                        && graph.Accessinfo.ScreenID == SharedFunctions.SetScreenIDToDotFormat(ID.ScreenID.SERVICE_ORDER)
                            && graph.ServiceOrderRecords.Current.AppointmentsCompletedCntr > 0)
                    {
                        wdr = view.Ask(TX.WebDialogTitles.CONFIRM_SERVICE_ORDER_CLOSING,
                                       TX.Messages.ASK_CONFIRM_SERVICE_ORDER_CLOSING,
                                       MessageButtons.YesNo);
                    }
                    
                    break;
                case ID.Status_ServiceOrder.CANCELED:
                    break;
            }
        }

        public static void ChangeStatus_Handler(
                                                ServiceOrderEntry graph,
                                                FSServiceOrder fsServiceOrderRow,
                                                FSBillingCycle fsBillingCycleRow,
                                                string newStatus)
        {
            if (graph.ServiceOrderRecords.Cache.GetStatus(fsServiceOrderRow) != PXEntryStatus.Inserted)
            {
                FSSrvOrdType fsSrvOrdTypeRow = PXSelect<FSSrvOrdType,
                               Where<
                                     FSSrvOrdType.srvOrdType, Equal<Required<FSSrvOrdType.srvOrdType>>>>
                               .Select(graph, fsServiceOrderRow.SrvOrdType);

                if (ServiceOrderCore.CheckServiceOrderStatusTransition(fsSrvOrdTypeRow.Behavior, fsServiceOrderRow, (bool)fsServiceOrderRow.Hold, newStatus) == false)
                {
                    throw new PXException(TX.Error.INVALID_SO_STATUS_TRANSITION);
                }

                WebDialogResult wdr;
                ShowAsk(graph, graph.ServiceOrderRecords.View, newStatus, out wdr);

                if (graph.ServiceOrderRecords.Cache.AllowUpdate == true)
                {
                    graph.SkipTaxCalcAndSave();
                }

                bool changeServiceOrderStatus = true;

                switch (newStatus)
                {
                    case ID.Status_ServiceOrder.COMPLETED:
                        CompleteAppointmentsInServiceOrder(graph, fsServiceOrderRow);
                        break;
                    case ID.Status_ServiceOrder.CLOSED:
                        changeServiceOrderStatus = CloseAppointmentsInServiceOrder(graph, fsServiceOrderRow, wdr);
                        break;
                    case ID.Status_ServiceOrder.CANCELED:
                        CancelAppointmentsInServiceOrder(graph, fsServiceOrderRow);
                        break;
                }

                if (changeServiceOrderStatus)
                {
                    graph.SelectTimeStamp();
                    graph.ServiceOrderRecords.Cache.AllowUpdate = true;
                    fsServiceOrderRow.Status = newStatus;

                    if (newStatus == ID.Status_ServiceOrder.OPEN
                            && fsServiceOrderRow.Mem_Invoiced == false
                                && fsServiceOrderRow.AllowInvoice == true)
                    {
                        graph.ServiceOrderRecords.Cache.SetValueExt<FSServiceOrder.allowInvoice>(fsServiceOrderRow, false);
                    }

                    if (newStatus == ID.Status_ServiceOrder.CLOSED
                            && fsServiceOrderRow.Mem_Invoiced == false
                                && fsServiceOrderRow.AllowInvoice == false
                                    && fsBillingCycleRow != null
                                        && fsBillingCycleRow.BillingBy == ID.Billing_By.SERVICE_ORDER)
                    {
                        fsServiceOrderRow.AllowInvoice = true;
                    }

                    graph.ServiceOrderRecords.Update(graph.ServiceOrderRecords.Current);
                    graph.SkipTaxCalcAndSave();
                }
            }

            graph.RecalculateExternalTaxes();
        }

        public static void OpenEmployeeBoard_Handler(
                                            PXGraph graph,
                                            ServiceOrderRecords_View serviceOrderRecords)
        {
            if (serviceOrderRecords.Current.Status != ID.Status_ServiceOrder.OPEN)
            {
                throw new PXException(TX.Error.INVALID_ACTION_FOR_CURRENT_SERVICE_ORDER_STATUS);
            }

            graph.GetSaveAction().Press();

            PXResultset<FSSODetService> bqlResultSet_SODetService = new PXResultset<FSSODetService>();
            PXResultset<FSSODetPart> bqlResultSet_SODetPart = new PXResultset<FSSODetPart>();

            ServiceOrderCore.GetPendingLines(graph, (int)serviceOrderRecords.Current.SOID, ref bqlResultSet_SODetService, ref bqlResultSet_SODetPart);

            if (bqlResultSet_SODetService.Count > 0 || bqlResultSet_SODetPart.Count > 0)
            {
                throw new PXRedirectToBoardRequiredException(
                    Paths.ScreenPaths.MULTI_EMPLOYEE_DISPATCH,
                    ServiceOrderCore.GetServiceOrderUrlArguments(serviceOrderRecords.Current));
            }
            else
            {
                throw new PXException(TX.Error.CURRENT_DOCUMENT_NOT_SERVICES_TO_SCHEDULE);
            }
        }
        #endregion

        #region Event Handlers

        public static void FSServiceOrder_BranchLocationID_FieldUpdated_Handler(
                                                                                PXGraph graph,
                                                                                PXFieldUpdatedEventArgs e,
                                                                                FSSrvOrdType fsSrvOrdTypeRow,
                                                                                PXSelectBase<FSServiceOrder> serviceOrderRelated)
        {
            if (e.Row == null)
            {
                return;
            }

            FSServiceOrder fsServiceOrderRow = (FSServiceOrder)e.Row;
        }

        public static void FSServiceOrder_LocationID_FieldUpdated_Handler(
                                                                            PXCache cache,
                                                                            PXFieldUpdatedEventArgs e,
                                                                            FSSrvOrdType fsSrvOrdTypeRow)
        {
            if (e.Row == null)
            {
                return;
            }

            FSServiceOrder fsServiceOrderRow = (FSServiceOrder)e.Row;
            SetBillCustomerAndLocationID(cache, fsServiceOrderRow);
        }

        public static void FSServiceOrder_ContactID_FieldUpdated_Handler(PXGraph graph, PXFieldUpdatedEventArgs e, FSSrvOrdType fsSrvOrdTypeRow)
        {
            if (e.Row == null)
            {
                return;
            }

            FSServiceOrder fsServiceOrderRow = (FSServiceOrder)e.Row;
        }

        public static void FSServiceOrder_BillCustomerID_FieldUpdated_Handler(PXCache cache, PXFieldUpdatedEventArgs e)
        {
            if (e.Row == null)
            {
                return;
            }

            FSServiceOrder fsServiceOrderRow = (FSServiceOrder)e.Row;

            cache.SetValueExt<FSServiceOrder.billLocationID>(e.Row, GetDefaultLocationID(cache.Graph, fsServiceOrderRow.BillCustomerID));
            cache.SetValueExt<FSServiceOrder.cBID>(fsServiceOrderRow, GetCBIDFromCustomer(cache.Graph, fsServiceOrderRow.BillCustomerID, fsServiceOrderRow.SrvOrdType));
        }

        public static void FSServiceOrder_RowPersisting_Handler(
                                                                ServiceOrderEntry graphServiceOrderEntry,
                                                                PXCache cacheServiceOrder,
                                                                PXRowPersistingEventArgs e,
                                                                FSSrvOrdType fsSrvOrdTypeRow,
                                                                PXSelectBase<FSSODetService> serviceOrderServiceDetail,
                                                                PXSelectBase<FSSODetPart> serviceOrderPartDetail,
                                                                ServiceOrderAppointments_View serviceOrderAppointments,
                                                                PXCache cacheFSSOAttendee,
                                                                PXCache cacheFSAppointmentAttendee,
                                                                ref bool replicateAttendees,
                                                                int totalAttendees,
                                                                AppointmentEntry graphAppointmentEntryCaller,
                                                                bool forceAppointmentCheckings)
        {
            if (e.Row == null)
            {
                return;
            }

            FSServiceOrder fsServiceOrderRow = (FSServiceOrder)e.Row;

            FSAppointment fsAppointmentRowBeingSaved = null;
            if (graphAppointmentEntryCaller != null)
            {
                fsAppointmentRowBeingSaved = graphAppointmentEntryCaller.AppointmentRecords.Current;
            }

            if (e.Operation == PXDBOperation.Insert || e.Operation == PXDBOperation.Update)
            {
                if (fsSrvOrdTypeRow == null)
                {
                    throw new PXException(TX.Error.SERVICE_ORDER_TYPE_NOT_FOUND, fsServiceOrderRow.SrvOrdType);
                }

                if (string.IsNullOrWhiteSpace(fsServiceOrderRow.DocDesc))
                {
                    SetDocDesc(graphServiceOrderEntry, fsServiceOrderRow);
                }

                if (AreAttendeesModified(cacheFSSOAttendee, cacheFSAppointmentAttendee)) 
                {
                    replicateAttendees = true;
                }
                else
                {
                    replicateAttendees = false;
                }

                if (fsServiceOrderRow.ProjectID != (int?)cacheServiceOrder.GetValueOriginal<FSServiceOrder.projectID>(fsServiceOrderRow)
                    || fsServiceOrderRow.BranchID != (int?)cacheServiceOrder.GetValueOriginal<FSServiceOrder.branchID>(fsServiceOrderRow))
                {
                    if (serviceOrderAppointments != null)
                    {
                        AppointmentEntry graphAppointmentEntry = PXGraph.CreateInstance<AppointmentEntry>();

                        foreach (FSAppointment fsAppointmentRow in serviceOrderAppointments.Select())
                        {
                            if (fsAppointmentRowBeingSaved == null || fsAppointmentRowBeingSaved.AppointmentID != fsAppointmentRow.AppointmentID)
                            {
                                FSAppointment fsAppointmentRow_local  = graphAppointmentEntry.AppointmentRecords.Current = graphAppointmentEntry.AppointmentRecords.Search<FSAppointment.refNbr>(fsAppointmentRow.RefNbr, fsAppointmentRow.SrvOrdType);
                                fsAppointmentRow_local.BranchID = fsServiceOrderRow.BranchID;
                                graphAppointmentEntry.AppointmentRecords.Update(fsAppointmentRow_local);

                                graphAppointmentEntry.UpdateDetailsFromProjectID(fsServiceOrderRow);
                                graphAppointmentEntry.UpdateDetailsFromBranchID(fsServiceOrderRow);

                                try
                                {
                                    graphAppointmentEntry.SkipServiceOrderUpdate = true;
                                    graphAppointmentEntry.Save.Press();
                                }
                                finally
                                {
                                    graphAppointmentEntry.SkipServiceOrderUpdate = false;
                                }
                            }
                        }

                        graphServiceOrderEntry.SelectTimeStamp();
                    }
                }

                if (fsSrvOrdTypeRow.RequireContact == true && fsServiceOrderRow.ContactID == null)
                {
                    throw new PXException(TX.Error.REQUIRED_CONTACT_MISSING);
                }

                UpdateServiceCounts(fsServiceOrderRow, serviceOrderServiceDetail, graphServiceOrderEntry, graphAppointmentEntryCaller, forceAppointmentCheckings);
                UpdatePendingPostFlags(fsServiceOrderRow, serviceOrderServiceDetail, serviceOrderPartDetail);

                if(fsServiceOrderRow.Quote != true)
                { 
                    UpdateWatingForPartsFlag(fsServiceOrderRow, serviceOrderServiceDetail, serviceOrderPartDetail);
                    UpdateAppointmentsNeededFlag(graphServiceOrderEntry, fsServiceOrderRow, serviceOrderServiceDetail, serviceOrderPartDetail, graphAppointmentEntryCaller);
                }

                bool updateCBID = false;
                bool updateCutOffDate = false;

                if (e.Operation == PXDBOperation.Insert)
                {
                    updateCBID = true;
                    updateCutOffDate = true;

                    SharedFunctions.CopyNotesAndFiles(cacheServiceOrder,
                                                      fsSrvOrdTypeRow,
                                                      fsServiceOrderRow,
                                                      fsServiceOrderRow.CustomerID,
                                                      fsServiceOrderRow.LocationID);
                }
                else if (e.Operation == PXDBOperation.Update)
                {
                    if ((int?)cacheServiceOrder.GetValueOriginal<FSServiceOrder.billCustomerID>(fsServiceOrderRow) != fsServiceOrderRow.BillCustomerID)
                    {
                        updateCBID = true;
                    }

                    if ((DateTime?)cacheServiceOrder.GetValueOriginal<FSServiceOrder.orderDate>(fsServiceOrderRow) != fsServiceOrderRow.OrderDate)
                    {
                        updateCutOffDate = true;
                    }
                }

                if (updateCBID)
                {
                    fsServiceOrderRow.CBID = GetCBIDFromCustomer(graphServiceOrderEntry, fsServiceOrderRow.BillCustomerID, fsServiceOrderRow.SrvOrdType);
                    updateCutOffDate = true;
                }

                if (updateCutOffDate)
                {
                    fsServiceOrderRow.CutOffDate = GetCutOffDate(graphServiceOrderEntry, fsServiceOrderRow.CBID, fsServiceOrderRow.OrderDate);
                }
            }
            else
            {
                if (CanDeleteServiceOrder(graphServiceOrderEntry, fsServiceOrderRow) == false)
                {
                    throw new PXException(TX.Error.SERVICE_ORDER_CANNOT_BE_DELETED_BECAUSE_OF_ITS_STATUS);
                }
            }

            fsServiceOrderRow.TotalAttendees = totalAttendees;
        }

        public static void FSServiceOrder_RowPersisted_PartialHandler(
                                                                    PXGraph graph,
                                                                    PXCache cache,
                                                                    PXRowPersistedEventArgs e,
                                                                    PXResultset<FSSOAttendee> bqlResultSet_FSSOAttendee,
                                                                    int? currentAppointmentID,
                                                                    PXResultset<FSAppointmentAttendee> bqlResultSet_FSAppointmentAttendee,
                                                                    ref bool replicateAttendees,
                                                                    FSSelectorHelper helper)
        {
            if (e.Row == null)
            {
                return;
            }

            FSServiceOrder fsServiceOrderRow = (FSServiceOrder)e.Row;

            if (replicateAttendees == true && e.TranStatus == PXTranStatus.Open
                        && (e.Operation == PXDBOperation.Insert || e.Operation == PXDBOperation.Update))
            {
                replicateAttendees = false;

                //Replicate attendees from current Appointment to its ServiceOrder
                if (bqlResultSet_FSSOAttendee == null)
                {
                    ReplicateAttendeesToServiceOrder(graph, fsServiceOrderRow.SOID, bqlResultSet_FSAppointmentAttendee, helper);
                    bqlResultSet_FSSOAttendee = FSSOAttendee.GetStaticSOAttendeeRows(graph, fsServiceOrderRow.SOID);
                }

                //Replicate attendees from ServiceOrder to its Appointments
                foreach (FSAppointment fsAppointmentRow in GetEditableAppointments(graph, fsServiceOrderRow.SOID, currentAppointmentID))
                {
                    ReplicateAttendeesToAppointment(graph, fsAppointmentRow.AppointmentID, bqlResultSet_FSSOAttendee, helper);
                }
            }
        }

        public static void FSServiceOrder_CustomerID_FieldUpdated_Handler(
                                                                        PXCache cacheServiceOrder,
                                                                        PXFieldUpdatedEventArgs e,
                                                                        FSSrvOrdType fsSrvOrdTypeRow,
                                                                        PXSelectBase<FSSODetService> serviceOrderDetServices,
                                                                        PXSelectBase<FSSODetPart> serviceOrderDetParts,
                                                                        AppointmentCore.AppointmentDetServices_View appointmentDetServices,
                                                                        AppointmentCore.AppointmentDetParts_View appointmentDetParts,
                                                                        AppointmentCore.AppointmentPickupDeliveryItems_View appointmentDetPickupDeliveries,
                                                                        PXResultset<FSAppointment> bqlResultSet_Appointment,
                                                                        DateTime? itemDateTime,
                                                                        bool allowCustomerChange,
                                                                        Customer customerRow)
        {
            if (e.Row == null)
            {
                return;
            }

            FSServiceOrder fsServiceOrderRow = (FSServiceOrder)e.Row;

            if (allowCustomerChange == false && CheckCustomerChange(cacheServiceOrder, e, bqlResultSet_Appointment) == false)
            {
                return;
            }

            fsServiceOrderRow.ContactID = null;

            cacheServiceOrder.SetValueExt<FSServiceOrder.locationID>(fsServiceOrderRow, GetDefaultLocationID(cacheServiceOrder.Graph, fsServiceOrderRow.CustomerID));

            SetBillCustomerAndLocationID(cacheServiceOrder, fsServiceOrderRow);

            if (serviceOrderDetServices != null)
            {
                ServiceOrderCore.RefreshSalesPricesInTheWholeDocument(serviceOrderDetServices, serviceOrderDetParts);
            }
            else if (appointmentDetServices != null)
            {
                AppointmentCore.RefreshSalesPricesInTheWholeDocument(appointmentDetServices, appointmentDetParts, appointmentDetPickupDeliveries);
            }

            // clear the ProjectID if it's not the default
            if (fsServiceOrderRow.ProjectID != 0)
            {
                fsServiceOrderRow.ProjectID = null;
            }
        }

        public static void FSServiceOrder_RowSelected_PartialHandler(
                                                                    PXGraph graph,
                                                                    PXCache cacheServiceOrder,
                                                                    FSServiceOrder fsServiceOrderRow,
                                                                    FSAppointment fsAppointmentRow,
                                                                    FSSrvOrdType fsSrvOrdTypeRow,
                                                                    FSBillingCycle fsBillingCycleRow,
                                                                    Contract contractRow,
                                                                    int appointmentCount,
                                                                    int serviceCount,
                                                                    int partCount,
                                                                    PXCache cacheServiceOrderDetailServices,
                                                                    PXCache cacheServiceOrderDetailParts,
                                                                    PXCache cacheServiceOrderAppointments,
                                                                    PXCache cacheServiceOrderAttendees,
                                                                    PXCache cacheServiceOrderEquipment,
                                                                    PXCache cacheServiceOrderEmployees,
                                                                    PXCache cacheServiceOrder_Contact,
                                                                    PXCache cacheServiceOrder_Address,
                                                                    bool? isBeingCalledFromQuickProcess,
                                                                    bool allowCustomerChange = false)
        {
            if (cacheServiceOrder.GetStatus(fsServiceOrderRow) == PXEntryStatus.Inserted)
            {
                if (fsSrvOrdTypeRow == null)
                {
                    throw new PXException(TX.Error.SERVICE_ORDER_TYPE_NOT_FOUND, fsServiceOrderRow.SrvOrdType);
                }

                SetServiceOrderTypeValues(fsServiceOrderRow, fsSrvOrdTypeRow);
            }

            EnableDisable_Document(
                                graph,
                                cacheServiceOrder,
                                fsServiceOrderRow,
                                fsAppointmentRow,
                                fsSrvOrdTypeRow,
                                fsBillingCycleRow,
                                appointmentCount,
                                serviceCount,
                                partCount,
                                cacheServiceOrderDetailServices,
                                cacheServiceOrderDetailParts,
                                cacheServiceOrderAppointments,
                                cacheServiceOrderAttendees,
                                cacheServiceOrderEquipment,
                                cacheServiceOrderEmployees,
                                cacheServiceOrder_Contact,
                                cacheServiceOrder_Address,
                                isBeingCalledFromQuickProcess,
                                allowCustomerChange);

            CheckIfCustomerBelongsToProject(graph, cacheServiceOrder, fsServiceOrderRow, contractRow);
        }

        private static void CheckIfCustomerBelongsToProject(PXGraph graph, PXCache cache, FSServiceOrder fsServiceOrderRow, Contract ContractRow)
        {
            if (fsServiceOrderRow == null)
            {
                return;
            }

            int? customerID = ContractRow?.CustomerID;

            Exception customerException = null;

            if (customerID != null
                    && fsServiceOrderRow.CustomerID != null
                        && customerID != fsServiceOrderRow.CustomerID)
            {
                customerException = new PXSetPropertyException(TX.Warning.CUSTOMER_DOES_NOT_MATCH_PROJECT, PXErrorLevel.Warning);
            }

            cache.RaiseExceptionHandling<FSServiceOrder.projectID>(
                                                                   fsServiceOrderRow,
                                                                   fsServiceOrderRow.ProjectID,
                                                                   customerException);
        }

        public static void RefreshSalesPricesInTheWholeDocument(
                                PXSelectBase<FSSODetService> serviceOrderDetServices,
                                PXSelectBase<FSSODetPart> serviceOrderDetParts)
        {
            // TODO:
            // This method should run and depend on BillCustomerID changes, and not on CustomerID changes
            // Besides that, check if this is necessary using the Sales-Price graph extension

            foreach (FSSODetService row in serviceOrderDetServices.Select())
            {
                serviceOrderDetServices.Cache.SetDefaultExt<FSSODetService.curyUnitPrice>(row);
                serviceOrderDetServices.Cache.Update(row);
            }

            foreach (FSSODetPart row in serviceOrderDetParts.Select())
            {
                serviceOrderDetParts.Cache.SetDefaultExt<FSSODetPart.curyUnitPrice>(row);
                serviceOrderDetParts.Cache.Update(row);
            }
        }

        public static void FSServiceOrder_ProjectID_FieldUpdated_PartialHandler(
                                                                                FSServiceOrder fsServiceOrderRow,
                                                                                PXSelectBase<FSSODetService> serviceOrderDetServices,
                                                                                PXSelectBase<FSSODetPart> serviceOrderDetParts)
        {
            if (fsServiceOrderRow.ProjectID == null)
            {
                return;
            }

            if (serviceOrderDetServices != null && serviceOrderDetParts != null)
            {
                foreach (FSSODetService fsSODetServiceRow in serviceOrderDetServices.Select())
                {
                    fsSODetServiceRow.ProjectID = fsServiceOrderRow.ProjectID;
                    fsSODetServiceRow.ProjectTaskID = null;
                    serviceOrderDetServices.Update(fsSODetServiceRow);
                }

                foreach (FSSODetPart fsSODetPartRow in serviceOrderDetParts.Select())
                {
                    fsSODetPartRow.ProjectID = fsServiceOrderRow.ProjectID;
                    fsSODetPartRow.ProjectTaskID = null;
                    serviceOrderDetParts.Update(fsSODetPartRow);
                }
            }
        }

        public static void FSServiceOrder_BranchID_FieldUpdated_PartialHandler(
                                                                                FSServiceOrder fsServiceOrderRow,
                                                                                PXSelectBase<FSSODetService> serviceOrderDetServices,
                                                                                PXSelectBase<FSSODetPart> serviceOrderDetParts)
        {
            if (fsServiceOrderRow.BranchID == null)
            {
                return;
            }

            if (serviceOrderDetServices != null && serviceOrderDetParts != null)
            {
                foreach (FSSODetService fsSODetServiceRow in serviceOrderDetServices.Select())
                {
                    fsSODetServiceRow.BranchID = fsServiceOrderRow.BranchID;
                    serviceOrderDetServices.Update(fsSODetServiceRow);
                }

                foreach (FSSODetPart fsSODetPartRow in serviceOrderDetParts.Select())
                {
                    fsSODetPartRow.BranchID = fsServiceOrderRow.BranchID;
                    serviceOrderDetParts.Update(fsSODetPartRow);
                }
            }
        }

        #endregion

        #region Methods

        /// <summary>
        /// Returns the url arguments for a Service Order [fsServiceOrderRow].
        /// </summary>
        public static KeyValuePair<string, string>[] GetServiceOrderUrlArguments(FSServiceOrder fsServiceOrderRow)
        {
            KeyValuePair<string, string>[] urlArgs = new KeyValuePair<string, string>[]
            {
                new KeyValuePair<string, string>(typeof(FSServiceOrder.refNbr).Name, fsServiceOrderRow.RefNbr),
                new KeyValuePair<string, string>("Date", fsServiceOrderRow.OrderDate.Value.ToString())
            };

            return urlArgs;
        }

        /// <summary>
        /// Returns true if a Service Order [fsServiceOrderRow] can change its status to [newServiceOrderStatus] based in the current status of the Service Order [fsServiceOrderRow].
        /// </summary>
        public static bool CheckServiceOrderStatusTransition(string srvOrdTypeBehavior, FSServiceOrder fsServiceOrderRow, bool hold, string newServiceOrderStatus)
        {
            if (srvOrdTypeBehavior == ID.Behavior_SrvOrderType.QUOTE)
            {
                // Quote => On Hold
                if ((fsServiceOrderRow.Status == ID.Status_ServiceOrder.QUOTE && hold == true)
                    && newServiceOrderStatus == ID.Status_ServiceOrder.ON_HOLD)
                {
                    return true;
                }

                // On Hold => Quote
                if ((fsServiceOrderRow.Status == ID.Status_ServiceOrder.ON_HOLD && hold == false)
                    && newServiceOrderStatus == ID.Status_ServiceOrder.QUOTE)
                {
                    return true;
                }
            }
            else
            {
                // New/Null => Open
                if ((fsServiceOrderRow.Status == null || fsServiceOrderRow.Status == ID.Status_ServiceOrder.OPEN)
                    && newServiceOrderStatus == ID.Status_ServiceOrder.OPEN)
                {
                    return true;
                }

                // Open => On Hold
                if ((fsServiceOrderRow.Status == ID.Status_ServiceOrder.OPEN && hold == true)
                        && newServiceOrderStatus == ID.Status_ServiceOrder.ON_HOLD)
                {
                    return true;
                }

                // On Hold => Open
                if ((fsServiceOrderRow.Status == ID.Status_ServiceOrder.ON_HOLD && hold == false)
                    && newServiceOrderStatus == ID.Status_ServiceOrder.OPEN)
                {
                    return true;
                }

                // Flag must be false for below transitions
                if (hold == true)
                {
                    return false;
                }

                // Open => Canceled
                if (fsServiceOrderRow.Status == ID.Status_ServiceOrder.OPEN
                        && newServiceOrderStatus == ID.Status_ServiceOrder.CANCELED)
                {
                    return true;
                }

                // Canceled => Open
                if (fsServiceOrderRow.Status == ID.Status_ServiceOrder.CANCELED
                    && newServiceOrderStatus == ID.Status_ServiceOrder.OPEN)
                {
                    return true;
                }

                // Open => Completed
                if (fsServiceOrderRow.Status == ID.Status_ServiceOrder.OPEN
                    && newServiceOrderStatus == ID.Status_ServiceOrder.COMPLETED)
                {
                    return true;
                }

                // Completed => Open
                if (fsServiceOrderRow.Status == ID.Status_ServiceOrder.COMPLETED
                    && newServiceOrderStatus == ID.Status_ServiceOrder.OPEN)
                {
                    return true;
                }

                // Completed => Closed (Disable edit document)
                if (fsServiceOrderRow.Status == ID.Status_ServiceOrder.COMPLETED
                    && newServiceOrderStatus == ID.Status_ServiceOrder.CLOSED)
                {
                    return true;
                }
            }

            return false;
        }

        public static bool AreAttendeesModified(PXCache cacheFSSOAttendee, PXCache cacheFSAppointmentAttendee)
        {
            if (cacheFSSOAttendee != null && cacheFSSOAttendee.IsDirty == true)
            {
                return true;
            }

            if (cacheFSAppointmentAttendee != null && cacheFSAppointmentAttendee.IsDirty == true)
            {
                return true;
            }

            return false;
        }

        public static void SetServiceOrderRecord_AsUpdated_IfItsNotchanged(PXCache cacheServiceOrder, FSServiceOrder fsServiceOrderRow)
        {
            if (cacheServiceOrder.GetStatus(fsServiceOrderRow) == PXEntryStatus.Notchanged)
            {
                cacheServiceOrder.SetStatus(fsServiceOrderRow, PXEntryStatus.Updated);
            }
        }

        public static void DeleteServiceOrder(FSServiceOrder fsServiceOrderRow, ServiceOrderEntry graphServiceOrderEntry)
        {
            graphServiceOrderEntry.Clear();

            graphServiceOrderEntry.ServiceOrderRecords.Current = graphServiceOrderEntry.ServiceOrderRecords
                                    .Search<FSServiceOrder.refNbr>(fsServiceOrderRow.RefNbr, fsServiceOrderRow.SrvOrdType);

            graphServiceOrderEntry.Delete.Press();
        }

        private static void ReplicateAttendeesToAppointment(
                                                            PXGraph graph,
                                                            int? appointmentID,
                                                            PXResultset<FSSOAttendee> bqlResultSet_FSSOAttendee,
                                                            FSSelectorHelper helper)
        {
            int? currentHelperValue = helper.Mem_int;

            PXCache<FSAppointmentAttendee> cacheAppointmentAttendee = new PXCache<FSAppointmentAttendee>(graph);

            PXResultset<FSAppointmentAttendee> fsAppointmentAttendeeSet =
                                                    PXSelect<FSAppointmentAttendee,
                                                    Where<
                                                        FSAppointmentAttendee.appointmentID, Equal<Required<FSAppointmentAttendee.appointmentID>>>>
                                                    .Select(graph, appointmentID);

            using (PXTransactionScope ts = new PXTransactionScope())
            {
                foreach (FSAppointmentAttendee fsAppointmentAttendeeRow in fsAppointmentAttendeeSet)
                {
                    cacheAppointmentAttendee.Delete(fsAppointmentAttendeeRow);
                }

                cacheAppointmentAttendee.Persist(PXDBOperation.Delete);

                foreach (FSSOAttendee fsSOAttendeeRow in bqlResultSet_FSSOAttendee)
                {
                    FSAppointmentAttendee fsAppointmentAttendeeRow = new FSAppointmentAttendee()
                    {
                        AppointmentID = appointmentID,
                        CustomerID = fsSOAttendeeRow.CustomerID,
                        ContactID = fsSOAttendeeRow.ContactID,
                        Comment = fsSOAttendeeRow.Comment
                    };
                    helper.Mem_int = fsAppointmentAttendeeRow.CustomerID;
                    cacheAppointmentAttendee.Insert(fsAppointmentAttendeeRow);
                }

                helper.Mem_int = currentHelperValue;
                cacheAppointmentAttendee.Persist(PXDBOperation.Insert);

                ts.Complete();
            }
        }

        private static void ReplicateAttendeesToServiceOrder(
                                                            PXGraph graph,
                                                            int? sOID,
                                                            PXResultset<FSAppointmentAttendee> bqlResultSet_FSAppointmentAttendee,
                                                            FSSelectorHelper helper)
        {
            int? currentHelperValue = helper.Mem_int;

            PXCache<FSSOAttendee> cacheSOAttendee = new PXCache<FSSOAttendee>(graph);

            PXResultset<FSSOAttendee> fsSOAttendeeSet = PXSelect<FSSOAttendee,
                                                         Where<
                                                            FSSOAttendee.sOID, Equal<Required<FSSOAttendee.sOID>>>>
                                                         .Select(graph, sOID);

            using (PXTransactionScope ts = new PXTransactionScope())
            {
                foreach (FSSOAttendee fsSOAttendeeRow in fsSOAttendeeSet)
                {
                    cacheSOAttendee.Delete(fsSOAttendeeRow);
                }

                cacheSOAttendee.Persist(PXDBOperation.Delete);

                foreach (FSAppointmentAttendee fsAppointmentAttendeeRow in bqlResultSet_FSAppointmentAttendee)
                {
                    FSSOAttendee fsSOAttendeeRow = new FSSOAttendee()
                    {
                        SOID = sOID,
                        CustomerID = fsAppointmentAttendeeRow.CustomerID,
                        ContactID = fsAppointmentAttendeeRow.ContactID,
                        Comment = fsAppointmentAttendeeRow.Comment
                    };
                    helper.Mem_int = fsSOAttendeeRow.CustomerID;
                    cacheSOAttendee.Insert(fsSOAttendeeRow);
                }

                helper.Mem_int = currentHelperValue;
                cacheSOAttendee.Persist(PXDBOperation.Insert);
                ts.Complete();
            }
        }

        private static PXResultset<FSAppointment> GetEditableAppointments(PXGraph graph, int? sOID, int? appointmentID)
        {
            return PXSelect<FSAppointment,
                   Where2<
                        Where<
                            FSAppointment.status, Equal<ListField_Status_Appointment.ManualScheduled>,
                            Or<FSAppointment.status, Equal<ListField_Status_Appointment.AutomaticScheduled>>>,
                        And<FSAppointment.sOID, Equal<Required<FSAppointment.sOID>>,
                        And<FSAppointment.appointmentID, NotEqual<Required<FSAppointment.appointmentID>>>>>>
                   .Select(graph, sOID, appointmentID);
        }

        #region EnableDisable
        /// <summary>
        /// Enable / Disable the document depending of the Status of the Appointment [fsAppointmentRow] and ServiceOrder [fsServiceOrderRow].
        /// </summary>
        private static void EnableDisable_Document(
                                                PXGraph graph,
                                                PXCache cacheServiceOrder,
                                                FSServiceOrder fsServiceOrderRow,
                                                FSAppointment fsAppointmentRow,
                                                FSSrvOrdType fsSrvOrdTypeRow,
                                                FSBillingCycle fsBillingCycleRow,
                                                int appointmentCount,
                                                int serviceCount,
                                                int partCount,
                                                PXCache cacheServiceOrderDetailServices,
                                                PXCache cacheServiceOrderDetailParts,
                                                PXCache cacheServiceOrderAppointments,
                                                PXCache cacheServiceOrderAttendees,
                                                PXCache cacheServiceOrderEquipment,
                                                PXCache cacheServiceOrderEmployees,
                                                PXCache cacheServiceOrder_Contact,
                                                PXCache cacheServiceOrder_Address,
                                                bool? isBeingCalledFromQuickProcess,
                                                bool allowCustomerChange = false)
        {
            bool enablePartsTab = true;
            bool enableServicesTab = true;

            if (fsServiceOrderRow != null
                && fsSrvOrdTypeRow != null)
            {
                if (fsSrvOrdTypeRow.Behavior != ID.Behavior_SrvOrderType.INTERNAL_APPOINTMENT)
                {
                    enablePartsTab = fsServiceOrderRow.CustomerID != null && fsSrvOrdTypeRow.PostTo != ID.SrvOrdType_PostTo.ACCOUNTS_RECEIVABLE_MODULE;
                    enableServicesTab = fsServiceOrderRow.CustomerID != null;
                }
            }

            bool enableDelete;
            bool enableInsertUpdate;
            bool isQuote = fsSrvOrdTypeRow.Behavior == ID.Behavior_SrvOrderType.QUOTE;

            if (fsAppointmentRow != null)
            {
                enableInsertUpdate = AppointmentCore.CanUpdateAppointment(fsAppointmentRow, fsSrvOrdTypeRow);
                enableDelete = AppointmentCore.CanDeleteAppointment(fsAppointmentRow, fsServiceOrderRow, fsSrvOrdTypeRow);
            }
            else
            {
                enableDelete = CanDeleteServiceOrder(graph, fsServiceOrderRow);
                enableInsertUpdate = CanUpdateServiceOrder(fsServiceOrderRow, fsSrvOrdTypeRow);
            }

            //TODO SD-7592
            //Enable/Disable all controls for the cache.
            //PXUIFieldAttribute.SetEnabled(cacheServiceOrder, fsServiceOrderRow, enableInsertUpdate);
            //Enable/Disable all view buttons
            cacheServiceOrder.AllowInsert = true;
            cacheServiceOrder.AllowUpdate = enableInsertUpdate || allowCustomerChange || (isBeingCalledFromQuickProcess ?? false);

            if (fsServiceOrderRow.Status == ID.Status_ServiceOrder.CANCELED)
            {
                cacheServiceOrder.AllowUpdate = false;
            }

            if (fsServiceOrderRow.Status == ID.Status_ServiceOrder.COMPLETED)
            {
                cacheServiceOrder.AllowUpdate = fsSrvOrdTypeRow.Active == true;
            }

            cacheServiceOrder.AllowDelete = enableDelete;

            if (cacheServiceOrderDetailServices != null)
            {
                cacheServiceOrderDetailServices.AllowInsert = enableInsertUpdate && enableServicesTab && fsServiceOrderRow.AllowInvoice == false;
                cacheServiceOrderDetailServices.AllowUpdate = enableInsertUpdate && enableServicesTab;
                cacheServiceOrderDetailServices.AllowDelete = enableInsertUpdate && enableServicesTab && fsServiceOrderRow.AllowInvoice == false;
            }

            if (cacheServiceOrderDetailParts != null)
            {
                bool isInventoryFeatureInstalled = PXAccess.FeatureInstalled<FeaturesSet.inventory>();

                cacheServiceOrderDetailParts.AllowInsert = isInventoryFeatureInstalled && enableInsertUpdate && enablePartsTab && fsServiceOrderRow.AllowInvoice == false;
                cacheServiceOrderDetailParts.AllowUpdate = isInventoryFeatureInstalled && enableInsertUpdate && enablePartsTab;
                cacheServiceOrderDetailParts.AllowDelete = isInventoryFeatureInstalled && enableInsertUpdate && enablePartsTab && fsServiceOrderRow.AllowInvoice == false;
            }

            if (cacheServiceOrder_Contact != null)
            {
                cacheServiceOrder_Contact.AllowInsert = enableInsertUpdate && fsServiceOrderRow.AllowInvoice == false;
                cacheServiceOrder_Contact.AllowUpdate = enableInsertUpdate && fsServiceOrderRow.AllowInvoice == false;
                cacheServiceOrder_Contact.AllowDelete = enableInsertUpdate && fsServiceOrderRow.AllowInvoice == false;
            }

            if (cacheServiceOrder_Address != null)
            {
                cacheServiceOrder_Address.AllowInsert = enableInsertUpdate && fsServiceOrderRow.AllowInvoice == false;
                cacheServiceOrder_Address.AllowUpdate = enableInsertUpdate && fsServiceOrderRow.AllowInvoice == false;
                cacheServiceOrder_Address.AllowDelete = enableInsertUpdate && fsServiceOrderRow.AllowInvoice == false;
            }

            if (cacheServiceOrderAppointments != null)
            {
                cacheServiceOrderAppointments.AllowInsert = enableInsertUpdate;
                cacheServiceOrderAppointments.AllowUpdate = enableInsertUpdate;
                cacheServiceOrderAppointments.AllowDelete = enableInsertUpdate;
            }

            if (cacheServiceOrderAttendees != null)
            {
                cacheServiceOrderAttendees.AllowSelect = !isQuote;
                cacheServiceOrderAttendees.AllowInsert = enableInsertUpdate && !isQuote;
                cacheServiceOrderAttendees.AllowUpdate = enableInsertUpdate && !isQuote;
                cacheServiceOrderAttendees.AllowDelete = enableInsertUpdate && !isQuote;
            }

            if (cacheServiceOrderEquipment != null)
            {
                cacheServiceOrderEquipment.AllowSelect = !isQuote;
                cacheServiceOrderEquipment.AllowInsert = enableInsertUpdate && !isQuote;
                cacheServiceOrderEquipment.AllowUpdate = enableInsertUpdate && !isQuote;
                cacheServiceOrderEquipment.AllowDelete = enableInsertUpdate && !isQuote;
            }

            if (cacheServiceOrderEmployees != null)
            {
                cacheServiceOrderEmployees.AllowSelect = !isQuote;
                cacheServiceOrderEmployees.AllowInsert = enableInsertUpdate && !isQuote;
                cacheServiceOrderEmployees.AllowUpdate = enableInsertUpdate && !isQuote;
                cacheServiceOrderEmployees.AllowDelete = enableInsertUpdate && !isQuote;
            }

            bool customerRequired = (bool)fsServiceOrderRow.BAccountRequired;
            bool contactRequired = (bool)fsSrvOrdTypeRow.RequireContact;
            bool enableServiceContractFields = fsBillingCycleRow != null
                                                    && fsBillingCycleRow.BillingBy == ID.Billing_By.SERVICE_ORDER
                                                    && (PXAccess.FeatureInstalled<FeaturesSet.equipmentManagementModule>()
                                                        || PXAccess.FeatureInstalled<FeaturesSet.routeManagementModule>());



            if (enableInsertUpdate == true)
            {
                bool isEnabledCustomerID = AllowEnableCustomerID(fsServiceOrderRow);

                PXUIFieldAttribute.SetEnabled<FSServiceOrder.customerID>(
                                                                        cacheServiceOrder,
                                                                        fsServiceOrderRow,
                                                                        customerRequired && isEnabledCustomerID);

                PXUIFieldAttribute.SetRequired<FSServiceOrder.contactID>(cacheServiceOrder, customerRequired && contactRequired);
                PXUIFieldAttribute.SetEnabled<FSServiceOrder.locationID>(cacheServiceOrder, fsServiceOrderRow, customerRequired);
                PXUIFieldAttribute.SetEnabled<FSServiceOrder.billCustomerID>(cacheServiceOrder, fsServiceOrderRow, customerRequired);
                PXUIFieldAttribute.SetEnabled<FSServiceOrder.billLocationID>(cacheServiceOrder, fsServiceOrderRow, customerRequired);
                PXUIFieldAttribute.SetEnabled<FSServiceOrder.billServiceContractID>(cacheServiceOrder, fsServiceOrderRow, enableServiceContractFields
                                                                                                                            && fsServiceOrderRow.AllowInvoice == false);
                PXUIFieldAttribute.SetVisible<FSServiceOrder.billServiceContractID>(cacheServiceOrder, fsServiceOrderRow, enableServiceContractFields);
                PXUIFieldAttribute.SetVisible<FSServiceOrder.billContractPeriodID>(cacheServiceOrder, fsServiceOrderRow, enableServiceContractFields && fsServiceOrderRow.BillServiceContractID != null);

                PXDefaultAttribute.SetPersistingCheck<FSServiceOrder.customerID>(
                                                                                cacheServiceOrder,
                                                                                fsServiceOrderRow,
                                                                                customerRequired && isEnabledCustomerID ? PXPersistingCheck.NullOrBlank : PXPersistingCheck.Nothing);

                PXDefaultAttribute.SetPersistingCheck<FSServiceOrder.contactID>(cacheServiceOrder, fsServiceOrderRow, customerRequired && contactRequired ? PXPersistingCheck.NullOrBlank : PXPersistingCheck.Nothing);
                PXDefaultAttribute.SetPersistingCheck<FSServiceOrder.locationID>(cacheServiceOrder, fsServiceOrderRow, customerRequired ? PXPersistingCheck.NullOrBlank : PXPersistingCheck.Nothing);

                EnableDisable_SLAETA(cacheServiceOrder, fsServiceOrderRow);
                EnableDisable_Hold(cacheServiceOrder, fsServiceOrderRow, appointmentCount);
            }

            bool noClosedOrCompletedAppointments = true;

            if (fsServiceOrderRow.AppointmentsCompletedOrClosedCntr > 0)
            {
                noClosedOrCompletedAppointments = false;
            }

            // Editing an Appointment, if there is more than 1 Appointment for the ServiceOrder, the fields that affect other Appointments are disabled.
            bool unrestrictedAppointmentEdition = fsAppointmentRow == null || appointmentCount <= 1;

            PXUIFieldAttribute.SetEnabled<FSServiceOrder.customerID>(cacheServiceOrder, fsServiceOrderRow, cacheServiceOrder.GetStatus(fsServiceOrderRow) == PXEntryStatus.Inserted && customerRequired && serviceCount == 0 && partCount == 0);

            PXUIFieldAttribute.SetEnabled<FSServiceOrder.billCustomerID>(cacheServiceOrder, fsServiceOrderRow, noClosedOrCompletedAppointments && unrestrictedAppointmentEdition);
            PXUIFieldAttribute.SetEnabled<FSServiceOrder.billLocationID>(cacheServiceOrder, fsServiceOrderRow, noClosedOrCompletedAppointments && unrestrictedAppointmentEdition);
            PXUIFieldAttribute.SetEnabled<FSServiceOrder.branchID>(cacheServiceOrder, fsServiceOrderRow, noClosedOrCompletedAppointments && unrestrictedAppointmentEdition);
            PXUIFieldAttribute.SetEnabled<FSServiceOrder.branchLocationID>(cacheServiceOrder, fsServiceOrderRow, noClosedOrCompletedAppointments && unrestrictedAppointmentEdition);
            PXUIFieldAttribute.SetEnabled<FSServiceOrder.projectID>(cacheServiceOrder, fsServiceOrderRow, noClosedOrCompletedAppointments && unrestrictedAppointmentEdition);
            PXUIFieldAttribute.SetVisible<FSServiceOrder.dfltProjectTaskID>(cacheServiceOrder, fsServiceOrderRow, !ProjectDefaultAttribute.IsNonProject(fsServiceOrderRow.ProjectID));
            PXUIFieldAttribute.SetEnabled<FSServiceOrder.dfltProjectTaskID>(cacheServiceOrder, fsServiceOrderRow, noClosedOrCompletedAppointments && unrestrictedAppointmentEdition && !ProjectDefaultAttribute.IsNonProject(fsServiceOrderRow.ProjectID));
            PXUIFieldAttribute.SetEnabled<FSServiceOrder.roomID>(cacheServiceOrder, fsServiceOrderRow, noClosedOrCompletedAppointments && unrestrictedAppointmentEdition);

            PXUIFieldAttribute.SetRequired<FSServiceOrder.contactID>(cacheServiceOrder, noClosedOrCompletedAppointments && customerRequired && contactRequired);
        }

        public static void EnableDisable_Hold(PXCache cacheServiceOrder, FSServiceOrder fsServiceOrderRow, int appointmentCount)
        {
            bool enable = appointmentCount == 0
                            && (fsServiceOrderRow.Status == ID.Status_ServiceOrder.OPEN
                                    || fsServiceOrderRow.Status == ID.Status_ServiceOrder.ON_HOLD
                                        || fsServiceOrderRow.Status == ID.Status_ServiceOrder.QUOTE);

            enable = enable
                        && (fsServiceOrderRow.BillServiceContractID == null
                                || (fsServiceOrderRow.BillServiceContractID != null && fsServiceOrderRow.BillContractPeriodID != null));

            PXUIFieldAttribute.SetEnabled<FSServiceOrder.hold>(cacheServiceOrder, fsServiceOrderRow, enable);
        }

        public static void EnableDisable_ActionButtons(
                ServiceOrderEntry graph,
                PXCache cache,
                FSServiceOrder fsServiceOrderRow,
                BAccount bAccountRow,
                FSSrvOrdType fsSrvOrdTypeRow,
                FSBillingCycle fsBillingCycle,
                PXAction<FSServiceOrder> completeOrder,
                PXAction<FSServiceOrder> cancelOrder,
                PXAction<FSServiceOrder> reopenOrder,
                PXAction<FSServiceOrder> closeOrder,
                PXAction<FSServiceOrder> invoiceOrder,
                PXAction<FSServiceOrder> uncloseOrder,
                PXAction<FSServiceOrder> openAppointmentScreen,
                PXAction<FSServiceOrder> openUserCalendar,
                PXAction<FSServiceOrder> openEmployeeBoard,
                PXAction<FSServiceOrder> openRoomBoard,
                PXAction<FSServiceOrder> openServiceSelector,
                PXAction<FSServiceOrder> openStaffSelectorFromServiceTab,
                PXAction<FSServiceOrder> openStaffSelectorFromStaffTab,
                PXAction<FSServiceOrder> viewDirectionOnMap,
                PXAction<FSServiceOrder> validateAddress,
                PXAction<FSServiceOrder> convertToServiceOrder,
                PXAction<FSServiceOrder> createPurchaseOrder,
                PXAction<FSServiceOrder> allowInvoice)
        {
            bool isSrvOrdTypeActive = fsSrvOrdTypeRow?.Active == true;

            // IF the record is being Inserted
            // THEN
            // Disable all actions
            if (fsSrvOrdTypeRow == null
                    || cache.GetStatus(fsServiceOrderRow) == PXEntryStatus.Inserted)
            {
                _EnableDisableActionButtons(
                    new PXAction<FSServiceOrder>[]
                    {
                        completeOrder,
                        cancelOrder,
                        reopenOrder,
                        closeOrder,
                        invoiceOrder,
                        uncloseOrder,
                        openAppointmentScreen,
                        openUserCalendar,
                        openEmployeeBoard,
                        openRoomBoard,
                        openServiceSelector,
                        openStaffSelectorFromServiceTab,
                        openStaffSelectorFromStaffTab,
                        viewDirectionOnMap,
                        validateAddress,
                        convertToServiceOrder,
                        createPurchaseOrder,
                        allowInvoice
                    },
                    false);
            }
            else
            {
                if (fsSrvOrdTypeRow.Behavior == ID.Behavior_SrvOrderType.QUOTE)
                {
                    _EnableDisableActionButtons(
                        new PXAction<FSServiceOrder>[]
                        {
                            completeOrder,
                            cancelOrder,
                            reopenOrder,
                            closeOrder,
                            invoiceOrder,
                            uncloseOrder,
                            openAppointmentScreen,
                            openUserCalendar,
                            openEmployeeBoard,
                            openRoomBoard,
                            openServiceSelector,
                            viewDirectionOnMap,
                            validateAddress,
                            createPurchaseOrder,
                            allowInvoice
                        },
                        false);

                    convertToServiceOrder.SetEnabled(true);
                }
                else
                {
                    bool isBillingBySO = fsBillingCycle != null && fsBillingCycle.BillingBy == ID.Billing_By.SERVICE_ORDER ? true : false;

                    completeOrder.SetEnabled(CheckServiceOrderStatusTransition(fsSrvOrdTypeRow.Behavior, fsServiceOrderRow, (bool)fsServiceOrderRow.Hold, ID.Status_ServiceOrder.COMPLETED));
                    cancelOrder.SetEnabled(CheckServiceOrderStatusTransition(fsSrvOrdTypeRow.Behavior, fsServiceOrderRow, (bool)fsServiceOrderRow.Hold, ID.Status_ServiceOrder.CANCELED)
                                                && fsServiceOrderRow.AllowInvoice == false);
                    reopenOrder.SetEnabled(CheckServiceOrderStatusTransition(fsSrvOrdTypeRow.Behavior, fsServiceOrderRow, (bool)fsServiceOrderRow.Hold, ID.Status_ServiceOrder.OPEN)
                                                && (fsServiceOrderRow.Status != ID.Status_ServiceOrder.OPEN || fsServiceOrderRow.AllowInvoice == true));
                    closeOrder.SetEnabled(CheckServiceOrderStatusTransition(fsSrvOrdTypeRow.Behavior, fsServiceOrderRow, (bool)fsServiceOrderRow.Hold, ID.Status_ServiceOrder.CLOSED));
                    uncloseOrder.SetEnabled(fsServiceOrderRow.Status == ID.Status_Appointment.CLOSED);
                    allowInvoice.SetEnabled(fsServiceOrderRow.AllowInvoice == false && isBillingBySO && fsServiceOrderRow.Status != ID.Status_ServiceOrder.CANCELED && fsServiceOrderRow.Hold != true);
                    invoiceOrder.SetEnabled(IsServiceOrderReadyToBeInvoiced(fsServiceOrderRow, fsBillingCycle));

                    bool isAServiceOrderForProspect = bAccountRow?.Type == BAccountType.ProspectType;

                    if (openAppointmentScreen != null)
                    {
                        openAppointmentScreen.SetEnabled(
                            !isAServiceOrderForProspect
                                &&  isSrvOrdTypeActive
                                && (fsServiceOrderRow.Status == ID.Status_ServiceOrder.OPEN
                                        || fsServiceOrderRow.Status == ID.Status_ServiceOrder.QUOTE));
                    }

                    openUserCalendar.SetEnabled(!isAServiceOrderForProspect);
                    openEmployeeBoard.SetEnabled(!isAServiceOrderForProspect);
                    openRoomBoard.SetEnabled(true);

                    convertToServiceOrder.SetEnabled(false);
                    createPurchaseOrder.SetEnabled(
                        fsServiceOrderRow.Status != ID.Status_ServiceOrder.CANCELED
                            && fsServiceOrderRow.Status != ID.Status_ServiceOrder.CLOSED
                                && AreThereAnyItemsForPO(graph, fsServiceOrderRow)
                                   && (fsSrvOrdTypeRow?.PostTo == ID.Batch_PostTo.SO || fsSrvOrdTypeRow?.PostTo == ID.Batch_PostTo.SI));
                }
            }

            bool enableEmployeeSelector, enableServiceSelector;

            enableEmployeeSelector = enableServiceSelector
                = fsServiceOrderRow.Status != ID.Status_ServiceOrder.CANCELED
                    && fsServiceOrderRow.Status != ID.Status_ServiceOrder.CLOSED
                     && isSrvOrdTypeActive;

            if (fsSrvOrdTypeRow.Behavior != ID.Behavior_SrvOrderType.INTERNAL_APPOINTMENT)
            {
                enableServiceSelector = enableServiceSelector
                                                && fsServiceOrderRow.CustomerID != null;
            }

            openServiceSelector.SetEnabled(enableServiceSelector);

            openStaffSelectorFromServiceTab.SetEnabled(enableEmployeeSelector);
            openStaffSelectorFromStaffTab.SetEnabled(enableEmployeeSelector);

            if (fsServiceOrderRow != null && !graph.UnattendedMode)
            {
                FSAddress fsAddressRow = graph.ServiceOrder_Address.SelectSingle();
                bool enableAddressValidation = (fsServiceOrderRow.Status != ID.Status_ServiceOrder.COMPLETED && fsServiceOrderRow.Status != ID.Status_ServiceOrder.CANCELED)
                                                && ((fsAddressRow != null && fsAddressRow.IsDefaultAddress == false && fsAddressRow.IsValidated == false));
                validateAddress.SetEnabled(enableAddressValidation);
            }
        }

        private static bool IsServiceOrderReadyToBeInvoiced(FSServiceOrder fsServiceOrderRow, FSBillingCycle fsBillingCycle)
        {
            return fsServiceOrderRow != null 
                    && fsBillingCycle != null
                    && fsServiceOrderRow.CBID != null
                    && fsServiceOrderRow.PostedBy == null
                    && fsBillingCycle.BillingBy == ID.Billing_By.SERVICE_ORDER
                    && (fsServiceOrderRow.Status != ID.Status_ServiceOrder.CANCELED
                            && fsServiceOrderRow.Status != ID.Status_ServiceOrder.ON_HOLD
                            && fsServiceOrderRow.Status != ID.Status_ServiceOrder.QUOTE)
                    && fsServiceOrderRow.AllowInvoice == true
                    && fsServiceOrderRow.PendingAPARSOPost == true
                    && (fsBillingCycle.InvoiceOnlyCompletedServiceOrder == false
                            || (fsBillingCycle.InvoiceOnlyCompletedServiceOrder == true
                                    && (fsServiceOrderRow.Status == ID.Status_ServiceOrder.COMPLETED
                                            || fsServiceOrderRow.Status == ID.Status_ServiceOrder.CLOSED)));
        }

        private static bool AreThereAnyItemsForPO(ServiceOrderEntry graph, FSServiceOrder fsServiceOrderRow)
        {
            if (fsServiceOrderRow == null)
            {
                return false;
            }

            foreach (FSSODet row in graph.ServiceOrderDetParts.Select())
            {
                if (row.EnablePO == true && string.IsNullOrEmpty(row.PONbr) == true)
                {
                    return true;
                }
            }

            foreach (FSSODet row in graph.ServiceOrderDetServices.Select())
            {
                if (row.EnablePO == true && string.IsNullOrEmpty(row.PONbr) == true)
                {
                    return true;
                }
            }

            return false;
        }

        public static void EnableDisable_Acct_Sub(PXCache cache, IFSSODetBase fsSODetRow, FSSrvOrdType fsSrvOrdTypeRow, FSServiceOrder fsServiceOrderRow)
        {
            bool enableAcctSub = fsSrvOrdTypeRow != null && fsSrvOrdTypeRow.Behavior != ID.Behavior_SrvOrderType.INTERNAL_APPOINTMENT
                                    && fsServiceOrderRow != null && fsServiceOrderRow.Quote == false
                                        && (fsSODetRow.LineType == ID.LineType_ServiceTemplate.NONSTOCKITEM
                                            || fsSODetRow.LineType == ID.LineType_ServiceTemplate.SERVICE
                                                || fsSODetRow.LineType == ID.LineType_ServiceTemplate.INVENTORY_ITEM);

            if (fsSODetRow is FSSODet)
            {
                PXUIFieldAttribute.SetEnabled<FSSODet.acctID>(cache, fsSODetRow, enableAcctSub);
                PXUIFieldAttribute.SetEnabled<FSSODet.subID>(cache, fsSODetRow, enableAcctSub);
            }
            else if (fsSODetRow is FSAppointmentDet)
            {
                PXUIFieldAttribute.SetEnabled<FSAppointmentDet.acctID>(cache, fsSODetRow, enableAcctSub);
                PXUIFieldAttribute.SetEnabled<FSAppointmentDet.subID>(cache, fsSODetRow, enableAcctSub);
            }

            if (enableAcctSub == false)
            {
                if (fsSODetRow is FSSODet)
                {
                    cache.SetValueExt<FSSODet.acctID>(fsSODetRow, null);
                    cache.SetValueExt<FSSODet.subID>(fsSODetRow, null);
                }
                else if (fsSODetRow is FSAppointmentDet)
                {
                    cache.SetValueExt<FSAppointmentDet.acctID>(fsSODetRow, null);
                    cache.SetValueExt<FSAppointmentDet.subID>(fsSODetRow, null);
                }
            }
        }

        /// <summary>
        /// Returns true if a Service order [fsServiceOrderRow] can be updated based on its status and its SrvOrdtype's status.
        /// </summary>
        public static bool CanUpdateServiceOrder(FSServiceOrder fsServiceOrderRow, FSSrvOrdType fsSrvOrdTypeRow)
        {
            if (fsServiceOrderRow == null
                    || fsSrvOrdTypeRow == null)
            {
                return false;
            }

            if ((fsServiceOrderRow.Status == ID.Status_ServiceOrder.CLOSED
                        || fsServiceOrderRow.Status == ID.Status_ServiceOrder.CANCELED)
                        || fsSrvOrdTypeRow.Active == false)
            {
                return false;
            }

            return true;
        }

        /// <summary>
        /// Returns true if a Service order [fsServiceOrderRow] can be deleted based based in its status.
        /// </summary>
        public static bool CanDeleteServiceOrder(PXGraph graph, FSServiceOrder fsServiceOrderRow)
        {
            if (fsServiceOrderRow == null
                    || fsServiceOrderRow.Mem_Invoiced == true
                    || fsServiceOrderRow.AllowInvoice == true
                    || (fsServiceOrderRow.Status != ID.Status_ServiceOrder.OPEN
                        && fsServiceOrderRow.Status != ID.Status_ServiceOrder.ON_HOLD
                        && fsServiceOrderRow.Status != ID.Status_ServiceOrder.QUOTE))
            {
                return false;
            }

            if (fsServiceOrderRow.AppointmentsCompletedCntr > 0)
            {
                return false;
            }

            return true;
        }

        /// <summary>
        /// Returns true if a Service order [fsServiceOrderRow] has an appointment assigned.
        /// </summary>
        public static bool ServiceOrderHasAppointment(PXGraph graph, FSServiceOrder fsServiceOrderRow)
        {
            PXResultset<FSAppointment> fsAppointmentSet = PXSelect<FSAppointment,
                                                           Where<
                                                                FSAppointment.sOID, Equal<Required<FSAppointment.sOID>>>>
                                                           .Select(graph, fsServiceOrderRow.SOID);

            if (fsAppointmentSet.Count == 0)
            {
                return false;
            }

            return true;
        }

        /// <summary>
        /// Returns true if a Service in the Service Order <c>fsSODetServiceRow</c> is linked with an appointment.
        /// </summary>
        public static bool FSSODetLinkedToAppointments(PXGraph graph, FSSODet fsSODetRow)
        {
            PXResultset<FSAppointmentDetService> fsAppointmentDetSet = PXSelect<FSAppointmentDetService,
                                                                        Where<
                                                                            FSAppointmentDetService.sODetID, Equal<Required<FSSODetService.sODetID>>>>
                                                                        .Select(graph, fsSODetRow.SODetID);

            if (fsAppointmentDetSet.Count > 0)
            {
                return true;
            }

            return false;
        }

        public static void EnableDisable_SLAETA(PXCache cacheServiceOrder, FSServiceOrder fsServiceOrderRow)
        {
            PXUIFieldAttribute.SetEnabled<FSServiceOrder.sLAETA>(cacheServiceOrder, fsServiceOrderRow, fsServiceOrderRow.SourceType != ID.SourceType_ServiceOrder.CASE);
        }

        #endregion
        
        #region Defaults

        public static int? GetDefaultLocationID(PXGraph graph, int? bAccountID)
        {
            if (bAccountID == null) 
            {
                return null;
            }

            Location locationRow = PXSelectJoin<Location,
                                   InnerJoin<BAccount,
                                         On<BAccount.bAccountID, Equal<Location.bAccountID>,
                                            And<BAccount.defLocationID, Equal<Location.locationID>>>>,
                                   Where<
                                        Location.bAccountID, Equal<Required<Location.bAccountID>>>>
                                   .Select(graph, bAccountID);

            if (locationRow == null)
            {
                return null;
            }

            return locationRow.LocationID;
        }

        public static int? GetCBIDFromCustomer(PXGraph graph, int? bAccountID, string srvOrdType)
        {
            if (bAccountID == null)
            {
                return null;
            }

            FSCustomerBillingSetup fsCustomerBillingSetupRow = PXSelectJoin<FSCustomerBillingSetup,
                                                                CrossJoinSingleTable<FSSetup>,
                                                                Where2<
                                                                    Where<
                                                                        FSCustomerBillingSetup.customerID, Equal<Required<FSCustomerBillingSetup.customerID>>>,
                                                                And<
                                                                    Where2 <
                                                                        Where<
                                                                            FSSetup.customerMultipleBillingOptions, Equal<False>,
                                                                            And<FSCustomerBillingSetup.srvOrdType, IsNull,
                                                                            And<FSCustomerBillingSetup.active, Equal<True>>>>,
                                                                    Or<
                                                                        Where<
                                                                            FSSetup.customerMultipleBillingOptions, Equal<True>,
                                                                            And<FSCustomerBillingSetup.srvOrdType, Equal<Required<FSServiceOrder.srvOrdType>>,
                                                                            And<FSCustomerBillingSetup.active, Equal<True>>>>>>>>>
                                                            .Select(graph, bAccountID, srvOrdType);

            if (fsCustomerBillingSetupRow == null)
            {
                return null;
            }

            return fsCustomerBillingSetupRow.CBID;
        }

        public static DateTime? CalculateCutOffDate(string frequencyType, int? weeklyFrequency, int? monthlyFrequency, DateTime? orderDate)
        {
            DateTime? cutOffDate = orderDate;

            if (frequencyType == ID.Frequency_Type.WEEKLY)
            {
                int offset = weeklyFrequency.Value - (int)orderDate.Value.DayOfWeek;

                if ((int)orderDate.Value.DayOfWeek > weeklyFrequency)
                {
                    offset += 7;
                }

                cutOffDate = orderDate.Value.AddDays(offset);
            }
            else if (frequencyType == ID.Frequency_Type.MONTHLY)
            {
                if (orderDate.Value.Day <= monthlyFrequency)
                {
                    int daysInMonth = DateTime.DaysInMonth(orderDate.Value.Year, orderDate.Value.Month);

                    if (monthlyFrequency <= daysInMonth)
                    {
                        cutOffDate = orderDate.Value.AddDays(monthlyFrequency.Value - orderDate.Value.Day);
                    }
                    else
                    {
                        cutOffDate = orderDate.Value.AddDays(daysInMonth - orderDate.Value.Day);
                    }
                }
                else
                {
                    cutOffDate = orderDate.Value.AddDays(monthlyFrequency.Value - orderDate.Value.Day).AddMonths(1);
                }
            }

            return cutOffDate;
        }

        public static DateTime? GetCutOffDate(PXGraph graph, int? CBID, DateTime? docDate)
        {
            string frequencyType = string.Empty;
            int? weeklyFrequency = null;
            int? monthlyFrequency = null;

            if (CBID != null)
            {
                FSCustomerBillingSetup fsCustomerBillingSetupRow = PXSelect<FSCustomerBillingSetup, 
                                                                   Where<
                                                                       FSCustomerBillingSetup.cBID, Equal<Required<FSCustomerBillingSetup.cBID>>>>
                                                                   .Select(graph, CBID);

                if (fsCustomerBillingSetupRow != null)
                {
                    if (fsCustomerBillingSetupRow.FrequencyType != ID.Frequency_Type.NONE)
                    {
                        frequencyType = fsCustomerBillingSetupRow.FrequencyType;
                        weeklyFrequency = fsCustomerBillingSetupRow.WeeklyFrequency;
                        monthlyFrequency = fsCustomerBillingSetupRow.MonthlyFrequency;
                    }
                    else
                    {
                        FSBillingCycle fsBillingCycleRow = PXSelect<FSBillingCycle,
                                                           Where<
                                                                FSBillingCycle.billingCycleID, Equal<Required<FSBillingCycle.billingCycleID>>>>
                                                           .Select(graph, fsCustomerBillingSetupRow.BillingCycleID);

                        if (fsBillingCycleRow != null && fsBillingCycleRow.BillingCycleType == ID.Billing_Cycle_Type.TIME_FRAME)
                        {
                            frequencyType = fsBillingCycleRow.TimeCycleType;
                            weeklyFrequency = fsBillingCycleRow.TimeCycleWeekDay;
                            monthlyFrequency = fsBillingCycleRow.TimeCycleDayOfMonth;
                        }
                    }
                }

                return CalculateCutOffDate(frequencyType, weeklyFrequency, monthlyFrequency, docDate);
            }
            
            return docDate;
        }
        #endregion

        #region Set Values

        public static void SetServiceOrderTypeValues(FSServiceOrder fsServiceOrderRow, FSSrvOrdType fsSrvOrdTypeRow)
        {                
            fsServiceOrderRow.BAccountRequired = fsSrvOrdTypeRow.BAccountRequired;
        }

        private static void SetDocDesc(PXGraph graph, FSServiceOrder fsServiceOrderRow)
        {
            FSSODet fsSODetRow = PXSelect<FSSODet,
                                 Where<
                                        FSSODet.sOID, Equal<Required<FSSODet.sOID>>>,
                                 OrderBy<Asc<FSSODet.sODetID>>>
                                 .Select(graph, fsServiceOrderRow.SOID);

            if (fsSODetRow != null)
            {
                fsServiceOrderRow.DocDesc = fsSODetRow.TranDesc;
            }
        }

        private static void SetBillCustomerAndLocationID(PXCache cache, FSServiceOrder fsServiceOrderRow)
        {
            BAccount bAccountRow = PXSelect<BAccount,
                                   Where<
                                       BAccount.bAccountID, Equal<Required<FSServiceOrder.customerID>>>>
                                   .Select(cache.Graph, fsServiceOrderRow.CustomerID);

            int? billCustomerID = null;
            int? billLocationID = null;

            if (bAccountRow == null || bAccountRow.Type != BAccountType.ProspectType)
            {
                Customer customerRow = SharedFunctions.GetCustomerRow(cache.Graph, fsServiceOrderRow.CustomerID);
                FSxCustomer fsxCustomerRow = PXCache<Customer>.GetExtension<FSxCustomer>(customerRow);

                switch (fsxCustomerRow.DefaultBillingCustomerSource)
                {
                    case ID.Default_Billing_Customer_Source.SERVICE_ORDER_CUSTOMER:
                        billCustomerID = fsServiceOrderRow.CustomerID;
                        billLocationID = fsServiceOrderRow.LocationID;
                        break;

                    case ID.Default_Billing_Customer_Source.DEFAULT_CUSTOMER:
                        billCustomerID = fsServiceOrderRow.CustomerID;
                        billLocationID = GetDefaultLocationID(cache.Graph, fsServiceOrderRow.CustomerID);
                        break;

                    case ID.Default_Billing_Customer_Source.SPECIFIC_CUSTOMER:
                        billCustomerID = fsxCustomerRow.BillCustomerID;
                        billLocationID = fsxCustomerRow.BillLocationID;
                        break;
                }
            }

            cache.SetValueExt<FSServiceOrder.billCustomerID>(fsServiceOrderRow, billCustomerID);
            cache.SetValueExt<FSServiceOrder.billLocationID>(fsServiceOrderRow, billLocationID);
        }

        #endregion
        
        public static AttendeeInfo GetAttendeeInfo(PXGraph graph, int? bAccountID, int? contactID)
        {
            AttendeeInfo attendeeInfo = new AttendeeInfo();
            bool updateCustomerContactName = true;

            if (bAccountID != null)
            {
                BAccount bAccountRow = PXSelect<BAccount,
                                       Where<
                                             BAccount.bAccountID, Equal<Required<BAccount.bAccountID>>>>
                                       .Select(graph, bAccountID);

                if (bAccountRow != null)
                {
                    attendeeInfo.CustomerContactName = bAccountRow.AcctName;

                    if (contactID == null)
                    {
                        contactID = bAccountRow.DefContactID;
                        updateCustomerContactName = false;
                    }
                }

                if (contactID != null)
                {
                    Contact contactRow = PXSelect<Contact,
                                         Where<
                                               Contact.contactID, Equal<Required<Contact.contactID>>>>
                                         .Select(graph, contactID);

                    if (contactRow != null)
                    {
                        if (updateCustomerContactName == true)
                        {
                            attendeeInfo.CustomerContactName = contactRow.DisplayName;
                        }

                        attendeeInfo.Phone1 = contactRow.Phone1;
                        attendeeInfo.EMail = contactRow.EMail;
                    }
                }
            }

            return attendeeInfo;
        }

        public static bool AllowEnableCustomerID(FSServiceOrder fsServiceOrderRow)
        {
            if (fsServiceOrderRow == null)
            {
                return false;
            }

            return fsServiceOrderRow.SourceType == ID.SourceType_ServiceOrder.SERVICE_DISPATCH;
        }

        public static void CheckPromiseDate(PXCache cache, FSServiceOrder fsServiceOrderRow) 
        {
            if (fsServiceOrderRow.PromisedDate > fsServiceOrderRow.SLAETA)
            {
                cache.RaiseExceptionHandling<FSServiceOrder.promisedDate>
                    (fsServiceOrderRow, fsServiceOrderRow.PromisedDate, new PXSetPropertyException(TX.Error.SLAETA_GREATER_EQUAL_PROMISEDDATE));
            }
        }

        public static void CheckSLAETA(PXCache cache, FSServiceOrder fsServiceOrderRow)
        {
            if (fsServiceOrderRow.SLAETA < fsServiceOrderRow.PromisedDate)
            {
                cache.RaiseExceptionHandling<FSServiceOrder.sLAETA>
                    (fsServiceOrderRow, fsServiceOrderRow.SLAETA, new PXSetPropertyException(TX.Error.SLAETA_GREATER_EQUAL_PROMISEDDATE));
            }
        }

        /// <summary>
        /// Search for <c>FSSODet</c> lines that are in Status "Open" or "InProcess" and return those rows in <c>fsSODetServiceSet</c> and <c>fsSODetPartSet</c>.
        /// </summary>
        public static void GetPendingLines(PXGraph graph, int? sOID, ref PXResultset<FSSODetService> bqlResultSet_FSSODetService, ref PXResultset<FSSODetPart> bqlResultSet_FSSODetPart)
        {
            bqlResultSet_FSSODetService = PXSelect<FSSODetService,
                                            Where2<
                                                Where<
                                                    FSSODetService.sOID, Equal<Required<FSSODetService.sOID>>>,
                                            And<
                                                Where2<
                                                    Where<
                                                        FSSODetService.status, Equal<FSSODetService.status.Open>,
                                                        Or<FSSODetService.status, Equal<FSSODetService.status.InProcess>>>,
                                                    And<
                                                        Where<
                                                            FSSODetService.lineType, Equal<ListField_LineType_Service_ServiceTemplate.Service>,
                                                            Or<FSSODetService.lineType, Equal<ListField_LineType_Service_ServiceTemplate.Comment_Service>,
                                                            Or<FSSODetService.lineType, Equal<ListField_LineType_Service_ServiceTemplate.Instruction_Service>,
                                                            Or<FSSODetService.lineType, Equal<ListField_LineType_Service_ServiceTemplate.NonStockItem>>>>>>>>>>
                                                    .Select(graph, sOID);

            bqlResultSet_FSSODetPart = PXSelect<FSSODetPart,
                                         Where2<
                                            Where<
                                                FSSODetPart.sOID, Equal<Required<FSSODetPart.sOID>>>,
                                            And<
                                                Where2<
                                                    Where<
                                                        FSSODetPart.status, Equal<FSSODetPart.status.Open>>,
                                                And<
                                                    Where<
                                                        FSSODetPart.lineType, Equal<ListField_LineType_Part_ALL.Inventory_Item>,
                                                        Or<FSSODetPart.lineType, Equal<ListField_LineType_Part_ALL.Comment_Part>,
                                                        Or<FSSODetPart.lineType, Equal<ListField_LineType_Part_ALL.Instruction_Part>>>>>>>>>
                                                .Select(graph, sOID);
        }

        public static bool CheckCustomerChange(
                                            PXCache cacheServiceOrder,
                                            PXFieldUpdatedEventArgs e,
                                            PXResultset<FSAppointment> bqlResultSet)
        {
            FSServiceOrder fsServiceOrderRow = (FSServiceOrder)e.Row;

            if (fsServiceOrderRow.Status != ID.Status_ServiceOrder.OPEN
                    && fsServiceOrderRow.Status != ID.Status_ServiceOrder.ON_HOLD
                    && fsServiceOrderRow.Status != ID.Status_ServiceOrder.QUOTE)
            {
                fsServiceOrderRow.CustomerID = (int?)e.OldValue;

                cacheServiceOrder.RaiseExceptionHandling<FSServiceOrder.customerID>(
                    e.Row,
                    e.OldValue,
                    new PXSetPropertyException(TX.Error.CUSTOMER_CHANGE_NOT_ALLOWED_SO_STATUS, PXErrorLevel.Warning));
                
                return false;
            }

            foreach (FSAppointment fsAppointmentRow in bqlResultSet)
            {
                if (SharedFunctions.IsAppointmentNotStarted(fsAppointmentRow) == false)
                {
                    fsServiceOrderRow.CustomerID = (int?)e.OldValue;

                    cacheServiceOrder.RaiseExceptionHandling<FSServiceOrder.customerID>(
                        e.Row,
                        e.OldValue,
                        new PXSetPropertyException(TX.Error.CUSTOMER_CHANGE_NOT_ALLOWED_APP_STATUS, PXErrorLevel.Warning)); 
                    
                    return false;
                }
            }

            return true;
        }

        private static void _EnableDisableActionButtons(PXAction<FSServiceOrder>[] pxActions, bool enable)
        {
            foreach (PXAction<FSServiceOrder> pxAction in pxActions)
            {
                if (pxAction != null)
                {
                    pxAction.SetEnabled(enable);
                }
            }
        }

        public static FSServiceOrder CreateServiceOrderCleanCopy(FSServiceOrder fsServiceOrderRow)
        {
            FSServiceOrder fsServiceOrderRow_Copy = PXCache<FSServiceOrder>.CreateCopy(fsServiceOrderRow);

            // Key fields are cleared to prevent bad references and calculations
            fsServiceOrderRow_Copy.SrvOrdType = null;
            fsServiceOrderRow_Copy.RefNbr = null;
            fsServiceOrderRow_Copy.SOID = null;

            fsServiceOrderRow_Copy.NoteID = null;
            fsServiceOrderRow_Copy.CuryInfoID = null;

            fsServiceOrderRow_Copy.BranchID = null;
            fsServiceOrderRow_Copy.BranchLocationID = null;
            fsServiceOrderRow_Copy.LocationID = null;
            fsServiceOrderRow_Copy.ContactID = null;
            fsServiceOrderRow_Copy.Status = null;

            //++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++
            //Clean total fields
            fsServiceOrderRow_Copy.EstimatedDurationTotal = 0;
            fsServiceOrderRow_Copy.ApptDurationTotal = 0;

            fsServiceOrderRow_Copy.CuryEstimatedOrderTotal = 0;
            fsServiceOrderRow_Copy.CuryApptOrderTotal = 0;
            fsServiceOrderRow_Copy.CuryBillableOrderTotal = 0;

            fsServiceOrderRow_Copy.EstimatedOrderTotal = 0;
            fsServiceOrderRow_Copy.ApptOrderTotal = 0;
            fsServiceOrderRow_Copy.BillableOrderTotal = 0;
            //----------------------------------------------------------------------

            return fsServiceOrderRow_Copy;
        }

        [Obsolete("Remove method no longer needed in a MAJOR Release")]
        public static int? Get_TranAcctID_DefaultValue(PXGraph graph, string salesAcctSource, int? inventoryID, FSServiceOrder fsServiceOrderRow)
        {
            int? salesAcctID = null;
            InventoryItem inventoryItemRow = null;
            INPostClass postclass = null;
            INSite inSiteRow = null;

            switch (salesAcctSource)
            {
                case ID.SrvOrdType_SalesAcctSource.INVENTORY_ITEM:
                    inventoryItemRow = SharedFunctions.GetInventoryItemRow(graph, inventoryID);
                    postclass = new INPostClass();

                    if (inventoryItemRow != null)
                    {
                        salesAcctID = inventoryItemRow.SalesAcctID;
                        if (salesAcctID == null)
                        {
                            postclass = PXSelectReadonly<INPostClass,
                                            Where<INPostClass.postClassID, Equal<Required<INPostClass.postClassID>>>>
                                        .Select(graph, inventoryItemRow.PostClassID);
                            salesAcctID = postclass?.SalesAcctID;
                        }
                    }

                    return salesAcctID;

                case ID.SrvOrdType_SalesAcctSource.WAREHOUSE:
                    inventoryItemRow = SharedFunctions.GetInventoryItemRow(graph, inventoryID);
                    
                    if (inventoryItemRow != null)
                    {
                        if (inventoryItemRow.StkItem == false)
                        {
                            FSBranchLocation fsBranchLocationRow = PXSelectReadonly<FSBranchLocation,
                                                                        Where<FSBranchLocation.branchLocationID, Equal<Required<FSBranchLocation.branchLocationID>>>>
                                                                    .Select(graph, fsServiceOrderRow.BranchLocationID);
                            if (fsBranchLocationRow != null)
                            {
                                inSiteRow = PXSelectReadonly<INSite,
                                                Where<INSite.siteID, Equal<Required<INSite.siteID>>>>
                                            .Select(graph, fsBranchLocationRow.DfltSiteID);
                                salesAcctID = inSiteRow?.SalesAcctID;
                            }
                        }
                        else
                        {
                            inSiteRow = PXSelectReadonly<INSite,
                                            Where<INSite.siteID, Equal<Required<INSite.siteID>>>>
                                        .Select(graph, inventoryItemRow.DfltSiteID);
                            salesAcctID = inSiteRow?.SalesAcctID;
                        }
                        }

                    return salesAcctID;

                case ID.SrvOrdType_SalesAcctSource.POSTING_CLASS:
                    inventoryItemRow = SharedFunctions.GetInventoryItemRow(graph, inventoryID);
                    postclass = new INPostClass();

                    if (inventoryItemRow != null)
                    {
                        postclass = PXSelectReadonly<INPostClass,
                                        Where<INPostClass.postClassID, Equal<Required<INPostClass.postClassID>>>>
                                    .Select(graph, inventoryItemRow.PostClassID);
                        salesAcctID = postclass?.SalesAcctID;
                    }

                    return salesAcctID;

                case ID.SrvOrdType_SalesAcctSource.CUSTOMER_LOCATION:

                    if (fsServiceOrderRow.CustomerID == null || fsServiceOrderRow.LocationID == null)
                    {
                        return null;
                    }

                    Location customerLocationRow = PXSelect<Location,
                                   Where<
                                        Location.bAccountID, Equal<Required<Location.bAccountID>>,
                                        And<Location.locationID, Equal<Required<Location.locationID>>>>>
                                   .Select(graph, fsServiceOrderRow.CustomerID, fsServiceOrderRow.LocationID);

                    return customerLocationRow?.CSalesAcctID;
            }

            return null;
        }

        public static int? Get_TranAcctID_DefaultValue(PXGraph graph, string salesAcctSource, int? inventoryID, int? siteID, FSServiceOrder fsServiceOrderRow)
        {
            int? salesAcctID = null;
            InventoryItem inventoryItemRow = null;
            INPostClass postclass = null;
            INSite inSiteRow = null;

            switch (salesAcctSource)
            {
                case ID.SrvOrdType_SalesAcctSource.INVENTORY_ITEM:
                    inventoryItemRow = SharedFunctions.GetInventoryItemRow(graph, inventoryID);
                    postclass = new INPostClass();

                    if (inventoryItemRow != null)
                    {
                        salesAcctID = inventoryItemRow.SalesAcctID;
                        if (salesAcctID == null)
                        {
                            postclass = PXSelectReadonly<INPostClass,
                                            Where<INPostClass.postClassID, Equal<Required<INPostClass.postClassID>>>>
                                        .Select(graph, inventoryItemRow.PostClassID);
                            salesAcctID = postclass?.SalesAcctID;
                        }
                    }

                    return salesAcctID;

                case ID.SrvOrdType_SalesAcctSource.WAREHOUSE:
                    inventoryItemRow = SharedFunctions.GetInventoryItemRow(graph, inventoryID);

                    if (inventoryItemRow != null)
                    {
                        if (inventoryItemRow.StkItem == false)
                        {
                            FSBranchLocation fsBranchLocationRow = PXSelectReadonly<FSBranchLocation,
                                                                        Where<FSBranchLocation.branchLocationID, Equal<Required<FSBranchLocation.branchLocationID>>>>
                                                                    .Select(graph, fsServiceOrderRow.BranchLocationID);
                            if (fsBranchLocationRow != null)
                            {
                                inSiteRow = PXSelectReadonly<INSite,
                                                Where<INSite.siteID, Equal<Required<INSite.siteID>>>>
                                            .Select(graph, fsBranchLocationRow.DfltSiteID);
                                salesAcctID = inSiteRow?.SalesAcctID;
                            }
                        }
                        else
                        {
                            inSiteRow = PXSelectReadonly<INSite,
                                            Where<INSite.siteID, Equal<Required<INSite.siteID>>>>
                                        .Select(graph, siteID);
                            salesAcctID = inSiteRow?.SalesAcctID;
                        }
                    }

                    return salesAcctID;

                case ID.SrvOrdType_SalesAcctSource.POSTING_CLASS:
                    inventoryItemRow = SharedFunctions.GetInventoryItemRow(graph, inventoryID);
                    postclass = new INPostClass();

                    if (inventoryItemRow != null)
                    {
                        postclass = PXSelectReadonly<INPostClass,
                                        Where<INPostClass.postClassID, Equal<Required<INPostClass.postClassID>>>>
                                    .Select(graph, inventoryItemRow.PostClassID);
                        salesAcctID = postclass?.SalesAcctID;
                    }

                    return salesAcctID;

                case ID.SrvOrdType_SalesAcctSource.CUSTOMER_LOCATION:

                    if (fsServiceOrderRow.CustomerID == null || fsServiceOrderRow.LocationID == null)
                    {
                        return null;
                    }

                    Location customerLocationRow = PXSelect<Location,
                                   Where<
                                        Location.bAccountID, Equal<Required<Location.bAccountID>>,
                                        And<Location.locationID, Equal<Required<Location.locationID>>>>>
                                   .Select(graph, fsServiceOrderRow.CustomerID, fsServiceOrderRow.LocationID);

                    return customerLocationRow?.CSalesAcctID;
            }

            return null;
        }

        public static int? Get_INItemAcctID_DefaultValue(PXGraph graph, string salesAcctSource, int? inventoryID, FSServiceContract fsServiceContractRow)
        {
            switch (salesAcctSource)
            {
                case ID.SrvOrdType_SalesAcctSource.INVENTORY_ITEM:
                    InventoryItem inventoryItemRow = SharedFunctions.GetInventoryItemRow(graph, inventoryID);

                    return inventoryItemRow?.SalesAcctID;

                case ID.SrvOrdType_SalesAcctSource.CUSTOMER_LOCATION:
                    Location customerLocationRow = PXSelect<Location,
                                   Where<
                                        Location.bAccountID, Equal<Required<Location.bAccountID>>,
                                        And<Location.locationID, Equal<Required<Location.locationID>>>>>
                                   .Select(graph, fsServiceContractRow.CustomerID, fsServiceContractRow.CustomerLocationID);

                    return customerLocationRow?.CSalesAcctID;
            }

            return null;
        }

        public static object Get_IFSSODetBase_SubID_DefaultValue(PXCache cache, IFSSODetBase fsSODetRow, FSServiceOrder fsServiceOrderRow, FSSrvOrdType fsSrvOrdTypeRow, FSAppointment fsAppointmentRow = null)
        {
            int? inventoryID = fsSODetRow.IsService ? fsSODetRow.InventoryID : fsSODetRow.InventoryID;
            int? salesPersonID = fsAppointmentRow == null ? fsServiceOrderRow.SalesPersonID : fsAppointmentRow.SalesPersonID;

            SharedClasses.SubAccountIDList subAcctIDs = SharedFunctions.GetSubAccountIDs(cache.Graph, 
                                                                                        fsSrvOrdTypeRow, 
                                                                                        inventoryID, 
                                                                                        fsServiceOrderRow.BranchID, 
                                                                                        fsServiceOrderRow.LocationID, 
                                                                                        fsServiceOrderRow.BranchLocationID,
                                                                                        salesPersonID,
                                                                                        fsSODetRow.IsService);

            if (subAcctIDs == null)
            {
                return null;
            }

            object value = null;

            try
            {
                value = SubAccountMaskAttribute.MakeSub<FSSrvOrdType.combineSubFrom>(
                            cache.Graph,
                            fsSrvOrdTypeRow.CombineSubFrom,
                            new object[] { subAcctIDs.branchLocation_SubID, subAcctIDs.branch_SubID, subAcctIDs.inventoryItem_SubID, subAcctIDs.customerLocation_SubID, subAcctIDs.postingClass_SubID, subAcctIDs.salesPerson_SubID, subAcctIDs.srvOrdType_SubID, subAcctIDs.warehouse_SubID },
                            new Type[] { typeof(FSBranchLocation.subID), typeof(Location.cMPSalesSubID), typeof(InventoryItem.salesSubID), typeof(Location.cSalesSubID), typeof(INPostClass.salesSubID), typeof(SalesPerson.salesSubID), typeof(FSSrvOrdType.subID), fsSODetRow.IsService ? typeof(INSite.salesSubID) : typeof(InventoryItem.salesSubID)});

                if (fsSODetRow is FSSODet)
                {
                    cache.RaiseFieldUpdating<FSSODet.subID>(fsSODetRow, ref value);
                }

                if (fsSODetRow is FSAppointmentDet)
                {
                    cache.RaiseFieldUpdating<FSAppointmentDet.subID>(fsSODetRow, ref value);
                }
            }
            catch (PXMaskArgumentException ex)
            {
                if (fsSODetRow is FSSODet)
                {
                    cache.RaiseExceptionHandling<FSSODet.subID>(fsSODetRow, null, new PXSetPropertyException(ex.Message));
                }

                if (fsSODetRow is FSAppointmentDet)
                {
                    cache.RaiseExceptionHandling<FSAppointmentDet.subID>(fsSODetRow, null, new PXSetPropertyException(ex.Message));
                }

                value = null;
            }
            catch (PXSetPropertyException ex)
            {
                if (fsSODetRow is FSSODet)
                {
                    cache.RaiseExceptionHandling<FSSODet.subID>(fsSODetRow, value, ex);
                }

                if (fsSODetRow is FSAppointmentDet)
                {
                    cache.RaiseExceptionHandling<FSAppointmentDet.subID>(fsSODetRow, value, ex);
                }

                value = null;
            }

            return value;
        }

        public static void UpdateServiceCounts(FSServiceOrder fsServiceOrderRow, PXSelectBase<FSSODetService> serviceOrderServiceDetail, ServiceOrderEntry graphServiceOrderEntry, AppointmentEntry graphAppointmentEntryCaller, bool forceRecalculation)
        {
            PXGraph graph;
            FSAppointment fsAppointmentRow;

            if (graphAppointmentEntryCaller != null)
            {
                graph = graphAppointmentEntryCaller;
                fsAppointmentRow = graphAppointmentEntryCaller.AppointmentRecords.Current;
            }
            else
            {
                graph = graphServiceOrderEntry;
                fsAppointmentRow = null;
            }

            UpdateServiceCounts(fsServiceOrderRow, serviceOrderServiceDetail, graph, fsAppointmentRow, forceRecalculation, false);
        }

        public static void UpdateServiceCounts(FSServiceOrder fsServiceOrderRow, PXSelectBase<FSSODetService> serviceOrderDetail, PXGraph graph, FSAppointment fsAppointmentRow, bool forceRecalculation, bool forceUpdateAppointmentDet)
        {
            if (forceRecalculation == false && forceUpdateAppointmentDet == false && serviceOrderDetail.Cache.IsDirty == false)
            {
                return;
            }

            fsServiceOrderRow.ServiceCount = 0;
            fsServiceOrderRow.CompleteServiceCount = 0;
            fsServiceOrderRow.ScheduledServiceCount = 0;

            foreach (FSSODetService fsSODetServiceRow in serviceOrderDetail.Select())
            {
                if (fsSODetServiceRow.LineType == ID.LineType_All.SERVICE)
                {
                    bool rowIsBeingInserted = serviceOrderDetail.Cache.GetStatus(fsSODetServiceRow) == PXEntryStatus.Inserted;

                    string oldStatus = string.Empty;
                    if (rowIsBeingInserted == false)
                    {
                        oldStatus = (string)serviceOrderDetail.Cache.GetValueOriginal<FSSODetService.status>(fsSODetServiceRow);
                    }

                    if (rowIsBeingInserted == false && (forceUpdateAppointmentDet == true || fsSODetServiceRow.Status != oldStatus))
                    {
                        PropagateSODetStatusToAppointmentLines(graph, fsSODetServiceRow, fsAppointmentRow);
                    }

                    bool scheduled = false;

                    if (ID.Status_AppointmentDet.CanBeScheduled(fsSODetServiceRow.Status) == true)
                    {
                        if (rowIsBeingInserted == false &&
                            (forceRecalculation == true || ID.Status_AppointmentDet.CanBeScheduled(oldStatus) == false))
                        {
                            scheduled = IsSODetScheduled(graph, fsSODetServiceRow, fsAppointmentRow);
                        }
                        else
                        {
                            scheduled = (bool)fsSODetServiceRow.Scheduled;
                        }
                    }

                    if (fsSODetServiceRow.Scheduled != scheduled)
                    {
                        fsSODetServiceRow.Scheduled = scheduled;
                        serviceOrderDetail.Update(fsSODetServiceRow);
                    }

                    fsServiceOrderRow.ServiceCount += (fsSODetServiceRow.Status != ID.Status_AppointmentDet.CANCELED) ? 1 : 0;
                    fsServiceOrderRow.CompleteServiceCount += (fsSODetServiceRow.Status == ID.Status_AppointmentDet.COMPLETED) ? 1 : 0;
                    fsServiceOrderRow.ScheduledServiceCount += (fsSODetServiceRow.Scheduled == true) ? 1 : 0;
                }
            }  
        }

        public static void PropagateSODetStatusToAppointmentLines(PXGraph graph, FSSODetService fsSODetServiceRow, FSAppointment fsAppointmentRow)
        {
            int? appointmentID = fsAppointmentRow?.AppointmentID;

            PXUpdateJoin<Set<FSAppointmentDet.status, Required<FSAppointmentDet.status>>,
                FSAppointmentDet,
                    InnerJoin<FSAppointment,
                        On<FSAppointment.appointmentID, Equal<FSAppointmentDet.appointmentID>>>,
                Where<
                    FSAppointmentDet.sODetID, Equal<Required<FSAppointmentDet.sODetID>>,
                    And2<
                        Where<FSAppointmentDet.status, NotEqual<Required<FSAppointmentDet.status>>,
                            And<
                                Where<
                                    FSAppointmentDet.appointmentID, NotEqual<Required<FSAppointmentDet.appointmentID>>,
                                    Or<Required<FSAppointmentDet.appointmentID>, IsNull>>>>,
                        And<
                            Where<FSAppointment.status, Equal<FSAppointment.status.AutomaticScheduled>,
                                Or<FSAppointment.status, Equal<FSAppointment.status.ManualScheduled>,
                                Or<FSAppointment.status, Equal<FSAppointment.status.InProcess>>>>>>>>
                .Update(graph, fsSODetServiceRow.Status, fsSODetServiceRow.SODetID, fsSODetServiceRow.Status, appointmentID, appointmentID);
        }
        
        public static bool IsSODetScheduled(PXGraph graph, FSSODetService fsSODetServiceRow, FSAppointment fsAppointmentRow)
        {
            int? appointmentID = fsAppointmentRow?.AppointmentID;
            string status = fsAppointmentRow != null ? fsAppointmentRow.Status : string.Empty;

            FSAppointmentDet fsAppointmentDetRow = PXSelectJoin<FSAppointmentDet,
                    InnerJoin<FSAppointment,
                            On<FSAppointment.appointmentID, Equal<FSAppointmentDet.appointmentID>>>,
                Where<
                    FSAppointmentDet.lineType, Equal<FSAppointmentDet.lineType.Service>,
                    And<FSAppointmentDet.sODetID, Equal<Required<FSAppointmentDet.sODetID>>,
                    And<
                        Where2<
                            Where<
                                    FSAppointmentDet.status, Equal<FSAppointmentDet.status.Open>,
                                    Or<FSAppointmentDet.status, Equal<FSAppointmentDet.status.InProcess>>>,
                            And<
                                Where2<
                                    Where2<
                                        Where<
                                            FSAppointmentDet.appointmentID, NotEqual<Required<FSAppointmentDet.appointmentID>>,
                                            Or<Required<FSAppointmentDet.appointmentID>, IsNull>>,
                                        And<
                                            Where<
                                                FSAppointment.status, Equal<FSAppointment.status.AutomaticScheduled>,
                                                Or<FSAppointment.status, Equal<FSAppointment.status.ManualScheduled>,
                                                Or<FSAppointment.status, Equal<FSAppointment.status.InProcess>>>>>>,
                                    Or<
                                        Where<
                                            FSAppointment.appointmentID, Equal<Required<FSAppointment.appointmentID>>,
                                            And<
                                                Where<
                                                    Required<FSAppointment.status>, Equal<FSAppointment.status.AutomaticScheduled>,
                                                    Or<Required<FSAppointment.status>, Equal<FSAppointment.status.ManualScheduled>,
                                                    Or<Required<FSAppointment.status>, Equal<FSAppointment.status.InProcess>>>>>>>>>>>>>>
                .SelectSingleBound(graph, null, fsSODetServiceRow.SODetID, appointmentID, appointmentID, appointmentID, status, status, status);

            if (fsAppointmentDetRow != null)
            {
                return true;
            }

            return false;
        }

        /// <summary>
        /// Gets the corresponding Service Order Detail from the <c>fsAppointmentDetRow.SODetID</c>.
        /// </summary>
        public static FSSODet GetSODetFromAppointmentDet(PXGraph graph, FSAppointmentDet fsAppointmentDetRow)
        {
            FSSODet fsSODetRow = new FSSODet();

            if (fsAppointmentDetRow != null)
            {
                fsSODetRow = PXSelect<FSSODet, Where<FSSODet.sODetID, Equal<Required<FSSODet.sODetID>>>>.Select(graph, fsAppointmentDetRow.SODetID);
            }

            return fsSODetRow;
        }

        private static void UpdatePendingPostFlags(
            FSServiceOrder fsServiceOrderRow,
            PXSelectBase<FSSODetService> serviceOrderServiceDetail,
            PXSelectBase<FSSODetPart> serviceOrderPartDetail)
        {
            int? servicesToPost = serviceOrderServiceDetail.Select().AsEnumerable().Where(y => ((FSSODetService)y).needToBePosted() == true)
                                                                    .RowCast<FSPostInfo>().Where(x => x.isPosted() == false).Count();

            int? partsToPost = serviceOrderPartDetail.Select().AsEnumerable().Where(y => ((FSSODetPart)y).needToBePosted() == true)
                                                              .RowCast<FSPostInfo>().Where(x => x.isPosted() == false).Count();

            fsServiceOrderRow.PendingAPARSOPost = fsServiceOrderRow.PostedBy == null && (servicesToPost > 0 || partsToPost > 0);
            fsServiceOrderRow.PendingINPost = false;
        }

        private static void UpdateWatingForPartsFlag(
            FSServiceOrder fsServiceOrderRow,
            PXSelectBase<FSSODetService> serviceOrderServiceDetail,
            PXSelectBase<FSSODetPart> serviceOrderPartDetail)
        {
            int? serviceWaitingForParts = serviceOrderServiceDetail.Select().AsEnumerable().Where(y => ((FSSODetService)y).waitingForParts() == true).Count();

            int? partsWaitingForParts = serviceOrderPartDetail.Select().AsEnumerable().Where(y => ((FSSODetPart)y).waitingForParts() == true).Count();

            fsServiceOrderRow.WaitingForParts = (serviceWaitingForParts > 0 || partsWaitingForParts > 0);
        }

        private static void UpdateAppointmentsNeededFlag(
            PXGraph graph,
            FSServiceOrder fsServiceOrderRow,
            PXSelectBase<FSSODetService> serviceOrderServiceDetail,
            PXSelectBase<FSSODetPart> serviceOrderPartDetail,
            AppointmentEntry graphAppointmentEntryCaller)
        {
            FSAppointment fsAppointmentRow = null;
            bool currentAppointment = false;

            if (graphAppointmentEntryCaller != null)
            {
                graph = graphAppointmentEntryCaller;
                fsAppointmentRow = graphAppointmentEntryCaller.AppointmentRecords.Current;
            }

            FSAppointment lastAppointment = PXSelect<FSAppointment,
                                                Where<FSAppointment.status, NotEqual<FSAppointment.status.Canceled>,
                                                And<FSAppointment.sOID, Equal<Required<FSAppointment.sOID>>>>,
                                                OrderBy<Desc<FSAppointment.scheduledDateTimeBegin>>>.SelectSingleBound(graph, null, fsServiceOrderRow.SOID);

            if (fsAppointmentRow != null && lastAppointment != null && fsAppointmentRow.AppointmentID == lastAppointment.AppointmentID)
            {
                lastAppointment = fsAppointmentRow;
                currentAppointment = true;
            }

            if (fsAppointmentRow != null && lastAppointment == null)
            {
                lastAppointment = fsAppointmentRow;
                currentAppointment = true;
            }

            if (lastAppointment != null && 
                    (ID.Status_Appointment.IsOpen(lastAppointment.Status)
                            || (lastAppointment.Finished == true)))
            {
                fsServiceOrderRow.AppointmentsNeeded = false;
            }
            else
            {
                fsServiceOrderRow.AppointmentsNeeded = true;
            }

            if (fsServiceOrderRow.AppointmentsNeeded == false)
            {
                IEnumerable<PXResult<FSAppointmentDetService>> services = null;

                if (currentAppointment == true && graphAppointmentEntryCaller != null)
                { 
                    services = graphAppointmentEntryCaller.AppointmentDetServices.Select().AsEnumerable();
                }

                foreach (FSSODetService fsSODetServiceRow in serviceOrderServiceDetail.Select())
                {
                    if (currentAppointment == true && IsSODetScheduled(graph, lastAppointment, fsSODetServiceRow) == false)
                    {
                        int serviceNotScheduled = services.Where(y => ((FSAppointmentDetService)y).Status != ID.Status_AppointmentDet.CANCELED && ((FSAppointmentDetService)y).SODetID == fsSODetServiceRow.SODetID).Count();
                        serviceNotScheduled += services.Where(y => ((FSAppointmentDetService)y).Status != ID.Status_AppointmentDet.CANCELED && ((FSAppointmentDetService)y).Mem_SODetRow != null && ((FSAppointmentDetService)y).Mem_SODetRow.SODetID == fsSODetServiceRow.SODetID).Count();

                        if (serviceNotScheduled == 0)
                        {
                            fsServiceOrderRow.AppointmentsNeeded = true;
                            break;
                        }
                    }
                    else
                    {
                        if (IsSODetScheduled(graph, fsSODetServiceRow) == false)
                        {
                            fsServiceOrderRow.AppointmentsNeeded = true;
                            break;
                        }
                    }
                }

                IEnumerable<PXResult<FSAppointmentDetPart>> parts = null;

                if (currentAppointment == true && graphAppointmentEntryCaller != null)
                {
                    parts = graphAppointmentEntryCaller.AppointmentDetParts.Select().AsEnumerable();
                }

                foreach (FSSODetPart fsSODetPartRow in serviceOrderPartDetail.Select())
                {
                    if (currentAppointment == true && IsSODetScheduled(graph, lastAppointment, fsSODetPartRow) == false)
                    {
                        int partsNotScheduled = parts.Where(y => ((FSAppointmentDetPart)y).Status != ID.Status_AppointmentDet.CANCELED && ((FSAppointmentDetPart)y).SODetID == fsSODetPartRow.SODetID).Count();
                        partsNotScheduled += parts.Where(y => ((FSAppointmentDetPart)y).Status != ID.Status_AppointmentDet.CANCELED && ((FSAppointmentDetPart)y).Mem_SODetRow != null && ((FSAppointmentDetPart)y).Mem_SODetRow.SODetID == fsSODetPartRow.SODetID).Count();

                        if (partsNotScheduled == 0)
                        {
                            fsServiceOrderRow.AppointmentsNeeded = true;
                            break;
                        }
                    }
                    else
                    {
                        if (IsSODetScheduled(graph, fsSODetPartRow) == false)
                        {
                            fsServiceOrderRow.AppointmentsNeeded = true;
                            break;
                        }
                    }
                }
            }
        }

        public static bool IsSODetScheduled(PXGraph graph, FSSODet fsSODetRow)
        {
            if ((fsSODetRow.LineType != ID.LineType_All.SERVICE 
                    && fsSODetRow.LineType != ID.LineType_All.INVENTORY_ITEM)
                        || fsSODetRow.Status == ID.Status_AppointmentDet.CANCELED)
            {
                return true;
            }

            FSAppointment fsAppointmentRow = PXSelectJoin<FSAppointment,
	                                            InnerJoin<FSAppointmentDet,
                                                    On<
                                                        FSAppointmentDet.appointmentID, Equal<FSAppointment.appointmentID>>>,
	                                            Where<
                                                    FSAppointment.sOID, Equal<Required<FSAppointment.sOID>>,
                                                    And<FSAppointmentDet.sODetID, Equal<Required<FSAppointmentDet.sODetID>>,
                                                    And<FSAppointment.status, NotEqual<FSAppointment.status.Canceled>>>>>
                                                .SelectWindowed(graph, 0, 1, fsSODetRow.SOID, fsSODetRow.SODetID);

            if (fsAppointmentRow != null)
            {
                return true;
            }

            return false;
        }

        public static bool IsSODetScheduled(PXGraph graph, FSAppointment actualFSAppointmentRow, FSSODet fsSODetRow)
        {
            if ((fsSODetRow.LineType != ID.LineType_All.SERVICE
                    && fsSODetRow.LineType != ID.LineType_All.INVENTORY_ITEM)
                        || fsSODetRow.Status == ID.Status_AppointmentDet.CANCELED)
            {
                return true;
            }

            FSAppointment fsAppointmentRow = PXSelectJoin<FSAppointment,
                                                InnerJoin<FSAppointmentDet,
                                                    On<
                                                        FSAppointmentDet.appointmentID, Equal<FSAppointment.appointmentID>>>,
                                                Where<
                                                    FSAppointment.sOID, Equal<Required<FSAppointment.sOID>>,
                                                    And<FSAppointmentDet.sODetID, Equal<Required<FSAppointmentDet.sODetID>>,
                                                    And<FSAppointment.status, NotEqual<FSAppointment.status.Canceled>,
                                                    And<FSAppointment.appointmentID, NotEqual<Required<FSAppointment.appointmentID>>>>>>>
                                                .SelectWindowed(graph, 0, 1, fsSODetRow.SOID, fsSODetRow.SODetID, actualFSAppointmentRow.AppointmentID);

            if (fsAppointmentRow != null)
            {
                return true;
            }

            return false;
        }

        public static void UpdateWarrantyFlag(PXCache cache, IFSSODetBase fsSODetRow, DateTime? docDate)
        {
            fsSODetRow.Warranty = false;

            if (docDate == null || fsSODetRow.SMEquipmentID == null)
            {
                return;
            }

            if (fsSODetRow.EquipmentAction != ID.Equipment_Action.REPLACING_TARGET_EQUIPMENT
                && fsSODetRow.EquipmentAction != ID.Equipment_Action.REPLACING_COMPONENT
                && fsSODetRow.EquipmentAction != ID.Equipment_Action.NONE
                && fsSODetRow.LineType != ID.LineType_All.SERVICE)
            {
                return;
            }

            if (fsSODetRow.EquipmentAction == ID.Equipment_Action.REPLACING_COMPONENT
                && fsSODetRow.EquipmentLineRef == null)
            {
                return;
            }

            FSEquipment equipmentRow = PXSelect<FSEquipment,
                                       Where<
                                            FSEquipment.SMequipmentID, Equal<Required<FSEquipment.SMequipmentID>>>>
                                       .Select(cache.Graph, fsSODetRow.SMEquipmentID);

            InventoryItem inventoryItemRow = PXSelect<InventoryItem,
                                       Where<
                                            InventoryItem.inventoryID, Equal<Required<InventoryItem.inventoryID>>>>
                                       .Select(cache.Graph, fsSODetRow.InventoryID);

            if(inventoryItemRow == null)
            {
                return;
            }

            FSxEquipmentModel fSxEquipmentModelRow = PXCache<InventoryItem>.GetExtension<FSxEquipmentModel>(inventoryItemRow);

            if (fSxEquipmentModelRow == null || (fSxEquipmentModelRow != null && (fSxEquipmentModelRow.EquipmentItemClass == ID.Equipment_Item_Class.MODEL_EQUIPMENT
                                                                                 || fSxEquipmentModelRow.EquipmentItemClass == ID.Equipment_Item_Class.COMPONENT)))
            {
            if (fsSODetRow.EquipmentLineRef == null)
            {
                if (equipmentRow.CpnyWarrantyEndDate >= docDate
                     || equipmentRow.VendorWarrantyEndDate >= docDate)
                {
                    fsSODetRow.Warranty = true;
                }
            }
            else
            {
                FSEquipmentComponent fsEquipmentComponentRow = PXSelect<FSEquipmentComponent,
                                                               Where<
                                                                    FSEquipmentComponent.SMequipmentID, Equal<Required<FSEquipmentComponent.SMequipmentID>>,
                                                                    And<FSEquipmentComponent.lineNbr, Equal<Required<FSEquipmentComponent.lineNbr>>>>>
                                                               .Select(cache.Graph, fsSODetRow.SMEquipmentID, fsSODetRow.EquipmentLineRef);

                if (fsEquipmentComponentRow.CpnyWarrantyEndDate != null
                    && fsEquipmentComponentRow.CpnyWarrantyEndDate >= docDate)
                {
                    fsSODetRow.Warranty = true;
                }
                else if (fsEquipmentComponentRow.VendorWarrantyEndDate != null
                            && fsEquipmentComponentRow.VendorWarrantyEndDate >= docDate)
                {
                    fsSODetRow.Warranty = true;
                }
                else if (fsEquipmentComponentRow.CpnyWarrantyEndDate == null
                            && fsEquipmentComponentRow.VendorWarrantyEndDate == null
                            && (equipmentRow.CpnyWarrantyEndDate >= docDate
                                    || equipmentRow.VendorWarrantyEndDate >= docDate))
                {
                    fsSODetRow.Warranty = true;
                }
            }
        }
            else if (fSxEquipmentModelRow.EquipmentItemClass == ID.Equipment_Item_Class.PART_OTHER_INVENTORY)
            {
                if (equipmentRow.CpnyWarrantyEndDate >= docDate
                         || equipmentRow.VendorWarrantyEndDate >= docDate)
                {
                    fsSODetRow.Warranty = true;
                }
            }
        }

        public static bool AccountIsAProspect(PXGraph graph, int? bAccountID)
        {
            BAccount bAccountRow = PXSelect<BAccount,
                                       Where<
                                             BAccount.bAccountID, Equal<Required<BAccount.bAccountID>>>>
                                       .Select(graph, bAccountID);

            return bAccountRow != null && bAccountRow.Type == BAccountType.ProspectType;
        }

        public static bool CustomerHasBillingCycle(PXGraph graph, FSSetup setupRecordRow, int? customerID, string srvOrdType)
        {
            Customer customerRow = PXSelect<Customer,
                                        Where<Customer.bAccountID, Equal<Required<Customer.bAccountID>>>>
                                   .Select(graph, customerID);

            if (customerRow != null)
            {
                FSxCustomer fsxCustomerRow = PXCache<Customer>.GetExtension<FSxCustomer>(customerRow);
                if (setupRecordRow != null
                                   && setupRecordRow.CustomerMultipleBillingOptions == true)
                {
                    var multipleBillingOptions = PXSelect<FSCustomerBillingSetup,
                                                    Where<FSCustomerBillingSetup.customerID, Equal<Required<FSCustomerBillingSetup.customerID>>,
                                                        And<FSCustomerBillingSetup.srvOrdType, Equal<Required<FSCustomerBillingSetup.srvOrdType>>>>>
                                                    .Select(graph, customerID, srvOrdType);

                    return multipleBillingOptions.Count() > 0;
                }
                else
                {
                    return fsxCustomerRow.BillingCycleID != null;
                }
            }

            return false;
        }

        public static void ValidateCustomerBillingCycle(PXCache cache, FSSetup setupRecordRow, FSSrvOrdType fsSrvOrdTypeRow, FSServiceOrder fsServiceOrderRow)
        {
            if (fsServiceOrderRow == null
                    || fsSrvOrdTypeRow == null
                        || fsSrvOrdTypeRow.Behavior == ID.Behavior_SrvOrderType.QUOTE
                            || fsSrvOrdTypeRow.Behavior == ID.Behavior_SrvOrderType.INTERNAL_APPOINTMENT)
            {
                return;
            }

            if (AccountIsAProspect(cache.Graph, fsServiceOrderRow.CustomerID) == false
                    && CustomerHasBillingCycle(cache.Graph, setupRecordRow, fsServiceOrderRow.CustomerID, fsServiceOrderRow.SrvOrdType) == false)
            {
                cache.RaiseExceptionHandling<FSServiceOrder.customerID>(fsServiceOrderRow, fsServiceOrderRow.CustomerID, new PXSetPropertyException(TX.Error.MISSING_CUSTOMER_BILLING_CYCLE, PXErrorLevel.RowError));
            }
        }

        public static void CreatePrepayment(FSServiceOrder fsServiceOrderRow, FSAppointment fsAppointmentRow, out PXGraph target, string paymentType = ARPaymentType.Payment)
        {
            ARPaymentEntry graphARPaymentEntry = PXGraph.CreateInstance<ARPaymentEntry>();
            target = graphARPaymentEntry;

            graphARPaymentEntry.Clear();

            ARPayment arPaymentRow = new ARPayment()
            {
                DocType = paymentType,
            };

            AROpenPeriodAttribute.SetThrowErrorExternal<ARPayment.adjFinPeriodID>(graphARPaymentEntry.Document.Cache, true);
            arPaymentRow = PXCache<ARPayment>.CreateCopy(graphARPaymentEntry.Document.Insert(arPaymentRow));
            AROpenPeriodAttribute.SetThrowErrorExternal<ARPayment.adjFinPeriodID>(graphARPaymentEntry.Document.Cache, false);

            arPaymentRow.CustomerID = fsServiceOrderRow.BillCustomerID;
            arPaymentRow.CustomerLocationID = fsServiceOrderRow.BillLocationID;

            decimal CuryDocTotal;

            if (string.Equals(fsServiceOrderRow.CuryID, arPaymentRow.CuryID))
            {
                CuryDocTotal = fsServiceOrderRow.CuryDocTotal ?? 0m;
            }
            else
            {
                CurrencyInfo so_info = PXSelect<CurrencyInfo, Where<CurrencyInfo.curyInfoID, Equal<Required<ARPayment.curyInfoID>>>>.Select(graphARPaymentEntry, arPaymentRow.CuryInfoID);

                if (graphARPaymentEntry.FindImplementation<IPXCurrencyHelper>() != null)
                {
                    graphARPaymentEntry.FindImplementation<IPXCurrencyHelper>().CuryConvCury((decimal)fsServiceOrderRow.DocTotal, out CuryDocTotal);
                }
                else
                {
                    CM.PXDBCurrencyAttribute.CuryConvCury(graphARPaymentEntry.Document.Cache, so_info, (decimal)fsServiceOrderRow.DocTotal, out CuryDocTotal);
                }
            }

            arPaymentRow.CuryOrigDocAmt = CuryDocTotal;

            arPaymentRow.ExtRefNbr = fsServiceOrderRow.CustWorkOrderRefNbr;
            arPaymentRow.DocDesc = fsServiceOrderRow.DocDesc;
            arPaymentRow = graphARPaymentEntry.Document.Update(arPaymentRow);

            InsertSOAdjustments(fsServiceOrderRow, fsAppointmentRow, graphARPaymentEntry, arPaymentRow);
        }

        public static void InsertSOAdjustments(FSServiceOrder fsServiceOrderRow, FSAppointment fSAppointmentRow, ARPaymentEntry arPaymentGraph, ARPayment arPaymentRow)
        {
            FSAdjust fsAdjustRow = new FSAdjust()
            {
                AdjdOrderType = fsServiceOrderRow.SrvOrdType,
                AdjdOrderNbr = fsServiceOrderRow.RefNbr,
                AdjdAppRefNbr = fSAppointmentRow != null ? fSAppointmentRow.RefNbr : null,
                SOCuryCompletedBillableTotal = fsServiceOrderRow.SOCuryCompletedBillableTotal
            };

            SM_ARPaymentEntry sm_ARPaymentEntry = arPaymentGraph.GetExtension<SM_ARPaymentEntry>();

            try
            {
                sm_ARPaymentEntry.FSAdjustments.Insert(fsAdjustRow);
            }
            catch (PXSetPropertyException)
            {
                arPaymentRow.CuryOrigDocAmt = 0m;
            }
        }

        public static void RecalcSOApplAmounts(PXGraph graph, ARPayment row)
        {
            if(row.SOApplAmt == null
                && row.CurySOApplAmt == null)
            {
                row.SOApplAmt = 0;
                row.CurySOApplAmt = 0;

                SOAdjust other = PXSelectGroupBy<SOAdjust,
                                    Where<SOAdjust.adjgDocType, Equal<Required<SOAdjust.adjgDocType>>,
                                        And<SOAdjust.adjgRefNbr, Equal<Required<SOAdjust.adjgRefNbr>>>>,
                                    Aggregate<
                                        GroupBy<SOAdjust.adjgDocType,
                                        GroupBy<SOAdjust.adjgRefNbr,
                                        Sum<SOAdjust.curyAdjgAmt,
                                        Sum<SOAdjust.adjAmt>>>>>>.Select(graph, row.DocType, row.RefNbr);

                if (other != null && other.AdjdOrderNbr != null)
                {
                    row.SOApplAmt += other.AdjAmt;
                    row.CurySOApplAmt += other.CuryAdjgAmt;
                }
            }

            if (row.ApplAmt == null
                && row.CuryApplAmt == null)
            {
                row.ApplAmt = 0;
                row.CuryApplAmt = 0;

                ARAdjust fromar = PXSelectGroupBy<ARAdjust, 
                                    Where<ARAdjust.adjgDocType, Equal<Required<ARAdjust.adjgDocType>>, 
                                        And<ARAdjust.adjgRefNbr, Equal<Required<ARAdjust.adjgRefNbr>>, 
                                        And<ARAdjust.released, Equal<False>>>>, 
                                    Aggregate<
                                        GroupBy<ARAdjust.adjgDocType, 
                                        GroupBy<ARAdjust.adjgRefNbr, 
                                        Sum<ARAdjust.curyAdjgAmt, 
                                        Sum<ARAdjust.adjAmt>>>>>>.Select(graph, row.DocType, row.RefNbr);

                if (fromar != null && fromar.AdjdRefNbr != null)
                {
                    row.ApplAmt += fromar.AdjAmt;
                    row.CuryApplAmt += fromar.CuryAdjgAmt;
                }
            }
        }

        public static decimal? GetServiceOrderBillableTotal(PXGraph graph, string srvOrdType, string refNbr)
        {
            if (String.IsNullOrEmpty(srvOrdType) == true 
                || String.IsNullOrEmpty(refNbr) == true)
            { 
                return null;
            }

            FSServiceOrder fsServiceOrderRow = null;

            fsServiceOrderRow = PXSelect<FSServiceOrder,
                                        Where<
                                            FSServiceOrder.srvOrdType, Equal<Required<FSServiceOrder.srvOrdType>>,
                                            And<FSServiceOrder.refNbr, Equal<Required<FSServiceOrder.refNbr>>>>>
                                    .Select(graph, srvOrdType, refNbr);

            if (fsServiceOrderRow == null)
            {
                return 0;
            }
            
            UpdateServiceOrderUnboundFields(graph, fsServiceOrderRow, null, null, null, false, false);

            return fsServiceOrderRow.SOCuryCompletedBillableTotal ?? 0;
        }

        public static void HidePrepayments(PXView fsAdjustmentsView, PXCache cache, FSServiceOrder fsServiceOrderRow, FSAppointment fsAppointmentRow, FSSrvOrdType fsSrvOrdTypeRow)
        {
            if (fsServiceOrderRow == null 
                || fsSrvOrdTypeRow == null)
            { 
                return;
            }

            bool showPrepayment = fsSrvOrdTypeRow.PostTo == ID.SrvOrdType_PostTo.SALES_ORDER_MODULE 
                                    || fsSrvOrdTypeRow.PostTo == ID.SrvOrdType_PostTo.SALES_ORDER_INVOICE;

            if (fsServiceOrderRow != null)
            {
                fsServiceOrderRow.IsPrepaymentEnable = showPrepayment;
            }

            if (fsAppointmentRow != null)
            {
                fsAppointmentRow.IsPrepaymentEnable = showPrepayment;
            }

            fsAdjustmentsView.AllowSelect = showPrepayment;

            PXUIFieldAttribute.SetVisible<FSServiceOrder.sOPrepaymentApplied>(cache, fsServiceOrderRow, showPrepayment);
            PXUIFieldAttribute.SetVisible<FSServiceOrder.sOPrepaymentReceived>(cache, fsServiceOrderRow, showPrepayment);
            PXUIFieldAttribute.SetVisible<FSServiceOrder.sOPrepaymentRemaining>(cache, fsServiceOrderRow, showPrepayment);
            PXUIFieldAttribute.SetVisible<FSServiceOrder.sOCuryUnpaidBalanace>(cache, fsServiceOrderRow, showPrepayment);
            PXUIFieldAttribute.SetVisible<FSServiceOrder.sOCuryBillableUnpaidBalanace>(cache, fsServiceOrderRow, showPrepayment);
        }

        public static void UpdateServiceOrderUnboundFields(PXGraph graph, FSServiceOrder fsServiceOrderRow, FSBillingCycle fsBillingCycleRow, PXGraph appointmentGraph, FSAppointment fsAppointmentRow, bool disableServiceOrderUnboundFieldCalc, bool calcPrepaymentAmount = true)
        {
            if (fsServiceOrderRow == null || disableServiceOrderUnboundFieldCalc == true)
                return;

            if (fsBillingCycleRow == null
                && fsServiceOrderRow != null
                    && fsServiceOrderRow.SOID != null)
            {
                fsBillingCycleRow = (FSBillingCycle)PXSelectJoin<FSBillingCycle,
                                                        InnerJoin<FSCustomerBillingSetup,
                                                            On<FSBillingCycle.billingCycleID, Equal<FSCustomerBillingSetup.billingCycleID>>>,
                                                        Where<FSCustomerBillingSetup.cBID, Equal<Required<FSServiceOrder.cBID>>>>
                                                        .Select(graph, fsServiceOrderRow.CBID);
            }

            if (fsBillingCycleRow == null)
                return;

            String billingBy = fsBillingCycleRow.BillingBy;

            if (fsServiceOrderRow != null)
            {
                if (billingBy == ID.Billing_By.APPOINTMENT)
                {
                    FSAppointment auxFSAppointmentRow = null;
                    decimal? docTotal = 0m;

                    if (fsAppointmentRow != null && fsAppointmentRow.AppointmentID > 0)
                    {
                        auxFSAppointmentRow = PXSelectGroupBy<FSAppointment,
                                            Where<
                                                FSAppointment.sOID, Equal<Required<FSAppointment.sOID>>,
                                            And<
                                                FSAppointment.appointmentID, NotEqual<Required<FSAppointment.appointmentID>>,
                                            And<
                                                Where<FSAppointment.status, Equal<FSAppointment.status.Completed>,
                                                    Or<FSAppointment.status, Equal<FSAppointment.status.Closed>>>>>>,
                                            Aggregate<
                                                GroupBy<FSAppointment.sOID,
                                                Sum<FSAppointment.curyDocTotal>>>>.Select(appointmentGraph ?? graph, fsServiceOrderRow.SOID, fsAppointmentRow.AppointmentID);

                        if (auxFSAppointmentRow != null)
                        { 
                            docTotal = auxFSAppointmentRow.CuryDocTotal ?? 0m;
                        }

                        if (fsAppointmentRow.Status == ID.Status_Appointment.COMPLETED
                                    || fsAppointmentRow.Status == ID.Status_Appointment.CLOSED)
                        {
                            if (auxFSAppointmentRow != null && auxFSAppointmentRow.SOID != null)
                            {
                                docTotal += fsAppointmentRow.CuryDocTotal != null ? fsAppointmentRow.CuryDocTotal : 0m;
                            }
                            else
                            {
                                docTotal = fsAppointmentRow.CuryDocTotal;
                            }

                            fsAppointmentRow.AppCompletedBillableTotal = fsAppointmentRow.CuryDocTotal;
                        }
                    }
                    else
                    {
                        auxFSAppointmentRow = PXSelectGroupBy<FSAppointment,
                                            Where<
                                                FSAppointment.sOID, Equal<Required<FSAppointment.sOID>>,
                                            And<
                                                Where<FSAppointment.status, Equal<FSAppointment.status.Completed>,
                                                    Or<FSAppointment.status, Equal<FSAppointment.status.Closed>>>>>,
                                            Aggregate<
                                                Sum<FSAppointment.curyDocTotal>>>.Select(appointmentGraph ?? graph, fsServiceOrderRow.SOID);

                        if (auxFSAppointmentRow != null && auxFSAppointmentRow.SOID != null)
                        {
                            docTotal = auxFSAppointmentRow.CuryDocTotal ?? 0m;
                        }
                    }

                    fsServiceOrderRow.SOCuryCompletedBillableTotal = docTotal;
                }
                else if (billingBy == ID.Billing_By.SERVICE_ORDER)
                {
                    fsServiceOrderRow.SOCuryCompletedBillableTotal = fsServiceOrderRow.CuryDocTotal;

                    if (fsAppointmentRow != null)
                    {
                        fsAppointmentRow.AppCompletedBillableTotal = 0;
                    }
                }
                else
                {
                    fsServiceOrderRow.SOCuryCompletedBillableTotal = 0;
                }
            }

            fsServiceOrderRow.SOPrepaymentReceived = 0;
            fsServiceOrderRow.SOPrepaymentRemaining = 0;
            fsServiceOrderRow.SOPrepaymentApplied = 0;
            fsServiceOrderRow.SOCuryUnpaidBalanace = 0;
            fsServiceOrderRow.SOCuryBillableUnpaidBalanace = 0;

            if (calcPrepaymentAmount == true)
            { 
                PXResultset<ARPayment> resultSet = null; 

                resultSet = (PXResultset<ARPayment>) PXSelectJoin<ARPayment,
                                                            InnerJoin<FSAdjust, 
                                                                On<ARPayment.docType, Equal<FSAdjust.adjgDocType>,
                                                                    And<ARPayment.refNbr, Equal<FSAdjust.adjgRefNbr>>>>,
                                                            Where<FSAdjust.adjdOrderType, Equal<Required<FSServiceOrder.srvOrdType>>,
                                                                And<FSAdjust.adjdOrderNbr, Equal<Required<FSServiceOrder.refNbr>>>>>
                                                            .Select(graph, fsServiceOrderRow.SrvOrdType, fsServiceOrderRow.RefNbr);

                fsServiceOrderRow.SOCuryUnpaidBalanace = fsServiceOrderRow.CuryDocTotal;
                fsServiceOrderRow.SOCuryBillableUnpaidBalanace = fsServiceOrderRow.SOCuryCompletedBillableTotal;

                foreach (PXResult<ARPayment> row in resultSet)
                {
                    ARPayment arPaymentRow = (ARPayment)row;

                    ServiceOrderCore.RecalcSOApplAmounts(graph, arPaymentRow);

                    fsServiceOrderRow.SOPrepaymentReceived += arPaymentRow.CuryDocBal ?? 0m;
                    fsServiceOrderRow.SOPrepaymentApplied += arPaymentRow.CuryApplAmt + arPaymentRow.CurySOApplAmt;
                    fsServiceOrderRow.SOPrepaymentRemaining += (arPaymentRow.CuryDocBal ?? 0m) - (arPaymentRow.CuryApplAmt + arPaymentRow.CurySOApplAmt);

                    fsServiceOrderRow.SOCuryUnpaidBalanace -= arPaymentRow.CuryDocBal ?? 0m;
                    fsServiceOrderRow.SOCuryBillableUnpaidBalanace -= arPaymentRow.CuryDocBal ?? 0m;
                }
            }
        }
        #endregion
    }
}
