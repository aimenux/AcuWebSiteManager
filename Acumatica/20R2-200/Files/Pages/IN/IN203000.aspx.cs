using PX.Objects.IN.Matrix.DAC.Unbound;
using PX.Objects.IN.Matrix.GraphExtensions;
using PX.Web.UI;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Web.UI.WebControls;

public partial class Page_IN203000 : PX.Web.UI.PXPage
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
		InventoryMatrixEntry.CreateItemsPage_ColumnsGenerated(((PXGrid)sender).Columns);
	}

	protected void MatrixAttributes_AfterSyncState(object sender, EventArgs e)
	{
		AddNewColumns(sender, typeof(AdditionalAttributes), TypeCode.String, GridColumnType.NotSet);

		InventoryMatrixEntry.EnableCommitChangesAndMoveExtraColumnAtTheEnd(((PXGrid)sender).Columns, 0);
	}

	protected void MatrixMatrix_AfterSyncState(object sender, EventArgs e)
	{
		AddNewColumns(sender, typeof(EntryMatrix), TypeCode.Boolean, GridColumnType.CheckBox);

		InventoryMatrixEntry.EnableCommitChangesAndMoveExtraColumnAtTheEnd(((PXGrid)sender).Columns);
	}

	protected void MatrixItems_AfterSyncState(object sender, EventArgs e)
	{
		InventoryMatrixEntry.InsertAttributeColumnsByTemplateColumn(
			((PXGrid)sender).Columns, ds.DataGraph.Caches[typeof(ItemsGridExt.MatrixInventoryItem)].Fields);
	}

	protected virtual void AddNewColumns(object sender, Type itemType, TypeCode dataType, GridColumnType type)
	{
		var columnCollection = ((PXGrid)sender).Columns;
		IEnumerable<PXGridColumn> columns = columnCollection.Cast<PXGridColumn>();
		var newFields = ds.DataGraph.Caches[itemType].Fields
			.Where(f => f.StartsWith(InventoryMatrixEntry.Template) && char.IsDigit(f.Last())
				&& !columns.Any(c => string.Compare(c.DataField, f, true) == 0));

		foreach (var newField in newFields)
			columnCollection.Add(new PXGridColumn() { DataField = newField, DataType = dataType, Type = type });
	}
}
