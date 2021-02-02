using System;
using System.Reflection;
using System.Web;
using System.Web.Compilation;
using PX.Common;
using PX.Data;
using PX.SM;

public class ShowRouter
{
	private const string _WIKIARTICLETYPE_ATTRIBUTE_TYPE = "PX.SM.WikiArticleTypeAttribute";
	private const string _TEMPLATEWIKIARTICLE_TYPE = "PX.Web.Objects.Wiki.TemplateWikiArticle";
	private const string _IS_TEMPLATE_METHOD = "IsTemplate";
	private const string _TYPE_CONST_NAME = "_KB_ARTICLE_TYPE";
	private static readonly int _kbArticleType;
	private static readonly MethodInfo _isTemplateArticle;

	private static readonly ShowRouter _instance;

	static ShowRouter()
	{
		_instance = new ShowRouter();
		_kbArticleType = -1;
		Type attType;
		FieldInfo typeConst;
		if ((attType = PXBuildManager.GetType(_WIKIARTICLETYPE_ATTRIBUTE_TYPE, false)) != null && 
			(typeConst = attType.GetField(_TYPE_CONST_NAME)) != null)
		{
			_kbArticleType = (int)typeConst.GetValue(null);
		}
		Type twaType;
		if ((twaType = PXBuildManager.GetType(_TEMPLATEWIKIARTICLE_TYPE, false)) != null)
		{
			_isTemplateArticle = twaType.GetMethod(_IS_TEMPLATE_METHOD);
		}
	}

	public static ShowRouter Instance
	{
		get { return _instance; }
	}

	public static int KBArticleType
	{
		get
		{
			return _kbArticleType;
		}
	}

	public virtual void TryRedirect(HttpRequest request, HttpContext context)
	{
		if (!request.Path.EndsWith("Wiki/Show.aspx", StringComparison.OrdinalIgnoreCase)) return;

		string pageName = "ShowWiki.aspx";
		context.Server.Transfer(string.Concat("~/Wiki/", pageName, "?", request.QueryString), true);
	}

	protected static int? GetArticleType(HttpRequest request)
	{
		Guid wikiId = GetGuidParameterValue(request.Params["wikiid"]);
		if (wikiId == Guid.Empty) wikiId = GetGuidParameterValue(request.Params["wiki"]);
		if (wikiId != Guid.Empty)
		{
			PXResultset<WikiDescriptor> wikiSet = PXSelect<WikiDescriptor, 
				Where<WikiDescriptor.pageID, Equal<Required<WikiDescriptor.pageID>>>>.
				Select(new PXGraph(), wikiId);
			if (wikiSet != null && wikiSet.Count > 0) 
				return ((WikiDescriptor)wikiSet[0]).WikiArticleType;
		}

		Guid pageId = GetGuidParameterValue(request.Params["pageid"]);
		if (pageId != Guid.Empty)
		{
			PXResultset<WikiPage> pageSet = PXSelectJoin<WikiPage,
				InnerJoin<WikiDescriptor, On<WikiDescriptor.pageID, Equal<WikiPage.wikiID>>>,
				Where<WikiPage.pageID, Equal<Required<WikiPage.pageID>>>>.
				Select(new PXGraph(), pageId);
			if (pageSet != null && pageSet.Count > 0) 
				return ((WikiDescriptor)pageSet[0][typeof(WikiDescriptor)]).WikiArticleType;
		}

		return null;
	}

	protected static string GetArticleName(HttpRequest request)
	{
		Guid pageId = GetGuidParameterValue(request.Params["pageid"]);
		if (pageId != Guid.Empty)
		{
			PXResultset<WikiPage> pageSet = PXSelect<WikiPage,
				Where<WikiPage.pageID, Equal<Required<WikiPage.pageID>>>>.
				Select(new PXGraph(), pageId);
			if (pageSet != null && pageSet.Count > 0)
				return ((WikiPage)pageSet[0][typeof(WikiPage)]).Name;
		}
		return null;
	}

	public static Guid GetArticleID(HttpRequest request)
	{
		String name = request.Params["Art"];
		if (!String.IsNullOrEmpty(name))
		{
			PXResultset<WikiPage> pageSet = PXSelect<WikiPage,
				Where<WikiPage.name, Equal<Required<WikiPage.name>>>>.
				Select(new PXGraph(), name);
			if (pageSet != null && pageSet.Count > 0)
				return (Guid)((WikiPage)pageSet[0][typeof(WikiPage)]).PageID;
		}
		return Guid.Empty;
	}



	public static Guid GetGuidParameterValue(string valueStr)
	{
		if (!string.IsNullOrEmpty(valueStr))
		{
			string[] valueStrArr = valueStr.Split(new Char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
			for (int i = 0; i < valueStrArr.Length; i++)
			{
				Guid value;
				if (GUID.TryParse(valueStrArr[i], out value)) return value;
			}
		}
		return Guid.Empty;
	}
}
