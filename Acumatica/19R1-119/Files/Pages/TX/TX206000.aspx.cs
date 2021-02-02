using System;
using PX.Web.UI;

public partial class Page_TX206000 : PX.Web.UI.PXPage
{
	protected void Page_Load(object sender, EventArgs e)
	{
		PXTab tab = this.PXTab1; 
		this.Page.ClientScript.RegisterClientScriptBlock(GetType(), "tab1ID", "var tab1ID=\"" + tab.ClientID + "\";", true);			
	}
}
