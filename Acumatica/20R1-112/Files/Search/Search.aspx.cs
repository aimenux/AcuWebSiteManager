using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Web;
using System.Web.Compilation;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;
using HtmlAgilityPack;
using PX.Common;
using PX.Data;
using PX.Data.RichTextEdit;
using PX.Data.Search;
using PX.SM;
using PX.Web.Controls;
using PX.Web.Controls.TitleModules;
using PX.Web.Controls.Wiki;
using PX.Web.UI;
using System.Collections.Specialized;
using Messages = PX.Data.Search.Messages;
using System.Reflection;

public partial class Search_Search : PXPage
{
	#region Controls

	private Control divMessage;
	private Label lblMessage;
	private PXTextEdit txtSearch;
	private HtmlTableRow filterrow;
	private PXButton btnSearch;
	private HtmlTable resultsTable;
	private Label lblFullTextWarning;
	protected LinkButton linkPrev;
	protected LinkButton linkNext;
	private HtmlGenericControl divTips;
	private Label lblSearchTips;
	private HtmlGenericControl liCheckSpelling;
	private HtmlGenericControl liSimplifyQuery;
	private HtmlGenericControl liTryOtherWords;
	private PXLabel lblActiveModule;
	private PXButton btnClearActiveModule;
	private PXButton btnShowList;
	private HtmlTableCell cellList;
	private HtmlTable filtertable;
	#endregion

	private SearchService.SearchLookupType searchType;
	private string activeModule;
	private IList<SearchService.SearchLookupItem> comboBoxLookupList;

	public Guid? ActiveModule
	{
		get
		{
			SearchService.SearchLookupType selectedItem = searchType;

			if (selectedItem == SearchService.SearchLookupType.AllHelp)
				return null;

			Guid result;
			if (Guid.TryParse(activeModule, out result))
			{
				return result;
			}

			return null;
		}
	}

	#region Event handlers

	protected virtual void Page_PreInit(object sender, EventArgs e)
	{
        pageTitle.ScreenID = PXSiteMap.IsPortal ? "SE.00.00.50" : "SE.00.00.40";
		pageTitle.ScreenTitle = PXMessages.LocalizeNoPrefix(Messages.scrSearch);
		pageTitle.FavoriteAvailable = false;
		pageTitle.CustomizationAvailable = false;
	}

	
	protected void Page_Init(object sender, EventArgs e)
	{
		RegisterThisId();

		#region Init Controls
		//Since Controls are placed inside the PXForm we need to manually find them and init the referencing them variables:
		divMessage = PXFormView1.FindControl("divMessage") as Control;
		lblMessage = PXFormView1.FindControl("lblMessage") as Label;
		txtSearch = PXFormView1.FindControl("txtSearch") as PXTextEdit;
		btnSearch = PXFormView1.FindControl("btnSearch") as PXButton;
		filterrow = PXFormView1.FindControl("filterrow") as HtmlTableRow;

		var pnlResults = PXFormView1.FindControl("pnlResults") as PXSmartPanel;
		resultsTable = pnlResults.FindControl("resultsTable") as HtmlTable;
		lblFullTextWarning = pnlResults.FindControl("lblFullTextWarning") as Label;
		linkPrev = pnlResults.FindControl("linkPrev") as LinkButton;
		linkNext = pnlResults.FindControl("linkNext") as LinkButton;
		
		divTips = PXFormView1.FindControl("divTips") as HtmlGenericControl;
		lblSearchTips = PXFormView1.FindControl("lblSearchTips") as Label;
		liCheckSpelling = PXFormView1.FindControl("liCheckSpelling") as HtmlGenericControl;
		liSimplifyQuery = PXFormView1.FindControl("liSimplifyQuery") as HtmlGenericControl;
		liTryOtherWords = PXFormView1.FindControl("liTryOtherWords") as HtmlGenericControl;
		lblSearchTips.Text = PXMessages.LocalizeNoPrefix(Messages.SearchTips);
		liCheckSpelling.InnerText = PXMessages.LocalizeNoPrefix(Messages.CheckSpelling);
		liSimplifyQuery.InnerText = PXMessages.LocalizeNoPrefix(Messages.SimplifyQuery);
		liTryOtherWords.InnerText = PXMessages.LocalizeNoPrefix(Messages.TryOtherWords);
		lblActiveModule = PXFormView1.FindControl("lblActiveModule") as PXLabel;
		btnClearActiveModule = PXFormView1.FindControl("btnClearActiveModule") as PXButton;
		btnShowList = PXFormView1.FindControl("btnShowList") as PXButton;
		cellList = PXFormView1.FindControl("cellList") as HtmlTableCell;
		filtertable = PXFormView1.FindControl("filtertable") as HtmlTable;
		#endregion

		lblMessage.Text = PXMessages.LocalizeNoPrefix(Messages.SpecifySearchRequest);

		activeModule = Request.Params["am"];
		Guid am;
		if (Guid.TryParse(activeModule, out am))
		{
			comboBoxLookupList = SearchService.BuildComboList(am);
		}
		else
		{
			comboBoxLookupList = SearchService.BuildComboList(null);
		}

		searchType = comboBoxLookupList[0].Type;

		string query = Request.Params["query"];
		string st = Request.Params["st"];
		if (!string.IsNullOrEmpty(st))
			Enum.TryParse<SearchService.SearchLookupType>(Request.Params["st"], true, out searchType);

		if (am == Guid.Empty)
		{
			if (searchType == SearchService.SearchLookupType.ActiveModule)
				searchType = SearchService.SearchLookupType.AllEntities;
			else if (searchType == SearchService.SearchLookupType.ActiveWiki)
				searchType = SearchService.SearchLookupType.AllHelp;
		}

		txtSearch.Text = Request.Params["query"];

		if (query == null || string.IsNullOrEmpty(query.Trim()))
		{
			divMessage.Visible = true;
		}
		else
		{
			divMessage.Visible = false;
		}

		if (searchType == SearchService.SearchLookupType.ActiveModule ||
			searchType == SearchService.SearchLookupType.ActiveWiki ||
			searchType == SearchService.SearchLookupType.Files || searchType == SearchService.SearchLookupType.Screen)
		{
			filtertable.Visible = true;
			lblActiveModule.Text = comboBoxLookupList.Single(a => a.Type.Equals(searchType)).Name; 
		}
		else
		{
			filtertable.Visible = false;
		}

		SearchService.SearchLookupType newSt = searchType;
		if (searchType == SearchService.SearchLookupType.ActiveModule || searchType == SearchService.SearchLookupType.Files || searchType == SearchService.SearchLookupType.Screen)
		{
			newSt = SearchService.SearchLookupType.AllEntities;
		}

		if (searchType == SearchService.SearchLookupType.ActiveWiki)
		{
			newSt = SearchService.SearchLookupType.AllHelp;
		}

		UriBuilder urlClear = new UriBuilder(new Uri(HttpContext.Current.Request.GetExternalUrl(), this.ResolveUrl(Request.RawUrl)));
		btnClearActiveModule.NavigateUrl = urlClear.Path + string.Format("?query={0}&am={1}&st={2}", query, activeModule, newSt);

		btnClearActiveModule.RenderAsButton = false;
		btnShowList.RenderAsButton = false;
		//txtSearch.BorderColor = Color.Transparent;

		foreach (SearchService.SearchLookupItem item in comboBoxLookupList)
		{
			bool addActiveModule = true;
			bool addActiveWiki = true;
			bool addAllEntity = true;
			bool addAllHelp = true;


			if (searchType == SearchService.SearchLookupType.ActiveModule)
			{
				addActiveModule = false;
				addAllEntity = false;
			}

			if (searchType == SearchService.SearchLookupType.ActiveWiki)
			{
				addActiveWiki = false;
				addAllHelp = false;
			}

			if (searchType == SearchService.SearchLookupType.AllEntities)
			{
				addAllEntity = false;
			}

			if (searchType == SearchService.SearchLookupType.AllHelp)
			{
				addAllHelp = false;
			}

			//bool addItem = true;

			if (item.Type == SearchService.SearchLookupType.ActiveModule && !addActiveModule)
				continue;
			if (item.Type == SearchService.SearchLookupType.ActiveWiki && !addActiveWiki)
				continue;
			if (item.Type == SearchService.SearchLookupType.AllEntities && !addAllEntity)
				continue;
			if (item.Type == SearchService.SearchLookupType.AllHelp && !addAllHelp)
				continue;

			UriBuilder url = new UriBuilder(new Uri(HttpContext.Current.Request.GetExternalUrl(), this.ResolveUrl(Request.RawUrl)));
			string navigateUrl = url.Path + string.Format("?query={0}&am={1}&st={2}", query, activeModule, item.Type);

            //var link = new HyperLink() { CssClass = "comboItem", NavigateUrl = navigateUrl, Text = item.Name };
            //cellList.Controls.Add(link);
            
			PXButton button = new PXButton();
			button.ApplyStyleSheetSkin(this.Page);
			button.RenderAsButton = false;
			button.TextAlign = HorizontalAlign.Left;
			button.CssClass = "comboItem";
			button.Text = item.Name;
			button.Styles.Hover.Cursor = WebCursor.Hand;
			button.NavigateUrl = navigateUrl;
			button.AutoCallBack.Command = "navigate";
			button.AutoCallBack.Enabled = true;
			button.AutoCallBack.Behavior.PostData = PostDataMode.Container;
			button.AutoCallBack.Behavior.RepaintControls = RepaintMode.All;
			button.AutoCallBack.Behavior.ContainerID = "PXFormView1";
			button.AutoCallBack.Behavior.BlockPage = true;
			button.ID = "Button_" + item.Type;
			cellList.Controls.Add(button);
            
		}
	}
	
	protected virtual void Page_LoadComplete(object sender, EventArgs e)
	{
        string noteID = Request.Params["navigate"];
        if (!string.IsNullOrEmpty(noteID))
            try
            {
                PXEntitySearch search = new PXEntitySearch();
                search.Redirect(Guid.Parse(noteID));
            }
            catch (PXRedirectRequiredException ex)
            {
                string url = PX.Web.UI.PXDataSource.getMainForm(ex.Graph.GetType());
                if (url != null)
                {
                    ex.Graph.Unload();
                    PXContext.Session.RedirectGraphType[PXUrl.ToAbsoluteUrl(url)] = ex.Graph.GetType();
                    Response.Redirect(this.ResolveUrl/**/(url));
                    //throw new PXRedirectRequiredException(url, ex.Graph, "Redirect0:" + this.ResolveUrl/**/(url));
                }
            }
        
        string fileName = Request.Params["file"];
        if (!string.IsNullOrEmpty(fileName)) try
        {
            WikiFileMaintenance graph = PXGraph.CreateInstance<WikiFileMaintenance>();
            UploadFileWithIDSelector cur =
            PXSelect<UploadFileWithIDSelector,
            Where<UploadFileWithIDSelector.name,
            Equal<Required<UploadFileWithIDSelector.name>>>>.Select(graph, HttpUtility.UrlDecode(fileName));
            if (cur != null)
            {
                graph.Files.Current = cur;
            }
            PXRedirectHelper.TryRedirect(graph, PXRedirectHelper.WindowMode.Same);
        }
        catch (PXRedirectRequiredException ex)
        {
            string url = PX.Web.UI.PXDataSource.getMainForm(ex.Graph.GetType());
            if (url != null)
            {
                ex.Graph.Unload();
                PXContext.Session.RedirectGraphType[PXUrl.ToAbsoluteUrl(url)] = ex.Graph.GetType();
                Response.Redirect(this.ResolveUrl/**/(url));
                //throw new PXRedirectRequiredException(url, ex.Graph, "Redirect0:" + this.ResolveUrl/**/(url));
            }
        }
        //if (Page.IsPostBack && !Page.IsCallback)
        //	AcceptInput();

        //int page = 0;
        //int.TryParse(Request.Params["page"], out page);
        string query = TrimLongString(txtSearch.Text);
		switch (searchType)
		{
			case SearchService.SearchLookupType.ActiveModule:
			case SearchService.SearchLookupType.AllEntities:
				if (PerformSearchEntity(query, pageIndex))
					divMessage.Visible = false;
				break;
			case SearchService.SearchLookupType.ActiveWiki:
			case SearchService.SearchLookupType.AllHelp:
				if (PerformSearchArticle(query, pageIndex))
					divMessage.Visible = false;
				break;
			case SearchService.SearchLookupType.Files:
				if (PerformSearchFile(query, pageIndex))
					divMessage.Visible = false;
				break;
		}
	}
	
	#endregion

	private void RegisterThisId()
	{
		this.Page.ClientScript.RegisterClientScriptBlock(this.GetType(), "varid", "var thisId = '" + this.ClientID + "';", true);
	}

	public void ShowFullTextWarning()
	{
        lblFullTextWarning.Text = string.Format(
"<table class='GrayBox'><tr><td><table class='GrayBoxContent'><tr><td class='warncell' contenteditable='false'><img src = '{0}' contenteditable='false' data-convert='icons'></td><td class='boxcontent' contenteditable='true'><b>{1}</b></td></tr></table></td></tr></table>"
, ResolveUrl("~/App_Themes/Default/Images/Wiki/warn.png"), Messages.IndexDisabled);
		lblFullTextWarning.Visible = true;
	}

	public virtual void DisplaySearchTips(string query)
	{
		divMessage.Visible = true;
		lblMessage.Visible = true;
		if (!string.IsNullOrEmpty(query))
		{
			lblMessage.Text = PXMessages.LocalizeNoPrefix(Messages.NothingFound) + ": <b>" + HttpUtility.HtmlEncode(query) + "</b>";
			divTips.Visible = true;
		}
	}

	public virtual void DisplayIndexTips()
	{
		divMessage.Visible = true;
		lblMessage.Visible = true;
		lblMessage.Text = PXMessages.LocalizeNoPrefix(Messages.SearchIndexIsEmpty);
	}

	public void AcceptInput()
	{
		var url = BuildUrl(TrimLongString(txtSearch.Text), null);
		Response.Redirect(url, false);
	}

	protected string TrimLongString(string source)
	{
		if (string.IsNullOrEmpty(source)) return string.Empty;
		if (source.Length > 500) return source.Substring(0, 500);
		return source;
	}
	
	protected virtual string BuildUrl(string query, int? page)
	{
		var url = PXUrl.ToAbsoluteUrl(Request.AppRelativeCurrentExecutionFilePath) + "?query=" + HttpUtility.UrlEncode(query) + "&st=" + this.searchType + "&am=" + this.activeModule;
		if (page != null)
		{
			url = url + "&Page=" + page.Value;
		}

		return url;
	}

	#region Article Search

	PXHelpSearch search = new PXHelpSearch();

	protected bool PerformSearchArticle(string query, int page)
    {
        Debug.Print("PerformSearch: {0}", query);
        Stopwatch sw = new Stopwatch();
        sw.Start();

        DateTime start = DateTime.Now;
        search.WikiID = ActiveModule;

        if (!search.IsFullText())
            ShowFullTextWarning();

        List<HelpSearchResult> searchResults = search.Search(query, page, 10);
        if (searchResults.Count == 0)
        {
            DisplaySearchTips(query);
            return false;
        }

        RenderResultsArticle(searchResults);

        linkPrev.Visible = search.HasPrevPage;
        linkNext.Visible = search.HasNextPage;
        StorePages(-search.PreviousIndex, search.NextIndex);

        sw.Stop();
        Debug.Print("PerformSearch Completed in {0} millisec.", sw.ElapsedMilliseconds);

        return true;
    }

    private void StorePages(int prev, int next)
    {
        //                            <asp:HiddenField ID="pages" runat="server" />

        var pnlResults = PXFormView1.FindControl("pnlResults") as PXSmartPanel;
        if (pnlResults != null)
        {
            var hidden = pnlResults.FindControl("pages") as HiddenField;
            if (hidden != null)
            {
                hidden.Value = prev.ToString() + "&" + next.ToString();
            }
        }
    }

    int pageIndex = 0;
    string[] parsePages()
    {
        var pnl = PXFormView1.FindControl("pnlResults") as PXSmartPanel;
        if (pnl != null)
        {
            var pages = pnl.FindControl("pages") as HiddenField;
            if (pages != null)
            {
                return (pages.Value??"").Split('&');
            }
        }
        return new string[] {};
    }
    protected void linkPrev_Command(object sender, CommandEventArgs e)
	{
        var pagesStr = parsePages();
        if (pagesStr.Length == 2)
            int.TryParse(pagesStr[0], out pageIndex);
	}

	protected void linkNext_Command(object sender, CommandEventArgs e)
	{
        var pagesStr = parsePages();
        if (pagesStr.Length == 2)
            int.TryParse(pagesStr[1], out pageIndex);
    }

    //private string FixUrl(string url)
    //{
    //    string site = Request.GetWebsiteUrl();
    //    var uri = new System.Uri(url,);
    //}
    private void RenderResultsArticle(List<HelpSearchResult> searchResults)
	{
		int cx = 0;
		foreach (HelpSearchResult record in searchResults)
		{
			int lastrowindex = resultsTable.Rows.Count;
			HtmlTableRow row = new HtmlTableRow();
			HtmlTableCell cell = new HtmlTableCell();
			cell.Style[HtmlTextWriterStyle.VerticalAlign] = "top";
			cell.ID = "articleCell" + cx.ToString();
			resultsTable.Rows.Add(row);
			row.Cells.Add(cell);

            var uri = new System.Uri(HttpContext.Current.Request.Url, ResolveUrl(HttpUtility.UrlPathEncode("~/Wiki/ShowWiki.aspx?PageID=" + record.PageID.ToString())));

			var query = Request.Params["query"];
			var link = new HyperLink() { CssClass = "searchresultTitle", Text = "<u><nobr>" + record.Title + "</nobr></u>", NavigateUrl = Request.GetWebsiteUrl().Trim('/') + uri.PathAndQuery + "&navigate:" + record.PageID.ToString() + "&query=" + query };
            cell.Controls.Add(link);
            /*
            //Title:
            PXButton b = new PXButton();
            b.BorderColor = Color.Transparent;
            b.BorderWidth = 0;
            b.RenderAsButton = false;
            b.CssClass = "searchresultTitle";
            b.Text = "<u><nobr>" + record.Title + "</nobr></u>";
            b.Styles.Hover.Cursor = WebCursor.Hand;
            b.CommandName = "navigate:" + record.PageID.ToString();
            b.ForeColor = Color.DarkBlue;
            b.AutoCallBack.Command = "navigate";
            b.AutoCallBack.Enabled = true;
            b.AutoCallBack.Behavior.PostData = PostDataMode.Container;
            b.AutoCallBack.Behavior.RepaintControls = RepaintMode.All;
            b.AutoCallBack.Behavior.ContainerID = "PXFormView1";
            b.AutoCallBack.Behavior.BlockPage = true;
            b.CallBack += new PXCallBackEventHandler(navigate_CallBack_Wiki);
            b.ID = "srchNavBtn_" + cell.ID + "_0";
			var menuItem1 = new PXMenuItem(Messages.Open) { CommandName = "Open", NavigateUrl = url };
			var menuItem = new PXMenuItem(Messages.OpenInNewTab) { CommandName = "Open", NavigateUrl = url, Target = "main", OpenFrameset = true };
			b.MenuItems.Add(menuItem1);
			b.MenuItems.Add(menuItem);
			b.RightButtonMenu = true;
			b.RenderMenuButton = false;
			b.RenderDropMenu = true;
            cell.Controls.Add(b);
            */

            //Path elements:
            WebControl divPath = new WebControl(HtmlTextWriterTag.Div);
			divPath.CssClass = "searchresultPath";
			divPath.Controls.Add(new LiteralControl(record.Path));
			cell.Controls.Add(divPath);

			//Lines:
			WebControl divLine1 = new WebControl(HtmlTextWriterTag.Div);
			divLine1.CssClass = "searchresultLine";
			divLine1.Controls.Add(new LiteralControl(record.Text));
			cell.Controls.Add(divLine1);

			cx++;
		}
	}

	#endregion

	#region Entity Search

	private int? GetCategory()
	{
		if (searchType == SearchService.SearchLookupType.ActiveModule)
		{
			foreach (SearchService.SearchLookupItem item in comboBoxLookupList)
			{
				if (item.Type == SearchService.SearchLookupType.ActiveModule && !string.IsNullOrEmpty(item.Module))
				{
					//Had to use reflection so that Acumatica Studio can compile.
					Type searchCategoryType = PXBuildManager.GetType("PX.Objects.SM.SearchCategory", false);
					if (searchCategoryType != null)
					{
						MethodInfo method = searchCategoryType.GetMethod("Parse");
						return Convert.ToInt32( method.Invoke(null, new object[] {item.Module}) );
					}
				}
			}
		}

		return null;
	}

	protected bool PerformSearchEntity(string query, int page)
	{
		Debug.Print("PerformSearch: {0}", query);
		Stopwatch sw = new Stopwatch();
		sw.Start();

		DateTime start = DateTime.Now;
		PXEntitySearch search = new PXEntitySearch();
		search.Category = GetCategory();

		if (!search.IsFullText())
			ShowFullTextWarning();

		List<EntitySearchResult> searchResults = search.Search(query, page, 10);
		if (searchResults.Count == 0)
		{
			if (!search.IsSearchIndexExists())
				DisplayIndexTips();
			else 
				DisplaySearchTips(query);

			return false;
		}

		RenderResultsEntity(searchResults);
		

		linkPrev.Visible = search.HasPrevPage;
		linkNext.Visible = search.HasNextPage;
        StorePages(-search.PreviousIndex, search.NextIndex);


		sw.Stop();
		Debug.Print("PerformSearch Completed in {0} millisec.", sw.ElapsedMilliseconds);

		return true;
	}

	private void RenderResultsEntity(List<EntitySearchResult> searchResults)
	{
		int cx = 0;
		foreach (EntitySearchResult record in searchResults)
		{
			int lastrowindex = resultsTable.Rows.Count;
			HtmlTableRow row = new HtmlTableRow();
			HtmlTableCell cell = new HtmlTableCell();
			cell.Style[HtmlTextWriterStyle.VerticalAlign] = "top";
			cell.ID = "entityCell" + cx.ToString();
			resultsTable.Rows.Add(row);
			row.Cells.Add(cell);

            var link = new HyperLink() { CssClass = "searchresultTitle", Text = "<u><nobr>" + record.Title + "</nobr></u>", NavigateUrl= Request.GetWebsiteUrl().Trim('/') + Request.Url.PathAndQuery + "&navigate=" + record.NoteID.ToString() };
            cell.Controls.Add(link);
            /*
            //Title:
            PXButton b = new PXButton();
			b.BorderColor = Color.Transparent;
			b.BorderWidth = 0;
			b.RenderAsButton = false;
			b.CssClass = "searchresultTitle";
			b.Text = "<u><nobr>" + record.Title + "</nobr></u>";
			//b.Text = "<a href='javascript:void'>" + record.Title + "</a>";
			b.Styles.Hover.Cursor = WebCursor.Hand;
			b.CommandName = "navigate:" + record.NoteID.ToString();
			b.ForeColor = Color.DarkBlue;
			b.AutoCallBack.Command = "navigate";
			b.AutoCallBack.Enabled = true;
			b.AutoCallBack.Behavior.PostData = PostDataMode.Container;
			b.AutoCallBack.Behavior.RepaintControls = RepaintMode.All;
			b.AutoCallBack.Behavior.ContainerID = "PXFormView1";
			b.AutoCallBack.Behavior.BlockPage = true;
			b.CallBack += new PXCallBackEventHandler(navigate_CallBack);
			b.ID = "srchNavBtn_" + cell.ID + "_0";
			cell.Controls.Add(b);
            */

			//Path elements:
			WebControl divPath = new WebControl(HtmlTextWriterTag.Div);
			divPath.CssClass = "searchresultPath";
			divPath.Controls.Add(new LiteralControl(record.Path));
			cell.Controls.Add(divPath);

			//Lines:
			WebControl divLine1 = new WebControl(HtmlTextWriterTag.Div);
			divLine1.CssClass = "searchresultLine";
			divLine1.Controls.Add(new LiteralControl(record.Line1));
			cell.Controls.Add(divLine1);

			WebControl divLine2 = new WebControl(HtmlTextWriterTag.Div);
			divLine2.CssClass = "searchresultLine";
			divLine2.Controls.Add(new LiteralControl(record.Line2));
			cell.Controls.Add(divLine2);

			cx++;
		}
	}

	private void navigate_CallBack(object sender, PXCallBackEventArgs e)
	{
		PXButton btn = (PXButton)sender;
		if (string.IsNullOrEmpty(btn.CommandName))
			return;

		NavHandler(btn.CommandName);
	}

    private void navigate_CallBack_Wiki(object sender, PXCallBackEventArgs e)
    {
        PXButton btn = (PXButton)sender;
        if (string.IsNullOrEmpty(btn.CommandName))
            return;

        NavHandlerWiki(btn.CommandName);
    }

    private void navigate_CallBack_Files(object sender, PXCallBackEventArgs e)
    {
        PXButton btn = (PXButton)sender;
        if (string.IsNullOrEmpty(btn.CommandName))
            return;

        NavHandlerFiles(btn.CommandName);
    }

	private bool NavHandler(string args)
	{
        Guid noteID = Guid.Parse(args.Split(':')[1]);

        try
        {
            PXEntitySearch search = new PXEntitySearch();
            search.Redirect(noteID);
        }
        catch (PXRedirectRequiredException ex)
        {
            string url = PX.Web.UI.PXDataSource.getMainForm(ex.Graph.GetType());
            if (url != null)
            {
                ex.Graph.Unload();
                PXContext.Session.RedirectGraphType[PXUrl.ToAbsoluteUrl(url)] = ex.Graph.GetType();
                throw new PXRedirectRequiredException(url, ex.Graph, "Redirect0:" + this.ResolveUrl/**/(url));
            }
        }
        return false;
	}

    private bool NavHandlerFiles(string args)
    {
        try
        {
            String FileName = args.Split(':')[1];
            WikiFileMaintenance graph = PXGraph.CreateInstance<WikiFileMaintenance>();
            UploadFileWithIDSelector cur =
            PXSelect<UploadFileWithIDSelector, 
            Where<UploadFileWithIDSelector.name,
            Equal<Required<UploadFileWithIDSelector.name>>>>.Select(graph, HttpUtility.UrlDecode(FileName));
            if (cur != null)
            {
                graph.Files.Current = cur;
            }
            PXRedirectHelper.TryRedirect(graph, PXRedirectHelper.WindowMode.Same);
        }
        catch (PXRedirectRequiredException ex)
        {
            string url = PX.Web.UI.PXDataSource.getMainForm(ex.Graph.GetType());
            if (url != null)
            {
                ex.Graph.Unload();
				PXContext.Session.RedirectGraphType[PXUrl.ToAbsoluteUrl(url)] = ex.Graph.GetType();
                throw new PXRedirectRequiredException(url, ex.Graph, "Redirect0:" + this.ResolveUrl/**/(url));
            }
        }
        return false;
    }

    private bool NavHandlerWiki(string args)
    {
		Guid PageID = Guid.Parse(args.Split(':')[1]);
		throw new PXRedirectToUrlException(PXUrl.ToAbsoluteUrl(HttpUtility.UrlPathEncode(string.Format("~/Wiki/ShowWiki.aspx?PageID={0}", PageID)))
			, null);
    }

	#endregion


	#region File Search

	protected bool PerformSearchFile(string query, int page)
	{
		Debug.Print("PerformSearch: {0}", query);
		Stopwatch sw = new Stopwatch();
		sw.Start();

		DateTime start = DateTime.Now;
		PXFileSearch search = new PXFileSearch();

		List<FileSearchResult> searchResults = search.Search(query, page, 10);
		if (searchResults.Count == 0)
		{
			DisplaySearchTips(query);
			return false;
		}

		RenderResultsFile(searchResults);

		linkPrev.Visible = search.HasPrevPage;
		linkNext.Visible = search.HasNextPage;
        StorePages(-search.PreviousIndex,search.NextIndex);

		sw.Stop();
		Debug.Print("PerformSearch Completed in {0} millisec.", sw.ElapsedMilliseconds);

		return true;
	}
	
	private void RenderResultsFile(List<FileSearchResult> searchResults)
	{
		int cx = 0;
		foreach (FileSearchResult record in searchResults)
		{
			int lastrowindex = resultsTable.Rows.Count;
			HtmlTableRow row = new HtmlTableRow();
			HtmlTableCell cell = new HtmlTableCell();
			cell.Style[HtmlTextWriterStyle.VerticalAlign] = "top";
			cell.ID = "fileCell" + cx.ToString();
			resultsTable.Rows.Add(row);
			row.Cells.Add(cell);

            //Title:
            /*HyperLink b = new HyperLink();
			b.NavigateUrl = "~/Pages/SM/SM202510.aspx?fileID=" + record.Url;
			b.CssClass = "searchresultTitle";
			b.Text = record.Title;
			b.ForeColor = Color.DarkBlue;
			b.ID = "srchNavBtn_" + cell.ID + "_0";
			cell.Controls.Add(b);*/

            var link = new HyperLink() { CssClass = "searchresultTitle", Text = "<u><nobr>" + record.Title + "</nobr></u>", NavigateUrl = Request.GetWebsiteUrl().Trim('/') + Request.Url.PathAndQuery + "&" + "file=" + HttpUtility.UrlEncode(record.Url) };
            cell.Controls.Add(link);

            /*
            //Title:
            PXButton b = new PXButton();
            b.BorderColor = Color.Transparent;
            b.BorderWidth = 0;
            b.RenderAsButton = false;
            b.CssClass = "searchresultTitle";
            b.Text = "<u><nobr>" + record.Title + "</nobr></u>";
            //b.Text = "<a href='javascript:void'>" + record.Title + "</a>";
            b.Styles.Hover.Cursor = WebCursor.Hand;
            b.CommandName = "navigate:" + record.Url;
            b.ForeColor = Color.DarkBlue;
            b.AutoCallBack.Command = "navigate";
            b.AutoCallBack.Enabled = true;
            b.AutoCallBack.Behavior.PostData = PostDataMode.Container;
            b.AutoCallBack.Behavior.RepaintControls = RepaintMode.All;
            b.AutoCallBack.Behavior.ContainerID = "PXFormView1";
            b.AutoCallBack.Behavior.BlockPage = true;
            b.CallBack += new PXCallBackEventHandler(navigate_CallBack_Files);
            b.ID = "srchNavBtn_" + cell.ID + "_0";
            cell.Controls.Add(b);
            */
			
			//Lines:
			WebControl divLine1 = new WebControl(HtmlTextWriterTag.Div);
			divLine1.CssClass = "searchresultLine";
			divLine1.Controls.Add(new LiteralControl(record.Line1));
			cell.Controls.Add(divLine1);

			cx++;
		}
	}

	#endregion
	
}