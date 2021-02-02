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
        public virtual void SetHeaderActualDateTimeBegin(PXCache cache, FSAppointment fsAppointmentRow, DateTime? dateTimeBegin)
        {
            if (fsAppointmentRow != null && fsAppointmentRow.HandleManuallyActualTime == false)
            {
                cache.SetValueExtIfDifferent<FSAppointment.executionDate>(fsAppointmentRow, dateTimeBegin.Value.Date);
                cache.SetValueExtIfDifferent<FSAppointment.actualDateTimeBegin>(fsAppointmentRow, dateTimeBegin);
            }
        }

        public virtual void SetHeaderActualDateTimeEnd(PXCache cache, FSAppointment fsAppointmentRow, DateTime? dateTimeEnd)
        {
            if (fsAppointmentRow != null && fsAppointmentRow.HandleManuallyActualTime == false)
            {
                cache.SetValueExtIfDifferent<FSAppointment.actualDateTimeEnd>(fsAppointmentRow, dateTimeEnd);
                cache.SetValueExtIfDifferent<FSAppointment.handleManuallyActualTime>(fsAppointmentRow, true);
            }
        }

        public virtual bool ActualDateAndTimeValidation(FSAppointment fsAppointmentRow)
        {
            return fsAppointmentRow.ActualDateTimeBegin != null && fsAppointmentRow.ActualDateTimeEnd != null;
        }
        #endregion

        #region AppointmentDet methods
        public virtual string GetValidAppDetStatus(FSAppointmentDet row, string newStatus) 
        {
            if (newStatus != ListField_Status_AppointmentDet.CANCELED
                && newStatus != ListField_Status_AppointmentDet.WaitingForPO
                && newStatus != ListField_Status_AppointmentDet.RequestForPO)
            {
                if (row.ShouldBeWaitingPO == true)
                {
                    return ListField_Status_AppointmentDet.WaitingForPO;
                }
                else if (row.ShouldBeRequestPO == true)
                {
                    return ListField_Status_AppointmentDet.RequestForPO;
                }
            }

            return newStatus;
        }
        
        public virtual void ForceAppointmentDetActualFieldsUpdate(bool reopeningAppointment)
        {
            foreach (FSAppointmentDet row in AppointmentDetails.Select().RowCast<FSAppointmentDet>().Where(r =>r.IsExpenseReceiptItem == false))
            {
                if (reopeningAppointment == true 
                    && row.Status != FSAppointmentDet.status.NOT_STARTED
                    && row.Status != FSAppointmentDet.status.WaitingForPO
                    && row.Status != FSAppointmentDet.status.RequestForPO)
                {
                    ChangeItemLineStatus(row, FSAppointmentDet.status.NOT_STARTED);
                }

                FSAppointmentDet copy = (FSAppointmentDet)AppointmentDetails.Cache.CreateCopy(row);

                AppointmentDetails.Cache.SetDefaultExt<FSAppointmentDet.areActualFieldsActive>(copy);

                if (AppointmentDetails.Cache.ObjectsEqual<FSAppointmentDet.curyEstimatedTranAmt,
                        FSAppointmentDet.actualDuration, FSAppointmentDet.actualQty, FSAppointmentDet.curyTranAmt,
                        FSAppointmentDet.curyExtPrice>(row, copy) == false)
                {
                    AppointmentDetails.Update(copy);
                }
            }
        }

        public virtual void OnApptStartTimeChangeUpdateLogStartTime(FSAppointment fsAppointmentRow, FSSrvOrdType fsSrvOrdTypeRow,
                                                                    AppointmentCore.AppointmentLog_View logRecords)
        {
            if (fsAppointmentRow != null && fsSrvOrdTypeRow?.OnStartTimeChangeUpdateLogStartTime == true)
            {
                foreach (FSAppointmentLog fsAppointmentLogRow in logRecords.Select().RowCast<FSAppointmentLog>()
                                                                            .Where(_ => _.ItemType != FSAppointmentLog.itemType.Values.Travel
                                                                                     && _.DateTimeBegin.HasValue == true)
                                                                            .GroupBy(_ => new { _.BAccountID, _.DetLineRef })
                                                                            .Select(_ => _.OrderBy(d => d.DateTimeBegin).First()))
                {
                    if (fsAppointmentLogRow.DateTimeBegin == fsAppointmentRow.ActualDateTimeBegin)
                        continue;

                    if (fsAppointmentLogRow.KeepDateTimes == true)
                        continue;

                    FSAppointmentLog copy = (FSAppointmentLog)logRecords.Cache.CreateCopy(fsAppointmentLogRow);
                    copy.DateTimeBegin = fsAppointmentRow.ActualDateTimeBegin;

                    if (copy.DateTimeEnd < copy.DateTimeBegin)
                        copy.DateTimeEnd = copy.DateTimeBegin;

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
                                                                            .Where(_ => _.ItemType != FSAppointmentLog.itemType.Values.Travel
                                                                                     && _.DateTimeEnd.HasValue == true)
                                                                            .GroupBy(_ => new { _.BAccountID, _.DetLineRef })
                                                                            .Select(_=> _.OrderByDescending(d => d.DateTimeEnd).First()))
                {
                    if (fsAppointmentLogRow.DateTimeEnd == fsAppointmentRow.ActualDateTimeEnd)
                        continue;

                    if (fsAppointmentLogRow.KeepDateTimes == true)
                        continue;

                    FSAppointmentLog copy = (FSAppointmentLog)logRecords.Cache.CreateCopy(fsAppointmentLogRow);
                    copy.DateTimeEnd = fsAppointmentRow.ActualDateTimeEnd;

                    if (copy.DateTimeBegin > copy.DateTimeEnd)
                        copy.DateTimeBegin = copy.DateTimeEnd;

                    logRecords.Cache.Update(copy);
                }
            }
        }
        #endregion

        #region TravelTimeAndLogFunctions

        public virtual void ClearAppointmentLog()
        {
            foreach (FSAppointmentLog fsAppointmentLogRow in LogRecords.Select().RowCast<FSAppointmentLog>().Where(_ => _.ItemType != FSAppointmentLog.itemType.Values.Travel))
            {
                LogRecords.Delete(fsAppointmentLogRow);
            }
        }

        public virtual void SetLogInfoFromDetails(PXCache cache, FSAppointmentLog fsLogRow)
        {
            FSAppointmentDet apptDet = null;

            if (string.IsNullOrWhiteSpace(fsLogRow.DetLineRef) == false)
            {
                apptDet = AppointmentDetails.Select().RowCast<FSAppointmentDet>().Where(_ => _.LineRef == fsLogRow.DetLineRef).FirstOrDefault();
            }

            if (fsLogRow.BAccountID == null)
            {
                fsLogRow.EarningType = null;
                fsLogRow.LaborItemID = null;

                if (apptDet != null
                    && (apptDet.LineType == ID.LineType_ALL.SERVICE ||
                        apptDet.LineType == ID.LineType_ALL.NONSTOCKITEM))
                {
                    fsLogRow.ProjectID = apptDet.ProjectID;
                    fsLogRow.ProjectTaskID = apptDet.ProjectTaskID;
                    fsLogRow.CostCodeID = apptDet.CostCodeID;
                }
            }

            if (apptDet != null)
            {
                fsLogRow.Descr = apptDet.TranDesc;
            }

            string logType = null;

            if (apptDet == null && fsLogRow.Travel == true)
            {
                logType = FSAppointmentLog.itemType.Values.Travel;
            }
            else
            {
                logType = GetLogTypeCheckingTravelWithLogFormula(cache, apptDet);
            }

            cache.SetValueExt<FSAppointmentLog.itemType>(fsLogRow, logType);
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

        public virtual void SetLogActionDefaultSelection(string type, bool fromStaffTab)
        {
            IEnumerable<FSAppointmentLogExtItemLine> logRows = null;

            if (type == FSAppointmentLogExtItemLine.itemType.Values.Travel || type == FSAppointmentLogExtItemLine.itemType.Values.Service)
            {
                logRows = LogActionLogRecords.Select().RowCast<FSAppointmentLogExtItemLine>();
            }

            if (type == FSAppointmentLogExtItemLine.itemType.Values.Travel)
            {
                if (AppointmentDetails.Current?.IsTravelItem == true
                    && CanLogBeStarted(AppointmentDetails.Current) == true)
                {
                    LogActionFilter.Current.DetLineRef = AppointmentDetails.Current.LineRef;
                }
                else
                {
                    LogActionFilter.Current.DetLineRef = SharedFunctions.GetItemLineRef(this, AppointmentRecords.Current.AppointmentID, true);
                }

                foreach (FSAppointmentLogExtItemLine row in logRows)
                {
                    row.Selected = row.DetLineRef == LogActionFilter.Current.DetLineRef || LogActionFilter.Current.DetLineRef == null;
                    LogActionLogRecords.Update(row);
                }
            }
            else if (type == FSAppointmentLogExtItemLine.itemType.Values.Service)
            {
                LogActionFilter.Current.DetLineRef = AppointmentDetails.Current?.LineType == ID.LineType_ALL.SERVICE ? AppointmentDetails.Current.LineRef : null;

                foreach (FSAppointmentLogExtItemLine row in logRows)
                {
                    if (fromStaffTab == false)
                    {
                        row.Selected = row.DetLineRef == LogActionFilter.Current.DetLineRef;
                    }
                    else
                    {
                        row.Selected = row.BAccountID == AppointmentServiceEmployees.Current?.EmployeeID;
                    }

                    LogActionLogRecords.Update(row);
                }
            }
            else if (type == FSAppointmentLogExtItemLine.itemType.Values.Staff)
            {
                FSAppointmentStaffExtItemLine row = LogActionStaffRecords.Select()
                                                                  .RowCast<FSAppointmentStaffExtItemLine>()
                                                                  .Where(_ => _.LineRef == AppointmentServiceEmployees.Current.LineRef)
                                                                  .FirstOrDefault();

                if (row != null)
                {
                    foreach (FSAppointmentStaffExtItemLine selectedRow in LogActionStaffRecords.Select()
                                                                                    .RowCast<FSAppointmentStaffExtItemLine>()
                                                                                    .Where(_ => _.Selected == true))
                    {
                        selectedRow.Selected = false;
                        LogActionStaffRecords.Update(row);
                    }

                    row.Selected = row.BAccountID == AppointmentServiceEmployees.Current.EmployeeID;
                    LogActionStaffRecords.Update(row);
                }
            }
        }
        
        public virtual void SetLogActionPanelDefaults(string dfltAction, string dfltLogType, bool fromStaffTab = false)
        {
            ClearLogActionsViewCaches();
            LogActionFilter.Cache.Clear();
            LogActionFilter.Cache.ClearQueryCache();

            LogActionFilter.Current.Action = dfltAction;
            LogActionFilter.Current.Type = dfltLogType;

            SetLogActionDefaultSelection(dfltLogType, fromStaffTab);

            LogActionFilter.Current.LogDateTime = PXDBDateAndTimeAttribute.CombineDateTime(Accessinfo.BusinessDate, PXTimeZoneInfo.Now);
        }

        public virtual void VerifyBeforeAction(string action, string type)
        {
            int itemsSelectedCount = 0;
            bool canPerformAction = false;

            if (action == ID.LogActions.START)
            {
                if (type == FSLogActionFilter.type.Values.Travel || type == FSLogActionFilter.type.Values.Service)
                {
                    itemsSelectedCount = LogActionStaffDistinctRecords.Select().RowCast<FSAppointmentStaffDistinct>().Where(x => x.Selected == true).Count();
                    canPerformAction = LogActionFilter.Current.Me == true || itemsSelectedCount > 0;

                    if (type == FSLogActionFilter.type.Values.Service && LogActionFilter.Current.DetLineRef == null)
                    {
                        LogActionFilter.Cache.RaiseExceptionHandling<FSLogActionFilter.detLineRef>(LogActionFilter.Current,
                                                                                                   LogActionFilter.Current.DetLineRef,
                                                                                                   new PXSetPropertyException(PXMessages.LocalizeFormatNoPrefix(PX.Data.ErrorMessages.FieldIsEmpty,
                                                                                                                                         PXUIFieldAttribute.GetDisplayName<FSLogActionFilter.detLineRef>(LogActionFilter.Cache))));

                        canPerformAction = false;
                    }
                }
                else if (type == FSLogActionFilter.type.Values.Staff)
                {
                    itemsSelectedCount = LogActionStaffRecords.Select().RowCast<FSAppointmentStaffExtItemLine>().Where(x => x.Selected == true).Count();
                    bool isLogMobile = Accessinfo.ScreenID == SharedFunctions.SetScreenIDToDotFormat(ID.ScreenID.LOG_ACTION_START_STAFF_MOBILE);
                    canPerformAction = itemsSelectedCount > 0 || (isLogMobile && LogActionFilter.Current.Me == true);
                }
                else if (type == FSLogActionFilter.type.Values.ServBasedAssignment)
                {
                    itemsSelectedCount = ServicesLogAction.Select().RowCast<FSDetailFSLogAction>().Where(x => x.Selected == true).Count();
                    canPerformAction = itemsSelectedCount > 0;
                }
            }
            else if (action == ID.LogActions.COMPLETE || action == ID.LogActions.PAUSE || action == ID.LogActions.RESUME)
            {
                itemsSelectedCount = LogActionLogRecords.Select().RowCast<FSAppointmentLogExtItemLine>().Where(x => x.Selected == true).Count();
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

        public virtual void RunLogActionBase(string action, string logType, FSAppointmentDet apptDet, PXSelectBase<FSAppointmentLog> logSelect, params object[] logSelectArgs)
        {
            if (action == ID.LogActions.START)
            {
                bool saveDocument = false;

                if (logType == FSLogActionFilter.type.Values.Travel)
                {
                    StartTravelAction();
                    saveDocument = LogRecords.Cache.IsDirty == true || AppointmentServiceEmployees.Cache.IsDirty;
                }
                else if (logType == FSLogActionFilter.type.Values.Service)
                {
                    StartServiceAction();
                    saveDocument = LogRecords.Cache.IsDirty == true;
                }
                else if (logType == FSLogActionFilter.type.Values.NonStock)
                {
                    StartNonStockAction(apptDet);
                    saveDocument = true;
                }
                else if (logType == FSLogActionFilter.type.Values.Staff)
                {
                    StartStaffAction();
                    saveDocument = LogRecords.Cache.IsDirty == true || AppointmentServiceEmployees.Cache.IsDirty == true;
                }
                else if (logType == FSLogActionFilter.type.Values.ServBasedAssignment)
                {
                    StartServiceBasedOnAssignmentAction();
                    saveDocument = LogRecords.Cache.IsDirty == true || AppointmentServiceEmployees.Cache.IsDirty == true;
                }

                if(saveDocument == true)
                {
                    this.Actions.PressSave();
                }
            }
            else if (action == ID.LogActions.COMPLETE || action == ID.LogActions.PAUSE || action == ID.LogActions.RESUME)
            {
                List<FSAppointmentLog> logList = null;

                if (logSelect != null)
                {
                    logList = new List<FSAppointmentLog>();

                    foreach (FSAppointmentLog logRow in logSelect.Select(logSelectArgs))
                    {
                        logList.Add(logRow);
                    }
                }
                else
                {
                    logList = LogActionLogRecords.Select()
                                                     .RowCast<FSAppointmentLogExtItemLine>()
                                                     .Where(_ => _.Selected == true)
                                                     .ToList<FSAppointmentLog>();
                }

                if (action == ID.LogActions.RESUME)
                {
                    foreach (FSAppointmentLog logRow in logList)
                    {
                        FSAppointmentLog fsAppointmentLogRow = new FSAppointmentLog()
                        {
                            ItemType = logRow.ItemType,
                            Status = ID.Status_Log.IN_PROCESS,
                            BAccountID = logRow.BAccountID,
                            DetLineRef = logRow.DetLineRef,
                            DateTimeBegin = LogActionFilter.Current?.LogDateTime
                        };

                        fsAppointmentLogRow = (FSAppointmentLog)LogRecords.Cache.Insert(fsAppointmentLogRow);
                    }
                }

                CompletePauseAction(action, LogActionFilter.Current.LogDateTime, apptDet, logList);

                if (logType == FSLogActionFilter.type.Values.Travel
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
            if (type != FSLogActionFilter.type.Values.NonStock)
            {
                VerifyBeforeAction(action, type);
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

        public virtual void StartTravelAction(IEnumerable<FSAppointmentStaffDistinct> createLogItems = null)
        {
            IEnumerable<FSAppointmentStaffDistinct> createLogItemsLocal = null;
            FSAppointmentLog fsAppointmentLogRow;
            string detLineRef = null;
            DateTime? dateTimeBegin = null;

            if (createLogItems == null)
            {
                detLineRef = LogActionFilter.Current?.DetLineRef;
                dateTimeBegin = LogActionFilter.Current?.LogDateTime;

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
                            ItemType = FSAppointmentLog.itemType.Values.Travel,
                            DetLineRef = detLineRef,
                            DateTimeBegin = dateTimeBegin
                        };

                        LogRecords.Cache.Insert(fsAppointmentLogRow);
                    }
                }
                else
                {
                    createLogItemsLocal = LogActionStaffDistinctRecords.Select().RowCast<FSAppointmentStaffDistinct>()
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
                foreach (FSAppointmentStaffDistinct row in createLogItemsLocal)
                {
                    fsAppointmentLogRow = new FSAppointmentLog()
                    {
                        BAccountID = row.BAccountID,
                        ItemType = FSAppointmentLog.itemType.Values.Travel,
                        DetLineRef = detLineRef,
                        DateTimeBegin = dateTimeBegin
                    };

                    LogRecords.Cache.Insert(fsAppointmentLogRow);
                }
            }
        }

        public virtual void StartServiceAction(IEnumerable<FSAppointmentStaffDistinct> createLogItems = null)
        {
            IEnumerable<FSAppointmentStaffDistinct> createLogItemsLocal = null;
            FSAppointmentLog fsAppointmentLogRow;
            string detLineRef = null;
            DateTime? dateTimeBegin = null;

            if (LogActionFilter.Current?.DetLineRef == null)
                return;

            if (createLogItems == null)
            {
                detLineRef = LogActionFilter.Current?.DetLineRef;
                dateTimeBegin = LogActionFilter.Current?.LogDateTime;

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
                            ItemType = FSAppointmentLog.itemType.Values.Service,
                            BAccountID = employeeByUserID.BAccountID,
                            DetLineRef = detLineRef,
                            DateTimeBegin = dateTimeBegin
                        };

                        fsAppointmentLogRow = (FSAppointmentLog)LogRecords.Cache.Insert(fsAppointmentLogRow);
                    }
                }
                else
                {
                    createLogItemsLocal = LogActionStaffDistinctRecords.Select().RowCast<FSAppointmentStaffDistinct>()
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
                foreach (FSAppointmentStaffDistinct row in createLogItemsLocal)
                {
                    fsAppointmentLogRow = new FSAppointmentLog()
                    {
                        ItemType = FSAppointmentLog.itemType.Values.Service,
                        BAccountID = row.BAccountID,
                        DetLineRef = detLineRef,
                        DateTimeBegin = dateTimeBegin
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
                ItemType = FSAppointmentLog.itemType.Values.NonStock,
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

        public virtual void CompletePauseAction(string logAction, DateTime? dateTimeEnd, FSAppointmentDet apptDet, List<FSAppointmentLog> logList)
        {
            string logStatus = logAction == ID.LogActions.PAUSE ? ID.Status_Log.PAUSED : ID.Status_Log.COMPLETED;
            bool isPauseOrResumeAction = logAction == ID.LogActions.PAUSE || logAction == ID.LogActions.RESUME;
            apptDet = isPauseOrResumeAction ? null : apptDet;

            int rowsAffected = CompletePauseMultipleLogs(dateTimeEnd, ID.Status_AppointmentDet.COMPLETED, logStatus, apptDet != null ? false : true, logList);

            if (apptDet != null)
            {
                rowsAffected += ChangeItemLineStatus(apptDet, ID.Status_AppointmentDet.COMPLETED);
            }

            if (rowsAffected > 0)
            {
                this.Actions.PressSave();
            }
        }

        public virtual int CompletePauseMultipleLogs(DateTime? dateTimeEnd, string newAppDetStatus, string logStatus, bool completeRelatedItemLines, List<FSAppointmentLog> logList)
        {
            if (dateTimeEnd == null)
            {
                dateTimeEnd = PXDBDateAndTimeAttribute.CombineDateTime(PXTimeZoneInfo.Now, PXTimeZoneInfo.Now);
            }

            int rowsAffected = 0;
            List<FSAppointmentDet> apptDetRows = null;

            if (completeRelatedItemLines == true)
            {
                apptDetRows = AppointmentDetails.Select().RowCast<FSAppointmentDet>().ToList();
            }

            if (logList != null && logList.Count > 0)
            {
                IEnumerable<FSAppointmentLog> viewRows = LogRecords.Select().RowCast<FSAppointmentLog>();

                foreach (FSAppointmentLog listRow in logList)
                {
                    FSAppointmentLog viewRow = viewRows.Where(_ => _.LineRef == listRow.LineRef).FirstOrDefault();

                    ChangeLogAndRelatedItemLinesStatus(viewRow, logStatus, dateTimeEnd.Value, newAppDetStatus, apptDetRows);
                    rowsAffected++;
                }
            }

            return rowsAffected;
        }

        public virtual void ResumeMultipleLogs(PXSelectBase<FSAppointmentLog> logSelect, params object[] logSelectArgs)
        {
            LogActionFilter.Current.LogDateTime = PXDBDateAndTimeAttribute.CombineDateTime(Accessinfo.BusinessDate, PXTimeZoneInfo.Now);
            RunLogActionBase(ID.LogActions.RESUME, null, null, logSelect, logSelectArgs);
        }

        public virtual FSAppointmentLog ChangeLogAndRelatedItemLinesStatus(FSAppointmentLog logRow, string newLogStatus, DateTime newDateTimeEnd, string newApptDetStatus, List<FSAppointmentDet> apptDetRows)
        {
            if (logRow == null)
                return null;

            bool keepPausedDateTime = logRow.Status == ID.Status_Log.PAUSED 
                                                && newLogStatus == ID.Status_Log.COMPLETED;

            logRow = PXCache<FSAppointmentLog>.CreateCopy(logRow);
            logRow.Status = newLogStatus;

            if (logRow.KeepDateTimes == false && keepPausedDateTime == false)
            {
                logRow.DateTimeEnd = newDateTimeEnd;
            }

            if (apptDetRows != null && string.IsNullOrWhiteSpace(logRow.DetLineRef) == false)
            {
                FSAppointmentDet apptDet = apptDetRows.Where(r => r.LineRef == logRow.DetLineRef).FirstOrDefault();

                if (apptDet != null)
                {
                    ChangeItemLineStatus(apptDet, newApptDetStatus);
                }
            }

            return (FSAppointmentLog)LogRecords.Cache.Update(logRow);
        }

        public virtual void StartStaffAction(IEnumerable<FSAppointmentStaffExtItemLine> createLogItems = null, DateTime? dateTimeBegin = null)
        {
            IEnumerable<FSAppointmentStaffExtItemLine> createLogItemsLocal = null;

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
                                ItemType = FSAppointmentLog.itemType.Values.Staff,
                                BAccountID = employeeByUserID.BAccountID,
                                DetLineRef = null,
                                DateTimeBegin = dateTimeBegin ?? LogActionFilter.Current.LogDateTime
                            };

                            LogRecords.Cache.Insert(fsAppointmentLogRow);
                        }
                        else
                        {
                            createLogItemsLocal = LogActionStaffRecords.Select().RowCast<FSAppointmentStaffExtItemLine>()
                                                                       .Where(x => x.Selected == true);
                        }
                    }
                }
                else
                {
                    createLogItemsLocal = LogActionStaffRecords.Select().RowCast<FSAppointmentStaffExtItemLine>()
                                                               .Where(x => x.Selected == true);
                }
            }
            else
            {
                createLogItemsLocal = createLogItems;
            }

            if (createLogItemsLocal != null)
            {
                foreach (FSAppointmentStaffExtItemLine row in createLogItemsLocal)
                {
                    int? timeDuration = row != null && row.EstimatedDuration != null ? row.EstimatedDuration : 0;

                    FSAppointmentLog fsAppointmentLogRow = new FSAppointmentLog()
                    {
                        ItemType = FSAppointmentLog.itemType.Values.Staff,
                        BAccountID = row.BAccountID,
                        DetLineRef = row.DetLineRef,
                        DateTimeBegin = dateTimeBegin ?? LogActionFilter.Current.LogDateTime,
                    };

                    LogRecords.Cache.Insert(fsAppointmentLogRow);
                }
            }
        }

        public virtual void StartServiceBasedOnAssignmentAction(IEnumerable<FSDetailFSLogAction> createLogItems = null, DateTime? dateTimeBegin = null)
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
                            fsAppointmentLogRow = new FSAppointmentLog()
                            {
                                ItemType = FSAppointmentLog.itemType.Values.Staff,
                                BAccountID = employeeRow.EmployeeID,
                                DetLineRef = employeeRow.ServiceLineRef,
                                DateTimeBegin = dateTimeBegin ?? LogActionFilter.Current.LogDateTime,
                            };

                            LogRecords.Cache.Insert(fsAppointmentLogRow);
                        }
                    }
                    else
                    {
                        fsAppointmentLogRow = new FSAppointmentLog()
                        {
                            ItemType = FSAppointmentLog.itemType.Values.Service,
                            BAccountID = null,
                            DetLineRef = fsDetailLogActionRow.LineRef,
                            DateTimeBegin = dateTimeBegin ?? LogActionFilter.Current.LogDateTime,
                        };

                        LogRecords.Cache.Insert(fsAppointmentLogRow);
                    }
                }
            }
        }

        public virtual void SetVisibleCompletePauseLogActionGrid(FSLogActionFilter filter)
        {
            LogActionLogRecords.Cache.AllowSelect = filter.Action == ID.LogActions.COMPLETE || filter.Action == ID.LogActions.PAUSE || filter.Action == ID.LogActions.RESUME;
        }

        public virtual void ClearLogActionsViewCaches()
        {
            LogActionLogRecords.Cache.Clear();
            LogActionLogRecords.Cache.ClearQueryCache();

            LogActionStaffRecords.Cache.Clear();
            LogActionStaffRecords.Cache.ClearQueryCache();

            LogActionStaffDistinctRecords.Cache.Clear();
            LogActionStaffDistinctRecords.Cache.ClearQueryCache();
        }

        public virtual void VerifyOnCompleteApptRequireLog(PXCache cache)
        {
            int? servicesWithoutLog = AppointmentDetails.Select().RowCast<FSAppointmentDet>()
                                        .Where(_ => _.LineType == ID.LineType_ALL.SERVICE 
                                                    && _.IsTravelItem == false
                                                    && _.IsCanceledNotPerformed == false
                                                    && _.LogRelatedCount == 0).Count();

            if (servicesWithoutLog > 0)
            {
                throw new PXException(TX.Error.LOG_DATE_TIMES_ARE_REQUIRED_FOR_SERVICES);
            }
        }

        public virtual void VerifyLogActionWithAppointmentStatus(string logActionID, string logActionLabel, string logType, FSAppointment appointment)
        {
            if (appointment.Status == ID.Status_Appointment.AUTOMATIC_SCHEDULED
                || appointment.Status == ID.Status_Appointment.MANUAL_SCHEDULED
                || appointment.Status == ID.Status_Appointment.COMPLETED)
            {
                if (logType != FSLogActionFilter.type.Values.Travel)
                {
                    throw new PXException(PXMessages.LocalizeFormatNoPrefix(TX.Error.LogActionNotAllowedWithCurrentAppointmentStatus,
                                            logActionLabel,
                                            TX.Status_Appointment.AUTOMATIC_SCHEDULED,
                                            TX.Status_Appointment.MANUAL_SCHEDULED,
                                            TX.Status_Appointment.COMPLETED));
                }
            }
        }

        public virtual string GetItemLineStatusFromLog(FSAppointmentDet appointmentDet)
        {
            string itemLineStatus = ID.Status_AppointmentDet.NOT_STARTED;

            if (appointmentDet != null &&
                (appointmentDet.LineType == ID.LineType_ALL.SERVICE ||
                 appointmentDet.LineType == ID.LineType_ALL.NONSTOCKITEM))
            {
                IEnumerable<FSAppointmentLog> itemLogRecords = LogRecords.Select().RowCast<FSAppointmentLog>().Where(_ => _.DetLineRef == appointmentDet.LineRef);
                bool anyInProcessPaused = itemLogRecords.Where(_ => _.Status == ID.Status_Log.IN_PROCESS || _.Status == ID.Status_Log.PAUSED).Any();

                if (anyInProcessPaused == false)
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
                            itemLineStatus = ID.Status_AppointmentDet.COMPLETED;
                        }
                    }
                }
                else
                {
                    itemLineStatus = ID.Status_AppointmentDet.IN_PROCESS;
                }
            }

            if (appointmentDet != null && IsNewItemLineStatusValid(appointmentDet, itemLineStatus) == false)
            {
                itemLineStatus = ID.Status_AppointmentDet.NOT_STARTED;
            }

            return itemLineStatus;
        }
        #endregion
    }
}
