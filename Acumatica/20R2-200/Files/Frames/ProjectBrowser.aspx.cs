using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.Configuration;
using PX.Common;
using PX.SM;
using PX.Web.Customization;
using PX.Web.UI;
using PX.Web.Controls;
using PX.Data;

public partial class Frames_ProjectBrowser : PXPage
{
	#region Event handlers

	//---------------------------------------------------------------------------
	/// <summary>
	/// The page Init event handler.
	/// </summary>
	protected override void OnInit(EventArgs e)
	{
		if (!this.IsCallback)
		{
			MainFrameHelpers.RegisterMainFrameScripts(this);
		}

		this.project = this.Request.QueryString["Project"];
		if (!string.IsNullOrEmpty(this.project))
			ProjectBrowserMaint.SelectProjectByName(this.project);

		string screenID = this.Request.QueryString["EditScreenID"];
		if (!string.IsNullOrEmpty(screenID)) ProjectBrowserMaint.ContextScreenID = screenID;

		if (!PX.Common.PXContext.PXIdentity.AuthUser.IsInRole(PXAccess.GetCustomizerRole()))
		{
			Redirector.Redirect(System.Web.HttpContext.Current, string.Format("~/Frames/Error.aspx?exceptionID={0}&typeID={1}", "Access denied", "error"));
		}

		base.OnInit(e);
	}

	//---------------------------------------------------------------------------
	/// <summary>
	/// The page Load event handler.
	/// </summary>
	protected void Page_Load(object sender, EventArgs e)
	{
		if (this.IsCallback) 
			return;


		var projectBroser = (ProjectBrowserMaint)this.DefaultDataSource.DataGraph;
		var filter = projectBroser.Filter.Current;
		ProjectBrowserMaint.ContextControlID = null;
		if (filter.InitSession != true && filter.ProjectID.HasValue)
		{
			projectBroser.InitSession();
			filter.InitSession = true;
			var navigateGraph = filter.InnerScreenConfig;

			if (navigateGraph != null)
			{			
				//filter.InnerScreenConfig.Unload();
				var node = PXSiteMap.Provider.FindSiteMapNode(navigateGraph.GetType());
				this.panelF.InnerPageUrl = PXPageCache.FixPageUrl( node.Url );
				//PXContext.Session.PageInfo[PXUrl.ToAbsoluteUrl(node.Url)] = navigateGraph.GetType();

				//инициализация двумя путями. либо редиректом либо из url
				//на загрузке следует инициализировать сессию	
			
				filter.InnerScreenConfig = null;
			}
		}

		this.project = ProjectBrowserMaint.ContextProjectName;

		if (!string.IsNullOrEmpty(project)) 
			projectLink.InnerText = project;
            
		if (string.IsNullOrEmpty(panelF.InnerPageUrl)) 
			panelF.InnerPageUrl = this.GetLastUrl();

		//logoImg.ImageUrl = "~/App_Themes/" + this.StyleSheetTheme + "/Images/logo.png";
        string hideNavigationPane = PX.Web.Controls.Messages.GetLocal(PX.Web.Controls.Messages.HideNavigationPane);
        string showNavigationPane = PX.Web.Controls.Messages.GetLocal(PX.Web.Controls.Messages.ShowNavigationPane);
        hideFrameBox.Attributes["title"] = hideNavigationPane;
        Page.ClientScript.RegisterClientScriptBlock(GetType(), "hideNavigationPane", "var hideNavigationPane=\"" + hideNavigationPane + "\";", true);
        Page.ClientScript.RegisterClientScriptBlock(GetType(), "showNavigationPane", "var showNavigationPane=\"" + showNavigationPane + "\";", true);
    }
	//protected void Tree_Node_Bound(object sender, PXTreeNodeEventArgs e)
	//{
	//	if (String.IsNullOrEmpty( panelF.InnerPageUrl))
	//	{
	//		var url = e.Node.NavigateUrl ?? "";
	//		bool content = url.Contains("&") || url.Contains(" (");
	//		if (content)
	//			panelF.InnerPageUrl = url;
	//	}
	//}
	/// <summary>
	/// The page PreRenderComplete event handler.
	/// </summary>
	protected override void OnPreRenderComplete(EventArgs e)
	{
		this.ClientScript.RegisterStartupScript(this.GetType(), "hideScript", "\nvar hideScript = 1; ", true);
		this.ClientScript.RegisterStartupScript(
			this.GetType(), "project", string.Format("\nvar __projectName = '{0}'; ", this.project), true);
		base.OnPreRenderComplete(e);
	}


	public void uploadPanel_Upload(PX.Web.UI.UserControls.PXUploadFilePanel.PXFileUploadedEventArgs args)
	{
		ProjectBrowserMaint.OnUploadPackage(args.FileName, args.BinData);

	}
	#endregion

	#region Private methods

	/// <summary>
	/// Fill the specified tree control by means of descriptor.
	/// </summary>
	private void FillTreeFromDescriptor(PXTreeView tree, string[] descr)
	{
		tree.Nodes.Clear();
		foreach (string d in descr)
		{
			string[] pair = d.Split('|');
			var node = new PXTreeNode(pair[0]);
			node.NavigateUrl = pair[1];
			tree.Nodes.Add(node);
		}
	}

	/// <summary>
	/// Fill the specified tree control by means of parent screen id.
	/// </summary>
	private List<string> FillTreeFromSiteMap(PXTreeView tree, string screenID)
	{
		PXSiteMapNode node = PXSiteMap.Provider.FindSiteMapNodeByScreenID(screenID);
		var children = new List<string>();
		if (node != null) foreach (PXSiteMapNode n in node.ChildNodes)
			{
				tree.Nodes.Add(new PXTreeNode(n.Title) { NavigateUrl = n.Url });
				children.Add(n.ScreenID);
			}
		return children;
	}

	/// <summary>
	/// Calculate the start page url.
	/// </summary>
	private string GetLastUrl()
	{
		string lastUrl = (string)PXContext.Session["LastUrl"];
		string screen = Page.Request.QueryString["ScreenId"];
		string company = Page.Request.QueryString[PXUrl.CompanyID];

		if (string.IsNullOrEmpty(lastUrl) || !string.IsNullOrEmpty(screen))
		{
			string url = lastUrl; int i;
			if (!string.IsNullOrEmpty(url) && (i = url.IndexOf('?')) > 0)	
				url = url.Substring(0, i);
			
			if (string.IsNullOrEmpty(url) || !url.Replace(".aspx", "").ToUpper().EndsWith(screen.ToUpper()))
			{
				PXSiteMapNode node = (screen != null) ? PXSiteMap.Provider.FindSiteMapNodeByScreenID(screen) : null;

				string query = Page.Request.Url.Query
					.Replace("ScreenId=" + screen, String.Empty).Replace("ScreenID=" + screen, String.Empty)
					.Replace("CompanyID=" + company, String.Empty).Replace("?", String.Empty)
					.Replace("Project=" + this.project, String.Empty);

				if (node != null && !string.IsNullOrEmpty(node.Url)) lastUrl = this.ResolveUrl(node.Url);
				if(lastUrl != null)
					lastUrl = PX.Common.PXUrl.CombineParameters(lastUrl, query);
			}
		}
		return lastUrl;
	}

	#endregion

	#region Constants & variables

	private const string _automationNodeID = "AU000000";
	private const string helpKey = "helpUrl";
	private const string helpUrl = "~/Wiki/Show.aspx?pageid={0}";
	private string project;

	#endregion
}