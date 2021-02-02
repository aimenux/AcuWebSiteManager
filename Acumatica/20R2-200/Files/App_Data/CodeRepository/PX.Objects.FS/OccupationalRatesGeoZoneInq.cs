//@TODO SD-6610
//using System;
//using System.Collections;
//using System.Collections.Generic;
//using PX.Data;
//using PX.Objects.EP;

//namespace PX.Objects.FS
//{
//    public class OccupationalRatesGeoZoneInq : PXGraph<OccupationalRatesGeoZoneInq>
//    {

//        #region Declaration
//        public PXFilter<OccupationalRatesFilter> Filter;
//        public PXCancel<OccupationalRatesFilter> Cancel;

//        [PXFilterable]
//        public PXSelectReadonly<OccupationalGeoZoneHelper> AppointmentGeoZone;
//        #endregion

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

//        public virtual IEnumerable appointmentGeoZone()        
//        {
//            var occupationByGeoZoneList = new List<OccupationalGeoZoneHelper>();

//            PXSelectBase<EPEmployee> cmd = new PXSelectJoin<EPEmployee,
//                                                    InnerJoin<FSGeoZoneEmp,
//                                                                On<FSGeoZoneEmp.employeeID, Equal<EPEmployee.bAccountID>>,
//                                                    InnerJoin<FSGeoZone,
//                                                            On<FSGeoZone.geoZoneID,Equal<FSGeoZoneEmp.geoZoneID>>,
//                                                    LeftJoin<FSAppointmentEmployee,
//                                                            On<FSAppointmentEmployee.employeeID, Equal<FSGeoZoneEmp.employeeID>>,
//                                                    LeftJoin<FSAppointment,
//                                                            On<FSAppointment.appointmentID, Equal<FSAppointmentEmployee.appointmentID>,
//                                                            And<FSAppointment.status, Equal<ListField_Status_Appointment.Completed>,
//                                                            And<FSAppointment.actualDateTimeBegin, Between<Current<OccupationalRatesFilter.dateBegin>, Current<OccupationalRatesFilter.dateEnd>>>>>>>>>,
//                                                        Where<
//                                                        FSxEPEmployee.sDEnabled, Equal<True>>>
//                                                    (this);

//            if (Filter.Current.DateInRange == null)
//            {
//                return occupationByGeoZoneList;
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
//                return occupationByGeoZoneList;
//            }

//            FSAppointment fsAppointmentRow;
//            FSGeoZoneEmp fsGeoZoneEmployeeRow;
//            FSGeoZone fsGeoZoneRow;            
//            foreach (PXResult<EPEmployee, FSGeoZoneEmp, FSGeoZone, FSAppointmentEmployee, FSAppointment> row in cmd.Select())
//            {
//                fsAppointmentRow = (FSAppointment)row;
//                fsGeoZoneEmployeeRow = (FSGeoZoneEmp)row;
//                fsGeoZoneRow = (FSGeoZone)row;

//                PXResultset<FSWrkEmployeeSchedule> fsWrkEmployeeScheduleSet = PXSelect<FSWrkEmployeeSchedule,
//                                                                                Where<
//                                                                                        FSWrkEmployeeSchedule.employeeID, Equal<Required<FSWrkEmployeeSchedule.employeeID>>,
//                                                                                    And<FSWrkEmployeeSchedule.timeStart, GreaterEqual<Required<FSWrkEmployeeSchedule.timeStart>>,
//                                                                                    And<FSWrkEmployeeSchedule.timeEnd, LessEqual<Required<FSWrkEmployeeSchedule.timeEnd>>>>>>
//                                                                                .Select(this, fsGeoZoneEmployeeRow.EmployeeID, Filter.Current.DateBegin, Filter.Current.DateEnd);

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

//                bool GeoZoneFound = false;
//                foreach (OccupationalGeoZoneHelper GeoZoneRow in occupationByGeoZoneList)
//                {
//                    if (GeoZoneRow.GeoZone == fsGeoZoneRow.GeoZoneID)
//                    {
//                        GeoZoneRow.ScheduledHours   += fsAppointmentRow.Mem_ScheduledHours;
//                        GeoZoneRow.AppointmentHours += fsAppointmentRow.Mem_AppointmentHours;
//                        if (GeoZoneRow.ScheduledHours != 0)
//                        {
//                            GeoZoneRow.OccupationalRate = GeoZoneRow.AppointmentHours / GeoZoneRow.ScheduledHours;
//                            GeoZoneRow.IdleRate = 1 - GeoZoneRow.OccupationalRate;
//                            GeoZoneRow.OccupationalRate = GeoZoneRow.OccupationalRate * 100;
//                            GeoZoneRow.IdleRate = GeoZoneRow.IdleRate * 100;
//                        }
//                        else
//                        {
//                            GeoZoneRow.IdleRate = null;
//                        }
//                        GeoZoneFound = true;
//                        break;
//                    }
//                }

//                if (GeoZoneFound == false)
//                {
//                    OccupationalGeoZoneHelper newElement = new OccupationalGeoZoneHelper();
//                    newElement.GeoZone                   = fsGeoZoneRow.GeoZoneID;
//                    newElement.ScheduledHours            = fsAppointmentRow.Mem_ScheduledHours;
//                    newElement.AppointmentHours          = fsAppointmentRow.Mem_AppointmentHours;
//                    if (newElement.ScheduledHours != 0)
//                    {
//                        newElement.OccupationalRate = fsAppointmentRow.Mem_AppointmentHours / fsAppointmentRow.Mem_ScheduledHours;
//                        newElement.IdleRate = 1 - newElement.OccupationalRate;
//                        newElement.OccupationalRate = newElement.OccupationalRate * 100;
//                        newElement.IdleRate = newElement.IdleRate * 100;
//                    }
//                    else
//                    {
//                        newElement.IdleRate = null;
//                    }
//                    occupationByGeoZoneList.Add(newElement);
//                }                
//            }
//            return occupationByGeoZoneList;                
//        }
//    }
//}
