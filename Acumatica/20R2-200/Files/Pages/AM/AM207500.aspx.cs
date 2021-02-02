using System;
using System.Linq;
using System.Configuration;
using System.Collections;
using System.Collections.Generic;
using System.Web;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.UI.HtmlControls;
using PX.Web.UI;
using PX.Web.Controls;
using PX.Objects.AM;

public partial class Page_AM207500 : PX.Web.UI.PXPage
{
    protected void Page_Load(object sender, EventArgs e)
    {
    }

    protected void edFormulaExpression(object sender, PXCallBackEventArgs e)
    {
        ConfigurationMaint graph = this.ds.DataGraph as ConfigurationMaint;
        if (graph == null) return;
        
        string[] attributes = graph.GetAllAttributes();
        e.Result = string.Join(";", attributes);        
    }

    protected void edFormulaAttributeExpression(object sender, PXCallBackEventArgs e)
    {
        ConfigurationMaint graph = this.ds.DataGraph as ConfigurationMaint;
        if (graph == null) return;

        string[] attributes = graph.GetAllButCurrentAttributes();
        e.Result = string.Join(";", attributes);
    }
}
