using System;
using System.Web.UI;
using System.Web.UI.WebControls;
using PX.Web.UI;

public partial class Page_SM203530 : PX.Web.UI.PXPage
{
	protected void Page_Init(object sender, EventArgs e)
	{
	}

	protected void Page_Load(object sender, EventArgs e)
	{
		if (!this.Page.IsCallback)
		{
			//Register variable to perform user friendly grid row selecting
			PXGrid grid = this.gridCompanies;
			this.Page.ClientScript.RegisterClientScriptBlock(GetType(), "gridCompaniesID", "var grdCompaniesID=\"" + grid.ClientID + "\";", true);
			this.Page.ClientScript.RegisterHiddenField("__FORCELOGOUT", "1");
		}
	}
}
