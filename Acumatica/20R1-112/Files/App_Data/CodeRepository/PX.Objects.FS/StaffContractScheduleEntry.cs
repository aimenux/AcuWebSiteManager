using PX.Data;
using PX.Objects.CR;
using PX.Objects.EP;
using PX.Objects.GL;
using System;

namespace PX.Objects.FS
{
    public class StaffContractScheduleEntry : PXGraph<StaffContractScheduleEntry, FSStaffSchedule>
    {
        public PXSelect<FSStaffSchedule> StaffScheduleRecords;
        public PXSelect<FSStaffSchedule, Where<FSStaffSchedule.scheduleID, Equal<Current<FSStaffSchedule.scheduleID>>>> StaffScheduleSelected;

        #region CacheAttached
        #region FSStaffSchedule_EndDate
        public abstract class endDate : PX.Data.BQL.BqlDateTime.Field<endDate> { }

        [PXDBDateAndTime(UseTimeZone = true, PreserveTime = true, DisplayNameDate = "End Date", DisplayNameTime = "End Time")]
        [PXDefault]
        [PXUIField(DisplayName = "End Date", Visibility = PXUIVisibility.SelectorVisible)]
        protected virtual void FSStaffSchedule_EndDate_CacheAttached(PXCache sender)
        {
        }
        #endregion
        #region FSStaffSchedule_BranchLocationID
        public abstract class branchLocationID : PX.Data.BQL.BqlInt.Field<branchLocationID> { }

        [PXDBInt]
        [PXDefault(PersistingCheck = PXPersistingCheck.Nothing)]
        [PXFormula(typeof(Default<FSStaffSchedule.branchID>))]
        [PXUIField(DisplayName = "Branch Location")]
        [FSSelectorBranchLocationByFSSchedule]
        protected virtual void FSStaffSchedule_BranchLocationID_CacheAttached(PXCache sender)
        {
        }
        #endregion
        #endregion

        #region Selects
        [PXHidden]
        public PXSetup<FSSetup> SetupRecord;
        #endregion

        #region Actions

        #region OpenStaffContractScheduleProcess
        public PXAction<FSStaffSchedule> openStaffContractScheduleProcess;
        [PXUIField(DisplayName = "Generate Staff Schedules", MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select)]
        [PXButton(OnClosingPopup = PXSpecialButtonType.Cancel)]
        public virtual void OpenStaffContractScheduleProcess()
        {
            StaffContractScheduleProcess staffContractScheduleProcessGraph = PXGraph.CreateInstance<StaffContractScheduleProcess>();

            StaffScheduleFilter filter = new StaffScheduleFilter();
            filter.ScheduleID = StaffScheduleRecords.Current.ScheduleID;
            filter.BAccountID = StaffScheduleRecords.Current.EmployeeID;

            //FSStaffSchedule.StartDate implements the PXDBDateAndTime attribute allowing values of hour, month and minute be > 0.
            //So, the filter must cover all time possible values in the current date in order to show this schedule .
            if (StaffScheduleRecords.Current.EndDate != null)
            {
                filter.ToDate = new DateTime(StaffScheduleRecords.Current.EndDate.Value.Year,
                                             StaffScheduleRecords.Current.EndDate.Value.Month,
                                             StaffScheduleRecords.Current.EndDate.Value.Day,
                                             23,
                                             59,
                                             59);
            }
            else
            {
                DateTime? dateTime_aux = StaffScheduleRecords.Current.StartDate.Value.AddYears(1);
                filter.ToDate = new DateTime(dateTime_aux.Value.Year,
                                             dateTime_aux.Value.Month,
                                             dateTime_aux.Value.Day,
                                             23,
                                             59,
                                             59);
            }

            staffContractScheduleProcessGraph.Filter.Insert(filter);

            throw new PXRedirectRequiredException(staffContractScheduleProcessGraph, null) { Mode = PXBaseRedirectException.WindowMode.NewWindow };
        }
        #endregion

        #endregion

        #region Events

        #region FSStaffSchedule
        #region FieldSelecting
        #endregion
        #region FieldDefaulting
        protected virtual void _(Events.FieldDefaulting<FSStaffSchedule, FSStaffSchedule.startTime> e)        
        {
            DateTime? bussinessDatePlusCurrentHours = new DateTime(Accessinfo.BusinessDate.Value.Year,
                                                                   Accessinfo.BusinessDate.Value.Month,
                                                                   Accessinfo.BusinessDate.Value.Day,
                                                                   DateTime.Now.Hour,
                                                                   DateTime.Now.Minute,
                                                                   DateTime.Now.Second);
            e.NewValue = bussinessDatePlusCurrentHours.Value;
        }

        protected virtual void _(Events.FieldDefaulting<FSStaffSchedule, FSStaffSchedule.endTime> e)
        {
            DateTime? bussinessDatePlusCurrentHours = new DateTime(Accessinfo.BusinessDate.Value.Year,
                                                                   Accessinfo.BusinessDate.Value.Month,
                                                                   Accessinfo.BusinessDate.Value.Day,
                                                                   DateTime.Now.Hour,
                                                                   DateTime.Now.Minute,
                                                                   DateTime.Now.Second);

            e.NewValue = bussinessDatePlusCurrentHours.Value.AddHours(1);
        }
        #endregion
        #region FieldUpdating
        #endregion
        #region FieldVerifying
        #endregion
        #region FieldUpdated
        protected virtual void _(Events.FieldUpdated<FSStaffSchedule, FSStaffSchedule.employeeID> e)
        {
            if (e.Row == null)
            {
                return;
            }

            FSStaffSchedule fsStaffScheduleRow = (FSStaffSchedule)e.Row;

            Branch branchRow = PXSelectJoin<Branch,
                               InnerJoin<BAccount,
                               On<
                                   BAccount.parentBAccountID, Equal<Branch.bAccountID>>,
                               InnerJoin<EPEmployee,
                               On<
                                   EPEmployee.bAccountID, Equal<BAccount.bAccountID>>>>,
                               Where<
                                   EPEmployee.bAccountID, Equal<Required<FSStaffSchedule.employeeID>>>>
                               .Select(this, fsStaffScheduleRow.EmployeeID);

            fsStaffScheduleRow.BranchID = branchRow.BranchID;
        }
        #endregion

        protected virtual void _(Events.RowSelecting<FSStaffSchedule> e)
        {
        }

        protected virtual void _(Events.RowSelected<FSStaffSchedule> e)
        {
            if (e.Row == null)
            {
                return;
            }

            FSStaffSchedule fsStaffScheduleRow = (FSStaffSchedule)e.Row;
            PXCache cache = e.Cache;

            bool enableExpirationDate = fsStaffScheduleRow.EnableExpirationDate.HasValue ? (bool)fsStaffScheduleRow.EnableExpirationDate : false;

            SetControlsState(cache, fsStaffScheduleRow);

            PXUIFieldAttribute.SetEnabled<FSStaffSchedule.monthly3Selected>(cache, fsStaffScheduleRow, fsStaffScheduleRow.Monthly2Selected == true);
            PXUIFieldAttribute.SetEnabled<FSStaffSchedule.monthly4Selected>(cache, fsStaffScheduleRow, fsStaffScheduleRow.Monthly3Selected == true);
            PXUIFieldAttribute.SetEnabled<FSStaffSchedule.endDate>(cache, fsStaffScheduleRow, enableExpirationDate || IsCopyPasteContext);
            PXDefaultAttribute.SetPersistingCheck<FSStaffSchedule.endDate>(cache,
                                                                           fsStaffScheduleRow,
                                                                           enableExpirationDate ? PXPersistingCheck.NullOrBlank : PXPersistingCheck.Nothing);

            fsStaffScheduleRow.SrvOrdTypeMessage = TX.Messages.SERVICE_ORDER_TYPE_USED_FOR_RECURRING_APPOINTMENTS;
            CheckDates(cache, fsStaffScheduleRow);
            CheckTimes(cache, fsStaffScheduleRow);

            FSSchedule fsScheduleRow = (FSSchedule)e.Row;
            SharedFunctions.SetRecurrenceDescription(fsScheduleRow);

            bool existAnyGenerationProcess = SharedFunctions.ShowWarningScheduleNotProcessed(cache, fsScheduleRow);
            openStaffContractScheduleProcess.SetEnabled(existAnyGenerationProcess == false);
        }

        protected virtual void _(Events.RowInserting<FSStaffSchedule> e)
        {
        }

        protected virtual void _(Events.RowInserted<FSStaffSchedule> e)
        {
        }

        protected virtual void _(Events.RowUpdating<FSStaffSchedule> e)
        {
        }

        protected virtual void _(Events.RowUpdated<FSStaffSchedule> e)
        {
        }

        protected virtual void _(Events.RowDeleting<FSStaffSchedule> e)
        {
        }

        protected virtual void _(Events.RowDeleted<FSStaffSchedule> e)
        {
            if (e.Row == null)
            {
                return;
            }

            FSStaffSchedule fsContractScheduleRow = (FSStaffSchedule)e.Row;

            //Detaching TimeSlots created by schedule generation linked to this Schedule.
            PXUpdate<
                Set<FSTimeSlot.scheduleID, Required<FSTimeSlot.scheduleID>>,
            FSTimeSlot,
            Where<
                FSTimeSlot.scheduleID, Equal<Required<FSTimeSlot.scheduleID>>>>
            .Update(this, null, fsContractScheduleRow.ScheduleID);
        }

        protected virtual void _(Events.RowPersisting<FSStaffSchedule> e)
        {
            if (e.Row == null)
            {
                return;
            }

            FSStaffSchedule fsStaffScheduleRow = (FSStaffSchedule)e.Row;
            PXCache cache = e.Cache;

            if (fsStaffScheduleRow.EnableExpirationDate == false && fsStaffScheduleRow.EndDate != null)
            {
                fsStaffScheduleRow.EndDate = null;
            }

            if (e.Operation == PXDBOperation.Insert || e.Operation == PXDBOperation.Update)
            {
                fsStaffScheduleRow.NextExecutionDate = SharedFunctions.GetNextExecution(cache, fsStaffScheduleRow);

                CheckDates(cache, fsStaffScheduleRow);
                CheckTimes(cache, fsStaffScheduleRow);

                if (e.Operation == PXDBOperation.Insert)
                {
                    StaffScheduleRecords.Ask(TX.ModuleName.SERVICE_DISPATCH,
                                             TX.Warning.SCHEDULE_WILL_NOT_AFFECT_SYSTEM_UNTIL_GENERATION_OCCURS,
                                             MessageButtons.OK,
                                             MessageIcon.Warning);
                }
            }
        }

        protected virtual void _(Events.RowPersisted<FSStaffSchedule> e)
        {
        }

        #endregion
        #endregion

        #region Virtual Methods

        /// <summary>
        /// Check if Start Date is prior to End Date.
        /// </summary>
        /// <param name="cache">FSStaffSchedule cache.</param>
        /// <param name="fsStaffScheduleRow">FSStaffSchedule Row.</param>
        public virtual void CheckDates(PXCache cache, FSStaffSchedule fsStaffScheduleRow)
        { 
            if (fsStaffScheduleRow.StartDate == null
                || fsStaffScheduleRow.EndDate == null)
            {
                return;
            }

            PXSetPropertyException exception = null;
            DateTime startDate  = (DateTime)fsStaffScheduleRow.StartDate;
            DateTime endDate    = (DateTime)fsStaffScheduleRow.EndDate;

            if (startDate.Year > endDate.Year
                || (startDate.Year == endDate.Year && startDate.Month > endDate.Month)
                || (startDate.Year == endDate.Year && startDate.Month == endDate.Month && startDate.Day > endDate.Day))
            {
                exception = new PXSetPropertyException(TX.Error.END_DATE_LESSER_THAN_START_DATE, PXErrorLevel.RowError);
            }

            cache.RaiseExceptionHandling<FSStaffSchedule.startDate>(fsStaffScheduleRow, startDate, exception);
            cache.RaiseExceptionHandling<FSStaffSchedule.endDate>(fsStaffScheduleRow, endDate, exception);
        }

        /// <summary>
        /// Check if Start Time is prior to End Time.
        /// </summary>
        /// <param name="cache">FSStaffSchedule cache.</param>
        /// <param name="fsStaffScheduleRow">FSStaffSchedule Row.</param>
        public virtual void CheckTimes(PXCache cache, FSStaffSchedule fsStaffScheduleRow)
        {
            if (fsStaffScheduleRow.StartTime == null
                || fsStaffScheduleRow.EndTime == null)
            {
                return;
            }

            PXSetPropertyException exception = null;
            DateTime startTime = (DateTime)fsStaffScheduleRow.StartTime;
            DateTime endTime = (DateTime)fsStaffScheduleRow.EndTime;

            if (startTime.Hour > endTime.Hour
                || (startTime.Hour == endTime.Hour && startTime.Minute >= endTime.Minute))
            {
                exception = new PXSetPropertyException(TX.Error.END_TIME_LESSER_THAN_START_TIME, PXErrorLevel.RowError);
            }

            cache.RaiseExceptionHandling<FSStaffSchedule.startTime>(fsStaffScheduleRow, startTime, exception);
            cache.RaiseExceptionHandling<FSStaffSchedule.endTime>(fsStaffScheduleRow, endTime, exception);

            if (fsStaffScheduleRow.StartDate != null
                && fsStaffScheduleRow.EndDate != null)
            {
                fsStaffScheduleRow.StartDate = new DateTime(
                                                           fsStaffScheduleRow.StartDate.Value.Year,
                                                           fsStaffScheduleRow.StartDate.Value.Month,
                                                           fsStaffScheduleRow.StartDate.Value.Day,
                                                           fsStaffScheduleRow.StartTime.Value.Hour,
                                                           fsStaffScheduleRow.StartTime.Value.Minute,
                                                           fsStaffScheduleRow.StartTime.Value.Second);

                fsStaffScheduleRow.EndDate = new DateTime(
                                                           fsStaffScheduleRow.EndDate.Value.Year,
                                                           fsStaffScheduleRow.EndDate.Value.Month,
                                                           fsStaffScheduleRow.EndDate.Value.Day,
                                                           fsStaffScheduleRow.EndTime.Value.Hour,
                                                           fsStaffScheduleRow.EndTime.Value.Minute,
                                                           fsStaffScheduleRow.EndTime.Value.Second);
            }
        }

        /// <summary>
        /// Makes visible the group that corresponds to the selected FrequencyType.
        /// </summary>
        public virtual void SetControlsState(PXCache cache, FSSchedule fsScheduleRow)
        {
            bool isWeekly           = fsScheduleRow.FrequencyType     == ID.Schedule_FrequencyType.WEEKLY;
            bool isDaily            = fsScheduleRow.FrequencyType     == ID.Schedule_FrequencyType.DAILY;
            bool isMonthly          = fsScheduleRow.FrequencyType     == ID.Schedule_FrequencyType.MONTHLY;
            bool isAnnually         = fsScheduleRow.FrequencyType     == ID.Schedule_FrequencyType.ANNUAL;
            bool isMonthly2Selected = fsScheduleRow.Monthly2Selected  == true;
            bool isMonthly3Selected = fsScheduleRow.Monthly3Selected  == true;
            bool isMonthly4Selected = fsScheduleRow.Monthly4Selected  == true;

            //Daily Frequency
            PXUIFieldAttribute.SetVisible<FSSchedule.dailyFrequency>(cache, fsScheduleRow, isDaily);
            PXUIFieldAttribute.SetVisible<FSSchedule.dailyLabel>(cache, fsScheduleRow, isDaily);

            //Weekly Frequency
            PXUIFieldAttribute.SetVisible<FSSchedule.weeklyFrequency>(cache, fsScheduleRow, isWeekly);
            PXUIFieldAttribute.SetVisible<FSSchedule.weeklyOnSun>(cache, fsScheduleRow, isWeekly);
            PXUIFieldAttribute.SetVisible<FSSchedule.weeklyOnMon>(cache, fsScheduleRow, isWeekly);
            PXUIFieldAttribute.SetVisible<FSSchedule.weeklyOnTue>(cache, fsScheduleRow, isWeekly);
            PXUIFieldAttribute.SetVisible<FSSchedule.weeklyOnWed>(cache, fsScheduleRow, isWeekly);
            PXUIFieldAttribute.SetVisible<FSSchedule.weeklyOnThu>(cache, fsScheduleRow, isWeekly);
            PXUIFieldAttribute.SetVisible<FSSchedule.weeklyOnFri>(cache, fsScheduleRow, isWeekly);
            PXUIFieldAttribute.SetVisible<FSSchedule.weeklyOnSat>(cache, fsScheduleRow, isWeekly);
            PXUIFieldAttribute.SetVisible<FSSchedule.weeklyLabel>(cache, fsScheduleRow, isWeekly);

            ////Monthly Frequency
            SetMonthlyControlsState(cache, fsScheduleRow);
            PXUIFieldAttribute.SetVisible<FSSchedule.monthlyFrequency>(cache, fsScheduleRow, isMonthly);
            PXUIFieldAttribute.SetVisible<FSSchedule.monthlyLabel>(cache, fsScheduleRow, isMonthly);
            PXUIFieldAttribute.SetVisible<FSSchedule.monthly2Selected>(cache, fsScheduleRow, isMonthly);
            PXUIFieldAttribute.SetVisible<FSSchedule.monthly3Selected>(cache, fsScheduleRow, isMonthly);
            PXUIFieldAttribute.SetVisible<FSSchedule.monthly4Selected>(cache, fsScheduleRow, isMonthly);
            PXUIFieldAttribute.SetVisible<FSSchedule.monthlyRecurrenceType1>(cache, fsScheduleRow, isMonthly);
            PXUIFieldAttribute.SetVisible<FSSchedule.monthlyRecurrenceType2>(cache, fsScheduleRow, isMonthly && isMonthly2Selected);
            PXUIFieldAttribute.SetVisible<FSSchedule.monthlyRecurrenceType3>(cache, fsScheduleRow, isMonthly && isMonthly3Selected);
            PXUIFieldAttribute.SetVisible<FSSchedule.monthlyRecurrenceType4>(cache, fsScheduleRow, isMonthly && isMonthly4Selected);
            PXUIFieldAttribute.SetVisible<FSSchedule.monthlyOnDay1>(cache, fsScheduleRow, isMonthly && fsScheduleRow.MonthlyRecurrenceType1 == ID.Schedule_FrequencyType.DAILY);
            PXUIFieldAttribute.SetVisible<FSSchedule.monthlyOnDay2>(cache, fsScheduleRow, isMonthly && isMonthly2Selected && fsScheduleRow.MonthlyRecurrenceType2 == ID.Schedule_FrequencyType.DAILY);
            PXUIFieldAttribute.SetVisible<FSSchedule.monthlyOnDay3>(cache, fsScheduleRow, isMonthly && isMonthly3Selected && fsScheduleRow.MonthlyRecurrenceType3 == ID.Schedule_FrequencyType.DAILY);
            PXUIFieldAttribute.SetVisible<FSSchedule.monthlyOnDay4>(cache, fsScheduleRow, isMonthly && isMonthly4Selected && fsScheduleRow.MonthlyRecurrenceType4 == ID.Schedule_FrequencyType.DAILY);
            PXUIFieldAttribute.SetVisible<FSSchedule.monthlyOnWeek1>(cache, fsScheduleRow, isMonthly && fsScheduleRow.MonthlyRecurrenceType1 == ID.Schedule_FrequencyType.WEEKLY);
            PXUIFieldAttribute.SetVisible<FSSchedule.monthlyOnWeek2>(cache, fsScheduleRow, isMonthly && isMonthly2Selected && fsScheduleRow.MonthlyRecurrenceType2 == ID.Schedule_FrequencyType.WEEKLY);
            PXUIFieldAttribute.SetVisible<FSSchedule.monthlyOnWeek3>(cache, fsScheduleRow, isMonthly && isMonthly3Selected && fsScheduleRow.MonthlyRecurrenceType3 == ID.Schedule_FrequencyType.WEEKLY);
            PXUIFieldAttribute.SetVisible<FSSchedule.monthlyOnWeek4>(cache, fsScheduleRow, isMonthly && isMonthly4Selected && fsScheduleRow.MonthlyRecurrenceType4 == ID.Schedule_FrequencyType.WEEKLY);
            PXUIFieldAttribute.SetVisible<FSSchedule.monthlyOnDayOfWeek1>(cache, fsScheduleRow, isMonthly && fsScheduleRow.MonthlyRecurrenceType1 == ID.Schedule_FrequencyType.WEEKLY);
            PXUIFieldAttribute.SetVisible<FSSchedule.monthlyOnDayOfWeek2>(cache, fsScheduleRow, isMonthly && isMonthly2Selected && fsScheduleRow.MonthlyRecurrenceType2 == ID.Schedule_FrequencyType.WEEKLY);
            PXUIFieldAttribute.SetVisible<FSSchedule.monthlyOnDayOfWeek3>(cache, fsScheduleRow, isMonthly && isMonthly3Selected && fsScheduleRow.MonthlyRecurrenceType3 == ID.Schedule_FrequencyType.WEEKLY);
            PXUIFieldAttribute.SetVisible<FSSchedule.monthlyOnDayOfWeek4>(cache, fsScheduleRow, isMonthly && isMonthly4Selected && fsScheduleRow.MonthlyRecurrenceType4 == ID.Schedule_FrequencyType.WEEKLY);

            ////Annual Frequency
            PXUIFieldAttribute.SetVisible<FSSchedule.annualFrequency>(cache, fsScheduleRow, isAnnually);
            PXUIFieldAttribute.SetVisible<FSSchedule.yearlyLabel>(cache, fsScheduleRow, isAnnually);
            PXUIFieldAttribute.SetVisible<FSSchedule.annualRecurrenceType>(cache, fsScheduleRow, isAnnually);
            PXUIFieldAttribute.SetVisible<FSSchedule.annualOnDay>(cache, fsScheduleRow, isAnnually && fsScheduleRow.AnnualRecurrenceType == ID.Schedule_FrequencyType.DAILY);
            PXUIFieldAttribute.SetVisible<FSSchedule.annualOnWeek>(cache, fsScheduleRow, isAnnually && fsScheduleRow.AnnualRecurrenceType == ID.Schedule_FrequencyType.WEEKLY);
            PXUIFieldAttribute.SetVisible<FSSchedule.annualOnDayOfWeek>(cache, fsScheduleRow, isAnnually && fsScheduleRow.AnnualRecurrenceType == ID.Schedule_FrequencyType.WEEKLY);
            PXUIFieldAttribute.SetEnabled<FSSchedule.annualOnDay>(cache, fsScheduleRow, isAnnually && fsScheduleRow.AnnualRecurrenceType == ID.Schedule_FrequencyType.DAILY);
            PXUIFieldAttribute.SetEnabled<FSSchedule.annualOnWeek>(cache, fsScheduleRow, isAnnually && fsScheduleRow.AnnualRecurrenceType == ID.Schedule_FrequencyType.WEEKLY);
            PXUIFieldAttribute.SetEnabled<FSSchedule.annualOnDayOfWeek>(cache, fsScheduleRow, isAnnually && fsScheduleRow.AnnualRecurrenceType == ID.Schedule_FrequencyType.WEEKLY);
        }

        public virtual void SetMonthlyControlsState(PXCache cache, FSSchedule fsScheduleRow)
        {
            bool isMonthly = fsScheduleRow.FrequencyType == ID.Schedule_FrequencyType.MONTHLY;
            
            PXUIFieldAttribute.SetEnabled<FSSchedule.monthlyOnDay1>(cache, fsScheduleRow, isMonthly && fsScheduleRow.MonthlyRecurrenceType1 == ID.Schedule_FrequencyType.DAILY);
            PXUIFieldAttribute.SetEnabled<FSSchedule.monthlyOnWeek1>(cache, fsScheduleRow, isMonthly && fsScheduleRow.MonthlyRecurrenceType1 == ID.Schedule_FrequencyType.WEEKLY);
            PXUIFieldAttribute.SetEnabled<FSSchedule.monthlyOnDayOfWeek1>(cache, fsScheduleRow, isMonthly && fsScheduleRow.MonthlyRecurrenceType1 == ID.Schedule_FrequencyType.WEEKLY);

            PXUIFieldAttribute.SetEnabled<FSSchedule.monthlyOnDay2>(cache, fsScheduleRow, isMonthly && fsScheduleRow.MonthlyRecurrenceType2 == ID.Schedule_FrequencyType.DAILY);
            PXUIFieldAttribute.SetEnabled<FSSchedule.monthlyOnWeek2>(cache, fsScheduleRow, isMonthly && fsScheduleRow.MonthlyRecurrenceType2 == ID.Schedule_FrequencyType.WEEKLY);
            PXUIFieldAttribute.SetEnabled<FSSchedule.monthlyOnDayOfWeek2>(cache, fsScheduleRow, isMonthly && fsScheduleRow.MonthlyRecurrenceType2 == ID.Schedule_FrequencyType.WEEKLY);

            PXUIFieldAttribute.SetEnabled<FSSchedule.monthlyOnDay3>(cache, fsScheduleRow, isMonthly && fsScheduleRow.MonthlyRecurrenceType3 == ID.Schedule_FrequencyType.DAILY);
            PXUIFieldAttribute.SetEnabled<FSSchedule.monthlyOnWeek3>(cache, fsScheduleRow, isMonthly && fsScheduleRow.MonthlyRecurrenceType3 == ID.Schedule_FrequencyType.WEEKLY);
            PXUIFieldAttribute.SetEnabled<FSSchedule.monthlyOnDayOfWeek3>(cache, fsScheduleRow, isMonthly && fsScheduleRow.MonthlyRecurrenceType3 == ID.Schedule_FrequencyType.WEEKLY);

            PXUIFieldAttribute.SetEnabled<FSSchedule.monthlyOnDay4>(cache, fsScheduleRow, isMonthly && fsScheduleRow.MonthlyRecurrenceType4 == ID.Schedule_FrequencyType.DAILY);
            PXUIFieldAttribute.SetEnabled<FSSchedule.monthlyOnWeek4>(cache, fsScheduleRow, isMonthly && fsScheduleRow.MonthlyRecurrenceType4 == ID.Schedule_FrequencyType.WEEKLY);
            PXUIFieldAttribute.SetEnabled<FSSchedule.monthlyOnDayOfWeek4>(cache, fsScheduleRow, isMonthly && fsScheduleRow.MonthlyRecurrenceType4 == ID.Schedule_FrequencyType.WEEKLY);
        }
        #endregion
    }
}