using System;
using System.Collections;
using System.Collections.Specialized;
using System.Drawing;
using System.Web.UI;
using System.Web.UI.WebControls;
using PX.Web.UI;
using PX.CS;

using System.Collections.Generic;
using PX.Data;

public partial class Page_CS206000 : PX.Web.UI.PXPage
{
	protected void Page_Load(object sender, EventArgs e)
	{
		string[] colors = PXSpecialResources.ColorNames;
		List<string> localizedColors = new List<string>();
		PXDropDown dd = (PXDropDown)this.form.DataControls["edStyleColor"];
		PXListItemCollection items = dd.Items; items.Clear();
		foreach (string n in colors)
		{
			string nLocalized = n;
			PXLocalizerRepository.SpecialLocalizer.LocalizeColorName(ref nLocalized);
			localizedColors.Add(nLocalized);

			items.Add(new PXListItem(string.Empty, nLocalized));
		}

		dd = (PXDropDown)this.form.DataControls["edStyleBackColor"];
		items = dd.Items; items.Clear();
		foreach (string n in localizedColors) items.Add(new PXListItem(string.Empty, n));

		dd = (PXDropDown)this.form.DataControls["edStyleFontName"];
		dd.Items.Clear();
		string[] fonts = PXSpecialResources.FontNames;
		foreach (string ff in fonts)
		{
			string ffLocalized = ff;
			PXLocalizerRepository.SpecialLocalizer.LocalizeFontName(ref ffLocalized);

			dd.Items.Add(new PXListItem(string.Empty, ffLocalized));
		}

		int[] size = new int[] { 8, 9, 10, 11, 12, 14, 16, 18, 20, 22, 24, 26, 28, 36, 48, 72 };
		dd = (PXDropDown)this.form.DataControls["edStyleFontSize"];
		items = dd.Items; items.Clear();
		foreach (int s in size) items.Add(new PXListItem(string.Empty, s.ToString()));
	}

	protected void form_DataBound(object sender, EventArgs e)
	{
		RMReportMaint graph = this.ds.DataGraph as RMReportMaint;
		if (graph.IsSiteMapAltered)
			this.ds.CallbackResultArg = "RefreshSitemap";
	}

	protected void Page_Init(object sender, EventArgs e)
	{
		RMReportMaint graph = (RMReportMaint)ds.DataGraph;

		PXDataSourceViewSchema schema = ds.GetSchema("Report");

		RMReport currentReport = graph.Report.Current;
		RMRowSet currentRowSet = (RMRowSet)graph.Caches[typeof(RMRowSet)].Current;
		RMColumnSet currentColumnSet = (RMColumnSet)graph.Caches[typeof(RMColumnSet)].Current;
		RMUnitSet currentUnitSet = (RMUnitSet)graph.Caches[typeof(RMUnitSet)].Current;
		string currentTypeReport = currentReport != null ? currentReport.Type : null;

		RMDataSource currentDataSource = graph.DataSourceByID.Current;

		if (currentTypeReport == null)
		{
			graph.Report.Current = currentReport = new RMReport();
		}
		if (currentRowSet == null)
		{
			graph.Caches[typeof(RMRowSet)].Current = new RMRowSet();
		}
		graph.Caches[typeof(RMColumnSet)].Current = null;
		graph.Caches[typeof(RMUnitSet)].Current = null;

		Dictionary<string, List<PXFieldSchema>> controls = new Dictionary<string, List<PXFieldSchema>>();

		string[] arrTypes = ((PXStringState)graph.Report.Cache.GetStateExt<RMReport.type>(null)).AllowedValues;

		foreach (string type in arrTypes)
		{
			graph.Report.Current.Type = type;
			((RMRowSet)graph.Caches[typeof(RMRowSet)].Current).Type = type;
			graph.Report.Current = currentReport;

			graph.DataSourceByID.Current = null;

			List<PXFieldSchema> lControls = new List<PXFieldSchema>();
			List<PXFieldSchema> list = RefreshFieldsList(schema, graph);

			foreach (PXFieldSchema f in list)
			{
				if (f.DataType == TypeCode.Object) continue;
				if (!ContainsControlDictionary(f, controls))
				{
					lControls.Add(f);
				}
			}
			controls.Add(type, lControls);
		}

		if (currentTypeReport == null)
		{
			graph.Report.Cache.Clear();
		}
		else
		{
			graph.Report.Current.Type = currentTypeReport;
		}

		graph.DataSourceByID.Current = currentDataSource;
		graph.Caches[typeof(RMRowSet)].Current = currentRowSet;
		graph.Caches[typeof(RMColumnSet)].Current = currentColumnSet;
		graph.Caches[typeof(RMUnitSet)].Current = currentUnitSet;

		_insertIndex = 0;
		foreach (Control control in this.form.TemplateContainer.Controls)
		{
			if (!string.IsNullOrEmpty(control.ID))
			{
				if (!control.ID.Equals("edStartUnitCode", StringComparison.OrdinalIgnoreCase))
					_insertIndex++;
				else
					break;
			}
			else
				_insertIndex++;
		}
		_insertIndex++;

		PXLayoutRule rule1 = new PXLayoutRule() { ID = "rule1", StartGroup = true, GroupCaption = "Default Data Source Settings" };
		(rule1).ApplyStyleSheetSkin(this.Page);
		this.form.TemplateContainer.Controls.AddAt(_insertIndex, rule1);
		_insertIndex++;

		for (int c = arrTypes.Length - 1; c >= 0; c--)
		{
			string type = arrTypes[c];
			var lDsControls = new Dictionary<string, PXFieldSchema>();

			for (int i = 0; i < controls[type].Count; i++)
			{
				if (controls[type][i].DataField.StartsWith("DataSource", StringComparison.OrdinalIgnoreCase) &&
						!controls[type][i].DataField.Equals("datasourceexpand", StringComparison.OrdinalIgnoreCase) &&
						!controls[type][i].DataField.Equals("datasourceamounttype", StringComparison.OrdinalIgnoreCase) &&
						!controls[type][i].DataField.Equals("datasourcestartPeriodOffset", StringComparison.OrdinalIgnoreCase) &&
						!controls[type][i].DataField.Equals("datasourceEndPeriodOffset", StringComparison.OrdinalIgnoreCase)
					//ContainsControl("Request" + controls[type][i].DataField.Replace("DataSource", string.Empty), controls[type])
						)
				{
					var control = controls[type][i];
					lDsControls.Add(control.DataField, control);
				}
			}

			foreach (var item in lDsControls)
			{
				var control = item.Value;
				if (!item.Key.StartsWith("DataSourceEnd", StringComparison.OrdinalIgnoreCase))
				{
					AddControl(control, controls[type]); // add 'Start' control
					string endControlDataField = control.DataField.Replace("DataSourceStart", "DataSourceEnd");
					if (item.Key != endControlDataField)
					{
						PXFieldSchema endControl;
						if (lDsControls.TryGetValue(endControlDataField, out endControl))
							AddControl(endControl, controls[type]);
					}
				}
			}
		}

		PXDropDown edDataSourceAmountType = new PXDropDown() { ID = "edDataSourceAmountType", AllowNull = false, DataField = "DataSourceAmountType" };
		form.TemplateContainer.Controls.AddAt(_insertIndex, edDataSourceAmountType);
		(edDataSourceAmountType).ApplyStyleSheetSkin(this.Page);
		_insertIndex++;

		PXCheckBox chkApplyRestrictionGroups = new PXCheckBox() { ID = "chkApplyRestrictionGroups", DataField = "ApplyRestrictionGroups" };
		form.TemplateContainer.Controls.AddAt(_insertIndex, chkApplyRestrictionGroups);
		(chkApplyRestrictionGroups).ApplyStyleSheetSkin(this.Page);
	}

	private void AddControl(PXFieldSchema control, List<PXFieldSchema> lRequestControls)
	{
		string id = control.DataField.Replace("DataSource", string.Empty);
		if (id.StartsWith("Usr", StringComparison.OrdinalIgnoreCase))
		{
			id = id.Insert(3, "Request");
		}
		else
		{
			id = "Request" + id;
		}
		if (ContainsControl(id, lRequestControls))
		{
			WebControl newDsCtrl = this.CreateControlForField(control);
			(newDsCtrl).ApplyStyleSheetSkin(this.Page);

			PXLayoutRule ruleMerge = new PXLayoutRule() { ID = "rule" + Guid.NewGuid().ToString(), Merge = true };
			(ruleMerge).ApplyStyleSheetSkin(this.Page);

			PXLayoutRule ruleEndMerge = new PXLayoutRule() { ID = "rule" + Guid.NewGuid().ToString() };
			(ruleEndMerge).ApplyStyleSheetSkin(this.Page);

			WebControl newRequestCtrl = CreateControlForField(GetControl(id, lRequestControls));
			(newRequestCtrl).ApplyStyleSheetSkin(this.Page);

			form.TemplateContainer.Controls.AddAt(_insertIndex, ruleMerge);
			_insertIndex++;

			form.TemplateContainer.Controls.AddAt(_insertIndex, newDsCtrl);
			_insertIndex++;

			form.TemplateContainer.Controls.AddAt(_insertIndex, newRequestCtrl);
			SetProperty(newRequestCtrl, "SuppressLabel", true);
			SetProperty(newRequestCtrl, "Size", "xs");
			_insertIndex++;

			form.TemplateContainer.Controls.AddAt(_insertIndex, ruleEndMerge);
			_insertIndex++;
		}
	}

	private void SetProperty(Object obj, String property, Object value)
	{
		System.Reflection.PropertyInfo info = obj.GetType().GetProperty(property, System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);
		if (info != null) info.SetValue(obj, value, null);
	}

	private PXFieldSchema GetControl(string dataField, List<PXFieldSchema> controls)
	{
		for (int i = 0; i < controls.Count; i++)
		{
			if (dataField == (controls[i]).DataField)
			{
				return controls[i];
			}
		}
		return null;
	}

	/// <summary>
	/// Refresh the list of data fields using specified view name.
	/// </summary>
	private List<PXFieldSchema> RefreshFieldsList(PXDataSourceViewSchema schema, RMReportMaint graph)
	{
		PX.Data.PXFieldState[] fields = schema.GetFields();
		List<PXFieldSchema> list = new List<PXFieldSchema>();

		if (fields != null)
		{
			foreach (PX.Data.PXFieldState f in fields)
			{
				if (String.IsNullOrEmpty(f.Name) || !f.Name.StartsWith("DataSource", StringComparison.OrdinalIgnoreCase) && !f.Name.StartsWith("Request", StringComparison.OrdinalIgnoreCase) && !f.Name.StartsWith("UsrRequest", StringComparison.OrdinalIgnoreCase))
					continue;

				string dsField = f.Name;
				if (dsField.StartsWith("DataSource", StringComparison.OrdinalIgnoreCase))
					dsField = dsField.Substring(10);
				else if (f.Name.StartsWith("Request", StringComparison.OrdinalIgnoreCase))
					dsField = dsField.Substring(7);
				else
					dsField = "Usr" + dsField.Substring(10);

				PXFieldState dsState = graph.DataSourceByID.Cache.GetStateExt(null, dsField) as PXFieldState;

				if (dsState == null || !dsState.Visible) continue;

				TypeCode dataType = Type.GetTypeCode(f.DataType);
				if (dataType == TypeCode.Object) continue;

				PXFieldSchema item = new PXFieldSchema(true, f.Name, dataType);
				item.ControlType = PXSchemaGenerator.GetControlType(f);
				item.PrimaryKey = f.PrimaryKey;
				item.ReadOnly = f.IsReadOnly;
				item.AllowNull = f.Nullable;
				item.MaxLength = (f.Length > 0) ? f.Length : 0;
				item.Precision = (f.Precision > 0) ? f.Precision : 0;

				item.Caption = f.DisplayName;
				item.ViewName = f.ViewName;
				item.HintField = f.DescriptionName;
				item.FieldList = f.FieldList;
				item.HeaderList = f.HeaderList;
				item.DefaultValue = f.DefaultValue;

				list.Add(item);
			}
		}

		return list;
	}

	/// <summary>
	/// True if the controls contains the control with control.dataField == dataField ; otherwise, false.
	/// </summary>	
	private bool ContainsControl(string dataField, List<PXFieldSchema> controls)
	{
		foreach (PXFieldSchema wc in controls)
		{
			if (dataField == wc.DataField) return true;
		}
		return false;
	}

	/// <summary>
	/// True if the controls contains the control; otherwise, false.
	/// </summary>
	private bool ContainsControlDictionary(PXFieldSchema control, Dictionary<string, List<PXFieldSchema>> controls)
	{
		Dictionary<string, List<PXFieldSchema>>.KeyCollection keyColl = controls.Keys;
		foreach (string type in keyColl)
		{
			foreach (PXFieldSchema wc in controls[type])
			{
				if (control.DataField == wc.DataField) return true;
			}
		}
		return false;
	}

	/// <summary>
	/// Create web control for specified field.
	/// </summary>
	private WebControl CreateControlForField(PXFieldSchema f)
	{
		System.Web.UI.WebControls.WebControl ctrl = null;
		switch (f.ControlType)
		{
			case PXSchemaControl.NumberEdit:
				ctrl = new PXNumberEdit();
				((PXNumberEdit)ctrl).DataField = f.DataField;
				((PXNumberEdit)ctrl).ValueType = f.DataType;
				((PXNumberEdit)ctrl).AllowNull = true;
				break;
			case PXSchemaControl.TextEdit:
				ctrl = new PXTextEdit();
				((PXTextEdit)ctrl).DataField = f.DataField;
				break;
			case PXSchemaControl.CheckBox:
				ctrl = new PXCheckBox();
				((PXCheckBox)ctrl).DataField = f.DataField;
				((PXCheckBox) ctrl).Size = "M";
				break;
			case PXSchemaControl.ComboBox:
				ctrl = new PXDropDown();
				((PXDropDown)ctrl).DataField = f.DataField;
				((PXDropDown)ctrl).AllowNull = false;
				break;
			case PXSchemaControl.Selector:
				ctrl = new PXSelector();
				((PXSelector)ctrl).DataSourceID = ds.ID;
				((PXSelector)ctrl).DataField = f.DataField;

				PXFieldState fs = ((RMReportMaint)ds.DataGraph).Report.Cache.GetStateExt(((RMReportMaint)ds.DataGraph).Report.Current, f.DataField) as PXFieldState;
				if (fs != null && !String.IsNullOrWhiteSpace(fs.DescriptionName))
				{
					((PXSelector)ctrl).TextMode = TextModeTypes.Search;
					((PXSelector)ctrl).DisplayMode = ValueDisplayMode.Text;
				}
				else if (fs.ValueField != null && fs.ValueField.ToLower() == "compositekey")
				{
					  ((PXSelector)ctrl).CommitChanges = true;
				}
				break;
			case PXSchemaControl.SegmentMask:
				ctrl = new PXSegmentMask();
				((PXSegmentMask)ctrl).DataMember = f.ViewName;
				break;
			case PXSchemaControl.DateTimeEdit:
				ctrl = new PXDateTimeEdit();
				((PXDateTimeEdit)ctrl).DataField = f.DataField;
				break;
		}

		if (ctrl != null)
		{
			ctrl.ID = f.DataField;
			((IFieldEditor)ctrl).DataField = f.DataField;

			if (f.DataField == "DataSourceOrganizationID" && ctrl is PXSegmentMask)
			{
				((PXSelectorBase)ctrl).CommitChanges = true;
			}
			else if (ctrl is PXSelectorBase && (f.DataField == "DataSourceStartBranch" || f.DataField == "DataSourceEndBranch"))
			{
				((PXSelectorBase)ctrl).AutoRefresh = true;
				((PXSelectorBase)ctrl).CommitChanges = true;
			}
		}
		return ctrl;
	}

	protected int _insertIndex;
}
