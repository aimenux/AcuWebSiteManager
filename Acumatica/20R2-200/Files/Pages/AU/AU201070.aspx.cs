using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using PX.SM;
using PX.Web.Controls;
using PX.Web.Customization;
using PX.Web.UI;

public partial class Pages_AU_AU201070 : PX.Web.UI.PXPage
{
    protected void Page_PreInit(object sender, EventArgs e)
    {
        ProjectBrowserMaint.InitSessionFromQueryString(HttpContext.Current);
    }

    protected void Page_Init(object sender, EventArgs e)
    {
        this.Master.FindControl("usrCaption").Visible = false;
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
        AUWorkflowEventHandlersMaint graph = this.ds.DataGraph as AUWorkflowEventHandlersMaint;
        if (graph != null)
        {
            var fields = graph.GetFieldsAndParams();
            e.Result = string.Join(";", fields);
        }
    }
   
}
