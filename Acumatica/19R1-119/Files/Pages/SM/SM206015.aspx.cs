using System;
using System.Configuration;
using System.Collections;
using System.Web;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.UI.HtmlControls;
using PX.Web.UI;
using PX.Data;

public partial class Page_SM206015 : PX.Web.UI.PXPage
{
	protected void Page_Load(object sender, EventArgs e)
	{
		if (!this.Page.IsCallback)
		{
		    var sp1 = tab.FindControl("sp1") as PXSplitContainer;
            PXGrid grLeft = sp1.FindControl("gridObjects") as PXGrid;
            PXGrid grRight = sp1.FindControl("gridFields") as PXGrid;

			this.Page.ClientScript.RegisterClientScriptBlock(GetType(), "vartab", "var tabId = '" + tab.ClientID + "';", true);
			this.Page.ClientScript.RegisterClientScriptBlock(GetType(), "varGridLeft", "var grLeft = '" + grLeft.ClientID + "';", true);
			this.Page.ClientScript.RegisterClientScriptBlock(GetType(), "varGridRight", "var grRight = '" + grRight.ClientID + "';", true);

            Control grid = this.tab.FindControl("grid");
            this.Page.ClientScript.RegisterClientScriptBlock(GetType(), "gridID", "var gridRevisionsID=\"" + grid.ClientID + "\";", true);
            this.Page.ClientScript.RegisterClientScriptBlock(GetType(), "dsID", "var dsID=\"" + this.ds.ClientID + "\";", true);            

			Page.ClientScript.RegisterClientScriptBlock(GetType(), "formID", "var formID = '" + form.ClientID + "';", true);
		}
	}

	protected void grid_RowDataBound(object sender, PXGridRowEventArgs e)
	{
		if (e.Row != null && e.Row.DataItem != null)
		{
			PXStringState state = this.ds.DataGraph.Caches[e.Row.DataItem.GetType()].GetStateExt(e.Row.DataItem, "Value") as PXStringState;
			if (state != null && state.InputMask == "*")
				e.Row.Cells["Value"].IsPassword = true;
		}
	}
}
