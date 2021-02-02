using System;
using System.Configuration;
using System.Collections;
using System.Web;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.UI.HtmlControls;
using PX.Web.UI;

public partial class Page_GL203500 : PX.Web.UI.PXPage
{
	protected void Page_Load(object sender, EventArgs e)
	{
		if (this.Page.IsCallback) return;

		PXGrid grid = this.tab.FindControl("grid") as PXGrid;
		PXGrid grid2 = this.tab.FindControl("grid2") as PXGrid;

		grid.ActionBar.Actions.EditRecord.Enabled = true;
		grid2.ActionBar.Actions.EditRecord.Enabled = true;

		grid.ActionBar.Actions.EditRecord.Text = PX.Data.PXMessages.LocalizeNoPrefix(PX.Objects.GL.Messages.ViewBatch);
		grid.ActionBar.Actions.EditRecord.Tooltip = PX.Data.PXMessages.LocalizeNoPrefix(PX.Objects.GL.Messages.ttipViewBatch);
		grid2.ActionBar.Actions.EditRecord.Text = PX.Data.PXMessages.LocalizeNoPrefix(PX.Objects.GL.Messages.ViewBatch);
		grid2.ActionBar.Actions.EditRecord.Tooltip = PX.Data.PXMessages.LocalizeNoPrefix(PX.Objects.GL.Messages.ttipViewBatch);
	}
}
