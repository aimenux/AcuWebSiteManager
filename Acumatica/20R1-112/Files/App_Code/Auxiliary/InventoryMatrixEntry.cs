using PX.Data;
using PX.Web.UI;
using System.Linq;
using System.Web.UI.WebControls;

public class InventoryMatrixEntry
{
	public const string TemplateField = "AttributeValue0";
	public const string Template = "AttributeValue";

	public static void EnableCommitChangesAndMoveExtraColumnAtTheEnd(PXGridColumnCollection columns, int? extraColumnWidth = null, string linkCommand = null)
	{
		PXGridColumn extra = null;
		foreach (PXGridColumn col in columns)
		{
			col.CommitChanges = true;
			if (col.DataField == "Extra")
				extra = col;

			if (col.DataField.StartsWith(InventoryMatrixEntry.Template) && char.IsDigit(col.DataField.Last()))
			{
				col.LinkCommand = linkCommand;
			}
		}
		if (extra != null)
		{
			columns.Remove(extra);
			if (extraColumnWidth != null)
				extra.Width = new Unit((int)extraColumnWidth);
			extra.AllowDragDrop = false;
			extra.AllowMove = false;
			extra.AllowResize = false;
			columns.Add(extra);
		}
	}

	public static void InsertAttributeColumnsByTemplateColumn(PXGridColumnCollection columns, PXFieldCollection fields)
	{
		var attributeFields = fields.Where(f => f.StartsWith(Template) && char.IsDigit(f.Last())).ToList();

		int templateFieldIndex = -1;
		PXGridColumn templateColumn = null;

		foreach (PXGridColumn column in columns)
		{
			if (column.DataField == TemplateField)
			{
				templateFieldIndex = columns.IndexOf(column);
				templateColumn = column;
			}

			attributeFields.Remove(column.DataField);
		}

		if (templateColumn != null)
		{
			foreach (string attributeField in attributeFields)
			{
				var newColumn = new PXGridColumn();
				newColumn.CopyFrom(templateColumn);
				newColumn.DataField = attributeField;
				columns.Insert(++templateFieldIndex, newColumn);
			}
		}
	}

	public static void CreateItemsPage_AddStyles(PXPage page)
	{
		Style escalated = new Style();
		escalated.BackColor = System.Drawing.Color.LightBlue;
		page.Page.Header.StyleSheet.CreateStyleRule(escalated, page, ".CssEscalated");
		Style cellStyle = new Style();
		cellStyle.BackColor = System.Drawing.Color.Blue;
		page.Page.Header.StyleSheet.CreateStyleRule(cellStyle, page, ".CellStyle");
	}

	public static void CreateItemsPage_RowDataBound(PXGridRow row)
	{
		PX.Objects.IN.Matrix.DAC.Unbound.EntryMatrix item = row.DataItem as PX.Objects.IN.Matrix.DAC.Unbound.EntryMatrix;
		if (item == null) return;
		if (item.IsPreliminary == true)
		{
			row.Style.CssClass = "CssEscalated";
			foreach (PXGridCell cell in row.Cells)
			{
				if (cell.DataField == "Preliminary")
				{
					cell.Style.CssClass = "CellStyle";
					break;
				}
			}
		}
	}
	public static void CreateItemsPage_ColumnsGenerated(PXGridColumnCollection columns)
	{
		foreach (PXGridColumn col in columns)
		{
			if (col.DataField == "Preliminary")
			{
				col.Style.CssClass = "CssEscalated";
				break;
			}
		}
	}
}