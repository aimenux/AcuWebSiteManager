using System;
using System.Configuration;
using System.Collections;
using System.Web;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.UI.HtmlControls;

public partial class Page_AM306000 : PX.Web.UI.PXPage
{
    protected void Page_Load(object sender, EventArgs e)
    {
    }

    protected void Page_Init(object sender, EventArgs e)
    {
        // Required when the page is a in page pop-up panel. 
        // This makes the default size larger than the small default
        Master.PopupHeight = 750;
        Master.PopupWidth = 1000;
    }
}