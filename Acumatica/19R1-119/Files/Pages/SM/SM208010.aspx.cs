using System;
using System.Collections.Generic;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using PX.Web.UI;
using PX.Olap.Maintenance;

public partial class Page_SM208010 : PX.Web.UI.PXPage
{
	protected void Page_Load(object sender, EventArgs e)
	{
	}

	protected void gridProps_RowDataBound(object sender, PXGridRowEventArgs e)
	{
		if ((string)e.Row.DataKey.Value == "Expression") e.Row.EditorID = "edFormula";
	}

	protected void form_DataBound(object sender, EventArgs e)
	{
		PivotMaint graph = this.ds.DataGraph as PivotMaint;
		if (graph.IsSiteMapAltered)
			this.ds.CallbackResultArg = "RefreshSitemap";
	}
}