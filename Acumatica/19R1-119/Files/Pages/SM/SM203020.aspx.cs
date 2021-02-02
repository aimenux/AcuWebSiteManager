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
using PX.Common;
using PX.Data;
using PX.Web.UI;

public partial class Page_SM207010 : PX.Web.UI.PXPage
{
	protected void Page_Load(object sender, EventArgs e)
	{
		PXToolBar toolBar = this.ds.ToolBar;
		PXToolBarButton save = null;
		foreach (PXToolBarButton button in toolBar.Items)
		{
			if (button.CommandName == "SaveFavorites")
			{
				save = button;
				break;
			}
		}
		if (save != null)
		{
			save.AutoCallBack.Enabled = true;
			save.AutoCallBack.Command = "Save";
			save.AutoCallBack.Behavior.RepaintControlsIDs = "ds,grid";
			save.AutoCallBack.Behavior.CommitChanges = true;
			toolBar.CallBack += new PXCallBackEventHandler(toolBar_CallBack);
			toolBar.CallBackHandler = "btnSaveClick";
		}
	}

	/// <summary>
	/// The data source toolbar callback event handler.
	/// </summary>
	void toolBar_CallBack(object sender, PXCallBackEventArgs e)
	{
		var grid = sp1.FindControl("grid") as PXGrid;
		if (e.Command.Name == "Save")
		{
			PX.SM.FavoritesMaintenance fv = ds.DataGraph as PX.SM.FavoritesMaintenance;
			PXAdapter adapter = new PXAdapter(fv.Views[ds.PrimaryView]);
			adapter.SortColumns = null;
			adapter.Descendings = null;
			adapter.Searches = null;
			adapter.Filters = null;
			adapter.StartRow = 0;
			adapter.MaximumRows = grid.PageSize;
			adapter.TotalRequired = true;
			foreach (object r in fv.SaveFavorites.Press(adapter)) { break; }
			PXSiteMap.FavoritesProvider.Clear();
			e.Result = PXSiteMap.FavoritesProvider.FavoritesExists() ? "1" : "0";
			PXContext.Session.FavoritesExists["FavoritesExists"] = null;
		}
	}

	/// <summary>
	/// The grid and tree toolbar callback event handler.
	/// </summary>
	protected void tbCommand_CallBack(object sender, PX.Web.UI.PXCallBackEventArgs e)
	{
		var tree = sp1.FindControl("tree") as PXTreeView;
		var grid = sp1.FindControl("grid") as PXGrid;		

		PX.SM.FavoritesMaintenance fv = ds.DataGraph as PX.SM.FavoritesMaintenance;
		Guid selectedNode;
		if (PX.Common.GUID.TryParse(tree.SelectedValue, out selectedNode))
		{
			PXAdapter adapter = new PXAdapter(fv.Views[ds.PrimaryView]);
			adapter.SortColumns = null;
			adapter.Descendings = null;
			adapter.Parameters = new object[] { selectedNode, grid.DataValues["NodeID"] };
			adapter.Searches = null;
			adapter.Filters = null;
			adapter.StartRow = 0;
			adapter.MaximumRows = grid.PageSize;
			adapter.TotalRequired = true;
			switch (e.Command.Name)
			{
				case "left":
					foreach (object r in fv.RowLeft.Press(adapter)) { break; }
					break;
				case "right":
					foreach (object r in fv.RowRight.Press(adapter)) { break; }
					break;
				case "up":
					foreach (object r in fv.RowUp.Press(adapter)) { break; }
					break;
				case "down":
					foreach (object r in fv.RowDown.Press(adapter)) { break; }
					break;
			}
			e.Result = "1";
		}
	}

	protected string NodeLeft()
	{
		var tree = sp1.FindControl("tree") as PXTreeView;
		if (tree.SelectedNode == null) return "";

		if (tree.SelectedNode.Parent == null || string.IsNullOrEmpty(tree.SelectedNode.Parent.Value))
			return "";

		// do not put on the same level as root Help item
		if (tree.SelectedNode.Parent.Parent == null || string.IsNullOrEmpty(tree.SelectedNode.Parent.Parent.Value)) 
			return "";

		return tree.SelectedNode.Parent.Value;
	}

	protected string NodeRight()
	{
		var tree = sp1.FindControl("tree") as PXTreeView;
		if (tree.SelectedNode == null) return "";
		if (tree.SelectedNode.PrevSibling == null) return "";

		return tree.SelectedNode.PrevSibling.Value;
	}

	protected string NodeCurrent()
	{
		var tree = sp1.FindControl("tree") as PXTreeView;
		return tree.SelectedValue;
	}
}
