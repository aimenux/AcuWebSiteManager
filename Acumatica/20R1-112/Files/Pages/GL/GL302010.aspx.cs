using System;
using System.Web.UI.WebControls;
using PX.Web.UI;


public partial class Page_GL302010 : PXPage
{
	protected void Page_Init(object sender, EventArgs e)
	{	
		for (int i = 1; ; i++)
		{
			string fieldName = "Period" + i;
			if (!ds.DataGraph.Caches["GLBudgetLine"].Fields.Contains(fieldName))
			{
				break;
			}

			PXGridColumn col = new PXGridColumn
			{
				DataField = fieldName,
				DataType = TypeCode.Decimal,
				Decimals = 2,
				TextAlign = HorizontalAlign.Right,
				AllowNull = false,
				RenderEditorText = true,
				AutoCallBack = true,
				Width = Unit.Pixel(70)
			};
			col.Header.Text = fieldName;
			var grid = sp1.FindControl("grid") as PXGrid;
			grid.Columns.Add(col);
		}
	}
}
