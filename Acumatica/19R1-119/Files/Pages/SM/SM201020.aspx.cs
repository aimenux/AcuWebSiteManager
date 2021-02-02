using System;
using System.Collections.Generic;
using System.Configuration;
using System.Collections;
using System.Web;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.WebControls;
using PX.Web.UI;

public partial class Page_SM200000 : PX.Web.UI.PXPage
{
	protected void Page_Init(object sender, EventArgs e)
	{
	}

	protected void Page_Load(object sender, EventArgs e)
	{
		var tree = sp1.FindControl("tree");
		tree.DataBind();
	}

	protected void tree_DataBinding(object sender, EventArgs e)
	{
		if (!ControlHelper.IsReloadPage(this)) return;

		string screen = this.Request.QueryString.Get("Screen");
		if (!string.IsNullOrEmpty(screen))
		{
			List<string> path = new List<string>();
			SiteMapNode node = System.Web.SiteMap.Provider.FindSiteMapNodeFromKey(screen);

			while (node != null)
			{
				path.Add(node.Key);
				node = node.ParentNode;
			}

			path.Reverse();
			((PXTreeView)sender).SetTreePaths(path.ToArray(), null);
		}
	}
}
