using System;
using System.Configuration;
using System.Collections;
using System.Web;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.UI.HtmlControls;
using PX.Data;
using PX.Web.UI;

public partial class Page_SM203535 : PXPage
{
	protected void Page_Load(object sender, EventArgs e)
	{
	}

    protected void grid_RowDataBound(object sender, PXGridRowEventArgs e)
    {
        if (e.Row != null && e.Row.DataItem != null)
        {
            var state = this.ds.DataGraph.Caches[e.Row.DataItem.GetType()].GetStateExt(e.Row.DataItem, "Value") as PXStringState;
            if (state != null && state.InputMask == "*")
            {
                e.Row.Cells["Value"].IsPassword = true;
            }
        }
    }
}
