using System;

public partial class Page_AU210020 : PX.Web.UI.PXPage
{
	protected void Page_Load(object sender, EventArgs e)
	{
		this.Master.FindControl("usrCaption").Visible = false;
	}
}
