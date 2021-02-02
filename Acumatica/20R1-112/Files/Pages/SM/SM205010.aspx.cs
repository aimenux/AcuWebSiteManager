using System;
using System.Configuration;
using System.Collections;
using System.Web;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.UI.HtmlControls;

public partial class Page_SM205010 : PX.Web.UI.PXPage
{
	protected void Page_Load(object sender, EventArgs e)
	{
		if (!this.Page.IsCallback)
		{
			PX.Web.UI.PXTextEdit editor = this.frmViewXml.FindControl("edDetailsXml") as PX.Web.UI.PXTextEdit;
			this.Page.ClientScript.RegisterClientScriptBlock(GetType(), "pnlViewXmlID", "var pnlViewXmlID=\"" + this.pnlViewXml.ClientID + "\";", true);
			this.Page.ClientScript.RegisterClientScriptBlock(GetType(), "frmID", "var frmID=\"" + this.frmViewXml.ClientID + "\";", true);
			this.Page.ClientScript.RegisterClientScriptBlock(GetType(), "txtID", "var txtID=\"" + editor.ClientID + "\";", true);
		}
	}
}
