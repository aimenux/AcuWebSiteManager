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

public partial class Page_FS300900 : PX.Web.UI.PXPage
{
    public String applicationName;
    public String pageUrl;
    public String infoRoute;
    public String RefNbr;
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

        string startDateRqst = Request.QueryString["Date"];
		DateTime? startDateBridge;

        // Filter By RefNbr
        RefNbr = Request.QueryString["RefNbr"];

        // Filter By BranchID
        branchID = Request.QueryString["BranchID"];

        try{
            startDateBridge = (!String.IsNullOrEmpty(startDateRqst)) ? Convert.ToDateTime(startDateRqst) : PXContext.GetBusinessDate();
        } catch (Exception) {
            throw;
        }

        startDateBridge = (startDateBridge != null) ? startDateBridge : PXTimeZoneInfo.Now;

        startDate = ((DateTime)startDateBridge).ToString("MM/dd/yyyy h:mm:ss tt", new CultureInfo("en-US"));

        // Route Information
        StreamReader streamReader = new StreamReader(Server.MapPath("../../Shared/templates/InfoRoute.html"));
        infoRoute = streamReader.ReadToEnd();
        streamReader.Close();
    }
}