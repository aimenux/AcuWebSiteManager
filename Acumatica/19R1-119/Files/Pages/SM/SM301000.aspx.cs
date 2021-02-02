using System;
using System.Configuration;
using System.Collections;
using System.Web;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.UI.HtmlControls;

public partial class Page_SM301000 : PX.Web.UI.PXPage
{
    protected void Page_Load(object sender, EventArgs e)
    {
    }

    protected void Page_Init(object sender, EventArgs e)
    {
        this.ClientScript.RegisterClientScriptInclude(this.GetType(), "jq", VirtualPathUtility.ToAbsolute("~/Scripts/jquery-3.1.1.min.js"));
        this.ClientScript.RegisterClientScriptInclude(this.GetType(), "jqsr", VirtualPathUtility.ToAbsolute("~/Scripts/jquery.signalR-2.2.1.min.js"));
        this.ClientScript.RegisterClientScriptInclude(this.GetType(), "hb", VirtualPathUtility.ToAbsolute("~/signalr/hubs"));
    }
}
