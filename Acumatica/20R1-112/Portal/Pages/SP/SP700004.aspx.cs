using System;
using PX.Web.UI;

public partial class Pages_SP700004 : PXPage
{
    protected void Page_Load(object sender, EventArgs e)
    {
        this.Master.PopupWidth = 420;
        this.Master.PopupHeight = 250;
        this.Master.FindControl("usrCaption").Visible = false;
    }
}