using System;
using System.Configuration;
using System.Collections;
using System.Web;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.UI.HtmlControls;
using PX.Data;
using PX.Objects.CS;
using PX.Web.UI;

public partial class Page_AP503000 : PX.Web.UI.PXPage
{
	protected void Page_Init(object sender, EventArgs e)
	{
		if (PXAccess.FeatureInstalled<FeaturesSet.construction>())
		{
			PXGrid grid = tab.FindControl("grid1") as PXGrid;

			grid.SyncPosition = true;
		}
	}
}
