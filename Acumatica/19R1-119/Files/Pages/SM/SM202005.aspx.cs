using System;
using System.Collections;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using PX.Data;
using PX.SM;
using PX.Web.Controls;
using PX.Web.UI;

public partial class Page_WikiSet : PX.Web.UI.PXPage
{
	private PXTabItem webSitePathsTabItem;

	protected void Page_Init(object sender, EventArgs e)
	{
		this.webSitePathsTabItem = this.tab.Items[4];

		//find import button
		for (int i = 0; i < ds.ToolBar.Items.Count; i++)
		{
			PXToolBarButton TemplateControl = ds.ToolBar.Items[i] as PXToolBarButton;
			if (TemplateControl != null)
			{
				if (TemplateControl.Text == "DITA")
					for (int j = 0; j < TemplateControl.MenuItems.Count; j++)
					{
						if (TemplateControl.MenuItems[j].CommandName == "dITA@Import")
							TemplateControl.MenuItems[j].PopupPanel = "pnlUploadFileSmart_DITA";
					}
			}
		}
		

	}

	public void uploadPanel_Upload(PX.Web.UI.UserControls.PXUploadFilePanel.PXFileUploadedEventArgs args)
	{
		PXAdapter adapter = new PXAdapter(this.ds.DataGraph.Views[ds.PrimaryView]);
		adapter.Parameters = new object[] { args.BinData, args.FileName };
		IEnumerator iterator = ds.DataGraph.Actions["ImportToDITA"].Press(adapter).GetEnumerator();
		while(iterator.MoveNext()) { }
	}

	protected void Page_Load(object sender, EventArgs e)
	{
		this.Page.ClientScript.RegisterClientScriptBlock(GetType(), "catGridID", "var catGridID=\"" + tab.ClientID + "_t1_gridCategories\";", true);
		this.Page.ClientScript.RegisterClientScriptBlock(GetType(), "kbCatGridID", "var kbCatGridID=\"" + tab.ClientID + "_t2_gridKBCategories\";", true);

		this.form.DataBound += new EventHandler(form_DataBound);
	}

	void form_DataBound(object sender, EventArgs e)
	{
		Type cachetype = typeof(WikiDescriptor); 
		if (ds.DataGraph.Caches.ContainsKey(cachetype))
		{
			WikiDescriptor row = this.ds.DataGraph.Caches[cachetype].Current as WikiDescriptor;
			if (row != null)
			{
				bool isKB = row.WikiArticleType == ShowRouter.KBArticleType;
				this.webSitePathsTabItem.Visible = row.WikiArticleType == WikiArticleType.SitePage;
			}
		}

		WikiMaintenance graph = this.ds.DataGraph as WikiMaintenance;
		if (graph.IsSiteMapAltered)
			this.ds.CallbackResultArg = "RefreshSitemap";
	}
}
