using System;
using System.Configuration;
using System.Collections;
using System.Web;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.UI.HtmlControls;
using PX.Web.UI;

public partial class Page_CO409061 : PX.Web.UI.PXPage
{
	protected void Page_Init(object sender, EventArgs e)
	{
		this.gridAnnouncements.FilterShortCuts = true;
		this.gridAnnouncements.ActionBar.Actions.AddNew.Enabled = false;
		this.gridAnnouncements.ActionBar.Actions.Delete.Enabled = false;
	}
}
