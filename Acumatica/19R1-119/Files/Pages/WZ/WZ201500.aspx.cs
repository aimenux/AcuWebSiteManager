using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using PX.Data;
using PX.Web.UI;
using PX.Objects.WZ;

public partial class Page_WZ201500 : PXPage
{
	protected void Page_PreInit(object sender, EventArgs e)
	{
		this.Master.FavoriteAvailable = false;
	}

	protected void Page_Init(object sender, EventArgs e)
	{
		WizardScenarioMaint graph = PXGraph.CreateInstance<WizardScenarioMaint>();
		if (!String.IsNullOrEmpty(this.Request.QueryString["ScenarioID"]))
		{
			Guid id = Guid.Parse(this.Request.QueryString["ScenarioID"]);
			WZScenario scenario = PXSelect<WZScenario, Where<WZScenario.scenarioID, Equal<Required<WZScenario.scenarioID>>>>.Select(graph, id);
			if (scenario != null && !String.IsNullOrEmpty(scenario.Name))
			{
				this.Master.ScreenTitle = scenario.Name;
			}
		}
	}

	protected void grid_RowDataBound(object sender, PXGridRowEventArgs e)
	{
		WZTask task = (WZTask)e.Row.DataItem;

		if (task.Offset != null && task.Offset > 0)
			e.Row.Cells["Name"].Style.CssClass = "PropOffset" + task.Offset;
	}
}