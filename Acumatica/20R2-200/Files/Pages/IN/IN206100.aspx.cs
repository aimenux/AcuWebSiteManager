using System;
using System.Configuration;
using System.Collections;
using System.Web;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.UI.HtmlControls;
using PX.Data;
using PX.Data.Wiki.Parser;
using PX.Objects.CR;
using PX.Web.Controls.KB;
using PX.Web.UI;
using PX.Web.Controls;

public partial class Page_IN206100 : PX.Web.UI.PXPage
{
	protected void Page_Init(object sender, EventArgs e)
	{
		this.Master.PopupHeight = 700;
		this.Master.PopupWidth = 900;
	}
}
