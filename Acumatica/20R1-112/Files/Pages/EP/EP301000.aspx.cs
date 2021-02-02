using System;
using System.Configuration;
using System.Collections;
using System.Web;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.UI.HtmlControls;
using PX.Objects.EP;
using PX.Web.UI;

public partial class Page_EP301000 : PX.Web.UI.PXPage
{
    protected void Page_Init(object sender, EventArgs e)
    {
		this.Master.PopupHeight = 700;
		this.Master.PopupWidth = 900;
	}

	protected override void OnLoad(EventArgs e)
	{
		base.OnLoad(e);
		var graph = this.ds.DataGraph as ExpenseClaimEntry;

		if (graph != null)
		{
			ExpenseClaimDetailEntry.ExpenseClaimDetailEntryExt.CheckAllowedUser(graph);
		}
	}
	
}
