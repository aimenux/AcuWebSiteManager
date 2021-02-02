using System;
using System.Configuration;
using System.Collections;
using System.Web;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.UI.HtmlControls;

public partial class Page_FS300100 : PX.Web.UI.PXPage
{
    protected void Page_Load(object sender, EventArgs e)
    {
    	Master.PopupHeight = 700;
        Master.PopupWidth = 1080;
    }
}
