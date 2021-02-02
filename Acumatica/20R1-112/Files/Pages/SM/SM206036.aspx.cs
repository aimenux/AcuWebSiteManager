using System;
using System.Configuration;
using System.Collections;
using System.Web;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.UI.HtmlControls;
using PX.Data;
using PX.Web.UI;
using PX.Api;
using System.Collections.Generic;

public partial class Page_SM206036 : PX.Web.UI.PXPage
{
	protected void Page_Load(object sender, EventArgs e)
	{
		PXGrid grid = this.tab.FindControl("gridPreparedData") as PXGrid;
		if (grid != null)
		{
			grid.RepaintColumns = true;
			grid.GenerateColumnsBeforeRepaint = true;
		}
	}

	protected void upl_FileUploaded(object sender, PXFileUploadEventArgs e)
	{
		((SYImportProcessSingle)this.ds.DataGraph).SaveNewFileVersion(e.UploadedFile);
	}

	
	protected void edValue_InternalFieldsNeeded(object sender, PXCallBackEventArgs e)
	{
		List<string> res = new List<string>();
		SYImportProcessSingle graph = (SYImportProcessSingle)this.ds.DataGraph;
		if (graph.MappingsSingle.Current == null || string.IsNullOrEmpty(graph.MappingsSingle.Current.ScreenID))
			return;

        PXSiteMap.ScreenInfo info = ScreenUtils.GetScreenInfo(graph.MappingsSingle.Current.ScreenID);
		Dictionary<string, bool> addedViews = new Dictionary<string, bool>();
		foreach (string viewname in info.Containers.Keys)
		{
			int index = viewname.IndexOf(": ");
			if (index != -1 && addedViews.ContainsKey(viewname.Substring(0, index)))
				continue;
			addedViews.Add(viewname, true);
			foreach (PX.Data.Description.FieldInfo field in info.Containers[viewname].Fields)
				res.Add("[" + viewname + "." + field.FieldName + "]");
		}
		e.Result = string.Join(";", res.ToArray());
	}

	protected void edValue_ExternalFieldsNeeded(object sender, PXCallBackEventArgs e)
	{
		List<string> res = new List<string>();
		foreach (SYProviderField field in PXSelect<SYProviderField, Where<SYProviderField.providerID, Equal<Current<SYMapping.providerID>>,
										And<SYProviderField.objectName, Equal<Current<SYMapping.providerObject>>,
										And<SYProviderField.isActive, Equal<True>>>>,
										OrderBy<Asc<SYProviderField.displayName>>>.Select(this.ds.DataGraph))
		{
			res.Add("[" + field.Name + "]");
		}
		e.Result = string.Join(";", res.ToArray());
	}
}
