using System;
using System.Configuration;
using System.Collections;
using System.Web;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.UI.HtmlControls;

public partial class Page_GL104000 : PX.Web.UI.PXPage
{
	protected void Page_Load(object sender, EventArgs e)
	{
		PX.Web.UI.PXGrid grid = this.tab.FindControl("gridUsers") as PX.Web.UI.PXGrid;
		if (grid != null)
		{
			grid.ActionBar.Actions.EditRecord.Text = PX.Objects.GL.Messages.Membership;
			grid.ActionBar.Actions.EditRecord.Tooltip = PX.Objects.GL.Messages.ttipMembership;
		}
	}
}
