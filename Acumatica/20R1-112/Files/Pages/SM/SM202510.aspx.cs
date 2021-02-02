using System;
using System.Web.UI;
using PX.Export.Excel.Core;
using PX.SM;
using PX.Web.UI;
using PX.Data;
using PX.Export.Excel;

public partial class Page_SM210000 : PX.Web.UI.PXPage
{
	private const string _FILES_CONF_KEY = "FilesPath";

	private static readonly string _filesPath;

	private PXFileEditButton _editButton;

	protected PXSelector selector;

	static Page_SM210000()
	{
		_filesPath = System.Configuration.ConfigurationManager.AppSettings[_FILES_CONF_KEY] ?? string.Empty;
	}

	protected void Page_Init(object sender, EventArgs e)
	{




		pnlNewRev.FileUploadFinished += new PXFileUploadEventHandler(pnlNewRev_FileUploadFinished);
		WikiFileMaintenance graph = (WikiFileMaintenance)ds.DataGraph;
		string authority = Request.GetWebsiteAuthority().GetLeftPart(UriPartial.Authority);
		string schema = Request.GetWebsiteAuthority().GetLeftPart(UriPartial.Scheme);
		graph.GetFileAddress = authority + ResolveUrl("~/Frames/GetFile.ashx");
		graph.WebDAVFilePrefix = ("https://" + authority.Substring(schema.Length)) + (string.IsNullOrEmpty(_filesPath) ? string.Empty : ResolveUrl(_filesPath));
		graph.GetDavFilePath = GetFilePath;
		_editButton = new PXFileEditButton();
		_editButton.CommandName = "edit";
		_editButton.Text = "Edit";
        _editButton.NavigateUrl = "about:blank";
		_editButton.WordImage = Sprite.Main.GetFullUrl(Sprite.Main.Doc);
		_editButton.ExcelImage = Sprite.Main.GetFullUrl(Sprite.Main.Excel);
		_editButton.PowerPointImage = Sprite.Main.GetFullUrl(Sprite.Main.Ppt);
		_editButton.Enabled = false;
		_editButton.Visible = false;
		_editButton.CollectParams = CollectParams.AnyCallback;
		int idx = ds.ToolBar.Items.Count - 3;
		PXToolBarItem oldButton = ds.ToolBar.Items["edit"];
		if (oldButton != null)
		{
			idx = ds.ToolBar.Items.IndexOf(oldButton);
			ds.ToolBar.Items.Remove(oldButton);
		}
		ds.ToolBar.Items.Insert(idx, _editButton);
		ds.PreRender += ds_PreRender;
		form.DataBound += form_DataBound;
	}

	protected void Page_Load(object sender, EventArgs e)
	{
		Control grid = this.tab.FindControl("gridRevisions");
		if (!this.Page.IsCallback)
		{
			this.Page.ClientScript.RegisterClientScriptBlock(GetType(), "gridID", "var gridRevisionsID=\"" + grid.ClientID + "\";", true);
			this.Page.ClientScript.RegisterClientScriptBlock(GetType(), "pnlNewRevID", "var pnlNewRevID=\"" + this.pnlNewRev.ClientID + "\";", true);
			this.Page.ClientScript.RegisterClientScriptBlock(GetType(), "dsID", "var dsID=\"" + this.ds.ClientID + "\";", true);
		}
		PXLabel lbl = this.tab.FindControl("lblAccessRights") as PXLabel;
		if (lbl != null)
			lbl.Text = ActionsMessages.AccessRights;
	}

	protected void pnlNewRev_FileUploadFinished(object sender, PXFileUploadEventArgs e)
	{
		WikiFileMaintenance graph = (WikiFileMaintenance) ds.DataGraph;		
		try
		{
			if(e.UploadedFile.BinData.Length > 0)
				graph.NewRevision(e.UploadedFile, this.pnlNewRev.CheckIn);
		}
		catch (PXException ex)
		{
			this.ClientScript.RegisterClientScriptBlock(this.GetType(), "uploadErr", "window.uploadErr = \"Error during file upload: " + ex.MessageNoPrefix.Replace('"', '\'') + "\";", true);
		}
	}	

	private string GetFilePath(Guid? fileID)
	{
		return WikiFileProvider.GetFilePath((WikiFileMaintenance)ds.DataGraph, fileID);
	}

	private void ds_PreRender(object sender, EventArgs e)
	{
		form.DataBind();
	}

	private void form_DataBound(object sender, EventArgs e)
	{
		WikiFileMaintenance graph = (WikiFileMaintenance)ds.DataGraph;
        _editButton.NavigateUrl = "about:blank";
		_editButton.Enabled = false;
		_editButton.Visible = false;
		if (graph.Files.Current != null && graph.Files.Current.FileID != null)
		{
			string ext = Utils.GetExtansion(graph.Files.Current.Name);
			if (string.Equals(ext, "doc", StringComparison.OrdinalIgnoreCase) ||
				string.Equals(ext, "docx", StringComparison.OrdinalIgnoreCase) ||
				string.Equals(ext, "xls", StringComparison.OrdinalIgnoreCase) ||
				string.Equals(ext, "xlsx", StringComparison.OrdinalIgnoreCase) ||
				string.Equals(ext, "ppt", StringComparison.OrdinalIgnoreCase) ||
				string.Equals(ext, "pptx", StringComparison.OrdinalIgnoreCase))
			{
				string filepath = GetFilePath(graph.Files.Current.FileID);
				_editButton.Enabled = true;
				_editButton.Visible = true;
				_editButton.NavigateUrl = Utils.CombinePaths(graph.WebDAVFilePrefix, filepath == null ? graph.Files.Current.Name : filepath);
			}
		}
	}

	public override void ProcessRequest(System.Web.HttpContext context)
	{
		string fileId = context.Request.QueryString["fileId"];
		PXBlobStorageUtils.OnBeforeEditFile(fileId);

		base.ProcessRequest(context);		
	}
}
