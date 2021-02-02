using System;
using System.Configuration;
using System.Collections;
using System.Web;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.UI.HtmlControls;
using PX.Data;
using PX.SM;
using PX.Web.UI;

public partial class Page_SM200570 : PXPage
{
	protected void Page_Load(object sender, EventArgs e)
	{
		PXSmartPanel panel = (PXSmartPanel)dlgUploadFile.FindControl(dlgUploadFile.PanelID);
		if(panel != null) panel.Key = "UploadVersionPanel";
		Control grid = this.tab.FindControl("gridAvailableVersions");
		if (!this.Page.IsCallback)
		{
			this.Page.ClientScript.RegisterClientScriptBlock(GetType(), "gridID", "var gridRevisionsID=\"" + grid.ClientID + "\";", true);
			this.Page.ClientScript.RegisterClientScriptBlock(GetType(), "pnlNewRevID", "var pnlNewRevID=\"" + panel.ClientID + "\";", true);
		}
	}
	protected void OnFileUploadFinished(PX.Web.UI.UserControls.PXUploadFilePanel.PXFileUploadedEventArgs e)
	{
		UpdateMaint graph = (UpdateMaint)this.ds.DataGraph;
		try
		{
			graph.OnPackageUploaded(e.FileName, e.Password, e.BinData);
		}
		catch (PXException ex)
		{
			this.ClientScript.RegisterClientScriptBlock(this.GetType(), "uploadErr", "window.uploadErr = \"Error during file upload: " + ex.MessageNoPrefix.Replace('"', '\'') + "\";", true);
		}
	}
}
