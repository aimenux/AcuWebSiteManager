using System;
using System.Collections.Generic;
using System.Reflection;
using System.Web.UI.WebControls;
using PX.Common;
using PX.Data;
using PX.SM;
using PX.Web.Controls.Wiki;
using PX.Web.UI;

public partial class Page_Show : PXPage
{
	protected void Page_Init(object sender, EventArgs e)
	{
		ShowRouter.Instance.TryRedirect(Request, Context);
	}
}
