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

public partial class Page_WZ201505 : PX.Web.UI.PXPage
{
    protected void Page_Load(object sender, EventArgs e)
    {
        string ScenarioID = Request.Params["ScenarioID"];
        WizardScenarioMaint graph = PXGraph.CreateInstance<WizardScenarioMaint>();
        if (!String.IsNullOrEmpty(ScenarioID))
        {
            Guid id;
            Guid.TryParse(ScenarioID, out id);
            WZScenario scenario = PXSelect<WZScenario, Where<WZScenario.scenarioID, Equal<Required<WZScenario.scenarioID>>>>.Select(graph, id);
            if (scenario != null && !String.IsNullOrEmpty(scenario.Name))
            {
                
                ((PXLabel) this.frmBottom.FindControl("lblErrCode")).Text = scenario.Name;
                if (scenario.ExecutionDate != null)
                {
                    ((PXLabel) this.frmBottom.FindControl("lastExecTimeVal")).Text = string.Format("{0}. ",
                        scenario.ExecutionDate);

                    if (scenario.OwnerID == null)
                    {
                        ((PXLabel) this.frmBottom.FindControl("lastExecByPre")).Visible = false;
                        ((PXLabel) this.frmBottom.FindControl("lastExecByVal")).Visible = false;
                    }

                }
                else
                {
                    ((PXLabel) this.frmBottom.FindControl("lastExecTimePre")).Visible = false;
                    ((PXLabel) this.frmBottom.FindControl("lastExecTimeVal")).Visible = false;
                    ((PXLabel) this.frmBottom.FindControl("lastExecByPre")).Visible = false;
                    ((PXLabel) this.frmBottom.FindControl("lastExecByVal")).Visible = false;
                }
            }
        }
    }
    protected void btnActivate_Click(object sender, EventArgs e)
    {
        string ScenarioID = Request.Params["ScenarioID"];
        if (!String.IsNullOrEmpty(ScenarioID))
        {
            WZTaskEntry graph = PXGraph.CreateInstance<WZTaskEntry>();
            Guid id = Guid.Parse(ScenarioID);
            WZScenario scenario = PXSelect<WZScenario, Where<WZScenario.scenarioID, Equal<Required<WZScenario.scenarioID>>>>.Select(graph, id);
            if (scenario != null && !String.IsNullOrEmpty(scenario.Name))
            {
                graph.Scenario.Current = scenario;
                graph.prepareTasksForActivation.Press();
                graph.activateScenarioWithoutRefresh.Press();
                PXSiteMap.Provider.Clear();
                string url = ResolveUrl(PXUrl.MainPagePath);
                if (id == Guid.Empty)
                {
                    Controls.Add(new LiteralControl(@"<script  type='text/javascript'>try { window.top.location.href='" + url + "'; } catch (ex) {}</script>\n"));    
                }
                else
                {
                    PXSiteMapNode node = PXSiteMap.Provider.FindSiteMapNodeFromKey(id);
                    if (node != null)
                    {
                        url = ResolveUrl("~/Pages/WZ/WZ201500.aspx");
                        Redirector.Redirect(this.Context, url + "?ScenarioID=" + id);
                    }
                    else
                    {
                        Controls.Add(new LiteralControl(@"<script  type='text/javascript'>try { window.top.location.href='" + url + "'; } catch (ex) {}</script>\n"));
                    }
                }
            }
        }
    }

    protected void btnHistory_Click(object sender, EventArgs e)
    {
        string ScenarioID = Request.Params["ScenarioID"];
        if (!String.IsNullOrEmpty(ScenarioID))
        {
            WizardScenarioMaint graph = PXGraph.CreateInstance<WizardScenarioMaint>();
            Guid id = Guid.Parse(ScenarioID);
            WZScenario scenario = PXSelect<WZScenario, Where<WZScenario.scenarioID, Equal<Required<WZScenario.scenarioID>>>>.Select(graph, id);
            if (scenario != null && !String.IsNullOrEmpty(scenario.Name))
            {               
                PXSiteMapNode node = PXSiteMap.Provider.FindSiteMapNodeFromKey(id);
                if (node != null)
                {
                    string url = ResolveUrl(PXUrl.MainPagePath + @"?ScreenId=WZ201501&ScenarioID=" + scenario.ScenarioID);
                    Controls.Add(new LiteralControl(@"<script  type='text/javascript'>try { window.top.location.href='" + url + "'; } catch (ex) {}</script>\n"));
                    
                }
            }
        }
    }
}