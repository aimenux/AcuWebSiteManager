using System;
using System.Configuration;
using System.Collections;
using System.Web;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.UI.HtmlControls;

public partial class Page_SM206506 : PX.Web.UI.PXPage
{
    protected void Page_Load(object sender, EventArgs e)
    {
        Control control = this.Master.FindControl("usrCaption");
        if (control != null)
        {
            control.Visible = false;
        }
        this.Master.PopupHeight = 350;
        this.Master.PopupWidth = 500;
    }
}