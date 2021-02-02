using PX.Objects.FS;
using PX.Common;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

public partial class Page_FS301200 : PX.Web.UI.PXPage
{
    public String applicationName;
    public String pageUrl;
    public String infoRoute;
    public String startDate;
    public String apiKey;
    public String branchID;

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
        apiKey = SharedFunctions.GetMapApiKey(new PX.Data.PXGraph());

        DateTime? startDateBridge;
        var date = PXContext.GetBusinessDate();

        // Filter By BranchID
        branchID = Request.QueryString["BranchID"];

        startDateBridge = (date != null) ? date : PXTimeZoneInfo.Now;

        // Date
        try
        {
            if (!String.IsNullOrEmpty(Request.QueryString["Date"]))
            {
                startDateBridge = Convert.ToDateTime(Request.QueryString["Date"]);
            }
        }
        catch (Exception)
        {
        }
        
        startDate = ((DateTime)startDateBridge).ToString("MM/dd/yyyy h:mm:ss tt", new CultureInfo("en-US"));

        // Route Information
        StreamReader streamReader = new StreamReader(Server.MapPath("../../Shared/templates/InfoEmployeeRoute.html"));
        infoRoute = streamReader.ReadToEnd();
        streamReader.Close();
    }
}