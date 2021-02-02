using System;
using System.Collections.Generic;
using System.Monads;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using PX.Data;
using PX.Data.Wiki.Parser;
using PX.SM;
using PX.Web.UI;
using PX.Dashboards;
using PX.Dashboards.DAC;
using PX.Dashboards.Widgets;
using PX.Api.Services;

public partial class Frames_Default : PX.Web.UI.PXPage
{
	public bool ShowWelcomePage { get; set; }
	public string WikiUrl { get; private set; }
	public string HomePage { get; private set; }

	protected override void OnPreInit(EventArgs e)
	{
		Master.ScreenID = null;
		Master.ScreenTitle = null;
		base.OnPreInit(e);
	}

	protected void Page_Init(object sender, EventArgs e)
	{
		this.ShowWelcomePage = false;
		if (LoginService.HasMobileIdentity())
		{
			this.Master.FindControl("usrCaption").Visible = false;
		}

		((IPXMasterPage)this.Master).CustomizationAvailable = false;
		((IPXMasterPage)this.Master).WebServicesAvailable = false;
		((IPXMasterPage)this.Master).AuditHistoryAvailable = false;
		((IPXMasterPage)this.Master).FavoriteAvailable = true;

		if (!this.IsCallback && !string.IsNullOrEmpty(Page.Request.QueryString["ScreenId"]) &&
				Page.Request.Path.IndexOf("/Pages/", StringComparison.InvariantCultureIgnoreCase) >= 0)
		{
			KeyValuePair<string, string>[] arr = new KeyValuePair<string, string>[Page.Request.QueryString.Count - 1];
			int i = 0;
			foreach (string key in Page.Request.QueryString.Keys)
			{
				if (key == null) continue;
				if (string.Compare(key, "ScreenId", true) == 0) continue;
				arr[i++] = new KeyValuePair<string, string>(key, Page.Request.QueryString[key]);
			}
			PX.Data.Handlers.PXEntityOpener.Open(Page.Request.QueryString["ScreenId"], true, arr);
		}

		var layoutGraph = ds.DataGraph as LayoutMaint;
		if (layoutGraph != null)
		{
			Dashboard dashb = layoutGraph.Dashboard.SelectSingle();
			var forceWelcome = !string.IsNullOrWhiteSpace(Page.Request.QueryString["welcomePage"]);
			this.HomePage = System.Configuration.ConfigurationManager.AppSettings["HomePageUrl"] ?? String.Empty;
			if (!forceWelcome && ( PXGraph.ProxyIsActive || (dashb != null && layoutGraph.IsDashboardAccessible())))
			{
				dashSet.DesignModeSwitched += (s, eargs) => layoutGraph.InDesignMode = eargs.Value;
			}
			else if(!string.IsNullOrWhiteSpace(HomePage))
			{
				this.Master.WillShowWelcomePage = true;
				ShowWelcomePage = true;
				this.WikiUrl = GetWikiUrl();
			}
			else
			{
				if (!this.IsCallback){
					Response.Redirect(GetWikiUrl());
				}
			}
		}
	}
	protected void Page_Load(object sender, EventArgs e)
	{
		ds.PrepareScreen();
	}

	private string GetWikiUrl()
	{
		var screenId = Master.ScreenID;
		if (string.IsNullOrEmpty(screenId)) screenId = "00.00.00.00";

		string art = screenId.Replace(".", "_"), menu = screenId.Replace(".", "");
		PXSiteMapNode mn = PXSiteMap.Provider.FindSiteMapNodeByScreenID(menu);
		if (mn != null)
		{
			string nodeguid = mn.NodeID.ToString(), parGuid = mn.ParentID.ToString(); ;
			//Response.Redirect(
			return string.Format("{0}?pageid={1}&PrevScreenID={2}&SiteMapGuid={3}&ParentGuid={4}&rootUrl={5}",
			ResolveUrl("~/Wiki/ShowWiki.aspx"), GetArticleID(art), screenId, nodeguid, parGuid, menu);//);
		}
		else
		{
			//Response.Redirect(
			return string.Format("{0}?pageid={1}&PrevScreenID={2}&rootUrl={3}",
				ResolveUrl("~/Wiki/ShowWiki.aspx"), GetArticleID(art), screenId, menu);//);
		}
	}

	private static Guid GetArticleID(string name)
	{
		if (!String.IsNullOrEmpty(name))
		{
			PXResultset<WikiPage> pageSet = PXSelect<WikiPage,
				Where<WikiPage.name, Equal<Required<WikiPage.name>>>>.Select(new PXGraph(), name);

			if (pageSet != null && pageSet.Count > 0)
				return (Guid)(((WikiPage)pageSet[0][typeof(WikiPage)]).PageID ?? Guid.Empty);
		}
		return Guid.Empty;
	}
}
