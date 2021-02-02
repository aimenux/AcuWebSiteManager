using PX.Data;
using PX.Objects.SM;
using System;
using PX.SM;
using System.Collections;
using System.Web;
using PX.Data.Search;

namespace PX.Objects.SM
{
	[Serializable]
	public class SPWikiCategoryMaint : PXGraph<SPWikiCategoryMaint>
	{
        public SPWikiCategoryMaint()
        {
            Wiki.BlockIfOnlineHelpIsOn();
        }

        #region WikiFoldersTree
		public class PXSelectWikiFoldersTree : PXSelectBase<WikiPageSimple>
		{
			public PXSelectWikiFoldersTree(PXGraph graph)
			{
				this.View = CreateView(graph, new PXSelectDelegate<Guid?>(folders));
			}
			public PXSelectWikiFoldersTree(PXGraph graph, Delegate handler)
			{
				this.View = CreateView(graph, handler);
			}

			private PXView CreateView(PXGraph graph, Delegate handler)
			{
				return new PXView(graph, false,
					new Select<WikiPageSimple, Where<WikiPageSimple.parentUID, Equal<Argument<Guid?>>>,
							OrderBy<Asc<WikiPageSimple.number>>>(),
					handler);
			}

			internal IEnumerable folders(
				[PXGuid]
			Guid? PageID
			)
			{
				bool needRoot = false;
				if (PageID == null)
				{
					PageID = Guid.Empty;
					needRoot = true;
				}

				HttpContext ctx = HttpContext.Current;
				HttpContext.Current = null; // remove context to avoid rights check.
				PXSiteMapNode parent = PXSiteMap.WikiProvider.FindSiteMapNodeFromKey(PageID.Value);
				if (needRoot)
					yield return this.CreateWikiPageSimple(parent);
				else
					foreach (PXSiteMapNode node in parent.ChildNodes)
						yield return this.CreateWikiPageSimple(node);
				HttpContext.Current = ctx;
			}

			private WikiPageSimple CreateWikiPageSimple(PXSiteMapNode node)
			{
				WikiPageSimple result = new WikiPageSimple();
				result.PageID = node.NodeID;
				result.Name = (node as PXWikiMapNode)?.Name;
				result.ParentUID = node.ParentID;
				result.Title = string.IsNullOrEmpty(node.Title) ? ((PXWikiMapNode)node).Name : node.Title;
				return result;
			}
		}
		#endregion

		#region Select
		public PXSelect<SPWikiCategory> WikiCategory;
		public PXSelect<SPWikiCategoryTags, Where<SPWikiCategoryTags.categoryID, Equal<Current<SPWikiCategory.categoryID>>>> WikiCategoryDetails;
		public PXSelectWikiFoldersTree Folders;
		
		#endregion

		#region Actions
		public PXSave<SPWikiCategory> Save;
		public PXCancel<SPWikiCategory> Cancel;
		public PXInsert<SPWikiCategory> Insert;
		public PXDelete<SPWikiCategory> Delete;
		public PXFirst<SPWikiCategory> First;
		public PXPrevious<SPWikiCategory> Previous;
		public PXNext<SPWikiCategory> Next;
		public PXLast<SPWikiCategory> Last;
		#endregion
		
		#region EventHandler
		protected virtual void SPWikiCategoryTags_RowInserting(PXCache sender, PXRowInsertingEventArgs e)
		{
			SPWikiCategoryTags row = e.Row as SPWikiCategoryTags;
			if (row != null)
			{
				row.CategoryID = WikiCategory.Current.CategoryID;

				if (row == null || row.PageName == null)
					return;

				PXResult<WikiPage, WikiPageLanguage> currentrow = (PXResult<WikiPage, WikiPageLanguage>)PXSelectJoin<WikiPage,
					LeftJoin<WikiPageLanguage, On<WikiPage.pageID, Equal<WikiPageLanguage.pageID>>>,
					Where<WikiPage.name, Equal<Required<WikiPage.name>>>>.SelectWindowed(this, 0, 1, row.PageName);

				if (currentrow != null)
				{
					WikiPage wp = currentrow[typeof(WikiPage)] as WikiPage;
					WikiPageLanguage wpl = currentrow[typeof(WikiPageLanguage)] as WikiPageLanguage;

					if (wp != null)
					{
						row.PageName = wp.Name;
						row.PageID = wp.PageID;
					}

					if (wpl != null)
					{
						row.PageTitle = wpl.Title;
					}
					else
					{
						row.PageTitle = row.PageName; 
					}
				}
			}			
		}

		protected virtual void SPWikiCategoryTags_RowUpdated(PXCache sender, PXRowUpdatedEventArgs e)
		{
			SPWikiCategoryTags row = e.Row as SPWikiCategoryTags;
			if (row == null || row.PageName == null)
					return;

			PXResult<WikiPage, WikiPageLanguage> currentrow = (PXResult<WikiPage, WikiPageLanguage>)PXSelectJoin<WikiPage,
					LeftJoin<WikiPageLanguage, On<WikiPage.pageID, Equal<WikiPageLanguage.pageID>>>,
					Where<WikiPage.name, Equal<Required<WikiPage.name>>>>.SelectWindowed(this, 0, 1, row.PageName);

			if (currentrow != null)
			{
				WikiPage wp = currentrow[typeof(WikiPage)] as WikiPage;
				WikiPageLanguage wpl = currentrow[typeof(WikiPageLanguage)] as WikiPageLanguage;

				if (wp != null)
				{
					row.PageName = wp.Name;
					row.PageID = wp.PageID;
				}

				if (wpl != null)
				{
					row.PageTitle = wpl.Title;
				}
				else
				{
					row.PageTitle = row.PageName;
				}
			}
		}
		#endregion
	}
}
