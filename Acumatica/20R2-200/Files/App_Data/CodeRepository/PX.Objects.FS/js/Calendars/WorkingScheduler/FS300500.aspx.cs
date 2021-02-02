using PX.Objects.FS;
using PX.Common;
using PX.Data;
using PX.Objects.CR;
using PX.Objects.EP;
using System;
using System.Globalization;
using System.IO;
using System.Collections.Generic;

public partial class Page_FS300500 : PX.Web.UI.PXPage
{
    public String applicationName;
    public String pageUrl;
    public String RefNbr;
    public String eventBodyTemplate;
    public String startDate;
    public String DefaultEmployee;
    public String ExternalEmployee;

    protected void Page_Init(object sender, EventArgs e)
    {
        if (!Page.IsCallback)
        {
            var dict = SharedFunctions.GetCalendarMessages();
            this.ClientScript.RegisterClientScriptBlock(GetType(), "localeStrings", "var __localeStrings=" + Newtonsoft.Json.JsonConvert.SerializeObject(dict) + ";", true);
        }
    }

    protected void Page_Load(object sender, EventArgs e)
    {
        applicationName = Request.ApplicationPath.TrimEnd('/');
        pageUrl = SharedFunctions.GetWebMethodPath(Request.Path);

		DateTime? startDateBridge;
        var date = PXContext.GetBusinessDate();

        startDateBridge = (date != null) ? date : PXTimeZoneInfo.Now;
		
        // Filter By RefNbr
        RefNbr = Request.QueryString["RefNbr"];

        // Employee
        ExternalEmployee = Request.QueryString["bAccountID"];

        // Date
        string externalDate = Request.QueryString["Date"];
        DateTime currentDate;

        if (!string.IsNullOrEmpty(externalDate)){
         currentDate = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Local).AddMilliseconds(Double.Parse(externalDate));
         startDateBridge = currentDate;
        }
		
		startDate = ((DateTime)startDateBridge).ToString("MM/dd/yyyy h:mm:ss tt", new CultureInfo("en-US"));

        var graphExternalControls = PXGraph.CreateInstance<ExternalControls>();
        var results = graphExternalControls.EmployeeSelected.Select();

        PXResult<EPEmployee, Contact> result = (PXResult<EPEmployee, Contact>)results;

        EPEmployee epEmployeeRow = result;
        
        if (epEmployeeRow != null){
          DefaultEmployee = epEmployeeRow.BAccountID.ToString();
        }

        if (string.IsNullOrEmpty(ExternalEmployee) && epEmployeeRow != null){
         ExternalEmployee = DefaultEmployee;
        }

        //Load Availability Event Body Template
        StreamReader streamReader = new StreamReader(Server.MapPath("../../Shared/templates/AvailabilityEventTemplate.html"));
        eventBodyTemplate = streamReader.ReadToEnd();
        streamReader.Close();
    }
}