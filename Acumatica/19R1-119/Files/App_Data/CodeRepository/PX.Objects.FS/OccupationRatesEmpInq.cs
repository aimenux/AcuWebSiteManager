//@TODO SD-6610
//using System;
//using System.Collections;
//using PX.Data;
//using PX.Objects.CR;
//using PX.Objects.EP;

//namespace PX.Objects.FS
//{
//    public class OccupationRatesEmpInq : PXGraph<OccupationRatesEmpInq>
//    {
//        #region Declaration
//        public PXFilter<OccupationalRatesFilter> Filter;
//        public PXCancel<OccupationalRatesFilter> Cancel;

//        public PXSelect<BAccount> BAccountRecords;

//        [PXFilterable]
//        public PXSelectJoin<BAccount,
//                            InnerJoin<EPEmployee,
//                                    On<EPEmployee.bAccountID, Equal<BAccount.bAccountID>>,
//                            LeftJoin<FSAppointmentEmployee,
//                                    On<FSAppointmentEmployee.employeeID, Equal<EPEmployee.bAccountID>>,
//                            LeftJoin<FSAppointment,
//                                    On<FSAppointment.appointmentID, Equal<FSAppointmentEmployee.appointmentID>>>>>,
//                            Where<
//                                FSxEPEmployee.sDEnabled,Equal<True>>,
//                            OrderBy<
//                                    Asc<BAccount.acctCD>>> EmployeeRecords;

//        #endregion

//        #region EventFilter
//        public OccupationRatesEmpInq()
//        {
//            this.EmployeeRecords.Cache.AllowInsert = false;
//            this.EmployeeRecords.Cache.AllowDelete = false;
//            this.EmployeeRecords.Cache.AllowUpdate = false;

//            PXUIFieldAttribute.SetDisplayName<BAccount.acctCD>(EmployeeRecords.Cache, "Employee ID");
//            PXUIFieldAttribute.SetDisplayName<BAccount.acctName>(EmployeeRecords.Cache, "Employee Name");
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

//        protected virtual IEnumerable employeeRecords()
//        {
//            #region SQL - EmployeeRecords
//            //Select SUM(FSAppointment.actualDurationTotal), * FROM BAccount
//            //INNER JOIN    EPEmployee           on (BAccount.BAccountID = EPEmployee.bAccountID)
//            //LEFT JOIN     FSAppoinmentEmployee on (FSAppointmentEmployee.employeeID = EPEmployee.bAccountID)
//            //LEFT JOIN     FSAppointment        on (FSAppointment.appointmentID = FSAppointmentEmployee.appointmentID AND
//            //                                       FSAppointment.Status = 'C' AND
//            //                                       FSAppointment.ActualDateTimeBegin BETWEEN 'XXXXXX' AND 'XXXXXX')
//            //WHERE
//            //    EPEmployee.sDEnabled = CONVERT(BIT, 1)
//            //GROUP BY BAccount.BAccountID
//            //ORDER BY MAX(BAccount.AcctCD) 
//            #endregion

//            PXSelectBase<BAccount> cmd = new PXSelectJoinGroupBy<
//                            BAccount,
//                                InnerJoin<EPEmployee,
//                                        On<EPEmployee.bAccountID, Equal<BAccount.bAccountID>>,
//                                LeftJoin<FSAppointmentEmployee,
//                                        On<FSAppointmentEmployee.employeeID, Equal<EPEmployee.bAccountID>>,
//                                LeftJoin<FSAppointment,
//                                        On<FSAppointment.appointmentID, Equal<FSAppointmentEmployee.appointmentID>,
//                                        And<FSAppointment.status, Equal<ListField_Status_Appointment.Completed>,
//                                        And<FSAppointment.actualDateTimeBegin, Between<Current<OccupationalRatesFilter.dateBegin>, Current<OccupationalRatesFilter.dateEnd>>>>>>>>,
//                                Where<
//                                    FSxEPEmployee.sDEnabled, Equal<True>>,
//                            Aggregate<GroupBy<
//                                BAccount.bAccountID,
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
//            EPEmployee epEmployeeRow;

//            foreach (PXResult<BAccount, EPEmployee, FSAppointmentEmployee, FSAppointment> row in cmd.Select())
//            {
//                fsAppointmentRow = (FSAppointment)row;
//                epEmployeeRow = (EPEmployee)row;

//                PXResultset<FSWrkEmployeeSchedule> fsWrkEmployeeScheduleSet = PXSelect<FSWrkEmployeeSchedule,
//                                                                                Where<
//                                                                                        FSWrkEmployeeSchedule.employeeID, Equal<Required<FSWrkEmployeeSchedule.employeeID>>,
//                                                                                    And<FSWrkEmployeeSchedule.timeStart, GreaterEqual<Required<FSWrkEmployeeSchedule.timeStart>>,
//                                                                                    And<FSWrkEmployeeSchedule.timeEnd, LessEqual<Required<FSWrkEmployeeSchedule.timeEnd>>>>>>
//                                                                                .Select(this, epEmployeeRow.BAccountID, Filter.Current.DateBegin, Filter.Current.DateEnd);
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
//        #endregion
//    }
//}