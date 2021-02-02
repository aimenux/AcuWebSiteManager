using System;
using System.Configuration;
using System.Collections;
using System.Web;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.UI.HtmlControls;
using PX.Common;

public partial class Page_AR505500 : PX.Web.UI.PXPage
{
	protected void Page_Init(object sender, EventArgs e)
	{
		if(!this.IsPostBack)
			PXContext.Session.PageInfo[PXUrl.ToAbsoluteUrl(Request.Path)] = null;
	}
}
