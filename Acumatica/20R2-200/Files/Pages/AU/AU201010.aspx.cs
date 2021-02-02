using System;
using System.Linq;
using System.Web;
using PX.SM;
using PX.Web.Customization;
using PX.Web.UI;

public partial class Page_AU201010 : PX.Web.UI.PXPage
{
	protected void Page_Init(object sender, EventArgs e)
	{
        this.Master.FindControl("usrCaption").Visible = false;
	    ProjectBrowserMaint.InitSessionFromQueryString(HttpContext.Current);
    }

	protected void grd_EditorsCreated_RelativeDates(object sender, EventArgs e)
	{
		PXGrid grid = sender as PXGrid;
		if (grid != null)
		{
			PXDateTimeEdit de = grid.PrimaryLevel.GetStandardEditor(GridStandardEditor.Date) as PXDateTimeEdit;
			if (de != null)
			{
				de.ShowRelativeDates = true;
			}
		}
	}

    protected void edValue_RootFieldsNeeded(object sender, PXCallBackEventArgs e)
    {
        var graph = this.ds.DataGraph as AUScreenConditionMaint;
        if (graph != null)
        {
            var fields = graph.GetFields();
            if (fields == null)
                return;
            e.Result = string.Join(";", fields.Select(it=>it.Item1).Union(graph.GetParameters()));
        }
    }
}
