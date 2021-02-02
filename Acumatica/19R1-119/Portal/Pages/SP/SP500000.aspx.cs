using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

public partial class Pages_NewComment : PX.Web.UI.PXPage
{
    protected void Page_Load(object sender, EventArgs e)
    {
    	this.Master.PopupWidth = 780;
		this.Master.PopupHeight = 640;
    }
}