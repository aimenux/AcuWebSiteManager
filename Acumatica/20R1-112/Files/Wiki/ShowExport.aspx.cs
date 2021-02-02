using System;
using System.Web;
using PX.Data;
using PX.Data.Wiki.Parser;
using PX.Data.Wiki.Parser.Txt;
using PX.Data.Wiki.Parser.Rtf;
using PX.SM;
using System.Text;

public partial class Wiki_ShowTxt : PX.Web.UI.PXPage
{
	protected void Page_Load(object sender, EventArgs e)
	{
		string exportType = Request["type"];
		WikiReader reader = PXGraph.CreateInstance<WikiReader>();
		PXWikiSettings settings = new PXWikiSettings(this, reader);
		this.SetReaderFilter(reader);
		WikiPage article = reader.Pages.SelectWindowed(0, 1);
		reader.Pages.Current = article;
		PXRenderer renderer = this.CreateRenderer(exportType, settings.Absolute);
		if (article != null && !string.IsNullOrEmpty(article.Content) && renderer != null)
		{
			var ctx = new PXDBContext(settings.Absolute);

			ctx.Renderer = renderer;
			ctx.WikiID = article.WikiID;
			string result = PXWikiParser.Parse(article.Content, ctx);
			string mime = MimeTypes.GetMimeType("." + exportType);
			Response.AddHeader("cache-control", "no-store, private");
			Response.Clear();
			Response.Cache.SetCacheability(HttpCacheability.Private);
			Response.Cache.SetExpires(DateTime.Now.AddSeconds(2));
			Response.Cache.SetValidUntilExpires(true);
			Response.AddHeader("Content-Type", mime);
			Response.AddHeader("Content-Encoding", "ansi");
			this.AdditionalResponseParams(renderer, article);
			Response.Write(result);
		}
	}

	protected override void OnInit(EventArgs e)
	{
		// we don't need basic page initialization here - this page works as HttpHandler.
		//base.OnInit(e);
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

	private PXRenderer CreateRenderer(string exportType, PXDBContext settings)
	{
		switch (exportType)
		{
			case "txt":
				PXTxtRenderer resultTxt = new PXTxtRenderer(settings);
				resultTxt.ColsCount = 100;
				return resultTxt;
			case "rtf":
				PXRtfRenderer resultRtf = new PXRtfRenderer(settings);
				return resultRtf;
		}
		return null;
	}

	private void AdditionalResponseParams(PXRenderer renderer, WikiPage article)
	{
		if (renderer is PXRtfRenderer)
		{
			Response.AddHeader("content-disposition", "attachment;filename=\"" + article.Name + ".rtf\"");
			Response.ContentEncoding = this.GetCultureDependentEncoding();
		}
	}

	private Encoding GetCultureDependentEncoding()
	{
		if (System.Threading.Thread.CurrentThread.CurrentCulture.Name == "en-US")
			return Encoding.Default;
		return Encoding.GetEncoding(System.Threading.Thread.CurrentThread.CurrentCulture.TextInfo.ANSICodePage);
	}
}
