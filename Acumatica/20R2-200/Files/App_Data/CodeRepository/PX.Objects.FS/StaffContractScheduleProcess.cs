using PX.Data;
using PX.Objects.CR;
using PX.Objects.FS.Scheduler;
using PX.SM;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace PX.Objects.FS
{
    public class StaffContractScheduleProcess : PXGraph<StaffContractScheduleProcess>
    {
        #region Global Variables
        public int? nextGenerationID = null;
        #endregion

        #region Generation Functions

        #region Delegate
        public StaffContractScheduleProcess()
        {
            StaffContractScheduleProcess processor = null;

            StaffSchedules.SetProcessDelegate(
                delegate(List<FSStaffSchedule> fsScheduleRowList)
                {
                    processor = PXGraph.CreateInstance<StaffContractScheduleProcess>();

                    int index = 0;
                    foreach (FSStaffSchedule fsScheduleRow in fsScheduleRowList)
                    {
                        try
                        {
                            DateTime? fromDate = fsScheduleRow.LastGeneratedElementDate ?? fsScheduleRow.StartDate;

                            processor.processStaffSchedule(Filter.Cache, processor, fsScheduleRow, fromDate, Filter.Current.ToDate);

                            PXProcessing<FSStaffSchedule>.SetInfo(index, TX.Messages.RECORD_PROCESSED_SUCCESSFULLY);
                        }
                        catch (Exception e)
                        {
                            PXProcessing<FSStaffSchedule>.SetError(index, e);
                        }

                        index++;
                    }
                });
        }
        #endregion

        /// <summary>
        /// Process all FSStaffSchedule. Generates one or more TimeSlot in the Scheduler Module.
        /// </summary>
        protected virtual void processStaffSchedule(PXCache cache, StaffContractScheduleProcess staffContractScheduleEnq, FSStaffSchedule fsScheduleRow, DateTime? fromDate, DateTime? toDate)
        {
            List<Schedule> mapScheduleResults = new List<Schedule>();
            mapScheduleResults = MapFSScheduleToSchedule.convertFSScheduleToSchedule(cache, fsScheduleRow, toDate, ID.RecordType_ServiceContract.EMPLOYEE_SCHEDULE_CONTRACT);
            generateTimeSlotAndUpdateStaffSchedule(mapScheduleResults, ID.RecordType_ServiceContract.EMPLOYEE_SCHEDULE_CONTRACT, fromDate, toDate, fsScheduleRow);
        }

        /// <summary>
        /// Generates an FSTimeSlot for each TimeSlot in the [scheduleRules] List.
        /// </summary>
        protected virtual void generateTimeSlotAndUpdateStaffSchedule(List<Schedule> scheduleRules, string recordType, DateTime? fromDate, DateTime? toDate, FSSchedule fsScheduleRow)
        {
            var generator = new TimeSlotGenerator();
            DateTime processEndDate = (DateTime)getProcessEndDate(scheduleRules.ElementAt(0), toDate);

            var period = new Period((DateTime)fromDate, (DateTime)toDate);

            // Determines the next generationID number
            if (nextGenerationID == null)
            {
                FSProcessIdentity fsProcessIdentityRow = new FSProcessIdentity()
                {
                    ProcessType = recordType,
                    FilterFromTo = fromDate,
                    FilterUpTo = toDate
                };

                var graphProcessIdentityMaint = PXGraph.CreateInstance<ProcessIdentityMaint>();

                graphProcessIdentityMaint.processIdentityRecords.Insert(fsProcessIdentityRow);

                graphProcessIdentityMaint.Save.Press();

                nextGenerationID = graphProcessIdentityMaint.processIdentityRecords.Current.ProcessID;
            }

            List<TimeSlot> timeSlots = generator.GenerateCalendar(period, scheduleRules, nextGenerationID);
            DateTime? failsOnDate = null;

            // Transaction to create the Time Slots
            using (PXTransactionScope ts = new PXTransactionScope())
            {
                try
                {
                    //Create all time slots
                    foreach (var timeSlot in timeSlots)
                    {
                        failsOnDate = timeSlot.DateTimeBegin;
                        createTimeSlot(timeSlot);
                    }

                    DateTime? lastGeneratedTimeSlotBySchedules = null;

                    if (timeSlots.Count > 0)
                    {
                        lastGeneratedTimeSlotBySchedules = timeSlots.Max(a => a.DateTimeBegin);
                    }

                    //Update and create a Contract Generation History
                    createContractGenerationHistory((int)nextGenerationID,
                                                    scheduleRules.ElementAt(0).ScheduleID,
                                                    processEndDate,
                                                    lastGeneratedTimeSlotBySchedules,
                                                    recordType);

                    //Update Last Generated Time Slot Date
                    updateGeneratedSchedule(scheduleRules.ElementAt(0).ScheduleID, processEndDate, lastGeneratedTimeSlotBySchedules, fsScheduleRow);
                }
                catch (PXException e)
                {
                    FSGenerationLogError fsGenerationLogError = new FSGenerationLogError()
                    {
                        ProcessType = recordType,
                        ErrorMessage = e.Message,
                        ScheduleID = scheduleRules.ElementAt(0).ScheduleID,
                        GenerationID = nextGenerationID,
                        ErrorDate = failsOnDate
                    };
                    ts.Dispose();

                    var grapGenerationLogErrorMaint = PXGraph.CreateInstance<GenerationLogErrorMaint>();

                    grapGenerationLogErrorMaint.LogErrorMessageRecords.Insert(fsGenerationLogError);

                    grapGenerationLogErrorMaint.Save.Press();

                    throw new PXException(e.Message);
                }

                ts.Complete(this);
            }
        }

        /// <summary>
        /// Create an FSTimeSlot from a TimeSlot.
        /// </summary>
        protected virtual void createTimeSlot(TimeSlot timeSlot)
        {
            var staffScheduleEntry = PXGraph.CreateInstance<TimeSlotMaint>();

            FSStaffSchedule fsStaffScheduleRow = PXSelect<FSStaffSchedule,
                                                 Where<
                                                     FSStaffSchedule.scheduleID, Equal<Required<FSStaffSchedule.scheduleID>>>>
                                                 .Select(this, timeSlot.ScheduleID);

            FSTimeSlot fsTimeSlotRow = new FSTimeSlot();
            DateTime dateBegin, dateEnd;
            
            fsTimeSlotRow.BranchID = fsStaffScheduleRow.BranchID;
            fsTimeSlotRow.BranchLocationID = fsStaffScheduleRow.BranchLocationID;
            fsTimeSlotRow.EmployeeID = fsStaffScheduleRow.EmployeeID;

            dateBegin = new DateTime(timeSlot.DateTimeBegin.Year, timeSlot.DateTimeBegin.Month, timeSlot.DateTimeBegin.Day, fsStaffScheduleRow.StartTime.Value.Hour, fsStaffScheduleRow.StartTime.Value.Minute, fsStaffScheduleRow.StartTime.Value.Second);
            dateEnd = new DateTime(timeSlot.DateTimeEnd.Year, timeSlot.DateTimeEnd.Month, timeSlot.DateTimeEnd.Day, fsStaffScheduleRow.EndTime.Value.Hour, fsStaffScheduleRow.EndTime.Value.Minute, fsStaffScheduleRow.EndTime.Value.Second);

            TimeSpan duration = dateEnd - dateBegin;

            fsTimeSlotRow.TimeStart = dateBegin;
            fsTimeSlotRow.TimeEnd = dateEnd;
            fsTimeSlotRow.RecordCount = 1;
            fsTimeSlotRow.ScheduleType = fsStaffScheduleRow.ScheduleType;
            fsTimeSlotRow.TimeDiff = (decimal)duration.TotalMinutes;
            fsTimeSlotRow.ScheduleID = timeSlot.ScheduleID;
            fsTimeSlotRow.GenerationID = timeSlot.GenerationID;

            fsTimeSlotRow = staffScheduleEntry.TimeSlotRecords.Insert(fsTimeSlotRow);

            staffScheduleEntry.Save.Press();
        }

        /// <summary>
        /// Create a ContractGenerationHistory.
        /// </summary>
        protected virtual void createContractGenerationHistory(int nextProcessID, int scheduleID, DateTime lastProcessDate, DateTime? lastGeneratedTimeSlotDate, string recordType)
        {
            var graphContractGenerationHistory = PXGraph.CreateInstance<ContractGenerationHistoryMaint>();

            FSContractGenerationHistory fsContractGenerationHistoryRow = new FSContractGenerationHistory()
            {
                GenerationID = nextProcessID,
                ScheduleID = scheduleID,
                LastProcessedDate = lastProcessDate,
                LastGeneratedElementDate = lastGeneratedTimeSlotDate,
                EntityType = ID.Schedule_EntityType.EMPLOYEE,
                RecordType = recordType
            };
            FSContractGenerationHistory fsContractGenerationHistoryRow_Previous = getLastGenerationHistoryRowBySchedule(scheduleID);

            if (fsContractGenerationHistoryRow_Previous != null && fsContractGenerationHistoryRow_Previous.ContractGenerationHistoryID != null)
            {
                if (fsContractGenerationHistoryRow_Previous != null)
                {
                    fsContractGenerationHistoryRow.PreviousGeneratedElementDate = fsContractGenerationHistoryRow_Previous.LastGeneratedElementDate;
                    fsContractGenerationHistoryRow.PreviousProcessedDate = fsContractGenerationHistoryRow_Previous.LastProcessedDate;

                    if (lastGeneratedTimeSlotDate == null)
                    {
                        fsContractGenerationHistoryRow.LastGeneratedElementDate = fsContractGenerationHistoryRow.PreviousGeneratedElementDate;
                    }
                }
            }

            graphContractGenerationHistory.ContractGenerationHistoryRecords.Insert(fsContractGenerationHistoryRow);

            graphContractGenerationHistory.Save.Press();
        }

        /// <summary>
        /// Update an Schedule (lastGeneratedTimeSlotBySchedules and lastProcessedDate).
        /// </summary>
        protected virtual void updateGeneratedSchedule(int scheduleID, DateTime? toDate, DateTime? lastGeneratedTimeSlotBySchedules, FSSchedule fsScheduleRow)
        {
            FSSchedule fsScheduleRow_InDB = PXSelect<FSSchedule,
                                            Where<
                                                FSSchedule.scheduleID, Equal<Required<FSSchedule.scheduleID>>>>
                                            .SelectSingleBound(this, null, scheduleID);

            if (fsScheduleRow_InDB != null)
            {
                if (lastGeneratedTimeSlotBySchedules != null)
                {
                    if (fsScheduleRow != null)
                    {
                        fsScheduleRow.LastGeneratedElementDate = lastGeneratedTimeSlotBySchedules;
                        fsScheduleRow.NextExecutionDate = SharedFunctions.GetNextExecution(this.StaffSchedules.Cache, fsScheduleRow);
                    }

                    PXUpdate<
                        Set<FSSchedule.lastGeneratedElementDate, Required<FSSchedule.lastGeneratedElementDate>,
                        Set<FSSchedule.nextExecutionDate, Required<FSSchedule.nextExecutionDate>>>, FSSchedule,
                    Where<
                        FSSchedule.scheduleID, Equal<Required<FSSchedule.scheduleID>>>>
                    .Update(this, lastGeneratedTimeSlotBySchedules, fsScheduleRow.NextExecutionDate, scheduleID);
                }
            }
        }

        /// <summary>
        /// Return the last FSContractGenerationHistory.
        /// </summary>
        protected virtual FSContractGenerationHistory getLastGenerationHistoryRow(string recordType)
        {
            FSContractGenerationHistory fsContractGenerationHistoryRow = PXSelectGroupBy<FSContractGenerationHistory,
                                                                         Where<
                                                                             FSContractGenerationHistory.recordType, Equal<Required<FSContractGenerationHistory.recordType>>>,
                                                                         Aggregate<
                                                                             Max<FSContractGenerationHistory.generationID>>>
                                                                         .Select(this, recordType);

            if (fsContractGenerationHistoryRow != null && fsContractGenerationHistoryRow.GenerationID != null)
            {
                return fsContractGenerationHistoryRow;
            }
            else
            {
                return null;
            }
        }

        /// <summary>
        /// Return the last FSContractGenerationHistory by Schedule.
        /// </summary>
        protected virtual FSContractGenerationHistory getLastGenerationHistoryRowBySchedule(int scheduleID)
        {
            FSContractGenerationHistory fsContractGenerationHistoryRow = PXSelectGroupBy<FSContractGenerationHistory,
                                                                         Where<
                                                                             FSContractGenerationHistory.scheduleID, Equal<Required<FSContractGenerationHistory.scheduleID>>>,
                                                                         Aggregate<
                                                                             Max<FSContractGenerationHistory.generationID>>>
                                                                         .Select(this, scheduleID);

            if (fsContractGenerationHistoryRow != null && fsContractGenerationHistoryRow.GenerationID != null)
            {
                return fsContractGenerationHistoryRow;
            }
            else
            {
                return null;
            }
        }

        /// <summary>
        /// Return the smallest date between schedule EndDate and Process EndDate.
        /// </summary>
        protected virtual DateTime? getProcessEndDate(Schedule fsScheduleRule, DateTime? toDate)
        {
            FSSchedule fsScheduleRow = PXSelect<FSSchedule,
                                       Where<
                                            FSSchedule.scheduleID, Equal<Required<FSSchedule.scheduleID>>>>
                                       .Select(this, fsScheduleRule.ScheduleID);

            if (fsScheduleRow.EnableExpirationDate == false)
            {
                return toDate.Value;
            }
            else
            {
                return (fsScheduleRow.EndDate < toDate) ? fsScheduleRow.EndDate : toDate.Value;
            }
        }

        #endregion

        #region CacheAttache
        #region BAccount_AcctName_
        [PXDBString(60, IsUnicode = true)]
        [PXUIField(DisplayName = "Employee Name", Enabled = false)]
        protected virtual void BAccount_AcctName_CacheAttached(PXCache sender)
        {
        }
        #endregion
        #region FSStaffSchedule_LastGeneratedElementDate
        [PXDBDate]
        [PXUIField(DisplayName = "Last Generated Schedule")]
        protected virtual void FSStaffSchedule_LastGeneratedElementDate_CacheAttached(PXCache sender)
        {
        }
        #endregion
        #endregion

        #region Filter+Select
        public PXSelect<BAccount> BAccountRecords;

        public PXFilter<StaffScheduleFilter> Filter;
        public PXCancel<StaffScheduleFilter> Cancel;

        [PXFilterable]
        [PXViewDetailsButton(typeof(StaffScheduleFilter))]
        public PXFilteredProcessingJoin<FSStaffSchedule, StaffScheduleFilter,
                InnerJoin<BAccount,
                    On<
                        BAccount.bAccountID, Equal<FSStaffSchedule.employeeID>,
                        And<BAccount.status, NotEqual<BAccount.status.inactive>>>>,
                Where<
                    FSStaffSchedule.active, Equal<True>,
                    And2<Where<
                            FSStaffSchedule.nextExecutionDate, LessEqual<Current<StaffScheduleFilter.toDate>>,
                            And<
                                Where<FSStaffSchedule.enableExpirationDate, Equal<False>,
                                    Or<FSStaffSchedule.endDate, Greater<FSStaffSchedule.nextExecutionDate>>>>>,
                    //Start filter conditions
                    And2<Where<Current<StaffScheduleFilter.bAccountID>, IsNull,
                            Or<FSStaffSchedule.employeeID, Equal<Current<StaffScheduleFilter.bAccountID>>>>,
                    And2<Where<Current<StaffScheduleFilter.branchID>, IsNull,
                            Or<FSStaffSchedule.branchID, Equal<Current<StaffScheduleFilter.branchID>>>>,
                    And2<Where<Current<StaffScheduleFilter.branchLocationID>, IsNull,
                            Or<FSStaffSchedule.branchLocationID, Equal<Current<StaffScheduleFilter.branchLocationID>>>>,
                    And2<Where<Current<StaffScheduleFilter.scheduleID>, IsNull,
                            Or<FSStaffSchedule.scheduleID, Equal<Current<StaffScheduleFilter.scheduleID>>>>,
                    And<FSStaffSchedule.startDate, LessEqual<Current<StaffScheduleFilter.toDate>>>>>>>>>> StaffSchedules;
                    //End filter conditions

        public PXSelectGroupBy<FSContractGenerationHistory,
                Where<
                    FSContractGenerationHistory.recordType, Equal<FSContractGenerationHistory.recordType.EmployeeScheduleContract>>,
                Aggregate<
                    GroupBy<FSContractGenerationHistory.generationID>>,
                OrderBy<
                    Desc<FSContractGenerationHistory.generationID>>> ContractHistoryRecords;
        #endregion

        #region Actions
        #region FixSchedulesWithoutNextExecutionDate
        public PXAction<StaffScheduleFilter> fixSchedulesWithoutNextExecutionDate;
        [PXUIField(DisplayName = "Fix Schedules Without Next Execution Date", Visible = false)]
        public virtual IEnumerable FixSchedulesWithoutNextExecutionDate(PXAdapter adapter)
        {
            SharedFunctions.UpdateSchedulesWithoutNextExecution(this, this.Filter.Cache);
            return adapter.Get();
        }
        #endregion
        #endregion

        #region Event Handlers
        protected virtual void _(Events.RowSelected<StaffScheduleFilter> e)
        {
            if (e.Row == null)
            {
                return;
            }
            
            string warningMessage = "";
            bool warning = false;
            StaffScheduleFilter staffScheduleFilterRow = (StaffScheduleFilter)e.Row;

            warningMessage = SharedFunctions.WarnUserWithSchedulesWithoutNextExecution(this, e.Cache, fixSchedulesWithoutNextExecutionDate, out warning);

            if (warning == true)
            { 
                e.Cache.RaiseExceptionHandling<StaffScheduleFilter.toDate>(staffScheduleFilterRow,
                                                                           staffScheduleFilterRow.ToDate,
                                                                           new PXSetPropertyException(warningMessage, PXErrorLevel.Warning));
            }
        }

        #endregion
    }
}