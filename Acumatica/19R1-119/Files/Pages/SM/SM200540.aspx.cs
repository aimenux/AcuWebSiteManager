using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.UI.WebControls;
using PX.Data;
using PX.SM;
using PX.Web.UI;

public partial class Page_SM260000 : PX.Web.UI.PXPage
{
	protected void Page_Init(object sender, EventArgs e)
	{
		TranslationMaint graph = ds.DataGraph as TranslationMaint;
		if (graph != null)
		{
			graph.Translator.OnLocalizationRecordsInitialised += OnInitialised;
			graph.Translator.OnLocalizationRecordsObsoleteInitialised += OnInitialisedObsolete;
			graph.Translator.OnLocalizationExceptionalRecordsInitialised += OnInitializedExceptional;
			graph.Translator.OnLocalizationExceptionalRecordsObsoleteInitialised += OnInitializedExceptiolanObsolete;
		}

		PXGrid[] grids = { GetValueGrid(), GetExceptionalValueGrid(), GetObsoleteValueGrid(), GetExceptionalObsoleteValueGrid() };
		foreach (PXGrid g in grids)
		{
			if (g != null)
			{
				g.AllowAutoHide = false;
				g.RepaintColumns = true;
			}
		}
	}

	protected void InitializeDynamicGrid(PXGrid grid, TranslationMaint graph, string gridDataMember)
	{
		if (grid != null && graph != null && graph.LanguageFilter.Current != null && !string.IsNullOrEmpty(gridDataMember))
		{
			if (!string.IsNullOrEmpty(graph.LanguageFilter.Current.Language))
			{
				CreateColumns(ds.GetSchema(gridDataMember).GetFields(), grid, graph.Translator.Locales, graph.LanguageFilter.Current.Language.Split(TranslationMaint.MultilingualTranslator.LANGUAGE_SEPARATOR));
			}
			else
			{
				DeleteOldColumns(ds.GetSchema(gridDataMember).GetFields(), grid, graph.Translator.LocaleKeys);
			}
		}
	}

	protected void OnInitialised()
	{
		InitializeDynamicGrid(GetValueGrid(), ds.DataGraph as TranslationMaint, "DeltaResourcesDistinct");
	}

	protected void OnInitialisedObsolete()
	{
		InitializeDynamicGrid(GetObsoleteValueGrid(), ds.DataGraph as TranslationMaint, "DeltaResourcesDistinctObsolete");
	}

	protected void OnInitializedExceptional()
	{
		InitializeDynamicGrid(GetExceptionalValueGrid(), ds.DataGraph as TranslationMaint, "ExceptionalResources");
	}

	protected void OnInitializedExceptiolanObsolete()
	{
		InitializeDynamicGrid(GetExceptionalObsoleteValueGrid(), ds.DataGraph as TranslationMaint, "ExceptionalResourcesObsolete");
	}

	protected void DeleteOldColumns(IEnumerable<PXFieldState> schemaFields, PXGrid grid, string[] locales)
	{
		PXGridColumn[] columnsToDelete = grid.Levels[0].Columns
										 .Cast<PXGridColumn>()
										 .Where(column => locales.Contains(column.DataField))
										 .ToArray();
		List<string> fastFilterFieldsList = new List<string>(grid.FastFilterFields);

		foreach (PXGridColumn column in columnsToDelete)
		{
			grid.Levels[0].Columns.Remove(column);
			fastFilterFieldsList.Remove(column.DataField);
		}

		grid.FastFilterFields = fastFilterFieldsList.ToArray();
	}

	protected void CreateColumns(IEnumerable<PXFieldState> schemaFields, PXGrid grid, Dictionary<string, string> locales, string[] languages)
	{
		if (locales != null)
		{
			List<string> list = grid.Levels[0].Columns
								.Cast<PXGridColumn>()
								.Where(col => !string.IsNullOrEmpty(col.DataField))
								.Select(col => col.DataField)
								.Concat(TranslationMaint.MultilingualTranslator.FORBIDDEN_COLUMNS)
								.ToList();
			PXFieldState[] dynamicFields = schemaFields
										   .Where(field => list.All(col => string.Compare(field.Name, col, StringComparison.InvariantCultureIgnoreCase) != 0))
										   .ToArray();
			//Array.Sort(dynamicFields, (f1, f2) => string.Compare(locales[f1.Name], locales[f2.Name], StringComparison.InvariantCultureIgnoreCase));

			foreach (PXFieldState field in dynamicFields)
			{
				PXGridColumn col = new PXGridColumn
				{
					DataField = field.Name,
					DataType = TypeCode.String,
					Multiline = true,
					TextAlign = HorizontalAlign.Left,
					Width = Unit.Pixel(250),
					Visible = true,
					AllowShowHide = AllowShowHide.Server,
				};

				col.Header.Text = locales[field.Name];
				grid.Columns.Add(col);
			}

			List<PXGridColumn> columnsToDelete = grid.Levels[0].Columns
												 .Cast<PXGridColumn>()
												 .Where(column => locales.ContainsKey(column.DataField) && !languages.Contains(column.DataField))
												 .ToList();
			foreach (PXGridColumn column in columnsToDelete)
			{
				grid.Levels[0].Columns.Remove(column);
			}
		}
	}

	protected PXGrid GetValueGrid()
	{
		PXGrid gridValue = null;
		PXSplitContainer container = tab.FindControl("sp1") as PXSplitContainer;

		if (container != null)
		{
			gridValue = container.FindControl("gridValue") as PXGrid;
		}

		return gridValue;
	}

	protected PXGrid GetExceptionalValueGrid()
	{
		PXGrid gridValueExceptional = null;
		PXSplitContainer container = tab.FindControl("sp1") as PXSplitContainer;

		if (container != null)
		{
			gridValueExceptional = container.FindControl("gridValueExceptional") as PXGrid;
		}

		return gridValueExceptional;
	}

	protected PXGrid GetObsoleteValueGrid()
	{
		PXGrid gridValueObsolete = null;
		PXSplitContainer container = tab.FindControl("sp2") as PXSplitContainer;

		if (container != null)
		{
			gridValueObsolete = container.FindControl("gridValueObsolete") as PXGrid;
		}

		return gridValueObsolete;
	}

	protected PXGrid GetExceptionalObsoleteValueGrid()
	{
		PXGrid gridExceptionalObsoleteValue = null;
		PXSplitContainer container = tab.FindControl("sp2") as PXSplitContainer;

		if (container != null)
		{
			gridExceptionalObsoleteValue = container.FindControl("gridValueExceptionalObsolete") as PXGrid;
		}

		return gridExceptionalObsoleteValue;
	}

	protected void resetGridPaging(object sender, EventArgs e)
	{
		PXGrid valueGrid = GetValueGrid();
		if (valueGrid != null)
		{
			valueGrid.PageIndex = 0;
		}

		PXGrid obsoleteValueGrid = GetObsoleteValueGrid();
		if (obsoleteValueGrid != null)
		{
			obsoleteValueGrid.PageIndex = 0;
		}
	}

	protected void form_DataBound(object sender, EventArgs e)
	{
		TranslationMaint graph = this.ds.DataGraph as TranslationMaint;

		if (graph != null && graph.IsSiteMapAltered)
		{
			this.ds.CallbackResultArg = "RefreshSitemap";
		}
	}
}
