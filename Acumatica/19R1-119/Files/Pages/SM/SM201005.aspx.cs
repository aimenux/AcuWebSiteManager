using System;
using System.Data;
using System.Configuration;
using System.Collections;
using System.Web;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.UI.WebControls.WebParts;
using System.Web.UI.HtmlControls;

public partial class Page_SM202000 : PX.Web.UI.PXPage
{
	public void Page_Load(object sender, EventArgs e)
	{
		if (PX.Data.Access.ActiveDirectoryProvider.Instance == PX.Data.Access.ActiveDirectoryProvider.Empty) 
			tab.Items["ad"].Visible = false;
	}
}
