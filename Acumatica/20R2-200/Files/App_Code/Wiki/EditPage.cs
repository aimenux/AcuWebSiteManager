using System;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using PX.SM;
using PX.Data;
using PX.Web.UI;
using PX.Data.Wiki.Parser;

public abstract class EditPage<T> : PXPage where T : PXGraph, IWikiPageMaint
{
	private PXDataSource ds;
	private PXTab tab;
	PXGrid gridSub, gridAttachments;
	bool hasQueryParams;
	PXWikiEdit edtWikiText;

	protected virtual void Page_Init(object sender, EventArgs e)
	{
		this.ds = this.DataSource;
		this.tab = this.TabView;
		T graph = (T)ds.DataGraph;

		/*T graph = Activator.CreateInstance(typeof(T)) as T;
		if (graph == null) return;
		ds.DataGraph = graph;*/

		/*if(string.IsNullOrEmpty(this.Request.QueryString["wiki"]))
		{			
		    WikiPage current = (WikiPage)graph.Caches[typeof(WikiPage)].Current;
		    if (current != null)
		    {
		        wiki = current.WikiID.ToString();
		        if (!this.Page.IsCallback && !this.Page.IsPostBack && !string.IsNullOrEmpty(wiki))
		        {
		            graph.InitNew(PX.Common.GUID.CreateGuid(wiki), PX.Common.GUID.CreateGuid(current.ParentUID.ToString()), current.Name);
		            ds.PageLoadBehavior = PXPageLoadBehavior.PopulateSavedValues;
		            this.hasQueryParams = true;					
		        }
		    }
		}
		else
		{
		   */
		if (!this.Page.IsCallback && !this.Page.IsPostBack && !string.IsNullOrEmpty(Request.QueryString["wiki"]))
		{
			graph.InitNew(PX.Common.GUID.CreateGuid(this.Request.QueryString["wiki"]), PX.Common.GUID.CreateGuid(this.Request.QueryString["parent"]), this.Request.QueryString["name"]);
			graph.Caches[typeof(PX.SM.WikiArticle)].IsDirty = false;
			ds.PageLoadBehavior = PXPageLoadBehavior.PopulateSavedValues;
			this.hasQueryParams = true;
		}
		//}
	}

	protected void Page_Load(object sender, EventArgs e)
	{
		T graph = (T)ds.DataGraph;
		this.gridSub = this.tab.FindControl("gridSub") as PXGrid;
		this.gridAttachments = this.tab.FindControl("gridAttachments") as PXGrid;

		WikiPage curr = (WikiPage)graph.Caches[typeof(WikiPage)].Current;
		if (!this.hasQueryParams && curr != null && !this.Page.IsPostBack && !this.Page.IsCallback)
		{
			this.ds.DataGraph.Clear();
            if(this.Request.Params.Count > 0)
                this.Response.Redirect(this.Request.RawUrl);
            else
                this.Response.Redirect(this.Request.RawUrl + "?wiki=" + curr.WikiID.GetValueOrDefault().ToString() + "&parent=" + curr.ParentUID.GetValueOrDefault().ToString() + "&name=" + curr.Name);
            return;
		}
		
		WikiReader reader = PXGraph.CreateInstance<WikiReader>();


		this.edtWikiText = (PXWikiEdit)this.tab.DataControls["edtWikiText"];
		this.edtWikiText.PreviewSettings = new PXWikiSettings(this.Page, reader).Relative;
		this.edtWikiText.FileNamePrefix = curr == null ? null : curr.Name;
		this.edtWikiText.FileUploaded += new PXFileUploadEventHandler(editor_FileUploaded);

		graph.GetFileUrl = Request.GetWebsiteAuthority().GetLeftPart(UriPartial.Authority) + ResolveUrl("~/Frames/GetFile.ashx");
		Guid? attachment = (Guid?) gridAttachments.DataValues["FileID"];		
		graph.CurrentAttachmentGuid = attachment ?? Guid.Empty;

		PXEntryStatus recStatus = ds.DataGraph.Caches[typeof(WikiPage)].GetStatus(curr);
		if (recStatus == PXEntryStatus.Deleted || recStatus == PXEntryStatus.Inserted || recStatus == PXEntryStatus.InsertedDeleted)
			edtWikiText.AttachFileEnabled = false;

		
	}

	protected void tab_DataBinding(object sender, EventArgs e)
	{
		this.Page.ClientScript.RegisterClientScriptBlock(GetType(), "gridID", "var gridID=\"" + this.gridSub.ClientID + "\";", true);
		WikiPage curr = (WikiPage)ds.DataGraph.Caches[typeof(WikiPage)].Current;
		if (curr != null)
		{
			/*PXRichTextEdit edHtmlText = (PXRichTextEdit)this.tab.DataControls["edHtmlText"];

			edHtmlText.WikiArticleId = curr.PageID;
			if (curr.IsHtml == true)
			{
				PXUIFieldAttribute.SetVisible<WikiPage.content>(ds.DataGraph.Caches[typeof(WikiPage)], curr, false);
				PXUIFieldAttribute.SetVisible<WikiPage.contentHtml>(ds.DataGraph.Caches[typeof(WikiPage)], curr, true);  
			}
			else
			{
				PXUIFieldAttribute.SetVisible<WikiPage.content>(ds.DataGraph.Caches[typeof(WikiPage)], curr, true);
				PXUIFieldAttribute.SetVisible<WikiPage.contentHtml>(ds.DataGraph.Caches[typeof(WikiPage)], curr, false);  
			}*/
			((PXDBContext)this.edtWikiText.PreviewSettings).WikiID = curr.WikiID;
			this.Page.Header.Controls.Add(new LiteralControl(PXHtmlFormatter.GetPageStyle(this.edtWikiText.PreviewSettings)));
			this.ScreenTitle = string.IsNullOrEmpty(curr.Title) ? (string.IsNullOrEmpty(curr.Name) ? this.ScreenTitle : curr.Name) : curr.Title;
			if (curr.Folder != true) tab.Items["Subarticles"].Visible = false;
			if (!Page.IsPostBack)
			{
				WikiDescriptor wiki = PXSelect<WikiDescriptor,
					Where<WikiDescriptor.pageID,
					Equal<Required<WikiDescriptor.pageID>>>>.SelectWindowed(new PXGraph(), 0, 1, curr.WikiID);
				PXAuditJournal.Register("WE000000", "Edit: " +
					Wiki.Link((wiki != null ? wiki.Name : curr.WikiID.ToString()), curr.Name));
			}

			PXWikiMapNode node = PXSiteMap.WikiProvider.FindSiteMapNodeFromKey(curr.PageID.GetValueOrDefault()) as PXWikiMapNode;
			if (node != null)
				this.CurrentNode = node;

			if (!this.Page.IsCallback && !this.Page.IsPostBack && !string.IsNullOrEmpty(Request.QueryString["wiki"]))
			{
				T graph = (T)ds.DataGraph;
				graph.Caches[typeof(PX.SM.WikiArticle)].IsDirty = false;
			}
		}
	}

	protected void editor_FileUploaded(object sender, PXFileUploadEventArgs e)
	{
		WikiPage current = (WikiPage)ds.DataGraph.Caches[typeof(WikiPage)].Current;
		if (current != null)
		{
			FileInfo file = e.UploadedFile;
			if (file != null && file.UID != null)
			{
				UploadFileMaintenance uploadGraph = PXGraph.CreateInstance<UploadFileMaintenance>();
				uploadGraph.AttachToPage((Guid)file.UID, (Guid)current.PageID);
			}
		}
	}

	protected abstract PXDataSource DataSource
	{
		get;
	}

	protected abstract PXTab TabView
	{
		get;
	}

	protected abstract string ScreenTitle
	{
		get;
		set;
	}

	protected virtual SiteMapNode CurrentNode
	{
		get { return _currentNode; }
		set { _currentNode = value; }
	}

	private SiteMapNode _currentNode;
}