using PX.Data;
using PX.Web.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

public partial class Frames_BrowserWarning : System.Web.UI.Page
{
    protected void Page_Load(object sender, EventArgs e)
    {
			this.lblUserAgent.Text = "User Agent: " + this.Request.UserAgent;

			PXLabel lbl = (PXLabel)form1.FindControl("lblMessage");
			lbl.Text = PXMessages.LocalizeFormatNoPrefix(PX.Data.ErrorMessages.SupportetBrowsers);
    }
}