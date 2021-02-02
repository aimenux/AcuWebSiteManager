using System;
using System.Web;
using PX.Data;
using PX.Data.Wiki.Parser;
using PX.Data.Wiki.Parser.Pdf;
using PX.SM;
using System.IO;

public partial class Wiki_ShowPdf : PX.Web.UI.PXPage
{
	protected void Page_Load(object sender, EventArgs e)
	{
		WikiReader reader = PXGraph.CreateInstance<WikiReader>();
        PXWikiSettings settings = new PXWikiSettings(this, reader);
		reader.Filter.Current.PageID = PX.Common.GUID.CreateGuid(this.Request["PageID"]);
		WikiPage article = reader.Pages.SelectWindowed(0, 1);
		reader.Pages.Current = article;
		if (article != null && !string.IsNullOrEmpty(article.Content))
		{
			MemoryStream stream = new MemoryStream();
			PXPdfRenderer renderer = new PXPdfRenderer(stream, settings.Relative);
			settings.Relative.Renderer = renderer;
			PXWikiParser.Parse(article.Content, settings.Relative);
			string mime = MimeTypes.GetMimeType(".pdf");
			byte[] bytes = new byte[renderer.ResultPDFStream.Length];

			renderer.ResultPDFStream.Position = 0;
			renderer.ResultPDFStream.Read(bytes, 0, bytes.Length);
			Response.AddHeader("cache-control", "no-store, private");
			Response.Clear();
			Response.Cache.SetCacheability(HttpCacheability.Private);
			Response.Cache.SetExpires(DateTime.Now.AddSeconds(2));
			Response.Cache.SetValidUntilExpires(true);
			Response.AddHeader("Content-Type", mime);
			Response.AddHeader("Content-Length", renderer.ResultPDFStream.Length.ToString());
			Response.BinaryWrite(bytes);
		}
	}
}
