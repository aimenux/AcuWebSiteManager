using System;
using System.Collections;
using PX.Common;
using PX.Web.UI;

public partial class Pages_SM_SM201530 : PXPage
{
    protected void Page_Load(object sender, EventArgs e)
    {
        
    }

    // I know this method is obsolete, but i didn't find ahother place, where JSManager would work properly
    public override void RegisterClientScriptBlock(string key, string script)
    {
        base.RegisterClientScriptBlock(key, script);
        var renderer = JSManager.GetRenderer(this);
        JSManager.RegisterModule(renderer, typeof(PXChart), JS.AmChart);
        JSManager.RegisterModule(renderer, typeof(PXChart), JS.Chart);
    }

    protected override void OnPreRender(EventArgs e)
    {
        var chart = (PXSerialChart)MainTabControl.FindControl("SerialChartCPU");
        chart.ValueAxis[0].Minimum = 0;
        chart.ValueAxis[0].Maximum = 100;
        chart.ValueAxis[0].GridCount = 100;
        
        base.OnPreRender(e);
    }

    protected void SerialChartCPU_OnLoad(object sender, EventArgs e)
    {
        var chart = (PXSerialChart)sender;
        chart.Visible = !WebConfig.IsClusterEnabled;
        chart.DataSource = GetFakeCPUDataSource();
    }

    protected void SerialChartWorkingSet_OnLoad(object sender, EventArgs e)
    {
        var chart = (PXSerialChart)sender;
        chart.Visible = !WebConfig.IsClusterEnabled;
        chart.DataSource = GetFakeMemoryDataSource();
    }

    private IEnumerable GetFakeCPUDataSource()
    {
        var result = new object[30];
        for (int i = 0; i < 30; i++)
        {
            result[i] = new {Category = "", Values = 0, Labels = "0%"};
        }

        return result;
    }

    private IEnumerable GetFakeMemoryDataSource()
    {
        var result = new object[30];
        for (int i = 0; i < 30; i++)
        {
            result[i] = new {Category = "", Values = 0, Labels = "0"};
        }

        return result;
    }

   
}
