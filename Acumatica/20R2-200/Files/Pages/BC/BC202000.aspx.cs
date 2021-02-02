using System;
using PX.Web.UI;
using PX.Commerce.Core;
using System.Collections.Generic;

public partial class  Page_BC202000 : PX.Web.UI.PXPage
{
	protected void Page_Load(object sender, EventArgs e)
	{
        if (!this.Page.IsCallback)
        {
            PXGrid gridImport = this.tab.FindControl("gridImportMapping") as PX.Web.UI.PXGrid;
            if (gridImport != null)
            {
                this.Page.ClientScript.RegisterClientScriptBlock(GetType(), "gridID", "var gridID=\"" + gridImport.ClientID + "\";", true);
            }
            PXGrid gridExport = this.tab.FindControl("gridExportMapping") as PX.Web.UI.PXGrid;
            if (gridExport != null)
            {
                this.Page.ClientScript.RegisterClientScriptBlock(GetType(), "gridID", "var gridID=\"" + gridExport.ClientID + "\";", true);
            }
        }
    }
    protected void edImportSourceField_InternalFieldsNeeded(object sender, PX.Web.UI.PXCallBackEventArgs e)
    {
        e.Result = GetFieldList(false);
    }

    protected void edImportSourceField_ExternalFieldsNeeded(object sender, PX.Web.UI.PXCallBackEventArgs e)
    {
        e.Result = GetFieldList(true);
    }

    protected void edExportSourceField_InternalFieldsNeeded(object sender, PXCallBackEventArgs e)
    {
        e.Result = GetFieldList(false);
    }

    protected void edExportSourceField_ExternalFieldsNeeded(object sender, PXCallBackEventArgs e)
    {
        e.Result = GetFieldList(true);
    }

    private string GetFieldList(bool isExternal)
    {
        var graph = (BCEntityMaint)this.ds.DataGraph;
        if (graph.CurrentEntity.Current == null) { return String.Empty; }

        List<Tuple<String, String>> internalObjects = graph.GetObjectList(isExternal);
        List<string> res = new List<string>();
        foreach (Tuple<String, String> internalObject in internalObjects)
        {
            var fieldList = graph.GetFieldList(isExternal, internalObject.Item1);
            foreach (var fieldInfo in fieldList)
            {
                res.Add("[" + internalObject.Item1 + "." + fieldInfo.Item1 + "]");
            }
        }
        return String.Join(";", res.ToArray()); ;
    }
}