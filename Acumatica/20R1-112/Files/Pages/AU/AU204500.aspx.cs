using System;
using System.Configuration;
using System.Collections;
using System.Web;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.UI.HtmlControls;
using PX.Data;

public partial class Page_AU204500 : PX.Web.UI.PXPage
{
	protected void Page_Init(object sender, EventArgs e)
	{
	    this.Master.FindControl("usrCaption").Visible = false;
		lblCaption.InnerText = PXMessages.LocalizeNoPrefix(PX.AscxControlsMessages.Messages.CustomFilesCaption);
	}
}
