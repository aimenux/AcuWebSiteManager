using System;
using System.Collections.Generic;
using System.Linq;
using PX.Api;
using PX.Data;
using PX.Web.UI;

public partial class Page_SM207025 : PXPage
{
	protected void Page_Load(object sender, EventArgs e)
	{
		if (!this.Page.IsCallback)
		{
			PXGrid grid = this.tab.FindControl("gridMapping") as PXGrid;
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
        if (info == null) return;

        var res = info.Containers
            .Select(c => new { container = c, viewName = c.Key.Split(new[] { ": " }, StringSplitOptions.None)[0] })
            .SelectMany(t => info.Containers[t.container.Key].Fields, (t, field) => "[" + t.viewName + "." + field.FieldName + "]")
            .Distinct();

        e.Result = string.Join(";", res);
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
		e.Result = string.Join(";", res);
	}

    protected void edValue_SubstitutionKeysNeeded(object sender, PXCallBackEventArgs e)
    {
        var res = new List<string>();
        foreach (SYSubstitution substitution in PXSelect<SYSubstitution>.Select(this.ds.DataGraph))
        {
            res.Add("'" + substitution.SubstitutionID + "'");
        }
        e.Result = string.Join(";", res);
    }

    protected void form_DataBound(object sender, EventArgs e)
    {
        var graph = this.ds.DataGraph as SYImportMaint;
        if (graph.IsSiteMapAltered)
            this.ds.CallbackResultArg = "RefreshSitemap";
    }
}
