using System;
using System.Configuration;
using System.Collections;
using System.Web;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.UI.HtmlControls;
using PX.SM;
using PX.Web.UI;

public partial class Page_SM205020 : PX.Web.UI.PXPage
{
	protected void Page_Init(object sender, EventArgs e)
	{
		this.Master.PopupHeight = 620;
		this.Master.PopupWidth = 920;
    }

    protected void Page_Load(object sender, EventArgs e)
    {
        
    }
}
