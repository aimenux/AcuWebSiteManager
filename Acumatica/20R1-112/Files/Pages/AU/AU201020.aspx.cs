using System;
using System.Web;
using PX.Web.Customization;

public partial class Page_AU201020 : PX.Web.UI.PXPage
{
	protected void Page_PreInit(object sender, EventArgs e)
	{
		ProjectBrowserMaint.InitSessionFromQueryString(HttpContext.Current);
	}

	protected void Page_Init(object sender, EventArgs e)
	{
        this.Master.FindControl("usrCaption").Visible = false;
    }
}
