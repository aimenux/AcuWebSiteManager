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
using PX.Web.UI;
using PX.Data.Wiki.Parser;
using System.Text;
using System.Globalization;

public partial class Wiki_Compare : PX.Web.UI.PXPage
{
	PX.SM.WikiReader helpObject = new PX.SM.WikiReader();
	protected PXLabel ver1, ver2;
	protected HtmlContainerControl content1, content2;

	protected void Page_Init(object sender, EventArgs e)
	{
		this.usrCaption.ScreenTitle = PX.Data.PXMessages.LocalizeNoPrefix(PX.SM.Messages.CompareVersions);

		((PXLabel)this.PXFormView1.FindControl("PXLabel1")).Text = PX.Data.PXMessages.LocalizeNoPrefix(PX.SM.Messages.DeletedContent);
		((PXLabel)this.PXFormView1.FindControl("PXLabel2")).Text = PX.Data.PXMessages.LocalizeNoPrefix(PX.SM.Messages.AddedContent);
		((PXLabel)this.PXFormView1.FindControl("PXLabel3")).Text = PX.Data.PXMessages.LocalizeNoPrefix(PX.SM.Messages.ChangedContent);

		((PXLabel)this.PXSmartPanel1.FindControl("PXLabel4")).Text = PX.Data.PXMessages.LocalizeNoPrefix(PX.SM.Messages.Words);
		((PXLabel)this.PXSmartPanel1.FindControl("PXLabel6")).Text = PX.Data.PXMessages.LocalizeNoPrefix(PX.SM.Messages.CharactersNoSpaces);
		((PXLabel)this.PXSmartPanel1.FindControl("PXLabel8")).Text = PX.Data.PXMessages.LocalizeNoPrefix(PX.SM.Messages.CharactersWithSpaces);
		((PXLabel)this.PXSmartPanel1.FindControl("PXLabel5")).Text = PX.Data.PXMessages.LocalizeNoPrefix(PX.SM.Messages.Symbols);
		((PXLabel)this.PXSmartPanel1.FindControl("PXLabel10")).Text = PX.Data.PXMessages.LocalizeNoPrefix(PX.SM.Messages.Words);
		((PXLabel)this.PXSmartPanel1.FindControl("PXLabel12")).Text = PX.Data.PXMessages.LocalizeNoPrefix(PX.SM.Messages.CharactersNoSpaces);
		((PXLabel)this.PXSmartPanel1.FindControl("PXLabel14")).Text = PX.Data.PXMessages.LocalizeNoPrefix(PX.SM.Messages.CharactersWithSpaces);
		((PXLabel)this.PXSmartPanel1.FindControl("PXLabel7")).Text = PX.Data.PXMessages.LocalizeNoPrefix(PX.SM.Messages.Symbols);
		((PXLabel)this.PXSmartPanel1.FindControl("PXLabel9")).Text = PX.Data.PXMessages.LocalizeNoPrefix(PX.SM.Messages.LinesUpdated);
		((PXLabel)this.PXSmartPanel1.FindControl("PXLabel13")).Text = PX.Data.PXMessages.LocalizeNoPrefix(PX.SM.Messages.LinesAdded);
		((PXLabel)this.PXSmartPanel1.FindControl("PXLabel16")).Text = PX.Data.PXMessages.LocalizeNoPrefix(PX.SM.Messages.LinesDeleted);
		((PXLabel)this.PXSmartPanel1.FindControl("PXLabel11")).Text = PX.Data.PXMessages.LocalizeNoPrefix(PX.SM.Messages.SymbolsChanged);
		((PXLabel)this.PXSmartPanel1.FindControl("PXLabel17")).Text = PX.Data.PXMessages.LocalizeNoPrefix(PX.SM.Messages.SymbolsAdded);
		((PXLabel)this.PXSmartPanel1.FindControl("PXLabel19")).Text = PX.Data.PXMessages.LocalizeNoPrefix(PX.SM.Messages.SymbolsDeleted);
	}

	protected void Page_Load(object sender, EventArgs e)
	{
		this.usrCaption.CustomizationAvailable = false;
		ver1 = PXFormView1.FindControl("ver1") as PXLabel;
		ver2 = PXFormView1.FindControl("ver2") as PXLabel;
		content1 = PXFormView1.FindControl("content1") as HtmlContainerControl;
		content2 = PXFormView1.FindControl("content2") as HtmlContainerControl;
		this.usrCaption.ScreenID = "WI.00.00.50";

		ver1.Text = Request.QueryString["id1"];
		ver2.Text = Request.QueryString["id2"];

		Guid pageID = PX.Common.GUID.CreateGuid(Request.QueryString["pageID"])?? Guid.Empty;
		int id1 = 0;
		int id2 = 0;
		

		if (!string.IsNullOrEmpty(ver1.Text) && !Int32.TryParse(ver1.Text, out id1))
			id1 = 0;
		if (!string.IsNullOrEmpty(ver2.Text) && !Int32.TryParse(ver2.Text, out id2))
			id2 = 0;

		string art1, art2;
		bool docompare = true;
		art1 = GetArticle(pageID, id1, ver1, this.pnlStatLeft);
		CollectLeftStats(art1);
		art2 = GetArticle(pageID, id2, ver2, this.pnlStatRight);
		CollectRightStats(art2);
		if (string.IsNullOrEmpty(art1))
		{
			art1 = PX.Data.PXMessages.LocalizeNoPrefix(PX.SM.Messages.SourceArticle);
			docompare = false;
		}
		if (string.IsNullOrEmpty(art2))
		{
			art2 = PX.Data.PXMessages.LocalizeNoPrefix(PX.SM.Messages.DestinationArticle);
			docompare = false;
		}

		if(docompare)
			CompareArticles(art1, art2, ref art1, ref art2);
		this.content1.InnerHtml = art1;
		this.content2.InnerHtml = art2;
	}

	private string GetArticle(Guid pageID, int id, PXLabel label, PXPanel statPanel)
	{
		DateTime date = DateTime.MinValue;
		string article = GetArticle(pageID, id, ref date);
		label.Text = date.ToString("g", System.Threading.Thread.CurrentThread.CurrentCulture);
		statPanel.Caption = label.Text;
		return article;
	}

	private string GetArticle(Guid pageID, int id, ref DateTime date)
	{
		helpObject.Clear();
		helpObject.Filter.Current.PageID = pageID;
		helpObject.Filter.Current.PageRevisionID = id;
		helpObject.Pages.Current = helpObject.Pages.Select();
		PX.SM.WikiPage article = helpObject.Pages.Current;
		if (article == null)
			return null;

		this.usrCaption.ScreenTitle = PX.Data.PXMessages.LocalizeNoPrefix(PX.SM.Messages.CompareVersions);
		this.usrCaption.ScreenTitle += ": " + article.Title;
		date = article.PageRevisionDateTime.GetValueOrDefault();
		return HttpUtility.HtmlEncode(article.Content);
	}

	private void CollectLeftStats(string article)
	{
		int words, chars, charswithspaces, symbols;
		DoCalcs(article, out words, out chars, out charswithspaces, out symbols);
		this.lblWords1.Text = words.ToString();
		this.lblChars1.Text = chars.ToString();
		this.lblCharsWithSpaces1.Text = charswithspaces.ToString();
		this.lblSymbols1.Text = symbols.ToString();
	}

	private void CollectRightStats(string article)
	{
		int words, chars, charswithspaces, symbols;
		DoCalcs(article, out words, out chars, out charswithspaces, out symbols);
		this.lblWords2.Text = words.ToString();
		this.lblChars2.Text = chars.ToString();
		this.lblCharsWithSpaces2.Text = charswithspaces.ToString();
		this.lblSymbols2.Text = symbols.ToString();
	}

	private void CollectLinesStats(int updated, int added, int deleted)
	{
		this.lblLinesUpdated.Text = updated.ToString();
		this.lblLinesAdded.Text = added.ToString();
		this.lblLinesDeleted.Text = deleted.ToString();
	}

	private void CollectSymbolsStats(int changed, int added, int deleted)
	{
		this.lblSymbolsUpdated.Text = changed.ToString();
		this.lblSymbolsAdded.Text = added.ToString();
		this.lblSymbolsDeleted.Text = deleted.ToString();
	}

	private void DoCalcs(string article, out int words, out int chars, out int charswithspaces, out int symbols)
	{
		words = chars = charswithspaces = symbols = 0;
		if (string.IsNullOrEmpty(article))
			return;
		symbols = article.Length;
		for (int i = 0; i < article.Length - 1; i++)
		{
			if (char.IsWhiteSpace(article[i + 1]) && !char.IsWhiteSpace(article[i]) && char.GetUnicodeCategory(article[i]) != UnicodeCategory.MathSymbol)
				words++;
			if (char.GetUnicodeCategory(article[i]) != UnicodeCategory.MathSymbol)
			{
				charswithspaces++;
				if (!char.IsWhiteSpace(article[i]))
					chars++;
			}
		}
		if (char.GetUnicodeCategory(article[symbols - 1]) != UnicodeCategory.MathSymbol)
		{
			charswithspaces++;
			if (!char.IsWhiteSpace(article[symbols - 1]))
			{
				words++;
				chars++;
			}
		}
	}

	private void CompareArticles(string ver1, string ver2, ref string leftRes, ref string rightRes)
	{
		System.Text.StringBuilder res1 = new System.Text.StringBuilder();
		System.Text.StringBuilder res2 = new System.Text.StringBuilder();
		DiffList_StringData s1 = new DiffList_StringData(ver1);
		DiffList_StringData s2 = new DiffList_StringData(ver2);
		DiffEngine de = new DiffEngine();
		ArrayList rep;
		int i = 0, count1 = 1, count2 = 1;
		int linesupdated = 0, linesadded = 0, linesdeleted = 0;
		int symbolschanged = 0, symbolsadded = 0, symbolsdeleted = 0;

		de.ProcessDiff(s1, s2, DiffEngineLevel.SlowPerfect);
		rep = de.DiffReport();

		foreach (DiffResultSpan drs in rep)
		{
			switch (drs.Status)
			{
				case DiffResultSpanStatus.DeleteSource:
					res1.Append("<a name=\"left_" + count1.ToString("0000") + "\">" + "</a>\r\n");
					for (i = 0; i < drs.Length; i++)
					{
						res1.Append(count1.ToString("0000") + "&nbsp;&nbsp;");
						res1.Append("&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;<span style=\"background-color: #FF7863\">");
						res1.Append(((TextLine)s1.GetByIndex(drs.SourceIndex + i)).Line + "<br />");
						res1.Append("</span>");
						res2.Append("<br />");
						count1++;
						linesdeleted++;
					}
					break;
				case DiffResultSpanStatus.NoChange: 
					for (i = 0; i < drs.Length; i++)
					{
						res1.Append(count1.ToString("0000") + "&nbsp;&nbsp;");
						res2.Append(count2.ToString("0000") + "&nbsp;&nbsp;");
						res1.Append("&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;");
						res2.Append("&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;");
						res1.Append(((TextLine)s1.GetByIndex(drs.SourceIndex + i)).Line + "<br />");
						res2.Append(((TextLine)s2.GetByIndex(drs.DestIndex + i)).Line + "<br />");
						count1++; count2++;
					}
					break;
				case DiffResultSpanStatus.AddDestination:
					res1.Append("<a name=\"right_" + count2.ToString("0000") + "\">" + "</a>\r\n");
					for (i = 0; i < drs.Length; i++)
					{
						res1.Append("<br />");
						res2.Append(count2.ToString("0000") + "&nbsp;&nbsp;");
						res2.Append("&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;<span style=\"background-color: #54C954\">");
						res2.Append(((TextLine)s2.GetByIndex(drs.DestIndex + i)).Line + "<br />");
						res2.Append("</span>");
						count2++;
						linesadded++;
					}
					break;
				case DiffResultSpanStatus.Replace:
					res1.Append("<a name=\"left_" + count1.ToString("0000") + "\">" + "</a>\r\n");
					for (i = 0; i < drs.Length; i++)
					{
						res1.Append(count1.ToString("0000") + "&nbsp;&nbsp;");
						res2.Append(count2.ToString("0000") + "&nbsp;&nbsp;");
						res1.Append("&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;<span style=\"background-color: #C1E5FF\">");
						res2.Append("&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;<span style=\"background-color: #C1E5FF\">");
						CompareLines(((TextLine)s1.GetByIndex(drs.SourceIndex + i)).Line, ((TextLine)s2.GetByIndex(drs.DestIndex + i)).Line, res1, res2, ref symbolschanged, ref symbolsadded, ref symbolsdeleted);
						res1.Append("</span>");
						res2.Append("</span>");
						res1.Append("<br />");
						res2.Append("<br />");
						count1++; count2++;
						linesupdated++;
					}
					break;
			}
		}
		CollectLinesStats(linesupdated, linesadded, linesdeleted);
		CollectSymbolsStats(symbolschanged, symbolsadded, symbolsdeleted);
		leftRes = res1.ToString();
		rightRes = res2.ToString();
	}

	private void CompareLines(string line1, string line2, StringBuilder res1, StringBuilder res2, ref int changed, ref int added, ref int deleted)
	{
		DiffList_CharData s1 = new DiffList_CharData(line1);
		DiffList_CharData s2 = new DiffList_CharData(line2);
		DiffEngine de = new DiffEngine();
		ArrayList rep;
		int i = 0;

		de.ProcessDiff(s1, s2, DiffEngineLevel.SlowPerfect);
		rep = de.DiffReport();
		foreach (DiffResultSpan drs in rep)
		{
			switch (drs.Status)
			{
				case DiffResultSpanStatus.DeleteSource:
					res1.Append("<span style=\"background-color: #FBB2A7\">");
					for (i = 0; i < drs.Length; i++)
					{
						res1.Append(s1.GetByIndex(drs.SourceIndex + i));
						deleted++;
					}
					res1.Append("</span>");
					break;
				case DiffResultSpanStatus.AddDestination:
					res2.Append("<span style=\"background-color: #85D685\">");
					for (i = 0; i < drs.Length; i++)
					{
						res2.Append(s2.GetByIndex(drs.DestIndex + i));
						added++;
					}
					res2.Append("</span>");
					break;
				case DiffResultSpanStatus.NoChange:
					for (i = 0; i < drs.Length; i++)
					{
						res1.Append(s1.GetByIndex(drs.SourceIndex + i));
						res2.Append(s2.GetByIndex(drs.DestIndex + i));
					}
					break;
				case DiffResultSpanStatus.Replace:
					res1.Append("<span style=\"background-color: #75C5FF\">");
					res2.Append("<span style=\"background-color: #75C5FF\">");
					for (i = 0; i < drs.Length; i++)
					{
						res1.Append(s1.GetByIndex(drs.SourceIndex + i));
						res2.Append(s2.GetByIndex(drs.DestIndex + i));
						changed++;
					}
					res1.Append("</span>");
					res2.Append("</span>");
					break;
			}
		}
	}
}
