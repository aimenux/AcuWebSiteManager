using System;
using System.Globalization;
using System.Configuration;
using System.Collections;
using System.Web;
using System.Web.Compilation;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.WebControls;
using PX.Data;
using PX.SM;
using PX.SM.Alias;
using PX.Web.Controls;
using PX.Web.UI;
using System.Collections.Generic;
using SiteMap = System.Web.SiteMap;

public partial class Menu : PX.Web.UI.PXPage
{
	#region Page event handlers

	protected override void OnInit(EventArgs e)
	{				
		if (!this.IsCallback)
		{
			// set company logo
			PXResult<Branch, UploadFile> res =
				(PXResult<Branch, UploadFile>)PXSelectJoin<Branch,
					InnerJoin<UploadFile, On<Branch.logoName, Equal<UploadFile.name>>>,
					Where<Branch.branchCD, Equal<Required<Branch.branchCD>>>>.
					Select(new PXGraph(), PXAccess.GetBranchCD());
			if (res != null)
			{
				PX.SM.UploadFile file = (PX.SM.UploadFile)res;
				if (file != null)
					this.logoCell.Style[HtmlTextWriterStyle.BackgroundImage] = ControlHelper.GetAttachedFileUrl(this, file.FileID.ToString());
			}
		}
		base.OnInit(e);
	}

	protected override void OnPreRender(EventArgs e)
	{
		if (ControlHelper.IsRtlCulture())
			this.Page.Form.Style[HtmlTextWriterStyle.Direction] = "rtl";
		base.OnPreRender(e);
	}

	protected void Page_Load(object sender, EventArgs e)
	{
		List<SiteMapNode> source = new List<SiteMapNode>();
		PXWikiProvider wiki = PXSiteMap.WikiProvider;
		SiteMapNode wikiRoot = wiki.RootNode;

		if (this.IsCallback)
			this.EnableViewStateMac = false;

		this.InitSearchBox();
		foreach (SiteMapNode node in SiteMap.RootNode.ChildNodes)
		{
			PX.Data.PXSiteMapNode swNode = node as PX.Data.PXSiteMapNode;
			if (swNode == null)
			    source.Add(node);
		}

		navPanel.DataBindings.TextField = "Title";
		navPanel.DataBindings.ImageUrlField = "Description";
		navPanel.DataBindings.ContentUrlField = "Url";
		navPanel.DataSource = source;
		navPanel.DataBind();
		bindComplete = false;
	}

	/// <summary>
	/// Create the tree view control inside each band.
	/// </summary>
	protected void navPanel_BarItemDataBound(object sender, PXBarItemDataBoundEventArgs e)
	{
		if (bindComplete) return;

		INavigateUIData dataItem = e.BarItem.DataItem as INavigateUIData;
		PX.Data.PXSiteMapNode swNode = dataItem as PX.Data.PXSiteMapNode;
		int index = navPanel.Bars.IndexOf(e.BarItem);
		string req = this.Request.Params["__CALLBACKID"];
		string trId = "tree" + swNode.Key.Replace('-', '_');
		string descr = dataItem.Description;

		if (this.IsCallback &&
			!req.Contains("sp" + index) &&
			!req.Contains(trId))
			return;

		if (!string.IsNullOrEmpty(descr))
		{
			string[] im = descr.Split('|');
			e.BarItem.ImageUrl = PXImages.ResolveImageUrl(im[0], this);
			if (im.Length > 1)
			{
				if (im[1].Contains(".") || im[1].Contains("@"))
					e.BarItem.SmallImageUrl = PXImages.ResolveImageUrl(im[1], this);
				else
					e.BarItem.SmallText = im[1];
			}
		}
		e.BarItem.Tooltip = dataItem.Name;
		e.BarItem.ContentUrl = this.ResolveUrl/**/(dataItem.NavigateUrl);
		e.BarItem.Target = "main";
		PXSmartPanel panel = new PXSmartPanel();
		panel.ID = "sp" + index.ToString();
		panel.LoadOnDemand = index != 0;
		panel.AllowResize = panel.AllowMove = false;
		panel.RenderVisible = true;
		panel.Width = panel.Height = Unit.Percentage(100);

		e.BarItem.TemplateContainer.Controls.Add(panel);
		SiteMapDataSource ds = new SiteMapDataSource();
		ds.ID = "ds" + index.ToString();
		ds.StartingNodeUrl = dataItem.NavigateUrl;
		ds.ShowStartingNode = false;
		panel.Controls.Add(ds);

		Control content = null;
        if (PXSiteMap.WikiProvider.FindSiteMapNodeFromKey(swNode.NodeID) != null)
		{
			if (PXSiteMap.WikiProvider.GetWikiID(swNode.Title) != Guid.Empty || PXSiteMap.WikiProvider.GetWikiIDFromUrl(swNode.Url) != Guid.Empty)
				content = CreateWikiTree(swNode, trId);
		}
		else
		{
			content = CreateTree(ds, trId);
		}

		if (content == null)
			return;
		panel.Controls.Add(content);
	}
	
	protected void navPanel_OnDataBound(object sender, EventArgs e)
	{
		bindComplete = true;
	}
	#endregion

	#region Methods to work with sitemap tree

	private PXTreeView CreateTree(SiteMapDataSource ds, string controlName)
	{
		PXTreeView tree = new PXTreeView();
		tree.DataSourceID = ds.ID;
		tree.ID = controlName;
		tree.Width = tree.Height = Unit.Percentage(100);
		tree.Style[HtmlTextWriterStyle.BorderWidth] = Unit.Pixel(0).ToString();
		tree.ShowRootNode = false;
		tree.Target = "main";
		tree.ApplyStyleSheetSkin(this);
		tree.Style[HtmlTextWriterStyle.Position] = "absolute";
		tree.NodeDataBound += new PXTreeNodeEventHandler(tree_NodeDataBound);
		tree.ShowLines = false;
		tree.ClientEvents.NodeClick = "treeClick";
		tree.Synchronize += new PXTreeSyncEventHandler(tree_Synchronize);
		return tree;
	}

	private PXWikiTree CreateWikiTree(PX.Data.PXSiteMapNode node, string treeId)
	{
		PXWikiTree tree = new PXWikiTree();
		string url = node.Url;
		Guid wikiId = PX.Data.PXSiteMap.WikiProvider.GetWikiIDFromUrl(node.Url);
		PX.SM.WikiReader reader = new PX.SM.WikiReader();
		PX.SM.WikiDescriptor wiki = reader.wikis.SelectWindowed(0, 1, wikiId);

		tree.Provider = PX.Data.PXSiteMap.WikiProvider;
		url = PX.SM.Wiki.Url(wikiId);

		tree.ID = treeId;
		tree.WikiID = wikiId;
		tree.ShowRootNode = false;
		tree.Target = "main";
		tree.StartingNodeUrl = this.ResolveUrl/**/(url);
		tree.SearchUrl = this.ResolveUrl/**/("~/Search/Search.aspx") + "?query=";
		tree.NewArticleUrl = wiki == null || string.IsNullOrEmpty(wiki.UrlEdit) ? "" : this.ResolveUrl/**/(wiki.UrlEdit) + "?wiki=" + wikiId;
		tree.Width = tree.Height = Unit.Percentage(100);
		tree.Style[HtmlTextWriterStyle.Position] = "absolute";

		tree.ClientEvents.NodeClick = "treeClick";
		tree.Synchronize += new PXTreeSyncEventHandler(wikiTree_Synchronize);
		return tree;
	}

	void tree_Synchronize(object sender, PXTreeSyncEventArgs e)
	{
		this.PrepareSynchronizationPath(e, System.Web.SiteMap.Provider);
	}

	void wikiTree_Synchronize(object sender, PXTreeSyncEventArgs e)
	{
		this.PrepareSynchronizationPath(e, PX.Data.PXSiteMap.WikiProvider);
	}

	private void PrepareSynchronizationPath(PXTreeSyncEventArgs e, SiteMapProvider prov)
	{
		List<string> path = new List<string>();
		System.Text.StringBuilder result = new System.Text.StringBuilder();
		SiteMapNode node = prov.FindSiteMapNodeFromKey(e.SyncNodeKey);

		while (node != null && node.ParentNode != prov.RootNode)
		{
			path.Add(node.Key);
			node = node.ParentNode;
		}
		for (int i = path.Count - 1; i >= 0; i--)
		{
			result.Append(path[i]);
			result.Append('|');
		}
		if (result.Length != 0)
			result = result.Remove(result.Length - 1, 1);
		e.NodePath = result.ToString();
	}

	/// <summary>
	/// The tree node bound event handler.
	/// </summary>
	private void tree_NodeDataBound(object sender, PXTreeNodeEventArgs e)
	{
		INavigateUIData dataItem = e.Node.DataItem as INavigateUIData;

		// sets the node images
		PX.Data.PXSiteMapNode node = e.Node.DataItem as PX.Data.PXSiteMapNode;

		string descr = dataItem.Description;
		if (!string.IsNullOrEmpty(descr))
		{
			string[] im = descr.Split('|');
			e.Node.Images.Normal = this.ResolveUrl/**/(im[0]);
			if (im.Length > 1) e.Node.Images.Selected = this.ResolveUrl/**/(im[1]);
		}

		// sets the node tooltip
		e.Node.ToolTip = dataItem.Name;
		if (!string.IsNullOrEmpty(node.ScreenID)) 
			e.Node.ToolTip += string.Format(" ({0})", node.ScreenID);

		if (node != null)
		{
			if (node.ChildNodes.Count == 0 && String.IsNullOrEmpty(node.Url))
			{
				if (((PXTreeView)sender).Nodes.Contains(e.Node))
				{
					((PXTreeView)sender).Nodes.Remove(e.Node);
				}
			}
		}
	}
	#endregion

	#region Private methods

	private bool SyncAvailable
	{
		get { return btnSyncMenu.Visible; }
		set { btnSyncMenu.Visible = value; }
	}

	private int CallbackIndex
	{
		get
		{
			int callbackIndex = 0;
			string spArgs = this.navPanel.ClientID.ToString() + "$sp";
			if (this.IsCallback &&
				 this.Request.Params["__CALLBACKID"].StartsWith(spArgs))
			{
				int.TryParse(this.Request.Params["__CALLBACKID"].Substring(spArgs.Length), out callbackIndex);
			}
			if (this.IsCallback &&
				this.Request.Params["__CALLBACKID"].EndsWith("$treeHlp"))
			{
				callbackIndex = this.navPanel.Bars.Count - 1;
			}
			return callbackIndex;
		}
	}

	private void InitSearchBox()
	{
		PXSearchBox box = this.srch;
		box.SearchNavigateUrl = this.ResolveUrl/**/("~/Search/Search.aspx") + "?query=";
		box.Text = PXMessages.LocalizeNoPrefix(PX.AscxControlsMessages.SearchBox.TypeYourQueryHere);
		box.ToolTip = PXMessages.LocalizeNoPrefix(PX.AscxControlsMessages.SearchBox.SearchSystem);
		box.AddNewVisible = false;
	}

	private bool VerifyRightsOnScreen(string screenUrl)
	{
		PXSiteMapNode node = PXSiteMap.Provider.FindSiteMapNode(screenUrl) as PXSiteMapNode;
		return node == null ? false : PXAccess.VerifyRights(node.ScreenID);
	}
	#endregion
	
	#region Fields

	bool bindComplete;	
	#endregion

}
