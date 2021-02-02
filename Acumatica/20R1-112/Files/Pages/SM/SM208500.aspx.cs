using PX.Data;
using System;
using System.Web.UI;
using System.Web.UI.WebControls;

public partial class Page_SM208500 : PX.Web.UI.PXPage
{
	protected void Page_Init(object sender, EventArgs e)
	{
	}

	protected void grid_OnDataBound(object sender, EventArgs e)
	{
		LEPMaint graph = this.ds.DataGraph as LEPMaint;
		if (graph.IsSiteMapAltered)
			this.ds.CallbackResultArg = "RefreshSitemap";
	}
}
