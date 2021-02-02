using System;
using System.Web.UI;
using System.Web.UI.WebControls;

public partial class Page_GL401000 : PX.Web.UI.PXPage
{
	protected void Page_Init(object sender, EventArgs e)
	{
		this.grid.FilterShortCuts = true;
	}
}
