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
using PX.Web.UI;

public partial class Page_GenericInquiry : PX.Web.UI.PXPage
{
	private const string LastEntryScreenSessionKey = "Lists$LastEntryScreen_ScreenID";

	protected void Page_Init(object sender, EventArgs e)
	{
		form.DataBinding += form_DataBinding;
		((IPXMasterPage)this.Master).CustomizationAvailable = false;
	}

	protected override void OnInitComplete(EventArgs e)
	{
		base.OnInitComplete(e);
		((IPXMasterPage)Master).AddTitleModule(new PX.Web.Controls.TitleModules.GenericInquiryTitleModule());

		var tlbTools = this.Master.FindControl("usrCaption").FindControl("tlbTools") as PXToolBar;
		if (tlbTools != null) tlbTools.CallbackUpdatable = true;
	}

	protected void Page_Load(object sender, EventArgs e)
	{
		ds.PrepareScreen();

		PXGenericInqGrph genGraph = (PXGenericInqGrph)ds.DataGraph;
		if (genGraph.Design != null)
		{
			// Check access rights - they are handled by SiteMapProvider by URL
			// but GI can be opened by using different URLs (by ID and by Name)
			if (!PXGraph.ProxyIsActive && !PXGraph.GeneratorIsActive && this.Context != null)
			{
				string nodeUrl = PXUrl.IgnoreSystemParameters(PXUrl.ToRelativeUrl(this.Request.Url.ToString()));
				var node = PXSiteMap.Provider.FindSiteMapNodeUnsecure(nodeUrl);

				if (node == null || !node.IsAccessibleToUser(this.Context))
				{
					// If there is no access to the GI itself or it is not in site map,
					// consider it as a "Preview" mode in the GI designer and check access rights for the GI designer itself
					var designerNode = PXSiteMap.Provider.FindSiteMapNodeByGraphType(typeof(PX.Data.Maintenance.GI.GenericInquiryDesigner).FullName);

					if (designerNode == null)
					{
						Redirector.SmartRedirect(HttpContext.Current, PXUrl.MainPagePath);
						return;
					}
				}
			}

			if (genGraph.Design.PrimaryScreenID != null && PXList.Provider.IsList(genGraph.Design.PrimaryScreenID))
				PXContext.Session.SetString(LastEntryScreenSessionKey, genGraph.Design.PrimaryScreenID);

			// Clear saved searches in case if parameters changed
			if (genGraph.Design.DesignID != null && PXGenericInqGrph.ParametersChanged[genGraph.Design.DesignID.Value])
			{
				ds.SavedSearchesClear(true);
				PXGenericInqGrph.ParametersChanged[genGraph.Design.DesignID.Value] = false;
			}
		}
		else // GI configuration with defined ID/Name does not exist in this company (for example, when changing company)
		{
			string screenID = PXContext.Session[LastEntryScreenSessionKey] as string;
			PXContext.Session.SetString("LastUrl", null); // clear last url to prevent loading GI page again (Main page loads LastUrl when there is no ScreenID)
			string redirectUrl = PXUrl.MainPagePath;
			if (!String.IsNullOrEmpty(screenID))
				redirectUrl += "?ScreenID=" + screenID;
			Redirector.SmartRedirect(HttpContext.Current, redirectUrl);
		}
		PXContext.SetSlot<Guid?>("__GEN_INQ_DESIGN_ID__", genGraph.Design.DesignID);
		var phF = this.FindControl("cont2");
		if (!form.Visible)
		{
			//grid.GridStyles.ToolsCell.CssClass += " transparent";
			form.Parent.Parent.Visible = false;
		}
	}

	protected void form_DataBinding(object sender, EventArgs e)
	{
		PXGenericInqGrph graph = (PXGenericInqGrph)this.DefaultDataSource.DataGraph;
		IPXMasterPage master = Page.Master as IPXMasterPage;

		if (master != null && graph != null && graph.Design != null)
		{
			master.ScreenTitle = graph.Design.SitemapTitle ?? graph.Design.Name;
		}
	}
}
