using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Script.Serialization;
using System.Web.Script.Services;
using System.Web.Services;
using PX.Objects.FS;
using PX.Common;
using PX.Data;
using PX.Objects.CR;
using PX.Objects.CS;
using PX.Objects.EP;
using PX.Objects.IN;
using PX.Objects.AR;
using PX.Objects.AP;
using PX.Objects.GL;
using PX.FS;
using System.Globalization;

public partial class Page_FS300000 : System.Web.UI.Page
{

    protected void Page_Load(object sender, EventArgs e)
    {

    }

    #region WebMethodSetup
    /// <summary>
    /// Get the FSSetup
    /// </summary>
    /// <returns>FSSetup</returns>
    [WebMethod]
    [ScriptMethod(UseHttpGet = true, ResponseFormat = ResponseFormat.Xml)]
    public static Object GetSetups()
    {
        var graphExternalControls = PXGraph.CreateInstance<ExternalControls>();
        List<object> retList = new List<object>();
        FSSetup fsSetupRow = (FSSetup)graphExternalControls.SetupRecords.Select();

        PX.SM.UserPreferences UserPreferencesRow = (PX.SM.UserPreferences)graphExternalControls.UserPreferencesRecords.Select();

        fsSetupRow.DfltBranchID = PXAccess.GetBranchID();
        retList.Add(fsSetupRow);

        if(UserPreferencesRow != null)
        {
            FSxUserPreferences FsxUserPreferencesRow = PXCache<PX.SM.UserPreferences>.GetExtension<FSxUserPreferences>(UserPreferencesRow);
            retList.Add(FsxUserPreferencesRow);
        }

        MetaData metaData = new MetaData();
        Response response = new Response(metaData, retList, true, null);

        return SerialJson(response);
    }

    /// <summary>
    /// Get the Calendar Rules
    /// </summary>
    /// <param name="scheduledDate">
    /// Double
    /// </param>
    /// <returns>CSCalendar</returns>
    [WebMethod]
    [ScriptMethod(UseHttpGet = true, ResponseFormat = ResponseFormat.Xml)]
    public static Object GetCSCalendars(DateTime scheduledDate)
    {
        var graphExternalControls = PXGraph.CreateInstance<ExternalControls>();

        List<object> retList = new List<object>();
        DateHandler requestDate = new DateHandler(scheduledDate);
        FSSetup fsSetupRow = (FSSetup)graphExternalControls.SetupRecords.Select();

        if (fsSetupRow != null && fsSetupRow.CalendarID != null)
        {
            CSCalendar csCalendarRow = (CSCalendar)graphExternalControls.CSCalendar
                                    .Select(fsSetupRow.CalendarID);

            retList.Add(csCalendarRow);
        }

        MetaData metaData = new MetaData();
        Response response = new Response(metaData, retList, true, null);
        return SerialJson(response);
    }

    /// <summary>
    /// Get the Calendar Exceptions Rules
    /// </summary>
    /// <param name="scheduledDate">
    /// Double
    /// </param>
    /// <returns>object[{StartDate,EndDate,WorkDay}]</returns>
    [WebMethod]
    [ScriptMethod(UseHttpGet = true, ResponseFormat = ResponseFormat.Xml)]
    public static Object GetCSCalendarExceptions(DateTime scheduledDate)
    {
        var graphExternalControls = PXGraph.CreateInstance<ExternalControls>();
        List<object> retList = new List<object>();
        DateHandler requestDate = new DateHandler(scheduledDate);
        FSSetup fsSetupRow = (FSSetup)graphExternalControls.SetupRecords.Select();

        if (fsSetupRow != null && fsSetupRow.CalendarID != null)
        {
            CSCalendarExceptions csCalendarExceptionsRow = (CSCalendarExceptions)graphExternalControls.CSCalendarExceptions
                                                            .Select(fsSetupRow.CalendarID, requestDate.StartOfDay());

            if (csCalendarExceptionsRow != null)
            {
                retList.Add(
                    new
                    {
                        StartDate = requestDate.SetHours(csCalendarExceptionsRow.StartTime),
                        EndDate = requestDate.SetHours(csCalendarExceptionsRow.EndTime),
                        WorkDay = csCalendarExceptionsRow.WorkDay
                    }
                );
            }
        }

        MetaData metaData = new MetaData();
        Response response = new Response(metaData, retList, true, null);

        return SerialJson(response);
    }

    /// <summary>
    /// Get the Calendar Rules from ScheduleDateStart to ScheduleDateEnd
    /// </summary>
    /// <param name="scheduledStartDate">
    /// Double
    /// </param>
    /// <param name="scheduledEndDate">
    /// Double
    /// </param>
    /// <returns>object[{StartDate,EndDate,WorkDay}]</returns>
    [WebMethod]
    [ScriptMethod(UseHttpGet = true, ResponseFormat = ResponseFormat.Xml)]
    public static Object GetFromToCSCalendarExceptions(DateTime scheduledStartDate, DateTime scheduledEndDate)
    {
        var graphExternalControls = PXGraph.CreateInstance<ExternalControls>();
        List<object> retList = new List<object>();
        DateHandler requestDateStart = new DateHandler(scheduledStartDate);
        DateHandler requestDateEnd = new DateHandler(scheduledEndDate);

        FSSetup fsSetupRow = (FSSetup)graphExternalControls.SetupRecords.Select();

        if (fsSetupRow != null && fsSetupRow.CalendarID != null)
        {
            var csCalendarExceptionsRows = graphExternalControls.FromToCSCalendarExceptions
                                                            .Select(fsSetupRow.CalendarID, requestDateStart.StartOfDay(), requestDateEnd.BeginOfNextDay());

            foreach (CSCalendarExceptions csCalendarExceptionsRow in csCalendarExceptionsRows)
            {
                if (csCalendarExceptionsRow != null)
                {
                    retList.Add(
                        new
                        {
                            StartDate = new DateTime(csCalendarExceptionsRow.Date.Value.Year,
                                                        csCalendarExceptionsRow.Date.Value.Month,
                                                        csCalendarExceptionsRow.Date.Value.Day,
                                                        csCalendarExceptionsRow.StartTime.Value.Hour,
                                                        csCalendarExceptionsRow.StartTime.Value.Minute,
                                                        csCalendarExceptionsRow.StartTime.Value.Second),

                            EndDate = new DateTime(csCalendarExceptionsRow.Date.Value.Year,
                                                        csCalendarExceptionsRow.Date.Value.Month,
                                                        csCalendarExceptionsRow.Date.Value.Day,
                                                        csCalendarExceptionsRow.EndTime.Value.Hour,
                                                        csCalendarExceptionsRow.EndTime.Value.Minute,
                                                        csCalendarExceptionsRow.EndTime.Value.Second),

                            WorkDay = csCalendarExceptionsRow.WorkDay
                        }
                    );
                }
            }
        }

        MetaData metaData = new MetaData();

        Response response = new Response(metaData, retList, true, null);

        return SerialJson(response);
    }

    /// <summary>
    /// Returns an object with the day rules and exceptions (start and end hours)
    /// </summary>
    /// <param name="dateTimeStart">
    /// Double start date time
    /// <param name="dateTimeEnd">
    /// Double end date time
    /// <returns>List of DateTime objects</returns>
    [WebMethod]
    [ScriptMethod(UseHttpGet = true, ResponseFormat = ResponseFormat.Xml)]
    public static Object GetDayRules(DateTime scheduledStartDate, DateTime? scheduledEndDate)
    {
        var graphExternalControls = PXGraph.CreateInstance<ExternalControls>();
        List<object> retList = new List<object>();

        DateHandler requestDateStart = new DateHandler(scheduledStartDate);
        DateHandler requestDateEnd = scheduledEndDate == null ? new DateHandler(scheduledStartDate) : new DateHandler((DateTime)scheduledEndDate);

        if (requestDateStart.IsSameDate(requestDateEnd.GetDate()))
        {
            requestDateEnd.SetDate(requestDateStart.EndOfDay());
        }

        FSSetup fsSetupRow = (FSSetup)graphExternalControls.SetupRecords.Select();

        if (fsSetupRow != null && fsSetupRow.CalendarID != null)
        {
            CSCalendar csCalendarRow = (CSCalendar)graphExternalControls.CSCalendar
                                    .Select(fsSetupRow.CalendarID);

            while (requestDateStart.GetDate() < requestDateEnd.GetDate()) {

                CSCalendarExceptions csCalendarExceptionsRow = (CSCalendarExceptions)graphExternalControls.CSCalendarExceptions
                                                               .Select(fsSetupRow.CalendarID, requestDateStart.StartOfDay());

                DateTime? startDate = (DateTime?)typeof(CSCalendar).GetProperty(requestDateStart.GetDay() + "StartTime").GetValue(csCalendarRow, null);
                DateTime? endDate = (DateTime?)typeof(CSCalendar).GetProperty(requestDateStart.GetDay() + "EndTime").GetValue(csCalendarRow, null);
                bool? dayChecked = (bool?)typeof(CSCalendar).GetProperty(requestDateStart.GetDay() + "WorkDay").GetValue(csCalendarRow, null);
                bool workExceptionFlag = false;

                if (csCalendarExceptionsRow != null)
                {
                    bool? workDay = csCalendarExceptionsRow.WorkDay;
                    bool showCalendarException = false;
                    DateTime? startExDate = csCalendarExceptionsRow.StartTime;
                    DateTime? endExDate = csCalendarExceptionsRow.EndTime;

                    if (workDay == true)
                    {
                        workExceptionFlag = true;

                        if (dayChecked == true)
                        {
                            // The Exception hours block is outside the working block
                            if (endExDate < startDate || endDate < startExDate)
                            {
                                showCalendarException = true;
                            }
                            else
                            {
                                if (startExDate < startDate)
                                {
                                    // Adjust start hour
                                    startDate = startExDate;
                                }

                                if (endExDate > endDate)
                                {
                                    // Adjust end hour
                                    endDate = endExDate;
                                }
                            }
                        }
                        else
                        {
                            startDate = startExDate;
                            endDate = endExDate;
                        }
                    }
                    else if (workDay == false)
                    {
                        showCalendarException = true;
                    }

                    if (showCalendarException)
                    {
                        retList.Add(
                            new
                            {
                                StartDate = requestDateStart.SetHours(startExDate).ToString("MM/dd/yyyy h:mm:ss tt", new CultureInfo("en-US")),
                                EndDate = requestDateStart.SetHours(endExDate).ToString("MM/dd/yyyy h:mm:ss tt", new CultureInfo("en-US")),
                                Type = PX.Objects.FS.ID.Calendar_ExceptionType.UNAVAILABILITY,
                                WorkDay = csCalendarExceptionsRow.WorkDay
                            }
                        );
                    }
                }

                if (dayChecked == true || workExceptionFlag)
                {
                    retList.Add(
                        new
                        {
                            StartDate = requestDateStart.SetHours(startDate).ToString("MM/dd/yyyy h:mm:ss tt", new CultureInfo("en-US")),
                            EndDate = requestDateStart.SetHours(endDate).ToString("MM/dd/yyyy h:mm:ss tt", new CultureInfo("en-US")),
                            Type = PX.Objects.FS.ID.Calendar_ExceptionType.AVAILABILITY
                        }
                    );
                }

                requestDateStart.SetDate(requestDateStart.GetDate().AddDays(1));
            }
        }

        MetaData metaData = new MetaData();
        Response response = new Response(metaData, retList, true, null);

        return SerialJson(response);
    }
    #endregion

    #region WebMethodsRoom
    /// <summary>
    /// Get all FSRoom TODO Filtering by Branch Location
    /// </summary>
    /// <returns> objects FSRoom </returns>
    [WebMethod]
    [ScriptMethod(UseHttpGet = true, ResponseFormat = ResponseFormat.Xml)]
    public static Object GetRooms(int? branchLocationID)
    {
        List<object> retList = new List<object>();
        var graphExternalControls = PXGraph.CreateInstance<ExternalControls>();
        RequestParams request = new RequestParams(HttpContext.Current.Request);

        PXResultset<FSRoom> fsRoomRows = graphExternalControls.RoomRecordsByBranchLocation.Select(branchLocationID);

        int totalRows = fsRoomRows.Count;
        int total = (request.Limit != 0 && request.Limit < fsRoomRows.Count) ? request.Limit : fsRoomRows.Count;
        int start = (request.Start != 0) ? request.Start : 0;

        //Paging Rooms
        for (int i = 0; i < total && (start + i < fsRoomRows.Count); i++)
        {
            FSRoom fsRoomRow = fsRoomRows[start + i];
            fsRoomRow.CustomRoomID = fsRoomRow.BranchLocationID.ToString() + "-" + fsRoomRow.RoomID;
            retList.Add(fsRoomRow);
        }

        MetaData metaData = new MetaData(totalRows);
        Response response = new Response(metaData, retList, true, null);

        return SerialJson(response);
    }
    #endregion

    #region SingleWebMethods
    /// <summary>
    /// Get a list of FSSkills
    /// </summary>
    /// <returns> List<FSSkill> </returns>
    [WebMethod]
    [ScriptMethod(UseHttpGet = true, ResponseFormat = ResponseFormat.Xml)]
    public static Object GetSkills()
    {
        var graphExternalControls = PXGraph.CreateInstance<ExternalControls>();
        List<object> retList = new List<object>();
        RequestParams request = new RequestParams(HttpContext.Current.Request);
        PXResultset<FSSkill> fsSkillRows = (PXResultset<FSSkill>)graphExternalControls.SkillRecords
                                          .Select();

        foreach (FSSkill fsSkillRow in fsSkillRows)
        {
            retList.Add(fsSkillRow);
        }

        MetaData metaData = new MetaData();
        Response response = new Response(metaData, retList, true, null);

        return SerialJson(response);
    }

    /// <summary>
    /// Get a list of FSEquipments with ResourceFlag = true
    /// </summary>
    /// <returns> List<FSEquipment> </returns>
    [WebMethod]
    [ScriptMethod(UseHttpGet = true, ResponseFormat = ResponseFormat.Xml)]
    public static Object GetResources()
    {
        var graphExternalControls = PXGraph.CreateInstance<ExternalControls>();
        List<object> retList = new List<object>();
        RequestParams request = new RequestParams(HttpContext.Current.Request);
        PXResultset<FSEquipment> fsEquipmentRows = (PXResultset<FSEquipment>)graphExternalControls.Resources
                                                .Select();

        foreach (FSEquipment fsEquipmentRow in fsEquipmentRows)
        {
            retList.Add(fsEquipmentRow);
        }

        MetaData metaData = new MetaData();
        Response response = new Response(metaData, retList, true, null);

        return SerialJson(response);
    }

    /// <summary>
    /// Get a list of FSGeoZone
    /// </summary>
    /// <returns> List<FSGeoZone> </returns>
    [WebMethod]
    [ScriptMethod(UseHttpGet = true, ResponseFormat = ResponseFormat.Xml)]
    public static Object GetGeoZones()
    {
        var graphExternalControls = PXGraph.CreateInstance<ExternalControls>();
        List<object> retList = new List<object>();
        RequestParams request = new RequestParams(HttpContext.Current.Request);
        PXResultset<FSGeoZone> fsGeoZoneRows = (PXResultset<FSGeoZone>)graphExternalControls.GeoZoneRecords.Select();

        foreach (FSGeoZone fsGeoZoneRow in fsGeoZoneRows)
        {
            retList.Add(fsGeoZoneRow);
        }

        MetaData metaData = new MetaData();
        Response response = new Response(metaData, retList, true, null);

        return SerialJson(response);
    }

    /// <summary>
    /// Get a list of FSProblem
    /// </summary>
    /// <returns> List<FSProblem> </returns>
    [WebMethod]
    [ScriptMethod(UseHttpGet = true, ResponseFormat = ResponseFormat.Xml)]
    public static Object GetProblems()
    {
        var graphExternalControls = PXGraph.CreateInstance<ExternalControls>();
        List<object> retList = new List<object>();
        RequestParams request = new RequestParams(HttpContext.Current.Request);
        PXResultset<FSProblem> fsProblemRows = (PXResultset<FSProblem>)graphExternalControls.ProblemRecords.Select();

        foreach (FSProblem fsProblemRow in fsProblemRows)
        {
            retList.Add(fsProblemRow);
        }

        MetaData metaData = new MetaData();
        Response response = new Response(metaData, retList, true, null);

        return SerialJson(response);
    }

    /// <summary>
    /// Get a list of INItemClass enable in SM
    /// </summary>
    /// <returns> List<INItemClass> </returns>
    [WebMethod]
    [ScriptMethod(UseHttpGet = true, ResponseFormat = ResponseFormat.Xml)]
    public static Object GetServiceClasses()
    {
        var graphExternalControls = PXGraph.CreateInstance<ExternalControls>();
        List<object> retList = new List<object>();
        RequestParams request = new RequestParams(HttpContext.Current.Request);
        PXResultset<INItemClass> inItemClassRows = (PXResultset<INItemClass>)graphExternalControls.ServiceClasses
                                                    .Select();

        foreach (INItemClass inItemClassRow in inItemClassRows)
        {
            retList.Add(inItemClassRow);
        }

        MetaData metaData = new MetaData();
        Response response = new Response(metaData, retList, true, null);

        return SerialJson(response);
    }

    /// <summary>
    /// Get a list of FSLicenseType
    /// </summary>
    /// <returns> List<FSLicenseType> </returns>
    [WebMethod]
    [ScriptMethod(UseHttpGet = true, ResponseFormat = ResponseFormat.Xml)]
    public static Object GetLicenseTypes()
    {
        var graphExternalControls = PXGraph.CreateInstance<ExternalControls>();
        List<object> retList = new List<object>();
        RequestParams request = new RequestParams(HttpContext.Current.Request);
        PXResultset<FSLicenseType> fsLicenseTypeRows = (PXResultset<FSLicenseType>)graphExternalControls.LicenseTypeRecords.Select();

        foreach (FSLicenseType fsLicenseTypeRow in fsLicenseTypeRows)
        {
            retList.Add(fsLicenseTypeRow);
        }

        MetaData metaData = new MetaData();
        Response response = new Response(metaData, retList, true, null);

        return SerialJson(response);
    }

    /// <summary>
    /// Get a list of FSBranchLocation
    /// </summary>
    /// <returns> List<FSBranchLocation> </returns>
    [WebMethod]
    [ScriptMethod(UseHttpGet = true, ResponseFormat = ResponseFormat.Xml)]
    public static Object GetBranchLocations(int? branchID)
    {
        var graphExternalControls = PXGraph.CreateInstance<ExternalControls>();
        branchID = branchID == -1 ? null : branchID;

        List<object> retList = new List<object>();
        RequestParams request = new RequestParams(HttpContext.Current.Request);
        PXResultset<FSBranchLocation> fsBranchLocationRows = (PXResultset<FSBranchLocation>)graphExternalControls.BranchLocationRecordsByBranch.Select(branchID, branchID);

        int totalRows = fsBranchLocationRows.Count;
        int total = (request.Limit > 0 && request.Limit < totalRows) ? request.Limit : totalRows;
        int start = (request.Start > 0) ? request.Start : 0;

        //Paging Branch Locations
        for (int i = 0; i < total && (start + i < totalRows); i++)
        {
            FSBranchLocation fsBranchLocationRow = fsBranchLocationRows[start + i];

            retList.Add(fsBranchLocationRow);
        }

        MetaData metaData = new MetaData(totalRows);
        Response response = new Response(metaData, retList, true, null);

        return SerialJson(response);
    }

    /// <summary>
    /// Get a list of Branches
    /// </summary>
    /// <returns>object[{BranchID,BranchCD,Deleted}]</returns>
    [WebMethod]
    [ScriptMethod(UseHttpGet = true, ResponseFormat = ResponseFormat.Xml)]
    public static Object GetBranches()
    {
        var graphExternalControls = PXGraph.CreateInstance<ExternalControls>();
        List<object> retList = new List<object>();
        RequestParams request = new RequestParams(HttpContext.Current.Request);

        var branches = PXSelect<Branch, Where<Branch.active, Equal<True>>>.Select(graphExternalControls).RowCast<Branch>().ToList();

        int totalRows = branches.Count;
        int total = (request.Limit != 0 && request.Limit < totalRows) ? request.Limit : totalRows;
        int start = (request.Start != 0) ? request.Start : 0;

        //Paging Branches
        for (int i = 0; i < total && (start + i < totalRows); i++)
        {
            var branch = branches[start + i];

            retList.Add(new
            {
                BranchID = branch.BranchID,
                BranchCD = branch.BranchCD,
                Name = branch.AcctName,
            });
        }

        MetaData metaData = new MetaData(totalRows);
        Response response = new Response(metaData, retList, true, null);

        return SerialJson(response);
    }

    /// <summary>
    /// Get a list of SrvOrdType
    /// </summary>
    /// <returns>List<FSSrvOrdType></returns>
    [WebMethod]
    [ScriptMethod(UseHttpGet = true, ResponseFormat = ResponseFormat.Xml)]
    public static Object GetSrvOrdTypes()
    {
        var graphExternalControls = PXGraph.CreateInstance<ExternalControls>();
        List<object> retList = new List<object>();
        RequestParams request = new RequestParams(HttpContext.Current.Request);
        PXResultset<FSSrvOrdType> fsSrvOrdTypeRows = (PXResultset<FSSrvOrdType>)graphExternalControls.SrvOrdTypeRecords.Select();

        int totalRows = fsSrvOrdTypeRows.Count;
        int total = (request.Limit > 0 && request.Limit < totalRows) ? request.Limit : totalRows;
        int start = (request.Start > 0) ? request.Start : 0;

        //Paging FSSrvOrdType
        for (int i = 0; i < total && (start + i < totalRows); i++)
        {
            FSSrvOrdType fsSrvOrdTypeRow = fsSrvOrdTypeRows[start + i];

            retList.Add(fsSrvOrdTypeRow);
        }

        MetaData metaData = new MetaData(totalRows);
        Response response = new Response(metaData, retList, true, null);

        return SerialJson(response);
    }

    /// <summary>
    /// Get a list of InventoryItem enable in SM
    /// </summary>
    /// <returns> List<InventoryItem> </returns>
    [WebMethod]
    [ScriptMethod(UseHttpGet = true, ResponseFormat = ResponseFormat.Xml)]
    public static Object GetServices()
    {
        var graphExternalControls = PXGraph.CreateInstance<ExternalControls>();
        List<object> retList = new List<object>();
        RequestParams request = new RequestParams(HttpContext.Current.Request);
        PXResultset<InventoryItem> inventoryItemRows = (PXResultset<InventoryItem>)graphExternalControls.Services
                                                        .Select();

        foreach (InventoryItem inventoryItemRow in inventoryItemRows)
        {
            retList.Add(inventoryItemRow);
        }

        MetaData metaData = new MetaData();
        Response response = new Response(metaData, retList, true, null);

        return SerialJson(response);
    }

    /// <summary>
    /// Get the Business Date
    /// </summary>
    /// <returns> List<FSLicenseType> </returns>
    [WebMethod]
    [ScriptMethod(UseHttpGet = true, ResponseFormat = ResponseFormat.Xml)]
    public static Object GetBusinessDate()
    {
        var graphExternalControls = PXGraph.CreateInstance<ExternalControls>();
        List<object> retList = new List<object>();
        RequestParams request = new RequestParams(HttpContext.Current.Request);

        var auxObject = new { Date = "", CustomDate = graphExternalControls.GetBusinessDate(), Cls = "sch-todayLine", Text = "" };
        retList.Add(auxObject);

        MetaData metaData = new MetaData();
        Response response = new Response(metaData, retList, true, null);

        return SerialJson(response);
    }
    #endregion

    #region WebMethodsEmployee
    /// <summary>
    /// Get the active employees in SM
    /// </summary>
    /// <returns> List<Object> {{EmployeeID={..},Contact={..}},...}</returns>
    [WebMethod]
    [ScriptMethod(UseHttpGet = true, ResponseFormat = ResponseFormat.Xml)]
    public static Object GetEmployees(int? branchID, int? branchLocationID, bool? ignoreActiveFlag, bool? ignoreAvailabilityFlag, DateTime? scheduledStartDate, DateTime? scheduledEndDate, bool? loadVendor)
    {
        var graphExternalControls = PXGraph.CreateInstance<ExternalControls>();
        RequestParams request = new RequestParams(HttpContext.Current.Request);
        List<object> retList = new List<object>();
        List<object> employeeList = new List<object>();

        DateHandler requestDateStart = new DateHandler(scheduledStartDate);
        DateHandler requestDateEnd = scheduledEndDate == null ? new DateHandler(scheduledStartDate) : new DateHandler((DateTime)scheduledEndDate);
        DateTime timeBegin = requestDateStart.StartOfDay();
        DateTime timeEnd = requestDateEnd.BeginOfNextDay();

        List<PXResult<EPEmployee>> epEmployeeRows = graphExternalControls.EmployeeRecords(branchID, branchLocationID, ignoreActiveFlag, ignoreAvailabilityFlag, timeBegin, timeEnd, request.Filters);
        PXResultset<Vendor> vendorRows = graphExternalControls.VendorRecords(branchID, branchLocationID, ignoreActiveFlag, ignoreAvailabilityFlag, timeBegin, timeEnd, request.Filters);

        int totalRows = epEmployeeRows.Count;

        if (loadVendor == true)
        {
            totalRows = totalRows + vendorRows.Count;
        }

        int total = (request.Limit != 0 && request.Limit < totalRows) ? request.Limit : totalRows;
        int start = (request.Start != 0) ? request.Start : 0;

        //Paging Service Order
        for (int i = 0; i < total && (start + i < totalRows); i++)
        {

            if(start + i < epEmployeeRows.Count || loadVendor == null)
            {
                EPEmployee epEmployeeRow = epEmployeeRows[start + i];

                Contact contactRow = (Contact)graphExternalControls.EmployeeContact.Select(epEmployeeRow.ParentBAccountID, epEmployeeRow.DefContactID);

                var auxObject = new { EmployeeID = epEmployeeRow.BAccountID, EmployeeCD = epEmployeeRow.AcctCD, Contact = contactRow, IsVendor = false };
                retList.Add(auxObject);
            }
            else
            {
                Vendor vendorRow = vendorRows[start + i - epEmployeeRows.Count];

                Contact contactRow = (Contact)graphExternalControls.VendorContact.Select(vendorRow.DefContactID);

                contactRow.DisplayName = contactRow.FullName;

                var auxObject = new { EmployeeID = vendorRow.BAccountID, EmployeeCD = vendorRow.AcctCD, Contact = contactRow, IsVendor = true };

                retList.Add(auxObject);
            }
        }

        MetaData metaData = new MetaData(totalRows);
        Response response = new Response(metaData, retList, true, null);

        return SerialJson(response);
    }

    /// <summary>
    /// Get a list of FSWrkEmployeeSchedule
    /// </summary>
    /// <returns> List<FSWrkEmployeeSchedule> </returns>
    [WebMethod]
    [ScriptMethod(UseHttpGet = true, ResponseFormat = ResponseFormat.Xml)]
    public static Object GetEmployeeWorkingSchedule(DateTime scheduledStartDate, DateTime? scheduledEndDate, bool compressSlot, int[] employeeIDList, int? branchID, int? branchLocationID)
    {
        var graphExternalControls = PXGraph.CreateInstance<ExternalControls>();
        List<object> retList = new List<object>();
        RequestParams request = new RequestParams(HttpContext.Current.Request);

        DateHandler requestDateStart = new DateHandler(scheduledStartDate);
        DateHandler requestDateEnd = scheduledEndDate == null ? new DateHandler(scheduledStartDate) : new DateHandler((DateTime)scheduledEndDate);
        DateTime timeBegin = requestDateStart.StartOfDay();
        DateTime timeEnd = requestDateEnd.BeginOfNextDay();

        retList = graphExternalControls.GetWorkingScheduleRecords(timeBegin, timeEnd, branchID, branchLocationID, compressSlot, employeeIDList).Cast<Object>().ToList();

        MetaData metaData = new MetaData();
        Response response = new Response(metaData, retList, true, null);

        return SerialJson(response);
    }

    /// <summary>
    /// PutEmployeeWorkingSchedule receives a FSWrkEmployeeSchedule that has to be modify on the database.
    /// </summary>
    /// <returns>Returns a Response object to see if succeded or failed.</returns>
    [WebMethod]
    [ScriptMethod(ResponseFormat = ResponseFormat.Xml)]
    public static Object PutEmployeeWorkingSchedule(FSTimeSlot availability)
    {
        var graphExternalControls = PXGraph.CreateInstance<ExternalControls>();

        ExternalControls.DispatchBoardAppointmentMessages messages = graphExternalControls.DBPutAvailability(availability);

        bool success = (messages != null && messages.ErrorMessages.Count == 0) ? true : false;
        MetaData metaData = new MetaData();
        Response response = new Response(metaData, null, success, messages);

        return SerialJson(response);
    }

    /// <summary>
    /// DeleteEmployeeWorkingSchedule receives a FSWrkEmployeeSchedule that has to be delete on the database.
    /// </summary>
    /// <returns>Returns a Response object to see if succeded or failed.</returns>
    [WebMethod]
    [ScriptMethod(ResponseFormat = ResponseFormat.Xml)]
    public static Object DeleteEmployeeWorkingSchedule(FSTimeSlot availability)
    {
        var graphExternalControls = PXGraph.CreateInstance<ExternalControls>();

        ExternalControls.DispatchBoardAppointmentMessages messages = graphExternalControls.DBDeleteAvailability(availability);

        bool success = (messages != null && messages.ErrorMessages.Count == 0) ? true : false;
        MetaData metaData = new MetaData();
        Response response = new Response(metaData, null, success, messages);

        return SerialJson(response);
    }

    #endregion

    #region WebMethodsAppointment
    /// <summary>
    /// Get a list of Appointments filtering by scheduledDate, employeeIDList and branchID
    /// </summary>
    /// <returns> List<FSAppointment> </returns>
    [WebMethod]
    [ScriptMethod(UseHttpGet = true, ResponseFormat = ResponseFormat.Xml)]
    public static Object GetDispatchBoardAppointments(DateTime scheduledStartDate, DateTime? scheduledEndDate, int[] employeeIDList, int? branchID, int? branchLocationID)
    {
        var graphExternalControls = PXGraph.CreateInstance<ExternalControls>();

        DateHandler requestDateStart = new DateHandler(scheduledStartDate);
        DateHandler requestDateEnd = scheduledEndDate == null ? new DateHandler(scheduledStartDate) : new DateHandler((DateTime)scheduledEndDate);
        DateTime timeBegin = requestDateStart.StartOfDay();
        DateTime timeEnd = requestDateEnd.BeginOfNextDay();

        List<object> retList = new List<object>();

        retList = graphExternalControls.GetAppointmentRecords(timeBegin, timeEnd, branchID, branchLocationID, employeeIDList).Cast<Object>().ToList();

        MetaData metaData = new MetaData();

        Response response = new Response(metaData, retList, true, null);
        return SerialJson(response);
    }

    /// <summary>
    /// Get a list of Appointments by rooms
    /// </summary>
    /// <returns> List<FSAppointment> </returns>
    [WebMethod]
    [ScriptMethod(UseHttpGet = true, ResponseFormat = ResponseFormat.Xml)]
    public static Object GetRoomAppointments(DateTime scheduledStartDate, DateTime? scheduledEndDate, int[] roomIDList, int? branchID, int? branchLocationID)
    {
        var graphExternalControls = PXGraph.CreateInstance<ExternalControls>();

        DateHandler requestDateStart = new DateHandler(scheduledStartDate);
        DateHandler requestDateEnd = scheduledEndDate == null ? new DateHandler(scheduledStartDate) : new DateHandler((DateTime)scheduledEndDate);
        DateTime timeBegin = requestDateStart.StartOfDay();
        DateTime timeEnd = requestDateEnd.BeginOfNextDay();

        List<object> retList = new List<object>();
        retList = graphExternalControls.GetAppointmentRecordsByRooms(timeBegin, timeEnd, branchID, branchLocationID, roomIDList).Cast<Object>().ToList();

        MetaData metaData = new MetaData();

        Response response = new Response(metaData, retList, true, null);
        return SerialJson(response);
    }

    /// <summary>
    /// Get a list of Appointments by employee
    /// </summary>
    /// <returns> List<FSAppointment> </returns>
    [WebMethod]
    [ScriptMethod(UseHttpGet = true, ResponseFormat = ResponseFormat.Xml)]
    public static Object GetUserBoardAppointments(DateTime scheduledStartDate, DateTime? scheduledEndDate, int? employeeID, int? branchID, int? branchLocationID)
    {
        var graphExternalControls = PXGraph.CreateInstance<ExternalControls>();

        DateHandler requestDateStart = new DateHandler(scheduledStartDate);
        DateHandler requestDateEnd = scheduledEndDate == null ? new DateHandler(scheduledStartDate) : new DateHandler((DateTime)scheduledEndDate);
        DateTime timeBegin = requestDateStart.StartOfDay();
        DateTime timeEnd = requestDateEnd.BeginOfNextDay();

        List<object> retList = new List<object>();

        List<FSAppointmentStaffScheduleBoard> fsAppointmentRows = graphExternalControls.GetAppointmentRecords(timeBegin, timeEnd, branchID, branchLocationID, new int[1] { (int)employeeID });

        foreach (FSAppointmentStaffScheduleBoard fsAppointmentRow in fsAppointmentRows)
        {

            fsAppointmentRow.AppointmentCustomID = fsAppointmentRow.AppointmentID.ToString() + "-0";
            fsAppointmentRow.CustomID = fsAppointmentRow.ScheduledDateTimeBegin.Value.Month.ToString("00") +
                                        fsAppointmentRow.ScheduledDateTimeBegin.Value.Day.ToString("00") +
                                        fsAppointmentRow.ScheduledDateTimeBegin.Value.Year.ToString("0000");

            retList.Add(fsAppointmentRow.Clone());

            if (fsAppointmentRow.ScheduledDateTimeEnd.Value.Day != fsAppointmentRow.ScheduledDateTimeBegin.Value.Day)
            {
                fsAppointmentRow.AppointmentCustomID = fsAppointmentRow.AppointmentID.ToString() + "-1";
                fsAppointmentRow.CustomID = fsAppointmentRow.ScheduledDateTimeEnd.Value.Month.ToString("00") +
                                            fsAppointmentRow.ScheduledDateTimeEnd.Value.Day.ToString("00") +
                                            fsAppointmentRow.ScheduledDateTimeEnd.Value.Year.ToString("0000");

                retList.Add(fsAppointmentRow.Clone());
            }
        }

        MetaData metaData = new MetaData();

        Response response = new Response(metaData, retList, true, null);

        return SerialJson(response);
    }

    /// <summary>
    /// Get an Appointments information
    /// </summary>
    /// <returns> List<Object> {employees = List<EPEmployee>, resources = List<FSEquipment>, srvOrdType = FSSrvOrdType, services = List<InventoryItem>, serviceOrder = FSSrvOrder, branchLocationDesc = String}  </returns>[WebMethod]
    [WebMethod]
    [ScriptMethod(UseHttpGet = true, ResponseFormat = ResponseFormat.Xml)]
    public static Object GetAppointmentToolTip(int appointmentID)
    {
        var graphExternalControls = PXGraph.CreateInstance<ExternalControls>();

        FSAppointment fsAppointmentRow = graphExternalControls.AppointmentRecord.Select(appointmentID);
        var fsAppointmentEmployeeRows = graphExternalControls.AppointmentBAccountStaffMember.Select(appointmentID);
        var fsEquipmentRows = graphExternalControls.AppointmentResources.Select(appointmentID);

        PXResultset<InventoryItem> inventoryItemRows = graphExternalControls.AppointmentServices.Select(appointmentID);

        var fsSrvOrdTypeRow = (FSSrvOrdType)graphExternalControls.SrvOrdTypeRecord.Select(fsAppointmentRow.SrvOrdType);
        var fsServiceOrderRow = (FSServiceOrder)graphExternalControls.ServiceOrderByAppointment
                                    .Select(appointmentID);

        List<object> employeeList = new List<object>();
        List<object> resourceList = new List<object>();
        List<object> serviceList = new List<object>();

        foreach (BAccountStaffMember fsAppointmentEmployeeRow in fsAppointmentEmployeeRows)
        {
            employeeList.Add(fsAppointmentEmployeeRow);
        }

        foreach (FSEquipment fsEquipmentRow in fsEquipmentRows)
        {
            resourceList.Add(fsEquipmentRow);
        }

        foreach (PXResult<InventoryItem, FSSODet, FSAppointmentDet, INItemClass> inventoryItemRow in inventoryItemRows)
        {
            var itemClassRow = (INItemClass)inventoryItemRow;
            var inventoryItem = (InventoryItem)inventoryItemRow;
            var fsAppointmentDet = (FSAppointmentDet)inventoryItemRow;
            var estimatedDuration = durationFormat((double)fsAppointmentDet.EstimatedDuration, (int)SharedFunctions.TimeFormat.Minutes);
            serviceList.Add(new { service = inventoryItem, estimatedDuration = estimatedDuration, itemClass = itemClassRow });
        }

        FSContact fsContactRow = (FSContact)graphExternalControls.ServiceOrderContact
                            .Select(fsServiceOrderRow.ServiceOrderContactID);

        if (fsContactRow != null)
        {
            fsServiceOrderRow.ContactEmail = fsContactRow.Email;
            fsServiceOrderRow.ContactPhone = fsContactRow.Phone1;
            fsServiceOrderRow.ContactName = fsContactRow.DisplayName;
        }

        FSBranchLocation fsBranchLocationRow = graphExternalControls.BranchLocationRecord.Select(fsServiceOrderRow.BranchLocationID);


        List<object> retList = new List<object>();

        retList.Add(new
        {
            employees = employeeList,
            resources = resourceList,
            srvOrdType = fsSrvOrdTypeRow,
            services = serviceList,
            serviceOrder = fsServiceOrderRow,
            branchLocationDesc = fsBranchLocationRow.Descr
        });

        MetaData metaData = new MetaData();

        Response response = new Response(metaData, retList, true, null);

        return SerialJson(response);
    }

    /// <summary>
    /// Get Appointment services list.
    /// </summary>
    /// <param name="appointmentID">Appointment ID (int)</param>
    /// <returns>A list of services (InventoryItem).</returns>
    [WebMethod]
    [ScriptMethod(UseHttpGet = true, ResponseFormat = ResponseFormat.Xml)]
    public static Object GetAppointmentServices(int appointmentID)
    {
        var graphExternalControls = PXGraph.CreateInstance<ExternalControls>();

        PXResultset<InventoryItem> inventoryItemRows = graphExternalControls.AppointmentServices.Select(appointmentID);
        List<object> servicesList = new List<object>();

        foreach (PXResult<InventoryItem> inventoryItemRow in inventoryItemRows)
        {
            var inventoryItem = (InventoryItem)inventoryItemRow;
            servicesList.Add(inventoryItem);
        }

        MetaData metaData = new MetaData();
        Response response = new Response(metaData, servicesList, true, null);

        return SerialJson(response);
    }

    /// <summary>
    /// Get an Appointments Resources
    /// </summary>
    /// <returns>List<Object> { employees = List<EPEmployee>, resources = List<FSEquipment>, services = List<InventoryItem>}</returns>
    [WebMethod]
    [ScriptMethod(UseHttpGet = true, ResponseFormat = ResponseFormat.Xml)]
    public static Object GetAppointmentResources(int appointmentID, int SOID)
    {
        var graphExternalControls = PXGraph.CreateInstance<ExternalControls>();
        var epEmployeeRows = graphExternalControls.AppointmentEPEmployees.Select(appointmentID);
        var fsEquipmentRows = graphExternalControls.AppointmentResources.Select(appointmentID);

        PXResultset<InventoryItem> inventoryItemRows = graphExternalControls.AppointmentServices.Select(appointmentID);

        PXResultset<FSSODet> fsSODetRows = graphExternalControls.SODetServices.Select(SOID);

        List<object> employeeList = new List<object>();
        List<object> resourceList = new List<object>();
        List<object> serviceList = new List<object>();

        foreach (EPEmployee epEmployeeRow in epEmployeeRows)
        {
            employeeList.Add(epEmployeeRow);
        }

        foreach (FSEquipment fsEquipmentRow in fsEquipmentRows)
        {
            resourceList.Add(fsEquipmentRow);
        }

        foreach (PXResult<InventoryItem, FSSODet, FSAppointmentDet> inventoryItemRow in inventoryItemRows)
        {
            var inventoryItem = (InventoryItem)inventoryItemRow;
            var fsSoDetRow = (FSSODet)inventoryItemRow;

            var fsAppointmentDet = (FSAppointmentDet)inventoryItemRow;
            var estimatedDuration = durationFormat((double)fsAppointmentDet.EstimatedDuration, (int)SharedFunctions.TimeFormat.Minutes);
            serviceList.Add(new { service = inventoryItem, estimatedDuration = estimatedDuration, sODetID = fsSoDetRow.SODetID, unassignedFlag = false });
        }
        foreach (FSSODet fsSODetRow in fsSODetRows)
        {
            FSAppointmentDet fsAppointmentDetRow = graphExternalControls.AppointmentDets.Select(fsSODetRow.SODetID);

            if (fsAppointmentDetRow == null)
            {
                InventoryItem soInventoryItemRow = (PXResult<InventoryItem>)graphExternalControls.InventoryItemBySODet
                                                        .Select(fsSODetRow.SODetID);

                var estimatedDuration = durationFormat((double)fsAppointmentDetRow.EstimatedDuration, (int)SharedFunctions.TimeFormat.Minutes);

                serviceList.Add(new { service = soInventoryItemRow, estimatedDuration = estimatedDuration, unassignedFlag = true, sODetID = fsSODetRow.SODetID });
            }
        }


        List<object> retList = new List<object>();

        retList.Add(new
        {
            employees = employeeList,
            resources = resourceList,
            services = serviceList
        });

        MetaData metaData = new MetaData();

        Response response = new Response(metaData, retList, true, null);

        return SerialJson(response);
    }

    /// <summary>
    /// Receives a FSAppointment that has to be modify on the database
    /// </summary>
    /// <param name="appointment">
    /// FSAppointment
    /// </param>
    /// <returns>Returns a Response object to see if succeded or failed.</returns>
    [WebMethod]
    [ScriptMethod(ResponseFormat = ResponseFormat.Xml)]
    public static Object PutAppointments(FSAppointmentScheduleBoard appointment)
    {
        var graphExternalControls = PXGraph.CreateInstance<ExternalControls>();

        List<object> retList = new List<object>();

        ExternalControls.DispatchBoardAppointmentMessages messages;

        bool isAppointment = false;
        int? entityID = null;
        bool success = false;

        if (appointment.OpenAppointmentScreenOnError != true)
        {
            entityID = graphExternalControls.DBPutAppointments(appointment, out isAppointment, out messages);

            success = (messages != null && messages.ErrorMessages.Count == 0) ? true : false;
        }
        else
        {
            entityID = graphExternalControls.DBUnassignAppointmentBridge(appointment, out isAppointment, out messages);

            success = isAppointment;
        }

        if (isAppointment == false)
        {
            retList.Add(new { ProcessID = entityID });
        }
        else
        {
            retList.Add(new { AppointmentID = entityID });
        }

        MetaData metaData = new MetaData();

        Response response = new Response(metaData, retList, success, messages);

        return SerialJson(response);
    }

    /// <summary>
    /// Receives a FSAppointment that has to be deleted on the database
    /// </summary>
    /// <param name="appointment">
    /// FSAppointment
    /// </param>
    /// <returns>Returns a Response object to see if succeded or failed.</returns>
    [WebMethod]
    [ScriptMethod(ResponseFormat = ResponseFormat.Xml)]
    public static Object DeleteAppointments(FSAppointment appointment)
    {
        var graphExternalControls = PXGraph.CreateInstance<ExternalControls>();

        ExternalControls.DispatchBoardAppointmentMessages messages = graphExternalControls.DBDeleteAppointments(appointment);

        bool success = (messages != null && messages.ErrorMessages.Count == 0) ? true : false;

        MetaData metaData = new MetaData();

        Response response = new Response(metaData, null, success, messages);

        return SerialJson(response);
    }
    #endregion

    #region WebMethodsServiceOrder
    /// <summary>
    /// Get a list of FSServiceOrder
    /// </summary>
    /// <returns> List<FSServiceOrder> </returns>
    [WebMethod]
    [ScriptMethod(UseHttpGet = true, ResponseFormat = ResponseFormat.Xml)]
    public static Object GetServiceOrders(int branchID, int? branchLocationID, DateTime? scheduledStartDate = null, DateTime? scheduledEndDate = null, bool? isRoomCalendar = false)
    {
        var graphExternalControls = PXGraph.CreateInstance<ExternalControls>();
        RequestParams request = new RequestParams(HttpContext.Current.Request);
        List<object> retList = new List<object>();

        PXResultset<FSServiceOrder> fsServiceOrderRows = ExternalControls.ServiceOrderRecords(branchID, branchLocationID, request.Filters, scheduledStartDate, scheduledEndDate, isRoomCalendar);

        foreach (FSServiceOrder fsServiceOrderRow in fsServiceOrderRows)
        {
            PXResultset<FSSODet> fsSODetRows = graphExternalControls.SODetServices.Select(fsServiceOrderRow.SOID);
            int soDetCount = fsSODetRows.Count;
            int appointmentDetCount = 0;
            List<int?> serviceClassesList = new List<int?>();
            bool rowAppDetFound = false;
            foreach (FSSODet fsSODetRow in fsSODetRows)
            {
                FSAppointmentDet fsAppointmentDetRow = graphExternalControls.AppointmentDets.Select(fsSODetRow.SODetID);

                if (fsAppointmentDetRow == null)
                {
                    serviceClassesList.Add(fsSODetRow.InventoryID);
                    if (!rowAppDetFound)
                    {
                        Customer customerRow = (Customer)graphExternalControls.Customer
                                             .Select(fsServiceOrderRow.CustomerID);

                        FSContact fsContactRow = (FSContact)graphExternalControls.ServiceOrderContact
                                            .Select(fsServiceOrderRow.ServiceOrderContactID);

                        BAccount assignedEmployee = (BAccount)graphExternalControls.EmployeeBAccount
                                                        .Select(fsServiceOrderRow.AssignedEmpID);

                        fsServiceOrderRow.CustomerDisplayName = (customerRow != null) ? customerRow.AcctName : "";
                        fsServiceOrderRow.AssignedEmployeeDisplayName = (assignedEmployee != null) ? assignedEmployee.AcctName : "";

                        if (fsContactRow != null)
                        {
                            fsServiceOrderRow.ContactName = fsContactRow.DisplayName;
                            fsServiceOrderRow.ContactEmail = fsContactRow.Email;
                            fsServiceOrderRow.ContactPhone = fsContactRow.Phone1;
                        }

                        fsServiceOrderRow.SLARemaning = " ";

                        if (fsServiceOrderRow.SLAETA != null)
                        {
                            var date = PXContext.GetBusinessDate();
                            var startDate = (date != null) ? date : PXTimeZoneInfo.Now;
                            TimeSpan diffTime = (TimeSpan)(fsServiceOrderRow.SLAETA - startDate);


                            fsServiceOrderRow.SLARemaning =
                                (diffTime.Days > 0 || diffTime.Hours > 0 || diffTime.Minutes > 0) ?
                                    diffTime.Days + "d " + diffTime.Hours + "h " + diffTime.Minutes + "m " :
                                    "Time Exceeded";
                        }
                        rowAppDetFound = true;
                    }
                }
                else
                {
                    appointmentDetCount++;
                }
            }
            if (soDetCount > appointmentDetCount
                    && (fsServiceOrderRow.Status == PX.Objects.FS.ID.Status_ServiceOrder.OPEN))//PreguntarDiego
            {
                fsServiceOrderRow.ServicesRemaning = (soDetCount - appointmentDetCount);
                fsServiceOrderRow.ServiceClassIDs = serviceClassesList.Distinct().ToArray();
                FSBranchLocation fsBranchLocationRow = graphExternalControls.BranchLocationRecord.Select(fsServiceOrderRow.BranchLocationID);
                fsServiceOrderRow.BranchLocationDesc = fsBranchLocationRow.Descr;
                retList.Add(fsServiceOrderRow);
            }
        }

        MetaData metaData = new MetaData();

        Response response = new Response(metaData, retList, true, null);

        return SerialJson(response);
    }

    /// <summary>
    /// Get a list of services in a service order
    /// </summary>
    /// <returns>List<object> {FSxService,unassignedFlag,sODetID,estimatedDuration}</returns>
    [WebMethod]
    [ScriptMethod(UseHttpGet = true, ResponseFormat = ResponseFormat.Xml)]
    public static Object GetServiceOrderServices(int sOID, bool serviceOrderUnassignedFlag)
    {
        var graphExternalControls = PXGraph.CreateInstance<ExternalControls>();
        List<object> retList = new List<object>();
        var fsSODetRows = graphExternalControls.SODetServices.Select(sOID);

        foreach (FSSODet fsSODetRow in fsSODetRows)
        {
            InventoryItem inventoryItemRow = (InventoryItem)graphExternalControls.InventoryItem
                                        .Select(fsSODetRow.InventoryID);

            if (inventoryItemRow != null)
            {
                var estimatedDuration = durationFormat((double)fsSODetRow.EstimatedDuration, (int)SharedFunctions.TimeFormat.Minutes);
                var auxObject = new { service = inventoryItemRow, unassignedFlag = true, sODetID = fsSODetRow.SODetID, estimatedDuration = estimatedDuration };

                retList.Add(auxObject);
            }
        }

        MetaData metaData = new MetaData();
        Response response = new Response(metaData, retList, true, null);

        return SerialJson(response);
    }
    #endregion

    #region WebMethodsTreeServiceOrder

    /// <summary>
    /// Get a list of Service Order with their Services or list of Services of the a Service Order.
    /// </summary>
    /// <param name="node">
    /// int - node request (0 - All Service Order)
    /// </param>
    /// <returns>List with a specific structure for Service Order Tree Grid</returns>
    [WebMethod]
    [ScriptMethod(UseHttpGet = true, ResponseFormat = ResponseFormat.Xml)]
    public static Object GetServiceOrdersTree(int? branchID, int? node, int? branchLocationID, DateTime? scheduledStartDate = null, DateTime? scheduledEndDate = null, bool? isRoomCalendar = false)
    {
        var graphExternalControls = PXGraph.CreateInstance<ExternalControls>();
        RequestParams request = new RequestParams(HttpContext.Current.Request);
        List<object> retList = new List<object>();
        MetaData metaData;

        //Search All Service Order
        if (node == 0)
        {
            PXResultset<FSServiceOrder> fsServiceOrderRows = ExternalControls.ServiceOrderRecords(branchID, branchLocationID, request.Filters, scheduledStartDate, scheduledEndDate, isRoomCalendar);

            int totalRows = fsServiceOrderRows.Count;
            int total = (request.Limit != 0 && request.Limit < fsServiceOrderRows.Count) ? request.Limit : fsServiceOrderRows.Count;
            int start = (request.Start != 0) ? request.Start : 0;
            List<object> tmpSODet;
            List<int?> serviceClassesList = new List<int?>();

            //Paging Service Order
            for (int i = 0; i < total && (start + i < fsServiceOrderRows.Count); i++)
            {
                FSServiceOrder fsServiceOrderRow = fsServiceOrderRows[start + i];
                PXResultset<FSSODet> fsSODetRows = graphExternalControls.SODetPendingLines.Select(fsServiceOrderRow.SOID);
                Customer customerRow = (Customer)graphExternalControls.Customer
                                             .Select(fsServiceOrderRow.CustomerID);

                FSContact fsContactRow = (FSContact)graphExternalControls.ServiceOrderContact
                                    .Select(fsServiceOrderRow.ServiceOrderContactID);

                BAccount assignedEmployee = (BAccount)graphExternalControls.EmployeeBAccount
                                    .Select(fsServiceOrderRow.AssignedEmpID);

                FSBranchLocation fsBranchLocationRow = graphExternalControls.BranchLocationRecord.Select(fsServiceOrderRow.BranchLocationID);

                fsServiceOrderRow.BranchLocationDesc = fsBranchLocationRow.Descr;
                fsServiceOrderRow.CustomerDisplayName = (customerRow != null) ? customerRow.AcctName : "";
                fsServiceOrderRow.AssignedEmployeeDisplayName = (assignedEmployee != null) ? assignedEmployee.AcctName : "";

                if (fsContactRow != null)
                {
                    fsServiceOrderRow.ContactName = fsContactRow.DisplayName;
                    fsServiceOrderRow.ContactEmail = fsContactRow.Email;
                    fsServiceOrderRow.ContactPhone = fsContactRow.Phone1;
                }

                fsServiceOrderRow.SLARemaning = "";

                if (fsServiceOrderRow.SLAETA != null)
                {
                    var date = PXContext.GetBusinessDate();
                    var startDate = (date != null) ? date : PXTimeZoneInfo.Now;
                    TimeSpan diffTime = (TimeSpan)(fsServiceOrderRow.SLAETA - startDate);

                    fsServiceOrderRow.SLARemaning =
                        (diffTime.Days > 0 || diffTime.Hours > 0 || diffTime.Minutes > 0) ?
                            diffTime.Days + "d " + diffTime.Hours + "h " + diffTime.Minutes + "m " :
                            "Time Exceeded";
                }

                tmpSODet = new List<object>();

                fsServiceOrderRow.ServicesCount = fsSODetRows.Count;
                fsServiceOrderRow.ServicesRemaning = fsSODetRows.Count;

                foreach (FSSODet fsSODetRow in fsSODetRows) //It can be ignored and treegrid request node by node
                {

                    InventoryItem inventoryItemRow = (InventoryItem)graphExternalControls.InventoryItem
                                                        .Select(fsSODetRow.InventoryID);

                    if (fsSODetRow.Scheduled == true)
                    {
                        fsServiceOrderRow.ServicesRemaning = fsServiceOrderRow.ServicesRemaning - 1;
                    }

                    tmpSODet.Add(new
                    {
                        SODetID = fsSODetRow.SODetID,
                        InventoryID = fsSODetRow.InventoryID,
                        Text = (String.IsNullOrEmpty(inventoryItemRow.Descr)) ? inventoryItemRow.InventoryCD : inventoryItemRow.Descr,
                        ServicesCount = 1,
                        ServicesRemaning = (fsSODetRow.Scheduled == true) ? 0 : 1,
                        EstimatedDurationTotal = fsSODetRow.EstimatedDuration,
                        Leaf = true
                    });

                    serviceClassesList.Add(fsSODetRow.InventoryID);
                }

                fsServiceOrderRow.TreeID = fsServiceOrderRow.SOID;
                fsServiceOrderRow.Text = fsServiceOrderRow.RefNbr;
                fsServiceOrderRow.Leaf = false;
                fsServiceOrderRow.Rows = tmpSODet;
                fsServiceOrderRow.ServiceClassIDs = serviceClassesList.Distinct().ToArray();
                serviceClassesList.Clear();
                retList.Add(fsServiceOrderRow);
            }

            metaData = new MetaData(totalRows);
        }
        else
        {
            //Specific Service Order
            PXResultset<FSSODet> fsSODetRows = graphExternalControls.SODetServices.Select(node);

            foreach (FSSODet fsSODetRow in fsSODetRows)
            {

                InventoryItem inventoryItemRow = (InventoryItem)graphExternalControls.InventoryItem
                                                    .Select(fsSODetRow.InventoryID);

                retList.Add(new
                {
                    Text = (String.IsNullOrEmpty(inventoryItemRow.Descr)) ? inventoryItemRow.InventoryCD : inventoryItemRow.Descr,
                    leaf = true,
                });
            }

            metaData = new MetaData();
        }

        Response response = new Response(metaData, retList, true, null);

        return SerialJson(response);
    }

    #endregion

    #region WebMethodsTreeUnassignedAppointment

    /// <summary>
    /// Get unassigned employees
    /// </summary>
    /// <param name="ScheduledDate">
    /// DateTime of the wanted date
    /// <param name="branchID">
    /// Int of the Branch Identificator
    /// <param name="branchLocationID">
    /// Int of the Branch Location Identificator
    /// </param>
    /// <param name="unassignedAppointmentByRoom">
    /// If true returns only Appointment without rooms
    /// </param>
    /// <returns>List<FSAppointment></returns>
    [WebMethod]
    [ScriptMethod(UseHttpGet = true, ResponseFormat = ResponseFormat.Xml)]
    public static Object GetUnassignedAppointments(DateTime scheduledStartDate, DateTime? scheduledEndDate, int? branchID, int? branchLocationID, bool? unassignedAppointmentByRoom)
    {
        var graphExternalControls = PXGraph.CreateInstance<ExternalControls>();

        RequestParams request = new RequestParams(HttpContext.Current.Request);

        DateHandler requestDateStart = new DateHandler(scheduledStartDate);
        DateHandler requestDateEnd = scheduledEndDate == null ? new DateHandler(scheduledStartDate) : new DateHandler((DateTime)scheduledEndDate);
        DateTime timeBegin = requestDateStart.StartOfDay();
        DateTime timeEnd = requestDateEnd.BeginOfNextDay();

        string jsonPage = HttpContext.Current.Request.QueryString["page"];
        string jsonStart = HttpContext.Current.Request.QueryString["start"];
        string jsonLimit = HttpContext.Current.Request.QueryString["limit"];

        List<object> retList = new List<object>();

        var results = graphExternalControls.UnassignedAppointmentRecords(timeBegin, timeEnd, branchID, branchLocationID, unassignedAppointmentByRoom, request.Filters);

        int totalRows = results.Count;
        int total = (jsonLimit != null && jsonLimit != "") ? Convert.ToInt32(jsonLimit) : totalRows;
        int start = (jsonStart != null && jsonStart != "") ? Convert.ToInt32(jsonStart) : 0;

        for (int i = 0; i < total && (start + i < totalRows); i++)
        {
            retList.Add(results[i + start]);
        }

        MetaData metaData = new MetaData(totalRows);

        Response response = new Response(metaData, retList, true, null);

        return SerialJson(response);
    }

    #endregion

    #region WebMethodsCalendarPreferences
    #region WebMethodsCustomFieldAppointment
    /// <summary>
    /// GetCustomFieldAppointments obtain a list of customFieldAppointments filtered (or not) by the field active
    /// </summary>
    /// <returns>Returns a Response object with a list of type FSCustomFieldAppointments.</returns>
    [WebMethod]
    [ScriptMethod(UseHttpGet = true, ResponseFormat = ResponseFormat.Xml)]
    public static Object GetCustomFieldAppointments(bool? isActive)
    {
        var graphExternalControls = PXGraph.CreateInstance<ExternalControls>();
        FSSetup fsSetupRow = graphExternalControls.SetupRecords.Select();

        List<object> retList = new List<object>();
        PXResultset<FSCustomFieldAppointment> results;

        if (isActive != null)
        {
            results = (PXResultset<FSCustomFieldAppointment>)graphExternalControls.ActiveCustomFieldAppointments
                            .Select(isActive);
        }
        else
        {
            results = (PXResultset<FSCustomFieldAppointment>)graphExternalControls.CustomFieldAppointments
                                .Select();
        }

        foreach (FSCustomFieldAppointment fsCustomFieldAppointmentRow in results )
        {
            if (fsSetupRow != null)
            {
                if ((fsCustomFieldAppointmentRow.FieldName != "Room" || fsSetupRow.ManageRooms == true))
                {
                    retList.Add(fsCustomFieldAppointmentRow);
                }
            }
            else
            {
                retList.Add(fsCustomFieldAppointmentRow);
            }
        }

        MetaData metaData = new MetaData();

        Response response = new Response(metaData, retList, true, null);

        return SerialJson(response);
    }

    /// <summary>
    /// PutCustomFieldAppointments receives an array of customFieldAppointments that has to be modify on the database.
    /// </summary>
    /// <returns>Returns a Response object to see if succeded or failed.</returns>
    [WebMethod]
    [ScriptMethod(ResponseFormat = ResponseFormat.Json)]
    public static Response PutCustomFieldAppointments(FSCustomFieldAppointment[] customFieldAppointments)
    {
        var graphExternalControls = PXGraph.CreateInstance<ExternalControls>();

        List<object> retList = new List<object>();

        graphExternalControls.PutCustomFieldAppointments(customFieldAppointments);

        MetaData metaData = new MetaData();

        Response response = new Response(metaData, retList, true, null);

        return response;
    }

    /// <summary>
    /// DeleteCustomFieldAppointments receives an array of customFieldAppointments that has to be deleted on the database.
    /// </summary>
    /// <returns>Returns a Response object to see if succeded or failed.</returns>
    [WebMethod]
    [ScriptMethod(ResponseFormat = ResponseFormat.Json)]
    public static Response DeleteCustomFieldAppointments(FSCustomFieldAppointment[] customFieldAppointments)
    {
        var graphExternalControls = PXGraph.CreateInstance<ExternalControls>();

        List<object> retList = new List<object>();

        graphExternalControls.DeleteCustomFieldAppointments(customFieldAppointments);

        MetaData metaData = new MetaData();

        Response response = new Response(metaData, retList, true, null);

        return response;
    }

    /// <summary>
    /// CreateCustomFieldAppointments receives an array of customFieldAppointments that has to be created on the database.
    /// </summary>
    /// <returns>Returns a Response object to see if succeded or failed.</returns>
    [WebMethod]
    [ScriptMethod(ResponseFormat = ResponseFormat.Json)]
    public static Response CreateCustomFieldAppointments(FSCustomFieldAppointment[] customFieldAppointments)
    {
        var graphExternalControls = PXGraph.CreateInstance<ExternalControls>();

        List<object> retList = new List<object>();

        graphExternalControls.CreateCustomFieldAppointments(customFieldAppointments);

        MetaData metaData = new MetaData();

        Response response = new Response(metaData, retList, true, null);

        return response;
    }
    #endregion
    #region WebMethodsCustomAppointmentStatus
    /// <summary>
    /// GetCustomAppointmentStatuses obtain a list of customAppointmentStatuses
    /// </summary>
    /// <returns>Returns a Response object with a list of type FSCustomAppointmentStatus.</returns>
    [WebMethod]
    [ScriptMethod(UseHttpGet = true, ResponseFormat = ResponseFormat.Xml)]
    public static Object GetCustomAppointmentStatuses()
    {
        var graphExternalControls = PXGraph.CreateInstance<ExternalControls>();

        List<object> retList = new List<object>();
        PXResultset<FSCustomAppointmentStatus> results;

        results = (PXResultset<FSCustomAppointmentStatus>)graphExternalControls.CustomAppointmentStatuses
                       .Select();

        for (int i = 0; i < results.Count; ++i)
        {
            retList.Add((FSCustomAppointmentStatus)results[i]);
        }

        MetaData metaData = new MetaData();

        Response response = new Response(metaData, retList, true, null);

        return SerialJson(response);
    }

    /// <summary>
    /// PutCustomAppointmentStatuses receives an array of customAppointmentStatuses that has to be modify on the database.
    /// </summary>
    /// <returns>Returns a Response object to see if succeded or failed.</returns>
    [WebMethod]
    [ScriptMethod(ResponseFormat = ResponseFormat.Json)]
    public static Response PutCustomAppointmentStatuses(FSCustomAppointmentStatus[] customAppointmentStatuses)
    {
        var graphExternalControls = PXGraph.CreateInstance<ExternalControls>();

        List<object> retList = new List<object>();

        graphExternalControls.PutCustomAppointmentStatuses(customAppointmentStatuses);

        MetaData metaData = new MetaData();

        Response response = new Response(metaData, retList, true, null);

        return response;
    }

    /// <summary>
    /// DeleteCustomAppointmentStatuses receives an array of customAppointmentStatuses that has to be deleted on the database.
    /// </summary>
    /// <returns>Returns a Response object to see if succeded or failed.</returns>
    [WebMethod]
    [ScriptMethod(ResponseFormat = ResponseFormat.Json)]
    public static Response DeleteCustomAppointmentStatuses(FSCustomAppointmentStatus[] customAppointmentStatuses)
    {
        var graphExternalControls = PXGraph.CreateInstance<ExternalControls>();

        List<object> retList = new List<object>();

        graphExternalControls.DeleteCustomAppointmentStatuses(customAppointmentStatuses);

        MetaData metaData = new MetaData();

        Response response = new Response(metaData, retList, true, null);

        return response;
    }

    /// <summary>
    /// CreateCustomAppointmentStatuses receives an array of customAppointmentStatuses that has to be created on the database.
    /// </summary>
    /// <returns>Returns a Response object to see if succeded or failed.</returns>
    [WebMethod]
    [ScriptMethod(ResponseFormat = ResponseFormat.Json)]
    public static Response CreateCustomAppointmentStatuses(FSCustomAppointmentStatus[] customAppointmentStatuses)
    {
        var graphExternalControls = PXGraph.CreateInstance<ExternalControls>();

        List<object> retList = new List<object>();

        graphExternalControls.CreateCustomAppointmentStatuses(customAppointmentStatuses);

        MetaData metaData = new MetaData();

        Response response = new Response(metaData, retList, true, null);

        return response;
    }
    #endregion
    #endregion

    #region WebMethodsRoutes
    /**
     * Web Methods related to the Routes Screen (Maps).
    */
    [WebMethod]
    [ScriptMethod(UseHttpGet = true, ResponseFormat = ResponseFormat.Xml)]
    public static Object GetRoutes(DateTime scheduledDate, int? node, int branchID)
    {
        var graphExternalControls = PXGraph.CreateInstance<ExternalControls>();
        RequestParams request = new RequestParams(HttpContext.Current.Request);
        List<object> retList = new List<object>();
        MetaData metaData;
        DateHandler requestDate = new DateHandler(scheduledDate);
        DateTime timeBegin = requestDate.StartOfDay();
        DateTime timeEnd = requestDate.BeginOfNextDay();
        string RefNbr = "";
        //Search All Routes
        if (node == 0)
        {
            ExternalControls.DispatchBoardFilters[] filters = request.Filters ;

            if (filters != null)
            {
                for (int i = 0; i < filters.Length; i++)
                {
                    if (filters[i].property == "RefNbr")
                    {
                        RefNbr = filters[i].value[0];
                        break;
                    }
                }
            }

            dynamic results;

            if (RefNbr != "")
            {
                results = graphExternalControls.RouteRecordsByDateAndRefNbr
                                .Select(timeBegin, RefNbr, branchID);
            }
            else
            {
                results = graphExternalControls.RouteRecordsByDate
                                    .Select(timeBegin, branchID);
            }

            int totalRows = results.Count;
            int total = (request.Limit != 0 && request.Limit < results.Count) ? request.Limit : results.Count;
            int start = (request.Start != 0) ? request.Start : 0;
            List<ExternalControls.RouteNode> tmpLeaf;

            //Paging Route
            for (int i = 0; i < total && (start + i < results.Count); i++)
            {
                PXResult<FSRouteDocument, FSRoute, FSEquipment, EPEmployee> result = (PXResult<FSRouteDocument, FSRoute, FSEquipment, EPEmployee>)results[start + i];
                FSRouteDocument fsRouteDocumentRow = result;
                FSRoute fsRoute = result;
                EPEmployee driver = result;
                FSEquipment vehicle = result;
                PXResultset<FSGPSTrackingRequest> fsGPSTrackingRequestRows = null;

                if (driver != null) { 
                    fsGPSTrackingRequestRows = graphExternalControls.GPSTrackingRequestByEmployee.Select(driver.BAccountID);
                }


                tmpLeaf = graphExternalControls.GetTreeAppointmentNodesByRoute((int)fsRouteDocumentRow.RouteDocumentID, fsRoute);

                //Route Tree Node
                retList.Add(new ExternalControls.RouteNode(fsRoute, fsRouteDocumentRow, tmpLeaf, driver, vehicle, fsRouteDocumentRow.RefNbr, fsGPSTrackingRequestRows));
            }

            metaData = new MetaData(totalRows);
        }
        else
        {
            //Specific Route
            retList = graphExternalControls.GetTreeAppointmentNodesByRoute((int)node, null).Cast<object>().ToList(); ;

            metaData = new MetaData();
        }

        Response response = new Response(metaData, retList, true, null);

        return SerialJson(response);
    }

    /// <summary>
    /// Get a list of resources (Employees/Machines) with thier related route appointments in a specific date.
    /// </summary>
    /// <param name="AppointmentsDate">
    /// double - Desired Date
    /// </param>
    /// <returns>List with a specific structure for Routes Screen</returns>
    [WebMethod]
    [ScriptMethod(UseHttpGet = true, ResponseFormat = ResponseFormat.Xml)]
    public static Object GetEmployeeRoutes(DateTime scheduledDate, int? node, int branchID, int? branchLocationID)
    {
        var graphExternalControls = PXGraph.CreateInstance<ExternalControls>();
        RequestParams request = new RequestParams(HttpContext.Current.Request);
        List<object> retList = new List<object>();
        MetaData metaData;

        DateHandler requestDate = new DateHandler(scheduledDate);
        DateTime timeBegin = requestDate.StartOfDay();
        DateTime timeEnd = requestDate.BeginOfNextDay();

        //Search All Routes
        if (node == 0)
        {
            var results = graphExternalControls.RoutesAndDriversByDate.Select(timeBegin, timeEnd, branchID);

            int totalRows = results.Count;
            int total = (request.Limit != 0 && request.Limit < results.Count) ? request.Limit : results.Count;
            int start = (request.Start != 0) ? request.Start : 0;
            List<ExternalControls.RouteNode> tmpLeaf;
            //Paging Route
            for (int i = 0; i < total && (start + i < results.Count); i++)
            {
                PXResult<FSRouteDocument, FSRoute, EPEmployee, BAccount, FSEquipment> result = (PXResult<FSRouteDocument, FSRoute, EPEmployee, BAccount, FSEquipment>)results[start+ i];

                FSRouteDocument fsRouteDocumentRow = result;
                FSRoute fsRoute = result;
                EPEmployee driver = result;
                FSEquipment vehicle = result;

                tmpLeaf = graphExternalControls.GetTreeAppointmentNodesByRoute((int)fsRouteDocumentRow.RouteDocumentID, fsRoute);

                PXResultset<FSGPSTrackingRequest> fsGPSTrackingRequestRows = null;

                if (driver != null)
                {
                    fsGPSTrackingRequestRows = graphExternalControls.GPSTrackingRequestByEmployee.Select(driver.BAccountID);
                }

                //Route Tree Node
                retList.Add(new ExternalControls.RouteNode(fsRoute, fsRouteDocumentRow, tmpLeaf, driver, vehicle, driver.AcctName, fsGPSTrackingRequestRows));
            }

            metaData = new MetaData(totalRows);
        }
        else
        {
            //Specific Route
            retList = graphExternalControls.GetTreeAppointmentNodesByRoute((int)node, null).Cast<object>().ToList(); ;

            metaData = new MetaData();
        }

        Response response = new Response(metaData, retList, true, null);

        return SerialJson(response);
    }

    /// <summary>
    /// Get a list of resources (Employees/Machines) with thier related appointments in a specific date.
    /// </summary>
    /// <param name="AppointmentsDate">
    /// double - Desired Date
    /// </param>
    /// <returns>List with a specific structure for Routes Screen</returns>
    [WebMethod]
    [ScriptMethod(UseHttpGet = true, ResponseFormat = ResponseFormat.Xml)]
    public static Object GetStaffAppointments(DateTime scheduledDate, int? node, int branchID, int? branchLocationID, bool unassignedAppointmentFlag)
    {
        var graphExternalControls = PXGraph.CreateInstance<ExternalControls>();
        RequestParams request = new RequestParams(HttpContext.Current.Request);
        List<object> retList = new List<object>();
        MetaData metaData;

        DateHandler requestDate = new DateHandler(scheduledDate);
        DateTime timeBegin = requestDate.StartOfDay();
        DateTime timeEnd = requestDate.BeginOfNextDay();

        //Search All Routes
        if (node == 0)
        {
            if (unassignedAppointmentFlag == true)
            {
                //Unassigned Appointment
                List<object> unassignedAppointment = graphExternalControls.GetUnassignedAppointmentNode(timeBegin, branchID, branchLocationID);

                if (unassignedAppointment.Count > 0)
                {
                    //Route Tree Node
                    retList.Add(new
                    {
                        NodeID = 0,
                        Text = "Unassigned",
                        Leaf = (unassignedAppointment.Count == 0),
                        Rows = unassignedAppointment,
                        Checked = true
                    });
                }
            }

            dynamic results;

            if (branchLocationID != null)
            {
                results = graphExternalControls.EmployeeAppointmentsByDateAndBranchLocation
                                .Select(timeBegin, timeEnd, branchID, branchLocationID);
            }
            else
            {
                results = graphExternalControls.EmployeeAppointmentsByDate
                                    .Select(timeBegin, timeEnd, branchID);
            }

            int totalRows = results.Count;
            int total = (request.Limit != 0 && request.Limit < results.Count) ? request.Limit : results.Count;
            int start = (request.Start != 0) ? request.Start : 0;
            List<ExternalControls.RouteNode> tmpLeaf;
            //Paging Route
            for (int i = 0; i < total && (start + i < results.Count); i++)
            {
                PXResult<EPEmployee, FSAppointmentEmployee, FSAppointment, FSServiceOrder, Branch, BAccount, Customer> result = (PXResult<EPEmployee, FSAppointmentEmployee, FSAppointment, FSServiceOrder, Branch, BAccount, Customer>)results[start + i];
                EPEmployee epEmployee = result;

                tmpLeaf = graphExternalControls.GetTreeAppointmentNodesByEmployee((int)epEmployee.BAccountID, timeBegin);

                string currentTrackingID = string.Empty;

                if (epEmployee != null)
                {
                    DayOfWeek dayOfWeek = scheduledDate.DayOfWeek;

                    PXResultset<FSGPSTrackingRequest> fsGPSTrackingRequestRows = null;
                    fsGPSTrackingRequestRows = graphExternalControls.GPSTrackingRequestByEmployee.Select(epEmployee.BAccountID);

                    if (fsGPSTrackingRequestRows != null && fsGPSTrackingRequestRows.Count > 0)
                    {
                        foreach (FSGPSTrackingRequest fsGPSTrackingRequestRow in fsGPSTrackingRequestRows)
                        {
                            switch (dayOfWeek)
                            {
                                case DayOfWeek.Sunday:
                                    if (fsGPSTrackingRequestRow.WeeklyOnDay1 == true)
                                    {
                                        currentTrackingID = fsGPSTrackingRequestRow.TrackingID.ToString();
                                    }
                                    break;
                                case DayOfWeek.Monday:
                                    if (fsGPSTrackingRequestRow.WeeklyOnDay2 == true)
                                    {
                                        currentTrackingID = fsGPSTrackingRequestRow.TrackingID.ToString();
                                    }
                                    break;
                                case DayOfWeek.Tuesday:
                                    if (fsGPSTrackingRequestRow.WeeklyOnDay3 == true)
                                    {
                                        currentTrackingID = fsGPSTrackingRequestRow.TrackingID.ToString();
                                    }
                                    break;
                                case DayOfWeek.Wednesday:
                                    if (fsGPSTrackingRequestRow.WeeklyOnDay4 == true)
                                    {
                                        currentTrackingID = fsGPSTrackingRequestRow.TrackingID.ToString();
                                    }
                                    break;
                                case DayOfWeek.Thursday:
                                    if (fsGPSTrackingRequestRow.WeeklyOnDay5 == true)
                                    {
                                        currentTrackingID = fsGPSTrackingRequestRow.TrackingID.ToString();
                                    }
                                    break;
                                case DayOfWeek.Friday:
                                    if (fsGPSTrackingRequestRow.WeeklyOnDay6 == true)
                                    {
                                        currentTrackingID = fsGPSTrackingRequestRow.TrackingID.ToString();
                                    }
                                    break;
                                case DayOfWeek.Saturday:
                                    if (fsGPSTrackingRequestRow.WeeklyOnDay7 == true)
                                    {
                                        currentTrackingID = fsGPSTrackingRequestRow.TrackingID.ToString();
                                    }
                                    break;
                                default:
                                    currentTrackingID = ((FSGPSTrackingRequest)fsGPSTrackingRequestRows[0]).TrackingID.ToString();
                                    break;
                            }

                            if (currentTrackingID != string.Empty)
                            {
                                break;
                            }
                        }
                    }
                }

                //Route Tree Node
                retList.Add(new
                {
                    NodeID = epEmployee.BAccountID,
                    Text = epEmployee.AcctName,
                    EmployeeID = epEmployee.AcctCD,
                    TrackingID = currentTrackingID,
                    Leaf = (tmpLeaf.Count == 0),
                    Rows = tmpLeaf,
                    Checked = true
                });
            }

            metaData = new MetaData(totalRows);
        }
        else
        {
            //Specific Route
            retList = graphExternalControls.GetTreeAppointmentNodesByEmployee((int)node, timeBegin).Cast<object>().ToList(); ;

            metaData = new MetaData();
        }

        Response response = new Response(metaData, retList, true, null);

        return SerialJson(response);
    }

    /// <summary>
    /// Get a list of resources (Employees/Machines) with thier related appointments in a specific date.
    /// </summary>
    /// <param name="appointmentsDate">
    /// double - Desired Date
    /// </param>
    /// <returns>List with a specific structure for Routes Screen</returns>
    [WebMethod]
    [ScriptMethod(UseHttpGet = true, ResponseFormat = ResponseFormat.Xml)]
    public static Object GetResourceAppointments(double appointmentsDate)
    {
        var graphExternalControls = PXGraph.CreateInstance<ExternalControls>();

        DateHandler requestDate = new DateHandler(appointmentsDate);
        DateTime timeBegin = requestDate.StartOfDay();
        DateTime timeEnd = requestDate.BeginOfNextDay();

        List<object> retList = new List<object>();

        var results = graphExternalControls.AppointmentRecords
                        .Select(timeBegin, timeEnd);

        int? lastEmployee = -1;
        bool unassignedAdded = false;

        List<object> tmpAppointment = new List<object>();

        foreach (PXResult<FSAppointment, FSServiceOrder, FSAppointmentEmployee, EPEmployee, Location, FSContact, Customer> result in results)
        {
            FSAppointment fsAppointmentRow = result;
            EPEmployee epEmployeeRow = result;
            Customer bAccountRow = result;
            FSContact fsContactRow = result;
            Location locationRow = result;
            FSServiceOrder fsServiceOrderRow = result;
            Contact employeeContactRow = (Contact)graphExternalControls.EmployeeContact.Select(epEmployeeRow.ParentBAccountID, epEmployeeRow.DefContactID);

            // Check for unassigned appointment
            if (epEmployeeRow.BAccountID == null)
            {
                Object tmpEmployee = new
                {
                    ID = "U-1",
                    OriginalID = -1, // Unassigned root indication
                    LastParentID = "0",
                    Text = "Unassigned",
                    //@TODO Refactor this att into the treeNode model (issue SD-3977)
                    iconCls = "unassigned-appointment-icon-cls",
                    expanded = true,
                    NodeType = "EMPLOYEE",
                    Rows = tmpAppointment
                };

                if (!unassignedAdded)
                {
                    retList.Add(tmpEmployee);
                    unassignedAdded = true;
                }

            }
            else
            {
                if (lastEmployee != epEmployeeRow.BAccountID)
                {
                    tmpAppointment = new List<object>();
                    lastEmployee = epEmployeeRow.BAccountID;
                    Object tmpEmployee = new
                    {
                        ID = "E-" + lastEmployee,
                        OriginalID = epEmployeeRow.BAccountID,
                        LastParentID = "0",
                        Text = epEmployeeRow.AcctName,
                        ResourceContact = employeeContactRow.Phone1,
                        //@TODO Refactor this att into the treeNode model (issue SD-3977)
                        iconCls = "employee-icon-cls",
                        expanded = true,
                        NodeType = "EMPLOYEE",
                        Rows = tmpAppointment
                    };

                    retList.Add(tmpEmployee);
                }
            }

            tmpAppointment.Add(new
            {
                //TreeView fields
                ID = "A-" + fsAppointmentRow.AppointmentID.ToString(),
                OriginalID = fsAppointmentRow.AppointmentID.ToString(),
                LastParentID = "E-" + lastEmployee,
                Text = fsAppointmentRow.RefNbr,
                RefNbr = fsAppointmentRow.RefNbr,
                Confirmed = fsAppointmentRow.Confirmed,
                CustomID = fsAppointmentRow.AppointmentID.ToString() + epEmployeeRow.BAccountID.ToString(),
                Duration = fsAppointmentRow.ActualDurationTotal,
                Address = fsAppointmentRow.MapLatitude.ToString() + ", " + fsAppointmentRow.MapLongitude.ToString(),
                //@TODO Refactor this att into the treeNode model (issue SD-3977)
                iconCls = "appointment-icon-cls",
                NodeType = "APPOINTMENT",
                leaf = true,
                MapLatitude = fsAppointmentRow.MapLatitude,
                MapLongitude = fsAppointmentRow.MapLongitude,

                //Tooltip fields
                ContactName = fsContactRow.DisplayName,
                ContactEmail = fsContactRow.Email,
                ContactPhone = fsContactRow.Phone1,
                CustomerName = bAccountRow.AcctName,
                SORefNbr = fsServiceOrderRow.RefNbr,
                SrvOrdType = fsServiceOrderRow.SrvOrdType,
                LocationDesc = locationRow.Descr,
                Status = fsAppointmentRow.Status,
                ScheduledDateTimeBegin = fsAppointmentRow.ScheduledDateTimeBegin,
                ScheduledDateTimeEnd = fsAppointmentRow.ScheduledDateTimeEnd,
                DocDesc = fsAppointmentRow.DocDesc
            });
        }

        MetaData metaData = new MetaData();

        Response response = new Response(metaData, retList, true, null);

        return SerialJson(response);
    }

    /// <summary>
    /// Receives a FSAppointment that has to be deleted on the database
    /// </summary>
    /// <param name="appointment">
    /// FSAppointment
    /// </param>
    /// <returns>Returns a Response object to see if succeded or failed.</returns>
    [WebMethod]
    [ScriptMethod(ResponseFormat = ResponseFormat.Xml)]
    public static Object PutRoutes(ExternalControls.RouteNode[] RouteNodes)
    {
        var graphExternalControls = PXGraph.CreateInstance<ExternalControls>();

        ExternalControls.DispatchBoardAppointmentMessages messages = graphExternalControls.DBPutRoutes(RouteNodes);

        bool success = true;

        MetaData metaData = new MetaData();

        Response response = new Response(metaData, null, success, messages);

        return SerialJson(response);
    }

    [WebMethod]
    [ScriptMethod(UseHttpGet = true, ResponseFormat = ResponseFormat.Xml)]
    public static Object GetRouteDevices(String[] RouteDevice)
    {
        var graphExternalControls = PXGraph.CreateInstance<ExternalControls>();

        bool success = true;
		
		List<object> retList = new List<object>();
		
        MetaData metaData = new MetaData();

        foreach (string trackingID in RouteDevice)
        {
            var result = graphExternalControls.FSGPSTrackingHistoryByTrackingID.SelectSingle(Guid.Parse(trackingID));

            if (result != null)
            {
                FSGPSTrackingHistory row = result;

                retList.Add(new
                {
                    TrackingID = row.TrackingID.ToString(),
                    ExecutionDate = row.ExecutionDate.ToString(),
                    Latitude = row.Latitude,
                    Longitude = row.Longitude,
                    Altitude = row.Altitude
                });
            }
        }

        Response response = new Response(metaData, retList, success, null);

        return SerialJson(response);
    }

    [WebMethod]
    [ScriptMethod(UseHttpGet = true, ResponseFormat = ResponseFormat.Xml)]
    public static Object GetTrackingHistory(string trackingID, DateTime currentDate, int minutesGap)
    {
        var graphExternalControls = PXGraph.CreateInstance<ExternalControls>();

        bool success = true;

        List<object> retList = new List<object>();

        MetaData metaData = new MetaData();

        var results = graphExternalControls.FSGPSTrackingHistoryByTrackingIDAndDate.Select(Guid.Parse(trackingID), currentDate, currentDate.AddDays(1));

        if (results != null && results.Count > 0)
        {
            FSGPSTrackingHistory pivot = results[0];
            DateTime? pivotDate = pivot.ExecutionDate;
            string lastDateAdded = "";

            foreach (FSGPSTrackingHistory row in results)
            {
                if (pivotDate.Equals(row.ExecutionDate)
                    || row.ExecutionDate > pivotDate)
                {
                    retList.Add(new
                    {
                        TrackingID = row.TrackingID.ToString(),
                        ExecutionDate = row.ExecutionDate.ToString(),
                        Latitude = row.Latitude,
                        Longitude = row.Longitude,
                        Altitude = row.Altitude
                    });

                    lastDateAdded = row.ExecutionDate.ToString();
                    pivotDate = pivotDate.Value.AddMinutes(minutesGap);
                }
            }

            pivot = results[results.Count - 1];

            if (string.IsNullOrEmpty(lastDateAdded) == false
                && pivot.ExecutionDate.ToString() != lastDateAdded)
            {
                retList.Add(new
                {
                    TrackingID = pivot.TrackingID.ToString(),
                    ExecutionDate = pivot.ExecutionDate.ToString(),
                    Latitude = pivot.Latitude,
                    Longitude = pivot.Longitude,
                    Altitude = pivot.Altitude
                });
            }
        }

        Response response = new Response(metaData, retList, success, null);

        return SerialJson(response);
    }
    #endregion

    #region WebMethodBridge
    /// <summary>
    /// Create an FSWrkProcess
    /// </summary>
    /// <param name="appointment">
    /// FSAppointment
    /// </param>
    /// <param name="sODetIDList">
    /// List<int>
    /// </param>
    /// <param name="employeeIDList">
    /// List<int>
    /// </param>
    /// <returns>Returns a Response object to see if succeded or failed.</returns>
    [WebMethod]
    [ScriptMethod(UseHttpGet = false, ResponseFormat = ResponseFormat.Xml)]
    public static Object NewAppointmentBridge(FSAppointmentScheduleBoard appointment, List<int> sODetIDList, List<int> employeeIDList)
    {
        var graphExternalControls = PXGraph.CreateInstance<ExternalControls>();

        MetaData metaData = new MetaData();
        List<object> retList = new List<object>();

        ExternalControls.DispatchBoardAppointmentMessages messages = graphExternalControls.CheckApppointmentCreationByAccessRights();

        bool success = (messages != null && messages.ErrorMessages.Count == 0) ? true : false;
        
        if(success)
        {
            bool isAppointment = false;
            int? entityID = graphExternalControls.DBCreateAppointmentBridge(appointment, sODetIDList, employeeIDList, out isAppointment);

            if (isAppointment == false)
            {
                retList.Add(new { ProcessID = entityID });
            }
            else
            {
                retList.Add(new { AppointmentID = entityID });
            }
        }

        Response response = new Response(metaData, retList, success, messages);

        return SerialJson(response);
    }

    /// <summary>
    /// Create an FSWrkEmployeeSchedule
    /// </summary>
    /// <param name="wrkEmployeeSchedule">
    /// FSWrkEmployeeSchedule
    /// </param>
    /// <returns>Returns a Response object to see if succeded or failed.</returns>
    [WebMethod]
    [ScriptMethod(UseHttpGet = false, ResponseFormat = ResponseFormat.Json)]
    public static Response NewWrkSchedulerBridge(FSWrkEmployeeSchedule wrkEmployeeSchedule)
    {
        var graphExternalControls = PXGraph.CreateInstance<ExternalControls>();

        MetaData metaData = new MetaData();
        List<object> retList = new List<object>();

        int? processID = graphExternalControls.DBCreateWrkSchedulerBridge(wrkEmployeeSchedule);

        retList.Add(new { ProcessID = processID });
        Response response = new Response(metaData, retList, true, null);

        return response;
    }
    #endregion

    #region PrivateClasses&Methods

    public static Object SerialJson(Response response)
    {
        JavaScriptSerializer jsJson = new JavaScriptSerializer();
        jsJson.MaxJsonLength = int.MaxValue;
        return jsJson.Serialize(response);
    }

    private static string durationFormat(double duration = 0, int format = (int)SharedFunctions.TimeFormat.Hours)
    {

        string value = "";

        int minutes = 0;
        int hours = 0;
        int days = 0;

        if (format == (int)SharedFunctions.TimeFormat.Hours)
        {
            duration = duration * 60;
        }

        if (format == (int)SharedFunctions.TimeFormat.Seconds)
        {
            duration = duration / 60;
        }

        hours = (int)(duration / 60);
        minutes = (int)(duration % 60);

        days = (int)duration / (60 * 24);
        hours = hours - days * 24;

        if (days > 0)
        {
            value = days + "d ";
        }

        value = value + hours + "h " + minutes + "m";

        return value;
    }

    public new class Response
    {
        public Result Result;
        public MetaData MetaData;
        public bool Success;
        public String Error;
        public ExternalControls.DispatchBoardAppointmentMessages Messages;

        public Response(MetaData metaData, List<object> rows, bool success, ExternalControls.DispatchBoardAppointmentMessages messages)
        {
            this.MetaData = metaData;
            this.Result = new Result();
            this.Result.Rows = (rows != null) ? rows : new List<object>();
            this.Result.TotalRows = (rows != null) ? rows.Count : 0;

            if (this.MetaData.TotalRows != null && this.MetaData.TotalRows > 0)
            {
                this.Result.TotalRows = this.MetaData.TotalRows;
            }

            this.Success = success;

            this.Messages = (messages != null) ? messages : null;

            if (!success)
            {
                HttpContext.Current.Response.StatusCode = (int)System.Net.HttpStatusCode.InternalServerError;
                HttpContext.Current.Response.StatusDescription = (messages.ErrorMessages.Count > 0) ? messages.ErrorMessages[0] : "";
            }

            this.Result.MetaData = metaData;
            this.Result.Success = success;
            this.Result.Messages = (messages != null) ? messages : null;
        }
    }

    public class MetaData
    {
        public int? TotalRows;
        public DateTime? minTimeBegin;
        public DateTime? maxTimeEnd;

        public MetaData(int? totalRows)
        {
            this.TotalRows = totalRows;
        }

        public MetaData(DateTime? minTimeBegin, DateTime? maxTimeEnd)
        {
            this.minTimeBegin = minTimeBegin;
            this.maxTimeEnd = maxTimeEnd;
        }

        public MetaData()
        {
            this.TotalRows = null;
        }
    }

    public class Result
    {
        public List<object> Rows;
        public int? TotalRows;
        public MetaData MetaData;
        public bool Success;
        public String Error;
        public ExternalControls.DispatchBoardAppointmentMessages Messages;
    }

    public class RequestParams
    {
        public ExternalControls.DispatchBoardFilters[] Filters;
        public int Page;
        public int Start;
        public int Limit;
        public bool PagingFlag = false;

        public RequestParams(HttpRequest request)
        {
            JavaScriptSerializer serializer = new JavaScriptSerializer();
            string jsonFilters = HttpContext.Current.Request.QueryString["filter"];
            string jsonPage = HttpContext.Current.Request.QueryString["page"];
            string jsonStart = HttpContext.Current.Request.QueryString["start"];
            string jsonLimit = HttpContext.Current.Request.QueryString["limit"];

            Filters = (jsonFilters != null) ?
                        serializer.Deserialize<ExternalControls.DispatchBoardFilters[]>(jsonFilters) :
                        null;

            Limit = (jsonLimit != null && jsonLimit != "") ? Convert.ToInt32(jsonLimit) : 0;
            Start = (jsonStart != null && jsonStart != "") ? Convert.ToInt32(jsonStart) : 0;
            Page = (jsonPage != null && jsonPage != "") ? Convert.ToInt32(jsonPage) : 0;
        }

        public RequestParams()
        {
            Page = 0;
            Start = 0;
            Limit = 0;
            Filters = null;
        }

        public void EnablePaging()
        {
            PagingFlag = true;
        }

        public void DisablePaging()
        {
            PagingFlag = false;
        }

        public bool IsPaging()
        {
            return PagingFlag;
        }
    }
    #endregion
}
