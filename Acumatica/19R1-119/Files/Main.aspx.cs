using System;
using System.Activities.Expressions;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Web;
using System.Web.Configuration;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Windows.Forms.VisualStyles;
using PX.Common;
using PX.Data;
using PX.Data.EP;
using PX.Data.Maintenance.GI;
using PX.SM;
using PX.Translation;
using PX.Web.Controls;
using PX.Web.UI;
using PX.Export.Authentication;

public partial class _Main : PX.Web.UI.PXPage, ITitleModuleController
{
	protected override bool IsContentPage
	{
		get
		{
			return false;
		}
	}
	#region Event handlers

	protected override void OnPreProcessRequest(System.Web.HttpContext context)
	{
		string target = context.Request.Form["__EVENTTARGET"];
		string argument = context.Request.Form["__EVENTARGUMENT"];
		if (target != null && target.EndsWith("menuuserName"))
		{
			string[] ar = argument.Split('@');
			if (ar.Length == 2 && ar[0].EndsWith("Company"))
				this.companySwitched = PXLogin.SwitchCompany(ar[1]);
		}
		base.OnPreProcessRequest(context);
	}

	//---------------------------------------------------------------------------
	/// <summary>
	/// The page Init event handler.
	/// </summary>
	protected override void OnInit(EventArgs e)
	{
		this.InitializeControlsOnInit();
        this.CheckLastScreenAccess();

		if (!this.IsCallback)
		{
			MainFrameHelpers.RegisterMainFrameScripts(this);
			MainFrameHelpers.RegisterMainFrameModules(this);

			if (!this.IsPostBack && this.Request.QueryString["ScreenID"] != null) PXContext.Session.SetString("LastUrl", null);
		}
		base.OnInit(e);
	}

	/// <summary>
	/// OnIniy part of the controls initialization.
	/// </summary>
	private void InitializeControlsOnInit()
	{
		this.InitBusinessDate();
		this.InitUserName();
		if (!this.IsCallback || this.IsReloadTopPanel)
		{
			this.logoImg.ImageUrl = "~/App_Themes/" + this.StyleSheetTheme + "/Images/logo.png";
			this.SetCompanyLogo();
		}
	}

	/// <summary>
	/// The page InitComplete event handler.
	/// </summary>
	protected override void OnInitComplete(EventArgs e)
	{
		bool initEvents = true;
		if (this.IsPostBack)
		{
			if (ControlHelper.IsPostbackOwner(toolsBar.UniqueID + "$menuuserName"))
				initEvents = false;
		}
		if (initEvents) this.InitTasksAndEvents(false);
		base.OnInitComplete(e);
	}

	//---------------------------------------------------------------------------
	/// <summary>
	/// The page load event handler.
	/// </summary>
	protected void Page_Load(object sender, EventArgs e)
	{
		if (ResourceCollectingManager.DontAddHeaderToResponce != true)
		{
			Response.TryAddHeader("cache-control", "no-store, private");
		}
		else
		{
			ResourceCollectingManager.DontAddHeaderToResponce = false;
		}	
		if (PXSiteMap.IsPortal) this.moduleLink.Visible = false;
		if (ControlHelper.IsRtlCulture()) this.Page.Form.Style[HtmlTextWriterStyle.Direction] = "rtl";

		if (this.IsCallback)
		{
			this.EnableViewStateMac = false;
			this.activeSystem = this.Request.Form[_activeSystemKey];
			this.activeModule = this.Request.Form[_activeModuleKey];
			this.activeSubMod = this.Request.Form[_activeSubModKey];

			Guid module;
			if (Guid.TryParse(this.activeModule, out module)) 
				this.searchBox.ActiveModule = module;

			// load the active panel data
			this.activePanelIndex = this.Request.Form[_activePanelKey];
			string[] ar = this.activePanelIndex.Split('|');
			if (ar.Length == 2)
			{
				this.activePanelIndex = ar[0]; this.activePanelKey = ar[1];
			}
		}
		else
		{
			string suffix = PXSessionStateStore.GetSuffix(this.Context);
			int pos = suffix.IndexOf("W("), pos2 = suffix.IndexOf(")");
			if(pos >= 0) suffix = int.Parse(suffix.Substring(2, pos2 - 2)).ToString();
			HttpCookie cookie = Request.Cookies["favoritesActive" + suffix];
			if (cookie != null && !string.IsNullOrEmpty(cookie.Value)) this.favoritesActive = true;
		}

        searchBox.ButtonText = PX.Web.Controls.Messages.GetLocal(PX.Web.Controls.Messages.SearchBox);
        this.InitializeControlsOnLoad();
        string hideNavigationPane = PX.Web.Controls.Messages.GetLocal(PX.Web.Controls.Messages.HideNavigationPane);
        string showNavigationPane = PX.Web.Controls.Messages.GetLocal(PX.Web.Controls.Messages.ShowNavigationPane);
        hideFrameBox.Attributes["title"] = hideNavigationPane;
        Page.ClientScript.RegisterClientScriptBlock(GetType(), "__hideNavigationPane", "var __hideNavigationPane=\"" + hideNavigationPane + "\";", true);
        Page.ClientScript.RegisterClientScriptBlock(GetType(), "__showNavigationPane", "var __showNavigationPane=\"" + showNavigationPane + "\";", true);
    }

	/// <summary>
	/// OnLoad part of the controls initialization.
	/// </summary>
	private void InitializeControlsOnLoad()
	{
		this.InitSearchBox();
		if (!this.IsCallback || this.IsReloadTopPanel)
		{
			this.InitTopMenu();
			this.InitModulesMenu();
        }
		if (!this.IsCallback || this.IsReloadMenuPanel)
		{
			this.activePanelIndex = "0";
			string hideSubModules = System.Configuration.ConfigurationManager.AppSettings["HideSubModuleBar"];
			if (string.IsNullOrEmpty(hideSubModules) || hideSubModules.ToLower() != "true")
				this.InitSubModulesMenu();
			else subModulesBar.Visible = false;
		}
		this.activeNavPanel = this.CreateActiveNavigationPanel();
	}

	/// <summary>
	/// The page PreRenderComplete event handler.
	/// </summary>
	protected override void OnPreRenderComplete(EventArgs e)
	{
		this.ClientScript.RegisterStartupScript(this.GetType(), "load", "\npx.loadFrameset();", true);
		this.ClientScript.RegisterStartupScript(this.GetType(), "hideScript", "\nvar hideScript = 1; ", true);
		this.ClientScript.RegisterStartupScript(this.GetType(),
			"time", string.Format("var timeShift ={0}; ", LocaleInfo.GetTimeZone().UtcOffset.TotalMilliseconds), true);
		if (PXSiteMap.IsPortal)
			this.ClientScript.RegisterStartupScript(this.GetType(), "portal", "var isPortal = 1;", true);
		
		var ms = WebConfigurationManager.GetSection(FormsAuthenticationSection._SECTION_PATH) as FormsAuthenticationSection;
		this.ClientScript.RegisterStartupScript(
			this.GetType(), "loginUrl", string.Format("var loginUrl = '{0}'; ", ms.LoginUrl), true);
		this.ClientScript.RegisterStartupScript(
			this.GetType(), "url", string.Format("\nvar lastUrl ='{0}';", this.GetLastUrl()), true);

		if (this.IsContextNavigation())
			this.ClientScript.RegisterStartupScript(this.GetType(), "contextNav", "var contextNav = 1;", true);
		
		this.ClientScript.RegisterHiddenField(_activeSystemKey, this.activeSystem);
		this.ClientScript.RegisterHiddenField(_activeModuleKey, this.activeModule);
		this.ClientScript.RegisterHiddenField(_activeSubModKey, this.activeSubMod);
		this.ClientScript.RegisterHiddenField(_activePanelKey, this.activePanelIndex);
		base.OnPreRenderComplete(e);
	}

	/// <summary>
	/// Page unload event handler.
	/// </summary>
	protected override void OnUnload(EventArgs e)
	{
		PXSiteMap.Provider.SetCurrentNode(null);
		base.OnUnload(e);
	}

	//---------------------------------------------------------------------------
	/// <summary>
	/// The set business callback event handler.
	/// </summary>
	protected void onSetDate_CallBack(object sender, PXCallBackEventArgs e)
	{
		object date = ((PXDateTimeEdit)pnlDate.FindControl("edEffDate")).Value;
		if (date != null)
		{
			PXContext.SetBusinessDate((DateTime)date);
			PXDateTimeEdit.SetDefaultDate((DateTime)date);
		}
	}


	/// <summary>
	/// The user menu items event handler.
	/// </summary>
	void UserNameMenu_ItemClick(object sender, PXMenuItemEventArgs e)
	{
		if (e.Item.Value == "LogOut")
		{
			PX.Data.PXLogin.LogoutUser(PXAccess.GetUserName(), Session.SessionID);
			PXContext.Session.SetString("UserLogin", string.Empty); // this session variable is used in global.asax

			string absoluteLoginUrl = PX.Export.Authentication.AuthenticationManagerModule.Instance.SignOut();

			Session.Abandon();
			var sessionCookieName = "ASP.NET_SessionId";
			var sessionStateSection = (SessionStateSection)WebConfigurationManager.GetSection("system.web/sessionState");
			if (sessionStateSection != null)
				sessionCookieName = sessionStateSection.CookieName;
			HttpContext.Current.Response.Cookies.Set(new HttpCookie(sessionCookieName, ""));
			PX.Data.Auth.ExternalAuthHelper.SignOut(Context, absoluteLoginUrl);
			
			PX.Export.Authentication.FormsAuthenticationModule.
				RedirectToLoginPage(PX.Data.Auth.ExternalAuthHelper.SILENT_LOGIN + "=None", true);
		}
		else if (e.Item.Value == "SwitchToNewUI")
		{
			using (new PXPreserveScope())
			{
				var prefGraph = PXGraph.CreateInstance<SMAccessPersonalMaint>();
				var prefs = prefGraph.UserPrefs.SelectSingle() ?? prefGraph.UserPrefs.Insert();
				prefs.UseLegacyUI = false;
				prefGraph.UserPrefs.Update(prefs);
				prefGraph.Persist();
			}
		}
		else if (e.Item.Value != null && e.Item.Value.StartsWith("Company"))
		{
			string companyId = e.Item.Text;
			if (!this.companySwitched)
			{

				HttpCookie cookie = Response.Cookies["CompanyID"];
				if (cookie != null) cookie.Value = companyId;
				else Response.Cookies.Add(new HttpCookie("CompanyID", companyId));
			}

			this.favoritesActive = false;
			this.activeSystem  = this.activeModule = this.activeSubMod = string.Empty;
			
			this.InitializeControlsOnInit();
			this.InitializeControlsOnLoad();
			this.InitTasksAndEvents(true);
		}
	}
	#endregion

	#region Methods to initialize naviagtion bars

	/// <summary>
	/// Check if current callback perform the navigation panel reload.
	/// </summary>
	private bool IsReloadTopPanel
	{
		get
		{
			if (this.IsCallback) return this.Request.Params["__CALLBACKID"] == "panelT";
			return false;
		}
	}

	/// <summary>
	/// Check if current callback perform the navigation panel reload.
	/// </summary>
	private bool IsReloadMenuPanel
	{
		get
		{
			if (this.IsCallback) return this.Request.Params["__CALLBACKID"].EndsWith("menuPanel");
			return false;
		}
	}

	static private bool IsValidNode(SiteMapNode node)
	{
		PXSiteMapNode pxNode = node as PXSiteMapNode;
		return pxNode == null || pxNode.ScreenID != "HD000000";
	}

	/// <summary>
	/// Get the active sitemap node at specified level.
	/// </summary>
	private SiteMapNode GetActiveNode(int level)
	{
		string url = GetPureLastUrl(this.GetLastUrl());
		SiteMapNode an = null;
		// FindSiteMapNode throws exception in case of external URL
		try
		{
			an = string.IsNullOrEmpty(url) ? null : PXSiteMap.Provider.FindSiteMapNode(url);
		}
		catch { }

		if (level < 0) return an;
         
		List<SiteMapNode> nodes = new List<SiteMapNode>();
		while (an != null && an != an.RootNode) { nodes.Insert(0, an); an = an.ParentNode; }
		
		if (nodes.Count > 0 && !IsValidNode(nodes[0])) nodes.Clear();
		return (level < nodes.Count) ? nodes[level] : null;
	}

	/// <summary>
	/// Check if current node in Favorites.
	/// </summary>
	private bool IsInFavorites(string nodeID)
	{
		using (PXDataRecord exist = PXDatabase.SelectSingle<Favorite>(
			new PXDataField("SiteMapID"), new PXDataFieldValue("SiteMapID", nodeID),
			new PXDataFieldValue("UserID", PXAccess.GetUserID())))
		{
			return exist != null;
		}
	}

    /// <summary>
	/// Check Access rights to the last screen (recursively).
	/// </summary>
    private void CheckLastScreenAccess()
    {
        String lasturl = PXContext.Session["LastUrl"] as String;
        if (!String.IsNullOrEmpty(lasturl))
        {
            SiteMapNode node = PXSiteMap.Provider.FindSiteMapNode(lasturl);
            do
            {
                if (node == null || !node.IsAccessibleToUser(HttpContext.Current))
                {
                    PXContext.Session.SetString("LastUrl", null);
                    break;
                }

                node = node.ParentNode;
            }
            while (node != null && node.Key != Guid.Empty.ToString());
        }
    }

	//---------------------------------------------------------------------------
	/// <summary>
	/// Create toolbar button with specified parameters.
	/// </summary>
	private static PXToolBarButton CreateToolsButton(PXToolBar tlb, string text, string tooltip, string key, string imageUrl, string imageSet, string imageKey)
	{
		var btn = new PXToolBarButton() { Key = key };
		if (!String.IsNullOrEmpty(imageUrl))
		{
			btn.Images.Normal = imageUrl;
			btn.Tooltip = tooltip;
		}
		else if (text != null)
		{
			btn.ImageSet = imageSet;
			btn.ImageKey = imageKey;
			btn.Text = text;
		}

		if (tlb != null) tlb.Items.Add(btn);
		return btn;
	}

	/// <summary>
	/// Create menu item with specified parameters.
	/// </summary>
	private static PXMenuItem CreateMenuItem(
		PXToolBarButton btn, string text, string key, string imageSet, string imageKey)
	{
		var item = new PXMenuItem() { Text = text, Value = key };
		item.ImageSet = imageSet; item.ImageKey = imageKey;
		if (btn != null) btn.MenuItems.Add(item);
		return item;
	}
	/// <summary>
	/// Create menu item with specified parameters.
	/// </summary>
	private static PXMenuItem CreateMenuItem(PXToolBarButton btn, string text, string key)
	{
		return CreateMenuItem(btn, text, key, string.Empty, string.Empty);
	}

	//---------------------------------------------------------------------------
	/// <summary>
	/// Initialize the top menu of the page.
	/// </summary>
	private void InitTopMenu()
	{
		SiteMapNode an = null;
		if (!this.Page.IsCallback || this.IsReloadTopPanel) an = GetActiveNode(0);
		systemsBar.Items.Clear();

		var nodes = this.GetTopMenuDataSource();
		PXToolBarButton activeButton = null;
		for (int i = 0; i < nodes.Count; i++)
		{
			SiteMapNode node = nodes[i];
			string descr = node.Description, image = null;
			bool active = false;
			if (!string.IsNullOrEmpty(descr))
			{
				string[] im = descr.Split('|');
				image = PXImages.ResolveImageUrl(im[0], this);
			}

			// sets the active module keys
			if (!this.Page.IsCallback && ((an == null && i == 0) || (an != null && an.Key == node.Key)))
			{
				this.activeSystem = node.Key; active = true; this.activeNode = node; 
			}
			
			var btn = CreateToolsButton(systemsBar, node.Title, node.Title, node.Key, null, null, null);
			//btn.Images.Normal = image;
			btn.NavigateUrl = node.Url;
			btn.ToggleGroup = "1";
			btn.ToggleMode = true;
			btn.Pushed = active;
			btn.AlreadyLocalized = true;
			if (active) activeButton = btn;
		}

		if (string.IsNullOrEmpty(this.activeSystem) && nodes.Count > 0)
		{
			this.activeSystem = nodes[0].Key;
			activeButton = ((PXToolBarButton)systemsBar.Items[0]);
			activeButton.Pushed = true;
		}
		if (activeButton != null)
		{
			this.moduleLink.InnerText = activeButton.Text;
			this.moduleLink.HRef = ControlHelper.FixHideScriptUrl(this.ResolveUrl(activeButton.NavigateUrl), true);
			//activeButton.Attributes["target"] = "main";
			//activeButton.Attributes["href"] = this.moduleLink.HRef;
		}

		if (systemsBar.Items.Count <= 1) systemsBar.Style[HtmlTextWriterStyle.Display] = "none";
		((ICompositeControlDesignerAccessor)systemsBar).RecreateChildControls();
	}

	/// <summary>
	/// Gets the root nodes data source.
	/// </summary>
	private List<SiteMapNode> GetTopMenuDataSource()
	{
		List<SiteMapNode> source = new List<SiteMapNode>();
		foreach (SiteMapNode node in System.Web.SiteMap.RootNode.ChildNodes)
			if (IsValidNode(node)) source.Add(node);
		return source;
	}

	//---------------------------------------------------------------------------
	/// <summary>
	/// Initialize the modules toolbar.
	/// </summary>
	private void InitModulesMenu()
	{
		SiteMapNode an = null, firstNode = null;
		bool reloadTopPanel = this.IsReloadTopPanel;
		if (!this.Page.IsCallback || reloadTopPanel)
		{
			an = GetActiveNode(1);
			if (an != null && this.favoritesActive && IsInFavorites(GetActiveNode(-1).Key)) an = null;
		}
		modulesBar.Items.Clear();

		PXToolBarButton firstButton = null, activeButton = null, fav = null;
		// create Favorites button
		if (!PXSiteMap.IsPortal)
		{
			fav = CreateToolsButton(modulesBar, null, null, _favorites, null, null, null);
			fav.NavigateUrl = _favoritesUrl;
			fav.Images.Normal = Sprite.Main.GetFullUrl(Sprite.Main.StarGray);
			fav.PushedImages.Normal = Sprite.Main.GetFullUrl(Sprite.Main.StarWhite);
			fav.ToggleMode = true;
			fav.Tooltip = PX.Web.Controls.Messages.Favorites;
			if (!this.Page.IsCallback || reloadTopPanel)
			{
				Boolean exist = PXSiteMap.FavoritesProvider.FavoritesExists();
				if (exist && this.activeModule == _favorites && reloadTopPanel)
					an = null;
				if (exist && an == null)
				{
					this.activeModule = _favorites;
					activeButton = fav;
					fav.Pushed = true;
				}
				fav.Visible = exist;
				modulesBar.Items.Add(new PXToolBarSeperator() {Visible = fav.Visible});
			}
		}

		var nodes = this.GetModulesDataSource();
		string parentKey = null;
		PXToolBarButton btn = null, visibleBtn = null;
		int groupIndex = 0, visibleCount = 0;
		for (int i = 0; i < nodes.Count; i++)
		{
			SiteMapNode node = nodes[i];
			string descr = node.Description, image = null;
			if (!string.IsNullOrEmpty(descr))
			{
				string[] im = descr.Split('|');
				image = PXImages.ResolveImageUrl(im[0], this);
			}

			if (parentKey != node.ParentNode.Key) { parentKey = node.ParentNode.Key; groupIndex++; }
			bool visible = true, active = false;
			if (!string.IsNullOrEmpty(this.activeSystem) && parentKey != this.activeSystem)
				visible = false;
			else
				visibleCount++;

			if ((an != null && an.Key == node.Key) || 
				(an == null && this.activeSystem == parentKey && string.IsNullOrEmpty(this.activeModule)))
			{
				this.activeModule = node.Key; active = true; this.activeNode = node; 
			}

			btn = CreateToolsButton(modulesBar, node.Title, node.Title, parentKey + "|" + node.Key, null, null, null);
			btn.Visible = visible;
			btn.NavigateUrl = node.Url;
			btn.ToggleGroup = groupIndex.ToString();
			btn.ToggleMode = true;
			btn.Pushed = active;
			btn.AlreadyLocalized = true;
			if (visible) visibleBtn = btn;
			
			if (active) activeButton = btn;
			if (visible && firstNode == null) 
			{ 
				firstNode = node; firstButton = btn;
				if (fav != null) fav.ToggleGroup = btn.ToggleGroup;
			}
			modulesBar.Items.Add(new PXToolBarSeperator() { Visible = visible });
		}

		if (visibleCount == 1 && (fav == null || !fav.Visible))
		{
			int index = modulesBar.Items.IndexOf(visibleBtn);
			visibleBtn.Visible = false;
			if (index < (modulesBar.Items.Count - 1)) modulesBar.Items[index + 1].Visible = false;
			visibleCount--;
		}
		
		// hide favorites if modules bar has no items
		if (visibleCount == 0)
		{
			this.activeModule = string.Empty; activeButton = null;
			if (fav != null && fav.Visible)
			{
				modulesBar.Items[fav.Index + 1].Visible = fav.Visible = false;
				fav.Attributes["data-hidden"] = "1";
			}
			if (!modulesBar.CssClass.Contains("hidden")) modulesBar.CssClass += " hidden";
			if (!panelT.CssClass.Contains("modulesHidden")) panelT.CssClass += " modulesHidden";
		}
		else
		{
			modulesBar.CssClass = modulesBar.CssClass.Replace("hidden", "").Trim();
			panelT.CssClass = panelT.CssClass.Replace("modulesHidden", "").Trim();
		}

		if (string.IsNullOrEmpty(this.activeModule) && firstNode != null)
		{
			this.activeModule = firstNode.Key;
			firstButton.Pushed = true; activeButton = firstButton;
		}
		if (activeButton != null)
		{
			this.moduleLink.InnerText = 
				string.IsNullOrEmpty(activeButton.Text) ? activeButton.Tooltip : activeButton.Text;
			this.moduleLink.HRef = ControlHelper.FixHideScriptUrl(this.ResolveUrl(activeButton.NavigateUrl), true);
		}
		((ICompositeControlDesignerAccessor)modulesBar).RecreateChildControls();
	}

	/// <summary>
	/// Gets the modules data source.
	/// </summary>
	private List<SiteMapNode> GetModulesDataSource()
	{
		List<SiteMapNode> source = new List<SiteMapNode>();
		foreach (SiteMapNode node in System.Web.SiteMap.RootNode.ChildNodes)
		{
			if (!IsValidNode(node)) continue; 
			foreach (SiteMapNode node2 in node.ChildNodes) source.Add(node2);
		}
		return source;
	}

	//---------------------------------------------------------------------------
	/// <summary>
	/// Initialize the Sub-module menu control.
	/// </summary>
	private void InitSubModulesMenu()
	{
		bool reloadMenuPanel = this.IsReloadMenuPanel;
		SiteMapNode an = null;
		if (!this.Page.IsCallback || reloadMenuPanel)
		{
			an = GetActiveNode(2);
			if (an != null && this.favoritesActive && IsInFavorites(GetActiveNode(-1).Key)) an = null;
		}
		subModulesBar.Items.Clear();

		List<SiteMapNode> nodes = this.GetSubModulesDataSource();
		PXToolBarButton btn = null, visibleBtn = null;
		string parentKey = null;
		int groupIndex = 0, visibleCount = 0;
		PXWikiProvider wp = PXSiteMap.WikiProvider;
		string module = string.IsNullOrEmpty(this.activeModule) ? this.activeSystem : this.activeModule;

		for (int i = 0; i < nodes.Count; i++)
		{
			SiteMapNode node = nodes[i];
				PXSiteMapNode pxNode = node as PXSiteMapNode;
			if (pxNode != null) pxNode.ResetChildNodesSource();

			if (PXSiteMap.Provider.GetChildNodesSimple(node).Count == 0 &&
				(pxNode == null || wp.FindSiteMapNodeFromKey(pxNode.NodeID) == null))
					continue;

			string descr = node.Description, url = null;
			if (!string.IsNullOrEmpty(descr))
			{
				string[] im = descr.Split('|');
				url = PXImages.ResolveImageUrl(im[0], this);
			}

			bool startGroup = (parentKey != node.ParentNode.Key);
			if (startGroup) { parentKey = node.ParentNode.Key; groupIndex++; }
			bool visible = true, active = false;
			if (!string.IsNullOrEmpty(module) && parentKey != module)
				visible = false;
			else
				visibleCount++;

			if ((an != null && an.Key == node.Key) ||
				(an == null && string.IsNullOrEmpty(this.activeSubMod) && (module == parentKey || module == node.Key)))
			{
				this.activeSubMod = node.Key; active = true; this.activeNode = node; 
			}
			if (startGroup && btn != null) btn.Attributes.Add("endGroup", "1");

		    btn = CreateToolsButton(subModulesBar, node.Title, node.Title, parentKey + "|" + node.Key, url, null, null);
			btn.Visible = visible;
			btn.ToggleGroup = groupIndex.ToString();
			btn.ToggleMode = true;
			btn.Pushed = active;
			btn.AlreadyLocalized = true;
			if (startGroup) btn.Attributes.Add("startGroup", "1");
			if (visible) visibleBtn = btn;
		}
		if (visibleCount == 1) { visibleBtn.Visible = false; visibleCount--; }

		var css = subModulesBar.CssClass;
		if (visibleCount == 0) {if (!css.Contains("hidden")) subModulesBar.CssClass += " hidden"; }
		else subModulesBar.CssClass = css.Replace("hidden", "").Trim();

		((ICompositeControlDesignerAccessor)subModulesBar).RecreateChildControls();
	}

	/// <summary>
	/// Gets the modules data source.
	/// </summary>
	private List<SiteMapNode> GetSubModulesDataSource()
	{
		List<SiteMapNode> source = new List<SiteMapNode>();

		foreach (SiteMapNode node in System.Web.SiteMap.RootNode.ChildNodes)
		{
			if (!node.HasChildNodes || !IsValidNode(node)) continue;
			foreach (SiteMapNode node2 in node.ChildNodes)
			{
				if (!node2.HasChildNodes || !IsValidNode(node2)) continue;
				foreach (SiteMapNode node3 in node2.ChildNodes) source.Add(node3);
			}
		}
		return source;
	}

	//---------------------------------------------------------------------------
	/// <summary>
	/// Create the panel for active navigation tree.
	/// </summary>
	private PXSmartPanel CreateActiveNavigationPanel()
	{
		if (this.activeNavPanel != null)
		{
			activeNavPanel.Parent.Controls.Remove(activeNavPanel);
			this.activeNavPanel = null;
		}

		List<string> reqParts = null;
		if (this.Page.IsCallback)
		{
			string req = this.Request.Params["__CALLBACKID"];
			reqParts = new List<string>(req.Split('$'));
		}

		// create favorites panel
		string module = string.IsNullOrEmpty(this.activeModule) ? this.activeSystem : this.activeModule;
		if (module == _favorites || (reqParts != null && reqParts.Contains("spFav")))
		{
			var panel = CreateNavigationPanel(null, true, "Fav");
			((IParserAccessor)this.menuPanel).AddParsedSubObject(panel);
			return panel;
		}

		bool createPanel = (!this.IsCallback || this.IsReloadMenuPanel);
		if (!createPanel && this.IsCallback)
		{
			string panelID = "sp" + this.activePanelIndex, trId = "tree" + this.activePanelIndex;
			if (reqParts.Contains(panelID) || reqParts.Contains(trId)) createPanel = true;
		}

		if (createPanel)
		{
			if (!string.IsNullOrEmpty(this.activePanelKey))
				this.activeNode = PXSiteMap.Provider.FindSiteMapNodeFromKey(this.activePanelKey);

			if (this.activeNode == null)
			{
				if (!string.IsNullOrEmpty(this.activeSubMod)) module = this.activeSubMod;
				this.activeNode = PXSiteMap.Provider.FindSiteMapNodeFromKey(module);
			}

			if (this.activeNode == null)
			{
				if (!string.IsNullOrEmpty(this.activeModule)) module = this.activeModule;
				this.activeNode = PXSiteMap.Provider.FindSiteMapNodeFromKey(module);
			}

			var panel = CreateNavigationPanel(this.activeNode, true, this.activePanelIndex);
			((IParserAccessor)this.menuPanel).AddParsedSubObject(panel);
			return panel;
		}
		return null;
	}

	private SiteMapDataSource CreateSiteMapDataSourceInstance()
	{
		return new PXSiteMapDataSource(new PXListsSiteMapModifier());
	}

	/// <summary>
	/// Create SmartPanel to render navigation tree.
	/// </summary>
	private PXSmartPanel CreateNavigationPanel(SiteMapNode node, bool active, object index)
	{
		string panelID = "sp" + index.ToString();
		bool favorites = (node == null);
		string trId = favorites ? _favTreeID : ("tree" + index.ToString());

		PXSmartPanel panel = new PXSmartPanel();
		panel.ID = panelID;
		panel.Key = favorites ? _favorites : node.Key;
		panel.LoadOnDemand = !active;
		panel.AllowResize = panel.AllowMove = false;
		panel.RenderVisible = active;
		panel.AutoSize.Enabled = true;
		panel.Position = PanelPosition.Original;
		panel.CssClass = "menuPanel";

		if ((this.IsCallback && !this.IsReloadMenuPanel) || active)
		{
			SiteMapDataSource ds = CreateSiteMapDataSourceInstance();
			ds.ID = "ds" + ControlHelper.SanitizeAll(index.ToString());
			ds.ShowStartingNode = false;
			panel.Controls.Add(ds);

			PXSiteMapNode pxNode = node as PXSiteMapNode;
			Control content = null;
			PXWikiProvider wp = PXSiteMap.WikiProvider;           

			if (favorites)
			{
				ds.Provider = PXSiteMap.FavoritesProvider;
				ds.StartingNodeUrl = System.Web.SiteMap.RootNode.Url;
				content = CreateTree(ds, trId);
			}
			else if (wp.FindSiteMapNodeFromKey(pxNode.NodeID) != null)
			{
				if (wp.GetWikiID(pxNode.Title) != Guid.Empty || wp.GetWikiIDFromUrl(pxNode.Url) != Guid.Empty)
					content = CreateWikiTree(pxNode, trId);
				if (PXSiteMap.IsPortal) 
					this.moduleLink.Visible = true;
			}
			else
			{
				INavigateUIData dataItem = node as INavigateUIData;
				if (string.IsNullOrEmpty(dataItem.NavigateUrl))
				{
					PXSiteMap.Provider.SetCurrentNode(PXSiteMap.Provider.FindSiteMapNodeFromKey(node.Key));
					ds.StartFromCurrentNode = true;
				}
				else ds.StartingNodeUrl = dataItem.NavigateUrl;
				content = CreateTree(ds, trId);
			}
			if (content != null) panel.Controls.Add(content);
		}
		return panel;
	}
	#endregion

	#region Methods to work with sitemap tree

	//---------------------------------------------------------------------------
	/// <summary>
	/// Create default menu tree control with specified name and data source.
	/// </summary>
	private PXTreeView CreateTree(SiteMapDataSource ds, string controlName)
	{
		PXTreeView tree = new PXTreeView();
		tree.DataSourceID = ds.ID;
		tree.ID = controlName;
		tree.Target = "main";
		tree.SkinID = "menu";
		tree.ApplyStyleSheetSkin(this);
		tree.ShowRootNode = false;
		tree.FastExpand = true;
		tree.NodeDataBound += new PXTreeNodeEventHandler(tree_NodeDataBound);
		tree.DataBound += tree_DataBound;
		tree.ShowDefaultImages = tree.ShowLines = false;
		tree.Synchronize += new PXTreeSyncEventHandler(tree_Synchronize);
		tree.ExclusiveExpand = this.IsContextNavigation();
		tree.SearchUrl = this.ResolveUrl("~/Search/Search.aspx") + "?am=" + (!string.IsNullOrEmpty(this.activeModule) ? this.activeModule : this.activeSystem) + "&query=";
	
		return tree;
	}

	/// <summary>
	/// Create default Wiki tree control with specified name and data source.
	/// </summary>
	private PXWikiTree CreateWikiTree(PXSiteMapNode node, string treeId)
	{
		PXWikiTree tree = new PXWikiTree();
		string url = node.Url;
		Guid wikiId = PXSiteMap.WikiProvider.GetWikiIDFromUrl(node.Url);
		WikiReader reader = PXGraph.CreateInstance<WikiReader>();
		WikiDescriptor wiki = reader.wikis.SelectWindowed(0, 1, wikiId);
		tree.Provider = PX.Data.PXSiteMap.WikiProvider;
		url = Wiki.Url(wikiId);

		tree.TreeSkin = "Help";
		tree.ID = treeId;
		tree.WikiID = wikiId;
		tree.ShowDefaultImages = tree.ShowRootNode = false;
		tree.Target = "main";
		tree.StartingNodeUrl = this.ResolveUrl(url);
		tree.SearchUrl = this.ResolveUrl("~/Search/Search.aspx") + "?am=" + (!string.IsNullOrEmpty(this.activeModule) ? this.activeModule : this.activeSystem) + "&query=";
		tree.NewArticleUrl = (wiki == null ||
			string.IsNullOrEmpty(wiki.UrlEdit)) ? "" : this.ResolveUrl(wiki.UrlEdit) + "?wiki=" + wikiId;
		tree.CssClass = "menuTreeHlp";

		tree.ClientEvents.NodeClick = "MainFrame.treeClick";
		tree.Synchronize += new PXTreeSyncEventHandler(wikiTree_Synchronize);
		return tree;
	}

	/// <summary>
	/// Hide left panel for empty tree control.
	/// </summary>
	void tree_DataBound(object sender, EventArgs e)
	{
		PXTreeView tree = sender as PXTreeView;
		if (!this.Page.IsCallback && tree != null && tree.Nodes.Count == 0 && this.IsContextNavigation())
		{
			frameT.Attributes["class"] = "menuHidden"; sp1.Enabled = false;
			hideFrameBox.Style[HtmlTextWriterStyle.Display] = "none";
            frameT.Rows[0].Cells[0].Style[HtmlTextWriterStyle.Display] = "none";
			frameT.Rows[0].Cells[1].Style[HtmlTextWriterStyle.Display] = "none";
		}
	}

	void tree_Synchronize(object sender, PXTreeSyncEventArgs e)
	{
		this.PrepareSynchronizationPath(e, System.Web.SiteMap.Provider);
	}

	void wikiTree_Synchronize(object sender, PXTreeSyncEventArgs e)
	{
		this.PrepareSynchronizationPath(e, PXSiteMap.WikiProvider);
	}

	//---------------------------------------------------------------------------
	/// <summary>
	/// Calculate the Node path for tree sinchronization.
	/// </summary>
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
		if (result.Length != 0) result = result.Remove(result.Length - 1, 1);
		e.NodePath = result.ToString();
	}

	/// <summary>
	/// The tree node bound event handler.
	/// </summary>
	private void tree_NodeDataBound(object sender, PXTreeNodeEventArgs e)
	{
		INavigateUIData dataItem = e.Node.DataItem as INavigateUIData;
		PX.Data.PXSiteMapNode node = e.Node.DataItem as PX.Data.PXSiteMapNode;
		PXTreeView tree = (PXTreeView)sender;
		
		string descr = dataItem.Description;
		// sets the node images
		if (!string.IsNullOrEmpty(descr))
		{
			string[] im = descr.Split('|');
			e.Node.Images.Normal = PXStyle.ResolveImageUrl(this, im[0]);
			if (im.Length > 1) e.Node.Images.Selected = PXStyle.ResolveImageUrl(this, im[1]);
		}
		else
		{
			if (node != null && node.IsFolder)
			{
				e.Node.Images.Normal = Sprite.Tree.GetFullUrl(Sprite.Tree.Folder);
				e.Node.Images.Selected = Sprite.Tree.GetFullUrl(Sprite.Tree.FolderS);
			}
		}

		// sets the node tooltip
		//e.Node.ToolTip = dataItem.Name;
		//if (!string.IsNullOrEmpty(node.ScreenID))
		//	e.Node.ToolTip += string.Format(" ({0})", node.ScreenID);
		e.Node.ToolTip = string.Empty;

		if (node != null)
		{
			if (node.ChildNodes.Count == 0 && String.IsNullOrEmpty(node.Url))
			{
				var nodes = e.Node.Parent.ChildNodes;
				if (nodes.Contains(e.Node)) nodes.Remove(e.Node);
			}
			else if (node.Expanded == true && !this.IsContextNavigation()) 
				e.Node.Expanded = true;
		}
		//e.Node.NavigateUrl = PXPageCache.FixPageUrl(e.Node.NavigateUrl);

		SiteMapNode an = this.GetActiveNode(-1);
		string url = e.Node.NavigateUrl;
		if (an != null && url == an.Url) tree.SelectedNode = e.Node;

		// for favorites tree store the actual sitemap data path
		bool isFav = (tree.ID == _favTreeID);
		e.Node.Value = e.Node.DataPath;
		if (isFav)
		{
			var n = PXSiteMap.Provider.FindSiteMapNode(url);
			if (n != null) e.Node.Value = n.Key;
		}
		if (e.Node.NavigateUrl != null && e.Node.NavigateUrl.Contains("sm204530.aspx"))
		{
			e.Node.OpenFrameset = false;
			e.Node.Target = "_blank";
			return;
		}

		if (!string.IsNullOrEmpty(url) && this.IsInternalUrl(url))
		{
			url += url.Contains("?") ? "&" : "?";
			e.Node.NavigateUrl = string.Format("{0}{1}={2}", url, PXUrl.HideScriptParameter, "On");
		}

		//IPXSiteMapModifier modifier = new GIPrimaryScreenSiteMapModifier();
		//modifier.Modify(node);

		//File.AppendAllText(@"C:\SourcesClear\Main\WebSites\Pure\Site\App_Data\Log_SiteMap.txt", String.Join(" | ",
		//	node.ScreenID, node.Title, node.NodeID, node.ParentID, Environment.NewLine));
	}

	private bool IsInternalUrl(string url)
	{
		return Uri.Compare(this.Request.Url, new Uri(this.Request.Url, this.ResolveUrl(url)),
			UriComponents.Host, UriFormat.Unescaped, StringComparison.InvariantCulture) == 0;
	}
	#endregion

	#region Methods to work with tools Bar

	//---------------------------------------------------------------------------
	/// <summary>
	/// Initialize the application business date.
	/// </summary>
	private void InitBusinessDate()
	{
        var btn = ((PXToolBarButton)toolsBar.Items["businessDate"]);

        if (IsAutoLoginMode())
        {
            btn.Visible = false;
            return;
        }

        var date = PXContext.GetBusinessDate();

		if (PXSiteMap.IsPortal || !PXAccess.IsUserBusinessDateOverride())
		{
			btn.Enabled = false;
			btn.Tooltip = string.Empty;
		}

		if (date != null)
		{
			this.SetBusinessDateText(date);
			PXDateTimeEdit.SetDefaultDate((DateTime)date);
			if (!Page.IsPostBack || Page.IsCallback)
			{
				PXDateTimeEdit dateCtrl = pnlDate.FindControl("edEffDate") as PXDateTimeEdit;
				if (dateCtrl != null) dateCtrl.Value = date;

                PXLabel lblEffDate = pnlDate.FindControl("lblEffDate") as PXLabel;
                if (lblEffDate != null)
                {
                    lblEffDate.Text = PX.Web.Controls.Messages.GetLocal(PX.Web.Controls.Messages.BusinessDate);
                }
            }
		}
		else this.SetBusinessDateText(PXTimeZoneInfo.Now);
	}

	/// <summary>
	/// Sets the business date button text.
	/// </summary>
	private void SetBusinessDateText(object date)
	{
		PXToolBarButton btn = (PXToolBarButton)toolsBar.Items["businessDate"];
		btn.Text = ((DateTime)date).ToShortDateString();
		if (!btn.AlreadyLocalized)
			btn.Tooltip = PXMessages.LocalizeNoPrefix(btn.Tooltip);
		btn.AlreadyLocalized = true;
	}

	/// <summary>
	/// The set business callback event handler.
	/// </summary>
	protected void toolsBar_OnCallBack(object sender, PXCallBackEventArgs e)
	{
		if (e.Command.Name == "EventsCount" && IsActivityTotalsVisible())
		{
			int count = 0, newCount = 0;
			string subresult = null;
			string _labelFormat = ControlHelper.IsRtlCulture() ? _LABLE_FORMAT_RTL : _LABLE_FORMAT;
			string _labelFullFormat = ControlHelper.IsRtlCulture() ? _LABLE_FULLFORMAT_RTL : _LABLE_FULLFORMAT;

			foreach (ActivityService.Total counter in ActivityService.GetCounts())
			{
				count += counter.Count ?? 0;
				newCount += counter.NewCount ?? 0;
				string f_count = counter.Count > 99 ? "99+" : String.Format("{0}", counter.Count);
				string f_newcount = counter.NewCount > 99 ? "<b>99+</b>" : String.Format("<b>{0}</b>", counter.NewCount);
				if (counter.NewCount == counter.Count)
				{
					f_count = string.Empty;
				}
				string title = counter.NewCount > 0
										? string.Format(_labelFullFormat, counter.Title, f_count, f_newcount)
										: string.Format(_labelFormat, counter.Title, f_count);
				if (counter.Count <= 0) title = string.Empty;

				subresult += "|" + title;
			}
			string full_count = count > 99 ? "99+" : String.Format("{0}", count);
			e.Result =
					(newCount > 0
							? string.Format(_labelFullFormat, string.Empty, full_count, newCount)
							: string.Format(_labelFormat, string.Empty, full_count))
					+ subresult;
		}
	}

	/// <summary>
	/// Initialize the tasks and events menu.
	/// </summary>
	private void InitTasksAndEvents(bool refreshToolbar)
	{        
		PXToolBarButton btn = (PXToolBarButton)toolsBar.Items["events"];
		btn.MenuItems.Clear();
		btn.Visible = IsActivityTotalsVisible();
		if (btn.Visible)
		{
			Func<PXMenuItem, PXMenuItem> addItem = delegate(PXMenuItem item) { btn.MenuItems.Add(item); return item; };
			int count = 0, newCount = 0;

			string _labelFormat = ControlHelper.IsRtlCulture() ? _LABLE_FORMAT_RTL : _LABLE_FORMAT;
			string _labelFullFormat = ControlHelper.IsRtlCulture() ? _LABLE_FULLFORMAT_RTL : _LABLE_FULLFORMAT;

			foreach (ActivityService.Total counter in ActivityService.GetCounts())
			{
				count += counter.Count ?? 0;
				newCount += counter.NewCount ?? 0;

				string f_count = counter.Count > 99 ? "99+" : String.Format("{0}", counter.Count);
				string f_newcount = counter.NewCount > 99 ? "<b>99+</b>" : String.Format("<b>{0}</b>", counter.NewCount);
				if (counter.NewCount == counter.Count)
				{
					f_count = string.Empty;
				}

				string title = counter.NewCount > 0
										? string.Format(_labelFullFormat, counter.Title, f_count, f_newcount)
										: string.Format(_labelFormat, counter.Title, f_count);

				addItem(new PXMenuItem(title)
				{
					Target = "main",
					NavigateUrl = counter.Url,
					ImageKey = counter.ImageKey,
					ImageSet = counter.ImageSet,
					Visible = counter.Count > 0,
					SyncText = true
				});
			}

			if (count > 0)
			{
				string count_f = count > 99 ? "99+" : String.Format("{0}", count);
				string title = newCount > 0
										 ? string.Format(_labelFullFormat, string.Empty, count_f, newCount)
										 : string.Format(_labelFormat, string.Empty, count_f);

				btn.Text = title;
				btn.RenderMenuButton = false;

				btn.Visible = true;
			}
			else
				btn.Visible = false;
		}

		if (!this.reminderInitialized)
		{
			var rem = new PX.Web.Controls.TitleModules.ReminderTitleModule();
			rem.PassiveMode = true; rem.Initialize(this);
			this.reminderInitialized = true;
		}            
		if (refreshToolbar) ((ICompositeControlDesignerAccessor)toolsBar).RecreateChildControls();
	}

    private bool IsAutoLoginMode()
    {
        var username = WebConfig.GetString("autoLoginUserName", null);
        var password = WebConfig.GetString("autoLoginPassword", null);
        return !string.IsNullOrEmpty(username) && !string.IsNullOrEmpty(password);
    }

    //---------------------------------------------------------------------------
    /// <summary>
    /// Initialize the user name button.
    /// </summary>
    private void InitUserName()
	{
		PXToolBarButton btn = (PXToolBarButton)toolsBar.Items["userName"];

        if (IsAutoLoginMode())
        {
            btn.Visible = false;
            return;
        }

		btn.MenuItems.Clear();
		btn.Text = this.GetUserName(false);
		btn.RenderMenuButton = false;

		Func<string, string, string, string> func = delegate(string txt, string val, string labelID)
		{
			return string.Format(
				"<div class='size-s inline-block'>{0}</div> <span id='{1}'>{2}</span>", txt, labelID, val);
		};
		Func<PXMenuItem, PXMenuItem> addItem = delegate(PXMenuItem item) { btn.MenuItems.Add(item); return item; };

		addItem(new PXMenuItem("Sign Out")
		{
			AutoPostBack = true,
			ShowSeparator = true,
			ImageSet = Sprite.AliasMain,
			ImageKey = Sprite.Main.Logout,
			Value = "LogOut"
		});

		addItem(new PXMenuItem("Switch to Modern UI")
		{
			NavigateUrl = "~/Main",
			AutoPostBack = true,
			Value = "SwitchToNewUI"
		});

		if (!PXSiteMap.IsPortal)
		{
			addItem(new PXMenuItem("Organize Favorites...")
			{
				Target = "main",
				NavigateUrl = _favoritesUrl
			});
		}

			addItem(new PXMenuItem("User Profile...")
			{
				Target = "main",
				ShowSeparator = true,
			NavigateUrl = PXSiteMap.IsPortal ? "~/pages/SP/SP408045.aspx" : "~/pages/sm/sm203010.aspx",
			});

		//addItem(new PXMenuItem("Trace...") { NavigateUrl = "~/Frames/Trace.aspx", Target = "_blank"});
		//addItem(new PXMenuItem("About...") { PopupPanel = "pnlAbout", ShowSeparator = true });


		var company = PXLogin.ExtractCompany(this.GetUserName(false));
		if (!String.IsNullOrEmpty(company))
		{
			string[] companies = PXAccess.GetCompanies();
			int lastI = companies.Length - 1;
			if (lastI >= 0) for (int i = 0; i < companies.Length; i++)
				{
					bool active = String.Equals(companies[i], company, StringComparison.InvariantCultureIgnoreCase);
					addItem(new PXMenuItem(companies[i])
					{
						AutoPostBack = !active,
						Value = "Company@" + companies[i],
						//ShowSeparator = (i == lastI),
						ShowCheckBox = active,
						Checked = active,
						Enabled = !active,
                        AlreadyLocalized = true
					});
				}
		}

		PXMenu menu = ((PXMenu)btn.DropDownMenu);
		if(menu != null) menu.ClientEvents.ItemClick = "MainFrame.subUserBar_Click";

		//addItem(new PXMenuItem(func(
		//  PXMessages.LocalizeNoPrefix(PX.AscxControlsMessages.PageTitle.ScreenID), this.GetScreenID(), "screenID")));
		//addItem(new PXMenuItem(func("Version", this.GetVersion(true), "sysVersion")) { ShowSeparator = true });

		//addItem(new PXMenuItem(func(PXMessages.LocalizeNoPrefix(PX.AscxControlsMessages.PageTitle.TimeZone),
		//  this.GetTimeZone(), "timeZone")) { ShowSeparator = true });

		((IPXMenu)btn.DropDownMenu).MenuItemClick += UserNameMenu_ItemClick;
	}
	#endregion

	#region Helper methods

	//---------------------------------------------------------------------------
	/// <summary>
	/// Calculate the start page url.
	/// </summary>
	private string GetLastUrl()
	{
		string lastUrl = (string)PXContext.Session["LastUrl"];
		if (lastUrl == null) lastUrl = this.GetDefaultUrl();

		string screen = Page.Request.QueryString["ScreenId"];
		string company = Page.Request.QueryString[PXUrl.CompanyID];
		if (string.IsNullOrEmpty(lastUrl) || !string.IsNullOrEmpty(screen))
		{
			// remove duplicates of the parameters
			if (!string.IsNullOrEmpty(screen) && screen.Contains(",")) screen = screen.Split(',')[0];
			if (!string.IsNullOrEmpty(company) && company.Contains(",")) company = company.Split(',')[0];

			string url = lastUrl;
			if (!string.IsNullOrEmpty(url))
			{
				int i = url.IndexOf('?'); if (i > 0) url = url.Substring(0, i);
			}

			if (string.IsNullOrEmpty(url) || !url.Replace(".aspx", "").ToUpper().EndsWith(screen.ToUpper()))
			{
				PXSiteMapNode node = (screen != null) ? PXSiteMap.Provider.FindSiteMapNodeByScreenID(screen) : null;
				
				if (node == null && !string.IsNullOrEmpty(screen) && screen.ToLower() == "showwiki")
					lastUrl = this.ResolveUrl("~/Wiki/Show.aspx");
				else
				{
					if (node != null && !string.IsNullOrEmpty(node.Url)) lastUrl = this.ResolveUrl(node.Url);
					else lastUrl = this.ResolveUrl("Frames/Default.aspx");
				}
			}

			string query = Page.Request.Url.Query.Replace("'", "%27")
				.Replace("ScreenId=" + screen, String.Empty)
				.Replace("ScreenID=" + screen, String.Empty)
				.Replace("CompanyID=" + System.Net.WebUtility.UrlEncode(company), String.Empty)
				.Replace("CompanyID=" + company, String.Empty)
				.Replace("?", String.Empty);

			lastUrl = PX.Common.PXUrl.CombineParameters(lastUrl, query);
		}
		return lastUrl;
	}

	/// <summary>
	/// Calculate the start page url.
	/// </summary>
	private string GetDefaultUrl()
	{
		string url = null;

	    if (usrHomePage == null)
		{
			using (PXDataRecord usrPref = PXDatabase.SelectSingle<UserPreferences>(new PXDataField<UserPreferences.homePage>(), new PXDataFieldValue<UserPreferences.userID>(PXAccess.GetUserID())))
			{
				if (usrPref != null)
					usrHomePage = usrPref.GetGuid(0);
			}
		}

		if (usrHomePage != null)
			url = ResolveLastUrl(usrHomePage);

	    if (url == null) 
			url = ResolveLastUrl(SitePolicy.HomePage);

	    if (url == null)
	        url = ResolveUrl(PXSiteMap.DefaultFrame);

        // start page for portal
        if (PXSiteMap.IsPortal)
        {
			Guid? portalHomePage = SitePolicy.PortalHomePage;
			if (portalHomePage != null)
			{
				PXSiteMapNode nodedashboard =
					(PXSiteMapNode)PXSiteMap.Provider.FindSiteMapNodeFromKey(portalHomePage.Value);
				if (nodedashboard != null)
					return ResolveUrl(nodedashboard.Url);

				if (PXSiteMap.WikiProvider.GetAccessRights(portalHomePage.Value) >= PXWikiRights.Select)
				{
					PXWikiMapNode node =
						(PXWikiMapNode)PXSiteMap.WikiProvider.FindSiteMapNodeFromKey(portalHomePage.Value);
					if (node != null)
						return ResolveUrl(node.Url);
				}
			}

			for (int i = 0; i < 9; i++)
			{
				string screenname = "SP_00_00_0" + i.ToString();
				WikiPage page = PXSelect<WikiPage,
					Where<WikiPage.name, Equal<Required<WikiPage.name>>>>.SelectWindowed(new PXGraph(), 0, 1, screenname);
				if (page != null)
					if (PXSiteMap.WikiProvider.GetAccessRights(page.PageID.Value) >= PXWikiRights.Select)
					{
						PXWikiMapNode node =
							(PXWikiMapNode)PXSiteMap.WikiProvider.FindSiteMapNodeFromKey(page.PageID.GetValueOrDefault());
						if (node != null)
						{
							return ResolveUrl(node.Url);
						}
					}
			}
		}
		return url;
	}

	/// <summary>
	/// Gets the page Url without session identifier.
	/// </summary>
	private static string GetPureLastUrl(string url)
	{
		int i1 = url.IndexOf("/(W("), i2 = url.IndexOf("/", i1 + 1);
		if (i1 > 0) url = url.Substring(0, i1) + url.Substring(i2);
		return url;
	}

	/// <summary>
	/// Gets the full page Url by specified page identifier.
	/// </summary>
	private static string ResolveLastUrl(object homePage)
	{
		if (homePage == null) return null;
		PXSiteMapNode lastUrlnode = PXSiteMap.Provider.FindSiteMapNodeFromKey((Guid)homePage);
		
		while (lastUrlnode != null && string.IsNullOrEmpty(lastUrlnode.Url))
			lastUrlnode = (PXSiteMapNode)PXSiteMap.Provider.GetParentNode(lastUrlnode);

		string url = string.Empty;
		if (lastUrlnode != null && !string.IsNullOrEmpty(lastUrlnode.Url))
		{
			url = lastUrlnode.Url;
			if (url.StartsWith("~/")) url = url.Remove(0, 2);
		}
		return url;
	}

	/// <summary>
	/// Check access right for specified Url.
	/// </summary>
	private bool VerifyRightsOnScreen(string screenUrl)
	{
		PXSiteMapNode node = PXSiteMap.Provider.FindSiteMapNode(screenUrl) as PXSiteMapNode;
		return node == null ? false : PXAccess.VerifyRights(node.ScreenID);
	}

	//---------------------------------------------------------------------------
	/// <summary>
	/// Initialize the search control.
	/// </summary>
	private void InitSearchBox()
	{
		PXSearchBox box = this.searchBox;
		box.SearchNavigateUrl = this.ResolveUrl("~/Search/Search.aspx?am=") + (!string.IsNullOrEmpty(this.activeModule) ? this.activeModule : this.activeSystem) + "&query=";

		if (!PXSiteMap.IsPortal)
		{
			SiteMapNode currenttopnode = PXSiteMap.Provider.FindSiteMapNodeFromKey(this.activeModule);
			if (currenttopnode != null && currenttopnode.ParentNode != null && currenttopnode.ParentNode.Title == "Help")
				box.SearchNavigateUrl = this.ResolveUrl("~/Search/Search.aspx?am=") + this.activeModule + "&st=ActiveWiki" +
																"&query=";
			else
				box.SearchNavigateUrl = this.ResolveUrl("~/Search/Search.aspx?am=") + this.activeModule +
																"&st=ActiveModule" + "&query=";
		}
		else
		{
			box.Style[HtmlTextWriterStyle.MarginRight] = Unit.Pixel(20).ToString();
		}

		box.Text = PXMessages.LocalizeNoPrefix(PX.AscxControlsMessages.SearchBox.TypeYourQueryHere);
		box.ToolTip = PXMessages.LocalizeNoPrefix(PX.AscxControlsMessages.SearchBox.SearchSystem);
		box.AlreadyLocalized = true;
		box.AddNewVisible = false;
		((ICompositeControlDesignerAccessor)box).RecreateChildControls();
	}

	/// <summary>
	/// Gets the root nodes data source.
	/// </summary>
	private List<SiteMapNode> GetNavPanelDataSource()
	{
		List<SiteMapNode> source = new List<SiteMapNode>();
		SiteMapNode favorite = null;
		string module = this.activeModule, system = this.activeSystem;
		SiteMapNodeCollection nodes = null, nodes2 = null;

		if (!string.IsNullOrEmpty(system))
		{
			SiteMapNode node2 = null;
			foreach (SiteMapNode node in System.Web.SiteMap.RootNode.ChildNodes)
				if (node.Key == system) { nodes = node.ChildNodes; node2 = node; break; }
			if (node2 != null && nodes.Count == 0) source.Add(node2);
		}

		foreach (SiteMapNode node in nodes)
		{
			PXSiteMapNode pxNode = node as PXSiteMapNode;
			bool flag = true;
			if (pxNode != null)
			{
				if (pxNode.ScreenID == "FV000000") { favorite = node; flag = false; }
				else if (pxNode.ScreenID == "HD000000") flag = false;
			}

			if (!string.IsNullOrEmpty(module))
			{
				if (module == node.Key)
				{ 
					nodes2 = node.ChildNodes;
					if (nodes2.Count == 0) source.Add(node);
					break;
				}
			}
			else if (flag) source.Add(node);
		}

		if (nodes2 != null) foreach (SiteMapNode node in nodes2)
			{
				PXSiteMapNode pxNode = node as PXSiteMapNode;
				if (pxNode == null || (pxNode.ScreenID != "HD000000")) source.Add(node);
			}
		
		bool? fvExists = PXContext.Session.FavoritesExists["FavoritesExists"];
		if (fvExists == null)
		{
			fvExists = PX.Data.PXSiteMap.FavoritesProvider.FavoritesExists();
			PXContext.Session.FavoritesExists["FavoritesExists"] = fvExists;
		}

		//if ((bool)fvExists && favorite != null) source.Insert(0, favorite);
		return source;
	}
	

	/// <summary>
	/// Sets the logo image for active company.
	/// </summary>
	private void SetCompanyLogo()
	{
		// set company logo
		Guid? fileId = SitePolicy.GetCompanyLogoFileID(PXAccess.GetBranchCD());
		if (fileId != null)
		{
			this.logoImg.ImageUrl = ControlHelper.GetAttachedFileUrl(this, fileId.Value.ToString());
		}

		string url = GetDefaultUrl();
		if (string.IsNullOrEmpty(url)) url = this.ResolveUrl("~/Frames/Default.aspx");
		url = ControlHelper.FixHideScriptUrl(url, true);
		this.logoCell.HRef = url;
	}

	//---------------------------------------------------------------------------
	/// <summary>
	/// Gets the active user name.
	/// </summary>
	private string GetUserName(bool addBranch)
	{
		if (PXContext.PXIdentity.Authenticated)
		{
			userName = PXContext.PXIdentity.IdentityName;
			if (addBranch)
			{
				string branch = PXAccess.GetBranchCD();
				if (!string.IsNullOrEmpty(branch)) userName += ":" + branch;
			}
		}
		return this.userName;
	}
	
	/// <summary>
	/// Gets the active time zone.
	/// </summary>
	private string GetTimeZone()
	{
		if (timeZone == null) timeZone = LocaleInfo.GetTimeZone().ShortName;
		return timeZone;
	}

	/// <summary>
	/// Gets the current version of the system.
	/// </summary>
	private string GetVersion(bool raw)
	{
		string ver = PXVersionInfo.Version;
		if (!raw) ver = PXMessages.LocalizeFormatNoPrefix(PX.AscxControlsMessages.PageTitle.Version, ver);

		string cstProjects = Customization.CstWebsiteStorage.PublishedProjectList;
		if (!string.IsNullOrEmpty(cstProjects))
			ver += " " + PXMessages.LocalizeNoPrefix(PX.AscxControlsMessages.PageTitle.Customization) + cstProjects;
		return ver;
	}

	/// <summary>
	/// Gets the active screen number.
	/// </summary>
	private string GetScreenID()
	{
		if (this.screenID == null) this.screenID = ControlHelper.GetScreenID();
		return this.screenID;
	}

	/// <summary>
	/// Returns true if site support context navigation.
	/// </summary>
	private bool IsContextNavigation()
	{
		var cn = System.Configuration.ConfigurationManager.AppSettings["ContextNavigation"];
		return !string.IsNullOrEmpty(cn) && cn.ToLower() == "true";
	}
    /// <summary>
    /// Returns true if site required to disable showing activity totals.
    /// </summary>
    private bool IsActivityTotalsVisible()
    {
        var cn = System.Configuration.ConfigurationManager.AppSettings["ActivityTotalsVisible"];
        return !string.IsNullOrEmpty(cn) && cn.ToLower() == "true";
    }
	#endregion

	#region ITitleModuleController

	void ITitleModuleController.AppendControl(Control control)
	{
		Page.Form.Controls.Add(control);
	}

	void ITitleModuleController.AppendToolbarItem(PXToolBarItem item)
	{
		var cont = item as PXToolBarContainer;
		if (cont != null && cont.TemplateContainer.Controls.Count > 0)
		{
			var but = cont.TemplateContainer.Controls[0] as PXNewsCheckerButton;
			if (but != null)
			{
				but.NormalCss = toolsBar.Styles.Normal.CssClass;
				but.HoverCss = toolsBar.Styles.Normal.CssClass;
				but.PushedCss = toolsBar.Styles.Pushed.CssClass;
				toolsBar.Items.Insert(1, item);
			}
		}
	}

	Page ITitleModuleController.Page
	{
		get { return this; }
	}
	#endregion

	#region Fields
	private const string _favoritesUrl = "~/pages/sm/sm203020.aspx";
	private const string _favorites = "favorites";
	private const string _favTreeID = "favTree";
	private const string _activeSystemKey = "__activeSystem";
	private const string _activeModuleKey = "__activeModule";
	private const string _activeSubModKey = "__activeSubMod";
	private const string _activePanelKey = "__activePanel";
	private const string _LABLE_FORMAT = "{0} {1}";
	private const string _LABLE_FORMAT_RTL = "{1} <span dir=\"rtl\" style=\"direction: rtl;\">{0}</span>";
	private const string _LABLE_FULLFORMAT = "{0} {1} ({2})";
	private const string _LABLE_FULLFORMAT_RTL = "({2}) {1} <span dir=\"rtl\" style=\"direction: rtl;\">{0}</span>";


	//private bool bindComplete = false;
	private string activeSystem = "", activeModule = "", activeSubMod = "";
	private bool favoritesActive = false, reminderInitialized = false;
	private bool companySwitched = false;
	private SiteMapNode activeNode;
	private string activePanelIndex = "0", activePanelKey;
	private string screenID, userName, timeZone;
	private PXSmartPanel activeNavPanel = null;
	private Guid? usrHomePage = null;

	#endregion
}
