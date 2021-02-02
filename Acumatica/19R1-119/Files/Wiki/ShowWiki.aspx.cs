using System;
using System.Collections.Generic;
using System.Data.Design;
using System.Reflection;
using System.Web.UI.WebControls;
using PX.Common;
using PX.Data;
using PX.SM;
using PX.Web.Controls.Wiki;
using PX.Web.UI;
using PX.Data.Search;

public partial class Page_ShowWiki : ShowPage
{
	PXDropDown Rate;
	PXLabel LblRate;	
	PXLabel	PXKB;
	PXLabel	PXCategori;
	PXLabel	PXProduct;
	PXLabel	PXKBName;
    PXLabel PXCreateDate;
	PXLabel PXLastPublished;
	PXLabel	PXLastModified;    
	PXLabel	PXViews;
	PXLabel	PXRating;
	PXImage PXImage1;
	PXImage PXImage2;
	PXImage PXImage3;
	PXImage PXImage4;
	PXImage PXImage5;
	string rateid;
	string feedbackid;
	string pageid;
	PXLabel PXdAvRate;
	PXLabel UserMessage;
	public Int16 CurrentRating = 0;

	private const string _TEMPLATE_WIKI_ARTICLE = "PX.Web.Objects.Wiki.TemplateWikiArticle";
	private const string _ROOTTEMPLATE_PROPERTY = "RootTemplate";
	private static readonly Type _templateWikiArticleType;
	private static readonly PropertyInfo _rootTemplate;
	
	private PXWikiShow _wikiShow;

	static Page_ShowWiki()
	{
		_templateWikiArticleType = System.Web.Compilation.PXBuildManager.GetType(_TEMPLATE_WIKI_ARTICLE, false);
		if (_templateWikiArticleType != null) _rootTemplate = _templateWikiArticleType.GetProperty(_ROOTTEMPLATE_PROPERTY);
	}

	protected override void Page_Init(object sender, EventArgs e)
	{
		base.Page_Init(sender, e);

		Rate = PXFormView2.FindControl("Rate") as PXDropDown;
		LblRate = PXFormView2.FindControl("lblRate") as PXLabel;
		PXKB = PXFormView1.FindControl("PXKB") as PXLabel;
		PXCategori = PXFormView1.FindControl("PXCategori") as PXLabel;
		PXProduct = PXFormView1.FindControl("PXProduct") as PXLabel;
		PXKBName = PXFormView1.FindControl("PXKBName") as PXLabel;
        PXCreateDate = PXFormView1.FindControl("PXCreateDate") as PXLabel;
		PXLastPublished = PXFormView1.FindControl("PXLastPublished") as PXLabel;
		PXLastModified = PXFormView1.FindControl("PXLastModified") as PXLabel;
		PXViews = PXFormView1.FindControl("PXViews") as PXLabel;
		PXRating = PXFormView1.FindControl("PXRating") as PXLabel;
		PXImage1 = PXFormView1.FindControl("PXImage1") as PXImage;
		PXImage2 = PXFormView1.FindControl("PXImage2") as PXImage;
		PXImage3 = PXFormView1.FindControl("PXImage3") as PXImage;
		PXImage4 = PXFormView1.FindControl("PXImage4") as PXImage;
		PXImage5 = PXFormView1.FindControl("PXImage5") as PXImage;
		PXdAvRate = PXFormView1.FindControl("PXdAvRate") as PXLabel;
		UserMessage = PXFormView3.FindControl("UserMessage") as PXLabel;
		rateid = Request["rateid"];
		feedbackid = Request["feedbackid"];
		pageid = Request["pageid"];

		if (pageid != null)
		{
			Guid currentwikipage = new Guid(pageid);
			if (!String.IsNullOrEmpty(pageid))
			{

				InitHeader(currentwikipage);
			}

			var wiki = (PXResult<WikiPage, WikiDescriptor, WikiRevision>)PXSelectJoin<WikiPage,
				                                                InnerJoin <WikiDescriptor, On<WikiDescriptor.pageID, Equal<WikiPage.wikiID>>,
				                                                InnerJoin <WikiRevision, On<WikiRevision.pageID, Equal<WikiPage.pageID>>>>,
				                                                Where<WikiPage.pageID, Equal<Required<WikiPage.pageID>>>>.Select(
					                                                new PXGraph(), currentwikipage);

			if (wiki == null)
				HideRateMenu();
			else
			{
				var currentwiki = wiki[typeof (WikiDescriptor)] as WikiDescriptor;
				if (!PXSiteMap.IsPortal || currentwiki == null || currentwiki.SPWikiArticleType != WikiArticleType.KBArticle)
				{
					HideRateMenu();
				}
				else
				{
					CreateRateMenu(Rate);
				}
			}
		}
		else
		{
			HideRateMenu();
		}
	}	
		
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
			try
			{
				object article = pageID == Guid.Empty
					? Activator.CreateInstance(_templateWikiArticleType, graph, Request.Params["wiki"], Request.Params["art"], language, correctRevision)
					: Activator.CreateInstance(_templateWikiArticleType, graph, pageID, language, correctRevision);
				_wikiShow = new TemplateWikiShow((IWikiArticle)article, (IRootTemplateInfo)_rootTemplate.GetValue(article, null));
			}
			catch (StackOverflowException) { throw; }
			catch (OutOfMemoryException) { throw; }
			catch
			{
				_wikiShow = new PXWikiShow();
			}
		}
		else _wikiShow = new PXWikiShow();
		_wikiShow.NotFoundMessage = PXMessages.LocalizeNoPrefix(Msg.WkNotFound);
		base.Initialize();		
	}

	protected new void Page_Load(object sender, EventArgs e)
	{
		if (!this.Page.IsCallback)
		{
			MainForm.DataBind();
			if (PXSiteMap.IsPortal)
			{
				ViewCount();
			}			
		}			
		this._wikiShow.Width = Unit.Percentage(100);
		Page.ClientScript.RegisterClientScriptBlock(this.GetType(), 
			"wikiShowId", "var wikiShowId = '" + this.form.ClientID + "_" + this._wikiShow.ID + "';", true);

		var context = this._wikiShow.WikiContext as PX.Web.Objects.Wiki.PageDBContext;
		if (context != null && context.WikiID.HasValue)
		{
			Page.ClientScript.RegisterClientScriptBlock(
				this.GetType(), "wikiId", "var __wikiID = '" + context.WikiID.ToString() + "';", true);
		}

		if (PXSiteMap.IsPortal)
		{
			this.toolbar1.Items.Clear();
			/*if (pageid != null)
			{
				Guid currentwikipage = new Guid(pageid);
				var _wikiscript = PXDatabase.GetSlot<WikiScript>(_SLOT_KEY_PREFIX, typeof(WikiPageAnalytic));
				if(_wikiscript != null)
				{
					WikiPage wiki = PXSelect<WikiPage, Where<WikiPage.pageID, Equal<Required<WikiPage.pageID>>>>.SelectSingleBound(new PXGraph(), null, currentwikipage);
					Literal literal = new Literal();
					if(wiki != null)
						literal.Text = _wikiscript.GetScript((Guid)wiki.WikiID);
					dvAnalytics.Controls.Add(literal);
				}
			}*/
			if (rateid != null)
			{
				UserMessage.Text = PXMessages.LocalizeNoPrefix(Msg.WkThankForRating);
				PXFormView3.Visible = true;
			}
			if (feedbackid != null)
			{
				UserMessage.Text = PXMessages.LocalizeNoPrefix(Msg.WkThankForFeedback);
				PXFormView3.Visible = true;
			}
		}
	}

	private void InitHeader(Guid pageID)
	{	
		PXResult<WikiPage, WikiPageLanguage> result = (PXResult<WikiPage, WikiPageLanguage>)PXSelectJoin<WikiPage,
			InnerJoin<WikiPageLanguage, On<WikiPageLanguage.pageID, Equal<WikiPage.pageID>>>,
			Where<WikiPage.pageID, Equal<Required<WikiPage.pageID>>>>.SelectWindowed(new PXGraph(), 0, 1, pageID);

		PXResult<KBResponseSummary> resultsummary = (PXResult<KBResponseSummary>)PXSelect<KBResponseSummary, Where<KBResponseSummary.pageID, Equal<Required<KBResponseSummary.pageID>>>>.SelectWindowed(new PXGraph(), 0, 1, pageID);

		if (result != null)
		{
			WikiPage wp = result[typeof(WikiPage)] as WikiPage;
			WikiPageLanguage wpl = result[typeof(WikiPageLanguage)] as WikiPageLanguage;
			KBResponseSummary kbrs = new KBResponseSummary();

			if (resultsummary != null)
				kbrs = resultsummary[typeof(KBResponseSummary)] as KBResponseSummary;
			
			PXKB.Text = wp.Name + ": " + wpl.Title;
			
			PXCategori.Text = PXMessages.LocalizeNoPrefix(Msg.Category)+ ": " ;
			bool firstcategory = false;
			foreach (PXResult<SPWikiCategoryTags, SPWikiCategory> category in PXSelectJoin<SPWikiCategoryTags,
										InnerJoin<SPWikiCategory, On<SPWikiCategory.categoryID, Equal<SPWikiCategoryTags.categoryID>>>,
										Where<SPWikiCategoryTags.pageID, Equal<Required<SPWikiCategoryTags.pageID>>>>.Select(new PXGraph(), pageID))
			{
				
				SPWikiCategory wc = category[typeof(SPWikiCategory)] as SPWikiCategory;
				if (firstcategory)
					PXCategori.Text = PXCategori.Text + ", ";
				PXCategori.Text = PXCategori.Text + wc.Description;
				firstcategory = true;

			}

			PXProduct.Text = PXMessages.LocalizeNoPrefix(Msg.AppliesTo) + ": ";
			bool firstproduct = false;
			foreach (PXResult<SPWikiProductTags, SPWikiProduct> category in PXSelectJoin<SPWikiProductTags,
										InnerJoin<SPWikiProduct, On<SPWikiProduct.productID, Equal<SPWikiProductTags.productID>>>,
										Where<SPWikiProductTags.pageID, Equal<Required<SPWikiProductTags.pageID>>>>.Select(new PXGraph(), pageID))
			{
				SPWikiProduct wc = category[typeof(SPWikiProduct)] as SPWikiProduct;
				if (firstproduct)
					PXProduct.Text = PXProduct.Text + ", ";
				PXProduct.Text = PXProduct.Text + wc.Description;
				firstproduct = true;
			}

			PXKBName.Text = PXMessages.LocalizeNoPrefix(Msg.Article)+ ": " + wp.Name + ' ';
            PXCreateDate.Text = PXMessages.LocalizeNoPrefix(Msg.CreateDate) + ": " + wp.CreatedDateTime;
            PXLastPublished.Text = PXMessages.LocalizeNoPrefix(Msg.LastModified) + ": " + wpl.LastPublishedDateTime + ' ';
			PXViews.Text = PXMessages.LocalizeNoPrefix(Msg.Views) + ": " + kbrs.Views.ToString() + ' ';

			PXRating.Text = PXMessages.LocalizeNoPrefix(Msg.Rating) + ": ";
			if (kbrs != null && kbrs.Markcount != null && kbrs.Markcount != 0 && kbrs.Marksummary != null && kbrs.Marksummary != 0)
			{
				int AvRate = (int)((int)kbrs.Marksummary / (int)kbrs.Markcount);
				Int32 Marksummary = (int)kbrs.Marksummary;
				Int32 Markcount = (int)kbrs.Markcount;
				double dAvRate = (double)Marksummary / (double)Markcount;
				PXdAvRate.Text = "(" + Math.Round(dAvRate, 2).ToString() + ")";
				switch (AvRate)
				{ 
					case 0:
						PXImage1.ImageUrl = "main@FavoritesGray";
						PXImage2.ImageUrl = "main@FavoritesGray";
						PXImage3.ImageUrl = "main@FavoritesGray";
						PXImage4.ImageUrl = "main@FavoritesGray";
						PXImage5.ImageUrl = "main@FavoritesGray";
						break;
					case 1:
						PXImage1.ImageUrl = "main@Favorites";
						PXImage2.ImageUrl = "main@FavoritesGray";
						PXImage3.ImageUrl = "main@FavoritesGray";
						PXImage4.ImageUrl = "main@FavoritesGray";
						PXImage5.ImageUrl = "main@FavoritesGray";
						break;
					case 2:
						PXImage1.ImageUrl = "main@Favorites";
						PXImage2.ImageUrl = "main@Favorites";
						PXImage3.ImageUrl = "main@FavoritesGray";
						PXImage4.ImageUrl = "main@FavoritesGray";
						PXImage5.ImageUrl = "main@FavoritesGray";
						break;
					case 3:
						PXImage1.ImageUrl = "main@Favorites";
						PXImage2.ImageUrl = "main@Favorites";
						PXImage3.ImageUrl = "main@Favorites";
						PXImage4.ImageUrl = "main@FavoritesGray";
						PXImage5.ImageUrl = "main@FavoritesGray";
						break;
					case 4:
						PXImage1.ImageUrl = "main@Favorites";
						PXImage2.ImageUrl = "main@Favorites";
						PXImage3.ImageUrl = "main@Favorites";
						PXImage4.ImageUrl = "main@Favorites";
						PXImage5.ImageUrl = "main@FavoritesGray";
						break;
					case 5:
						PXImage1.ImageUrl = "main@Favorites";
						PXImage2.ImageUrl = "main@Favorites";
						PXImage3.ImageUrl = "main@Favorites";
						PXImage4.ImageUrl = "main@Favorites";
						PXImage5.ImageUrl = "main@Favorites";
						break;
					default:
						PXImage1.ImageUrl = "main@FavoritesGray";
						PXImage2.ImageUrl = "main@FavoritesGray";
						PXImage3.ImageUrl = "main@FavoritesGray";
						PXImage4.ImageUrl = "main@FavoritesGray";
						PXImage5.ImageUrl = "main@FavoritesGray";
						break;
				}				
			}	
		}		
	}	

	public void HideRateMenu()
	{
		PXFormView1.Visible = false;
		PXFormView2.Visible = false;		
	}

	protected override PXDataSource DS
	{
		get { return ds; }
	}

	protected override PXDataSource TreeDS
	{
		get { return dsTemplate; }
	}

	protected override PXFormView MainForm
	{
		get { return form; }
	}

	protected override PXToolBar MainToolbar
	{
		get { return toolbar1; }
	}

	protected override PXSmartPanel WikiTextPanel
	{
		get { return pnlWikiText; }
	}

	protected override PXTextEdit WikiTextEditor
	{
		get { return edWikiText; }
	}

	protected override PXTextEdit UrlEditor
	{
		get { return edtUrl; }
	}

	protected override PXTextEdit PublicUrlEditor
	{
		get { return edPublicUrl; }
	}

	protected override PXTextEdit LinkEditor
	{
		get { return edtLink; }
	}
		
	protected void ddRate_PageRate(object sender, PXCallBackEventArgs e)
	{
		//Filltables();		
	}

	protected void Rate_PageRate(object sender, EventArgs e)
	{
        Filltables();
        Guid currentwikipage = new Guid(pageid);

		PXGraph article = PXGraph.CreateInstance(typeof(KBArticleMaint));
		
		PXCache response = article.Caches[typeof(KBResponse)];
		PXCache responsesummary = article.Caches[typeof(KBResponseSummary)];

		PXResult<WikiPage, WikiPageLanguage, WikiRevision> result = (PXResult<WikiPage, WikiPageLanguage, WikiRevision>)PXSelectJoin
			<WikiPage,
 			InnerJoin<WikiPageLanguage, On<WikiPageLanguage.pageID, Equal<WikiPage.pageID>>,
			InnerJoin<WikiRevision, On<WikiRevision.pageID, Equal<WikiPage.pageID>>>>,						
			Where<WikiPage.pageID, Equal<Required<WikiPage.pageID>>>,
			OrderBy<Desc<WikiRevision.pageRevisionID>>>.SelectWindowed(new PXGraph(), 0, 1, currentwikipage);
		
		Guid userId = PXAccess.GetUserID();
		PXResult<KBResponseSummary> resultsummary = (PXResult<KBResponseSummary>)PXSelect<KBResponseSummary, Where<KBResponseSummary.pageID, Equal<Required<KBResponseSummary.pageID>>>>
			.SelectWindowed(article, 0, 1, currentwikipage);
			
		PXResult<KBResponse> resnonse = (PXResult<KBResponse>)PXSelect<KBResponse, 
			Where<KBResponse.userID, Equal<Required<KBResponse.userID>>,
					And<KBResponse.pageID, Equal<Required<KBResponse.pageID>>>>>.SelectWindowed(article, 0, 1, userId, currentwikipage);

		if (result != null)
		{
			WikiPage wp = result[typeof(WikiPage)] as WikiPage;
			WikiPageLanguage wpl = result[typeof(WikiPageLanguage)] as WikiPageLanguage;

			KBResponseSummary kbrs = responsesummary.CreateInstance() as KBResponseSummary;
			KBResponse newresnonse = response.CreateInstance() as KBResponse;

			if (resultsummary != null)
				kbrs = resultsummary[typeof(KBResponseSummary)] as KBResponseSummary;

			if (resnonse != null)
			{
				newresnonse = resnonse[typeof(KBResponse)] as KBResponse;
				if (wp != null && wpl != null && newresnonse.NewMark != null)
				{
					if (newresnonse.OldMark != 0)
					{
						kbrs.Marksummary = kbrs.Marksummary - newresnonse.OldMark;
						kbrs.Marksummary = kbrs.Marksummary + newresnonse.NewMark;
					}
					else 
					{
						kbrs.Markcount = kbrs.Markcount + 1;
						kbrs.Marksummary = kbrs.Marksummary + newresnonse.NewMark;						
					}

					int AvRate = (int)((int)kbrs.Marksummary / (int)kbrs.Markcount);
					Int32 Marksummary = (int)kbrs.Marksummary;
					Int32 Markcount = (int)kbrs.Markcount;
					double dAvRate = (double)Marksummary / (double)Markcount;
					kbrs.AvRate = dAvRate;

					responsesummary.Update(kbrs);
					responsesummary.PersistUpdated(kbrs);
					responsesummary.Clear();


					newresnonse.PageID = currentwikipage;
					newresnonse.RevisionID = 1;
					newresnonse.OldMark = newresnonse.NewMark;
					newresnonse.Date = PXTimeZoneInfo.Now;
					newresnonse.Summary = "";
					newresnonse.CreatedByID = wp.CreatedByID;
					newresnonse.CreatedByScreenID = "WP00000";
					newresnonse.CreatedDateTime = wp.CreatedDateTime;
					newresnonse.LastModifiedByID = wp.LastModifiedByID;
					newresnonse.LastModifiedByScreenID = "WP00000";
					newresnonse.LastModifiedDateTime = wp.LastModifiedDateTime;

					response.Update(newresnonse);
					response.PersistUpdated(newresnonse);
					response.Clear();					
				}
			}

			else
			{
				if (wp != null && wpl != null && newresnonse.NewMark != null)
				{
					newresnonse.PageID = currentwikipage;
					newresnonse.RevisionID = 1;
					newresnonse.OldMark = newresnonse.NewMark;
					newresnonse.Date = PXTimeZoneInfo.Now;
					newresnonse.UserID = userId;
					newresnonse.Summary = "";
					newresnonse.CreatedByID = wp.CreatedByID;
					newresnonse.CreatedByScreenID = "WP00000";
					newresnonse.CreatedDateTime = wp.CreatedDateTime;
					newresnonse.LastModifiedByID = wp.LastModifiedByID;
					newresnonse.LastModifiedByScreenID = "WP00000";
					newresnonse.LastModifiedDateTime = wp.LastModifiedDateTime;

					if (kbrs == null || kbrs.PageID == null)
					{
						kbrs.PageID = currentwikipage;
						kbrs.CreatedByID = wp.CreatedByID;
						kbrs.CreatedByScreenID = "WP00000";
						kbrs.CreatedDateTime = wp.CreatedDateTime;
						kbrs.LastModifiedByID = wp.LastModifiedByID;
						kbrs.LastModifiedByScreenID = "WP00000";
						kbrs.LastModifiedDateTime = wp.LastModifiedDateTime;
						kbrs.Markcount = 1;
						kbrs.Marksummary = newresnonse.NewMark;

						int AvRate = (int)((int)kbrs.Marksummary / (int)kbrs.Markcount);
						Int32 Marksummary = (int)kbrs.Marksummary;
						Int32 Markcount = (int)kbrs.Markcount;
						double dAvRate = (double)Marksummary / (double)Markcount;
						kbrs.AvRate = dAvRate;

						responsesummary.Insert(kbrs);
						responsesummary.PersistInserted(kbrs);
						responsesummary.Clear();
					}
					else
					{
						kbrs.Markcount = kbrs.Markcount + 1;
						kbrs.Marksummary = kbrs.Marksummary + newresnonse.NewMark;
						responsesummary.Update(kbrs);
						responsesummary.PersistUpdated(kbrs);
						responsesummary.Clear();
					}
					response.Insert(newresnonse);
					response.PersistInserted(newresnonse);
					response.Clear();					
				}
			}

			string path = PXUrl.SiteUrlWithPath();
			path += path.EndsWith("/") ? "" : "/";
			var url = string.Format("{0}Wiki/{1}?pageid={2}",
				path, this.ResolveClientUrl("~/Wiki/ShowWiki.aspx"), pageid);
			url = url + "&rateid=" + Rate.Value;
			throw new Exception("Redirect0:" + url);
		}			
	}

	protected void Feedback_Rate(object sender, EventArgs e)
	{
		KBFeedbackMaint graph = PXGraph.CreateInstance<KBFeedbackMaint>();
		KBFeedback command = graph.Responses.Insert();
		command.PageID = Guid.Parse(pageid);
		graph.Responses.Current = command;
		new PXDataSource.RedirectHelper(ds).TryRedirect(new PXPopupRedirectException(graph, ""));
	}

	private void CreateRateMenu(PXDropDown dd)
	{
		if (PXSiteMap.IsPortal)
		{
			String[] MarkList = new String[5]
			{
			    PXMessages.LocalizeNoPrefix(Msg.Terrible),
                PXMessages.LocalizeNoPrefix(Msg.NotHelpful),
                PXMessages.LocalizeNoPrefix(Msg.Average),
                PXMessages.LocalizeNoPrefix(Msg.Helpful),
                PXMessages.LocalizeNoPrefix(Msg.Excellent)
			};

			if (PXSiteMap.IsPortal)
			{
				PXListItem liall = new PXListItem("<"+PXMessages.LocalizeNoPrefix(Msg.SelectTip) +">");
				dd.Items.Add(liall);
				for (Int16 i = 5; i > 0; i--)
				{
					PXListItem li = new PXListItem(i.ToString() + " - " + MarkList[i - 1], i.ToString());
					dd.Items.Add(li);
				}
			}

			for (int i = 0; i < dd.Items.Count; i++)
			{
				if (rateid == dd.Items[i].Value)
				{
					dd.SelectedIndex = i;
				}
			}			
		}
	}

	protected void Filltables()
	{
		Guid currentwikipage = new Guid(pageid);

		PXGraph article = PXGraph.CreateInstance(typeof(KBArticleMaint));

		PXCache response = article.Caches[typeof(KBResponse)];
		PXCache responsesummary = article.Caches[typeof(KBResponseSummary)];

		PXResult<WikiPage, WikiPageLanguage> result = (PXResult<WikiPage, WikiPageLanguage>)PXSelectJoin<WikiPage,
			InnerJoin<WikiPageLanguage, On<WikiPageLanguage.pageID, Equal<WikiPage.pageID>>>,
			Where<WikiPage.pageID, Equal<Required<WikiPage.pageID>>>,
			OrderBy<Desc<WikiRevision.pageRevisionID>>>
			.SelectWindowed(new PXGraph(), 0, 1, currentwikipage);

		Guid userId = PXAccess.GetUserID();
		PXResult<KBResponseSummary> resultsummary = (PXResult<KBResponseSummary>)PXSelect<KBResponseSummary, Where<KBResponseSummary.pageID, Equal<Required<KBResponseSummary.pageID>>>>.
			SelectWindowed(article, 0, 1, currentwikipage);

		PXResult<KBResponse> resnonse = (PXResult<KBResponse>)PXSelect<KBResponse,
			Where<KBResponse.userID, Equal<Required<KBResponse.userID>>,
					And<KBResponse.pageID, Equal<Required<KBResponse.pageID>>>>>.
					SelectWindowed(article, 0, 1, userId, currentwikipage);

		if (result != null)
		{
			WikiPage wp = result[typeof(WikiPage)] as WikiPage;
			WikiPageLanguage wpl = result[typeof(WikiPageLanguage)] as WikiPageLanguage;

			KBResponse newresnonse = response.CreateInstance() as KBResponse;

			if (resnonse != null)
			{
				newresnonse = resnonse[typeof(KBResponse)] as KBResponse;
				if (wp != null && wpl != null)
				{
					Int16 res;
					if (Int16.TryParse(Rate.Value.ToString(), out res))
					{
						newresnonse.PageID = currentwikipage;
						newresnonse.RevisionID = 1;
						newresnonse.NewMark = res;
						newresnonse.Date = PXTimeZoneInfo.Now;
						newresnonse.Summary = "";
						newresnonse.CreatedByID = wp.CreatedByID;
						newresnonse.CreatedByScreenID = "WP00000";
						newresnonse.CreatedDateTime = wp.CreatedDateTime;
						newresnonse.LastModifiedByID = wp.LastModifiedByID;
						newresnonse.LastModifiedByScreenID = "WP00000";
						newresnonse.LastModifiedDateTime = wp.LastModifiedDateTime;

						response.Update(newresnonse);
						response.PersistUpdated(newresnonse);
						response.Clear();
					}
				}
			}
		}
	}

	protected void ViewCount()
	{
		if (pageid != null)
		{
			Guid currentwikipage = new Guid(pageid);

			PXGraph article = PXGraph.CreateInstance(typeof(KBArticleMaint));
			PXCache responsesummary = article.Caches[typeof(KBResponseSummary)];
			PXCache responses = article.Caches[typeof(KBResponse)];

			PXResult<WikiPage, WikiPageLanguage, WikiRevision> result = (PXResult<WikiPage, WikiPageLanguage, WikiRevision>)PXSelectJoin<WikiPage,
				InnerJoin<WikiPageLanguage, On<WikiPageLanguage.pageID, Equal<WikiPage.pageID>>,
				InnerJoin<WikiRevision, On<WikiRevision.pageID, Equal<WikiPage.pageID>>>>,
				Where<WikiPage.pageID, Equal<Required<WikiPage.pageID>>>>
				.SelectWindowed(new PXGraph(), 0, 1, currentwikipage);

			Guid userId = PXAccess.GetUserID();
			PXResult<KBResponseSummary> resultsummary = (PXResult<KBResponseSummary>)PXSelect<KBResponseSummary, Where<KBResponseSummary.pageID, Equal<Required<KBResponseSummary.pageID>>>>
				.SelectWindowed(article, 0, 1, currentwikipage);
			PXResult<KBResponse> resnonse = (PXResult<KBResponse>)PXSelect<KBResponse,
				Where<KBResponse.userID, Equal<Required<KBResponse.userID>>,
				And<KBResponse.pageID, Equal<Required<KBResponse.pageID>>>>>
				.SelectWindowed(article, 0, 1, userId, currentwikipage);

			if (result != null)
			{
				WikiPage wp = result[typeof(WikiPage)] as WikiPage;
				WikiPageLanguage wpl = result[typeof(WikiPageLanguage)] as WikiPageLanguage;

				KBResponseSummary kbrs = responsesummary.CreateInstance() as KBResponseSummary;
				KBResponse newresnonse = responses.CreateInstance() as KBResponse;

				if (resultsummary != null)
					kbrs = resultsummary[typeof(KBResponseSummary)] as KBResponseSummary;

				if (resnonse != null)
					newresnonse = resnonse[typeof(KBResponse)] as KBResponse;

				if (wp != null && wpl != null)
				{
					if (kbrs == null || kbrs.PageID == null)
					{
						kbrs.PageID = currentwikipage;
						kbrs.Views = 1;
						kbrs.Markcount = 0;
						kbrs.Marksummary = 0;
						kbrs.CreatedByID = wp.CreatedByID;
						kbrs.CreatedByScreenID = "WP00000";
						kbrs.CreatedDateTime = wp.CreatedDateTime;
						kbrs.LastModifiedByID = wp.LastModifiedByID;
						kbrs.LastModifiedByScreenID = "WP00000";
						kbrs.LastModifiedDateTime = wp.LastModifiedDateTime;
						kbrs.tstamp = wp.tstamp;
						responsesummary.Insert(kbrs);
						responsesummary.PersistInserted(kbrs);
						responsesummary.Clear();
					}
					else
					{
						kbrs.Views++;
						responsesummary.Update(kbrs);
						responsesummary.PersistUpdated(kbrs);
						responsesummary.Clear();
					}

					if (newresnonse == null || newresnonse.PageID == null)
					{
						newresnonse.PageID = currentwikipage;
						newresnonse.RevisionID = 1;
						newresnonse.OldMark = 0;
						newresnonse.NewMark = 0;
						newresnonse.Date = PXTimeZoneInfo.Now;
						newresnonse.UserID = userId;
						newresnonse.Summary = "";
						newresnonse.CreatedByID = wp.CreatedByID;
						newresnonse.CreatedByScreenID = "WP00000";
						newresnonse.CreatedDateTime = wp.CreatedDateTime;
						newresnonse.LastModifiedByID = wp.LastModifiedByID;
						newresnonse.LastModifiedByScreenID = "WP00000";
						newresnonse.LastModifiedDateTime = wp.LastModifiedDateTime;
						newresnonse.tstamp = wp.tstamp;
						responses.Insert(newresnonse);
						responses.PersistInserted(newresnonse);
						responses.Clear();
					}
				}
			}
		}
	}
}