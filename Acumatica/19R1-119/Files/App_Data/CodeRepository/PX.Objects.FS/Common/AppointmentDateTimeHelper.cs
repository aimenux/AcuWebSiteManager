using PX.Common;
using PX.Data;
using System;
using System.Linq;

namespace PX.Objects.FS
{
    public class AppointmentDateTimeHelper
    {
        public const int DEFAULT_DURATION_MINUTES = 1;

        public enum Action
        {
            OnStartAppointment,
            OnCompleteAppointment,
            Always
        }

        public enum Level
        {
            Header,
            ServiceLine,
            Both
        }

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

        #region Start Appointment actions

        /// <summary>
        /// Verifies if the ActualDateTimeBegin values in the specified level and action can be updated.
        /// </summary>
        /// <param name="fsSrvOrdTypeRow">Service Order type instance.</param>
        /// <param name="onAction">Run on this Action.</param>
        /// <param name="onLevel">Run on this Level.</param>
        /// <returns>True if values can be updated, false, otherwise.</returns>
        public static bool CanUpdateActualDateTimeBegin(FSSrvOrdType fsSrvOrdTypeRow, Action onAction, Level onLevel)
        {
            if (fsSrvOrdTypeRow == null)
            {
                return false;
            }

            switch (onAction)
            {
                case Action.OnStartAppointment:

                    if (fsSrvOrdTypeRow.StartAppointmentActionBehavior == ID.SrvOrdType_StartAppointmentActionBehavior.NOTHING)
                    {
                        return false;
                    }
                    else
                    {
                        if (onLevel == Level.Header)
                        {
                            return fsSrvOrdTypeRow.StartAppointmentActionBehavior == ID.SrvOrdType_StartAppointmentActionBehavior.HEADER_ONLY
                                        || fsSrvOrdTypeRow.StartAppointmentActionBehavior == ID.SrvOrdType_StartAppointmentActionBehavior.HEADER_SERVICE_LINES;
                        }

                        if (onLevel == Level.ServiceLine)
                        {
                            return fsSrvOrdTypeRow.UpdateServiceActualDateTimeBegin == true
                                        || fsSrvOrdTypeRow.StartAppointmentActionBehavior == ID.SrvOrdType_StartAppointmentActionBehavior.HEADER_SERVICE_LINES;
                        }
                    }

                    break;
                case Action.Always:
                    return fsSrvOrdTypeRow.UpdateServiceActualDateTimeBegin == true;
                default:
                    break;
            }

            return false;
        }

        /// <summary>
        /// Updates the ActualDateTimeBegin values in the specified level and action.
        /// </summary>
        /// <param name="fsSrvOrdTypeRow">Service Order Type instance.</param>
        /// <param name="fsAppointmentRow">Appointment instance.</param>
        /// <param name="appointmentDetServicesView">Appointment details view.</param>
        /// <param name="appointmentEmployeeView">Appointment employees view.</param>
        /// <param name="onAction">Run on this Action.</param>
        /// <param name="onLevel">Run on this level.</param>
        public static void UpdateActualDateTimeBegin(
                                                     FSSrvOrdType fsSrvOrdTypeRow,
                                                     FSAppointment fsAppointmentRow,
                                                     AppointmentCore.AppointmentDetServices_View appointmentDetServicesView,
                                                     AppointmentCore.AppointmentEmployees_View appointmentEmployeeView,
                                                     AppointmentDateTimeHelper.Action onAction,
                                                     AppointmentDateTimeHelper.Level onLevel,
                                                     PXCache cacheAppointmentHeader,
                                                     PXCache cacheAppointmentService)
        {
            Level startLevel = Level.Header;
            Level endLevel = Level.ServiceLine;

            if (onLevel != Level.Both)
            {
                startLevel = endLevel = onLevel;
            }

            for (Level currLevel = startLevel; currLevel <= endLevel; currLevel++)
            {
                if (CanUpdateActualDateTimeBegin(fsSrvOrdTypeRow, onAction, currLevel))
                {
                    switch (currLevel)
                    {
                        case Level.Header:
                            SetNowInHeaderActualDateTimeBegin(cacheAppointmentHeader, fsAppointmentRow);
                            break;
                        case Level.ServiceLine:
                            UpdateServicesActualDateTimeBegin(fsSrvOrdTypeRow, fsAppointmentRow, appointmentDetServicesView, appointmentEmployeeView, onAction);
                            break;
                        default:
                            break;
                    }
                }
            }
        }

        public static void SetNowInHeaderActualDateTimeBegin(PXCache cache, FSAppointment fsAppointmentRow)
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
                                                SharedFunctions.GetTimeWithSpecificDate(actualTime, actualDate, true));
            }
        }

        /// <summary>
        /// Updates All services' ActualDateTimeBegin with the ones specified in the header.
        /// </summary>
        /// <param name="fsSrvOrdTypeRow">Service Order Type instance.</param>
        /// <param name="fsAppointmentRow">Appointment instance.</param>
        /// <param name="appointmentDetServicesView">Appointment Details view.</param>
        /// <param name="appointmentEmployeeView">Appointment Employees view.</param>
        /// <param name="onAction">Run on this Action.</param>
        public static void UpdateServicesActualDateTimeBegin(
                                                             FSSrvOrdType fsSrvOrdTypeRow,
                                                             FSAppointment fsAppointmentRow,
                                                             AppointmentCore.AppointmentDetServices_View appointmentDetServicesView,
                                                             AppointmentCore.AppointmentEmployees_View appointmentEmployeeView,
                                                             Action onAction)
        {
            if (fsAppointmentRow.ActualDateTimeBegin.HasValue)
            {
                DateTime? actualDateTimeBegin = fsAppointmentRow.ActualDateTimeBegin.Value.AddSeconds(-fsAppointmentRow.ActualDateTimeBegin.Value.Second);

                foreach (FSAppointmentDetService fsAppointmentDetServiceRow in appointmentDetServicesView.Select())
                {
                    if (fsAppointmentDetServiceRow.LineType != ID.LineType_All.SERVICE)
                    {
                        continue;
                    }

                    if (fsAppointmentDetServiceRow.StaffRelated == true)
                    {
                        foreach (FSAppointmentEmployee fsAppointmentEmployeeRow in appointmentEmployeeView.Select().Where(y => ((FSAppointmentEmployee)y).ServiceLineRef == fsAppointmentDetServiceRow.LineRef))
                        {
                            if (KeepStaffActualDateTimes(fsSrvOrdTypeRow, fsAppointmentEmployeeRow))
                            {
                                continue;
                            }

                            fsAppointmentEmployeeRow.ActualDateTimeBegin = actualDateTimeBegin;
                            UpdateLineActualDateTimeEnd(onAction, fsAppointmentDetServiceRow, fsAppointmentEmployeeRow);
                            appointmentEmployeeView.Update(fsAppointmentEmployeeRow);

                            UpdateServiceActualTimeBeginFromStaff(appointmentEmployeeView.Cache, appointmentDetServicesView, fsAppointmentEmployeeRow, fsAppointmentDetServiceRow);
                            UpdateServiceActualTimeEndFromStaff(appointmentEmployeeView.Cache, appointmentDetServicesView, fsAppointmentEmployeeRow, fsAppointmentDetServiceRow);
                        }
                    }
                    else
                    {
                        if (KeepServiceActualDateTimes(fsSrvOrdTypeRow, fsAppointmentDetServiceRow))
                        {
                            continue;
                        }

                        fsAppointmentDetServiceRow.ActualDateTimeBegin = actualDateTimeBegin;
                        UpdateLineActualDateTimeEnd(onAction, fsAppointmentDetServiceRow);
                        appointmentDetServicesView.Update(fsAppointmentDetServiceRow);
                    }

                    if (onAction == Action.OnStartAppointment)
                    {
                        AppointmentDateTimeHelper.CalculateAppointmentDetServiceActualDuration(appointmentDetServicesView.Cache, fsAppointmentDetServiceRow);
                    }
                }

                //Staff members linked to a service were already updated.
                foreach(FSAppointmentEmployee fsAppointmentEmployeeRow in appointmentEmployeeView.Select().Where(y => ((FSAppointmentEmployee)y).ServiceLineRef == null))
                {
                    if (KeepStaffActualDateTimes(fsSrvOrdTypeRow, fsAppointmentEmployeeRow))
                    {
                        continue;
                    }

                    fsAppointmentEmployeeRow.ActualDateTimeBegin = actualDateTimeBegin;
                    UpdateStaffActualDateTimeEndFromActualDuration(fsAppointmentEmployeeRow);
                    appointmentEmployeeView.Update(fsAppointmentEmployeeRow);
                }
            }
        }

        public static void UpdateStaffActualDuration(PXGraph graph, AppointmentCore.AppointmentEmployees_View appointmentEmployeeView)
        {
            foreach (FSAppointmentEmployee fsAppointmentEmployeeRow in appointmentEmployeeView.Select().Where(y => ((FSAppointmentEmployee)y).ServiceLineRef != null))
            {
                FSAppointmentDetService fsAppointmentDetRow = PXSelectJoin<FSAppointmentDetService,
                                                           InnerJoin<FSSODet,
                                                               On<FSSODet.sODetID, Equal<FSAppointmentDetService.sODetID>>>,
                                                           Where<
                                                               FSSODet.lineRef, Equal<Required<FSSODet.lineRef>>,
                                                               And<FSSODet.sOID, Equal<Current<FSAppointment.sOID>>>>>
                                                           .Select(graph, fsAppointmentEmployeeRow.ServiceLineRef);

                fsAppointmentEmployeeRow.ActualDuration = fsAppointmentDetRow?.EstimatedDuration ?? 0;

                appointmentEmployeeView.Update(fsAppointmentEmployeeRow);
            }
        }

        /// <summary>
        /// Updates the ActualDateTimeEnd of the current line: whether if it is a Service or a Staff.
        /// </summary>
        /// <param name="onAction">Run on this Action.</param>
        /// <param name="fsAppointmentDetServiceRow">Current <c>AppointmentDetService</c> instance.</param>
        /// <param name="fsAppointmentEmployeeRow">Current AppointmentEmployee instance.</param>
        public static void UpdateLineActualDateTimeEnd(Action onAction, FSAppointmentDetService fsAppointmentDetServiceRow, FSAppointmentEmployee fsAppointmentEmployeeRow = null)
        {
            switch (onAction)
            {
                case Action.OnStartAppointment:
                    if (fsAppointmentEmployeeRow == null)
                    {
                        UpdateServiceActualDateTimeEndFromEstimatedDuration(fsAppointmentDetServiceRow);
                    }
                    else
                    {
                        UpdateStaffActualDateTimeEndFromEstimatedDuration(fsAppointmentDetServiceRow, fsAppointmentEmployeeRow);
                    }

                    break;
                case Action.Always:
                    if (fsAppointmentEmployeeRow == null)
                    {
                        if (fsAppointmentDetServiceRow.ActualDuration != 0)
                        {
                            UpdateServiceActualDateTimeEndFromActualDuration(fsAppointmentDetServiceRow);
                        }
                        else
                        {
                            UpdateServiceActualDateTimeEndFromEstimatedDuration(fsAppointmentDetServiceRow);
                        }
                    }
                    else
                    {
                        if (fsAppointmentEmployeeRow.ActualDuration != 0)
                        {
                            UpdateStaffActualDateTimeEndFromActualDuration(fsAppointmentEmployeeRow);
                        }
                        else
                        {
                            UpdateStaffActualDateTimeEndFromEstimatedDuration(fsAppointmentDetServiceRow, fsAppointmentEmployeeRow);
                        }
                    }

                    break;
                default:
                    break;
            }
        }

        public static void UpdateStaffActualDateTimeOnCompleteApp(AppointmentCore.AppointmentEmployees_View AppointmentEmployees, FSAppointment fsAppointmentRow)
        {
            foreach (FSAppointmentEmployee fsAppointmentEmployeeRow in AppointmentEmployees.Select())
            {
                if (fsAppointmentEmployeeRow.ActualDateTimeBegin.HasValue == false
                            && fsAppointmentRow.ActualDateTimeBegin.HasValue == true)
                {
                    fsAppointmentEmployeeRow.ActualDateTimeBegin = fsAppointmentRow.ActualDateTimeBegin;
                    DateTime? dateTimeEnd = fsAppointmentEmployeeRow.ActualDuration == 0 ? fsAppointmentRow.ActualDateTimeEnd :
                                                                                           fsAppointmentRow.ActualDateTimeBegin.Value.AddMinutes((int)fsAppointmentEmployeeRow.ActualDuration);

                    fsAppointmentEmployeeRow.ActualDateTimeEnd = dateTimeEnd;
                    AppointmentEmployees.Cache.Update(fsAppointmentEmployeeRow);
                }
            }
        }

        #endregion

        #region Complete Appointment actions

        /// <summary>
        /// Verifies if the ActualDateTimeEnd values in the specified level and action can be updated.
        /// </summary>
        /// <param name="fsSrvOrdTypeRow">Service Order Type instance.</param>
        /// <param name="onAction">Run on this Action.</param>
        /// <param name="onLevel">Run on this level.</param>
        /// <returns>True if the values can be updated, False, otherwise.</returns>
        public static bool CanUpdateActualDateTimeEnd(FSSrvOrdType fsSrvOrdTypeRow, Action onAction, Level onLevel)
        {
            if (fsSrvOrdTypeRow == null)
            {
                return false;
            }

            switch (onAction)
            {
                case Action.OnCompleteAppointment:
                    if (fsSrvOrdTypeRow.CompleteAppointmentActionBehavior == ID.SrvOrdType_CompleteAppointmentActionBehavior.NOTHING)
                    {
                        return false;
                    }
                    else
                    {
                        if (onLevel == Level.Header)
                        {
                            return fsSrvOrdTypeRow.CompleteAppointmentActionBehavior == ID.SrvOrdType_CompleteAppointmentActionBehavior.HEADER_ONLY
                                        || fsSrvOrdTypeRow.CompleteAppointmentActionBehavior == ID.SrvOrdType_CompleteAppointmentActionBehavior.HEADER_SERVICE_LINES;
                        }

                        if (onLevel == Level.ServiceLine)
                        {
                            return fsSrvOrdTypeRow.UpdateServiceActualDateTimeEnd == true
                                        || fsSrvOrdTypeRow.CompleteAppointmentActionBehavior == ID.SrvOrdType_CompleteAppointmentActionBehavior.HEADER_SERVICE_LINES;
                        }
                    }

                    break;
                case Action.Always:
                    return fsSrvOrdTypeRow.UpdateServiceActualDateTimeEnd == true;
                default:
                    break;
            }

            return false;
        }

        /// <summary>
        /// Updates the ActualDateTimeEnd values in the specified level and action.
        /// </summary>
        /// <param name="fsSrvOrdTypeRow">Service Order Type instance.</param>
        /// <param name="fsAppointmentRow">Appointment instance.</param>
        /// <param name="appointmentDetServicesView">Appointment Details view.</param>
        /// <param name="appointmentEmployeeView">Appointment Employees view.</param>
        /// <param name="onAction">Run on this Action.</param>
        /// <param name="onLevel">Run on this level.</param>
        public static void UpdateActualDateTimeEnd(
                                                   FSSrvOrdType fsSrvOrdTypeRow,
                                                   FSAppointment fsAppointmentRow,
                                                   AppointmentCore.AppointmentDetServices_View appointmentDetServicesView,
                                                   AppointmentCore.AppointmentEmployees_View appointmentEmployeeView,
                                                   Action onAction,
                                                   Level onLevel)
        {
            Level startLevel = Level.Header;
            Level endLevel = Level.ServiceLine;

            if (onLevel != Level.Both)
            {
                startLevel = endLevel = onLevel;
            }

            for (Level currLevel = startLevel; currLevel <= endLevel; currLevel++)
            {
                if (currLevel == Level.ServiceLine
                        && onAction == Action.OnCompleteAppointment)
                {
                    ValidateActualDateTimeEnd(fsSrvOrdTypeRow, fsAppointmentRow, appointmentDetServicesView, appointmentEmployeeView);
                }

                if (CanUpdateActualDateTimeEnd(fsSrvOrdTypeRow, onAction, currLevel))
                {
                    switch (currLevel)
                    {
                        case Level.Header:
                            UpdateHeaderActualDateTimeEnd(fsAppointmentRow);
                            break;
                        case Level.ServiceLine:
                            UpdateServicesActualDateTimeEnd(fsSrvOrdTypeRow, fsAppointmentRow, appointmentDetServicesView, appointmentEmployeeView);
                            break;
                        default:
                            break;
                    }
                }
            }
        }

        public static void UpdateHeaderActualDateTimeEnd(FSAppointment fsAppointmentRow)
        {
            if (fsAppointmentRow.ActualDateTimeBegin == null
                    || fsAppointmentRow.HandleManuallyActualTime == true)
            {
                return;
            }

            DateTime? newEndDate = SharedFunctions.GetTimeWithSpecificDate(PXTimeZoneInfo.Now, fsAppointmentRow.ActualDateTimeBegin, true);

            DateTime? actualDateTimeBegin = SharedFunctions.GetTimeWithSpecificDate(fsAppointmentRow.ActualDateTimeBegin, fsAppointmentRow.ActualDateTimeBegin, true);

            if (DateTime.Compare(actualDateTimeBegin.Value, newEndDate.Value) == 0)
            {
                newEndDate = newEndDate.Value.AddMinutes(AppointmentDateTimeHelper.DEFAULT_DURATION_MINUTES);
            }

            if (fsAppointmentRow.ActualDateTimeEnd != newEndDate)
            {
            fsAppointmentRow.ActualDateTimeEnd = newEndDate;
                fsAppointmentRow.HandleManuallyActualTime = true;
            }
        }

        /// <summary>
        /// Updates All services' ActualDateTimeEnd values with the ones specified in the header.
        /// </summary>
        /// <param name="fsSrvOrdTypeRow">Service Order Type instance.</param>
        /// <param name="fsAppointmentRow">Appointment instance.</param>
        /// <param name="appointmentDetServicesView">Appointment Details view.</param>
        /// <param name="appointmentEmployeeView">Appointment Employees view.</param>
        public static void UpdateServicesActualDateTimeEnd(
                                                           FSSrvOrdType fsSrvOrdTypeRow,
                                                           FSAppointment fsAppointmentRow,
                                                           AppointmentCore.AppointmentDetServices_View appointmentDetServicesView,
                                                           AppointmentCore.AppointmentEmployees_View appointmentEmployeeView)
        {
            if (fsAppointmentRow.ActualDateTimeEnd.HasValue)
            {
                DateTime? actualDateTimeEnd = fsAppointmentRow.ActualDateTimeEnd.Value.AddSeconds(-fsAppointmentRow.ActualDateTimeEnd.Value.Second);

                foreach (FSAppointmentDetService fsAppointmentDetServiceRow in appointmentDetServicesView.Select())
                {
                    if (fsAppointmentDetServiceRow.LineType != ID.LineType_All.SERVICE)
                    {
                        continue;
                    }

                    if (KeepServiceActualDateTimes(fsSrvOrdTypeRow, fsAppointmentDetServiceRow))
                    {
                        continue;
                    }

                    fsAppointmentDetServiceRow.ActualDateTimeEnd = actualDateTimeEnd;
                    appointmentDetServicesView.Update(fsAppointmentDetServiceRow);
                }

                foreach (FSAppointmentEmployee fsAppointmentEmployeeRow in appointmentEmployeeView.Select())
                {
                    if (KeepStaffActualDateTimes(fsSrvOrdTypeRow, fsAppointmentEmployeeRow))
                    {
                        continue;
                    }

                    fsAppointmentEmployeeRow.ActualDateTimeEnd = actualDateTimeEnd;
                    appointmentEmployeeView.Update(fsAppointmentEmployeeRow);
                }
            }
        }

        /// <summary>
        /// Checks that the services' ActualDateTimeBegin values are less or equal than the header's ActualDateTimeEnd.
        /// </summary>
        /// <param name="fsAppointmentRow">Appointment instance.</param>
        /// <param name="appointmentDetServicesView">Appointment Details view.</param>
        /// <param name="appointmentEmployeeView">Appointment Employees view.</param>
        /// <param name="errorMessage">Error message.</param>
        /// <returns>True if there are no errors, False, otherwise.</returns>
        public static bool CheckServiceActualDateTimeBegin(
                                                           FSAppointment fsAppointmentRow,
                                                           AppointmentCore.AppointmentDetServices_View appointmentDetServicesView,
                                                           AppointmentCore.AppointmentEmployees_View appointmentEmployeeView,
                                                           ref string errorMessage)
        {
            errorMessage = string.Empty;

            if (fsAppointmentRow.ActualDateTimeEnd.HasValue)
            {
                DateTime headerEndDate = fsAppointmentRow.ActualDateTimeEnd.Value;

                foreach (FSAppointmentDetService fsAppointmentDetServiceRow in appointmentDetServicesView.Select())
                {
                    if (fsAppointmentDetServiceRow.LineType != ID.LineType_All.SERVICE)
                    {
                        continue;
                    }

                    if (fsAppointmentDetServiceRow.StaffRelated == true)
                    {
                        foreach (FSAppointmentEmployee fsAppointmentEmployeeRow in appointmentEmployeeView.Select().Where(y => ((FSAppointmentEmployee)y).ServiceLineRef == fsAppointmentDetServiceRow.LineRef))
                        {
                            if (fsAppointmentEmployeeRow.ActualDateTimeBegin.HasValue)
                            {
                                DateTime staffEndDate = fsAppointmentEmployeeRow.ActualDateTimeBegin.Value;

                                if (headerEndDate < staffEndDate)
                                {
                                    errorMessage = PXMessages.LocalizeFormatNoPrefix(TX.Error.START_TIME_GREATER_HEADER_ACTUAL_END_TIME, TX.WildCards.STAFF);
                                    appointmentEmployeeView.Cache.RaiseExceptionHandling<FSAppointmentEmployee.actualDateTimeBegin>(
                                                                                                      fsAppointmentEmployeeRow,
                                                                                                      null,
                                                                                                      new PXSetPropertyException(errorMessage, PXErrorLevel.Error));
                                }
                            }
                        }
                    }
                    else
                    {
                        if (fsAppointmentDetServiceRow.ActualDateTimeBegin.HasValue)
                        {
                            DateTime serviceEndDate = fsAppointmentDetServiceRow.ActualDateTimeBegin.Value;

                            if (headerEndDate < serviceEndDate)
                            {
                                errorMessage = PXMessages.LocalizeFormatNoPrefix(TX.Error.START_TIME_GREATER_HEADER_ACTUAL_END_TIME, TX.WildCards.SERVICE);
                                appointmentDetServicesView.Cache.RaiseExceptionHandling<FSAppointmentDetService.actualDateTimeBegin>(
                                                                                                  fsAppointmentDetServiceRow,
                                                                                                  null,
                                                                                                  new PXSetPropertyException(errorMessage, PXErrorLevel.Error));
                            }
                        }
                    }
                }
            }

            return string.IsNullOrEmpty(errorMessage) == true;
        }

        /// <summary>
        /// Validates the ActualDateTimeEnd belonging to the Service/Staff is less or equal than the header's when completing the appointment. 
        /// </summary>
        /// <param name="fsSrvOrdTypeRow">Service Order type instance.</param>
        /// <param name="fsAppointmentRow">Appointment instance.</param>
        /// <param name="appointmentDetServicesView">Appointment Details view.</param>
        /// <param name="appointmentEmployeeView">Appointment Employees view.</param>
        public static void ValidateActualDateTimeEnd(
                                                     FSSrvOrdType fsSrvOrdTypeRow,                                                                 
                                                     FSAppointment fsAppointmentRow,
                                                     AppointmentCore.AppointmentDetServices_View appointmentDetServicesView,
                                                     AppointmentCore.AppointmentEmployees_View appointmentEmployeeView)
        {
            if (fsAppointmentRow.ActualDateTimeEnd.HasValue)
            {
                TimeSpan headerEndTime = fsAppointmentRow.ActualDateTimeEnd.Value.TimeOfDay;

                foreach (FSAppointmentDetService fsAppointmentDetServiceRow in appointmentDetServicesView.Select())
                {
                    if (fsAppointmentDetServiceRow.LineType != ID.LineType_All.SERVICE)
                    {
                        continue;
                    }

                    if (fsAppointmentDetServiceRow.StaffRelated == true)
                    {
                        foreach (FSAppointmentEmployee fsAppointmentEmployeeRow in appointmentEmployeeView.Select().Where(y => ((FSAppointmentEmployee)y).ServiceLineRef == fsAppointmentDetServiceRow.LineRef))
                        {
                            if (KeepStaffActualDateTimes(fsSrvOrdTypeRow, fsAppointmentEmployeeRow))
                            {
                                continue;
                            }

                            DateTime? staffDateTimeEnd = GetValidActualDateTimeEnd(
                                                                                   fsAppointmentEmployeeRow.ActualDateTimeBegin,
                                                                                   fsAppointmentEmployeeRow.ActualDateTimeEnd,
                                                                                   fsAppointmentRow.ActualDateTimeEnd);
                            if (staffDateTimeEnd != null)
                            {
                                fsAppointmentEmployeeRow.ActualDateTimeEnd = staffDateTimeEnd;
                                appointmentEmployeeView.Update(fsAppointmentEmployeeRow);
                            }
                        }
                    }
                    else
                    {
                        if (KeepServiceActualDateTimes(fsSrvOrdTypeRow, fsAppointmentDetServiceRow))
                        {
                            continue;
                        }

                        DateTime? serviceDateTimeEnd = GetValidActualDateTimeEnd(
                                                                                 fsAppointmentDetServiceRow.ActualDateTimeBegin, 
                                                                                 fsAppointmentDetServiceRow.ActualDateTimeEnd, 
                                                                                 fsAppointmentRow.ActualDateTimeEnd);
                        if (serviceDateTimeEnd != null)
                        {
                            fsAppointmentDetServiceRow.ActualDateTimeEnd = serviceDateTimeEnd;
                            appointmentDetServicesView.Update(fsAppointmentDetServiceRow);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Gets the valid ActualDateTimeEnd of the line depending on the header's value. 
        /// It will truncate those lines that exceeds header's value.
        /// </summary>
        /// <param name="lineDateTimeBegin">Current line DateTimeBegin.</param>
        /// <param name="lineDateTimeEnd">Current line DateTimeEnd.</param>
        /// <param name="headerDateTimeEnd">Header DateTimeEnd.</param>
        /// <returns>Null if the current line does not exceed the header's value, otherwise, it returns the value the line should have.</returns>
        public static DateTime? GetValidActualDateTimeEnd(DateTime? lineDateTimeBegin, DateTime? lineDateTimeEnd, DateTime? headerDateTimeEnd)
        {
            DateTime? validActualDateTimeEnd = null;

            if (lineDateTimeBegin.HasValue
                    && lineDateTimeEnd.HasValue)
            {
                TimeSpan lineBeginTime = lineDateTimeBegin.Value.TimeOfDay;
                TimeSpan lineEndTime = lineDateTimeEnd.Value.TimeOfDay;
                TimeSpan headerEndTime = headerDateTimeEnd.Value.TimeOfDay;

                if (lineBeginTime <= headerEndTime
                            && lineEndTime > headerEndTime)
                {
                    validActualDateTimeEnd = SharedFunctions.GetCustomDateTime(lineDateTimeEnd, headerDateTimeEnd);
                }
            }

            return validActualDateTimeEnd;
        }

        #endregion

        #region CheckBoxes actions

        #region Start Time of Header will update Start Time of Service lines.

        /// <summary>
        /// Updates the current service's ActualDateTimeBegin.
        /// </summary>
        /// <param name="fsSrvOrdTypeRow">Service Order Type instance.</param>
        /// <param name="fsAppointmentRow">Appointment instance.</param>
        /// <param name="appointmentDetServicesView">Appointment Details view.</param>
        public static void UpdateServiceActualDateTimeBegin(
                                                            FSSrvOrdType fsSrvOrdTypeRow,
                                                            FSAppointment fsAppointmentRow,
                                                            AppointmentCore.AppointmentDetServices_View appointmentDetServicesView)
        {
            DateTime? serviceDateTimeBegin = null;
            FSAppointmentDet fsAppointmentDetRow = appointmentDetServicesView.Current;

            if (KeepServiceActualDateTimes(fsSrvOrdTypeRow, fsAppointmentDetRow))
            {
                return;
            }

            if (CanUpdateActualDateTimeBegin(fsSrvOrdTypeRow, Action.Always, Level.ServiceLine))
            {
                if (fsAppointmentRow.ActualDateTimeBegin.HasValue)
                {
                    serviceDateTimeBegin = fsAppointmentRow.ActualDateTimeBegin;
                }
            }

            if (serviceDateTimeBegin.HasValue)
            {
                appointmentDetServicesView.SetValueExt<FSAppointmentDetService.actualDateTimeBegin>((FSAppointmentDetService)fsAppointmentDetRow, serviceDateTimeBegin);
            }
        }

        /// <summary>
        /// Updates the current staff's ActualDateTimeBegin.
        /// </summary>
        /// <param name="fsSrvOrdTypeRow">Service Order Type instance.</param>
        /// <param name="fsAppointmentRow">Appointment instance.</param>
        /// <param name="appointmentDetServicesView">Appointment Details view.</param>
        /// <param name="appointmentEmployeeView">Appointment Employees view.</param>
        public static void UpdateStaffActualDateTimeBegin(
                                                          FSSrvOrdType fsSrvOrdTypeRow,
                                                          FSAppointment fsAppointmentRow,
                                                          AppointmentCore.AppointmentDetServices_View appointmentDetServicesView,
                                                          AppointmentCore.AppointmentEmployees_View appointmentEmployeeView)
        {
            DateTime? staffDateTimeBegin = null;
            FSAppointmentEmployee fsAppointmentEmployeeRow = appointmentEmployeeView.Current;

            if (KeepStaffActualDateTimes(fsSrvOrdTypeRow, fsAppointmentEmployeeRow))
            {
                return;
            }

            if (CanUpdateActualDateTimeBegin(fsSrvOrdTypeRow, Action.Always, Level.ServiceLine))
            {
                if (fsAppointmentRow.ActualDateTimeBegin.HasValue)
                {
                    staffDateTimeBegin = fsAppointmentRow.ActualDateTimeBegin;
                }
            }

            if (staffDateTimeBegin.HasValue)
            {
                appointmentEmployeeView.SetValueExt<FSAppointmentEmployee.actualDateTimeBegin>(fsAppointmentEmployeeRow, staffDateTimeBegin);

                FSAppointmentDetService fsAppointmentDetServiceRow = GetServiceFromLinkedEmployee(
                                                                                                  fsAppointmentEmployeeRow.ServiceLineRef,
                                                                                                  appointmentDetServicesView);

                DateTime? staffDateTimeEnd = GetStaffActualDateTimeEndFromEstimatedDuration(fsAppointmentDetServiceRow, fsAppointmentEmployeeRow);
                appointmentEmployeeView.SetValueExt<FSAppointmentEmployee.actualDateTimeEnd>(fsAppointmentEmployeeRow, staffDateTimeEnd);
            }
        }

        /// <summary>
        /// Gets an <c>FSAppointmentDetService</c> instance of the service for the <c>sODetID</c> provided.
        /// </summary>
        /// <param name="sODetID">SO Detail ID.</param>
        /// <param name="appointmentDetServicesView">Appointment Details view.</param>
        /// <returns>Requested <c>AppointmentDetService</c> Instance.</returns>
        public static FSAppointmentDetService GetServiceFromLinkedEmployee(string lineRef, AppointmentCore.AppointmentDetServices_View appointmentDetServicesView)
        {
            if (string.IsNullOrEmpty(lineRef))
            {
                return null;
            }

            //Getting linked service.
            PXResult<FSAppointmentDetService> fsAppointmentDetServiceRow_tmp = appointmentDetServicesView.Select()
                                                                                                         .Where(y => ((FSAppointmentDetService)y).LineRef == lineRef)
                                                                                                         .First();

            return (FSAppointmentDetService)fsAppointmentDetServiceRow_tmp;
        }
        #endregion

        #region End Time of Header will update End Time of Service lines.

        /// <summary>
        /// Updates the current service's ActualDateTimeEnd.
        /// </summary>
        /// <param name="fsSrvOrdTypeRow">Service Order Type instance.</param>
        /// <param name="fsAppointmentRow">Appointment instance.</param>
        /// <param name="appointmentDetServicesView">Appointment details view.</param>
        public static void UpdateServiceActualDateTimeEnd(
                                                          FSSrvOrdType fsSrvOrdTypeRow,
                                                          FSAppointment fsAppointmentRow,
                                                          AppointmentCore.AppointmentDetServices_View appointmentDetServicesView)
        {
            DateTime? serviceDateTimeEnd = null;
            FSAppointmentDet fsAppointmentDetRow = appointmentDetServicesView.Current;

            if (KeepServiceActualDateTimes(fsSrvOrdTypeRow, fsAppointmentDetRow))
            {
                return;
            }

            if (CanUpdateActualDateTimeEnd(fsSrvOrdTypeRow, Action.Always, Level.ServiceLine))
            {
                if (fsAppointmentRow.ActualDateTimeEnd.HasValue)
                {
                    serviceDateTimeEnd = fsAppointmentRow.ActualDateTimeEnd;
                }
            }

            if (serviceDateTimeEnd.HasValue
                    && fsAppointmentDetRow.ActualDateTimeBegin.HasValue)
            {
                appointmentDetServicesView.SetValueExt<FSAppointmentDetService.actualDateTimeEnd>((FSAppointmentDetService)fsAppointmentDetRow, serviceDateTimeEnd);
            }
        }

        /// <summary>
        /// Updates the current staff's ActualDateTimeEnd.
        /// </summary>
        /// <param name="fsSrvOrdTypeRow">Service Order Type instance.</param>
        /// <param name="fsAppointmentRow">Appointment instance.</param>
        /// <param name="appointmentEmployeeView">Appointment Employees view.</param>
        [Obsolete("Remove this method in 2019R2")]
        public static void UpdateStaffActualDateTimeEnd(
                                                        FSSrvOrdType fsSrvOrdTypeRow,
                                                        FSAppointment fsAppointmentRow,
                                                        AppointmentCore.AppointmentEmployees_View appointmentEmployeeView)
        {
            DateTime? staffDateTimeEnd = null;
            FSAppointmentEmployee fsAppointmentEmployeeRow = appointmentEmployeeView.Current;

            if (KeepStaffActualDateTimes(fsSrvOrdTypeRow, fsAppointmentEmployeeRow))
            {
                return;
            }

            if (CanUpdateActualDateTimeEnd(fsSrvOrdTypeRow, Action.Always, Level.ServiceLine))
            {
                if (fsAppointmentRow.ActualDateTimeEnd.HasValue)
                {
                    staffDateTimeEnd = fsAppointmentRow.ActualDateTimeEnd;
                }
            }

            if (staffDateTimeEnd.HasValue
                    && fsAppointmentEmployeeRow.ActualDateTimeBegin.HasValue)
            {
                appointmentEmployeeView.SetValueExt<FSAppointmentEmployee.actualDateTimeEnd>(fsAppointmentEmployeeRow, staffDateTimeEnd);
            }
        }

        #endregion

        #region Require Actual Start/End Times of Service lines to Complete Appointment

        public static bool AreServiceActualDateTimesRequired(FSSrvOrdType fsSrvOrdTypeRow)
        {
            return fsSrvOrdTypeRow != null
                        && fsSrvOrdTypeRow.RequireServiceActualDateTimes == true;
        }

        /// <summary>
        /// Checks if the services' ActualDateTime values are provided when the 'Require Service Actual Date Times' 
        /// in the Service Order type is selected when trying to complete the appointment.
        /// </summary>
        /// <param name="fsSrvOrdTypeRow">Service Order Type instance.</param>
        /// <param name="appointmentDetServicesView">Appointment Details view.</param>
        /// <param name="appointmentEmployeeView">Appointment Employees view.</param>
        /// <param name="errorMessage">Error message.</param>
        /// <returns>True if there are no errors, False, otherwise.</returns>
        public static bool CheckServiceActualDateTimes(
                                                        FSSrvOrdType fsSrvOrdTypeRow,
                                                        AppointmentCore.AppointmentDetServices_View appointmentDetServicesView,
                                                        AppointmentCore.AppointmentEmployees_View appointmentEmployeeView,
                                                        ref string errorMessage)
        {
            errorMessage = string.Empty;

            if (AreServiceActualDateTimesRequired(fsSrvOrdTypeRow))
            {
                foreach (FSAppointmentDet fsAppointmentDetRow in appointmentDetServicesView.Select())
                {
                    if (fsAppointmentDetRow.LineType != ID.LineType_All.SERVICE)
                    {
                        continue;
                    }

                    if (fsAppointmentDetRow.StaffRelated == true)
                    {
                        string lineRef = fsAppointmentDetRow.LineRef;

                        foreach (FSAppointmentEmployee fsAppointmentEmployeeRow in appointmentEmployeeView.Select()
                                                                                                          .Where(y => ((FSAppointmentEmployee)y).ServiceLineRef == lineRef))
                        {
                            if (fsAppointmentEmployeeRow.ActualDateTimeBegin.HasValue == false
                                    || fsAppointmentEmployeeRow.ActualDateTimeEnd.HasValue == false)
                            {
                                errorMessage = PXMessages.LocalizeFormatNoPrefix(TX.Error.ACTUAL_DATE_TIMES_ARE_REQUIRED, TX.WildCards.STAFF);
                                appointmentEmployeeView.Cache.RaiseExceptionHandling<FSAppointmentEmployee.actualDateTimeBegin>(
                                                                                                                              fsAppointmentEmployeeRow,
                                                                                                                              null,
                                                                                                                              new PXSetPropertyException(errorMessage, PXErrorLevel.Error));
                            }
                        }
                    }
                    else
                    {
                        if (fsAppointmentDetRow.ActualDateTimeBegin.HasValue == false
                                || fsAppointmentDetRow.ActualDateTimeEnd.HasValue == false)
                        {
                            errorMessage = PXMessages.LocalizeFormatNoPrefix(TX.Error.ACTUAL_DATE_TIMES_ARE_REQUIRED, TX.WildCards.SERVICE);
                            appointmentDetServicesView.Cache.RaiseExceptionHandling<FSAppointmentDet.actualDateTimeBegin>(
                                                                                                                          fsAppointmentDetRow,
                                                                                                                          null,
                                                                                                                          new PXSetPropertyException(errorMessage, PXErrorLevel.Error));
                        }
                    }
                }
            }

            return string.IsNullOrEmpty(errorMessage) == true;
        }

        public static bool CanAutoRequireServiceActualDateTimes(FSSrvOrdType fsSrvOrdTypeRow)
        {
            return fsSrvOrdTypeRow != null
                        && (fsSrvOrdTypeRow.StartAppointmentActionBehavior == ID.SrvOrdType_StartAppointmentActionBehavior.HEADER_SERVICE_LINES
                            || fsSrvOrdTypeRow.CompleteAppointmentActionBehavior == ID.SrvOrdType_CompleteAppointmentActionBehavior.HEADER_SERVICE_LINES);
        }

        public static void AutoCheckRequireServiceActualDateTimes(PXCache srvOrdTypeCache, FSSrvOrdType fsSrvOrdTypeRow)
        {
            if (CanAutoRequireServiceActualDateTimes(fsSrvOrdTypeRow))
            {
                srvOrdTypeCache.SetValueExt<FSSrvOrdType.requireServiceActualDateTimes>(fsSrvOrdTypeRow, true);
            }
        }

        public static void EnableDisable_RequireServiceActualDateTimes(PXCache srvOrdTypeCache, FSSrvOrdType fsSrvOrdTypeRow)
        {
            bool checkRequireServiceActualDateTimes = CanAutoRequireServiceActualDateTimes(fsSrvOrdTypeRow);
            PXUIFieldAttribute.SetEnabled<FSSrvOrdType.requireServiceActualDateTimes>(srvOrdTypeCache, fsSrvOrdTypeRow, checkRequireServiceActualDateTimes == false);
        }

        #endregion

        #region Modify Actual Start/End Time of Header based on Minimun/Maximun Service Line

        public static bool CanUpdateActualDateTimeWithServiceMinMax(FSSrvOrdType fsSrvOrdTypeRow)
        {
            return fsSrvOrdTypeRow != null
                        && fsSrvOrdTypeRow.UpdateHeaderActualDateTimes == true;
        }

        public static void UpdateActualDateTimeWithServiceMinMax(
                                                                FSSrvOrdType fsSrvOrdTypeRow,
                                                                AppointmentCore.AppointmentRecords_View appointmentRecordsView,
                                                                AppointmentCore.AppointmentDetServices_View appointmentDetServicesView,
                                                                AppointmentCore.AppointmentEmployees_View appointmentEmployeeView)
        {
            DateTime? minServiceTimeBegin = DateTime.MaxValue;
            DateTime? maxServiceTimeEnd = DateTime.MinValue;

            bool updateTimeBegin = false;
            bool updateTimeEnd = false;

            if (CanUpdateActualDateTimeWithServiceMinMax(fsSrvOrdTypeRow))
            {
                foreach (FSAppointmentDetService fsAppointmentDetServiceRow in appointmentDetServicesView.Select())
                {
                    if (fsAppointmentDetServiceRow.StaffRelated != true)
                    {
                        if (fsAppointmentDetServiceRow.ActualDateTimeBegin.HasValue
                                    && minServiceTimeBegin > fsAppointmentDetServiceRow.ActualDateTimeBegin)
                        {
                            minServiceTimeBegin = fsAppointmentDetServiceRow.ActualDateTimeBegin;
                            updateTimeBegin = true;
                        }

                        if (fsAppointmentDetServiceRow.ActualDateTimeEnd.HasValue
                                    && maxServiceTimeEnd < fsAppointmentDetServiceRow.ActualDateTimeEnd)
                        {
                            maxServiceTimeEnd = fsAppointmentDetServiceRow.ActualDateTimeEnd;
                            updateTimeEnd = true;
                        }
                    }
                    else
                    {
                        foreach (FSAppointmentEmployee fsAppointmentEmployeeRow in appointmentEmployeeView.Select().Where(y => ((FSAppointmentEmployee)y).ServiceLineRef == fsAppointmentDetServiceRow.LineRef))
                        {
                            if (fsAppointmentEmployeeRow.ActualDateTimeBegin.HasValue
                                    && minServiceTimeBegin > fsAppointmentEmployeeRow.ActualDateTimeBegin)
                            {
                                minServiceTimeBegin = fsAppointmentEmployeeRow.ActualDateTimeBegin;
                                updateTimeBegin = true;
                            }

                            if (fsAppointmentEmployeeRow.ActualDateTimeEnd.HasValue
                                        && maxServiceTimeEnd < fsAppointmentEmployeeRow.ActualDateTimeEnd)
                            {
                                maxServiceTimeEnd = fsAppointmentEmployeeRow.ActualDateTimeEnd;
                                updateTimeEnd = true;
                            }
                        }
                    }
                }

                if (updateTimeBegin)
                {
                    appointmentRecordsView.Current.ActualDateTimeBegin = minServiceTimeBegin;                    
                }

                if (updateTimeEnd)
                {
                    appointmentRecordsView.Current.ActualDateTimeEnd = maxServiceTimeEnd;
                }
            }
        }
        #endregion

        #region Maintain Actual Start/End Time of Header and Service Lines if changed manually

        public static bool isVisibleKeepActualDateTimes(FSSrvOrdType fsSrvOrdTypeRow)
        {
            return fsSrvOrdTypeRow != null
                        && fsSrvOrdTypeRow.KeepActualDateTimes == true;
        }

        public static void SetVisible_KeepServiceActualDateTimesCheckBox(FSSrvOrdType fsSrvOrdTypeRow, PXCache appointmentDetCache)
        {
            bool visible = isVisibleKeepActualDateTimes(fsSrvOrdTypeRow);
            PXUIFieldAttribute.SetVisible<FSAppointmentDetService.keepActualDateTimes>(appointmentDetCache, null, visible);
        }

        public static void SetVisible_KeepStaffActualDateTimesCheckBox(FSSrvOrdType fsSrvOrdTypeRow, PXCache appointmentEmployeeCache)
        {
            bool visible = isVisibleKeepActualDateTimes(fsSrvOrdTypeRow);
            PXUIFieldAttribute.SetVisible<FSAppointmentDetEmployee.keepActualDateTimes>(appointmentEmployeeCache, null, visible);
        }

        public static bool KeepServiceActualDateTimes(FSSrvOrdType fsSrvOrdTypeRow, FSAppointmentDet fsAppointmentDetRow)
        {
            return fsSrvOrdTypeRow != null
                        && fsAppointmentDetRow != null
                            && fsSrvOrdTypeRow.KeepActualDateTimes == true
                                && fsAppointmentDetRow.KeepActualDateTimes == true;
        }

        public static bool KeepStaffActualDateTimes(FSSrvOrdType fsSrvOrdTypeRow, FSAppointmentEmployee fsAppointmentEmployeeRow)
        {
            return fsSrvOrdTypeRow != null
                        && fsAppointmentEmployeeRow != null
                            && fsSrvOrdTypeRow.KeepActualDateTimes == true
                                && fsAppointmentEmployeeRow.KeepActualDateTimes == true;
        }

        #endregion

        #endregion

        #region Service ActualDateTime Methods

        public static void UpdateServiceActualDateTimeEndFromEstimatedDuration(FSAppointmentDet fsAppointmentDetRow)
        {
            fsAppointmentDetRow.ActualDateTimeEnd = GetServiceActualDateTimeEndFromEstimatedDuration(fsAppointmentDetRow);
        }

        public static void UpdateServiceActualDateTimeEndFromActualDuration(FSAppointmentDet fsAppointmentDetRow)
        {
            if (fsAppointmentDetRow != null
                    && fsAppointmentDetRow.ActualDateTimeBegin != null)
            {
                fsAppointmentDetRow.ActualDateTimeEnd = fsAppointmentDetRow.ActualDateTimeBegin.Value.AddMinutes((int)fsAppointmentDetRow.ActualDuration);
            }
        }

        public static DateTime? GetServiceActualDateTimeEndFromEstimatedDuration(FSAppointmentDet fsAppointmentDetRow)
        {
            DateTime? actualDateTimeEnd = null;

            if (fsAppointmentDetRow != null)
            {
                int minutes = GetValidDuration(fsAppointmentDetRow.EstimatedDuration);
                actualDateTimeEnd = fsAppointmentDetRow.ActualDateTimeBegin.Value.AddMinutes(minutes);
            }

            return actualDateTimeEnd;
        }

        public static int GetValidDuration(int? duration)
        {
            if (duration == null || duration == 0)
            {
                return AppointmentDateTimeHelper.DEFAULT_DURATION_MINUTES;
            }
            else
            {
                return (int)duration;
            }
        }

        public static DateTime? GetServiceActualDateTimeEnd(FSAppointmentDet fsAppointmentDetRow)
        {
            if (fsAppointmentDetRow.ActualDateTimeBegin == null)
            {
                return null;
            }

            DateTime? newEndDate = SharedFunctions.GetTimeWithSpecificDate(PXTimeZoneInfo.Now, fsAppointmentDetRow.ActualDateTimeBegin, true);

            if (SharedFunctions.AreTimePartsEqual(fsAppointmentDetRow.ActualDateTimeBegin.Value, newEndDate.Value) == true)
            {
                newEndDate = newEndDate.Value.AddMinutes(1);
            }

            return newEndDate;
        }

        public static DateTime? GetServiceActualDateTimeBegin(FSAppointmentDet fsAppointmentDetRow)
        {
            DateTime? newStartDate = PXTimeZoneInfo.Now;
            newStartDate = newStartDate.Value.AddSeconds(-newStartDate.Value.Second);

            if (fsAppointmentDetRow != null)
            {
                return newStartDate;
            }

            return null;
        }

        public static void UpdateStaffActualTimeFromService(FSAppointmentDet fsAppointmentDetServiceRow, AppointmentCore.AppointmentEmployees_View appointmentEmployeeView)
        {
            FSAppointmentEmployee fsAppointmentEmployeeRow = appointmentEmployeeView.Select()
                                                                                    .RowCast<FSAppointmentEmployee>()
                                                                                    .Where(_ => _.ServiceLineRef == fsAppointmentDetServiceRow.LineRef)
                                                                                    .First();

            if (fsAppointmentEmployeeRow != null)
            {
                fsAppointmentEmployeeRow.ActualDateTimeBegin = fsAppointmentDetServiceRow.ActualDateTimeBegin;
                fsAppointmentEmployeeRow.ActualDateTimeEnd = fsAppointmentDetServiceRow.ActualDateTimeEnd;
                fsAppointmentEmployeeRow.ActualDuration = fsAppointmentDetServiceRow.ActualDuration;
                appointmentEmployeeView.Update(fsAppointmentEmployeeRow);
            }
        }

        public static void UpdateServiceActualTimeBeginFromStaff(
                                                                 PXCache cache,
                                                                 AppointmentCore.AppointmentDetServices_View appointmentDetServicesView,
                                                                 FSAppointmentEmployee fsAppointmentEmployeeRow, 
                                                                 FSAppointmentDetService fsAppointmentDetRow = null)
        {
            if (fsAppointmentEmployeeRow.ServiceLineRef != null)
            {
                if (fsAppointmentDetRow == null)
                {
                    fsAppointmentDetRow = (FSAppointmentDetService)PXSelectorAttribute.Select<FSAppointmentEmployee.serviceLineRef>(cache, fsAppointmentEmployeeRow);
                }

                if (fsAppointmentDetRow?.StaffRelatedCount == 1)
                {
                    fsAppointmentDetRow.ActualDateTimeBegin = fsAppointmentEmployeeRow.ActualDateTimeBegin;
                    appointmentDetServicesView.Update(fsAppointmentDetRow);
                }
            }
        }

        public static void UpdateServiceActualTimeEndFromStaff(
                                                               PXCache cache,
                                                               AppointmentCore.AppointmentDetServices_View appointmentDetServicesView,
                                                               FSAppointmentEmployee fsAppointmentEmployeeRow,
                                                               FSAppointmentDetService fsAppointmentDetRow = null)
        {
            if (fsAppointmentEmployeeRow.ServiceLineRef != null)
            {
                if (fsAppointmentDetRow == null)
                {
                    fsAppointmentDetRow = (FSAppointmentDetService)PXSelectorAttribute.Select<FSAppointmentEmployee.serviceLineRef>(cache, fsAppointmentEmployeeRow);
                }

                if (fsAppointmentDetRow?.StaffRelatedCount == 1)
                {
                    fsAppointmentDetRow.ActualDateTimeEnd = fsAppointmentEmployeeRow.ActualDateTimeEnd;
                    appointmentDetServicesView.Update(fsAppointmentDetRow);
                }
            }
        }

        #endregion

        #region Staff ActualDateTimeEnd Methods

        public static void UpdateStaffActualDateTimeEndFromEstimatedDuration(FSAppointmentDet fsAppointmentDetRow, FSAppointmentEmployee fsAppointmentEmployeeRow)
        {
            fsAppointmentEmployeeRow.ActualDateTimeEnd = GetStaffActualDateTimeEndFromEstimatedDuration(fsAppointmentDetRow, fsAppointmentEmployeeRow);
        }

        public static void UpdateStaffActualDateTimeEndFromActualDuration(FSAppointmentEmployee fsAppointmentEmployeeRow)
        {
            if (fsAppointmentEmployeeRow != null
                    && fsAppointmentEmployeeRow.ActualDateTimeBegin != null)
            {
                int minutes = GetValidDuration(fsAppointmentEmployeeRow.ActualDuration);
                if (minutes > 0)
                {
                    fsAppointmentEmployeeRow.ActualDateTimeEnd = fsAppointmentEmployeeRow.ActualDateTimeBegin.Value.AddMinutes(minutes);
                }
            }
        }

        public static DateTime? GetStaffActualDateTimeEndFromEstimatedDuration(FSAppointmentDet fsAppointmentDetRow, FSAppointmentEmployee fsAppointmentEmployeeRow)
        {
            DateTime? actualDateTimeEnd = null;

            if (fsAppointmentDetRow != null 
                    && fsAppointmentEmployeeRow != null
                        && fsAppointmentEmployeeRow.ActualDateTimeBegin.HasValue)
            {
                int minutes = GetValidDuration(fsAppointmentDetRow.EstimatedDuration);
                actualDateTimeEnd = fsAppointmentEmployeeRow.ActualDateTimeBegin.Value.AddMinutes(minutes);
            }

            return actualDateTimeEnd;
        }

        public static DateTime? GetStaffActualDateTimeEnd(FSAppointmentEmployee fsAppointmentDetRow)
        {
            if (fsAppointmentDetRow.ActualDateTimeBegin == null)
            {
                return null;
            }

            DateTime? newEndDate = SharedFunctions.GetTimeWithSpecificDate(PXTimeZoneInfo.Now, fsAppointmentDetRow.ActualDateTimeBegin, true);

            if (SharedFunctions.AreTimePartsEqual(fsAppointmentDetRow.ActualDateTimeBegin.Value, newEndDate.Value) == true)
            {
                newEndDate = newEndDate.Value.AddMinutes(DEFAULT_DURATION_MINUTES);
            }

            return newEndDate;
        }

        public static DateTime? GetStaffActualDateTimeBegin(FSAppointmentEmployee fsAppointmentEmployeeRow)
        {
            DateTime? newStartDate = SharedFunctions.GetTimeWithSpecificDate(PXTimeZoneInfo.Now, PXTimeZoneInfo.Now, true);
            newStartDate = newStartDate.Value.AddSeconds(-newStartDate.Value.Second);

            if (fsAppointmentEmployeeRow != null)
            {
                return newStartDate;
            }

            return null;
        }

        #endregion

        public static void CalculateAppointmentDetServiceActualDuration(PXCache cache, FSAppointmentDetService fsAppointmentDetServiceRow)
        {
            if (fsAppointmentDetServiceRow.StaffRelated == true && fsAppointmentDetServiceRow.ActualDateTimeBegin == null)
            {
                cache.SetValueExtIfDifferent<FSAppointmentDetService.actualDuration>(fsAppointmentDetServiceRow, fsAppointmentDetServiceRow.StaffActualDuration);
            }
            else if (fsAppointmentDetServiceRow.ActualDateTimeBegin != null && fsAppointmentDetServiceRow.ActualDateTimeEnd != null)
            {
                TimeSpan dateDiff = SharedFunctions.GetTimeSpanDiff(fsAppointmentDetServiceRow.ActualDateTimeBegin.Value, fsAppointmentDetServiceRow.ActualDateTimeEnd.Value);

                cache.SetValueExtIfDifferent<FSAppointmentDetService.actualDuration>(fsAppointmentDetServiceRow, (int?)dateDiff.TotalMinutes); 
            }
            else
            {
                cache.SetValueExtIfDifferent<FSAppointmentDetService.actualDuration>(fsAppointmentDetServiceRow, fsAppointmentDetServiceRow.EstimatedDuration); 
            }
        }
    }
}
