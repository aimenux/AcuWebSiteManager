using System;
using System.Configuration;
using System.Collections;
using System.Web;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.UI.HtmlControls;

public partial class Page_EP203000 : PX.Web.UI.PXPage
{
    protected void Page_Load(object sender, EventArgs e)
    {
			this.Master.PopupWidth = 950;
    	this.Master.PopupHeight = 680;    	
    }
}
