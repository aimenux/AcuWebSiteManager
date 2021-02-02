using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using PX.Common;
using PX.Data;
using PX.Objects.WZ;
using PX.Web.UI;

public partial class Page_WZ201520 : PXPage
{
    protected void Page_Load(object sender, EventArgs e)
    {

    }

    protected void enableBtn_OnClick(object sender, EventArgs e)
    {
        WZSetupMaint graph = PXGraph.CreateInstance<WZSetupMaint>();
        graph.enableWizards.Press();
        string url = ResolveUrl(PXUrl.MainPagePath);
        Controls.Add(new LiteralControl(@"<script  type='text/javascript'>try { window.top.location.href='" + url + "'; } catch (ex) {}</script>\n")); 
    }

    protected void disableBtn_OnClick(object sender, EventArgs e)
    {
        WZSetupMaint graph = PXGraph.CreateInstance<WZSetupMaint>();
        graph.disableWizards.Press();
        string url = ResolveUrl(PXUrl.MainPagePath);
        Controls.Add(new LiteralControl(@"<script  type='text/javascript'>try { window.top.location.href='" + url + "'; } catch (ex) {}</script>\n")); 
    }
}