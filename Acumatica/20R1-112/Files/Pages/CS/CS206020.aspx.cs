using System;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Collections.Generic;
using PX.Data;
using PX.Web.UI;

public partial class Page_CS206020 : PX.Web.UI.PXPage
{
	protected void Page_Load(object sender, EventArgs e)
	{
	    var gridHeader = sp1.FindControl("gridHeader") as PXGrid;
        var gridColumn = sp1.FindControl("gridColumn") as PXGrid;
		ds.DataGraph.SetValue("Parameter", ds.DataGraph.Views["Parameter"].Cache.Current, "HeaderCellPredAIndex", GetPredAIndex(gridHeader));
		ds.DataGraph.SetValue("Parameter", ds.DataGraph.Views["Parameter"].Cache.Current, "ColumnCellPredAIndex", GetPredAIndex(gridColumn));
				
		string parameter = "  A";
		for (int i = 0; i < (gridHeader.ClientState.ActiveCell - 3) / 2 - 1; i++)
		{
			parameter = PX.CS.RMColumnCodeAttribute.ShiftCode(parameter, true);
		}
		ds.DataGraph.SetValue("Parameter", ds.DataGraph.Views["Parameter"].Cache.Current, "HeaderCell", parameter);
		ds.DataGraph.SetValue("Parameter", ds.DataGraph.Views["Parameter"].Cache.Current, "HeaderActiveCellIndex", gridHeader.ClientState.ActiveCell);
		parameter = "  A";
		for (int i = 0; i < gridColumn.ClientState.ActiveCell - 2; i++)
		{
			parameter = PX.CS.RMColumnCodeAttribute.ShiftCode(parameter, true);
		}
		ds.DataGraph.SetValue("Parameter", ds.DataGraph.Views["Parameter"].Cache.Current, "ColumnCell", parameter);
		ds.DataGraph.SetValue("Parameter", ds.DataGraph.Views["Parameter"].Cache.Current, "ColumnActiveCellIndex", gridColumn.ClientState.ActiveCell);
		string rowID = gridHeader.ClientState.ActiveRowID;
		if (!String.IsNullOrEmpty(rowID))
		{
			int usc = rowID.LastIndexOf('_');
			if (usc != -1 && usc < rowID.Length - 1)
			{
				rowID = rowID.Substring(usc + 1);
				if (int.TryParse(rowID, out usc))
				{
					ds.DataGraph.SetValue("Parameter", ds.DataGraph.Views["Parameter"].Cache.Current, "HeaderRow", usc + 1);
				}
			}
		}
        if (!this.Page.IsCallback)
        {
            this.Page.ClientScript.RegisterClientScriptBlock(GetType(), "dsID", "var dsID=\"" + this.ds.ClientID + "\";", true);
        }

    }

    protected void Page_Init(object sender, EventArgs e)
	{
        var gridHeader = sp1.FindControl("gridHeader") as PXGrid;
        var gridColumn = sp1.FindControl("gridColumn") as PXGrid;
        
        ds.DataGraph.Views["Headers"].RefreshRequested += new EventHandler(Page_Headers_RefreshRequested);
		ds.DataGraph.Views["Properties"].RefreshRequested += new EventHandler(Page_Properties_RefreshRequested);
		if (PXPageCache.IsReloadPage) // !IsCallback
		{
			ds.DataGraph.RowSelected.AddHandler("ColumnSet", Page_ColumnSet_RowSelected);
		}
		gridColumn.RepaintColumns = true;
		CreateColumns(ds.GetSchema("Properties"), gridColumn);
		gridHeader.RepaintColumns = true;
		CreateColumns(ds.GetSchema("Headers"), gridHeader);
	}

    void Page_ColumnSet_RowSelected(PXCache sender, PXRowSelectedEventArgs e)
	{
        var gridHeader = sp1.FindControl("gridHeader") as PXGrid;
        var gridColumn = sp1.FindControl("gridColumn") as PXGrid;
        
        CreateColumns(ds.GetSchema("Headers"), gridHeader);
		CreateColumns(ds.GetSchema("Properties"), gridColumn);
	}

	void Page_Properties_RefreshRequested(object sender, EventArgs e)
	{
        var gridColumn = sp1.FindControl("gridColumn") as PXGrid;
        
        CreateColumns(ds.GetSchema("Properties"), gridColumn);
	}

	void Page_Headers_RefreshRequested(object sender, EventArgs e)
	{
        var gridHeader = sp1.FindControl("gridHeader") as PXGrid;
         
        CreateColumns(ds.GetSchema("Headers"), gridHeader);
	}

	protected void CreateColumns(PXDataSourceViewSchema schema, PXGrid grid)
	{
        var gridColumn = sp1.FindControl("gridColumn") as PXGrid;
        
        List<string> list = new List<string>();
		foreach (PXGridColumn col in grid.Levels[0].Columns)
		{
			if (!String.IsNullOrEmpty(col.DataField))
			{
				list.Add(col.DataField);
			}
		}
		foreach (PXFieldState field in schema.GetFields())
		{
			if (field.Name.Length <= 3 || field.Name.EndsWith("_StyleID"))
			{
				if (!list.Contains(field.Name))
				{
					PXGridColumn col = new PXGridColumn();
					col.DataField = field.Name;
					col.DataType = Type.GetTypeCode(field.DataType);
					col.Width = Unit.Pixel(150);
					col.Header.Text = field.DisplayName;
					col.AllowShowHide = AllowShowHide.Server;
					if (grid == gridColumn)
					{
						col.TextField = field.Name + "_Text";
					}
					else if (field.Name.Length <= 3)
					{
						col.EditorID = "edColHeader";
					}
					grid.Columns.Add(col);
				}
			}
			list.Remove(field.Name);
		}
		foreach (string field in list)
		{
			foreach (PXGridColumn col in grid.Levels[0].Columns)
			{
				if (col.DataField == field)
				{
					grid.Levels[0].Columns.Remove(col);
					break;
				}
			}
		}
	}

	/// <summary>
	/// The row data bound event handler.
	/// </summary>
	protected void gridColumn_RowDataBound(object sender, PXGridRowEventArgs e)
	{
		switch ((string)e.Row.DataKey[0])
		{
			case "StyleID":
				e.Row.EditorID = "edColumnST";
				break;
			case "DataSourceID":
				e.Row.EditorID = "edColumnDS";
				break;
			case "Formula":
            case "VisibleFormula":
				e.Row.EditorID = "edColumnF";
				break;
		}
	}

	private int GetPredAIndex(PXGrid grid)
	{
		int predAIndex = 0;
		foreach (PXGridColumn column in grid.Columns)
		{
			if (!string.Equals(column.Header.Text, "  A", StringComparison.OrdinalIgnoreCase))
				predAIndex++;
			else
				break;
		}
		predAIndex--;
		return predAIndex;
	}
}
