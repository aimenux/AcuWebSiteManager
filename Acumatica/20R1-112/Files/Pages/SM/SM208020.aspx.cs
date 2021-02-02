using System;
using PX.Web.UI;
using PX.Olap.Maintenance;

public partial class Page_SM208020 : PX.Web.UI.PXPage
{
	protected void Page_Load(object sender, EventArgs e)
	{
	}

	protected void gridProps_RowDataBound(object sender, PXGridRowEventArgs e)
	{
		if ((string)e.Row.DataKey.Value == "Expression") e.Row.EditorID = "edFormula";
	}
}