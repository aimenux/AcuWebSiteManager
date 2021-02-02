using System;
using PX.Web.UI;
using PX.Objects.WZ;

public partial class Page_WZ201000 : PXPage
{
    protected void Page_Init(object sender, EventArgs e)
    {

	}
	protected void grid_OnDataBound(object sender, EventArgs e)
	{
		WZScenarioEntry graph = this.ds.DataGraph as WZScenarioEntry;
		if (graph.IsSiteMapAltered)
			this.ds.CallbackResultArg = "RefreshSitemap";
	}
}