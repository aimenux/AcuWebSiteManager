using PX.Data;
using System;
using System.Collections.Generic;
using System.Linq;

namespace PX.Objects.FS
{
    public class WrkProcess : PXGraph<WrkProcess, FSWrkProcess>
    {
        #region Public Members
        public const char SEPARATOR = ',';
        public bool LaunchTargetScreen = true;
        public int? processID;
        #endregion

        public PXSelect<FSWrkProcess> WrkProcessRecords;

        #region Static Methods

        /// <summary>
        /// Split a string in several substrings by a separator character.
        /// </summary>
        /// <param name="parameters">String representing the whole parameters.</param>
        /// <param name="separator">Char representing the separation of parameters.</param>
        /// <returns>A string list.</returns>
        public static List<string> GetParameterList(string parameters, char separator)
        {
            List<string> parameterList = new List<string>();

            if (string.IsNullOrEmpty(parameters))
            {
                return parameterList;
            }

            parameterList = parameters.Split(separator).ToList();

            return parameterList;
        }

        /// <summary>
        /// Try to save a <c>FSWrkProcess</c> record to the database.
        /// </summary>
        /// <param name="fsWrkProcessRow"><c>FSWrkProcess</c> row.</param>
        /// <returns>Returns the ProcessID of the saved record.</returns>
        public static int? SaveWrkProcessParameters(FSWrkProcess fsWrkProcessRow)
        {
            var graphWrkProcess = PXGraph.CreateInstance<WrkProcess>();

            graphWrkProcess.LaunchTargetScreen = false;
            graphWrkProcess.WrkProcessRecords.Current = graphWrkProcess.WrkProcessRecords.Insert(fsWrkProcessRow);
            graphWrkProcess.Save.Press();

            return graphWrkProcess.WrkProcessRecords.Current.ProcessID;
        }

        #endregion

        #region Private Functions

        /// <summary>
        /// Delete old records from database.
        /// </summary>
        private void DeleteOldRecords()
        {
            PXDatabase.Delete<FSWrkProcess>(
                new PXDataFieldRestrict<FSWrkProcess.createdDateTime>(PXDbType.DateTime, 8, DateTime.Now.AddDays(-2), PXComp.LE));
        }

        #region Service Order Methods

        /// <summary>
        /// Try to get the appropriate ServiceOrderType from this sources:
        /// a. <c>FSServiceOrder</c>
        /// b. <c>FSWrkProcessRow</c>
        /// c. <c>FSSetup</c>
        /// </summary>
        /// <param name="fsWrkProcessRow"><c>FSWrkProcess</c> row.</param>
        /// <param name="fsServiceOrderRow">FSServiceOrder row.</param>
        private static string GetSrvOrdType(PXGraph graph, FSWrkProcess fsWrkProcessRow, FSServiceOrder fsServiceOrderRow)
        {
            // a) Try to retrieve the ServiceOrderType from the ServiceOrder row
            if (fsWrkProcessRow.SOID != null
                    && fsServiceOrderRow != null
                      && !string.IsNullOrEmpty(fsServiceOrderRow.SrvOrdType))
            {
                return fsServiceOrderRow.SrvOrdType;
            }

            // b) Try to retrieve the ServiceOrderType from the WrkProcess row
            if (!string.IsNullOrEmpty(fsWrkProcessRow.SrvOrdType) &&
                !string.IsNullOrWhiteSpace(fsWrkProcessRow.SrvOrdType))
            {
                return fsWrkProcessRow.SrvOrdType;
            }

            // c) Try to retrieve the ServiceOrderType from the users preferences
            PX.SM.UserPreferences userPreferencesRow = PXSelect<PX.SM.UserPreferences,
                                                       Where<
                                                           PX.SM.UserPreferences.userID, Equal<CurrentValue<AccessInfo.userID>>>>
                                                       .Select(graph);

            if (userPreferencesRow != null)
            {
                FSxUserPreferences fsxUserPreferencesRow = PXCache<PX.SM.UserPreferences>.GetExtension<FSxUserPreferences>(userPreferencesRow);

                if (!string.IsNullOrEmpty(fsxUserPreferencesRow.DfltSrvOrdType))
                {
                    return fsxUserPreferencesRow.DfltSrvOrdType;
                }
            }


            // d) Try to retrieve the Default ServiceOrderType from the Setup row
            FSSetup fsSetupRow = PXSetup<FSSetup>.Select(graph);

            if (fsSetupRow != null
                    && !string.IsNullOrEmpty(fsSetupRow.DfltSrvOrdType))
            {
                return fsSetupRow.DfltSrvOrdType;
            }

            return null;
        }

        /// <summary>
        /// Try to retrieve a ServiceOrder row associated to the supplied <c>WrkProcess</c> row.
        /// </summary>
        /// <param name="fsWrkProcessRow"><c>FSWrkProcess</c> row.</param>
        /// <returns><c>FSServiceOrder</c> row.</returns>
        private static FSServiceOrder GetServiceOrder(PXGraph graph, FSWrkProcess fsWrkProcessRow)
        {
            if (fsWrkProcessRow == null)
            {
                return null;
            }

            if (fsWrkProcessRow.SOID != null)
            {
                return PXSelect<FSServiceOrder,
                       Where<
                            FSServiceOrder.sOID, Equal<Required<FSServiceOrder.sOID>>>>
                       .Select(graph, fsWrkProcessRow.SOID);
            }

            return null;
        }

        #endregion

        #region LauncScreen Methods

        /// <summary>
        /// Launches the target screen specified in the <c>FSWrkProcess</c> row.
        /// </summary>
        /// <param name="processID"><c>Int</c> id of the process.</param>
        private void LaunchScreen(int? processID)
        {
            DeleteOldRecords();

            FSWrkProcess fsWrkProcessRow = PXSelect<FSWrkProcess,
                                           Where<
                                               FSWrkProcess.processID, Equal<Required<FSWrkProcess.processID>>>>
                                           .Select(this, processID);

            if (fsWrkProcessRow == null)
            {
                return;
            }

            switch (fsWrkProcessRow.TargetScreenID)
            {
                case ID.ScreenID.APPOINTMENT:
                    LaunchAppointmentEntryScreen(this, fsWrkProcessRow);
                    break;

                default:
                    return;
            }
        }

        public static void AssignAppointmentRoom(AppointmentEntry graphAppointmentEntry, FSWrkProcess fsWrkProcessRow, FSServiceOrder fsServiceOrderRow = null)
        {
            if (string.IsNullOrEmpty(fsWrkProcessRow.RoomID) == false
                    && string.IsNullOrWhiteSpace(fsWrkProcessRow.RoomID) == false)
            {
                graphAppointmentEntry.ServiceOrderRelated.SetValueExt<FSServiceOrder.roomID>
                                        (graphAppointmentEntry.ServiceOrderRelated.Current, fsWrkProcessRow.RoomID);

                if (fsServiceOrderRow == null)
                {
                    fsServiceOrderRow = GetServiceOrder(graphAppointmentEntry, fsWrkProcessRow);
                }

                if (fsServiceOrderRow != null)
                {
                    graphAppointmentEntry.ServiceOrderRelated.Cache.SetStatus(graphAppointmentEntry.ServiceOrderRelated.Current, PXEntryStatus.Updated);
                }
            }
        }

        public static void AssignAppointmentEmployeeByList(AppointmentEntry graphAppointmentEntry, List<string> employeeList, List<string> soDetIDList)
        {
            employeeList.Reverse();

            if (employeeList.Count > 0 && soDetIDList.Count <= 0)
            {
                for (int i = 0; i < employeeList.Count; i++)
                {
                    FSAppointmentEmployee fsAppointmentEmployeeRow = new FSAppointmentEmployee();

                    fsAppointmentEmployeeRow = graphAppointmentEntry.AppointmentServiceEmployees.Insert(fsAppointmentEmployeeRow);
                    graphAppointmentEntry.AppointmentServiceEmployees.Cache.SetValueExt<FSAppointmentEmployee.employeeID>(fsAppointmentEmployeeRow, (int?)Convert.ToInt32(employeeList[i]));
                }
            }
        }

        /// <summary>
        /// Launches the AppointmentEntry screen with some preloaded values.
        /// </summary>
        /// <param name="fsWrkProcessRow"><c>FSWrkProcess</c> row.</param>
        public static int? LaunchAppointmentEntryScreen(PXGraph graph, FSWrkProcess fsWrkProcessRow, bool redirect = true)
        {
            AppointmentEntry graphAppointmentEntry = PXGraph.CreateInstance<AppointmentEntry>();

            List<string> soDetIDList = GetParameterList(fsWrkProcessRow.LineRefList, SEPARATOR);
            List<string> employeeList = GetParameterList(fsWrkProcessRow.EmployeeIDList, SEPARATOR);

            if (fsWrkProcessRow.AppointmentID != null)
            {
                graphAppointmentEntry.AppointmentRecords.Current = graphAppointmentEntry.AppointmentRecords.Search<FSAppointment.appointmentID>(fsWrkProcessRow.AppointmentID, fsWrkProcessRow.SrvOrdType);
                AssignAppointmentRoom(graphAppointmentEntry, fsWrkProcessRow);
                AssignAppointmentEmployeeByList(graphAppointmentEntry, employeeList, soDetIDList);
            }
            else
            {
                FSAppointment fsAppointmentRow = new FSAppointment();
                FSServiceOrder fsServiceOrderRow = GetServiceOrder(graph, fsWrkProcessRow);
                fsAppointmentRow.SrvOrdType = GetSrvOrdType(graph, fsWrkProcessRow, fsServiceOrderRow);

                if (fsAppointmentRow.SrvOrdType == null)
                {
                    throw new PXException(TX.Error.DEFAULT_SERVICE_ORDER_TYPE_NOT_DEFINED);
                }

                if (fsAppointmentRow.SOID == null)
                {
                    SharedFunctions.ValidateSrvOrdTypeNumberingSequence(graph, fsAppointmentRow.SrvOrdType);
                }

                graphAppointmentEntry.AppointmentRecords.Current = graphAppointmentEntry.AppointmentRecords.Insert(fsAppointmentRow);

                #region ScheduleDateTime Fields
                // to know if we want to set false to the flag KeepTotalServicesDuration
                bool scheduleTimeFlag = true;

                if (fsWrkProcessRow.ScheduledDateTimeBegin.HasValue == true)
                {
                    graphAppointmentEntry.AppointmentRecords.SetValueExt<FSAppointment.scheduledDateTimeBegin>
                                                             (graphAppointmentEntry.AppointmentRecords.Current,
                                                              fsWrkProcessRow.ScheduledDateTimeBegin);
                }
                else
                {
                    scheduleTimeFlag = false;
                }

                if (fsWrkProcessRow.ScheduledDateTimeEnd.HasValue && scheduleTimeFlag)
                {
                    graphAppointmentEntry.AppointmentRecords.SetValueExt<FSAppointment.handleManuallyScheduleTime>
                                                             (graphAppointmentEntry.AppointmentRecords.Current, true);

                    if (fsWrkProcessRow.ScheduledDateTimeBegin != fsWrkProcessRow.ScheduledDateTimeEnd)
                    {
                        graphAppointmentEntry.AppointmentRecords.SetValueExt<FSAppointment.scheduledDateTimeEnd>
                                                                 (graphAppointmentEntry.AppointmentRecords.Current,
                                                                  fsWrkProcessRow.ScheduledDateTimeEnd);
                    }
                    else
                    {
                        graphAppointmentEntry.AppointmentRecords.SetValueExt<FSAppointment.scheduledDateTimeEnd>
                                                                 (graphAppointmentEntry.AppointmentRecords.Current,
                                                                  fsWrkProcessRow.ScheduledDateTimeBegin.Value.AddHours(1));
                    }
                }

                #endregion

                #region ServiceOrder Fields

                if (fsServiceOrderRow == null)
                {
                    if (fsWrkProcessRow.BranchID.HasValue == true)
                    {
                        graphAppointmentEntry.ServiceOrderRelated.SetValueExt<FSServiceOrder.branchID>
                                                                  (graphAppointmentEntry.ServiceOrderRelated.Current,
                                                                   fsWrkProcessRow.BranchID);
                    }

                    if (fsWrkProcessRow.BranchLocationID.HasValue == true && fsWrkProcessRow.BranchLocationID > 0)
                    {
                        graphAppointmentEntry.ServiceOrderRelated.SetValueExt<FSServiceOrder.branchLocationID>
                                                                  (graphAppointmentEntry.ServiceOrderRelated.Current,
                                                                   fsWrkProcessRow.BranchLocationID);
                    }

                    if (fsWrkProcessRow.CustomerID.HasValue == true && fsWrkProcessRow.CustomerID > 0)
                    {
                        graphAppointmentEntry.ServiceOrderRelated.SetValueExt<FSServiceOrder.customerID>
                                                                  (graphAppointmentEntry.ServiceOrderRelated.Current,
                                                                   fsWrkProcessRow.CustomerID);
                    }
                }
                else
                {
                    graphAppointmentEntry.AppointmentRecords.SetValueExt<FSAppointment.soRefNbr>
                                                             (graphAppointmentEntry.AppointmentRecords.Current,
                                                              fsServiceOrderRow.RefNbr);
                }

                AssignAppointmentRoom(graphAppointmentEntry, fsWrkProcessRow, fsServiceOrderRow);

                #endregion

                #region Get Appointment Values

                #region Services
                soDetIDList.Reverse();

                if (soDetIDList.Count > 0)
                {
                    var appointmentDetails = graphAppointmentEntry.AppointmentDetails.Select().RowCast<FSAppointmentDet>()
                                                                                     .Where(x => x.IsInventoryItem == false);

                    foreach (FSAppointmentDet fsAppointmentDetRow in appointmentDetails)
                    {
                        if (soDetIDList.Contains(fsAppointmentDetRow.SODetID.ToString()) == false)
                        {
                            graphAppointmentEntry.AppointmentDetails.Delete(fsAppointmentDetRow);
                        }
                        else
                        {
                            InsertEmployeeLinkedToService(graphAppointmentEntry, employeeList, fsAppointmentDetRow.LineRef);
                        }
                    }

                    var employees = graphAppointmentEntry.AppointmentServiceEmployees.Select()
                                                         .RowCast<FSAppointmentEmployee>()
                                                         .Where(_ => _.ServiceLineRef != null);

                    foreach (FSAppointmentEmployee fsAppointmentEmployeeRow in employees)
                    {
                        FSAppointmentDet fsAppointmentDetRow = (FSAppointmentDet)PXSelectorAttribute.Select<FSAppointmentEmployee.serviceLineRef>
                                                                                                     (graphAppointmentEntry.AppointmentServiceEmployees.Cache,
                                                                                                      fsAppointmentEmployeeRow);

                        if (fsAppointmentDetRow != null && soDetIDList.Contains(fsAppointmentDetRow.SODetID.ToString()) == false)
                        {
                            graphAppointmentEntry.AppointmentServiceEmployees.Delete(fsAppointmentEmployeeRow);
                        }
                    }
                }
                #endregion
                #region Employees

                AssignAppointmentEmployeeByList(graphAppointmentEntry, employeeList, soDetIDList);

                #endregion
                #region Equipment
                List<string> equipmentList = GetParameterList(fsWrkProcessRow.EquipmentIDList, SEPARATOR);
                equipmentList.Reverse();

                if (equipmentList.Count > 0)
                {
                    for (int i = 0; i < equipmentList.Count; i++)
                    {
                        FSAppointmentResource fsAppointmentResourceRow = new FSAppointmentResource();

                        fsAppointmentResourceRow.SMEquipmentID = (int?)Convert.ToInt32(equipmentList[i]);
                        graphAppointmentEntry.AppointmentResources.Insert(fsAppointmentResourceRow);
                    }
                }
                #endregion
                #endregion
            }

            if (fsWrkProcessRow.SMEquipmentID.HasValue == true)
            {
                graphAppointmentEntry.AppointmentRecords.SetValueExt<FSAppointment.mem_SMequipmentID>
                                                         (graphAppointmentEntry.AppointmentRecords.Current,
                                                          fsWrkProcessRow.SMEquipmentID);

                redirect = true;
            }

            if (redirect == true)
            {
                throw new PXRedirectRequiredException(graphAppointmentEntry, null) { Mode = PXBaseRedirectException.WindowMode.Same };
            }
            else
            {
                try
                {
                    graphAppointmentEntry.RecalculateExternalTaxesSync = true;
                    graphAppointmentEntry.Actions.PressSave();
                }
                catch (Exception e)
                {
                    throw e;
                }
                finally
                {
                    graphAppointmentEntry.RecalculateExternalTaxesSync = false;
                }

                return graphAppointmentEntry.AppointmentRecords.Current?.AppointmentID;
            }
        }



        /// <summary>
        /// Launches the AppointmentEntry screen with some preloaded values.
        /// </summary>
        /// <param name="fsWrkProcessRow"><c>FSWrkProcess</c> row.</param>
        public static void AssignAppointmentEmployee(FSAppointmentScheduleBoard fsAppointmentRow, ExternalControls graphExternalControls, AppointmentEntry graphAppointmentMaint, ref ExternalControls.DispatchBoardAppointmentMessages response)
        {
            FSServiceOrder fsServiceOrderRow = graphAppointmentMaint.ServiceOrderRelated.Current;
            if (string.IsNullOrEmpty(fsAppointmentRow.RoomID) == false
                    && string.IsNullOrWhiteSpace(fsAppointmentRow.RoomID) == false)
            {
                fsServiceOrderRow.RoomID = fsAppointmentRow.RoomID;
            }

            graphAppointmentMaint.ServiceOrderRelated.Update(fsServiceOrderRow);

            if (fsAppointmentRow.OldEmployeeID != fsAppointmentRow.EmployeeID)
            {
                FSAppointmentEmployee fsAppointmentEmployeeRow = graphExternalControls.AppointmentEmployeeByEmployee
                                                                                      .Select(fsAppointmentRow.AppointmentID, 
                                                                                              fsAppointmentRow.EmployeeID);

                if (fsAppointmentEmployeeRow != null)
                {
                    response.ErrorMessages.Add(ExternalControls.GetErrorMessage(ExternalControls.ErrorCode.APPOINTMENT_SHARED));
                }
                else
                {
                    FSAppointmentEmployee fsAppointmentEmployeeRow_New = new FSAppointmentEmployee()
                    {
                        AppointmentID = fsAppointmentRow.AppointmentID,
                        EmployeeID = fsAppointmentRow.EmployeeID
                    };

                    fsAppointmentEmployeeRow_New = graphAppointmentMaint.AppointmentServiceEmployees.Insert(fsAppointmentEmployeeRow_New);

                    foreach (FSAppointmentEmployee fsAppointmentEmployeeRow_Old in graphAppointmentMaint.AppointmentServiceEmployees.Select())
                    {
                        if (fsAppointmentEmployeeRow_Old.EmployeeID == fsAppointmentRow.OldEmployeeID)
                        {
                            graphAppointmentMaint.AppointmentServiceEmployees.Delete(fsAppointmentEmployeeRow_Old);
                        }
                    }
                }
            }
        }

        private static void InsertEmployeeLinkedToService(AppointmentEntry graphAppointmentEntry, List<string> employeeList, string lineRef)
        {
            foreach (string employeeID in employeeList)
            {
                int employeeIntID = -1;

                if (int.TryParse(employeeID, out employeeIntID))
                {
                    FSAppointmentEmployee fsAppointmentEmployeeRow = new FSAppointmentEmployee();
                    fsAppointmentEmployeeRow = graphAppointmentEntry.AppointmentServiceEmployees.Insert(fsAppointmentEmployeeRow);

                    graphAppointmentEntry.AppointmentServiceEmployees.Cache.SetValueExt<FSAppointmentEmployee.employeeID>(fsAppointmentEmployeeRow, employeeIntID);
                    graphAppointmentEntry.AppointmentServiceEmployees.Cache.SetValueExt<FSAppointmentEmployee.serviceLineRef>(fsAppointmentEmployeeRow, lineRef);
                }
            }
        }
        #endregion

        #endregion

        #region FSWrkProcess Events

        protected virtual void FSWrkProcess_RowSelected(PXCache cache, PXRowSelectedEventArgs e)
        {
            if (e.Row == null)
            {
                return;
            }

            if (LaunchTargetScreen == false)
            {
                return;
            }

            FSWrkProcess fsWrkProcessRow = (FSWrkProcess)e.Row;
            if (fsWrkProcessRow.ProcessID > 0 && processID == null)
            {
                processID = fsWrkProcessRow.ProcessID;
            }

            if (processID > 0)
            {
                LaunchScreen(processID);
                LaunchTargetScreen = false;
            }
        }

        #endregion
    }
}