using System;
using System.Configuration;
using System.Collections;
using System.Web;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.UI.HtmlControls;
using System.Drawing;
using PX.Data;
using PX.Objects.EP;
using PX.Objects.PM;

public partial class Page_PM308000 : PX.Web.UI.PXPage
{
    protected void Page_Load(object sender, EventArgs e)
    {        
    	this.Master.PopupWidth = 1000;
		this.Master.PopupHeight = 700;
    }
}
