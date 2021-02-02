using System;
using PX.Data;
using PX.Api;
using PX.Web.UI;
using System.Collections.Generic;
using System.Linq;

public partial class Page_SM207025 : PX.Web.UI.PXPage
{
	protected void Page_Load(object sender, EventArgs e)
	{
		if (!this.Page.IsCallback)
		{
			PXGrid grid = this.tab.FindControl("gridMapping") as PX.Web.UI.PXGrid;
			if (grid != null)
				this.Page.ClientScript.RegisterClientScriptBlock(GetType(), "gridID", "var gridID=\"" + grid.ClientID + "\";", true);
		}
	}

    protected void edValue_InternalFieldsNeeded(object sender, PXCallBackEventArgs e)
    {
        var graph = (SYExportMaint)this.ds.DataGraph;
        if (graph.Mappings.Current == null || string.IsNullOrEmpty(graph.Mappings.Current.ScreenID))
            return;

        var info = ScreenUtils.GetScreenInfo(graph.Mappings.Current.ScreenID);
        var res = info.Containers.Select(
                c => new { container = c, viewName = c.Key.Split(new[] { ": " }, StringSplitOptions.None)[0] })
            .SelectMany(t => info.Containers[t.container.Key].Fields,
                (t, field) => "[" + t.viewName + "." + field.FieldName + "]").Distinct();

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
