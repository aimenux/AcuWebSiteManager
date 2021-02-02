using PX.Data;
using PX.SM;
using PX.Web.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.UI.WebControls;

public partial class Pages_SM205530 : PX.Web.UI.PXPage
{
    //Select view to generate grid columns
    protected void OnGridLayoutLoad(object sender, PXGridLayoutEventArgs e)
    {
        PXGrid grid = sender as PXGrid;
        PXGraph graph = grid.DataGraph;

        int startRow = 0;
        int maximumRows = 1;
        int totalRows = 0;

        graph.ExecuteSelect("Changes", new object[0], new object[0], new string[0], new bool[0], new PXFilterRow[0], ref startRow, maximumRows, ref totalRows);
    }
    protected void Page_Init(object sender, EventArgs e)
    {
		AUAuditInquire graph = ds.DataGraph as AUAuditInquire;
		graph.OnInitialised += OnInitialised;
		
		PXGrid gridKeys = sp1.FindControl("gridKeys") as PXGrid;
		PXGrid gridChanges = sp1.FindControl("gridChanges") as PXGrid;
		gridKeys.AllowAutoHide = false;
		gridKeys.RepaintColumns = true;
		gridChanges.AllowAutoHide = false;
		gridChanges.RepaintColumns = true;
    }

	protected void OnInitialised()
	{
		PXGrid gridKeys = sp1.FindControl("gridKeys") as PXGrid;
		PXGrid gridChanges = sp1.FindControl("gridChanges") as PXGrid;

		CreateColumns(ds.GetSchema("Keys"), gridKeys);
		CreateColumns(ds.GetSchema("Changes"), gridChanges);

		AddFastFilterFields(gridKeys);
	}

	private void AddFastFilterFields(PXGrid grid)
	{
		if (grid != null && grid.Levels != null && grid.Levels[0] != null && grid.Levels[0].Columns != null)
		{
			List<string> fastFilterFields = new List<string>();

			for (int i = 0; i < grid.Levels[0].Columns.Count; i++)
			{
				if (!string.IsNullOrEmpty(grid.Levels[0].Columns[i].DataField))
				{
					fastFilterFields.Add(grid.Levels[0].Columns[i].DataField);
				}
			}

			if (fastFilterFields.Count > 0)
			{
				grid.FastFilterFields = fastFilterFields.ToArray();
			}
		}
	}

	protected void CreateColumns(PXDataSourceViewSchema schema, PXGrid grid)
	{
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
			if (!AUAuditInquire.FORBIDDEN_COLUMNS.Contains(field.Name) && field.Name.EndsWith(AUAuditInquire.VIRTUAL_FIELD_SUFFIX) && !list.Contains(field.Name))
			{
				PXGridColumn col = new PXGridColumn();
				col.DataField = field.Name;
				col.DataType = Type.GetTypeCode(field.DataType);
				col.TextAlign = HorizontalAlign.Right;
				col.Width = Unit.Pixel(120);
				switch (col.DataType)
				{
					case TypeCode.Boolean:
						col.Width = Unit.Pixel(60);
						col.Type = GridColumnType.CheckBox;
						col.TextAlign = HorizontalAlign.Center;
						break;
					case TypeCode.DateTime:
						col.TextAlign = HorizontalAlign.Left;
						col.DisplayFormat = "g";
						break;
					case TypeCode.String:
						col.TextAlign = HorizontalAlign.Left;
						break;
				}
				col.Visible = true;
				col.Header.Text = field.DisplayName == field.Name
                    ? field.DisplayName.Substring(0, field.DisplayName.Length - AUAuditInquire.VIRTUAL_FIELD_SUFFIX.Length)
					: field.DisplayName;
				col.AllowShowHide = AllowShowHide.Server;
				grid.Columns.Add(col);
			}

			list.Remove(field.Name);
		}

		foreach (string field in list)
		{
			PXGridColumn col = GetColumn(grid, field);
			if (col != null) grid.Levels[0].Columns.Remove(col);
		}

        grid.FastFilterFields = grid.Columns.Items.Select(col => col.DataField).ToArray();

		//if (grid.Levels[0].Columns.Count <= 1) grid.AllowAutoHide = true;
	}

	private PXGridColumn GetColumn(PXGrid grid, String name)
	{
		foreach (PXGridColumn col in grid.Levels[0].Columns)
		{
			if (col.DataField == name) return col;
		}
		return null;
	}
}