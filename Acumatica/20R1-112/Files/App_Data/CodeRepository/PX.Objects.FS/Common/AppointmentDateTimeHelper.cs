using Autofac;
using PX.Common;
using PX.Data;
using PX.Objects.EP;
using PX.Objects.IN;
using System;
using System.Collections.Generic;
using System.Linq;

namespace PX.Objects.FS
{
    public partial class AppointmentEntry
    {
        public const int DEFAULT_DURATION_MINUTES = 1;

        public enum DateFieldType
        {
            ScheduleField,
            ActualField
        }

        public class int_24hrs : PX.Data.BQL.BqlInt.Constant<int_24hrs>
        {
            public int_24hrs()
                : base(1440)
            {
            }
        }

        #region AppointmentHeaderFunctions
        public virtual void SetNowInHeaderActualDateTimeBegin(PXCache cache, FSAppointment fsAppointmentRow)
        {
            if (fsAppointmentRow != null && fsAppointmentRow.HandleManuallyActualTime == false)
            {
                DateTime? actualDate = fsAppointmentRow.ExecutionDate;

                if (cache.Graph.IsMobile == true)
                {
                    actualDate = PXTimeZoneInfo.Now;

                    cache.SetValueExtIfDifferent<FSAppointment.executionDate>(fsAppointmentRow, actualDate.Value.Date);
                }

                DateTime actualTime = PXTimeZoneInfo.Now;

                cache.SetValueExtIfDifferent<FSAppointment.actualDateTimeBegin>(fsAppointmentRow,
                                                PXDBDateAndTimeAttribute.CombineDateTime(actualDate, actualTime));
            }
        }

        public virtual void SetNowInHeaderActualDateTimeEnd(PXCache cache, FSAppointment fsAppointmentRow)
        {
            if (fsAppointmentRow != null && fsAppointmentRow.HandleManuallyActualTime == false)
            {
                DateTime? actualDate = fsAppointmentRow.ExecutionDate;
                DateTime actualTime = PXTimeZoneInfo.Now;

                DateTime? actualDateTimeEnd = PXDBDateAndTimeAttribute.CombineDateTime(actualDate, actualTime);

                if (actualDateTimeEnd.Equals(fsAppointmentRow.ActualDateTimeBegin))
                {
                    actualDateTimeEnd = actualDateTimeEnd.Value.AddMinutes(DEFAULT_DURATION_MINUTES);
                }

                cache.SetValueExtIfDifferent<FSAppointment.actualDateTimeEnd>(fsAppointmentRow, actualDateTimeEnd);
                cache.SetValueExtIfDifferent<FSAppointment.handleManuallyActualTime>(fsAppointmentRow, true);
            }
        }

        public virtual bool ActualDateAndTimeValidation(FSAppointment fsAppointmentRow)
        {
            return fsAppointmentRow.ActualDateTimeBegin != null && fsAppointmentRow.ActualDateTimeEnd != null;
        }
        #endregion

        #region AppointmentDetFunctions
        /// <summary>
        /// Sets properly the right values for ActualDuration & Qty for services and inventory items and the
        /// ActualDateTimeBegin & ActualDateTimeEnd for the staff members when starting & re-opening the appointment.
        /// </summary>
        public virtual void UpdateAppointmentDetActualFields(AppointmentCore.AppointmentDetails_View appointmentDetails,
                                                            AppointmentCore.AppointmentServiceEmployees_View appointmentEmployees,
                                                            bool isReOpen = false)
        {
            foreach (FSAppointmentDet fsAppointmentDetRow in appointmentDetails.Select().RowCast<FSAppointmentDet>())
            {
                if (fsAppointmentDetRow.IsService || fsAppointmentDetRow.IsInventoryItem)
                {
                    if (fsAppointmentDetRow.IsService && fsAppointmentDetRow.StaffRelated == true)
                    {
                        appointmentDetails.Cache.SetDefaultExt<FSAppointmentDet.actualDuration>(fsAppointmentDetRow);
                    }
                    else if (fsAppointmentDetRow.IsService && fsAppointmentDetRow.StaffRelated == false)
                    {
                        appointmentDetails.Cache.SetDefaultExt<FSAppointmentDet.actualDuration>(fsAppointmentDetRow);
                    }

                    if (fsAppointmentDetRow.BillingRule == ID.BillingRule.FLAT_RATE
                        || fsAppointmentDetRow.BillingRule == ID.BillingRule.NONE
                        || fsAppointmentDetRow.IsInventoryItem == true)
                    {
                        fsAppointmentDetRow.Qty = isReOpen ? 0 : fsAppointmentDetRow.EstimatedQty;
                    }

                    appointmentDetails.Update(fsAppointmentDetRow);
                }
            }
        }
        public virtual void OnApptStartTimeChangeUpdateLogStartTime(FSAppointment fsAppointmentRow, FSSrvOrdType fsSrvOrdTypeRow,
                                                                    AppointmentCore.AppointmentLog_View logRecords)
        {
            if (fsAppointmentRow != null && fsSrvOrdTypeRow?.OnStartTimeChangeUpdateLogStartTime == true)
            {
                foreach (FSAppointmentLog fsAppointmentLogRow in logRecords.Select().RowCast<FSAppointmentLog>()
                                                                            .Where(_ => _.KeepDateTimes == false && _.Type != ID.Type_Log.TRAVEL))
                {
                    FSAppointmentLog copy = (FSAppointmentLog)logRecords.Cache.CreateCopy(fsAppointmentLogRow);
                    copy.DateTimeBegin = fsAppointmentRow.ActualDateTimeBegin;
                    logRecords.Cache.Update(copy);
                }
            }
        }

        public virtual void OnApptEndTimeChangeUpdateLogEndTime(FSAppointment fsAppointmentRow, FSSrvOrdType fsSrvOrdTypeRow,
                                                                    AppointmentCore.AppointmentLog_View logRecords)
        {
            if (fsAppointmentRow != null && fsSrvOrdTypeRow?.OnEndTimeChangeUpdateLogEndTime == true)
            {
                foreach (FSAppointmentLog fsAppointmentLogRow in logRecords.Select().RowCast<FSAppointmentLog>()
                                                                            .Where(_ => _.KeepDateTimes == false && _.Type != ID.Type_Log.TRAVEL))
                {
                    FSAppointmentLog copy = (FSAppointmentLog)logRecords.Cache.CreateCopy(fsAppointmentLogRow);
                    copy.DateTimeEnd = fsAppointmentRow.ActualDateTimeEnd;
                    logRecords.Cache.Update(copy);
                }
            }
        }
        #endregion

        #region TravelTimeAndLogFunctions

        public virtual void ClearAppointmentLog()
        {
            foreach (FSAppointmentLog fsAppointmentLogRow in LogRecords.Select().RowCast<FSAppointmentLog>().Where(_ => _.Type != ID.Type_Log.TRAVEL))
            {
                LogRecords.Delete(fsAppointmentLogRow);
            }
        }

        /// <summary>
        /// Resets certain Log fields when there's no employee selected.
        /// </summary>
        /// <param name="fsLogRow"></param>
        public virtual void ResetLogFieldsNoEmployee(FSAppointmentLog fsLogRow)
        {
            if (fsLogRow.BAccountID == null)
            {
                fsLogRow.EarningType = null;
                fsLogRow.LaborItemID = null;

                FSAppointmentDet fsAppointmentDetRow = null;

                if (string.IsNullOrWhiteSpace(fsLogRow.DetLineRef) == false)
                {
                    fsAppointmentDetRow = AppointmentDetails.Select().RowCast<FSAppointmentDet>().Where(_ => _.LineRef == fsLogRow.DetLineRef).FirstOrDefault();
                }

                if (fsAppointmentDetRow != null
                    && (fsAppointmentDetRow.LineType == ID.LineType_ALL.SERVICE ||
                        fsAppointmentDetRow.LineType == ID.LineType_ALL.NONSTOCKITEM))
                {
                    fsLogRow.ProjectID = fsAppointmentDetRow.ProjectID;
                    fsLogRow.ProjectTaskID = fsAppointmentDetRow.ProjectTaskID;
                    fsLogRow.CostCodeID = fsAppointmentDetRow.CostCodeID;
                }
            }
        }

        public virtual void SetLogInfoFromDetails(PXCache cache, FSAppointmentLog fsLogRow)
        {
            string logType = fsLogRow.Travel == true ? ID.Type_Log.TRAVEL : ID.Type_Log.SERVICE;

            if (fsLogRow.DetLineRef != null)
            {
                FSAppointmentDet fsAppointmentDetRow = AppointmentDetails.Select().RowCast<FSAppointmentDet>().Where(_ => _.LineRef == fsLogRow.DetLineRef).FirstOrDefault();

                if (fsAppointmentDetRow != null)
                {
                    fsLogRow.Descr = fsAppointmentDetRow.TranDesc;
                    if (fsAppointmentDetRow.IsTravelItem == true)
                    {
                        logType = ID.Type_Log.TRAVEL;
                    }
                    else if (fsAppointmentDetRow.LineType == ID.LineType_ALL.NONSTOCKITEM)
                    {
                        logType = ID.Type_Log.NON_STOCK;
                    }
                    else
                    {
                        logType = ID.Type_Log.SERVICE;
                    }
                }
            }
            else if (fsLogRow.BAccountID != null)
            {
                if (fsLogRow.Travel == true)
                {
                    logType = ID.Type_Log.TRAVEL;
                }
                else
                {
                    logType = ID.Type_Log.STAFF_ASSIGMENT;
                }
            }

            cache.SetValueExt<FSAppointmentLog.type>(fsLogRow, logType);
        }

        public virtual void PrimaryDriver_FieldUpdated_Handler(PXCache cache, FSAppointmentEmployee fsAppointmentEmployeeRow)
        {
            PXResultset<FSAppointmentEmployee> employeeRows = AppointmentServiceEmployees.Select();
            foreach (FSAppointmentEmployee row in employeeRows.RowCast<FSAppointmentEmployee>()
                                                        .Where(_ => _.EmployeeID == fsAppointmentEmployeeRow.EmployeeID))
            {
                row.PrimaryDriver = fsAppointmentEmployeeRow.PrimaryDriver;
                if (cache.GetStatus(row) == PXEntryStatus.Notchanged)
                {
                    cache.SetStatus(row, PXEntryStatus.Updated);
                }
            }

            if (fsAppointmentEmployeeRow.PrimaryDriver == true)
            {
                foreach (FSAppointmentEmployee row in employeeRows.RowCast<FSAppointmentEmployee>()
                                                        .Where(_ => _.EmployeeID != fsAppointmentEmployeeRow.EmployeeID &&
                                                                    _.PrimaryDriver == true))
                {
                    row.PrimaryDriver = false;
                    if (cache.GetStatus(row) == PXEntryStatus.Notchanged)
                    {
                        cache.SetStatus(row, PXEntryStatus.Updated);
                    }
                }
            }

            AppointmentServiceEmployees.View.RequestRefresh();
        }

        public virtual void PrimaryDriver_RowDeleting_Handler(PXCache cache, FSAppointmentEmployee fsAppointmentEmployeeRow)
        {
            if (fsAppointmentEmployeeRow.PrimaryDriver == true)
            {
                PXResultset<FSAppointmentEmployee> fsAppointmentEmployeeRows = AppointmentServiceEmployees.Select();

                if (fsAppointmentEmployeeRows.RowCast<FSAppointmentEmployee>()
                                             .Where(_ => _.EmployeeID == fsAppointmentEmployeeRow.EmployeeID).Any() == false)
                {
                    IEnumerable<FSAppointmentEmployee> firstAppointmentEmployeeRow = fsAppointmentEmployeeRows.RowCast<FSAppointmentEmployee>()
                                                                                       .OrderBy(_ => _.LineNbr);
                    if (firstAppointmentEmployeeRow.Any() == true)
                    {
                        cache.SetValueExt<FSAppointmentEmployee.primaryDriver>(firstAppointmentEmployeeRow.First(), true);
                    }

                    AppointmentRecords.Current.PrimaryDriver = firstAppointmentEmployeeRow.FirstOrDefault()?.EmployeeID;
                }
            }
        }

        public virtual void ValidatePrimaryDriver()
        {
            PXResultset<FSAppointmentEmployee> fsAppointmentEmployeeRow = AppointmentServiceEmployees.Select();
            if (fsAppointmentEmployeeRow.Count > 0 &&
                fsAppointmentEmployeeRow.RowCast<FSAppointmentEmployee>().Where(_ => _.PrimaryDriver == true).Any() == false)
            {
                throw new PXException(TX.Messages.MISSING_PRIMARY_DRIVER);
            }
        }

        public virtual void UpdateLogActionViews(string type, bool fromStaffTab)
        {
            if (type == ID.Type_Log.TRAVEL)
            {
                if (AppointmentDetails.Current?.IsTravelItem == true)
                {
                    LogActionFilter.Current.DetLineRef = AppointmentDetails.Current.LineRef;
                }
                else
                {
                    LogActionFilter.Current.DetLineRef = SharedFunctions.GetItemLineRef(this, AppointmentRecords.Current.AppointmentID, true);
                }

                if (LogActionFilter.Current.DetLineRef != null)
                {
                    foreach (FSLogActionTravelDetail row in LogActionTravelDetails.Select())
                    {
                        row.Selected = row.DetLineRef == LogActionFilter.Current.DetLineRef;

                        if (LogActionTravelDetails.Cache.GetStatus(row) == PXEntryStatus.Notchanged)
                        {
                            LogActionTravelDetails.Cache.SetStatus(row, PXEntryStatus.Updated);
                        }
                    }
                }
            }
            else if (type == ID.Type_Log.SERVICE)
            {
                LogActionFilter.Current.DetLineRef = AppointmentDetails.Current?.LineType == ID.LineType_ALL.SERVICE ? AppointmentDetails.Current.LineRef : null;

                foreach (NoTravelLogInProcess row in LogActionServiceDetails.Select())
                {
                    if (fromStaffTab == false)
                    {
                        row.Selected = row.DetLineRef == LogActionFilter.Current.DetLineRef;
                    }
                    else
                    {
                        row.Selected = row.BAccountID == AppointmentServiceEmployees.Current?.EmployeeID;
                    }

                    if (LogActionServiceDetails.Cache.GetStatus(row) == PXEntryStatus.Notchanged)
                    {
                        LogActionServiceDetails.Cache.SetStatus(row, PXEntryStatus.Updated);
                    }
                }
            }
            else if (type == ID.Type_Log.STAFF_ASSIGMENT)
            {
                FSStaffLogActionDetail row = LogStaffActionDetails.Select()
                                                                      .RowCast<FSStaffLogActionDetail>()
                                                                      .Where(_ => _.LineRef == AppointmentServiceEmployees.Current.LineRef)
                                                                      .FirstOrDefault();

                if (row != null)
                {
                    foreach (FSStaffLogActionDetail selectedRow in LogStaffActionDetails.Select()
                                                                                    .RowCast<FSStaffLogActionDetail>()
                                                                                    .Where(_ => _.Selected == true))
                    {
                        selectedRow.Selected = false;

                        if (LogStaffActionDetails.Cache.GetStatus(selectedRow) == PXEntryStatus.Notchanged)
                        {
                            LogStaffActionDetails.Cache.SetStatus(selectedRow, PXEntryStatus.Updated);
                        }
                    }

                    row.Selected = row.BAccountID == AppointmentServiceEmployees.Current.EmployeeID;

                    if (LogStaffActionDetails.Cache.GetStatus(row) == PXEntryStatus.Notchanged)
                    {
                        LogStaffActionDetails.Cache.SetStatus(row, PXEntryStatus.Updated);
                    }
                }
            }
        }

        public virtual void SetFilterValues(string action, string type, bool fromStaffTab = false)
        {
            ClearLogActionsViewCaches();

            LogActionFilter.Cache.Clear();
            LogActionFilter.Cache.ClearQueryCache();

            LogActionFilter.Current.Action = action;
            LogActionFilter.Current.Type = type;

            UpdateLogActionViews(type, fromStaffTab);

            LogActionFilter.Current.LogTime = PXDBDateAndTimeAttribute.CombineDateTime(AppointmentRecords.Current.ExecutionDate, PXTimeZoneInfo.Now);
        }

        public virtual void VerifyBeforeTransaction(string action, string type)
        {
            int itemsSelectedCount = 0;
            bool canPerformAction = false;

            if (action == ID.LogActions.START)
            {
                if (type == ID.Type_Log.TRAVEL || type == ID.Type_Log.SERVICE)
                {
                    itemsSelectedCount = StaffMemberLogStartAction.Select().RowCast<FSAppointmentEmployeeFSLogStart>().Where(x => x.Selected == true).Count();
                    canPerformAction = LogActionFilter.Current.Me == true || itemsSelectedCount > 0;

                    if (type == ID.Type_Log.SERVICE && LogActionFilter.Current.DetLineRef == null)
                    {
                        LogActionFilter.Cache.RaiseExceptionHandling<FSLogActionFilter.detLineRef>(LogActionFilter.Current,
                                                                                                   LogActionFilter.Current.DetLineRef,
                                                                                                   new PXSetPropertyException(PXMessages.LocalizeFormatNoPrefix(TX.Error.FIELD_MAY_NOT_BE_EMPTY,
                                                                                                                                         PXUIFieldAttribute.GetDisplayName<FSLogActionFilter.detLineRef>(LogActionFilter.Cache))));

                        canPerformAction = false;
                    }
                }
                else if (type == ID.Type_Log.STAFF_ASSIGMENT)
                {
                    itemsSelectedCount = LogStaffActionDetails.Select().RowCast<FSStaffLogActionDetail>().Where(x => x.Selected == true).Count();
                    canPerformAction = itemsSelectedCount > 0;
                }
                else if (type == ID.Type_Log.SERV_BASED_ASSIGMENT)
                {
                    itemsSelectedCount = ServicesLogAction.Select().RowCast<FSDetailFSLogAction>().Where(x => x.Selected == true).Count();
                    canPerformAction = itemsSelectedCount > 0;
                }
            }
            else if (action == ID.LogActions.COMPLETE)
            {
                if (type == ID.Type_Log.TRAVEL)
                {
                    itemsSelectedCount = LogActionTravelDetails.Select().RowCast<FSLogActionTravelDetail>().Where(x => x.Selected == true).Count();
                }
                else if (type == ID.Type_Log.SERVICE || type == ID.Type_Log.STAFF_ASSIGMENT)
                {
                    itemsSelectedCount = LogActionServiceDetails.Select().RowCast<NoTravelLogInProcess>().Where(x => x.Selected == true).Count();
                }

                canPerformAction = itemsSelectedCount > 0;
            }

            if (canPerformAction == false)
            {
                LogActionFilter.Cache.RaiseExceptionHandling<FSLogActionFilter.action>(LogActionFilter.Current,
                                                                                       LogActionFilter.Current.Action,
                                                                                       new PXSetPropertyException(TX.Error.CANNOT_PERFORM_LOG_ACTION_RECORD_NOT_SELECTED));

                throw new PXRowPersistingException(null, null, TX.Error.CANNOT_PERFORM_LOG_ACTION_RECORD_NOT_SELECTED);
            }
        }

        public virtual void RunLogActionBase(string action, string type, FSAppointmentDet apptDet, PXSelectBase<FSAppointmentLog> logSelect, params object[] logSelectArgs)
        {
            if (action == ID.LogActions.START)
            {
                bool saveDocument = false;

                if (type == ID.Type_Log.TRAVEL)
                {
                    StartTravelAction();
                    saveDocument = LogRecords.Cache.IsDirty == true || AppointmentServiceEmployees.Cache.IsDirty;
                }
                else if (type == ID.Type_Log.SERVICE)
                {
                    StartServiceAction();
                    saveDocument = LogRecords.Cache.IsDirty == true;
                }
                else if (type == ID.Type_Log.NON_STOCK)
                {
                    StartNonStockAction(apptDet);
                    saveDocument = true;
                }
                else if (type == ID.Type_Log.STAFF_ASSIGMENT)
                {
                    StartStaffAction();
                    saveDocument = LogRecords.Cache.IsDirty == true || AppointmentServiceEmployees.Cache.IsDirty == true;
                }
                else if (type == ID.Type_Log.SERV_BASED_ASSIGMENT)
                {
                    StartServiceBasedOnAssignmentAction();
                    saveDocument = LogRecords.Cache.IsDirty == true || AppointmentServiceEmployees.Cache.IsDirty == true;
                }

                if(saveDocument == true)
                {
                    this.Actions.PressSave();
                }
            }
            else if (action == ID.LogActions.COMPLETE)
            {
                /***************************************************************/
                // Change this to use logSelect and logSelectArgs

                IEnumerable<ILogDetail> completeLogItems = null;

                if (type == ID.Type_Log.TRAVEL)
                {
                    completeLogItems = LogActionTravelDetails.Select().RowCast<FSLogActionTravelDetail>().Where(_ => _.Selected == true);
                }
                else if (type == ID.Type_Log.SERVICE || type == ID.Type_Log.STAFF_ASSIGMENT)
                {
                    completeLogItems = LogActionServiceDetails.Select().RowCast<NoTravelLogInProcess>().Where(_ => _.Selected == true);
                }

                /***************************************************************/

                CompleteAction(apptDet, completeLogItems, LogActionFilter.Current.LogTime, logSelect, logSelectArgs);

                if (type == ID.Type_Log.TRAVEL
                        && ServiceOrderTypeSelected.Current.OnTravelCompleteStartAppt == true
                            && (AppointmentRecords.Current?.Status == ID.Status_Appointment.AUTOMATIC_SCHEDULED
                                || AppointmentRecords.Current?.Status == ID.Status_Appointment.MANUAL_SCHEDULED))
                {
                    startAppointment.PressImpl();
                }
            }
        }

        public virtual void RunLogAction(string action, string type, FSAppointmentDet apptDet, PXSelectBase<FSAppointmentLog> logSelect, params object[] logSelectArgs)
        {
            if (type != ID.Type_Log.NON_STOCK)
            {
                VerifyBeforeTransaction(action, type);
            }

            if (this.SkipLongOperation == false)
            {
                PXLongOperation.StartOperation(this,
                delegate ()
                {
                    RunLogActionBase(action, type, apptDet, logSelect, logSelectArgs);
                });
            }
            else
            {
                RunLogActionBase(action, type, apptDet, logSelect, logSelectArgs);
            }
        }

        public virtual void StartTravelAction(IEnumerable<FSAppointmentEmployeeFSLogStart> createLogItems = null)
        {
            IEnumerable<FSAppointmentEmployeeFSLogStart> createLogItemsLocal = null;
            FSAppointmentLog fsAppointmentLogRow;
            FSAppointmentDet fsAppointmentDetRow = null;
            string detLineRef = null;
            DateTime? dateTimeBegin = null;
            int? timeDuration = 0;

            if (createLogItems == null)
            {
                detLineRef = LogActionFilter.Current?.DetLineRef;
                dateTimeBegin = LogActionFilter.Current?.LogTime;

                if (LogActionFilter.Current.DetLineRef != null)
                {
                    fsAppointmentDetRow = PXSelect<FSAppointmentDet,
                                          Where<
                                              FSAppointmentDet.appointmentID, Equal<Required<FSAppointmentDet.appointmentID>>,
                                              And<FSAppointmentDet.lineRef, Equal<Required<FSAppointmentDet.lineRef>>>>>
                                          .Select(this, AppointmentRecords.Current.AppointmentID, LogActionFilter.Current.DetLineRef);

                    timeDuration = fsAppointmentDetRow != null ? fsAppointmentDetRow.EstimatedDuration : 0;

                }

                if (LogActionFilter.Current.Me == true)
                {
                    EPEmployee employeeByUserID = PXSelect<EPEmployee,
                                                  Where<
                                                      EPEmployee.userID, Equal<Current<AccessInfo.userID>>>>
                                                  .Select(this);

                    if (employeeByUserID != null)
                    {
                        bool isEmployeeInGrid = AppointmentServiceEmployees.Select().RowCast<FSAppointmentEmployee>()
                                                                           .Where(x => x.EmployeeID == employeeByUserID.BAccountID)
                                                                           .Count() > 0;

                        bool isTherePrimaryDriver = AppointmentServiceEmployees.Select().RowCast<FSAppointmentEmployee>()
                                                                               .Where(x => x.PrimaryDriver == true)
                                                                               .Count() > 0;

                        if (isEmployeeInGrid == false)
                        {
                            FSAppointmentEmployee fsAppointmentEmployeeRow = new FSAppointmentEmployee()
                            {
                                EmployeeID = employeeByUserID.BAccountID,
                            };

                            if (isTherePrimaryDriver == false)
                            {
                                fsAppointmentEmployeeRow.PrimaryDriver = true;
                            }

                            AppointmentServiceEmployees.Cache.Insert(fsAppointmentEmployeeRow);
                        }


                        fsAppointmentLogRow = new FSAppointmentLog()
                        {
                            BAccountID = employeeByUserID.BAccountID,
                            Type = ID.Type_Log.TRAVEL,
                            DetLineRef = detLineRef,
                            TimeDuration = timeDuration,
                            DateTimeBegin = dateTimeBegin
                        };

                        LogRecords.Cache.Insert(fsAppointmentLogRow);
                    }
                }
                else
                {
                    createLogItemsLocal = StaffMemberLogStartAction.Select().RowCast<FSAppointmentEmployeeFSLogStart>()
                                                                   .Where(x => x.Selected == true);
                }
            }
            else
            {
                detLineRef = null;
                dateTimeBegin = PXDBDateAndTimeAttribute.CombineDateTime(AppointmentRecords.Current.ExecutionDate, PXTimeZoneInfo.Now);
                createLogItemsLocal = createLogItems;
            }

            if (createLogItemsLocal != null)
            {
                foreach (FSAppointmentEmployeeFSLogStart row in createLogItemsLocal)
                {
                    fsAppointmentLogRow = new FSAppointmentLog()
                    {
                        BAccountID = row.BAccountID,
                        Type = ID.Type_Log.TRAVEL,
                        DetLineRef = detLineRef,
                        TimeDuration = timeDuration,
                        DateTimeBegin = dateTimeBegin
                    };

                    LogRecords.Cache.Insert(fsAppointmentLogRow);
                }
            }
        }

        public virtual void StartServiceAction(IEnumerable<FSAppointmentEmployeeFSLogStart> createLogItems = null)
        {
            IEnumerable<FSAppointmentEmployeeFSLogStart> createLogItemsLocal = null;
            FSAppointmentLog fsAppointmentLogRow;
            FSAppointmentDet fsAppointmentDetRow = null;
            string detLineRef = null;
            DateTime? dateTimeBegin = null;
            int timeDuration = 0;

            if (LogActionFilter.Current?.DetLineRef == null)
                return;

            if (createLogItems == null)
            {
                detLineRef = LogActionFilter.Current?.DetLineRef;
                dateTimeBegin = LogActionFilter.Current?.LogTime;

                if (LogActionFilter.Current.DetLineRef != null)
                {
                    fsAppointmentDetRow = PXSelect<FSAppointmentDet,
                                          Where<
                                              FSAppointmentDet.appointmentID, Equal<Required<FSAppointmentDet.appointmentID>>,
                                              And<FSAppointmentDet.lineRef, Equal<Required<FSAppointmentDet.lineRef>>>>>
                                          .Select(this, AppointmentRecords.Current.AppointmentID, LogActionFilter.Current.DetLineRef);

                    if (fsAppointmentDetRow != null)
                        timeDuration = fsAppointmentDetRow.EstimatedDuration ?? 0;
                }

                if (LogActionFilter.Current.Me == true)
                {
                    EPEmployee employeeByUserID = PXSelect<EPEmployee,
                                                  Where<
                                                      EPEmployee.userID, Equal<Current<AccessInfo.userID>>>>
                                                  .Select(this);

                    if (employeeByUserID != null)
                    {
                        bool isEmployeeInGrid = AppointmentServiceEmployees.Select().RowCast<FSAppointmentEmployee>()
                                                                           .Where(x => x.EmployeeID == employeeByUserID.BAccountID)
                                                                           .Count() > 0;

                        bool isTherePrimaryDriver = AppointmentServiceEmployees.Select().RowCast<FSAppointmentEmployee>()
                                                                               .Where(x => x.PrimaryDriver == true)
                                                                               .Count() > 0;

                        if (isEmployeeInGrid == false)
                        {
                            FSAppointmentEmployee fsAppointmentEmployeeRow = new FSAppointmentEmployee()
                            {
                                EmployeeID = employeeByUserID.BAccountID,
                            };

                            if (isTherePrimaryDriver == false)
                            {
                                fsAppointmentEmployeeRow.PrimaryDriver = true;
                            }

                            AppointmentServiceEmployees.Cache.Insert(fsAppointmentEmployeeRow);
                        }

                        fsAppointmentLogRow = new FSAppointmentLog()
                        {
                            Type = ID.Type_Log.SERVICE,
                            BAccountID = employeeByUserID.BAccountID,
                            DetLineRef = detLineRef,
                            DateTimeBegin = dateTimeBegin,
                            TimeDuration = timeDuration
                        };

                        LogRecords.Cache.Insert(fsAppointmentLogRow);
                    }
                }
                else
                {
                    createLogItemsLocal = StaffMemberLogStartAction.Select().RowCast<FSAppointmentEmployeeFSLogStart>()
                                                                   .Where(x => x.Selected == true);
                }
            }
            else
            {
                detLineRef = null;
                dateTimeBegin = PXDBDateAndTimeAttribute.CombineDateTime(AppointmentRecords.Current.ExecutionDate, PXTimeZoneInfo.Now);
                createLogItemsLocal = createLogItems;
            }

            if (createLogItemsLocal != null)
            {
                foreach (FSAppointmentEmployeeFSLogStart row in createLogItemsLocal)
                {
                    fsAppointmentLogRow = new FSAppointmentLog()
                    {
                        Type = ID.Type_Log.SERVICE,
                        BAccountID = row.BAccountID,
                        DetLineRef = detLineRef,
                        DateTimeBegin = dateTimeBegin,
                        TimeDuration = timeDuration
                    };

                    LogRecords.Cache.Insert(fsAppointmentLogRow);
                }
            }
        }

        public virtual void StartNonStockAction(FSAppointmentDet fsAppointmentDet)
        {
            if (fsAppointmentDet == null)
                return;

            FSAppointmentLog fsAppointmentLogRow = new FSAppointmentLog()
            {
                Type = ID.Type_Log.NON_STOCK,
                BAccountID = null,
                DetLineRef = fsAppointmentDet.LineRef,
                DateTimeBegin = PXDBDateAndTimeAttribute.CombineDateTime(AppointmentRecords.Current.ExecutionDate, PXTimeZoneInfo.Now),
                TimeDuration = fsAppointmentDet.EstimatedDuration ?? 0,
                Status = ID.Status_Log.IN_PROCESS,
                TrackTime = false,
                Descr = fsAppointmentDet.TranDesc,
                TrackOnService = true,
                ProjectID = fsAppointmentDet.ProjectID,
                ProjectTaskID = fsAppointmentDet.ProjectTaskID,
                CostCodeID = fsAppointmentDet.CostCodeID
            };

            LogRecords.Cache.Insert(fsAppointmentLogRow);

            this.Actions.PressSave();
        }

        public virtual void CompleteAction(FSAppointmentDet apptDet, IEnumerable<ILogDetail> completeLogItems, DateTime? dateTimeEnd, PXSelectBase<FSAppointmentLog> logSelect, params object[] logSelectArgs)
        {
            int rowsAffected = CompleteMultipleLogs(dateTimeEnd, ID.Status_AppointmentDet.COMPLETED, apptDet != null ? false : true, completeLogItems, logSelect, logSelectArgs);

            if (apptDet != null)
            {
                rowsAffected += ProcessStopItemLine(apptDet, ID.Status_AppointmentDet.COMPLETED);
            }

            if (rowsAffected > 0)
            {
                this.Actions.PressSave();
            }
        }

        public virtual int CompleteMultipleLogs(DateTime? dateTimeEnd, string newApptDetStatus, bool completeRelatedItemLines, IEnumerable<ILogDetail> completeLogItems, PXSelectBase<FSAppointmentLog> logSelect, params object[] logSelectArgs)
        {
            if (dateTimeEnd == null)
            {
                dateTimeEnd = PXTimeZoneInfo.Now;
            }

            int rowsAffected = 0;
            List<FSAppointmentDet> apptDetRows = null;

            if (completeRelatedItemLines == true)
            {
                apptDetRows = AppointmentDetails.Select().RowCast<FSAppointmentDet>().ToList();
            }

            if (completeLogItems != null)
            {
                var logRows = LogRecords.Select();

                foreach (ILogDetail row in completeLogItems)
                {
                    FSAppointmentLog logRow = logRows.RowCast<FSAppointmentLog>().Where(_ => _.LineRef == row.LineRef).FirstOrDefault();

                    ProcessCompleteLog(dateTimeEnd.Value, newApptDetStatus, logRow, apptDetRows);
                    rowsAffected++;
                }
            }

            if (logSelect != null)
            {
                foreach (FSAppointmentLog logRow in logSelect.Select(logSelectArgs))
                {
                    ProcessCompleteLog(dateTimeEnd.Value, newApptDetStatus, logRow, apptDetRows);
                    rowsAffected++;
                }
            }

            return rowsAffected;
        }

        public virtual FSAppointmentLog ProcessCompleteLog(DateTime dateTimeEnd, string newApptDetStatus, FSAppointmentLog logRow, List<FSAppointmentDet> apptDetRows)
        {
            if (logRow == null)
                return null;

            logRow = PXCache<FSAppointmentLog>.CreateCopy(logRow);

            logRow.Status = ID.Status_Log.COMPLETED;
            if (logRow.KeepDateTimes == false)
            {
                logRow.DateTimeEnd = dateTimeEnd;
            }

            if (apptDetRows != null && string.IsNullOrWhiteSpace(logRow.DetLineRef) == false)
            {
                FSAppointmentDet apptDet = apptDetRows.Where(r => r.LineRef == logRow.DetLineRef).FirstOrDefault();
                if (apptDet != null)
                {
                    ProcessStopItemLine(apptDet, newApptDetStatus);
                }
            }

            return (FSAppointmentLog)LogRecords.Cache.Update(logRow);
        }

        public virtual void StartStaffAction()
        {
            StartStaffAction(null, null);
        }

        [Obsolete("This method will be deleted in the next major release.")]
        public virtual void StartStaffAction(IEnumerable<FSStaffLogActionDetail> createLogItems)
        {
            StartStaffAction(createLogItems, null);
        }

        public virtual void StartStaffAction(IEnumerable<FSStaffLogActionDetail> createLogItems, DateTime? startTime)
        {
            IEnumerable<FSStaffLogActionDetail> createLogItemsLocal = null;

            startTime = startTime ?? LogActionFilter.Current.LogTime;

            if (createLogItems == null)
            {
                if (LogActionFilter.Current.Me == true)
                {
                    EPEmployee employeeByUserID = PXSelect<EPEmployee,
                                                  Where<
                                                      EPEmployee.userID, Equal<Current<AccessInfo.userID>>>>
                                                  .Select(this);

                    if (employeeByUserID != null)
                    {
                        bool isEmployeeInGrid = AppointmentServiceEmployees.Select().RowCast<FSAppointmentEmployee>()
                                                                           .Where(x => x.EmployeeID == employeeByUserID.BAccountID)
                                                                           .Count() > 0;

                        bool isTherePrimaryDriver = AppointmentServiceEmployees.Select().RowCast<FSAppointmentEmployee>()
                                                                               .Where(x => x.PrimaryDriver == true)
                                                                               .Count() > 0;

                        if (isEmployeeInGrid == false)
                        {
                            FSAppointmentEmployee fsAppointmentEmployeeRow = new FSAppointmentEmployee()
                            {
                                EmployeeID = employeeByUserID.BAccountID
                            };

                            if (isTherePrimaryDriver == false)
                            {
                                fsAppointmentEmployeeRow.PrimaryDriver = true;
                            }

                            AppointmentServiceEmployees.Cache.Insert(fsAppointmentEmployeeRow);

                            FSAppointmentLog fsAppointmentLogRow = new FSAppointmentLog()
                            {
                                Type = ID.Type_Log.STAFF_ASSIGMENT,
                                BAccountID = employeeByUserID.BAccountID,
                                DetLineRef = null,
                                DateTimeBegin = startTime
                            };

                            LogRecords.Cache.Insert(fsAppointmentLogRow);
                        }
                        else
                        {
                            createLogItemsLocal = LogStaffActionDetails.Select().RowCast<FSStaffLogActionDetail>()
                                                                       .Where(x => x.Selected == true);
                        }
                    }
                }
                else
                {
                    createLogItemsLocal = LogStaffActionDetails.Select().RowCast<FSStaffLogActionDetail>()
                                                               .Where(x => x.Selected == true);
                }
            }
            else
            {
                createLogItemsLocal = createLogItems;
            }

            if (createLogItemsLocal != null)
            {
                foreach (FSStaffLogActionDetail fsStaffLogActionDetailRow in createLogItemsLocal)
                {
                    int? timeDuration = fsStaffLogActionDetailRow != null && fsStaffLogActionDetailRow.EstimatedDuration != null ? fsStaffLogActionDetailRow.EstimatedDuration : 0;

                    FSAppointmentLog fsAppointmentLogRow = new FSAppointmentLog()
                    {
                        Type = ID.Type_Log.STAFF_ASSIGMENT,
                        BAccountID = fsStaffLogActionDetailRow.BAccountID,
                        DetLineRef = fsStaffLogActionDetailRow.DetLineRef,
                        DateTimeBegin = startTime,
                        TimeDuration = timeDuration
                    };

                    LogRecords.Cache.Insert(fsAppointmentLogRow);
                }
            }
        }

        public virtual void StartServiceBasedOnAssignmentAction()
        {
            StartServiceBasedOnAssignmentAction(null, null);
        }

        [Obsolete("This method will be deleted in the next major release.")]
        public virtual void StartServiceBasedOnAssignmentAction(IEnumerable<FSDetailFSLogAction> createLogItems)
        {
            StartServiceBasedOnAssignmentAction(createLogItems, null);
        }

        public virtual void StartServiceBasedOnAssignmentAction(IEnumerable<FSDetailFSLogAction> createLogItems, DateTime? startTime)
        {
            IEnumerable<FSDetailFSLogAction> createLogItemsLocal = null;
            FSAppointmentLog fsAppointmentLogRow;

            if (createLogItems == null)
            {
                createLogItemsLocal = ServicesLogAction.Select().RowCast<FSDetailFSLogAction>().Where(x => x.Selected == true);
            }
            else
            {
                createLogItemsLocal = createLogItems;
            }

            startTime = startTime ?? LogActionFilter.Current.LogTime;

            if (createLogItemsLocal != null)
            {
                foreach (FSDetailFSLogAction fsDetailLogActionRow in createLogItemsLocal)
                {
                    var employeesRelatedToService = AppointmentServiceEmployees.Select().RowCast<FSAppointmentEmployee>()
                                                                               .Where(x => x.ServiceLineRef == fsDetailLogActionRow.LineRef);

                    if (employeesRelatedToService.Count() > 0)
                    {
                        foreach (FSAppointmentEmployee employeeRow in employeesRelatedToService)
                        {
                            int? timeDuration = employeeRow != null && fsDetailLogActionRow.EstimatedDuration != null ? fsDetailLogActionRow.EstimatedDuration : 0;

                            fsAppointmentLogRow = new FSAppointmentLog()
                            {
                                Type = ID.Type_Log.STAFF_ASSIGMENT,
                                BAccountID = employeeRow.EmployeeID,
                                DetLineRef = employeeRow.ServiceLineRef,
                                DateTimeBegin = startTime,
                                TimeDuration = fsDetailLogActionRow.EstimatedDuration
                            };

                            LogRecords.Cache.Insert(fsAppointmentLogRow);
                        }
                    }
                    else
                    {
                        fsAppointmentLogRow = new FSAppointmentLog()
                        {
                            Type = ID.Type_Log.SERVICE,
                            BAccountID = null,
                            DetLineRef = fsDetailLogActionRow.LineRef,
                            DateTimeBegin = startTime,
                            TimeDuration = fsDetailLogActionRow.EstimatedDuration
                        };

                        LogRecords.Cache.Insert(fsAppointmentLogRow);
                    }
                }
            }
        }

        public virtual void SetVisibleCompleteLogActionGrid(FSLogActionFilter filter)
        {
            LogActionTravelDetails.Cache.AllowSelect = filter.Action == ID.LogActions.COMPLETE && filter.Type == ID.Type_Log.TRAVEL;
            LogActionServiceDetails.Cache.AllowSelect = filter.Action == ID.LogActions.COMPLETE && filter.Type != ID.Type_Log.TRAVEL;
        }

        public virtual void ClearLogActionsViewCaches()
        {
            LogActionTravelDetails.Cache.Clear();
            LogActionTravelDetails.Cache.ClearQueryCache();

            LogActionServiceDetails.Cache.Clear();
            LogActionServiceDetails.Cache.ClearQueryCache();

            LogStaffActionDetails.Cache.Clear();
            LogStaffActionDetails.Cache.ClearQueryCache();

            StaffMemberLogStartAction.Cache.Clear();
            StaffMemberLogStartAction.Cache.ClearQueryCache();
        }

        public virtual void VerifyOnCompleteApptRequireLog(PXCache cache)
        {
            int? servicesWithOutLog = AppointmentDetails.Select().RowCast<FSAppointmentDet>()
                                        .Where(_ => _.LineType == ID.LineType_ALL.SERVICE 
                                                    && _.IsTravelItem == false
                                                    && _.IsCanceledNotPerformed == false
                                                    && _.LogRelatedCount == 0).Count();

            if (servicesWithOutLog > 0)
            {
                throw new PXException(TX.Error.LOG_DATE_TIMES_ARE_REQUIRED_FOR_SERVICES);
            }
        }

        public virtual void VerifyLogActionBaseOnAppointmentStatus(FSAppointment fsAppointmentRow, string type)
        {
            if (fsAppointmentRow.Status == ID.Status_Appointment.AUTOMATIC_SCHEDULED
                || fsAppointmentRow.Status == ID.Status_Appointment.MANUAL_SCHEDULED
                || fsAppointmentRow.Status == ID.Status_Appointment.COMPLETED)
            {
                if (type != ID.Type_Log.TRAVEL)
                {
                    throw new PXException(PXMessages.LocalizeFormatNoPrefix(TX.Error.LOG_ACTION_NOT_ALLOWED_BY_STATUS
                                            , TX.Status_Appointment.AUTOMATIC_SCHEDULED
                                            , TX.Status_Appointment.MANUAL_SCHEDULED
                                            , TX.Status_Appointment.COMPLETED));
                }
            }
        }

        public virtual void VerifyLogActionBaseTypeAndServiceSelected(string logActionType, string detLineRef, string logActionTypeName)
        {
            if ((logActionType == ID.Type_Log.SERVICE
                    || logActionType == ID.Type_Log.TRAVEL)
                && detLineRef == null)

            {
                throw new PXException(PXMessages.LocalizeFormatNoPrefix(TX.Error.LOG_ACTION_NOT_ALLOWED, logActionTypeName));
            }
        }

        public virtual string GetItemStatusFromLog(FSAppointmentDet appointmentDet)
        {
            if (appointmentDet?.Status == ID.Status_AppointmentDet.NOT_FINISHED)
            {
                return ID.Status_AppointmentDet.NOT_FINISHED;
            }

            if (appointmentDet != null &&
                (appointmentDet.LineType == ID.LineType_ALL.SERVICE ||
                 appointmentDet.LineType == ID.LineType_ALL.NONSTOCKITEM))
            {
                IEnumerable<FSAppointmentLog> itemLogRecords = LogRecords.Select().RowCast<FSAppointmentLog>().Where(_ => _.DetLineRef == appointmentDet.LineRef);
                bool anyInProcess = itemLogRecords.Where(_ => _.Status == ID.Status_Log.IN_PROCESS).Any();

                if (anyInProcess == false)
                {
                    if (itemLogRecords.Count() > 0)
                    {
                        if (appointmentDet.Status == ID.Status_AppointmentDet.COMPLETED ||
                            appointmentDet.Status == ID.Status_AppointmentDet.NOT_FINISHED)
                        {
                            return appointmentDet.Status;
                        }
                        else
                        { 
                            return ID.Status_AppointmentDet.COMPLETED;
                        }
                    }
                }
                else
                {
                    return ID.Status_AppointmentDet.IN_PROCESS;
                }
            }

            return ID.Status_AppointmentDet.NOT_STARTED;
        }
        #endregion
    }
}
