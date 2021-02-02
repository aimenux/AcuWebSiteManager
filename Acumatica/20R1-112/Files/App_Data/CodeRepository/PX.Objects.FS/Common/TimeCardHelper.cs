using PX.Data;
using PX.Objects.CR;
using PX.Objects.CS;
using PX.Objects.EP;
using System;
using System.Collections.Generic;
using System.Linq;
using TMEPEmployee = PX.TM.PXOwnerSelectorAttribute.EPEmployee;

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

        public static void InsertUpdateDeleteTimeActivities(AppointmentEntry graphAppointmentEntry,
                                                            PXCache appointmentCache,
                                                            FSAppointment fsAppointmentRow,
                                                            FSServiceOrder fsServiceOrderRow,
                                                            AppointmentCore.AppointmentLog_View appointmentEmployeeLogs,
                                                            List<FSAppointmentLog> deleteReleatedTimeActivity,
                                                            List<FSAppointmentLog> createReleatedTimeActivity)
        {
            if (!TimeCardHelper.IsTheTimeCardIntegrationEnabled(graphAppointmentEntry)
                    || (fsAppointmentRow.Status != ID.Status_Appointment.COMPLETED
                            && fsAppointmentRow.Status != ID.Status_Appointment.MANUAL_SCHEDULED))
            {
                return;
            }

            if (PXAccess.FeatureInstalled<FeaturesSet.timeReportingModule>() == false)
            {
                return;
            }

            EmployeeActivitiesEntry graphEmployeeActivitiesEntry = PXGraph.CreateInstance<EmployeeActivitiesEntry>();

            if (IsNecessaryToUpdateTimeCards(appointmentCache, appointmentEmployeeLogs, fsAppointmentRow))
            {
                foreach (FSAppointmentLog fsAppointmentLogRow in appointmentEmployeeLogs.Select().RowCast<FSAppointmentLog>().Where(row => row.BAccountType == BAccountType.EmployeeType))
                {
                    EPActivityApprove epActivityApproveRow = null;
                    EPActivityApprove oldEPActivityApproveRow = null;
                    TMEPEmployee epEmployeeRow = null;

                    FindTMEmployee(graphAppointmentEntry, fsAppointmentLogRow.BAccountID, ref epEmployeeRow);
                    FindEPActivityApprove(graphAppointmentEntry, fsAppointmentLogRow, epEmployeeRow, ref epActivityApproveRow);

                    if (fsAppointmentLogRow.TrackTime == true)
                    {
                        if (fsAppointmentLogRow.BAccountID != null)
                        {
                            TMEPEmployee oldEPEmployeeRow = null;
                            FindTMEmployee(graphAppointmentEntry, fsAppointmentLogRow.BAccountID, ref oldEPEmployeeRow);
                            FindEPActivityApprove(graphAppointmentEntry, fsAppointmentLogRow, oldEPEmployeeRow, ref oldEPActivityApproveRow);
                            DeleteEPActivityApprove(graphEmployeeActivitiesEntry, oldEPActivityApproveRow);
                        }

                        InsertUpdateEPActivityApprove(graphAppointmentEntry, graphEmployeeActivitiesEntry, fsAppointmentLogRow, fsAppointmentRow, fsServiceOrderRow, epActivityApproveRow, epEmployeeRow);
                    }
                    else
                    {
                        if (epActivityApproveRow != null)
                        {
                            DeleteEPActivityApprove(graphEmployeeActivitiesEntry, epActivityApproveRow);
                        }
                    }
                }

                foreach (FSAppointmentLog fsAppointmentLogRow in appointmentEmployeeLogs.Cache.Deleted.RowCast<FSAppointmentLog>().Where(row => row.BAccountType == BAccountType.EmployeeType))
                {
                    SearchAndDeleteEPActivity(graphAppointmentEntry, fsAppointmentLogRow, graphEmployeeActivitiesEntry);
                }
            }
            else if ((string)appointmentCache.GetValueOriginal<FSAppointment.status>(fsAppointmentRow) == ID.Status_Appointment.COMPLETED
                        && fsAppointmentRow.Status == ID.Status_Appointment.MANUAL_SCHEDULED)
            {
                foreach (FSAppointmentLog fsAppointmentLogRow in appointmentEmployeeLogs.Select().RowCast<FSAppointmentLog>().Where(row => row.BAccountType == BAccountType.EmployeeType))
                {
                    SearchAndDeleteEPActivity(graphAppointmentEntry, fsAppointmentLogRow, graphEmployeeActivitiesEntry);
                }

                foreach (FSAppointmentLog fsAppointmentLogRow in appointmentEmployeeLogs.Cache.Deleted.RowCast<FSAppointmentLog>().Where(row => row.BAccountType == BAccountType.EmployeeType))
                {
                    SearchAndDeleteEPActivity(graphAppointmentEntry, fsAppointmentLogRow, graphEmployeeActivitiesEntry);
                }
            }

            if (deleteReleatedTimeActivity != null)
            {
                //Deleting time activities related with canceled service lines
                foreach (FSAppointmentLog fsAppointmentLogRow in deleteReleatedTimeActivity)
                {
                    if (fsAppointmentLogRow.BAccountType == BAccountType.EmployeeType)
                    {
                        SearchAndDeleteEPActivity(graphAppointmentEntry, fsAppointmentLogRow, graphEmployeeActivitiesEntry);
                    }
                }
            }

            if (createReleatedTimeActivity != null)
            {
                //Creating time activities related with re-opened service lines
                foreach (FSAppointmentLog fsAppointmentLogRow in createReleatedTimeActivity)
                {
                    if (fsAppointmentLogRow.BAccountType == BAccountType.EmployeeType)
                    {
                        TMEPEmployee epEmployeeRow = null;
                        FindTMEmployee(graphAppointmentEntry, fsAppointmentLogRow.BAccountID, ref epEmployeeRow);
                        InsertUpdateEPActivityApprove(graphAppointmentEntry, graphEmployeeActivitiesEntry, fsAppointmentLogRow, fsAppointmentRow, fsServiceOrderRow, null, epEmployeeRow);
                    }
                }
            }

            if (graphEmployeeActivitiesEntry.IsDirty == true)
            {
                graphEmployeeActivitiesEntry.Save.Press();
            }
        }

        private static void SearchAndDeleteEPActivity(AppointmentEntry graphAppointmentEntry, 
                                                      FSAppointmentLog fsAppointmentLogRow,
                                                      EmployeeActivitiesEntry graphEmployeeActivitiesEntry)
        {
            EPActivityApprove epActivityApproveRow = null;
            TMEPEmployee epEmployeeRow = null;

            FindTMEmployee(graphAppointmentEntry, fsAppointmentLogRow.BAccountID, ref epEmployeeRow);
            FindEPActivityApprove(graphAppointmentEntry, fsAppointmentLogRow, epEmployeeRow, ref epActivityApproveRow);

            if (epActivityApproveRow != null)
            {
                DeleteEPActivityApprove(graphEmployeeActivitiesEntry, epActivityApproveRow);
            }
        }

        private static void DeleteEPActivityApprove(EmployeeActivitiesEntry graphEmployeeActivitiesEntry,
                                                    EPActivityApprove epActivityApproveRow)
        {
            if (epActivityApproveRow != null)
            {
                if (epActivityApproveRow.ApprovalStatus == ActivityStatusListAttribute.Approved 
                    || epActivityApproveRow.ApprovalStatus == ActivityStatusListAttribute.Released 
                    || epActivityApproveRow.TimeCardCD != null)
                {
                    return;
                }

                graphEmployeeActivitiesEntry.Activity.Delete(epActivityApproveRow);
            }
        }

        public static void FindTMEmployee(
            PXGraph graph,
            int? employeeID,
            ref TMEPEmployee epEmployeeRow)
        {
            epEmployeeRow = PXSelect<TMEPEmployee,
                            Where<
                                TMEPEmployee.bAccountID, Equal<Required<TMEPEmployee.bAccountID>>>>
                            .Select(graph, employeeID);

            if (epEmployeeRow == null)
            {
                throw new Exception(TX.Error.MISSING_LINK_ENTITY_STAFF_MEMBER);
            }
        }

        public static void FindEPActivityApprove(PXGraph graph,
                                                 FSAppointmentLog fsAppointmentLogRow,
            TMEPEmployee epEmployeeRow,
            ref EPActivityApprove epActivityApproveRow)
        {
            epActivityApproveRow = PXSelect<EPActivityApprove,
                                   Where<
                                       EPActivityApprove.ownerID, Equal<Required<EPActivityApprove.ownerID>>,
                                       And<FSxPMTimeActivity.appointmentID, Equal<Required<FSxPMTimeActivity.appointmentID>>,
                                       And<FSxPMTimeActivity.logLineNbr, Equal<Required<FSxPMTimeActivity.logLineNbr>>>>>>
                                   .Select(graph, epEmployeeRow.PKID, fsAppointmentLogRow.DocID, fsAppointmentLogRow.LineNbr);
        }

        public static void InsertUpdateEPActivityApprove(PXGraph graph,
                                                         EmployeeActivitiesEntry graphEmployeeActivitiesEntry,
                                                         FSAppointmentLog fsAppointmentLogRow,
                                                         FSAppointment fsAppointmentRow,
                                                         FSServiceOrder fsServiceOrderRow,
                                                         EPActivityApprove epActivityApproveRow,
                                                         TMEPEmployee epEmployeeRow)
        {
            if (epActivityApproveRow != null &&
                    (epActivityApproveRow.ApprovalStatus == ActivityStatusListAttribute.Approved || epActivityApproveRow.TimeCardCD != null))
            {
                return;
            }

            FSAppointmentDet fsAppointmentDetServiceRow =
                                        PXSelect<FSAppointmentDet,
                                        Where<
                                            FSAppointmentDet.lineRef, Equal<Required<FSAppointmentDet.lineRef>>,
                                            And<FSAppointmentDet.appointmentID, Equal<Required<FSAppointmentDet.appointmentID>>>>>
                                        .Select(graph, fsAppointmentLogRow.DetLineRef, fsAppointmentRow.AppointmentID);

            if (fsAppointmentDetServiceRow != null && 
                    fsAppointmentDetServiceRow.IsCanceledNotPerformed == true)
            {
                return;
            }

            if (epActivityApproveRow == null)
            {
                epActivityApproveRow = new EPActivityApprove();
                epActivityApproveRow.OwnerID = epEmployeeRow.PKID;
                epActivityApproveRow = graphEmployeeActivitiesEntry.Activity.Insert(epActivityApproveRow);
            }

            graphEmployeeActivitiesEntry.Activity.SetValueExt<EPActivityApprove.hold>(epActivityApproveRow, false);
            epActivityApproveRow.Date = PXDBDateAndTimeAttribute.CombineDateTime(fsAppointmentRow.ExecutionDate, fsAppointmentLogRow.DateTimeBegin);
            epActivityApproveRow.EarningTypeID = fsAppointmentLogRow.EarningType;
            epActivityApproveRow.TimeSpent = fsAppointmentLogRow.TimeDuration;
            epActivityApproveRow.Summary = GetDescriptionToUseInEPActivityApprove(fsAppointmentRow, fsAppointmentLogRow, fsAppointmentDetServiceRow);
            epActivityApproveRow.CostCodeID = fsAppointmentLogRow?.CostCodeID;

            FSxPMTimeActivity fsxPMTimeActivityRow = PXCache<PMTimeActivity>.GetExtension<FSxPMTimeActivity>((PMTimeActivity)epActivityApproveRow);

            fsxPMTimeActivityRow.SOID = fsAppointmentRow.SOID;
            fsxPMTimeActivityRow.AppointmentID = fsAppointmentRow.AppointmentID;
            fsxPMTimeActivityRow.AppointmentCustomerID = fsServiceOrderRow.CustomerID;
            fsxPMTimeActivityRow.LogLineNbr = fsAppointmentLogRow.LineNbr;

            if (fsAppointmentLogRow.DetLineRef != null)
            {
                fsxPMTimeActivityRow.ServiceID = fsAppointmentDetServiceRow == null ? null : fsAppointmentDetServiceRow.InventoryID;
            }

            epActivityApproveRow = graphEmployeeActivitiesEntry.Activity.Update(epActivityApproveRow);

            graphEmployeeActivitiesEntry.Activity.SetValueExt<EPActivityApprove.projectID>(epActivityApproveRow, fsServiceOrderRow.ProjectID);
            graphEmployeeActivitiesEntry.Activity.SetValueExt<EPActivityApprove.projectTaskID>(epActivityApproveRow, GetProjectTaskIDToUseInEPActivityApprove(fsAppointmentRow, fsAppointmentLogRow, fsAppointmentDetServiceRow));
            graphEmployeeActivitiesEntry.Activity.SetValueExt<EPActivityApprove.isBillable>(epActivityApproveRow, fsAppointmentLogRow.IsBillable);
            graphEmployeeActivitiesEntry.Activity.SetValueExt<EPActivityApprove.timeBillable>(epActivityApproveRow, fsAppointmentLogRow.BillableTimeDuration);
            graphEmployeeActivitiesEntry.Activity.SetValueExt<EPActivityApprove.approvalStatus>(epActivityApproveRow, GetStatusToUseInEPActivityApprove());
            graphEmployeeActivitiesEntry.Activity.SetValueExt<EPActivityApprove.labourItemID>(epActivityApproveRow, fsAppointmentLogRow.LaborItemID);
        }

        private static string GetStatusToUseInEPActivityApprove()
        {
            return ActivityStatusListAttribute.Completed;
        }

        private static object GetProjectTaskIDToUseInEPActivityApprove(FSAppointment fsAppointmentRow,
                                                                       FSAppointmentLog fsAppointmentLogRow,
                                                                       FSAppointmentDet fsAppointmentDetRow)
        {
            if (fsAppointmentLogRow != null && fsAppointmentLogRow.DetLineRef != null && fsAppointmentDetRow != null)
            {
                return fsAppointmentDetRow.ProjectTaskID;
            }
            else
            {
                return fsAppointmentRow.DfltProjectTaskID;
            }
        }

        private static string GetDescriptionToUseInEPActivityApprove(FSAppointment fsAppointmentRow,
                                                                     FSAppointmentLog fsAppointmentLogRow,
                                                                     FSAppointmentDet fsAppointmentDetRow)
        {
            if(fsAppointmentLogRow != null)
            {
                if(fsAppointmentLogRow.Type == ID.Type_Log.TRAVEL)
                {
                    return fsAppointmentLogRow.Descr;
                }
                else if(fsAppointmentLogRow.DetLineRef != null &&
                        fsAppointmentDetRow != null)
                {
                    return fsAppointmentDetRow.TranDesc;
                }
            }

            return fsAppointmentRow.DocDesc;
        }

        private static bool IsNecessaryToUpdateTimeCards(PXCache appointmentCache,
                                                         AppointmentCore.AppointmentLog_View appointmentEmployeeLogs, 
                                                         FSAppointment fsAppointmentRow)
        {
            DateTime? oldActualDateTimeBegin = (DateTime?)appointmentCache.GetValueOriginal<FSAppointment.actualDateTimeBegin>(fsAppointmentRow);
            DateTime? oldActualDateTimeEnd = (DateTime?)appointmentCache.GetValueOriginal<FSAppointment.actualDateTimeEnd>(fsAppointmentRow);
            DateTime? oldExecutionDate = (DateTime?)appointmentCache.GetValueOriginal<FSAppointment.executionDate>(fsAppointmentRow);
            string oldDocDesc = (string)appointmentCache.GetValueOriginal<FSAppointment.docDesc>(fsAppointmentRow);
            bool appointmentModified = false;

            if (fsAppointmentRow.ActualDateTimeBegin != oldActualDateTimeBegin
                || fsAppointmentRow.ActualDateTimeEnd != oldActualDateTimeEnd
                || fsAppointmentRow.ExecutionDate != oldExecutionDate
                || fsAppointmentRow.DocDesc != oldDocDesc)
            {
                appointmentModified = true;
            }

            if (fsAppointmentRow.Status == ID.Status_Appointment.COMPLETED
                && (
                    ((string)appointmentCache.GetValueOriginal<FSAppointment.status>(fsAppointmentRow) != ID.Status_Appointment.COMPLETED
                        && (string)appointmentCache.GetValueOriginal<FSAppointment.status>(fsAppointmentRow) != ID.Status_Appointment.CLOSED)
                    || appointmentEmployeeLogs.Cache.IsInsertedUpdatedDeleted
                    || appointmentModified))
            {
                return true;
            }

            return false;
        }
    }
}
