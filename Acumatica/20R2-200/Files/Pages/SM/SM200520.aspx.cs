using System;
using PX.SiteMap.Graph;

public partial class Page_SM200520 : PX.Web.UI.PXPage
{
    protected void form_DataBound(object sender, EventArgs e)
    {
        SiteMapMaint graph = this.ds.DataGraph as SiteMapMaint;
        if (graph.IsSiteMapAltered)
            this.ds.CallbackResultArg = "RefreshSitemap";
    }
}
