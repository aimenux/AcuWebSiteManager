//@TODO SD-6610
//using System;
//using System.Collections;
//using System.Collections.Generic;
//using System.Linq;
//using PX.Data;
//using PX.Objects.CR;
//using PX.Objects.IN;
//#if Acumatica_4_20
//using PX.SM;
//#endif

//namespace PX.Objects.FS
//{
//    public class EmployeeRoomAvailability : PXGraph<EmployeeRoomAvailability>
//    {

//        #region Actions
//        [PXViewDetailsButton(typeof(EmployeeRoomFilter))]
//        public PXAction<EmployeeRoomFilter> createAppointment;
//        [PXUIField(DisplayName = "Create Appointment", MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select)]
//        [PXButton]
//        public virtual IEnumerable CreateAppointment(PXAdapter adapter)
//        {
//            AppointmentEntry appointmentEntry = PXGraph.CreateInstance<AppointmentEntry>();            
//            EmployeeRoomHelper employeeRoomHelperRow = EmployeeRoomAvailabilityRecords.Current;
//            FSAppointment fsAppointmentRow = new FSAppointment();
//            FSAppointmentEmployee fsAppointmentEmployee = new FSAppointmentEmployee();
//            FSAppointmentDetService fsAppointmentDetRow = new FSAppointmentDetService();

//            fsAppointmentRow.SrvOrdType = Setup.Current.DfltSrvOrdType;

//            appointmentEntry.AppointmentRecords.Insert(fsAppointmentRow);

//            appointmentEntry.ServiceOrderRelated.Current.BranchLocationID = employeeRoomHelperRow.BranchLocationID;

//            appointmentEntry.ServiceOrderRelated.Current.RoomID = employeeRoomHelperRow.RoomID;

//            appointmentEntry.AppointmentRecords.Current.ScheduledDateTimeBegin = employeeRoomHelperRow.TimeStart;

//            appointmentEntry.AppointmentRecords.Current.ScheduledDateTimeEnd = employeeRoomHelperRow.TimeStart.Value.AddMinutes((double)employeeRoomHelperRow.ServiceEstimatedDuration);

//            fsAppointmentEmployee.EmployeeID = employeeRoomHelperRow.EmployeeID;

//            appointmentEntry.AppointmentEmployees.Insert(fsAppointmentEmployee);

//            fsAppointmentDetRow.ServiceID = Filter.Current.ServiceID;

//            appointmentEntry.AppointmentDetServices.Insert(fsAppointmentDetRow);

//            PXRedirectHelper.TryRedirect(appointmentEntry, PXRedirectHelper.WindowMode.NewWindow);

//            return adapter.Get();
//        }
//        #endregion
//        #region Filter
//        public PXSetup<FSSetup> Setup;

//        public PXFilter<EmployeeRoomFilter> Filter;
//        public PXCancel<EmployeeRoomFilter> Cancel;

//        public PXSelectReadonly<EmployeeRoomHelper> EmployeeRoomAvailabilityRecords;
//        public PXSelect<BAccount> bAccountRecords;
//        #endregion

//        public virtual IEnumerable employeeRoomAvailabilityRecords()
//        {
//            var newAvailabilityRecordList = new List<EmployeeRoomHelper>();

//            if (Filter.Current.ServiceID == null)
//            {
//                return newAvailabilityRecordList;
//            }

//            int errorNbr = 0;
//            int recordID = 1;
//            string errorMsg = "";
//            FSRoom fsRoomRow = null;
//            SharedFunctions.AppIsContained isContained = SharedFunctions.AppIsContained.NotContained;
//            bool timeCanceledByAppointment = false;
//            DateTime? SlotBegin, SlotEnd;
//            TimeSpan? SlotDuration;
//            string customerAddress = "";
//            PXResult<EmployeeRoomHelper> tempEmployeeRoomHelper;
//            PXResultset<FSWrkEmployeeSchedule> fsWrkEmployeeScheduleSet;
//            PXResultset<FSRoom> fsRoomAppointmentsRows; 
//            PXResultset<FSRoom> fsRoomAppointmentsSpecificUseRows;
//            PXResultset<FSAppointment> fsAppointmentRows;

//            FSBranchLocation fsBranchLocationRow = PXSelect<FSBranchLocation,
//                                    Where<
//                                        FSBranchLocation.branchLocationID, Equal<Required<FSBranchLocation.branchLocationID>>>>
//                                    .Select(this, Filter.Current.BranchLocationID);  

//            List<object> wrkEmployeeScheduleArgs                             = new List<object>();
//            List<object> roomArgs                                            = new List<object>();
//            List<object> roomSpecificArgs                                    = new List<object>();
//            List<object> appointmentsArgs                                    = new List<object>();
//            List<SharedClasses.CpnyLocationDistance> cpnyLocationDistances   = new List<SharedClasses.CpnyLocationDistance>();

//            PXDatabase.Execute("FS_pFSEmployee_Schedule_CompanyID_Date_EmployeeList",
//                        new PXSPInParameter("CompanyID", SharedFunctions.GetCurrentCompanyId()),
//                        new PXSPInParameter("BranchID", fsBranchLocationRow.BranchID),
//                        new PXSPInParameter("BranchLocationID", Filter.Current.BranchLocationID),
//                        new PXSPInParameter("DateMin", Filter.Current.FromDate),
//                        new PXSPInParameter("DateMax", Filter.Current.ToDate),
//                        new PXSPInParameter("EmployeeList", Filter.Current.EmployeeID),
//                        new PXSPInParameter("ConsiderateAppointments", 1 /*Yes, Consider Appointments*/),
//                        new PXSPOutParameter("ErrorNbr", errorNbr),
//                        new PXSPOutParameter("ErrorMsg", errorMsg)
//                        );

//            InventoryItem inventoryItemRow = PXSelectReadonly<InventoryItem,
//                                                Where<
//                                                    InventoryItem.inventoryID, Equal<Required<InventoryItem.inventoryID>>>>
//                                                .Select(this, Filter.Current.ServiceID);

//            FSxService fsxServiceRow = PXCache<InventoryItem>.GetExtension<FSxService>(inventoryItemRow);

//            //FSWrkEmployeeSchedule selection
//            PXSelectBase<FSWrkEmployeeSchedule> wrkEmployeeScheduleBase = new PXSelect<FSWrkEmployeeSchedule>(this);
//            wrkEmployeeScheduleBase.WhereAnd<Where<FSWrkEmployeeSchedule.timeStart, GreaterEqual<Required<FSWrkEmployeeSchedule.timeStart>>>>();
//            wrkEmployeeScheduleBase.WhereAnd<Where<FSWrkEmployeeSchedule.timeEnd, LessEqual<Required<FSWrkEmployeeSchedule.timeEnd>>>>();            
//            wrkEmployeeScheduleBase.WhereAnd<Where<FSWrkEmployeeSchedule.scheduleType, Equal<FSWrkEmployeeSchedule.scheduleType.Availability>>>();
//            wrkEmployeeScheduleArgs.Add(Filter.Current.FromDate);
//            wrkEmployeeScheduleArgs.Add(Filter.Current.ToDate);

//            if (Filter.Current.BranchLocationID != null)
//            {
//                wrkEmployeeScheduleBase.WhereAnd<Where<FSWrkEmployeeSchedule.branchLocationID, Like<Required<FSWrkEmployeeSchedule.branchLocationID>>>>();
//                wrkEmployeeScheduleArgs.Add(Filter.Current.BranchLocationID);
//            }

//            if (Filter.Current.EmployeeID != null)
//            {
//                wrkEmployeeScheduleBase.WhereAnd<Where<FSWrkEmployeeSchedule.employeeID, Like<Required<FSWrkEmployeeSchedule.employeeID>>>>();
//                wrkEmployeeScheduleArgs.Add(Filter.Current.EmployeeID);
//            }
//            fsWrkEmployeeScheduleSet = wrkEmployeeScheduleBase.Select(wrkEmployeeScheduleArgs.ToArray());

//            PXSelectBase<FSRoom> roomBase = new PXSelectOrderBy<FSRoom, OrderBy<Asc<FSRoom.roomID>>>(this);
//            roomBase.WhereAnd<Where<FSRoom.specificUse, Equal<False>, Or<Where<FSRoom.specificUse, IsNull>>>>();

//            if (Filter.Current.BranchLocationID != null)
//            {
//                roomBase.WhereAnd<Where<FSRoom.branchLocationID, Equal<Required<FSRoom.branchLocationID>>>>();
//                roomArgs.Add(Filter.Current.BranchLocationID);
//            }

//            if (Filter.Current.RoomID != null)
//            {
//                roomBase.WhereAnd<Where<FSRoom.roomID, Equal<Required<FSRoom.roomID>>>>();
//                roomArgs.Add(Filter.Current.RoomID);
//            }
//            fsRoomAppointmentsRows = roomBase.Select(roomArgs.ToArray());

//            //FSRoom (specific use) selection
//            PXSelectBase<FSRoom> roomSpecificBase = new PXSelectOrderBy<FSRoom, OrderBy<Asc<FSRoom.roomID>>>(this);
//            roomSpecificBase.WhereAnd<Where<FSRoom.specificUse, Equal<True>>>();

//            if (Filter.Current.BranchLocationID != null)
//            {
//                roomSpecificBase.WhereAnd<Where<FSRoom.branchLocationID, Equal<Required<FSRoom.branchLocationID>>>>();
//                roomSpecificArgs.Add(Filter.Current.BranchLocationID);
//            }

//            if (Filter.Current.RoomID != null)
//            {
//                roomSpecificBase.WhereAnd<Where<FSRoom.roomID, Equal<Required<FSRoom.roomID>>>>();
//                roomSpecificArgs.Add(Filter.Current.RoomID);
//            }
//            fsRoomAppointmentsSpecificUseRows = roomSpecificBase.Select(roomSpecificArgs.ToArray());

//            foreach(PXResult<FSRoom>fsRoomAppointmentsRow in fsRoomAppointmentsSpecificUseRows)
//            {
//                fsRoomRow = (FSRoom)fsRoomAppointmentsRow;
//                FSRoomServiceClass serviceClassRequired =   PXSelectReadonly<FSRoomServiceClass,
//                                                            Where<
//                                                                FSRoomServiceClass.serviceClassID, Equal<Required<FSRoomServiceClass.serviceClassID>>,
//                                                                And<FSRoomServiceClass.branchLocationID,Equal<Required<FSRoomServiceClass.branchLocationID>>,
//                                                                And<FSRoomServiceClass.roomID,Equal<Required<FSRoomServiceClass.roomID>>>>>>
//                                                            .Select(this, inventoryItemRow.ItemClassID, fsRoomRow.BranchLocationID, fsRoomRow.RoomID);
//                if (serviceClassRequired != null)
//                {
//                    fsRoomAppointmentsRows.Add(fsRoomAppointmentsRow);
//                }
//            }
//            fsRoomRow = null;

//            //The address for the Customer Location is obtained
//            if (Filter.Current.customerID != null && Filter.Current.CustomerLocationID != null)
//            {
//                customerAddress = GetCustomerAddress((int)Filter.Current.customerID, (int)Filter.Current.CustomerLocationID);
//            }

//            //The address(es) for the CpnyLocation(s) is obtained
//            cpnyLocationDistances = GetCpnyLocationAddress();

//            //The Google Maps WebService is used to calculate the distance between Customer and CompanyLocation
//            if (customerAddress != string.Empty)
//            {
//                foreach (SharedClasses.CpnyLocationDistance cpnyLocationDistance in cpnyLocationDistances)
//                {
//                    cpnyLocationDistance.distance = CalculatesDistance(customerAddress, cpnyLocationDistance.address);
//                }
//            }

//            //iterations to fill the newAvailabilityRecordList List (grid content)
//            foreach (FSWrkEmployeeSchedule fSWrkEmployeeScheduleRow in fsWrkEmployeeScheduleSet) 
//            {
//                if (fSWrkEmployeeScheduleRow.ScheduleType == ID.ScheduleType.AVAILABILITY)
//                {                    
//                    if (Filter.Current.ConsiderLicenses == true && ValidateLicenses(Filter.Current.ServiceID, fSWrkEmployeeScheduleRow.EmployeeID) == false)
//                    {
//                        continue;
//                    }

//                    if (Filter.Current.ConsiderSkills == true && ValidateSkills(Filter.Current.ServiceID, fSWrkEmployeeScheduleRow.EmployeeID) == false)
//                    {
//                        continue;
//                    }                                                                                                                                            

//                    foreach (PXResult<FSRoom> fsRoomAppointmentsRow in fsRoomAppointmentsRows)
//                    {
//                        SlotBegin = fSWrkEmployeeScheduleRow.TimeStart;
//                        SlotEnd = fSWrkEmployeeScheduleRow.TimeEnd;

//                        fsRoomRow = (FSRoom)fsRoomAppointmentsRow;

//                        DateTime DayBegin = fSWrkEmployeeScheduleRow.TimeStart.Value.Date;
//                        DateTime DayEnd = fSWrkEmployeeScheduleRow.TimeEnd.Value.Date.AddDays(1);

//                        PXSelectBase<FSAppointment> appointmentsBase = new PXSelectJoinOrderBy<FSAppointment,
//                                                                            InnerJoin<FSServiceOrder,
//                                                                        On<FSServiceOrder.sOID, Equal<FSAppointment.sOID>>>,
//                                                                        OrderBy<Asc<FSAppointment.scheduledDateTimeBegin>>>(this);

//                        appointmentsBase.WhereAnd<Where<FSServiceOrder.branchLocationID, Equal<Required<FSServiceOrder.branchLocationID>>>>();
//                        appointmentsBase.WhereAnd<Where<FSServiceOrder.roomID, Equal<Required<FSServiceOrder.roomID>>>>();
//                        appointmentsBase.WhereAnd<Where<FSAppointment.scheduledDateTimeBegin, GreaterEqual<Required<FSAppointment.scheduledDateTimeBegin>>>>();
//                        appointmentsBase.WhereAnd<Where<FSAppointment.scheduledDateTimeEnd, LessEqual<Required<FSAppointment.scheduledDateTimeEnd>>>>();
//                        appointmentsBase.WhereAnd<Where<FSAppointment.scheduledDateTimeBegin, Less<Required<FSAppointment.scheduledDateTimeBegin>>>>();
//                        appointmentsBase.WhereAnd<Where<FSAppointment.scheduledDateTimeEnd, Greater<Required<FSAppointment.scheduledDateTimeEnd>>>>();
//                        appointmentsArgs.Add(fsRoomRow.BranchLocationID);
//                        appointmentsArgs.Add(fsRoomRow.RoomID);
//                        appointmentsArgs.Add(DayBegin);
//                        appointmentsArgs.Add(DayEnd);
//                        appointmentsArgs.Add(fSWrkEmployeeScheduleRow.TimeEnd);
//                        appointmentsArgs.Add(fSWrkEmployeeScheduleRow.TimeStart);

//                        fsAppointmentRows = appointmentsBase.Select(appointmentsArgs.ToArray());
//                        appointmentsArgs.Clear();

//                        if (Filter.Current.RoomAvailability == false)
//                        {
//                            fsAppointmentRows.Clear();
//                        }

//                        //iterations on the appointments for the day
//                        foreach (FSAppointment fsAppointmentRow in fsAppointmentRows)
//                        {
//                            if (fsAppointmentRow != null && fsAppointmentRow.AppointmentID != null)
//                            {
//                                isContained = SharedFunctions.AppointmentIsContained(SlotBegin, SlotEnd, fsAppointmentRow);
//                                if (isContained == SharedFunctions.AppIsContained.Contained)
//                                {
//                                    SlotDuration = fsAppointmentRow.ScheduledDateTimeBegin - SlotBegin;
//                                    if (fsxServiceRow.EstimatedDuration <= SlotDuration.Value.TotalMinutes)
//                                    {
//                                        tempEmployeeRoomHelper = CreateInterval(fSWrkEmployeeScheduleRow, fsRoomRow, fsxServiceRow, SlotBegin, fsAppointmentRow.ScheduledDateTimeBegin, recordID++, cpnyLocationDistances);
//                                        newAvailabilityRecordList.Add(tempEmployeeRoomHelper);
//                                    }
//                                    SlotBegin = fsAppointmentRow.ScheduledDateTimeEnd;
//                                }
//                                if (isContained == SharedFunctions.AppIsContained.PartiallyContained && fsAppointmentRow.ScheduledDateTimeBegin < SlotBegin)
//                                {
//                                    SlotBegin = fsAppointmentRow.ScheduledDateTimeEnd;
//                                }
//                                if (isContained == SharedFunctions.AppIsContained.PartiallyContained && fsAppointmentRow.ScheduledDateTimeBegin < SlotEnd)
//                                {
//                                    if (SlotBegin < fsAppointmentRow.ScheduledDateTimeBegin)
//                                    {
//                                        SlotEnd = fsAppointmentRow.ScheduledDateTimeBegin;
//                                    }
//                                }
//                                if (isContained == SharedFunctions.AppIsContained.ExceedsContainment)
//                                {
//                                    timeCanceledByAppointment = true;
//                                    break;
//                                }
//                            }
//                        }

//                        //last iteration or slots without appointments
//                        if (timeCanceledByAppointment == false)
//                        {
//                            if (fsRoomRow != null && fSWrkEmployeeScheduleRow != null)
//                            {
//                                SlotDuration = SlotEnd - SlotBegin;
//                                if (fsxServiceRow.EstimatedDuration <= SlotDuration.Value.TotalMinutes)
//                                {
//                                    tempEmployeeRoomHelper = CreateInterval(fSWrkEmployeeScheduleRow, fsRoomRow, fsxServiceRow, SlotBegin, SlotEnd, recordID++, cpnyLocationDistances);
//                                    newAvailabilityRecordList.Add(tempEmployeeRoomHelper);
//                                }
//                            }
//                        }
//                        timeCanceledByAppointment = false;
//                    }                    
//                }
//            }
//            return newAvailabilityRecordList;
//        }

//        #region PrivateFunctions
//        /// <summary>
//        /// Obtains the address for the specified customer location
//        /// </summary>
//        /// <param name="customerID">ID of the Customer</param>
//        /// <param name="customerLocationID">ID of the customer location</param>
//        /// <returns>The concatenated address</returns>
//        private string GetCustomerAddress(int customerID, int customerLocationID)
//        {
//            Location customerLocation = new Location();
//            Address customerAddress = new Address();
//            string customerFullAddress;

//            customerLocation = PXSelect<Location,
//                            Where<Location.bAccountID, Equal<Required<EmployeeRoomFilter.customerID>>,
//                            And<Location.locationID, Equal<Required<EmployeeRoomFilter.customerLocationID>>>>>.
//                            Select(this, customerID, customerLocationID);

//            customerAddress = PXSelect<Address,
//                            Where<Address.bAccountID, Equal<Required<EmployeeRoomFilter.customerID>>,
//                            And<Address.addressID, Equal<Required<EmployeeRoomFilter.customerLocationID>>>>>.
//                            Select(this, customerID, customerLocation.DefAddressID);

//            customerFullAddress = GetAddress(customerAddress.AddressLine1, customerAddress.AddressLine2,
//                                            customerAddress.City, customerAddress.State, customerAddress.CountryID);

//            return customerFullAddress;
//        }

//        /// <summary>
//        /// Obtains a List with the Company Locations and their corresponding addresses
//        /// </summary>
//        /// <returns>A list of the Company Locations and their corresponding address</returns>
//        private List<SharedClasses.CpnyLocationDistance> GetCpnyLocationAddress()
//        {
//            List<SharedClasses.CpnyLocationDistance> cpnyLocationDistances = new List<SharedClasses.CpnyLocationDistance>();
//            PXResultset<FSBranchLocation> fsBranchLocationRows;
//            string cpnyLocationFullAddress;

//            fsBranchLocationRows = PXSelect<FSBranchLocation>.Select(this);

//            foreach (FSBranchLocation fsBranchLocationRow in fsBranchLocationRows)
//            {
//                cpnyLocationFullAddress = GetAddress(fsBranchLocationRow.AddressLine1, fsBranchLocationRow.AddressLine2,
//                                                fsBranchLocationRow.City, fsBranchLocationRow.State, fsBranchLocationRow.CountryID);

//                SharedClasses.CpnyLocationDistance cpnyLocationDistance = new SharedClasses.CpnyLocationDistance(fsBranchLocationRow, 
//                                                                                cpnyLocationFullAddress, 0);
//                //return cpnyLocationFullAddress;
//                cpnyLocationDistances.Add(cpnyLocationDistance);
//            }

//            return cpnyLocationDistances;

//        }

//        private string GetAddress(string addressLine1, string addressLine2, string city, string state, string countryID)
//        {
//            string completeAddress = "";
//            bool firstValue = true;

//            if (!string.IsNullOrEmpty(addressLine1))
//            {
//                completeAddress = (firstValue == true) ? addressLine1.Trim() : completeAddress + "," + addressLine1.Trim();
//                firstValue = false;
//            }

//            if (!string.IsNullOrEmpty(addressLine2))
//            {
//                completeAddress = (firstValue == true) ? addressLine2.Trim() : completeAddress + "," + addressLine2.Trim();
//                firstValue = false;
//            }

//            if (!string.IsNullOrEmpty(city))
//            {
//                completeAddress = (firstValue == true) ? city.Trim() : completeAddress + "," + city.Trim();
//                firstValue = false;
//            }

//            if (!string.IsNullOrEmpty(state))
//            {
//                completeAddress = (firstValue == true) ? state.Trim() : completeAddress + "," + state.Trim();
//                firstValue = false;
//            }

//            if (!string.IsNullOrEmpty(countryID))
//            {
//                completeAddress = (firstValue == true) ? countryID.Trim() : completeAddress + "," + countryID.Trim();
//                firstValue = false;
//            }

//            return completeAddress;
//        }

//        /// <summary>
//        /// Uses the Google Maps API to obtain the distance between 2 points
//        /// </summary>
//        /// <param name="source">Point of origin</param>
//        /// <param name="destination">Point of destination</param>
//        /// <returns>The distance between 2 points</returns>
//        private int CalculatesDistance(string source, string destination)
//        {
//            Route route = RouteDirections.GetRoute(false, new GLocation(source), new GLocation(destination));

//            return route.Distance;
//        }

//        /// <summary>
//        /// Compares 2 EmployeeRoomHelper elements.
//        /// </summary>
//        /// <param name="employeeRoomHelperRow_X"> First parameter to compare</param>
//        /// <param name="employeeRoomHelper_Y">Second parameter to compare</param>
//        /// <returns>Less than 0 if employeeRoomHelperRow_X precedes employeeRoomHelper_Y in the sort order, or greater than 0 for the inverse case</returns>
//        private static int Comparison(EmployeeRoomHelper employeeRoomHelperRow_X, EmployeeRoomHelper employeeRoomHelper_Y)
//        {
//            switch(employeeRoomHelperRow_X.EmployeeID.Value.CompareTo(employeeRoomHelper_Y.EmployeeID.Value))
//            {
//                case 0:
//                switch (employeeRoomHelperRow_X.RoomID.CompareTo(employeeRoomHelper_Y.RoomID))
//                {
//                    case 0:
//                        switch (employeeRoomHelperRow_X.DateStart.Value.CompareTo(employeeRoomHelper_Y.DateStart.Value))
//                        {
//                            case 0:
//                                switch (employeeRoomHelperRow_X.TimeStart.Value.CompareTo(employeeRoomHelper_Y.TimeStart.Value))
//                                {
//                                    case 0:
//                                        return employeeRoomHelperRow_X.Distance.Value.CompareTo(employeeRoomHelper_Y.Distance.Value);
//                                    default:
//                                        return employeeRoomHelperRow_X.TimeStart.Value.CompareTo(employeeRoomHelper_Y.TimeStart.Value);
//                                }
//                            default:
//                                return employeeRoomHelperRow_X.DateStart.Value.CompareTo(employeeRoomHelper_Y.DateStart.Value);
//                        }
//                    default:
//                        return employeeRoomHelperRow_X.RoomID.CompareTo(employeeRoomHelper_Y.RoomID);
//                }
//                default:
//                    return employeeRoomHelperRow_X.EmployeeID.Value.CompareTo(employeeRoomHelper_Y.EmployeeID.Value);
//            }
//        }

//        /// <summary>
//        /// Returns the day of the week for a given date
//        /// </summary>
//        /// <param name="date">Date</param>
//        /// <returns>Day of the week</returns>
//        private DayOfWeek DayOFTheWeek(DateTime? date) 
//        {
//            return date.Value.DayOfWeek;
//        }

//        /// <summary>
//        /// Returns a single entry for the Employee's availability 
//        /// </summary>
//        /// <param name="fSWrkEmployeeScheduleRow">Row of FSWrkEmployeeSchedule</param>
//        /// <param name="fsRoom">Row of FSRoom</param>
//        /// <param name="slotBegin">DateTime of Start of the Employee Schedule</param>
//        /// <param name="slotEnd">DateTime of End of the Employeee Schedule</param>
//        /// <param name="recordID">Uniquie identifier</param>
//        /// <param name="cpnyLocationDistances">List of SharedClasses.CpnyLocationDistance</param>
//        /// <returns>A single entry of the Employees Availability</returns>
//        private PXResult<EmployeeRoomHelper> CreateInterval(FSWrkEmployeeSchedule fSWrkEmployeeScheduleRow, FSRoom fsRoom, 
//                                                FSxService fsxServiceRow, DateTime? slotBegin, DateTime? slotEnd, int? recordID, 
//                                                List<SharedClasses.CpnyLocationDistance> cpnyLocationDistances) 
//        {
//            int decimalPosition;
//            string distanceInMiles;

//            EmployeeRoomHelper employeeRoomHelperRow = new EmployeeRoomHelper();
//            BAccount bAccountAux = PXSelectReadonly<BAccount,
//                                        Where<
//                                            BAccount.bAccountID,
//                                                Equal<Required<FSWrkEmployeeSchedule.employeeID>>>>
//                                    .Select(this, fSWrkEmployeeScheduleRow.EmployeeID);

//            employeeRoomHelperRow.BranchLocationID = fsRoom.BranchLocationID;
//            employeeRoomHelperRow.EmployeeID = fSWrkEmployeeScheduleRow.EmployeeID;            
//            employeeRoomHelperRow.EmployeeName = bAccountAux.AcctName;
//            employeeRoomHelperRow.RoomID = fsRoom.RoomID;
//            employeeRoomHelperRow.Descr = fsRoom.Descr;
//            employeeRoomHelperRow.SpecificUse = fsRoom.SpecificUse;
//            employeeRoomHelperRow.DateStart = slotBegin;
//            employeeRoomHelperRow.TimeStart = slotBegin;
//            employeeRoomHelperRow.TimeEnd = slotEnd;
//            employeeRoomHelperRow.ValidOnMonday = false;
//            employeeRoomHelperRow.ValidOnTuesday = false;
//            employeeRoomHelperRow.ValidOnWednesday = false;
//            employeeRoomHelperRow.ValidOnThursday = false;
//            employeeRoomHelperRow.ValidOnFriday = false;
//            employeeRoomHelperRow.ValidOnSaturday = false;
//            employeeRoomHelperRow.ValidOnSunday = false;
//            employeeRoomHelperRow.ServiceEstimatedDuration = fsxServiceRow.EstimatedDuration;
//            employeeRoomHelperRow.RecordID = recordID;

//            foreach (SharedClasses.CpnyLocationDistance cpnyLocationDistance in cpnyLocationDistances)
//            {
//                if (cpnyLocationDistance.fsBranchLocation.BranchLocationID == fsRoom.BranchLocationID)
//                {
//                    employeeRoomHelperRow.Distance = cpnyLocationDistance.distance;
//                    distanceInMiles = ((double)cpnyLocationDistance.distance / 1609.34).ToString();
//                    decimalPosition = distanceInMiles.IndexOf(".");
//                    employeeRoomHelperRow.DistanceInMiles = distanceInMiles.Substring(0, decimalPosition + 2) + " mi"; //a single decimal is displayed in the grid
//                    break;
//                }
//            }
//            if (employeeRoomHelperRow.Distance == 0)
//            {
//                employeeRoomHelperRow.DistanceInMiles = TX.Messages.NO_CUSTOMER_LOCATION;
//            }

//            switch (DayOFTheWeek(slotBegin)) 
//            {
//                case DayOfWeek.Monday:
//                    employeeRoomHelperRow.ValidOnMonday = true;
//                    break;
//                case DayOfWeek.Tuesday:
//                    employeeRoomHelperRow.ValidOnTuesday = true;
//                    break;
//                case DayOfWeek.Wednesday:
//                    employeeRoomHelperRow.ValidOnWednesday = true;
//                    break;
//                case DayOfWeek.Thursday:
//                    employeeRoomHelperRow.ValidOnThursday = true;
//                    break;
//                case DayOfWeek.Friday:
//                    employeeRoomHelperRow.ValidOnFriday = true;
//                    break;
//                case DayOfWeek.Saturday:
//                    employeeRoomHelperRow.ValidOnSaturday = true;
//                    break;
//                case DayOfWeek.Sunday:
//                    employeeRoomHelperRow.ValidOnSunday = true;
//                    break;
//            }

//            PXResult<EmployeeRoomHelper> employeeRoomHelperRows = new PXResult<EmployeeRoomHelper>(employeeRoomHelperRow);
//            return employeeRoomHelperRows;
//        }

//        /// <summary>
//        /// Validates licenses for the selected Employee
//        /// </summary>
//        /// <param name="serviceID">ServiceID</param>
//        /// <param name="employeeID">EmployeeID</param>
//        /// <returns>bool indicating if the license is valid</returns>
//        private bool ValidateLicenses(int? serviceID, int? employeeID)
//        {
//            List<int?> employeeLicenseTypes = new List<int?>();
//            List<int?> serviceLicenseTypes = new List<int?>();

//            var fsLicenseRows = PXSelectReadonly<FSLicense,
//                                        Where<
//                                            FSLicense.employeeID, Equal<Required<FSLicense.employeeID>>,
//                                            And<FSLicense.expirationDate, Greater<Today>>>>
//                                        .Select(this, employeeID);
//            foreach(FSLicense fsLicenseRow in fsLicenseRows)
//            {
//                employeeLicenseTypes.Add(fsLicenseRow.LicenseTypeID);
//            }

//            var fsServiceLicenseTypeRows = PXSelectReadonly<FSServiceLicenseType,
//                                                            Where<
//                                                                FSServiceLicenseType.serviceID, Equal<Required<FSServiceLicenseType.serviceID>>>>
//                                                            .Select(this, serviceID);
//            foreach (FSServiceLicenseType fsServiceLicenseTypeRow in fsServiceLicenseTypeRows)
//            {
//                serviceLicenseTypes.Add(fsServiceLicenseTypeRow.LicenseTypeID);
//            }

//            if (serviceLicenseTypes.Except(employeeLicenseTypes).Any() == true)
//            {
//                return false;
//            }

//            return true;
//        }

//        /// <summary>
//        /// Validates skills for the selected Employee
//        /// </summary>
//        /// <param name="serviceID">ServiceID</param>
//        /// <param name="employeeID">EmployeeID</param>
//        /// <returns>bool indicating if the skills are valid</returns>
//        private bool ValidateSkills(int? serviceID, int? employeeID)
//        {
//            List<int?> employeeSkills = new List<int?>();
//            List<int?> serviceSkills = new List<int?>();

//            var fsEmployeeSkillRows = PXSelectReadonly<FSEmployeeSkill,
//                                            Where<
//                                            FSEmployeeSkill.employeeID, Equal<Required<FSEmployeeSkill.employeeID>>>>
//                                            .Select(this, employeeID);

//            foreach (FSEmployeeSkill fsEmployeeSkillRow in fsEmployeeSkillRows)
//            {
//                employeeSkills.Add(fsEmployeeSkillRow.SkillID);
//            }

//            var fsServiceSkillRows = PXSelectReadonly<FSServiceSkill,
//                                                Where<
//                                                    FSServiceSkill.serviceID, Equal<Required<FSServiceSkill.serviceID>>>>
//                                                .Select(this, serviceID);
//            foreach (FSServiceSkill fsServiceSkillRow in fsServiceSkillRows)
//            {
//                serviceSkills.Add((int)fsServiceSkillRow.SkillID);
//            }
//            if (serviceSkills.Except(employeeSkills).Any() == true)
//            {
//                return false;
//            }

//            return true;
//        }
//        #endregion

//        #region Events

//        protected virtual void EmployeeRoomFilter_RowSelected(PXCache cache, PXRowSelectedEventArgs e)
//        {
//            if (e.Row == null)
//            {
//                return;
//            }

//            EmployeeRoomFilter filter = (EmployeeRoomFilter)e.Row;

//            PXUIFieldAttribute.SetEnabled<EmployeeRoomFilter.roomID>(cache, filter, (bool)filter.RoomAvailability);
//            if (filter.RoomAvailability == false)
//            {
//                filter.RoomID = null;
//            }
//        }

//        protected virtual void EmployeeRoomFilter_ToDate_FieldDefaulting(PXCache cache, PXFieldDefaultingEventArgs e)
//        {
//            if (e.Row == null)
//            {
//                return;
//            }

//            EmployeeRoomFilter filter = (EmployeeRoomFilter)e.Row;

//            filter.ToDate = Accessinfo.BusinessDate.Value.AddDays(7); // 1 week
//        }

//        protected virtual void EmployeeRoomFilter_CustomerID_FieldUpdated(PXCache cache, PXFieldUpdatedEventArgs e)
//        {
//            if (e.Row == null)
//            {
//                return;
//            }

//            EmployeeRoomFilter filter = (EmployeeRoomFilter)e.Row;

//            filter.CustomerLocationID = ServiceOrderCore.GetDefaultLocationID(this, filter.customerID);
//        }

//        protected virtual void EmployeeRoomFilter_BranchLocationID_FieldUpdated(PXCache cache, PXFieldUpdatedEventArgs e)
//        {
//            if (e.Row == null)
//            {
//                return;
//            }

//            EmployeeRoomFilter filter = (EmployeeRoomFilter)e.Row;

//            filter.RoomID = null;
//        }

//        #endregion

//    }
//}