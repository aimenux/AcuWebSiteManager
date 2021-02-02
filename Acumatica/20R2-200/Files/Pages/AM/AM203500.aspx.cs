using PX.Objects.AM;
using PX.Web.UI;
using System;

public partial class Page_AM203500 : PX.Web.UI.PXPage
{
    protected void Page_Load(object sender, EventArgs e)
    {
    }

    protected void edFormulaExpression(object sender, PXCallBackEventArgs e)
    {
        var graph = this.ds.DataGraph as FeatureMaint;
        if (graph == null) return;

        string[] attributes = graph.GetAllAttributes();
        e.Result = string.Join(";", attributes);
    }

    protected void edFormulaAttributeExpression(object sender, PXCallBackEventArgs e)
    {
        FeatureMaint graph = this.ds.DataGraph as FeatureMaint;
        if (graph == null) return;

        string[] attributes = graph.GetAllButCurrentAttributes();
        e.Result = string.Join(";", attributes);
    }
}
