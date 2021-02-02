using System;
using System.Data;
using System.Configuration;
using System.Collections;
using System.Web;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.UI.WebControls.WebParts;
using System.Web.UI.HtmlControls;
using PX.Web.UI;
using PX.Data;

public partial class Page_SM204000 : PX.Web.UI.PXPage
{
	protected void Page_Load(object sender, EventArgs e)
	{
        var tree = sp1.FindControl("tree") as PXTreeView;
        this.ClientScript.RegisterClientScriptBlock(GetType(), "varreg", "var treeId='" + tree.ClientID + "';", true);
	}
	
	protected void tree_NodeDataBound(object sender, PXTreeNodeEventArgs e)
	{
		e.Node.NavigateUrl = "";
		e.Node.Value = ((SiteMapNode)e.Node.DataItem).Key;
	}

	protected void PXToolBar1_CallBack(object sender, PXCallBackEventArgs e)
	{
        var tree = sp1.FindControl("tree") as PXTreeView;
        string nodeToPopulate = "";
		tree.DataBind();
		if (e.Command.Name == "left")
			nodeToPopulate = NodeLeft();
		else if (e.Command.Name == "right")
			nodeToPopulate = NodeRight();
		else if (e.Command.Name == "paste")
			nodeToPopulate = PasteArticles();

		e.Result = tree.ClientID + "|" + nodeToPopulate;
		PXSiteMap.WikiProvider.Clear();
	}

	protected string PasteArticles()
	{
        var tree = sp1.FindControl("tree") as PXTreeView;
		string datapath = tree.SelectedValue;
		PX.SM.WikiPageMapMaintenance graph = this.ds.DataGraph as PX.SM.WikiPageMapMaintenance;
		if (graph == null)
			return datapath;
		
		Guid pasteid = PX.Common.GUID.CreateGuid(datapath).GetValueOrDefault();
		// check whether user is not trying to insert one of articles inside itself
		foreach (PX.SM.WikiPage item in graph.selectedArticles)
		{
			if (item.PageID == pasteid || IsPastingToChild(item.PageID.Value)) // trying to paste inside of ourselves?
				return datapath + "|" + "The destination folder is a subfolder of the source folder or folder's child.";
			graph.SetParentId(item, pasteid);
		}
		graph.Children.Cache.IsDirty = false;
		graph.Children.Cache.Clear();
		graph.Children.Cache.IsDirty = false;
		graph.selectedArticles.Clear();
		return datapath;
	}

	private bool IsPastingToChild(Guid checkId)
	{
        var tree = sp1.FindControl("tree") as PXTreeView;
		string checkdatapath = checkId.ToString();
		PXTreeNode pastenode = tree.SelectedNode;
		while (pastenode.Parent != null)
		{
			if (pastenode.Parent.DataPath == checkdatapath)
				return true;
			pastenode = pastenode.Parent;
		}
		return false;
	}

	protected void grid_CallBack(object sender, PXCallBackEventArgs e)
	{
        var tree = sp1.FindControl("tree") as PXTreeView;
        var grid = sp1.FindControl("grid") as PXGrid;
		string moveGridCursor = "";
		PX.SM.WikiPageMapMaintenance graph = this.ds.DataGraph as PX.SM.WikiPageMapMaintenance;
		if (graph == null)
			return;

		if (e.Command.Name == "RowDown")
		{
			MoveRow(graph, true);
			moveGridCursor = "|" + grid.ClientID + "|down";
		}
		else if (e.Command.Name == "RowUp")
		{
			MoveRow(graph, false);
			moveGridCursor = "|" + grid.ClientID + "|up";
		}

		e.Result = tree.ClientID + "|" + tree.SelectedValue + moveGridCursor;
		PXSiteMap.WikiProvider.Clear();
	}

	protected string NodeLeft()
	{
        var tree = sp1.FindControl("tree") as PXTreeView;
		if (tree.SelectedNode == null)
			return "";
		if (tree.SelectedNode.Level <= 1) // do not put on the same level as root Wiki items
			return "";

		Guid nodeId = PX.Common.GUID.CreateGuid(tree.SelectedValue).GetValueOrDefault();
		Guid parentId = PX.Common.GUID.CreateGuid(tree.SelectedNode.Parent.Parent.DataPath).GetValueOrDefault();
		SetParentId(nodeId, parentId);
		return tree.SelectedNode.Parent.DataPath;
	}

	protected string NodeRight()
	{
        var tree = sp1.FindControl("tree") as PXTreeView;
		if (tree.SelectedNode == null)
			return "";
		if (tree.SelectedNode.Level == 0) // do not allow root wikis to move right
			return "";

		Guid nodeId = PX.Common.GUID.CreateGuid(tree.SelectedValue).GetValueOrDefault();
		if (tree.SelectedNode.PrevSibling == null)
			return "";
		Guid prevNodeId = PX.Common.GUID.CreateGuid(tree.SelectedNode.PrevSibling.DataPath).GetValueOrDefault();
		SetParentId(nodeId, prevNodeId);
		return tree.SelectedNode.PrevSibling.DataPath;
	}

	private void MoveRow(PX.SM.WikiPageMapMaintenance graph, bool moveDown)
	{
        var tree = sp1.FindControl("tree") as PXTreeView;
        var grid = sp1.FindControl("grid") as PXGrid;
		PXAdapter adapter = new PXAdapter(graph.Views[ds.PrimaryView]);
		adapter.SortColumns = null;
		adapter.Descendings = null;
		adapter.Parameters = new object[] { tree.SelectedValue, grid.DataValues["PageID"] };
		adapter.Searches = null;
		adapter.Filters = null;
		adapter.StartRow = 0;
		adapter.MaximumRows = grid.PageSize;
		adapter.TotalRequired = true;

		foreach (PX.SM.WikiPage map in graph.Children.Select(PX.Common.GUID.CreateGuid(tree.SelectedValue)))
		{
			if (map.PageID.Value == ((grid.DataValues["PageID"] as Guid?) ?? PX.Common.GUID.CreateGuid(grid.DataValues["PageID"].ToString())))
			{
				graph.Children.Current = map;
				break;
			}
		}

		if (moveDown)
			foreach (PX.SM.WikiPage item in graph.RowDown.Press(adapter)) { break; }
		else
			foreach (PX.SM.WikiPage item in graph.RowUp.Press(adapter)) { break; }
		graph.Children.Cache.IsDirty = false;
	}

	protected void SetParentId(Guid nodeId, Guid parentId)
	{
		PX.SM.WikiPageMapMaintenance graph = this.ds.DataGraph as PX.SM.WikiPageMapMaintenance;
		graph.SetParentId(nodeId, parentId);
	}

	protected void tlbMain_CallBack(object sender, PXCallBackEventArgs e)
	{
		ds.DataGraph.Persist();
		ds.DataGraph.Caches[typeof(PX.SM.SiteMap)].IsDirty = false;
	}
}
