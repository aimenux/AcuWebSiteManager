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
	public class SPWikiProductMaint : PXGraph<SPWikiProductMaint>
	{
        public SPWikiProductMaint()
        {
            Wiki.BlockIfOnlineHelpIsOn();
        }

        #region Select
		public PXSelect<SPWikiProduct> WikiProduct;
		public PXSelect<SPWikiProductTags, Where<SPWikiProductTags.productID, Equal<Current<SPWikiProduct.productID>>>> WikiProductDetails;
		public SPWikiCategoryMaint.PXSelectWikiFoldersTree Folders;
		#endregion

		#region Actions
		public PXSave<SPWikiProduct> Save;
		public PXCancel<SPWikiProduct> Cancel;
		public PXInsert<SPWikiProduct> Insert;
		public PXDelete<SPWikiProduct> Delete;
		public PXFirst<SPWikiProduct> First;
		public PXPrevious<SPWikiProduct> Previous;
		public PXNext<SPWikiProduct> Next;
		public PXLast<SPWikiProduct> Last;
		#endregion

		#region EventHandler
		protected virtual void SPWikiProductTags_RowInserting(PXCache sender, PXRowInsertingEventArgs e)
		{
			SPWikiProductTags row = e.Row as SPWikiProductTags;
			if (row != null)
			{
				row.ProductID = WikiProduct.Current.ProductID;

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

		protected virtual void SPWikiProductTags_RowUpdated(PXCache sender, PXRowUpdatedEventArgs e)
		{
			SPWikiProductTags row = e.Row as SPWikiProductTags;
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

