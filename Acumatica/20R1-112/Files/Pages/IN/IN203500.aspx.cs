using System;
using System.Configuration;
using System.Collections;
using System.Web;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.UI.HtmlControls;
using PX.Web.UI;

public partial class Page_IN203500 : PX.Web.UI.PXPage
{
	protected void Page_Load(object sender, EventArgs e)
	{
		InventoryMatrixEntry.CreateItemsPage_AddStyles(this);
	}

	protected void MatrixMatrix_RowDataBound(object sender, PXGridRowEventArgs e)
	{
		InventoryMatrixEntry.CreateItemsPage_RowDataBound(e.Row);
	}

	protected void MatrixMatrix_ColumnsGenerated(object sender, EventArgs e)
	{
		InventoryMatrixEntry.CreateItemsPage_ColumnsGenerated(MatrixMatrix.Columns);
	}

	protected void MatrixAttributes_AfterSyncState(object sender, EventArgs e)
	{
		InventoryMatrixEntry.EnableCommitChangesAndMoveExtraColumnAtTheEnd(MatrixAttributes.Columns, 0);
	}

	protected void MatrixMatrix_AfterSyncState(object sender, EventArgs e)
	{
		InventoryMatrixEntry.EnableCommitChangesAndMoveExtraColumnAtTheEnd(MatrixMatrix.Columns);
	}
}

