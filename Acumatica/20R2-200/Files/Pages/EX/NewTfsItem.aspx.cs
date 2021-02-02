using System;
using System.Configuration;
using System.Collections;
using System.Web;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.UI.HtmlControls;
using PX.Web.Controls.KB;
using PX.Web.UI;
using PX.Data;
using PX.Data.Wiki.Parser;

public partial class  Page_NewTfsItem : PX.Web.UI.PXPage
{
	protected void Page_Load(object sender, EventArgs e)
	{
        this.Master.PopupWidth = 700;
        this.Master.PopupHeight = 600;
	}
}
