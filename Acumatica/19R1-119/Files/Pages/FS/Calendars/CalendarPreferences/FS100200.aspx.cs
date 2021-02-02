using System;
using System.IO;
using PX.Objects.FS;
using System.Collections.Generic;

public partial class Page_FS100200 : PX.Web.UI.PXPage
{
    public string applicationName;
    public string pageUrl;
    public string preferencesTemplate;

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

        // Load Preference Template
        StreamReader streamReader = new StreamReader(Server.MapPath("../../Shared/templates/PreferencesTemplate.html"));
        preferencesTemplate = streamReader.ReadToEnd();
        streamReader.Close();
    }
}