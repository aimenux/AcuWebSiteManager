using System;
using System.Configuration;
using System.Collections;
using System.Web;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.UI.HtmlControls;
using PX.Common;
using PX.Data;
using PX.SM;
using PX.Web.UI;

public partial class Page_SM200575 : PXPage
{
	protected void Page_Load(object sender, EventArgs e)
	{
		if (!this.Page.IsCallback) this.Page.ClientScript.RegisterHiddenField("__FORCELOGOUT", "1");
		this.Master.PopupWidth = 960;
		this.Master.PopupHeight = 700;
	}

	protected void OnFileUploadFinished(PX.Web.UI.UserControls.PXUploadFilePanel.PXFileUploadedEventArgs e)
	{
		CompanyMaint graph = (CompanyMaint)this.ds.DataGraph;
		try
		{
			graph.OnPackageUploaded(e.FileName, e.Password, e.BinData);
		}
		catch (PXException ex)
		{
			this.ClientScript.RegisterClientScriptBlock(this.GetType(), "uploadErr", "window.uploadErr = \"Error during file upload: " + ex.MessageNoPrefix.Replace('"', '\'') + "\";", true);
			throw;
		}
	}
}
