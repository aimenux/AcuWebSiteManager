using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using PX.Data;
using PX.Objects.WZ;

public partial class Page_WZ201510 : PX.Web.UI.PXPage
{
    protected void Page_PreInit(object sender, EventArgs e)
    {
        WizardArticleMaint graph = PXGraph.CreateInstance<WizardArticleMaint>();
        if (!String.IsNullOrEmpty(this.Request.QueryString["TaskID"]))
        {
            Guid id = Guid.Parse(this.Request.QueryString["TaskID"]);
            WZTask task = PXSelect<WZTask, Where<WZTask.taskID, Equal<Required<WZTask.taskID>>>>.Select(graph, id);
            
            if (task != null && !String.IsNullOrEmpty(task.Name))
            {
                WZScenario scenario = PXSelect<WZScenario, Where<WZScenario.scenarioID, Equal<Required<WZScenario.scenarioID>>>>.Select(graph, task.ScenarioID);
                if (scenario != null)
                {
                    this.Master.ScreenTitle = scenario.Name + " - ";
                }
                this.Master.ScreenTitle += task.Name;
                if ((bool)task.IsOptional)
                {
                    this.Master.ScreenTitle += " (Optional)";
                }
            }
        }
    }

	protected void Page_Init(object sender, EventArgs e)
	{
		((IPXMasterPage)this.Master).CustomizationAvailable = false;
		((IPXMasterPage)this.Master).HelpAvailable = false;
	}

    protected void Page_Load(object sender, EventArgs e)
    {
    }
}