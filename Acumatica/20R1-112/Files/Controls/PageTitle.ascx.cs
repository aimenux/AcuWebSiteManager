#region Copyright (c) 1994-2006 PXSoft, Inc. All rights reserved.

/* ---------------------------------------------------------------------*
*                               PXSoft, Inc.                            *
*              Copyright (c) 1994-2006 All rights reserved.             *
*                                                                       *
*                                                                       *
* This file and its contents are protected by United States and         *
* International copyright laws.  Unauthorized reproduction and/or       *
* distribution of all or any portion of the code contained herein       *
* is strictly prohibited and will result in severe civil and criminal   *
* penalties.  Any violations of this copyright will be prosecuted       *
* to the fullest extent possible under law.                             *
*                                                                       *
* UNDER NO CIRCUMSTANCES MAY THE SOURCE CODE BE USED IN WHOLE OR IN     *
* PART, AS THE BASIS FOR CREATING A PRODUCT THAT PROVIDES THE SAME, OR  *
* SUBSTANTIALLY THE SAME, FUNCTIONALITY AS ANY PXSoft PRODUCT.          *
*                                                                       *
* THIS COPYRIGHT NOTICE MAY NOT BE REMOVED FROM THIS FILE.              *
* --------------------------------------------------------------------- *
*/

#endregion Copyright (c) 1994-2006 PXSoft, Inc. All rights reserved.

#region

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Web;
using System.Web.Configuration;
using System.Web.UI;
using System.Web.UI.WebControls;
using PX.Common;
using PX.Data;
using PX.Data.DependencyInjection;
using PX.SM;
using PX.Web.Controls;
using PX.Web.Controls.Wiki;
using PX.Web.UI;
using System.Web.UI.HtmlControls;
using System.Globalization;
using System.Xml;
using CommonServiceLocator;
using PX.Web.UI.Frameset.Services;
using Autofac;
using PX.Web.UI.Frameset.Model.DAC;

#pragma warning disable 618

#endregion

[Themeable(true)]
public partial class User_PageTitle : TitlePanel, ITitleModuleController
{
	#region Public properties

	private HtmlAnchor LabelScreen
	{
		get
		{
			if (labelScreen == null)
			{
				PXToolBarContainer cont = (PXToolBarContainer)tlbPath.Items["title"];
				foreach (Control c in cont.TemplateContainer.Controls)
					if (c.ID == "lblScreenTitle") { labelScreen = c as HtmlAnchor; break; }
			}
			return labelScreen;
		}
	}

	public PXDataViewBar DataViewBar
	{
		get { return this.tlbDataView; }
	}

	//---------------------------------------------------------------------------
	/// <summary>
	/// Gets or sets the skin to apply to the control.
	/// </summary>
	[Browsable(true)]
	public override string SkinID
	{
		get { return base.SkinID; }
		set { base.SkinID = value; }
	}

	/// <summary>
	/// Gets or sets the screen image url.
	/// </summary>
	[Category("Appearance"), Description("The screen logo image url.")]
	public string ScreenImage
	{
		get { return screenImage; }
		set { screenImage = value; }
	}

	/// <summary>
	/// Gets or sets the screen identifier.
	/// </summary>
	[Category("Appearance"), Description("The screen identifier.")]
	public override string ScreenID
	{
		get { return screenID; }
		set { screenID = value; }
	}

	/// <summary>
	/// If true - prevent redirect to features configuration.
	/// </summary>
	public bool WillShowWelcomePage
	{
		get { return willShowWelcomePage; }
		set { willShowWelcomePage = value; }
	}

	/// <summary>
	/// Gets or sets the screen title.
	/// </summary>
	[Category("Appearance"), Description("The screen title.")]
	public override string ScreenTitle
	{
		get { return screenTitle; }
		set
		{
			screenTitle = value;

			if (LabelScreen != null)
			{
				LabelScreen.InnerText = screenTitle;
				Page.Title = HttpUtility.HtmlEncode(screenTitle);
			}
		}
	}
	public string ScreenUrl
	{
		get { return screenUrl; }
		set
		{
			screenUrl = value;

			if (LabelScreen != null)
			{
				LabelScreen.HRef = screenUrl;				
			}

		}
	}

	/// <summary>
	/// Gets or sets branch visibility in title.
	/// </summary>
	public bool BranchAvailable
	{
		get
		{
			if (tlbPath != null && tlbPath.Items != null && tlbPath.Items.Count > 0)
			{
				return tlbPath.Items["branch"].Visible;
			}
			return false;
		}
		set
		{
			if (tlbPath != null && tlbPath.Items != null && tlbPath.Items.Count > 0)
			{
				tlbPath.Items["branch"].Visible = value;
				//tlbPath.Items["branchLabel"].Visible = value;
				//tlbPath.Items["branchSep"].Visible = value;
			}
		}
	}

	//---------------------------------------------------------------------------
	/// <summary>
	/// Gets or sets the favorites availability.
	/// </summary>
	[Category("Appearance"), Description("The favorites availability."), DefaultValue(true)]
	public override bool FavoriteAvailable
	{
		get { return favoriteAvalable; }
		set
		{
			favoriteAvalable = value;
			tlbPath.Items["favorites"].Visible = value;
		}
	}

	/// <summary>
	/// Gets or sets the refresh availability.
	/// </summary>
	[Category("Appearance"), Description("The refresh availability.")]
	public bool RefreshAvailable
	{
		get { return Button(_TOOLBAR_REFRESH).Visible; }
		set { Button(_TOOLBAR_REFRESH).Visible = value; }
	}

	/// <summary>
	/// Gets or sets the logount availability.
	/// </summary>
	[Category("Appearance"), Description("The help availability.")]
	public bool HelpAvailable
	{
		get { return Button(_TOOLBAR_HELP).Visible; }
		set { Button(_TOOLBAR_HELP).Visible = value; }
	}

	/// <summary>
	/// Gets or sets the customization availability.
	/// </summary>
	[Category("Appearance"), Description("The customization availability.")]
	public override bool CustomizationAvailable
	{
		get { return customizationAvalable; }
		set { customizationAvalable = value; }
	}

	/// <summary>
	/// Gets or sets the web services availability.
	/// </summary>
	[Category("Appearance"), Description("The web services availability.")]
	public override bool WebServicesAvailable
	{
		get { return webServicesAvailable; }
		set { webServicesAvailable = value; }
	}

	/// <summary>
	/// Gets or sets the audit history availability.
	/// </summary>
	[Category("Appearance"), Description("The audit history availability.")]
	public override bool AuditHistoryAvailable
	{
		get { return auditHistoryAvailable; }
		set { auditHistoryAvailable = value; }
	}

	#endregion

	#region Event handlers

	//---------------------------------------------------------------------------
	/// <summary>
	/// The page Init event handler.
	/// </summary>
	protected void Page_Init(object sender, EventArgs e)
	{
        var lifetimeScope = this.Context.GetLifetimeScope();
        if (lifetimeScope != null)
        {
            screenRepository = lifetimeScope.Resolve<IScreenRepository>();
        }
        else
        {
            var serviceLocator = ServiceLocator.Current;
            screenRepository = serviceLocator.GetInstance<IScreenRepository>();
        }
		JSManager.RegisterModule(new MSScriptRenderer(Page.ClientScript), typeof(AppJS), AppJS.PageTitle);

		if (screenID == null)
		{
			this.screenID = ControlHelper.GetScreenID();
		}
		if (screenTitle == null)
		{
			if (System.Web.SiteMap.CurrentNode != null)
			{
				if (company == null || System.Web.SiteMap.CurrentNode.ParentNode != null)
					screenTitle = PXSiteMap.CurrentNode.Title;
				else
					screenTitle = company;
			}
		}

		string hide = this.Page.Request.QueryString[PXUrl.HidePageTitle];
		if (!string.IsNullOrEmpty(hide)) { this.Visible = false; return; }

		this.Page.InitComplete += new EventHandler(Page_InitComplete);
		PXCallbackManager.GetInstance().PreGetCallbackResult += PreGetCallbackResult;

		tlbPath.Items["syncTOC"].Visible = false;
		tlbPath.Items["branch"].Visible = false;
		if (!this.Page.IsCallback) ((WebControl)LabelScreen.Parent).CssClass = "pageTitleCont";

		if (PXDataSource.RedirectHelper.IsPopupPage(Page))
		{
			if (!PXList.Provider.HasList(PXSiteMap.CurrentNode.ScreenID))
				tlbPath.Items["syncTOC"].Visible = false;
			this.FavoriteAvailable = false;
			if (!PXDataSource.RedirectHelper.IsPopupInline(Page))
				GetBranchCombo().Enabled = false;
		}

		if (PXSiteMap.IsPortal)
		{
			this.CustomizationAvailable = PXAccess.GetAdministratorRoles().Any(System.Web.Security.Roles.IsUserInRole);
			this.BranchAvailable = false;
			this.FavoriteAvailable = false;
			if (ControlHelper.IsRtlCulture()) pnlTBR.CssClass = "panelTBRSP-rtl"; else pnlTBR.CssClass = "panelTBRSP";
		}
		else
		{
			if (ControlHelper.IsRtlCulture())
			{
				pnlTBR.CssClass = "panelTBL";
				pnlTBL.CssClass = "panelTBR";
			}
		}

		if (PXContext.PXIdentity.Authenticated)
		{
			userName = PXContext.PXIdentity.IdentityName;
			string branch = PXAccess.GetBranchCD();
			if (!string.IsNullOrEmpty(branch)) userName += ":" + branch;
		}

		var date = PXContext.GetBusinessDate();
		if (date != null) PXDateTimeEdit.SetDefaultDate((DateTime)date);

		if (!Page.IsCallback) Session.Remove("StoredSearch");
    }

    protected void PreGetCallbackResult(PXCallbackManager sender, XmlWriter writer)
	{
		FillBrachesList();
	}

	protected void Page_InitComplete(object sender, EventArgs e)
	{
		this.InitializeModules();
		if (!this.Page.IsCallback)
			this.FillBrachesList();
		this.InitAuditMenu();

		if (!this.Page.IsCallback || ControlHelper.IsReloadPage(tlbPath))
		{
			var favBtn = (PXToolBarButton)tlbPath.Items["favorites"];
			PXSiteMapNode currentNode = System.Web.SiteMap.CurrentNode as PXSiteMapNode;
            try
            {
			if (currentNode == null || String.IsNullOrEmpty(currentNode.ScreenID))
			{
				favBtn.Visible = false;
			}
			else
			{
				tlbPath.CallbackUpdatable = favBtn.DynamicText = true;
				var entryScreenId = GetEntryNodeId(PXSiteMap.CurrentNode.ScreenID);
	                    if (!entryScreenId.HasValue)
	                    {
	                        favBtn.Visible = false;
	                    }
	                    else
	                    {
			            	favBtn.Pushed = this.IsInFavorites(entryScreenId.Value);
				favBtn.Tooltip = favBtn.Pushed ? InfoMessages.RemoveFromFavorites : InfoMessages.AddToFavorites;
			}
		}
            }
            catch (Exception ex)
            {                
                PXTrace.WriteError(ex);
            }
		}

		// Force the customization controls creation !!!
		this.EnsureChildControls();

	    
    }

	protected void Page_PreRender(object sender, EventArgs e)
	{
		RegisterModules();
	}

	//---------------------------------------------------------------------------
	/// <summary>
	/// The page Load event handler.
	/// </summary>
	protected void Page_Load(object sender, EventArgs e)
	{
		var ds = PXPage.GetDefaultDataSource(this.Page);
		if (ds != null && ds.HasActiveContext)
		{
			if (ControlHelper.IsReloadPage(this))
			{
				var ctx = ds.DataGraph.GetContextViewDescriptor();
				if (ctx != null)
				{
					if (ctx.HeaderValues != null)
					{
						screenTitle = string.Join(" - ", ctx.HeaderValues).ToUpper();
					}
					if (this.Visible)
					{
					var cm = PXCallbackManager.GetInstance();
					PXCallbackManager.RegisterJavaScript(
						string.Format("px.elemByID('{0}').innerHTML = '{1}';", LabelScreen.ClientID, screenTitle));
				}
			}
			}
			else screenTitle = " ";
		}

		if (!string.IsNullOrEmpty(screenTitle))
		{
			this.ScreenTitle = HttpUtility.HtmlDecode(screenTitle);
		}
		if (!Page.IsCallback)
		{
			Page.EnableViewState = false;
			RegisterSyncTreeVars();
		}

		if (!this.Visible) return;

		Page.ClientScript.RegisterClientScriptBlock(GetType(), "addToFavorites", "var addToFavorites=\"" + PXMessages.Localize(InfoMessages.AddToFavorites) + "\";", true);
		Page.ClientScript.RegisterClientScriptBlock(GetType(), "removeFromFavorites", "var removeFromFavorites=\"" + PXMessages.Localize(InfoMessages.RemoveFromFavorites) + "\";", true);
		if (!this.Page.IsCallback || ControlHelper.IsReloadPage(tlbPath))
		{
			InitHelpMenu();
			RearrangeAuditMenu();
		}

		if (!willShowWelcomePage &&  this.Request.RawUrl.IndexOf("CS100000.aspx", StringComparison.InvariantCultureIgnoreCase) < 0 &&
						this.Request.RawUrl.IndexOf("/soap/", StringComparison.InvariantCultureIgnoreCase) == -1 &&
						this.Request.RawUrl.IndexOf("/wiki/", StringComparison.InvariantCultureIgnoreCase) == -1)
		{
			if (!PXAccess.FeatureSetInstalled("PX.Objects.CS.FeaturesSet"))
			{
				PXSiteMapNode cs = PXSiteMap.Provider.FindSiteMapNodeByScreenID("CS100000");
				if (cs != null)
				{
					string navigateUrl = ResolveUrl(cs.Url);
					if (!Page.IsCallback) Response.Redirect(navigateUrl);
				}
			}
		}

		string localPath = Request.Url.LocalPath;
		string query = Request.Url.Query;
		if (!PXUrl.IsMainPage(Request.RawUrl) && !query.Contains("PopupPanel=On") && !query.Contains("PopupPanel=Layer"))
		{
			if (!localPath.EndsWith("Default.aspx"))
			{
				string lastUrl = (string)PXContext.Session["LastUrl"];
				if (String.IsNullOrEmpty(lastUrl) || lastUrl.EndsWith("Default.aspx"))
					Controls.Add(new LiteralControl("<script  type=\"text/javascript\">try { window.top.lastUrl=null; } catch (ex) {}</script>\n"));
			}
			PXContext.Session.SetString("LastUrl", Request.RawUrl);
		}

		if (!Page.IsPostBack && !String.IsNullOrEmpty(ScreenID))
			PX.Data.PXAuditJournal.Register(ScreenID);

		if (!string.IsNullOrEmpty(screenTitle))
		{
			string relPath = ResolveUrl(Request.RawUrl);
			string url = ResolveUrl(Request.RawUrl);
			LabelScreen.HRef = (Page != null && Page.GetType().Name == "wiki_showwiki_aspx") ? PXSessionStateStore.GetSessionUrl(HttpContext.Current, url) : url;

			if (PXSiteMap.CurrentNode != null && PXList.Provider.HasList(PXSiteMap.CurrentNode.ScreenID))
			{
				string listScreenID = PXList.Provider.GetListID(PXSiteMap.CurrentNode.ScreenID);
				PXSiteMapNode listNode = PXSiteMap.Provider.FindSiteMapNodeByScreenID(listScreenID);
				if (listNode != null)
				{
					LabelScreen.HRef = ResolveUrl(listNode.Url);
					LabelScreen.ServerClick += (o, args) =>
					{
						PXList.Provider.SetCurrentSearches(PXSiteMap.CurrentNode.ScreenID, null);
						Response.Redirect(ControlHelper.FixHideScriptUrl(PXPageCache.FixPageUrl(ResolveUrl(listNode.Url)), false));
					};
				}
			}
			if (screenUrl != null)
				LabelScreen.HRef = screenUrl;

			this.ScreenTitle = screenTitle;
		}

		if (!Page.IsCallback)
		{
			Page.ClientScript.RegisterClientScriptBlock(GetType(), "toolbarNum", "var __toolbarID=\"" + this.tlbTools.ClientID + "\";", true);
		}
		if (ControlHelper.IsReloadPage(this))
		{
			SharedColumnSettings.SaveGrids();
		}
	}
	protected void shareColumnsDlg_LoadContent(object sender, EventArgs e)
	{
		PXGraph.CreateInstance<SharedColumnSettings>().Clear();
	}

	protected void PanelDynamicForm_LoadContent(object sender, EventArgs e)
	{
		PanelDynamicForm.Caption = PX.Web.Controls.Messages.Transition;
		PanelDynamicForm.DefaultMsgText = PX.Web.Controls.Messages.Transition;
	}

	//---------------------------------------------------------------------------
	/// <summary>
	/// The About panel load event handler.
	/// </summary>
	protected void pnlAbout_LoadContent(object sender, EventArgs e)
	{
		PXLabel lbl = (PXLabel)pnlAbout.FindControl("lblVersion");
		lbl.Text = this.GetVersion(false);

		lbl = (PXLabel)pnlAbout.FindControl("lblAcumatica");
		lbl.Text = "Acumatica " + PXVersionInfo.ProductVersion;

		if (PX.SM.UpdateMaint.CheckForUpdates())
		{
			lbl = (PXLabel)pnlAbout.FindControl("lblUpdates");
			lbl.Text = PXMessages.LocalizeFormatNoPrefix(PX.AscxControlsMessages.PageTitle.Updates, PXVersionInfo.Version);
			lbl.Style["display"] = "";
		}

        var lastRestoredSnapshot = CompanyMaint.GetLastRestoredSnapshot();

        if (lastRestoredSnapshot != null 
            && lastRestoredSnapshot.IsSafe != null 
            && !lastRestoredSnapshot.IsSafe.Value
            && lastRestoredSnapshot.Dismissed != null
            && !lastRestoredSnapshot.Dismissed.Value)
        {
            lbl = (PXLabel)pnlAbout.FindControl("lblRestoredSnapshotIsUnsafe");
            lbl.Text = PXMessages.LocalizeFormatNoPrefixNLA(PX.Data.Update.Messages.UnsafeSnapshotRestoredShort,
                lastRestoredSnapshot.CreatedDateTime != null ? lastRestoredSnapshot.CreatedDateTime.ToString() : "uknown date");
            lbl.ForeColor = System.Drawing.Color.Red;
            lbl.Style["display"] = "";
        }

		lbl = (PXLabel)pnlAbout.FindControl("lblCopyright2");
		lbl.Text = PXMessages.LocalizeFormatNoPrefix(PX.AscxControlsMessages.PageTitle.Copyright2);

		lbl = (PXLabel)pnlAbout.FindControl("lblInstallationID");
		// hiding InstallationID if it is empty
		if (String.IsNullOrEmpty(PXVersionInfo.InstallationID)) lbl.Visible = false;
		lbl.Text = PXMessages.LocalizeFormatNoPrefix(PX.AscxControlsMessages.PageTitle.InstallationID, PXLicenseHelper.InstallationID);

		string copyR = PXVersionInfo.Copyright;
		lbl = (PXLabel)pnlAbout.FindControl("lblCopyright1");
		if (!string.IsNullOrEmpty(copyR)) lbl.Text = copyR;
	}

	/// <summary>
	/// The Audit panel load event handler.
	/// </summary>
	protected void pnlAudit_LoadContent(object sender, EventArgs e)
	{
		PXDataSource datasource = ((PXPage)this.Page).DefaultDataSource;

		PX.Data.Process.AUAuditPanelInfo info = null;
		if (datasource != null) info = PX.Data.Process.PXAuditHelper.CollectInfo(datasource.DataGraph, datasource.PrimaryView);
		if (info != null)
		{
			foreach (String field in PX.Data.Process.PXAuditHelper.FIELDS)
			{
				PXTextEdit edit = (PXTextEdit)frmAudit.FindControl("ed" + field);
				if (edit != null)
				{
					Object result = typeof(PX.Data.Process.AUAuditPanelInfo).InvokeMember(field, System.Reflection.BindingFlags.GetProperty | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance, null, info, null);
					edit.Text = result == null ? null : result.ToString();
				}
			}
		}
		else
		{
			frmAudit.Visible = false;
			((PXLabel)pnlAudit.FindControl("lblWarning")).Visible = true;
		}

		PXSiteMapNode node = PXSiteMap.Provider.FindSiteMapNodeByScreenID("SM205510");
		if (node != null)
		{
			PXButton btnActivate = (PXButton)pnlAuditButtons.FindControl("btnAuditActivate");
			btnActivate.Visible = true;
			btnActivate.NavigateUrl = String.Concat(ResolveUrl(node.Url), "?ScreenId=", this.screenID.Replace(".", ""));
		}
	}

	//---------------------------------------------------------------------------
	protected void tlbTools_CallBack(object sender, PXCallBackEventArgs e)
	{
		if (e.Argument == "refresh")
		{
			if (!PXSiteMap.IsPortal || !Page.Request.Path.ToLower().Contains("reportlauncher"))
			{
				Session.Remove(VirtualPathUtility.ToAbsolute(Page.Request.Path));
				RefreshByRedirect();
			}
		}

		if (e.Argument == "help")
		{
			throw new Exception("Redirect:" + HelpUrl);
		}

		if (e.Command.Name == "auditHistory")
			btnAuditDetails_CallBack(sender, e);
	}
	protected void btnAuditDetails_CallBack(object sender, PXCallBackEventArgs e)
	{
		if (this.Page is PXPage)
		{
			PXDataSource dataSource = ((PXPage)this.Page).DefaultDataSource;
			if (dataSource != null)
			{
				string key = PX.Data.Process.PXAuditHelper.CollectAudit(dataSource.DataGraph, dataSource.PrimaryView);
				string message = null;

				if (key != null) 
					message = String.Concat("Redirect4:", ResolveUrl("~/frames/audit.aspx"), "?key=", key, "&preserveSession=true");
				else 
					message = PX.Data.ErrorMessages.AuditNotAvailable;

				throw new Exception(message);
			}
		}
	}

	private void RefreshByRedirect()
	{
		string screen = PXSiteMap.CurrentScreenID ?? ScreenID;
		if (!string.IsNullOrEmpty(screen))
		{
			screen = screen.Replace(".", string.Empty);
			if (screen.Length > 2 && screen.Substring(2) == "000000")
				throw new Exception(string.Concat("Redirect:", ResolveUrl/**/("~/frames/default.aspx"), "?scrid=", screen));
		}
		throw new Exception("Redirect:" + Request.RawUrl);
	}
	#endregion

	#region Method to work with Branches

	/// <summary>
	/// Find the braches drop down control.
	/// </summary>
	private PXDropDown GetBranchCombo()
	{
		PXToolBarContainer cont = (PXToolBarContainer)tlbPath.Items["branch"];
		PXDropDown dropDown = null;
		foreach (Control c in cont.TemplateContainer.Controls)
			if (c.ID == "cmdBranch") { dropDown = c as PXDropDown; break; }
		return dropDown;
	}

	/// <summary>
	/// Fill the branches drop-down list.
	/// </summary>
	private void FillBrachesList()
	{
		PXToolBarContainer cont = (PXToolBarContainer)tlbPath.Items["branch"];
		PXDropDown dropDown = this.GetBranchCombo();
		if (ControlHelper.IsReloadPage(dropDown)) dropDown.CallbackUpdatable = true;

		string currBranchId = null;
		int? currentBranch = PXAccess.GetBranchID();

		List<PXAccess.BranchCollection.Info> items = GetBranches().ToList();
		items.Sort((a, b) => string.Compare(a.Cd, b.Cd));

		dropDown.Items.Clear();
		foreach (PXAccess.BranchCollection.Info item in items)
		{
			var current = (currentBranch != null && currentBranch.Equals(item.Id));
			dropDown.Items.Add(new PXListItem(item.Name, item.Id.ToString()));
			if (current) currBranchId = item.Id.ToString();
		}

		if (PXAccess.FeatureInstalled("PX.Objects.CS.FeaturesSet+Branch") && dropDown.Items.Count > 0)
		{
			if (ControlHelper.IsReloadPage(tlbPath)) tlbPath.CallbackUpdatable = true;
			if (currentBranch != null) dropDown.Value = currBranchId;
			else dropDown.Items.Insert(0, new PXListItem("Select Branch", null));
			dropDown.ItemChanged += Branch_ItemClick;
		}
		else
		{
			cont.Visible = false;
			PXToolBarLabel lbl = tlbPath.Items["branchLabel"] as PXToolBarLabel;
			if (lbl != null) lbl.Visible = false;
			lbl = tlbPath.Items["branchSep"] as PXToolBarLabel;
			if (lbl != null) lbl.Visible = false;
		}
	}

	/// <summary>
	/// Gets the braches enumerator.
	/// </summary>
	private IEnumerable<PXAccess.BranchCollection.Info> GetBranches()
	{
		var currentUser = PXAccess.GetUserName();
		foreach (PXAccess.BranchCollection.Info item in PXAccess.GetBranches(currentUser,true))
			yield return item;
	}

	/// <summary>
	/// The branch menu item click event handler.
	/// </summary>
	private void Branch_ItemClick(object sender, PXListItemEventArgs e)
	{
		var targetBranch = int.Parse(e.Item.Value);
		if (targetBranch < 1) return;

		PXDropDown dropDown = this.GetBranchCombo();
		foreach (PXAccess.BranchCollection.Info item in GetBranches())
			if (targetBranch.Equals(item.Id))
			{
				PXContext.SetBranchID(targetBranch);
				HttpCookie branchCooky = HttpContext.Current.Response.Cookies["UserBranch"];
				if (dropDown != null) dropDown.Value = item.Id;

				//if (this.Page is PXPage) ((PXPage)this.Page).ResetCurrentUser(HttpContext.Current);

				string branchObj = targetBranch.ToString();
				if (branchCooky == null)
					HttpContext.Current.Response.Cookies.Add(new HttpCookie("UserBranch", branchObj));
				else
					branchCooky.Value = branchObj;
				break;
			}
	}
	#endregion

	#region Methods to work with Favorites

	/// <summary>
	/// The tlbPath callback event handler.
	/// </summary>
	protected void tlbPath_CallBack(object sender, PXCallBackEventArgs e)
	{
		if (e.Command.Name == "AddFav" && PXSiteMap.CurrentNode != null)
		{
			var currentNode = PXSiteMap.CurrentNode;
			var entryScreenId = GetEntryNodeId(currentNode.ScreenID);
            if (entryScreenId.HasValue)
            {
                if (!IsInFavorites(entryScreenId.Value))
            {
                    screenRepository.SetFavoriteForScreen(entryScreenId.Value, true);
			}
			else
			{
                    screenRepository.SetFavoriteForScreen(entryScreenId.Value, false);
			}
			}
			
			PXContext.Session.FavoritesExists["FavoritesExists"] = null;

            // check if favorites exists
            e.Result = entryScreenId.HasValue && IsInFavorites(entryScreenId.Value) ? "0" : "1";
		}
	}

	private Guid? GetEntryNodeId(string screenId)
	{
		string entryScreenId = screenId;
		if (PXList.Provider.IsList(screenId))
			entryScreenId = PXList.Provider.GetEntryScreenID(screenId);
		var node = PXSiteMap.Provider.FindSiteMapNodeByScreenID(entryScreenId);

        if (node == null)
        {
            return null;
        }
		return node.NodeID;
	}

	/// <summary>
	/// Check if current node in Favorites.
	/// </summary>
	private bool IsInFavorites(Guid nodeId)
	{
        using (PXDataRecord exist = PXDatabase.SelectSingle<MUIFavoriteScreen>(
                new PXDataField("NodeID"), new PXDataFieldValue("NodeID", nodeId),
                new PXDataFieldValue("IsPortal", PXSiteMap.IsPortal),
                new PXDataFieldValue("Username", PXAccess.GetUserLogin())))
        {
            return exist != null;
        }
	}
	#endregion

	#region Helper methods

	private void RearrangeAuditMenu()
	{
		PXToolBarButton btn = (PXToolBarButton)tlbTools.Items[_TOOLBAR_HELP];

		if (btn != null && btn.MenuItems != null && btn.MenuItems.Count >= 3 && auditMenuEnabled)
		{
			PXMenuItem auditItem = btn.MenuItems[0];
			btn.MenuItems.RemoveAt(0);
			btn.MenuItems.Insert(btn.MenuItems.Count - 3, auditItem);
		}
	}

	//---------------------------------------------------------------------------
	/// <summary>
	/// Initialize the user name button.
	/// </summary>
	private void InitHelpMenu()
	{
		PXToolBarButton btn = (PXToolBarButton)tlbTools.Items[_TOOLBAR_HELP];

		Func<string, string, string, string> func = delegate (string txt, string val, string labelID)
		{
			return string.Format(
					"<div class='size-xs inline-block'>{0}</div> <span id='{1}'>{2}</span>", txt, labelID, val);
		};
		Func<PXMenuItem, PXMenuItem> addItem = delegate (PXMenuItem item) { btn.MenuItems.Add(item); return item; };

		var prefix = "";
		if (Page is PXPage)
		{
			bool isCustomized = ((PXPage)Page).IsPageCustomized;
			if (isCustomized)
			{
				prefix = "CST.";
			}
		}

		PXMenuItem lastItem;

		string menuText = PXMessages.LocalizeNoPrefix(PX.AscxControlsMessages.PageTitle.ScreenID);
		lastItem = addItem(new PXMenuItem(func(menuText, prefix + ControlHelper.SanitizeHtml(this.screenID), "screenID")));
		lastItem.HtmlEncode = false;
		lastItem = addItem(new PXMenuItem(Msg.GetLink) { CommandSourceID = "tlbDataView", CommandName = "LinkShow" });

		PXMenuItem webItem = null;
        PXSiteMapNode siteMapNode = null;
		if (!String.IsNullOrEmpty(this.screenID))
		{
			siteMapNode = PXSiteMap.Provider.FindSiteMapNodeByScreenID(screenID.Replace(".", ""));
			if (WebServicesAvailable && (siteMapNode != null && !String.IsNullOrEmpty(siteMapNode.GraphType)
					&& System.Web.Compilation.PXBuildManager.GetType(siteMapNode.GraphType, false) != null) || (siteMapNode != null && siteMapNode.Url.ToLower().Contains("frames/reportlauncher.aspx")) || (siteMapNode != null && siteMapNode.Url.ToLower().Contains("frames/rmlauncher.aspx")))
			{
				PXMenuItem item = new PXMenuItem(ActionsMessages.WebService);
				item.NavigateUrl = "~/Soap/" + screenID.Replace(".", "") + ".asmx";
				item.OpenFrameset = false;
				item.Target = "_blank";
				item.ShowSeparator = true;
				btn.MenuItems.Add(webItem = item);
			}
		}
		if (webItem == null) lastItem.ShowSeparator = true;

		if (!String.IsNullOrEmpty(this.screenID) && PXSiteMap.CurrentNode != null)
		{
			PXSiteMapNode node = PXSiteMap.CurrentNode;
			if (PXList.Provider.IsList(screenID))
			{
				string entryScreenID = PXList.Provider.GetEntryScreenID(screenID);
				node = PXSiteMap.Provider.FindSiteMapNodeByScreenID(entryScreenID) ?? node;
			}
			string accessUrl = string.Format("~/Pages/SM/SM201020.aspx?Screen={0}", node.NodeID.ToString());
			addItem(new PXMenuItem("Access Rights...") { NavigateUrl = accessUrl, Target = "_blank" });
		}
		if (System.Web.Security.Roles.IsUserInRole(PXAccess.GetAdministratorRoles().First()))
		{
            PXSiteMapNode node = siteMapNode ?? PXSiteMap.CurrentNode;
			var hideSharing = node is PX.Data.PXWikiMapNode ||
				PX.Data.PXSiteMap.IsDashboard(node) ||
				PX.Data.PXSiteMap.IsReport(node.Url) ||
				PX.Data.PXSiteMap.IsARmReport(node.Url) ||
				Page.Request.Url.AbsolutePath.StartsWith(PX.Common.PXUrl.RemoveSessionSplit(Page.ResolveUrl(PX.Olap.Maintenance.PXPivotTableGraph.PivotUri)));
			if (!hideSharing)
			{
				addItem(new PXMenuItem(PX.Web.Controls.Messages.ShareColumnConfiguration)
				{
					PopupPanel = "shareColumnsDlg",
					DefaultMsgText = PX.Web.Controls.Messages.ShareColumnConfiguration
				});
			}
		}
		addItem(new PXMenuItem("Trace...") { NavigateUrl = "~/Frames/Trace.aspx?preserveSession=true", Target = "_blank", Value = "trace" });
		addItem(new PXMenuItem("About...") { PopupPanel = "pnlAbout" });

		btn.Text = PXMessages.LocalizeNoPrefix(PX.AscxControlsMessages.PageTitle.Tools);
		btn.Tooltip = PXMessages.LocalizeNoPrefix(PX.AscxControlsMessages.PageTitle.ToolsTip);
		btn.AlreadyLocalized = true;
	}

	private void InitAuditMenu()
	{
		auditMenuEnabled = false;
		PXToolBarButton btn = (PXToolBarButton)tlbTools.Items[_TOOLBAR_HELP];

		if (AuditHistoryAvailable && !String.IsNullOrEmpty(this.screenID) && PX.Data.Process.PXAuditHelper.IsUserAuditor)
		{
			PXMenuItem item = null;
			if (PX.Data.PXDatabase.AuditRequired(this.screenID.Replace(".", "")))
			{
				item = new PXMenuItem("Audit History...");
				item.AutoCallBack.Command = "auditHistory";
				item.AutoCallBack.ActiveBehavior = true;
				item.AutoCallBack.Behavior.PostData = PostDataMode.Page;
				item.AutoCallBack.Behavior.Name = "auditHistory";
				item.Value = "auditHistory";
			}
			else if (this.Page is PXPage)
			{
				//Don't initialize Audit History button on wiki page
				if (!(PXSiteMap.CurrentNode is PXWikiMapNode))
				{
					PXDataSource datasource = ((PXPage)this.Page).DefaultDataSource;
					if (datasource != null && PX.Data.Process.PXAuditHelper.IsInfoAvailable(datasource.DataGraph, datasource.PrimaryView))
					{
						PXCache cache = datasource.DataGraph.Views[datasource.PrimaryView].Cache;
						item = new PXMenuItem("Audit History...") { PopupPanel = "pnlAudit" };
					}
				}
			}

			if (item != null)
			{
				btn.MenuItems.Add(item);
				auditMenuEnabled = true;
			}
		}
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
	/// Sets the specified control CSS class.
	/// </summary>
	private void SetControlCss(WebControl ctrl, string cssClassName)
	{
		if (string.IsNullOrEmpty(cssClassName)) return;
		ctrl.ControlStyle.Reset();
		ctrl.CssClass = cssClassName;
	}

	//---------------------------------------------------------------------------
	private PXToolBarButton Button(string button)
	{
		foreach (PXToolBarItem item in tlbTools.Items)
			if (item.Key == button) return item as PXToolBarButton;
		return null;
	}

	private bool IsCallback(params string[] paramsEnds)
	{
		if (Page.IsCallback && Request.Params["__CALLBACKID"] != null)
		{
			if (paramsEnds.Length == 0) return true;
			for (int i = 0; i < paramsEnds.Length; i++)
				if (Request.Params["__CALLBACKID"].EndsWith(paramsEnds[i]))
					return true;
		}
		return false;
	}

	public static Guid GetArticleID(string name)
	{
		if (!String.IsNullOrEmpty(name))
		{
			return PXSiteMap.WikiProvider.GetWikiPageIDByPageName(name);
		}
		return Guid.Empty;
	}

	private string HelpUrl
	{
		get
		{
			string help = WebConfigurationManager.AppSettings[helpKey] ?? helpUrl;
			string url;
			if (Page.GetType().FullName == "ASP.wiki_showwiki_aspx")
			{
				url = string.Format(help, Guid.Empty.ToString());
				PreferencesGeneral row = PXSelect<PreferencesGeneral>.Select(new PXGraph());
				if (row != null)
				{
					if (row.HelpPage != null)
						url = string.Format(help, row.HelpPage.ToString());
				}
			}

			else
			{
				// Replace Help link for the List screen by the link from the Entry Screen
				string actualScreenID = ScreenID;
				PXSiteMapNode node = PXSiteMap.CurrentNode;

				string pageName = actualScreenID.With(_ => _.Replace('.', '_').ToString());
				url = string.Format(help, GetArticleID(pageName));
				WikiPage getPage = PXSelect<WikiPage,
					Where<WikiPage.name, Equal<Required<WikiPage.name>>>>.Select(new PXGraph(), pageName);

				if (getPage == null)
				{
					if (node != null && PXList.Provider.IsList(node.ScreenID))
					{
						string entryScreenID = PXList.Provider.GetEntryScreenID(node.ScreenID);
						if (!String.IsNullOrEmpty(entryScreenID))
							actualScreenID = Mask.Format("CC.CC.CC.CC", entryScreenID);
						url = string.Format(help, GetArticleID(actualScreenID.With(_ => _.Replace('.', '_').ToString())));
					}
					else
					{
						if (getPage == null)
						{
							if (node.Url.LastIndexOf("/") > -1)
							{
								string newpageName = node.Url.Substring(node.Url.LastIndexOf("/"), 9);
								if (newpageName.Length == 9)
								{
									pageName = (newpageName[1].ToString() + newpageName[2].ToString() + '_' +
										newpageName[3].ToString() + newpageName[4].ToString() + '_' +
										newpageName[5].ToString() + newpageName[6].ToString() + '_' +
										newpageName[7].ToString() + newpageName[8].ToString());
									pageName = pageName.ToUpper();
									url = string.Format(help, GetArticleID(pageName));
								}
							}
						}
					}
				}
			}
			return HttpUtility.UrlPathEncode(url);
		}
	}
	#endregion

	#region Methods to register script

	private void RegisterSyncTreeVars()
	{
		//var screen = string.Format("{0}{1}", Customization.WebsiteEntryPoints.GetCustomizationMarker(), screenID);
		Page.ClientScript.RegisterClientScriptBlock(GetType(), "screenID",
			"var __screenID=\"" + ControlHelper.SanitizeHtml(screenID) + "\";", true);
	}

	private void RegisterModules()
	{
		IScriptRenderer scriptRenderer = JSManager.GetRenderer(this.Page);
		JSManager.RegisterSystemModules(this.Page);
		JSManager.RegisterCallbackModules(this.Page);
		JSManager.RegisterModule(scriptRenderer, typeof(PXTextEdit), JS.TextEdit);
		JSManager.RegisterModule(scriptRenderer, typeof(PXCheckBox), JS.CheckBox);
		JSManager.RegisterModule(scriptRenderer, typeof(PXDropDown), JS.DropDown);
	}
	#endregion

	#region Variables

	private const string _TOOLBAR_REFRESH = "keyBtnRefresh";
	private const string _TOOLBAR_HELP = "help";
	private const string helpUrl = "~/Wiki/Show.aspx?pageid={0}";
	private const string helpKey = "helpUrl";

	private bool auditMenuEnabled;
	private bool customizationAvalable = true;
	private bool favoriteAvalable = true;
	private bool webServicesAvailable = true;
	private bool auditHistoryAvailable = true;
	private bool willShowWelcomePage = false;
	private string screenImage, screenTitle, screenUrl, screenID, userName;
	private string company = null;
	private HtmlAnchor labelScreen = null;
	
	private IScreenRepository screenRepository;

	#endregion

	#region Customization

	protected override void CreateChildControls()
	{
		base.CreateChildControls();
		if (customizationAvalable && this.Visible)
		{
			Customization.WebsiteEntryPoints.AddCustomizationMenuAndDialogs(CustomizationContainer);
		}

	    
	    InitProcessing();
	    InitAUWorkflow();
	}

    private bool _InitAUWorkflow;

    void InitAUWorkflow()
    {
        if(_InitAUWorkflow)
            return;

        _InitAUWorkflow = true;

        var p = (PXPage)this.Page;

        if (p.DefaultDataSource == null)
            return;

        var graph = p.DefaultDataSource.DataGraph;
        if (PXGraph.ProxyIsActive)
            return;

        PXDynamicFormGenerator.CreateChildControls(graph, FormPreview, true);
    }

    private bool _isInitProcessing;

    void InitProcessing()
    {
        if (_isInitProcessing)
            return;

        _isInitProcessing = true;
        if(!WebConfig.ProcessingProgressDialog)
            return;


        var p = (PXPage) this.Page;

        if (p.DefaultDataSource == null)
            return;

        var graph = p.DefaultDataSource.DataGraph;
		if (graph == null)
			throw new PXScreenMisconfigurationException(PXMessages.LocalizeFormatNoPrefixNLA(Msg.InvalidTypeSpecified, p.DefaultDataSource.TypeName));

		if (!graph.IsProcessing || PXGraph.ProxyIsActive)
            return;

        var c = this.Page.LoadControl("~/Controls/ProcessingDialogs.ascx");
        c.ID = "ProcessingDialogs";
        this.Controls.Add(c);

    }

    #endregion

        #region ITitleModuleController

        void ITitleModuleController.AppendControl(Control control)
	{
		Page.Form.Controls.Add(control);
	}

	void ITitleModuleController.AppendToolbarItem(PXToolBarItem item)
	{
		tlbTools.Items.Insert(1, item);
		if (item.Key == "reminder") item.Visible = false;
	}

	Page ITitleModuleController.Page
	{
		get { return Page; }
	}

	private void InitializeModules()
	{
		List<ITitleModule> list = new List<ITitleModule>(TitleModuleService.Handlers);
		for (int i = list.Count - 1; i >= 0; i--)
		{
			ITitleModule module = list[i];
			module.Initialize(this);
		}
	}


	protected void tlbDataView_DataBound(object sender, EventArgs e)
	{
	}
	#endregion

}

