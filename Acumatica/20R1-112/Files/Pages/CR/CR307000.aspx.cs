using System;
using PX.Data.Wiki.Parser;
using PX.Objects.CR;
using PX.Web.Controls.KB;
using PX.Web.UI;

public partial class CR307000 : PXPage
{
	protected void Page_Init(object sender, EventArgs e)
	{
		this.Master.PopupWidth = 750;
		this.Master.PopupHeight = 500;
	}

	protected void Page_Load(object sender, EventArgs e)
	{
		PXDBContext context = new PXDBContext(new PXWikiSettings(this).Absolute);
		SendKBArticleMaint graph = ds.DataGraph as SendKBArticleMaint;
		if (graph != null)
		{
			graph.WikiSettings = context.Settings;
			context.WikiID = graph.Message.Current.WikiID;
		}

		PXWikiEdit wikiEdit = form.FindControl("wikiEdit") as PXWikiEdit;
		if (wikiEdit != null) 
		{
			wikiEdit.PreviewSettings = context;
			form.DataBound += new EventHandler(delegate(object o, EventArgs args)
			{
				PXDBContext settings = context;
				//settings.StyleID = SitePolicy.DefaultMailStyle;
				wikiEdit.PreviewSettings = settings;
			});
		}

		PXKBShow edContent = frmInsertArticle.FindControl("edContent") as PXKBShow;
	}
}
