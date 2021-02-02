using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using PX.Dashboards;
using PX.Data.Maintenance.GI;
using PX.Web.UI;

public partial class Page_SM208600 : PXPage
{
    protected void Page_Load(object sender, EventArgs e)
    {

    }

	protected void frmHeader_DataBound(object sender, EventArgs e)
	{
		DashboardMaint graph = (DashboardMaint) this.ds.DataGraph;
		if (graph.IsSiteMapAltered)
			this.ds.CallbackResultArg = "RefreshSitemap";
	}

	protected void grd_EditorsCreated_RelativeDates(object sender, EventArgs e)
	{
		PXGrid grid = sender as PXGrid;
		if (grid != null)
		{
			PXDateTimeEdit de = grid.PrimaryLevel.GetStandardEditor(GridStandardEditor.Date) as PXDateTimeEdit;
			if (de != null)
			{
				de.ShowRelativeDates = true;
			}
		}
	}
}