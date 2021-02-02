using PX.Data;
using PX.Objects.CR;
using PX.Objects.EP;
using System.Linq;
using System;

namespace PX.Objects.FS
{
    public static class TimeCardHelper
    {
        public static bool IsAccessedFromAppointment(string screenID)
        {
            string[] logActionsMobileScreens = { 
                ID.ScreenID.LOG_ACTION_START_TRAVEL_SRV_MOBILE,
                ID.ScreenID.LOG_ACTION_START_STAFF_MOBILE,
                ID.ScreenID.LOG_ACTION_START_STAFF_ASSIGNED_MOBILE,
                ID.ScreenID.LOG_ACTION_COMPLETE_SERVICE_MOBILE,
                ID.ScreenID.LOG_ACTION_COMPLETE_TRAVEL_MOBILE
            };

            return screenID == SharedFunctions.SetScreenIDToDotFormat(ID.ScreenID.APPOINTMENT)
                    || Array.Exists(logActionsMobileScreens, x => SharedFunctions.SetScreenIDToDotFormat(x) == screenID);
        }

        public static bool CanCurrentUserEnterTimeCards(PXGraph graph, string callerScreenID)
        {
            if (callerScreenID != SharedFunctions.SetScreenIDToDotFormat(ID.ScreenID.APPOINTMENT)
                && callerScreenID != SharedFunctions.SetScreenIDToDotFormat(ID.ScreenID.APPOINTMENT_INQUIRY))
            {
                return true;
            }

            EPEmployee employeeByUserID = PXSelect<EPEmployee, Where<EPEmployee.userID, Equal<Current<AccessInfo.userID>>>>.Select(graph);
            if (employeeByUserID == null)
            {
                return false;
            }

            return true;
        }

        public static void PMTimeActivity_RowPersisting_Handler(PXCache cache, PXGraph graph, PMTimeActivity pmTimeActivityRow, PXRowPersistingEventArgs e)
        {
            FSxPMTimeActivity fsxPMTimeActivityRow = PXCache<PMTimeActivity>.GetExtension<FSxPMTimeActivity>(pmTimeActivityRow);

            if (e.Operation == PXDBOperation.Delete
                    && graph.Accessinfo.ScreenID != SharedFunctions.SetScreenIDToDotFormat(ID.ScreenID.APPOINTMENT))
            {
                if (fsxPMTimeActivityRow.AppointmentID != null
                        && fsxPMTimeActivityRow.LogLineNbr != null)
                {
                    PXUpdate<
                        Set<FSAppointmentLog.trackTime, False>,
                    FSAppointmentLog,
                    Where<
                        FSAppointmentLog.docID, Equal<Required<FSAppointmentLog.docID>>,
                        And<FSAppointmentLog.lineNbr, Equal<Required<FSAppointmentLog.lineNbr>>>>>
                    .Update(graph, fsxPMTimeActivityRow.AppointmentID, fsxPMTimeActivityRow.LogLineNbr);
                }
            }

            if ((e.Operation == PXDBOperation.Insert || e.Operation == PXDBOperation.Update)
                    && !IsAccessedFromAppointment(graph.Accessinfo.ScreenID))
            {
                if (fsxPMTimeActivityRow.AppointmentID != null
                        && fsxPMTimeActivityRow.LogLineNbr != null
                                && (
                                    (int?)cache.GetValueOriginal<EPActivityApprove.timeSpent>(pmTimeActivityRow) != pmTimeActivityRow.TimeSpent
                                        || (int?)cache.GetValueOriginal<EPActivityApprove.timeBillable>(pmTimeActivityRow) != pmTimeActivityRow.TimeBillable
                                            || (bool?)cache.GetValueOriginal<EPActivityApprove.isBillable>(pmTimeActivityRow) != pmTimeActivityRow.IsBillable))
                {
                    AppointmentEntry graphAppointmentEntry = PXGraph.CreateInstance<AppointmentEntry>();

                    FSAppointment fsAppointmentRow = PXSelect<FSAppointment,
                                                     Where<
                                                         FSAppointment.appointmentID, Equal<Required<FSAppointment.appointmentID>>>>
                                                     .Select(graph, fsxPMTimeActivityRow.AppointmentID);

                    FSAppointmentLog fsAppointmentLogRow = PXSelect<FSAppointmentLog,
                                                                     Where<
                                                                         FSAppointmentLog.docID, Equal<Required<FSAppointmentLog.docID>>,
                                                                         And<FSAppointmentLog.lineNbr, Equal<Required<FSAppointmentLog.lineNbr>>>>>
                                                                     .Select(graph, fsxPMTimeActivityRow.AppointmentID, fsxPMTimeActivityRow.LogLineNbr);

                    fsAppointmentRow = graphAppointmentEntry.AppointmentRecords.Current = graphAppointmentEntry.AppointmentRecords.Search<FSAppointment.appointmentID>
                                                            (fsAppointmentRow.AppointmentID, fsAppointmentRow.SrvOrdType);

                    graphAppointmentEntry.SkipTimeCardUpdate = true;
                    fsAppointmentLogRow.TimeDuration = pmTimeActivityRow.TimeSpent;
                    fsAppointmentLogRow.EarningType = pmTimeActivityRow.EarningTypeID;

                    FSSrvOrdType fsSrvOrdTypeRow = graphAppointmentEntry.ServiceOrderTypeSelected.Current;

                    if (fsSrvOrdTypeRow != null
                            && fsSrvOrdTypeRow.PostTo == ID.SrvOrdType_PostTo.PROJECTS
                            && fsSrvOrdTypeRow.BillingType == ID.SrvOrdType_BillingType.COST_AS_COST
                            && fsSrvOrdTypeRow.CreateTimeActivitiesFromAppointment == true)
                    {
                        fsAppointmentLogRow.IsBillable = pmTimeActivityRow.IsBillable;
                        fsAppointmentLogRow.BillableTimeDuration = pmTimeActivityRow.TimeBillable;
                    }

                    fsAppointmentLogRow = graphAppointmentEntry.LogRecords.Update(fsAppointmentLogRow);
                    graphAppointmentEntry.Save.Press();
                }
            }
        }

        /// <summary>
        /// Checks if the Employee Time Cards Integration is enabled in the Service Management Setup.
        /// </summary>
        public static bool IsTheTimeCardIntegrationEnabled(PXGraph graph)
        {
            FSSetup fsSetupRow = ServiceManagementSetup.GetServiceManagementSetup(graph);
            return fsSetupRow != null && fsSetupRow.EnableEmpTimeCardIntegration == true;
        }

        public static void PMTimeActivity_RowSelected_Handler(PXCache cache, PMTimeActivity pmTimeActivityRow)
        {
            FSxPMTimeActivity fsxPMTimeActivityRow = cache.GetExtension<FSxPMTimeActivity>(pmTimeActivityRow);

            PXUIFieldAttribute.SetEnabled<FSxPMTimeActivity.appointmentID>(cache, pmTimeActivityRow, false);
            PXUIFieldAttribute.SetEnabled<FSxPMTimeActivity.appointmentCustomerID>(cache, pmTimeActivityRow, false);
            PXUIFieldAttribute.SetEnabled<FSxPMTimeActivity.sOID>(cache, pmTimeActivityRow, false);
            PXUIFieldAttribute.SetEnabled<FSxPMTimeActivity.logLineNbr>(cache, pmTimeActivityRow, false);
            PXUIFieldAttribute.SetEnabled<FSxPMTimeActivity.serviceID>(cache, pmTimeActivityRow, false);
        }

        /// <summary>
        /// Checks if the all Appointment Service lines are approved by a Time Card, then sets Time Register to true and completes the appointment.
        /// </summary>
        public static void CheckTimeCardAppointmentApprovalsAndComplete(AppointmentEntry graphAppointmentEntry, PXCache cache, FSAppointment fsAppointmentRow)
        {
            bool allLinesApprovedByTimeCard = true;
            bool isEmployeeLines = false;
            if (fsAppointmentRow.Status == ID.Status_Appointment.COMPLETED)
            {
                var employeesLogs = graphAppointmentEntry.LogRecords.Select().RowCast<FSAppointmentLog>()
                                                                                 .Where(y => y.BAccountType == BAccountType.EmployeeType
                                                                                          && y.TrackTime == true);

                foreach (FSAppointmentLog fsAppointmentLogRow in employeesLogs)
                {
                    isEmployeeLines = true;
                    allLinesApprovedByTimeCard = allLinesApprovedByTimeCard && (bool)fsAppointmentLogRow.ApprovedTime;

                    if (!allLinesApprovedByTimeCard)
                    {
                        break;
                    }
                }

                if (allLinesApprovedByTimeCard && isEmployeeLines)
                {
                    fsAppointmentRow.TimeRegistered = true;
                    fsAppointmentRow.Status = ID.Status_Appointment.COMPLETED;
                }
            }
        }
    }
}
