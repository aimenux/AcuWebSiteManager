using System;

public partial class Page_CR401000 : PX.Web.UI.PXPage
{
	protected void Page_Init(object sender, EventArgs e)
	{
		this.grid.FilterShortCuts = true;
	}
}
