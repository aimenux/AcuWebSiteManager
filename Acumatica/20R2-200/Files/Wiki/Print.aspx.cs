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
using System.Reflection;
using PX.Common;
using PX.Data;
using PX.SM;
using PX.Data.Wiki.Parser;
using PX.Web.UI;
using PX.Web.Controls.Wiki;

public partial class Wiki_Print : ShowPage
{
	private const string _TEMPLATE_WIKI_ARTICLE = "PX.Web.Objects.Wiki.TemplatePrintWikiArticle";
	private const string _ROOTTEMPLATE_PROPERTY = "RootTemplate";
	private static readonly Type _templateWikiArticleType;
	private static readonly PropertyInfo _rootTemplate;

	static Wiki_Print()
	{
		_templateWikiArticleType = System.Web.Compilation.PXBuildManager.GetType(_TEMPLATE_WIKI_ARTICLE, false);
		if (_templateWikiArticleType != null) _rootTemplate = _templateWikiArticleType.GetProperty(_ROOTTEMPLATE_PROPERTY);
	}

	private PXWikiShow _wikiShow;

	protected override PXWikiShow CreateWikiShow()
	{
		return _wikiShow;
	}

	protected override void Initialize()
	{
		if (_templateWikiArticleType != null && _rootTemplate != null)
		{
			string language = Request.Params["Language"];
			if (string.IsNullOrEmpty(language)) language = LocaleInfo.GetCulture().Name;
			int revision;
			Guid pageID = WikiPageId;
			int? correctRevision = int.TryParse(Request.Params["PageRevisionID"], out revision) ? (int?)revision : null;
			WikiReader graph = PXGraph.CreateInstance<WikiReader>();
			object article = pageID == Guid.Empty
				? Activator.CreateInstance(_templateWikiArticleType, graph, Request.Params["wiki"], Request.Params["art"], language, correctRevision)
				: Activator.CreateInstance(_templateWikiArticleType, graph, pageID, language, correctRevision);
			_wikiShow = new TemplateWikiShow((IWikiArticle)article, (IRootTemplateInfo)_rootTemplate.GetValue(article, null));
		}
		else _wikiShow = new PXWikiShow();

		base.Initialize();
		this.MainForm.DataBinding += new EventHandler(MainForm_DataBinding);
	}

	void MainForm_DataBinding(object sender, EventArgs e)
	{
		this._wikiShow.WikiContext.RenderSectionLink = false;
		this._wikiShow.WikiContext.EnableScript = false;
		this._wikiShow.AutoSize.Enabled = false;
		this._wikiShow.Style[HtmlTextWriterStyle.OverflowY] = "visible";
		if (((WikiReader)ds.DataGraph).Pages.Current != null)
		{
			WikiDescriptor descr = PXSelect<WikiDescriptor, Where<WikiDescriptor.pageID, Equal<Required<WikiDescriptor.pageID>>>>.Select(ds.DataGraph, ((WikiReader)ds.DataGraph).Pages.Current.WikiID);
			if (descr != null)
				((PXDBContext)this._wikiShow.WikiContext).StyleID = descr.CssPrintID;
		}
	}

	protected override PXDataSource DS
	{
		get { return ds; }
	}

	protected override PXDataSource TreeDS
	{
		get { return null; }
	}

	protected override PXFormView MainForm
	{
		get { return form; }
	}

	protected override PXToolBar MainToolbar
	{
		get { return null; }
	}

	protected override PXSmartPanel WikiTextPanel
	{
		get { return null; }
	}

	protected override PXTextEdit WikiTextEditor
	{
		get { return null; }
	}

	protected override PXTextEdit UrlEditor
	{
		get { return null; }
	}

	protected override PXTextEdit PublicUrlEditor
	{
		get { return null; }
	}

	protected override PXTextEdit LinkEditor
	{
		get { return null; }
	}
}
