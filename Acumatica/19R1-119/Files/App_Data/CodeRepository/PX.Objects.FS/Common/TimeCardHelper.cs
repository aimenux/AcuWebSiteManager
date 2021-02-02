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
                        && fsxPMTimeActivityRow.AppEmpID != null)
                {
                    PXUpdate<Set<FSAppointmentEmployee.trackTime, False>,
                                FSAppointmentEmployee,
                                Where<
                                    FSAppointmentEmployee.appointmentID, Equal<Required<FSAppointmentEmployee.appointmentID>>,
                                    And<FSAppointmentEmployee.lineNbr, Equal<Required<FSAppointmentEmployee.lineNbr>>>>>
                                    .Update(graph, fsxPMTimeActivityRow.AppointmentID, fsxPMTimeActivityRow.AppEmpID);
                }
            }

            if ((e.Operation == PXDBOperation.Insert || e.Operation == PXDBOperation.Update)
                    && graph.Accessinfo.ScreenID != SharedFunctions.SetScreenIDToDotFormat(ID.ScreenID.APPOINTMENT))
            {
                if (fsxPMTimeActivityRow.AppointmentID != null
                            && fsxPMTimeActivityRow.AppEmpID != null
                            && fsxPMTimeActivityRow.ServiceID != null
                            && (int?)cache.GetValueOriginal<EPActivityApprove.timeSpent>(pmTimeActivityRow) != pmTimeActivityRow.TimeSpent)
                {
                    AppointmentEntry graphAppointmentEntry = PXGraph.CreateInstance<AppointmentEntry>();

                    FSAppointment fsAppointmentRow = PXSelect<FSAppointment,
                                                        Where<FSAppointment.appointmentID, Equal<Required<FSAppointment.appointmentID>>>>
                                                        .Select(graph, fsxPMTimeActivityRow.AppointmentID);

                    FSAppointmentEmployee fsAppointmentEmployeeRow = PXSelect<FSAppointmentEmployee,
                                                                        Where<
                                                                            FSAppointmentEmployee.appointmentID, Equal<Required<FSAppointmentEmployee.appointmentID>>,
                                                                            And<FSAppointmentEmployee.lineNbr, Equal<Required<FSAppointmentEmployee.lineNbr>>>>>
                                                                        .Select(graph, fsxPMTimeActivityRow.AppointmentID, fsxPMTimeActivityRow.AppEmpID);

                    fsAppointmentRow = graphAppointmentEntry.AppointmentRecords.Current = graphAppointmentEntry.AppointmentRecords.Search<FSAppointment.appointmentID>
                                                            (fsAppointmentRow.AppointmentID, fsAppointmentRow.SrvOrdType);

                    graphAppointmentEntry.SkipTimeCardUpdate = true;
                    fsAppointmentEmployeeRow.ActualDuration = pmTimeActivityRow.TimeSpent;
                    AppointmentDateTimeHelper.UpdateStaffActualDateTimeEndFromActualDuration(fsAppointmentEmployeeRow);
                    fsAppointmentEmployeeRow.EarningType = pmTimeActivityRow.EarningTypeID;
                    fsAppointmentEmployeeRow = graphAppointmentEntry.AppointmentEmployees.Update(fsAppointmentEmployeeRow);
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
            PXUIFieldAttribute.SetEnabled<FSxPMTimeActivity.appEmpID>(cache, pmTimeActivityRow, false);
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
                foreach (FSAppointmentEmployee fsAppointmentEmployeeRow in graphAppointmentEntry.AppointmentEmployees.Select()
                                                                            .Where(y => ((FSAppointmentEmployee)y).Type == BAccountType.EmployeeType
                                                                                    && ((FSAppointmentEmployee)y).TrackTime == true))
                {
                    isEmployeeLines = true;
                    allLinesApprovedByTimeCard = allLinesApprovedByTimeCard && (bool)fsAppointmentEmployeeRow.ApprovedTime;

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

        public static void InsertUpdateDeleteTimeActivities(
                AppointmentEntry graphAppointmentEntry,
                PXCache appointmentCache,
                FSAppointment fsAppointmentRow,
                FSServiceOrder fsServiceOrderRow,
                AppointmentCore.AppointmentEmployees_View appointmentEmployees,
                List<FSAppointmentEmployee> deleteReleatedTimeActivity,
                List<FSAppointmentEmployee> createReleatedTimeActivity)
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

            if (IsNecessaryToUpdateTimeCards(appointmentCache, appointmentEmployees, fsAppointmentRow))
            {
	            foreach (FSAppointmentEmployee fsAppointmentEmployee in appointmentEmployees.Select().RowCast<FSAppointmentEmployee>().Where(row => row.Type == BAccountType.EmployeeType))
				{
                    EPActivityApprove epActivityApproveRow = null;
                    EPActivityApprove oldEPActivityApproveRow = null;
                    TMEPEmployee epEmployeeRow = null;
                    int? oldEmployeeIDValue;

                    FindTMEmployee(graphAppointmentEntry, fsAppointmentEmployee.EmployeeID, ref epEmployeeRow);
                    FindEPActivityApprove(graphAppointmentEntry, fsAppointmentEmployee, epEmployeeRow, ref epActivityApproveRow);

                    if (fsAppointmentEmployee.TrackTime == true)
                    {
                        oldEmployeeIDValue = (int?)graphAppointmentEntry.AppointmentEmployees.Cache.GetValueOriginal<FSAppointmentEmployee.employeeID>(fsAppointmentEmployee);
                        if (oldEmployeeIDValue != fsAppointmentEmployee.EmployeeID)
                        {
                            TMEPEmployee oldEPEmployeeRow = null;
                            FindTMEmployee(graphAppointmentEntry, oldEmployeeIDValue, ref oldEPEmployeeRow);
                            FindEPActivityApprove(graphAppointmentEntry, fsAppointmentEmployee, oldEPEmployeeRow, ref oldEPActivityApproveRow);
                            DeleteEPActivityApprove(graphEmployeeActivitiesEntry, oldEPActivityApproveRow);
                        }

                        InsertUpdateEPActivityApprove(graphAppointmentEntry, graphEmployeeActivitiesEntry, fsAppointmentEmployee, fsAppointmentRow, fsServiceOrderRow, epActivityApproveRow, epEmployeeRow);
                    }
                    else
                    {
                        if (epActivityApproveRow != null)
                        {
                            DeleteEPActivityApprove(graphEmployeeActivitiesEntry, epActivityApproveRow);
                        }
                    }
                }

                foreach (FSAppointmentEmployee fsAppointmentEmployee in appointmentEmployees.Cache.Deleted.RowCast<FSAppointmentEmployee>().Where(row => row.Type == BAccountType.EmployeeType))
                {
                    SearchAndDeleteEPActivity(graphAppointmentEntry, fsAppointmentEmployee, graphEmployeeActivitiesEntry);
                }
            }
            else if ((string)appointmentCache.GetValueOriginal<FSAppointment.status>(fsAppointmentRow) == ID.Status_Appointment.COMPLETED
                        && fsAppointmentRow.Status == ID.Status_Appointment.MANUAL_SCHEDULED)
            {
                foreach (FSAppointmentEmployee fsAppointmentEmployee in appointmentEmployees.Select().RowCast<FSAppointmentEmployee>().Where(row => row.Type == BAccountType.EmployeeType))
                {
                    SearchAndDeleteEPActivity(graphAppointmentEntry, fsAppointmentEmployee, graphEmployeeActivitiesEntry);
                }

                foreach (FSAppointmentEmployee fsAppointmentEmployee in appointmentEmployees.Cache.Deleted.RowCast<FSAppointmentEmployee>().Where(row => row.Type == BAccountType.EmployeeType))
                {
                    SearchAndDeleteEPActivity(graphAppointmentEntry, fsAppointmentEmployee, graphEmployeeActivitiesEntry);
                }
            }

            if (deleteReleatedTimeActivity != null)
            {
                //Deleting time activities related with cancelled service lines
                foreach (FSAppointmentEmployee fsAppointmentEmployee in deleteReleatedTimeActivity)
                {
                    if (fsAppointmentEmployee.Type == BAccountType.EmployeeType)
                    {
                        SearchAndDeleteEPActivity(graphAppointmentEntry, fsAppointmentEmployee, graphEmployeeActivitiesEntry);
                    }
                }
            }

            if (createReleatedTimeActivity != null)
            {
                //Creating time activities related with re-opened service lines
                foreach (FSAppointmentEmployee fsAppointmentEmployee in createReleatedTimeActivity)
                {
                    if (fsAppointmentEmployee.Type == BAccountType.EmployeeType)
                    {
                        TMEPEmployee epEmployeeRow = null;
                        FindTMEmployee(graphAppointmentEntry, fsAppointmentEmployee.EmployeeID, ref epEmployeeRow);
                        InsertUpdateEPActivityApprove(graphAppointmentEntry, graphEmployeeActivitiesEntry, fsAppointmentEmployee, fsAppointmentRow, fsServiceOrderRow, null, epEmployeeRow);
                    }
                }
            }
        }

        private static void SearchAndDeleteEPActivity(
            AppointmentEntry graphAppointmentEntry, 
            FSAppointmentEmployee fsAppointmentEmployee,
            EmployeeActivitiesEntry graphEmployeeActivitiesEntry)
        {
            EPActivityApprove epActivityApproveRow = null;
            TMEPEmployee epEmployeeRow = null;

            FindTMEmployee(graphAppointmentEntry, fsAppointmentEmployee.EmployeeID, ref epEmployeeRow);
            FindEPActivityApprove(graphAppointmentEntry, fsAppointmentEmployee, epEmployeeRow, ref epActivityApproveRow);

            if (epActivityApproveRow != null)
            {
                DeleteEPActivityApprove(graphEmployeeActivitiesEntry, epActivityApproveRow);
            }
        }

        private static void DeleteEPActivityApprove(
            EmployeeActivitiesEntry graphEmployeeActivitiesEntry,
            EPActivityApprove epActivityApproveRow)
        {
            if (epActivityApproveRow != null)
            {
                if (epActivityApproveRow.ApprovalStatus == ActivityStatusListAttribute.Approved || epActivityApproveRow.TimeCardCD != null)
                {
                    return;
                }

                graphEmployeeActivitiesEntry.Activity.Delete(epActivityApproveRow);
                graphEmployeeActivitiesEntry.Save.Press();
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

        public static void FindEPActivityApprove(
            PXGraph graph,
            FSAppointmentEmployee fsAppointmentEmployeeRow,
            TMEPEmployee epEmployeeRow,
            ref EPActivityApprove epActivityApproveRow)
        {
            epActivityApproveRow = PXSelect<EPActivityApprove,
                                    Where<
                                        EPActivityApprove.ownerID, Equal<Required<EPActivityApprove.ownerID>>,
                                        And<FSxPMTimeActivity.appointmentID, Equal<Required<FSAppointmentEmployee.appointmentID>>,
                                        And<FSxPMTimeActivity.appEmpID, Equal<Required<FSAppointmentEmployee.lineNbr>>>>>>
                                    .Select(graph, epEmployeeRow.PKID, fsAppointmentEmployeeRow.AppointmentID, fsAppointmentEmployeeRow.LineNbr);
        }

        public static void InsertUpdateEPActivityApprove(
            PXGraph graph,
            EmployeeActivitiesEntry graphEmployeeActivitiesEntry,
            FSAppointmentEmployee fsAppointmentEmployeeRow,
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

            FSAppointmentDetService fsAppointmentDetServiceRow =
                                        PXSelectJoin<FSAppointmentDetService,
                                        InnerJoin<FSSODet,
                                            On<FSSODet.sODetID, Equal<FSAppointmentDetService.sODetID>>>,
                                        Where<
                                            FSSODet.lineRef, Equal<Required<FSSODet.lineRef>>,
                                            And<FSSODet.sOID, Equal<Required<FSSODet.sOID>>,
                                            And<FSAppointmentDetService.appointmentID, Equal<Required<FSAppointmentDetService.appointmentID>>>>>>
                                        .Select(graph, fsAppointmentEmployeeRow.ServiceLineRef, fsAppointmentRow.SOID, fsAppointmentRow.AppointmentID);

            if (fsAppointmentDetServiceRow != null && 
                    fsAppointmentDetServiceRow.Status == ID.Status_AppointmentDet.CANCELED)
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
            epActivityApproveRow.Date = SharedFunctions.GetCustomDateTime(fsAppointmentRow.ExecutionDate, fsAppointmentEmployeeRow.ActualDateTimeBegin);
            epActivityApproveRow.EarningTypeID = fsAppointmentEmployeeRow.EarningType;
            epActivityApproveRow.TimeSpent = fsAppointmentEmployeeRow.ActualDuration;
            epActivityApproveRow.Summary = GetDescriptionToUseInEPActivityApprove(fsAppointmentRow, fsAppointmentEmployeeRow, fsAppointmentDetServiceRow);
            epActivityApproveRow.CostCodeID = fsAppointmentEmployeeRow?.CostCodeID;

            FSxPMTimeActivity fsxPMTimeActivityRow = PXCache<PMTimeActivity>.GetExtension<FSxPMTimeActivity>((PMTimeActivity)epActivityApproveRow);

            fsxPMTimeActivityRow.SOID = fsAppointmentRow.SOID;
            fsxPMTimeActivityRow.AppointmentID = fsAppointmentRow.AppointmentID;
            fsxPMTimeActivityRow.AppointmentCustomerID = fsServiceOrderRow.CustomerID;
            fsxPMTimeActivityRow.AppEmpID = fsAppointmentEmployeeRow.LineNbr;

            if (fsAppointmentEmployeeRow.ServiceLineRef != null)
            {
                fsxPMTimeActivityRow.ServiceID = fsAppointmentDetServiceRow == null ? null : fsAppointmentDetServiceRow.InventoryID;
            }

            epActivityApproveRow = graphEmployeeActivitiesEntry.Activity.Update(epActivityApproveRow);

            graphEmployeeActivitiesEntry.Activity.SetValueExt<EPActivityApprove.projectID>(epActivityApproveRow, fsServiceOrderRow.ProjectID);
            graphEmployeeActivitiesEntry.Activity.SetValueExt<EPActivityApprove.projectTaskID>(epActivityApproveRow, GetProjectTaskIDToUseInEPActivityApprove(fsAppointmentRow, fsAppointmentEmployeeRow, fsAppointmentDetServiceRow));
            graphEmployeeActivitiesEntry.Activity.SetValueExt<EPActivityApprove.isBillable>(epActivityApproveRow, false);
            graphEmployeeActivitiesEntry.Activity.SetValueExt<EPActivityApprove.approvalStatus>(epActivityApproveRow, GetStatusToUseInEPActivityApprove());
            graphEmployeeActivitiesEntry.Activity.SetValueExt<EPActivityApprove.labourItemID>(epActivityApproveRow, fsAppointmentEmployeeRow.LaborItemID);

            graphEmployeeActivitiesEntry.Save.Press();
        }

        private static string GetStatusToUseInEPActivityApprove()
        {
            return ActivityStatusListAttribute.Completed;
        }

        private static object GetProjectTaskIDToUseInEPActivityApprove(
            FSAppointment fsAppointmentRow,
            FSAppointmentEmployee fsAppointmentEmployeeRow,
            FSAppointmentDetService fsAppointmentDetServiceRow)
        {
            if (fsAppointmentEmployeeRow != null && fsAppointmentEmployeeRow.ServiceLineRef != null && fsAppointmentDetServiceRow != null)
            {
                return fsAppointmentDetServiceRow.ProjectTaskID;
            }
            else
            {
                return fsAppointmentRow.DfltProjectTaskID;
            }
        }

        private static string GetDescriptionToUseInEPActivityApprove(
            FSAppointment fsAppointmentRow,
            FSAppointmentEmployee fsAppointmentEmployeeRow,
            FSAppointmentDetService fsAppointmentDetServiceRow)
        {
            if (fsAppointmentEmployeeRow != null
                    && fsAppointmentEmployeeRow.ServiceLineRef != null 
                    && fsAppointmentDetServiceRow != null)
            {
                return fsAppointmentDetServiceRow.TranDesc;
            }
            else
            {
                return fsAppointmentRow.DocDesc;
            }
        }

        private static int? GetTimeSpentToUseInEPActivityApprove(
            FSAppointment fsAppointmentRow,
            FSAppointmentEmployee fsAppointmentEmployeeRow)
        {
            if (fsAppointmentEmployeeRow != null 
                    && fsAppointmentEmployeeRow.ServiceLineRef != null)
            {
                return fsAppointmentEmployeeRow.ActualDuration;
            }
            else
            {
                return (int?)(fsAppointmentRow.ActualDateTimeEnd - fsAppointmentRow.ActualDateTimeBegin).Value.TotalMinutes;
            }
        }

        private static DateTime? GetDateToUseInEPActivityApprove(
            FSAppointment fsAppointmentRow,
            FSAppointmentEmployee fsAppointmentEmployeeRow)
        {
            if (fsAppointmentEmployeeRow != null 
                    && fsAppointmentEmployeeRow.ServiceLineRef != null 
                    && fsAppointmentEmployeeRow.ActualDateTimeBegin != null)
            {
                return SharedFunctions.GetCustomDateTime(fsAppointmentRow.ExecutionDate, fsAppointmentEmployeeRow.ActualDateTimeBegin);
            }
            else
            {
                return SharedFunctions.GetCustomDateTime(fsAppointmentRow.ExecutionDate, fsAppointmentRow.ActualDateTimeBegin);
            }
        }

        private static bool IsNecessaryToUpdateTimeCards(
            PXCache appointmentCache,
            AppointmentCore.AppointmentEmployees_View appointmentEmployees, 
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
                    (string)appointmentCache.GetValueOriginal<FSAppointment.status>(fsAppointmentRow) != ID.Status_Appointment.COMPLETED
                    || appointmentEmployees.Cache.IsInsertedUpdatedDeleted
                    || appointmentModified))
            {
                return true;
            }

            return false;
        }
    }
}
