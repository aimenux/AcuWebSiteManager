using System;
using System.Configuration;
using System.Collections;
using System.Web;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.UI.HtmlControls;
using PX.Web.UI;

public partial class Page_AP402000 : PX.Web.UI.PXPage
{
	protected void Page_Init(object sender, EventArgs e)
	{
		this.grid.FilterShortCuts = true;
	}

	protected void Page_Load(object sender, EventArgs e)
	{
		PXToolBar toolBar = this.ds.ToolBar;
		toolBar.Items.RemoveAt(0);
	}
}
