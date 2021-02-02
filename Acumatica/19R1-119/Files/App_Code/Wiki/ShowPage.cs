using System;
using System.Drawing;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using PX.Data;
using PX.Data.Wiki.Parser;
using PX.SM;
using PX.Web.UI;
using PX.Common;

public abstract class ShowPage : PX.Web.UI.PXPage
{
	private const string _PAGE_DB_CONTEXT = "PX.Web.Objects.Wiki.PageDBContext";
	private const string _FILES_CONF_KEY = "FilesPath";

	private static readonly Type _pageDBContextType;
	private static readonly string _filesPath;

	static ShowPage()
	{
		_pageDBContextType = PX.Common.Tools.TryLoadType(_PAGE_DB_CONTEXT);
		_filesPath = System.Configuration.ConfigurationManager.AppSettings[_FILES_CONF_KEY] ?? string.Empty;
	}

	#region Fields

	private Guid _pageId = Guid.Empty;
	private PXWikiSettings _wikiSettings;
	private WikiPage article;
	
	#endregion

	#region Protected Methods

	#region Event Handlers

	protected virtual void Page_Init(object sender, EventArgs e)
	{
		IPXMasterPage pxMaster = Master as IPXMasterPage;
		if (pxMaster != null)
		{
			pxMaster.CustomizationAvailable = false;
		}
	}

	protected override void OnInit(EventArgs e)
	{
		base.OnInit(e);

		Form.Controls.Add(GenerateFilesLinkPanel());

		Initialize();
	}

	protected void Page_Load(object sender, EventArgs e)
	{
		if (!this.Page.IsCallback) MainForm.DataBind();
	}

	protected void Page_LoadComplete(object sender, EventArgs e)
	{
	}

	protected void Page_Unload(object sender, EventArgs e)
	{
	}

	protected void form_DataBinding(object sender, EventArgs e)
	{
		WikiReader graph = (WikiReader)this.DS.DataGraph;
		PXCache cache = graph.Views[this.DS.PrimaryView].Cache;
		WikiPage current = cache.Current as WikiPage;
		if (current != null)
		{
			string language = Request.Params["Language"];
			if (string.IsNullOrEmpty(language)) language = LocaleInfo.GetCulture().Name;

			WikiRevision currentrevision =
				PXSelect<WikiRevision,
					Where<WikiRevision.pageID,
						Equal<Required<WikiRevision.pageID>>,
						And<WikiRevision.language, Equal<Required<WikiRevision.language>>>>, OrderBy<Desc<WikiRevision.pageRevisionID>>>.SelectWindowed(new PXGraph(), 0, 1, current.PageID, language);

			if (this.PublicUrlEditor != null)
				this.PublicUrlEditor.Text = string.Empty;

			if (current.WikiID != null)
			{
				if (Master is IPXMasterPage) (Master as IPXMasterPage).ScreenTitle = current.Title;
				if (Master is IPXMasterPage) (Master as IPXMasterPage).BranchAvailable = false;
				graph.Filter.Current.WikiID = current.WikiID;
				WikiDescriptor wiki = graph.wikis.SelectWindowed(0, 1, current.WikiID);
				PXWikiShow content = MainForm.FindControl("edContent") as PXWikiShow;
				if (wiki != null)
				{
					if (!string.IsNullOrEmpty(wiki.PubVirtualPath))
					{
						if (PublicUrlEditor != null)
						{
							PublicUrlEditor.Text = Request.GetExternalUrl().ToString();
							int index = PublicUrlEditor.Text.IndexOf("Wiki");
							if (index > -1)
							{
								PublicUrlEditor.Text = PublicUrlEditor.Text.Remove(0, index);
								PublicUrlEditor.Text = wiki.PubVirtualPath + "/" + PublicUrlEditor.Text;
							}
						}
					}
					if (!Page.IsPostBack)
						PXAuditJournal.Register("WI000000", Wiki.Link(wiki.Name, current.Name));
				}

				if (content != null)
				{
					PXDBContext wikiContext = (PXDBContext)content.WikiContext;

					if (currentrevision != null)
						wikiContext.Text = currentrevision.Content;
					if (_pageDBContextType == null)
					{
						wikiContext.ContentWidth = current.Width.GetValueOrDefault(0);
						wikiContext.ContentHeight = current.Height.GetValueOrDefault(0);
						wikiContext.PageTitle = current.Title;
					}

					wikiContext.WikiID = current.WikiID.Value != Guid.Empty ? current.WikiID.Value : current.PageID.Value;

					if (current.PageID != null && (_pageDBContextType == null &&
					                               (PXSiteMap.WikiProvider.GetAccessRights(current.PageID.Value) >= PXWikiRights.Update &&
					                                current.PageRevisionID > 0)))
					{
						wikiContext.LastModified = current.PageRevisionDateTime;
						wikiContext.LastModifiedByLogin = cache.GetStateExt(current, "PageRevisionCreatedByID").ToString();
					}
					if (current.PageID != null && PXSiteMap.WikiProvider.GetAccessRights(current.PageID.Value) >= PXWikiRights.Update)
						wikiContext.RenderSectionLink = true;
					else
					{
						wikiContext.HideBrokenLink = true;
						wikiContext.RedirectAvailable = true;
					}

					if (this.WikiTextPanel != null)
						this.WikiTextPanel.Caption = current.Title;
					if (this.WikiTextEditor != null)
						this.WikiTextEditor.Text = graph.ConvertGuidToLink(current.Content);
				}
				if (this.LinkEditor != null)
					this.LinkEditor.Text = "[" + (wiki != null && wiki.Name != null ? wiki.Name + "\\" : string.Empty) + current.Name + "]";
			}
		}
		this.Page.ClientScript.RegisterClientScriptBlock(GetType(), "varreg", "var printLink=\"" + ResolveUrl("~/Wiki/Print.aspx") + "?" + ControlHelper.SanitizeUrl(Request.QueryString.ToString()) + "\";", true);

		if (this.LinkEditor != null)
			LinkEditor.ReadOnly = true;
		if (this.UrlEditor != null)
		{
			UrlEditor.Text = Request.GetExternalUrl().ToString();
			UrlEditor.ReadOnly = true;
		}
		if (this.PublicUrlEditor != null)
		{
			
			PublicUrlEditor.Enabled = !string.IsNullOrEmpty(PublicUrlEditor.Text);
			PublicUrlEditor.ReadOnly = true;
		}
	}
	
	protected void show_EditorCreated(object sender, EventArgs e)
	{
		PXWikiShow show = sender as PXWikiShow;
		WikiReader graph = (WikiReader)this.DS.DataGraph;
		if (show != null)
		{
			if (graph.Pages.Current != null)
			{
				((PXDBContext)show.Editor.PreviewSettings).WikiID = graph.Pages.Current.WikiID;
				if (graph.Pages.Current.ArticleType == WikiArticleType.Notification)
				{
					WikiNotificationTemplateMaintenance g = TreeDS.DataGraph as WikiNotificationTemplateMaintenance;
					if (g != null)
					{
						g.Filter.Current.PageID = graph.Pages.Current.PageID;
						g.Pages.Current = g.Pages.SelectWindowed(0, 1);
					}

					show.Editor.TemplateDataSourceID = TreeDS.ID;
					PXTreeItemBinding binding = new PXTreeItemBinding();
					binding.DataMember = "EntityItems";
					binding.TextField = "Name";
					binding.ValueField = "Path";
					binding.ImageUrlField = "Icon";
					show.Editor.TemplateDataBindings.Add(binding);
				}
			}
		}
	}

	protected void show_SectionEditStarting(object sender, PXWikiSectionEventArgs e)
	{
		WikiReader graph = (WikiReader)this.DS.DataGraph;
		e.SectionContent = graph.ConvertGuidToLink(e.SectionContent);
	}

	protected void show_SectionUpdated(object sender, PXWikiSectionEventArgs e)
	{
		WikiPage current = this.CurrentRec;
		PXGraph graph = null;
		WikiPage art = null;
		Type wikiType = Wiki.GraphType(current.ArticleType);

		graph = (PXGraph)PXGraph.CreateInstance(wikiType);
		graph.Views["Filter"].Cache.SetValue(graph.Views["Filter"].Cache.Current, "PageID", current.PageID);
		graph.Views["Filter"].Cache.SetValue(graph.Views["Filter"].Cache.Current, "PageRevisionID", 0);
		art = (WikiPage)graph.Views["Pages"].SelectSingle();

		if (art != null)
		{
			art.Content = e.Content;
			art = graph.Views["Pages"].Cache.Update(art) as WikiPage;
			graph.Persist();
		}

		graph.Views["Pages"].Clear();
		graph.Views["Pages"].Cache.Clear();
		graph.Views["Revisions"].Clear();
		graph.Views["Revisions"].Cache.Clear();
		this.DS.DataGraph.Views["Pages"].Clear();
		this.DS.DataGraph.Views["Pages"].Cache.Clear();
		this.DS.DataGraph.Views["Revisions"].Clear();
		this.DS.DataGraph.Views["Revisions"].Cache.Clear();
		this.DS.DataGraph.TypedViews.Clear();
		this.DS.DataGraph.Caches[typeof(WikiPageLanguage)].Clear();
	}

	protected void edContent_EditorCreated(object sender, EventArgs e)
	{
		PXWikiShow show = (PXWikiShow)MainForm.DataControls["edContent"];
		show.Editor.FileUploaded += new PXFileUploadEventHandler(editor_FileUploaded);

		WikiPage current = this.CurrentRec;
		if (current != null) show.Editor.FileNamePrefix = current.Name;
	}

	#endregion

	protected Guid WikiPageId
	{
		get
		{
			if (_pageId == Guid.Empty)
			{
				if(!String.IsNullOrEmpty(Request.Params["pageid"]))
					_pageId = ShowRouter.GetGuidParameterValue(Request.Params["pageid"]);
				if(!String.IsNullOrEmpty(Request.Params["Art"]))
					_pageId = (Guid)ShowRouter.GetArticleID(Request);
			}
			return _pageId;
		}
	}

	protected virtual void Initialize()
	{
		Guid pageId = WikiPageId;
		article = new WikiPage();
		article = GetArticle();

		PXWikiShow show = GetRenderControl("edContent", "Content", "lblContent");
		MainForm.TemplateContainer.Controls.Add(show);

		MainForm.DataBinding += new EventHandler(form_DataBinding);
		if (show != null)
		{
			
			show.WikiContext = _pageDBContextType == null 
				? new PXDBContext(WikiSettings.Relative)
				: (PXDBContext)Activator.CreateInstance(_pageDBContextType, WikiSettings.Relative, pageId);
			show.SectionEditStarting += new PXWikiSectionEventHandler(show_SectionEditStarting);
			show.SectionUpdated += new PXWikiSectionEventHandler(show_SectionUpdated);
			show.EditorCreated += new EventHandler(show_EditorCreated);
			this.ClientScript.RegisterClientScriptBlock(GetType(), "showid", "var wikiShowId='" + show.ClientID + "';", true);
		}
		if (!DesignMode)
		{
			string prevScreenID = Page.Request.QueryString["PrevScreenID"];
			if (!string.IsNullOrEmpty(prevScreenID) && this.Master is IPXMasterPage)
				(Master as IPXMasterPage).ScreenID = prevScreenID;
		}
		string navurl = this.GetExportUrl();
		if (this.MainToolbar != null)
		{
			PXToolBarButton exportBtn = ((PXToolBarButton)this.MainToolbar.Items["export"]);
			exportBtn.MenuItems[0].NavigateUrl = navurl + "&type=txt";
			exportBtn.MenuItems[1].NavigateUrl = navurl + "&type=rtf";
			/*if (article != null)
			{
				exportBtn.MenuItems[2].NavigateUrl = "ExportDita.axd?DITAConversionType=ConversionType1&pageid=" + article.PageID + "&lang=" + article.Language + "&name=" + HttpUtility.HtmlEncode(article.Title) + "&type=last";
				exportBtn.MenuItems[3].NavigateUrl = "ExportDita.axd?DITAConversionType=ConversionType1&pageid=" + article.PageID + "&lang=" + article.Language + "&name=" + HttpUtility.HtmlEncode(article.Title) + "&type=published";
			}*/
		}
		WikiReader wikiReader = DS.DataGraph as WikiReader;
		if (wikiReader != null)
		{
			wikiReader.AppVirtualPath = Request.GetWebsiteAuthority().GetLeftPart(UriPartial.Authority);
			wikiReader.WebDAVFilesPath = !string.IsNullOrEmpty(_filesPath) ? ResolveUrl(_filesPath) : ResolveUrl("~/Files");
		}
	}


	private void SetReaderFilter(WikiReader reader)
	{
		if (!string.IsNullOrEmpty(this.Request["PageID"]))
			reader.Filter.Current.PageID = PX.Common.GUID.CreateGuid(this.Request["PageID"]);
		else if (!string.IsNullOrEmpty(this.Request["wiki"]) && !string.IsNullOrEmpty(this.Request["art"]))
		{
			reader.Filter.Current.Wiki = this.Request["wiki"];
			reader.Filter.Current.Art = this.Request["art"];
		}
	}

	protected PXWikiSettings WikiSettings
	{
		get
		{
            return _wikiSettings ?? (_wikiSettings = new PXWikiSettings(this, (WikiReader)DS.DataGraph));
		}
	}
    
	protected PXWikiShow GetRenderControl(string id, string dataField, string labelId)
	{
		PXWikiShow wikiShow = CreateWikiShow();

		wikiShow.ID = id;
		wikiShow.DataField = dataField;
		wikiShow.Height = Unit.Percentage(100.0D);
		wikiShow.LabelID = labelId;
		wikiShow.Width = Unit.Percentage(100.0D);
		wikiShow.SkinID = "dummy";
		wikiShow.NotFoundMessage = "There is no article to show in this view.";
		wikiShow.BackColor = Color.White;
		wikiShow.Style["z-index"] = "103";
		wikiShow.EditorCreated += edContent_EditorCreated;
		wikiShow.AutoSize.Enabled = true;
		if (article != null)
			wikiShow.IsHtml = (bool)article.IsHtml;
		return wikiShow;
	}

	protected abstract PXWikiShow CreateWikiShow();

	#endregion

	#region Abastract Properties

	protected abstract PXDataSource DS { get; }

	protected abstract PXDataSource TreeDS { get; }

	protected abstract PXFormView MainForm { get; }

	protected abstract PXToolBar MainToolbar { get; }

	protected abstract PXSmartPanel WikiTextPanel { get; }

	protected abstract PXTextEdit WikiTextEditor { get; }

	protected abstract PXTextEdit UrlEditor { get; }

	protected abstract PXTextEdit PublicUrlEditor { get; }

	protected abstract PXTextEdit LinkEditor { get; }

	#endregion

	#region Private Methods

	private Control GenerateFilesLinkPanel()
	{
		PXSmartPanel result = new PXSmartPanel();
		result.ID = "filesLinkPanel";
		result.Key = "FilesLink";
		result.AllowResize = false;
		result.AutoCallBack.Enabled = true;
		result.AutoCallBack.Command = "Refresh";
		result.AutoCallBack.Target = "form";
		result.Style[HtmlTextWriterStyle.Position] = "absolute";
		result.Style[HtmlTextWriterStyle.Left] = Unit.Pixel(250).ToString();
		result.Style[HtmlTextWriterStyle.Top] = Unit.Pixel(250).ToString();
		result.Width = Unit.Pixel(400);
		result.Height = Unit.Pixel(150);

		PXFormView form = new PXFormView();
		form.ID = "form";
		form.Caption = "WebDAV Links for the list of files";
		form.AllowCollapse = false;
		form.DataSourceID = "ds";
		form.DataMember = "FilesLink";
		form.Style[HtmlTextWriterStyle.Position] = "absolute";
		form.Style[HtmlTextWriterStyle.Left] = Unit.Pixel(9).ToString();
		form.Style[HtmlTextWriterStyle.Top] = Unit.Pixel(9).ToString();
		form.Width = Unit.Pixel(363);
		form.Height = Unit.Pixel(100);

		PXLabel extLabel = new PXLabel("External Link");
		extLabel.ID = "lblExt";
		extLabel.Style[HtmlTextWriterStyle.Position] = "absolute";
		extLabel.Style[HtmlTextWriterStyle.Left] = Unit.Pixel(9).ToString();
		extLabel.Style[HtmlTextWriterStyle.Top] = Unit.Pixel(9).ToString();
		extLabel.ApplyStyleSheetSkin(Page);
		form.TemplateContainer.Controls.Add(extLabel);
		PXTextEdit extEdit = new PXTextEdit();
		extEdit.ID = "edExt";
		extEdit.LabelID = "lblExt";
		extEdit.DataField = "InternalPath";
		extEdit.ReadOnly = true;
		extEdit.Style[HtmlTextWriterStyle.Position] = "absolute";
		extEdit.Style[HtmlTextWriterStyle.Left] = Unit.Pixel(81).ToString();
		extEdit.Style[HtmlTextWriterStyle.Top] = Unit.Pixel(9).ToString();
		extEdit.Width = Unit.Pixel(250);
		extEdit.ApplyStyleSheetSkin(Page);
		form.TemplateContainer.Controls.Add(extEdit);

		PXLabel pubLabel = new PXLabel("Public Link");
		pubLabel.ID = "lblPub";
		pubLabel.Style[HtmlTextWriterStyle.Position] = "absolute";
		pubLabel.Style[HtmlTextWriterStyle.Left] = Unit.Pixel(9).ToString();
		pubLabel.Style[HtmlTextWriterStyle.Top] = Unit.Pixel(36).ToString();
		pubLabel.ApplyStyleSheetSkin(Page);
		form.TemplateContainer.Controls.Add(pubLabel);
		PXTextEdit pubEdit = new PXTextEdit();
		pubEdit.ID = "edPub";
		pubEdit.LabelID = "lblPub";
		pubEdit.DataField = "ExternalPath";
		pubEdit.ReadOnly = true;
		pubEdit.Style[HtmlTextWriterStyle.Position] = "absolute";
		pubEdit.Style[HtmlTextWriterStyle.Left] = Unit.Pixel(81).ToString();
		pubEdit.Style[HtmlTextWriterStyle.Top] = Unit.Pixel(36).ToString();
		pubEdit.Width = Unit.Pixel(250);
		pubEdit.ApplyStyleSheetSkin(Page);
		form.TemplateContainer.Controls.Add(pubEdit);

		PXButton closeButton = new PXButton();
		closeButton.Text = "Close";
		closeButton.DialogResult = WebDialogResult.Cancel;
		closeButton.Style[HtmlTextWriterStyle.Position] = "absolute";
		closeButton.Style[HtmlTextWriterStyle.Left] = Unit.Pixel(247).ToString();
		closeButton.Style[HtmlTextWriterStyle.Top] = Unit.Pixel(63).ToString();
		closeButton.Width = Unit.Pixel(90);
		closeButton.ApplyStyleSheetSkin(Page);
		form.TemplateContainer.Controls.Add(closeButton);

		form.ApplyStyleSheetSkin(Page);

		result.Controls.Add(form);
		return result;
	}

	private void editor_FileUploaded(object sender, PXFileUploadEventArgs e)
	{
		WikiPage current = this.CurrentRec;
		if (current != null)
		{
			FileInfo file = e.UploadedFile;
			if (file != null)
			{
				UploadFileMaintenance uploadGraph = PXGraph.CreateInstance<UploadFileMaintenance>();
				uploadGraph.AttachToPage((Guid)file.UID, (Guid)current.PageID);
			}
		}
	}

	
	
	private WikiPage GetArticle()
	{
		WikiReader reader = PXGraph.CreateInstance<WikiReader>();
		SetReaderFilter(reader);
		WikiPage article = reader.Pages.SelectWindowed(0, 1);
		reader.Pages.Current = article;
		return article;
	}

	private WikiPage CurrentRec
	{
		get
		{
			PXCache cache = this.DS.DataGraph.Views[this.DS.PrimaryView].Cache;
			return cache.Current as WikiPage;
		}
	}

	private string GetExportUrl()
	{
		string url = this.ResolveUrl/**/("~/Wiki/ShowExport.aspx");
		if (!string.IsNullOrEmpty(Request["pageid"]))
			url += "?PageID=" + ShowRouter.GetGuidParameterValue(Request["pageid"]).ToString();
		else if (!string.IsNullOrEmpty(Request["wiki"]) && !string.IsNullOrEmpty(Request["art"]))
			url += "?wiki=" + Request["wiki"] + "&art=" + Request["art"];
		return url;
	}

	#endregion
}
