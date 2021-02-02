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
using PortalMap = PX.SM.PortalMap;

public partial class Page_SM200521 : PX.Web.UI.PXPage
{
	protected void Page_Init(object sender, EventArgs e)
	{
		var tree = sp1.FindControl("tree") as PXTreeView;
		tree.NodeDataBound += new PXTreeNodeEventHandler(tree_NodeDataBound);
	}

	protected void Page_Load(object sender, EventArgs e)
	{
		var tree = sp1.FindControl("tree") as PXTreeView;
		var grid = sp1.FindControl("grid") as PXGrid;
		this.Page.ClientScript.RegisterClientScriptBlock(GetType(), "varreg", "var treeId='" + tree.ClientID + "';", true);
		this.Page.ClientScript.RegisterClientScriptBlock(GetType(), "gridID", "var gridID=\"" + grid.ClientID + "\";", true);
	}

	private void tree_NodeDataBound(object sender, PXTreeNodeEventArgs e)
	{
		PX.SM.PortalMap dataItem = e.Node.DataItem as PX.SM.PortalMap;

		string descr = dataItem.Icon;
		if (!string.IsNullOrEmpty(descr))
		{
			string[] im = descr.Split('|');
			e.Node.Images.Normal = this.ResolveUrl/**/(im[0]);
			if (im.Length > 1 && (im[1].Contains(".") || im[1].Contains("@"))) 
				e.Node.Images.Selected = this.ResolveUrl/**/(im[1]);
		}
		else
		{
			if (dataItem != null && dataItem.IsFolder == null)
			{
				e.Node.Images.Normal = Sprite.Tree.GetFullUrl(Sprite.Tree.Folder);
				e.Node.Images.Selected = Sprite.Tree.GetFullUrl(Sprite.Tree.FolderS);
			}
		}
	}

}
