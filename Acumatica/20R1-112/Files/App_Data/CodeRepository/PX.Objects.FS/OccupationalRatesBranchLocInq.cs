//@TODO SD-6610
//using System;
//using System.Collections;
//using PX.Data;
//using PX.Objects.EP;

//namespace PX.Objects.FS
//{
//    public class OccupationalRatesBranchLocInq : PXGraph<OccupationalRatesBranchLocInq>
//    {
//        #region Declaration
//        public PXFilter<OccupationalRatesFilter> Filter;
//        public PXCancel<OccupationalRatesFilter> Cancel;

//        [PXFilterable]
//        public PXSelectJoin<EPEmployee,
//                            InnerJoin<FSEmployeeSchedule,
//                                     On<FSEmployeeSchedule.employeeID, Equal<EPEmployee.bAccountID>>,
//                            LeftJoin<FSAppointmentEmployee,
//                                    On<FSAppointmentEmployee.employeeID,Equal<EPEmployee.bAccountID>>,
//                            LeftJoin<FSAppointment,
//                                     On<FSAppointment.appointmentID,Equal<FSAppointmentEmployee.appointmentID>>>>>,
//                            Where<
//                                FSxEPEmployee.sDEnabled,Equal<True>>,
//                            OrderBy<
//                                    Asc<FSEmployeeSchedule.branchLocationID>>> AppointmentBranchLocation;

//        #endregion

//        public OccupationalRatesBranchLocInq()
//        {
//            this.AppointmentBranchLocation.Cache.AllowInsert = false;
//            this.AppointmentBranchLocation.Cache.AllowDelete = false;
//            this.AppointmentBranchLocation.Cache.AllowUpdate = false;
//        }

//        protected virtual IEnumerable filter()
//        {
//            PXCache cache = this.Caches[typeof(OccupationalRatesFilter)];
//            if (cache != null)
//            {
//                OccupationalRatesFilter filter = cache.Current as OccupationalRatesFilter;
//                if (filter != null 
//                        && filter.DateInRange.HasValue
//                            && !string.IsNullOrEmpty(filter.PeriodType))
//                {
//                    SharedFunctions.DateRange dateRange = SharedFunctions.GetDateRange(filter.DateInRange.Value, filter.PeriodType);

//                    filter.DateBegin = dateRange.startDate;
//                    filter.DateEnd = dateRange.endDate;
//                }
//                else
//                {
//                    filter.DateBegin = null;
//                    filter.DateEnd = null;
//                }
//                yield return cache.Current;
//                cache.IsDirty = false;
//            }
//        }

//        protected virtual IEnumerable appointmentBranchLocation()
//        {
//            #region SQL - AppointmentBranchLocation
//            //Select SUM(FSAppointment.actualDurationTotal), * FROM EPEmployee
//            //INNER JOIN    FSEmployeeSchedule   on (FSEmployeeSchedule.employeeID = EPEmployee.bAccountID)
//            //LEFT JOIN     FSAppoinmentEmployee on (FSAppointmentEmployee.employeeID = EPEmployee.bAccountID)
//            //LEFT JOIN     FSAppointment        on (FSAppointment.appointmentID = FSAppointmentEmployee.appointmentID)
//            //WHERE
//            //    EPEmployee.sDEnabled = CONVERT(BIT, 1)
//            //    AND (
//            //        FSAppointment.AppointmentID is NULL
//            //        OR (
//            //            FSAppointment.Status = 'C'
//            //            AND FSAppointment.ActualDateTimeBegin BETWEEN 'OccupationalRatesFilter.dateBegin' AND 'OccupationalRatesFilter.dateEnd'
//            //        )
//            //    )
//            //GROUP BY FSEmployeeSchedule.branchLocationID
//            //ORDER BY MAX(BAccount.AcctCD) 
//            #endregion

//            PXSelectBase<EPEmployee> cmd = new PXSelectJoinGroupBy<EPEmployee,
//                            InnerJoin<FSEmployeeSchedule,
//                                     On<FSEmployeeSchedule.employeeID, Equal<EPEmployee.bAccountID>>,
//                            LeftJoin<FSAppointmentEmployee,
//                                    On<FSAppointmentEmployee.employeeID, Equal<FSEmployeeSchedule.employeeID>>,
//                            LeftJoin<FSAppointment,
//                                    On<FSAppointment.appointmentID, Equal<FSAppointmentEmployee.appointmentID>,
//                                    And<FSAppointment.status, Equal<ListField_Status_Appointment.Completed>,
//                                    And<FSAppointment.actualDateTimeBegin, Between<Current<OccupationalRatesFilter.dateBegin>, Current<OccupationalRatesFilter.dateEnd>>>>>>>>,
//                            Where<
//                                FSxEPEmployee.sDEnabled,Equal<True>>,
//                            Aggregate<GroupBy<
//                                FSEmployeeSchedule.branchLocationID,
//                                Sum<FSAppointment.actualDurationTotal>>>>
//                            (this);
 
//            if (Filter.Current.DateInRange == null)
//            {
//                yield break;
//            }

//            //Calculate the Scheduled hours
//            int errorNbr = 0;
//            string errorMsg = "";

//            PXDatabase.Execute("FS_pFSEmployee_Schedule_CompanyID_Date_EmployeeList",
//                        new PXSPInParameter("CompanyID", SharedFunctions.GetCurrentCompanyId()),
//                        new PXSPInParameter("BranchID", Accessinfo.BranchID),
//                        new PXSPInParameter("BranchLocationID", null),
//                        new PXSPInParameter("DateMin", Filter.Current.DateBegin),
//                        new PXSPInParameter("DateMax", Filter.Current.DateEnd),
//                        new PXSPInParameter("EmployeeList", null),
//                        new PXSPInParameter("ConsiderateAppointments", 0),
//                        new PXSPOutParameter("ErrorNbr", errorNbr),
//                        new PXSPOutParameter("ErrorMsg", errorMsg)
//                        );

//            if (errorNbr != 0)
//            {
//                yield break;
//            }

//            FSAppointment fsAppointmentRow;
//            FSEmployeeSchedule fsEmployeeScheduleRow;

//            foreach (PXResult<EPEmployee, FSEmployeeSchedule, FSAppointmentEmployee, FSAppointment> row in cmd.Select())
//            {
//                fsAppointmentRow = (FSAppointment)row;
//                fsEmployeeScheduleRow = (FSEmployeeSchedule)row;

//                PXResultset<FSWrkEmployeeSchedule> fsWrkEmployeeScheduleSet = PXSelect<FSWrkEmployeeSchedule,
//                                                                                Where<
//                                                                                        FSWrkEmployeeSchedule.branchLocationID, Equal<Required<FSWrkEmployeeSchedule.branchLocationID>>,
//                                                                                    And<FSWrkEmployeeSchedule.timeStart, GreaterEqual<Required<FSWrkEmployeeSchedule.timeStart>>,
//                                                                                    And<FSWrkEmployeeSchedule.timeEnd, LessEqual<Required<FSWrkEmployeeSchedule.timeEnd>>>>>>
//                                                                                .Select(this, fsEmployeeScheduleRow.BranchLocationID, Filter.Current.DateBegin, Filter.Current.DateEnd);
//                fsAppointmentRow.Mem_ScheduledHours = 0;
//                foreach (FSWrkEmployeeSchedule fsWrkEmployeeScheduleRow in fsWrkEmployeeScheduleSet)
//                {
//                    TimeSpan? diffTime = fsWrkEmployeeScheduleRow.TimeEnd - fsWrkEmployeeScheduleRow.TimeStart;
//                    fsAppointmentRow.Mem_ScheduledHours += (decimal)diffTime.Value.TotalMinutes;
//                }
//                fsAppointmentRow.Mem_ScheduledHours = fsAppointmentRow.Mem_ScheduledHours / (decimal)60;

//                if (fsAppointmentRow.ActualDurationTotal == null)
//                {
//                    fsAppointmentRow.ActualDurationTotal = 0;
//                }
//                fsAppointmentRow.Mem_AppointmentHours = fsAppointmentRow.ActualDurationTotal / (decimal)60;

//                if (fsAppointmentRow.Mem_ScheduledHours != 0)
//                {
//                    fsAppointmentRow.Mem_OccupationalRate = fsAppointmentRow.Mem_AppointmentHours / fsAppointmentRow.Mem_ScheduledHours;
//                    fsAppointmentRow.Mem_IdleRate = 1 - fsAppointmentRow.Mem_OccupationalRate;

//                    fsAppointmentRow.Mem_IdleRate = fsAppointmentRow.Mem_IdleRate * 100;
//                    fsAppointmentRow.Mem_OccupationalRate = fsAppointmentRow.Mem_OccupationalRate * 100;
//                }
//                else
//                {
//                    fsAppointmentRow.Mem_IdleRate = null;
//                }

//                yield return row;
//            }
//        }
//    }
//}aa//@TODO SD-6610
//using System;
//using System.Collections;
//using PX.Data;
//using PX.Objects.EP;

//namespace PX.Objects.FS
//{
//    public class OccupationalRatesBranchLoc : PXGraph<OccupationalRatesBranchLoc>
//    {
//        #region Declaration
//        public PXFilter<OccupationalRatesFilter> Filter;
//        public PXCancel<OccupationalRatesFilter> Cancel;

//        [PXFilterable]
//        public PXSelectJoin<EPEmployee,
//                            InnerJoin<FSEmployeeSchedule,
//                                     On<FSEmployeeSchedule.employeeID, Equal<EPEmployee.bAccountID>>,
//                            LeftJoin<FSAppointmentEmployee,
//                                    On<FSAppointmentEmployee.employeeID,Equal<EPEmployee.bAccountID>>,
//                            LeftJoin<FSAppointment,
//                                     On<FSAppointment.appointmentID,Equal<FSAppointmentEmployee.appointmentID>>>>>,
//                            Where<
//                                FSxEPEmployee.sDEnabled,Equal<True>>,
//                            OrderBy<
//                                    Asc<FSEmployeeSchedule.branchLocationID>>> AppointmentBranchLocation;

//        #endregion

//        public OccupationalRatesBranchLoc()
//        {
//            this.AppointmentBranchLocation.Cache.AllowInsert = false;
//            this.AppointmentBranchLocation.Cache.AllowDelete = false;
//            this.AppointmentBranchLocation.Cache.AllowUpdate = false;
//        }

//        protected virtual IEnumerable filter()
//        {
//            PXCache cache = this.Caches[typeof(OccupationalRatesFilter)];
//            if (cache != null)
//            {
//                OccupationalRatesFilter filter = cache.Current as OccupationalRatesFilter;
//                if (filter != null 
//                        && filter.DateInRange.HasValue
//                            && !string.IsNullOrEmpty(filter.PeriodType))
//                {
//                    SharedFunctions.DateRange dateRange = SharedFunctions.GetDateRange(filter.DateInRange.Value, filter.PeriodType);

//                    filter.DateBegin = dateRange.startDate;
//                    filter.DateEnd = dateRange.endDate;
//                }
//                else
//                {
//                    filter.DateBegin = null;
//                    filter.DateEnd = null;
//                }
//                yield return cache.Current;
//                cache.IsDirty = false;
//            }
//        }

//        protected virtual IEnumerable appointmentBranchLocation()
//        {
//            #region SQL - AppointmentBranchLocation
//            //Select SUM(FSAppointment.actualDurationTotal), * FROM EPEmployee
//            //INNER JOIN    FSEmployeeSchedule   on (FSEmployeeSchedule.employeeID = EPEmployee.bAccountID)
//            //LEFT JOIN     FSAppoinmentEmployee on (FSAppointmentEmployee.employeeID = EPEmployee.bAccountID)
//            //LEFT JOIN     FSAppointment        on (FSAppointment.appointmentID = FSAppointmentEmployee.appointmentID)
//            //WHERE
//            //    EPEmployee.sDEnabled = CONVERT(BIT, 1)
//            //    AND (
//            //        FSAppointment.AppointmentID is NULL
//            //        OR (
//            //            FSAppointment.Status = 'C'
//            //            AND FSAppointment.ActualDateTimeBegin BETWEEN 'OccupationalRatesFilter.dateBegin' AND 'OccupationalRatesFilter.dateEnd'
//            //        )
//            //    )
//            //GROUP BY FSEmployeeSchedule.branchLocationID
//            //ORDER BY MAX(BAccount.AcctCD) 
//            #endregion

//            PXSelectBase<EPEmployee> cmd = new PXSelectJoinGroupBy<EPEmployee,
//                            InnerJoin<FSEmployeeSchedule,
//                                     On<FSEmployeeSchedule.employeeID, Equal<EPEmployee.bAccountID>>,
//                            LeftJoin<FSAppointmentEmployee,
//                                    On<FSAppointmentEmployee.employeeID, Equal<FSEmployeeSchedule.employeeID>>,
//                            LeftJoin<FSAppointment,
//                                    On<FSAppointment.appointmentID, Equal<FSAppointmentEmployee.appointmentID>,
//                                    And<FSAppointment.status, Equal<ListField_Status_Appointment.Completed>,
//                                    And<FSAppointment.actualDateTimeBegin, Between<Current<OccupationalRatesFilter.dateBegin>, Current<OccupationalRatesFilter.dateEnd>>>>>>>>,
//                            Where<
//                                FSxEPEmployee.sDEnabled,Equal<True>>,
//                            Aggregate<GroupBy<
//                                FSEmployeeSchedule.branchLocationID,
//                                Sum<FSAppointment.actualDurationTotal>>>>
//                            (this);

//            if (Filter.Current.DateInRange == null)
//            {
//                yield break;
//            }

//            //Calculate the Scheduled hours
//            int errorNbr = 0;
//            string errorMsg = "";

//            PXDatabase.Execute("FS_pFSEmployee_Schedule_CompanyID_Date_EmployeeList",
//                        new PXSPInParameter("CompanyID", SharedFunctions.GetCurrentCompanyId()),
//                        new PXSPInParameter("BranchID", Accessinfo.BranchID),
//                        new PXSPInParameter("BranchLocationID", null),
//                        new PXSPInParameter("DateMin", Filter.Current.DateBegin),
//                        new PXSPInParameter("DateMax", Filter.Current.DateEnd),
//                        new PXSPInParameter("EmployeeList", null),
//                        new PXSPInParameter("ConsiderateAppointments", 0),
//                        new PXSPOutParameter("ErrorNbr", errorNbr),
//                        new PXSPOutParameter("ErrorMsg", errorMsg)
//                        );

//            if (errorNbr != 0)
//            {
//                yield break;
//            }

//            FSAppointment fsAppointmentRow;
//            FSEmployeeSchedule fsEmployeeScheduleRow;

//            foreach (PXResult<EPEmployee, FSEmployeeSchedule, FSAppointmentEmployee, FSAppointment> row in cmd.Select())
//            {
//                fsAppointmentRow = (FSAppointment)row;
//                fsEmployeeScheduleRow = (FSEmployeeSchedule)row;

//                PXResultset<FSWrkEmployeeSchedule> fsWrkEmployeeScheduleSet = PXSelect<FSWrkEmployeeSchedule,
//                                                                                Where<
//                                                                                        FSWrkEmployeeSchedule.branchLocationID, Equal<Required<FSWrkEmployeeSchedule.branchLocationID>>,
//                                                                                    And<FSWrkEmployeeSchedule.timeStart, GreaterEqual<Required<FSWrkEmployeeSchedule.timeStart>>,
//                                                                                    And<FSWrkEmployeeSchedule.timeEnd, LessEqual<Required<FSWrkEmployeeSchedule.timeEnd>>>>>>
//                                                                                .Select(this, fsEmployeeScheduleRow.BranchLocationID, Filter.Current.DateBegin, Filter.Current.DateEnd);
//                fsAppointmentRow.Mem_ScheduledHours = 0;
//                foreach (FSWrkEmployeeSchedule fsWrkEmployeeScheduleRow in fsWrkEmployeeScheduleSet)
//                {
//                    TimeSpan? diffTime = fsWrkEmployeeScheduleRow.TimeEnd - fsWrkEmployeeScheduleRow.TimeStart;
//                    fsAppointmentRow.Mem_ScheduledHours += (decimal)diffTime.Value.TotalMinutes;
//                }
//                fsAppointmentRow.Mem_ScheduledHours = fsAppointmentRow.Mem_ScheduledHours / (decimal)60;

//                if (fsAppointmentRow.ActualDurationTotal == null)
//                {
//                    fsAppointmentRow.ActualDurationTotal = 0;
//                }
//                fsAppointmentRow.Mem_AppointmentHours = fsAppointmentRow.ActualDurationTotal / (decimal)60;

//                if (fsAppointmentRow.Mem_ScheduledHours != 0)
//                {
//                    fsAppointmentRow.Mem_OccupationalRate = fsAppointmentRow.Mem_AppointmentHours / fsAppointmentRow.Mem_ScheduledHours;
//                    fsAppointmentRow.Mem_IdleRate = 1 - fsAppointmentRow.Mem_OccupationalRate;

//                    fsAppointmentRow.Mem_IdleRate = fsAppointmentRow.Mem_IdleRate * 100;
//                    fsAppointmentRow.Mem_OccupationalRate = fsAppointmentRow.Mem_OccupationalRate * 100;
//                }
//                else
//                {
//                    fsAppointmentRow.Mem_IdleRate = null;
//                }

//                yield return row;
//            }
//        }
//    }
//}